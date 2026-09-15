using System;

/// <summary>
/// 인벤토리 아이템 처리 결과
/// </summary>
public enum InventoryItemActionResult
{
    Success,                //처리 성공
    InvalidStack,           //잘못된 스택
    InvalidQuantity,        //잘못된 수량
    CannotSell,             //판매 불가
    CannotDelete,           //삭제 불가
    CalculationFailed,      //금액 계산 실패
    Failed,                 //처리 실패
}

/// <summary>
/// 인벤토리 아이템 처리 모델 - 선택 스택 판매와 삭제 규칙 관리
/// </summary>
public class InventoryItemActionModel
{
    const float DefaultSaleRate = 0.5f;      //기본 판매 가격 비율

    InventoryModel _inventoryModel;          //인벤토리 모델
    PlayStateModel _playStateModel;          //플레이 상태 모델
    EmployeeModel _employeeModel;            //직원 효과 모델
    float _sellRate;                         //현재 판매 가격 비율

    /// <summary>
    /// 인벤토리 아이템 판매 완료 이벤트
    /// </summary>
    public event Action<float> OnSold;


    /// <summary>
    /// 인벤토리 아이템 처리 모델 생성
    /// </summary>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="employeeModel">직원 효과 모델</param>
    /// <param name="saleRate">판매 가격 비율</param>
    public InventoryItemActionModel (
        InventoryModel inventoryModel, PlayStateModel playStateModel,
        EmployeeModel employeeModel, float saleRate = DefaultSaleRate )
    {
        _inventoryModel = inventoryModel;
        _playStateModel = playStateModel;
        _employeeModel = employeeModel;

        //잘못된 판매 비율 차단
        if ( float.IsNaN( saleRate ) || float.IsInfinity( saleRate ) || saleRate < 0f )
            throw new ArgumentOutOfRangeException( nameof( saleRate ) );

        _sellRate = saleRate;
    }

    /// <summary>
    /// 판매 가능한 아이템인지 확인
    /// </summary>
    /// <param name="data">확인할 아이템 데이터</param>
    /// <returns>판매 가능 여부</returns>
    public bool CanSell ( PurchasableData data )
    {
        //파츠와 소모용품만 판매 허용
        return data != null &&
            ( data.ProductType == ProductType.BabyPart ||
              data.ProductType == ProductType.Consumable );
    }

    /// <summary>
    /// 삭제 가능한 아이템인지 확인
    /// </summary>
    /// <param name="data">확인할 아이템 데이터</param>
    /// <returns>삭제 가능 여부</returns>
    public bool CanDelete ( PurchasableData data )
    {
        //파츠와 소모용품만 삭제 허용
        return data != null &&
            ( data.ProductType == ProductType.BabyPart ||
              data.ProductType == ProductType.Consumable );
    }

    /// <summary>
    /// 선택 수량 사용 가능 여부 확인
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="quantity">선택 수량</param>
    /// <returns>사용 가능 여부</returns>
    public bool CanSetQuantity ( string slotId, int quantity )
    {
        return _inventoryModel.CanRemoveFromStack( slotId, quantity );
    }

    /// <summary>
    /// 현재 선택 수량 변경
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="currentQuantity">현재 선택 수량</param>
    /// <param name="amount">변경 수량</param>
    /// <param name="changedQuantity">변경된 수량</param>
    /// <returns>변경 성공 여부</returns>
    public bool ChangeQuantity ( string slotId, int currentQuantity,
        int amount, out int changedQuantity )
    {
        changedQuantity = currentQuantity;

        //정수 범위를 넘지 않도록 long으로 계산
        long result = ( long ) currentQuantity + amount;

        if ( result < int.MinValue || result > int.MaxValue ||
            CanSetQuantity( slotId, ( int ) result ) == false )
            return false;

        changedQuantity = ( int ) result;
        return true;
    }

    /// <summary>
    /// 선택한 스택의 개당 판매 가격 조회
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="price">개당 판매 가격</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetSellPrice ( string slotId, out float price )
    {
        price = 0f;

        //판매 가능한 스택 조회
        if ( _inventoryModel.GetStack( slotId, out var stack ) == false ||
            CanSell( stack.Data ) == false )
            return false;

        float saleRate = _sellRate;

        //재고 담당 효과값은 기본 판매율에 곱하는 값이 아니라 최종 판매율
        if ( _employeeModel.HasEffect(
            EmployeeEffectType.InventorySaleRate ) )
        {
            saleRate = _employeeModel.GetEffectValue(
                EmployeeEffectType.InventorySaleRate );
        }

        price = stack.Data.BasePrice * saleRate;

        return float.IsNaN( price ) == false &&
            float.IsInfinity( price ) == false && price >= 0f;
    }

    /// <summary>
    /// 선택 수량의 예상 판매 금액 계산
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="quantity">판매 수량</param>
    /// <param name="totalPrice">예상 판매 금액</param>
    /// <returns>계산 성공 여부</returns>
    public bool CalculateSellTotal (
        string slotId, int quantity, out float totalPrice )
    {
        totalPrice = 0f;

        //판매 수량과 개당 판매 가격 확인
        if ( CanSetQuantity( slotId, quantity ) == false ||
            GetSellPrice( slotId, out var unitPrice ) == false )
            return false;

        totalPrice = unitPrice * quantity;

        return float.IsNaN( totalPrice ) == false &&
            float.IsInfinity( totalPrice ) == false && totalPrice >= 0f;
    }

    /// <summary>
    /// 선택한 스택 수량 판매
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="quantity">판매 수량</param>
    /// <returns>판매 처리 결과</returns>
    public InventoryItemActionResult Sell ( string slotId, int quantity )
    {
        //선택한 스택 확인
        if ( _inventoryModel.GetStack( slotId, out var stack ) == false )
            return InventoryItemActionResult.InvalidStack;

        //판매 가능한 아이템 확인
        if ( CanSell( stack.Data ) == false )
            return InventoryItemActionResult.CannotSell;

        //판매 수량 확인
        if ( CanSetQuantity( slotId, quantity ) == false )
            return InventoryItemActionResult.InvalidQuantity;

        //판매 금액 계산
        if ( CalculateSellTotal( slotId, quantity, out var totalPrice ) == false )
            return InventoryItemActionResult.CalculationFailed;

        //선택 수량 제거
        if ( _inventoryModel.RemoveFromStack( slotId, quantity ) == false )
            return InventoryItemActionResult.Failed;

        //판매 금액이 있으면 자금에 추가
        if ( totalPrice > 0f &&
            _playStateModel.AddBudget( totalPrice ) == false )
        {
            //자금 추가 실패 시 제거한 아이템 복구
            _inventoryModel.AddItem( stack.Data, quantity );
            return InventoryItemActionResult.Failed;
        }

        //판매 완료 수익 전달
        OnSold?.Invoke( totalPrice );

        return InventoryItemActionResult.Success;
    }

    /// <summary>
    /// 선택한 스택 수량 삭제
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="quantity">삭제 수량</param>
    /// <returns>삭제 처리 결과</returns>
    public InventoryItemActionResult Delete ( string slotId, int quantity )
    {
        //선택한 스택 확인
        if ( _inventoryModel.GetStack( slotId, out var stack ) == false )
            return InventoryItemActionResult.InvalidStack;

        //삭제 가능한 아이템 확인
        if ( CanDelete( stack.Data ) == false )
            return InventoryItemActionResult.CannotDelete;

        //삭제 수량 확인
        if ( CanSetQuantity( slotId, quantity ) == false )
            return InventoryItemActionResult.InvalidQuantity;

        return _inventoryModel.RemoveFromStack( slotId, quantity )
            ? InventoryItemActionResult.Success
            : InventoryItemActionResult.Failed;
    }
}
