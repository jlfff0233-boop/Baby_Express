/// <summary>
/// 주문별 배송 일정
/// </summary>
public class DeliverySchedule
{
    /// <summary>
    /// 배송 주문 아이디
    /// </summary>
    public string OrderId { get; set; }

    /// <summary>
    /// 배송 소요일
    /// </summary>
    public int DeliveryDays { get; set; }

    /// <summary>
    /// 배송 출발일(누적 영업일 기준)
    /// </summary>
    public int DepartureTotalDay { get; set; }

    /// <summary>
    /// 도착 예정일(누적 영업일)
    /// </summary>
    public int ArrivalTotalDay { get; set; }

    /// <summary>
    /// 배송 출발 여부
    /// </summary>
    public bool IsStarted => DepartureTotalDay > 0;

    /// <summary>
    /// 확정된 배송 방식
    /// </summary>
    public DeliveryMethod Method { get; set; }

    /// <summary>
    /// 배치된 직원 아이디
    /// </summary>
    public string EmployeeId { get; set; }

    /// <summary>
    /// 출발 시 지불한 배송비
    /// </summary>
    public float DeliveryCost { get; set; }
}