using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 튜토리얼 프레젠터 - 구매 파츠 확인과 제작 이동 중재
/// </summary>
public class TutorialInventoryPresenter : ITutorialSection
{
    const string InventoryDialogueId = "Dialogue_11_Day1_Inventory";
    const string InventoryCheckDialogueId = "Dialogue_12_Day1_InventoryCheck";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 데이터
    TutorialView _tutorialView;       //튜토리얼 표시 뷰
    InventoryPresenter _inventoryPresenter;       //인벤토리 행동 결과 프레젠터
    CraftPresenter _craftPresenter;       //제작 목록 진입 결과 프레젠터
    InventoryModel _inventoryModel;       //구매 파츠 확인 모델
    /// <summary>
    /// 즉시 대화 재생(대사 아이디, 재생 성공 여부)
    /// </summary>
    Func<string , bool> _playDialogue;
    /// <summary>
    /// 현재 대화 이후 재생 예약(대사 아이디)
    /// </summary>
    Action<string> _queueDialogue;
    Action<CoreTutorialStep> _completeStep;       //핵심 단계 완료
    bool _wasInventoryPartChecked;       //구매 파츠 상세 확인 여부
    bool _isInventoryDialogueCompleted;       //인벤토리 안내 대화 완료 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 인벤토리 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 튜토리얼 데이터</param>
    /// <param name="tutorialView">튜토리얼 표시 뷰</param>
    /// <param name="inventoryPresenter">인벤토리 행동 결과 프레젠터</param>
    /// <param name="craftPresenter">제작 목록 진입 결과 프레젠터</param>
    /// <param name="inventoryModel">구매 파츠 확인 모델</param>
    /// <param name="playDialogue">즉시 대화 재생 함수</param>
    /// <param name="queueDialogue">현재 대화 이후 재생 예약 함수</param>
    /// <param name="completeStep">핵심 단계 완료 함수</param>
    public TutorialInventoryPresenter (
        TutorialModel tutorialModel ,
        TutorialDay1Data tutorialDay1Data ,
        TutorialView tutorialView ,
        InventoryPresenter inventoryPresenter ,
        CraftPresenter craftPresenter ,
        InventoryModel inventoryModel ,
        Func<string , bool> playDialogue ,
        Action<string> queueDialogue ,
        Action<CoreTutorialStep> completeStep )
    {
        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
        _tutorialView = tutorialView;
        _inventoryPresenter = inventoryPresenter;
        _craftPresenter = craftPresenter;
        _inventoryModel = inventoryModel;
        _playDialogue = playDialogue;
        _queueDialogue = queueDialogue;
        _completeStep = completeStep;
    }

    /// <summary>
    /// 인벤토리 튜토리얼 담당 단계 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>인벤토리 튜토리얼 담당 단계 여부</returns>
    public bool Handles ( CoreTutorialStep step )
    {
        return step == CoreTutorialStep.InventoryCheck;
    }

    /// <summary>
    /// 인벤토리 확인 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 여부</returns>
    public bool Begin ( CoreTutorialStep step )
    {
        if ( Handles ( step ) == false ) return false;

        _wasInventoryPartChecked = false;
        _isInventoryDialogueCompleted = false;
        return _playDialogue ( InventoryDialogueId );
    }

    /// <summary>
    /// 인벤토리 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>인벤토리 튜토리얼에서 처리한 신호 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_InventoryButton":
                //대화 패널 퇴장 완료 후 인벤토리 버튼 입력 안내
                return true;

            case "Highlight_CraftButton":
                //인벤토리 대화와 패널 퇴장이 끝난 뒤 제작 버튼 안내
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 인벤토리 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>인벤토리 튜토리얼에서 처리한 대화 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId == InventoryDialogueId )
        {
            _isInventoryDialogueCompleted = true;
            ShowInventoryTargetOrContinue ( );
            return true;
        }

        if ( dialogueId != InventoryCheckDialogueId ) return false;

        //인벤토리 화면 퇴장 후 제작 버튼 안내
        _inventoryPresenter.HidePanel (
            ( ) => _tutorialView.ShowTarget (
                TutorialTargetId.CraftButton , TutorialGuideMode.Blocking ) );
        return true;
    }

    /// <summary>
    /// 인벤토리와 제작 이동 이벤트 연결
    /// </summary>
    public void SubscribeEvents ( )
    {
        if ( _isSubscribed ) return;

        _inventoryPresenter.OnPanelOpened += HandleInventoryPanelOpened;
        _inventoryPresenter.OnItemSelected += HandleInventoryItemSelected;
        _craftPresenter.OnListOpened += HandleCraftListOpened;

        _isSubscribed = true;
    }

    /// <summary>
    /// 인벤토리와 제작 이동 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ( )
    {
        if ( _isSubscribed == false ) return;

        _inventoryPresenter.OnPanelOpened -= HandleInventoryPanelOpened;
        _inventoryPresenter.OnItemSelected -= HandleInventoryItemSelected;
        _craftPresenter.OnListOpened -= HandleCraftListOpened;

        _isSubscribed = false;
    }

    /// <summary>
    /// 현재 인벤토리 표시 상태에 맞춰 버튼 또는 구매 파츠를 안내
    /// </summary>
    void ShowInventoryTargetOrContinue ( )
    {
        if ( _inventoryPresenter.IsShowing == true )
        {
            ShowInventoryPartTarget ( );
            return;
        }

        _tutorialView.ShowTarget (
            TutorialTargetId.InventoryButton , TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 인벤토리 진입 후 구매한 파츠 슬롯 안내
    /// </summary>
    void HandleInventoryPanelOpened ( )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.InventoryCheck ||
            _isInventoryDialogueCompleted == false )
        {
            return;
        }

        _tutorialView.HideInstant ( );
        ShowInventoryPartTarget ( );
    }

    /// <summary>
    /// 구매한 튜토리얼 파츠 상세 확인 후 제작 안내
    /// </summary>
    /// <param name="itemId">확인한 아이템 아이디</param>
    void HandleInventoryItemSelected ( string itemId )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.InventoryCheck ||
            _wasInventoryPartChecked == true ||
            IsTutorialPurchasePart ( itemId ) == false )
        {
            return;
        }

        _wasInventoryPartChecked = true;
        _tutorialView.HideInstant ( );
        _queueDialogue ( InventoryCheckDialogueId );
    }

    /// <summary>
    /// 제작 주문 목록 진입 후 인벤토리 단계 완료
    /// </summary>
    void HandleCraftListOpened ( )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.InventoryCheck )
        {
            return;
        }

        _completeStep ( CoreTutorialStep.InventoryCheck );
    }

    /// <summary>
    /// 구매한 필수 파츠 슬롯을 런타임 강조 대상으로 연결
    /// </summary>
    void ShowInventoryPartTarget ( )
    {
        //필수 파츠 보유 여부 확인
        if ( TryGetInventoryGuidePart ( out string partId ) == false )
        {
            return;
        }

        //강조 대상 확인
        if ( _inventoryPresenter.TryGetItemSlotTarget (
            partId , out RectTransform target ) == false )
        {
            return;
        }

        //인벤토리 아이템 연출 준비
        _tutorialView.SetRuntimeTarget ( TutorialTargetId.InventoryPartSlot , target );

        //인벤토리 아이템 슬롯 강조 연출
        _tutorialView.ShowTarget (
            TutorialTargetId.InventoryPartSlot , TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 인벤토리에 보유한 첫 필수 가이드 파츠 조회
    /// </summary>
    /// <param name="partId">조회한 파츠 아이디</param>
    /// <returns>조회 성공 여부</returns>
    bool TryGetInventoryGuidePart ( out string partId )
    {
        IReadOnlyList<TutorialPurchaseGuideData> guides =
            _tutorialDay1Data.PurchaseGuides;

        for ( int i = 0 ; i < guides.Count ; i++ )
        {
            TutorialPurchaseGuideData guide = guides [ i ];

            if ( guide.RequiredForProgress == false ||
                _inventoryModel.HasQuantity ( guide.PartId , 1 ) == false )
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
    /// 선택한 아이템이 구매 가이드 파츠인지 확인
    /// </summary>
    /// <param name="itemId">선택한 아이템 아이디</param>
    /// <returns>튜토리얼 구매 파츠 여부</returns>
    bool IsTutorialPurchasePart ( string itemId )
    {
        IReadOnlyList<TutorialPurchaseGuideData> guides =
            _tutorialDay1Data.PurchaseGuides;

        for ( int i = 0 ; i < guides.Count ; i++ )
        {
            TutorialPurchaseGuideData guide = guides [ i ];

            if ( guide.PartId == itemId && _inventoryModel.HasQuantity ( itemId , 1 ) )
            {
                return true;
            }
        }

        return false;
    }
}
