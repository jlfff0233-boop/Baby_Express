using System;

/// <summary>
/// 제작 주문 정렬 방식
/// </summary>
public enum CraftOrderSortType
{
    DeadlineAscending,       //마감일 빠른 순
    DeadlineDescending,      //마감일 늦은 순
    SpecialFirst,        //특수 주문 상단
    NormalFirst,     //일반 주문 상단
}

/// <summary>
/// 제작 주문 목록 프레젠터 - 목록 정렬과 선택 흐름 중재
/// </summary>
public class CraftOrderListPresenter
{
    CustomerOrderModel _orderModel;       //주문 모델
    CraftModel _craftModel;               //제작 모델
    CraftListView _craftListView;         //제작 주문 목록 뷰
    CraftOrderListBuilder _listBuilder;   //제작 주문 목록 빌더

    CraftOrderSortType _selectedSortType =
        CraftOrderSortType.DeadlineAscending;       //현재 주문 정렬 방식
    string _selectedOrderId;              //선택한 주문 아이디
    bool _isSubscribed;                   //이벤트 연결 여부

    /// <summary>
    /// 제작 주문 열기 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnCraftSelected;

    /// <summary>
    /// 제작 목록 주문 선택 완료 이벤트
    /// </summary>
    public event Action<string> OnOrderSelected;

    /// <summary>
    /// 제작 주문 목록 프레젠터 생성
    /// </summary>
    /// <param name="orderModel">주문 모델</param>
    /// <param name="craftModel">제작 모델></param>
    /// <param name="craftListView">제작 주문 목록 뷰</param>
    /// <param name="listBuilder">제작 주문 목룍 뷰 데이터 생성기</param>
    public CraftOrderListPresenter (
        CustomerOrderModel orderModel, CraftModel craftModel,
        CraftListView craftListView, CraftOrderListBuilder listBuilder )
    {
        _orderModel = orderModel;
        _craftModel = craftModel;
        _craftListView = craftListView;
        _listBuilder = listBuilder;
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 제작 주문 목록 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        //주문 선택 이벤트 연결
        _craftListView.OnOrderSelected += SelectCraftOrder;
        //정렬 방식 선택 이벤트 연결
        _craftListView.OnSortSelected += SelectCraftOrderSort;
        //선택 주문 제작 시작 이벤트 연결
        _craftListView.OnCraftStarted += StartSelectedCraft;
        //목록 갱신 이벤트 연결
        _orderModel.OnOrderChanged += HandleOrderChanged;

        _isSubscribed = true;
    }

    /// <summary>
    /// 제작 주문 목록 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _craftListView.OnOrderSelected -= SelectCraftOrder;
        _craftListView.OnSortSelected -= SelectCraftOrderSort;
        _craftListView.OnCraftStarted -= StartSelectedCraft;
        _orderModel.OnOrderChanged -= HandleOrderChanged;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 제작 주문 -----
    /// <summary>
    /// 제작 가능 주문 목록 갱신
    /// </summary>
    /// <param name="restoreSelection">기존 목록 선택 복구 여부</param>
    public void Refresh ( bool restoreSelection )
    {
        //주문 표시
        _craftListView.ShowOrders(
            _listBuilder.Build(
                _orderModel.GetOrders( OrderProgressState.Production ),
                _selectedSortType ) );

        //호출 목적에 맞게 기존 선택 복구 또는 초기화
        if ( restoreSelection )
            RestoreSelection( );
        else
            ClearSelection( );
    }

    /// <summary>
    /// 제작 주문 목록 초기화
    /// </summary>
    public void Reset ()
    {
        _selectedOrderId = null;
        _craftListView.ClearOrders( );
        _craftListView.SetCraftInteractable( false );
    }

    /// <summary>
    /// 제작 목록 주문 선택 상태 설정
    /// </summary>
    /// <param name="orderId">선택할 주문 아이디</param>
    public void SetSelection ( string orderId )
    {
        //선택 주문 저장
        _selectedOrderId = orderId;

        //선택 외곽선과 제작 버튼 상태 갱신
        _craftListView.SetSelectedOrder( orderId );
        _craftListView.SetCraftInteractable( true );
    }

    /// <summary>
    /// 제작 목록 선택 초기화
    /// </summary>
    public void ClearSelection ()
    {
        //선택 주문 초기화
        _selectedOrderId = null;

        //선택 외곽선과 제작 버튼 상태 초기화
        _craftListView.SetSelectedOrder( null );
        _craftListView.SetCraftInteractable( false );
    }

    /// <summary>
    /// 제작 목록 선택 주문 복구
    /// </summary>
    public void RestoreSelection ()
    {
        //목록에서 선택한 주문이 아직 유효하면 유지
        if ( IsProductionOrder( _selectedOrderId ) )
        {
            SetSelection( _selectedOrderId );
            return;
        }

        //현재 제작 중인 주문이 있으면 목록 기본 선택으로 사용
        string currentOrderId = _craftModel.HasSelectedOrder
            ? _craftModel.SelectedOrderId
            : null;

        if ( IsProductionOrder( currentOrderId ) )
        {
            //현재 제작 주문을 목록 선택으로 복구
            SetSelection( currentOrderId );
            return;
        }

        ClearSelection( );
    }

    /// <summary>
    /// 주문 변경에 따른 제작 목록 갱신
    /// </summary>
    void HandleOrderChanged ()
    {
        //현재 선택을 유지하며 진행 중 주문 목록 갱신
        Refresh( true );
    }

    /// <summary>
    /// 제작 주문 정렬 방식 선택
    /// </summary>
    /// <param name="sortIndex">선택한 정렬 번호</param>
    void SelectCraftOrderSort ( int sortIndex )
    {
        if ( sortIndex < 0 ||
            sortIndex > ( int ) CraftOrderSortType.NormalFirst )
            return;

        //선택한 정렬 방식 저장
        _selectedSortType = ( CraftOrderSortType ) sortIndex;

        //현재 제작 주문 목록 갱신
        Refresh( true );
    }

    /// <summary>
    /// 제작 목록 주문 선택
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    void SelectCraftOrder ( string orderId )
    {
        //실제 제작 중 주문인지 확인
        if ( IsProductionOrder( orderId ) == false )
        {
            ClearSelection( );
            return;
        }

        //제작 목록 선택 상태 설정
        SetSelection( orderId );

        //사용자가 실제로 선택한 주문 전달
        OnOrderSelected?.Invoke( orderId );
    }

    /// <summary>
    /// 선택한 주문 제작 시작
    /// </summary>
    void StartSelectedCraft ()
    {
        //선택한 주문이 없으면 종료
        if ( string.IsNullOrWhiteSpace( _selectedOrderId ) )
        {
            ClearSelection( );
            return;
        }

        OnCraftSelected?.Invoke( _selectedOrderId );
    }

    /// <summary>
    /// 제작 중 주문 여부 확인
    /// </summary>
    /// <param name="orderId">확인할 주문 아이디</param>
    /// <returns>제작 가능한 주문 여부</returns>
    bool IsProductionOrder ( string orderId )
    {
        if ( string.IsNullOrWhiteSpace( orderId ) ) return false;

        //주문 상태 반환
        return _orderModel.GetOrder( orderId, out CustomerOrder order )
            && order.ProgressState == OrderProgressState.Production;
    }
    #endregion
}
