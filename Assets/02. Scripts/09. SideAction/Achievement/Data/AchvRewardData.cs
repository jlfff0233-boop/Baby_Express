using System;
using UnityEngine;

/// <summary>
/// 업적 단계 보상 데이터
/// </summary>
[Serializable]
public class AchvRewardData
{
    [SerializeField] AchvRewardType _type;       //보상 종류
    [SerializeField] float _amount;      //골드 보상 수치
    [Tooltip( "해금 대상 아이디" )]
    [SerializeField] string _targetId;

    /// <summary>
    /// 보상 종류
    /// </summary>
    public AchvRewardType Type => _type;

    /// <summary>
    /// 골드 보상 수치
    /// </summary>
    public float Amount => _amount;

    /// <summary>
    /// 해금 대상 아이디
    /// </summary>
    public string TargetId => _targetId;
}