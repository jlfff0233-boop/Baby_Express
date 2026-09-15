using System;


/// <summary>
/// 상품 - 개별 상품 현재 상태
/// </summary>
public class ShopItem
{
    PurchasableData _data;      //상품 데이터
    float _currentPrice;        //현재 가격
    int _remainingStock;        //남은 재고 수량
    int _maxStock;      //최대 재고 수량
    int _restockSpan;       //재입고 간격
    int _nextRestockDay;       //다음 일반 재입고 예정일
    int _pendingRestockSpan;       //다음 주기부터 적용할 재입고 간격

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 상품 데이터
    /// </summary>
    public PurchasableData Data => _data;
    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string Id => _data.Id;
    /// <summary>
    /// 현재 가격
    /// </summary>
    public float CurrentPrice => _currentPrice;
    /// <summary>
    /// 남은 재고 수량
    /// </summary>
    public int RemainingStock => _remainingStock;
    /// <summary>
    /// 최대 재고 수량
    /// </summary>
    public int MaxStock => _maxStock;
    /// <summary>
    /// 재입고 간격
    /// </summary>
    public int RestockSpan => _restockSpan;
    /// <summary>
    /// 품절 여부
    /// </summary>
    public bool IsSoldOut => _remainingStock <= 0;
    /// <summary>
    /// 다음 일반 재입고 예정일
    /// </summary>
    public int NextRestockDay => _nextRestockDay;
    /// <summary>
    /// 적용 대기 재입고 간격
    /// </summary>
    public int PendingRestockSpan => _pendingRestockSpan;
    /// <summary>
    /// 적용 대기 간격 여부
    /// </summary>
    public bool HasPendingRestockSpan => _pendingRestockSpan > 0;

    #endregion

    /// <summary>
    /// 상품 생성자
    /// </summary>
    /// <param name="data">구매 가능 상품 데이터</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    public ShopItem ( PurchasableData data, int totalDay )
    {
        if ( data == null )
            throw new ArgumentNullException( nameof( data ), $"{data}가 없습니다." );

        _data = data;

        _currentPrice = data.BasePrice;
        _maxStock = data.BaseStockQuantity;
        _remainingStock = _maxStock;
        _restockSpan = data.BaseRestockSpan;
        _nextRestockDay = _restockSpan > 0
            ? totalDay + _restockSpan
            : 0;
    }


    /// <summary>
    /// 현재 가격 갱신
    /// </summary>
    /// <param name="price">변경 가격</param>
    public void SetPrice ( float price )
    {
        _currentPrice = price;
    }

    /// <summary>
    /// 남은 재고 수량 갱신
    /// </summary>
    /// <param name="quantity">변경 수량</param>
    public void SetStock ( int quantity )
    {
        _remainingStock = quantity;
    }

    /// <summary>
    /// 최대 재입고 수량 갱신
    /// </summary>
    /// <param name="quantity">변경 재고 수량</param>
    public void SetMaxStock ( int quantity )
    {
        _maxStock = quantity;
    }

    /// <summary>
    /// 현재 재입고 간격 갱신
    /// </summary>
    /// <param name="span">변경 재입고 간격</param>
    public void SetRestockSpan ( int span )
    {
        _restockSpan = span;
    }

    /// <summary>
    /// 다음 주기 재입고 간격 설정
    /// </summary>
    /// <param name="span">적용 대기 재입고 간격</param>
    public void SetPendingRestockSpan ( int span )
    {
        _pendingRestockSpan = span;
    }

    /// <summary>
    /// 적용 대기 중인 재입고 간격 반영
    /// </summary>
    public void ApplyPendingRestockSpan ()
    {
        if ( HasPendingRestockSpan == false ) return;

        _restockSpan = _pendingRestockSpan;
        _pendingRestockSpan = 0;
    }

    /// <summary>
    /// 다음 일반 재입고 예정일 설정
    /// </summary>
    /// <param name="totalDay">다음 재입고 예정일</param>
    public void SetNextRestockDay ( int totalDay )
    {
        _nextRestockDay = totalDay;
    }
}
