using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파츠 선택 뷰 - 보유 파츠 슬롯 표시와 선택 입력 전달
/// </summary>
public class PartsSelectView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Transform _content;        //파츠 슬롯 생성 위치
    [SerializeField] ItemSlotView _slotPrefab;      //아이템 슬롯 프리팹
    [SerializeField] ScrollRect _scrollRect;        //파츠 목록 스크롤
    [SerializeField] TMP_Dropdown _sortDropdown;       //파츠 목록 정렬 선택
    [SerializeField] Button _openCloseButton;       //패널 열기, 닫기 버튼
    [SerializeField] TMP_Text _openCloseButtonText;      //패널 열기, 닫기 버튼 문구
    TooltipView _tooltipView;      //Canvas 공용 파츠 정보 툴팁

    [Header( "----- 패널 이동 -----" )]
    [SerializeField] float _closedOffsetX = 600f;       //닫힘 상태 X축 이동 거리
    [SerializeField, Min( 0f )]
    float _slideDuration = 0.25f;       //패널이 좌우로 이동하는 시간

    RectTransform _rectTransform;       //파츠 선택 패널 RectTransform
    Vector2 _openPosition;      //열림 상태 위치
    Tween _panelTween;       //현재 실행 중인 패널 이동 트윈
    bool _isOpen;       //현재 파츠 선택 패널 열림 여부
    bool _isInitialized;       //런타임 초기화 여부

    static readonly string [ ] _sortOptions =
    {
        "파츠 타입 순",
        "이름 순",
        "수량 많은 순",
        "코스트 낮은 순",
    };

    /// <summary>
    /// 파츠 아이디별 슬롯 뷰
    /// </summary>
    Dictionary<string, ItemSlotView> _slotViews = new Dictionary<string, ItemSlotView>( );

    /// <summary>
    /// 파츠 아이디별 툴팁 설명
    /// </summary>
    Dictionary<string, string> _tooltipDescriptions = new Dictionary<string, string>( );

    /// <summary>
    /// 파츠 선택 이벤트(파츠 아이디)
    /// </summary>
    public event Action<string> OnPartSelected;

    /// <summary>
    /// 파츠 목록 정렬 선택 이벤트
    /// </summary>
    public event Action<int> OnSortSelected;

    /// <summary>
    /// 패널 열기, 닫기 이벤트
    /// </summary>
    public event Action OnOpenClose;

    /// <summary>
    /// 패널 열기 또는 닫기 연출 완료 이벤트
    /// </summary>
    public event Action<bool> OnPanelTransitionCompleted;

    /// <summary>
    /// 현재 파츠 선택 패널 열림 여부
    /// </summary>
    public bool IsOpen => _isOpen;

    /// <summary>
    /// 파츠 선택 패널 열기 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">패널 열기 버튼 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetOpenCloseTarget ( out RectTransform target )
    {
        target = _openCloseButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 지정 파츠 슬롯의 튜토리얼 강조 대상 조회
    /// </summary>
    /// <param name="partId">조회할 파츠 아이디</param>
    /// <param name="target">파츠 슬롯 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetSlotTarget (
        string partId, out RectTransform target )
    {
        if ( _slotViews.TryGetValue(
            partId, out ItemSlotView slotView ) == false )
        {
            target = null;
            return false;
        }

        target = slotView.transform as RectTransform;

        if ( target == null ) return false;

        MoveSlotIntoViewport( target );
        return true;
    }

    /// <summary>
    /// 화면 밖 파츠 슬롯이 보이도록 스크롤 위치 조정
    /// </summary>
    /// <param name="target">이동할 파츠 슬롯</param>
    void MoveSlotIntoViewport ( RectTransform target )
    {
        if ( _scrollRect == null ||
            _scrollRect.content == null ||
            _scrollRect.viewport == null )
        {
            return;
        }

        Canvas.ForceUpdateCanvases( );
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            _scrollRect.content );
        Canvas.ForceUpdateCanvases( );

        RectTransform viewport = _scrollRect.viewport;
        Bounds contentBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport, _scrollRect.content );
        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport, target );

        float hiddenHeight =
            contentBounds.size.y - viewport.rect.height;

        if ( hiddenHeight <= 0f ) return;

        float centerOffset =
            viewport.rect.center.y - targetBounds.center.y;

        _scrollRect.StopMovement( );
        _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            _scrollRect.verticalNormalizedPosition -
            centerOffset / hiddenHeight );

        Canvas.ForceUpdateCanvases( );
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 패널 위치와 입력 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        if ( _isInitialized ) return;

        //패널 위치 기준과 열림 위치 저장
        _rectTransform = GetComponent<RectTransform>( );
        _openPosition = _rectTransform.anchoredPosition;

        Canvas canvas =
            GetComponentInParent<Canvas>( true );

        //파츠 슬롯도 루트 Canvas의 공용 툴팁 사용
        _tooltipView =
            canvas.rootCanvas
                .GetComponentInChildren<TooltipView>( true );

        //패널 열기 닫기 입력 연결
        _openCloseButton.onClick.AddListener( OpenClosePanel );

        //Inspector에 연결된 정렬 드롭다운의 이전 옵션 교체
        if ( _sortDropdown != null )
        {
            _sortDropdown.SetOptions( _sortOptions );
            _sortDropdown.onValueChanged.AddListener( SelectSort );
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 비활성화 시 실행 중인 패널 이동 연출 제거
    /// </summary>
    void OnDisable ()
    {
        KillPanelTween( );
    }

    /// <summary>
    /// 파츠 선택 패널 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        KillPanelTween( );

        //패널 열기, 닫기 입력 해제
        _openCloseButton.onClick.RemoveListener( OpenClosePanel );

        if ( _sortDropdown != null )
            _sortDropdown.onValueChanged.RemoveListener( SelectSort );

        //생성된 파츠 슬롯 제거
        ClearParts( );
    }

    #region ----- 파츠 슬롯 -----
    /// <summary>
    /// 파츠 슬롯 생성
    /// </summary>
    /// <param name="viewData">파츠 슬롯 표시 데이터</param>
    void CreatePart ( ItemSlotViewData viewData )
    {
        //데이터나 파츠 아이디가 없으면 종료
        if ( viewData == null ||
            string.IsNullOrWhiteSpace( viewData.ItemId ) )
            return;

        //남은 수량이 없거나 같은 파츠 슬롯이 있으면 종료
        if ( viewData.Quantity <= 0 ||
            _slotViews.ContainsKey( viewData.ItemId ) )
            return;

        //파츠 슬롯 생성
        ItemSlotView slotView = Instantiate( _slotPrefab, _content );

        //파츠 초기화
        slotView.Init( viewData );
        //파츠 선택과 툴팁 입력 구독
        slotView.OnSelected += SelectPart;
        slotView.OnPointerEntered += ShowPartTooltip;
        slotView.OnPointerExited += HidePartTooltip;

        //파츠 아이디 기준으로 슬롯과 툴팁 설명 저장
        _slotViews.Add( viewData.ItemId, slotView );
        _tooltipDescriptions.Add( viewData.ItemId, viewData.Description );
    }

    /// <summary>
    /// 파츠 슬롯 제거
    /// </summary>
    /// <param name="partId">제거할 파츠 아이디</param>
    void RemovePart ( string partId )
    {
        //파츠 슬롯 가져오기
        if ( _slotViews.TryGetValue( partId, out ItemSlotView slotView ) == false )
            return;

        //파츠 선택과 툴팁 입력 해제
        slotView.OnSelected -= SelectPart;
        slotView.OnPointerEntered -= ShowPartTooltip;
        slotView.OnPointerExited -= HidePartTooltip;

        //슬롯 목록과 오브젝트 제거
        _slotViews.Remove( partId );
        _tooltipDescriptions.Remove( partId );
        Destroy( slotView.gameObject );

        //제거한 슬롯 툴팁 숨김
        HidePartTooltip( );
    }

    /// <summary>
    /// 파츠 선택 입력 중계
    /// </summary>
    /// <param name="slotId">슬롯 아이디</param>
    /// <param name="partId">파츠 아이디</param>
    void SelectPart ( string slotId, string partId )
    {
        OnPartSelected?.Invoke( partId );
    }

    /// <summary>
    /// 파츠 정보 툴팁 표시
    /// </summary>
    /// <param name="partId">파츠 아이디</param>
    /// <param name="tooltipPoint">툴팁 표시 위치</param>
    void ShowPartTooltip ( string partId, RectTransform tooltipPoint )
    {
        if ( _tooltipDescriptions.TryGetValue( partId, out string description ) )
            _tooltipView.Show( description, tooltipPoint );
    }

    /// <summary>
    /// 파츠 정보 툴팁 숨김
    /// </summary>
    void HidePartTooltip ()
    {
        //씬 종료 중 제거된 공용 툴팁에는 접근하지 않음
        if ( _tooltipView != null )
            _tooltipView.Hide( );
    }

    /// <summary>
    /// 파츠 슬롯 목록 표시
    /// </summary>
    /// <param name="viewDatas">파츠 슬롯 표시 데이터 목록</param>
    public void ShowParts ( IReadOnlyList<ItemSlotViewData> viewDatas )
    {
        //기존 목록이 있으면 현재 스크롤 위치 저장
        bool hasCurrentParts = _slotViews.Count > 0;
        float scrollPosition = _scrollRect != null
            ? _scrollRect.verticalNormalizedPosition
            : 1f;

        //기존 파츠 슬롯 제거
        ClearParts( );

        //표시할 파츠가 없으면 종료
        if ( viewDatas == null || viewDatas.Count == 0 ) return;

        //파츠별 슬롯 생성
        for ( int i = 0; i < viewDatas.Count; i++ )
        {
            CreatePart( viewDatas [ i ] );
        }

        //첫 목록은 맨 위, 갱신 목록은 기존 스크롤 위치로 이동
        if ( _scrollRect != null )
            _scrollRect.verticalNormalizedPosition = hasCurrentParts
                ? scrollPosition
                : 1f;
    }

    /// <summary>
    /// 단일 파츠 슬롯 갱신
    /// </summary>
    /// <param name="viewData">파츠 슬롯 표시 데이터</param>
    public void UpdatePart ( ItemSlotViewData viewData )
    {
        //데이터 또는 파츠 아이디가 없으면 종료
        if ( viewData == null ||
            string.IsNullOrWhiteSpace( viewData.ItemId ) )
            return;

        //남은 수량이 없으면 슬롯 제거
        if ( viewData.Quantity <= 0 )
        {
            RemovePart( viewData.ItemId );
            return;
        }

        //기존 슬롯이 있으면 표시 갱신
        if ( _slotViews.TryGetValue( viewData.ItemId, out ItemSlotView slotView ) )
        {
            _tooltipDescriptions [ viewData.ItemId ] = viewData.Description;
            slotView.UpdateView( viewData );
            return;
        }

        //파츠 슬롯 생성
        CreatePart( viewData );
    }

    /// <summary>
    /// 생성된 파츠 슬롯 전체 제거
    /// </summary>
    public void ClearParts ()
    {
        //생성된 파츠 슬롯 순회
        foreach ( ItemSlotView slotView in _slotViews.Values )
        {
            //파츠 선택과 툴팁 입력 해제 후 오브젝트 제거
            slotView.OnSelected -= SelectPart;
            slotView.OnPointerEntered -= ShowPartTooltip;
            slotView.OnPointerExited -= HidePartTooltip;
            Destroy( slotView.gameObject );
        }

        _slotViews.Clear( );
        _tooltipDescriptions.Clear( );
        HidePartTooltip( );
    }
    #endregion

    #region ----- 패널 표시 -----
    /// <summary>
    /// 파츠 목록 정렬 입력 전달
    /// </summary>
    /// <param name="sortIndex">선택한 정렬 번호</param>
    void SelectSort ( int sortIndex )
    {
        OnSortSelected?.Invoke( sortIndex );
    }

    /// <summary>
    /// 패널 열기 닫기 입력 중계
    /// </summary>
    void OpenClosePanel ()
    {
        OnOpenClose?.Invoke( );
    }

    /// <summary>
    /// 파츠 선택 패널 열림 상태 설정
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    /// <param name="playAnimation">좌우 이동 연출 재생 여부</param>
    public void SetPanelOpen (
        bool isOpen, bool playAnimation = true )
    {
        _isOpen = isOpen;

        //열림 상태 위치 가져오기
        Vector2 panelPosition = _openPosition;

        //닫힘 상태면 오른쪽으로 이동
        if ( isOpen == false )
            panelPosition.x += _closedOffsetX;

        KillPanelTween( );

        if ( playAnimation )
        {
            //현재 위치에서 열림 또는 닫힘 위치까지 좌우로 이동합니다.
            _panelTween = _rectTransform
                .DOAnchorPos( panelPosition, _slideDuration )
                .SetEase( isOpen ? Ease.OutCubic : Ease.InCubic )
                .SetUpdate( true )
                .OnComplete( CompletePanelTween );
        }
        else
        {
            _rectTransform.anchoredPosition = panelPosition;
            OnPanelTransitionCompleted?.Invoke( _isOpen );
        }

        //현재 상태에 맞는 버튼 문구 표시
        _openCloseButtonText.text = isOpen ? "닫기" : "열 기";
    }

    /// <summary>
    /// 패널 이동 완료 처리
    /// </summary>
    void CompletePanelTween ()
    {
        _panelTween = null;
        OnPanelTransitionCompleted?.Invoke( _isOpen );
    }

    /// <summary>
    /// 실행 중인 패널 이동 연출 제거
    /// </summary>
    void KillPanelTween ()
    {
        if ( _panelTween == null ) return;

        _panelTween.Kill( );
        _panelTween = null;
    }
    #endregion
}
