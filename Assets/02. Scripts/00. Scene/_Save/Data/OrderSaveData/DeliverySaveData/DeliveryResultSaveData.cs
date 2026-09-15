using System;
using UnityEngine;

/// <summary>
/// 배송 결과 세이브 데이터
/// </summary>
[Serializable]
public class DeliveryResultSaveData
{
    [SerializeField] bool _isMethodConfirmed;       //배송 방식 확정 여부
    [SerializeField] string _orderId;       //주문 아이디
    [SerializeField] int _departureTotalDay;        //배송 출발일
    [SerializeField] int _arrivalTotalDay;      //도착 예정일

    [SerializeField] OrderOutcome _outcome;     //배송 결과
    [SerializeField] float _craftScore;     //제작 점수
    [SerializeField] float _standardScore;      //주문 기준 점수
    [SerializeField] float _achievementRate;        //주문 기준 점수 달성률
    [SerializeField] DeliveryGrade _grade;      //배송 평가 등급

    [SerializeField] float _baseOrderReward;        //기본 주문 대금
    [SerializeField] float _basePriceTotal;     //사용 파츠 기본 가격 합계
    [SerializeField] int _requirementRewardLimit;       //주요 요구 미달성으로 인한 주문 대금 상환

    [SerializeField] float _difficultyRate;     //난이도 보정 배율
    [SerializeField] float _gradeRate;      //등급 보정 배율
    [SerializeField] float _vipRate;        //vip 보정 배율
    [SerializeField] float _deliveryRate;       //배송 보정 배율
    [SerializeField] int _finalOrderReward;     //최종 주문 대금

    [SerializeField] DeliveryMethod _method;        //배송 방식
    [SerializeField] string _employeeId;        //직원 아이디
    [SerializeField] float _deliveryCost;       //배송비

    /// <summary>
    /// 배송 방식 확정 여부
    /// </summary>
    public bool IsMethodConfirmed => _isMethodConfirmed;
    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId => _orderId;
    /// <summary>
    /// 배송 출발일
    /// </summary>
    public int DepartureTotalDay => _departureTotalDay;
    /// <summary>
    /// 도착 예정일
    /// </summary>
    public int ArrivalTotalDay => _arrivalTotalDay;
    /// <summary>
    /// 배송 결과
    /// </summary>
    public OrderOutcome Outcome => _outcome;
    /// <summary>
    /// 제작 점수
    /// </summary>
    public float CraftScore => _craftScore;
    /// <summary>
    /// 주문 기준 점수
    /// </summary>
    public float StandardScore => _standardScore;
    /// <summary>
    /// 주문 기준 점수 달성률
    /// </summary>
    public float AchievementRate => _achievementRate;
    /// <summary>
    /// 배송 평가 등급
    /// </summary>
    public DeliveryGrade Grade => _grade;
    /// <summary>
    /// 기본 주문 대금
    /// </summary>
    public float BaseOrderReward => _baseOrderReward;
    /// <summary>
    /// 사용 파츠 기본 가격 합계
    /// </summary>
    public float BasePriceTotal => _basePriceTotal;
    /// <summary>
    /// 주문 대금 상환(요구 사항 미달성)
    /// </summary>
    public int RequirementRewardLimit => _requirementRewardLimit;
    /// <summary>
    /// 난이도 보정 배율
    /// </summary>
    public float DifficultyRate => _difficultyRate;
    /// <summary>
    /// 등급 보정 배율
    /// </summary>
    public float GradeRate => _gradeRate;
    /// <summary>
    /// vip 보정 배율
    /// </summary>
    public float VipRate => _vipRate;
    /// <summary>
    /// 배송 평가 보정치
    /// </summary>
    public float DeliveryRate => _deliveryRate;
    /// <summary>
    /// 최종 주문 재금
    /// </summary>
    public int FinalOrderReward => _finalOrderReward;
    /// <summary>
    /// 배송 방식
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
    /// 배송 결과 세이브 데이터 생성
    /// </summary>
    /// <param name="result">배송 결과</param>
    public DeliveryResultSaveData ( DeliveryResult result )
    {
        _isMethodConfirmed = result.IsMethodConfirmed;
        _orderId = result.OrderId;
        _departureTotalDay = result.DepartureTotalDay;
        _arrivalTotalDay = result.ArrivalTotalDay;

        _outcome = result.Outcome;
        _craftScore = result.CraftScore;
        _standardScore = result.StandardScore;
        _achievementRate = result.AchievementRate;
        _grade = result.Grade;

        _baseOrderReward = result.BaseOrderReward;
        _basePriceTotal = result.BasePriceTotal;
        _requirementRewardLimit = result.RequirementRewardLimit;

        _difficultyRate = result.DifficultyRate;
        _gradeRate = result.GradeRate;
        _vipRate = result.VipRate;
        _deliveryRate = result.DeliveryRate;
        _finalOrderReward = result.FinalOrderReward;

        _method = result.Method;
        _employeeId = result.EmployeeId;
        _deliveryCost = result.DeliveryCost;
    }
}