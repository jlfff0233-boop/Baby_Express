/// <summary>
/// 플레이 씬 저장 복구 처리기 - 저장 데이터 검증과 전체 시스템 상태 복구
/// </summary>
public class PlaySceneSaveRestorer
{
    PlayStateModel _playStateModel;       //공용 플레이 상태 모델
    BusinessDayModel _businessDayModel;       //영업일 모델
    AchvModel _achvModel;       //업적 모델
    InventoryModel _inventoryModel;       //인벤토리 모델
    ShopModel _shopModel;       //상점 모델
    PurchasableDataMap _purchasableDataMap;       //구매 가능 상품 데이터 맵
    CustomerOrderModel _orderModel;       //고객 주문 모델
    OrderGeneratorModel _orderGeneratorModel;       //주문 생성 모델
    CraftCompleteModel _craftCompleteModel;       //확정 제작 결과 모델
    DeliveryModel _deliveryModel;       //배송 일정과 결과 모델
    DailyRecordModel _dailyRecordModel;       //일일 기록 모델
    SettlementModel _settlementModel;       //결산 모델
    MaintenanceModel _maintenanceModel;       //정비 모델
    EmployeeModel _employeeModel;       //직원 모델
    TutorialModel _tutorialModel;       //튜토리얼 진행 모델

    PlaySceneSaveValidator _saveValidator =
        new PlaySceneSaveValidator( );       //시스템 간 저장 참조 검증
    CustomerOrderSaveRestorer _orderSaveRestorer =
        new CustomerOrderSaveRestorer( );       //고객 주문 저장 복구 처리
    CraftCompleteSaveRestorer _craftCompleteSaveRestorer =
        new CraftCompleteSaveRestorer( );       //확정 제작 결과 저장 복구 처리
    EmployeeSaveRestorer _employeeSaveRestorer =
        new EmployeeSaveRestorer( );       //직원 저장 복구 처리
    DeliverySaveRestorer _deliverySaveRestorer =
        new DeliverySaveRestorer( );       //배송 저장 복구 처리
    DailyRecordSaveRestorer _dailyRecordSaveRestorer =
        new DailyRecordSaveRestorer( );       //일일 기록 저장 복구 처리
    SettlementSaveRestorer _settlementSaveRestorer =
        new SettlementSaveRestorer( );       //주간 결산 저장 복구 처리

    /// <summary>
    /// 플레이 씬 저장 복구 처리기 생성
    /// </summary>
    public PlaySceneSaveRestorer (
        PlayStateModel playStateModel,
        BusinessDayModel businessDayModel,
        AchvModel achvModel,
        InventoryModel inventoryModel,
        ShopModel shopModel,
        PurchasableDataMap purchasableDataMap,
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
        _purchasableDataMap = purchasableDataMap;
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
    /// 전체 저장 데이터를 검증하고 플레이 씬 복구 상태 생성
    /// </summary>
    /// <param name="saveFileData">복구할 저장 파일 데이터</param>
    /// <param name="restoreState">생성한 플레이 씬 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        SaveFileData saveFileData,
        out PlaySceneRestoreState restoreState )
    {
        restoreState = null;

        if ( ValidateRestoreStep(
            saveFileData != null, "저장 파일" ) == false )
        {
            return false;
        }

        //주문 원본을 먼저 복구 상태로 변환해 제작과 배송 검증에서 함께 사용
        if ( ValidateRestoreStep(
            _orderSaveRestorer.TryCreate(
                saveFileData.OrderData,
                _purchasableDataMap,
                out CustomerOrderRestoreState orderState ),
            "주문" ) == false )
        {
            return false;
        }

        //상품 원본을 공유하는 상점과 인벤토리 상태 검증
        if ( ValidateRestoreStep(
            _shopModel.CanRestore(
                saveFileData.ShopData ),
            "상점" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _inventoryModel.CanRestore(
                saveFileData.InventoryData,
                _purchasableDataMap ),
            "인벤토리" ) == false )
        {
            return false;
        }

        //주문 생성 상태와 주문을 참조하는 확정 제작 결과 검증
        if ( ValidateRestoreStep(
            _orderGeneratorModel.CanRestore(
                saveFileData.OrderGeneratorData ),
            "주문 생성" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _craftCompleteSaveRestorer.TryCreate(
                saveFileData.CraftData,
                saveFileData.OrderData,
                _purchasableDataMap,
                out CraftCompleteRestoreState craftCompleteState ),
            "제작 결과" ) == false )
        {
            return false;
        }

        //직원 상태를 먼저 만든 뒤 현재 배송 배치와 결과 검증
        if ( ValidateRestoreStep(
            _employeeSaveRestorer.TryCreate(
                saveFileData.EmployeeData,
                saveFileData.Data.TotalDay,
                _employeeModel,
                out EmployeeRestoreState employeeState ),
            "직원" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _deliverySaveRestorer.TryCreate(
                saveFileData.DeliveryData,
                saveFileData.OrderData,
                employeeState,
                out DeliveryRestoreState deliveryState ),
            "배송" ) == false )
        {
            return false;
        }

        //현재 영업일 기록을 먼저 만든 뒤 누적 주간 결산 검증
        if ( ValidateRestoreStep(
            _dailyRecordSaveRestorer.TryCreate(
                saveFileData.DailyRecordData,
                saveFileData.Data.TotalDay,
                _purchasableDataMap,
                out DailyRecordRestoreState dailyRecordState ),
            "일일 기록" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _settlementSaveRestorer.TryCreate(
                saveFileData.SettlementData,
                saveFileData.Data.TotalDay,
                _purchasableDataMap,
                out SettlementRestoreState settlementState ),
            "주간 결산" ) == false )
        {
            return false;
        }

        //독립 진행 상태와 시스템 사이의 최종 참조 관계 검증
        if ( ValidateRestoreStep(
            _maintenanceModel.CanRestore(
                saveFileData.MaintenanceData ),
            "정비" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _achvModel.CanRestore(
                saveFileData.AchvData ),
            "업적" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _tutorialModel.CanRestore(
                saveFileData.TutorialData ),
            "튜토리얼" ) == false )
        {
            return false;
        }

        if ( ValidateRestoreStep(
            _saveValidator.CanRestore( saveFileData ),
            "시스템 간 참조" ) == false )
        {
            return false;
        }

        restoreState = new PlaySceneRestoreState(
            orderState,
            craftCompleteState,
            employeeState,
            deliveryState,
            dailyRecordState,
            settlementState );
        return true;
    }

    /// <summary>
    /// 저장 복구 단계의 검증 결과를 확인하고 실패 지점을 기록
    /// </summary>
    /// <param name="isValid">현재 단계 검증 결과</param>
    /// <param name="step">현재 검증 단계</param>
    /// <returns>전달받은 검증 결과</returns>
    bool ValidateRestoreStep ( bool isValid, string step )
    {
        if ( isValid == false )
        {
            UnityEngine.Debug.LogWarning(
                $"저장 복구 심층 검증 실패: {step}" );
        }

        return isValid;
    }

    /// <summary>
    /// 검증을 마친 저장 상태를 전체 시스템에 복구
    /// </summary>
    /// <param name="saveFileData">복구할 저장 파일 데이터</param>
    /// <param name="restoreState">검증을 마친 플레이 씬 복구 상태</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore (
        SaveFileData saveFileData,
        PlaySceneRestoreState restoreState )
    {
        //다른 시스템이 참조하는 날짜를 먼저 복구
        _playStateModel.Restore( saveFileData.Data );

        //상품 가격과 재고를 복구한 뒤 이를 참조하는 인벤토리 복구
        if ( _shopModel.Restore( saveFileData.ShopData ) == false )
        {
            return false;
        }

        if ( _inventoryModel.Restore(
            saveFileData.InventoryData, _purchasableDataMap ) == false )
        {
            return false;
        }

        //주문을 먼저 복구해 이후 제작 결과와 배송 일정의 원본으로 사용
        _orderModel.Restore( restoreState.CustomerOrderState );

        if ( _orderGeneratorModel.Restore(
            saveFileData.OrderGeneratorData ) == false )
        {
            return false;
        }

        //주문을 참조하는 제작 결과와 직원 배치를 참조하는 배송 상태 복구
        _craftCompleteModel.Restore( restoreState.CraftCompleteState );
        _employeeModel.Restore( restoreState.EmployeeState );

        //직원 효과를 조회할 수 있게 된 뒤 영업일 제작 할당량 복구
        _businessDayModel.Restore( saveFileData.BusinessDayData );

        _deliveryModel.Restore( restoreState.DeliveryState );

        //현재와 과거 기록을 복구한 뒤 결산 조회 상태 복구
        _dailyRecordModel.Restore( restoreState.DailyRecordState );
        _settlementModel.Restore( restoreState.SettlementState );

        //다른 시스템 복구가 끝난 뒤 독립 진행 상태 복구
        if ( _maintenanceModel.Restore( saveFileData.MaintenanceData ) == false )
        {
            return false;
        }

        if ( _achvModel.Restore( saveFileData.AchvData ) == false )
        {
            return false;
        }

        if ( _tutorialModel.Restore( saveFileData.TutorialData ) == false )
        {
            return false;
        }

        return true;
    }
}
