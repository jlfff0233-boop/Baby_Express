using System;
using System.Collections.Generic;
using UnityEngine;
// 테스트 브랜치 주석

/// <summary>
/// 정비 프레젠터 - 정비 상태 조회와 화면 표시 중재
/// </summary>
public class MaintenancePresenter : MonoBehaviour
{
    const int EmployeeUnlockDay = 7;

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] MaintenanceView _maintenanceView;       //정비 뷰

    MaintenanceModel _maintenanceModel;       //정비 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    SettlementModel _settlementModel;       //결산 모델
    MaintenanceUpgradeModel _maintenanceUpgradeModel;       //정비 업그레이드 모델
    MaintenanceViewDataBuilder _viewDataBuilder;       //정비 표시 데이터 생성기
    EmployeeModel _employeeModel;       //직원 모델
    EmployeeViewDataBuilder _employeeViewDataBuilder;       //직원 표시 데이터 생성기

    MaintenanceTab _selectedTab;       //현재 선택 탭
    string _selectedMaintenanceId;       //현재 선택 정비 아이디
    string _guideEmployeeId;       //가이드에서 우선 표시할 직원 아이디
    int _debugWeeklySettlementCount;       //직원 해금 검증용 임시 주간 결산 수
    bool _isEmployeeGuideMode;       //직원 가이드 전용 정렬 적용 여부
    bool _isInitialized;       //초기화 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 정비 화면 열림 이벤트
    /// </summary>
    public event Action OnPanelOpened;

    /// <summary>
    /// 정비 화면 닫기 이벤트
    /// </summary>
    public event Action OnPanelClosed;

    /// <summary>
    /// 고용 탭 열림 이벤트
    /// </summary>
    public event Action OnEmployeeTabOpened;

    /// <summary>
    /// 정비 가이드의 시설 상점 이동 요청 이벤트
    /// </summary>
    public event Action OnMoveToShop;

    /// <summary>
    /// 정비 가이드의 정비 상세 이동 요청 이벤트
    /// </summary>
    public event Action OnMoveToGuideDetail;

    #region ----- 초기화 -----

    /// <summary>
    /// 정비 프레젠터 초기화
    /// </summary>
    /// <param name="maintenanceModel">정비 모델</param>
    /// <param name="maintenanceUpgradeModel">정비 상위 단계 구매 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="settlementModel">결산 모델</param>
    /// <param name="employeeModel">직원 모델</param>
    public void Init (
        MaintenanceModel maintenanceModel,
        MaintenanceUpgradeModel maintenanceUpgradeModel,
        ShopModel shopModel, PlayStateModel playStateModel,
        SettlementModel settlementModel, EmployeeModel employeeModel )
    {
        //비활성 정비 뷰의 공용 풀 먼저 초기화
        _maintenanceView.InitializeRuntime( );

        _maintenanceModel = maintenanceModel;
        _maintenanceUpgradeModel = maintenanceUpgradeModel;
        _playStateModel = playStateModel;
        _settlementModel = settlementModel;
        _employeeModel = employeeModel;

        _viewDataBuilder = new MaintenanceViewDataBuilder(
            maintenanceModel, maintenanceUpgradeModel, shopModel );

        _employeeViewDataBuilder = new EmployeeViewDataBuilder( employeeModel );

        _selectedMaintenanceId = null;
        _guideEmployeeId = null;
        _debugWeeklySettlementCount = 0;
        _isEmployeeGuideMode = false;
        _isInitialized = true;

        RefreshEmployeeAccess( );

        if ( isActiveAndEnabled ) SubscribeEvents( );

        _maintenanceView.HideInstant( );
    }

    /// <summary>
    /// 정비 입력 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        if ( _isInitialized ) SubscribeEvents( );
    }

    /// <summary>
    /// 정비 입력 이벤트 해제
    /// </summary>
    void OnDisable ()
    {
        UnsubscribeEvents( );
    }

    #endregion

    #region ----- 이벤트 연결 -----

    /// <summary>
    /// 정비 입력과 상태 변경 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        //중복 연결과 초기화 전 연결 차단
        if ( _isSubscribed || _isInitialized == false ) return;

        _maintenanceView.OnManagementSelected += ShowManagement;
        _maintenanceView.OnFacilitySelected += ShowFacility;
        _maintenanceView.OnResearchSelected += ShowResearch;
        _maintenanceView.OnConvenienceSelected += ShowConvenience;
        _maintenanceView.OnEmployeeSelected += ShowEmployee;
        _maintenanceView.OnMaintenanceSelected += ShowMaintenanceDetail;
        _maintenanceView.OnClose += ClosePanel;
        _maintenanceView.OnUpgrade += UpgradeMaintenance;
        _maintenanceView.OnEmployeeAction += ChangeEmployee;
        _maintenanceView.OnDetailClose += CloseDetail;
        _employeeModel.OnEmployeeChanged += RefreshEmployee;
        _playStateModel.OnDateChanged += HandleDateChanged;

        _maintenanceModel.OnMaintenanceChanged += RefreshMaintenance;
        _isSubscribed = true;
    }

    /// <summary>
    /// 정비 입력과 상태 변경 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _maintenanceView.OnManagementSelected -= ShowManagement;
        _maintenanceView.OnFacilitySelected -= ShowFacility;
        _maintenanceView.OnResearchSelected -= ShowResearch;
        _maintenanceView.OnConvenienceSelected -= ShowConvenience;
        _maintenanceView.OnEmployeeSelected -= ShowEmployee;
        _maintenanceView.OnMaintenanceSelected -= ShowMaintenanceDetail;
        _maintenanceView.OnClose -= ClosePanel;
        _maintenanceView.OnUpgrade -= UpgradeMaintenance;
        _maintenanceView.OnEmployeeAction -= ChangeEmployee;
        _maintenanceView.OnDetailClose -= CloseDetail;
        _employeeModel.OnEmployeeChanged -= RefreshEmployee;
        _playStateModel.OnDateChanged -= HandleDateChanged;

        _maintenanceModel.OnMaintenanceChanged -= RefreshMaintenance;
        _isSubscribed = false;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 정비 화면 표시
    /// </summary>
    public void ShowPanel ()
    {
        RefreshEmployeeAccess( );
        _maintenanceView.ShowPanel( );
        ShowManagement( );
        OnPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 시설 상점 화면 이동 요청
    /// </summary>
    public void RequestShopMove ()
    {
        OnMoveToShop?.Invoke( );
    }

    /// <summary>
    /// 정비 상세 화면 이동 요청
    /// </summary>
    public void RequestGuideDetailMove ()
    {
        OnMoveToGuideDetail?.Invoke( );
    }

    /// <summary>
    /// 정비 가이드용 첫 시설 정비 상세 표시
    /// </summary>
    /// <returns>상세 표시 성공 여부</returns>
    public bool ShowGuideDetail ()
    {
        RefreshEmployeeAccess( );
        _maintenanceView.ShowPanel( );

        IReadOnlyList<MaintenanceSlotViewData> viewDatas =
            ShowMaintenanceList(
                MaintenanceTab.Facility,
                MaintenanceType.Facility );

        OnPanelOpened?.Invoke( );

        if ( viewDatas.Count == 0 )
            return false;

        ShowMaintenanceDetail( viewDatas [ 0 ].Id );
        return true;
    }

    /// <summary>
    /// 정비 화면의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        if ( targetId == TutorialTargetId.GuideEmployeeCard ||
            targetId == TutorialTargetId.EmployeeCost ||
            targetId == TutorialTargetId.EmployeeEffect )
        {
            return _maintenanceView.TryGetEmployeeTutorialTarget(
                _guideEmployeeId, targetId, out target );
        }

        return _maintenanceView.TryGetTutorialTarget(
            targetId, out target );
    }

    /// <summary>
    /// 직원 가이드용 대상 카드 우선 표시 설정
    /// </summary>
    /// <param name="isEnabled">가이드 정렬 적용 여부</param>
    /// <param name="employeeId">우선 표시할 직원 아이디</param>
    public void SetEmployeeGuideMode (
        bool isEnabled, string employeeId )
    {
        _isEmployeeGuideMode = isEnabled;
        _guideEmployeeId = isEnabled ? employeeId : null;

        if ( _maintenanceView != null &&
            _maintenanceView.IsShowing( ) &&
            _selectedTab == MaintenanceTab.Employee )
        {
            ShowEmployee( );
        }
    }

    /// <summary>
    /// 사용자 입력으로 정비 화면 닫기
    /// </summary>
    void ClosePanel ()
    {
        HidePanel( () => OnPanelClosed?.Invoke( ) );
    }

    /// <summary>
    /// 정비 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        _selectedMaintenanceId = null;
        _maintenanceView.HidePanel( onComplete );
    }

    /// <summary>
    /// 정비 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _selectedMaintenanceId = null;
        _maintenanceView.HideInstant( );
    }

    /// <summary>
    /// 정비 상세 닫기
    /// </summary>
    void CloseDetail ()
    {
        _selectedMaintenanceId = null;
        _maintenanceView.HideDetail( );
    }

    /// <summary>
    /// 관리 탭 표시
    /// </summary>
    void ShowManagement ()
    {
        _selectedTab = MaintenanceTab.Management;
        _selectedMaintenanceId = null;

        _maintenanceView.ShowManagement(
            _viewDataBuilder.CreateManagementViewData(
                _playStateModel.TotalDay, GetLatestWeeklyRating( ),
                _employeeViewDataBuilder.CreateManagementSummary( ) ) );
    }

    /// <summary>
    /// 시설 탭 표시
    /// </summary>
    void ShowFacility ()
    {
        ShowMaintenanceList(
            MaintenanceTab.Facility,
            MaintenanceType.Facility );
    }

    /// <summary>
    /// 연구 탭 표시
    /// </summary>
    void ShowResearch ()
    {
        ShowMaintenanceList(
            MaintenanceTab.Research,
            MaintenanceType.Research );
    }

    /// <summary>
    /// 편의 탭 표시
    /// </summary>
    void ShowConvenience ()
    {
        ShowMaintenanceList(
            MaintenanceTab.Convenience,
            MaintenanceType.Convenience );
    }

    /// <summary>
    /// 직원 가이드 중 대상 직원을 목록 첫 번째로 배치
    /// </summary>
    /// <param name="viewDatas">기존 직원 표시 데이터 목록</param>
    /// <returns>가이드 정렬을 적용한 직원 표시 데이터 목록</returns>
    IReadOnlyList<EmployeeSlotViewData> CreateEmployeeGuideOrder (
        IReadOnlyList<EmployeeSlotViewData> viewDatas )
    {
        if ( _isEmployeeGuideMode == false ||
            string.IsNullOrEmpty( _guideEmployeeId ) )
        {
            return viewDatas;
        }

        int guideIndex = -1;

        for ( int i = 0; i < viewDatas.Count; i++ )
        {
            if ( viewDatas [ i ].EmployeeId == _guideEmployeeId )
            {
                guideIndex = i;
                break;
            }
        }

        if ( guideIndex <= 0 )
            return viewDatas;

        var ordered = new List<EmployeeSlotViewData>( viewDatas.Count )
        {
            viewDatas [ guideIndex ]
        };

        for ( int i = 0; i < viewDatas.Count; i++ )
        {
            if ( i != guideIndex )
                ordered.Add( viewDatas [ i ] );
        }

        return ordered;
    }

    /// <summary>
    /// 고용 탭 표시
    /// </summary>
    void ShowEmployee ()
    {
        if ( CanOpenEmployee( ) == false )
            return;

        _selectedTab = MaintenanceTab.Employee;
        _selectedMaintenanceId = null;

        IReadOnlyList<EmployeeSlotViewData> viewDatas =
            _employeeViewDataBuilder.CreateViewDatas(
                GetWeeklySettlementCount( ) );

        _maintenanceView.ShowEmployeeContent(
            CreateEmployeeGuideOrder( viewDatas ) );

        OnEmployeeTabOpened?.Invoke( );
    }

    /// <summary>
    /// 정비 종류별 목록 표시
    /// </summary>
    /// <param name="tab">선택 탭</param>
    /// <param name="type">정비 종류</param>
    IReadOnlyList<MaintenanceSlotViewData> ShowMaintenanceList (
        MaintenanceTab tab, MaintenanceType type )
    {
        _selectedTab = tab;
        _selectedMaintenanceId = null;

        IReadOnlyList<MaintenanceState> states =
            _maintenanceModel.GetStates( type );

        IReadOnlyList<MaintenanceSlotViewData> viewDatas =
            _viewDataBuilder.CreateSlotViewDatas(
                states, _playStateModel.TotalDay,
                GetLatestWeeklyRating( ) );

        _maintenanceView.ShowMaintenanceList( viewDatas );
        return viewDatas;
    }

    /// <summary>
    /// 선택 정비 상세 표시
    /// </summary>
    /// <param name="id">정비 아이디</param>
    void ShowMaintenanceDetail ( string id )
    {
        if ( _maintenanceModel.GetState(
            id, out MaintenanceState state ) == false )
            return;

        _selectedMaintenanceId = id;
        _maintenanceView.ShowDetail(
            _viewDataBuilder.CreateDetailViewData(
                state, _playStateModel.TotalDay,
                GetLatestWeeklyRating( ) ) );
    }

    #endregion

    #region ----- 상태 조회 -----

    /// <summary>
    /// 현재 날짜에 고용 탭을 사용할 수 있는지 확인
    /// </summary>
    /// <returns>고용 탭 사용 가능 여부</returns>
    bool CanOpenEmployee ()
    {
        return _playStateModel != null &&
            _playStateModel.TotalDay >= EmployeeUnlockDay;
    }

    /// <summary>
    /// 현재 날짜에 맞춰 고용 탭 입력 상태 갱신
    /// </summary>
    void RefreshEmployeeAccess ()
    {
        _maintenanceView
            .SetEmployeeInteractable( CanOpenEmployee( ) );
    }

    /// <summary>
    /// 날짜 변경 후 고용 탭 입력 상태 갱신
    /// </summary>
    /// <param name="month">현재 월</param>
    /// <param name="day">현재 일</param>
    void HandleDateChanged ( int month, int day )
    {
        RefreshEmployeeAccess( );
    }

    /// <summary>
    /// 최근 주간 영업 평가 반환
    /// </summary>
    WeeklyRating GetLatestWeeklyRating ()
    {
        //주간 결산 데이터 가져오기
        IReadOnlyList<WeeklySettlementData> settlements =
            _settlementModel.WeeklySettlements;

        if ( settlements.Count == 0 )
            return WeeklyRating.Insufficient;

        return settlements [ settlements.Count - 1 ].Rating;
    }

    /// <summary>
    /// 완료된 주간 결산 수 반환
    /// </summary>
    /// <returns>완료된 주간 결산 수</returns>
    int GetWeeklySettlementCount ()
    {
        return Math.Max(
            _settlementModel.WeeklySettlements.Count,
            _debugWeeklySettlementCount );
    }

    /// <summary>
    /// 다음 직원 해금 조건을 충족하도록 검증용 주간 결산 수 설정
    /// </summary>
    [ContextMenu( "다음 직원 해금 조건 충족" )]
    void DebugUnlockNextEmployee ()
    {
        if ( _isInitialized == false )
        {
            Debug.LogWarning( "직원 해금 디버그는 Play Mode에서 사용해 주세요." );
            return;
        }

        int currentCount = GetWeeklySettlementCount( );
        int nextRequiredCount = int.MaxValue;

        foreach ( EmployeeState state in _employeeModel.States )
        {
            int requiredCount = state.Data.RequiredWeeklySettlementCount;

            if ( requiredCount > currentCount )
                nextRequiredCount = Math.Min( nextRequiredCount, requiredCount );
        }

        if ( nextRequiredCount == int.MaxValue )
        {
            Debug.Log( "모든 직원의 해금 조건을 이미 충족했습니다." );
            return;
        }

        //실제 결산 기록은 변경하지 않고 현재 실행 중에만 다음 해금 조건 충족
        _debugWeeklySettlementCount = nextRequiredCount;

        if ( _maintenanceView.IsShowing( ) )
        {
            if ( _selectedTab == MaintenanceTab.Employee )
                ShowEmployee( );
            else if ( _selectedTab == MaintenanceTab.Management )
                ShowManagement( );
        }

        Debug.Log(
            $"직원 해금 검증용 주간 결산 수 설정: {nextRequiredCount}회" );
    }
    #endregion

    #region ----- 상태 변경 -----

    /// <summary>
    /// 선택 정비 상위 단계 구매
    /// </summary>
    /// <param name="id">정비 아이디</param>
    void UpgradeMaintenance ( string id )
    {
        //최근 주간 영업 평가 조회
        WeeklyRating rating = GetLatestWeeklyRating( );

        //정비 상위 단계 구매
        MaintenanceResult result = _maintenanceUpgradeModel.Upgrade(
            id, _playStateModel.TotalDay, rating,
            out float paidCost );

        if ( result == MaintenanceResult.Success )
        {
            Debug.Log( $"정비 완료: {id}, 지출: {paidCost:N0}G" );
            return;
        }

        Debug.LogWarning( $"정비 실패: {GetMaintenanceResultText( result )}" );

        //실패 원인을 반영하도록 상세 정보 갱신
        ShowMaintenanceDetail( id );
    }

    /// <summary>
    /// 정비 처리 결과 문구 반환
    /// </summary>
    /// <param name="result">정비 처리 결과</param>
    /// <returns>정비 처리 결과 문구</returns>
    string GetMaintenanceResultText ( MaintenanceResult result )
    {
        switch ( result )
        {
            case MaintenanceResult.InvalidData:
                return "정비 데이터를 확인해 주세요.";

            case MaintenanceResult.NotFound:
                return "정비 항목을 찾을 수 없습니다.";

            case MaintenanceResult.NotOwned:
                return "상점에서 먼저 구매해야 합니다.";

            case MaintenanceResult.MaxLevel:
                return "이미 최대 단계입니다.";

            case MaintenanceResult.RequirementNotMet:
                return "선행 조건을 달성하지 못했습니다.";

            case MaintenanceResult.EffectPending:
                return "이전 단계 효과가 적용 대기 중입니다.";

            case MaintenanceResult.InsufficientBudget:
                return "자금이 부족합니다.";

            case MaintenanceResult.EffectApplyFailed:
                return "정비 효과를 적용하지 못했습니다. 정비 상태는 변경되지 않았습니다.";

            case MaintenanceResult.RollbackFailed:
                return "정비 실패 후 상태 복구에 실패했습니다.";

            default:
                return "정비를 완료하지 못했습니다.";
        }
    }

    /// <summary>
    /// 정비 상태 변경 후 현재 화면 갱신
    /// </summary>
    void RefreshMaintenance ( string id )
    {
        if ( _maintenanceView.IsShowing( ) == false ) return;

        string selectedId = _selectedMaintenanceId;
        RefreshSelectedTab( );

        if ( selectedId == id )
            ShowMaintenanceDetail( id );
    }

    /// <summary>
    /// 현재 선택 탭 갱신
    /// </summary>
    void RefreshSelectedTab ()
    {
        switch ( _selectedTab )
        {
            case MaintenanceTab.Management:
                ShowManagement( );
                break;

            case MaintenanceTab.Facility:
                ShowFacility( );
                break;

            case MaintenanceTab.Research:
                ShowResearch( );
                break;

            case MaintenanceTab.Convenience:
                ShowConvenience( );
                break;

            case MaintenanceTab.Employee:
                ShowEmployee( );
                break;
        }
    }

    /// <summary>
    /// 직원 고용 또는 해고
    /// </summary>
    /// <param name="id">직원 아이디</param>
    void ChangeEmployee ( string id )
    {
        if ( _employeeModel.GetState(
            id, out EmployeeState state ) == false )
        {
            Debug.LogWarning( "직원 정보를 찾을 수 없습니다." );
            return;
        }

        EmployeeResult result;
        float paidCost = 0f;

        if ( state.IsHired )
        {
            result = _employeeModel.Fire( id );
        }
        else
        {
            result = _employeeModel.Hire(
                id, GetWeeklySettlementCount( ),
                out paidCost );
        }

        if ( result == EmployeeResult.Success )
        {
            string action = state.IsHired
                ? "고용"
                : "해고";

            Debug.Log(
                $"직원 {action} 완료: {id}, " +
                $"지출: {paidCost:N0}G" );
            return;
        }

        Debug.LogWarning(
            $"직원 처리 실패: " +
            _employeeViewDataBuilder.GetResultText( result ) );

        //실패 원인을 현재 카드 상태에 반영
        ShowEmployee( );
    }

    /// <summary>
    /// 직원 상태 변경 후 현재 화면 갱신
    /// </summary>
    /// <param name="id">변경된 직원 아이디</param>
    void RefreshEmployee ( string id )
    {
        if ( _maintenanceView.IsShowing( ) == false ) return;

        if ( _selectedTab == MaintenanceTab.Employee )
            ShowEmployee( );
        else if ( _selectedTab == MaintenanceTab.Management )
            ShowManagement( );
    }

    #endregion
}
