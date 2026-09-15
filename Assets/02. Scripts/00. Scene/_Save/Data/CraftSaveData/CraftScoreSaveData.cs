using System;
using UnityEngine;


/// <summary>
/// 제작 점수 저장 데이터
/// </summary>
[Serializable]
public class CraftScoreSaveData
{
    [SerializeField] float _assemblyScore;      //제작 완성 점수
    [SerializeField] float _requirementScore;       //주요 요구 사항 점수
    [SerializeField] float _wishScore;      //희망 사항 점수

    [SerializeField] float _themeBaseScore;     //테마 기본 점수
    [SerializeField] float _targetThemeBonus;       //목표 테마 보너스
    [SerializeField] float _themeResearchBonus;     //연구 보너스

    [SerializeField] float _themeSubtotal;      //테마 점수 소계(감쇠 미적용)
    [SerializeField] float _themeRate;      //복수 테마 적용 비율
    [SerializeField] float _themeReduction;     //복수 테마 감쇠 점수
    [SerializeField] float _themeLimit;     //적용된 테마 상한
    [SerializeField] float _themeLimitReduction;        //테마 상한 감소 점수

    [SerializeField] int _conflictCount;        //상극 테마 쌍 수
    [SerializeField] float _conflictPenalty;        //상극 테마 감점
    [SerializeField] float _themeScore;     //최종 테마 점수

    [SerializeField] float _rawScore;       //원점수
    [SerializeField] int _finalScore;       //최종 점수

    /// <summary>
    /// 제작 완성 점수
    /// </summary>
    public float AssemblyScore => _assemblyScore;
    /// <summary>
    /// 주요 요구 사항 점수
    /// </summary>
    public float RequirementScore => _requirementScore;
    /// <summary>
    /// 희망 사항 점수
    /// </summary>
    public float WishScore => _wishScore;
    /// <summary>
    /// 테마 기본 점수
    /// </summary>
    public float ThemeBaseScore => _themeBaseScore;
    /// <summary>
    /// 목표 테마 보너스
    /// </summary>
    public float TargetThemeBonus => _targetThemeBonus;
    /// <summary>
    /// 테마 연구 보너스
    /// </summary>
    public float ThemeResearchBonus => _themeResearchBonus;
    /// <summary>
    /// 점수 소계(감쇠 미적용)
    /// </summary>
    public float ThemeSubtotal => _themeSubtotal;
    /// <summary>
    /// 복수 테마 적용 비율
    /// </summary>
    public float ThemeRate => _themeRate;
    /// <summary>
    /// 복수 테마 감쇠 점수
    /// </summary>
    public float ThemeReduction => _themeReduction;
    /// <summary>
    /// 적용된 테마 제한
    /// </summary>
    public float ThemeLimit => _themeLimit;
    /// <summary>
    /// 테마 상한 감소 점수
    /// </summary>
    public float ThemeLimitReduction => _themeLimitReduction;
    /// <summary>
    /// 상극 테마 쌍 수
    /// </summary>
    public int ConflictCount => _conflictCount;
    /// <summary>
    /// 상극 테마 감점
    /// </summary>
    public float ConflictPenalty => _conflictPenalty;
    /// <summary>
    /// 최종 테마 점수
    /// </summary>
    public float ThemeScore => _themeScore;
    /// <summary>
    /// 원점수
    /// </summary>
    public float RawScore => _rawScore;
    /// <summary>
    /// 최종 점수
    /// </summary>
    public int FinalScore => _finalScore;


    /// <summary>
    /// 제작 점수 세이브 데이터 생성
    /// </summary>
    /// <param name="result">점수 결과</param>
    public CraftScoreSaveData ( CraftScoreResult result )
    {
        _assemblyScore = result.AssemblyScore;
        _requirementScore = result.RequirementScore;
        _wishScore = result.WishScore;

        _themeBaseScore = result.ThemeBaseScore;
        _targetThemeBonus = result.TargetThemeBonus;
        _themeResearchBonus = result.ThemeResearchBonus;
        _themeSubtotal = result.ThemeSubtotal;
        _themeRate = result.ThemeRate;
        _themeReduction = result.ThemeReduction;
        _themeLimit = result.ThemeLimit;
        _themeLimitReduction = result.ThemeLimitReduction;
        _conflictCount = result.ConflictCount;
        _conflictPenalty = result.ConflictPenalty;
        _themeScore = result.ThemeScore;

        _rawScore = result.RawScore;
        _finalScore = result.FinalScore;
    }
}