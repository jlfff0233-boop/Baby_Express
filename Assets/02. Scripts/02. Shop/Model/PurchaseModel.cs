using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구매 처리 결과
/// </summary>
public enum PurchaseResult
{
    Success,                //구매 성공
    EmptyCart,              //장바구니 비어 있음
    InvalidItem,            //잘못된 상품
    LockedItem,             //잠긴 상품
    AlreadyOwnedProduct,    //이미 보유한 상품
    InsufficientStock,      //재고 부족
    InsufficientCapacity,   //인벤토리 용량 부족
    InsufficientBudget,     //자금 부족
    CalculationFailed,      //금액 계산 실패
    Failed,                 //구매 처리 실패
}


/// <summary>
/// 구매 완료 상품 내역
/// </summary>
public class PurchaseReceiptItem
{
    /// <summary>
    /// 구매한 상품 데이터
    /// </summary>
    public PurchasableData Data { get; set; }

    /// <summary>
    /// 구매 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 상품 구매 총액
    /// </summary>
    public float PriceTotal { get; set; }
}

/// <summary>
/// 장바구니 금액 계산 및 구매 규칙 관리
/// </summary>
public class PurchaseModel
{
    ShopModel _shopModel;               //현재 상품 상태
    CartModel _cartModel;               //현재 장바구니 상태
    InventoryModel _inventoryModel;     //현재 인벤토리 상태
    PlayStateModel _playStateModel;     //현재 플레이 상태
    MaintenanceModel _maintenanceModel;     //정비 구매 상태
    MaintenanceEffectModel _maintenanceEffectModel;       //정비 효과 적용 모델

    /// <summary>
    /// 상품 구매 완료 이벤트
    /// </summary>
    public event Action<IReadOnlyList<PurchaseReceiptItem>> OnPurchased;

    /// <summary>
    /// 구매 모델 생성
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="cartModel">장바구니 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="maintenanceModel">정비 모델</param>
    /// <param name="maintenanceEffectModel">정비 효과 모델</param>
    public PurchaseModel ( ShopModel shopModel, CartModel cartModel,
        InventoryModel inventoryModel, PlayStateModel playStateModel,
        MaintenanceModel maintenanceModel,
        MaintenanceEffectModel maintenanceEffectModel )
    {
        _shopModel = shopModel;
        _cartModel = cartModel;
        _inventoryModel = inventoryModel;
        _playStateModel = playStateModel;
        _maintenanceModel = maintenanceModel;
        _maintenanceEffectModel = maintenanceEffectModel;
    }

    /// <summary>
    /// 상품의 현재 가격 조회
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="price">현재 가격</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetCurrentPrice ( string id, out float price )
    {
        price = 0f;

        //상품 조회
        if ( _shopModel.TryGetItem( id, out var itemModel ) == false )
        {
            return false;
        }

        //현재 가격 반환
        price = itemModel.Item.CurrentPrice;
        return true;
    }

    /// <summary>
    /// 상품 소계 계산
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">장바구니 수량</param>
    /// <param name="subtotal">상품 소계</param>
    /// <returns>계산 성공 여부</returns>
    public bool CalculateSubtotal ( string id, int quantity, out float subtotal )
    {
        subtotal = 0f;

        //유효 수량 확인
        if ( quantity <= 0 )
        {
            return false;
        }

        //현재 가격 조회
        if ( GetCurrentPrice( id, out var price ) == false )
        {
            return false;
        }

        //현재 가격을 기준으로 소계 계산
        subtotal = price * quantity;
        return true;
    }

    /// <summary>
    /// 장바구니 총액 계산
    /// </summary>
    /// <param name="totalPrice">장바구니 총액</param>
    /// <returns>계산 성공 여부</returns>
    public bool CalculateTotal ( out float totalPrice )
    {
        totalPrice = 0f;

        //현재 장바구니 항목 순회
        foreach ( var item in _cartModel.Items )
        {
            //상품 소계 계산
            if ( CalculateSubtotal( item.Data.Id, item.Quantity, out var subtotal ) == false )
            {
                totalPrice = 0f;
                return false;
            }

            //전체 금액에 소계 누적
            totalPrice += subtotal;
        }

        return true;
    }

    /// <summary>
    /// 구매할 인벤토리 아이템 목록 생성
    /// </summary>
    /// <param name="purchaseItems">구매할 아이템 목록</param>
    /// <returns>생성 성공 여부</returns>
    bool CreatePurchaseItems (
        out List<InventoryItemAmount> purchaseItems )
    {
        purchaseItems = new List<InventoryItemAmount>( _cartModel.Count );

        //장바구니 상품을 인벤토리 아이템 목록으로 변환
        foreach ( var cartItem in _cartModel.Items )
        {
            purchaseItems.Add(
                new InventoryItemAmount(
                    cartItem.Data,
                    cartItem.Quantity ) );
        }

        return purchaseItems.Count > 0;
    }

    /// <summary>
    /// 장바구니 아이템 구매
    /// </summary>
    /// <returns>구매 처리 결과</returns>
    public PurchaseResult Purchase ()
    {
        //모든 구매 조건 사전 검증
        PurchaseResult result = ValidatePurchase(
            out List<InventoryItemAmount> purchaseItems,
            out float totalPrice );

        if ( result != PurchaseResult.Success ) return result;

        //현재 상품 가격으로 구매 내역 생성
        List<PurchaseReceiptItem> receipt = CreatePurchaseReceipt( purchaseItems );

        //구매 상품을 인벤토리에 일괄 추가
        if ( _inventoryModel.AddItems( purchaseItems ) == false )
            return PurchaseResult.Failed;

        int removedStockCount = 0;

        //구매 상품 재고 차감
        for ( int i = 0; i < purchaseItems.Count; i++ )
        {
            InventoryItemAmount purchaseItem = purchaseItems [ i ];

            if ( _shopModel.RemoveStock(
                purchaseItem.Data.Id, purchaseItem.Quantity ) == false )
            {
                RollbackPurchase( purchaseItems, removedStockCount );
                return PurchaseResult.Failed;
            }

            removedStockCount++;
        }

        //구매 금액 차감
        if ( totalPrice > 0f &&
            _playStateModel.SpendBudget( totalPrice ) == false )
        {
            RollbackPurchase( purchaseItems, removedStockCount );
            return PurchaseResult.Failed;
        }

        //구매한 정비 상품의 최초 단계를 등록
        var acquiredMaintenanceIds = new List<string>( );

        if ( AcquireMaintenance(
            purchaseItems, acquiredMaintenanceIds ) == false )
        {
            bool isMaintenanceRolledBack =
                RollbackMaintenance( acquiredMaintenanceIds );

            bool isBudgetRolledBack = totalPrice <= 0f ||
                _playStateModel.AddBudget( totalPrice );

            RollbackPurchase( purchaseItems, removedStockCount );

            if ( isMaintenanceRolledBack == false ||
                isBudgetRolledBack == false )
                Debug.LogWarning( "구매 실패 후 정비 또는 자금 복구에 실패했습니다." );

            return PurchaseResult.Failed;
        }

        //구매 완료 후 장바구니 초기화
        if ( _cartModel.Clear( ) == false )
        {
            bool isMaintenanceRolledBack =
                RollbackMaintenance( acquiredMaintenanceIds );

            bool isBudgetRolledBack = totalPrice <= 0f ||
                _playStateModel.AddBudget( totalPrice );

            RollbackPurchase( purchaseItems, removedStockCount );

            if ( isMaintenanceRolledBack == false ||
                isBudgetRolledBack == false )
                Debug.LogWarning( "장바구니 초기화 실패 후 정비 또는 자금 복구에 실패했습니다." );

            return PurchaseResult.Failed;
        }

        //구매 완료 내역 전달
        OnPurchased?.Invoke( receipt );
        return PurchaseResult.Success;
    }

    /// <summary>
    /// 정비 아이템 구매 및 효과 적용
    /// </summary>
    /// <param name="purchaseItems">구매 상품 목록</param>
    /// <param name="acquiredMaintenanceIds">등록한 정비 아이디 목록</param>
    /// <returns>정비 등록 성공 여부</returns>
    bool AcquireMaintenance (
        IReadOnlyList<InventoryItemAmount> purchaseItems,
        List<string> acquiredMaintenanceIds )
    {
        for ( int i = 0; i < purchaseItems.Count; i++ )
        {
            InventoryItemAmount purchaseItem = purchaseItems [ i ];

            //정비 상품이 아니면 다음 상품 확인
            if ( purchaseItem.Data is not MaintenanceData )
                continue;

            string id = purchaseItem.Data.Id;

            //최초 정비 구매 등록
            if ( _maintenanceModel.Acquire( id ) !=
                MaintenanceResult.Success )
                return false;

            acquiredMaintenanceIds.Add( id );

            //즉시 적용 정비 효과 처리
            if ( _maintenanceEffectModel.ApplyImmediate( id ) !=
                MaintenanceEffectResult.Success )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 구매 과정에서 등록한 정비 상태와 즉시 효과 복구
    /// </summary>
    /// <param name="maintenanceIds">복구할 정비 아이디 목록</param>
    /// <returns>전체 정비 상태와 효과 복구 성공 여부</returns>
    bool RollbackMaintenance ( IReadOnlyList<string> maintenanceIds )
    {
        bool isRolledBack = true;

        //등록한 역순으로 효과와 구매 단계 복구
        for ( int i = maintenanceIds.Count - 1; i >= 0; i-- )
        {
            string id = maintenanceIds [ i ];

            MaintenanceEffectResult effectResult =
                _maintenanceEffectModel.RollbackImmediate( id );

            if ( effectResult != MaintenanceEffectResult.Success )
            {
                Debug.LogWarning(
                    $"정비 효과 복구 실패: {id}, {effectResult}" );
                isRolledBack = false;
                continue;
            }

            MaintenanceResult stateResult =
                _maintenanceModel.RollbackUpgrade( id );

            if ( stateResult == MaintenanceResult.Success ) continue;

            Debug.LogWarning(
                $"정비 구매 단계 복구 실패: {id}, {stateResult}" );
            isRolledBack = false;
        }

        return isRolledBack;
    }

    /// <summary>
    /// 구매 완료 상품 내역 생성
    /// </summary>
    /// <param name="purchaseItems">구매할 아이템 목록</param>
    /// <returns>구매 완료 상품 내역</returns>
    List<PurchaseReceiptItem> CreatePurchaseReceipt (
        IReadOnlyList<InventoryItemAmount> purchaseItems )
    {
        var receipt = new List<PurchaseReceiptItem>( purchaseItems.Count );

        for ( int i = 0; i < purchaseItems.Count; i++ )
        {
            //구매 아이템 수량 가져오기
            InventoryItemAmount purchaseItem = purchaseItems [ i ];

            //현재 상품 가격 조회
            GetCurrentPrice( purchaseItem.Data.Id, out float unitPrice );

            //영수증에 구매 아이템 추가
            receipt.Add( new PurchaseReceiptItem
            {
                Data = purchaseItem.Data,
                Quantity = purchaseItem.Quantity,
                PriceTotal = unitPrice * purchaseItem.Quantity
            } );
        }

        return receipt;
    }

    /// <summary>
    /// 구매 가능 여부 검증
    /// </summary>
    /// <param name="purchaseItems">구매할 아이템 목록</param>
    /// <param name="totalPrice">구매 총액</param>
    /// <returns>구매 처리 결과</returns>
    PurchaseResult ValidatePurchase ( out List<InventoryItemAmount> purchaseItems, out float totalPrice )
    {
        purchaseItems = null;
        totalPrice = 0f;

        //빈 장바구니 차단
        if ( _cartModel.IsCartEmpty )
            return PurchaseResult.EmptyCart;

        //구매 아이템 목록 생성
        if ( CreatePurchaseItems( out purchaseItems ) == false )
            return PurchaseResult.InvalidItem;

        //모든 상품의 현재 재고 확인
        foreach ( var purchaseItem in purchaseItems )
        {
            string id = purchaseItem.Data.Id;

            //상점에 없는 상품 차단
            if ( _shopModel.TryGetItem( id, out var itemModel ) == false )
                return PurchaseResult.InvalidItem;

            //잠긴 상품 차단
            if ( _playStateModel.IsUnlocked( id ) == false )
                return PurchaseResult.LockedItem;

            //정비 상품의 최초 구매 상태 확인
            if ( purchaseItem.Data is MaintenanceData )
            {
                //정비 상품은 한 번에 하나만 구매
                if ( purchaseItem.Quantity != 1 )
                    return PurchaseResult.InvalidItem;

                //정비 결과 가져오기
                MaintenanceResult maintenanceResult =
                    _maintenanceModel.GetAcquireResult( id );

                //이미 구매한 정비 상품 차단
                if ( maintenanceResult == MaintenanceResult.AlreadyOwned )
                    return PurchaseResult.AlreadyOwnedProduct;

                //잘못된 정비 데이터 차단
                if ( maintenanceResult != MaintenanceResult.Success )
                    return PurchaseResult.InvalidItem;
            }

            //재고 부족 상품 차단
            if ( itemModel.CanPurchase( purchaseItem.Quantity ) == false )
                return PurchaseResult.InsufficientStock;
        }

        //현재 가격 기준 총액 계산
        if ( CalculateTotal( out totalPrice ) == false ||
            float.IsNaN( totalPrice ) ||
            float.IsInfinity( totalPrice ) ||
            totalPrice < 0f )
            return PurchaseResult.CalculationFailed;

        //인벤토리 전체 용량 확인
        if ( _inventoryModel.CanAddItems( purchaseItems ) == false )
            return PurchaseResult.InsufficientCapacity;

        //현재 자금 확인
        if ( _playStateModel.CanSpendBudget( totalPrice ) == false )
            return PurchaseResult.InsufficientBudget;

        return PurchaseResult.Success;
    }

    /// <summary>
    /// 실패한 구매 변경 내용 복구
    /// </summary>
    /// <param name="purchaseItems">구매 아이템 목록</param>
    /// <param name="removedStockCount">재고가 차감된 상품 수</param>
    void RollbackPurchase ( IReadOnlyList<InventoryItemAmount> purchaseItems, int removedStockCount )
    {
        //차감된 상품 재고 복구
        for ( int i = 0; i < removedStockCount; i++ )
        {
            InventoryItemAmount purchaseItem = purchaseItems [ i ];

            _shopModel.AddStock( purchaseItem.Data.Id, purchaseItem.Quantity );
        }

        //추가된 인벤토리 아이템 복구
        foreach ( var purchaseItem in purchaseItems )
        {
            _inventoryModel.RemoveItem( purchaseItem.Data.Id, purchaseItem.Quantity );
        }
    }

    /// <summary>
    /// 구매 후 예상 잔액 계산
    /// </summary>
    /// <param name="expectedBudget">구매 후 예상 잔액</param>
    /// <returns>계산 성공 여부</returns>
    public bool CalculateExpectedBudget ( out float expectedBudget )
    {
        expectedBudget = _playStateModel.Budget;

        //현재 가격 기준 장바구니 총액 계산
        if ( CalculateTotal( out float totalPrice ) == false )
            return false;

        //음수 잔액도 표시하기 위해 차감 가능 여부는 확인하지 않음
        expectedBudget = _playStateModel.Budget - totalPrice;

        return float.IsNaN( expectedBudget ) == false &&
            float.IsInfinity( expectedBudget ) == false;
    }

}
