using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 제작 판정 저장 데이터
/// </summary>
[Serializable]
public class CraftReviewSaveData
{
    [SerializeField] bool _isAssemblyCompleted;     //최소 제작 조건 달성 여부
    [SerializeField] bool _isCostExceeded;      //제작 코스트 초과 여부
    [SerializeField] int _usedCraftCost;        //사용한 제작 코스트
    [SerializeField] int _usedPartCount;        //사용한 파츠 개수
    [SerializeField] bool _isPartCountExceeded;     //파츠 개수 제한 초과 여부

    [SerializeField] List<CraftConditionResultSaveData> _requirementResults;        //주요 요구 사항 충족 결과
    [SerializeField] List<CraftConditionResultSaveData> _wishResults;       //희망 사항 달성 결과
    [SerializeField] List<CraftThemeResultSaveData> _themeResults;      //테마 달성 결과
    [SerializeField] List<CraftTargetThemeResultSaveData> _targetThemeResults;      //목표 테마 달성 결과
    [SerializeField] List<CraftExcludedThemeResultSaveData> _excludedThemeResults;      //제외 테마 달성 결과
    [SerializeField] CraftScoreSaveData _scoreResult;       //점수 결과

    /// <summary>
    /// 최소 제작 조건 달성 여부
    /// </summary>
    public bool IsAssemblyCompleted => _isAssemblyCompleted;
    /// <summary>
    /// 제작 코스트 초과 여부
    /// </summary>
    public bool IsCostExceeded => _isCostExceeded;
    /// <summary>
    /// 사용한 제작 코스트
    /// </summary>
    public int UsedCraftCost => _usedCraftCost;
    /// <summary>
    /// 사용한 파츠 개수
    /// </summary>
    public int UsedPartCount => _usedPartCount;
    /// <summary>
    /// 파츠 개수 제한 초과 여부
    /// </summary>
    public bool IsPartCountExceeded => _isPartCountExceeded;

    /// <summary>
    /// 주요 요구 사항 충족 결과
    /// </summary>
    public IReadOnlyList<CraftConditionResultSaveData> RequirementResults => _requirementResults;
    /// <summary>
    /// 희망 사항 달성 결과
    /// </summary>
    public IReadOnlyList<CraftConditionResultSaveData> WishResults => _wishResults;
    /// <summary>
    /// 테마 달성 결과
    /// </summary>
    public IReadOnlyList<CraftThemeResultSaveData> ThemeResults => _themeResults;
    /// <summary>
    /// 목표 테마 달성 결과
    /// </summary>
    public IReadOnlyList<CraftTargetThemeResultSaveData> TargetThemeResults => _targetThemeResults;
    /// <summary>
    /// 제외 테마 달성 결과
    /// </summary>
    public IReadOnlyList<CraftExcludedThemeResultSaveData> ExcludedThemeResults => _excludedThemeResults;
    /// <summary>
    /// 점수 결과
    /// </summary>
    public CraftScoreSaveData ScoreResult => _scoreResult;


    /// <summary>
    /// 제작 판정 세이브 데이터 생성
    /// </summary>
    /// <param name="reviewData">제작 판정 결과</param>
    public CraftReviewSaveData ( CraftReviewData reviewData )
    {
        _isAssemblyCompleted = reviewData.IsAssemblyCompleted;
        _isCostExceeded = reviewData.IsCostExceeded;
        _usedCraftCost = reviewData.UsedCraftCost;
        _usedPartCount = reviewData.UsedPartCount;
        _isPartCountExceeded = reviewData.IsPartCountExceeded;

        _requirementResults = CreateConditionResults ( reviewData.RequirementResults );
        _wishResults = CreateConditionResults ( reviewData.WishResults );

        _themeResults = new List<CraftThemeResultSaveData> ( reviewData.ThemeResults.Count );

        for ( int i = 0 ; i < reviewData.ThemeResults.Count ; i++ )
        {
            _themeResults.Add (
                new CraftThemeResultSaveData ( reviewData.ThemeResults [ i ] ) );
        }

        _targetThemeResults =
            new List<CraftTargetThemeResultSaveData> ( reviewData.TargetThemeResults.Count );

        for ( int i = 0 ; i < reviewData.TargetThemeResults.Count ; i++ )
        {
            _targetThemeResults.Add (
                new CraftTargetThemeResultSaveData ( reviewData.TargetThemeResults [ i ] ) );
        }

        _excludedThemeResults =
            new List<CraftExcludedThemeResultSaveData> ( reviewData.ExcludedThemeResults.Count );

        for ( int i = 0 ; i < reviewData.ExcludedThemeResults.Count ; i++ )
        {
            _excludedThemeResults.Add (
                new CraftExcludedThemeResultSaveData ( reviewData.ExcludedThemeResults [ i ] ) );
        }

        _scoreResult = new CraftScoreSaveData ( reviewData.ScoreResult );
    }

    /// <summary>
    /// 제작 조건 판정 결과 생성
    /// </summary>
    /// <param name="results">제작 조건 결과</param>
    /// <returns>제작 조건 판정 저장 데이터 목록</returns>
    List<CraftConditionResultSaveData> CreateConditionResults ( IReadOnlyList<CraftConditionResult> results )
    {
        var saveDatas = new List<CraftConditionResultSaveData> ( results.Count );

        for ( int i = 0 ; i < results.Count ; i++ )
        {
            saveDatas.Add ( new CraftConditionResultSaveData ( results [ i ] ) );
        }

        return saveDatas;
    }
}
