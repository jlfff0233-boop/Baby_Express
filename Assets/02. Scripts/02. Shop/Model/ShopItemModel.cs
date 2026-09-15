using System;


/// <summary>
/// 상품 모델 - 개별 상품 상태 확인 및 변경
/// </summary>
public class ShopItemModel
{
    ShopItem _item;     //관리할 상품

    /// <summary>
    /// 상품
    /// </summary>
    public ShopItem Item => _item;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="item">관리할 상품</param>
    public ShopItemModel ( ShopItem item )
    {
        //예외 처리
        if ( item == null ) throw new ArgumentNullException( nameof( item ), $"{item} 없음" );

        //상품 저장
        _item = item;
    }

    /// <summary>
    /// 재고 보유 여부 확인
    /// </summary>
    /// <param name="quantity">요구 수량</param>
    /// <returns>요구 수량 보유 여부</returns>
    public bool HasStock ( int quantity )
    {
        //잘못된 수량 차단
        if ( quantity <= 0 ) return false;

        //남은 재고와 요구 수량 비교
        return _item.RemainingStock >= quantity;
    }

    /// <summary>
    /// 구매 가능 여부 확인
    /// </summary>
    /// <param name="quantity">구매 수량</param>
    /// <returns>구매 가능 여부</returns>
    public bool CanPurchase ( int quantity )
    {
        //현재 재고 확인
        return HasStock( quantity );
    }

    /// <summary>
    /// 현재 재고 차감
    /// </summary>
    /// <param name="quantity">차감 수량</param>
    /// <returns>재고 차감 성공 여부</returns>
    public bool RemoveStock ( int quantity )
    {
        //구매할 수 없으면 종료
        if ( CanPurchase( quantity ) == false ) return false;

        //현재 재고 수량에서 quantity만큼 차감
        _item.SetStock( _item.RemainingStock - quantity );

        return true;
    }

    /// <summary>
    /// 현재 가격 갱신
    /// </summary>
    /// <param name="price">변경할 가격</param>
    /// <returns>가격 변경 성공 여부</returns>
    public bool SetPrice ( float price )
    {
        //음수 차단
        if ( price < 0 ) return false;

        //현재 가격 갱신
        _item.SetPrice( price );

        return true;
    }

    /// <summary>
    /// 다음 주기 재입고 간격 예약
    /// </summary>
    /// <param name="span">다음 주기부터 적용할 간격</param>
    /// <returns>예약 성공 여부</returns>
    public bool ScheduleRestockSpan ( int span )
    {
        if ( span <= 0 ) return false;

        _item.SetPendingRestockSpan( span );
        return true;
    }

    /// <summary>
    /// 최대 재고 수량 증가
    /// </summary>
    /// <param name="amount">증가 수량</param>
    /// <returns>최대 재고 증가 성공 여부</returns>
    public bool ExpandMaxStock ( int amount )
    {
        //잘못된 수량 차단
        if ( amount <= 0 ) return false;

        //추가 후 수량이 int 범위 초과 시 종료
        if ( _item.MaxStock > int.MaxValue - amount ) return false;

        //최대 재고 수량 갱신
        _item.SetMaxStock( _item.MaxStock + amount );

        return true;
    }

    /// <summary>
    /// 최대 재고 수량 설정
    /// </summary>
    /// <param name="quantity">변경할 최대 재고 수량</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetMaxStock ( int quantity )
    {
        //현재 재고보다 작은 최대 재고 수량 차단
        if ( quantity <= 0 || quantity < _item.RemainingStock ) return false;

        _item.SetMaxStock( quantity );
        return true;
    }

    /// <summary>
    /// 부분 입고
    /// </summary>
    /// <param name="quantity">추가 수량</param>
    /// <returns>재고 추가 성공 여부</returns>
    public bool AddStock ( int quantity )
    {
        //잘못된 수량 차단
        if ( quantity <= 0 || _item.RemainingStock >= _item.MaxStock ) return false;

        //quantity와 추가 가능 수량 중 더 작은 값 반환(최대 재고 수량 초과 방지)
        int amount = Math.Min( quantity, _item.MaxStock - _item.RemainingStock );

        //남은 재고 수량 갱신
        _item.SetStock( _item.RemainingStock + amount );

        return true;
    }

    /// <summary>
    /// 상품 재입고
    /// </summary>
    public void Restock ()
    {
        _item.SetStock( _item.MaxStock );
    }

    /// <summary>
    /// 지정 영업일의 일반 재입고 처리
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <returns>재입고 여부</returns>
    public bool RestockForDay ( int totalDay )
    {
        //자동 재입고가 없거나 아직 예정일이 아니면 제외
        if ( _item.NextRestockDay <= 0 ||
            totalDay < _item.NextRestockDay )
            return false;

        //현재 최대 재고까지 재입고
        Restock( );

        //기존 주기가 끝난 뒤 예약된 새 간격 적용
        _item.ApplyPendingRestockSpan( );

        //현재 간격을 기준으로 다음 재입고 예정일 설정
        _item.SetNextRestockDay(
            _item.RestockSpan > 0
                ? totalDay + _item.RestockSpan
                : 0 );

        return true;
    }
}
