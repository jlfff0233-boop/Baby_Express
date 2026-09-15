using System;
using System.Collections.Generic;

/// <summary>
/// 주문 목록 프레젠터 - 탭 상태와 주문 슬롯 표시 중재
/// </summary>
public class OrderListPresenter
{
    const int ClosedOrderDisplayLimit = 20;       //종료 주문 최대 표시 수
    const string WaitingCountTooltip = "수락 대기 주문 수 / 최대 대기 주문 수";
    const string ProducingCountTooltip = "제작 완료 주문 수 / 전체 진행 중 주문 수";
    const string ClosedCountTooltip = "최근 종료 주문 수 / 전체 종료 주문 수";

    CustomerOrderModel _orderModel;       //고객 주문 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    CustomerOrderView _orderView;         //고객 주문 뷰
    OrderViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    OrderTab _selectedTab = OrderTab.Waiting;       //현재 선택한 탭
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 현재 선택한 주문 탭
    /// </summary>
    public OrderTab SelectedTab => _selectedTab;

    /// <summary>
    /// 주문 탭 변경 이벤트
    /// </summary>
    public event Action OnTabChanged;

    /// <summary>
    /// 주문 목록 프레젠터 생성
    /// </summary>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="orderView">고객 주문 뷰</param>
    /// <param name="viewDataBuilder">표시 데이터 생성기</param>
    public OrderListPresenter (
        CustomerOrderModel orderModel, PlayStateModel playStateModel,
        CustomerOrderView orderView, OrderViewDataBuilder viewDataBuilder )
    {
        _orderModel = orderModel;
        _playStateModel = playStateModel;
        _orderView = orderView;
        _viewDataBuilder = viewDataBuilder;
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 주문 목록 입력과 상태 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _orderView.OnWaitingSelected += SelectWaiting;
        _orderView.OnProducingSelected += SelectProducing;
        _orderView.OnClosedSelected += SelectClosed;

        _orderModel.OnOrderChanged += Refresh;
        _playStateModel.OnDateChanged += RefreshDate;
        _playStateModel.OnItemUnlockChanged += RefreshInformationUnlock;

        _isSubscribed = true;
    }

    /// <summary>
    /// 주문 목록 입력과 상태 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _orderView.OnWaitingSelected -= SelectWaiting;
        _orderView.OnProducingSelected -= SelectProducing;
        _orderView.OnClosedSelected -= SelectClosed;

        _orderModel.OnOrderChanged -= Refresh;
        _playStateModel.OnDateChanged -= RefreshDate;
        _playStateModel.OnItemUnlockChanged -= RefreshInformationUnlock;

        _isSubscribed = false;
    }

    /// <summary>
    /// 날짜 변경 후 주문 목록 갱신
    /// </summary>
    /// <param name="month">현재 월</param>
    /// <param name="day">현재 일</param>
    void RefreshDate ( int month, int day )
    {
        Refresh( );
    }

    /// <summary>
    /// 기한 정보 해금 후 주문 목록 갱신
    /// </summary>
    /// <param name="id">해금 아이디</param>
    /// <param name="isUnlocked">해금 여부</param>
    void RefreshInformationUnlock ( string id, bool isUnlocked )
    {
        if ( id != InformationUnlockId.OrderDeadlineAlert ) return;

        Refresh( );
    }
    #endregion

    #region ----- 주문 목록 -----
    /// <summary>
    /// 기본 주문 탭 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _selectedTab = OrderTab.Waiting;
    }

    /// <summary>
    /// 지정 주문 탭 선택
    /// </summary>
    /// <param name="tab">선택할 주문 탭</param>
    public void SelectTab ( OrderTab tab )
    {
        _selectedTab = tab;
        OnTabChanged?.Invoke( );
        Refresh( );
    }

    /// <summary>
    /// 현재 탭 주문 목록 갱신
    /// </summary>
    public void Refresh ()
    {
        IReadOnlyList<CustomerOrder> orders = GetSelectedOrders( );

        _orderView.CreateSlotList(
            _viewDataBuilder.CreateSlots( orders, _playStateModel ) );
        UpdateOrderCount( orders.Count );
        _orderView.UpdateSelectedTab( _selectedTab );
    }

    /// <summary>
    /// 현재 탭 주문 목록 조회
    /// </summary>
    /// <returns>현재 탭 주문 목록</returns>
    IReadOnlyList<CustomerOrder> GetSelectedOrders ()
    {
        switch ( _selectedTab )
        {
            case OrderTab.Waiting:
                return _orderModel.GetOrders( OrderProgressState.Waiting );
            case OrderTab.Producing:
                return _orderModel.GetProgressOrders( );
            case OrderTab.Closed:
                return _orderModel.GetRecentClosedOrders(
                    ClosedOrderDisplayLimit );
            default:
                return Array.Empty<CustomerOrder>( );
        }
    }

    /// <summary>
    /// 현재 탭 주문 수 표시 갱신
    /// </summary>
    /// <param name="displayCount">현재 표시 중인 주문 수</param>
    void UpdateOrderCount ( int displayCount )
    {
        switch ( _selectedTab )
        {
            case OrderTab.Waiting:
                _orderView.UpdateOrderCount(
                    displayCount, _orderModel.WaitingLimit,
                    WaitingCountTooltip );
                break;

            case OrderTab.Producing:
                int craftedCount = _orderModel.GetOrderCount(
                    OrderProgressState.Crafted );
                _orderView.UpdateOrderCount(
                    craftedCount, displayCount,
                    ProducingCountTooltip );
                break;

            case OrderTab.Closed:
                int closedCount = _orderModel.GetOrderCount(
                    OrderProgressState.Closed );
                _orderView.UpdateOrderCount(
                    displayCount, closedCount,
                    ClosedCountTooltip );
                break;
        }
    }
    #endregion

    #region ----- 주문 탭 -----
    /// <summary>
    /// 수락 대기 주문 탭 선택
    /// </summary>
    void SelectWaiting ()
    {
        SelectTab( OrderTab.Waiting );
    }

    /// <summary>
    /// 진행 중 주문 탭 선택
    /// </summary>
    void SelectProducing ()
    {
        SelectTab( OrderTab.Producing );
    }

    /// <summary>
    /// 종료 주문 탭 선택
    /// </summary>
    void SelectClosed ()
    {
        SelectTab( OrderTab.Closed );
    }
    #endregion
}
