/// <summary>
/// 인벤토리 아이템 - 아이템 데이터와 전체 보유 수량 관리
/// </summary>
public class InventoryItem
{
    PurchasableData _data;       //보유 아이템 데이터
    int _quantity;               //전체 보유 수량

    /// <summary>
    /// 보유 아이템 데이터
    /// </summary>
    public PurchasableData Data => _data;

    /// <summary>
    /// 전체 보유 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 인벤토리 아이템 생성
    /// </summary>
    /// <param name="data">보유 아이템 데이터</param>
    /// <param name="quantity">초기 보유 수량</param>
    internal InventoryItem ( PurchasableData data, int quantity )
    {
        _data = data;
        _quantity = quantity;
    }

    /// <summary>
    /// 수량 추가 가능 여부 확인
    /// </summary>
    /// <param name="quantity">추가 수량</param>
    /// <returns>추가 가능 여부</returns>
    internal bool CanAddQuantity ( int quantity )
    {
        //잘못된 추가 수량 차단
        if ( quantity <= 0 ) return false;

        //정수 범위 초과 차단
        return _quantity <= int.MaxValue - quantity;
    }

    /// <summary>
    /// 보유 수량 추가
    /// </summary>
    /// <param name="quantity">추가 수량</param>
    /// <returns>추가 성공 여부</returns>
    internal bool AddQuantity ( int quantity )
    {
        //수량 추가 가능 여부 확인
        if ( CanAddQuantity( quantity ) == false ) return false;

        _quantity += quantity;
        return true;
    }

    /// <summary>
    /// 수량 제거 가능 여부 확인
    /// </summary>
    /// <param name="quantity">제거 수량</param>
    /// <returns>제거 가능 여부</returns>
    internal bool CanRemoveQuantity ( int quantity )
    {
        //잘못된 제거 수량 차단
        if ( quantity <= 0 ) return false;

        return _quantity >= quantity;
    }

    /// <summary>
    /// 보유 수량 제거
    /// </summary>
    /// <param name="quantity">제거 수량</param>
    /// <returns>제거 성공 여부</returns>
    internal bool RemoveQuantity ( int quantity )
    {
        //수량 제거 가능 여부 확인
        if ( CanRemoveQuantity( quantity ) == false ) return false;

        _quantity -= quantity;
        return true;
    }
}
