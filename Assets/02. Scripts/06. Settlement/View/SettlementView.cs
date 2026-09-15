using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 결산 구역과 타이틀 아래 슬롯 정렬 영역 연결
/// </summary>
[Serializable]
class SettlementSectionBinding
{
    [SerializeField] SettlementSectionType _sectionType;       //결산 구역 종류
    [SerializeField] Transform _content;       //구역 타이틀 아래 슬롯 정렬 영역

    /// <summary>
    /// 결산 구역 종류
    /// </summary>
    public SettlementSectionType SectionType => _sectionType;

    /// <summary>
    /// 구역 타이틀 아래 슬롯 정렬 영역
    /// </summary>
    public Transform SlotContent => _content;
}

/// <summary>
/// 결산 뷰 - 구역별 결산 슬롯 표시와 확인 입력 관리
/// </summary>
public class SettlementView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //결산 패널 연출
    [SerializeField] GameObject _settlementCanvas;       //결산 Canvas
    [SerializeField] ScrollRect _settlementScroll;       //결산 스크롤

    [Header( "----- 표시 -----" )]
    [SerializeField] TMP_Text _titleText;       //결산 제목

    [Header( "----- Content -----" )]
    [SerializeField] GameObject _dailyContent;       //일일 결산 Content
    [SerializeField] GameObject _weeklyContent;       //주간 결산 Content

    [Header( "----- 슬롯 -----" )]
    [SerializeField] SettlementSlotView _slotPrefab;       //기본 결산 슬롯 프리팹
    [SerializeField] SettlementProgressSlotView _progressSlotPrefab;       //진행 바 결산 슬롯 프리팹

    [Header( "----- 일일 결산 구역 -----" )]
    [SerializeField]
    List<SettlementSectionBinding> _dailySections =
        new List<SettlementSectionBinding>( );       //일일 결산 구역별 슬롯 정렬 영역

    [Header( "----- 주간 결산 구역 -----" )]
    [SerializeField]
    List<SettlementSectionBinding> _weeklySections =
        new List<SettlementSectionBinding>( );       //주간 결산 구역별 슬롯 정렬 영역

    [Header( "----- 결산 아이콘 -----" )]
    [SerializeField] SettlementIconData _iconData;       //결산 공용 아이콘 데이터

    [Header( "----- 입력 -----" )]
    [SerializeField] Button _confirmButton;       //결산 확인 버튼

    [Header( "----- 스크롤 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _confirmScrollDuration = 0.35f;       //확인 시 최하단 이동 시간

    Coroutine _scrollResetCoroutine;       //스크롤 위치 초기화 코루틴
    Tween _confirmScrollTween;       //확인 시 최하단 이동 트윈
    PoolManager _poolManager;       //공용 슬롯 풀 관리자

    Dictionary<SettlementSectionType, Transform> _dailySectionMap =
        new Dictionary<SettlementSectionType, Transform>( );       //일일 구역별 슬롯 정렬 영역

    Dictionary<SettlementSectionType, Transform> _weeklySectionMap =
        new Dictionary<SettlementSectionType, Transform>( );       //주간 구역별 슬롯 정렬 영역

    List<SettlementSlotView> _activeSlots =
        new List<SettlementSlotView>( );       //현재 표시 중인 결산 슬롯

    /// <summary>
    /// 결산 확인 입력 이벤트
    /// </summary>
    public event Action OnConfirmed;

    /// <summary>
    /// 결산 확인 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetConfirmTarget ( out RectTransform target )
    {
        target = _confirmButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 결산 슬롯 풀과 구역별 정렬 영역 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        _poolManager =
            GameManager.Instance.PoolManager;

        CreateSectionMap(
            _dailySections, _dailySectionMap );

        CreateSectionMap(
            _weeklySections, _weeklySectionMap );
    }

    /// <summary>
    /// 결산 입력 연결
    /// </summary>
    void Awake ()
    {
        //결산 확인 버튼에 공용 클릭 연출 연결
        _confirmButton.BindClickHighlight( );

        _confirmButton.onClick.AddListener( Confirm );
    }

    /// <summary>
    /// 결산 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        KillConfirmScrollTween( );
        _confirmButton.onClick.RemoveListener( Confirm );
    }

    /// <summary>
    /// 일일 결산 표시
    /// </summary>
    /// <param name="viewData">일일 결산 화면 표시 데이터</param>
    public void ShowDaily (
        SettlementPageViewData viewData )
    {
        ShowPage(
            viewData, _dailyContent, _weeklyContent, _dailySectionMap );
    }

    /// <summary>
    /// 주간 결산 표시
    /// </summary>
    /// <param name="viewData">주간 결산 화면 표시 데이터</param>
    public void ShowWeekly (
        SettlementPageViewData viewData )
    {
        ShowPage(
            viewData, _weeklyContent, _dailyContent, _weeklySectionMap );
    }

    /// <summary>
    /// 지정한 결산 구역을 화면 중앙으로 이동하고 강조 대상을 반환
    /// </summary>
    /// <param name="sectionType">이동할 결산 구역</param>
    /// <param name="target">강조할 결산 구역 RectTransform</param>
    /// <returns>결산 구역 조회 여부</returns>
    public bool TryFocusSection (
        SettlementSectionType sectionType,
        out RectTransform target )
    {
        target = null;

        Dictionary<SettlementSectionType, Transform> sectionMap =
            _dailyContent.activeSelf
                ? _dailySectionMap
                : _weeklySectionMap;

        if ( sectionMap.TryGetValue(
            sectionType, out Transform slotContent ) == false )
        {
            return false;
        }

        target = slotContent.parent as RectTransform;

        if ( target == null )
            target = slotContent as RectTransform;

        if ( target == null ) return false;

        FocusSection( target );
        return true;
    }

    /// <summary>
    /// 결산 구역이 보이도록 현재 스크롤 위치 조정
    /// </summary>
    /// <param name="target">화면에 표시할 결산 구역</param>
    void FocusSection ( RectTransform target )
    {
        if ( _scrollResetCoroutine != null )
        {
            StopCoroutine( _scrollResetCoroutine );
            _scrollResetCoroutine = null;
        }

        RectTransform content = _settlementScroll.content;
        RectTransform viewport = _settlementScroll.viewport;

        if ( viewport == null )
            viewport = _settlementScroll.transform as RectTransform;

        Canvas.ForceUpdateCanvases( );
        LayoutRebuilder.ForceRebuildLayoutImmediate( content );
        Canvas.ForceUpdateCanvases( );

        Bounds viewportBounds =
            new Bounds( viewport.rect.center, viewport.rect.size );
        Bounds contentBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport, content );
        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport, target );

        float hiddenHeight =
            contentBounds.size.y - viewportBounds.size.y;

        if ( hiddenHeight <= 0f ) return;

        float centerOffset =
            viewportBounds.center.y - targetBounds.center.y;

        _settlementScroll.StopMovement( );
        _settlementScroll.verticalNormalizedPosition = Mathf.Clamp01(
            _settlementScroll.verticalNormalizedPosition -
            centerOffset / hiddenHeight );
    }

    /// <summary>
    /// 결산 페이지와 구역별 슬롯 표시
    /// </summary>
    /// <param name="viewData">결산 페이지 표시 데이터</param>
    /// <param name="visibleContent">표시할 Content</param>
    /// <param name="hiddenContent">숨길 Content</param>
    /// <param name="sectionMap">구역별 슬롯 정렬 영역</param>
    void ShowPage (
        SettlementPageViewData viewData,
        GameObject visibleContent,
        GameObject hiddenContent,
        Dictionary<SettlementSectionType, Transform> sectionMap )
    {
        KillConfirmScrollTween( );
        _confirmButton.interactable = true;

        _settlementCanvas.SetActive( true );
        gameObject.SetActive( true );

        ResetCurrentScroll( );
        ClearSlots( );

        visibleContent.SetActive( true );
        hiddenContent.SetActive( false );

        _settlementScroll.content =
            ( RectTransform ) visibleContent.transform;

        _titleText.text = viewData.Title;

        CreateSlots(
            viewData.Sections, sectionMap );

        RefreshScroll( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 결산 구역별 슬롯 생성
    /// </summary>
    /// <param name="sections">결산 구역 표시 데이터</param>
    /// <param name="sectionMap">구역별 슬롯 정렬 영역</param>
    void CreateSlots (
        IReadOnlyList<SettlementSectionViewData> sections,
        Dictionary<SettlementSectionType, Transform> sectionMap )
    {
        for ( int i = 0; i < sections.Count; i++ )
        {
            SettlementSectionViewData section =
                sections [ i ];

            if ( sectionMap.TryGetValue(
                section.SectionType, out Transform slotContent ) == false )
            {
                Debug.LogWarning(
                    $"결산 슬롯 정렬 영역이 없습니다: " +
                    $"{section.SectionType}" );
                continue;
            }

            for ( int j = 0; j < section.Slots.Count; j++ )
            {
                CreateSlot( section.Slots [ j ], slotContent );
            }
        }
    }

    /// <summary>
    /// 결산 슬롯 하나 생성
    /// </summary>
    /// <param name="viewData">결산 슬롯 표시 데이터</param>
    /// <param name="slotContent">구역 타이틀 아래 슬롯 정렬 영역</param>
    void CreateSlot (
        SettlementSlotViewData viewData,
        Transform slotContent )
    {
        SettlementSlotView prefab = _slotPrefab;

        if ( viewData.ShowProgress )
            prefab = _progressSlotPrefab;

        GameObject instance =
            _poolManager.GetFromPool(
                prefab.gameObject, slotContent );

        SettlementSlotView slotView =
            instance.GetComponent<SettlementSlotView>( );

        slotView.Init(
            viewData,
            _iconData.GetIcon( viewData.IconType ) );

        _activeSlots.Add( slotView );
    }

    /// <summary>
    /// 현재 결산 슬롯 전체 반환
    /// </summary>
    void ClearSlots ()
    {
        for ( int i = 0; i < _activeSlots.Count; i++ )
        {
            SettlementSlotView slotView =
                _activeSlots [ i ];

            slotView.ResetView( );
            slotView
                .GetComponent<Poolable>( )
                .ReturnToPool( );
        }

        _activeSlots.Clear( );
    }

    /// <summary>
    /// 결산 구역별 슬롯 정렬 영역 구성
    /// </summary>
    /// <param name="bindings">Inspector 구역 연결 목록</param>
    /// <param name="sectionMap">구성할 구역 딕셔너리</param>
    void CreateSectionMap (
        IReadOnlyList<SettlementSectionBinding> bindings,
        Dictionary<SettlementSectionType, Transform> sectionMap )
    {
        sectionMap.Clear( );

        for ( int i = 0; i < bindings.Count; i++ )
        {
            SettlementSectionBinding binding =
                bindings [ i ];

            sectionMap [ binding.SectionType ] =
                binding.SlotContent;
        }
    }

    /// <summary>
    /// 현재 Content의 스크롤 위치 초기화
    /// </summary>
    void ResetCurrentScroll ()
    {
        _settlementScroll.StopMovement( );
        _settlementScroll.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 결산 레이아웃과 스크롤 위치 갱신
    /// </summary>
    void RefreshScroll ()
    {
        //이전 스크롤 초기화가 남아 있으면 중단
        if ( _scrollResetCoroutine != null )
            StopCoroutine( _scrollResetCoroutine );

        //이전 Content의 스크롤 위치와 이동 상태 초기화
        _settlementScroll.StopMovement( );
        _settlementScroll.verticalNormalizedPosition = 1f;

        _scrollResetCoroutine = StartCoroutine( ResetScrollPosition( ) );
    }

    /// <summary>
    /// 레이아웃 계산 후 스크롤을 최상단으로 이동
    /// </summary>
    IEnumerator ResetScrollPosition ()
    {
        //텍스트와 Content Size Fitter의 첫 레이아웃 계산 완료 대기
        yield return new WaitForEndOfFrame( );

        //활성 Content의 최종 크기 반영
        Canvas.ForceUpdateCanvases( );
        LayoutRebuilder.ForceRebuildLayoutImmediate( _settlementScroll.content );
        Canvas.ForceUpdateCanvases( );

        //ScrollRect의 Content 경계 갱신 후 최상단으로 이동
        _settlementScroll.StopMovement( );
        _settlementScroll.verticalNormalizedPosition = 1f;

        //ScrollRect의 LateUpdate 이후 위치 재확정
        yield return new WaitForEndOfFrame( );
        _settlementScroll.StopMovement( );
        _settlementScroll.verticalNormalizedPosition = 1f;
        _scrollResetCoroutine = null;
    }

    /// <summary>
    /// 결산 화면 숨김
    /// </summary>
    public void Hide ( Action onComplete = null )
    {
        KillConfirmScrollTween( );
        _panelTween.Hide( () => CompleteHide( onComplete ) );
    }

    /// <summary>
    /// 결산 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( Action onComplete )
    {
        ClearSlots( );

        _settlementCanvas.SetActive( false );
        gameObject.SetActive( false );
        onComplete?.Invoke( );
    }

    /// <summary>
    /// 결산 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        KillConfirmScrollTween( );

        if ( _scrollResetCoroutine != null )
        {
            StopCoroutine( _scrollResetCoroutine );
            _scrollResetCoroutine = null;
        }

        ClearSlots( );

        _panelTween.SetVisible( false );
        _settlementCanvas.SetActive( false );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 결산 확인 입력 중계
    /// </summary>
    void Confirm ()
    {
        if ( _confirmButton.interactable == false ) return;

        //레이아웃 초기화가 남아 있으면 확인 스크롤보다 먼저 종료
        if ( _scrollResetCoroutine != null )
        {
            StopCoroutine( _scrollResetCoroutine );
            _scrollResetCoroutine = null;
        }

        _confirmButton.interactable = false;
        _settlementScroll.StopMovement( );
        KillConfirmScrollTween( );

        _confirmScrollTween = DOTween.To(
                () => _settlementScroll.verticalNormalizedPosition,
                value => _settlementScroll.verticalNormalizedPosition = value,
                0f,
                _confirmScrollDuration )
            .SetEase( Ease.InOutQuad )
            .SetUpdate( true )
            .OnComplete( CompleteConfirmScroll );
    }

    /// <summary>
    /// 최하단 이동 완료 후 결산 확인 전달
    /// </summary>
    void CompleteConfirmScroll ()
    {
        _confirmScrollTween = null;
        OnConfirmed?.Invoke( );
    }

    /// <summary>
    /// 실행 중인 결산 확인 스크롤 트윈 제거
    /// </summary>
    void KillConfirmScrollTween ()
    {
        if ( _confirmScrollTween == null ) return;

        _confirmScrollTween.Kill( );
        _confirmScrollTween = null;
    }
}
