using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 단계 데이터
/// </summary>
[Serializable]
public class AchvStageData
{
    [Tooltip( "달성 목표값" )]
    [SerializeField] int _targetValue;
    [Tooltip( "단계별 표시 이름\n단일형 업적을 공백으로 두면 됨" )]
    [SerializeField] string _displayName;
    [Tooltip( "단계별 표시 설명" )]
    [SerializeField] string _description;
    [Tooltip( "단계별 보상 목록" )]
    [SerializeField] AchvRewardData [ ] _rewards;

    /// <summary>
    /// 단계 달성 목표값
    /// </summary>
    public int TargetValue => _targetValue;

    /// <summary>
    /// 단계별 표시 이름
    /// </summary>
    public string DisplayName => _displayName;

    /// <summary>
    /// 단계별 표시 설명
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// 단계별 보상 목록
    /// </summary>
    public IReadOnlyList<AchvRewardData> Rewards => _rewards;
}