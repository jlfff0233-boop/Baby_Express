using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일일 기록 모델 세이브 데이터
/// </summary>
[Serializable]
public class DailyRecordModelSaveData
{
    [SerializeField] DailyRecordSaveData _currentRecord;       //현재 영업일 기록
    [SerializeField] List<DailyRecordSaveData> _pastRecords;       //과거 일일 기록

    /// <summary>
    /// 현재 영업일 기록
    /// </summary>
    public DailyRecordSaveData CurrentRecord => _currentRecord;
    /// <summary>
    /// 과거 일일 기록
    /// </summary>
    public IReadOnlyList<DailyRecordSaveData> PastRecords => _pastRecords;

    /// <summary>
    /// 일일 기록 모델 세이브 데이터 생성
    /// </summary>
    /// <param name="currentRecord">현재 영업일 기록</param>
    /// <param name="pastRecords">과거 일일 기록</param>
    public DailyRecordModelSaveData (
        DailyRecordSaveData currentRecord,
        IReadOnlyCollection<DailyRecordSaveData> pastRecords )
    {
        _currentRecord = currentRecord;
        _pastRecords = new List<DailyRecordSaveData>( pastRecords );
    }
}
