using UnityEngine;

/// <summary>
/// 카탈로그 파츠 상세 표시 데이터
/// </summary>
public class CatalogDetailViewData
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
    /// 파츠 설명
    /// </summary>
    public string DescriptionText { get; }

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public string PartTypeText { get; }

    /// <summary>
    /// 제작 코스트
    /// </summary>
    public string CraftCostText { get; }

    /// <summary>
    /// 파츠 테마
    /// </summary>
    public string ThemeText { get; }

    /// <summary>
    /// 상극 테마
    /// </summary>
    public string ConflictThemeText { get; }

    /// <summary>
    /// 해금 상태 안내
    /// </summary>
    public string UnlockText { get; }

    /// <summary>
    /// 현재 보유 수량
    /// </summary>
    public string QuantityText { get; }

    /// <summary>
    /// 파츠 잠금 여부
    /// </summary>
    public bool IsLocked { get; }

    /// <summary>
    /// 카탈로그 파츠 상세 표시 데이터 생성자
    /// </summary>
    /// <param name="id">파츠 아이디</param>
    /// <param name="icon">파츠 아이콘</param>
    /// <param name="displayName">표시할 파츠 이름</param>
    /// <param name="descriptionText">파츠 설명</param>
    /// <param name="partTypeText">파츠 타입</param>
    /// <param name="craftCostText">제작 코스트</param>
    /// <param name="themeText">파츠 테마</param>
    /// <param name="conflictThemeText">상극 테마</param>
    /// <param name="unlockText">해금 상태 안내</param>
    /// <param name="quantityText">현재 보유 수량</param>
    /// <param name="isLocked">파츠 잠금 여부</param>
    public CatalogDetailViewData (
        string id, Sprite icon, string displayName,
        string descriptionText, string partTypeText,
        string craftCostText, string themeText,
        string conflictThemeText, string unlockText,
        string quantityText, bool isLocked )
    {
        Id = id;
        Icon = icon;
        DisplayName = displayName;
        DescriptionText = descriptionText;
        PartTypeText = partTypeText;
        CraftCostText = craftCostText;
        ThemeText = themeText;
        ConflictThemeText = conflictThemeText;
        UnlockText = unlockText;
        QuantityText = quantityText;
        IsLocked = isLocked;
    }
}
