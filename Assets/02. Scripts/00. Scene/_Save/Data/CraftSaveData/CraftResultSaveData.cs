using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 주문별 확정 제작 결과 저장 데이터
/// </summary>
[Serializable]
public class CraftResultSaveData
{
    [SerializeField] string _orderId;       //주문 아이디
    [SerializeField] int _rootPlacementNumber;       //몸통 배치 번호
    [SerializeField] List<PlacedPartSaveData> _placedParts;       //배치 파츠
    [SerializeField] CraftReviewSaveData _reviewData;       //제작 판정 결과

    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId => _orderId;
    /// <summary>
    /// 몸통 배치 번호
    /// </summary>
    public int RootPlacementNumber => _rootPlacementNumber;
    /// <summary>
    /// 배치 파츠 목록
    /// </summary>
    public IReadOnlyList<PlacedPartSaveData> PlacedParts => _placedParts;
    /// <summary>
    /// 제작 판정 결과
    /// </summary>
    public CraftReviewSaveData ReviewData => _reviewData;

    /// <summary>
    /// 제작 판정 결과 세이브 데이터 생성
    /// </summary>
    /// <param name="result">제작 판정 결과</param>
    public CraftResultSaveData ( CraftResult result )
    {
        _orderId = result.OrderId;
        _rootPlacementNumber = result.RootPlacementNumber;

        _placedParts = new List<PlacedPartSaveData> ( result.PlacedParts.Count );

        for ( int i = 0 ; i < result.PlacedParts.Count ; i++ )
        {
            _placedParts.Add ( new PlacedPartSaveData ( result.PlacedParts [ i ] ) );
        }

        _reviewData = new CraftReviewSaveData ( result.ReviewData );
    }
}