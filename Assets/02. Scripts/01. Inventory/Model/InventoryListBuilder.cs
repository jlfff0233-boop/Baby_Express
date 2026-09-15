using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 인벤토리 목록 빌더 - 아이템 필터, 정렬, 스택 목록 생성
/// </summary>
public class InventoryListBuilder
{

    #region ----- 선택 목록 -----
    /// <summary>
    /// 현재 선택 조건에 맞는 인벤토리 스택 목록 생성
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="category">선택한 인벤토리 카테고리</param>
    /// <param name="partType">선택한 파츠 타입</param>
    /// <param name="facilityType">선택한 시설 타입</param>
    /// <param name="sortType">선택한 정렬 타입</param>
    /// <returns>현재 선택 조건에 맞는 인벤토리 스택 목록</returns>
    public IReadOnlyList<InventoryStack> GetSelectedStacks (
        IReadOnlyCollection<InventoryItem> inventoryItems,
        InventoryCategoryType category, PartType? partType,
        InventoryFacilityFilterType? facilityType,
        InventorySortType sortType )
    {
        switch ( category )
        {
            case InventoryCategoryType.Part:
                return partType.HasValue
                    ? GetPartStacks( inventoryItems, partType.Value, sortType )
                    : GetStacks( inventoryItems, ProductType.BabyPart, sortType );

            case InventoryCategoryType.Facility:
                return GetSelectedFacilityStacks(
                    inventoryItems, facilityType, sortType );

            case InventoryCategoryType.Consumable:
                return GetStacks(
                    inventoryItems, ProductType.Consumable, sortType );

            default:
                return GetStacks( inventoryItems, sortType );
        }
    }

    /// <summary>
    /// 현재 선택한 시설 타입의 인벤토리 스택 목록 생성
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="facilityType">선택한 시설 타입</param>
    /// <param name="sortType">선택한 정렬 타입</param>
    /// <returns>현재 선택한 시설 타입의 인벤토리 스택 목록</returns>
    IReadOnlyList<InventoryStack> GetSelectedFacilityStacks (
        IReadOnlyCollection<InventoryItem> inventoryItems,
        InventoryFacilityFilterType? facilityType,
        InventorySortType sortType )
    {
        switch ( facilityType )
        {
            case InventoryFacilityFilterType.Equipment:
                return GetStacks(
                    inventoryItems, ProductType.Equipment, sortType );

            case InventoryFacilityFilterType.Furniture:
                return GetStacks(
                    inventoryItems, ProductType.Furniture, sortType );

            case InventoryFacilityFilterType.Facility:
                return GetStacks(
                    inventoryItems, ProductType.Expansion, sortType );

            default:
                return GetFacilityStacks( inventoryItems, sortType );
        }
    }
    #endregion

    #region ----- 전체/상품 목록 -----
    /// <summary>
    /// 전체 인벤토리 스택 목록 생성
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="sortType">정렬 타입</param>
    /// <returns>전체 인벤토리 스택 목록</returns>
    public IReadOnlyList<InventoryStack> GetStacks (
        IReadOnlyCollection<InventoryItem> inventoryItems,
        InventorySortType sortType = InventorySortType.Default )
    {
        //전체 아이템을 정렬한 뒤 스택 목록으로 변환
        return CreateStacks( SortItems( inventoryItems, sortType ) );
    }

    /// <summary>
    /// 아이템 타입별 인벤토리 스택 목록 생성
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="productType">조회할 아이템 타입</param>
    /// <param name="sortType">정렬 타입</param>
    /// <returns>아이템 타입별 인벤토리 스택 목록</returns>
    public IReadOnlyList<InventoryStack> GetStacks (
        IReadOnlyCollection<InventoryItem> inventoryItems,
        ProductType productType,
        InventorySortType sortType = InventorySortType.Default )
    {
        //아이템 타입별 목록 생성
        var items = new List<InventoryItem>( );

        //전체 인벤토리 아이템 순회
        foreach ( var item in inventoryItems )
        {
            //같은 아이템 타입이면 목록에 추가
            if ( item.Data.ProductType == productType )
                items.Add( item );
        }

        //선별한 아이템을 정렬한 뒤 스택 목록으로 변환
        return CreateStacks( SortItems( items, sortType ) );
    }
    #endregion

    #region ----- 파츠/시설 목록 -----
    /// <summary>
    /// 파츠 타입별 인벤토리 스택 목록 생성
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="partType">조회할 파츠 타입</param>
    /// <param name="sortType">정렬 타입</param>
    /// <returns>파츠 타입별 인벤토리 스택 목록</returns>
    public IReadOnlyList<InventoryStack> GetPartStacks (
        IReadOnlyCollection<InventoryItem> inventoryItems,
        PartType partType,
        InventorySortType sortType = InventorySortType.Default )
    {
        //파츠 타입별 목록 생성
        var items = new List<InventoryItem>( );

        foreach ( var item in inventoryItems )
        {
            //같은 파츠 타입이면 목록에 추가
            if ( item.Data is PartsData partsData && partsData.PartType == partType )
                items.Add( item );
        }

        //파츠 정렬 후 스택 목록으로 변환
        return CreateStacks( SortItems( items, sortType ) );
    }

    /// <summary>
    /// 시설 관련 인벤토리 스택 목록 생성
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="sortType">정렬 타입</param>
    /// <returns>시설 관련 인벤토리 스택 목록</returns>
    public IReadOnlyList<InventoryStack> GetFacilityStacks (
        IReadOnlyCollection<InventoryItem> inventoryItems,
        InventorySortType sortType = InventorySortType.Default )
    {
        //시설 관련 아이템 목록 생성
        var items = new List<InventoryItem>( );

        foreach ( var item in inventoryItems )
        {
            //현재 아이템 타입 가져오기
            ProductType itemType = item.Data.ProductType;

            //시설, 장비, 가구이면 목록에 추가
            if ( itemType == ProductType.Expansion ||
                itemType == ProductType.Equipment ||
                itemType == ProductType.Furniture )
                items.Add( item );
        }

        //선별한 아이템을 정렬한 뒤 스택 목록으로 변환
        return CreateStacks( SortItems( items, sortType ) );
    }
    #endregion

    #region ----- 슬롯 조회 -----
    /// <summary>
    /// 슬롯 아이디로 인벤토리 스택 조회
    /// </summary>
    /// <param name="inventoryItems">인벤토리 아이템 목록</param>
    /// <param name="slotId">조회할 슬롯 아이디</param>
    /// <param name="stack">조회한 인벤토리 스택</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetStack ( IReadOnlyCollection<InventoryItem> inventoryItems,
        string slotId, out InventoryStack stack )
    {
        stack = null;

        if ( string.IsNullOrWhiteSpace( slotId ) ) return false;

        //전체 스택 목록 생성
        IReadOnlyList<InventoryStack> stacks = GetStacks( inventoryItems );

        for ( int i = 0; i < stacks.Count; i++ )
        {
            //슬롯 아이디가 다르면 다음 스택 확인
            if ( stacks [ i ].SlotId != slotId ) continue;

            stack = stacks [ i ];
            return true;
        }

        return false;
    }
    #endregion

    #region ----- 정렬/스택 변환 -----
    /// <summary>
    /// 인벤토리 아이템 정렬
    /// </summary>
    /// <param name="items">정렬할 인벤토리 아이템 목록</param>
    /// <param name="sortType">정렬 타입</param>
    /// <returns>정렬된 인벤토리 아이템 목록</returns>
    IEnumerable<InventoryItem> SortItems (
        IEnumerable<InventoryItem> items, InventorySortType sortType )
    {
        //선택한 정렬 타입 확인
        switch ( sortType )
        {
            case InventorySortType.NameAscending:
                //이름 오름차순 정렬
                return items.OrderBy(
                    item => item.Data.Name, StringComparer.Ordinal );

            case InventorySortType.NameDescending:
                //이름 내림차순 정렬
                return items.OrderByDescending(
                    item => item.Data.Name, StringComparer.Ordinal );

            case InventorySortType.PriceAscending:
                //가격 오름차순 정렬
                return items.OrderBy( item => item.Data.BasePrice );

            case InventorySortType.PriceDescending:
                //가격 내림차순 정렬
                return items.OrderByDescending( item => item.Data.BasePrice );

            case InventorySortType.QuantityAscending:
                //수량 오름차순 정렬
                return items.OrderBy( item => item.Quantity );

            case InventorySortType.QuantityDescending:
                //수량 내림차순 정렬
                return items.OrderByDescending( item => item.Quantity );

            default:
                //기본 정렬이면 현재 순서 유지
                return items;
        }
    }

    /// <summary>
    /// 아이템 목록을 스택 목록으로 변환
    /// </summary>
    /// <param name="items">변환할 인벤토리 아이템 목록</param>
    /// <returns>변환된 인벤토리 스택 목록</returns>
    IReadOnlyList<InventoryStack> CreateStacks ( IEnumerable<InventoryItem> items )
    {
        var stacks = new List<InventoryStack>( );

        foreach ( var item in items )
        {
            //스택으로 나눌 남은 수량 설정
            int remainingQuantity = item.Quantity;
            //현재 아이템의 스택 인덱스 초기화
            int stackIndex = 0;

            //남은 수량이 없을 때까지 스택 생성
            while ( remainingQuantity > 0 )
            {
                //스택 최대 수량과 남은 수량 중 작은 값 사용
                int stackQuantity = Math.Min(
                    InventoryModel.MaxStackQuantity, remainingQuantity );
                //아이템 아이디와 스택 인덱스로 슬롯 아이디 생성
                string slotId = $"{item.Data.Id}_{stackIndex}";

                //인벤토리 스택 추가
                stacks.Add( new InventoryStack( slotId, item.Data, stackQuantity ) );

                //남은 수량에서 현재 스택 수량 제거
                remainingQuantity -= stackQuantity;
                //다음 스택 인덱스로 이동
                stackIndex++;
            }
        }

        //생성한 인벤토리 스택 목록 반환
        return stacks;
    }
    #endregion
}
