using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정비 모델 세이브 데이터
/// </summary>
[Serializable]
public class MaintenanceSaveData
{
    [SerializeField] List<MaintenanceStateSaveData> _states;       //정비 항목 상태

    /// <summary>
    /// 정비 항목 상태 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<MaintenanceStateSaveData> States => _states;

    /// <summary>
    /// 정비 모델 세이브 데이터 생성
    /// </summary>
    /// <param name="states">정비 항목 상태 목록</param>
    public MaintenanceSaveData (
        IReadOnlyCollection<MaintenanceState> states )
    {
        _states = CreateStateSaveDatas( states );
    }

    /// <summary>
    /// 정비 항목 상태 세이브 데이터 목록 생성
    /// </summary>
    /// <param name="states">정비 항목 상태 목록</param>
    /// <returns>정비 항목 상태 세이브 데이터 목록</returns>
    List<MaintenanceStateSaveData> CreateStateSaveDatas (
        IReadOnlyCollection<MaintenanceState> states )
    {
        var saveDatas = new List<MaintenanceStateSaveData>( states.Count );

        foreach ( MaintenanceState state in states )
        {
            //세이브 데이터 추가
            saveDatas.Add( new MaintenanceStateSaveData( state ) );
        }

        return saveDatas;
    }
}