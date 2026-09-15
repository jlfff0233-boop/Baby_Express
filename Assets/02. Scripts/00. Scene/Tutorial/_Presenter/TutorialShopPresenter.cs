using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 튜토리얼 프레젠터 - 필수 파츠 구매 행동 유도 중재
/// </summary>
public class TutorialShopPresenter : ITutorialSection
{
    const string ShopDialogueId = "Dialogue_5_Day1_Shop";
    const string RequiredPartDialogueId = "Dialogue_6_Day1_RequiredEye";
    const string AddCartDialogueId = "Dialogue_7_Day1_AddCart";
    const string MinimumPartsDialogueId = "Dialogue_8_Day1_MinimumParts";
    const string WishPurchaseDialogueId = "Dialogue_9_Day1_WishBuy";
    const string PurchaseDialogueId = "Dialogue_10_Day1_Purchase";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 데이터
    TutorialView _tutorialView;       //튜토리얼 표시 뷰
    ShopPresenter _shopPresenter;       //상점 행동 결과 프레젠터
    InventoryModel _inventoryModel;       //구매 완료 파츠 확인 모델
    /// <summary>
    /// 즉시 대화 재생(아이디, 재생 성공 여부)
    /// </summary>
    Func<string , bool> _playDialogue;
    /// <summary>
    /// 현재 대화 이후 재생 예약(대사 아이디)
    /// </summary>
    Action<string> _queueDialogue;
    Action<CoreTutorialStep> _completeStep;       //핵심 단계 완료
    TutorialPurchaseGuideData _currentPurchaseGuide;       //현재 안내할 파츠
    bool _wasWishPurchaseExplained;       //희망 사항 구매 설명 완료 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 상점 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 튜토리얼 데이터</param>
    /// <param name="tutorialView">튜토리얼 표시 뷰</param>
    /// <param name="shopPresenter">상점 행동 결과 프레젠터</param>
    /// <param name="inventoryModel">구매 완료 파츠 확인 모델</param>
    /// <param name="playDialogue">즉시 대화 재생 함수</param>
    /// <param name="queueDialogue">현재 대화 이후 재생 예약 함수</param>
    /// <param name="completeStep">핵심 단계 완료 함수</param>
    public TutorialShopPresenter (
        TutorialModel tutorialModel ,
        TutorialDay1Data tutorialDay1Data ,
        TutorialView tutorialView ,
        ShopPresenter shopPresenter ,
        InventoryModel inventoryModel ,
        Func<string , bool> playDialogue ,
        Action<string> queueDialogue ,
        Action<CoreTutorialStep> completeStep )
    {
        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
        _tutorialView = tutorialView;
        _shopPresenter = shopPresenter;
        _inventoryModel = inventoryModel;
        _playDialogue = playDialogue;
        _queueDialogue = queueDialogue;
        _completeStep = completeStep;
    }

    /// <summary>
    /// 상점 튜토리얼 담당 단계 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>상점 튜토리얼 담당 단계 여부</returns>
    public bool Handles ( CoreTutorialStep step )
    {
        return step == CoreTutorialStep.ShopPurchase;
    }

    /// <summary>
    /// 상점 구매 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 여부</returns>
    public bool Begin ( CoreTutorialStep step )
    {
        if ( Handles ( step ) == false ) return false;

        _currentPurchaseGuide = null;
        _wasWishPurchaseExplained = false;
        return _playDialogue ( ShopDialogueId );
    }

    /// <summary>
    /// 상점 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>상점 튜토리얼에서 처리한 신호 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_ShopButton":
                //대화 패널 퇴장 완료 후 상점 버튼 입력 안내
                return true;

            case "Highlight_RequiredEye":
                //대화 패널 퇴장 완료 후 상품 슬롯 입력 안내
                return true;

            case "Highlight_AddCart":
                //대화 패널 퇴장 완료 후 장바구니 추가 입력 안내
                return true;

            case "Highlight_PurchaseButton":
                //대화창이 구매 버튼을 가리는 동안 입력 차단을 시작하지 않음
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 상점 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>상점 튜토리얼에서 처리한 대화 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( dialogueId == ShopDialogueId )
        {
            _tutorialView.ShowTarget (
                TutorialTargetId.ShopButton , TutorialGuideMode.Blocking );
            return true;
        }

        if ( dialogueId == RequiredPartDialogueId )
        {
            ShowCurrentPartTarget ( );
            return true;
        }

        if ( dialogueId == AddCartDialogueId )
        {
            _tutorialView.ShowTarget (
                TutorialTargetId.AddCartButton , TutorialGuideMode.Blocking );
            return true;
        }

        if ( dialogueId == MinimumPartsDialogueId )
        {
            ContinuePurchaseGuide ( );
            return true;
        }

        if ( dialogueId == WishPurchaseDialogueId )
        {
            ShowCartTarget ( );
            return true;
        }

        if ( dialogueId == PurchaseDialogueId )
        {
            _tutorialView.ShowTarget (
                TutorialTargetId.PurchaseButton , TutorialGuideMode.Blocking );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 상점 구매 이벤트 연결
    /// </summary>
    public void SubscribeEvents ( )
    {
        if ( _isSubscribed ) return;

        _shopPresenter.OnPanelOpened += HandleShopPanelOpened;
        _shopPresenter.OnProductOpened += HandleShopProductOpened;
        _shopPresenter.OnAddedToCart += HandleAddedToCart;
        _shopPresenter.OnCartOpened += HandleCartOpened;
        _shopPresenter.OnPurchased += HandlePurchased;

        _isSubscribed = true;
    }

    /// <summary>
    /// 상점 구매 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ( )
    {
        if ( _isSubscribed == false ) return;

        _shopPresenter.OnPanelOpened -= HandleShopPanelOpened;
        _shopPresenter.OnProductOpened -= HandleShopProductOpened;
        _shopPresenter.OnAddedToCart -= HandleAddedToCart;
        _shopPresenter.OnCartOpened -= HandleCartOpened;
        _shopPresenter.OnPurchased -= HandlePurchased;

        _isSubscribed = false;
    }

    /// <summary>
    /// 저장된 인벤토리를 기준으로 상점 구매 단계 복구
    /// </summary>
    public void Recover ( )
    {
        //가이드 단계 확인, 필요 파츠 확인
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.ShopPurchase ||
            HasAllRequiredPartsInInventory ( ) == false )
        {
            return;
        }

        _tutorialModel.CompleteCoreStep ( CoreTutorialStep.ShopPurchase );
    }

    /// <summary>
    /// 상점 진입 후 현재 필요한 파츠 안내
    /// </summary>
    void HandleShopPanelOpened ( )
    {
        //가이드 단계 확인
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.ShopPurchase )
        {
            return;
        }

        _tutorialView.HideInstant ( );

        //필수 파츠 보유 여부 확인
        if ( TryGetFirstMissingRequiredGuide ( out _currentPurchaseGuide ) == false )
        {
            //장바구니 강조 연출
            ShowCartTarget ( );
            return;
        }

        //가이드 진행 순서 확인
        if ( _currentPurchaseGuide.Sequence == 0 )
        {
            //대사 재생 예약
            _queueDialogue ( RequiredPartDialogueId );
            return;
        }

        ShowCurrentPartTarget ( );
    }

    /// <summary>
    /// 안내 중인 상품 상세 진입 후 장바구니 추가 유도
    /// </summary>
    /// <param name="itemId">표시한 상품 아이디</param>
    void HandleShopProductOpened ( string itemId )
    {
        //단계 확인, 가이드 데이터 확인, 가이드 대상 파츠 아이디 확인
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.ShopPurchase ||
            _currentPurchaseGuide == null || _currentPurchaseGuide.PartId != itemId )
        {
            return;
        }

        _tutorialView.HideInstant ( );

        if ( _currentPurchaseGuide.Sequence == 0 )
        {
            _queueDialogue ( AddCartDialogueId );
            return;
        }

        _tutorialView.ShowTarget (
            TutorialTargetId.AddCartButton , TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 안내 상품 장바구니 추가 성공 후 다음 파츠 안내
    /// </summary>
    /// <param name="itemId">추가한 상품 아이디</param>
    /// <param name="quantity">추가한 수량</param>
    void HandleAddedToCart ( string itemId , int quantity )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.ShopPurchase ||
            _currentPurchaseGuide == null ||
            _currentPurchaseGuide.PartId != itemId ||
            quantity <= 0 )
        {
            return;
        }

        int completedSequence = _currentPurchaseGuide.Sequence;
        _currentPurchaseGuide = null;

        //닫히는 상품 상세의 이전 강조와 입력 제한을 즉시 제거
        _tutorialView.HideInstant ( );

        if ( completedSequence == 0 )
        {
            _queueDialogue ( MinimumPartsDialogueId );
            return;
        }

        ContinuePurchaseGuide ( );
    }

    /// <summary>
    /// 장바구니 진입 후 실제 구매 버튼 안내
    /// </summary>
    void HandleCartOpened ( )
    {
        //단계 및 필요 파츠 보유 여부 확인
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.ShopPurchase ||
            HasAllRequiredPartsReady ( ) == false )
        {
            return;
        }

        _tutorialView.HideInstant ( );
        _queueDialogue ( PurchaseDialogueId );
    }

    /// <summary>
    /// 실제 구매 완료 후 필수 파츠 확보 여부 확인
    /// </summary>
    /// <param name="receipt">구매 완료 상품 내역</param>
    void HandlePurchased ( IReadOnlyList<PurchaseReceiptItem> receipt )
    {
        if ( _tutorialModel.CurrentCoreStep != CoreTutorialStep.ShopPurchase ||
            HasAllRequiredPartsInInventory ( ) == false )
        {
            return;
        }

        _tutorialView.HideInstant ( );
        //상점 화면 퇴장 후 인벤토리 안내 단계 시작
        _shopPresenter.HidePanel (
            ( ) => _completeStep ( CoreTutorialStep.ShopPurchase ) );
    }

    /// <summary>
    /// 아직 준비하지 않은 다음 필수 파츠 안내
    /// </summary>
    void ContinuePurchaseGuide ( )
    {
        if ( TryGetFirstMissingRequiredGuide ( out _currentPurchaseGuide ) )
        {
            ShowCurrentPartTarget ( );
            return;
        }

        if ( _wasWishPurchaseExplained == false )
        {
            _wasWishPurchaseExplained = true;
            _playDialogue ( WishPurchaseDialogueId );
            return;
        }

        ShowCartTarget ( );
    }

    /// <summary>
    /// 현재 안내 파츠의 슬롯을 런타임 대상으로 연결
    /// </summary>
    void ShowCurrentPartTarget ( )
    {
        if ( _currentPurchaseGuide == null ) return;

        //강조 대상 슬롯 확인
        if ( _shopPresenter.TryGetProductSlotTarget (
            _currentPurchaseGuide.PartId , out RectTransform target ) == false )
        {
            return;
        }

        _tutorialView.SetRuntimeTarget ( TutorialTargetId.RequiredPartSlot , target );

        //주요 요구 사항 슬롯 강조 연출
        _tutorialView.ShowTarget (
            TutorialTargetId.RequiredPartSlot , TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 필수 파츠 준비 후 장바구니 버튼 안내
    /// </summary>
    void ShowCartTarget ( )
    {
        //장바구니 버튼 강조 연출
        _tutorialView.ShowTarget (
            TutorialTargetId.CartButton , TutorialGuideMode.Blocking );
    }

    /// <summary>
    /// 아직 확보하지 않은 첫 필수 파츠 조회
    /// </summary>
    /// <param name="guide">조회한 구매 가이드</param>
    /// <returns>부족한 필수 파츠 존재 여부</returns>
    bool TryGetFirstMissingRequiredGuide (
        out TutorialPurchaseGuideData guide )
    {
        IReadOnlyList<TutorialPurchaseGuideData> guides =
            _tutorialDay1Data.PurchaseGuides;

        for ( int i = 0 ; i < guides.Count ; i++ )
        {
            TutorialPurchaseGuideData current = guides [ i ];

            if ( current.RequiredForProgress == false || HasPartReady ( current.PartId ) )
            {
                continue;
            }

            guide = current;
            return true;
        }

        guide = null;
        return false;
    }

    /// <summary>
    /// 파츠가 인벤토리 또는 장바구니에 준비됐는지 확인
    /// </summary>
    /// <param name="partId">확인할 파츠 아이디</param>
    /// <returns>파츠 준비 여부</returns>
    bool HasPartReady ( string partId )
    {
        //보유 수량 확인
        return _inventoryModel.HasQuantity ( partId , 1 ) ||
            _shopPresenter.HasCartQuantity ( partId , 1 );
    }

    /// <summary>
    /// 모든 필수 파츠가 구매 준비됐는지 확인
    /// </summary>
    /// <returns>필수 파츠 준비 여부</returns>
    bool HasAllRequiredPartsReady ( )
    {
        return TryGetFirstMissingRequiredGuide ( out _ ) == false;
    }

    /// <summary>
    /// 모든 필수 파츠가 실제 인벤토리에 들어왔는지 확인
    /// </summary>
    /// <returns>필수 파츠 구매 완료 여부</returns>
    bool HasAllRequiredPartsInInventory ( )
    {
        IReadOnlyList<TutorialPurchaseGuideData> guides =
            _tutorialDay1Data.PurchaseGuides;

        for ( int i = 0 ; i < guides.Count ; i++ )
        {
            TutorialPurchaseGuideData guide = guides [ i ];

            if ( guide.RequiredForProgress &&
                _inventoryModel.HasQuantity ( guide.PartId , 1 ) == false )
            {
                return false;
            }
        }

        return true;
    }
}
