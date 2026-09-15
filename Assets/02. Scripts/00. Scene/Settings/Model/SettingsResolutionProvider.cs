using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 실행 환경에서 지원하는 해상도 목록 제공
/// </summary>
public static class SettingsResolutionProvider
{
    /// <summary>
    /// 중복을 제거한 지원 해상도 목록 조회
    /// </summary>
    /// <returns>너비와 높이 오름차순 해상도 목록</returns>
    public static IReadOnlyList<SettingsResolution> GetSupportedResolutions ()
    {
        List<SettingsResolution> resolutions =
            new List<SettingsResolution>( );

        Resolution [ ] screenResolutions = Screen.resolutions;

        for ( int i = 0; i < screenResolutions.Length; i++ )
        {
            SettingsResolution resolution = new SettingsResolution(
                screenResolutions [ i ].width,
                screenResolutions [ i ].height );

            AddIfNotExists( resolutions, resolution );
        }

        //현재 화면 해상도가 지원 목록에 없으면 별도로 추가
        SettingsResolution currentResolution =
            new SettingsResolution( Screen.width, Screen.height );

        AddIfNotExists( resolutions, currentResolution );

        resolutions.Sort( CompareResolution );

        return resolutions;
    }

    /// <summary>
    /// 중복되지 않은 해상도 추가
    /// </summary>
    /// <param name="resolutions">해상도 목록</param>
    /// <param name="resolution">추가할 해상도</param>
    static void AddIfNotExists (
        List<SettingsResolution> resolutions, SettingsResolution resolution )
    {
        for ( int i = 0; i < resolutions.Count; i++ )
        {
            if ( resolutions [ i ].IsSame( resolution ) )
                return;
        }

        resolutions.Add( resolution );
    }

    /// <summary>
    /// 해상도 너비와 높이 오름차순 비교
    /// </summary>
    /// <param name="left">왼쪽 해상도</param>
    /// <param name="right">오른쪽 해상도</param>
    /// <returns>정렬 비교 결과</returns>
    static int CompareResolution (
        SettingsResolution left, SettingsResolution right )
    {
        int widthComparison = left.Width.CompareTo( right.Width );

        if ( widthComparison != 0 )
            return widthComparison;

        return left.Height.CompareTo( right.Height );
    }
}