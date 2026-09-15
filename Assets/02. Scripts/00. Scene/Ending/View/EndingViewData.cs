
/// <summary>
/// 엔딩 총 결산 화면 표시 데이터
/// </summary>
public class EndingViewData
{
    #region ----- 운영 결과 -----

    /// <summary>
    /// 총 영업일
    /// </summary>
    public int TotalBusinessDay { get; set; }

    /// <summary>
    /// 최종 보유 골드
    /// </summary>
    public float FinalBudget { get; set; }

    /// <summary>
    /// 최종 고용 직원 수
    /// </summary>
    public int HiredEmployeeCount { get; set; }

    #endregion

    #region ----- 경제 결과 -----

    /// <summary>
    /// 누적 총수입
    /// </summary>
    public float TotalIncome { get; set; }

    /// <summary>
    /// 누적 총지출
    /// </summary>
    public float TotalExpense { get; set; }

    /// <summary>
    /// 누적 순이익
    /// </summary>
    public float NetProfit => TotalIncome - TotalExpense;

    #endregion

    #region ----- 주문/제작 결과 -----

    /// <summary>
    /// 완료한 주문 수
    /// </summary>
    public int CompletedOrderCount { get; set; }

    /// <summary>
    /// S등급 주문 수
    /// </summary>
    public int SGradeCount { get; set; }

    /// <summary>
    /// A등급 주문 수
    /// </summary>
    public int AGradeCount { get; set; }

    /// <summary>
    /// B등급 주문 수
    /// </summary>
    public int BGradeCount { get; set; }

    /// <summary>
    /// C등급 주문 수
    /// </summary>
    public int CGradeCount { get; set; }

    /// <summary>
    /// D등급 주문 수
    /// </summary>
    public int DGradeCount { get; set; }

    /// <summary>
    /// 제작 완료 수
    /// </summary>
    public int CraftCompletedCount { get; set; }

    /// <summary>
    /// 제작에 사용한 전체 파츠 수
    /// </summary>
    public int UsedPartCount { get; set; }

    /// <summary>
    /// 직원 배송 완료 수
    /// </summary>
    public int EmployeeDeliveryCount { get; set; }

    #endregion

    #region ----- 업적 결과 -----

    /// <summary>
    /// 달성한 업적 수
    /// </summary>
    public int CompletedAchvCount { get; set; }

    /// <summary>
    /// 전체 업적 수
    /// </summary>
    public int TotalAchvCount { get; set; }

    #endregion

    #region ----- 슬롯 표시 데이터 -----

    /// <summary>
    /// 운영 결과 슬롯
    /// </summary>
    public EndingSlotViewData OperationSlot { get; set; }

    /// <summary>
    /// 경제 결과 슬롯
    /// </summary>
    public EndingSlotViewData EconomySlot { get; set; }

    /// <summary>
    /// 주문 평가 슬롯
    /// </summary>
    public EndingSlotViewData OrderSlot { get; set; }

    /// <summary>
    /// 제작과 배송 슬롯
    /// </summary>
    public EndingSlotViewData CraftSlot { get; set; }

    /// <summary>
    /// 업적 결과 슬롯
    /// </summary>
    public EndingSlotViewData AchievementSlot { get; set; }

    #endregion
}
