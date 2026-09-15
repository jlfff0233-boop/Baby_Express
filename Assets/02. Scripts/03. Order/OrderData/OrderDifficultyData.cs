using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 정수 범위(기간 체크용)
/// </summary>
[Serializable]
public class IntRange
{
    [SerializeField] int _min;       //최소값
    [SerializeField] int _max;       //최대값

    /// <summary>
    /// 최소값
    /// </summary>
    public int Min => _min;

    /// <summary>
    /// 최대값
    /// </summary>
    public int Max => _max;

    /// <summary>
    /// 정수 범위 생성
    /// </summary>
    /// <param name="min">최소값</param>
    /// <param name="max">최대값</param>
    public IntRange ( int min, int max )
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// 범위 설정값 확인
    /// </summary>
    /// <param name="minimum">허용 최소값</param>
    /// <returns>사용 가능한 범위 여부</returns>
    public bool IsValid ( int minimum )
    {
        return _min >= minimum && _max >= _min;
    }
}

/// <summary>
/// 주문 난이도 설정 데이터
/// </summary>
[CreateAssetMenu( fileName = "OrderDifficultyData", menuName = "OrderSettings/DifficultyData" )]
public class OrderDifficultyData : ScriptableObject
{
    [Header( "----- 난이도 -----" )]
    [SerializeField] OrderDifficulty _difficulty;       //주문 난이도
    [SerializeField, Min( 1 )] int _weight = 1;       //기본 등장 가중치

    [Header( "----- 주문 기간 -----" )]
    [SerializeField] IntRange _acceptSpan = new IntRange( 1, 2 );       //수락 가능 기간
    [SerializeField] IntRange _deliverySpan = new IntRange( 3, 5 );       //수락 이후 납품 기간
    [SerializeField] IntRange _delaySpan = new IntRange( 1, 2 );       //지연 허용 기간

    [Header( "----- 제작 조건 -----" )]
    [SerializeField] IntRange _craftCostRange = new IntRange( 20, 50 );       //최대 제작 코스트 범위
    [SerializeField] IntRange _conditionCountRange = new IntRange( 1, 2 );       //주요 요구와 희망 개수 범위
    [SerializeField] IntRange _conditionQuantityRange = new IntRange( 1, 3 );       //주요 요구와 희망 수량 범위

    #region ----- 프로퍼티 -----
    public OrderDifficulty Difficulty => _difficulty;       //주문 난이도
    public int Weight => _weight;       //기본 등장 가중치
    public IntRange AcceptSpan => _acceptSpan;      //수락 가능 기간
    public IntRange DeliverySpan => _deliverySpan;      //납품 기간
    public IntRange DelaySpan => _delaySpan;        //지연 허용 기간
    public IntRange CraftCostRange => _craftCostRange;      //최대 제작 코스트 범위
    /// <summary>
    /// 주요 요구와 희망 사항 개수 범위
    /// </summary>
    public IntRange ConditionCountRange => _conditionCountRange;
    /// <summary>
    /// 주요 요구와 희망 사항 수량 범위
    /// </summary>
    public IntRange ConditionQuantityRange => _conditionQuantityRange;
    #endregion

    /// <summary>
    /// 난이도 설정값 확인
    /// </summary>
    /// <returns>사용 가능한 설정 여부</returns>
    public bool IsValid ()
    {
        return _weight > 0 && _acceptSpan.IsValid( 0 )
            && _deliverySpan.IsValid( 1 )
            && _delaySpan.IsValid( 0 )
            && _craftCostRange.IsValid( 0 )
            && _conditionCountRange.IsValid( 0 )
            && _conditionQuantityRange.IsValid( 1 );
    }
}
