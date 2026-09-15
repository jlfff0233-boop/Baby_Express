using System.Collections.Generic;

/// <summary>
/// 상점 목록 빌더 - 선택 조건에 맞는 상품 목록 구성
/// </summary>
public class ShopListBuilder
{
    /// <summary>
    /// 현재 선택 조건에 맞는 상품 목록 조회
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="category">선택한 상점 카테고리</param>
    /// <param name="partType">선택한 파츠 타입</param>
    /// <param name="facilityType">선택한 시설 타입</param>
    /// <param name="sortType">선택한 정렬 타입</param>
    /// <returns>현재 선택 조건에 맞는 상품 목록</returns>
    public IReadOnlyList<ShopItemModel> GetItems (
        ShopModel shopModel, ShopCategoryType category,
        PartType? partType, FacilityFilterType? facilityType,
        ShopSortType sortType )
    {
        switch ( category )
        {
            case ShopCategoryType.Part:
                return partType.HasValue
                    ? shopModel.GetParts( partType.Value, sortType )
                    : shopModel.GetItems( ProductType.BabyPart, sortType );

            case ShopCategoryType.Facility:
                return GetFacilityItems(
                    shopModel, facilityType, sortType );

            case ShopCategoryType.Consumable:
                return shopModel.GetItems(
                    ProductType.Consumable, sortType );

            default:
                return shopModel.GetAllItems( sortType );
        }
    }

    /// <summary>
    /// 현재 선택한 시설 타입의 상품 목록 조회
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="facilityType">선택한 시설 타입</param>
    /// <param name="sortType">선택한 정렬 타입</param>
    /// <returns>현재 선택한 시설 타입의 상품 목록</returns>
    IReadOnlyList<ShopItemModel> GetFacilityItems (
        ShopModel shopModel, FacilityFilterType? facilityType,
        ShopSortType sortType )
    {
        switch ( facilityType )
        {
            case FacilityFilterType.Facility:
                return shopModel.GetItems(
                    ProductType.Expansion, sortType );

            case FacilityFilterType.Equipment:
                return shopModel.GetItems(
                    ProductType.Equipment, sortType );

            case FacilityFilterType.Furniture:
                return shopModel.GetItems(
                    ProductType.Furniture, sortType );

            default:
                return shopModel.GetFacilities( sortType );
        }
    }
}
