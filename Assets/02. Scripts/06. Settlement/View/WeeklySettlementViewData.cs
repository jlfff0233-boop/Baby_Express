
/// <summary>
/// 주간 결산 화면 표시 데이터
/// </summary>
public class WeeklySettlementViewData
{
    /// <summary>
    /// 결산 제목
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 주간 결산 요약
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 주간 운영 현황
    /// </summary>
    public string Operation { get; set; }

    /// <summary>
    /// 주간 주문 결과와 평가
    /// </summary>
    public string OrderResult { get; set; }

    /// <summary>
    /// 주간 경제 요약
    /// </summary>
    public string Economy { get; set; }

    /// <summary>
    /// 주간 직원 현황
    /// </summary>
    public string Employee { get; set; }

    /// <summary>
    /// 구매 상품 상세
    /// </summary>
    public string PurchaseDetail { get; set; }

    /// <summary>
    /// 사용 파츠 상세
    /// </summary>
    public string UsedPartDetail { get; set; }

    /// <summary>
    /// 다음 주 주문 변화
    /// </summary>
    public string NextWeek { get; set; }
}
