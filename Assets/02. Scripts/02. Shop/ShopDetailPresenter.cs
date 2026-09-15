using UnityEngine;

/// <summary>
/// 상점 상세 프레젠터 - 상품 선택, 수량, 장바구니 추가 중재
/// </summary>
public class ShopDetailPresenter
{
    ShopModel _shopModel;       //상점 모델
    CartModel _cartModel;       //장바구니 모델
    PlayStateModel _playStateModel;       //상품 해금 상태 모델
    MaintenanceModel _maintenanceModel;       //연구 해금 조건 모델
    ShopView _shopView;         //상점 뷰
    ShopViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    int _quantity = 1;       //상세 패널 선택 수량
    bool _isSubscribed;      //이벤트 연결 여부

    /// <summary>
    /// 상품 상세 표시 이벤트
    /// </summary>
    public event System.Action<string> OnProductOpened;

    /// <summary>
    /// 상품 장바구니 추가 성공 이벤트
    /// </summary>
    public event System.Action<string, int> OnAddedToCart;

    /// <summary>
    /// 상점 상세 프레젠터 생성
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="cartModel">장바구니 모델</param>
    /// <param name="playStateModel">상품 해금 상태 모델</param>
    /// <param name="maintenanceModel">연구 해금 조건 모델</param>
    /// <param name="shopView">상점 뷰</param>
    public ShopDetailPresenter (
        ShopModel shopModel, CartModel cartModel,
        PlayStateModel playStateModel,
        MaintenanceModel maintenanceModel, ShopView shopView )
    {
        _shopModel = shopModel;
        _cartModel = cartModel;
        _playStateModel = playStateModel;
        _maintenanceModel = maintenanceModel;
        _shopView = shopView;
        _viewDataBuilder = new ShopViewDataBuilder( );
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 상품 상세 입력 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _shopView.OnSlotSelected += ShowProduct;
        _shopView.OnDetailQuantityChanged += ChangeQuantity;
        _shopView.OnDetailQuantitySet += SetQuantity;
        _shopView.OnDetailQuantityMin += SetMin;
        _shopView.OnDetailQuantityMax += SetMax;
        _shopView.OnDetailAddCart += AddToCart;
        _shopView.OnDetailClose += Hide;

        _isSubscribed = true;
    }

    /// <summary>
    /// 상품 상세 입력 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _shopView.OnSlotSelected -= ShowProduct;
        _shopView.OnDetailQuantityChanged -= ChangeQuantity;
        _shopView.OnDetailQuantitySet -= SetQuantity;
        _shopView.OnDetailQuantityMin -= SetMin;
        _shopView.OnDetailQuantityMax -= SetMax;
        _shopView.OnDetailAddCart -= AddToCart;
        _shopView.OnDetailClose -= Hide;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 상세 표시 -----
    /// <summary>
    /// 상세 선택 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _quantity = 1;
    }

    /// <summary>
    /// 선택한 상품 상세 표시
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    public void ShowProduct ( string itemId )
    {
        if ( _shopModel.TryGetItem(
            itemId, out ShopItemModel itemModel ) == false )
        {
            Debug.LogWarning( $"상품 조회 실패: {itemId}" );
            return;
        }

        _quantity = 1;
        _shopView.ShowItemDetail( CreateViewData( itemModel ) );

        OnProductOpened?.Invoke( itemId );
    }

    /// <summary>
    /// 현재 상세 상품 표시 갱신
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    public void RefreshProduct ( string itemId )
    {
        if ( _shopModel.TryGetItem(
            itemId, out ShopItemModel itemModel ) == false )
            return;

        _shopView.ShowItemDetail( CreateViewData( itemModel ) );
        _shopView.UpdateDetailQuantity( _quantity );
    }

    /// <summary>
    /// 상품 상세 패널 숨김
    /// </summary>
    public void Hide ()
    {
        _shopView.HideItemDetail( );
    }

    /// <summary>
    /// 상품 상세 표시 데이터 생성
    /// </summary>
    /// <param name="itemModel">상품 모델</param>
    /// <returns>상품 상세 표시 데이터</returns>
    ShopDetailViewData CreateViewData ( ShopItemModel itemModel )
    {
        ShopItem item = itemModel.Item;
        PurchasableData data = item.Data;
        string description = data.Description;
        bool isLocked = _playStateModel.IsUnlocked( item.Id ) == false;

        if ( data is MaintenanceData maintenanceData )
            description = maintenanceData.GetDescription( 1 );

        if ( isLocked )
            description += $"\n\n잠금 안내\n{GetUnlockText( data )}";

        bool canQuickRestock =
            isLocked == false && data is MaintenanceData == false;

        return _viewDataBuilder.CreateDetail(
            itemModel, description, isLocked, canQuickRestock );
    }

    /// <summary>
    /// 잠긴 상품의 해금 안내 문구 반환
    /// </summary>
    /// <param name="data">잠긴 상품 데이터</param>
    /// <returns>상품 해금 안내 문구</returns>
    string GetUnlockText ( PurchasableData data )
    {
        if ( data is PartsData &&
            _maintenanceModel.TryGetPartUnlockRequirement(
                data.Id, out MaintenanceState researchState,
                out int requiredLevel ) )
        {
            return _viewDataBuilder.CreateResearchUnlockText(
                researchState.Data.Name, requiredLevel );
        }

        return _viewDataBuilder.CreateLockedText( );
    }
    #endregion

    #region ----- 상세 수량 -----
    /// <summary>
    /// 상세 패널 수량 변경
    /// </summary>
    void ChangeQuantity ( string itemId, int amount )
    {
        if ( _shopModel.ChangePurchaseQuantity(
            itemId, _quantity, amount, out int changedQuantity ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            return;
        }

        _quantity = changedQuantity;
        _shopView.UpdateDetailQuantity( _quantity );
    }

    /// <summary>
    /// 상세 패널 수량 직접 설정
    /// </summary>
    void SetQuantity ( string itemId, string value )
    {
        if ( int.TryParse( value, out int quantity ) == false ||
            _shopModel.CanPurchase( itemId, quantity ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            return;
        }

        _quantity = quantity;
        _shopView.UpdateDetailQuantity( _quantity );
    }

    /// <summary>
    /// 상세 패널 수량을 최소로 설정
    /// </summary>
    void SetMin ( string itemId )
    {
        if ( _shopModel.CanPurchase( itemId, 1 ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            return;
        }

        _quantity = 1;
        _shopView.UpdateDetailQuantity( _quantity );
    }

    /// <summary>
    /// 상세 패널 수량을 최대로 설정
    /// </summary>
    void SetMax ( string itemId )
    {
        int cartQuantity = 0;

        if ( _cartModel.GetItem( itemId, out var cartItem ) )
            cartQuantity = cartItem.Quantity;

        if ( _shopModel.GetAddableQuantity(
            itemId, cartQuantity, out var quantity ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            return;
        }

        _quantity = quantity;
        _shopView.UpdateDetailQuantity( _quantity );
    }
    #endregion

    #region ----- 장바구니 추가 -----
    /// <summary>
    /// 상세 상품 장바구니 추가
    /// </summary>
    void AddToCart ( string itemId, string value )
    {
        if ( _playStateModel.IsUnlocked( itemId ) == false )
        {
            Debug.LogWarning(
                $"장바구니 추가 실패: 잠긴 상품 / {itemId}" );
            return;
        }

        if ( int.TryParse( value, out int quantity ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            Debug.LogWarning(
                $"장바구니 추가 실패: 잘못된 수량 {value}" );
            return;
        }

        if ( _shopModel.CanPurchase( itemId, quantity ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            Debug.LogWarning(
                $"장바구니 추가 실패: 구매할 수 없는 수량 {quantity}" );
            return;
        }

        int cartQuantity = 0;

        if ( _cartModel.GetItem( itemId, out var cartItem ) )
            cartQuantity = cartItem.Quantity;

        if ( _shopModel.CanAddToCart(
            itemId, cartQuantity, quantity ) == false )
        {
            _shopView.UpdateDetailQuantity( _quantity );
            Debug.LogWarning(
                $"장바구니 추가 실패: 상품 재고 부족 / {itemId}" );
            return;
        }

        if ( _shopModel.TryGetItem(
            itemId, out var itemModel ) == false )
        {
            Debug.LogWarning(
                $"장바구니 추가 실패: 상품 없음 / {itemId}" );
            return;
        }

        if ( _cartModel.AddItem(
            itemModel.Item.Data, quantity ) == false )
        {
            Debug.LogWarning( $"장바구니 추가 실패: {itemId}" );
            return;
        }

        Debug.Log(
            $"{itemModel.Item.Data.Name} 장바구니에 {quantity}개 추가 완료" );

        OnAddedToCart?.Invoke( itemId, quantity );

        Hide( );
    }
    #endregion
}
