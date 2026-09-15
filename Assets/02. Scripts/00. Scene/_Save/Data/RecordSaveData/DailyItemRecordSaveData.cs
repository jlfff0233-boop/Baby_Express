using System;
using UnityEngine;

/// <summary>
/// 일일 상품 처리 기록 세이브 데이터
/// </summary>
[Serializable]
public class DailyItemRecordSaveData
{
    [SerializeField] string _itemId;       //상품 아이디
    [SerializeField] int _quantity;       //처리 수량
    [SerializeField] float _priceTotal;       //상품별 총금액

    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string ItemId => _itemId;

    /// <summary>
    /// 처리 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 상품별 총금액
    /// </summary>
    public float PriceTotal => _priceTotal;

    /// <summary>
    /// 일일 상품 처리 기록 세이브 데이터 생성
    /// </summary>
    /// <param name="record">일일 상품 처리 기록</param>
    public DailyItemRecordSaveData ( DailyItemRecord record )
    {
        _itemId = record.Data.Id;
        _quantity = record.Quantity;
        _priceTotal = record.PriceTotal;
    }
}
