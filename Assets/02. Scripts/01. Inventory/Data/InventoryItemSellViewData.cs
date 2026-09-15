using UnityEngine;

/// <summary>
/// 인벤토리 아이템 판매 패널 표시 데이터
/// </summary>
public class InventoryItemSellViewData
{
    public Sprite Icon { get; }              //아이템 아이콘
    public string ItemName { get; }          //아이템 이름
    public float UnitPrice { get; }          //개당 판매 가격
    public int MaxQuantity { get; }          //선택한 스택의 최대 판매 수량
    public int SelectedQuantity { get; }     //현재 선택한 판매 수량
    public float TotalPrice { get; }         //예상 획득 금액

    /// <summary>
    /// 판매 패널 표시 데이터 생성
    /// </summary>
    public InventoryItemSellViewData (
        Sprite icon, string itemName, float unitPrice,
        int maxQuantity, int selectedQuantity, float totalPrice )
    {
        Icon = icon;
        ItemName = itemName;
        UnitPrice = unitPrice;
        MaxQuantity = maxQuantity;
        SelectedQuantity = selectedQuantity;
        TotalPrice = totalPrice;
    }
}
