using System;
using UnityEngine;

/// <summary>
/// 결산 튜토리얼 프레젠터 - 일일 결산 구역 안내와 확인 결과 중재
/// </summary>
public class TutorialSettlementPresenter : ITutorialSection
{
    const string SettlementStartDialogueId = "Dialogue_24_Day1_SettlementStart";
    const string SettlementOrderDialogueId = "Dialogue_25_Day1_SettlementOrder";
    const string SettlementEconomyDialogueId = "Dialogue_26_Day1_SettlementEconomy";
    const string SettlementMoneyDialogueId = "Dialogue_27_Day1_SettlementMoney";
    const string TutorialEndDialogueId = "Dialogue_28_Day1_End";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialView _tutorialView;       //튜토리얼 표시 뷰
    SettlementPresenter _settlementPresenter;       //결산 표시와 입력 프레젠터

    Func<string, bool> _playDialogue;       //즉시 대화 재생
    Action<CoreTutorialStep> _completeStep;       //핵심 단계 완료

    bool _isGuideStarted;       //결산 안내 시작 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 결산 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialView">튜토리얼 표시 뷰</param>
    /// <param name="settlementPresenter">결산 표시와 입력 프레젠터</param>
    /// <param name="playDialogue">즉시 대화 재생 함수</param>
    /// <param name="completeStep">핵심 단계 완료 함수</param>
    public TutorialSettlementPresenter (
        TutorialModel tutorialModel,
        TutorialView tutorialView,
        SettlementPresenter settlementPresenter,
        Func<string, bool> playDialogue,
        Action<CoreTutorialStep> completeStep )
    {
        _tutorialModel = tutorialModel;
        _tutorialView = tutorialView;
        _settlementPresenter = settlementPresenter;
        _playDialogue = playDialogue;
        _completeStep = completeStep;
    }

    /// <summary>
    /// 결산 튜토리얼 담당 단계 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>결산 튜토리얼 담당 단계 여부</returns>
    public bool Handles ( CoreTutorialStep step )
    {
        return step == CoreTutorialStep.Settlement;
    }

    /// <summary>
    /// 현재 결산 튜토리얼 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 또는 대기 여부</returns>
    public bool Begin ( CoreTutorialStep step )
    {
        if ( step != CoreTutorialStep.Settlement )
            return false;

        _isGuideStarted = false;

        //결산 화면이 아직 열리지 않았으면 표시 이벤트까지 대기
        if ( _settlementPresenter.IsShowingDaily == false )
            return true;

        return BeginSettlementGuide( );
    }

    /// <summary>
    /// 결산 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>결산 튜토리얼에서 처리한 신호 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_SettlementOrderResult":
                return ShowSectionTarget(
                    SettlementSectionType.OrderResult,
                    TutorialTargetId.SettlementOrderResult );

            case "Highlight_SettlementIncomeExpense":
                return ShowSectionTarget(
                    SettlementSectionType.Economy,
                    TutorialTargetId.SettlementEconomy );

            case "Highlight_SettlementFinalMoney":
                return ShowSectionTarget(
                    SettlementSectionType.Summary,
                    TutorialTargetId.SettlementSummary );

            case "Highlight_NextDayButton":
                //대화 종료 후 결산 확인 버튼 입력을 허용
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 결산 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>결산 튜토리얼에서 처리한 대화 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId == SettlementStartDialogueId )
        {
            _playDialogue( SettlementOrderDialogueId );
            return true;
        }

        if ( dialogueId == SettlementOrderDialogueId )
        {
            _playDialogue( SettlementEconomyDialogueId );
            return true;
        }

        if ( dialogueId == SettlementEconomyDialogueId )
        {
            _playDialogue( SettlementMoneyDialogueId );
            return true;
        }

        if ( dialogueId == SettlementMoneyDialogueId )
        {
            _playDialogue( TutorialEndDialogueId );
            return true;
        }

        if ( dialogueId == TutorialEndDialogueId )
        {
            _tutorialView.ShowTarget(
                TutorialTargetId.SettlementConfirmButton,
                TutorialGuideMode.Blocking );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 결산 행동 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _settlementPresenter.OnDailyShown += HandleDailyShown;
        _settlementPresenter.OnDailyConfirmed +=
            HandleDailyConfirmed;

        _isSubscribed = true;
    }

    /// <summary>
    /// 결산 행동 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _settlementPresenter.OnDailyShown -= HandleDailyShown;
        _settlementPresenter.OnDailyConfirmed -=
            HandleDailyConfirmed;

        _isSubscribed = false;
    }

    /// <summary>
    /// 일일 결산 화면 표시 후 결산 안내 시작
    /// </summary>
    void HandleDailyShown ()
    {
        if ( _tutorialModel.CurrentCoreStep !=
            CoreTutorialStep.Settlement )
        {
            return;
        }

        BeginSettlementGuide( );
    }

    /// <summary>
    /// 일일 결산 확인 완료 후 핵심 튜토리얼 완료
    /// </summary>
    void HandleDailyConfirmed ()
    {
        if ( _tutorialModel.CurrentCoreStep !=
            CoreTutorialStep.Settlement )
        {
            return;
        }

        _tutorialView.HideInstant( );
        _completeStep( CoreTutorialStep.Settlement );
    }

    /// <summary>
    /// 결산 시작 대화 재생
    /// </summary>
    /// <returns>결산 안내 시작 여부</returns>
    bool BeginSettlementGuide ()
    {
        if ( _isGuideStarted )
            return true;

        if ( _playDialogue( SettlementStartDialogueId ) == false )
            return false;

        _isGuideStarted = true;
        return true;
    }

    /// <summary>
    /// 결산 구역으로 이동한 뒤 런타임 강조 대상 표시
    /// </summary>
    /// <param name="sectionType">표시할 결산 구역</param>
    /// <param name="targetId">연결할 튜토리얼 대상 아이디</param>
    /// <returns>결산 구역 표시 여부</returns>
    bool ShowSectionTarget (
        SettlementSectionType sectionType,
        TutorialTargetId targetId )
    {
        if ( _settlementPresenter.TryFocusSection(
            sectionType, out RectTransform target ) == false )
        {
            return true;
        }

        _tutorialView.SetRuntimeTarget( targetId, target );

        return _tutorialView.ShowTarget(
            targetId, TutorialGuideMode.Focus );
    }
}
