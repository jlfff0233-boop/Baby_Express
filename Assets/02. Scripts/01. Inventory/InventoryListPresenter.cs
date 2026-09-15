using System.Collections.Generic;

/// <summary>
/// 인벤토리 카테고리
/// </summary>
public enum InventoryCategoryType
{
    All,            //전체
    Part,           //파츠
    Facility,       //시설 관련
    Consumable,     //소모용품
}

/// <summary>
/// 인벤토리 시설 세부 타입
/// </summary>
public enum InventoryFacilityFilterType
{
    All,            //시설 전체
    Equipment,      //장비
    Furniture,      //가구
    Facility,       //시설 확장
}

/// <summary>
/// 인벤토리 목록 프레젠터 - 목록 조건과 용량 표시 중재
/// </summary>
public class InventoryListPresenter
{
    InventoryModel _inventoryModel;       //인벤토리 모델
    InventoryView _inventoryView;         //인벤토리 뷰
    InventoryListBuilder _listBuilder;    //표시 목록 빌더
    InventoryViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    InventoryCategoryType _selectedCategory = InventoryCategoryType.All;       //현재 카테고리
    PartType? _selectedPartType;       //현재 파츠 타입
    InventoryFacilityFilterType? _selectedFacilityType;       //현재 시설 타입
    InventorySortType _selectedSortType = InventorySortType.Default;       //현재 정렬 타입

    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 인벤토리 목록 프레젠터 생성
    /// </summary>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="inventoryView">인벤토리 뷰</param>
    public InventoryListPresenter (
        InventoryModel inventoryModel, InventoryView inventoryView )
    {
        _inventoryModel = inventoryModel;
        _inventoryView = inventoryView;
        _listBuilder = new InventoryListBuilder( );
        _viewDataBuilder = new InventoryViewDataBuilder( );
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 목록과 카테고리 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _inventoryModel.OnInventoryChanged += Refresh;
        _inventoryModel.OnCapacityChanged += UpdateCapacity;

        _inventoryView.OnAllSelected += SelectAll;
        _inventoryView.OnPartSelected += SelectPart;
        _inventoryView.OnFacilitySelected += SelectFacility;
        _inventoryView.OnConsumableSelected += SelectConsumable;
        _inventoryView.OnSortSelected += SelectSort;

        _isSubscribed = true;
    }

    /// <summary>
    /// 목록과 카테고리 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _inventoryModel.OnInventoryChanged -= Refresh;
        _inventoryModel.OnCapacityChanged -= UpdateCapacity;

        _inventoryView.OnAllSelected -= SelectAll;
        _inventoryView.OnPartSelected -= SelectPart;
        _inventoryView.OnFacilitySelected -= SelectFacility;
        _inventoryView.OnConsumableSelected -= SelectConsumable;
        _inventoryView.OnSortSelected -= SelectSort;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 목록 갱신 -----
    /// <summary>
    /// 인벤토리 목록과 용량 표시 갱신
    /// </summary>
    public void Refresh ()
    {
        RefreshSlots( );
        UpdateCapacity( _inventoryModel.Capacity );
    }

    /// <summary>
    /// 현재 선택 조건의 아이템 슬롯 갱신
    /// </summary>
    void RefreshSlots ()
    {
        //인벤토리 스택 목록 생성
        IReadOnlyList<InventoryStack> stacks =
            _listBuilder.GetSelectedStacks(
                _inventoryModel.Items, _selectedCategory,
                _selectedPartType, _selectedFacilityType,
                _selectedSortType );

        _inventoryView.ShowSlots( _viewDataBuilder.CreateSlots( stacks ) );
    }

    /// <summary>
    /// 인벤토리 용량 표시 갱신
    /// </summary>
    /// <param name="capacity">현재 최대 슬롯 수</param>
    void UpdateCapacity ( int capacity )
    {
        _inventoryView.UpdateCapacity( _inventoryModel.UsedSlotCount, capacity );
    }
    #endregion

    #region ----- 카테고리 -----
    /// <summary>
    /// 전체 아이템 선택
    /// </summary>
    void SelectAll ()
    {
        _selectedCategory = InventoryCategoryType.All;
        RefreshSlots( );
    }

    /// <summary>
    /// 파츠 타입 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 번호</param>
    void SelectPart ( int optionIndex )
    {
        if ( optionIndex == 0 )
        {
            _selectedPartType = null;
        }
        else
        {
            int partTypeValue = optionIndex - 1;

            if ( partTypeValue < 0 ||
                partTypeValue >= ( int ) PartType.Count )
                return;

            _selectedPartType = ( PartType ) partTypeValue;
        }

        _selectedCategory = InventoryCategoryType.Part;
        RefreshSlots( );
    }

    /// <summary>
    /// 시설 타입 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 번호</param>
    void SelectFacility ( int optionIndex )
    {
        if ( optionIndex < 0 ||
            optionIndex > ( int ) InventoryFacilityFilterType.Facility )
            return;

        _selectedCategory = InventoryCategoryType.Facility;
        _selectedFacilityType =
            ( InventoryFacilityFilterType ) optionIndex;

        RefreshSlots( );
    }

    /// <summary>
    /// 소모용품 타입 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 번호</param>
    void SelectConsumable ( int optionIndex )
    {
        if ( optionIndex != 0 ) return;

        _selectedCategory = InventoryCategoryType.Consumable;
        RefreshSlots( );
    }

    /// <summary>
    /// 아이템 정렬 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 번호</param>
    void SelectSort ( int optionIndex )
    {
        if ( optionIndex < 0 ||
            optionIndex > ( int ) InventorySortType.QuantityDescending )
            return;

        _selectedSortType = ( InventorySortType ) optionIndex;
        RefreshSlots( );
    }
    #endregion
}
