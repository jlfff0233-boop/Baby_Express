using System;
using UnityEngine;

/// <summary>
/// 제작 제외 테마 판정 결과 세이브 데이터
/// </summary>
[Serializable]
public class CraftExcludedThemeResultSaveData
{
    [SerializeField] PartTheme _theme;      //파츠 테마
    [SerializeField] int _usedPartCount;        //사용 파츠 개수

    /// <summary>
    /// 파츠 테마
    /// </summary>
    public PartTheme Theme => _theme;
    /// <summary>
    /// 사용 파츠 개수
    /// </summary>
    public int UsedPartCount => _usedPartCount;

    /// <summary>
    /// 제작 제외 테마 판정 세이브 데이터 생성
    /// </summary>
    /// <param name="result">제외 테마 판정 결과</param>
    public CraftExcludedThemeResultSaveData ( CraftExcludedThemeResult result )
    {
        _theme = result.Theme;
        _usedPartCount = result.UsedPartCount;
    }
}