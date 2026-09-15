using System;
using UnityEngine;

/// <summary>
/// 주문 상세 프레젠터 - 선택 주문 표시와 수락, 거절, 취소 중재
/// </summary>
public class OrderDetailPresenter
{
    CustomerOrderModel _orderModel;       //고객 주문 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    InventoryModel _inventoryModel;       //인벤토리 모델
    ShopModel _shopModel;                 //상점 모델
    CraftCompleteModel _craftCompleteModel;       //제작 확정 모델
    CustomerOrderView _orderView;         //고객 주문 뷰
    OrderViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기
    Func<OrderTab> _getSelectedTab;       //현재 주문 탭 조회

    string _selectedOrderId;       //현재 선택한 주문 아이디
    string _pendingOrderId;        //경고 확인 대기 주문 아이디
    bool _isSubscribed;            //이벤트 연결 여부

    /// <summary>
    /// 제작 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToCraft;

    /// <summary>
    /// 주문 처리 완료 이벤트
    /// </summary>
    public event Action OnOrderCompleted;

    /// <summary>
    /// 배송 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToDelivery;

    /// <summary>
    /// 배송 결과 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToResult;

    /// <summary>
    /// 주문 상세 표시 이벤트
    /// </summary>
    public event Action<string> OnDetailOpened;

    /// <summary>
    /// 주문 수락 완료 이벤트
    /// </summary>
    public event Action<string> OnOrderAccepted;


    /// <summary>
    /// 주문 상세 프레젠터 생성
    /// </summary>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="craftCompleteModel">제작 확정 모델</param>
    /// <param name="orderView">고객 주문 뷰</param>
    /// <param name="viewDataBuilder">표시 데이터 생성기</param>
    /// <param name="getSelectedTab">현재 주문 탭 조회 함수</param>
    public OrderDetailPresenter (
        CustomerOrderModel orderModel , PlayStateModel playStateModel ,
        InventoryModel inventoryModel , ShopModel shopModel ,
        CraftCompleteModel craftCompleteModel , CustomerOrderView orderView ,
        OrderViewDataBuilder viewDataBuilder , Func<OrderTab> getSelectedTab )
    {
        _orderModel = orderModel;
        _playStateModel = playStateModel;
        _inventoryModel = inventoryModel;
        _shopModel = shopModel;
        _craftCompleteModel = craftCompleteModel;
        _orderView = orderView;
        _viewDataBuilder = viewDataBuilder;
        _getSelectedTab = getSelectedTab;
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 주문 상세 입력과 상태 이벤트 연결
    /// </summary>
    public void SubscribeEvents ( )
    {
        if ( _isSubscribed ) return;

        _orderView.OnSlotSelected += ShowOrder;
        _orderView.OnOrderConfirmed += ConfirmOrder;
        _orderView.OnOrderCanceled += ShowCancelWarning;
        _orderView.OnDetailClose += Hide;
        _orderView.OnWarningConfirmed += ConfirmCancelOrder;
        _orderView.OnWarningCanceled += CloseWarning;

        _orderModel.OnOrderChanged += Refresh;
        _inventoryModel.OnInventoryChanged += Refresh;
        _playStateModel.OnDateChanged += RefreshDate;
        _playStateModel.OnItemUnlockChanged += RefreshInformationUnlock;

        _isSubscribed = true;
    }

    /// <summary>
    /// 주문 상세 입력과 상태 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ( )
    {
        if ( _isSubscribed == false ) return;

        _orderView.OnSlotSelected -= ShowOrder;
        _orderView.OnOrderConfirmed -= ConfirmOrder;
        _orderView.OnOrderCanceled -= ShowCancelWarning;
        _orderView.OnDetailClose -= Hide;
        _orderView.OnWarningConfirmed -= ConfirmCancelOrder;
        _orderView.OnWarningCanceled -= CloseWarning;

        _orderModel.OnOrderChanged -= Refresh;
        _inventoryModel.OnInventoryChanged -= Refresh;
        _playStateModel.OnDateChanged -= RefreshDate;
        _playStateModel.OnItemUnlockChanged -= RefreshInformationUnlock;

        _isSubscribed = false;
    }

    /// <summary>
    /// 날짜 변경 후 선택 주문 상세 갱신
    /// </summary>
    /// <param name="month">현재 월</param>
    /// <param name="day">현재 일</param>
    void RefreshDate ( int month , int day )
    {
        Refresh ( );
    }

    /// <summary>
    /// 주문 편의 정보 해금 후 선택 주문 상세 갱신
    /// </summary>
    /// <param name="id">해금 아이디</param>
    /// <param name="isUnlocked">해금 여부</param>
    void RefreshInformationUnlock ( string id , bool isUnlocked )
    {
        if ( InformationUnlockId.IsDefined ( id ) == false ) return;

        Refresh ( );
    }
    #endregion

    #region ----- 상세 표시 -----

    /// <summary>
    /// 선택한 주문 상세 표시
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    public void ShowOrder ( string orderId )
    {
        //주문 조회
        if ( _orderModel.GetOrder ( orderId , out var order ) == false ) return;

        //제작 완료거나 배송 중이라면 배송 화면 표시
        if ( order.ProgressState == OrderProgressState.Crafted ||
            order.ProgressState == OrderProgressState.Shipping )
        {
            Hide ( );
            OnMoveToDelivery?.Invoke ( orderId );
            return;
        }

        //배송 완료 주문이면 배송 결과 화면 표시
        if ( order.ProgressState == OrderProgressState.Closed &&
            ( order.Outcome == OrderOutcome.NormalDelivery ||
            order.Outcome == OrderOutcome.LateDelivery ) )
        {
            Hide ( );
            OnMoveToResult?.Invoke ( orderId );
            return;
        }

        _selectedOrderId = orderId;
        ShowDetail ( order );

        OnDetailOpened?.Invoke ( orderId );
    }

    /// <summary>
    /// 선택 주문 상세 표시 갱신
    /// </summary>
    public void Refresh ( )
    {
        if ( string.IsNullOrWhiteSpace ( _selectedOrderId ) ) return;

        //주문 조회
        if ( _orderModel.GetOrder ( _selectedOrderId , out var order ) == false )
        {
            Hide ( );
            return;
        }

        //선택 주문이 현재 탭을 벗어나면 상세 패널 닫기
        if ( IsOrderInTab ( order , _getSelectedTab ( ) ) == false )
        {
            Hide ( );
            return;
        }

        ShowDetail ( order );
    }

    /// <summary>
    /// 주문이 지정 탭에 포함되는지 확인
    /// </summary>
    /// <param name="order">확인할 주문</param>
    /// <param name="tab">확인할 주문 탭</param>
    /// <returns>지정 탭 포함 여부</returns>
    bool IsOrderInTab ( CustomerOrder order , OrderTab tab )
    {
        switch ( tab )
        {
            case OrderTab.Waiting:
                return order.ProgressState == OrderProgressState.Waiting;

            case OrderTab.Producing:
                return order.ProgressState == OrderProgressState.Production ||
                    order.ProgressState == OrderProgressState.Crafted ||
                    order.ProgressState == OrderProgressState.Shipping;

            case OrderTab.Closed:
                return order.ProgressState == OrderProgressState.Closed;

            default:
                return false;
        }
    }

    /// <summary>
    /// 주문 상세 표시
    /// </summary>
    /// <param name="order">표시할 주문</param>
    void ShowDetail ( CustomerOrder order )
    {
        _orderView.ShowDetail (
            _viewDataBuilder.CreateDetail (
                order , _playStateModel ,
                _inventoryModel , _shopModel ) );
    }

    /// <summary>
    /// 주문 상세과 경고 패널 숨김
    /// </summary>
    public void Hide ( )
    {
        CloseWarning ( );
        _selectedOrderId = string.Empty;
        _orderView.HideDetail ( );
    }
    #endregion

    #region ----- 주문 처리 -----
    /// <summary>
    /// 주문 확인 처리
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void ConfirmOrder ( string orderId )
    {
        if ( _orderModel.GetOrder ( orderId , out var order ) == false ) return;

        if ( order.ProgressState == OrderProgressState.Production )
        {
            OnMoveToCraft?.Invoke ( orderId );
            Hide ( );
            return;
        }

        if ( order.ProgressState != OrderProgressState.Waiting ) return;

        OrderResult result = _orderModel.AcceptOrder (
            orderId , _playStateModel.TotalDay );

        LogFailure ( "주문 확인" , result );

        if ( result != OrderResult.Success ) return;

        Hide ( );
        OnOrderAccepted?.Invoke ( orderId );
        OnOrderCompleted?.Invoke ( );
    }

    /// <summary>
    /// 주문 거절 또는 취소 경고 표시
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void ShowCancelWarning ( string orderId )
    {
        if ( _orderModel.GetOrder ( orderId , out var order ) == false ) return;

        //보호 주문은 거절과 취소 경고도 표시하지 않음
        if ( _orderModel.IsOrderProtected ( orderId ) ) return;

        string title;
        string description;
        string confirmText;

        if ( order.ProgressState == OrderProgressState.Waiting )
        {
            title = "주문 거절";
            description =
                "주문을 거절하시겠습니까?\n" +
                "거절하면 평가 페널티를 받습니다.";
            confirmText = "거절";
        }
        else if ( order.ProgressState == OrderProgressState.Production ||
            order.ProgressState == OrderProgressState.Crafted )
        {
            title = "주문 취소";
            description =
                "주문을 취소하시겠습니까?\n" +
                "사용된 파츠와 제작물은 복구되지 않습니다.";
            confirmText = "취소";
        }
        else return;

        _pendingOrderId = orderId;
        _orderView.ShowWarning ( title , description , confirmText , "돌아가기" );
    }

    /// <summary>
    /// 주문 거절 또는 취소 확정
    /// </summary>
    void ConfirmCancelOrder ( )
    {
        string orderId = _pendingOrderId;
        CloseWarning ( );

        //경고 표시 이후 보호된 경우 실제 처리도 차단
        if ( _orderModel.IsOrderProtected ( orderId ) ) return;

        //주문 조회
        if ( _orderModel.GetOrder ( orderId , out var order ) == false ) return;

        //취소하려는 주문이 제작 완료 상태인지 여부
        bool discardCraftResult = order.ProgressState == OrderProgressState.Crafted;
        OrderResult result;

        //대기 중 상태라면
        if ( order.ProgressState == OrderProgressState.Waiting )
        {
            //주문 거절
            result = _orderModel.RejectOrder ( orderId , _playStateModel.TotalDay );
        }
        //제작 중 상태거나 제작 완료 상태라면
        else if ( order.ProgressState == OrderProgressState.Production ||
            order.ProgressState == OrderProgressState.Crafted )
        {
            //주문 취소
            result = _orderModel.CancelOrder ( orderId , _playStateModel.TotalDay );
        }
        else
        {
            result = OrderResult.InvalidState;
        }

        LogFailure ( "주문 거절 또는 취소" , result );

        if ( result != OrderResult.Success ) return;

        //주문 처리 결과
        if ( discardCraftResult )
            _craftCompleteModel.DiscardResult ( orderId );

        Hide ( );
        OnOrderCompleted?.Invoke ( );
    }

    /// <summary>
    /// 주문 경고 패널 닫기
    /// </summary>
    void CloseWarning ( )
    {
        _pendingOrderId = string.Empty;
        _orderView.HideWarning ( );
    }

    /// <summary>
    /// 주문 처리 실패 로그
    /// </summary>
    /// <param name="action">처리 내용</param>
    /// <param name="result">주문 처리 결과</param>
    void LogFailure ( string action , OrderResult result )
    {
        if ( result == OrderResult.Success ) return;

        Debug.Log ( $"{action} 실패: {result}" );
    }
    #endregion
}
