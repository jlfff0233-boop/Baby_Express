using System;
using UnityEngine;

/// <summary>
/// 결산 상품 상세 세이브 데이터
/// </summary>
[Serializable]
public class SettlementItemSaveData
{
    [SerializeField] string _itemId;       //상품 아이디
    [SerializeField] int _quantity;       //상품 합산 수량
    [SerializeField] float _priceTotal;       //상품 합산 금액

    #region ----- 프로퍼티 -----

    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string ItemId => _itemId;

    /// <summary>
    /// 상품 합산 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 상품 합산 금액
    /// </summary>
    public float PriceTotal => _priceTotal;

    #endregion

    /// <summary>
    /// 결산 상품 상세 세이브 데이터 생성
    /// </summary>
    /// <param name="data">결산 상품 상세</param>
    public SettlementItemSaveData ( ItemSettlement data )
    {
        _itemId = data.Data.Id;
        _quantity = data.Quantity;
        _priceTotal = data.PriceTotal;
    }
}