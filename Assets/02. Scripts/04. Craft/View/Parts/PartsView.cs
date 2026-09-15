using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 배치 파츠 뷰 - 파츠 표시와 선택, 드래그 입력 전달
/// </summary>
public class PartsView : MonoBehaviour,
    IPointerDownHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _renderer;                 //파츠 이미지
    [SerializeField] Outline _selectionOutline;       //선택 강조

    [Header( "----- 시각 연출 -----" )]
    [SerializeField, Range( 0f, 1f )]
    float _appearScale = 0.6f;       //신규 배치 시작 크기
    [SerializeField, Min( 0f )]
    float _appearDuration = 0.2f;       //신규 배치 등장 시간
    [SerializeField, Min( 0f )]
    float _settleScale = 0.12f;       //입력 종료 안착 크기
    [SerializeField, Min( 0f )]
    float _settleDuration = 0.18f;       //입력 종료 안착 시간
    [SerializeField, Min( 0f )]
    float _scaleSettleDelay = 0.12f;       //휠 입력 종료 판정 시간

    RectTransform _rectTransform;                     //배치 RectTransform
    int _placementNumber;                             //배치 번호
    bool _isSelected;                                 //선택 여부
    bool _isDragging;                                 //드래그 여부
    bool _isPinching;                                 //핀치 입력 여부
    float _previousPinchDistance;                     //이전 두 터치 간 거리
    bool _isScaleSettlePending;       //크기 입력 종료 대기 여부
    float _scaleSettleTimer;       //크기 입력 종료 대기 시간
    Tween _visualTween;       //현재 파츠 시각 연출

    /// <summary>
    /// 파츠 선택 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnSelected;

    /// <summary>
    /// 파츠 드래그 시작 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnDragStarted;

    /// <summary>
    /// 파츠 드래그 이동 이벤트(배치 번호, 화면 좌표)
    /// </summary>
    public event Action<int, Vector2> OnDragged;

    /// <summary>
    /// 파츠 드래그 종료 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnDragEnded;

    /// <summary>
    /// 파츠 스케일 입력 이벤트(배치 번호, 휠 방향)
    /// </summary>
    public event Action<int, float> OnScaleInput;

    /// <summary>
    /// 파츠 핀치 스케일 입력 이벤트(배치 번호, 이전 프레임 대비 거리 비율)
    /// </summary>
    public event Action<int, float> OnPinchScaleInput;

    /// <summary>
    /// 파츠 제거 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnRemoved;

    /// <summary>
    /// 비활성 상태에서도 사용할 파츠 시각 상태 초기화
    /// </summary>
    void InitializeRuntime ()
    {
        if ( _rectTransform != null ) return;

        _rectTransform = transform as RectTransform;
        _rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 실행 중인 파츠 시각 연출 제거
    /// </summary>
    void KillVisualTween ()
    {
        if ( _visualTween == null ) return;

        _visualTween.Kill( );
        _visualTween = null;
    }

    /// <summary>
    /// 파츠 시각 연출 상태 즉시 복구
    /// </summary>
    public void ResetVisual ()
    {
        InitializeRuntime( );
        KillVisualTween( );

        _isScaleSettlePending = false;
        _scaleSettleTimer = 0f;
        _rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 파츠 시각 연출 완료 처리
    /// </summary>
    void CompleteVisual ()
    {
        _visualTween = null;
        _rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 신규 배치 파츠 등장 연출
    /// </summary>
    void PlayAppear ()
    {
        ResetVisual( );

        _rectTransform.localScale =
            Vector3.one * _appearScale;

        _visualTween =
            _rectTransform
                .DOScale(
                    Vector3.one,
                    _appearDuration )
                .SetEase( Ease.OutBack )
                .SetUpdate( true )
                .OnComplete( CompleteVisual );
    }

    /// <summary>
    /// 이동 또는 크기 변경 종료 안착 연출
    /// </summary>
    void PlaySettle ()
    {
        ResetVisual( );

        _visualTween =
            _rectTransform
                .DOPunchScale(
                    Vector3.one * _settleScale,
                    _settleDuration,
                    4,
                    0.5f )
                .SetUpdate( true )
                .OnComplete( CompleteVisual );
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    void Awake ()
    {
        InitializeRuntime( );
        SetSelected( false );
    }

    /// <summary>
    /// 비활성화 시 드래그 상태 초기화
    /// </summary>
    void OnDisable ()
    {
        _isDragging = false;
        _isPinching = false;
        _previousPinchDistance = 0f;
        ResetVisual( );
    }

    /// <summary>
    /// 모바일 핀치와 연속 휠 입력 처리
    /// </summary>
    void Update ()
    {
        UpdatePinchInput( );

        if ( _isScaleSettlePending == false )
            return;

        _scaleSettleTimer -=
            Time.unscaledDeltaTime;

        if ( _scaleSettleTimer > 0f )
            return;

        _isScaleSettlePending = false;
        PlaySettle( );
    }

    /// <summary>
    /// 선택 파츠의 두 손가락 핀치 입력 처리
    /// </summary>
    void UpdatePinchInput ()
    {
        if ( _isSelected == false || Input.touchCount != 2 )
        {
            EndPinchInput( );
            return;
        }

        Touch firstTouch = Input.GetTouch( 0 );
        Touch secondTouch = Input.GetTouch( 1 );
        float currentDistance = Vector2.Distance(
            firstTouch.position, secondTouch.position );

        if ( currentDistance <= Mathf.Epsilon ) return;

        if ( _isPinching == false )
        {
            StartPinchInput( currentDistance );
            return;
        }

        float scaleRatio = currentDistance / _previousPinchDistance;
        _previousPinchDistance = currentDistance;

        if ( Mathf.Approximately( scaleRatio, 1f ) ) return;

        OnPinchScaleInput?.Invoke( _placementNumber, scaleRatio );
    }

    /// <summary>
    /// 핀치 입력 시작
    /// </summary>
    /// <param name="pinchDistance">두 터치 간 시작 거리</param>
    void StartPinchInput ( float pinchDistance )
    {
        if ( _isDragging )
        {
            _isDragging = false;
            OnDragEnded?.Invoke( _placementNumber );
        }

        KillVisualTween( );
        _rectTransform.localScale = Vector3.one;
        _isScaleSettlePending = false;
        _isPinching = true;
        _previousPinchDistance = pinchDistance;
    }

    /// <summary>
    /// 핀치 입력 종료 및 안착 연출 예약
    /// </summary>
    void EndPinchInput ()
    {
        if ( _isPinching == false ) return;

        _isPinching = false;
        _previousPinchDistance = 0f;
        _scaleSettleTimer = _scaleSettleDelay;
        _isScaleSettlePending = true;
    }

    /// <summary>
    /// 배치 파츠 초기화
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="icon">파츠 아이콘</param>
    /// <param name="localPosition">로컬 위치</param>
    /// <param name="rotation">회전값</param>
    /// <param name="scale">균등 스케일</param>
    /// <param name="partIndex">앞뒤 순서 인덱스</param>
    /// <param name="playAppear">신규 배치 등장 연출 여부</param>
    public void Init (
        int placementNumber, Sprite icon, Vector2 localPosition,
        float rotation, float scale, int partIndex,
        bool playAppear )
    {
        ResetVisual( );

        //배치 번호 설정
        _placementNumber = placementNumber;
        //아이콘 설정
        _renderer.sprite = icon;

        //트랜스폼 설정
        SetPosition( localPosition );
        SetRotation( rotation );
        SetScale( scale );

        //인덱스 갱신
        SetPartIndex( partIndex );
        //선택 여부
        SetSelected( false );

        _isDragging = false;

        if ( playAppear )
            PlayAppear( );
    }

    #region ----- 표시 갱신 -----

    /// <summary>
    /// 파츠 아이콘 갱신
    /// </summary>
    /// <param name="icon">변경할 파츠 아이콘</param>
    public void SetIcon ( Sprite icon )
    {
        _renderer.sprite = icon;
    }

    /// <summary>
    /// 파츠 로컬 위치 갱신
    /// </summary>
    /// <param name="localPosition">변경할 로컬 위치</param>
    public void SetPosition ( Vector2 localPosition )
    {
        _rectTransform.anchoredPosition = localPosition;
    }

    /// <summary>
    /// 파츠 회전값 갱신
    /// </summary>
    /// <param name="rotation">변경할 회전값</param>
    public void SetRotation ( float rotation )
    {
        _rectTransform.localEulerAngles = new Vector3( 0f, 0f, rotation );
    }

    /// <summary>
    /// 파츠 균등 스케일 갱신
    /// </summary>
    /// <param name="scale">변경할 균등 스케일</param>
    public void SetScale ( float scale )
    {
        //파츠 루트가 아닌 이미지 스케일 갱신
        _renderer.rectTransform.localScale =
            new Vector3( scale, scale, 1f );
    }

    /// <summary>
    /// 파츠 앞뒤 표시 순서 갱신
    /// </summary>
    /// <param name="partIndex">앞뒤 순서 인덱스</param>
    public void SetPartIndex ( int partIndex )
    {
        _rectTransform.SetSiblingIndex( partIndex );
    }

    /// <summary>
    /// 파츠를 임시로 가장 앞에 표시
    /// </summary>
    public void ShowInFront ()
    {
        _rectTransform.SetAsLastSibling( );
    }

    /// <summary>
    /// 파츠 선택 표시 갱신
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected ( bool isSelected )
    {
        _isSelected = isSelected;
        _selectionOutline.enabled = isSelected;
    }

    #endregion

    #region ----- 선택/드래그 -----

    /// <summary>
    /// 파츠 선택 입력 전달
    /// </summary>
    public void OnPointerDown ( PointerEventData eventData )
    {
        if ( eventData.button != PointerEventData.InputButton.Left ) return;

        if ( _isDragging ) return;

        //선택 이벤트 발행(배치 번호)
        OnSelected?.Invoke( _placementNumber );
    }

    /// <summary>
    /// 파츠 클릭 입력 전달
    /// </summary>
    public void OnPointerClick ( PointerEventData eventData )
    {
        //왼쪽 클릭은 부모의 빈 영역 클릭 처리만 차단
        if ( eventData.button == PointerEventData.InputButton.Left )
        {
            eventData.Use( );
            return;
        }

        //오른쪽 클릭이면 파츠 제거 입력 전달
        if ( eventData.button == PointerEventData.InputButton.Right )
        {
            eventData.Use( );
            OnRemoved?.Invoke( _placementNumber );
        }
    }

    /// <summary>
    /// 선택 파츠의 마우스 휠 입력 전달
    /// </summary>
    public void OnScroll ( PointerEventData eventData )
    {
        //선택되지 않았거나 휠 입력이 없으면 종료
        if ( _isSelected == false ||
            Mathf.Approximately( eventData.scrollDelta.y, 0f ) )
            return;

        //연속 휠 입력 중에는 연출을 생성하지 않음
        KillVisualTween( );
        _rectTransform.localScale = Vector3.one;

        //스케일 입력 발행(배치 번호, 휠 방향)
        OnScaleInput?.Invoke( _placementNumber, eventData.scrollDelta.y );

        //마지막 입력 이후 일정 시간이 지나면 한 번만 안착
        _scaleSettleTimer = _scaleSettleDelay;
        _isScaleSettlePending = true;
    }

    /// <summary>
    /// 파츠 드래그 시작 입력 전달
    /// </summary>
    public void OnBeginDrag ( PointerEventData eventData )
    {
        if ( eventData.button != PointerEventData.InputButton.Left ) return;

        //선택된 파츠만 드래그 가능
        if ( _isSelected == false || _isDragging ||
            _isPinching || Input.touchCount > 1 )
            return;

        //드래그 중에는 이전 등장이나 안착 연출을 유지하지 않음
        ResetVisual( );

        _isDragging = true;
        OnDragStarted?.Invoke( _placementNumber );
    }

    /// <summary>
    /// 파츠 드래그 이동 입력 전달
    /// </summary>
    public void OnDrag ( PointerEventData eventData )
    {
        if ( _isDragging == false || _isPinching ||
            Input.touchCount > 1 )
            return;

        OnDragged?.Invoke( _placementNumber, eventData.position );
    }

    /// <summary>
    /// 파츠 드래그 종료 입력 전달
    /// </summary>
    public void OnEndDrag ( PointerEventData eventData )
    {
        if ( _isDragging == false )
            return;

        _isDragging = false;

        //프레젠터의 위치와 앞뒤 순서 반영이 먼저 완료됨
        OnDragEnded?.Invoke( _placementNumber );

        //최종 저장 위치에서 안착 연출 재생
        PlaySettle( );
    }

    #endregion
}
