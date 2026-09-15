using System.Collections.Generic;

/// <summary>
/// 영업일의 제작/주문/거래 원본 기록
/// </summary>
public class DailyRecord
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
    /// 일일 제작 할당량
    /// </summary>
    public int CraftLimit { get; set; }

    /// <summary>
    /// 일일 빠른 재입고 최대 횟수
    /// </summary>
    public int QuickRestockLimit { get; set; }

    /// <summary>
    /// 일일 직원 고용 성공 횟수
    /// </summary>
    public int EmployeeHireCount { get; set; }

    /// <summary>
    /// 일일 제작 완료 기록
    /// </summary>
    public List<DailyCraftRecord> CraftRecords { get; set; } = new List<DailyCraftRecord> ( );

    /// <summary>
    /// 일일 배송 출발 주문 아이디
    /// </summary>
    public List<string> DeliveryStartedOrderIds { get; set; } = new List<string> ( );

    /// <summary>
    /// 일일 종료 주문 기록
    /// </summary>
    public List<DailyOrderRecord> OrderRecords { get; set; } = new List<DailyOrderRecord> ( );

    /// <summary>
    /// 일일 상점 구매 기록
    /// </summary>
    public List<DailyItemRecord> PurchaseRecords { get; set; } = new List<DailyItemRecord> ( );

    /// <summary>
    /// 일일 인벤토리 판매 수익
    /// </summary>
    public List<float> InventorySaleIncomes { get; set; } = new List<float> ( );

    /// <summary>
    /// 일일 빠른 재입고 기록
    /// </summary>
    public List<DailyQuickRestockRecord> QuickRestockRecords { get; set; } =
        new List<DailyQuickRestockRecord> ( );

    /// <summary>
    /// 일일 직접 배송비
    /// </summary>
    public List<float> DeliveryExpenses { get; set; } = new List<float> ( );

    /// <summary>
    /// 일일 직원 고용비
    /// </summary>
    public List<float> EmployeeHireExpenses { get; set; } = new List<float> ( );

    /// <summary>
    /// 일일 직원 주급
    /// </summary>
    public List<float> EmployeeWageExpenses { get; set; } = new List<float> ( );

    /// <summary>
    /// 일일 기타 수입
    /// </summary>
    public List<float> OtherIncomes { get; set; } = new List<float> ( );

    /// <summary>
    /// 일일 기타 지출
    /// </summary>
    public List<float> OtherExpenses { get; set; } = new List<float> ( );
}

/// <summary>
/// 일일 상품 수량과 금액 기록
/// </summary>
public class DailyItemRecord
{
    /// <summary>
    /// 상품 데이터
    /// </summary>
    public PurchasableData Data { get; set; }

    /// <summary>
    /// 처리 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 상품별 총금액
    /// </summary>
    public float PriceTotal { get; set; }
}

/// <summary>
/// 일일 제작 완료 기록
/// </summary>
public class DailyCraftRecord
{
    /// <summary>
    /// 제작 주문 아이디
    /// </summary>
    public string OrderId { get; set; }

    /// <summary>
    /// 제작물 취소, 폐기 여부
    /// </summary>
    public bool IsDiscarded { get; set; }

    /// <summary>
    /// 일반 테마 활성 여부
    /// </summary>
    public bool HasActivatedTheme { get; set; }

    /// <summary>
    /// 완성 테마 달성 여부
    /// </summary>
    public bool HasCompleteTheme { get; set; }

    /// <summary>
    /// 상극 테마 쌍 수
    /// </summary>
    public int ThemeConflictCount { get; set; }

    /// <summary>
    /// 목표 테마 완성 여부
    /// </summary>
    public bool IsTargetThemeCompleted { get; set; }

    /// <summary>
    /// 모든 주요 요구 달성 여부
    /// </summary>
    public bool AreAllRequirementsCompleted { get; set; }

    /// <summary>
    /// 모든 희망 사항 달성 여부
    /// </summary>
    public bool AreAllWishesCompleted { get; set; }

    /// <summary>
    /// 제작에 소비한 파츠 기록
    /// </summary>
    public List<DailyItemRecord> UsedParts { get; set; } = new List<DailyItemRecord> ( );
}

/// <summary>
/// 일일 종료 주문 기록
/// </summary>
public class DailyOrderRecord
{
    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId { get; set; }

    /// <summary>
    /// 주문 종료 결과
    /// </summary>
    public OrderOutcome Outcome { get; set; }

    /// <summary>
    /// 배송 평가 포함 여부
    /// </summary>
    public bool HasEvaluation { get; set; }

    /// <summary>
    /// 배송 평가 등급
    /// </summary>
    public DeliveryGrade Grade { get; set; }

    /// <summary>
    /// 확정 배송 방식
    /// </summary>
    public DeliveryMethod DeliveryMethod { get; set; }

    /// <summary>
    /// 배송 담당 직원 아이디
    /// </summary>
    public string EmployeeId { get; set; }

    /// <summary>
    /// 납품 마감일 당일 정상 납품 여부
    /// </summary>
    public bool IsDeadlineDayNormalDelivery { get; set; }

    /// <summary>
    /// 달성한 주요 요구 수
    /// </summary>
    public int CompletedRequirementCount { get; set; }

    /// <summary>
    /// 전체 주요 요구 수
    /// </summary>
    public int RequirementCount { get; set; }

    /// <summary>
    /// 달성한 희망 수
    /// </summary>
    public int CompletedWishCount { get; set; }

    /// <summary>
    /// 전체 희망 수
    /// </summary>
    public int WishCount { get; set; }

    /// <summary>
    /// 지급된 최종 주문 대금
    /// </summary>
    public int OrderReward { get; set; }
}

/// <summary>
/// 일일 빠른 재입고 기록
/// </summary>
public class DailyQuickRestockRecord
{
    /// <summary>
    /// 재입고한 상품 데이터
    /// </summary>
    public PurchasableData Data { get; set; }

    /// <summary>
    /// 빠른 재입고 이용료
    /// </summary>
    public float Fee { get; set; }
}
