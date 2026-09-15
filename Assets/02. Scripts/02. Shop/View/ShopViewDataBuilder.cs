/// <summary>
/// 상점 표시 데이터 생성기 - 검증된 상품 상태를 화면 데이터로 변환
/// </summary>
public class ShopViewDataBuilder
{
    /// <summary>
    /// 상품 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="itemModel">상품 모델</param>
    /// <param name="isLocked">상품 잠금 여부</param>
    /// <returns>상품 슬롯 표시 데이터</returns>
    public ShopSlotViewData CreateSlot (
        ShopItemModel itemModel, bool isLocked )
    {
        ShopItem item = itemModel.Item;
        PurchasableData data = item.Data;

        return new ShopSlotViewData(
            item.Id, data.Icon, data.Name,
            item.CurrentPrice, item.RemainingStock,
            item.IsSoldOut, isLocked );
    }

    /// <summary>
    /// 상품 상세 표시 데이터 생성
    /// </summary>
    /// <param name="itemModel">상품 모델</param>
    /// <param name="description">표시할 상품 설명</param>
    /// <param name="isLocked">상품 잠금 여부</param>
    /// <param name="canQuickRestock">빠른 재입고 표시 여부</param>
    /// <returns>상품 상세 표시 데이터</returns>
    public ShopDetailViewData CreateDetail (
        ShopItemModel itemModel, string description,
        bool isLocked, bool canQuickRestock )
    {
        ShopItem item = itemModel.Item;
        PurchasableData data = item.Data;

        return new ShopDetailViewData(
            item.Id, data.Icon, data.Name,
            description, item.CurrentPrice,
            item.RemainingStock, isLocked, canQuickRestock );
    }

    /// <summary>
    /// 장바구니 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="item">장바구니 상품</param>
    /// <param name="price">상품 현재 가격</param>
    /// <param name="subtotal">상품별 소계</param>
    /// <returns>장바구니 슬롯 표시 데이터</returns>
    public CartItemViewData CreateCartItem (
        CartItem item, float price, float subtotal )
    {
        return new CartItemViewData(
            item.Data.Id, item.Data.Name,
            item.Quantity, price, subtotal );
    }

    /// <summary>
    /// 파츠 연구 해금 안내 문구 생성
    /// </summary>
    /// <param name="researchName">연구 이름</param>
    /// <param name="requiredLevel">요구 연구 단계</param>
    /// <returns>파츠 연구 해금 안내 문구</returns>
    public string CreateResearchUnlockText (
        string researchName, int requiredLevel )
    {
        return $"{researchName} Lv.{requiredLevel} 연구 필요";
    }

    /// <summary>
    /// 일반 잠금 안내 문구 생성
    /// </summary>
    /// <returns>일반 잠금 안내 문구</returns>
    public string CreateLockedText ()
    {
        return "아직 해금되지 않은 상품입니다.";
    }

    /// <summary>
    /// 빠른 재입고 실패 안내 문구 반환
    /// </summary>
    /// <param name="result">빠른 재입고 처리 결과</param>
    /// <returns>빠른 재입고 실패 안내 문구</returns>
    public string GetQuickRestockFailureText (
        QuickRestockResult result )
    {
        switch ( result )
        {
            case QuickRestockResult.InvalidItem:
                return "상품 정보를 확인할 수 없습니다.";

            case QuickRestockResult.LockedItem:
                return "아직 해금되지 않은 상품입니다.";

            case QuickRestockResult.FullStock:
                return "이미 재고가 최대치입니다.";

            case QuickRestockResult.DailyLimitReached:
                return "오늘 빠른 재입고 횟수를 모두 사용했습니다.";

            case QuickRestockResult.InvalidFee:
                return "빠른 재입고 이용료를 계산할 수 없습니다.";

            case QuickRestockResult.InsufficientBudget:
                return "빠른 재입고 이용료가 부족합니다.";

            case QuickRestockResult.BudgetUpdateFailed:
                return "빠른 재입고 이용료 차감에 실패했습니다.";

            default:
                return "빠른 재입고를 완료하지 못했습니다.";
        }
    }
}
