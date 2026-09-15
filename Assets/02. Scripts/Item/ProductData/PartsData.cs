using UnityEngine;


/// <summary>
/// 신체 파츠 공통 데이터
/// </summary>
public abstract class PartsData : PurchasableData
{
    [Header ( "----- 설정 데이터(파츠) -----" )]
    [SerializeField, Min ( 0 )] int _craftCost;       //제작 코스트
    [SerializeField] PartTheme [ ] _themes = System.Array.Empty<PartTheme>( );       //파츠 테마

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public abstract PartType PartType { get; }

    /// <summary>
    /// 제작 코스트
    /// </summary>
    public int CraftCost => _craftCost;

    /// <summary>
    /// 파츠 테마 목록
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<PartTheme> Themes => _themes;
}
