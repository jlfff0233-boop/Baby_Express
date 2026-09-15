/// <summary>
/// 인벤토리 아이템 삭제 패널 표시 데이터
/// </summary>
public class InventoryItemDeleteViewData
{
    public string ItemName { get; }          //아이템 이름
    public int MaxQuantity { get; }          //선택한 스택의 최대 삭제 수량
    public int SelectedQuantity { get; }     //현재 선택한 삭제 수량

    /// <summary>
    /// 삭제 패널 표시 데이터 생성
    /// </summary>
    public InventoryItemDeleteViewData (
        string itemName, int maxQuantity, int selectedQuantity )
    {
        ItemName = itemName;
        MaxQuantity = maxQuantity;
        SelectedQuantity = selectedQuantity;
    }
}
