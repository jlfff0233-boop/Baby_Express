using UnityEngine;

/// <summary>
/// 배송 등급과 보상 설정 데이터
/// </summary>
[CreateAssetMenu( fileName = "DeliverySettingData", menuName = "DeliverySettings/SettingData" )]
public class DeliverySettingData : ScriptableObject
{
    [Header( "----- 등급 달성률 -----" )]
    [SerializeField, Min( 0f )] float _sGradeRate = 1.1f;       //S등급 최소 달성률
    [SerializeField, Min( 0f )] float _aGradeRate = 0.95f;       //A등급 최소 달성률
    [SerializeField, Min( 0f )] float _bGradeRate = 0.7f;       //B등급 최소 달성률
    [SerializeField, Min( 0f )] float _cGradeRate = 0.4f;       //C등급 최소 달성률

    [Header( "----- 난이도 보정 -----" )]
    [SerializeField, Min( 0f )] float _easyDifficultyRate = 1.2f;       //쉬움 난이도 배율
    [SerializeField, Min( 0f )] float _normalDifficultyRate = 1.3f;       //보통 난이도 배율
    [SerializeField, Min( 0f )] float _hardDifficultyRate = 1.4f;       //어려움 난이도 배율

    [Header( "----- 등급 보정 -----" )]
    [SerializeField, Min( 0f )] float _sGradeRewardRate = 1.5f;       //S등급 보상 배율
    [SerializeField, Min( 0f )] float _aGradeRewardRate = 1.25f;       //A등급 보상 배율
    [SerializeField, Min( 0f )] float _bGradeRewardRate = 1f;       //B등급 보상 배율
    [SerializeField, Min( 0f )] float _cGradeRewardRate = 0.7f;       //C등급 보상 배율
    [SerializeField, Min( 0f )] float _dGradeRewardRate = 0.3f;       //D등급 보상 배율

    [Header( "----- 주문과 배송 보정 -----" )]
    [SerializeField, Min( 0f )] float _normalOrderRate = 1f;       //일반 주문 배율
    [SerializeField, Min( 0f )] float _vipOrderRate = 1.5f;       //VIP 주문 배율
    [SerializeField, Min( 0f )] float _normalDeliveryRate = 1f;       //정상 배송 배율
    [SerializeField, Min( 0f )] float _lateDeliveryRate = 0.5f;       //지연 배송 배율
    [SerializeField, Range( 0f, 1f )] float _requirementFailedRewardRate = 0.8f;       //주요 요구 미달성 지급 상한 비율
    [SerializeField, Min( 1 )] int _rewardRoundUnit = 10;       //최종 보상 반올림 단위

    [Header( "----- 배송 방식 -----" )]
    [SerializeField, Min( 0f )] float _directDeliveryCost;       //기본 직접 배송 고정 비용

    [Header( "----- 배송 기간 -----" )]
    [SerializeField, Min( 0 )] int _minimumDeliveryDays;       //최소 배송 소요일
    [SerializeField, Min( 0 )] int _maximumDeliveryDays = 2;       //최대 배송 소요일

    /// <summary>
    /// 기본 직접 배송 고정 비용
    /// </summary>
    public float DirectDeliveryCost => _directDeliveryCost;

    /// <summary>
    /// 최소 배송 소요일
    /// </summary>
    public int MinimumDeliveryDays => _minimumDeliveryDays;

    /// <summary>
    /// 최대 배송 소요일
    /// </summary>
    public int MaximumDeliveryDays => _maximumDeliveryDays;

    /// <summary>
    /// 최종 보상 반올림 단위
    /// </summary>
    public int RewardRoundUnit => _rewardRoundUnit;

    /// <summary>
    /// 주요 요구 미달성 지급 상한 비율
    /// </summary>
    public float RequirementFailedRewardRate => _requirementFailedRewardRate;


    /// <summary>
    /// 제작 점수 달성률에 맞는 배송 등급 조회
    /// </summary>
    /// <param name="achievementRate">주문 기준점수 달성률</param>
    /// <returns>배송 등급</returns>
    public DeliveryGrade GetGrade ( float achievementRate )
    {
        if ( achievementRate >= _sGradeRate ) return DeliveryGrade.S;
        if ( achievementRate >= _aGradeRate ) return DeliveryGrade.A;
        if ( achievementRate >= _bGradeRate ) return DeliveryGrade.B;
        if ( achievementRate >= _cGradeRate ) return DeliveryGrade.C;

        return DeliveryGrade.D;
    }

    /// <summary>
    /// 주문 난이도 보정 배율 조회
    /// </summary>
    /// <param name="difficulty">주문 난이도</param>
    /// <returns>난이도 보정 배율</returns>
    public float GetDifficultyRate ( OrderDifficulty difficulty )
    {
        switch ( difficulty )
        {
            case OrderDifficulty.Easy:
                return _easyDifficultyRate;

            case OrderDifficulty.Normal:
                return _normalDifficultyRate;

            case OrderDifficulty.Hard:
                return _hardDifficultyRate;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// 배송 등급 보정 배율 조회
    /// </summary>
    /// <param name="grade">배송 등급</param>
    /// <returns>등급 보정 배율</returns>
    public float GetGradeRate ( DeliveryGrade grade )
    {
        switch ( grade )
        {
            case DeliveryGrade.S:
                return _sGradeRewardRate;

            case DeliveryGrade.A:
                return _aGradeRewardRate;

            case DeliveryGrade.B:
                return _bGradeRewardRate;

            case DeliveryGrade.C:
                return _cGradeRewardRate;

            case DeliveryGrade.D:
                return _dGradeRewardRate;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// VIP 주문 보정 배율 조회
    /// </summary>
    /// <param name="specialType">특수 주문 종류</param>
    /// <returns>VIP 주문 보정 배율</returns>
    public float GetVipRate ( OrderSpecialType specialType )
    {
        switch ( specialType )
        {
            case OrderSpecialType.None:
                return _normalOrderRate;

            case OrderSpecialType.Vip:
                return _vipOrderRate;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// 배송 결과 보정 배율 조회
    /// </summary>
    /// <param name="outcome">주문 결과</param>
    /// <returns>배송 결과 보정 배율</returns>
    public float GetDeliveryRate ( OrderOutcome outcome )
    {
        switch ( outcome )
        {
            case OrderOutcome.NormalDelivery:
                return _normalDeliveryRate;

            case OrderOutcome.LateDelivery:
                return _lateDeliveryRate;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// 배송 설정 유효성 확인
    /// </summary>
    /// <returns>설정 사용 가능 여부</returns>
    public bool IsValid ()
    {
        //등급 달성률 순서 확인
        if ( _sGradeRate <= _aGradeRate ||
            _aGradeRate <= _bGradeRate ||
            _bGradeRate <= _cGradeRate ||
            _cGradeRate < 0f )
            return false;

        //보상 배율과 반올림 단위 확인
        return _minimumDeliveryDays >= 0 &&
            _maximumDeliveryDays >= _minimumDeliveryDays &&
            _easyDifficultyRate > 0f &&
            _normalDifficultyRate > 0f &&
            _hardDifficultyRate > 0f &&
            _sGradeRewardRate > 0f &&
            _aGradeRewardRate > 0f &&
            _bGradeRewardRate > 0f &&
            _cGradeRewardRate > 0f &&
            _dGradeRewardRate > 0f &&
            _normalOrderRate > 0f &&
            _vipOrderRate > 0f &&
            _normalDeliveryRate > 0f &&
            _lateDeliveryRate > 0f &&
            _directDeliveryCost >= 0f &&
            float.IsNaN( _directDeliveryCost ) == false &&
            float.IsInfinity( _directDeliveryCost ) == false &&
            _requirementFailedRewardRate >= 0f &&
            _requirementFailedRewardRate < 1f &&
            _rewardRoundUnit > 0;

    }
}
