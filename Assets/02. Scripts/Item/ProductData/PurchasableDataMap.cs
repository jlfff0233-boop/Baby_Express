using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 구매 가능 상품 데이터 목록 관리
/// </summary>
[CreateAssetMenu( fileName = "PurchasableDataMap", menuName = "PurchaseSettings/PurchasableDataMap" )]
public class PurchasableDataMap : ScriptableObject
{
    [Header( "----- 데이터 -----" )]
    [SerializeField] PurchasableData [ ] _datas;        //상품 데이터 배열

    /// <summary>
    /// 읽기 전용 구매 가능 상품 목록
    /// </summary>
    public IReadOnlyList<PurchasableData> PurchasableDatas => _datas;


    /// <summary>
    /// 상품 아이디로 구매 가능 상품 데이터 조회
    /// </summary>
    /// <param name="id">조회할 상품 아이디</param>
    /// <param name="data">조회한 상품 데이터</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetData ( string id, out PurchasableData data )
    {
        data = null;

        //아이디가 이상하거나 데이터가 없으면 종료
        if ( string.IsNullOrWhiteSpace( id ) == true || _datas == null )
            return false;


        //같은 아이디를 가진 상품 데이터 조회
        for ( int i = 0; i < _datas.Length; i++ )
        {
            PurchasableData currentData = _datas [ i ];

            if ( currentData != null && currentData.Id == id )
            {
                data = currentData;
                return true;
            }
        }

        return false;
    }
}
