using UnityEngine;

/// <summary>
/// 상점 슬롯 표시 데이터 - 아이디, 아이콘, 이름, 가격, 재고, 품절 여부
/// </summary>
public class ShopSlotViewData
{
    public string Id { get; }
    public Sprite Icon { get; }
    public string Name { get; }
    public float Price { get; }
    public int RemainingStock { get; }
    public bool IsSoldOut { get; }
    public bool IsLocked { get; }

    /// <summary>
    /// 상점 슬롯 표시 데이터 생성자
    /// </summary>
    /// <param name="id">아이디</param>
    /// <param name="icon">아이콘</param>
    /// <param name="name">이름</param>
    /// <param name="price">가격</param>
    /// <param name="remainingStock">재고 수량</param>
    /// <param name="isSoldOut">품절 여부</param>
    /// <param name="isLocked">잠금 여부</param>
    public ShopSlotViewData ( string id, Sprite icon, string name,
        float price, int remainingStock, bool isSoldOut, bool isLocked )
    {
        //표시 데이터 저장
        Id = id;
        Icon = icon;
        Name = name;
        Price = price;
        RemainingStock = remainingStock;
        IsSoldOut = isSoldOut;
        IsLocked = isLocked;
    }
}
