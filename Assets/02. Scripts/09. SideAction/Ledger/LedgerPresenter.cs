using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 가계부 프레젠터 - 주간 기록 목록과 선택 상세 표시 중재
/// </summary>
public class LedgerPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] LedgerView _ledgerView;       //가계부 뷰

    SettlementModel _settlementModel;       //결산 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    SettlementViewDataBuilder _viewDataBuilder;       //가계부 목록 표시 데이터 생성기
    SettlementPageViewDataBuilder _pageViewDataBuilder;       //가계부 상세 페이지 생성기

    List<WeeklySettlementData> _weeklySettlements =
        new List<WeeklySettlementData>( );       //최신순 주간 결산 목록
    List<int> _monthlyWeeks = new List<int>( );       //선택 연도와 월의 주차
    List<int> _years = new List<int>( );       //조회 가능한 연도 목록
    List<int> _months = new List<int>( );       //조회 가능한 월 목록
#if UNITY_EDITOR
    List<WeeklySettlementData> _debugWeeklySettlements =
        new List<WeeklySettlementData>( );       //가계부 검증용 주간 결산 목록
#endif
    int _selectedYear;       //현재 조회 연도
    int _selectedMonth;       //현재 조회 월
    int _latestYear;       //조회 가능한 최근 연도
    int _latestMonth;       //조회 가능한 최근 월
    SettlementPageViewData _detailPage;       //현재 선택한 주간 상세 페이지
    int _detailSectionIndex;       //현재 상세 항목 인덱스
    bool _isDetail;       //상세 화면 표시 여부

    /// <summary>
    /// 가계부 닫기 요청 이벤트
    /// </summary>
    public event Action OnCloseRequested;

    /// <summary>
    /// 새 가계부 기록 알림 상태 변경 이벤트
    /// </summary>
    public event Action<bool> OnNotificationChanged;

    /// <summary>
    /// 새 가계부 기록 알림 여부
    /// </summary>
    public bool HasNotification => _settlementModel.HasNotification;

    #region ----- 초기화/이벤트 -----
    /// <summary>
    /// 가계부 프레젠터 초기화
    /// </summary>
    /// <param name="settlementModel">결산 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    public void Init (
        SettlementModel settlementModel, PlayStateModel playStateModel )
    {
        _settlementModel = settlementModel;
        _playStateModel = playStateModel;
        _viewDataBuilder = new SettlementViewDataBuilder( );
        _pageViewDataBuilder =
            new SettlementPageViewDataBuilder( );

        _settlementModel.OnNotificationChanged += ChangeNotification;

        _ledgerView.InitializeRuntime( );
        _ledgerView.HideInstant( );
    }

    /// <summary>
    /// 가계부 입력 연결
    /// </summary>
    void Awake ()
    {
        _ledgerView.OnWeekSelected += SelectWeek;
        _ledgerView.OnYearSelected += SelectYear;
        _ledgerView.OnMonthSelected += SelectMonth;
        _ledgerView.OnPreviousRequested += ShowPrevious;
        _ledgerView.OnNextRequested += ShowNext;
        _ledgerView.OnBackRequested += ShowList;
        _ledgerView.OnClosed += RequestClose;
    }

    /// <summary>
    /// 가계부 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _settlementModel != null )
            _settlementModel.OnNotificationChanged -= ChangeNotification;

        _ledgerView.OnWeekSelected -= SelectWeek;
        _ledgerView.OnYearSelected -= SelectYear;
        _ledgerView.OnMonthSelected -= SelectMonth;
        _ledgerView.OnPreviousRequested -= ShowPrevious;
        _ledgerView.OnNextRequested -= ShowNext;
        _ledgerView.OnBackRequested -= ShowList;
        _ledgerView.OnClosed -= RequestClose;
    }
    #endregion

    #region ----- 표시 -----
    /// <summary>
    /// 최신 주간 기록으로 가계부 표시
    /// </summary>
    public void Show ()
    {
        _settlementModel.MarkNotificationViewed( );
        RefreshWeeklySettlements( );
        SetLatestDateFilter( );
        RefreshPage( );
    }

    /// <summary>
    /// 가계부 목록의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 주차 선택 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        return _ledgerView.TryGetTutorialTarget(
            targetId, out target );
    }

    /// <summary>
    /// 새 가계부 기록 알림 상태 전달
    /// </summary>
    /// <param name="hasNotification">알림 존재 여부</param>
    void ChangeNotification ( bool hasNotification )
    {
        OnNotificationChanged?.Invoke( hasNotification );
    }

    /// <summary>
    /// 가계부 숨김
    /// </summary>
    public void Hide ()
    {
        _ledgerView.Hide( );
    }

    /// <summary>
    /// 가계부 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _ledgerView.HideInstant ( );
    }

    /// <summary>
    /// 가계부 닫기 요청 전달
    /// </summary>
    void RequestClose ()
    {
        OnCloseRequested?.Invoke( );
    }

    /// <summary>
    /// 선택한 연도로 목록 갱신
    /// </summary>
    /// <param name="optionIndex">선택한 연도 인덱스</param>
    void SelectYear ( int optionIndex )
    {
        if ( optionIndex < 0 || optionIndex >= _years.Count ) return;

        _selectedYear = _years [ optionIndex ];
        _selectedMonth = Mathf.Min(
            _selectedMonth, GetLatestMonth( _selectedYear ) );

        SetMonthOptions( );
        RefreshPage( );
    }

    /// <summary>
    /// 선택한 월로 목록 갱신
    /// </summary>
    /// <param name="optionIndex">선택한 월 인덱스</param>
    void SelectMonth ( int optionIndex )
    {
        if ( optionIndex < 0 || optionIndex >= _months.Count ) return;

        _selectedMonth = _months [ optionIndex ];
        RefreshPage( );
    }

    /// <summary>
    /// 이전 월 또는 이전 상세 항목 표시
    /// </summary>
    void ShowPrevious ()
    {
        if ( _isDetail )
        {
            ShowPreviousDetail( );
            return;
        }

        if ( _selectedYear == 1 && _selectedMonth == 1 ) return;

        if ( _selectedMonth > 1 )
        {
            _selectedMonth--;
        }
        else
        {
            _selectedYear--;
            _selectedMonth = 12;
        }

        SetDateOptions( );
        RefreshPage( );
    }

    /// <summary>
    /// 다음 월 또는 다음 상세 항목 표시
    /// </summary>
    void ShowNext ()
    {
        if ( _isDetail )
        {
            ShowNextDetail( );
            return;
        }

        if ( _selectedYear == _latestYear &&
            _selectedMonth == _latestMonth ) return;

        if ( _selectedMonth < 12 )
        {
            _selectedMonth++;
        }
        else
        {
            _selectedYear++;
            _selectedMonth = 1;
        }

        SetDateOptions( );
        RefreshPage( );
    }
    #endregion

    #region ----- 상세 표시 -----
    /// <summary>
    /// 상세에서 현재 주간 목록으로 돌아가기
    /// </summary>
    void ShowList ()
    {
        _isDetail = false;
        _detailPage = null;
        _detailSectionIndex = 0;

        _ledgerView.ShowList( );
        SetNavigation( );
    }

    /// <summary>
    /// 선택한 주차의 상세 표시
    /// </summary>
    /// <param name="week">선택한 결산 주차</param>
    void SelectWeek ( int week )
    {
        for ( int i = 0; i < _weeklySettlements.Count; i++ )
        {
            WeeklySettlementData data = _weeklySettlements [ i ];

            if ( data.Week != week ) continue;

            _detailPage = CreateDetail( data );
            _detailSectionIndex = 0;
            _isDetail = true;

            _ledgerView.SetSelectedWeek( week );
            ShowDetailSection( );
            return;
        }
    }

    /// <summary>
    /// 이전 상세 항목 표시
    /// </summary>
    void ShowPreviousDetail ()
    {
        if ( _detailSectionIndex <= 0 ) return;

        _detailSectionIndex--;
        ShowDetailSection( );
    }

    /// <summary>
    /// 다음 상세 항목 표시
    /// </summary>
    void ShowNextDetail ()
    {
        if ( _detailPage == null ||
            _detailSectionIndex >=
            _detailPage.Sections.Count - 1 ) return;

        _detailSectionIndex++;
        ShowDetailSection( );
    }

    /// <summary>
    /// 현재 상세 항목 표시
    /// </summary>
    void ShowDetailSection ()
    {
        SettlementSectionViewData section =
            _detailPage.Sections [ _detailSectionIndex ];

        _ledgerView.ShowDetail( section );
        SetNavigation( );
    }
    #endregion

    #region ----- 기록 조회 -----
    /// <summary>
    /// 완료된 주간 결산을 최신순으로 정렬
    /// </summary>
    void RefreshWeeklySettlements ()
    {
        IReadOnlyList<WeeklySettlementData> source =
            _settlementModel.WeeklySettlements;

        _weeklySettlements.Clear( );

        for ( int i = source.Count - 1; i >= 0; i-- )
            _weeklySettlements.Add( source [ i ] );

#if UNITY_EDITOR
        for ( int i = _debugWeeklySettlements.Count - 1; i >= 0; i-- )
            _weeklySettlements.Add( _debugWeeklySettlements [ i ] );
#endif
    }

    /// <summary>
    /// 가장 최근 결산의 연도와 월을 기본 조회 조건으로 설정
    /// </summary>
    void SetLatestDateFilter ()
    {
        _years.Clear( );

        int latestTotalDay = _playStateModel.TotalDay;

#if UNITY_EDITOR
        if ( _debugWeeklySettlements.Count > 0 )
        {
            latestTotalDay = Mathf.Max(
                latestTotalDay,
                _debugWeeklySettlements [ _debugWeeklySettlements.Count - 1 ].EndTotalDay );
        }
#endif

        _latestYear = _playStateModel.GetYear( latestTotalDay );
        _playStateModel.GetDate( latestTotalDay, out _latestMonth, out _ );

        //첫해부터 현재 연도까지 오름차순으로 조회 가능
        for ( int year = 1; year <= _latestYear; year++ )
            _years.Add( year );

        _selectedYear = _latestYear;
        _selectedMonth = _latestMonth;

        SetDateOptions( );
    }

    /// <summary>
    /// 현재 선택 날짜에 맞춰 연도와 월 드롭다운 설정
    /// </summary>
    void SetDateOptions ()
    {
        _ledgerView.SetYearOptions(
            CreateDateOptions( _years, "년" ), _selectedYear - 1 );
        SetMonthOptions( );
    }

    /// <summary>
    /// 현재 연도에서 조회 가능한 월 목록 설정
    /// </summary>
    void SetMonthOptions ()
    {
        _months.Clear( );

        int latestMonth = GetLatestMonth( _selectedYear );

        //첫 달부터 조회 가능한 최근 월까지 오름차순으로 표시
        for ( int month = 1; month <= latestMonth; month++ )
            _months.Add( month );

        _ledgerView.SetMonthOptions(
            CreateDateOptions( _months, "월" ), _selectedMonth - 1 );
    }

    /// <summary>
    /// 지정 연도에서 조회 가능한 최근 월 반환
    /// </summary>
    /// <param name="year">조회 연도</param>
    /// <returns>조회 가능한 최근 월</returns>
    int GetLatestMonth ( int year )
    {
        return year == _latestYear ? _latestMonth : 12;
    }

    /// <summary>
    /// 현재 조회 연도와 월의 주간 목록 갱신
    /// </summary>
    void RefreshPage ()
    {
        _isDetail = false;
        _detailPage = null;
        _detailSectionIndex = 0;

        CreateMonthlyWeeks( );

        if ( _monthlyWeeks.Count == 0 )
        {
            _ledgerView.ShowEmpty( );
            SetNavigation( );
            return;
        }

        //주간 기록 목록 표시
        _ledgerView.ShowList( CreateSummaries( 0, _monthlyWeeks.Count ) );
        SetNavigation( );
    }

    /// <summary>
    /// 선택한 연도와 월의 예정 주차 생성
    /// </summary>
    void CreateMonthlyWeeks ()
    {
        _monthlyWeeks.Clear( );

        int firstWeek = ( _selectedYear - 1 ) * _playStateModel.WeeksPerYear + 1;
        int lastWeek = firstWeek + _playStateModel.WeeksPerYear;

        //주간 종료일이 선택한 연도와 월에 속하는 주차만 표시
        for ( int week = firstWeek; week < lastWeek; week++ )
        {
            int endTotalDay = week * 7;
            int year = _playStateModel.GetYear( endTotalDay );
            _playStateModel.GetDate( endTotalDay, out int month, out _ );

            if ( year == _selectedYear && month == _selectedMonth )
                _monthlyWeeks.Add( week );
        }
    }

    /// <summary>
    /// 현재 화면의 이전과 다음 이동 가능 상태 갱신
    /// </summary>
    void SetNavigation ()
    {
        if ( _isDetail )
        {
            int sectionCount =
                _detailPage != null
                    ? _detailPage.Sections.Count
                    : 0;

            _ledgerView.SetNavigation(
                _detailSectionIndex > 0,
                _detailSectionIndex < sectionCount - 1 );

            return;
        }

        _ledgerView.SetNavigation(
            _selectedYear > 1 || _selectedMonth > 1,
            _selectedYear < _latestYear ||
            _selectedMonth < _latestMonth );
    }
    #endregion

    #region ----- 표시 데이터 생성 -----
    /// <summary>
    /// 최신순 주간 목록 표시 데이터 생성
    /// </summary>
    /// <returns>주간 목록 표시 데이터</returns>
    IReadOnlyList<WeeklyLedgerSummaryViewData> CreateSummaries (
        int startIndex, int count )
    {
        var summaries = new List<WeeklyLedgerSummaryViewData>( count );

        for ( int i = startIndex; i < startIndex + count; i++ )
        {
            int week = _monthlyWeeks [ i ];
            int displayWeek = ( week - 1 ) % _playStateModel.WeeksPerYear + 1;

            if ( GetSettlement( week, out WeeklySettlementData data ) )
            {
                summaries.Add(
                    _viewDataBuilder.CreateWeeklyLedgerSummary(
                        data, displayWeek ) );
            }
            else
            {
                summaries.Add(
                    _viewDataBuilder.CreatePendingWeeklyLedgerSummary(
                        week, displayWeek ) );
            }
        }

        return summaries;
    }

    /// <summary>
    /// 연도 또는 월 Dropdown 문구 생성
    /// </summary>
    /// <param name="values">날짜 값 목록</param>
    /// <param name="suffix">날짜 단위</param>
    /// <returns>Dropdown 문구 목록</returns>
    IReadOnlyList<string> CreateDateOptions (
        IReadOnlyList<int> values, string suffix )
    {
        var options = new List<string>( values.Count );

        for ( int i = 0; i < values.Count; i++ )
            options.Add( $"{values [ i ]}{suffix}" );

        return options;
    }

    /// <summary>
    /// 지정 주차의 완료된 결산 조회
    /// </summary>
    /// <param name="week">누적 결산 주차</param>
    /// <param name="data">완료된 주간 결산</param>
    /// <returns>결산 존재 여부</returns>
    bool GetSettlement ( int week, out WeeklySettlementData data )
    {
        for ( int i = 0; i < _weeklySettlements.Count; i++ )
        {
            if ( _weeklySettlements [ i ].Week != week ) continue;

            data = _weeklySettlements [ i ];
            return true;
        }

        data = null;
        return false;
    }

    /// <summary>
    /// 선택한 주간 기록의 상세 표시 데이터 생성
    /// </summary>
    /// <param name="data">선택한 주간 결산</param>
    /// <returns>주간 상세 페이지 표시 데이터</returns>
    SettlementPageViewData CreateDetail ( WeeklySettlementData data )
    {
        _playStateModel.GetDate(
            data.StartTotalDay, out int startMonth, out int startDay );
        _playStateModel.GetDate(
            data.EndTotalDay, out int endMonth, out int endDay );
        int displayWeek =
            _playStateModel.GetWeekOfYear( data.EndTotalDay );

        return _pageViewDataBuilder.CreateWeeklyLedgerPage(
            data, displayWeek,
            startMonth, startDay,
            endMonth, endDay );
    }
    #endregion

#if UNITY_EDITOR
    #region ----- 검증 -----
    /// <summary>
    /// 월, 연도, 상세 화면 검증용 주간 결산 데이터 생성
    /// </summary>
    [ContextMenu( "가계부 검증 데이터 생성" )]
    void DebugCreateWeeklySettlements ()
    {
        if ( Application.isPlaying == false )
        {
            Debug.Log( "Play Mode에서 확인해 주세요." );
            return;
        }

        _debugWeeklySettlements.Clear( );

        //두 번째 연도 4주차까지 생성하되 일부 주차는 미결산 상태로 유지
        for ( int week = 1; week <= 56; week++ )
        {
            if ( week % 4 == 0 ) continue;

            _debugWeeklySettlements.Add(
                CreateDebugWeeklySettlement( week ) );
        }

        Debug.Log(
            "가계부 검증용 주간 결산 데이터 생성 완료 - 가계부 버튼으로 확인해 주세요." );
    }

    /// <summary>
    /// 지정 주차의 가계부 검증 데이터 생성
    /// </summary>
    /// <param name="week">생성할 누적 결산 주차</param>
    /// <returns>가계부 검증용 주간 결산</returns>
    WeeklySettlementData CreateDebugWeeklySettlement ( int week )
    {
        int endTotalDay = week * 7;
        float income = 10000f + week * 750f;
        float expense = 4000f + week * 300f;

        return new WeeklySettlementData
        {
            Week = week,
            StartTotalDay = endTotalDay - 6,
            EndTotalDay = endTotalDay,
            StartBudget = 50000f + ( week - 1 ) * 5000f,
            EndBudget = 50000f + week * 5000f,
            HiredEmployeeCount = week % 4,
            Operation = new OperationSettlement
            {
                ProductionOrderCount = 1,
                CraftCompletedCount = 12,
                CraftLimit = 14,
                DiscardedCraftCount = 2,
                DeliveryStartedCount = 10,
                PendingDeliveryCount = 1,
                InDeliveryCount = 2,
                QuickRestockCount = 3,
                QuickRestockLimit = 5
            },
            Orders = new OrderSettlement
            {
                NormalDeliveryCount = 8,
                LateDeliveryCount = 2,
                RejectedCount = 1,
                AutoRejectedCount = 1,
                CancelledCount = 1,
                FailedCount = 1
            },
            Evaluation = new EvaluationSettlement
            {
                CompletedRequirementCount = 18,
                RequirementCount = 20,
                CompletedWishCount = 8,
                WishCount = 10,
                SGradeCount = 2,
                AGradeCount = 4,
                BGradeCount = 3,
                CGradeCount = 1
            },
            Economy = new EconomySettlement
            {
                OrderRewardIncome = income,
                PurchaseExpense = expense,
                QuickRestockExpense = 500f,
                DeliveryExpense = 1000f,
                EmployeeWeeklyExpense = 1500f
            },
            EvaluationOrderCount = 10,
            HasEnoughData = true,
            WeeklyScore = 70f + week % 30,
            Rating = ( WeeklyRating ) ( week % 5 + 1 ),
            NextWeekAdjustment = new WeeklyOrderAdjustment
            {
                IsApplied = true,
                OrderCountCorrection = week % 3 - 1
            }
        };
    }
    #endregion
#endif
}
