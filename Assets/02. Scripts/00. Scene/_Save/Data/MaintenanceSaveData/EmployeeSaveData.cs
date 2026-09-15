using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직원 모델 세이브 데이터
/// </summary>
[Serializable]
public class EmployeeSaveData
{
    [SerializeField] int _lastPaidTotalDay;       //마지막 주급 지급 영업일
    [SerializeField] List<EmployeeStateSaveData> _states;       //직원별 상태

    /// <summary>
    /// 마지막 주급 지급 영업일
    /// </summary>
    public int LastPaidTotalDay => _lastPaidTotalDay;

    /// <summary>
    /// 직원별 런타임 상태 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<EmployeeStateSaveData> States => _states;

    /// <summary>
    /// 직원 모델 세이브 데이터 생성
    /// </summary>
    /// <param name="lastPaidTotalDay">마지막 주급 지급 영업일</param>
    /// <param name="states">직원 상태 목록</param>
    public EmployeeSaveData (
        int lastPaidTotalDay,
        IReadOnlyCollection<EmployeeState> states )
    {
        _lastPaidTotalDay = lastPaidTotalDay;
        _states = CreateStateSaveDatas( states );
    }

    /// <summary>
    /// 직원 상태 세이브 데이터 목록 생성
    /// </summary>
    /// <param name="states">직원 상태 목록</param>
    /// <returns>직원 상태 세이브 데이터 목록</returns>
    List<EmployeeStateSaveData> CreateStateSaveDatas (
        IReadOnlyCollection<EmployeeState> states )
    {
        var saveDatas = new List<EmployeeStateSaveData>( states.Count );

        foreach ( EmployeeState state in states )
        {
            saveDatas.Add( new EmployeeStateSaveData( state ) );
        }

        return saveDatas;
    }
}