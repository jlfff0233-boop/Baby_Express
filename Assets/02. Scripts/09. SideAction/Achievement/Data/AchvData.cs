using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "AchievementSettings/AchievementData" )]
public class AchvData : ScriptableObject
{
    [Header( "----- 업적 정보 -----" )]
    [SerializeField] string _id;       //업적 아이디
    [SerializeField] AchvCategory _category;      //업적 분류
    [SerializeField] string _displayName;        //업적 표시 이름
    [SerializeField, TextArea( 5, 10 )] string _description;        //업적 기본 설명
    [SerializeField] bool _isHidden;       //숨김 업적 여부

    [Header( "----- 진행 설정 -----" )]
    [SerializeField] AchvProgressType _progressType;      //진행도 판정 종류
    [SerializeField] AchvStageData [ ] _stages;        //단계별 업적 데이터

    /// <summary>
    /// 업적 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 업적 분류
    /// </summary>
    public AchvCategory Category => _category;

    /// <summary>
    /// 업적 표시 이름
    /// </summary>
    public string DisplayName => _displayName;

    /// <summary>
    /// 업적 기본 설명
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// 숨김 업적 여부
    /// </summary>
    public bool IsHidden => _isHidden;

    /// <summary>
    /// 진행도 판정 종류
    /// </summary>
    public AchvProgressType ProgressType => _progressType;

    /// <summary>
    /// 단계별 업적 데이터
    /// </summary>
    public IReadOnlyList<AchvStageData> Stages => _stages;
}
