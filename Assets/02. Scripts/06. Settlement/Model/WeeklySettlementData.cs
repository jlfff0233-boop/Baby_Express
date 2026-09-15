using System.Collections.Generic;

/// <summary>
/// 주간 영업 평가
/// </summary>
public enum WeeklyRating
{
    Insufficient,       //자료 부족
    Best,       //최상
    Good,       //우수
    Normal,       //보통
    Caution,       //주의
    Danger,       //위험
}

/// <summary>
/// 주간 결산 계산 결과
/// </summary>
public class WeeklySettlementData
{
    /// <summary>
    /// 결산 주차
    /// </summary>
    public int Week { get; set; }

    /// <summary>
    /// 주간 시작 누적 영업일
    /// </summary>
    public int StartTotalDay { get; set; }

    /// <summary>
    /// 주간 종료 누적 영업일
    /// </summary>
    public int EndTotalDay { get; set; }

    /// <summary>
    /// 주간 시작 자금
    /// </summary>
    public float StartBudget { get; set; }

    /// <summary>
    /// 주간 종료 자금
    /// </summary>
    public float EndBudget { get; set; }

    /// <summary>
    /// 주간 결산 당시 고용 직원 수
    /// </summary>
    public int HiredEmployeeCount { get; set; }

    /// <summary>
    /// 주간 운영 현황
    /// </summary>
    public OperationSettlement Operation { get; set; }

    /// <summary>
    /// 주간 주문 결과
    /// </summary>
    public OrderSettlement Orders { get; set; }

    /// <summary>
    /// 주간 주문 평가 결과
    /// </summary>
    public EvaluationSettlement Evaluation { get; set; }

    /// <summary>
    /// 주간 경제 결과
    /// </summary>
    public EconomySettlement Economy { get; set; }

    /// <summary>
    /// 주간 평가 대상 주문 수
    /// </summary>
    public int EvaluationOrderCount { get; set; }

    /// <summary>
    /// 주간 평가 자료 충족 여부
    /// </summary>
    public bool HasEnoughData { get; set; }

    /// <summary>
    /// 주간 평가 점수
    /// </summary>
    public float WeeklyScore { get; set; }

    /// <summary>
    /// 주간 영업 평가
    /// </summary>
    public WeeklyRating Rating { get; set; }

    /// <summary>
    /// 다음 주 주문 생성 보정
    /// </summary>
    public WeeklyOrderAdjustment NextWeekAdjustment { get; set; }

    /// <summary>
    /// 상품별 주간 구매 상세
    /// </summary>
    public List<ItemSettlement> PurchaseDetails { get; set; } =
        new List<ItemSettlement>( );

    /// <summary>
    /// 파츠별 주간 제작 소비 상세
    /// </summary>
    public List<ItemSettlement> UsedPartDetails { get; set; } =
        new List<ItemSettlement>( );

    /// <summary>
    /// 상품별 주간 빠른 재입고 상세
    /// </summary>
    public List<ItemSettlement> QuickRestockDetails { get; set; } =
        new List<ItemSettlement>( );
}

/// <summary>
/// 다음 주 주문 생성 보정
/// </summary>
public class WeeklyOrderAdjustment
{
    /// <summary>
    /// 주간 보정 적용 여부
    /// </summary>
    public bool IsApplied { get; set; }

    /// <summary>
    /// 일일 주문 수 보정
    /// </summary>
    public int OrderCountCorrection { get; set; }

    /// <summary>
    /// 쉬움 난이도 가중치
    /// </summary>
    public int EasyWeight { get; set; }

    /// <summary>
    /// 보통 난이도 가중치
    /// </summary>
    public int NormalWeight { get; set; }

    /// <summary>
    /// 어려움 난이도 가중치
    /// </summary>
    public int HardWeight { get; set; }
}
