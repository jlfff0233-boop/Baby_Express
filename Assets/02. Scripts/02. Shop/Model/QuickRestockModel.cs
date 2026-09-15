using System;

/// <summary>
/// 빠른 재입고 처리 결과
/// </summary>
public enum QuickRestockResult
{
    Success,       //재입고 성공
    InvalidItem,       //잘못된 상품
    LockedItem,       //잠긴 상품
    FullStock,       //최대 재고
    DailyLimitReached,       //일일 최대 횟수 도달
    InvalidFee,       //잘못된 이용료
    InsufficientBudget,       //자금 부족
    BudgetUpdateFailed,       //자금 차감 실패
}

/// <summary>
/// 빠른 재입고 모델 - 이용료, 상품 재고, 일일 횟수 통합 처리
/// </summary>
public class QuickRestockModel
{
    ShopModel _shopModel;       //상점 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    BusinessDayModel _businessDayModel;       //영업일 모델
    EmployeeModel _employeeModel;       //직원 효과 모델
    float _feeRate;       //빠른 재입고 이용료 배율

    /// <summary>
    /// 오늘 빠른 재입고 횟수
    /// </summary>
    public int TodayCount => _businessDayModel.QuickRestockCount;

    /// <summary>
    /// 일일 빠른 재입고 최대 횟수
    /// </summary>
    public int DailyLimit => _businessDayModel.QuickRestockLimit;

    /// <summary>
    /// 빠른 재입고 완료 이벤트
    /// </summary>
    public event Action<PurchasableData, float> OnRestocked;


    /// <summary>
    /// 빠른 재입고 모델 생성
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="businessDayModel">영업일 모델</param>
    /// <param name="employeeModel">직원 효과 모델</param>
    /// <param name="feeRate">빠른 재입고 이용료 배율</param>
    public QuickRestockModel ( ShopModel shopModel,
        PlayStateModel playStateModel,
        BusinessDayModel businessDayModel, EmployeeModel employeeModel,
        float feeRate )
    {
        _shopModel = shopModel;
        _playStateModel = playStateModel;
        _businessDayModel = businessDayModel;
        _employeeModel = employeeModel;

        if ( float.IsNaN( feeRate ) || float.IsInfinity( feeRate ) || feeRate <= 0f )
            throw new ArgumentOutOfRangeException( nameof( feeRate ) );

        _feeRate = feeRate;
    }

    /// <summary>
    /// 빠른 재입고 가능 여부와 이용료 확인
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    /// <param name="fee">빠른 재입고 이용료</param>
    /// <returns>빠른 재입고 처리 결과</returns>
    public QuickRestockResult CheckRestock ( string itemId, out float fee )
    {
        return CheckRestock( itemId, out _, out fee );
    }

    /// <summary>
    /// 선택 상품 빠른 재입고
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    /// <param name="fee">지불한 빠른 재입고 이용료</param>
    /// <returns>빠른 재입고 처리 결과</returns>
    public QuickRestockResult Restock ( string itemId, out float fee )
    {
        QuickRestockResult result = CheckRestock(
            itemId, out ShopItemModel itemModel, out fee );

        //전체 사전 검증 실패 시 상태를 변경하지 않음
        if ( result != QuickRestockResult.Success ) return result;

        //빠른 재입고 이용료 차감
        if ( _playStateModel.SpendBudget( fee ) == false )
            return QuickRestockResult.BudgetUpdateFailed;

        //선택 상품을 현재 최대 재고까지 충전
        itemModel.Restock( );

        //오늘 빠른 재입고 횟수 증가
        _businessDayModel.AddQuickRestock( );

        //재입고 상품과 지불한 이용료 전달
        OnRestocked?.Invoke( itemModel.Item.Data, fee );

        return QuickRestockResult.Success;
    }

    /// <summary>
    /// 빠른 재입고 대상과 이용료 확인
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    /// <param name="itemModel">조회한 상품 모델</param>
    /// <param name="fee">빠른 재입고 이용료</param>
    /// <returns>빠른 재입고 처리 결과</returns>
    QuickRestockResult CheckRestock ( string itemId,
        out ShopItemModel itemModel, out float fee )
    {
        itemModel = null;
        fee = 0f;

        //선택 상품 조회
        if ( _shopModel.TryGetItem( itemId, out itemModel ) == false )
            return QuickRestockResult.InvalidItem;

        //영구 소유하는 정비 상품은 빠른 재입고 대상에서 제외
        if ( itemModel.Item.Data is MaintenanceData )
            return QuickRestockResult.InvalidItem;

        //잠긴 상품 차단
        if ( _playStateModel.IsUnlocked( itemId ) == false )
            return QuickRestockResult.LockedItem;

        ShopItem item = itemModel.Item;

        //이미 최대 재고인 상품 차단
        if ( item.RemainingStock >= item.MaxStock )
            return QuickRestockResult.FullStock;

        //오늘 빠른 재입고 최대 횟수 확인
        if ( _businessDayModel.CanQuickRestock == false )
            return QuickRestockResult.DailyLimitReached;

        //상품 기본 단일 가격으로 이용료 계산
        float employeeRate = _employeeModel.GetEffectValue(
            EmployeeEffectType.QuickRestockCostRate );
        fee = item.Data.BasePrice * _feeRate * employeeRate;

        //잘못된 이용료 차단
        if ( float.IsNaN( fee ) ||
            float.IsInfinity( fee ) || fee <= 0f )
            return QuickRestockResult.InvalidFee;

        //현재 자금 확인
        if ( _playStateModel.CanSpendBudget( fee ) == false )
            return QuickRestockResult.InsufficientBudget;

        return QuickRestockResult.Success;
    }
}
