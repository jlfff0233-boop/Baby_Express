
/// <summary>
/// 카탈로그 정렬 타입
/// </summary>
public enum CatalogSortType
{
    Default,                //해금 우선, 아이디 오름차순
    NameAscending,          //이름 오름차순
    NameDescending,         //이름 내림차순
    IdAscending,            //아이디 오름차순
    IdDescending,           //아이디 내림차순
    UnlockFirst,            //해금 우선
    OwnedFirst,             //보유 우선
}

/// <summary>
/// 카탈로그 분류 종류
/// </summary>
public enum CatalogCategoryType
{
    Part,                   //파츠 종류
    Theme,                  //테마
}

/// <summary>
/// 카탈로그 검색, 필터, 정렬 상태
/// </summary>
public class CatalogFilter
{
    /// <summary>
    /// 검색어
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// 선택한 파츠 타입
    /// </summary>
    public PartType? PartType { get; set; }

    /// <summary>
    /// 선택한 파츠 테마
    /// </summary>
    public PartTheme? Theme { get; set; }

    /// <summary>
    /// 선택한 정렬 타입
    /// </summary>
    public CatalogSortType SortType { get; set; } =
        CatalogSortType.Default;
}
