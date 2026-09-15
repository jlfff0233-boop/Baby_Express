using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장바구니 프레젠터 - 수량, 금액 표시와 구매 처리 중재
/// </summary>
public class CartPresenter
{
    ShopModel _shopModel;       //상점 모델
    CartModel _cartModel;       //장바구니 모델
    PurchaseModel _purchaseModel;       //구매 모델
    PlayStateModel _playStateModel;       //상품 해금 상태 모델
    ShopView _shopView;         //상점 뷰
    CartView _cartView;         //장바구니 뷰
    ShopViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 구매 성공 이벤트
    /// </summary>
    public event Action OnPurchased;

    /// <summary>
    /// 장바구니 패널 퇴장 완료 이벤트
    /// </summary>
    public event Action OnClosed;

    /// <summary>
    /// 장바구니 프레젠터 생성
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="cartModel">장바구니 모델</param>
    /// <param name="purchaseModel">구매 모델</param>
    /// <param name="playStateModel">상품 해금 상태 모델</param>
    /// <param name="shopView">상점 뷰</param>
    /// <param name="cartView">장바구니 뷰</param>
    public CartPresenter (
        ShopModel shopModel, CartModel cartModel,
        PurchaseModel purchaseModel, PlayStateModel playStateModel,
        ShopView shopView, CartView cartView )
    {
        _shopModel = shopModel;
        _cartModel = cartModel;
        _purchaseModel = purchaseModel;
        _playStateModel = playStateModel;
        _shopView = shopView;
        _cartView = cartView;
        _viewDataBuilder = new ShopViewDataBuilder( );
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 장바구니 입력과 상태 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _shopView.OnCartOpen += Open;
        _cartView.OnQuantityChange += ChangeQuantity;
        _cartView.OnQuantitySet += SetQuantity;
        _cartView.OnRemove += RemoveItem;
        _cartView.OnClear += Clear;
        _cartView.OnPurchase += Purchase;
        _cartView.OnClose += Hide;
        _cartModel.OnCartChanged += Refresh;

        _isSubscribed = true;
    }

    /// <summary>
    /// 장바구니 입력과 상태 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _shopView.OnCartOpen -= Open;
        _cartView.OnQuantityChange -= ChangeQuantity;
        _cartView.OnQuantitySet -= SetQuantity;
        _cartView.OnRemove -= RemoveItem;
        _cartView.OnClear -= Clear;
        _cartView.OnPurchase -= Purchase;
        _cartView.OnClose -= Hide;
        _cartModel.OnCartChanged -= Refresh;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 장바구니 패널 -----
    /// <summary>
    /// 장바구니 패널 표시
    /// </summary>
    void Open ()
    {
        Refresh( );
        _cartView.ShowPanel( );
    }

    /// <summary>
    /// 장바구니 패널 숨김
    /// </summary>
    public void Hide ()
    {
        _cartView.HidePanel( () => OnClosed?.Invoke( ) );
    }

    /// <summary>
    /// 장바구니 패널 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _cartView.HideInstant( );
    }

    /// <summary>
    /// 장바구니 표시 갱신
    /// </summary>
    public void Refresh ()
    {
        var viewDatas = new List<CartItemViewData>( );

        foreach ( CartItem item in _cartModel.Items )
        {
            //장바구니 아이템 뷰 데이터 가져오기
            CartItemViewData viewData = CreateViewData( item );

            //없으면 추가
            if ( viewData != null )
                viewDatas.Add( viewData );
        }

        //장바구니 아이템 총액 계산
        if ( _purchaseModel.CalculateTotal( out var totalPrice ) == false )
            totalPrice = 0f;

        //구매 후 잔액 계산
        if ( _purchaseModel.CalculateExpectedBudget(
            out float expectedBudget ) == false )
            expectedBudget = 0f;

        //장바구니뷰 갱신
        _cartView.RefreshItems(
            viewDatas, totalPrice, expectedBudget );
    }

    /// <summary>
    /// 장바구니 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="item">장바구니 상품</param>
    /// <returns>장바구니 슬롯 표시 데이터</returns>
    CartItemViewData CreateViewData ( CartItem item )
    {
        //상품 현재가 가져오기
        if ( _purchaseModel.GetCurrentPrice(
            item.Data.Id, out var price ) == false )
            return null;

        //상품 소계 계산
        if ( _purchaseModel.CalculateSubtotal(
            item.Data.Id, item.Quantity, out var subtotal ) == false )
            return null;

        //슬롯뷰 데이터 생성
        return _viewDataBuilder.CreateCartItem(
            item, price, subtotal );
    }
    #endregion

    #region ----- 장바구니 처리 -----
    /// <summary>
    /// 장바구니 수량 변경
    /// </summary>
    void ChangeQuantity ( string itemId, int amount )
    {
        //상품 조회
        if ( _cartModel.GetItem( itemId, out var item ) == false ) return;

        //장바구니 수량 변경
        if ( _shopModel.ChangeCartQuantity(
            itemId, item.Quantity, amount,
            out var changedQuantity ) == false )
        {
            //지정 상품 수량 복구
            _cartView.UpdateItemQuantity( itemId, item.Quantity );
            return;
        }

        //수량 설정
        _cartModel.SetQuantity( itemId, changedQuantity );
    }

    /// <summary>
    /// 장바구니 최종 수량 설정
    /// </summary>
    void SetQuantity ( string itemId, string value )
    {
        //상품 조회
        if ( _cartModel.GetItem( itemId, out var item ) == false ) return;

        //상품 수량 확인 및 설정 시도
        if ( int.TryParse( value, out var quantity ) == false ||
            _shopModel.CanSetCartQuantity( itemId, quantity ) == false )
        {
            //실패 시 수량 복구
            _cartView.UpdateItemQuantity( itemId, item.Quantity );
            return;
        }

        //수량 직접 설정(인풋 필드)
        _cartModel.SetQuantity( itemId, quantity );
    }

    /// <summary>
    /// 장바구니 상품 제거
    /// </summary>
    void RemoveItem ( string itemId )
    {
        _cartModel.RemoveItem( itemId );
    }

    /// <summary>
    /// 장바구니 전체 비우기
    /// </summary>
    void Clear ()
    {
        _cartModel.Clear( );
    }

    /// <summary>
    /// 장바구니 아이템 구매
    /// </summary>
    void Purchase ()
    {
        PurchaseResult result = _purchaseModel.Purchase( );
        LogResult( result );

        if ( result != PurchaseResult.Success ) return;

        Hide( );
        OnPurchased?.Invoke( );
    }

    /// <summary>
    /// 구매 처리 결과 로그
    /// </summary>
    /// <param name="result">구매 처리 결과</param>
    void LogResult ( PurchaseResult result )
    {
        switch ( result )
        {
            case PurchaseResult.Success:
                Debug.Log( "구매 완료" );
                break;
            case PurchaseResult.EmptyCart:
                Debug.LogWarning( "구매 실패: 장바구니가 비어 있음" );
                break;
            case PurchaseResult.InvalidItem:
                Debug.LogWarning( "구매 실패: 잘못된 상품 포함" );
                break;
            case PurchaseResult.LockedItem:
                Debug.LogWarning( "구매 실패: 잠긴 상품 포함" );
                break;
            case PurchaseResult.InsufficientStock:
                Debug.LogWarning( "구매 실패: 상품 재고 부족" );
                break;
            case PurchaseResult.InsufficientCapacity:
                Debug.LogWarning( "구매 실패: 인벤토리 용량 부족" );
                break;
            case PurchaseResult.InsufficientBudget:
                Debug.LogWarning( "구매 실패: 자금 부족" );
                break;
            case PurchaseResult.CalculationFailed:
                Debug.LogWarning( "구매 실패: 금액 계산 오류" );
                break;
            case PurchaseResult.AlreadyOwnedProduct:
                Debug.LogWarning( "구매 실패: 이미 보유한 상품" );
                break;
            default:
                Debug.LogWarning( "구매 처리 중 오류 발생" );
                break;
        }
    }
    #endregion

#if UNITY_EDITOR
    #region ----- 테스트 -----
    /// <summary>
    /// 전체 파츠 1개씩 구매
    /// </summary>
    public void PurchaseAllPartsDebug ()
    {
        _cartModel.Clear( );
        int addedCount = 0;

        foreach ( ShopItemModel itemModel in _shopModel.Items )
        {
            ShopItem item = itemModel.Item;

            if ( item.Data is not PartsData partData ||
                item.RemainingStock <= 0 )
                continue;

            _playStateModel.SetUnlocked( partData.Id, true );

            if ( _cartModel.AddItem( partData, 1 ) )
                addedCount++;
        }

        if ( addedCount <= 0 )
        {
            Debug.LogWarning(
                "전체 파츠 구매 실패: 구매 가능한 파츠 없음" );
            return;
        }

        Purchase( );
    }
    #endregion
#endif
}
