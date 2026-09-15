using System;
using UnityEngine;

/// <summary>
/// Day 7 직원 후속 가이드 프레젠터
/// </summary>
public class EmployeeGuidePresenter : ITutorialGuideSection
{
    const int GuideStartDay = 7;

    const string IntroDialogueId = "Dialogue_Day7_EmployeeIntro_Niamh";

    const string GuideDialogueId = "Dialogue_Day7_EmployeeGuide";

    enum GuidePhase
    {
        None,                   //미진행
        Intro,                  //직원 등장 대화
        WaitingForPanel,        //정비 화면 진입 대기
        WaitingForEmployee,     //고용 탭 진입 대기
        Guide,                  //직원 카드 설명
    }

    TutorialModel _tutorialModel;               //튜토리얼 진행 상태
    PlayStateModel _playStateModel;             //현재 누적 영업일
    MaintenancePresenter _maintenancePresenter; //정비와 직원 화면 중재
    TutorialView _tutorialView;                 //공용 강조 표시 뷰

    Func<string, bool> _playDialogue;            //공용 대화 재생 함수
    Func<TutorialGuideId, bool> _requestGuide;  //후속 가이드 요청 함수

    string _guideEmployeeId;             //가이드 대상 직원 아이디
    string _currentDialogueId;           //현재 재생 중인 대화
    GuidePhase _phase;                   //현재 가이드 단계
    bool _isPanelOpened;                 //정비 화면 표시 여부
    bool _isEmployeeOpened;              //고용 탭 표시 여부
    bool _isGuideModeApplied;            //직원 가이드 정렬 적용 여부
    bool _isSubscribed;                  //이벤트 연결 여부

    /// <summary>
    /// 직원 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// Day 7 직원 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="maintenancePresenter">정비 프레젠터</param>
    /// <param name="tutorialView">공용 강조 표시 뷰</param>
    /// <param name="guideEmployeeId">가이드 대상 직원 아이디</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public EmployeeGuidePresenter (
        TutorialModel tutorialModel,
        PlayStateModel playStateModel,
        MaintenancePresenter maintenancePresenter,
        TutorialView tutorialView,
        string guideEmployeeId,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _playStateModel = playStateModel;
        _maintenancePresenter = maintenancePresenter;
        _tutorialView = tutorialView;
        _guideEmployeeId = guideEmployeeId;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 직원 가이드 요청 가능 여부 확인
    /// </summary>
    /// <returns>가이드 요청 가능 여부</returns>
    bool CanRequestGuide ()
    {
        return _tutorialModel.CoreTutorialCompleted &&
            _playStateModel.TotalDay >= GuideStartDay &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.Employee ) == false &&
            _phase == GuidePhase.None;
    }

    /// <summary>
    /// 직원 가이드를 중복 없이 요청
    /// </summary>
    void RequestEmployeeGuide ()
    {
        if ( CanRequestGuide( ) )
            _requestGuide( TutorialGuideId.Employee );
    }

    /// <summary>
    /// 직원 소개 대화 재생
    /// </summary>
    /// <returns>재생 성공 여부</returns>
    bool PlayIntroDialogue ()
    {
        ApplyGuideMode( );

        _phase = GuidePhase.Intro;
        _currentDialogueId = IntroDialogueId;

        if ( _playDialogue( IntroDialogueId ) )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 직원 카드 설명 대화 재생
    /// </summary>
    /// <returns>재생 성공 여부</returns>
    bool PlayGuideDialogue ()
    {
        if ( _isEmployeeOpened == false )
        {
            _phase = GuidePhase.WaitingForEmployee;
            _currentDialogueId = string.Empty;
            return false;
        }

        _tutorialView.HideInstant( );

        _phase = GuidePhase.Guide;
        _currentDialogueId = GuideDialogueId;

        if ( _playDialogue( GuideDialogueId ) )
            return true;

        _phase = GuidePhase.WaitingForEmployee;
        _currentDialogueId = string.Empty;
        return false;
    }

    /// <summary>
    /// 대상 직원을 고용 목록 최상단에 배치
    /// </summary>
    void ApplyGuideMode ()
    {
        if ( _isGuideModeApplied )
            return;

        _maintenancePresenter.SetEmployeeGuideMode( true, _guideEmployeeId );

        _isGuideModeApplied = true;
    }

    /// <summary>
    /// 직원 목록의 기존 정렬 상태 복구
    /// </summary>
    void RestoreGuideMode ()
    {
        if ( _isGuideModeApplied == false )
            return;

        _maintenancePresenter.SetEmployeeGuideMode( false, string.Empty );

        _isGuideModeApplied = false;
    }

    /// <summary>
    /// 동적 직원 UI를 튜토리얼 대상으로 등록
    /// </summary>
    /// <param name="targetId">등록할 대상 아이디</param>
    /// <returns>등록 성공 여부</returns>
    bool RegisterTarget ( TutorialTargetId targetId )
    {
        //메인 정비 버튼은 초기화 때 등록한 ActionView 고정 대상을 사용
        if ( targetId == TutorialTargetId.MaintenanceButton )
            return true;

        if ( _maintenancePresenter.TryGetTutorialTarget(
            targetId, out RectTransform target ) == false )
        {
            return false;
        }

        return _tutorialView.RegisterTarget(
            targetId, target, Vector2.zero );
    }

    /// <summary>
    /// 지정 대상에 화살표 표시
    /// </summary>
    /// <param name="targetId">강조할 대상 아이디</param>
    /// <returns>표시 성공 여부</returns>
    bool ShowTarget ( TutorialTargetId targetId )
    {
        if ( RegisterTarget( targetId ) == false )
            return false;

        return _tutorialView.ShowTarget(
            targetId, TutorialGuideMode.Focus );
    }

    /// <summary>
    /// 지정 대상을 화살표 없이 확대 강조
    /// </summary>
    /// <param name="targetId">강조할 대상 아이디</param>
    /// <returns>강조 성공 여부</returns>
    bool PlayTargetPunch ( TutorialTargetId targetId )
    {
        _tutorialView.HideInstant( );

        if ( RegisterTarget( targetId ) == false )
            return false;

        return _tutorialView.PlayTargetPunch( targetId );
    }

    /// <summary>
    /// 직원 가이드 완료 처리
    /// </summary>
    void CompleteGuide ()
    {
        _tutorialView.HideInstant( );
        RestoreGuideMode( );

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke( TutorialGuideId.Employee );
    }

    /// <summary>
    /// 직원 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 가이드 아이디</param>
    /// <returns>직원 가이드 담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Employee;
    }

    /// <summary>
    /// 직원 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <returns>시작 성공 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( guideId != TutorialGuideId.Employee ||
            CanRequestGuide( ) == false )
        {
            return false;
        }

        return PlayIntroDialogue( );
    }

    /// <summary>
    /// 직원 가이드 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_EmployeeButton"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.MaintenanceEmployeeTab );

            case "Highlight_DullahanEmployeeCard"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.GuideEmployeeCard );

            case "Highlight_EmployeeHireCost"
                when _phase == GuidePhase.Guide:
            case "Highlight_EmployeeWeeklySalary"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.EmployeeCost );

            case "Highlight_EmployeeEffect"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.EmployeeEffect );

            default:
                return false;
        }
    }

    /// <summary>
    /// 직원 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 가이드 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId != _currentDialogueId )
            return false;

        if ( _phase == GuidePhase.Intro )
        {
            _currentDialogueId = string.Empty;

            if ( _isPanelOpened )
            {
                _phase = GuidePhase.WaitingForEmployee;

                ShowTarget(
                    TutorialTargetId.MaintenanceEmployeeTab );
            }
            else
            {
                _phase = GuidePhase.WaitingForPanel;

                ShowTarget(
                    TutorialTargetId.MaintenanceButton );
            }

            return true;
        }

        if ( _phase != GuidePhase.Guide )
            return false;

        CompleteGuide( );
        return true;
    }

    /// <summary>
    /// 날짜와 정비 화면 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed )
            return;

        _playStateModel.OnDateChanged += HandleDateChanged;
        _maintenancePresenter.OnPanelOpened += HandlePanelOpened;
        _maintenancePresenter.OnPanelClosed += HandlePanelClosed;
        _maintenancePresenter.OnEmployeeTabOpened +=
            HandleEmployeeTabOpened;

        _isSubscribed = true;

        RequestEmployeeGuide( );
    }

    /// <summary>
    /// 날짜와 정비 화면 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false )
            return;

        _playStateModel.OnDateChanged -= HandleDateChanged;
        _maintenancePresenter.OnPanelOpened -= HandlePanelOpened;
        _maintenancePresenter.OnPanelClosed -= HandlePanelClosed;
        _maintenancePresenter.OnEmployeeTabOpened -=
            HandleEmployeeTabOpened;

        _isSubscribed = false;
    }

    /// <summary>
    /// 날짜 변경 후 직원 가이드 조건 확인
    /// </summary>
    /// <param name="month">현재 월</param>
    /// <param name="day">현재 일</param>
    void HandleDateChanged ( int month, int day )
    {
        RequestEmployeeGuide( );
    }

    /// <summary>
    /// 정비 화면 진입 처리
    /// </summary>
    void HandlePanelOpened ()
    {
        _isPanelOpened = true;

        if ( _phase != GuidePhase.WaitingForPanel )
            return;

        _tutorialView.HideInstant( );
        _phase = GuidePhase.WaitingForEmployee;

        ShowTarget(
            TutorialTargetId.MaintenanceEmployeeTab );
    }

    /// <summary>
    /// 정비 화면 종료 처리
    /// </summary>
    void HandlePanelClosed ()
    {
        _isPanelOpened = false;
        _isEmployeeOpened = false;
        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 고용 탭 진입 후 직원 카드 설명 시작
    /// </summary>
    void HandleEmployeeTabOpened ()
    {
        _isEmployeeOpened = true;

        if ( _phase == GuidePhase.WaitingForEmployee )
            PlayGuideDialogue( );
    }

    /// <summary>
    /// 진행 중인 직원 가이드 임시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        RestoreGuideMode( );

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;
        _isPanelOpened = false;
        _isEmployeeOpened = false;
    }
}
