using UnityEngine;

/// <summary>
/// 정비 상세 표시 데이터
/// </summary>
public class MaintenanceDetailViewData
{
    /// <summary>
    /// 정비 아이디
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// 정비 아이콘
    /// </summary>
    public Sprite Icon { get; }
    /// <summary>
    /// 정비 정보(이름, 카테고리, 설명)
    /// </summary>
    public string Info { get; }
    /// <summary>
    /// 현재 적용 효과
    /// </summary>
    public string CurrentEffect { get; }
    /// <summary>
    /// 다음 적용 효과
    /// </summary>
    public string NextEffect { get; }
    /// <summary>
    /// 정비 비용
    /// </summary>
    public string Cost { get; }
    /// <summary>
    /// 선행 조건
    /// </summary>
    public string Requirement { get; }
    /// <summary>
    /// 적용 시점
    /// </summary>
    public string Apply { get; }
    /// <summary>
    /// 정비 버튼 활성 여부
    /// </summary>
    public bool CanUpgrade { get; }

    /// <summary>
    /// 정비 상세 표시 데이터 생성
    /// </summary>
    public MaintenanceDetailViewData (
        string id, Sprite icon, string info,
        string currentEffect, string nextEffect,
        string cost, string requirement, string apply,
        bool canUpgrade )
    {
        Id = id;
        Icon = icon;
        Info = info;
        CurrentEffect = currentEffect;
        NextEffect = nextEffect;
        Cost = cost;
        Requirement = requirement;
        Apply = apply;
        CanUpgrade = canUpgrade;
    }
}