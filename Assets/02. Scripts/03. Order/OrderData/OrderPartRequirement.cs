using System;

/// <summary>
/// 주문 파츠 조건 - 파츠 아이디와 수량 관리
/// </summary>
public class OrderPartCondition
{
    string _partId;       //조건 파츠 아이디
    int _quantity;       //조건 수량

    /// <summary>
    /// 조건 파츠 아이디
    /// </summary>
    public string PartId => _partId;

    /// <summary>
    /// 조건 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 주문 파츠 조건 생성
    /// </summary>
    /// <param name="partId">조건 파츠 아이디</param>
    /// <param name="quantity">조건 수량</param>
    public OrderPartCondition ( string partId, int quantity )
    {
        //잘못된 파츠 아이디 차단
        if ( string.IsNullOrWhiteSpace( partId ) )
            throw new ArgumentException( nameof( partId ) );

        //잘못된 조건 수량 차단
        if ( quantity <= 0 )
            throw new ArgumentOutOfRangeException( nameof( quantity ) );

        _partId = partId;
        _quantity = quantity;
    }
}
