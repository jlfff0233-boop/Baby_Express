using System;
using UnityEngine;

/// <summary>
/// 직원 상태 세이브 데이터
/// </summary>
[Serializable]
public class EmployeeStateSaveData
{
    [SerializeField] string _employeeId;       //직원 아이디
    [SerializeField] bool _isHired;       //고용 여부
    [SerializeField] bool _isAssigned;       //배송 배치 여부
    [SerializeField] int _unpaidWorkedDayCount;       //미지급 근무일 수
    [SerializeField] int _lastWorkedTotalDay;       //마지막 근무 기록 영업일

    #region ----- 프로퍼티 -----

    /// <summary>
    /// 직원 아이디
    /// </summary>
    public string EmployeeId => _employeeId;

    /// <summary>
    /// 고용 여부
    /// </summary>
    public bool IsHired => _isHired;

    /// <summary>
    /// 배송 배치 여부
    /// </summary>
    public bool IsAssigned => _isAssigned;

    /// <summary>
    /// 미지급 근무일 수
    /// </summary>
    public int UnpaidWorkedDayCount => _unpaidWorkedDayCount;

    /// <summary>
    /// 마지막 근무 기록 영업일
    /// </summary>
    public int LastWorkedTotalDay => _lastWorkedTotalDay;

    #endregion

    /// <summary>
    /// 직원별 런타임 상태 세이브 데이터 생성
    /// </summary>
    /// <param name="state">직원별 런타임 상태</param>
    public EmployeeStateSaveData ( EmployeeState state )
    {
        _employeeId = state.Data.Id;
        _isHired = state.IsHired;
        _isAssigned = state.IsAssigned;
        _unpaidWorkedDayCount = state.UnpaidWorkedDayCount;
        _lastWorkedTotalDay = state.LastWorkedTotalDay;
    }
}