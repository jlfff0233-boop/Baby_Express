using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 튜토리얼 뷰 - 대상 강조와 단계별 입력 제한 표시
/// </summary>
public class TutorialView : MonoBehaviour
{
    [Header( "----- 입력 차단 영역 -----" )]
    [SerializeField] TutorialBlockerView _blocker;       //대상 외 전체 입력 차단

    [Header( "----- 행동 유도 -----" )]
    [SerializeField] RectTransform _arrow;       //행동 유도 화살표
    [SerializeField] UIHighlightView _arrowHighlight;       //화살표 반복 연출
    [SerializeField] Vector2 _arrowPositionOffset;       //전체 화살표 위치 보정
    [SerializeField] Vector2 _blockingPadding = new Vector2( 10f, 10f );       //강조 대상 주변 입력 허용 여백

    [Header( "----- 튜토리얼 제어 -----" )]
    [SerializeField] Button _skipButton;       //현재 튜토리얼 또는 가이드 건너뛰기

    RectTransform _guideRoot;       //전체 튜토리얼 표시 영역
    RectTransform _arrowParent;       //화살표 좌표 계산 기준 부모
    Canvas _arrowCanvas;       //화살표가 속한 Canvas
    Vector3 [ ] _targetWorldCorners = new Vector3 [ 4 ];       //대상 월드 모서리 임시 저장
    TutorialTargetData _currentTarget;       //현재 강조 대상
    Dictionary<TutorialTargetId, TutorialTargetData> _targets =
        new Dictionary<TutorialTargetId, TutorialTargetData>( );       //런타임 UI 대상
    Sequence _targetPunchSequence;       //여러 대상 순차 강조 연출
    bool _isInitialized;       //런타임 표시 영역 초기화 여부
    bool _isSkipVisible;       //스킵 버튼 표시 여부
    bool _isSkipInitialized;       //스킵 버튼 이벤트 연결 여부

    /// <summary>
    /// 현재 튜토리얼 또는 가이드 스킵 요청
    /// </summary>
    public event Action OnSkip;

    /// <summary>
    /// 튜토리얼 대상 초기화
    /// </summary>
    void Awake ()
    {
        //외부 초기화가 먼저 실행된 비활성 뷰는 표시 상태를 다시 지우지 않음
        if ( _isInitialized == true ) return;

        InitRuntime( );
        HideInstant( );
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 튜토리얼 표시 영역 초기화
    /// </summary>
    public void InitRuntime ()
    {
        if ( _isInitialized == true ) return;

        _guideRoot = transform as RectTransform;

        if ( _arrow != null )
        {
            _arrowParent = _arrow.parent as RectTransform;
            _arrowCanvas = _arrow.GetComponentInParent<Canvas>( );
        }

        if ( _skipButton != null && _isSkipInitialized == false )
        {
            _skipButton.onClick.AddListener( RequestSkip );
            _skipButton.gameObject.SetActive( false );
            _isSkipInitialized = true;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 스킵 버튼 이벤트 연결 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _skipButton != null && _isSkipInitialized == true )
            _skipButton.onClick.RemoveListener( RequestSkip );
    }

    /// <summary>
    /// 활성 대상의 위치 변화에 맞춰 표시 위치 갱신
    /// </summary>
    void LateUpdate ()
    {
        if ( _currentTarget == null ) return;

        //풀로 반환되거나 제거된 런타임 대상이면 표시 종료
        if ( _currentTarget.Target == null ||
            _currentTarget.Target.gameObject.activeInHierarchy == false )
        {
            HideInstant( );
            return;
        }

        UpdateTargetDisplay( );
    }

    /// <summary>
    /// 비활성화할 때 강조 상태 복구
    /// </summary>
    void OnDisable ()
    {
        StopTargetPunchSequence( );
        ClearCurrentTarget( );

        if ( _blocker != null )
            _blocker.Hide( );
    }

    /// <summary>
    /// 지정 대상의 튜토리얼 표시
    /// </summary>
    /// <param name="targetId">강조 대상 아이디</param>
    /// <param name="mode">입력 제한 방식</param>
    /// <returns>대상 표시 성공 여부</returns>
    public bool ShowTarget (
        TutorialTargetId targetId, TutorialGuideMode mode )
    {
        //초기화
        InitRuntime( );
        StopTargetPunchSequence( );
        ClearCurrentTarget( );

        //튜토리얼에 쓸 대상 가져오기
        if ( TryGetTarget( targetId, out TutorialTargetData target ) == false )
        {
            HideInstant( );
            return false;
        }

        //타겟이 없거나 숨겨져 있거나 연출뷰가 없으면
        if ( target.Target == null ||
            target.Target.gameObject.activeInHierarchy == false ||
            target.Highlight == null )
        {
            HideInstant( );
            return false;
        }

        _currentTarget = target;

        gameObject.SetActive( true );

        //이전 대상 위치의 화살표가 한 프레임 노출되지 않도록 먼저 숨김
        if ( _arrow != null )
            _arrow.gameObject.SetActive( false );

        if ( mode == TutorialGuideMode.Blocking )
        {
            if ( _blocker != null )
                _blocker.Show( _currentTarget.Target, _blockingPadding );
        }
        else if ( _blocker != null )
        {
            _blocker.Hide( );
        }

        //스크롤과 레이아웃 이동 결과를 반영한 뒤 새 대상 위치 표시
        Canvas.ForceUpdateCanvases( );
        UpdateTargetDisplay( );

        if ( _arrow != null )
            _arrow.gameObject.SetActive( true );

        //대상은 처음 한 번만 강조하고 화살표만 반복 표시
        _currentTarget.Highlight.PlayPunch( );
        if ( _arrowHighlight != null )
            _arrowHighlight.PlayAppearLoop( true );

        return true;
    }

    /// <summary>
    /// 화살표와 입력 차단 없이 지정 대상을 한 번 확대 강조
    /// </summary>
    /// <param name="targetId">강조 대상 아이디</param>
    /// <returns>강조 재생 성공 여부</returns>
    public bool PlayTargetPunch ( TutorialTargetId targetId )
    {
        InitRuntime( );

        if ( TryGetTarget( targetId,
            out TutorialTargetData target ) == false ||
            target.Target == null ||
            target.Target.gameObject.activeInHierarchy == false ||
            target.Highlight == null )
        {
            return false;
        }

        target.Highlight.PlayPunch( );
        return true;
    }

    /// <summary>
    /// 화살표와 입력 차단 없이 여러 대상을 차례대로 확대 강조
    /// </summary>
    /// <param name="interval">대상별 강조 시작 간격</param>
    /// <param name="targetIds">강조할 대상 아이디 목록</param>
    /// <returns>순차 강조 시작 여부</returns>
    public bool PlayTargetPunchSequence (
        float interval, params TutorialTargetId [ ] targetIds )
    {
        InitRuntime( );
        StopTargetPunchSequence( );

        if ( targetIds == null || targetIds.Length == 0 )
            return false;

        _targetPunchSequence = DOTween.Sequence( );

        for ( int i = 0; i < targetIds.Length; i++ )
        {
            TutorialTargetId targetId = targetIds [ i ];

            _targetPunchSequence.AppendCallback(
                () => PlayTargetPunch( targetId ) );

            if ( i < targetIds.Length - 1 )
                _targetPunchSequence.AppendInterval(
                    Mathf.Max( 0f, interval ) );
        }

        _targetPunchSequence.SetUpdate( true );
        _targetPunchSequence.OnComplete(
            () => _targetPunchSequence = null );
        return true;
    }

    /// <summary>
    /// 행동 허용 대상 없이 튜토리얼 외 화면 입력 전체 차단
    /// </summary>
    public void ShowInputBlocker ( )
    {
        InitRuntime( );
        StopTargetPunchSequence( );
        ClearCurrentTarget( );

        gameObject.SetActive( true );

        if ( _arrow != null )
            _arrow.gameObject.SetActive( false );

        if ( _blocker != null )
            _blocker.ShowAll( );
    }

    /// <summary>
    /// 튜토리얼 표시 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        InitRuntime( );
        StopTargetPunchSequence( );
        ClearCurrentTarget( );

        if ( _blocker != null )
            _blocker.Hide( );

        if ( _arrow != null )
            _arrow.gameObject.SetActive( false );

        //스킵 버튼이 표시 중이면 튜토리얼 루트는 유지
        RefreshRootActive( );
    }

    /// <summary>
    /// 현재 튜토리얼 스킵 버튼 표시 상태 변경
    /// </summary>
    /// <param name="isVisible">스킵 버튼 표시 여부</param>
    public void SetSkipVisible ( bool isVisible )
    {
        InitRuntime( );

        _isSkipVisible = isVisible && _skipButton != null;

        if ( _skipButton != null )
            _skipButton.gameObject.SetActive( _isSkipVisible );

        RefreshRootActive( );
    }

    /// <summary>
    /// 스킵 버튼 입력 전달
    /// </summary>
    void RequestSkip ()
    {
        OnSkip?.Invoke( );
    }

    /// <summary>
    /// 타깃 또는 스킵 버튼 사용 여부에 따라 루트 활성 상태 갱신
    /// </summary>
    void RefreshRootActive ()
    {
        bool shouldShow = _currentTarget != null || _isSkipVisible == true;

        if ( gameObject.activeSelf != shouldShow )
            gameObject.SetActive( shouldShow );
    }

    /// <summary>
    /// 진행 중인 대상 순차 강조 연출 정지
    /// </summary>
    void StopTargetPunchSequence ()
    {
        if ( _targetPunchSequence == null )
            return;

        _targetPunchSequence.Kill( );
        _targetPunchSequence = null;
    }

    /// <summary>
    /// 풀링 등으로 런타임에 생성된 강조 대상 연결
    /// </summary>
    /// <param name="targetId">연결할 대상 아이디</param>
    /// <param name="target">현재 활성 UI 대상</param>
    /// <returns>대상 연결 성공 여부</returns>
    public bool SetRuntimeTarget (
        TutorialTargetId targetId, RectTransform target )
    {
        return RegisterTarget(
            targetId, target, Vector2.zero );
    }

    /// <summary>
    /// 기존 뷰 참조를 튜토리얼 대상으로 런타임 등록
    /// </summary>
    /// <param name="targetId">등록할 대상 아이디</param>
    /// <param name="target">강조할 실제 UI</param>
    /// <param name="arrowOffset">대상 기준 화살표 위치 보정</param>
    /// <returns>대상 등록 성공 여부</returns>
    public bool RegisterTarget (
        TutorialTargetId targetId, RectTransform target, Vector2 arrowOffset )
    {
        InitRuntime( );

        if ( targetId == TutorialTargetId.None ||
            target == null )
        {
            return false;
        }

        if ( _targets.TryGetValue(
            targetId, out TutorialTargetData targetData ) )
        {
            targetData.SetTarget( target, arrowOffset );
            return true;
        }

        _targets.Add( targetId,
            new TutorialTargetData( targetId, target, arrowOffset ) );

        return true;
    }

    /// <summary>
    /// 대상 아이디에 대응하는 연결 데이터 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 대상 데이터</param>
    /// <returns>대상 존재 여부</returns>
    bool TryGetTarget (
        TutorialTargetId targetId, out TutorialTargetData target )
    {
        return _targets.TryGetValue( targetId, out target );
    }

    /// <summary>
    /// 현재 대상 위치에 화살표 배치
    /// </summary>
    void UpdateTargetDisplay ()
    {
        if ( _guideRoot == null || _arrow == null ||
            _arrowParent == null ||
            _currentTarget?.Target == null )
        {
            return;
        }

        RectTransform target = _currentTarget.Target;
        target.GetWorldCorners( _targetWorldCorners );

        //대상 상단 중앙의 월드 위치를 화면 좌표로 변환
        Vector3 targetTopCenter =
            ( _targetWorldCorners [ 1 ] + _targetWorldCorners [ 2 ] ) * 0.5f;
        Canvas targetCanvas = target.GetComponentInParent<Canvas>( );
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            GetCanvasCamera( targetCanvas ), targetTopCenter );

        if ( RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _arrowParent, screenPoint, GetCanvasCamera( _arrowCanvas ),
            out Vector2 arrowPosition ) == false )
        {
            return;
        }

        //서로 다른 Canvas에서도 화살표 부모 기준 좌표로 정확히 배치
        _arrow.anchoredPosition =
            arrowPosition + _currentTarget.ArrowOffset +
            _arrowPositionOffset;
    }

    /// <summary>
    /// Canvas 렌더 방식에 맞는 UI 좌표 변환 카메라 반환
    /// </summary>
    /// <param name="canvas">좌표를 변환할 Canvas</param>
    /// <returns>오버레이 Canvas는 null, 그 외에는 연결 카메라</returns>
    Camera GetCanvasCamera ( Canvas canvas )
    {
        if ( canvas == null ||
            canvas.renderMode == RenderMode.ScreenSpaceOverlay )
        {
            return null;
        }

        return canvas.worldCamera;
    }

    /// <summary>
    /// 현재 대상과 화살표 강조 상태 복구
    /// </summary>
    void ClearCurrentTarget ()
    {
        if ( _currentTarget != null )
        {
            if ( _currentTarget.Highlight != null )
                _currentTarget.Highlight.Stop( );

            _currentTarget = null;
        }

        if ( _arrowHighlight != null )
            _arrowHighlight.Stop( );
    }
}
