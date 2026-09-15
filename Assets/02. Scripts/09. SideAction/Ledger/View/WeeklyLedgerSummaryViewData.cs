
/// <summary>
/// 가계부 주간 목록 표시 데이터
/// </summary>
public class WeeklyLedgerSummaryViewData
{
    /// <summary>
    /// 결산 주차
    /// </summary>
    public int Week { get; }

    /// <summary>
    /// 선택 연도에서 표시할 주차
    /// </summary>
    public int DisplayWeek { get; }

    /// <summary>
    /// 주간 결산 완료 여부
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// 주간 영업 평가
    /// </summary>
    public string Rating { get; }

    /// <summary>
    /// 주간 총수입
    /// </summary>
    public string TotalIncome { get; }

    /// <summary>
    /// 주간 총지출
    /// </summary>
    public string TotalExpense { get; }

    /// <summary>
    /// 주간 순이익
    /// </summary>
    public string NetProfit { get; }

    /// <summary>
    /// 가계부 주간 목록 표시 데이터 생성자
    /// </summary>
    /// <param name="week">결산 주차</param>
    /// <param name="displayWeek">선택 연도에서 표시할 주차</param>
    /// <param name="isCompleted">주간 결산 완료 여부</param>
    /// <param name="rating">주간 영업 평가</param>
    /// <param name="totalIncome">주간 총수입</param>
    /// <param name="totalExpense">주간 총지출</param>
    /// <param name="netProfit">주간 순이익</param>
    public WeeklyLedgerSummaryViewData (
        int week, int displayWeek, bool isCompleted,
        string rating, string totalIncome,
        string totalExpense, string netProfit )
    {
        Week = week;
        DisplayWeek = displayWeek;
        IsCompleted = isCompleted;
        Rating = rating;
        TotalIncome = totalIncome;
        TotalExpense = totalExpense;
        NetProfit = netProfit;
    }
}
