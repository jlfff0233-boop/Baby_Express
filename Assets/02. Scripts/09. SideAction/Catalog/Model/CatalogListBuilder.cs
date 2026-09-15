using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 카탈로그 목록 빌더 - 파츠 추출, 검색, 필터, 정렬 처리
/// </summary>
public class CatalogListBuilder
{
    /// <summary>
    /// 현재 조건에 맞는 카탈로그 파츠 목록 생성
    /// </summary>
    /// <param name="datas">전체 구매 가능 상품 데이터</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="filter">카탈로그 검색과 필터 상태</param>
    /// <returns>현재 조건에 맞는 파츠 목록</returns>
    public IReadOnlyList<PartsData> Build (
        IReadOnlyList<PurchasableData> datas,
        InventoryModel inventoryModel, PlayStateModel playStateModel,
        CatalogFilter filter )
    {
        var parts = new List<PartsData>( );
        var unlockStates = new Dictionary<string, bool>( );
        var ownedStates = new Dictionary<string, bool>( );
        string searchText = filter.SearchText?.Trim( ) ?? string.Empty;

        for ( int i = 0; i < datas.Count; i++ )
        {
            //카탈로그에는 파츠 상품만 표시
            if ( datas [ i ] is not PartsData part ) continue;

            bool isUnlocked = playStateModel.IsUnlocked( part.Id );
            int quantity = inventoryModel.GetQuantity( part.Id );

            if ( MatchesSearch( part, isUnlocked, searchText ) == false )
                continue;

            if ( MatchesPartType( part, filter.PartType ) == false )
                continue;

            if ( MatchesTheme( part, isUnlocked, filter.Theme ) == false )
                continue;

            parts.Add( part );
            unlockStates [ part.Id ] = isUnlocked;
            ownedStates [ part.Id ] = isUnlocked && quantity > 0;
        }

        return SortParts(
            parts, unlockStates, ownedStates, filter.SortType );
    }

    /// <summary>
    /// 검색어 일치 여부 확인
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="isUnlocked">파츠 해금 여부</param>
    /// <param name="searchText">정리된 검색어</param>
    /// <returns>검색어 일치 여부</returns>
    bool MatchesSearch (
        PartsData part, bool isUnlocked, string searchText )
    {
        //검색하지 않을 때는 잠긴 파츠도 목록에 포함
        if ( string.IsNullOrEmpty( searchText ) ) return true;

        //해금 파츠만 검색
        return isUnlocked &&
            part.Name.IndexOf( searchText, StringComparison.OrdinalIgnoreCase ) >= 0;
    }

    /// <summary>
    /// 파츠 타입 필터 일치 여부 확인
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="partType">선택한 파츠 타입</param>
    /// <returns>파츠 타입 일치 여부</returns>
    bool MatchesPartType ( PartsData part, PartType? partType )
    {
        return partType.HasValue == false || part.PartType == partType.Value;
    }

    /// <summary>
    /// 파츠 테마 필터 일치 여부 확인
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="isUnlocked">파츠 해금 여부</param>
    /// <param name="theme">선택한 파츠 테마</param>
    /// <returns>파츠 테마 일치 여부</returns>
    bool MatchesTheme (
        PartsData part, bool isUnlocked, PartTheme? theme )
    {
        if ( theme.HasValue == false ) return true;

        //잠긴 파츠의 숨겨진 테마가 필터 결과로 노출되지 않게 차단
        if ( isUnlocked == false ) return false;

        for ( int i = 0; i < part.Themes.Count; i++ )
        {
            if ( part.Themes [ i ] == theme.Value )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 카탈로그 파츠 목록 정렬
    /// </summary>
    /// <param name="parts">정렬할 파츠 목록</param>
    /// <param name="unlockStates">파츠별 해금 상태</param>
    /// <param name="ownedStates">파츠별 보유 상태</param>
    /// <param name="sortType">선택한 정렬 타입</param>
    /// <returns>정렬된 파츠 목록</returns>
    IReadOnlyList<PartsData> SortParts (
        IReadOnlyList<PartsData> parts,
        IReadOnlyDictionary<string, bool> unlockStates,
        IReadOnlyDictionary<string, bool> ownedStates,
        CatalogSortType sortType )
    {
        switch ( sortType )
        {
            case CatalogSortType.NameAscending:     //이름 오름차순
                return parts.OrderBy(
                    part => GetUnlockOrder( part, unlockStates ) )      //해금 파츠 정렬
                    .ThenBy(
                        part => GetSortName( part, unlockStates ),      //이름순 정렬
                        StringComparer.Ordinal )
                    .ThenBy(
                        part => part.Id, StringComparer.Ordinal )       //아이디순 정렬
                    .ToList( );

            case CatalogSortType.NameDescending:        //이름 내림차순
                return parts.OrderBy(
                    part => GetUnlockOrder( part, unlockStates ) )
                    .ThenByDescending(
                        part => GetSortName( part, unlockStates ),
                        StringComparer.Ordinal )
                    .ThenBy(
                        part => part.Id, StringComparer.Ordinal )
                    .ToList( );

            case CatalogSortType.IdAscending:       //아이디 오름차순
                return parts.OrderBy(
                    part => part.Id, StringComparer.Ordinal ).ToList( );

            case CatalogSortType.IdDescending:      //아이디 내림차순
                return parts.OrderByDescending(
                    part => part.Id, StringComparer.Ordinal ).ToList( );

            case CatalogSortType.UnlockFirst:       //해금 우선
                return parts.OrderBy(
                    part => GetUnlockOrder( part, unlockStates ) )
                    .ThenBy(
                        part => part.Id, StringComparer.Ordinal )
                    .ToList( );

            case CatalogSortType.OwnedFirst:        //보유 우선
                return parts.OrderBy(
                    part => GetOwnedOrder( part, ownedStates ) )
                    .ThenBy(
                        part => GetUnlockOrder( part, unlockStates ) )
                    .ThenBy(
                        part => part.Id, StringComparer.Ordinal )
                    .ToList( );

            default:
                //해금 파츠를 먼저 표시하고 같은 상태에서는 아이디순 정렬
                return parts.OrderBy(
                    part => GetUnlockOrder( part, unlockStates ) )
                    .ThenBy(
                        part => part.Id, StringComparer.Ordinal )
                    .ToList( );
        }
    }

    /// <summary>
    /// 해금 파츠 우선 정렬값 반환
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="unlockStates">파츠별 해금 상태</param>
    /// <returns>해금 파츠 0, 잠긴 파츠 1</returns>
    int GetUnlockOrder (
        PartsData part, IReadOnlyDictionary<string, bool> unlockStates )
    {
        return unlockStates [ part.Id ] ? 0 : 1;
    }

    /// <summary>
    /// 보유 파츠 우선 정렬값 반환
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="ownedStates">파츠별 보유 상태</param>
    /// <returns>보유 파츠 0, 미보유 파츠 1</returns>
    int GetOwnedOrder (
        PartsData part, IReadOnlyDictionary<string, bool> ownedStates )
    {
        return ownedStates [ part.Id ] ? 0 : 1;
    }

    /// <summary>
    /// 이름 정렬에 사용할 공개 이름 반환
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="unlockStates">파츠별 해금 상태</param>
    /// <returns>정렬에 사용할 이름</returns>
    string GetSortName (
        PartsData part, IReadOnlyDictionary<string, bool> unlockStates )
    {
        return unlockStates [ part.Id ]
            ? part.Name
            : string.Empty;
    }
}
