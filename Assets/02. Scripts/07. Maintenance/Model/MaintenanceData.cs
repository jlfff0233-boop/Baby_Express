using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정비 데이터
/// </summary>
[CreateAssetMenu( menuName = "MaintenanceSettings/MaintenanceData" )]
public class MaintenanceData : PurchasableData
{
    [Header( "----- 설정 데이터(정비) -----" )]
    [SerializeField] MaintenanceType _type;      //정비 종류
    [SerializeField] MaintenanceApplyType _applyType;      //효과 적용 시점
    [SerializeField] MaintenanceLevelData [ ] _levels;      //단계별 정비 데이터

    /// <summary>
    /// 정비 종류
    /// </summary>
    public MaintenanceType Type => _type;

    /// <summary>
    /// 효과 적용 시점
    /// </summary>
    public MaintenanceApplyType ApplyType => _applyType;

    /// <summary>
    /// 최대 정비 단계
    /// </summary>
    public int MaxLevel => _levels.Length;

    /// <summary>
    /// 단계별 정비 데이터
    /// </summary>
    public IReadOnlyList<MaintenanceLevelData> Levels => _levels;

    /// <summary>
    /// 지정 단계 데이터 조회
    /// </summary>
    /// <param name="level">조회할 정비 단계</param>
    /// <param name="levelData">조회한 단계 데이터</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetLevelData ( int level, out MaintenanceLevelData levelData )
    {
        levelData = null;

        //정비 단계는 1부터 시작
        if ( level < 1 || level > _levels.Length ) return false;

        levelData = _levels [ level - 1 ];
        return levelData != null;
    }

    /// <summary>
    /// 지정 단계 효과값을 적용한 설명 반환
    /// </summary>
    /// <param name="level">표시할 정비 단계</param>
    /// <returns>정비 설명</returns>
    public string GetDescription ( int level )
    {
        if ( GetLevelData( level, out MaintenanceLevelData levelData ) == false ||
            levelData.Effects == null || levelData.Effects.Count == 0 )
            return Description;

        //현재 사용하는 첫 번째 효과값을 설명의 {0}에 적용
        return Description.Replace(
            "{0}", levelData.Effects [ 0 ].Value.ToString( "0.##" ) );
    }
}

/// <summary>
/// 정비 선행 조건 데이터
/// </summary>
[Serializable]
public class MaintenanceRequirementData
{
    [SerializeField] MaintenanceRequirementType _type;      //선행 조건 종류
    [SerializeField] int _requiredValue;     //요구 누적 영업일
    [SerializeField] WeeklyRating _requiredRating;       //요구 주간 영업 평가

    [SerializeField] string _requiredMaintenanceId;      //선행 정비 아이디
    [SerializeField] int _requiredLevel;     //선행 정비 요구 단계

    /// <summary>
    /// 선행 조건 종류
    /// </summary>
    public MaintenanceRequirementType Type => _type;

    /// <summary>
    /// 요구 수치
    /// </summary>
    public int RequiredValue => _requiredValue;

    /// <summary>
    /// 요구 주간 영업 평가
    /// </summary>
    public WeeklyRating RequiredRating => _requiredRating;

    /// <summary>
    /// 선행 정비 아이디
    /// </summary>
    public string RequiredMaintenanceId => _requiredMaintenanceId;

    /// <summary>
    /// 선행 정비 요구 단계
    /// </summary>
    public int RequiredLevel => _requiredLevel;
}

/// <summary>
/// 정비 단계 데이터
/// </summary>
[Serializable]
public class MaintenanceLevelData
{
    [SerializeField] float _upgradeCost;        //해당 단계 정비 비용, 1단계는 상점 기본가 사용
    [SerializeField] EffectData [ ] _effects;       //해당 단계 효과
    [SerializeField] MaintenanceRequirementData [ ] _requirements;      //해당 단계 선행 조건

    /// <summary>
    /// 해당 단계 정비 비용
    /// </summary>
    public float UpgradeCost => _upgradeCost;

    /// <summary>
    /// 해당 단계 효과
    /// </summary>
    public IReadOnlyList<EffectData> Effects => _effects;

    /// <summary>
    /// 해당 단계 선행 조건
    /// </summary>
    public IReadOnlyList<MaintenanceRequirementData> Requirements => _requirements;
}
