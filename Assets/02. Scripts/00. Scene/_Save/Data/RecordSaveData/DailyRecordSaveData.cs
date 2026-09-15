using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영업일 원본 기록 세이브 데이터
/// </summary>
[Serializable]
public class DailyRecordSaveData
{
    [SerializeField] int _totalDay;       //누적 영업일
    [SerializeField] DayEndReason _endReason;       //영업 종료 사유
    [SerializeField] float _startBudget;       //영업 시작 자금
    [SerializeField] float _endBudget;       //영업 종료 자금
    [SerializeField] int _craftLimit;       //일일 제작 할당량
    [SerializeField] int _quickRestockLimit;       //빠른 재입고 최대 횟수
    [SerializeField] int _employeeHireCount;       //직원 고용 성공 횟수

    [SerializeField] List<DailyCraftRecordSaveData> _craftRecords;       //제작 기록
    [SerializeField] List<string> _deliveryStartedOrderIds;       //배송 출발 주문
    [SerializeField] List<DailyOrderRecordSaveData> _orderRecords;       //종료 주문 기록
    [SerializeField] List<DailyItemRecordSaveData> _purchaseRecords;       //상점 구매 기록
    [SerializeField] List<float> _inventorySaleIncomes;       //인벤토리 판매 수익
    [SerializeField] List<DailyQuickRestockRecordSaveData> _quickRestockRecords;       //빠른 재입고 기록
    [SerializeField] List<float> _deliveryExpenses;       //직접 배송비
    [SerializeField] List<float> _employeeHireExpenses;       //직원 고용비
    [SerializeField] List<float> _employeeWageExpenses;       //직원 주급
    [SerializeField] List<float> _otherIncomes;       //기타 수입
    [SerializeField] List<float> _otherExpenses;       //기타 지출

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 누적 영업일
    /// </summary>
    public int TotalDay => _totalDay;
    /// <summary>
    /// 영업 종료 사유
    /// </summary>
    public DayEndReason EndReason => _endReason;
    /// <summary>
    /// 시작 자금
    /// </summary>
    public float StartBudget => _startBudget;
    /// <summary>
    /// 자금(영업 종료 기준)
    /// </summary>
    public float EndBudget => _endBudget;
    /// <summary>
    /// 일일 제작 할당량
    /// </summary>
    public int CraftLimit => _craftLimit;
    /// <summary>
    /// 빠른 재입고 제한
    /// </summary>
    public int QuickRestockLimit => _quickRestockLimit;
    /// <summary>
    /// 직원 고용 수
    /// </summary>
    public int EmployeeHireCount => _employeeHireCount;

    /// <summary>
    /// 제작 기록 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<DailyCraftRecordSaveData> CraftRecords => _craftRecords;
    /// <summary>
    /// 배송 출발 주문 아이디 목록
    /// </summary>
    public IReadOnlyList<string> DeliveryStartedOrderIds => _deliveryStartedOrderIds;
    /// <summary>
    /// 주문 기록 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<DailyOrderRecordSaveData> OrderRecords => _orderRecords;
    /// <summary>
    /// 구매 기록 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<DailyItemRecordSaveData> PurchaseRecords => _purchaseRecords;
    /// <summary>
    /// 인벤토리 판매 수익 목록
    /// </summary>
    public IReadOnlyList<float> InventorySaleIncomes => _inventorySaleIncomes;
    /// <summary>
    /// 빠른 재입고 기록 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<DailyQuickRestockRecordSaveData> QuickRestockRecords => _quickRestockRecords;
    /// <summary>
    /// 배송비 목록
    /// </summary>
    public IReadOnlyList<float> DeliveryExpenses => _deliveryExpenses;
    /// <summary>
    /// 직원 고용비 목록
    /// </summary>
    public IReadOnlyList<float> EmployeeHireExpenses => _employeeHireExpenses;
    /// <summary>
    /// 직원 주급 지출 목록
    /// </summary>
    public IReadOnlyList<float> EmployeeWageExpenses => _employeeWageExpenses;
    /// <summary>
    /// 기타 수익 목록
    /// </summary>
    public IReadOnlyList<float> OtherIncomes => _otherIncomes;
    /// <summary>
    /// 기타 지출 목록
    /// </summary>
    public IReadOnlyList<float> OtherExpenses => _otherExpenses;

    #endregion

    /// <summary>
    /// 영업일 원본 기록 세이브 데이터 생성
    /// </summary>
    /// <param name="record">영업일 원본 기록</param>
    public DailyRecordSaveData ( DailyRecord record )
    {
        _totalDay = record.TotalDay;
        _endReason = record.EndReason;
        _startBudget = record.StartBudget;
        _endBudget = record.EndBudget;
        _craftLimit = record.CraftLimit;
        _quickRestockLimit = record.QuickRestockLimit;
        _employeeHireCount = record.EmployeeHireCount;

        _craftRecords = CreateCraftRecordSaveDatas( record.CraftRecords );

        _deliveryStartedOrderIds = new List<string>( record.DeliveryStartedOrderIds );

        _orderRecords = CreateOrderRecordSaveDatas( record.OrderRecords );

        _purchaseRecords = CreateItemRecordSaveDatas( record.PurchaseRecords );

        _inventorySaleIncomes = new List<float>( record.InventorySaleIncomes );

        _quickRestockRecords = CreateQuickRestockSaveDatas( record.QuickRestockRecords );

        _deliveryExpenses = new List<float>( record.DeliveryExpenses );

        _employeeHireExpenses = new List<float>( record.EmployeeHireExpenses );

        _employeeWageExpenses = new List<float>( record.EmployeeWageExpenses );

        _otherIncomes = new List<float>( record.OtherIncomes );

        _otherExpenses = new List<float>( record.OtherExpenses );
    }

    /// <summary>
    /// 제작 기록 세이브 데이터 생성
    /// </summary>
    List<DailyCraftRecordSaveData> CreateCraftRecordSaveDatas (
        IReadOnlyList<DailyCraftRecord> records )
    {
        var saveDatas = new List<DailyCraftRecordSaveData>( records.Count );

        for ( int i = 0; i < records.Count; i++ )
        {
            saveDatas.Add( new DailyCraftRecordSaveData( records [ i ] ) );
        }

        return saveDatas;
    }

    /// <summary>
    /// 종료 주문 기록 세이브 데이터 생성
    /// </summary>
    List<DailyOrderRecordSaveData> CreateOrderRecordSaveDatas (
        IReadOnlyList<DailyOrderRecord> records )
    {
        var saveDatas = new List<DailyOrderRecordSaveData>( records.Count );

        for ( int i = 0; i < records.Count; i++ )
        {
            saveDatas.Add( new DailyOrderRecordSaveData( records [ i ] ) );
        }

        return saveDatas;
    }

    /// <summary>
    /// 상품 기록 세이브 데이터 생성
    /// </summary>
    List<DailyItemRecordSaveData> CreateItemRecordSaveDatas (
        IReadOnlyList<DailyItemRecord> records )
    {
        var saveDatas = new List<DailyItemRecordSaveData>( records.Count );

        for ( int i = 0; i < records.Count; i++ )
        {
            saveDatas.Add( new DailyItemRecordSaveData( records [ i ] ) );
        }

        return saveDatas;
    }

    /// <summary>
    /// 빠른 재입고 기록 세이브 데이터 생성
    /// </summary>
    List<DailyQuickRestockRecordSaveData>
        CreateQuickRestockSaveDatas ( IReadOnlyList<DailyQuickRestockRecord> records )
    {
        var saveDatas = new List<DailyQuickRestockRecordSaveData>( records.Count );

        for ( int i = 0; i < records.Count; i++ )
        {
            saveDatas.Add( new DailyQuickRestockRecordSaveData( records [ i ] ) );
        }

        return saveDatas;
    }
}