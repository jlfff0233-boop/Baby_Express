using System;
using UnityEngine;

/// <summary>
/// 인벤토리 아이템 저장 데이터
/// </summary>
[Serializable]
public class InventoryItemSaveData
{
    [SerializeField] string _id;      //상품 아이디
    [SerializeField] int _quantity;       //전체 보유 수량

    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 전체 보유 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 인벤토리 아이템 저장 데이터 생성
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">전체 보유 수량</param>
    public InventoryItemSaveData ( string id, int quantity )
    {
        _id = id;
        _quantity = quantity;
    }
}