using System;
using UnityEngine;

/// <summary>
/// 주문 생성 모델 저장 데이터
/// </summary>
[Serializable]
public class OrderGeneratorSaveData
{
    [SerializeField] bool _isApplied;       //주간 보정 적용 여부
    [SerializeField] int _orderCountCorrection;      //일일 주문 수 보정
    [SerializeField] int _easyWeight;       //쉬움 난이도 가중치
    [SerializeField] int _normalWeight;     //보통 난이도 가중치
    [SerializeField] int _hardWeight;       //어려움 난이도 가중치

    /// <summary>
    /// 주간 보정 적용 여부
    /// </summary>
    public bool IsApplied => _isApplied;

    /// <summary>
    /// 일일 주문 수 보정
    /// </summary>
    public int OrderCountCorrection => _orderCountCorrection;

    /// <summary>
    /// 쉬움 난이도 가중치
    /// </summary>
    public int EasyWeight => _easyWeight;

    /// <summary>
    /// 보통 난이도 가중치
    /// </summary>
    public int NormalWeight => _normalWeight;

    /// <summary>
    /// 어려움 난이도 가중치
    /// </summary>
    public int HardWeight => _hardWeight;

    /// <summary>
    /// 주문 생성 모델 저장 데이터 생성
    /// </summary>
    /// <param name="isApplied">주간 보정 적용 여부</param>
    /// <param name="orderCountCorrection">일일 주문 수 보정</param>
    /// <param name="easyWeight">쉬움 난이도 가중치</param>
    /// <param name="normalWeight">중간 난이도 가중치</param>
    /// <param name="hardWeight">어려움 난이도 가중치</param>
    public OrderGeneratorSaveData (
        bool isApplied, int orderCountCorrection,
        int easyWeight, int normalWeight, int hardWeight )
    {
        _isApplied = isApplied;
        _orderCountCorrection = orderCountCorrection;
        _easyWeight = easyWeight;
        _normalWeight = normalWeight;
        _hardWeight = hardWeight;
    }
}