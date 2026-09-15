using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 모델 세이브 데이터
/// </summary>
[Serializable]
public class AchvSaveData
{
    [SerializeField] List<AchvStateSaveData> _states;       //업적별 상태

    /// <summary>
    /// 업적 상태 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<AchvStateSaveData> States => _states;

    /// <summary>
    /// 업적 모델 세이브 데이터 생성
    /// </summary>
    /// <param name="states">업적 상태</param>
    public AchvSaveData (
        IReadOnlyDictionary<string, AchvState> states )
    {
        _states = CreateStateSaveDatas( states );
    }

    /// <summary>
    /// 업적 상태 세이브 데이터 목록 생성
    /// </summary>
    /// <param name="states">업적 상태</param>
    /// <returns>업적 상태 세이브 데이터 목록</returns>
    List<AchvStateSaveData> CreateStateSaveDatas (
        IReadOnlyDictionary<string, AchvState> states )
    {
        var saveDatas =
            new List<AchvStateSaveData>( states.Count );

        foreach ( AchvState state in states.Values )
        {
            saveDatas.Add( new AchvStateSaveData( state ) );
        }

        return saveDatas;
    }
}