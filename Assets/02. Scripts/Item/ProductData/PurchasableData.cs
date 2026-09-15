using UnityEngine;
/// <summary>
/// 구매 가능 상품 타입
/// </summary>
public enum ProductType
{
    BabyPart,   // 제작용 신체 파츠
    Furniture,  // 가구, 진열대, 보관함 등
    Equipment,  // 제작 장비
    Expansion,  // 시설 확장
    Consumable  // 일회용 소모품(포장 상자 등)
}

/// <summary>
/// 구매 가능 상품 공통 데이터
/// </summary>
public abstract class PurchasableData : ScriptableObject
{
    [Header ( "----- 설정 데이터(공통) -----" )]
    [SerializeField] protected ProductType _productType;      //상품 타입
    [SerializeField] protected string _name;       //상품 이름
    [SerializeField] protected string _id;     //상품 아이디
    [SerializeField] protected float _basePrice;        //상품 기본 가격
    [SerializeField] protected int _baseStockQuantity;      //기본 재고 수량
    [SerializeField] protected int _baseRestockSpan;        //기본 재입고 간격
    [SerializeField] protected Sprite _icon;       //상품 아이콘
    [TextArea ( 5 , 10 )][SerializeField] protected string _disc;        //상품 설명

    /// <summary>
    /// 상품 타입
    /// </summary>
    public ProductType ProductType => _productType;
    /// <summary>
    /// 상품 이름
    /// </summary>
    public string Name => _name;
    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string Id => _id;
    /// <summary>
    /// 상품 가격
    /// </summary>
    public float BasePrice => _basePrice;
    /// <summary>
    /// 기본 재고 수량
    /// </summary>
    public int BaseStockQuantity => _baseStockQuantity;
    /// <summary>
    /// 기본 재입고 간격
    /// </summary>
    public int BaseRestockSpan => _baseRestockSpan;
    /// <summary>
    /// 상품 설명
    /// </summary>
    public string Description => _disc;
    /// <summary>
    /// 상품 아이콘
    /// </summary>
    public Sprite Icon => _icon;

}
