using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상극 테마 한 쌍
/// </summary>
[Serializable]
public class ThemeConflict
{
    [SerializeField] PartTheme _first;       //첫 번째 테마
    [SerializeField] PartTheme _second;      //두 번째 테마

    /// <summary>
    /// 첫 번째 테마
    /// </summary>
    public PartTheme First => _first;

    /// <summary>
    /// 두 번째 테마
    /// </summary>
    public PartTheme Second => _second;
}

/// <summary>
/// 파츠 테마와 대표 이미지 연결 데이터
/// </summary>
[Serializable]
public class CraftThemeIcon
{
    [SerializeField] PartTheme _theme;       //표시할 파츠 테마
    [SerializeField] Sprite _icon;       //테마 대표 이미지

    /// <summary>
    /// 표시할 파츠 테마
    /// </summary>
    public PartTheme Theme => _theme;

    /// <summary>
    /// 테마 대표 이미지
    /// </summary>
    public Sprite Icon => _icon;
}

/// <summary>
/// 제작 점수 설정 데이터
/// </summary>
[CreateAssetMenu( fileName = "CraftScoreSettingsData", menuName = "CraftSettings/ScoreSettingsData" )]
public class CraftScoreSettingsData : ScriptableObject
{
    [Header( "----- 조건 점수 -----" )]
    [SerializeField, Min( 0f )] float _assemblyCompletedScore = 100f;       //조립 완성 점수
    [SerializeField, Min( 0f )] float _requirementCompletedScore = 100f;       //주요 요구 달성 점수
    [SerializeField] float _requirementFailedScore = -100f;       //주요 요구 미달성 점수
    [SerializeField, Min( 0f )] float _wishCompletedScore = 40f;       //희망 달성 점수

    [Header( "----- 테마 점수 -----" )]
    [SerializeField, Min( 0f )] float _normalThemeScore = 60f;       //일반 테마 점수
    [SerializeField, Min( 0f )] float _completedThemeScore = 140f;       //완성 테마 점수
    [SerializeField, Min( 1f )] float _targetThemeRate = 1.5f;       //목표 테마 배율
    [SerializeField, Min( 0f )] float _conflictScorePenalty = 40f;       //상극 한 쌍 감점

    [Header( "----- 테마 제한 -----" )]
    [SerializeField, Range( 0f, 1f )] float _twoThemeRate = 0.85f;       //활성 테마 2개 적용 비율
    [SerializeField, Range( 0f, 1f )] float _multipleThemeRate = 0.7f;       //활성 테마 3개 이상 적용 비율
    [SerializeField, Min( 0f )] float _normalThemeLimit = 200f;       //일반 주문 테마 상한
    [SerializeField, Min( 0f )] float _targetThemeLimit = 250f;       //목표 테마 주문 테마 상한

    [Header( "----- 상극 테마 -----" )]
    [SerializeField] ThemeConflict [ ] _themeConflicts =
        Array.Empty<ThemeConflict>( );       //상극 테마 목록

    [Header( "----- 테마 대표 이미지 -----" )]
    [SerializeField] CraftThemeIcon [ ] _themeIcons =
        Array.Empty<CraftThemeIcon>( );       //종족 테마 등의 대표 이미지 목록

    /// <summary>
    /// 조립 완성 점수
    /// </summary>
    public float AssemblyCompletedScore => _assemblyCompletedScore;

    /// <summary>
    /// 주요 요구 달성 점수
    /// </summary>
    public float RequirementCompletedScore => _requirementCompletedScore;

    /// <summary>
    /// 주요 요구 미달성 점수
    /// </summary>
    public float RequirementFailedScore => _requirementFailedScore;

    /// <summary>
    /// 희망 달성 점수
    /// </summary>
    public float WishCompletedScore => _wishCompletedScore;

    /// <summary>
    /// 일반 테마 점수
    /// </summary>
    public float NormalThemeScore => _normalThemeScore;

    /// <summary>
    /// 완성 테마 점수
    /// </summary>
    public float CompletedThemeScore => _completedThemeScore;

    /// <summary>
    /// 목표 테마 배율
    /// </summary>
    public float TargetThemeRate => _targetThemeRate;

    /// <summary>
    /// 상극 한 쌍 감점
    /// </summary>
    public float ConflictScorePenalty => _conflictScorePenalty;

    /// <summary>
    /// 활성 테마 2개 적용 비율
    /// </summary>
    public float TwoThemeRate => _twoThemeRate;

    /// <summary>
    /// 활성 테마 3개 이상 적용 비율
    /// </summary>
    public float MultipleThemeRate => _multipleThemeRate;

    /// <summary>
    /// 일반 주문 테마 상한
    /// </summary>
    public float NormalThemeLimit => _normalThemeLimit;

    /// <summary>
    /// 목표 테마 주문 테마 상한
    /// </summary>
    public float TargetThemeLimit => _targetThemeLimit;

    /// <summary>
    /// 상극 테마 목록
    /// </summary>
    public IReadOnlyList<ThemeConflict> ThemeConflicts => _themeConflicts;

    /// <summary>
    /// 파츠 테마에 등록된 대표 이미지 조회
    /// </summary>
    /// <param name="theme">조회할 파츠 테마</param>
    /// <returns>등록된 대표 이미지, 없으면 null</returns>
    public Sprite GetThemeIcon ( PartTheme theme )
    {
        if ( _themeIcons == null ) return null;

        for ( int i = 0; i < _themeIcons.Length; i++ )
        {
            CraftThemeIcon current = _themeIcons [ i ];

            if ( current != null && current.Theme == theme )
                return current.Icon;
        }

        return null;
    }
}
