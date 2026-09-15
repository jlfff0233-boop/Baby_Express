using System.Collections.Generic;

/// <summary>
/// 플레이 씬에서 발생한 일일 기록 이벤트 처리
/// </summary>
public class PlaySceneRecordHandler
{
    DailyRecordModel _dailyRecordModel;       //일일 기록 모델

    PurchaseModel _purchaseModel;       //구매 모델
    InventoryItemActionModel _itemActionModel;       //인벤토리 판매 모델
    QuickRestockModel _quickRestockModel;       //빠른 재입고 모델

    CraftCompleteModel _craftCompleteModel;       //제작 확정 모델
    DeliveryModel _deliveryModel;       //배송 모델
    CustomerOrderModel _customerOrderModel;       //고객 주문 모델
    EmployeeModel _employeeModel;       //직원 모델

    /// <summary>
    /// 플레이 씬 일일 기록 이벤트 처리 객체 생성
    /// </summary>
    /// <param name="dailyRecordModel">일일 기록 모델</param>
    /// <param name="purchaseModel">구매 모델</param>
    /// <param name="itemActionModel">인벤토리 판매 모델</param>
    /// <param name="quickRestockModel">빠른 재입고 모델</param>
    /// <param name="craftCompleteModel">제작 확정 모델</param>
    /// <param name="deliveryModel">배송 모델</param>
    /// <param name="customerOrderModel">고객 주문 모델</param>
    /// <param name="employeeModel">직원 모델</param>
    public PlaySceneRecordHandler (
        DailyRecordModel dailyRecordModel, PurchaseModel purchaseModel,
        InventoryItemActionModel itemActionModel, QuickRestockModel quickRestockModel,
        CraftCompleteModel craftCompleteModel, DeliveryModel deliveryModel,
        CustomerOrderModel customerOrderModel, EmployeeModel employeeModel )
    {
        _dailyRecordModel = dailyRecordModel;
        _purchaseModel = purchaseModel;
        _itemActionModel = itemActionModel;
        _quickRestockModel = quickRestockModel;
        _craftCompleteModel = craftCompleteModel;
        _deliveryModel = deliveryModel;
        _customerOrderModel = customerOrderModel;
        _employeeModel = employeeModel;
    }

    #region ----- 이벤트 연결 -----

    /// <summary>
    /// 일일 기록 이벤트 연결
    /// </summary>
    public void ConnectEvents ()
    {
        _purchaseModel.OnPurchased += RecordPurchase;
        _itemActionModel.OnSold += RecordInventorySale;
        _quickRestockModel.OnRestocked += RecordQuickRestock;

        _craftCompleteModel.OnCraftCompleted += RecordCraft;
        _deliveryModel.OnDeliveryStarted += RecordDeliveryStarted;
        _deliveryModel.OnDeliveryCompleted += RecordDeliveryCompleted;
        _deliveryModel.OnDeliveryCostPaid += RecordDeliveryCost;
        _customerOrderModel.OnOrderClosed += RecordClosedOrder;

        _employeeModel.OnHireCostPaid += RecordEmployeeHireCost;
        _employeeModel.OnEmployeeHired += RecordEmployeeHire;
        _employeeModel.OnWeeklyWagePaid += RecordEmployeeWeeklyWage;
    }

    /// <summary>
    /// 일일 기록 이벤트 해제
    /// </summary>
    public void DisconnectEvents ()
    {
        _purchaseModel.OnPurchased -= RecordPurchase;
        _itemActionModel.OnSold -= RecordInventorySale;
        _quickRestockModel.OnRestocked -= RecordQuickRestock;

        _craftCompleteModel.OnCraftCompleted -= RecordCraft;
        _deliveryModel.OnDeliveryStarted -= RecordDeliveryStarted;
        _deliveryModel.OnDeliveryCompleted -= RecordDeliveryCompleted;
        _deliveryModel.OnDeliveryCostPaid -= RecordDeliveryCost;
        _customerOrderModel.OnOrderClosed -= RecordClosedOrder;

        _employeeModel.OnHireCostPaid -= RecordEmployeeHireCost;
        _employeeModel.OnEmployeeHired -= RecordEmployeeHire;
        _employeeModel.OnWeeklyWagePaid -= RecordEmployeeWeeklyWage;
    }

    #endregion

    #region ----- 거래 기록 -----

    /// <summary>
    /// 상점 구매 내역 기록
    /// </summary>
    /// <param name="receipt">구매 완료 상품 내역</param>
    void RecordPurchase (
        IReadOnlyList<PurchaseReceiptItem> receipt )
    {
        for ( int i = 0; i < receipt.Count; i++ )
        {
            PurchaseReceiptItem item = receipt [ i ];

            _dailyRecordModel.RecordPurchase(
                item.Data, item.Quantity, item.PriceTotal );
        }
    }

    /// <summary>
    /// 인벤토리 판매 수익 기록
    /// </summary>
    /// <param name="income">판매 수익</param>
    void RecordInventorySale ( float income )
    {
        _dailyRecordModel.RecordInventorySale( income );
    }

    /// <summary>
    /// 빠른 재입고 이용 내역 기록
    /// </summary>
    /// <param name="data">재입고한 상품 데이터</param>
    /// <param name="fee">빠른 재입고 이용료</param>
    void RecordQuickRestock ( PurchasableData data, float fee )
    {
        _dailyRecordModel.RecordQuickRestock( data, fee );
    }

    #endregion

    #region ----- 제작과 주문 기록 -----

    /// <summary>
    /// 제작 완료 기록
    /// </summary>
    /// <param name="craftResult">확정 제작 결과</param>
    void RecordCraft ( CraftResult craftResult )
    {
        _dailyRecordModel.RecordCraft( craftResult );
    }

    /// <summary>
    /// 배송 출발 기록
    /// </summary>
    /// <param name="orderId">출발한 배송 주문 아이디</param>
    void RecordDeliveryStarted ( string orderId )
    {
        _dailyRecordModel.RecordDeliveryStarted( orderId );
    }

    /// <summary>
    /// 배송 완료 기록
    /// </summary>
    /// <param name="deliveryResult">확정 배송 결과</param>
    void RecordDeliveryCompleted ( DeliveryResult deliveryResult )
    {
        //제작 당시 판정 결과 조회
        if ( _craftCompleteModel.GetResult(
            deliveryResult.OrderId, out CraftResult craftResult ) == false )
        {
            return;
        }

        //납품 마감일 기록에 사용할 주문 조회
        if ( _customerOrderModel.GetOrder(
            deliveryResult.OrderId, out CustomerOrder order ) == false )
        {
            return;
        }

        _dailyRecordModel.RecordDelivery(
            deliveryResult, craftResult.ReviewData, order.DeliveryDue );
    }

    /// <summary>
    /// 배송 이외 주문 종료 기록
    /// </summary>
    /// <param name="orderId">종료 주문 아이디</param>
    /// <param name="outcome">주문 종료 결과</param>
    void RecordClosedOrder ( string orderId, OrderOutcome outcome )
    {
        _dailyRecordModel.RecordOrderResult( orderId, outcome );

        //취소와 최종 실패한 제작물의 폐기 기록
        if ( outcome == OrderOutcome.Cancelled ||
            outcome == OrderOutcome.Failed )
        {
            _dailyRecordModel.RecordCraftDiscard( orderId );
        }

        //최종 기한 실패한 확정 제작 결과 제거
        if ( outcome == OrderOutcome.Failed )
            _craftCompleteModel.DiscardResult( orderId );
    }

    /// <summary>
    /// 직접 배송비 지출 기록
    /// </summary>
    /// <param name="deliveryCost">실제 지출한 배송비</param>
    void RecordDeliveryCost ( float deliveryCost )
    {
        _dailyRecordModel.RecordDeliveryExpense( deliveryCost );
    }

    #endregion

    #region ----- 직원 기록 -----

    /// <summary>
    /// 직원 고용비 지출 기록
    /// </summary>
    /// <param name="hireCost">실제 지출한 고용비</param>
    void RecordEmployeeHireCost ( float hireCost )
    {
        _dailyRecordModel.RecordEmployeeHireExpense( hireCost );
    }

    /// <summary>
    /// 직원 고용 성공 기록
    /// </summary>
    void RecordEmployeeHire ()
    {
        _dailyRecordModel.RecordEmployeeHire( );
    }

    /// <summary>
    /// 직원 주급 지출 기록
    /// </summary>
    /// <param name="weeklyWage">실제 지급한 주급</param>
    void RecordEmployeeWeeklyWage ( float weeklyWage )
    {
        _dailyRecordModel.RecordEmployeeWageExpense( weeklyWage );
    }

    #endregion
}
