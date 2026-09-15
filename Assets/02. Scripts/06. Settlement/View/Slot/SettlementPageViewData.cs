using System.Collections.Generic;

/// <summary>
/// 결산 구역 종류
/// </summary>
public enum SettlementSectionType
{
    Summary,                //결산 요약
    Operation,              //운영 현황
    OrderResult,            //주문 결과
    Evaluation,             //주문 평가
    Economy,                //경제 요약
    PurchaseDetail,         //구매 상세
    UsedPartDetail,         //사용 파츠 상세
    QuickRestockDetail,     //빠른 재입고 상세
    Employee,               //직원 현황
    NextWeek,               //다음 주 변화
}

/// <summary>
/// 결산 구역 표시 데이터
/// </summary>
public class SettlementSectionViewData
{
    /// <summary>
    /// 슬롯을 표시할 결산 구역
    /// </summary>
    public SettlementSectionType SectionType { get; set; }

    /// <summary>
    /// 구역에 표시할 슬롯 목록
    /// </summary>
    public List<SettlementSlotViewData> Slots { get; set; } =
        new List<SettlementSlotViewData>( );
}

/// <summary>
/// 일일 또는 주간 결산 페이지 표시 데이터
/// </summary>
public class SettlementPageViewData
{
    /// <summary>
    /// 결산 페이지 제목
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 페이지에 표시할 결산 구역 목록
    /// </summary>
    public List<SettlementSectionViewData> Sections { get; set; } =
        new List<SettlementSectionViewData>( );
}