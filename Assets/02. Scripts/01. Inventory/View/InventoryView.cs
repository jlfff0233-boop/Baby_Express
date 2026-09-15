using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 뷰 - 아이템 슬롯과 상세 패널 표시 및 입력 전달
/// </summary>
public class InventoryView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;             //인벤토리 패널 연출

    [Header ( "--- 아이템 목록 ---" )]
    [SerializeField] Transform _content;                    //아이템 슬롯 생성 위치
    [SerializeField] ItemSlotView _slotPrefab;              //아이템 슬롯 프리팹
    [SerializeField] ScrollRect _scrollRect;                //아이템 목록 스크롤

    [Header ( "--- 상세 패널 ---" )]
    [SerializeField] InventoryDetailView _detailView;       //인벤토리 상세 뷰
    [SerializeField] InventoryItemSellView _sellView;       //아이템 판매 뷰
    [SerializeField] InventoryItemDeleteView _deleteView;   //아이템 삭제 뷰

    [Header ( "--- 카테고리 ---" )]
    [SerializeField] Button _allButton;                     //전체 아이템 선택 버튼
    [SerializeField] Button _partButton;                    //파츠 선택 버튼
    [SerializeField] Button _facilityButton;                //시설 선택 버튼
    [SerializeField] Button _consumableButton;              //소모용품 선택 버튼
    [SerializeField] TMP_Dropdown _filterDropdown;          //현재 카테고리 분류 선택
    [SerializeField] TMP_Dropdown _sortDropdown;            //아이템 정렬 선택

    [Header ( "--- 인벤토리 정보 ---" )]
    [SerializeField] TMP_Text _capacityText;                //사용 슬롯과 최대 슬롯 표시
    [SerializeField] Button _closeButton;                   //인벤토리 닫기 버튼

    /// <summary>
    /// 전체 옵션
    /// </summary>
    static readonly string [ ] _allOptions =
    {
        "전체",
    };

    /// <summary>
    /// 시설 옵션
    /// </summary>
    static readonly string [ ] _facilityOptions =
    {
        "시설 전체",
        "장비",
        "가구",
        "확장",
    };

    /// <summary>
    /// 소모용품 옵션
    /// </summary>
    static readonly string [ ] _consumableOptions =
    {
        "소모용품 전체",
    };

    /// <summary>
    /// 정렬 옵션
    /// </summary>
    static readonly string [ ] _sortOptions =
    {
        "기본",
        "이름 오름차순",
        "이름 내림차순",
        "가격 낮은 순",
        "가격 높은 순",
        "수량 적은 순",
        "수량 많은 순",
    };

    /// <summary>
    /// 슬롯 딕셔너리(아이디, 아이템 슬롯)
    /// </summary>
    Dictionary<string , ItemSlotView> _slotViews = new Dictionary<string , ItemSlotView> ( );
    List<string> _partOptions;                                 //파츠 분류 옵션
    InventoryCategoryType _selectedCategory;                   //현재 메인 카테고리
    PoolManager _poolManager;                                  //게임 공용 오브젝트 풀 매니저

    /// <summary>
    /// 인벤토리 화면 표시 여부
    /// </summary>
    public bool IsShowing => gameObject.activeInHierarchy;

    #region ----- 이벤트 -----
    /// <summary>
    /// 아이템 슬롯 선택 이벤트(슬롯 아이디, 아이템 아이디)
    /// </summary>
    public event Action<string , string> OnSlotSelected;

    /// <summary>
    /// 전체 아이템 선택 이벤트
    /// </summary>
    public event Action OnAllSelected;

    /// <summary>
    /// 파츠 타입 선택 이벤트(타입)
    /// </summary>
    public event Action<int> OnPartSelected;

    /// <summary>
    /// 시설 타입 선택 이벤트(타입)
    /// </summary>
    public event Action<int> OnFacilitySelected;

    /// <summary>
    /// 소모용품 타입 선택 이벤트(타입)
    /// </summary>
    public event Action<int> OnConsumableSelected;

    /// <summary>
    /// 아이템 정렬 선택 이벤트(타입)
    /// </summary>
    public event Action<int> OnSortSelected;

    /// <summary>
    /// 상점 이동 이벤트(선택한 아이템 아이디)
    /// </summary>
    public event Action<string> OnMoveToShop;

    /// <summary>
    /// 아이템 사용 이벤트(선택한 아이템 아이디)
    /// </summary>
    public event Action<string> OnUse;

    /// <summary>
    /// 제작 이동 요청 이벤트(선택한 아이템 아이디)
    /// </summary>
    public event Action<string> OnMoveToCraft;

    /// <summary>
    /// 아이템 판매 패널 열기 이벤트(슬롯 아이디)
    /// </summary>
    public event Action<string> OnSell;

    /// <summary>
    /// 아이템 삭제 패널 열기 이벤트(슬롯 아이디)
    /// </summary>
    public event Action<string> OnDelete;

    /// <summary>
    /// 판매 수량 변경 이벤트
    /// </summary>
    public event Action<int> OnSellQuantityChanged;

    /// <summary>
    /// 판매 수량 직접 설정 이벤트
    /// </summary>
    public event Action<string> OnSellQuantitySet;

    /// <summary>
    /// 최소 판매 수량 이벤트
    /// </summary>
    public event Action OnSellMin;

    /// <summary>
    /// 최대 판매 수량 이벤트
    /// </summary>
    public event Action OnSellMax;

    /// <summary>
    /// 판매 확정 이벤트
    /// </summary>
    public event Action OnSellConfirmed;

    /// <summary>
    /// 판매 패널 닫기 이벤트
    /// </summary>
    public event Action OnSellClosed;

    /// <summary>
    /// 삭제 수량 변경 이벤트
    /// </summary>
    public event Action<int> OnDeleteQuantityChanged;

    /// <summary>
    /// 삭제 수량 직접 설정 이벤트
    /// </summary>
    public event Action<string> OnDeleteQuantitySet;

    /// <summary>
    /// 최소 삭제 수량 이벤트
    /// </summary>
    public event Action OnDeleteMin;

    /// <summary>
    /// 최대 삭제 수량 이벤트
    /// </summary>
    public event Action OnDeleteMax;

    /// <summary>
    /// 삭제 확정 이벤트
    /// </summary>
    public event Action OnDeleteConfirmed;

    /// <summary>
    /// 삭제 패널 닫기 이벤트
    /// </summary>
    public event Action OnDeleteClosed;

    /// <summary>
    /// 상세 패널 닫기 요청 이벤트
    /// </summary>
    public event Action OnDetailClose;

    /// <summary>
    /// 인벤토리 닫기 요청 이벤트
    /// </summary>
    public event Action OnClose;
    #endregion

    /// <summary>
    /// 비활성 상태에서도 사용할 런타임 값 초기화
    /// </summary>
    public void InitializeRuntime ( )
    {
        _poolManager = GameManager.Instance.PoolManager;
        InitDropdowns ( );
    }

    /// <summary>
    /// 인벤토리 입력 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        InitDropdowns ( );

        //메인 카테고리 버튼 연결
        _allButton.onClick.AddListener ( SelectAllCategory );
        _partButton.onClick.AddListener ( SelectPartCategory );
        _facilityButton.onClick.AddListener ( SelectFacilityCategory );
        _consumableButton.onClick.AddListener ( SelectConsumableCategory );

        //분류와 정렬 드롭다운 연결
        _filterDropdown.onValueChanged.AddListener ( SelectFilter );
        _sortDropdown.onValueChanged.AddListener ( SelectSort );

        //상세 패널 입력 연결
        _detailView.OnMoveToShop += MoveToShop;
        _detailView.OnUse += Use;
        _detailView.OnMoveToCraft += MoveToCraft;
        _detailView.OnSell += Sell;
        _detailView.OnDelete += Delete;
        _detailView.OnClose += CloseDetail;

        //판매 패널 입력 연결
        _sellView.OnQuantityChanged += ChangeSellQuantity;
        _sellView.OnQuantitySet += SetSellQuantity;
        _sellView.OnSellMin += SetSellMin;
        _sellView.OnSellMax += SetSellMax;
        _sellView.OnSellConfirm += ConfirmSell;
        _sellView.OnClose += CloseSell;

        //삭제 패널 입력 연결
        _deleteView.OnQuantityChanged += ChangeDeleteQuantity;
        _deleteView.OnQuantitySet += SetDeleteQuantity;
        _deleteView.OnDeleteMin += SetDeleteMin;
        _deleteView.OnDeleteMax += SetDeleteMax;
        _deleteView.OnDeleteConfirm += ConfirmDelete;
        _deleteView.OnClose += CloseDelete;

        //인벤토리 닫기 입력 연결
        _closeButton.onClick.AddListener ( Close );

        //상세 패널 초기 숨김
        _detailView.HidePanel ( );
        _sellView.HideInstant ( );
        _deleteView.HideInstant ( );
    }

    /// <summary>
    /// 인벤토리 드롭다운 표시 항목 초기화
    /// </summary>
    void InitDropdowns ( )
    {
        //이미 준비한 드롭다운을 다시 초기화하지 않음
        if ( _partOptions != null ) return;

        _partOptions = new List<string> ( ( int ) PartType.Count + 1 )
        {
            "파츠 전체",
        };

        for ( int i = 0 ; i < ( int ) PartType.Count ; i++ )
        {
            _partOptions.Add (
                ( ( PartType ) i ).GetDisplayName ( ) );
        }

        SetFilterOptions ( _selectedCategory );
        _sortDropdown.SetOptions ( _sortOptions );
    }

    /// <summary>
    /// 비활성화 전에 활성 인벤토리 슬롯 반환
    /// </summary>
    void OnDisable ( )
    {
        ClearSlotViews ( );
    }

    /// <summary>
    /// 인벤토리 입력 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        //메인 카테고리 버튼 연결 해제
        _allButton.onClick.RemoveListener ( SelectAllCategory );
        _partButton.onClick.RemoveListener ( SelectPartCategory );
        _facilityButton.onClick.RemoveListener ( SelectFacilityCategory );
        _consumableButton.onClick.RemoveListener ( SelectConsumableCategory );

        //분류와 정렬 드롭다운 연결 해제
        _filterDropdown.onValueChanged.RemoveListener ( SelectFilter );
        _sortDropdown.onValueChanged.RemoveListener ( SelectSort );

        //상세 패널 입력 해제
        _detailView.OnMoveToShop -= MoveToShop;
        _detailView.OnUse -= Use;
        _detailView.OnMoveToCraft -= MoveToCraft;
        _detailView.OnSell -= Sell;
        _detailView.OnDelete -= Delete;
        _detailView.OnClose -= CloseDetail;

        //판매 패널 입력 해제
        _sellView.OnQuantityChanged -= ChangeSellQuantity;
        _sellView.OnQuantitySet -= SetSellQuantity;
        _sellView.OnSellMin -= SetSellMin;
        _sellView.OnSellMax -= SetSellMax;
        _sellView.OnSellConfirm -= ConfirmSell;
        _sellView.OnClose -= CloseSell;

        //삭제 패널 입력 해제
        _deleteView.OnQuantityChanged -= ChangeDeleteQuantity;
        _deleteView.OnQuantitySet -= SetDeleteQuantity;
        _deleteView.OnDeleteMin -= SetDeleteMin;
        _deleteView.OnDeleteMax -= SetDeleteMax;
        _deleteView.OnDeleteConfirm -= ConfirmDelete;
        _deleteView.OnClose -= CloseDelete;

        //인벤토리 닫기 입력 해제
        _closeButton.onClick.RemoveListener ( Close );

    }

    #region ----- 아이템 슬롯 -----

    /// <summary>
    /// 아이템 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">아이템 슬롯 표시 데이터</param>
    /// <param name="siblingIndex">현재 정렬 순서</param>
    void CreateSlotView (
        ItemSlotViewData viewData, int siblingIndex )
    {
        //데이터가 없거나 슬롯 아이디, 아이템 아이디가 공백이면 종료
        if ( viewData == null ||
            string.IsNullOrWhiteSpace ( viewData.SlotId ) ||
            string.IsNullOrWhiteSpace ( viewData.ItemId ) )
            return;

        //같은 슬롯 아이디가 있으면 종료
        if ( _slotViews.ContainsKey ( viewData.SlotId ) ) return;

        //프리팹별 공용 풀에서 슬롯을 가져와 현재 목록에 배치
        GameObject slotObject = _poolManager.GetFromPool (
            _slotPrefab.gameObject , _content );
        ItemSlotView slotView = slotObject.GetComponent<ItemSlotView> ( );
        slotView.transform.SetSiblingIndex( siblingIndex );

        //표시 데이터와 입력 이벤트 연결
        slotView.Init ( viewData , true );
        slotView.OnSelected += SelectSlot;

        //슬롯 아이디 기준으로 저장
        _slotViews.Add ( viewData.SlotId , slotView );
    }

    /// <summary>
    /// 아이템 슬롯을 공용 풀로 반환
    /// </summary>
    /// <param name="slotView">제거할 아이템 슬롯</param>
    void RemoveSlotView ( ItemSlotView slotView )
    {
        //슬롯 입력 이벤트 해제
        slotView.OnSelected -= SelectSlot;

        //식별자와 실행 중인 연출을 초기화한 뒤 원래 풀로 반환
        slotView.ResetForReuse ( );
        slotView.GetComponent<Poolable> ( ).ReturnToPool ( );
    }

    /// <summary>
    /// 표시 중인 아이템 슬롯 전체 반환
    /// </summary>
    void ClearSlotViews ( )
    {
        //현재 활성 슬롯의 입력을 해제하고 공용 풀로 반환
        foreach ( var slotView in _slotViews.Values )
            RemoveSlotView ( slotView );

        _slotViews.Clear ( );
    }

    /// <summary>
    /// 아이템 슬롯 선택 입력 중계
    /// </summary>
    /// <param name="slotId">슬롯 아이디</param>
    /// <param name="itemId">아이템 아이디</param>
    void SelectSlot ( string slotId , string itemId )
    {
        //현재 표시 중인 슬롯 아이디와 일치하는 슬롯만 강조
        foreach ( var pair in _slotViews )
            pair.Value.SetSelected( pair.Key == slotId );

        //실제 아이템 선택 처리는 Presenter로 전달
        OnSlotSelected?.Invoke ( slotId , itemId );
    }

    /// <summary>
    /// 아이템 슬롯 목록 생성
    /// </summary>
    /// <param name="viewDatas">아이템 슬롯 표시 데이터 목록</param>
    public void ShowSlots ( IReadOnlyList<ItemSlotViewData> viewDatas )
    {
        //기존 활성 슬롯을 공용 풀로 반환
        ClearSlotViews ( );

        //표시할 슬롯이 없으면 종료
        if ( viewDatas == null || viewDatas.Count == 0 ) return;

        //표시 데이터별 신규 슬롯 생성 또는 풀 슬롯 재사용
        for ( int i = 0; i < viewDatas.Count; i++ )
            CreateSlotView( viewDatas [ i ], i );

        //목록 스크롤을 맨 위로 이동
        if ( _scrollRect != null )
            _scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 아이템 아이디에 대응하는 첫 번째 활성 슬롯 조회
    /// </summary>
    /// <param name="itemId">아이템 아이디</param>
    /// <param name="view">조회한 슬롯 뷰</param>
    /// <returns>슬롯 조회 성공 여부</returns>
    public bool GetFirstSlotView (
        string itemId, out ItemSlotView view )
    {
        foreach ( ItemSlotView currentView in _slotViews.Values )
        {
            if ( currentView.ItemId != itemId ) continue;

            view = currentView;
            return true;
        }

        view = null;
        return false;
    }

    /// <summary>
    /// 지정 아이템 슬롯을 화면 안으로 이동하고 강조 위치 반환
    /// </summary>
    /// <param name="itemId">아이템 아이디</param>
    /// <param name="target">화면 안으로 이동한 슬롯 위치</param>
    /// <returns>대상 슬롯 조회 성공 여부</returns>
    public bool FocusSlot (
        string itemId, out RectTransform target )
    {
        target = null;

        if ( GetFirstSlotView(
            itemId, out ItemSlotView view ) == false )
        {
            return false;
        }

        target = view.transform as RectTransform;

        if ( target == null ) return false;

        MoveSlotIntoViewport( target );
        return true;
    }

    /// <summary>
    /// 화면 밖 아이템 슬롯이 보이도록 스크롤 위치 조정
    /// </summary>
    /// <param name="target">이동할 아이템 슬롯</param>
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
    /// 인벤토리 용량 표시 갱신
    /// </summary>
    /// <param name="usedSlotCount">현재 사용 슬롯 수</param>
    /// <param name="capacity">현재 최대 슬롯 수</param>
    public void UpdateCapacity ( int usedSlotCount , int capacity )
    {
        _capacityText.text = $"{usedSlotCount} / {capacity}";
    }

    #endregion

    #region ----- 카테고리 -----

    /// <summary>
    /// 전체 아이템 카테고리 선택
    /// </summary>
    void SelectAllCategory ( )
    {
        SelectCategory ( InventoryCategoryType.All );
    }

    /// <summary>
    /// 파츠 카테고리 선택
    /// </summary>
    void SelectPartCategory ( )
    {
        SelectCategory ( InventoryCategoryType.Part );
    }

    /// <summary>
    /// 시설 카테고리 선택
    /// </summary>
    void SelectFacilityCategory ( )
    {
        SelectCategory ( InventoryCategoryType.Facility );
    }

    /// <summary>
    /// 소모용품 카테고리 선택
    /// </summary>
    void SelectConsumableCategory ( )
    {
        SelectCategory ( InventoryCategoryType.Consumable );
    }

    /// <summary>
    /// 메인 카테고리 선택과 전체 분류 적용
    /// </summary>
    /// <param name="category">선택한 메인 카테고리</param>
    void SelectCategory ( InventoryCategoryType category )
    {
        _selectedCategory = category;

        //선택 카테고리에 맞는 분류 옵션과 전체 분류 표시
        SetFilterOptions ( category );

        //버튼 선택 직후 해당 카테고리 전체 목록 요청
        PublishFilterSelection ( 0 );
    }

    /// <summary>
    /// 현재 카테고리에 맞는 분류 옵션 설정
    /// </summary>
    /// <param name="category">현재 메인 카테고리</param>
    void SetFilterOptions ( InventoryCategoryType category )
    {
        switch ( category )
        {
            case InventoryCategoryType.Part:
                _filterDropdown.SetOptions ( _partOptions );
                _filterDropdown.interactable = true;
                break;

            case InventoryCategoryType.Facility:
                _filterDropdown.SetOptions ( _facilityOptions );
                _filterDropdown.interactable = true;
                break;

            case InventoryCategoryType.Consumable:
                _filterDropdown.SetOptions ( _consumableOptions );
                _filterDropdown.interactable = false;
                break;

            default:
                _filterDropdown.SetOptions ( _allOptions );
                _filterDropdown.interactable = false;
                break;
        }
    }

    /// <summary>
    /// 공용 분류 선택
    /// </summary>
    /// <param name="optionIndex">선택한 분류 번호</param>
    void SelectFilter ( int optionIndex )
    {
        PublishFilterSelection ( optionIndex );
    }

    /// <summary>
    /// 현재 메인 카테고리에 맞는 목록 이벤트 발행
    /// </summary>
    /// <param name="optionIndex">선택한 분류 번호</param>
    void PublishFilterSelection ( int optionIndex )
    {
        switch ( _selectedCategory )
        {
            case InventoryCategoryType.Part:
                OnPartSelected?.Invoke ( optionIndex );
                break;

            case InventoryCategoryType.Facility:
                OnFacilitySelected?.Invoke ( optionIndex );
                break;

            case InventoryCategoryType.Consumable:
                OnConsumableSelected?.Invoke ( optionIndex );
                break;

            default:
                OnAllSelected?.Invoke ( );
                break;
        }
    }

    /// <summary>
    /// 아이템 정렬 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 번호</param>
    void SelectSort ( int optionIndex )
    {
        OnSortSelected?.Invoke ( optionIndex );
    }

    #endregion

    #region ----- 상세 패널 -----

    /// <summary>
    /// 상점 이동 요청 중계
    /// </summary>
    /// <param name="itemId">아이템 아이디</param>
    void MoveToShop ( string itemId )
    {
        OnMoveToShop?.Invoke ( itemId );
    }

    /// <summary>
    /// 아이템 사용 요청 중계
    /// </summary>
    /// <param name="itemId">아이템 아이디</param>
    void Use ( string itemId )
    {
        OnUse?.Invoke ( itemId );
    }

    /// <summary>
    /// 제작 이동 요청 중계
    /// </summary>
    /// <param name="itemId">아이템 아이디</param>
    void MoveToCraft ( string itemId )
    {
        OnMoveToCraft?.Invoke ( itemId );
    }

    /// <summary>
    /// 판매 패널 열기 요청 중계
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    void Sell ( string slotId )
    {
        OnSell?.Invoke ( slotId );
    }

    /// <summary>
    /// 삭제 패널 열기 요청 중계
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    void Delete ( string slotId )
    {
        OnDelete?.Invoke ( slotId );
    }

    /// <summary>
    /// 판매 수량 변경 요청 중계
    /// </summary>
    void ChangeSellQuantity ( int amount )
    {
        OnSellQuantityChanged?.Invoke ( amount );
    }

    /// <summary>
    /// 판매 수량 직접 설정 요청 중계
    /// </summary>
    void SetSellQuantity ( string value )
    {
        OnSellQuantitySet?.Invoke ( value );
    }

    /// <summary>
    /// 최소 판매 수량 요청 중계
    /// </summary>
    void SetSellMin ( )
    {
        OnSellMin?.Invoke ( );
    }

    /// <summary>
    /// 최대 판매 수량 요청 중계
    /// </summary>
    void SetSellMax ( )
    {
        OnSellMax?.Invoke ( );
    }

    /// <summary>
    /// 판매 확정 요청 중계
    /// </summary>
    void ConfirmSell ( )
    {
        OnSellConfirmed?.Invoke ( );
    }

    /// <summary>
    /// 판매 패널 닫기 요청 중계
    /// </summary>
    void CloseSell ( )
    {
        OnSellClosed?.Invoke ( );
    }

    /// <summary>
    /// 삭제 수량 변경 요청 중계
    /// </summary>
    void ChangeDeleteQuantity ( int amount )
    {
        OnDeleteQuantityChanged?.Invoke ( amount );
    }

    /// <summary>
    /// 삭제 수량 직접 설정 요청 중계
    /// </summary>
    void SetDeleteQuantity ( string value )
    {
        OnDeleteQuantitySet?.Invoke ( value );
    }

    /// <summary>
    /// 최소 삭제 수량 요청 중계
    /// </summary>
    void SetDeleteMin ( )
    {
        OnDeleteMin?.Invoke ( );
    }

    /// <summary>
    /// 최대 삭제 수량 요청 중계
    /// </summary>
    void SetDeleteMax ( )
    {
        OnDeleteMax?.Invoke ( );
    }

    /// <summary>
    /// 삭제 확정 요청 중계
    /// </summary>
    void ConfirmDelete ( )
    {
        OnDeleteConfirmed?.Invoke ( );
    }

    /// <summary>
    /// 삭제 패널 닫기 요청 중계
    /// </summary>
    void CloseDelete ( )
    {
        OnDeleteClosed?.Invoke ( );
    }

    /// <summary>
    /// 상세 패널 닫기 요청 중계
    /// </summary>
    void CloseDetail ( )
    {
        OnDetailClose?.Invoke ( );
    }

    /// <summary>
    /// 인벤토리 상세 패널 표시
    /// </summary>
    /// <param name="viewData">상세 패널 표시 데이터</param>
    public void ShowDetail ( InventoryDetailViewData viewData )
    {
        _detailView.ShowPanel ( viewData );
    }

    /// <summary>
    /// 인벤토리 상세 패널 숨김
    /// </summary>
    public void HideDetail ( )
    {
        _detailView.HidePanel ( );
    }

    /// <summary>
    /// 아이템 판매 패널 표시
    /// </summary>
    public void ShowSell ( InventoryItemSellViewData viewData )
    {
        _deleteView.HidePanel ( );
        _sellView.ShowPanel ( viewData );
    }

    /// <summary>
    /// 아이템 판매 패널 갱신
    /// </summary>
    public void UpdateSell ( InventoryItemSellViewData viewData )
    {
        _sellView.UpdateView ( viewData );
    }

    /// <summary>
    /// 아이템 판매 패널 숨김
    /// </summary>
    public void HideSell ( )
    {
        _sellView.HidePanel ( );
    }

    /// <summary>
    /// 아이템 삭제 패널 표시
    /// </summary>
    public void ShowDelete ( InventoryItemDeleteViewData viewData )
    {
        _sellView.HidePanel ( );
        _deleteView.ShowPanel ( viewData );
    }

    /// <summary>
    /// 아이템 삭제 패널 갱신
    /// </summary>
    public void UpdateDelete ( InventoryItemDeleteViewData viewData )
    {
        _deleteView.UpdateView ( viewData );
    }

    /// <summary>
    /// 아이템 삭제 패널 숨김
    /// </summary>
    public void HideDelete ( )
    {
        _deleteView.HidePanel ( );
    }

    #endregion

    #region ----- 인벤토리 패널 -----

    /// <summary>
    /// 인벤토리 닫기 요청
    /// </summary>
    void Close ( )
    {
        OnClose?.Invoke ( );
    }

    /// <summary>
    /// 인벤토리 패널 표시
    /// </summary>
    public void ShowPanel ( )
    {
        gameObject.SetActive ( true );
        _panelTween.Show ( );
    }

    /// <summary>
    /// 인벤토리 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        //하위 패널을 모두 닫고 인벤토리 숨김
        _detailView.HidePanel ( );
        _sellView.HideInstant ( );
        _deleteView.HideInstant ( );

        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 인벤토리 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _detailView.HidePanel ( );
        _sellView.HideInstant ( );
        _deleteView.HideInstant ( );

        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 인벤토리 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    #endregion
}
