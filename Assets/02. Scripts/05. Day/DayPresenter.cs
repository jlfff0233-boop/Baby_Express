using System;
using UnityEngine;

/// <summary>
/// 영업일 프레젠터 - 자동 영업 종료와 결산 요청 중재
/// </summary>
public class DayPresenter : MonoBehaviour
{
    BusinessDayModel _businessDayModel;       //영업일 모델
    CustomerOrderModel _orderModel;       //고객 주문 모델
    BankruptcyModel _bankruptcyModel;       //파산 모델

    /// <summary>
    /// 결산 화면 요청 이벤트
    /// </summary>
    public event Action<DayEndReason> OnDayFinished;

    /// <summary>
    /// 파산 화면 요청 이벤트
    /// </summary>
    public event Action OnBankruptcyDetected;


    /// <summary>
    /// 영업일 프레젠터 초기화
    /// </summary>
    /// <param name="businessDayModel">영업일 모델</param>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="bankruptcyModel">파산 모델</param>
    public void Init (
        BusinessDayModel businessDayModel, CustomerOrderModel orderModel,
        BankruptcyModel bankruptcyModel )
    {
        _businessDayModel = businessDayModel;
        _orderModel = orderModel;
        _bankruptcyModel = bankruptcyModel;

        //결산 요청 연결
        _businessDayModel.OnDayFinished += RequestSettlement;
    }

    /// <summary>
    /// 영업일 모델 이벤트 해제
    /// </summary>
    private void OnDestroy ()
    {
        if ( _businessDayModel != null )
            _businessDayModel.OnDayFinished -= RequestSettlement;
    }

    /// <summary>
    /// 현재 상태에서 결산 또는 파산
    /// </summary>
    public void TryStartSettlement ()
    {
        //기존 종료 사유가 없으면 현재 영업 상태 확인
        if ( _businessDayModel.IsEndPending == false )
            CheckAvailableOrders( );

        //종료 대기 상태일 때만 결산 시작
        if ( _businessDayModel.IsEndPending )
            _businessDayModel.StartEnd( );
    }

    /// <summary>
    /// 처리할 주문 소진과 파산 가능성 확인
    /// </summary>
    void CheckAvailableOrders ()
    {
        //수락 가능한 주문이 남아 있으면 영업 유지
        if ( _orderModel.GetOrderCount(
            OrderProgressState.Waiting ) > 0 )
            return;

        //진행 주문이 있으면 최소 조립 가능성 확인
        if ( _orderModel.GetOrderCount(
            OrderProgressState.Production ) > 0 )
        {
            BankruptcyResult result =
                _bankruptcyModel.CheckBankruptcy( );

            //어떤 방법으로도 제작할 수 없으면 파산 요청
            if ( result == BankruptcyResult.Bankrupt )
                OnBankruptcyDetected?.Invoke( );

            return;
        }

        //제작 완료 주문은 이월 가능하므로 종료를 막지 않음
        _businessDayModel.RequestEnd( DayEndReason.NoAvailableOrders );
    }

    /// <summary>
    /// 파산 판정 결과 문구 반환
    /// </summary>
    /// <param name="result">파산 판정 결과</param>
    /// <returns>파산 판정 결과 문구</returns>
    string GetBankruptcyResultText ( BankruptcyResult result )
    {
        switch ( result )
        {
            case BankruptcyResult.NoProductionOrder:
                return "진행 중인 제작 주문 없음";

            case BankruptcyResult.CanCraftWithInventory:
                return "현재 인벤토리로 최소 조립 가능";

            case BankruptcyResult.CanCraftAfterPurchase:
                return "상점 구매 후 최소 조립 가능";

            case BankruptcyResult.CanCraftAfterQuickRestock:
                return "빠른 재입고와 구매 후 최소 조립 가능";

            case BankruptcyResult.Bankrupt:
                return "모든 진행 주문의 최소 조립 불가, 파산";

            default:
                return "확인 불가";
        }
    }

    /// <summary>
    /// 결산 화면 요청 전달
    /// </summary>
    /// <param name="reason">영업 종료 사유</param>
    void RequestSettlement ( DayEndReason reason )
    {
        OnDayFinished?.Invoke( reason );
    }

    #region ----- 확인용 -----

    /// <summary>
    /// 현재 상태의 파산 판정 검증
    /// </summary>
    [ContextMenu( "현재 파산 판정 검증" )]
    void DebugBankruptcy ()
    {
        if ( Application.isPlaying == false )
        {
            Debug.Log( "Play Mode에서 확인해 주세요." );
            return;
        }

        int waitingOrderCount =
            _orderModel.GetOrderCount( OrderProgressState.Waiting );
        int productionOrderCount =
            _orderModel.GetOrderCount( OrderProgressState.Production );
        int remainingRestockCount =
            _bankruptcyModel.RemainingQuickRestockCount;
        BankruptcyResult result =
            _bankruptcyModel.CheckBankruptcy( );

        Debug.Log(
            $"[파산 판정 검증]\n" +
            $"수락 대기 주문: {waitingOrderCount}\n" +
            $"진행 중 주문: {productionOrderCount}\n" +
            $"현재 자금: {_bankruptcyModel.CurrentBudget:N0}G\n" +
            $"남은 빠른 재입고: {remainingRestockCount}회\n" +
            $"판정 결과: {GetBankruptcyResultText( result )}" );

        //실제 파산 결과면 파산 화면 연결도 함께 검증
        if ( waitingOrderCount == 0 &&
            result == BankruptcyResult.Bankrupt )
            OnBankruptcyDetected?.Invoke( );
    }

    #endregion
}
