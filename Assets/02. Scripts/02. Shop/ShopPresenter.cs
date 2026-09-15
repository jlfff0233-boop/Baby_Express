using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 프레젠터 - 상점 화면 생명주기와 기능 간 결과 중재
/// </summary>
public class ShopPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] ShopView _shopView;        //상점 뷰
    [SerializeField] CartView _cartView;        //장바구니 뷰

    PlayStateModel _playStateModel;       //상품 해금 상태 모델
    CartModel _cartModel;       //현재 장바구니 상태 모델
    PurchaseModel _purchaseModel;       //실제 구매 처리 모델
    ShopListPresenter _listPresenter;       //상품 목록 프레젠터
    ShopDetailPresenter _detailPresenter;       //상품 상세 프레젠터
    CartPresenter _cartPresenter;       //장바구니 프레젠터
    QuickRestockPresenter _quickRestockPresenter;       //빠른 재입고 프레젠터

    bool _isInitialized;       //모델 전달 완료 여부
    bool _isSubscribed;        //이벤트 연결 여부

    /// <summary>
    /// 상점 화면 닫기 이벤트
    /// </summary>
    public event System.Action OnPanelClosed;

    /// <summary>
    /// 상점 패널 표시 이벤트
    /// </summary>
    public event Action OnPanelOpened;

    /// <summary>
    /// 상품 상세 표시 이벤트
    /// </summary>
    public event Action<string> OnProductOpened;

    /// <summary>
    /// 상품 장바구니 추가 성공 이벤트
    /// </summary>
    public event Action<string, int> OnAddedToCart;

    /// <summary>
    /// 장바구니 패널 표시 이벤트
    /// </summary>
    public event Action OnCartOpened;

    /// <summary>
    /// 장바구니 패널 퇴장 완료 이벤트
    /// </summary>
    public event Action OnCartClosed;

    /// <summary>
    /// 실제 상품 구매 완료 이벤트
    /// </summary>
    public event Action<IReadOnlyList<PurchaseReceiptItem>> OnPurchased;

    #region ----- 시작 -----
    /// <summary>
    /// 상점 시스템 모델 연결
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="cartModel">장바구니 모델</param>
    /// <param name="purchaseModel">구매 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="quickRestockModel">빠른 재입고 모델</param>
    /// <param name="maintenanceModel">정비 모델</param>
    public void Init (
        ShopModel shopModel, CartModel cartModel,
        PurchaseModel purchaseModel, PlayStateModel playStateModel,
        QuickRestockModel quickRestockModel,
        MaintenanceModel maintenanceModel )
    {
        //비활성 상점 뷰의 런타임 의존성 먼저 초기화
        _shopView.InitializeRuntime( );

        UnsubscribeEvents( );

        _playStateModel = playStateModel;
        _cartModel = cartModel;
        _purchaseModel = purchaseModel;

        _listPresenter = new ShopListPresenter(
            shopModel, playStateModel, _shopView );

        _detailPresenter = new ShopDetailPresenter(
            shopModel, cartModel, playStateModel,
            maintenanceModel, _shopView );

        _cartPresenter = new CartPresenter(
            shopModel, cartModel, purchaseModel,
            playStateModel, _shopView, _cartView );

        _quickRestockPresenter = new QuickRestockPresenter(
            shopModel, quickRestockModel, _shopView );

        _isInitialized = true;

        if ( isActiveAndEnabled ) SubscribeEvents( );

        HideInstant( );
    }

    /// <summary>
    /// 상점 이벤트 연결
    /// </summary>
    private void OnEnable ()
    {
        if ( _isInitialized ) SubscribeEvents( );
    }

    /// <summary>
    /// 상점 이벤트 해제
    /// </summary>
    private void OnDisable ()
    {
        UnsubscribeEvents( );
    }

    /// <summary>
    /// 상품 목록 표시 시작
    /// </summary>
    private void Start ()
    {
        if ( _isInitialized == false ) return;

        _listPresenter.Refresh( );
        //씬 진입 초기화에서는 퇴장 연출과 패널 효과음을 재생하지 않음
        _cartPresenter.HideInstant( );
    }

#if UNITY_EDITOR
    /// <summary>
    /// 상점 확인용 입력
    /// </summary>
    private void Update ()
    {
        if ( _isInitialized == false ) return;

        if ( Input.GetKeyDown( KeyCode.Alpha0 ) )
            _cartPresenter.PurchaseAllPartsDebug( );
    }
#endif
    #endregion

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 상점과 하위 프레젠터 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        if ( _isSubscribed || _isInitialized == false ) return;

        _listPresenter.SubscribeEvents( );
        _detailPresenter.SubscribeEvents( );
        _cartPresenter.SubscribeEvents( );
        _quickRestockPresenter.SubscribeEvents( );

        _cartPresenter.OnPurchased += RefreshPurchasedProducts;
        _cartPresenter.OnClosed += HandleCartClosed;
        _detailPresenter.OnProductOpened += HandleProductOpened;
        _detailPresenter.OnAddedToCart += HandleAddedToCart;
        _quickRestockPresenter.OnRestocked += RefreshRestockedProduct;
        _playStateModel.OnItemUnlockChanged += UpdateUnlockState;
        _shopView.OnCartOpen += HandleCartOpened;
        _shopView.OnClose += ClosePanel;
        _purchaseModel.OnPurchased += HandlePurchased;

        _isSubscribed = true;
    }

    /// <summary>
    /// 상점과 하위 프레젠터 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _listPresenter.UnsubscribeEvents( );
        _detailPresenter.UnsubscribeEvents( );
        _cartPresenter.UnsubscribeEvents( );
        _quickRestockPresenter.UnsubscribeEvents( );

        _cartPresenter.OnPurchased -= RefreshPurchasedProducts;
        _cartPresenter.OnClosed -= HandleCartClosed;
        _detailPresenter.OnProductOpened -= HandleProductOpened;
        _detailPresenter.OnAddedToCart -= HandleAddedToCart;
        _quickRestockPresenter.OnRestocked -= RefreshRestockedProduct;
        _playStateModel.OnItemUnlockChanged -= UpdateUnlockState;
        _shopView.OnCartOpen -= HandleCartOpened;
        _shopView.OnClose -= ClosePanel;
        _purchaseModel.OnPurchased -= HandlePurchased;

        _isSubscribed = false;
    }

    /// <summary>
    /// 구매 성공 후 상품 목록 갱신
    /// </summary>
    void RefreshPurchasedProducts ()
    {
        _listPresenter.Refresh( );
    }

    /// <summary>
    /// 상품 상세 표시 결과 전달
    /// </summary>
    /// <param name="itemId">표시한 상품 아이디</param>
    void HandleProductOpened ( string itemId )
    {
        OnProductOpened?.Invoke( itemId );
    }

    /// <summary>
    /// 장바구니 추가 성공 결과 전달
    /// </summary>
    /// <param name="itemId">추가한 상품 아이디</param>
    /// <param name="quantity">추가 수량</param>
    void HandleAddedToCart ( string itemId, int quantity )
    {
        OnAddedToCart?.Invoke( itemId, quantity );
    }

    /// <summary>
    /// 장바구니 표시 결과 전달
    /// </summary>
    void HandleCartOpened ()
    {
        OnCartOpened?.Invoke( );
    }

    /// <summary>
    /// 장바구니 퇴장 완료 결과 전달
    /// </summary>
    void HandleCartClosed ()
    {
        OnCartClosed?.Invoke( );
    }

    /// <summary>
    /// 실제 구매 완료 내역 전달
    /// </summary>
    /// <param name="receipt">구매 완료 상품 내역</param>
    void HandlePurchased (
        IReadOnlyList<PurchaseReceiptItem> receipt )
    {
        OnPurchased?.Invoke( receipt );
    }

    /// <summary>
    /// 빠른 재입고 성공 후 관련 화면 갱신
    /// </summary>
    /// <param name="itemId">재입고한 상품 아이디</param>
    void RefreshRestockedProduct ( string itemId )
    {
        _listPresenter.RefreshProduct( itemId );
        _detailPresenter.RefreshProduct( itemId );
        _cartPresenter.Refresh( );
    }

    /// <summary>
    /// 상품 해금 상태 변경 반영
    /// </summary>
    void UpdateUnlockState ( string itemId, bool isUnlocked )
    {
        _detailPresenter.Hide( );
        _listPresenter.Refresh( );
    }
    #endregion

    #region ----- 상점 패널 -----
    /// <summary>
    /// 상점 패널 표시
    /// </summary>
    public void ShowPanel ()
    {
        _listPresenter.Reset( );
        _detailPresenter.Reset( );
        _quickRestockPresenter.Reset( );

        _shopView.ResetSelection( );
        _shopView.ShowPanel( );
        _listPresenter.Refresh( );

        OnPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 시설 카테고리를 선택한 상태로 상점 패널 표시
    /// </summary>
    public void ShowFacilityPanel ()
    {
        _listPresenter.Reset( );
        _detailPresenter.Reset( );
        _quickRestockPresenter.Reset( );

        _shopView.ResetSelection( );
        _shopView.ShowPanel( );
        _shopView.SelectCategory( ShopCategoryType.Facility );

        OnPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 선택한 상품 상세 패널 표시
    /// </summary>
    /// <param name="itemId">표시할 상품 아이디</param>
    public void ShowProduct ( string itemId )
    {
        ShowPanel( );
        _detailPresenter.ShowProduct( itemId );
    }

    /// <summary>
    /// 사용자 입력으로 상점 화면 닫기
    /// </summary>
    void ClosePanel ()
    {
        HidePanel( () => OnPanelClosed?.Invoke( ) );
    }

    /// <summary>
    /// 상점 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        _detailPresenter?.Hide( );
        _quickRestockPresenter?.Hide( );
        _cartPresenter?.HideInstant( );
        _shopView.HidePanel( onComplete );
    }

    /// <summary>
    /// 상점 패널 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _detailPresenter?.Reset( );
        _quickRestockPresenter?.Reset( );
        _cartPresenter?.HideInstant( );
        _shopView.HideInstant( );
    }

    /// <summary>
    /// 상품 아이디에 대응하는 현재 슬롯 강조 대상 조회
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    /// <param name="target">현재 활성 슬롯 위치</param>
    /// <returns>강조 대상 조회 성공 여부</returns>
    public bool TryGetProductSlotTarget (
        string itemId, out RectTransform target )
    {
        return _shopView.FocusSlot( itemId, out target );
    }

    /// <summary>
    /// 상점 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        if ( targetId == TutorialTargetId.PurchaseButton )
            return _cartView.TryGetPurchaseTarget( out target );

        return _shopView.TryGetTutorialTarget(
            targetId, out target );
    }

    /// <summary>
    /// 지정 상품이 장바구니에 필요한 수량만큼 있는지 확인
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    /// <param name="quantity">확인할 수량</param>
    /// <returns>필요 수량 보유 여부</returns>
    public bool HasCartQuantity ( string itemId, int quantity )
    {
        return _cartModel.GetItem(
            itemId, out CartItem item ) &&
            item.Quantity >= quantity;
    }
    #endregion
}
