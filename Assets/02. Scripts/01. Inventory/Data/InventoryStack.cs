
/// <summary>
/// 인벤토리 스택 - 슬롯 하나에 표시할 아이템과 수량
/// </summary>
public class InventoryStack
{
    public string SlotId { get; }             //스택 슬롯 아이디
    public string ItemId { get; }          //아이템 아이디
    public PurchasableData Data { get; }      //아이템 데이터
    public int Quantity { get; }              //현재 스택 수량

    /// <summary>
    /// 인벤토리 스택 생성
    /// </summary>
    /// <param name="slotId">스택 슬롯 아이디</param>
    /// <param name="data">아이템 데이터</param>
    /// <param name="quantity">스택 수량</param>
    public InventoryStack ( string slotId, PurchasableData data, int quantity )
    {
        SlotId = slotId;
        ItemId = data.Id;
        Data = data;
        Quantity = quantity;
    }
}