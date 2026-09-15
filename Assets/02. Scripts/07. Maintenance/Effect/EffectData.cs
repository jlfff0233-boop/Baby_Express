using System;
using UnityEngine;

/// <summary>
/// 정비 효과 데이터
/// </summary>
[Serializable]
public class EffectData
{
    [SerializeField] string _targetId;      //적용 대상 아이디
    [SerializeField] MaintenanceEffectType _type;       //효과 종류
    [SerializeField] float _value;      //적용값

    /// <summary>
    /// 적용 대상 아이디
    /// </summary>
    public string TargetId => _targetId;

    /// <summary>
    /// 효과 종류
    /// </summary>
    public MaintenanceEffectType Type => _type;

    /// <summary>
    /// 적용값
    /// </summary>
    public float Value => _value;

}