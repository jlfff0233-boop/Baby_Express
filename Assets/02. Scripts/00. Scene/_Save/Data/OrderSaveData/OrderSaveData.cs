using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 고객 주문 모델 저장 데이터
/// </summary>
[Serializable]
public class OrderSaveData
{
    [SerializeField] int _waitingLimit;      //수락 대기 제한
    [SerializeField] int _nextCreatedNumber;        //다음 주문 생성 번호

    [SerializeField] OrderPenaltyType _penaltyType;     //주문량 페널티 원인
    [SerializeField] int _orderReduction;       //일일 주문 감소량
    [SerializeField] int _penaltyRemainingDays;     //페널티 남은 기간

    [SerializeField] List<CustomerOrderSaveData> _orders;       //전체 주문 목록

    /// <summary>
    /// 수락 대기 제한
    /// </summary>
    public int WaitingLimit => _waitingLimit;

    /// <summary>
    /// 다음 주문 생성 번호
    /// </summary>
    public int NextCreatedNumber => _nextCreatedNumber;

    /// <summary>
    /// 주문량 페널티 타입
    /// </summary>
    public OrderPenaltyType PenaltyType => _penaltyType;

    /// <summary>
    /// 일일 주문 감소량
    /// </summary>
    public int OrderReduction => _orderReduction;

    /// <summary>
    /// 페널티 남은 기간
    /// </summary>
    public int PenaltyRemainingDays => _penaltyRemainingDays;

    /// <summary>
    /// 전체 주문 목록
    /// </summary>
    public IReadOnlyList<CustomerOrderSaveData> Orders => _orders;


    /// <summary>
    /// 고객 주문 모델 저장 데이터 생성
    /// </summary>
    /// <param name="waitingLimit">대기 제한</param>
    /// <param name="nextCreatedNumber">다음 날 주문 생성 수</param>
    /// <param name="penaltyType">페널티 타입</param>
    /// <param name="orderReduction">주문 감소량</param>
    /// <param name="penaltyRemainingDays">페널티 남은 기간</param>
    /// <param name="orders">전체 주문 목록</param>
    public OrderSaveData (
        int waitingLimit, int nextCreatedNumber,
        OrderPenaltyType penaltyType,
        int orderReduction, int penaltyRemainingDays,
        IReadOnlyCollection<CustomerOrderSaveData> orders )
    {
        _waitingLimit = waitingLimit;
        _nextCreatedNumber = nextCreatedNumber;

        _penaltyType = penaltyType;
        _orderReduction = orderReduction;
        _penaltyRemainingDays = penaltyRemainingDays;

        _orders = new List<CustomerOrderSaveData>( orders );
    }
}