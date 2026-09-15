using UnityEngine;

/// <summary>
/// 새 게임 시작 시 사용할 플레이 초기 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "PlaySettings/PlayStartSettingsData" )]
public class PlayStartSettingsData : ScriptableObject
{
    [Header( "----- 플레이 초기값 -----" )]
    [SerializeField, Min( 0f )] float _time;       //시작 시간
    [SerializeField, Min( 1 )] int _totalDay = 1;       //시작 누적 영업일
    [SerializeField, Min( 0 )] int _highGradeEvaluationCount;       //시작 평가 누적 수
    [SerializeField, Min( 0f )] float _budget;       //시작 자금

    [Header( "----- 초기 해금 -----" )]
    [SerializeField] PurchasableDataMap _unlockedProductMap;       //초기 해금 상품 데이터 맵

    /// <summary>
    /// 시작 시간
    /// </summary>
    public float Time => _time;

    /// <summary>
    /// 시작 누적 영업일
    /// </summary>
    public int TotalDay => _totalDay;

    /// <summary>
    /// 시작 B등급 이상 평가 누적 수
    /// </summary>
    public int HighGradeEvaluationCount =>
        _highGradeEvaluationCount;

    /// <summary>
    /// 시작 자금
    /// </summary>
    public float Budget => _budget;

    /// <summary>
    /// 초기 해금 상품 데이터 맵
    /// </summary>
    public PurchasableDataMap UnlockedProductMap =>
        _unlockedProductMap;
}
