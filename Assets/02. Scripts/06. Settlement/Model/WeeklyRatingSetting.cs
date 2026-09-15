using System;
using UnityEngine;

/// <summary>
/// 주간 평가별 주문 생성 설정
/// </summary>
[Serializable]
public class WeeklyRatingSetting
{
    [SerializeField] WeeklyRating _rating;       //주간 영업 평가
    [SerializeField, Min( 0f )] float _minimumScore;       //최소 평가 점수
    [SerializeField] int _orderCountCorrection;       //일일 주문 수 보정
    [SerializeField, Min( 0 )] int _easyWeight;       //쉬움 난이도 가중치
    [SerializeField, Min( 0 )] int _normalWeight;       //보통 난이도 가중치
    [SerializeField, Min( 0 )] int _hardWeight;       //어려움 난이도 가중치

    /// <summary>
    /// 주간 영업 평가
    /// </summary>
    public WeeklyRating Rating => _rating;

    /// <summary>
    /// 최소 평가 점수
    /// </summary>
    public float MinimumScore => _minimumScore;

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
}