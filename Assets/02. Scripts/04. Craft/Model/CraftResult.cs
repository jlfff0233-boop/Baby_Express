using System.Collections.Generic;

#region ----- 제작 판정 처리 결과 -----

/// <summary>
/// 제작 판정 처리 결과
/// </summary>
public enum CraftReviewResult
{
    Success,        //제작 가능
    InvalidOrder,       //잘못된 주문
    InvalidParts,       //잘못된 배치 파츠
    MissingMinimumParts,        //최소 제작 조건 미달성
    CostExceeded,       //최대 제작 코스트 초과
    PartCountExceeded,      //최대 파츠 개수 초과
    ExcludedThemeUsed,      //제외 테마 사용
}

#endregion

#region ----- 제작 조건 결과 -----

/// <summary>
/// 제작 조건 달성 결과
/// </summary>
public class CraftConditionResult
{
    /// <summary>
    /// 조건 파츠 아이디
    /// </summary>
    public string PartId { get; set; }
    /// <summary>
    /// 조건 수량
    /// </summary>
    public int RequiredQuantity { get; set; }
    /// <summary>
    /// 실제 사용 수량
    /// </summary>
    public int UsedQuantity { get; set; }
    /// <summary>
    /// 달성 여부
    /// </summary>
    public bool IsCompleted { get; set; }
    /// <summary>
    /// 변경 점수
    /// </summary>
    public float ScoreChanged { get; set; }
}

#endregion

#region ----- 제작 테마 결과 -----

/// <summary>
/// 제작 테마 달성 결과
/// </summary>
public class CraftThemeResult
{
    /// <summary>
    /// 활성 테마
    /// </summary>
    public PartTheme Theme { get; set; }
    /// <summary>
    /// 테마에 사용된 파츠 타입 수
    /// </summary>
    public int UsedTypeCount { get; set; }
    /// <summary>
    /// 제작에 사용된 전체 파츠 타입 수
    /// </summary>
    public int TotalTypeCount { get; set; }
    /// <summary>
    /// 완성 테마 여부
    /// </summary>
    public bool IsCompleted { get; set; }
    /// <summary>
    /// 목표 테마 여부
    /// </summary>
    public bool IsTargetTheme { get; set; }
    /// <summary>
    /// 기본 테마 점수
    /// </summary>
    public float BaseScore { get; set; }
    /// <summary>
    /// 목표 보너스 점수
    /// </summary>
    public float TargetBonus { get; set; }
    /// <summary>
    /// 현재 적용된 테마 연구 보너스
    /// </summary>
    public float ResearchBonus { get; set; }

    /// <summary>
    /// 목표 테마와 연구 보너스를 포함한 적용 점수
    /// </summary>
    public float AppliedScore => BaseScore + TargetBonus + ResearchBonus;
}

#endregion

#region ----- 목표 테마 결과 -----

/// <summary>
/// 목표 테마 진행 결과
/// </summary>
public class CraftTargetThemeResult
{
    /// <summary>
    /// 목표 테마
    /// </summary>
    public PartTheme Theme { get; set; }
    /// <summary>
    /// 목표 테마 사용 타입 수
    /// </summary>
    public int UsedTypeCount { get; set; }
    /// <summary>
    /// 전체 사용 타입 수
    /// </summary>
    public int TotalTypeCount { get; set; }
    /// <summary>
    /// 목표 테마 활성 여부
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// 목표 테마 완성 여부
    /// </summary>
    public bool IsCompleted { get; set; }
}

#endregion

#region ----- 제외 테마 결과 -----

/// <summary>
/// 제외 테마 사용 결과
/// </summary>
public class CraftExcludedThemeResult
{
    /// <summary>
    /// 제외 테마
    /// </summary>
    public PartTheme Theme { get; set; }
    /// <summary>
    /// 제외 테마 파츠 사용 수
    /// </summary>
    public int UsedPartCount { get; set; }
    /// <summary>
    /// 제외 테마 사용 여부
    /// </summary>
    public bool IsUsed => UsedPartCount > 0;
}

#endregion

#region ----- 제작 점수 결과 -----

/// <summary>
/// 제작 점수 결과
/// </summary>
public class CraftScoreResult
{
    /// <summary>
    /// 조립 완성 점수
    /// </summary>
    public float AssemblyScore { get; set; }
    /// <summary>
    /// 주요 요구 점수
    /// </summary>
    public float RequirementScore { get; set; }
    /// <summary>
    /// 희망 점수
    /// </summary>
    public float WishScore { get; set; }

    /// <summary>
    /// 테마 기본 점수
    /// </summary>
    public float ThemeBaseScore { get; set; }
    /// <summary>
    /// 목표 테마 보정 점수
    /// </summary>
    public float TargetThemeBonus { get; set; }
    /// <summary>
    /// 적용된 테마 연구 보너스 합계
    /// </summary>
    public float ThemeResearchBonus { get; set; }
    /// <summary>
    /// 감쇠 전 테마 점수
    /// </summary>
    public float ThemeSubtotal { get; set; }
    /// <summary>
    /// 복수 테마 적용 비율
    /// </summary>
    public float ThemeRate { get; set; }
    /// <summary>
    /// 복수 테마 감쇠 점수
    /// </summary>
    public float ThemeReduction { get; set; }
    /// <summary>
    /// 적용된 테마 상한
    /// </summary>
    public float ThemeLimit { get; set; }
    /// <summary>
    /// 테마 상한 감소 점수
    /// </summary>
    public float ThemeLimitReduction { get; set; }
    /// <summary>
    /// 상극 테마 쌍 수
    /// </summary>
    public int ConflictCount { get; set; }
    /// <summary>
    /// 상극 테마 감점
    /// </summary>
    public float ConflictPenalty { get; set; }
    /// <summary>
    /// 최종 테마 점수
    /// </summary>
    public float ThemeScore { get; set; }

    /// <summary>
    /// 전체 원점수
    /// </summary>
    public float RawScore { get; set; }
    /// <summary>
    /// 최종 제작 점수
    /// </summary>
    public int FinalScore { get; set; }
}

#endregion

#region ----- 제작 판정 데이터 -----

/// <summary>
/// 제작 판정 데이터
/// </summary>
public class CraftReviewData
{
    /// <summary>
    /// 최소 제작 조건 달성 여부
    /// </summary>
    public bool IsAssemblyCompleted { get; set; }
    /// <summary>
    /// 최대 제작 코스트 초과 여부
    /// </summary>
    public bool IsCostExceeded { get; set; }
    /// <summary>
    /// 사용 제작 코스트
    /// </summary>
    public int UsedCraftCost { get; set; }
    /// <summary>
    /// 사용한 전체 파츠 수
    /// </summary>
    public int UsedPartCount { get; set; }
    /// <summary>
    /// 최대 파츠 개수 초과 여부
    /// </summary>
    public bool IsPartCountExceeded { get; set; }

    /// <summary>
    /// 주요 요구 사항 달성 결과
    /// </summary>
    public List<CraftConditionResult> RequirementResults { get; set; }
    /// <summary>
    /// 희망 사항 달성 결과
    /// </summary>
    public List<CraftConditionResult> WishResults { get; set; }
    /// <summary>
    /// 활성 테마 결과
    /// </summary>
    public List<CraftThemeResult> ThemeResults { get; set; }
    /// <summary>
    /// 목표 테마 진행 결과
    /// </summary>
    public List<CraftTargetThemeResult> TargetThemeResults { get; set; }
    /// <summary>
    /// 제외 테마 사용 결과
    /// </summary>
    public List<CraftExcludedThemeResult> ExcludedThemeResults { get; set; }
    /// <summary>
    /// 제작 점수 결과
    /// </summary>
    public CraftScoreResult ScoreResult { get; set; }
}

#endregion

#region ----- 확정 제작 결과 -----

/// <summary>
/// 확정된 제작 결과
/// </summary>
public class CraftResult
{
    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId { get; set; }
    /// <summary>
    /// 루트 몸통 배치 번호
    /// </summary>
    public int RootPlacementNumber { get; set; }
    /// <summary>
    /// 확정 배치 파츠
    /// </summary>
    public List<PlacedPartData> PlacedParts { get; set; }
    /// <summary>
    /// 확정 당시 판정 데이터
    /// </summary>
    public CraftReviewData ReviewData { get; set; }
}

#endregion
