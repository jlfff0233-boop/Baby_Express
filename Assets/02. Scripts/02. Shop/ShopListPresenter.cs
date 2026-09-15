using System.Collections.Generic;

/// <summary>
/// 현재 선택한 상점 카테고리
/// </summary>
public enum ShopCategoryType
{
    All,            //전체
    Part,           //파츠
    Facility,       //시설, 장비, 가구
    Consumable,     //소모용품
}

/// <summary>
/// 시설 카테고리의 세부 필터 타입
/// </summary>
public enum FacilityFilterType
{
    All,            //시설 전체
    Equipment,      //장비
    Furniture,      //가구
    Facility,       //시설 확장
}


/// <summary>
/// 상점 목록 프레젠터 - 카테고리, 필터, 정렬과 상품 슬롯 중재
/// </summary>
public class ShopListPresenter
{
    ShopModel _shopModel;       //상점 모델
    PlayStateModel _playStateModel;       //상품 해금 상태 모델
    ShopView _shopView;         //상점 뷰
    ShopListBuilder _listBuilder;       //상품 목록 빌더
    ShopViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    ShopCategoryType _selectedCategory = ShopCategoryType.All;       //현재 카테고리
    PartType? _selectedPartType;       //현재 파츠 타입
    FacilityFilterType? _selectedFacilityType;       //현재 시설 타입
    ShopSortType _selectedSortType = ShopSortType.Default;       //현재 정렬 타입

    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 상점 목록 프레젠터 생성
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="playStateModel">상품 해금 상태 모델</param>
    /// <param name="shopView">상점 뷰</param>
    public ShopListPresenter (
        ShopModel shopModel, PlayStateModel playStateModel, ShopView shopView )
    {
        _shopModel = shopModel;
        _playStateModel = playStateModel;
        _shopView = shopView;
        _listBuilder = new ShopListBuilder( );
        _viewDataBuilder = new ShopViewDataBuilder( );
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 상품 목록 입력 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _shopView.OnAllSelected += SelectAll;
        _shopView.OnPartSelected += SelectPart;
        _shopView.OnFacilitySelected += SelectFacility;
        _shopView.OnConsumableSelected += SelectConsumable;
        _shopView.OnSortSelected += SelectSort;

        _isSubscribed = true;
    }

    /// <summary>
    /// 상품 목록 입력 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _shopView.OnAllSelected -= SelectAll;
        _shopView.OnPartSelected -= SelectPart;
        _shopView.OnFacilitySelected -= SelectFacility;
        _shopView.OnConsumableSelected -= SelectConsumable;
        _shopView.OnSortSelected -= SelectSort;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 목록 갱신 -----
    /// <summary>
    /// 상점 진입 목록 조건 초기화
    /// </summary>
    public void Reset ()
    {
        _selectedCategory = ShopCategoryType.All;
        _selectedPartType = null;
        _selectedFacilityType = null;
        _selectedSortType = ShopSortType.Default;
    }

    /// <summary>
    /// 현재 선택 조건의 상품 목록 갱신
    /// </summary>
    public void Refresh ()
    {
        IReadOnlyList<ShopItemModel> items =
            _listBuilder.GetItems(
                _shopModel, _selectedCategory,
                _selectedPartType, _selectedFacilityType,
                _selectedSortType );

        var viewDatas = new List<ShopSlotViewData>( items.Count );

        foreach ( ShopItemModel itemModel in items )
        {
            if ( ShouldShowItem( itemModel ) == false )
                continue;

            viewDatas.Add( CreateSlotViewData( itemModel ) );
        }

        _shopView.CreateSlotList( viewDatas );
    }

    /// <summary>
    /// 상점 목록에 상품 슬롯을 표시할지 확인
    /// </summary>
    /// <param name="itemModel">확인할 상품 모델</param>
    /// <returns>상품 슬롯 표시 여부</returns>
    bool ShouldShowItem ( ShopItemModel itemModel )
    {
        if ( IsFacilityProduct( itemModel.Item.Data.ProductType ) )
            return true;

        return _playStateModel.IsUnlocked( itemModel.Item.Id );
    }

    /// <summary>
    /// 정비 시스템 진입을 위해 잠금 상태에서도 표시할 시설 상품 확인
    /// </summary>
    /// <param name="productType">확인할 상품 타입</param>
    /// <returns>시설 카테고리 상품 여부</returns>
    bool IsFacilityProduct ( ProductType productType )
    {
        return productType == ProductType.Expansion ||
            productType == ProductType.Equipment ||
            productType == ProductType.Furniture;
    }

    /// <summary>
    /// 지정 상품 슬롯 표시 갱신
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    public void RefreshProduct ( string itemId )
    {
        if ( _shopModel.TryGetItem(
            itemId, out ShopItemModel itemModel ) == false )
            return;

        _shopView.UpdateSlot( CreateSlotViewData( itemModel ) );
    }

    /// <summary>
    /// 상품 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="itemModel">상품 모델</param>
    /// <returns>상품 슬롯 표시 데이터</returns>
    ShopSlotViewData CreateSlotViewData ( ShopItemModel itemModel )
    {
        bool isLocked = _playStateModel.IsUnlocked(
            itemModel.Item.Id ) == false;

        return _viewDataBuilder.CreateSlot( itemModel, isLocked );
    }
    #endregion

    #region ----- 카테고리 -----
    /// <summary>
    /// 전체 상품 선택
    /// </summary>
    void SelectAll ()
    {
        _selectedCategory = ShopCategoryType.All;
        Refresh( );
    }

    /// <summary>
    /// 파츠 카테고리 선택
    /// </summary>
    /// <param name="filterIndex">파츠 필터 번호</param>
    void SelectPart ( int filterIndex )
    {
        if ( filterIndex == 0 )
        {
            _selectedPartType = null;
        }
        else
        {
            int partTypeValue = filterIndex - 1;

            if ( partTypeValue < 0 ||
                partTypeValue >= ( int ) PartType.Count )
                return;

            _selectedPartType = ( PartType ) partTypeValue;
        }

        _selectedCategory = ShopCategoryType.Part;
        Refresh( );
    }

    /// <summary>
    /// 시설 카테고리 선택
    /// </summary>
    /// <param name="filterIndex">시설 필터 번호</param>
    void SelectFacility ( int filterIndex )
    {
        if ( filterIndex < 0 ||
            filterIndex > ( int ) FacilityFilterType.Facility )
            return;

        _selectedCategory = ShopCategoryType.Facility;
        _selectedFacilityType = ( FacilityFilterType ) filterIndex;
        Refresh( );
    }

    /// <summary>
    /// 소모용품 카테고리 선택
    /// </summary>
    void SelectConsumable ()
    {
        _selectedCategory = ShopCategoryType.Consumable;
        Refresh( );
    }

    /// <summary>
    /// 상품 정렬 방식 선택
    /// </summary>
    /// <param name="sortIndex">정렬 방식 번호</param>
    void SelectSort ( int sortIndex )
    {
        if ( sortIndex < 0 ||
            sortIndex > ( int ) ShopSortType.StockDescending )
            return;

        _selectedSortType = ( ShopSortType ) sortIndex;
        Refresh( );
    }
    #endregion
}
