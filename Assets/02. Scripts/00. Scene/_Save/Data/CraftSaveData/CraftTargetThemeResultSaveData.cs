using System;
using UnityEngine;


/// <summary>
/// 제작 목표 테마 판정 결과 세이브 데이터
/// </summary>
[Serializable]
public class CraftTargetThemeResultSaveData
{
    [SerializeField] PartTheme _theme;      //파츠 테마
    [SerializeField] int _usedTypeCount;        //사용한 타입 개수
    [SerializeField] int _totalTypeCount;       //전체 사용 타입 개수
    [SerializeField] bool _isActive;        //테마 활성화 여부
    [SerializeField] bool _isCompleted;     //테마 완성 여부

    /// <summary>
    /// 파츠 테마
    /// </summary>
    public PartTheme Theme => _theme;
    /// <summary>
    /// 사용한 타입 개수
    /// </summary>
    public int UsedTypeCount => _usedTypeCount;
    /// <summary>
    /// 전체 사용 타입 개수
    /// </summary>
    public int TotalTypeCount => _totalTypeCount;
    /// <summary>
    /// 테마 활성화 여부
    /// </summary>
    public bool IsActive => _isActive;
    /// <summary>
    /// 테마 완성 여부
    /// </summary>
    public bool IsCompleted => _isCompleted;


    /// <summary>
    /// 제작 목표 테마 판정 결과 세이브 데이터 생성
    /// </summary>
    /// <param name="result">목표 테마 판정 결과</param>
    public CraftTargetThemeResultSaveData ( CraftTargetThemeResult result )
    {
        _theme = result.Theme;
        _usedTypeCount = result.UsedTypeCount;
        _totalTypeCount = result.TotalTypeCount;
        _isActive = result.IsActive;
        _isCompleted = result.IsCompleted;
    }
}