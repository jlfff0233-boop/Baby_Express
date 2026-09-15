using UnityEngine;

/// <summary>
/// 아이템 슬롯 표시 데이터
/// </summary>
public class ItemSlotViewData
{
    public string SlotId { get; }         //슬롯 아이디
    public string ItemId { get; }         //아이템 아이디
    public Sprite Icon { get; }           //아이템 아이콘
    public string Name { get; }           //아이템 이름
    public int Quantity { get; }          //슬롯 수량
    public string Description { get; }        //슬롯 툴팁 설명

    /// <summary>
    /// 아이템 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="slotId">슬롯 아이디</param>
    /// <param name="itemId">아이템 아이디</param>
    /// <param name="icon">아이템 아이콘</param>
    /// <param name="name">아이템 이름</param>
    /// <param name="quantity">슬롯 수량</param>
    /// <param name="description">슬롯 툴팁 설명</param>
    public ItemSlotViewData ( string slotId, string itemId,
        Sprite icon, string name, int quantity, string description = null )
    {
        SlotId = slotId;
        ItemId = itemId;
        Icon = icon;
        Name = name;
        Quantity = quantity;
        Description = description;
    }
}
