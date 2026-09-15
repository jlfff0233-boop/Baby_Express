using System.Collections.Generic;

/// <summary>
/// 인벤토리 표시 데이터 생성기 - 검증된 상태를 화면 데이터로 변환
/// </summary>
public class InventoryViewDataBuilder
{
    /// <summary>
    /// 아이템 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="stack">인벤토리 스택</param>
    /// <returns>아이템 슬롯 표시 데이터</returns>
    ItemSlotViewData CreateSlot ( InventoryStack stack )
    {
        return new ItemSlotViewData(
            stack.SlotId, stack.ItemId,
            stack.Data.Icon, stack.Data.Name, stack.Quantity );
    }

    /// <summary>
    /// 스택 목록을 슬롯 표시 데이터로 변환
    /// </summary>
    /// <param name="stacks">표시할 인벤토리 스택</param>
    /// <returns>아이템 슬롯 표시 데이터 목록</returns>
    public IReadOnlyList<ItemSlotViewData> CreateSlots (
        IReadOnlyList<InventoryStack> stacks )
    {
        var viewDatas = new List<ItemSlotViewData>( stacks.Count );

        for ( int i = 0; i < stacks.Count; i++ )
            viewDatas.Add( CreateSlot( stacks [ i ] ) );

        return viewDatas;
    }

    /// <summary>
    /// 인벤토리 상세 표시 데이터 생성
    /// </summary>
    /// <param name="stack">선택한 인벤토리 스택</param>
    /// <param name="canOpenShop">상점 이동 가능 여부</param>
    /// <param name="canUse">사용 가능 여부</param>
    /// <param name="canCraft">제작 가능 여부</param>
    /// <param name="canSell">판매 가능 여부</param>
    /// <param name="canDelete">삭제 가능 여부</param>
    /// <returns>인벤토리 상세 표시 데이터</returns>
    public InventoryDetailViewData CreateDetail (
        InventoryStack stack,
        bool canOpenShop, bool canUse, bool canCraft,
        bool canSell, bool canDelete )
    {
        return new InventoryDetailViewData(
            stack.SlotId, stack.ItemId,
            stack.Data.Icon, stack.Data.Name, stack.Data.Description,
            stack.Quantity, canOpenShop, canUse, canCraft,
            canSell, canDelete );
    }

    /// <summary>
    /// 판매 패널 표시 데이터 생성
    /// </summary>
    /// <param name="stack">선택한 인벤토리 스택</param>
    /// <param name="selectedQuantity">선택한 판매 수량</param>
    /// <param name="unitPrice">판매 단가</param>
    /// <param name="totalPrice">예상 획득 금액</param>
    /// <returns>판매 패널 표시 데이터</returns>
    public InventoryItemSellViewData CreateSell (
        InventoryStack stack, int selectedQuantity,
        float unitPrice, float totalPrice )
    {
        return new InventoryItemSellViewData(
            stack.Data.Icon, stack.Data.Name, unitPrice,
            stack.Quantity, selectedQuantity, totalPrice );
    }

    /// <summary>
    /// 삭제 패널 표시 데이터 생성
    /// </summary>
    /// <param name="stack">선택한 인벤토리 스택</param>
    /// <param name="selectedQuantity">선택한 삭제 수량</param>
    /// <returns>삭제 패널 표시 데이터</returns>
    public InventoryItemDeleteViewData CreateDelete (
        InventoryStack stack, int selectedQuantity )
    {
        return new InventoryItemDeleteViewData(
            stack.Data.Name, stack.Quantity, selectedQuantity );
    }
}
