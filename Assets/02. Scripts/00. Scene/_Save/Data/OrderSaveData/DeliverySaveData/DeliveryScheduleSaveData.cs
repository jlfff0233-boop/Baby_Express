using System;
using UnityEngine;

/// <summary>
/// 배송 일정 세이브 데이터
/// </summary>
[Serializable]
public class DeliveryScheduleSaveData
{
    [SerializeField] string _orderId;       //주문 아이디
    [SerializeField] int _deliveryDays;     //배송 소요일
    [SerializeField] int _departureTotalDay;        //배송 출발일(누적 영업일 기준)
    [SerializeField] int _arrivalTotalDay;      //도착 예정일(누적 영업일 기준)
    [SerializeField] DeliveryMethod _method;        //배송 방법
    [SerializeField] string _employeeId;        //직원 아이디
    [SerializeField] float _deliveryCost;       //배송비

    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId => _orderId;
    /// <summary>
    /// 배송 소요일
    /// </summary>
    public int DeliveryDays => _deliveryDays;
    /// <summary>
    /// 배송 출발일(누적 영업일 기준)
    /// </summary>
    public int DepartureTotalDay => _departureTotalDay;
    /// <summary>
    /// 도착 예정일(누적 영업일 기준)
    /// </summary>
    public int ArrivalTotalDay => _arrivalTotalDay;
    /// <summary>
    /// 배송 방법
    /// </summary>
    public DeliveryMethod Method => _method;
    /// <summary>
    /// 직원 아이디
    /// </summary>
    public string EmployeeId => _employeeId;
    /// <summary>
    /// 배송비
    /// </summary>
    public float DeliveryCost => _deliveryCost;


    /// <summary>
    /// 배송 일정 세이브 데이터 생성
    /// </summary>
    /// <param name="schedule">주문별 배송 일정</param>
    public DeliveryScheduleSaveData ( DeliverySchedule schedule )
    {
        _orderId = schedule.OrderId;
        _deliveryDays = schedule.DeliveryDays;
        _departureTotalDay = schedule.DepartureTotalDay;
        _arrivalTotalDay = schedule.ArrivalTotalDay;
        _method = schedule.Method;
        _employeeId = schedule.EmployeeId;
        _deliveryCost = schedule.DeliveryCost;
    }
}