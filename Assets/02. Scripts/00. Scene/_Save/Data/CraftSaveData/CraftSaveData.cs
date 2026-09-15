using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 확정 제작 결과 저장 데이터
/// </summary>
[Serializable]
public class CraftSaveData
{
    [SerializeField] List<CraftResultSaveData> _results;       //주문별 제작 결과

    /// <summary>
    /// 주문별 제작 결과
    /// </summary>
    public IReadOnlyList<CraftResultSaveData> Results => _results;


    /// <summary>
    /// 제작 세이브 데이터 생성자
    /// </summary>
    /// <param name="results">제작 결과</param>
    public CraftSaveData ( IReadOnlyCollection<CraftResultSaveData> results )
    {
        _results = new List<CraftResultSaveData> ( results );
    }
}