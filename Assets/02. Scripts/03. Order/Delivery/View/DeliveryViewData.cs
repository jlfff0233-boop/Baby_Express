using System.Collections.Generic;

/// <summary>
/// 배송 예상과 완료 표시 데이터
/// </summary>
public class DeliveryViewData
{
    /// <summary>
    /// 주문 제목
    /// </summary>
    public string OrderTitle { get; set; }
    /// <summary>
    /// 제작 코스트 문구
    /// </summary>
    public string CraftCost { get; set; }
    /// <summary>
    /// 특수 주문 문구
    /// </summary>
    public string SpecialOrder { get; set; }
    /// <summary>
    /// 특수 조건 문구
    /// </summary>
    public string SpecialCondition { get; set; }
    /// <summary>
    /// 제작 점수 문구
    /// </summary>
    public string CraftScoreText { get; set; }
    /// <summary>
    /// 예상 또는 확정 등급 문구
    /// </summary>
    public string ExpectedGradeText { get; set; }
    /// <summary>
    /// 배송 결과 문구
    /// </summary>
    public string DeliveryResultText { get; set; }
    /// <summary>
    /// 주요 요구 달성 수
    /// </summary>
    public int RequirementCompletedCount { get; set; }
    /// <summary>
    /// 주요 요구 전체 수
    /// </summary>
    public int RequirementTotalCount { get; set; }
    /// <summary>
    /// 희망 사항 달성 수
    /// </summary>
    public int WishCompletedCount { get; set; }
    /// <summary>
    /// 희망 사항 전체 수
    /// </summary>
    public int WishTotalCount { get; set; }
    /// <summary>
    /// 활성 테마 슬롯 표시 데이터
    /// </summary>
    public IReadOnlyList<OrderInfoSlotViewData> ActiveThemes { get; set; }
    /// <summary>
    /// 기본 보상 문구
    /// </summary>
    public string BaseRewardText { get; set; }
    /// <summary>
    /// 등급 보정 문구
    /// </summary>
    public string GradeCorrectionText { get; set; }
    /// <summary>
    /// 배송 보정 문구
    /// </summary>
    public string DeliveryCorrectionText { get; set; }
    /// <summary>
    /// 특수 보정 문구
    /// </summary>
    public string SpecialCorrectionText { get; set; }
    /// <summary>
    /// 예상 또는 최종 보상 문구
    /// </summary>
    public string ExpectedRewardText { get; set; }
    /// <summary>
    /// 배송 상태 제목
    /// </summary>
    public string DeliveryTitle { get; set; }
    /// <summary>
    /// 배송 정보 문구
    /// </summary>
    public string DeliveryInfo { get; set; }
    /// <summary>
    /// 제작 평가 요약 문구
    /// </summary>
    public string CraftResultInfo { get; set; }
    /// <summary>
    /// 주문 대금 내역 문구
    /// </summary>
    public string RewardInfo { get; set; }
}
