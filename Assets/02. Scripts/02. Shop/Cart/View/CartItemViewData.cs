
/// <summary>
/// 장바구니 슬롯 표시 데이터
/// </summary>
public class CartItemViewData
{
    public string Id { get; }             //상품 아이디
    public string Name { get; }           //상품 이름
    public int Quantity { get; }          //선택 수량
    public float Price { get; }           //현재 가격
    public float Subtotal { get; }        //상품 소계

    /// <summary>
    /// 장바구니 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="id">아이디</param>
    /// <param name="name">이름</param>
    /// <param name="quantity">수량</param>
    /// <param name="price">가격</param>
    /// <param name="subtotal">소계</param>
    public CartItemViewData ( string id, string name, int quantity, float price, float subtotal )
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        Price = price;
        Subtotal = subtotal;
    }
}
