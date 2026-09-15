using System;
using UnityEngine;

/// <summary>
/// 업적 후속 가이드 프레젠터
/// </summary>
public class AchievementGuidePresenter : ITutorialGuideSection
{
    const int GuideStartDay = 2;

    const string IntroDialogueId = "Dialogue_Day6_AchievementIntro";

    const string GuideDialogueId = "Dialogue_Day6_AchievementGuide";

    enum GuidePhase
    {
        None,               //미진행
        Intro,              //업적 소개
        WaitingForPanel,    //업적 화면 진입 대기
        Guide,              //업적 내부 설명
    }

    TutorialModel _tutorialModel;               //튜토리얼 진행 상태
    PlayStateModel _playStateModel;             //현재 누적 영업일
    AchvModel _achvModel;                       //업적 상태
    SideActionPresenter _sideActionPresenter;   //업적 화면 중재
    TutorialView _tutorialView;                 //공용 강조 표시 뷰

    /// <summary>
    /// 대화 재생 함수(가이드 대사 아이디, 재생 요청 성공 여부)
    /// </summary>
    Func<string, bool> _playDialogue;
    /// <summary>
    /// 후속 가이드 요청 함수(튜토리얼 가이드 아이디, 요청 재생 성공 여부)
    /// </summary>
    Func<TutorialGuideId, bool> _requestGuide;

    GuidePhase _phase;                   //현재 업적 가이드 단계
    string _currentDialogueId;           //현재 재생 중인 대화
    bool _isAchievementOpened;           //업적 화면 표시 여부
    bool _isGuideDialogueCompleted;      //내부 설명 완료 여부
    bool _hasClaimedReward;              //실제 보상 수령 여부
    bool _isSubscribed;                  //이벤트 연결 여부

    /// <summary>
    /// 업적 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// 업적 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="achvModel">업적 상태 모델</param>
    /// <param name="sideActionPresenter">사이드 액션 프레젠터</param>
    /// <param name="tutorialView">공용 강조 표시 뷰</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public AchievementGuidePresenter (
        TutorialModel tutorialModel,
        PlayStateModel playStateModel,
        AchvModel achvModel,
        SideActionPresenter sideActionPresenter,
        TutorialView tutorialView,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _playStateModel = playStateModel;
        _achvModel = achvModel;
        _sideActionPresenter = sideActionPresenter;
        _tutorialView = tutorialView;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 업적 가이드 요청 가능 여부 확인
    /// </summary>
    /// <returns>가이드 요청 가능 여부</returns>
    bool CanRequestGuide ()
    {
        //핵심 튜토리얼 완료 여부 확인, 일자 확인,
        //보상 수령 여부 확인, 가이드 완료 여부 확인, 단계 확인
        return _tutorialModel.CoreTutorialCompleted &&
            _playStateModel.TotalDay >= GuideStartDay &&
            _achvModel.HasUnclaimedReward &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.Achievement ) == false &&
            _phase == GuidePhase.None;
    }

    /// <summary>
    /// 업적 가이드를 중복 없이 요청
    /// </summary>
    void RequestAchievementGuide ()
    {
        if ( CanRequestGuide( ) == false )
            return;

        _requestGuide( TutorialGuideId.Achievement );
    }

    /// <summary>
    /// 업적 소개 대사 재생
    /// </summary>
    /// <returns>대사 재생 성공 여부</returns>
    bool PlayIntroDialogue ()
    {
        _phase = GuidePhase.Intro;
        _currentDialogueId = IntroDialogueId;
        _isGuideDialogueCompleted = false;
        _hasClaimedReward = false;

        if ( _playDialogue( IntroDialogueId ) )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 업적 내부 설명 대사 재생
    /// </summary>
    /// <returns>대사 재생 성공 여부</returns>
    bool PlayGuideDialogue ()
    {
        if ( _isAchievementOpened == false )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;
            return false;
        }

        _tutorialView.HideInstant( );

        _phase = GuidePhase.Guide;
        _currentDialogueId = GuideDialogueId;
        _isGuideDialogueCompleted = false;

        if ( _playDialogue( GuideDialogueId ) == true )
            return true;

        _phase = GuidePhase.WaitingForPanel;
        _currentDialogueId = string.Empty;
        return false;
    }

    /// <summary>
    /// 현재 업적 UI를 튜토리얼 대상으로 등록
    /// </summary>
    /// <param name="targetId">등록할 대상 아이디</param>
    /// <returns>대상 등록 성공 여부</returns>
    bool RegisterTarget ( TutorialTargetId targetId )
    {
        if ( _sideActionPresenter.TryGetTutorialTarget(
            targetId,
            out RectTransform target,
            out Vector2 arrowOffset ) == false )
        {
            return false;
        }

        //목표 아이디, 위치와 화살표 오프셋 반환
        return _tutorialView.RegisterTarget(
            targetId, target, arrowOffset );
    }

    /// <summary>
    /// 지정 업적 대상을 화살표 없이 한 번 확대
    /// </summary>
    /// <param name="targetId">강조할 대상 아이디</param>
    /// <returns>확대 강조 성공 여부</returns>
    bool PlayTargetPunch ( TutorialTargetId targetId )
    {
        _tutorialView.HideInstant( );

        if ( RegisterTarget( targetId ) == false )
            return false;

        return _tutorialView.PlayTargetPunch( targetId );
    }

    /// <summary>
    /// 지정 업적 대상에 화살표 표시
    /// </summary>
    /// <param name="targetId">강조할 대상 아이디</param>
    /// <param name="mode">입력 제한 방식</param>
    /// <returns>강조 표시 성공 여부</returns>
    bool ShowTarget (
        TutorialTargetId targetId, TutorialGuideMode mode )
    {
        if ( RegisterTarget( targetId ) == false )
            return false;

        return _tutorialView.ShowTarget( targetId, mode );
    }

    /// <summary>
    /// 대사 완료와 실제 보상 수령 완료 여부 확인
    /// </summary>
    void TryCompleteGuide ()
    {
        //가이드 대사가 안 끝났꺼나 보상을 안 받았다면
        if ( _isGuideDialogueCompleted == false ||
            _hasClaimedReward == false )
        {
            return;
        }

        _tutorialView.HideInstant( );

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke( TutorialGuideId.Achievement );
    }

    /// <summary>
    /// 업적 후속 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>업적 가이드 담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Achievement;
    }

    /// <summary>
    /// 업적 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 후속 가이드 아이디</param>
    /// <returns>가이드 시작 성공 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( guideId != TutorialGuideId.Achievement ||
            CanRequestGuide( ) == false )
        {
            return false;
        }

        return PlayIntroDialogue( );
    }

    /// <summary>
    /// 업적 가이드 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_AchievementButton"
                when _phase == GuidePhase.Intro:
                return ShowTarget(
                    TutorialTargetId.AchievementButton,
                    TutorialGuideMode.Focus );

            case "Highlight_AchievementProgress"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.AchievementProgress );

            case "Highlight_AchievementStage"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.AchievementStage );

            case "Highlight_AchievementReward"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.AchievementReward );

            case "Highlight_AchievementClaimButton"
                when _phase == GuidePhase.Guide:
                return ShowTarget(
                    TutorialTargetId.AchievementClaimButton,
                    TutorialGuideMode.Blocking );

            default:
                return false;
        }
    }

    /// <summary>
    /// 업적 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 업적 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        //완료 대사 아이디가 현재 대사 아이디와 다르다면
        if ( dialogueId != _currentDialogueId )
            return false;

        //현재 단계가 인트로라면
        if ( _phase == GuidePhase.Intro )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;

            //패널이 열리면 대사 재생
            if ( _isAchievementOpened )
                PlayGuideDialogue( );

            return true;
        }

        //가이드 단계가 아니면 종료
        if ( _phase != GuidePhase.Guide )
            return false;

        _currentDialogueId = string.Empty;
        _isGuideDialogueCompleted = true;

        TryCompleteGuide( );
        return true;
    }

    /// <summary>
    /// 날짜와 업적 시스템 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed )
            return;

        //날짜 변경 후 가이드 조건 확인 이벤트 연결
        _playStateModel.OnDateChanged += HandleDateChanged;
        //업적 상태 변경 이벤트 연결
        _achvModel.OnNotificationChanged += HandleNotificationChanged;

        //업적 진입 이벤트 연결
        _sideActionPresenter
            .OnAchievementOpened += HandleAchievementOpened;
        //업적 화면 닫기 이벤트 연결
        _sideActionPresenter
            .OnAchievementClosed += HandleAchievementClosed;
        //보상 수령 이벤트 연결
        _sideActionPresenter
            .OnAchievementRewardClaimed += HandleRewardClaimed;

        _isSubscribed = true;

        RequestAchievementGuide( );
    }

    /// <summary>
    /// 날짜와 업적 시스템 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false )
            return;

        _playStateModel.OnDateChanged -= HandleDateChanged;
        _achvModel.OnNotificationChanged -=
            HandleNotificationChanged;

        _sideActionPresenter.OnAchievementOpened -=
            HandleAchievementOpened;
        _sideActionPresenter.OnAchievementClosed -=
            HandleAchievementClosed;
        _sideActionPresenter.OnAchievementRewardClaimed -=
            HandleRewardClaimed;

        _isSubscribed = false;
    }

    /// <summary>
    /// 날짜 변경 후 업적 가이드 조건 확인
    /// </summary>
    /// <param name="month">변경된 월</param>
    /// <param name="day">변경된 일</param>
    void HandleDateChanged ( int month, int day )
    {
        //가이드 요청
        RequestAchievementGuide( );
    }

    /// <summary>
    /// 업적 알림 변경 후 수령 가능 상태 확인
    /// </summary>
    /// <param name="hasNotification">업적 알림 존재 여부</param>
    void HandleNotificationChanged ( bool hasNotification )
    {
        if ( hasNotification )
            RequestAchievementGuide( );
    }

    /// <summary>
    /// 업적 화면 진입 후 내부 설명 시작
    /// </summary>
    void HandleAchievementOpened ()
    {
        _isAchievementOpened = true;

        if ( _phase == GuidePhase.WaitingForPanel )
            PlayGuideDialogue( );
    }

    /// <summary>
    /// 업적 화면 종료 상태 반영
    /// </summary>
    void HandleAchievementClosed ()
    {
        _isAchievementOpened = false;
        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 실제 업적 보상 수령 완료 처리
    /// </summary>
    /// <param name="achievementId">보상을 수령한 업적 아이디</param>
    void HandleRewardClaimed ( string achievementId )
    {
        if ( _phase != GuidePhase.Guide )
            return;

        _hasClaimedReward = true;
        _tutorialView.HideInstant( );

        TryCompleteGuide( );
    }

    /// <summary>
    /// 진행 중인 업적 가이드 임시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;
        _isAchievementOpened = false;
        _isGuideDialogueCompleted = false;
        _hasClaimedReward = false;
    }
}
