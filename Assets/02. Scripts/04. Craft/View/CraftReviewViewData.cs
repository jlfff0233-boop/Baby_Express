using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 조건 달성 상태
/// </summary>
public enum CraftConditionState
{
    Completed,      //달성
    Partial,        //일부 달성
    Failed,         //미달성
}

/// <summary>
/// 제작 판정 문구 상태
/// </summary>
public enum CraftReviewTextState
{
    Normal,         //일반
    Positive,       //점수 추가
    Partial,        //일부 달성
    Negative,       //점수 감소
}

/// <summary>
/// 제작 조건 표시 데이터
/// </summary>
public class CraftConditionViewData
{
    /// <summary>
    /// 파츠 아이콘
    /// </summary>
    public Sprite Icon { get; }
    /// <summary>
    /// 파츠 이름
    /// </summary>
    public string PartName { get; }
    /// <summary>
    /// 사용 수량
    /// </summary>
    public int UsedQuantity { get; }
    /// <summary>
    /// 조건 수량
    /// </summary>
    public int RequiredQuantity { get; }
    /// <summary>
    /// 최소 수량 조건 여부
    /// </summary>
    public bool IsMinimum { get; }
    /// <summary>
    /// 변경 점수
    /// </summary>
    public float ScoreChanged { get; }
    /// <summary>
    /// 달성 상태
    /// </summary>
    public CraftConditionState State { get; }

    /// <summary>
    /// 제작 조건 표시 데이터 생성
    /// </summary>
    public CraftConditionViewData (
        Sprite icon, string partName,
        int usedQuantity, int requiredQuantity,
        bool isMinimum, float scoreChanged, CraftConditionState state )
    {
        Icon = icon;
        PartName = partName;
        UsedQuantity = usedQuantity;
        RequiredQuantity = requiredQuantity;
        IsMinimum = isMinimum;
        ScoreChanged = scoreChanged;
        State = state;
    }
}

/// <summary>
/// 제작 판정 문구 표시 데이터
/// </summary>
public class CraftReviewTextData
{
    /// <summary>
    /// 표시 문구
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// 문구 상태
    /// </summary>
    public CraftReviewTextState State { get; }

    /// <summary>
    /// 제작 판정 문구 표시 데이터 생성
    /// </summary>
    public CraftReviewTextData ( string text, CraftReviewTextState state )
    {
        Text = text;
        State = state;
    }
}

/// <summary>
/// 제작 판정 UI 표시 데이터
/// </summary>
public class CraftReviewViewData
{
    /// <summary>
    /// 주요 요구 결과
    /// </summary>
    public List<CraftConditionViewData> RequirementResults { get; set; }
    /// <summary>
    /// 주요 요구 슬롯 표시 데이터
    /// </summary>
    public List<OrderInfoSlotViewData> RequirementSlots { get; set; }
    /// <summary>
    /// 희망 결과
    /// </summary>
    public List<CraftConditionViewData> WishResults { get; set; }
    /// <summary>
    /// 희망 사항 슬롯 표시 데이터
    /// </summary>
    public List<OrderInfoSlotViewData> WishSlots { get; set; }
    /// <summary>
    /// 특수 조건 결과
    /// </summary>
    public List<CraftReviewTextData> SpecialResults { get; set; }
    /// <summary>
    /// 특수 조건 슬롯 표시 데이터
    /// </summary>
    public List<OrderInfoSlotViewData> SpecialSlots { get; set; }
    /// <summary>
    /// 테마 결과
    /// </summary>
    public List<CraftReviewTextData> ThemeResults { get; set; }
    /// <summary>
    /// 테마 슬롯 표시 데이터
    /// </summary>
    public List<CraftThemeSlotViewData> ThemeSlots { get; set; }
    /// <summary>
    /// 점수 보정 결과
    /// </summary>
    public List<CraftReviewTextData> ScoreResults { get; set; }
    /// <summary>
    /// 예상 또는 최종 제작 점수
    /// </summary>
    public int FinalScore { get; set; }
}
