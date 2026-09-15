using System;
using UnityEngine;

/// <summary>
/// 상점 상품 저장 데이터
/// </summary>
[Serializable]
public class ShopItemSaveData
{
    [SerializeField] string _id;      //상품 아이디
    [SerializeField] float _currentPrice;       //현재 가격
    [SerializeField] int _remainingStock;       //현재 재고
    [SerializeField] int _maxStock;      //최대 재고
    [SerializeField] int _restockSpan;       //현재 재입고 간격
    [SerializeField] int _nextRestockDay;        //다음 재입고 예정일
    [SerializeField] int _pendingRestockSpan;        //적용 대기 재입고 간격

    /// <summary>
    /// 상품 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 현재 가격
    /// </summary>
    public float CurrentPrice => _currentPrice;

    /// <summary>
    /// 현재 재고
    /// </summary>
    public int RemainingStock => _remainingStock;

    /// <summary>
    /// 최대 재고
    /// </summary>
    public int MaxStock => _maxStock;

    /// <summary>
    /// 현재 재입고 간격
    /// </summary>
    public int RestockSpan => _restockSpan;

    /// <summary>
    /// 다음 재입고 예정일
    /// </summary>
    public int NextRestockDay => _nextRestockDay;

    /// <summary>
    /// 적용 대기 재입고 간격
    /// </summary>
    public int PendingRestockSpan => _pendingRestockSpan;

    /// <summary>
    /// 상점 상품 저장 데이터 생성
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="currentPrice">현재 가격</param>
    /// <param name="remainingStock">남은 재고</param>
    /// <param name="maxStock">최대 재고</param>
    /// <param name="restockSpan">재입고 간격</param>
    /// <param name="nextRestockDay">다음 재입고 예정일</param>
    /// <param name="pendingRestockSpan">적용 대기 재입고 간격</param>
    public ShopItemSaveData (
        string id, float currentPrice,
        int remainingStock, int maxStock,
        int restockSpan, int nextRestockDay,
        int pendingRestockSpan )
    {
        _id = id;
        _currentPrice = currentPrice;

        _remainingStock = remainingStock;
        _maxStock = maxStock;

        _restockSpan = restockSpan;
        _nextRestockDay = nextRestockDay;
        _pendingRestockSpan = pendingRestockSpan;
    }
}