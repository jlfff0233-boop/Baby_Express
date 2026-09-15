using System;
using System.Collections.Generic;

/// <summary>
/// 주문 - 주문 생성, 주문 받기, 완료 처리
/// </summary>
public class CustomerOrder
{
    int _createdNumber;     //누적 생성 번호
    string _orderId;     //주문 아이디
    string _title;      //주문 제목
    int _maxCraftCost;      //최대 제작 코스트

    List<OrderPartCondition> _requirements;       //주요 요구 사항
    List<OrderPartCondition> _wishes;       //희망 사항

    OrderSpecialType _specialType;       //특수 주문 종류
    int _maxPartCount;       //최대 전체 파츠 개수
    List<PartTheme> _targetThemes;       //목표 테마 목록
    List<PartTheme> _excludedThemes;       //제외 테마 목록

    int _createdTotalDay;       //주문 생성 누적 영업일
    int _acceptDue;     //수락 마감일
    int _deliveryDue;      //납품 마감일
    int _maxDelay;      //지연 허용 일수
    int _closedTotalDay;       //주문 종료 누적 영업일

    OrderDifficulty _difficulty;        //난이도
    OrderProgressState _progressState;       //진행 상태
    OrderOutcome _outcome;     //결과

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 주문 번호
    /// </summary>
    public int CreatedNumber => _createdNumber;
    public string OrderId => _orderId;
    /// <summary>
    /// 주문 제목
    /// </summary>
    public string Title => _title;
    public int MaxCraftCost => _maxCraftCost;

    /// <summary>
    /// 주요 요구 사항
    /// </summary>
    public IReadOnlyList<OrderPartCondition> Requirements => _requirements;
    /// <summary>
    /// 희망 사항
    /// </summary>
    public IReadOnlyList<OrderPartCondition> Wishes => _wishes;

    /// <summary>
    /// 특수 주문 종류
    /// </summary>
    public OrderSpecialType SpecialType => _specialType;
    /// <summary>
    /// 특수 주문 여부
    /// </summary>
    public bool IsSpecial =>
        _specialType != OrderSpecialType.None ||
        _maxPartCount > 0 ||
        _targetThemes.Count > 0 ||
        _excludedThemes.Count > 0;

    /// <summary>
    /// 최대 파츠 개수
    /// </summary>
    public int MaxPartCount => _maxPartCount;
    /// <summary>
    /// 목표 테마 목록
    /// </summary>
    public IReadOnlyList<PartTheme> TargetThemes => _targetThemes;
    /// <summary>
    /// 제외 테마 목록
    /// </summary>
    public IReadOnlyList<PartTheme> ExcludedThemes => _excludedThemes;

    /// <summary>
    /// 주문 일자(누적 영업일 기준)
    /// </summary>
    public int CreatedTotalDay => _createdTotalDay;
    /// <summary>
    /// 수락 마감일
    /// </summary>
    public int AcceptDue => _acceptDue;
    /// <summary>
    /// 납품 마감일
    /// </summary>
    public int DeliveryDue => _deliveryDue;
    /// <summary>
    /// 지연 허용 일수
    /// </summary>
    public int MaxDelay => _maxDelay;
    /// <summary>
    /// 납품 최종 기한(납품 마감일 + 최대 지연일)
    /// </summary>
    public int FinalDeadline => _deliveryDue + _maxDelay;
    /// <summary>
    /// 주문 종료일
    /// </summary>
    public int ClosedTotalDay => _closedTotalDay;

    /// <summary>
    /// 주문 난이도
    /// </summary>
    public OrderDifficulty Difficulty => _difficulty;
    /// <summary>
    /// 현재 진행 상태
    /// </summary>
    public OrderProgressState ProgressState => _progressState;
    /// <summary>
    /// 주문 처리 결과
    /// </summary>
    public OrderOutcome Outcome => _outcome;

    #endregion

    /// <summary>
    /// 고객 주문 생성자
    /// </summary>
    /// <param name="createData">주문 생성 데이터</param>
    public CustomerOrder ( OrderCreateData createData )
    {
        //생성 데이터 확인
        if ( createData == null )
            throw new ArgumentNullException( nameof( createData ) );

        _createdNumber = createData.CreatedNumber;      //주문 번호
        _orderId = createData.Id;       //주문 아이디
        _title = createData.Title;      //주문 제목
        _maxCraftCost = createData.MaxCraftCost;        //최대 제작 코스트

        //주요 요구 사항을 주문 내부 목록으로 복사
        _requirements = createData.Requirements == null
            //없으면 그냥 리스트 생성
            ? new List<OrderPartCondition>( )
            //있으면 요구 사항 리스트 생성
            : new List<OrderPartCondition>( createData.Requirements );

        //희망 사항을 주문 내부 목록으로 복사
        _wishes = createData.Wishes == null
            ? new List<OrderPartCondition>( )
            : new List<OrderPartCondition>( createData.Wishes );

        //특수 주문 종류 저장
        _specialType = createData.SpecialType;
        //특수 주문 조건 저장
        _maxPartCount = createData.MaxPartCount;
        //목표 테마 저장
        _targetThemes = createData.TargetThemes == null
            ? new List<PartTheme>( )
            : new List<PartTheme>( createData.TargetThemes );
        //제외 테마 저장
        _excludedThemes = createData.ExcludedThemes == null
            ? new List<PartTheme>( )
            : new List<PartTheme>( createData.ExcludedThemes );

        //주문 생성일 저장
        _createdTotalDay = createData.CreatedTotalDay;

        //기한 설정
        _acceptDue = createData.AcceptDue;
        _deliveryDue = createData.DeliveryDue;
        _maxDelay = createData.MaxDelay;

        //난이도 설정
        _difficulty = createData.Difficulty;

        //대기 중 상태
        _progressState = OrderProgressState.Waiting;

        //결과 미정
        _outcome = OrderOutcome.None;
    }

    /// <summary>
    /// 진행 상태 설정
    /// </summary>
    /// <param name="progress">진행 상태</param>
    public void SetProgress ( OrderProgressState progress )
    {
        _progressState = progress;
    }

    /// <summary>
    /// 주문 종료
    /// </summary>
    /// <param name="outcome">주문 결과</param>
    /// <param name="totalDay">종료 누적 영업일</param>
    public void Close ( OrderOutcome outcome, int totalDay )
    {
        _progressState = OrderProgressState.Closed;
        _outcome = outcome;
        _closedTotalDay = totalDay;
    }

    /// <summary>
    /// 저장된 주문 진행 상태 복구
    /// </summary>
    /// <param name="progressState">진행 상태</param>
    /// <param name="outcome">주문 결과</param>
    /// <param name="closedTotalDay">주문 종료 누적 영업일</param>
    internal void RestoreState (
        OrderProgressState progressState,
        OrderOutcome outcome, int closedTotalDay )
    {
        _progressState = progressState;
        _outcome = outcome;
        _closedTotalDay = closedTotalDay;
    }

    /// <summary>
    /// 현재 상태에 맞는 기한 조회
    /// </summary>
    /// <param name="currentTotalDay">현재 누적 영업일</param>
    /// <returns>현재 적용 중인 기한</returns>
    public int GetCurrentDeadline ( int currentTotalDay )
    {
        //수락 대기 중에는 수락 마감일 반환
        if ( _progressState == OrderProgressState.Waiting )
            return _acceptDue;

        //납품 마감 전에는 납품 마감일 반환
        if ( currentTotalDay <= _deliveryDue )
            return _deliveryDue;

        //연체 후에는 최종 기한 반환
        return FinalDeadline;
    }

}
