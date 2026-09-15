using System;
using UnityEngine;

/// <summary>
/// 저장 파일 데이터
/// </summary>
[Serializable]
public class SaveFileData
{
    [SerializeField] SaveData _data;        //저장 데이터
    [SerializeField] BusinessDaySaveData _businessDayData;      //영업일 데이터
    [SerializeField] InventorySaveData _inventoryData;      //인벤토리 데이터
    [SerializeField] ShopSaveData _shopData;        //상점 데이터

    [SerializeField] OrderSaveData _orderData;      //고객 주문 데이터
    [SerializeField] OrderGeneratorSaveData _orderGeneratorData;      //주문 생성 데이터
    [SerializeField] CraftSaveData _craftData;       //확정 제작 결과
    [SerializeField] DeliverySaveData _deliveryData;       //배송 일정과 결과

    [SerializeField] DailyRecordModelSaveData _dailyRecordData;       //일일 기록
    [SerializeField] SettlementSaveData _settlementData;       //과거 주간 결산
    [SerializeField] MaintenanceSaveData _maintenanceData;       //정비 상태
    [SerializeField] EmployeeSaveData _employeeData;       //직원 상태

    [SerializeField] AchvSaveData _achvData;       //업적 상태
    [SerializeField] TutorialSaveData _tutorialData;       //튜토리얼 진행 상태

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 저장 파일 기본 정보
    /// </summary>
    public SaveData Data => _data;

    /// <summary>
    /// 영업일 저장 정보
    /// </summary>
    public BusinessDaySaveData BusinessDayData => _businessDayData;

    /// <summary>
    /// 인벤토리 저장 정보
    /// </summary>
    public InventorySaveData InventoryData => _inventoryData;

    /// <summary>
    /// 상점 저장 정보
    /// </summary>
    public ShopSaveData ShopData => _shopData;

    /// <summary>
    /// 고객 주문 저장 정보
    /// </summary>
    public OrderSaveData OrderData => _orderData;

    /// <summary>
    /// 주문 생성 저장 정보
    /// </summary>
    public OrderGeneratorSaveData OrderGeneratorData => _orderGeneratorData;

    /// <summary>
    /// 확정 제작 결과 저장 정보
    /// </summary>
    public CraftSaveData CraftData => _craftData;

    /// <summary>
    /// 배송 일정과 결과 저장 정보
    /// </summary>
    public DeliverySaveData DeliveryData => _deliveryData;

    /// <summary>
    /// 일일 기록 저장 정보
    /// </summary>
    public DailyRecordModelSaveData DailyRecordData => _dailyRecordData;

    /// <summary>
    /// 과거 주간 결산 저장 정보
    /// </summary>
    public SettlementSaveData SettlementData => _settlementData;

    /// <summary>
    /// 정비 상태 저장 정보
    /// </summary>
    public MaintenanceSaveData MaintenanceData => _maintenanceData;

    /// <summary>
    /// 직원 상태 저장 정보
    /// </summary>
    public EmployeeSaveData EmployeeData => _employeeData;

    /// <summary>
    /// 업적 상태 저장 정보
    /// </summary>
    public AchvSaveData AchvData => _achvData;

    /// <summary>
    /// 튜토리얼 진행 저장 정보
    /// </summary>
    public TutorialSaveData TutorialData => _tutorialData;
    #endregion


    /// <summary>
    /// 저장 파일 데이터 생성
    /// </summary>
    /// <param name="data">저장 파일 기본 정보</param>
    /// <param name="businessDayData">영업일 저장 정보</param>
    /// <param name="inventoryData">인벤토리 저장 정보</param>
    /// <param name="shopData">상점 저장 정보</param>
    /// <param name="orderData">고객 주문 저장 정보</param>
    /// <param name="orderGeneratorData">주문 생성 저장 정보</param>
    /// <param name="craftData">제작 저장 정보</param>
    /// <param name="deliveryData">배송 저장 정보</param>
    /// <param name="dailyRecordData">일일 기록 저장 정보</param>
    /// <param name="settlementData">결산 저장 정보</param>
    /// <param name="maintenanceData">정비 상태 저장 정보</param>
    /// <param name="employeeData">직원 상태 저장 정보</param>
    /// <param name="achvData">업적 저장 정보</param>
    /// <param name="tutorialData">튜토리얼 진행 저장 정보</param>
    public SaveFileData (
        SaveData data, BusinessDaySaveData businessDayData,
        InventorySaveData inventoryData, ShopSaveData shopData,
        OrderSaveData orderData,
        OrderGeneratorSaveData orderGeneratorData,
        CraftSaveData craftData,
        DeliverySaveData deliveryData,
        DailyRecordModelSaveData dailyRecordData,
        SettlementSaveData settlementData,
        MaintenanceSaveData maintenanceData,
        EmployeeSaveData employeeData,
        AchvSaveData achvData,
        TutorialSaveData tutorialData )
    {
        _data = data;
        _businessDayData = businessDayData;
        _inventoryData = inventoryData;
        _shopData = shopData;
        _orderData = orderData;
        _orderGeneratorData = orderGeneratorData;
        _craftData = craftData;
        _deliveryData = deliveryData;
        _dailyRecordData = dailyRecordData;
        _settlementData = settlementData;
        _maintenanceData = maintenanceData;
        _employeeData = employeeData;
        _achvData = achvData;
        _tutorialData = tutorialData;
    }
}
