using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 고객 주문 저장 데이터
/// </summary>
[Serializable]
public class CustomerOrderSaveData
{
    [SerializeField] int _createdNumber;       //누적 생성 번호
    [SerializeField] string _orderId;      //주문 아이디
    [SerializeField] string _title;        //주문 제목
    [SerializeField] int _maxCraftCost;        //최대 제작 코스트

    [SerializeField] List<OrderPartConditionSaveData> _requirements;      //주요 요구 사항
    [SerializeField] List<OrderPartConditionSaveData> _wishes;        //희망 사항

    [SerializeField] OrderSpecialType _specialType;        //특수 주문 종류
    [SerializeField] int _maxPartCount;        //최대 전체 파츠 수
    [SerializeField] List<PartTheme> _targetThemes;        //목표 테마
    [SerializeField] List<PartTheme> _excludedThemes;      //제외 테마

    [SerializeField] int _createdTotalDay;     //주문 생성일
    [SerializeField] int _acceptDue;       //수락 마감일
    [SerializeField] int _deliveryDue;     //납품 마감일
    [SerializeField] int _maxDelay;        //최대 지연일
    [SerializeField] int _closedTotalDay;      //주문 종료일

    [SerializeField] OrderDifficulty _difficulty;      //주문 난이도
    [SerializeField] OrderProgressState _progressState;        //진행 상태
    [SerializeField] OrderOutcome _outcome;        //주문 결과

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 주문 생성 번호
    /// </summary>
    public int CreatedNumber => _createdNumber;
    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId => _orderId;
    /// <summary>
    /// 주문 제목
    /// </summary>
    public string Title => _title;
    /// <summary>
    /// 최대 제작 코스트
    /// </summary>
    public int MaxCraftCost => _maxCraftCost;

    public IReadOnlyList<OrderPartConditionSaveData> Requirements =>
        _requirements;

    public IReadOnlyList<OrderPartConditionSaveData> Wishes =>
        _wishes;

    public OrderSpecialType SpecialType => _specialType;
    public int MaxPartCount => _maxPartCount;
    public IReadOnlyList<PartTheme> TargetThemes => _targetThemes;
    public IReadOnlyList<PartTheme> ExcludedThemes => _excludedThemes;

    public int CreatedTotalDay => _createdTotalDay;
    public int AcceptDue => _acceptDue;
    public int DeliveryDue => _deliveryDue;
    public int MaxDelay => _maxDelay;
    public int ClosedTotalDay => _closedTotalDay;

    public OrderDifficulty Difficulty => _difficulty;
    public OrderProgressState ProgressState => _progressState;
    public OrderOutcome Outcome => _outcome;
    #endregion

    /// <summary>
    /// 고객 주문 저장 데이터 생성
    /// </summary>
    /// <param name="createdNumber">주문 생성 번호</param>
    /// <param name="orderId">주문 아이디</param>
    /// <param name="title">주문 제목</param>
    /// <param name="maxCraftCost">최대 제작 코스트</param>
    /// <param name="requirements">주요 요구 사항</param>
    /// <param name="wishes">희망 사항</param>
    /// <param name="specialType">특수 주문 타입</param>
    /// <param name="maxPartCount">최대 파츠 개수</param>
    /// <param name="targetThemes">목표 테마</param>
    /// <param name="excludedThemes">제외 테마</param>
    /// <param name="createdTotalDay">생성 총 영업일</param>
    /// <param name="acceptDue">수락일</param>
    /// <param name="deliveryDue">배송일</param>
    /// <param name="maxDelay">최대 배송 지연일</param>
    /// <param name="closedTotalDay">종료 총 영업일</param>
    /// <param name="difficulty">난이도</param>
    /// <param name="progressState">진행 상태</param>
    /// <param name="outcome">결과</param>
    public CustomerOrderSaveData (
        int createdNumber, string orderId,
        string title, int maxCraftCost,
        IReadOnlyCollection<OrderPartConditionSaveData> requirements,
        IReadOnlyCollection<OrderPartConditionSaveData> wishes,
        OrderSpecialType specialType, int maxPartCount,
        IReadOnlyCollection<PartTheme> targetThemes,
        IReadOnlyCollection<PartTheme> excludedThemes,
        int createdTotalDay, int acceptDue,
        int deliveryDue, int maxDelay,
        int closedTotalDay,
        OrderDifficulty difficulty,
        OrderProgressState progressState,
        OrderOutcome outcome )
    {
        _createdNumber = createdNumber;
        _orderId = orderId;
        _title = title;
        _maxCraftCost = maxCraftCost;

        _requirements =
            new List<OrderPartConditionSaveData>( requirements );
        _wishes =
            new List<OrderPartConditionSaveData>( wishes );

        _specialType = specialType;
        _maxPartCount = maxPartCount;
        _targetThemes = new List<PartTheme>( targetThemes );
        _excludedThemes = new List<PartTheme>( excludedThemes );

        _createdTotalDay = createdTotalDay;
        _acceptDue = acceptDue;
        _deliveryDue = deliveryDue;
        _maxDelay = maxDelay;
        _closedTotalDay = closedTotalDay;

        _difficulty = difficulty;
        _progressState = progressState;
        _outcome = outcome;
    }
}