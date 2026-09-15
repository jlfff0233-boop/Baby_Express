using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 결산 모델 세이브 데이터
/// </summary>
[Serializable]
public class SettlementSaveData
{
    [SerializeField] List<WeeklySettlementSaveData> _weeklySettlements;       //과거 주간 결산
    [SerializeField] bool _hasNotification;       //새 주간 결산 알림 여부

    /// <summary>
    /// 과거 주간 결산 목록
    /// </summary>
    public IReadOnlyList<WeeklySettlementSaveData> WeeklySettlements =>
        _weeklySettlements;

    /// <summary>
    /// 새 주간 결산 알림 여부
    /// </summary>
    public bool HasNotification => _hasNotification;

    /// <summary>
    /// 결산 모델 세이브 데이터 생성
    /// </summary>
    /// <param name="weeklySettlements">과거 주간 결산 목록</param>
    /// <param name="hasNotification">새 주간 결산 알림 여부</param>
    public SettlementSaveData (
        IReadOnlyList<WeeklySettlementData> weeklySettlements,
        bool hasNotification )
    {
        _weeklySettlements =
            CreateWeeklySettlementSaveDatas( weeklySettlements );
        _hasNotification = hasNotification;
    }

    /// <summary>
    /// 주간 결산 세이브 데이터 목록 생성
    /// </summary>
    /// <param name="weeklySettlements">과거 주간 결산 목록</param>
    /// <returns>주간 결산 세이브 데이터 목록</returns>
    List<WeeklySettlementSaveData> CreateWeeklySettlementSaveDatas (
        IReadOnlyList<WeeklySettlementData> weeklySettlements )
    {
        var saveDatas =
            new List<WeeklySettlementSaveData>( weeklySettlements.Count );

        for ( int i = 0; i < weeklySettlements.Count; i++ )
        {
            saveDatas.Add(
                new WeeklySettlementSaveData(
                    weeklySettlements [ i ] ) );
        }

        return saveDatas;
    }
}
