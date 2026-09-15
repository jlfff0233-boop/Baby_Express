using System;
using UnityEngine;

/// <summary>
/// 배송 튜토리얼 프레젠터 - 직접 배송 안내와 출발 결과 중재
/// </summary>
public class TutorialDeliveryPresenter : ITutorialSection
{
    const string DirectDeliveryDialogueId = "Dialogue_21_Day1_DirectDelivery";
    const string DeliverySummaryDialogueId = "Dialogue_22_Day1_DeliverySummary";
    const string DeliveryStartDialogueId = "Dialogue_23_Day1_DeliveryStart";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 데이터
    TutorialView _tutorialView;       //튜토리얼 표시 뷰
    DeliveryPresenter _deliveryPresenter;       //배송 행동 결과 프레젠터
    CustomerOrderModel _orderModel;       //튜토리얼 주문 상태 모델

    Func<string, bool> _playDialogue;       //즉시 대화 재생
    Action<CoreTutorialStep> _completeStep;       //핵심 단계 완료

    bool _isSubscribed;       //이벤트 연결 여부
    bool _isWaitingDirectDelivery;       //직접 배송 토글 입력 대기 여부
    bool _isTutorialDeliveryClosing;       //튜토리얼 배송 화면 퇴장 대기 여부

    /// <summary>
    /// 배송 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 튜토리얼 데이터</param>
    /// <param name="tutorialView">튜토리얼 표시 뷰</param>
    /// <param name="deliveryPresenter">배송 행동 결과 프레젠터</param>
    /// <param name="orderModel">튜토리얼 주문 상태 모델</param>
    /// <param name="playDialogue">즉시 대화 재생 함수</param>
    /// <param name="completeStep">핵심 단계 완료 함수</param>
    public TutorialDeliveryPresenter (
        TutorialModel tutorialModel,
        TutorialDay1Data tutorialDay1Data,
        TutorialView tutorialView,
        DeliveryPresenter deliveryPresenter,
        CustomerOrderModel orderModel,
        Func<string, bool> playDialogue,
        Action<CoreTutorialStep> completeStep )
    {
        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
        _tutorialView = tutorialView;
        _deliveryPresenter = deliveryPresenter;
        _orderModel = orderModel;
        _playDialogue = playDialogue;
        _completeStep = completeStep;
    }

    /// <summary>
    /// 배송 튜토리얼 담당 단계 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>배송 튜토리얼 담당 단계 여부</returns>
    public bool Handles ( CoreTutorialStep step )
    {
        return step == CoreTutorialStep.Delivery;
    }

    /// <summary>
    /// 배송 튜토리얼 입력 대기 상태 초기화
    /// </summary>
    public void ResetGuideState ()
    {
        _isWaitingDirectDelivery = false;
        _deliveryPresenter.SetTutorialMethodSelectionPending( false );
    }

    /// <summary>
    /// 현재 배송 튜토리얼 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 여부</returns>
    public bool Begin ( CoreTutorialStep step )
    {
        if ( step != CoreTutorialStep.Delivery )
            return false;

        //이미 배송을 출발했거나 완료했다면 다음 단계로 복구
        if ( IsTutorialOrderPastCrafted( ) )
        {
            _completeStep( CoreTutorialStep.Delivery );
            return true;
        }

        if ( IsTutorialOrderCrafted( ) == false )
            return false;

        //불러오기 후 배송 화면이 닫혀 있으면 다시 표시
        if ( _deliveryPresenter.IsShowingPreview(
            _tutorialDay1Data.OrderId ) == false )
        {
            _deliveryPresenter.ShowDelivery(
                _tutorialDay1Data.OrderId );
        }

        if ( _deliveryPresenter.IsShowingPreview(
            _tutorialDay1Data.OrderId ) == false )
        {
            return false;
        }

        //대사 종료 후 직접 선택하도록 대기
        _deliveryPresenter.SetTutorialMethodSelectionPending( true );
        _isWaitingDirectDelivery = false;

        bool started = _playDialogue( DirectDeliveryDialogueId );

        if ( started == false )
            ResetGuideState( );

        return started;
    }

    /// <summary>
    /// 실제 주문 상태를 기준으로 배송 단계 복구
    /// </summary>
    public void Recover ()
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.Delivery ||
            IsTutorialOrderPastCrafted( ) == false )
        {
            return;
        }

        _tutorialModel.CompleteCoreStep(
            CoreTutorialStep.Delivery );
    }

    /// <summary>
    /// 배송 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>배송 튜토리얼에서 처리한 신호 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        TutorialTargetId targetId;
        TutorialGuideMode mode;

        switch ( signalId )
        {
            case "Highlight_DirectDelivery":
                //직접 배송 강조는 대화 패널이 완전히 닫힌 뒤 표시
                return true;

            case "Highlight_DeliverySummary":
                targetId = TutorialTargetId.DeliverySummary;
                mode = TutorialGuideMode.Focus;
                break;

            case "Highlight_DeliveryStart":
                //대화 종료 후 배송 시작 버튼 입력을 허용
                return true;

            default:
                return false;
        }

        _tutorialView.ShowTarget( targetId, mode );
        return true;
    }

    /// <summary>
    /// 배송 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>배송 튜토리얼에서 처리한 대화 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId == DirectDeliveryDialogueId )
        {
            _isWaitingDirectDelivery = true;

            _tutorialView.ShowTarget(
                TutorialTargetId.DirectDelivery,
                TutorialGuideMode.Blocking );

            return true;
        }

        if ( dialogueId == DeliverySummaryDialogueId )
        {
            _playDialogue( DeliveryStartDialogueId );
            return true;
        }

        if ( dialogueId == DeliveryStartDialogueId )
        {
            _tutorialView.ShowTarget(
                TutorialTargetId.DeliveryStartButton,
                TutorialGuideMode.Blocking );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 직접 배송 토글 입력 후 다음 배송 안내 재생
    /// </summary>
    /// <param name="method">사용자가 선택한 배송 방식</param>
    void HandleDeliveryMethodSelected ( DeliveryMethod method )
    {
        if ( _isWaitingDirectDelivery == false ||
            method != DeliveryMethod.Direct )
        {
            return;
        }

        _isWaitingDirectDelivery = false;
        _tutorialView.HideInstant( );
        _playDialogue( DeliverySummaryDialogueId );
    }

    /// <summary>
    /// 배송 행동 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _deliveryPresenter.OnPreviewShown += HandlePreviewShown;
        _deliveryPresenter.OnDeliveryMethodSelected +=
            HandleDeliveryMethodSelected;
        _deliveryPresenter.OnDeliveryStarted +=
            HandleDeliveryStarted;
        _deliveryPresenter.OnDeliveryClosed += HandleDeliveryClosed;

        _isSubscribed = true;
    }

    /// <summary>
    /// 배송 행동 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _deliveryPresenter.OnPreviewShown -= HandlePreviewShown;
        _deliveryPresenter.OnDeliveryMethodSelected -=
            HandleDeliveryMethodSelected;
        _deliveryPresenter.OnDeliveryStarted -=
            HandleDeliveryStarted;
        _deliveryPresenter.OnDeliveryClosed -= HandleDeliveryClosed;

        ResetGuideState( );
        _isTutorialDeliveryClosing = false;
        _isSubscribed = false;
    }

    /// <summary>
    /// 배송 예상 화면 표시 후 제작 결과 단계 완료
    /// </summary>
    /// <param name="orderId">표시한 배송 주문 아이디</param>
    void HandlePreviewShown ( string orderId )
    {
        if ( orderId != _tutorialDay1Data.OrderId )
            return;

        //Day 1 배송 화면은 직접 배송 상태로 표시
        _deliveryPresenter.TrySelectDeliveryMethod(
            DeliveryMethod.Direct );

        if ( _tutorialModel.CurrentCoreStep !=
            CoreTutorialStep.CraftComplete )
        {
            return;
        }

        _completeStep( CoreTutorialStep.CraftComplete );
    }

    /// <summary>
    /// 튜토리얼 주문 직접 배송 성공 후 배송 단계 완료
    /// </summary>
    /// <param name="orderId">출발한 배송 주문 아이디</param>
    /// <param name="method">사용한 배송 방식</param>
    void HandleDeliveryStarted (
        string orderId, DeliveryMethod method )
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.Delivery ||
            orderId != _tutorialDay1Data.OrderId ||
            method != DeliveryMethod.Direct )
        {
            return;
        }

        _tutorialView.HideInstant( );
        //배송 화면 퇴장 완료까지 단계 전환 대기
        _isTutorialDeliveryClosing = true;
    }

    /// <summary>
    /// 튜토리얼 배송 화면 퇴장 후 배송 단계 완료
    /// </summary>
    void HandleDeliveryClosed ()
    {
        if ( _isTutorialDeliveryClosing == false ) return;

        _isTutorialDeliveryClosing = false;
        _completeStep( CoreTutorialStep.Delivery );
    }

    /// <summary>
    /// 튜토리얼 주문 제작 완료 여부 확인
    /// </summary>
    /// <returns>배송 대기 상태 여부</returns>
    bool IsTutorialOrderCrafted ()
    {
        return _orderModel.GetOrder(
            _tutorialDay1Data.OrderId, out CustomerOrder order ) &&
            order.ProgressState == OrderProgressState.Crafted;
    }

    /// <summary>
    /// 튜토리얼 주문이 배송 단계 이후인지 확인
    /// </summary>
    /// <returns>배송 중이거나 종료된 상태 여부</returns>
    bool IsTutorialOrderPastCrafted ()
    {
        //고객 주문 가져오기
        if ( _orderModel.GetOrder( _tutorialDay1Data.OrderId,
            out CustomerOrder order ) == false )
        {
            return false;
        }

        return order.ProgressState == OrderProgressState.Shipping ||
            order.ProgressState == OrderProgressState.Closed;
    }
}
