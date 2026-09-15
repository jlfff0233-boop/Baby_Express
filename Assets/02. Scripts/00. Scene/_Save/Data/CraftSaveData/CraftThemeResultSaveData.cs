using System;
using UnityEngine;

/// <summary>
/// 제작 테마 결과 세이브 데이터
/// </summary>
[Serializable]
public class CraftThemeResultSaveData
{
    [SerializeField] PartTheme _theme;      //파츠 테마
    [SerializeField] int _usedTypeCount;       //테마에 사용된 파츠 타입 수
    [SerializeField] int _totalTypeCount;      //제작에 사용된 전체 파츠 타입 수
    [SerializeField] bool _isCompleted;     //완성 여부
    [SerializeField] bool _isTargetTheme;       //목표 테마 여부
    [SerializeField] float _baseScore;      //기본 점수
    [SerializeField] float _targetBonus;        //목표 보너스
    [SerializeField] float _researchBonus;      //연구 보너스

    /// <summary>
    /// 파츠 테마
    /// </summary>
    public PartTheme Theme => _theme;
    /// <summary>
    /// 테마에 사용된 파츠 타입 수
    /// </summary>
    public int UsedTypeCount => _usedTypeCount;
    /// <summary>
    /// 제작에 사용된 전체 파츠 타입 수
    /// </summary>
    public int TotalTypeCount => _totalTypeCount;
    /// <summary>
    /// 완성 여부
    /// </summary>
    public bool IsCompleted => _isCompleted;
    /// <summary>
    /// 목표 테마 여부
    /// </summary>
    public bool IsTargetTheme => _isTargetTheme;
    /// <summary>
    /// 기본 점수
    /// </summary>
    public float BaseScore => _baseScore;
    /// <summary>
    /// 목표 보너스
    /// </summary>
    public float TargetBonus => _targetBonus;
    /// <summary>
    /// 연구 보너스
    /// </summary>
    public float ResearchBonus => _researchBonus;


    /// <summary>
    /// 제작 테마 판정 결과 세이브 데이터 생성
    /// </summary>
    /// <param name="result">테마 판정 결과</param>
    public CraftThemeResultSaveData ( CraftThemeResult result )
    {
        _theme = result.Theme;
        _usedTypeCount = result.UsedTypeCount;
        _totalTypeCount = result.TotalTypeCount;
        _isCompleted = result.IsCompleted;
        _isTargetTheme = result.IsTargetTheme;
        _baseScore = result.BaseScore;
        _targetBonus = result.TargetBonus;
        _researchBonus = result.ResearchBonus;
    }
}
