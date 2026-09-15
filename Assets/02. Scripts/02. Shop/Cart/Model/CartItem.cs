/// <summary>
/// 장바구니 상품 - 등록된 상품, 선택 수량 관리
/// </summary>
public class CartItem
{
    PurchasableData _data;       //등록된 상품 데이터
    int _quantity;       //선택 수량

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 등록된 상품 데이터
    /// </summary>
    public PurchasableData Data => _data;
    /// <summary>
    /// 선택 수량
    /// </summary>
    public int Quantity => _quantity;
    #endregion


    /// <summary>
    /// 장바구니 상품 생성
    /// </summary>
    /// <param name="data">등록된 상품 데이터</param>
    /// <param name="quantity">선택 수량</param>
    public CartItem ( PurchasableData data,  int quantity )
    {
        _data = data;
        _quantity = quantity;
    }

    /// <summary>
    /// 수량 선택(직접 설정)
    /// </summary>
    /// <param name="quantity">변경할 수량</param>
    public void SetQuantity ( int quantity )
    {
        //전달받은 수량으로 설정
        _quantity = quantity;
    }

    /// <summary>
    /// 수량 변경
    /// </summary>
    /// <param name="amount">변경 수량</param>
    public void ChangeQuantity ( int amount )
    {
        //현재 수량에 amount만큼 더하거나 빼기
        _quantity += amount;
    }
}
