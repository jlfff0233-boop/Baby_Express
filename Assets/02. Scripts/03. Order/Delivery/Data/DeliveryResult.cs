/// <summary>
/// 배송 예상과 확정 결과
/// </summary>
public class DeliveryResult
{
    /// <summary>
    /// 배송 방식 확정 여부
    /// </summary>
    public bool IsMethodConfirmed { get; set; }

    /// <summary>
    /// 배송한 주문 아이디
    /// </summary>
    public string OrderId { get; set; }

    /// <summary>
    /// 배송 출발일(누적 영업일 기준)
    /// </summary>
    public int DepartureTotalDay { get; set; }

    /// <summary>
    /// 예상 또는 실제 도착일(누적 영업일 기준)
    /// </summary>
    public int ArrivalTotalDay { get; set; }

    /// <summary>
    /// 정상 또는 지연 배송 결과
    /// </summary>
    public OrderOutcome Outcome { get; set; }

    /// <summary>
    /// 확정 제작 점수
    /// </summary>
    public float CraftScore { get; set; }

    /// <summary>
    /// 주문 기준점수
    /// </summary>
    public float StandardScore { get; set; }

    /// <summary>
    /// 주문 기준점수 달성률
    /// </summary>
    public float AchievementRate { get; set; }

    /// <summary>
    /// 배송 평가 등급
    /// </summary>
    public DeliveryGrade Grade { get; set; }

    /// <summary>
    /// 기본 주문 대금
    /// </summary>
    public float BaseOrderReward { get; set; }

    /// <summary>
    /// 사용한 모든 파츠의 기본 가격 합계
    /// </summary>
    public float BasePriceTotal { get; set; }

    /// <summary>
    /// 주요 요구 미달성으로 적용된 주문 대금 상한
    /// </summary>
    public int RequirementRewardLimit { get; set; }

    /// <summary>
    /// 주문 난이도 보정 배율
    /// </summary>
    public float DifficultyRate { get; set; }

    /// <summary>
    /// 배송 등급 보정 배율
    /// </summary>
    public float GradeRate { get; set; }

    /// <summary>
    /// VIP 주문 보정 배율
    /// </summary>
    public float VipRate { get; set; }

    /// <summary>
    /// 배송 결과 보정 배율
    /// </summary>
    public float DeliveryRate { get; set; }

    /// <summary>
    /// 배율 적용 후 최종 주문 대금
    /// </summary>
    public int FinalOrderReward { get; set; }

    /// <summary>
    /// 확정된 배송 방식
    /// </summary>
    public DeliveryMethod Method { get; set; }

    /// <summary>
    /// 배송 담당 직원 아이디
    /// </summary>
    public string EmployeeId { get; set; }

    /// <summary>
    /// 출발 시 지불한 배송비
    /// </summary>
    public float DeliveryCost { get; set; }
}
