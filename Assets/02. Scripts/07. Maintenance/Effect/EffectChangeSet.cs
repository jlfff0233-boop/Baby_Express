using System.Collections.Generic;

/// <summary>
/// 정비 효과 한 건의 적용값과 변경 전 상태
/// </summary>
public class MaintenanceEffectChangeSet
{
    /// <summary>
    /// 정비 아이디
    /// </summary>
    public string MaintenanceId { get; }

    /// <summary>
    /// 적용할 정비 효과값
    /// </summary>
    internal MaintenanceEffectValues EffectValues { get; }

    /// <summary>
    /// 효과 적용 전 상태
    /// </summary>
    internal PreviousEffectValues PreviousValues { get; }

    /// <summary>
    /// 정비 효과 변경 내용 생성
    /// </summary>
    /// <param name="maintenanceId">정비 아이디</param>
    /// <param name="effectValues">적용할 정비 효과값</param>
    /// <param name="previousValues">효과 적용 전 상태</param>
    internal MaintenanceEffectChangeSet (
        string maintenanceId,
        MaintenanceEffectValues effectValues,
        PreviousEffectValues previousValues )
    {
        MaintenanceId = maintenanceId;
        EffectValues = effectValues;
        PreviousValues = previousValues;
    }
}

/// <summary>
/// 단계별 정비 효과값
/// </summary>
class MaintenanceEffectValues
{
    /// <summary>
    /// 적용할 인벤토리 최대 용량
    /// </summary>
    public int? InventoryCapacity { get; set; }

    /// <summary>
    /// 적용할 파츠 최대 재고 보정값
    /// </summary>
    public int? PartMaxStockBonus { get; set; }

    /// <summary>
    /// 적용할 파츠 재입고 간격
    /// </summary>
    public int? RestockSpan { get; set; }

    /// <summary>
    /// 적용할 수락 대기 주문 한도
    /// </summary>
    public int? OrderWaitingLimit { get; set; }

    /// <summary>
    /// 적용할 다음 영업일 제작 할당량
    /// </summary>
    public int? CraftQuota { get; set; }

    /// <summary>
    /// 적용할 배송 소요일 단축값
    /// </summary>
    public int? DeliverySpanReduction { get; set; }

    /// <summary>
    /// 현재 단계에서 새로 해금할 파츠 아이디
    /// </summary>
    public HashSet<string> PartUnlockIds { get; } =
        new HashSet<string>( System.StringComparer.Ordinal );

    /// <summary>
    /// 현재 단계에서 새로 해금할 정보 표시 아이디
    /// </summary>
    public HashSet<string> InformationUnlockIds { get; } =
        new HashSet<string>( System.StringComparer.Ordinal );

    /// <summary>
    /// 현재 단계에서 점수 보너스를 적용할 성질 테마
    /// </summary>
    public HashSet<PartTheme> ThemeScoreThemes { get; } =
        new HashSet<PartTheme>( );
}

/// <summary>
/// 정비 효과 적용 전 상태
/// </summary>
class PreviousEffectValues
{
    /// <summary>
    /// 이전 인벤토리 최대 용량
    /// </summary>
    public int InventoryCapacity { get; set; }

    /// <summary>
    /// 이전 파츠 최대 재고 보정값
    /// </summary>
    public int PartMaxStockBonus { get; set; }

    /// <summary>
    /// 이전 파츠 재입고 간격
    /// </summary>
    public int RestockSpan { get; set; }

    /// <summary>
    /// 이전 수락 대기 주문 한도
    /// </summary>
    public int OrderWaitingLimit { get; set; }

    /// <summary>
    /// 이전 다음 영업일 제작 할당량
    /// </summary>
    public int CraftQuota { get; set; }

    /// <summary>
    /// 이전 배송 소요일 단축값
    /// </summary>
    public int DeliverySpanReduction { get; set; }

    /// <summary>
    /// 파츠별 이전 해금 상태
    /// </summary>
    public Dictionary<string, bool> PartUnlockStates { get; } =
        new Dictionary<string, bool>( );

    /// <summary>
    /// 정보 표시별 이전 해금 상태
    /// </summary>
    public Dictionary<string, bool> InformationUnlockStates { get; } =
        new Dictionary<string, bool>( );
}
