using System;
using System.Collections.Generic;

/// <summary>
/// 상점 품절과 빠른 재입고 후속 가이드 프레젠터
/// </summary>
public class ShopGuidePresenter : ITutorialGuideSection
{
    const string OutOfStockDialogueId = "Dialogue_Day3_OutOfStockGuide";
    const string QuickRestockDialogueId = "Dialogue_Day3_QuickRestockGuide";

    TutorialModel _tutorialModel;       //튜토리얼 완료 상태
    PlayStateModel _playStateModel;       //현재 누적 영업일
    ShopModel _shopModel;       //상품 재고 상태
    ShopPresenter _shopPresenter;       //상점 화면과 구매 이벤트
    TutorialView _tutorialView;       //강조 표시 뷰

    Func<string, bool> _playDialogue;       //공용 대화 재생 함수
    Func<TutorialGuideId, bool> _requestGuide;       //가이드 요청 함수

    TutorialGuideId? _currentGuideId;       //현재 실행 중인 가이드
    string _currentDialogueId;       //현재 실행 중인 대화
    string _soldOutItemId;       //처음 품절된 파츠 상품
    string _openedProductId;       //현재 상세 화면 상품

    bool _isShopPanelOpened;       //상점 패널 표시 여부
    bool _isCartOpened;       //장바구니 패널 표시 여부
    bool _hasPendingOutOfStockGuide;       //장바구니 퇴장 후 품절 안내 대기 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 상점 후속 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// 상점 품절과 빠른 재입고 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 완료 상태 모델</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="shopModel">상품 재고 상태 모델</param>
    /// <param name="shopPresenter">상점 화면 프레젠터</param>
    /// <param name="tutorialView">튜토리얼 강조 표시 뷰</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public ShopGuidePresenter (
        TutorialModel tutorialModel,
        PlayStateModel playStateModel,
        ShopModel shopModel,
        ShopPresenter shopPresenter,
        TutorialView tutorialView,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _playStateModel = playStateModel;
        _shopModel = shopModel;
        _shopPresenter = shopPresenter;
        _tutorialView = tutorialView;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 상점 후속 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>상점 후속 가이드 담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.OutOfStock ||
            guideId == TutorialGuideId.QuickRestock;
    }

    /// <summary>
    /// 지정된 상점 후속 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 후속 가이드 아이디</param>
    /// <returns>가이드 시작 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( CanBeginGuide( guideId ) == false )
            return false;

        if ( guideId == TutorialGuideId.OutOfStock &&
            RegisterSoldOutTarget( ) == false )
        {
            return false;
        }

        string dialogueId = GetDialogueId( guideId );

        if ( string.IsNullOrEmpty( dialogueId ) )
            return false;

        _currentGuideId = guideId;
        _currentDialogueId = dialogueId;

        if ( _playDialogue( dialogueId ) )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 상점 후속 가이드 시작 조건 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>현재 가이드 시작 가능 여부</returns>
    bool CanBeginGuide ( TutorialGuideId guideId )
    {
        if ( _tutorialModel.CoreTutorialCompleted == false ||
            _isShopPanelOpened == false || _isCartOpened )
        {
            return false;
        }

        switch ( guideId )
        {
            case TutorialGuideId.OutOfStock:
                return IsSoldOutPart( _soldOutItemId );

            case TutorialGuideId.QuickRestock:
                return
                    _tutorialModel.HasShownGuide(
                        TutorialGuideId.OutOfStock ) &&
                    IsSoldOutPart( _openedProductId );

            default:
                return false;
        }
    }

    /// <summary>
    /// 가이드에 대응하는 대화 아이디 반환
    /// </summary>
    /// <param name="guideId">후속 가이드 아이디</param>
    /// <returns>가이드 대화 아이디</returns>
    string GetDialogueId ( TutorialGuideId guideId )
    {
        switch ( guideId )
        {
            case TutorialGuideId.OutOfStock:
                return OutOfStockDialogueId;

            case TutorialGuideId.QuickRestock:
                return QuickRestockDialogueId;

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 지정 상품이 실제 품절된 파츠인지 확인
    /// </summary>
    /// <param name="itemId">확인할 상품 아이디</param>
    /// <returns>품절된 파츠 상품 여부</returns>
    bool IsSoldOutPart ( string itemId )
    {
        if ( string.IsNullOrEmpty( itemId ) ||
            _shopModel.TryGetItem( itemId,
            out ShopItemModel itemModel ) == false )
        {
            return false;
        }

        return _playStateModel.IsUnlocked( itemId ) &&
            itemModel.Item.Data.ProductType ==
                ProductType.BabyPart && itemModel.Item.IsSoldOut;
    }

    /// <summary>
    /// 처음 품절된 상품 슬롯을 런타임 강조 대상으로 등록
    /// </summary>
    /// <returns>품절 상품 슬롯 등록 여부</returns>
    bool RegisterSoldOutTarget ()
    {
        if ( _shopPresenter.TryGetProductSlotTarget(
            _soldOutItemId, out UnityEngine.RectTransform target ) ==
            false )
        {
            return false;
        }

        return _tutorialView.RegisterTarget(
            TutorialTargetId.OutOfStockProduct,
            target, UnityEngine.Vector2.zero );
    }

    /// <summary>
    /// 상점 후속 가이드 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        if ( _currentGuideId.HasValue == false )
            return false;

        switch ( signalId )
        {
            case "Highlight_OutOfStockProduct"
                when _currentGuideId == TutorialGuideId.OutOfStock:
                return _tutorialView.ShowTarget(
                    TutorialTargetId.OutOfStockProduct,
                    TutorialGuideMode.Focus );

            case "Highlight_QuickRestockButton"
                when _currentGuideId == TutorialGuideId.QuickRestock:
            case "Highlight_QuickRestockCost"
                when _currentGuideId == TutorialGuideId.QuickRestock:
                //상세 뷰에서 비용을 설명하므로 빠른 재입고 버튼 강조
                return _tutorialView.ShowTarget(
                    TutorialTargetId.QuickRestockButton, TutorialGuideMode.Focus );

            default:
                return false;
        }
    }

    /// <summary>
    /// 현재 상점 후속 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 가이드 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( _currentGuideId.HasValue == false ||
            dialogueId != _currentDialogueId )
        {
            return false;
        }

        TutorialGuideId completedGuideId = _currentGuideId.Value;

        _currentGuideId = null;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke( completedGuideId );
        return true;
    }

    /// <summary>
    /// 상점 화면과 구매 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _shopPresenter.OnPanelOpened += HandlePanelOpened;
        _shopPresenter.OnPanelClosed += HandlePanelClosed;
        _shopPresenter.OnProductOpened += HandleProductOpened;
        _shopPresenter.OnCartOpened += HandleCartOpened;
        _shopPresenter.OnCartClosed += HandleCartClosed;
        _shopPresenter.OnPurchased += HandlePurchased;

        _isSubscribed = true;
    }

    /// <summary>
    /// 상점 화면과 구매 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _shopPresenter.OnPanelOpened -= HandlePanelOpened;
        _shopPresenter.OnPanelClosed -= HandlePanelClosed;
        _shopPresenter.OnProductOpened -= HandleProductOpened;
        _shopPresenter.OnCartOpened -= HandleCartOpened;
        _shopPresenter.OnCartClosed -= HandleCartClosed;
        _shopPresenter.OnPurchased -= HandlePurchased;

        _isSubscribed = false;
    }

    /// <summary>
    /// 상점 패널 표시 상태 반영
    /// </summary>
    void HandlePanelOpened ()
    {
        _isShopPanelOpened = true;
    }

    /// <summary>
    /// 상점 패널 종료 상태 반영
    /// </summary>
    void HandlePanelClosed ()
    {
        _isShopPanelOpened = false;
        _isCartOpened = false;
        _openedProductId = string.Empty;
    }

    /// <summary>
    /// 장바구니 표시 상태 반영
    /// </summary>
    void HandleCartOpened ()
    {
        _isCartOpened = true;
    }

    /// <summary>
    /// 장바구니 퇴장 후 대기 중인 품절 안내 요청
    /// </summary>
    void HandleCartClosed ()
    {
        _isCartOpened = false;

        if ( _hasPendingOutOfStockGuide == false )
            return;

        _hasPendingOutOfStockGuide = false;
        RequestGuide( TutorialGuideId.OutOfStock );
    }

    /// <summary>
    /// 품절 상품 상세 표시 후 빠른 재입고 안내 요청
    /// </summary>
    /// <param name="itemId">상세 화면에 표시된 상품 아이디</param>
    void HandleProductOpened ( string itemId )
    {
        _openedProductId = itemId;

        //첫 품절 안내 이후 품절 상품 상세를 열면 상세 가이드 시작
        if ( _tutorialModel.HasShownGuide(
                TutorialGuideId.OutOfStock ) && IsSoldOutPart( itemId ) )
        {
            RequestGuide( TutorialGuideId.QuickRestock );
        }
    }

    /// <summary>
    /// 구매 완료 후 처음 품절된 파츠 상품 확인
    /// </summary>
    /// <param name="receipt">구매 완료 상품 내역</param>
    void HandlePurchased (
        IReadOnlyList<PurchaseReceiptItem> receipt )
    {
        if ( receipt == null ||
            _tutorialModel.HasShownGuide( TutorialGuideId.OutOfStock ) )
        {
            return;
        }

        for ( int i = 0; i < receipt.Count; i++ )
        {
            //구매한 상품 데이터 가져오기
            PurchasableData data = receipt [ i ].Data;

            //상품 타입, 품절 여부 확인
            if ( data == null ||
                data.ProductType != ProductType.BabyPart ||
                IsSoldOutPart( data.Id ) == false )
            {
                continue;
            }

            _soldOutItemId = data.Id;
            _hasPendingOutOfStockGuide = true;
            return;
        }
    }

    /// <summary>
    /// 지정 가이드를 중복 없이 요청
    /// </summary>
    /// <param name="guideId">요청할 가이드 아이디</param>
    void RequestGuide ( TutorialGuideId guideId )
    {
        if ( _tutorialModel.CoreTutorialCompleted == false ||
            _tutorialModel.HasShownGuide( guideId ) ||
            _currentGuideId == guideId )
        {
            return;
        }

        _requestGuide( guideId );
    }

    /// <summary>
    /// 현재 실행 중인 상점 가이드 임시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _currentGuideId = null;
        _currentDialogueId = string.Empty;
    }
}
