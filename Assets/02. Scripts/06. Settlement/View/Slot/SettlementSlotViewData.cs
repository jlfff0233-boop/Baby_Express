using UnityEngine;

/// <summary>
/// 결산 슬롯 아이콘 종류
/// </summary>
public enum SettlementIconType
{
    None,               //직접 아이콘 또는 아이콘 없음
    Business,           //영업 종료
    Budget,             //보유 자금
    Craft,              //제작
    Delivery,           //배송
    Restock,            //빠른 재입고
    Order,              //주문 결과
    GoldTrophy,         //금 트로피
    SilverTrophy,       //은 트로피
    BronzeTrophy,       //동 트로피
    UnratedTrophy,      //미정 평가
    Requirement,        //주요 요구
    Wish,               //희망 사항
    Income,             //수입
    Expense,            //지출
    Profit,             //순이익
    Employee,           //직원
    NextWeek,           //다음 주 변화
}

/// <summary>
/// 결산 슬롯 문구 색상 종류
/// </summary>
public enum SettlementTextTone
{
    Default,        //기본 문구
    Income,         //수입 문구
    Expense,        //지출 문구
}

/// <summary>
/// 결산 슬롯 표시 데이터
/// </summary>
public class SettlementSlotViewData
{
    /// <summary>
    /// 공용 결산 아이콘 종류
    /// </summary>
    public SettlementIconType IconType { get; set; }

    /// <summary>
    /// 상품이나 파츠에서 직접 가져온 아이콘
    /// </summary>
    public Sprite Icon { get; set; }

    /// <summary>
    /// 슬롯 묘사 문구
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 슬롯 문구 색상 종류
    /// </summary>
    public SettlementTextTone TextTone { get; set; }

    /// <summary>
    /// 진행 바 표시 여부
    /// </summary>
    public bool ShowProgress { get; set; }

    /// <summary>
    /// 0부터 1 사이의 진행률
    /// </summary>
    public float Progress { get; set; }
}