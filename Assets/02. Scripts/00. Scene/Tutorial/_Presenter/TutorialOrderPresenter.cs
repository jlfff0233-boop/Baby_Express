using System;
using UnityEngine;

/// <summary>
/// 주문 튜토리얼 프레젠터 - 주문 확인과 수락 행동 유도 중재
/// </summary>
public class TutorialOrderPresenter : ITutorialSection
{
    const string Day1StartDialogueId = "Dialogue_0_Day1_Start";
    const string OrderCheckDialogueId = "Dialogue_1_Day1_OrderCheck";
    const string RequirementDialogueId = "Dialogue_2_Day1_Requirement";
    const string WishDialogueId = "Dialogue_3_Day1_Wish";
    const string AcceptDialogueId = "Dialogue_4_Day1_Accept";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 데이터
    TutorialView _tutorialView;       //튜토리얼 표시 뷰
    OrderPresenter _orderPresenter;       //주문 행동 결과 프레젠터
    /// <summary>
    /// 즉시 대화 재생(대사 아이디, 재생 성공 여부)
    /// </summary>
    Func<string , bool> _playDialogue;
    /// <summary>
    /// 현재 대화 이후 재생 예약(대사 아이디)
    /// </summary>
    Action<string> _queueDialogue;
    Action<CoreTutorialStep> _completeStep;       //핵심 단계 완료
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 주문 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 튜토리얼 데이터</param>
    /// <param name="tutorialView">튜토리얼 표시 뷰</param>
    /// <param name="orderPresenter">주문 행동 결과 프레젠터</param>
    /// <param name="playDialogue">즉시 대화 재생 함수</param>
    /// <param name="queueDialogue">현재 대화 이후 재생 예약 함수</param>
    /// <param name="completeStep">핵심 단계 완료 함수</param>
    public TutorialOrderPresenter (
        TutorialModel tutorialModel ,
        TutorialDay1Data tutorialDay1Data ,
        TutorialView tutorialView ,
        OrderPresenter orderPresenter ,
        Func<string , bool> playDialogue ,
        Action<string> queueDialogue ,
        Action<CoreTutorialStep> completeStep )
    {
        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
        _tutorialView = tutorialView;
        _orderPresenter = orderPresenter;
        _playDialogue = playDialogue;
        _queueDialogue = queueDialogue;
        _completeStep = completeStep;
    }

    /// <summary>
    /// 주문 튜토리얼 담당 단계 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>주문 튜토리얼 담당 단계 여부</returns>
    public bool Handles ( CoreTutorialStep step )
    {
        return step == CoreTutorialStep.OrderDetail ||
            step == CoreTutorialStep.OrderAccept;
    }

    /// <summary>
    /// 현재 주문 튜토리얼 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 여부</returns>
    public bool Begin ( CoreTutorialStep step )
    {
        switch ( step )
        {
            case CoreTutorialStep.OrderDetail:
                return _playDialogue ( Day1StartDialogueId );

            case CoreTutorialStep.OrderAccept:
                return _playDialogue ( AcceptDialogueId );

            default:
                return false;
        }
    }

    /// <summary>
    /// 주문 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>주문 튜토리얼에서 처리한 신호 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        TutorialTargetId targetId;

        switch ( signalId )
        {
            case "Highlight_OrderButton":
                //대화 패널 퇴장 완료 후 주문 버튼 입력 안내
                return true;

            case "Highlight_TutorialOrder":
                //대화 패널 퇴장 완료 후 주문 슬롯 입력 안내
                return true;

            case "Highlight_Requirement":
                targetId = TutorialTargetId.Requirement;
                break;

            case "Highlight_Wish":
                targetId = TutorialTargetId.Wish;
                break;

            case "Highlight_OrderDeadline":
                targetId = TutorialTargetId.OrderDeadline;
                break;

            case "Highlight_AcceptButton":
                //대화 상자보다 화살표가 먼저 나타나지 않도록
                //대화 종료 후 실제 입력 안내를 표시
                return true;

            default:
                return false;
        }

        //설명 대상은 화살표 없이 한 번 확대 후 원래 크기로 복구
        _tutorialView.PlayTargetPunch ( targetId );
        return true;
    }

    /// <summary>
    /// 주문 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>주문 튜토리얼에서 처리한 대화 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId == Day1StartDialogueId )
        {
            //주문 버튼 강조 연출
            _tutorialView.ShowTarget (
                TutorialTargetId.OrderButton , TutorialGuideMode.Blocking );
            return true;
        }

        if ( dialogueId == OrderCheckDialogueId )
        {
            ShowTutorialOrderTarget ( );
            return true;
        }

        //주요 요구 사항 설명 이후 희망 사항 재생 예약
        if ( dialogueId == RequirementDialogueId )
        {
            _playDialogue ( WishDialogueId );
            return true;
        }

        if ( dialogueId == WishDialogueId )
        {
            _completeStep ( CoreTutorialStep.OrderDetail );
            return true;
        }

        if ( dialogueId == AcceptDialogueId )
        {
            //주문 수락 버튼 강조 연출
            _tutorialView.ShowTarget (
                TutorialTargetId.OrderAcceptButton , TutorialGuideMode.Blocking );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 튜토리얼 주문 슬롯을 런타임 대상으로 연결하고 표시
    /// </summary>
    void ShowTutorialOrderTarget ( )
    {
        if ( _orderPresenter.TryGetSlotTarget (
            _tutorialDay1Data.OrderId , out RectTransform target ) == false )
        {
            return;
        }

        _tutorialView.SetRuntimeTarget (
            TutorialTargetId.TutorialOrderSlot , target );
        _tutorialView.ShowTarget (
            TutorialTargetId.TutorialOrderSlot , TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 주문 행동 이벤트 연결
    /// </summary>
    public void SubscribeEvents ( )
    {
        if ( _isSubscribed ) return;

        _orderPresenter.OnPanelOpened += HandleOrderPanelOpened;
        _orderPresenter.OnDetailOpened += HandleOrderDetailOpened;
        _orderPresenter.OnOrderAccepted += HandleOrderAccepted;

        _isSubscribed = true;
    }

    /// <summary>
    /// 주문 행동 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ( )
    {
        if ( _isSubscribed == false ) return;

        _orderPresenter.OnPanelOpened -= HandleOrderPanelOpened;
        _orderPresenter.OnDetailOpened -= HandleOrderDetailOpened;
        _orderPresenter.OnOrderAccepted -= HandleOrderAccepted;

        _isSubscribed = false;
    }

    /// <summary>
    /// 실제 주문 상태를 기준으로 주문 단계 복구
    /// </summary>
    public void Recover ( )
    {
        if ( IsTutorialOrderAccepted ( ) == false ) return;

        if ( _tutorialModel.CurrentCoreStep == CoreTutorialStep.OrderDetail )
            _tutorialModel.CompleteCoreStep ( CoreTutorialStep.OrderDetail );

        if ( _tutorialModel.CurrentCoreStep == CoreTutorialStep.OrderAccept )
            _tutorialModel.CompleteCoreStep ( CoreTutorialStep.OrderAccept );
    }

    /// <summary>
    /// 튜토리얼 주문이 이미 수락됐는지 확인
    /// </summary>
    /// <returns>주문 수락 이후 상태 여부</returns>
    bool IsTutorialOrderAccepted ( )
    {
        if ( _orderPresenter.TryGetOrderProgress (
            _tutorialDay1Data.OrderId , out OrderProgressState progress ) == false )
        {
            return false;
        }

        return progress == OrderProgressState.Production ||
            progress == OrderProgressState.Crafted ||
            progress == OrderProgressState.Shipping ||
            progress == OrderProgressState.Closed;
    }

    /// <summary>
    /// 주문 패널 진입 후 주문 확인 대화 시작
    /// </summary>
    void HandleOrderPanelOpened ( )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.OrderDetail )
        {
            return;
        }

        _tutorialView.HideInstant ( );
        _queueDialogue ( OrderCheckDialogueId );
    }

    /// <summary>
    /// 튜토리얼 주문 상세 선택 후 조건 설명 시작
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    void HandleOrderDetailOpened ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.OrderDetail ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.HideInstant ( );
        _queueDialogue ( RequirementDialogueId );
    }

    /// <summary>
    /// 튜토리얼 주문 수락 후 다음 단계 진행
    /// </summary>
    /// <param name="orderId">수락한 주문 아이디</param>
    void HandleOrderAccepted ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.OrderAccept ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.HideInstant ( );
        //주문 화면 퇴장 후 상점 안내 단계 시작
        _orderPresenter.HidePanel (
            ( ) => _completeStep ( CoreTutorialStep.OrderAccept ) );
    }
}
