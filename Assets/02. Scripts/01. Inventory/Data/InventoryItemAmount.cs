
/// <summary>
/// 인벤토리 아이템 수량 데이터
/// </summary>
public class InventoryItemAmount
{
    public PurchasableData Data { get; }      //상품 데이터
    public int Quantity { get; }              //처리 수량

    /// <summary>
    /// 아이템 수량 데이터 생성
    /// </summary>
    /// <param name="data">아이템 데이터</param>
    /// <param name="quantity">처리 수량</param>
    public InventoryItemAmount ( PurchasableData data, int quantity )
    {
        Data = data;
        Quantity = quantity;
    }
}