using System;
using System.Collections.Generic;

/// <summary>
/// 플레이 씬 저장 데이터 생성기 - 현재 시스템 상태를 저장 파일 데이터로 변환
/// </summary>
public class PlaySceneSaveDataBuilder
{
    PlayStateModel _playStateModel;       //공용 플레이 상태 모델
    BusinessDayModel _businessDayModel;       //영업일 모델
    AchvModel _achvModel;       //업적 모델
    InventoryModel _inventoryModel;       //인벤토리 모델
    ShopModel _shopModel;       //상점 모델
    CustomerOrderModel _orderModel;       //고객 주문 모델
    OrderGeneratorModel _orderGeneratorModel;       //주문 생성 모델
    CraftCompleteModel _craftCompleteModel;       //확정 제작 결과 모델
    DeliveryModel _deliveryModel;       //배송 일정과 결과 모델
    DailyRecordModel _dailyRecordModel;       //일일 기록 모델
    SettlementModel _settlementModel;       //결산 모델
    MaintenanceModel _maintenanceModel;       //정비 모델
    EmployeeModel _employeeModel;       //직원 모델
    TutorialModel _tutorialModel;       //튜토리얼 진행 모델

    /// <summary>
    /// 플레이 씬 저장 데이터 생성기 생성
    /// </summary>
    public PlaySceneSaveDataBuilder (
        PlayStateModel playStateModel,
        BusinessDayModel businessDayModel,
        AchvModel achvModel,
        InventoryModel inventoryModel,
        ShopModel shopModel,
        CustomerOrderModel orderModel,
        OrderGeneratorModel orderGeneratorModel,
        CraftCompleteModel craftCompleteModel,
        DeliveryModel deliveryModel,
        DailyRecordModel dailyRecordModel,
        SettlementModel settlementModel,
        MaintenanceModel maintenanceModel,
        EmployeeModel employeeModel,
        TutorialModel tutorialModel )
    {
        _playStateModel = playStateModel;
        _businessDayModel = businessDayModel;
        _achvModel = achvModel;
        _inventoryModel = inventoryModel;
        _shopModel = shopModel;
        _orderModel = orderModel;
        _orderGeneratorModel = orderGeneratorModel;
        _craftCompleteModel = craftCompleteModel;
        _deliveryModel = deliveryModel;
        _dailyRecordModel = dailyRecordModel;
        _settlementModel = settlementModel;
        _maintenanceModel = maintenanceModel;
        _employeeModel = employeeModel;
        _tutorialModel = tutorialModel;
    }

    /// <summary>
    /// 현재 플레이 상태를 저장 파일 데이터로 변환
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    /// <returns>생성한 저장 파일 데이터</returns>
    public SaveFileData Create ( int slotNumber )
    {
        SaveData saveData = new SaveData(
            SaveFileHandler.CurrentVersion,
            slotNumber,
            _businessDayModel.BusinessTime,
            _playStateModel.TotalDay,
            _playStateModel.HighGradeEvaluationCount,
            _achvModel.CompletedCount,
            _playStateModel.Budget,
            _playStateModel.UnlockedIds,
            _playStateModel.NewUnlockedIds,
            DateTime.UtcNow );

        BusinessDaySaveData businessDayData = new BusinessDaySaveData(
            _businessDayModel.CraftCount,
            _businessDayModel.CraftLimit,
            _businessDayModel.BaseCraftLimit,
            _businessDayModel.NextCraftLimit,
            _businessDayModel.EmployeeCraftBonus,
            _businessDayModel.QuickRestockCount,
            _businessDayModel.QuickRestockLimit,
            _businessDayModel.IsEndStarted,
            _businessDayModel.EndReason );

        InventorySaveData inventoryData =
            CreateInventorySaveData( );

        ShopSaveData shopData =
            CreateShopSaveData( );

        OrderSaveData orderData =
            CreateOrderSaveData( );

        OrderGeneratorSaveData orderGeneratorData =
            _orderGeneratorModel.CreateSaveData( );

        CraftSaveData craftData =
            _craftCompleteModel.CreateSaveData( );

        DeliverySaveData deliveryData =
            _deliveryModel.CreateSaveData( );

        DailyRecordModelSaveData dailyRecordData =
            _dailyRecordModel.CreateSaveData( );

        SettlementSaveData settlementData =
            _settlementModel.CreateSaveData( );

        MaintenanceSaveData maintenanceData =
            _maintenanceModel.CreateSaveData( );

        EmployeeSaveData employeeData =
            _employeeModel.CreateSaveData( );

        AchvSaveData achvData =
            _achvModel.CreateSaveData( );

        TutorialSaveData tutorialData =
            _tutorialModel.CreateSaveData( );

        return new SaveFileData(
            saveData, businessDayData,
            inventoryData, shopData, orderData,
            orderGeneratorData, craftData, deliveryData,
            dailyRecordData, settlementData,
            maintenanceData, employeeData, achvData,
            tutorialData );
    }

    /// <summary>
    /// 현재 인벤토리 상태를 저장 데이터로 변환
    /// </summary>
    /// <returns>인벤토리 저장 데이터</returns>
    InventorySaveData CreateInventorySaveData ()
    {
        var items = new List<InventoryItemSaveData>( );

        //현재 보유 아이템을 저장용 수량 데이터로 변환
        foreach ( InventoryItem item in _inventoryModel.Items )
        {
            items.Add(
                new InventoryItemSaveData(
                    item.Data.Id, item.Quantity ) );
        }

        return new InventorySaveData(
            _inventoryModel.Capacity, items );
    }

    /// <summary>
    /// 현재 상점 상태를 저장 데이터로 변환
    /// </summary>
    /// <returns>상점 저장 데이터</returns>
    ShopSaveData CreateShopSaveData ()
    {
        var items = new List<ShopItemSaveData>( );

        //상품별 가격과 재입고 상태를 저장 데이터로 변환
        foreach ( ShopItemModel itemModel in _shopModel.Items )
        {
            ShopItem item = itemModel.Item;

            items.Add( new ShopItemSaveData(
                item.Id,
                item.CurrentPrice,
                item.RemainingStock,
                item.MaxStock,
                item.RestockSpan,
                item.NextRestockDay,
                item.PendingRestockSpan ) );
        }

        return new ShopSaveData(
            _shopModel.PartMaxStockBonus,
            _shopModel.PartRestockSpan,
            items );
    }

    /// <summary>
    /// 현재 고객 주문 상태를 저장 데이터로 변환
    /// </summary>
    /// <returns>고객 주문 모델 저장 데이터</returns>
    OrderSaveData CreateOrderSaveData ()
    {
        var orders = new List<CustomerOrderSaveData>( );

        //진행 상태와 결과를 포함한 전체 주문을 저장 데이터로 변환
        foreach ( CustomerOrder order in _orderModel.Orders )
        {
            orders.Add( new CustomerOrderSaveData(
                order.CreatedNumber,
                order.OrderId,
                order.Title,
                order.MaxCraftCost,

                CreateConditionSaveDatas( order.Requirements ),
                CreateConditionSaveDatas( order.Wishes ),
                order.SpecialType,

                order.MaxPartCount,
                order.TargetThemes,
                order.ExcludedThemes,

                order.CreatedTotalDay,
                order.AcceptDue,
                order.DeliveryDue,
                order.MaxDelay,
                order.ClosedTotalDay,

                order.Difficulty,
                order.ProgressState,
                order.Outcome ) );
        }

        return new OrderSaveData(
            _orderModel.WaitingLimit,
            _orderModel.NextCreatedNumber,
            _orderModel.PenaltyType,
            _orderModel.OrderReduction,
            _orderModel.PenaltyRemainingDays,
            orders );
    }

    /// <summary>
    /// 주문 파츠 조건을 저장 데이터로 변환
    /// </summary>
    /// <param name="conditions">주문 파츠 조건 목록</param>
    /// <returns>주문 파츠 조건 저장 데이터 목록</returns>
    List<OrderPartConditionSaveData> CreateConditionSaveDatas (
        IReadOnlyList<OrderPartCondition> conditions )
    {
        var saveDatas =
            new List<OrderPartConditionSaveData>( conditions.Count );

        for ( int i = 0; i < conditions.Count; i++ )
        {
            OrderPartCondition condition = conditions [ i ];

            saveDatas.Add(
                new OrderPartConditionSaveData(
                    condition.PartId, condition.Quantity ) );
        }

        return saveDatas;
    }
}
