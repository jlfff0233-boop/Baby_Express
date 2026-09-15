using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배송 일정과 결과 저장 데이터
/// </summary>
[Serializable]
public class DeliverySaveData
{
    [SerializeField] int _deliverySpanReduction;       //배송 소요일 단축값
    [SerializeField] List<DeliveryScheduleSaveData> _schedules;     //배송 일정 세이브 데이터 목록
    [SerializeField] List<DeliveryResultSaveData> _results;     //배송 결과 세이브 데이터 목록

    /// <summary>
    /// 배송 소요일 단축값
    /// </summary>
    public int DeliverySpanReduction => _deliverySpanReduction;
    /// <summary>
    /// 배송 일정 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<DeliveryScheduleSaveData> Schedules => _schedules;
    /// <summary>
    /// 배송 결과 세이브 데이터 목록
    /// </summary>
    public IReadOnlyList<DeliveryResultSaveData> Results => _results;

    /// <summary>
    /// 배송 일정과 결과 세이브 데이터 생성
    /// </summary>
    /// <param name="deliverySpanReduction">배송 소요일 단축값</param>
    /// <param name="schedules">배송 일정 세이브 데이터 목록</param>
    /// <param name="results">배송 결과 세이브 데이터 목록</param>
    public DeliverySaveData (
        int deliverySpanReduction,
        IReadOnlyCollection<DeliveryScheduleSaveData> schedules,
        IReadOnlyCollection<DeliveryResultSaveData> results )
    {
        _deliverySpanReduction = deliverySpanReduction;
        _schedules =
            new List<DeliveryScheduleSaveData>( schedules );
        _results =
            new List<DeliveryResultSaveData>( results );
    }
}