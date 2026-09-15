using System.Collections.Generic;

/// <summary>
/// 제작 주문 목록 빌더 - 정렬과 슬롯 표시 데이터 생성
/// </summary>
public class CraftOrderListBuilder
{
    PlayStateModel _playStateModel;       //플레이 상태 모델
    OrderIconData _iconData;              //주문 공용 상태 아이콘 데이터

    /// <summary>
    /// 제작 주문 목록 빌더 생성
    /// </summary>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="iconData">주문 공용 상태 아이콘 데이터</param>
    public CraftOrderListBuilder (
        PlayStateModel playStateModel, OrderIconData iconData )
    {
        _playStateModel = playStateModel;
        _iconData = iconData;
    }

    /// <summary>
    /// 제작 주문 슬롯 목록 생성
    /// </summary>
    /// <param name="orders">제작 주문 목록</param>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>주문 슬롯 표시 데이터</returns>
    public IReadOnlyList<OrderSlotViewData> Build (
        IReadOnlyList<CustomerOrder> orders, CraftOrderSortType sortType )
    {
        var sortedOrders = new List<CustomerOrder>( orders );

        //선택한 방식으로 제작 주문 정렬
        sortedOrders.Sort( ( first, second ) =>
            CompareCraftOrders( first, second, sortType ) );

        //주문 슬롯 표시 데이터 생성
        var viewDatas = new List<OrderSlotViewData>( sortedOrders.Count );

        for ( int i = 0; i < sortedOrders.Count; i++ )
        {
            //뷰 리스트에 추가
            viewDatas.Add( CreateCraftOrderSlotData( sortedOrders [ i ] ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 제작 주문 표시 순서 비교
    /// </summary>
    /// <param name="first">첫 번째 비교 주문</param>
    /// <param name="second">두 번째 비교 주문</param>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>정렬 비교값</returns>
    int CompareCraftOrders (
        CustomerOrder first, CustomerOrder second,
        CraftOrderSortType sortType )
    {
        int result;

        switch ( sortType )
        {
            case CraftOrderSortType.DeadlineDescending:
                //납품 마감일이 늦은 주문 우선
                result = second.DeliveryDue.CompareTo( first.DeliveryDue );
                break;
            case CraftOrderSortType.SpecialFirst:
                //특수 주문 우선
                result = second.IsSpecial.CompareTo( first.IsSpecial );
                break;
            case CraftOrderSortType.NormalFirst:
                //일반 주문 우선
                result = first.IsSpecial.CompareTo( second.IsSpecial );
                break;
            default:
                //납품 마감일이 빠른 주문 우선
                result = first.DeliveryDue.CompareTo( second.DeliveryDue );
                break;
        }

        if ( result != 0 ) return result;

        //같은 우선순위면 주문 번호순 정렬
        return first.CreatedNumber.CompareTo( second.CreatedNumber );
    }

    /// <summary>
    /// 제작 주문 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <returns>주문 슬롯 표시 데이터</returns>
    OrderSlotViewData CreateCraftOrderSlotData ( CustomerOrder order )
    {
        //주문 생성일 변환
        _playStateModel.GetDate(
            order.CreatedTotalDay, out int createdMonth, out int createdDay );

        return new OrderSlotViewData(
            order.OrderId, order.CreatedNumber.ToString( ),
            createdMonth, createdDay, GetCraftDeadlineText( order ),
            _iconData.GetIcon( OrderProgressState.Production ),
            order.IsSpecial );
    }

    /// <summary>
    /// 제작 주문 기한 문구 생성
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <returns>기한 표시 문구</returns>
    string GetCraftDeadlineText ( CustomerOrder order )
    {
        //현재 누적 영업일
        int currentTotalDay = _playStateModel.TotalDay;

        //정상 납품 기한 전
        if ( currentTotalDay <= order.DeliveryDue )
        {
            int remainDay = order.DeliveryDue - currentTotalDay;
            return $"납품 마감: D-{remainDay}";
        }

        //지연 납품 최종 기한
        int finalRemainDay = order.FinalDeadline - currentTotalDay;

        return finalRemainDay < 0
            ? "최종 실패 기한: 만료"
            : $"최종 실패: D-{finalRemainDay}";
    }
}
