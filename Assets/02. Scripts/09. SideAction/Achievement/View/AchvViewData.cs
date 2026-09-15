
/// <summary>
/// 업적 슬롯 표시 데이터
/// </summary>
public class AchvViewData
{
    /// <summary>
    /// 업적 아이디
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 업적 분류
    /// </summary>
    public AchvCategory Category { get; }

    /// <summary>
    /// 공용 결산 아이콘 종류
    /// </summary>
    public SettlementIconType IconType { get; }

    /// <summary>
    /// 표시할 업적 이름
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 표시할 업적 설명
    /// </summary>
    public string DescriptionText { get; }

    /// <summary>
    /// 숫자 진행도
    /// </summary>
    public string ProgressText { get; }

    /// <summary>
    /// 보상 안내
    /// </summary>
    public string RewardText { get; }

    /// <summary>
    /// 진행 게이지 비율
    /// </summary>
    public float ProgressRate { get; }

    /// <summary>
    /// 숨김 정보 표시 여부
    /// </summary>
    public bool IsHidden { get; }

    /// <summary>
    /// 보상 수령 가능 여부
    /// </summary>
    public bool CanClaim { get; }

    /// <summary>
    /// 최종 단계 달성 여부
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// 신규 달성 여부
    /// </summary>
    public bool IsNew { get; }

    /// <summary>
    /// 여러 단계로 구성된 업적 여부
    /// </summary>
    public bool IsMultiStage { get; }

    /// <summary>
    /// 업적 슬롯 표시 데이터 생성자
    /// </summary>
    public AchvViewData (
        string id, AchvCategory category,
        SettlementIconType iconType,
        string displayName, string descriptionText,
        string progressText, string rewardText,
        float progressRate, bool isHidden,
        bool canClaim, bool isCompleted, bool isNew,
        bool isMultiStage )
    {
        Id = id;
        Category = category;
        IconType = iconType;
        DisplayName = displayName;
        DescriptionText = descriptionText;
        ProgressText = progressText;
        RewardText = rewardText;
        ProgressRate = progressRate;
        IsHidden = isHidden;
        CanClaim = canClaim;
        IsCompleted = isCompleted;
        IsNew = isNew;
        IsMultiStage = isMultiStage;
    }
}
