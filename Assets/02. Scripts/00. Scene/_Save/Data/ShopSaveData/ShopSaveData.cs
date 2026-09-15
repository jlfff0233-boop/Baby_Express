using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 저장 데이터
/// </summary>
[Serializable]
public class ShopSaveData
{
    [SerializeField] int _partMaxStockBonus;      //파츠 최대 재고 보정값
    [SerializeField] int _partRestockSpan;        //파츠 재입고 간격 정비값
    [SerializeField] List<ShopItemSaveData> _items;       //상품 상태 목록

    /// <summary>
    /// 파츠 최대 재고 보정값
    /// </summary>
    public int PartMaxStockBonus => _partMaxStockBonus;

    /// <summary>
    /// 파츠 재입고 간격
    /// </summary>
    public int PartRestockSpan => _partRestockSpan;

    /// <summary>
    /// 상품 상태 목록
    /// </summary>
    public IReadOnlyList<ShopItemSaveData> Items => _items;

    /// <summary>
    /// 상점 저장 데이터 생성
    /// </summary>
    /// <param name="partMaxStockBonus">파츠 최대 재고 보정값</param>
    /// <param name="partRestockSpan">파츠 재입고 간격</param>
    /// <param name="items">상품 목록</param>
    public ShopSaveData (
        int partMaxStockBonus, int partRestockSpan,
        IReadOnlyCollection<ShopItemSaveData> items )
    {
        _partMaxStockBonus = partMaxStockBonus;
        _partRestockSpan = partRestockSpan;
        _items = new List<ShopItemSaveData>( items );
    }
}