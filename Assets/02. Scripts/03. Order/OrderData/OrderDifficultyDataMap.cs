using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 주문 난이도 데이터 목록과 난이도별 조회 관리
/// </summary>
[CreateAssetMenu( menuName = "OrderSettings/DifficultyDataMap" )]
public class OrderDifficultyDataMap : ScriptableObject
{
    [Header( "----- 주문 난이도 데이터 -----" )]
    [SerializeField] OrderDifficultyData [ ] _datas;       //전체 주문 난이도 데이터

    List<OrderDifficultyData> _validDatas =
        new List<OrderDifficultyData>( );       //사용 가능한 주문 난이도 데이터

    Dictionary<OrderDifficulty, OrderDifficultyData> _dataMap =
        new Dictionary<OrderDifficulty, OrderDifficultyData>( );       //난이도별 데이터 조회 맵

    /// <summary>
    /// 사용 가능한 주문 난이도 데이터 목록
    /// </summary>
    public IReadOnlyList<OrderDifficultyData> Datas => _validDatas;

    /// <summary>
    /// 주문 난이도 데이터 조회 맵 초기화
    /// </summary>
    void OnEnable ()
    {
        CreateDataMap( );
    }

    /// <summary>
    /// 지정 난이도의 주문 설정 데이터 조회
    /// </summary>
    /// <param name="difficulty">조회할 주문 난이도</param>
    /// <param name="data">조회한 주문 난이도 데이터</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetData (
        OrderDifficulty difficulty, out OrderDifficultyData data )
    {
        return _dataMap.TryGetValue( difficulty, out data );
    }

    /// <summary>
    /// 사용 가능한 주문 난이도 데이터 조회 맵 생성
    /// </summary>
    void CreateDataMap ()
    {
        _validDatas.Clear( );
        _dataMap.Clear( );

        for ( int i = 0; i < _datas.Length; i++ )
        {
            OrderDifficultyData data = _datas [ i ];

            if ( data == null )
            {
                Debug.LogError(
                    "주문 난이도 데이터 목록에 빈 항목이 있습니다.", this );
                continue;
            }

            if ( _dataMap.TryAdd( data.Difficulty, data ) == false )
            {
                Debug.LogError(
                    $"주문 난이도 데이터가 중복되었습니다: {data.Difficulty}",
                    data );
                continue;
            }

            _validDatas.Add( data );
        }
    }
}
