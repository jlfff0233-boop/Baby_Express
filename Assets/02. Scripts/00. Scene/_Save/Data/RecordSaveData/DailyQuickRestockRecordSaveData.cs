using System;
using UnityEngine;

/// <summary>
/// 일일 빠른 재입고 기록 세이브 데이터
/// </summary>
[Serializable]
public class DailyQuickRestockRecordSaveData
{
    [SerializeField] string _itemId;       //재입고 상품 아이디
    [SerializeField] float _fee;       //빠른 재입고 이용료

    /// <summary>
    /// 재입고 상품 아이디
    /// </summary>
    public string ItemId => _itemId;

    /// <summary>
    /// 빠른 재입고 이용료
    /// </summary>
    public float Fee => _fee;

    /// <summary>
    /// 일일 빠른 재입고 기록 세이브 데이터 생성
    /// </summary>
    /// <param name="record">일일 빠른 재입고 기록</param>
    public DailyQuickRestockRecordSaveData ( DailyQuickRestockRecord record )
    {
        _itemId = record.Data.Id;
        _fee = record.Fee;
    }
}
