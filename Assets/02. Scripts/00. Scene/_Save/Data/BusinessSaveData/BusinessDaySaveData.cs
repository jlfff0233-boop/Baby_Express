using System;
using UnityEngine;

/// <summary>
/// 영업일 저장 데이터
/// </summary>
[Serializable]
public class BusinessDaySaveData
{
    [SerializeField] int _craftCount;       //제작 완료 횟수
    [SerializeField] int _craftLimit;       //일일 제작 할당량
    [SerializeField] int _baseCraftLimit;       //기본 제작 할당량 제한
    [SerializeField] int _nextCraftLimit;       //다음 제작 할당량 제한
    [SerializeField] int _employeeCraftBonus;       //저장 시 적용된 직원 제작 할당량 보너스

    [SerializeField] int _quickRestockCount;        //빠른 재입고 횟수
    [SerializeField] int _quickRestockLimit;        //빠른 재입고 제한

    [SerializeField] bool _isEndStarted;        //결산 시작 여부
    [SerializeField] DayEndReason _endReason;       //영업 종료 사유

    /// <summary>
    /// 오늘 제작 완료 수
    /// </summary>
    public int CraftCount => _craftCount;

    /// <summary>
    /// 현재 제작 할당량
    /// </summary>
    public int CraftLimit => _craftLimit;

    /// <summary>
    /// 기본 제작 할당량
    /// </summary>
    public int BaseCraftLimit => _baseCraftLimit;

    /// <summary>
    /// 다음 영업일 제작 할당량
    /// </summary>
    public int NextCraftLimit => _nextCraftLimit;

    /// <summary>
    /// 저장 시 적용된 직원 제작 할당량 보너스
    /// </summary>
    public int EmployeeCraftBonus => _employeeCraftBonus;

    /// <summary>
    /// 오늘 빠른 재입고 횟수
    /// </summary>
    public int QuickRestockCount => _quickRestockCount;

    /// <summary>
    /// 일일 빠른 재입고 제한
    /// </summary>
    public int QuickRestockLimit => _quickRestockLimit;

    /// <summary>
    /// 결산 시작 여부
    /// </summary>
    public bool IsEndStarted => _isEndStarted;

    /// <summary>
    /// 영업 종료 사유
    /// </summary>
    public DayEndReason EndReason => _endReason;

    /// <summary>
    /// 영업일 저장 데이터 생성
    /// </summary>
    /// <param name="craftCount">오늘 제작 완료 수</param>
    /// <param name="craftLimit">현재 제작 할당량</param>
    /// <param name="baseCraftLimit">기본 제작 할당량</param>
    /// <param name="nextCraftLimit">다음 영업일 제작 할당량</param>
    /// <param name="employeeCraftBonus">적용된 직원 제작 할당량 보너스</param>
    /// <param name="quickRestockCount">오늘 빠른 재입고 횟수</param>
    /// <param name="quickRestockLimit">일일 빠른 재입고 제한</param>
    /// <param name="isEndStarted">결산 시작 여부</param>
    /// <param name="endReason">영업 종료 사유</param>
    public BusinessDaySaveData (
        int craftCount, int craftLimit,
        int baseCraftLimit, int nextCraftLimit,
        int employeeCraftBonus,
        int quickRestockCount, int quickRestockLimit,
        bool isEndStarted, DayEndReason endReason )
    {
        _craftCount = craftCount;
        _craftLimit = craftLimit;
        _baseCraftLimit = baseCraftLimit;
        _nextCraftLimit = nextCraftLimit;
        _employeeCraftBonus = employeeCraftBonus;

        _quickRestockCount = quickRestockCount;
        _quickRestockLimit = quickRestockLimit;

        _isEndStarted = isEndStarted;
        _endReason = endReason;
    }
}
