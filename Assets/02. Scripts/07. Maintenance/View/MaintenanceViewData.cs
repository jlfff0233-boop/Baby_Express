using UnityEngine;

/// <summary>
/// 정비 슬롯 표시 데이터
/// </summary>
public class MaintenanceSlotViewData
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
    /// 정비 이름(단계)
    /// </summary>
    public string Title { get; }
    /// <summary>
    /// 설명, 비용, 조건
    /// </summary>
    public string Info { get; }
    /// <summary>
    /// 잠금 여부
    /// </summary>
    public bool IsLocked { get; }
    /// <summary>
    /// 최대 단계 여부
    /// </summary>
    public bool IsMaxLevel { get; }

    /// <summary>
    /// 정비 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="icon">정비 아이콘</param>
    /// <param name="title">정비 이름(단계)</param>
    /// <param name="info">정비 정보</param>
    /// <param name="isLocked">잠금 여부</param>
    /// <param name="isMaxLevel">최대 단계 여부</param>
    public MaintenanceSlotViewData (
        string id, Sprite icon, string title, string info,
        bool isLocked, bool isMaxLevel )
    {
        Id = id;
        Icon = icon;
        Title = title;
        Info = info;
        IsLocked = isLocked;
        IsMaxLevel = isMaxLevel;
    }
}
