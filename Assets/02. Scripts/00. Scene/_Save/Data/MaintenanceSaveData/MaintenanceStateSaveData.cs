using System;
using UnityEngine;

/// <summary>
/// 정비 항목 상태 세이브 데이터
/// </summary>
[Serializable]
public class MaintenanceStateSaveData
{
    [SerializeField] string _maintenanceId;       //정비 아이디
    [SerializeField] int _purchasedLevel;       //구매한 정비 단계
    [SerializeField] int _appliedLevel;       //적용된 정비 단계

    #region ----- 프로퍼티 -----

    /// <summary>
    /// 정비 아이디
    /// </summary>
    public string MaintenanceId => _maintenanceId;

    /// <summary>
    /// 구매한 정비 단계
    /// </summary>
    public int PurchasedLevel => _purchasedLevel;

    /// <summary>
    /// 적용된 정비 단계
    /// </summary>
    public int AppliedLevel => _appliedLevel;

    #endregion

    /// <summary>
    /// 정비 항목 상태 세이브 데이터 생성
    /// </summary>
    /// <param name="state">정비 항목 런타임 상태</param>
    public MaintenanceStateSaveData ( MaintenanceState state )
    {
        _maintenanceId = state.Data.Id;
        _purchasedLevel = state.PurchasedLevel;
        _appliedLevel = state.AppliedLevel;
    }
}