using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 튜토리얼 프레젠터 - 파츠 배치와 제작 정보 행동 유도 중재
/// </summary>
public class TutorialCraftPresenter : ITutorialSection
{
    const string CraftOrderDialogueId = "Dialogue_13_Day1_CraftOrder";
    const string FreePlaceDialogueId = "Dialogue_14_Day1_FreePlace";
    const string BodyPlaceDialogueId = "Dialogue_15_Day1_BodyPlace";
    const string MovePartDialogueId = "Dialogue_16_Day1_MovePart";
    const string CraftCostDialogueId = "Dialogue_17_Day1_CraftCost";
    const string OrderConditionsDialogueId = "Dialogue_18_Day1_OrderConditions";
    const string CraftCompleteDialogueId = "Dialogue_19_Day1_CraftComplete";
    const string CraftResultDialogueId = "Dialogue_20_Day1_Result";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 데이터
    TutorialView _tutorialView;       //튜토리얼 표시 뷰
    CraftPresenter _craftPresenter;       //제작 행동 결과 프레젠터
    InventoryModel _inventoryModel;       //튜토리얼 파츠 확인 모델
    CustomerOrderModel _orderModel;       //튜토리얼 주문 상태 모델
    /// <summary>
    /// 즉시 대화 재생(대사 아이디, 재생 성공 여부)
    /// </summary>
    Func<string, bool> _playDialogue;
    /// <summary>
    /// 현재 대화 이후 재생 예약(대사 아이디)
    /// </summary>
    Action<string> _queueDialogue;
    Action<CoreTutorialStep> _completeStep;       //핵심 단계 완료

    int _tutorialEyePlacementNumber;       //편집을 안내할 눈 파츠 배치 번호
    bool _wasTutorialEyeMoved;       //안내한 눈 파츠 이동 완료 여부
    bool _wasTutorialEyeScaled;       //안내한 눈 파츠 크기 변경 완료 여부
    bool _isMovePartDialogueCompleted;       //눈 편집 안내 대사 완료 여부
    bool _isWaitingPartsPanelOpen;       //파츠 선택 패널 열림 대기 여부
    bool _isWaitingCraftInfoOpen;       //제작 정보 패널 열림 대기 여부
    bool _isWaitingUsedPartsOpen;       //사용 파츠 Content 표시 대기 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 제작 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 튜토리얼 데이터</param>
    /// <param name="tutorialView">튜토리얼 표시 뷰</param>
    /// <param name="craftPresenter">제작 행동 결과 프레젠터</param>
    /// <param name="inventoryModel">튜토리얼 파츠 확인 모델</param>
    /// <param name="orderModel">튜토리얼 주문 상태 모델</param>
    /// <param name="playDialogue">즉시 대화 재생 함수</param>
    /// <param name="queueDialogue">현재 대화 이후 재생 예약 함수</param>
    /// <param name="completeStep">핵심 단계 완료 함수</param>
    public TutorialCraftPresenter (
        TutorialModel tutorialModel,
        TutorialDay1Data tutorialDay1Data,
        TutorialView tutorialView,
        CraftPresenter craftPresenter,
        InventoryModel inventoryModel, CustomerOrderModel orderModel,
        Func<string, bool> playDialogue,
        Action<string> queueDialogue,
        Action<CoreTutorialStep> completeStep )
    {
        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
        _tutorialView = tutorialView;
        _craftPresenter = craftPresenter;
        _inventoryModel = inventoryModel;
        _orderModel = orderModel;
        _playDialogue = playDialogue;
        _queueDialogue = queueDialogue;
        _completeStep = completeStep;
    }

    /// <summary>
    /// 제작 튜토리얼 담당 단계 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>제작 튜토리얼 담당 단계 여부</returns>
    public bool Handles ( CoreTutorialStep step )
    {
        return step == CoreTutorialStep.Craft ||
            step == CoreTutorialStep.CraftComplete;
    }

    /// <summary>
    /// 현재 제작 튜토리얼 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 여부</returns>
    public bool Begin ( CoreTutorialStep step )
    {
        switch ( step )
        {
            case CoreTutorialStep.Craft:
                _tutorialEyePlacementNumber = 0;
                _wasTutorialEyeMoved = false;
                _wasTutorialEyeScaled = false;
                _isMovePartDialogueCompleted = false;
                _isWaitingPartsPanelOpen = false;
                _isWaitingCraftInfoOpen = false;
                _isWaitingUsedPartsOpen = false;
                return _playDialogue( CraftOrderDialogueId );

            case CoreTutorialStep.CraftComplete:
                if ( IsTutorialOrderPastCrafted( ) )
                {
                    _completeStep( CoreTutorialStep.CraftComplete );
                    return true;
                }

                if ( IsTutorialOrderCrafted( ) )
                    return _playDialogue( CraftResultDialogueId );

                return _playDialogue( CraftCompleteDialogueId );

            default:
                return false;
        }
    }

    /// <summary>
    /// 실제 주문 상태를 기준으로 제작 이후 단계 복구
    /// </summary>
    public void Recover ()
    {
        if ( IsTutorialOrderCrafted( ) )
        {
            if ( _tutorialModel.CurrentCoreStep ==
                CoreTutorialStep.Craft )
            {
                _tutorialModel.CompleteCoreStep(
                    CoreTutorialStep.Craft );
            }

            return;
        }

        if ( IsTutorialOrderPastCrafted( ) == false )
            return;

        if ( _tutorialModel.CurrentCoreStep ==
            CoreTutorialStep.Craft )
        {
            _tutorialModel.CompleteCoreStep(
                CoreTutorialStep.Craft );
        }

        if ( _tutorialModel.CurrentCoreStep ==
            CoreTutorialStep.CraftComplete )
        {
            _tutorialModel.CompleteCoreStep(
                CoreTutorialStep.CraftComplete );
        }
    }

    /// <summary>
    /// 제작 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>제작 튜토리얼에서 처리한 신호 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        TutorialTargetId targetId;
        TutorialGuideMode mode = TutorialGuideMode.Blocking;

        switch ( signalId )
        {
            case "Highlight_TutorialCraftOrder":
                //대화 패널 퇴장 완료 후 제작 주문 슬롯 입력 안내
                return true;

            case "Highlight_BodyPart":
                //대화 종료 후 실제 파츠 슬롯 입력을 허용
                return true;

            case "Highlight_CraftCost":
                targetId = TutorialTargetId.CraftCost;
                mode = TutorialGuideMode.Focus;
                break;

            case "Highlight_OrderConditions":
                targetId = TutorialTargetId.OrderConditions;
                mode = TutorialGuideMode.Focus;
                break;

            case "Highlight_CraftCompleteButton":
                //대화 종료 후 제작 완료 버튼 입력을 허용
                return true;

            case "Highlight_DeliveryButton":
                //결과 대화 패널 퇴장 완료 후 배송 버튼 입력 안내
                return true;

            default:
                return false;
        }

        _tutorialView.ShowTarget( targetId, mode );
        return true;
    }

    /// <summary>
    /// 제작 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>제작 튜토리얼에서 처리한 대화 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId == CraftOrderDialogueId )
        {
            ShowTutorialCraftOrderTarget( );
            return true;
        }

        if ( dialogueId == FreePlaceDialogueId )
        {
            ShowPartsPanelGuide( );
            return true;
        }

        if ( dialogueId == BodyPlaceDialogueId )
        {
            ShowCraftPartTarget( PartType.Body );
            return true;
        }

        if ( dialogueId == MovePartDialogueId )
        {
            _isMovePartDialogueCompleted = true;

            if ( _wasTutorialEyeMoved &&
                _wasTutorialEyeScaled )
            {
                ContinuePartPlacementGuide( );
            }
            else
            {
                ShowPlacedEyeTarget( );
            }

            return true;
        }

        if ( dialogueId == CraftCostDialogueId )
        {
            _playDialogue( OrderConditionsDialogueId );
            return true;
        }

        if ( dialogueId == OrderConditionsDialogueId )
        {
            _isWaitingUsedPartsOpen = true;
            _tutorialView.ShowTarget(
                TutorialTargetId.CraftInfoSwitchButton,
                TutorialGuideMode.Blocking );

            return true;
        }

        if ( dialogueId == CraftCompleteDialogueId )
        {
            _tutorialView.ShowTarget(
                TutorialTargetId.CraftDoneButton,
                TutorialGuideMode.Blocking );
            return true;
        }

        if ( dialogueId == CraftResultDialogueId )
        {
            _tutorialView.ShowTarget(
                TutorialTargetId.DeliveryButton,
                TutorialGuideMode.Blocking );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 튜토리얼 제작 주문 슬롯을 런타임 대상으로 연결하고 표시
    /// </summary>
    void ShowTutorialCraftOrderTarget ()
    {
        if ( _craftPresenter.TryGetOrderSlotTarget(
            _tutorialDay1Data.OrderId,
            out RectTransform target ) == false )
        {
            return;
        }

        _tutorialView.SetRuntimeTarget(
            TutorialTargetId.CraftOrderSlot, target );
        _tutorialView.ShowTarget(
            TutorialTargetId.CraftOrderSlot,
            TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 제작 행동 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _craftPresenter.OnOrderSelected += HandleCraftOrderSelected;
        _craftPresenter.OnCraftStartRequested += HandleCraftStartRequested;
        _craftPresenter.OnCraftStartFailed += HandleCraftStartFailed;
        _craftPresenter.OnCraftOpened += HandleCraftOpened;
        _craftPresenter.OnPartPlaced += HandleCraftPartPlaced;
        _craftPresenter.OnPartDragCompleted +=
            HandleCraftPartDragCompleted;
        _craftPresenter.OnPartScaled += HandleCraftPartScaled;
        _craftPresenter.OnCraftSucceeded += HandleCraftSucceeded;
        _craftPresenter.OnPartsSelectPanelOpened +=
            HandlePartsSelectPanelOpened;
        _craftPresenter.OnCraftInfoPanelOpened +=
            HandleCraftInfoPanelOpened;
        _craftPresenter.OnUsedPartsShown += HandleUsedPartsShown;

        _isSubscribed = true;
    }

    /// <summary>
    /// 제작 행동 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _craftPresenter.OnOrderSelected -= HandleCraftOrderSelected;
        _craftPresenter.OnCraftStartRequested -= HandleCraftStartRequested;
        _craftPresenter.OnCraftStartFailed -= HandleCraftStartFailed;
        _craftPresenter.OnCraftOpened -= HandleCraftOpened;
        _craftPresenter.OnPartPlaced -= HandleCraftPartPlaced;
        _craftPresenter.OnPartDragCompleted -=
            HandleCraftPartDragCompleted;
        _craftPresenter.OnPartScaled -= HandleCraftPartScaled;
        _craftPresenter.OnCraftSucceeded -= HandleCraftSucceeded;
        _craftPresenter.OnPartsSelectPanelOpened -=
            HandlePartsSelectPanelOpened;
        _craftPresenter.OnCraftInfoPanelOpened -=
            HandleCraftInfoPanelOpened;
        _craftPresenter.OnUsedPartsShown -= HandleUsedPartsShown;

        _isWaitingPartsPanelOpen = false;
        _isWaitingCraftInfoOpen = false;
        _isWaitingUsedPartsOpen = false;
        _isSubscribed = false;
    }

    /// <summary>
    /// 튜토리얼 제작 주문 선택 후 제작 시작 버튼 안내
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    void HandleCraftOrderSelected ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.Craft ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.HideInstant( );
        _tutorialView.ShowTarget(
            TutorialTargetId.CraftStartButton, TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 튜토리얼 제작 시작 입력 직후 강조 제거
    /// </summary>
    /// <param name="orderId">제작 시작을 요청한 주문 아이디</param>
    void HandleCraftStartRequested ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.Craft ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 튜토리얼 제작 시작 실패 시 버튼 강조 복구
    /// </summary>
    /// <param name="orderId">제작 시작에 실패한 주문 아이디</param>
    void HandleCraftStartFailed ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.Craft ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.ShowTarget(
            TutorialTargetId.CraftStartButton,
            TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 튜토리얼 제작 화면 진입 후 자유 배치 설명 시작
    /// </summary>
    /// <param name="orderId">진입한 주문 아이디</param>
    void HandleCraftOpened ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.Craft ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.HideInstant( );
        _queueDialogue( FreePlaceDialogueId );
    }

    /// <summary>
    /// 튜토리얼 파츠 배치 순서 진행
    /// </summary>
    /// <param name="partId">배치한 파츠 아이디</param>
    /// <param name="partType">배치한 파츠 타입</param>
    /// <param name="placementNumber">배치 번호</param>
    void HandleCraftPartPlaced (
        string partId, PartType partType, int placementNumber )
    {
        if ( _tutorialModel.CurrentCoreStep !=
            CoreTutorialStep.Craft )
        {
            return;
        }

        _tutorialView.HideInstant( );
        switch ( partType )
        {
            case PartType.Body:
                ShowCraftPartTarget( PartType.Eye );
                break;

            case PartType.Eye:
                _tutorialEyePlacementNumber = placementNumber;
                _wasTutorialEyeMoved = false;
                _wasTutorialEyeScaled = false;
                _isMovePartDialogueCompleted = false;
                _queueDialogue( MovePartDialogueId );
                break;

            case PartType.Nose:
                TryContinuePartPlacementGuide( );
                break;

            case PartType.Mouth:
                TryContinuePartPlacementGuide( );
                break;
        }
    }

    /// <summary>
    /// 파츠 선택 패널 열기 버튼을 안내하거나 바디 설명 시작
    /// </summary>
    void ShowPartsPanelGuide ()
    {
        if ( _craftPresenter.IsPartsSelectOpen )
        {
            _playDialogue( BodyPlaceDialogueId );
            return;
        }

        _isWaitingPartsPanelOpen = true;
        _tutorialView.ShowTarget(
            TutorialTargetId.PartsSelectOpenButton,
            TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 파츠 선택 패널 열림 완료 후 바디 배치 설명 시작
    /// </summary>
    void HandlePartsSelectPanelOpened ()
    {
        if ( _isWaitingPartsPanelOpen == false ||
            _tutorialModel.CurrentCoreStep != CoreTutorialStep.Craft )
        {
            return;
        }

        _isWaitingPartsPanelOpen = false;
        _tutorialView.HideInstant( );
        _playDialogue( BodyPlaceDialogueId );
    }

    /// <summary>
    /// 제작 정보 패널 열기 버튼을 안내하거나 비용 설명 시작
    /// </summary>
    void ShowCraftInfoGuide ()
    {
        if ( _craftPresenter.IsCraftInfoOpen )
        {
            _queueDialogue( CraftCostDialogueId );
            return;
        }

        _isWaitingCraftInfoOpen = true;
        _tutorialView.ShowTarget(
            TutorialTargetId.CraftInfoOpenButton,
            TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 제작 정보 패널 열림 완료 후 비용 설명 시작
    /// </summary>
    void HandleCraftInfoPanelOpened ()
    {
        if ( _isWaitingCraftInfoOpen == false ||
            _tutorialModel.CurrentCoreStep != CoreTutorialStep.Craft )
        {
            return;
        }

        _isWaitingCraftInfoOpen = false;
        _tutorialView.HideInstant( );
        _playDialogue( CraftCostDialogueId );
    }

    /// <summary>
    /// 사용 파츠 Content 확인 후 제작 안내 단계 완료
    /// </summary>
    void HandleUsedPartsShown ()
    {
        if ( _isWaitingUsedPartsOpen == false ||
            _tutorialModel.CurrentCoreStep != CoreTutorialStep.Craft ||
            HasMinimumCraftParts( ) == false )
        {
            return;
        }

        _isWaitingUsedPartsOpen = false;
        _tutorialView.HideInstant( );
        _completeStep( CoreTutorialStep.Craft );
    }

    /// <summary>
    /// 안내한 눈 파츠 이동 완료 상태 기록
    /// </summary>
    /// <param name="partId">이동한 파츠 아이디</param>
    /// <param name="partType">이동한 파츠 타입</param>
    /// <param name="placementNumber">배치 번호</param>
    void HandleCraftPartDragCompleted (
        string partId, PartType partType, int placementNumber )
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.Craft ||
            partType != PartType.Eye ||
            placementNumber != _tutorialEyePlacementNumber )
        {
            return;
        }

        _wasTutorialEyeMoved = true;
        TryContinuePartPlacementGuide( );
    }

    /// <summary>
    /// 안내한 눈 파츠 크기 변경 완료 상태 기록
    /// </summary>
    /// <param name="partId">크기를 변경한 파츠 아이디</param>
    /// <param name="partType">크기를 변경한 파츠 타입</param>
    /// <param name="placementNumber">배치 번호</param>
    void HandleCraftPartScaled (
        string partId, PartType partType, int placementNumber )
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.Craft ||
            partType != PartType.Eye ||
            placementNumber != _tutorialEyePlacementNumber )
        {
            return;
        }

        _wasTutorialEyeScaled = true;
        TryContinuePartPlacementGuide( );
    }

    /// <summary>
    /// 눈 편집 안내 완료 후 현재 파츠 배치 상태에 맞춰 다음 안내 진행
    /// </summary>
    void TryContinuePartPlacementGuide ()
    {
        if ( _isMovePartDialogueCompleted == false ||
            _wasTutorialEyeMoved == false ||
            _wasTutorialEyeScaled == false )
        {
            return;
        }

        ContinuePartPlacementGuide( );
    }

    /// <summary>
    /// 이미 배치한 코와 입을 건너뛰고 필요한 다음 제작 안내 표시
    /// </summary>
    void ContinuePartPlacementGuide ()
    {
        if ( _craftPresenter.HasPlacedPartType( PartType.Nose ) == false )
        {
            ShowCraftPartTarget( PartType.Nose );
            return;
        }

        if ( _craftPresenter.HasPlacedPartType( PartType.Mouth ) == false )
        {
            ShowCraftPartTarget( PartType.Mouth );
            return;
        }

        if ( HasMinimumCraftParts( ) )
            ShowCraftInfoGuide( );
    }

    /// <summary>
    /// 튜토리얼 제작 확정 성공 후 결과 설명 시작
    /// </summary>
    /// <param name="orderId">제작을 완료한 주문 아이디</param>
    void HandleCraftSucceeded ( string orderId )
    {
        if ( _tutorialModel.CurrentCoreStep !=
                CoreTutorialStep.CraftComplete ||
            orderId != _tutorialDay1Data.OrderId )
        {
            return;
        }

        _tutorialView.HideInstant( );
        _queueDialogue( CraftResultDialogueId );
    }

    /// <summary>
    /// 튜토리얼 파츠 선택 슬롯 강조
    /// </summary>
    /// <param name="partType">강조할 파츠 타입</param>
    void ShowCraftPartTarget ( PartType partType )
    {
        if ( TryGetGuidePartId(
            partType, out string partId ) == false )
        {
            return;
        }

        if ( _craftPresenter.TryGetPartSlotTarget(
            partId, out RectTransform target ) == false )
        {
            return;
        }

        TutorialTargetId targetId = GetPartTargetId( partType );

        _tutorialView.SetRuntimeTarget( targetId, target );
        _tutorialView.ShowTarget(
            targetId, TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 배치된 눈 파츠 이동과 크기 변경 대상 표시
    /// </summary>
    void ShowPlacedEyeTarget ()
    {
        if ( _craftPresenter.TryGetPlacedPartTarget(
            _tutorialEyePlacementNumber,
            out RectTransform target ) == false )
        {
            return;
        }

        _tutorialView.SetRuntimeTarget(
            TutorialTargetId.PlacedEyePart,
            target );

        _tutorialView.ShowTarget(
            TutorialTargetId.PlacedEyePart, TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 지정 타입에 해당하는 튜토리얼 파츠 아이디 조회
    /// </summary>
    /// <param name="partType">조회할 파츠 타입</param>
    /// <param name="partId">조회한 파츠 아이디</param>
    /// <returns>조회 성공 여부</returns>
    bool TryGetGuidePartId (
        PartType partType, out string partId )
    {
        IReadOnlyList<TutorialPurchaseGuideData> guides =
            _tutorialDay1Data.PurchaseGuides;

        for ( int i = 0; i < guides.Count; i++ )
        {
            TutorialPurchaseGuideData guide = guides [ i ];

            if ( guide.RequiredForProgress == false ||
                _inventoryModel.GetItem(
                    guide.PartId, out InventoryItem item ) == false ||
                item.Data is not PartsData partData ||
                partData.PartType != partType )
            {
                continue;
            }

            partId = guide.PartId;
            return true;
        }

        partId = string.Empty;
        return false;
    }

    /// <summary>
    /// 파츠 타입에 대응하는 강조 대상 아이디 반환
    /// </summary>
    /// <param name="partType">파츠 타입</param>
    /// <returns>튜토리얼 강조 대상 아이디</returns>
    TutorialTargetId GetPartTargetId ( PartType partType )
    {
        switch ( partType )
        {
            case PartType.Body:
                return TutorialTargetId.BodyPartSlot;

            case PartType.Eye:
                return TutorialTargetId.EyePartSlot;

            case PartType.Nose:
                return TutorialTargetId.NosePartSlot;

            case PartType.Mouth:
                return TutorialTargetId.MouthPartSlot;

            default:
                return TutorialTargetId.None;
        }
    }

    /// <summary>
    /// 제작에 필요한 최소 파츠 타입 배치 여부 확인
    /// </summary>
    /// <returns>최소 제작 조건 충족 여부</returns>
    bool HasMinimumCraftParts ()
    {
        return
            _craftPresenter.HasPlacedPartType( PartType.Body ) &&
            _craftPresenter.HasPlacedPartType( PartType.Eye ) &&
            _craftPresenter.HasPlacedPartType( PartType.Nose ) &&
            _craftPresenter.HasPlacedPartType( PartType.Mouth );
    }

    /// <summary>
    /// 튜토리얼 주문 제작 완료 여부 확인
    /// </summary>
    /// <returns>배송 대기 상태 여부</returns>
    bool IsTutorialOrderCrafted ()
    {
        return _orderModel.GetOrder(
            _tutorialDay1Data.OrderId,
            out CustomerOrder order ) &&
            order.ProgressState == OrderProgressState.Crafted;
    }

    /// <summary>
    /// 튜토리얼 주문이 배송 단계 이후인지 확인
    /// </summary>
    /// <returns>배송 중이거나 종료된 상태 여부</returns>
    bool IsTutorialOrderPastCrafted ()
    {
        if ( _orderModel.GetOrder(
            _tutorialDay1Data.OrderId,
            out CustomerOrder order ) == false )
        {
            return false;
        }

        return order.ProgressState == OrderProgressState.Shipping ||
            order.ProgressState == OrderProgressState.Closed;
    }
}
