using UnityEngine;

/// <summary>
/// 카탈로그 파츠 슬롯 표시 데이터
/// </summary>
public class CatalogSlotViewData
{
    /// <summary>
    /// 파츠 아이디
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 파츠 아이콘
    /// </summary>
    public Sprite Icon { get; }

    /// <summary>
    /// 표시할 파츠 이름
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 파츠 타입 표시 문자열
    /// </summary>
    public string PartTypeText { get; }

    /// <summary>
    /// 파츠 잠금 여부
    /// </summary>
    public bool IsLocked { get; }

    /// <summary>
    /// 카탈로그 파츠 슬롯 표시 데이터 생성자
    /// </summary>
    /// <param name="id">파츠 아이디</param>
    /// <param name="icon">파츠 아이콘</param>
    /// <param name="displayName">표시할 파츠 이름</param>
    /// <param name="partTypeText">파츠 타입 표시 문자열</param>
    /// <param name="isLocked">파츠 잠금 여부</param>
    public CatalogSlotViewData (
        string id, Sprite icon, string displayName,
        string partTypeText, bool isLocked )
    {
        Id = id;
        Icon = icon;
        DisplayName = displayName;
        PartTypeText = partTypeText;
        IsLocked = isLocked;
    }
}