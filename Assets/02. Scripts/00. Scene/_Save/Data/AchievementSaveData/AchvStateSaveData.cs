using System;
using UnityEngine;

/// <summary>
/// 업적 런타임 상태 세이브 데이터
/// </summary>
[Serializable]
public class AchvStateSaveData
{
    [SerializeField] string _id;       //업적 아이디
    [SerializeField] int _reachedStageIndex;       //마지막 달성 단계
    [SerializeField] int _claimedStageIndex;       //마지막 보상 수령 단계
    [SerializeField] bool _isCompleted;       //최종 단계 달성 여부
    [SerializeField] bool _isNew;       //신규 달성 여부

    #region ----- 프로퍼티 -----

    /// <summary>
    /// 업적 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 최근 달성 단계 인덱스
    /// </summary>
    public int ReachedStageIndex => _reachedStageIndex;

    /// <summary>
    /// 최근 수령 단계 인덱스
    /// </summary>
    public int ClaimedStageIndex => _claimedStageIndex;

    /// <summary>
    /// 최종 단계 달성 여부
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// 신규 달성 여부
    /// </summary>
    public bool IsNew => _isNew;

    #endregion

    /// <summary>
    /// 업적 런타임 상태 세이브 데이터 생성
    /// </summary>
    /// <param name="state">업적 런타임 상태</param>
    public AchvStateSaveData ( AchvState state )
    {
        _id = state.Id;
        _reachedStageIndex = state.ReachedStageIndex;
        _claimedStageIndex = state.ClaimedStageIndex;
        _isCompleted = state.IsCompleted;
        _isNew = state.IsNew;
    }
}