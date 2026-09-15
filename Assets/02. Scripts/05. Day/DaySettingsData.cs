using UnityEngine;

/// <summary>
/// 영업일과 빠른 재입고의 기본 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "DaySettings/DaySettingsData" )]
public class DaySettingsData : ScriptableObject
{
    [Header( "----- 영업일 설정 -----" )]
    [SerializeField, Min( 1 )] int _craftLimit = 2;       //기본 제작 할당량

    [Header( "----- 빠른 재입고 설정 -----" )]
    [SerializeField, Min( 1 )] int _quickRestockLimit = 3;       //일일 최대 횟수
    [SerializeField, Min( 0.01f )] float _quickRestockFeeRate = 1.5f;       //이용료 배율

    /// <summary>
    /// 기본 일일 제작 할당량
    /// </summary>
    public int CraftLimit => _craftLimit;

    /// <summary>
    /// 일일 빠른 재입고 최대 횟수
    /// </summary>
    public int QuickRestockLimit => _quickRestockLimit;

    /// <summary>
    /// 빠른 재입고 이용료 배율
    /// </summary>
    public float QuickRestockFeeRate => _quickRestockFeeRate;
}