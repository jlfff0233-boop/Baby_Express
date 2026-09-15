using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 상태와 Sprite 연결 데이터
/// </summary>
[Serializable]
public class OrderIconEntry
{
    [SerializeField] OrderProgressState _progressState;       //주문 진행 상태
    [SerializeField] Sprite _sprite;       //상태 표시 아이콘

    /// <summary>
    /// 주문 진행 상태
    /// </summary>
    public OrderProgressState ProgressState => _progressState;

    /// <summary>
    /// 상태 표시 아이콘
    /// </summary>
    public Sprite Sprite => _sprite;
}

/// <summary>
/// 주문 공용 상태 아이콘 데이터
/// </summary>
[CreateAssetMenu ( fileName = "OrderIconData" , menuName = "Order/IconData" )]
public class OrderIconData : ScriptableObject
{
    [SerializeField]
    List<OrderIconEntry> _icons = new List<OrderIconEntry> ( );       //상태별 아이콘 목록

    Dictionary<OrderProgressState , Sprite> _iconMap;       //상태 아이콘 조회 목록

    /// <summary>
    /// 주문 진행 상태에 대응하는 Sprite 반환
    /// </summary>
    /// <param name="progressState">주문 진행 상태</param>
    /// <returns>상태 표시 아이콘</returns>
    public Sprite GetIcon ( OrderProgressState progressState )
    {
        if ( _iconMap == null )
            CreateIconMap ( );

        _iconMap.TryGetValue ( progressState , out Sprite icon );

        return icon;
    }

    /// <summary>
    /// 주문 상태별 아이콘 조회 목록 구성
    /// </summary>
    void CreateIconMap ( )
    {
        _iconMap =
            new Dictionary<OrderProgressState , Sprite> ( );

        for ( int i = 0 ; i < _icons.Count ; i++ )
        {
            OrderIconEntry entry = _icons [ i ];

            _iconMap [ entry.ProgressState ] = entry.Sprite;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Inspector 데이터 변경 시 조회 목록 초기화
    /// </summary>
    void OnValidate ( )
    {
        _iconMap = null;
    }
#endif
}