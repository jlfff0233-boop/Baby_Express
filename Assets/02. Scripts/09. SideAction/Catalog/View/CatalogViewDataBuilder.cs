using System.Collections.Generic;
using System.Text;

/// <summary>
/// 카탈로그 표시 데이터 생성기
/// </summary>
public class CatalogViewDataBuilder
{
    CraftScoreSettingsData _scoreSettings;       //제작 점수와 상극 설정

    /// <summary>
    /// 카탈로그 표시 데이터 생성기 생성자
    /// </summary>
    /// <param name="scoreSettings">제작 점수 설정 데이터</param>
    public CatalogViewDataBuilder (
        CraftScoreSettingsData scoreSettings )
    {
        _scoreSettings = scoreSettings;
    }

    /// <summary>
    /// 카탈로그 파츠 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="part">파츠 데이터</param>
    /// <param name="isUnlocked">파츠 해금 여부</param>
    /// <returns>파츠 슬롯 표시 데이터</returns>
    public CatalogSlotViewData CreateSlot (
        PartsData part, bool isUnlocked )
    {
        return new CatalogSlotViewData(
            part.Id,
            part.Icon,
            isUnlocked ? part.Name : "???",
            part.PartType.GetDisplayName( ),
            isUnlocked == false );
    }

    /// <summary>
    /// 카탈로그 파츠 상세 표시 데이터 생성
    /// </summary>
    /// <param name="part">파츠 데이터</param>
    /// <param name="isUnlocked">파츠 해금 여부</param>
    /// <param name="quantity">현재 보유 수량</param>
    /// <returns>파츠 상세 표시 데이터</returns>
    public CatalogDetailViewData CreateDetail (
        PartsData part, bool isUnlocked, int quantity )
    {
        if ( isUnlocked == false )
            return CreateLockedDetail( part );

        return new CatalogDetailViewData(
            part.Id,
            part.Icon,
            part.Name,
            string.IsNullOrWhiteSpace( part.Description )
                ? "설명: 없음"
                : $"설명: {part.Description}",
            $"분류: {part.PartType.GetDisplayName( )}",
            $"코스트: {part.CraftCost}",
            $"테마: {GetThemeText( part.Themes )}",
            $"상극 테마: {GetConflictText( part.Themes )}",
            "해금됨",
            $"{quantity}개",
            false );
    }

    /// <summary>
    /// 잠긴 파츠 상세 표시 데이터 생성
    /// </summary>
    /// <param name="part">잠긴 파츠 데이터</param>
    /// <returns>잠긴 파츠 상세 표시 데이터</returns>
    CatalogDetailViewData CreateLockedDetail ( PartsData part )
    {
        return new CatalogDetailViewData(
            part.Id,
            part.Icon,
            "???",
            "설명: 상세 정보는 해금 후 확인할 수 있습니다.",
            $"분류: {part.PartType.GetDisplayName( )}",
            "코스트: -",
            "테마: -",
            "상극 테마: -",
            "잠김",
            "-",
            true );
    }

    /// <summary>
    /// 파츠 테마 표시 문자열 생성
    /// </summary>
    /// <param name="themes">파츠 테마 목록</param>
    /// <returns>파츠 테마 표시 문자열</returns>
    string GetThemeText ( IReadOnlyList<PartTheme> themes )
    {
        var text = new StringBuilder( );

        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ] == PartTheme.None ) continue;

            if ( text.Length > 0 )
                text.Append( " / " );

            text.Append(
                themes [ i ].GetDisplayName( ) );
        }

        return text.Length > 0 ? text.ToString( ) : "없음";
    }

    /// <summary>
    /// 파츠 테마와 연결된 상극 표시 문자열 생성
    /// </summary>
    /// <param name="themes">파츠 테마 목록</param>
    /// <returns>상극 표시 문자열</returns>
    string GetConflictText ( IReadOnlyList<PartTheme> themes )
    {
        var conflictThemes = new List<PartTheme>( );
        IReadOnlyList<ThemeConflict> conflicts =
            _scoreSettings.ThemeConflicts;

        for ( int i = 0; i < conflicts.Count; i++ )
        {
            ThemeConflict conflict = conflicts [ i ];

            if ( ContainsTheme( themes, conflict.First ) )
                AddConflictTheme( conflictThemes, conflict.Second );

            if ( ContainsTheme( themes, conflict.Second ) )
                AddConflictTheme( conflictThemes, conflict.First );
        }

        if ( conflictThemes.Count == 0 ) return "없음";

        var conflictTexts = new List<string>( conflictThemes.Count );

        for ( int i = 0; i < conflictThemes.Count; i++ )
        {
            conflictTexts.Add(
                conflictThemes [ i ].GetDisplayName( ) );
        }

        return string.Join( " / ", conflictTexts );
    }

    /// <summary>
    /// 상극 테마를 중복 없이 추가
    /// </summary>
    /// <param name="themes">상극 테마 목록</param>
    /// <param name="theme">추가할 상극 테마</param>
    void AddConflictTheme ( List<PartTheme> themes, PartTheme theme )
    {
        if ( themes.Contains( theme ) == false )
            themes.Add( theme );
    }

    /// <summary>
    /// 테마 목록에 지정한 테마가 있는지 확인
    /// </summary>
    /// <param name="themes">확인할 테마 목록</param>
    /// <param name="target">찾을 테마</param>
    /// <returns>테마 포함 여부</returns>
    bool ContainsTheme (
        IReadOnlyList<PartTheme> themes, PartTheme target )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ] == target )
                return true;
        }

        return false;
    }
}
