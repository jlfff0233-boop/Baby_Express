using System;
using UnityEngine;

/// <summary>
/// 첫 주간 결산 후속 가이드 프레젠터
/// </summary>
public class WeeklySettlementGuidePresenter : ITutorialGuideSection
{
    const string GuideDialogueId =
        "Dialogue_Day7_WeeklySettlementGuide";

    TutorialModel _tutorialModel;               //튜토리얼 진행 상태
    SettlementPresenter _settlementPresenter;   //결산 화면 중재
    TutorialView _tutorialView;                 //공용 강조 표시 뷰

    Func<string, bool> _playDialogue;            //공용 대화 재생 함수
    Func<TutorialGuideId, bool> _requestGuide;  //후속 가이드 요청 함수

    string _currentDialogueId;       //현재 재생 중인 대화
    bool _isWeeklyOpened;            //주간 결산 표시 여부
    bool _isRunning;                 //가이드 진행 여부
    bool _isSubscribed;              //이벤트 연결 여부

    /// <summary>
    /// 첫 주간 결산 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// 첫 주간 결산 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="settlementPresenter">결산 프레젠터</param>
    /// <param name="tutorialView">공용 강조 표시 뷰</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public WeeklySettlementGuidePresenter (
        TutorialModel tutorialModel,
        SettlementPresenter settlementPresenter,
        TutorialView tutorialView,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _settlementPresenter = settlementPresenter;
        _tutorialView = tutorialView;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 첫 주간 결산 가이드 요청 가능 여부 확인
    /// </summary>
    /// <returns>가이드 요청 가능 여부</returns>
    bool CanRequestGuide ()
    {
        return _tutorialModel.CoreTutorialCompleted &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.WeeklySettlement ) == false &&
            _isWeeklyOpened &&
            _isRunning == false;
    }

    /// <summary>
    /// 첫 주간 결산 가이드 요청
    /// </summary>
    void RequestWeeklyGuide ()
    {
        if ( CanRequestGuide( ) )
            _requestGuide( TutorialGuideId.WeeklySettlement );
    }

    /// <summary>
    /// 지정 결산 구역을 화면에 표시하고 확대 강조
    /// </summary>
    /// <param name="targetId">등록할 튜토리얼 대상</param>
    /// <param name="sectionType">이동할 결산 구역</param>
    /// <returns>강조 성공 여부</returns>
    bool PlaySectionPunch (
        TutorialTargetId targetId,
        SettlementSectionType sectionType )
    {
        _tutorialView.HideInstant( );

        if ( _settlementPresenter.TryFocusSection(
            sectionType, out RectTransform target ) == false )
        {
            return false;
        }

        if ( _tutorialView.RegisterTarget(
            targetId, target, Vector2.zero ) == false )
        {
            return false;
        }

        return _tutorialView.PlayTargetPunch( targetId );
    }

    /// <summary>
    /// 첫 주간 결산 가이드 완료 처리
    /// </summary>
    void CompleteGuide ()
    {
        _tutorialView.HideInstant( );

        _isRunning = false;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke(
            TutorialGuideId.WeeklySettlement );
    }

    /// <summary>
    /// 첫 주간 결산 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 가이드 아이디</param>
    /// <returns>담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.WeeklySettlement;
    }

    /// <summary>
    /// 첫 주간 결산 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <returns>시작 성공 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( guideId != TutorialGuideId.WeeklySettlement ||
            CanRequestGuide( ) == false )
        {
            return false;
        }

        _isRunning = true;
        _currentDialogueId = GuideDialogueId;

        if ( _playDialogue( GuideDialogueId ) )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 주간 결산 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        if ( _isRunning == false )
            return false;

        switch ( signalId )
        {
            case "Highlight_WeeklyDeliverySummary":
                return PlaySectionPunch(
                    TutorialTargetId.WeeklyDeliverySummary,
                    SettlementSectionType.Operation );

            case "Highlight_WeeklyGradeDistribution":
                return PlaySectionPunch(
                    TutorialTargetId.WeeklyGradeDistribution,
                    SettlementSectionType.Evaluation );

            case "Highlight_WeeklyRequirementRate":
                return PlaySectionPunch(
                    TutorialTargetId.WeeklyRequirementRate,
                    SettlementSectionType.Evaluation );

            case "Highlight_WeeklyFinance":
                return PlaySectionPunch(
                    TutorialTargetId.WeeklyFinance,
                    SettlementSectionType.Economy );

            case "Highlight_WeeklyBusinessGrade":
                return PlaySectionPunch(
                    TutorialTargetId.WeeklyBusinessGrade,
                    SettlementSectionType.Summary );

            default:
                return false;
        }
    }

    /// <summary>
    /// 주간 결산 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 가이드 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( _isRunning == false ||
            dialogueId != _currentDialogueId )
        {
            return false;
        }

        CompleteGuide( );
        return true;
    }

    /// <summary>
    /// 주간 결산 화면 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed )
            return;

        _settlementPresenter.OnWeeklyShown += HandleWeeklyShown;
        _settlementPresenter.OnWeeklyConfirmed += HandleWeeklyConfirmed;

        _isSubscribed = true;
    }

    /// <summary>
    /// 주간 결산 화면 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false )
            return;

        _settlementPresenter.OnWeeklyShown -= HandleWeeklyShown;
        _settlementPresenter.OnWeeklyConfirmed -= HandleWeeklyConfirmed;

        _isSubscribed = false;
    }

    /// <summary>
    /// 첫 주간 결산 표시 후 가이드 요청
    /// </summary>
    void HandleWeeklyShown ()
    {
        _isWeeklyOpened = true;
        RequestWeeklyGuide( );
    }

    /// <summary>
    /// 주간 결산 확인 후 표시 상태 초기화
    /// </summary>
    void HandleWeeklyConfirmed ()
    {
        _isWeeklyOpened = false;
        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 진행 중인 주간 결산 가이드 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _currentDialogueId = string.Empty;
        _isWeeklyOpened = false;
        _isRunning = false;
    }
}
