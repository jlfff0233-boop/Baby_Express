using System.Collections.Generic;

/// <summary>
/// 주문 상세 뷰 데이터
/// </summary>
public class OrderDetailViewData
{
    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId { get; set; }
    /// <summary>
    /// 주문 제목
    /// </summary>
    public string OrderTitle { get; set; }
    /// <summary>
    /// 최대 제작 코스트
    /// </summary>
    public int MaxCraftCost { get; set; }
    /// <summary>
    /// 생성 월
    /// </summary>
    public int CreatedMonth { get; set; }
    /// <summary>
    /// 생성 일
    /// </summary>
    public int CreatedDay { get; set; }
    /// <summary>
    /// 납품 월
    /// </summary>
    public int DeliveryMonth { get; set; }
    /// <summary>
    /// 납품 일
    /// </summary>
    public int DeliveryDay { get; set; }
    /// <summary>
    /// 남은 기한 문구
    /// </summary>
    public string DeadlineText { get; set; }
    /// <summary>
    /// 기한 임박 여부
    /// </summary>
    public bool IsDeadlineImminent { get; set; }
    /// <summary>
    /// 최종 납품 기한 월
    /// </summary>
    public int FinalDueMonth { get; set; }
    /// <summary>
    /// 최종 납품 기한 일
    /// </summary>
    public int FinalDueDay { get; set; }
    /// <summary>
    /// 최종 납품 기한 표시 여부
    /// </summary>
    public bool ShowFinalDue { get; set; }
    /// <summary>
    /// 주문 상태
    /// </summary>
    public string OrderState { get; set; }
    /// <summary>
    /// 확인 버튼 문구
    /// </summary>
    public string ConfirmText { get; set; }
    /// <summary>
    /// 취소 버튼 문구
    /// </summary>
    public string CancelText { get; set; }
    /// <summary>
    /// 주요 요구 사항 슬롯 데이터
    /// </summary>
    public IReadOnlyList<OrderInfoSlotViewData> Requirements { get; set; }
    /// <summary>
    /// 희망 사항 슬롯 데이터
    /// </summary>
    public IReadOnlyList<OrderInfoSlotViewData> Wishes { get; set; }
    /// <summary>
    /// 특수 조건 문구
    /// </summary>
    public string SpecialConditions { get; set; }
}
