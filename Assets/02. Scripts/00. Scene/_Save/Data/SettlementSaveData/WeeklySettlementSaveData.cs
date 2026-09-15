using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주간 결산 세이브 데이터
/// </summary>
[Serializable]
public class WeeklySettlementSaveData
{
    #region ----- 기본 정보 -----

    [SerializeField] int _week;       //결산 주차
    [SerializeField] int _startTotalDay;       //주간 시작 누적 영업일
    [SerializeField] int _endTotalDay;       //주간 종료 누적 영업일
    [SerializeField] float _startBudget;       //주간 시작 자금
    [SerializeField] float _endBudget;       //주간 종료 자금
    [SerializeField] int _hiredEmployeeCount;       //결산 당시 고용 직원 수

    #endregion

    #region ----- 운영 현황 -----

    [SerializeField] int _productionOrderCount;       //제작 진행 주문 수
    [SerializeField] int _craftCompletedCount;       //제작 완료 수
    [SerializeField] int _craftLimit;       //주간 제작 할당량
    [SerializeField] int _discardedCraftCount;       //취소, 폐기한 제작물 수
    [SerializeField] int _deliveryStartedCount;       //배송 출발 수
    [SerializeField] int _pendingDeliveryCount;       //배송 대기 수
    [SerializeField] int _inDeliveryCount;       //배송 중 수
    [SerializeField] int _quickRestockCount;       //빠른 재입고 사용 횟수
    [SerializeField] int _quickRestockLimit;       //빠른 재입고 최대 횟수

    #endregion

    #region ----- 주문 결과 -----

    [SerializeField] int _normalDeliveryCount;       //정상 배송 수
    [SerializeField] int _lateDeliveryCount;       //지연 배송 수
    [SerializeField] int _rejectedCount;       //직접 거절 수
    [SerializeField] int _autoRejectedCount;       //자동 거절 수
    [SerializeField] int _cancelledCount;       //주문 취소 수
    [SerializeField] int _failedCount;       //최종 실패 수

    #endregion

    #region ----- 주문 평가 -----

    [SerializeField] int _completedRequirementCount;       //달성한 주요 요구 수
    [SerializeField] int _requirementCount;       //전체 주요 요구 수
    [SerializeField] int _completedWishCount;       //달성한 희망 사항 수
    [SerializeField] int _wishCount;       //전체 희망 사항 수
    [SerializeField] int _sGradeCount;       //S등급 수
    [SerializeField] int _aGradeCount;       //A등급 수
    [SerializeField] int _bGradeCount;       //B등급 수
    [SerializeField] int _cGradeCount;       //C등급 수
    [SerializeField] int _dGradeCount;       //D등급 수

    #endregion

    #region ----- 경제 결과 -----

    [SerializeField] float _orderRewardIncome;       //주문 대금 수익
    [SerializeField] float _inventorySaleIncome;       //인벤토리 판매 수익
    [SerializeField] float _otherIncome;       //기타 수익
    [SerializeField] float _purchaseExpense;       //상점 구매 지출
    [SerializeField] float _quickRestockExpense;       //빠른 재입고 이용료
    [SerializeField] float _deliveryExpense;       //직접 배송비
    [SerializeField] float _employeeHireExpense;       //직원 고용비
    [SerializeField] float _employeeWeeklyExpense;       //직원 주급
    [SerializeField] float _otherExpense;       //기타 지출

    #endregion

    #region ----- 주간 평가 -----

    [SerializeField] int _evaluationOrderCount;       //평가 대상 주문 수
    [SerializeField] bool _hasEnoughData;       //평가 자료 충족 여부
    [SerializeField] float _weeklyScore;       //주간 평가 점수
    [SerializeField] WeeklyRating _rating;       //주간 영업 평가

    [SerializeField] bool _isAdjustmentApplied;       //다음 주 보정 적용 여부
    [SerializeField] int _orderCountCorrection;       //일일 주문 수 보정
    [SerializeField] int _easyWeight;       //쉬움 난이도 가중치
    [SerializeField] int _normalWeight;       //보통 난이도 가중치
    [SerializeField] int _hardWeight;       //어려움 난이도 가중치

    #endregion

    #region ----- 상세 기록 -----

    [SerializeField] List<SettlementItemSaveData> _purchaseDetails;       //구매 상세
    [SerializeField] List<SettlementItemSaveData> _usedPartDetails;       //사용 파츠 상세
    [SerializeField] List<SettlementItemSaveData> _quickRestockDetails;       //빠른 재입고 상세

    #endregion

    #region ----- 프로퍼티 -----

    /// <summary>
    /// 결산 주차
    /// </summary>
    public int Week => _week;

    /// <summary>
    /// 주간 시작 누적 영업일
    /// </summary>
    public int StartTotalDay => _startTotalDay;

    /// <summary>
    /// 주간 종료 누적 영업일
    /// </summary>
    public int EndTotalDay => _endTotalDay;

    /// <summary>
    /// 주간 시작 자금
    /// </summary>
    public float StartBudget => _startBudget;

    /// <summary>
    /// 주간 종료 자금
    /// </summary>
    public float EndBudget => _endBudget;

    /// <summary>
    /// 결산 당시 고용 직원 수
    /// </summary>
    public int HiredEmployeeCount => _hiredEmployeeCount;

    /// <summary>
    /// 제작 진행 주문 수
    /// </summary>
    public int ProductionOrderCount => _productionOrderCount;

    /// <summary>
    /// 제작 완료 수
    /// </summary>
    public int CraftCompletedCount => _craftCompletedCount;

    /// <summary>
    /// 주간 제작 할당량
    /// </summary>
    public int CraftLimit => _craftLimit;

    /// <summary>
    /// 취소, 폐기한 제작물 수
    /// </summary>
    public int DiscardedCraftCount => _discardedCraftCount;

    /// <summary>
    /// 배송 출발 수
    /// </summary>
    public int DeliveryStartedCount => _deliveryStartedCount;

    /// <summary>
    /// 배송 대기 수
    /// </summary>
    public int PendingDeliveryCount => _pendingDeliveryCount;

    /// <summary>
    /// 배송 중 수
    /// </summary>
    public int InDeliveryCount => _inDeliveryCount;

    /// <summary>
    /// 빠른 재입고 사용 횟수
    /// </summary>
    public int QuickRestockCount => _quickRestockCount;

    /// <summary>
    /// 빠른 재입고 최대 횟수
    /// </summary>
    public int QuickRestockLimit => _quickRestockLimit;

    /// <summary>
    /// 정상 배송 수
    /// </summary>
    public int NormalDeliveryCount => _normalDeliveryCount;

    /// <summary>
    /// 지연 배송 수
    /// </summary>
    public int LateDeliveryCount => _lateDeliveryCount;

    /// <summary>
    /// 직접 거절 수
    /// </summary>
    public int RejectedCount => _rejectedCount;

    /// <summary>
    /// 자동 거절 수
    /// </summary>
    public int AutoRejectedCount => _autoRejectedCount;

    /// <summary>
    /// 주문 취소 수
    /// </summary>
    public int CancelledCount => _cancelledCount;

    /// <summary>
    /// 최종 실패 수
    /// </summary>
    public int FailedCount => _failedCount;

    /// <summary>
    /// 달성한 주요 요구 수
    /// </summary>
    public int CompletedRequirementCount => _completedRequirementCount;

    /// <summary>
    /// 전체 주요 요구 수
    /// </summary>
    public int RequirementCount => _requirementCount;

    /// <summary>
    /// 달성한 희망 사항 수
    /// </summary>
    public int CompletedWishCount => _completedWishCount;

    /// <summary>
    /// 전체 희망 사항 수
    /// </summary>
    public int WishCount => _wishCount;

    /// <summary>
    /// S등급 수
    /// </summary>
    public int SGradeCount => _sGradeCount;

    /// <summary>
    /// A등급 수
    /// </summary>
    public int AGradeCount => _aGradeCount;

    /// <summary>
    /// B등급 수
    /// </summary>
    public int BGradeCount => _bGradeCount;

    /// <summary>
    /// C등급 수
    /// </summary>
    public int CGradeCount => _cGradeCount;

    /// <summary>
    /// D등급 수
    /// </summary>
    public int DGradeCount => _dGradeCount;

    /// <summary>
    /// 주문 대금 수익
    /// </summary>
    public float OrderRewardIncome => _orderRewardIncome;

    /// <summary>
    /// 인벤토리 판매 수익
    /// </summary>
    public float InventorySaleIncome => _inventorySaleIncome;

    /// <summary>
    /// 기타 수익
    /// </summary>
    public float OtherIncome => _otherIncome;

    /// <summary>
    /// 상점 구매 지출
    /// </summary>
    public float PurchaseExpense => _purchaseExpense;

    /// <summary>
    /// 빠른 재입고 이용료
    /// </summary>
    public float QuickRestockExpense => _quickRestockExpense;

    /// <summary>
    /// 직접 배송비
    /// </summary>
    public float DeliveryExpense => _deliveryExpense;

    /// <summary>
    /// 직원 고용비
    /// </summary>
    public float EmployeeHireExpense => _employeeHireExpense;

    /// <summary>
    /// 직원 주급
    /// </summary>
    public float EmployeeWeeklyExpense => _employeeWeeklyExpense;

    /// <summary>
    /// 기타 지출
    /// </summary>
    public float OtherExpense => _otherExpense;

    /// <summary>
    /// 평가 대상 주문 수
    /// </summary>
    public int EvaluationOrderCount => _evaluationOrderCount;

    /// <summary>
    /// 평가 자료 충족 여부
    /// </summary>
    public bool HasEnoughData => _hasEnoughData;

    /// <summary>
    /// 주간 평가 점수
    /// </summary>
    public float WeeklyScore => _weeklyScore;

    /// <summary>
    /// 주간 영업 평가
    /// </summary>
    public WeeklyRating Rating => _rating;

    /// <summary>
    /// 다음 주 보정 적용 여부
    /// </summary>
    public bool IsAdjustmentApplied => _isAdjustmentApplied;

    /// <summary>
    /// 일일 주문 수 보정
    /// </summary>
    public int OrderCountCorrection => _orderCountCorrection;

    /// <summary>
    /// 쉬움 난이도 가중치
    /// </summary>
    public int EasyWeight => _easyWeight;

    /// <summary>
    /// 보통 난이도 가중치
    /// </summary>
    public int NormalWeight => _normalWeight;

    /// <summary>
    /// 어려움 난이도 가중치
    /// </summary>
    public int HardWeight => _hardWeight;

    /// <summary>
    /// 구매 상세 세이브 데이터
    /// </summary>
    public IReadOnlyList<SettlementItemSaveData> PurchaseDetails =>
        _purchaseDetails;

    /// <summary>
    /// 사용 파츠 상세 세이브 데이터
    /// </summary>
    public IReadOnlyList<SettlementItemSaveData> UsedPartDetails =>
        _usedPartDetails;

    /// <summary>
    /// 빠른 재입고 상세 세이브 데이터
    /// </summary>
    public IReadOnlyList<SettlementItemSaveData> QuickRestockDetails =>
        _quickRestockDetails;

    #endregion

    /// <summary>
    /// 주간 결산 세이브 데이터 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    public WeeklySettlementSaveData ( WeeklySettlementData data )
    {
        SetBasicData( data );
        SetOperationData( data.Operation );
        SetOrderData( data.Orders );
        SetEvaluationData( data.Evaluation );
        SetEconomyData( data.Economy );
        SetWeeklyEvaluationData( data );
        SetItemDetails( data );
    }

    /// <summary>
    /// 주간 결산 기본 정보 설정
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    void SetBasicData ( WeeklySettlementData data )
    {
        _week = data.Week;
        _startTotalDay = data.StartTotalDay;
        _endTotalDay = data.EndTotalDay;
        _startBudget = data.StartBudget;
        _endBudget = data.EndBudget;
        _hiredEmployeeCount = data.HiredEmployeeCount;
    }

    /// <summary>
    /// 주간 운영 현황 설정
    /// </summary>
    /// <param name="data">주간 운영 현황</param>
    void SetOperationData ( OperationSettlement data )
    {
        _productionOrderCount = data.ProductionOrderCount;
        _craftCompletedCount = data.CraftCompletedCount;
        _craftLimit = data.CraftLimit;
        _discardedCraftCount = data.DiscardedCraftCount;
        _deliveryStartedCount = data.DeliveryStartedCount;
        _pendingDeliveryCount = data.PendingDeliveryCount;
        _inDeliveryCount = data.InDeliveryCount;
        _quickRestockCount = data.QuickRestockCount;
        _quickRestockLimit = data.QuickRestockLimit;
    }

    /// <summary>
    /// 주간 주문 결과 설정
    /// </summary>
    /// <param name="data">주간 주문 결과</param>
    void SetOrderData ( OrderSettlement data )
    {
        _normalDeliveryCount = data.NormalDeliveryCount;
        _lateDeliveryCount = data.LateDeliveryCount;
        _rejectedCount = data.RejectedCount;
        _autoRejectedCount = data.AutoRejectedCount;
        _cancelledCount = data.CancelledCount;
        _failedCount = data.FailedCount;
    }

    /// <summary>
    /// 주간 주문 평가 설정
    /// </summary>
    /// <param name="data">주간 주문 평가</param>
    void SetEvaluationData ( EvaluationSettlement data )
    {
        _completedRequirementCount =
            data.CompletedRequirementCount;
        _requirementCount = data.RequirementCount;
        _completedWishCount = data.CompletedWishCount;
        _wishCount = data.WishCount;

        _sGradeCount = data.SGradeCount;
        _aGradeCount = data.AGradeCount;
        _bGradeCount = data.BGradeCount;
        _cGradeCount = data.CGradeCount;
        _dGradeCount = data.DGradeCount;
    }

    /// <summary>
    /// 주간 경제 결과 설정
    /// </summary>
    /// <param name="data">주간 경제 결과</param>
    void SetEconomyData ( EconomySettlement data )
    {
        _orderRewardIncome = data.OrderRewardIncome;
        _inventorySaleIncome = data.InventorySaleIncome;
        _otherIncome = data.OtherIncome;

        _purchaseExpense = data.PurchaseExpense;
        _quickRestockExpense = data.QuickRestockExpense;
        _deliveryExpense = data.DeliveryExpense;
        _employeeHireExpense = data.EmployeeHireExpense;
        _employeeWeeklyExpense = data.EmployeeWeeklyExpense;
        _otherExpense = data.OtherExpense;
    }

    /// <summary>
    /// 주간 평가와 다음 주 보정 설정
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    void SetWeeklyEvaluationData ( WeeklySettlementData data )
    {
        _evaluationOrderCount = data.EvaluationOrderCount;
        _hasEnoughData = data.HasEnoughData;
        _weeklyScore = data.WeeklyScore;
        _rating = data.Rating;

        WeeklyOrderAdjustment adjustment =
            data.NextWeekAdjustment;

        _isAdjustmentApplied = adjustment.IsApplied;
        _orderCountCorrection = adjustment.OrderCountCorrection;
        _easyWeight = adjustment.EasyWeight;
        _normalWeight = adjustment.NormalWeight;
        _hardWeight = adjustment.HardWeight;
    }

    /// <summary>
    /// 주간 상품 상세 세이브 데이터 설정
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    void SetItemDetails ( WeeklySettlementData data )
    {
        _purchaseDetails =
            CreateItemSaveDatas( data.PurchaseDetails );
        _usedPartDetails =
            CreateItemSaveDatas( data.UsedPartDetails );
        _quickRestockDetails =
            CreateItemSaveDatas( data.QuickRestockDetails );
    }

    /// <summary>
    /// 결산 상품 상세 세이브 데이터 목록 생성
    /// </summary>
    /// <param name="details">결산 상품 상세 목록</param>
    /// <returns>결산 상품 상세 세이브 데이터 목록</returns>
    List<SettlementItemSaveData> CreateItemSaveDatas (
        IReadOnlyList<ItemSettlement> details )
    {
        var saveDatas =
            new List<SettlementItemSaveData>( details.Count );

        for ( int i = 0; i < details.Count; i++ )
        {
            saveDatas.Add(
                new SettlementItemSaveData( details [ i ] ) );
        }

        return saveDatas;
    }
}