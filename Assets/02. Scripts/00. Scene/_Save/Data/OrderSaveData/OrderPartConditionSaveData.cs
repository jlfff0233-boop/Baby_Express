using System;
using UnityEngine;

/// <summary>
/// 주문 파츠 조건 저장 데이터
/// </summary>
[Serializable]
public class OrderPartConditionSaveData
{
    [SerializeField] string _partId;       //조건 파츠 아이디
    [SerializeField] int _quantity;        //조건 수량

    /// <summary>
    /// 조건 파츠 아이디
    /// </summary>
    public string PartId => _partId;

    /// <summary>
    /// 조건 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 주문 파츠 조건 저장 데이터 생성
    /// </summary>
    public OrderPartConditionSaveData ( string partId, int quantity )
    {
        _partId = partId;
        _quantity = quantity;
    }
}