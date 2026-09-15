using System;

/// <summary>
/// Day 4 정비 소개 후속 가이드 프레젠터
/// </summary>
public class MaintenanceGuidePresenter : ITutorialGuideSection
{
    const int GuideStartDay = 4;

    const float CategoryPunchInterval = 0.45f;

    const float DetailPunchInterval = 0.55f;

    const string IntroDialogueId = "Dialogue_Day4_MaintenanceIntro";

    const string GuideDialogueId = "Dialogue_Day4_MaintenanceGuide";

    enum GuidePhase
    {
        None,       //없음
        Intro,      //인트로
        WaitingForPanel,        //대기
        Guide,      //가이드
    }

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태
    PlayStateModel _playStateModel;       //현재 누적 영업일
    MaintenancePresenter _maintenancePresenter;       //정비 화면 상태
    TutorialView _tutorialView;       //공용 강조 표시 뷰

    Func<string, bool> _playDialogue;       //공용 대화 재생 함수
    Func<TutorialGuideId, bool> _requestGuide;       //가이드 요청 함수

    GuidePhase _phase;       //현재 정비 가이드 진행 단계
    string _currentDialogueId;       //현재 재생 중인 대화
    bool _isMaintenancePanelOpened;       //정비 패널 표시 여부
    bool _isDetailPunchQueued;       //다음 단계 신호까지 포함한 상세 순차 강조 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 정비 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// Day 4 정비 소개 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="maintenancePresenter">정비 화면 프레젠터</param>
    /// <param name="tutorialView">공용 튜토리얼 강조 표시 뷰</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public MaintenanceGuidePresenter (
        TutorialModel tutorialModel,
        PlayStateModel playStateModel,
        MaintenancePresenter maintenancePresenter,
        TutorialView tutorialView,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _playStateModel = playStateModel;
        _maintenancePresenter = maintenancePresenter;
        _tutorialView = tutorialView;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 정비 가이드 요청 가능 여부 확인
    /// </summary>
    /// <returns>정비 가이드 요청 가능 여부</returns>
    bool CanRequestGuide ()
    {
        return _tutorialModel.CoreTutorialCompleted &&
            _playStateModel.TotalDay >= GuideStartDay &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.Maintenance ) == false &&
            _phase == GuidePhase.None;
    }

    /// <summary>
    /// 정비 가이드 실제 시작 가능 여부 확인
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <returns>가이드 시작 가능 여부</returns>
    bool CanBeginGuide ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Maintenance &&
            CanRequestGuide( );
    }

    /// <summary>
    /// 정비 소개 대화 재생
    /// </summary>
    /// <returns>대화 재생 성공 여부</returns>
    bool PlayIntroDialogue ()
    {
        _phase = GuidePhase.Intro;
        _currentDialogueId = IntroDialogueId;

        if ( _playDialogue( IntroDialogueId ) )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 정비 화면 설명 대화 재생
    /// </summary>
    /// <returns>대화 재생 성공 여부</returns>
    bool PlayGuideDialogue ()
    {
        if ( _isMaintenancePanelOpened == false )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;
            return false;
        }

        //메인 정비 버튼에 남아 있던 화살표 제거
        _tutorialView.HideInstant( );

        _phase = GuidePhase.Guide;
        _currentDialogueId = GuideDialogueId;

        if ( _playDialogue( GuideDialogueId ) )
            return true;

        //데이터가 준비되지 않은 경우 패널 재진입 시 다시 시도
        _phase = GuidePhase.WaitingForPanel;
        _currentDialogueId = string.Empty;
        return false;
    }

    /// <summary>
    /// 정비 가이드를 중복 없이 요청
    /// </summary>
    void RequestMaintenanceGuide ()
    {
        if ( CanRequestGuide( ) == false )
            return;

        _requestGuide( TutorialGuideId.Maintenance );
    }

    /// <summary>
    /// 화살표 없이 지정된 정비 항목을 한 번 확대 강조
    /// </summary>
    /// <param name="targetId">강조할 정비 대상 아이디</param>
    /// <returns>강조 재생 성공 여부</returns>
    bool PlayTargetPunch ( TutorialTargetId targetId )
    {
        if ( _isMaintenancePanelOpened == false )
            return false;

        return _tutorialView.PlayTargetPunch( targetId );
    }

    /// <summary>
    /// 정비의 시설, 연구, 편의 탭을 차례대로 확대 강조
    /// </summary>
    /// <returns>순차 강조 시작 여부</returns>
    bool PlayCategoryPunchSequence ()
    {
        if ( _isMaintenancePanelOpened == false )
            return false;

        return _tutorialView.PlayTargetPunchSequence(
            CategoryPunchInterval,
            TutorialTargetId.MaintenanceFacilityTab,
            TutorialTargetId.MaintenanceResearchTab,
            TutorialTargetId.MaintenanceConvenienceTab );
    }

    /// <summary>
    /// 시설 카테고리를 선택한 상점으로 이동
    /// </summary>
    /// <returns>이동 신호 처리 여부</returns>
    bool MoveToMaintenanceShop ()
    {
        _tutorialView.HideInstant( );
        _isMaintenancePanelOpened = false;
        _maintenancePresenter.RequestShopMove( );
        return true;
    }

    /// <summary>
    /// 첫 시설 정비 상세를 열고 현재와 다음 단계를 차례대로 확대 강조
    /// </summary>
    /// <returns>상세 이동 신호 처리 여부</returns>
    bool OpenDetailAndPlayLevelSequence ()
    {
        _tutorialView.HideInstant( );
        _maintenancePresenter.RequestGuideDetailMove( );

        _isDetailPunchQueued =
            _tutorialView.PlayTargetPunchSequence(
                DetailPunchInterval,
                TutorialTargetId.MaintenanceCurrentLevel,
                TutorialTargetId.MaintenanceNextLevel );

        return true;
    }

    /// <summary>
    /// 현재 단계 신호에서 예약한 다음 단계 강조의 중복 실행 방지
    /// </summary>
    /// <returns>다음 단계 신호 처리 여부</returns>
    bool HandleNextLevelPunch ()
    {
        if ( _isDetailPunchQueued )
        {
            _isDetailPunchQueued = false;
            return true;
        }

        return PlayTargetPunch(
            TutorialTargetId.MaintenanceNextLevel );
    }

    /// <summary>
    /// 정비 후속 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>정비 후속 가이드 담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Maintenance;
    }

    /// <summary>
    /// Day 4 정비 소개 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 후속 가이드 아이디</param>
    /// <returns>가이드 시작 성공 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( CanBeginGuide( guideId ) == false )
            return false;

        return PlayIntroDialogue( );
    }

    /// <summary>
    /// 정비 가이드 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_MaintenanceButton"
                when _phase == GuidePhase.Intro:
                return _tutorialView.ShowTarget(
                    TutorialTargetId.MaintenanceButton,
                    TutorialGuideMode.Focus );

            case "Highlight_MaintenanceCategories"
                when _phase == GuidePhase.Guide:
                return PlayCategoryPunchSequence( );

            case "Highlight_MaintenanceShopLink"
                when _phase == GuidePhase.Guide:
                return MoveToMaintenanceShop( );

            case "Highlight_MaintenanceCurrentLevel"
                when _phase == GuidePhase.Guide:
                return OpenDetailAndPlayLevelSequence( );

            case "Highlight_MaintenanceNextLevel"
                when _phase == GuidePhase.Guide:
                return HandleNextLevelPunch( );

            case "Highlight_MaintenanceFacility"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.MaintenanceFacilityTab );

            case "Highlight_MaintenanceResearch"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.MaintenanceResearchTab );

            case "Highlight_MaintenanceConvenience"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.MaintenanceConvenienceTab );

            default:
                return false;
        }
    }

    /// <summary>
    /// 정비 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 정비 가이드 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId != _currentDialogueId )
            return false;

        if ( _phase == GuidePhase.Intro )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;

            if ( _isMaintenancePanelOpened )
                PlayGuideDialogue( );

            return true;
        }

        if ( _phase != GuidePhase.Guide )
            return false;

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke( TutorialGuideId.Maintenance );
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
        _maintenancePresenter.OnPanelOpened += HandleMaintenancePanelOpened;
        _maintenancePresenter.OnPanelClosed += HandleMaintenancePanelClosed;

        _isSubscribed = true;

        //Day 4 이상 세이브를 불러온 경우에도 최초 안내 요청
        RequestMaintenanceGuide( );
    }

    /// <summary>
    /// 날짜와 정비 화면 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false )
            return;

        _playStateModel.OnDateChanged -= HandleDateChanged;
        _maintenancePresenter.OnPanelOpened -= HandleMaintenancePanelOpened;
        _maintenancePresenter.OnPanelClosed -= HandleMaintenancePanelClosed;

        _isSubscribed = false;
    }

    /// <summary>
    /// 날짜 변경 후 Day 4 정비 소개 조건 확인
    /// </summary>
    /// <param name="month">변경된 월</param>
    /// <param name="day">변경된 일</param>
    void HandleDateChanged ( int month, int day )
    {
        RequestMaintenanceGuide( );
    }

    /// <summary>
    /// 정비 화면 진입 후 대기 중인 화면 설명 시작
    /// </summary>
    void HandleMaintenancePanelOpened ()
    {
        _isMaintenancePanelOpened = true;

        if ( _phase == GuidePhase.WaitingForPanel )
            PlayGuideDialogue( );
    }

    /// <summary>
    /// 정비 화면 종료 상태 반영
    /// </summary>
    void HandleMaintenancePanelClosed ()
    {
        _isMaintenancePanelOpened = false;

        //비활성 탭을 계속 참조하지 않도록 강조만 정리
        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 진행 중인 정비 가이드 임시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;
        _isDetailPunchQueued = false;
    }
}
