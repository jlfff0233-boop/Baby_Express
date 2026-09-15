using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주간 결산 평가 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "Settlement/WeeklySettingData" )]
public class WeeklySettlementSettingData : ScriptableObject
{
    [Header( "----- 평가 기준 -----" )]
    [SerializeField, Min( 1 )] int _minimumOrderCount = 7;       //평가 최소 주문 수
    [SerializeField, Min( 0 )] int _sGradeScore = 100;       //S등급 점수
    [SerializeField, Min( 0 )] int _aGradeScore = 85;       //A등급 점수
    [SerializeField, Min( 0 )] int _bGradeScore = 70;       //B등급 점수
    [SerializeField, Min( 0 )] int _cGradeScore = 45;       //C등급 점수
    [SerializeField, Min( 0 )] int _dGradeScore = 20;       //D등급 점수

    [Header( "----- 주문 결과 -----" )]
    [SerializeField, Range( 0f, 1f )] float _lateDeliveryRate = 0.7f;       //지연 배송 점수 배율
    [SerializeField, Min( 0 )] int _rejectedScore = 35;       //직접 거절 점수
    [SerializeField, Min( 0 )] int _autoRejectedScore = 25;       //자동 거절 점수
    [SerializeField, Min( 0 )] int _cancelledScore = 10;       //주문 취소 점수
    [SerializeField, Min( 0 )] int _failedScore;       //최종 실패 점수

    [Header( "----- 다음 주 보정 -----" )]
    [SerializeField] WeeklyRatingSetting [ ] _ratingSettings;       //평가별 주문 생성 설정

    /// <summary>
    /// 평가 최소 주문 수
    /// </summary>
    public int MinimumOrderCount => _minimumOrderCount;

    /// <summary>
    /// 지연 배송 점수 배율
    /// </summary>
    public float LateDeliveryRate => _lateDeliveryRate;

    /// <summary>
    /// 직접 거절 점수
    /// </summary>
    public int RejectedScore => _rejectedScore;

    /// <summary>
    /// 자동 거절 점수
    /// </summary>
    public int AutoRejectedScore => _autoRejectedScore;

    /// <summary>
    /// 주문 취소 점수
    /// </summary>
    public int CancelledScore => _cancelledScore;

    /// <summary>
    /// 최종 실패 점수
    /// </summary>
    public int FailedScore => _failedScore;

    /// <summary>
    /// 평가별 주문 생성 설정
    /// </summary>
    public IReadOnlyList<WeeklyRatingSetting> RatingSettings => _ratingSettings;

    /// <summary>
    /// 배송 등급 점수 반환
    /// </summary>
    /// <param name="grade">배송 평가 등급</param>
    /// <returns>배송 등급 점수</returns>
    public int GetGradeScore ( DeliveryGrade grade )
    {
        switch ( grade )
        {
            case DeliveryGrade.S: return _sGradeScore;
            case DeliveryGrade.A: return _aGradeScore;
            case DeliveryGrade.B: return _bGradeScore;
            case DeliveryGrade.C: return _cGradeScore;
            case DeliveryGrade.D: return _dGradeScore;
            default: return 0;
        }
    }
}