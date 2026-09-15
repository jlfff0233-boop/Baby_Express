using UnityEngine;

/// <summary>
/// 상품 상세 표시 데이터
/// </summary>
public class ShopDetailViewData
{
    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// 상품 아이콘
    /// </summary>
    public Sprite Icon { get; }
    /// <summary>
    /// 상품 이름
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 상품 설명
    /// </summary>
    public string Desc { get; }
    /// <summary>
    /// 상품 가격
    /// </summary>
    public float Price { get; }
    /// <summary>
    /// 남은 재고 수량
    /// </summary>
    public int RemainingStock { get; }
    /// <summary>
    /// 빠른 재입고 가능 여부
    /// </summary>
    public bool CanQuickRestock { get; }
    /// <summary>
    /// 상품 잠금 여부
    /// </summary>
    public bool IsLocked { get; }


    /// <summary>
    /// 상품 상세 표시 데이터 생성자
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="icon">상품 아이콘</param>
    /// <param name="name">상품 이름</param>
    /// <param name="desc">상품 설명</param>
    /// <param name="price">현재 가격</param>
    /// <param name="remainingStock">남은 재고</param>
    /// <param name="isLocked">상품 잠금 여부</param>
    /// <param name="canQuickRestock">빠른 재입고 표시 여부</param>
    public ShopDetailViewData (
        string id, Sprite icon, string name,
        string desc, float price, int remainingStock,
        bool isLocked, bool canQuickRestock )
    {
        Id = id;
        Icon = icon;
        Name = name;
        Desc = desc;
        Price = price;
        RemainingStock = remainingStock;
        IsLocked = isLocked;
        CanQuickRestock = canQuickRestock;
    }
}
