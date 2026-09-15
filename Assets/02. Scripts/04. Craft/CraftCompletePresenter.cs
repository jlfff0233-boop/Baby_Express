using System;

/// <summary>
/// 제작 확정 프레젠터 - 제작 완료와 배송 이동 흐름 중재
/// </summary>
public class CraftCompletePresenter
{
    CraftCompleteModel _completeModel;       //제작 확정 모델
    BusinessDayModel _businessDayModel;              //영업일 모델
    CraftReviewViewBuilder _reviewViewBuilder;       //제작 판정 표시 데이터 생성기
    CraftView _craftView;                    //제작 뷰
    CraftCompleteView _completeView;         //제작 완료 뷰
    /// <summary>
    /// 현재 제작 주문 조회
    /// </summary>
    Func<CustomerOrder> _getSelectedOrder;

    string _completedOrderId;                //완료 패널에 표시 중인 주문 아이디
    bool _isCompletingCraft;                 //제작 확정 처리 중 여부
    bool _isSubscribed;                      //이벤트 연결 여부

    /// <summary>
    /// 제작 확정 처리 중 여부
    /// </summary>
    public bool IsCompleting => _isCompletingCraft;

    /// <summary>
    /// 제작 확정 성공 이벤트
    /// </summary>
    public event Action OnCraftSucceeded;

    /// <summary>
    /// 제작 완료 주문 배송 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToDelivery;

    /// <summary>
    /// 제작 확정 프레젠터 생성
    /// </summary>
    /// <param name="completeModel">제작 확정 모델</param>
    /// <param name="businessDayModel">영업일 모델</param>
    /// <param name="reviewViewBuilder">제작 판정 표시 데이터 생성기</param>
    /// <param name="craftView">제작 뷰</param>
    /// <param name="completeView">제작 완료 뷰</param>
    /// <param name="getSelectedOrder">현재 제작 주문 조회 함수</param>
    public CraftCompletePresenter (
        CraftCompleteModel completeModel,
        BusinessDayModel businessDayModel,
        CraftReviewViewBuilder reviewViewBuilder,
        CraftView craftView, CraftCompleteView completeView,
        Func<CustomerOrder> getSelectedOrder )
    {
        _completeModel = completeModel;
        _businessDayModel = businessDayModel;
        _reviewViewBuilder = reviewViewBuilder;
        _craftView = craftView;
        _completeView = completeView;
        _getSelectedOrder = getSelectedOrder;
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 제작 확정 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        //제작 완료 이벤트 연결
        _craftView.OnCraftDone += CompleteCraft;
        //배송 완료 이벤트 연결
        _completeView.OnDeliverySelected += OpenCompletedDelivery;
        //이벤트 연결 성공 여부
        _isSubscribed = true;
    }

    /// <summary>
    /// 제작 확정 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _craftView.OnCraftDone -= CompleteCraft;
        _completeView.OnDeliverySelected -= OpenCompletedDelivery;
        _isSubscribed = false;
    }
    #endregion

    #region ----- 제작 확정 -----
    /// <summary>
    /// 제작 완료 표시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _completeView.Hide( );
        _completedOrderId = null;
        _isCompletingCraft = false;
    }

    /// <summary>
    /// 제작 완료 표시 상태 즉시 초기화
    /// </summary>
    public void ResetInstant ()
    {
        _completeView.HideInstant( );
        _completedOrderId = null;
        _isCompletingCraft = false;
    }

    /// <summary>
    /// 현재 제작물 확정
    /// </summary>
    void CompleteCraft ()
    {
        //선택한 주문 조회
        CustomerOrder completedOrder = _getSelectedOrder( );

        //완료 처리
        _isCompletingCraft = true;
        CraftCompleteResult result =
            _completeModel.Complete( out CraftResult craftResult );
        _isCompletingCraft = false;

        //결과가 성공이 아니라면
        if ( result != CraftCompleteResult.Success )
        {
            _craftView.ShowCraftFailure(
                GetCompleteFailureText( result ) );
            return;
        }

        //제작 확정 성공 건만 오늘 제작 완료 수에 반영
        _businessDayModel.AddCraftCompletion( );

        //완료 주문 아이디 설정
        _completedOrderId = craftResult.OrderId;
        OnCraftSucceeded?.Invoke( );

        //제작 결과 보여주기
        _completeView.Show(
            _reviewViewBuilder.Create( completedOrder, craftResult.ReviewData ) );
    }

    /// <summary>
    /// 완료한 주문의 배송 화면 이동
    /// </summary>
    void OpenCompletedDelivery ()
    {
        //완료 주문 아이디 확인
        if ( string.IsNullOrWhiteSpace( _completedOrderId ) ) return;

        //돌아올 수 있도록 제작 완료 패널과 주문 아이디 유지
        OnMoveToDelivery?.Invoke( _completedOrderId );
    }

    /// <summary>
    /// 제작 확정 실패 안내 문구 반환
    /// </summary>
    string GetCompleteFailureText ( CraftCompleteResult result )
    {
        switch ( result )
        {
            case CraftCompleteResult.InvalidOrder:
                return "제작할 수 없는 주문입니다.";
            case CraftCompleteResult.InvalidParts:
                return "배치된 파츠 정보를 확인해 주세요.";
            case CraftCompleteResult.MissingMinimumParts:
                return "몸통, 눈, 코, 입을 각각 한 개 이상 배치해 주세요.";
            case CraftCompleteResult.CostExceeded:
                return "최대 제작 코스트를 초과했습니다.";
            case CraftCompleteResult.PartCountExceeded:
                return "최대 파츠 개수를 초과했습니다.";
            case CraftCompleteResult.ExcludedThemeUsed:
                return "제외 테마 파츠가 포함되어 있습니다.";
            case CraftCompleteResult.NotEnoughParts:
                return "보유한 파츠 수량이 부족합니다.";
            case CraftCompleteResult.AlreadyCompleted:
                return "이미 제작을 완료한 주문입니다.";
            case CraftCompleteResult.InventoryUpdateFailed:
                return "파츠 소비에 실패했습니다. 인벤토리를 확인해 주세요.";
            case CraftCompleteResult.InventoryRollbackFailed:
                return "소비한 파츠 복구에 실패했습니다. 인벤토리 수량을 확인해 주세요.";
            case CraftCompleteResult.OrderUpdateFailed:
                return "주문 상태 변경에 실패했습니다.";
            default:
                return "제작을 완료하지 못했습니다.";
        }
    }
    #endregion
}
