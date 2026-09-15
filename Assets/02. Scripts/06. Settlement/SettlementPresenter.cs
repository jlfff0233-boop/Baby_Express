using System;
using UnityEngine;

/// <summary>
/// 현재 결산 페이지
/// </summary>
enum SettlementPage
{
    None,       //결산 미표시
    Daily,       //일일 결산
    Weekly,       //주간 결산
}

/// <summary>
/// 결산 프레젠터 - 결산 표시와 확인 입력 중재
/// </summary>
public class SettlementPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] SettlementView _settlementView;       //결산 뷰

    PlayStateModel _playStateModel;       //플레이 상태 모델
    CustomerOrderModel _orderModel;       //고객 주문 모델
    OrderGeneratorModel _generatorModel;       //주문 생성 모델
    SettlementPageViewDataBuilder _pageViewDataBuilder; //결산 페이지 표시 데이터 생성기
    SettlementPage _currentPage;       //현재 결산 페이지

    /// <summary>
    /// 일일 결산 표시 이벤트
    /// </summary>
    public event Action OnDailyShown;

    /// <summary>
    /// 일일 결산 확인 이벤트
    /// </summary>
    public event Action OnDailyConfirmed;

    /// <summary>
    /// 주간 결산 확인 이벤트
    /// </summary>
    public event Action OnWeeklyConfirmed;

    /// <summary>
    /// 주간 결산 표시 이벤트
    /// </summary>
    public event Action OnWeeklyShown;

    /// <summary>
    /// 현재 영업일 결산 검증 이벤트
    /// </summary>
    public event Action OnSettlementDebug;

    /// <summary>
    /// 일일 결산 표시 여부
    /// </summary>
    public bool IsShowingDaily =>
        _currentPage == SettlementPage.Daily;

    /// <summary>
    /// 결산 입력 연결
    /// </summary>
    void Awake ()
    {
        _settlementView.OnConfirmed += ConfirmSettlement;
    }

    /// <summary>
    /// 결산 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _settlementView.OnConfirmed -= ConfirmSettlement;
    }

    /// <summary>
    /// 결산 프레젠터 초기화
    /// </summary>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="generatorModel">주문 생성 모델</param>
    public void Init (
        PlayStateModel playStateModel,
        CustomerOrderModel orderModel,
        OrderGeneratorModel generatorModel )
    {
        _playStateModel = playStateModel;
        _orderModel = orderModel;
        _generatorModel = generatorModel;
        _pageViewDataBuilder = new SettlementPageViewDataBuilder( );
        _currentPage = SettlementPage.None;

        //결산 슬롯 풀과 표시 위치 초기화
        _settlementView.InitializeRuntime( );
        _settlementView.HideInstant( );
    }

    #region ----- 결산 표시 -----

    /// <summary>
    /// 일일 결산 표시
    /// </summary>
    /// <param name="data">일일 결산 계산 결과</param>
    public void ShowDaily ( DailySettlementData data )
    {
        //날짜 가져오기
        _playStateModel.GetDate( data.TotalDay, out int month, out int day );

        //현재 표시 중인 일일/주간 결산 페이지
        _currentPage = SettlementPage.Daily;

        //일일 결산 표시
        _settlementView.ShowDaily(
            _pageViewDataBuilder.CreateDailyPage( data, month, day ) );

        OnDailyShown?.Invoke( );
    }

    /// <summary>
    /// 주간 결산 표시
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    public void ShowWeekly ( WeeklySettlementData data )
    {
        //날짜, 다음 주 주문 범위 조회
        _playStateModel.GetDate(
            data.StartTotalDay, out int startMonth, out int startDay );
        _playStateModel.GetDate(
            data.EndTotalDay, out int endMonth, out int endDay );

        //주문 감소량 가져오기
        int nextDayReduction = _orderModel.NextDayOrderReduction;
        _generatorModel.GetExpectedOrderRange(
            nextDayReduction, data.NextWeekAdjustment,
            out int minCount, out int maxCount );

        //주간 결산 표시
        _currentPage = SettlementPage.Weekly;
        //결산 뷰 표시
        _settlementView.ShowWeekly(
            _pageViewDataBuilder.CreateWeeklyPage(
                data,
                startMonth, startDay,
                endMonth, endDay,
                nextDayReduction,
                minCount, maxCount ) );

        OnWeeklyShown?.Invoke( );
    }

    /// <summary>
    /// 지정한 결산 구역으로 이동하고 강조 대상을 반환
    /// </summary>
    /// <param name="sectionType">이동할 결산 구역</param>
    /// <param name="target">강조할 결산 구역 RectTransform</param>
    /// <returns>결산 구역 조회 여부</returns>
    public bool TryFocusSection (
        SettlementSectionType sectionType,
        out RectTransform target )
    {
        return _settlementView.TryFocusSection(
            sectionType, out target );
    }

    /// <summary>
    /// 결산 확인 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetConfirmTarget ( out RectTransform target )
    {
        return _settlementView.TryGetConfirmTarget( out target );
    }

    /// <summary>
    /// 결산 화면 숨김
    /// </summary>
    public void Hide ()
    {
        _currentPage = SettlementPage.None;
        _settlementView.Hide( );
    }

    /// <summary>
    /// 결산 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _currentPage = SettlementPage.None;
        _settlementView.HideInstant( );
    }

    /// <summary>
    /// 현재 결산 확인 입력 중계
    /// </summary>
    void ConfirmSettlement ()
    {
        if ( _currentPage == SettlementPage.None )
            return;

        SettlementPage confirmedPage = _currentPage;
        _currentPage = SettlementPage.None;

        _settlementView.Hide( () => CompleteConfirmation( confirmedPage ) );
    }

    /// <summary>
    /// 확인 완료
    /// </summary>
    /// <param name="confirmedPage">결산 페이지</param>
    void CompleteConfirmation ( SettlementPage confirmedPage )
    {
        //일일 결산일 때
        if ( confirmedPage == SettlementPage.Daily )
            OnDailyConfirmed?.Invoke( );
        //주간 결산일 때
        else if ( confirmedPage == SettlementPage.Weekly )
            OnWeeklyConfirmed?.Invoke( );
    }

    #endregion

    #region ----- 확인용 -----

    /// <summary>
    /// 현재 영업일 결산 검증 요청
    /// </summary>
    [ContextMenu( "현재 영업일 결산 검증" )]
    void DebugSettlement ()
    {
        if ( Application.isPlaying == false )
        {
            Debug.Log( "Play Mode에서 확인해 주세요." );
            return;
        }

        //실제 결산 흐름 시작 요청
        OnSettlementDebug?.Invoke( );
    }

    #endregion
}
