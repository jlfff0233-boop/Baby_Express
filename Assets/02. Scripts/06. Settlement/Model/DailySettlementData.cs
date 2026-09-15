using System.Collections.Generic;

/// <summary>
/// 일일 결산 계산 결과
/// </summary>
public class DailySettlementData
{
    /// <summary>
    /// 결산 누적 영업일
    /// </summary>
    public int TotalDay { get; set; }

    /// <summary>
    /// 영업 종료 사유
    /// </summary>
    public DayEndReason EndReason { get; set; }

    /// <summary>
    /// 영업 시작 자금
    /// </summary>
    public float StartBudget { get; set; }

    /// <summary>
    /// 영업 종료 자금
    /// </summary>
    public float EndBudget { get; set; }

    /// <summary>
    /// 일일 운영 현황
    /// </summary>
    public OperationSettlement Operation { get; set; }

    /// <summary>
    /// 일일 주문 결과
    /// </summary>
    public OrderSettlement Orders { get; set; }

    /// <summary>
    /// 일일 주문 평가
    /// </summary>
    public EvaluationSettlement Evaluation { get; set; }

    /// <summary>
    /// 일일 경제 결과
    /// </summary>
    public EconomySettlement Economy { get; set; }

    /// <summary>
    /// 상품별 구매 상세
    /// </summary>
    public List<ItemSettlement> PurchaseDetails { get; set; } =
        new List<ItemSettlement>( );

    /// <summary>
    /// 파츠별 제작 소비 상세
    /// </summary>
    public List<ItemSettlement> UsedPartDetails { get; set; } =
        new List<ItemSettlement>( );

    /// <summary>
    /// 상품별 빠른 재입고 상세
    /// </summary>
    public List<ItemSettlement> QuickRestockDetails { get; set; } =
        new List<ItemSettlement>( );
}

/// <summary>
/// 결산 상품별 수량과 금액
/// </summary>
public class ItemSettlement
{
    /// <summary>
    /// 상품 데이터
    /// </summary>
    public PurchasableData Data { get; set; }

    /// <summary>
    /// 상품 합산 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 상품 합산 금액
    /// </summary>
    public float PriceTotal { get; set; }
}

/// <summary>
/// 일일 운영 현황
/// </summary>
public class OperationSettlement
{
    /// <summary>
    /// 현재 제작 진행 주문 수
    /// </summary>
    public int ProductionOrderCount { get; set; }

    /// <summary>
    /// 제작 완료 수
    /// </summary>
    public int CraftCompletedCount { get; set; }

    /// <summary>
    /// 일일 제작 할당량
    /// </summary>
    public int CraftLimit { get; set; }

    /// <summary>
    /// 취소, 폐기한 제작물 수
    /// </summary>
    public int DiscardedCraftCount { get; set; }

    /// <summary>
    /// 배송 출발 수
    /// </summary>
    public int DeliveryStartedCount { get; set; }

    /// <summary>
    /// 현재 배송 대기 수
    /// </summary>
    public int PendingDeliveryCount { get; set; }

    /// <summary>
    /// 현재 배송 중 수
    /// </summary>
    public int InDeliveryCount { get; set; }

    /// <summary>
    /// 빠른 재입고 사용 횟수
    /// </summary>
    public int QuickRestockCount { get; set; }

    /// <summary>
    /// 빠른 재입고 최대 횟수
    /// </summary>
    public int QuickRestockLimit { get; set; }
}

/// <summary>
/// 일일 주문 결과
/// </summary>
public class OrderSettlement
{
    /// <summary>
    /// 정상 배송 수
    /// </summary>
    public int NormalDeliveryCount { get; set; }

    /// <summary>
    /// 지연 배송 수
    /// </summary>
    public int LateDeliveryCount { get; set; }

    /// <summary>
    /// 전체 도착, 납품 수
    /// </summary>
    public int DeliveredCount => NormalDeliveryCount + LateDeliveryCount;

    /// <summary>
    /// 직접 거절 수
    /// </summary>
    public int RejectedCount { get; set; }

    /// <summary>
    /// 자동 거절 수
    /// </summary>
    public int AutoRejectedCount { get; set; }

    /// <summary>
    /// 주문 취소 수
    /// </summary>
    public int CancelledCount { get; set; }

    /// <summary>
    /// 최종 실패 수
    /// </summary>
    public int FailedCount { get; set; }
}

/// <summary>
/// 일일 주문 평가 결과
/// </summary>
public class EvaluationSettlement
{
    /// <summary>
    /// 달성한 주요 요구 사항 수
    /// </summary>
    public int CompletedRequirementCount { get; set; }

    /// <summary>
    /// 전체 주요 요구 사항 수
    /// </summary>
    public int RequirementCount { get; set; }

    /// <summary>
    /// 주요 요구 사항 달성률
    /// </summary>
    public float RequirementCompletionRate =>
        RequirementCount > 0
            ? ( float ) CompletedRequirementCount / RequirementCount
            : 0f;

    /// <summary>
    /// 달성한 희망 사항 수
    /// </summary>
    public int CompletedWishCount { get; set; }

    /// <summary>
    /// 전체 희망 사항 수
    /// </summary>
    public int WishCount { get; set; }

    /// <summary>
    /// 희망 사항 달성률
    /// </summary>
    public float WishCompletionRate =>
        WishCount > 0
            ? ( float ) CompletedWishCount / WishCount
            : 0f;

    /// <summary>
    /// S등급 수
    /// </summary>
    public int SGradeCount { get; set; }

    /// <summary>
    /// A등급 수
    /// </summary>
    public int AGradeCount { get; set; }

    /// <summary>
    /// B등급 수
    /// </summary>
    public int BGradeCount { get; set; }

    /// <summary>
    /// C등급 수
    /// </summary>
    public int CGradeCount { get; set; }

    /// <summary>
    /// D등급 수
    /// </summary>
    public int DGradeCount { get; set; }
}

/// <summary>
/// 일일 경제 결과
/// </summary>
public class EconomySettlement
{
    /// <summary>
    /// 주문 대금 수익
    /// </summary>
    public float OrderRewardIncome { get; set; }

    /// <summary>
    /// 인벤토리 판매 수익
    /// </summary>
    public float InventorySaleIncome { get; set; }

    /// <summary>
    /// 직접 배송비
    /// </summary>
    public float DeliveryExpense { get; set; }

    /// <summary>
    /// 직원 고용비
    /// </summary>
    public float EmployeeHireExpense { get; set; }

    /// <summary>
    /// 직원 주급
    /// </summary>
    public float EmployeeWeeklyExpense { get; set; }

    /// <summary>
    /// 기타 수입
    /// </summary>
    public float OtherIncome { get; set; }

    /// <summary>
    /// 상점 구매 지출
    /// </summary>
    public float PurchaseExpense { get; set; }

    /// <summary>
    /// 빠른 재입고 이용료
    /// </summary>
    public float QuickRestockExpense { get; set; }

    /// <summary>
    /// 기타 지출
    /// </summary>
    public float OtherExpense { get; set; }

    /// <summary>
    /// 일일 총수입
    /// </summary>
    public float TotalIncome =>
        OrderRewardIncome + InventorySaleIncome + OtherIncome;

    /// <summary>
    /// 일일 총지출
    /// </summary>
    public float TotalExpense =>
        PurchaseExpense + QuickRestockExpense +
        DeliveryExpense + EmployeeHireExpense + EmployeeWeeklyExpense +
        OtherExpense;

    /// <summary>
    /// 일일 순이익
    /// </summary>
    public float NetProfit => TotalIncome - TotalExpense;
}
