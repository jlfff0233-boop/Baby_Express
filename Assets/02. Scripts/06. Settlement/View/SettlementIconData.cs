using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 결산 아이콘 종류와 Sprite 연결 데이터
/// </summary>
[Serializable]
public class SettlementIconEntry
{
    [SerializeField] SettlementIconType _iconType;       //결산 아이콘 종류
    [SerializeField] Sprite _sprite;       //실제 표시 아이콘

    /// <summary>
    /// 결산 아이콘 종류
    /// </summary>
    public SettlementIconType IconType => _iconType;

    /// <summary>
    /// 실제 표시 아이콘
    /// </summary>
    public Sprite Sprite => _sprite;
}

/// <summary>
/// 결산 공용 아이콘 데이터 - 결산과 가계부의 아이콘 원본 관리
/// </summary>
[CreateAssetMenu (
    fileName = "SettlementIconData" , menuName = "Settlement/IconData" )]
public class SettlementIconData : ScriptableObject
{
    [SerializeField]
    List<SettlementIconEntry> _icons =
        new List<SettlementIconEntry> ( );       //아이콘 종류별 Sprite

    Dictionary<SettlementIconType , Sprite> _iconMap;       //아이콘 조회 목록

    /// <summary>
    /// 결산 아이콘 종류에 대응하는 Sprite 반환
    /// </summary>
    /// <param name="iconType">결산 아이콘 종류</param>
    /// <returns>표시할 Sprite</returns>
    public Sprite GetIcon ( SettlementIconType iconType )
    {
        if ( _iconMap == null )
            CreateIconMap ( );

        _iconMap.TryGetValue ( iconType , out Sprite icon );

        return icon;
    }

    /// <summary>
    /// 아이콘 종류별 조회 목록 구성
    /// </summary>
    void CreateIconMap ( )
    {
        _iconMap = new Dictionary<SettlementIconType , Sprite> ( );

        for ( int i = 0 ; i < _icons.Count ; i++ )
        {
            SettlementIconEntry entry = _icons [ i ];

            _iconMap [ entry.IconType ] = entry.Sprite;
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