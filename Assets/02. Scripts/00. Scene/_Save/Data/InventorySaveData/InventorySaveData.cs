using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 저장 데이터
/// </summary>
[Serializable]
public class InventorySaveData
{
    [SerializeField] int _capacity;       //최대 슬롯 수
    [SerializeField] List<InventoryItemSaveData> _items;      //보유 아이템 목록

    /// <summary>
    /// 최대 슬롯 수
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// 보유 아이템 목록
    /// </summary>
    public IReadOnlyList<InventoryItemSaveData> Items => _items;

    /// <summary>
    /// 인벤토리 저장 데이터 생성
    /// </summary>
    /// <param name="capacity">최대 슬롯 수</param>
    /// <param name="items">보유 아이템 목록</param>
    public InventorySaveData ( int capacity,
        IReadOnlyCollection<InventoryItemSaveData> items )
    {
        _capacity = capacity;
        _items = new List<InventoryItemSaveData>( items );
    }
}