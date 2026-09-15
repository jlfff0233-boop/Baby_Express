
/// <summary>
/// 결산 화면 표시 데이터
/// </summary>
public class SettlementViewData
{
    /// <summary>
    /// 결산 제목
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 일일 결산 요약
    /// </summary>
    public string DailySummary { get; set; }

    /// <summary>
    /// 주문 결과와 평가
    /// </summary>
    public string OrderResult { get; set; }

    /// <summary>
    /// 경제 요약과 빠른 재입고 내역
    /// </summary>
    public string Economy { get; set; }

    /// <summary>
    /// 상점 구매 상세
    /// </summary>
    public string PurchaseDetail { get; set; }

    /// <summary>
    /// 제작 소비 파츠 상세
    /// </summary>
    public string UsedPartDetail { get; set; }
}