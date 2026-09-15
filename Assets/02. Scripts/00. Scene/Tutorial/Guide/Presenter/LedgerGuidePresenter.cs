using System;
using UnityEngine;

/// <summary>
/// 첫 주간 결산 이후 가계부 후속 가이드 프레젠터
/// </summary>
public class LedgerGuidePresenter : ITutorialGuideSection
{
    const string IntroDialogueId = "Dialogue_Day7_LedgerIntro";

    const string GuideDialogueId = "Dialogue_Day7_LedgerGuide";

    enum GuidePhase
    {
        None,               //미진행
        Intro,              //가계부 소개
        WaitingForPanel,    //가계부 진입 대기
        Guide,              //가계부 내부 설명
    }

    TutorialModel _tutorialModel;               //튜토리얼 진행 상태
    SettlementModel _settlementModel;           //완료된 주간 결산
    SideActionPresenter _sideActionPresenter;   //가계부 화면 중재
    TutorialView _tutorialView;                 //공용 강조 표시 뷰

    Func<string, bool> _playDialogue;            //공용 대화 재생 함수
    Func<TutorialGuideId, bool> _requestGuide;  //후속 가이드 요청 함수

    GuidePhase _phase;                   //현재 가이드 단계
    string _currentDialogueId;           //현재 재생 중인 대화
    bool _isLedgerOpened;                //가계부 표시 여부
    bool _isSubscribed;                  //이벤트 연결 여부

    /// <summary>
    /// 가계부 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// 가계부 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="settlementModel">결산 기록 모델</param>
    /// <param name="sideActionPresenter">사이드 액션 프레젠터</param>
    /// <param name="tutorialView">공용 강조 표시 뷰</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public LedgerGuidePresenter (
        TutorialModel tutorialModel,
        SettlementModel settlementModel,
        SideActionPresenter sideActionPresenter,
        TutorialView tutorialView,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _settlementModel = settlementModel;
        _sideActionPresenter = sideActionPresenter;
        _tutorialView = tutorialView;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 가계부 가이드 요청 가능 여부 확인
    /// </summary>
    /// <returns>가이드 요청 가능 여부</returns>
    bool CanRequestGuide ()
    {
        return _tutorialModel.CoreTutorialCompleted &&
            _settlementModel.WeeklySettlements.Count > 0 &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.WeeklySettlement ) &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.Ledger ) == false &&
            _phase == GuidePhase.None;
    }

    /// <summary>
    /// 가계부 가이드를 중복 없이 요청
    /// </summary>
    void RequestLedgerGuide ()
    {
        if ( CanRequestGuide( ) )
            _requestGuide( TutorialGuideId.Ledger );
    }

    /// <summary>
    /// 가계부 소개 대사 재생
    /// </summary>
    /// <returns>재생 성공 여부</returns>
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
    /// 가계부 내부 설명 대사 재생
    /// </summary>
    /// <returns>재생 성공 여부</returns>
    bool PlayGuideDialogue ()
    {
        if ( _isLedgerOpened == false )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;
            return false;
        }

        _tutorialView.HideInstant( );

        _phase = GuidePhase.Guide;
        _currentDialogueId = GuideDialogueId;

        if ( _playDialogue( GuideDialogueId ) )
            return true;

        _phase = GuidePhase.WaitingForPanel;
        _currentDialogueId = string.Empty;
        return false;
    }

    /// <summary>
    /// 가계부 UI를 공용 튜토리얼 대상으로 등록
    /// </summary>
    /// <param name="targetId">등록할 대상 아이디</param>
    /// <returns>등록 성공 여부</returns>
    bool RegisterTarget ( TutorialTargetId targetId )
    {
        if ( _sideActionPresenter.TryGetTutorialTarget(
            targetId,
            out RectTransform target,
            out Vector2 arrowOffset ) == false )
        {
            return false;
        }

        return _tutorialView.RegisterTarget(
            targetId, target, arrowOffset );
    }

    /// <summary>
    /// 지정 가계부 대상에 화살표 표시
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
    /// 지정 가계부 대상을 확대 강조
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
    /// 가계부 가이드 완료 처리
    /// </summary>
    void CompleteGuide ()
    {
        _tutorialView.HideInstant( );

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke( TutorialGuideId.Ledger );
    }

    /// <summary>
    /// 가계부 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 가이드 아이디</param>
    /// <returns>담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Ledger;
    }

    /// <summary>
    /// 가계부 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <returns>시작 성공 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( guideId != TutorialGuideId.Ledger ||
            CanRequestGuide( ) == false )
        {
            return false;
        }

        return PlayIntroDialogue( );
    }

    /// <summary>
    /// 가계부 가이드 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_LedgerButton"
                when _phase == GuidePhase.Intro:
                return ShowTarget(
                    TutorialTargetId.LedgerButton );

            case "Highlight_LedgerWeekSelector"
                when _phase == GuidePhase.Guide:
                return PlayTargetPunch(
                    TutorialTargetId.LedgerWeekSelector );

            default:
                return false;
        }
    }

    /// <summary>
    /// 가계부 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 가계부 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId != _currentDialogueId )
            return false;

        if ( _phase == GuidePhase.Intro )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;

            if ( _isLedgerOpened )
                PlayGuideDialogue( );

            return true;
        }

        if ( _phase != GuidePhase.Guide )
            return false;

        CompleteGuide( );
        return true;
    }

    /// <summary>
    /// 주간 결산과 가계부 화면 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed )
            return;

        _sideActionPresenter.OnLedgerOpened += HandleLedgerOpened;
        _sideActionPresenter.OnLedgerClosed += HandleLedgerClosed;
        _sideActionPresenter.OnWeeklySettlementConfirmed +=
            HandleWeeklySettlementConfirmed;

        _isSubscribed = true;

        RequestLedgerGuide( );
    }

    /// <summary>
    /// 주간 결산과 가계부 화면 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false )
            return;

        _sideActionPresenter.OnLedgerOpened -= HandleLedgerOpened;
        _sideActionPresenter.OnLedgerClosed -= HandleLedgerClosed;
        _sideActionPresenter.OnWeeklySettlementConfirmed -=
            HandleWeeklySettlementConfirmed;

        _isSubscribed = false;
    }

    /// <summary>
    /// 첫 주간 결산 확인 후 가계부 안내 요청
    /// </summary>
    void HandleWeeklySettlementConfirmed ()
    {
        RequestLedgerGuide( );
    }

    /// <summary>
    /// 가계부 진입 후 내부 설명 시작
    /// </summary>
    void HandleLedgerOpened ()
    {
        _isLedgerOpened = true;

        if ( _phase == GuidePhase.WaitingForPanel )
            PlayGuideDialogue( );
    }

    /// <summary>
    /// 가계부 종료 상태 반영
    /// </summary>
    void HandleLedgerClosed ()
    {
        _isLedgerOpened = false;
        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 진행 중인 가계부 가이드 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;
        _isLedgerOpened = false;
    }
}
