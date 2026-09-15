using UnityEngine;

/// <summary>
/// 특수 주문 생성 설정 데이터
/// </summary>
[CreateAssetMenu( fileName = "OrderSpecialSettingsData", menuName = "OrderSettings/SpecialSettingsData" )]
public class OrderSpecialSettingsData : ScriptableObject
{
    [Header( "----- 등장 가중치 -----" )]
    [SerializeField, Min( 1 )] int _regularWeight;       //일반 주문 가중치
    [SerializeField, Min( 1 )] int _vipWeight = 3;       //VIP 주문 가중치

    [Header( "----- VIP 해금 -----" )]
    [SerializeField, Min( 1 )] int _vipRequiredGradeCount = 50;       //필요 B등급 이상 평가 누적 수

    [Header( "----- 특수 조건 가중치 -----" )]
    [SerializeField, Min( 0 )] int _noConditionWeight = 1;       //특수 조건 없음 가중치
    [SerializeField, Min( 0 )] int _partCountWeight = 1;       //전체 파츠 개수 제한 가중치
    [SerializeField, Min( 0 )] int _targetThemeWeight = 1;       //목표 테마 가중치
    [SerializeField, Min( 0 )] int _excludedThemeWeight = 1;       //제외 테마 가중치

    [Header( "----- 전체 파츠 개수 제한 -----" )]
    [SerializeField] IntRange _partCountBufferRange = new IntRange( 0, 2 );       //최소 해결 수량에 더할 여유 범위

    /// <summary>
    /// 일반 주문 가중치
    /// </summary>
    public int RegularWeight => _regularWeight;

    /// <summary>
    /// VIP 주문 가중치
    /// </summary>
    public int VipWeight => _vipWeight;

    /// <summary>
    /// VIP 해금에 필요한 B등급 이상 평가 누적 수
    /// </summary>
    public int VipRequiredGradeCount => _vipRequiredGradeCount;

    /// <summary>
    /// 특수 조건 없음 가중치
    /// </summary>
    public int NoConditionWeight => _noConditionWeight;

    /// <summary>
    /// 전체 파츠 개수 제한 가중치
    /// </summary>
    public int PartCountWeight => _partCountWeight;

    /// <summary>
    /// 목표 테마 가중치
    /// </summary>
    public int TargetThemeWeight => _targetThemeWeight;

    /// <summary>
    /// 제외 테마 가중치
    /// </summary>
    public int ExcludedThemeWeight => _excludedThemeWeight;

    /// <summary>
    /// 최소 해결 수량에 더할 여유 범위
    /// </summary>
    public IntRange PartCountBufferRange => _partCountBufferRange;

    /// <summary>
    /// 특수 주문 설정값 확인
    /// </summary>
    /// <returns>사용 가능한 설정 여부</returns>
    public bool IsValid ()
    {
        int conditionWeight = _noConditionWeight + _partCountWeight +
            _targetThemeWeight + _excludedThemeWeight;

        return _regularWeight > 0 && _vipWeight > 0 &&
            _vipRequiredGradeCount > 0 && conditionWeight > 0 &&
            _partCountBufferRange != null &&
            _partCountBufferRange.Min >= 0 &&
            _partCountBufferRange.Max >= _partCountBufferRange.Min;
    }
}
