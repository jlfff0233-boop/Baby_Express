using System;
using UnityEngine;

/// <summary>
/// 일일 종료 주문 기록 세이브 데이터
/// </summary>
[Serializable]
public class DailyOrderRecordSaveData
{
    [SerializeField] string _orderId;       //주문 아이디
    [SerializeField] OrderOutcome _outcome;       //주문 종료 결과
    [SerializeField] bool _hasEvaluation;       //배송 평가 포함 여부
    [SerializeField] DeliveryGrade _grade;       //배송 평가 등급
    [SerializeField] DeliveryMethod _deliveryMethod;       //배송 방식
    [SerializeField] string _employeeId;       //배송 담당 직원 아이디
    [SerializeField] bool _isDeadlineDayNormalDelivery;       //마감일 정상 배송 여부
    [SerializeField] int _completedRequirementCount;       //달성 주요 요구 사항 수
    [SerializeField] int _requirementCount;       //전체 주요 요구 수
    [SerializeField] int _completedWishCount;       //달성 희망 사항 수
    [SerializeField] int _wishCount;       //전체 희망 사항 수
    [SerializeField] int _orderReward;       //최종 주문 대금

    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId => _orderId;
    /// <summary>
    /// 주문 종료 결과
    /// </summary>
    public OrderOutcome Outcome => _outcome;
    /// <summary>
    /// 배송 평가 포함 여부
    /// </summary>
    public bool HasEvaluation => _hasEvaluation;
    /// <summary>
    /// 배송 평가 등급
    /// </summary>
    public DeliveryGrade Grade => _grade;
    /// <summary>
    /// 배송 방법
    /// </summary>
    public DeliveryMethod DeliveryMethod => _deliveryMethod;
    /// <summary>
    /// 직원 아이디
    /// </summary>
    public string EmployeeId => _employeeId;
    /// <summary>
    /// 정상 배송 여부
    /// </summary>
    public bool IsDeadlineDayNormalDelivery => _isDeadlineDayNormalDelivery;
    /// <summary>
    /// 달성 주요 요구 사항 수
    /// </summary>
    public int CompletedRequirementCount => _completedRequirementCount;
    /// <summary>
    /// 전체 주요 요구 사항 수
    /// </summary>
    public int RequirementCount => _requirementCount;
    /// <summary>
    /// 달성 희망 사항 수
    /// </summary>
    public int CompletedWishCount => _completedWishCount;
    /// <summary>
    /// 전체 희망 사항 수
    /// </summary>
    public int WishCount => _wishCount;
    /// <summary>
    /// 주문 대금
    /// </summary>
    public int OrderReward => _orderReward;

    /// <summary>
    /// 일일 종료 주문 기록 세이브 데이터 생성
    /// </summary>
    /// <param name="record">일일 종료 주문 기록</param>
    public DailyOrderRecordSaveData ( DailyOrderRecord record )
    {
        _orderId = record.OrderId;
        _outcome = record.Outcome;

        _hasEvaluation = record.HasEvaluation;
        _grade = record.Grade;

        _deliveryMethod = record.DeliveryMethod;
        _employeeId = record.EmployeeId;
        _isDeadlineDayNormalDelivery = record.IsDeadlineDayNormalDelivery;

        _completedRequirementCount = record.CompletedRequirementCount;
        _requirementCount = record.RequirementCount;

        _completedWishCount = record.CompletedWishCount;
        _wishCount = record.WishCount;
        _orderReward = record.OrderReward;
    }
}
