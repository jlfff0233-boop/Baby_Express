using System;
using UnityEngine;

/// <summary>
/// 인벤토리 아이템 처리 프레젠터 - 상세, 판매, 삭제 흐름 중재
/// </summary>
public class InventoryItemActionPresenter
{
    InventoryModel _inventoryModel;       //인벤토리 모델
    InventoryItemActionModel _itemActionModel;       //아이템 처리 모델
    InventoryView _inventoryView;         //인벤토리 뷰
    InventoryViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    string _selectedSlotId;       //선택한 슬롯 아이디
    int _selectedQuantity = 1;    //선택 수량
    bool _isSelling;              //현재 판매 처리 여부
    bool _isSubscribed;           //이벤트 연결 여부

    /// <summary>
    /// 상점 이동 요청 이벤트(아이템 아이디)
    /// </summary>
    public event Action<string> OnMoveToShop;

    /// <summary>
    /// 제작 이동 요청 이벤트(아이템 아이디)
    /// </summary>
    public event Action<string> OnMoveToCraft;

    /// <summary>
    /// 인벤토리 아이템 상세 표시 이벤트
    /// </summary>
    public event Action<string> OnItemSelected;

    /// <summary>
    /// 인벤토리 아이템 처리 프레젠터 생성
    /// </summary>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="itemActionModel">아이템 처리 모델</param>
    /// <param name="inventoryView">인벤토리 뷰</param>
    public InventoryItemActionPresenter (
        InventoryModel inventoryModel,
        InventoryItemActionModel itemActionModel,
        InventoryView inventoryView )
    {
        _inventoryModel = inventoryModel;
        _itemActionModel = itemActionModel;
        _inventoryView = inventoryView;
        _viewDataBuilder = new InventoryViewDataBuilder( );
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 아이템 상세와 처리 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _inventoryView.OnSlotSelected += SelectItem;
        _inventoryView.OnMoveToShop += MoveToShop;
        _inventoryView.OnUse += UseItem;
        _inventoryView.OnMoveToCraft += MoveToCraft;
        _inventoryView.OnSell += OpenSell;
        _inventoryView.OnDelete += OpenDelete;
        _inventoryView.OnDetailClose += CloseDetail;

        _inventoryView.OnSellQuantityChanged += ChangeQuantity;
        _inventoryView.OnSellQuantitySet += SetQuantity;
        _inventoryView.OnSellMin += SetMinQuantity;
        _inventoryView.OnSellMax += SetMaxQuantity;
        _inventoryView.OnSellConfirmed += ConfirmSell;
        _inventoryView.OnSellClosed += CloseSell;

        _inventoryView.OnDeleteQuantityChanged += ChangeQuantity;
        _inventoryView.OnDeleteQuantitySet += SetQuantity;
        _inventoryView.OnDeleteMin += SetMinQuantity;
        _inventoryView.OnDeleteMax += SetMaxQuantity;
        _inventoryView.OnDeleteConfirmed += ConfirmDelete;
        _inventoryView.OnDeleteClosed += CloseDelete;

        _isSubscribed = true;
    }

    /// <summary>
    /// 아이템 상세와 처리 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _inventoryView.OnSlotSelected -= SelectItem;
        _inventoryView.OnMoveToShop -= MoveToShop;
        _inventoryView.OnUse -= UseItem;
        _inventoryView.OnMoveToCraft -= MoveToCraft;
        _inventoryView.OnSell -= OpenSell;
        _inventoryView.OnDelete -= OpenDelete;
        _inventoryView.OnDetailClose -= CloseDetail;

        _inventoryView.OnSellQuantityChanged -= ChangeQuantity;
        _inventoryView.OnSellQuantitySet -= SetQuantity;
        _inventoryView.OnSellMin -= SetMinQuantity;
        _inventoryView.OnSellMax -= SetMaxQuantity;
        _inventoryView.OnSellConfirmed -= ConfirmSell;
        _inventoryView.OnSellClosed -= CloseSell;

        _inventoryView.OnDeleteQuantityChanged -= ChangeQuantity;
        _inventoryView.OnDeleteQuantitySet -= SetQuantity;
        _inventoryView.OnDeleteMin -= SetMinQuantity;
        _inventoryView.OnDeleteMax -= SetMaxQuantity;
        _inventoryView.OnDeleteConfirmed -= ConfirmDelete;
        _inventoryView.OnDeleteClosed -= CloseDelete;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 상세 패널 -----
    /// <summary>
    /// 아이템 처리 패널 모두 숨김
    /// </summary>
    public void HidePanels ()
    {
        _inventoryView.HideSell( );
        _inventoryView.HideDelete( );
        _inventoryView.HideDetail( );
    }

    /// <summary>
    /// 선택한 아이템 상세 표시
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="itemId">선택한 아이템 아이디</param>
    void SelectItem ( string slotId, string itemId )
    {
        if ( _inventoryModel.GetStack( slotId, out var stack ) == false ||
            stack.ItemId != itemId )
        {
            Debug.LogWarning( $"인벤토리 스택 조회 실패: {slotId}" );
            return;
        }

        _inventoryView.HideSell( );
        _inventoryView.HideDelete( );

        //사용과 제작 기능은 추후 담당 시스템 연결 시 활성화
        bool canUse = false;
        bool canCraft = false;

        _inventoryView.ShowDetail(
            _viewDataBuilder.CreateDetail(
                stack, true, canUse, canCraft,
                _itemActionModel.CanSell( stack.Data ),
                _itemActionModel.CanDelete( stack.Data ) ) );

        OnItemSelected?.Invoke( itemId );
    }

    /// <summary>
    /// 인벤토리 상세 패널 닫기
    /// </summary>
    void CloseDetail ()
    {
        HidePanels( );
    }

    /// <summary>
    /// 상점 이동 요청 전달
    /// </summary>
    /// <param name="itemId">선택한 아이템 아이디</param>
    void MoveToShop ( string itemId )
    {
        OnMoveToShop?.Invoke( itemId );
        Debug.Log( $"상점 이동 요청: {itemId}" );
    }

    /// <summary>
    /// 아이템 사용 요청 처리
    /// </summary>
    /// <param name="itemId">선택한 아이템 아이디</param>
    void UseItem ( string itemId )
    {
        Debug.Log( $"아이템 사용 기능 준비 중: {itemId}" );
    }

    /// <summary>
    /// 제작 이동 요청 전달
    /// </summary>
    /// <param name="itemId">선택한 아이템 아이디</param>
    void MoveToCraft ( string itemId )
    {
        OnMoveToCraft?.Invoke( itemId );
        Debug.Log( $"제작 이동 요청: {itemId}" );
    }
    #endregion

    #region ----- 판매와 삭제 -----
    /// <summary>
    /// 아이템 판매 패널 열기
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    void OpenSell ( string slotId )
    {
        if ( _inventoryModel.GetStack( slotId, out var stack ) == false ||
            _itemActionModel.CanSell( stack.Data ) == false )
            return;

        _selectedSlotId = slotId;
        _selectedQuantity = 1;
        _isSelling = true;

        _inventoryView.ShowSell( CreateSellViewData( stack ) );
    }

    /// <summary>
    /// 판매 패널 표시 데이터 생성
    /// </summary>
    /// <param name="stack">선택한 인벤토리 스택</param>
    /// <returns>판매 패널 표시 데이터</returns>
    InventoryItemSellViewData CreateSellViewData ( InventoryStack stack )
    {
        //판매 가격 가져오기
        _itemActionModel.GetSellPrice( stack.SlotId, out var unitPrice );
        //가격 계산
        _itemActionModel.CalculateSellTotal(
            stack.SlotId, _selectedQuantity, out var totalPrice );

        return _viewDataBuilder.CreateSell(
            stack, _selectedQuantity, unitPrice, totalPrice );
    }

    /// <summary>
    /// 아이템 삭제 패널 열기
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    void OpenDelete ( string slotId )
    {
        if ( _inventoryModel.GetStack( slotId, out var stack ) == false ||
            _itemActionModel.CanDelete( stack.Data ) == false )
            return;

        _selectedSlotId = slotId;
        _selectedQuantity = 1;
        _isSelling = false;

        _inventoryView.ShowDelete(
            _viewDataBuilder.CreateDelete( stack, _selectedQuantity ) );
    }

    /// <summary>
    /// 작업 수량 변경
    /// </summary>
    /// <param name="amount">변경 수량</param>
    void ChangeQuantity ( int amount )
    {
        if ( _itemActionModel.ChangeQuantity(
            _selectedSlotId, _selectedQuantity,
            amount, out var changedQuantity ) )
            _selectedQuantity = changedQuantity;

        RefreshActionPanel( );
    }

    /// <summary>
    /// 작업 수량 직접 설정
    /// </summary>
    /// <param name="value">입력한 수량</param>
    void SetQuantity ( string value )
    {
        if ( int.TryParse( value, out var quantity ) )
            ApplyQuantity( quantity );
        else
            RefreshActionPanel( );
    }

    /// <summary>
    /// 검증된 작업 수량 적용
    /// </summary>
    /// <param name="quantity">적용할 수량</param>
    void ApplyQuantity ( int quantity )
    {
        if ( _itemActionModel.CanSetQuantity(
            _selectedSlotId, quantity ) )
            _selectedQuantity = quantity;

        RefreshActionPanel( );
    }

    /// <summary>
    /// 작업 수량을 최소로 설정
    /// </summary>
    void SetMinQuantity ()
    {
        ApplyQuantity( 1 );
    }

    /// <summary>
    /// 작업 수량을 최대로 설정
    /// </summary>
    void SetMaxQuantity ()
    {
        if ( _inventoryModel.GetStack( _selectedSlotId, out var stack ) )
            ApplyQuantity( stack.Quantity );
    }

    /// <summary>
    /// 현재 작업 패널 갱신
    /// </summary>
    void RefreshActionPanel ()
    {
        if ( _inventoryModel.GetStack(
            _selectedSlotId, out var stack ) == false )
            return;

        if ( _isSelling )
        {
            _inventoryView.UpdateSell( CreateSellViewData( stack ) );
            return;
        }

        _inventoryView.UpdateDelete(
            _viewDataBuilder.CreateDelete( stack, _selectedQuantity ) );
    }

    /// <summary>
    /// 아이템 판매 확정
    /// </summary>
    void ConfirmSell ()
    {
        InventoryItemActionResult result = _itemActionModel.Sell(
            _selectedSlotId, _selectedQuantity );

        LogResult( "판매", result );

        if ( result == InventoryItemActionResult.Success )
        {
            _inventoryView.HideSell( );
            _inventoryView.HideDetail( );
        }
    }

    /// <summary>
    /// 아이템 삭제 확정
    /// </summary>
    void ConfirmDelete ()
    {
        InventoryItemActionResult result = _itemActionModel.Delete(
            _selectedSlotId, _selectedQuantity );

        LogResult( "삭제", result );

        if ( result == InventoryItemActionResult.Success )
        {
            _inventoryView.HideDelete( );
            _inventoryView.HideDetail( );
        }
    }

    /// <summary>
    /// 아이템 처리 결과 로그
    /// </summary>
    /// <param name="action">처리 내용</param>
    /// <param name="result">처리 결과</param>
    void LogResult ( string action, InventoryItemActionResult result )
    {
        if ( result == InventoryItemActionResult.Success )
        {
            Debug.Log( $"아이템 {action} 성공" );
            return;
        }

        Debug.LogWarning( $"아이템 {action} 실패: {result}" );
    }

    /// <summary>
    /// 판매 패널 닫기
    /// </summary>
    void CloseSell ()
    {
        _inventoryView.HideSell( );
    }

    /// <summary>
    /// 삭제 패널 닫기
    /// </summary>
    void CloseDelete ()
    {
        _inventoryView.HideDelete( );
    }
    #endregion
}
