using System.Collections.Generic;

/// <summary>
/// 파산 판정 결과
/// </summary>
public enum BankruptcyResult
{
    NoProductionOrder,       //진행 중인 제작 주문 없음
    CanCraftWithInventory,       //현재 인벤토리로 제작 가능
    CanCraftAfterPurchase,       //일반 구매 후 제작 가능
    CanCraftAfterQuickRestock,       //빠른 재입고와 구매 후 제작 가능
    Bankrupt,       //어떤 방법으로도 제작 불가
}

/// <summary>
/// 파산 모델 - 진행 주문의 최소 조립 가능성 계산
/// </summary>
public class BankruptcyModel
{
    /// <summary>
    /// 파츠 확보 방법
    /// </summary>
    enum PartSupplyType
    {
        Inventory,       //인벤토리 사용
        Purchase,       //상점 구매
        QuickRestock,       //빠른 재입고 후 구매
    }

    /// <summary>
    /// 최소 조립 파츠 후보(계산용)
    /// </summary>
    class MinimumPartOption
    {
        /// <summary>
        /// 파츠 데이터
        /// </summary>
        public PartsData Part { get; set; }

        /// <summary>
        /// 파츠 확보 방법
        /// </summary>
        public PartSupplyType SupplyType { get; set; }

        /// <summary>
        /// 파츠 확보에 필요한 자금
        /// </summary>
        public float RequiredBudget { get; set; }

        /// <summary>
        /// 필요한 빠른 재입고 횟수
        /// </summary>
        public int QuickRestockCount { get; set; }
    }

    static readonly PartType [ ] MinimumPartTypes =
    {
        PartType.Body,
        PartType.Eye,
        PartType.Nose,
        PartType.Mouth
    };

    CustomerOrderModel _orderModel;       //주문 모델
    InventoryModel _inventoryModel;       //인벤토리 모델
    ShopModel _shopModel;       //상점 모델
    QuickRestockModel _quickRestockModel;       //빠른 재입고 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델

    /// <summary>
    /// 현재 자금
    /// </summary>
    public float CurrentBudget => _playStateModel.Budget;

    /// <summary>
    /// 오늘 남은 빠른 재입고 횟수
    /// </summary>
    public int RemainingQuickRestockCount =>
        _quickRestockModel.DailyLimit - _quickRestockModel.TodayCount;

    /// <summary>
    /// 파산 모델 생성
    /// </summary>
    /// <param name="orderModel">주문 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="quickRestockModel">빠른 재입고 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    public BankruptcyModel (
        CustomerOrderModel orderModel, InventoryModel inventoryModel, ShopModel shopModel,
        QuickRestockModel quickRestockModel, PlayStateModel playStateModel )
    {
        _orderModel = orderModel;
        _inventoryModel = inventoryModel;
        _shopModel = shopModel;
        _quickRestockModel = quickRestockModel;
        _playStateModel = playStateModel;
    }

    /// <summary>
    /// 현재 진행 주문 기준 파산 여부 확인
    /// </summary>
    /// <returns>파산 판정 결과</returns>
    public BankruptcyResult CheckBankruptcy ()
    {
        //진행 중 주문 조회
        IReadOnlyList<CustomerOrder> orders =
            _orderModel.GetOrders( OrderProgressState.Production );

        //진행 중 주문이 없으면 파산으로 판정하지 않음
        if ( orders.Count == 0 )
            return BankruptcyResult.NoProductionOrder;

        //제작 가능한 주문이 하나라도 있는지 확인
        for ( int i = 0; i < orders.Count; i++ )
        {
            BankruptcyResult result = CheckOrder( orders [ i ] );

            if ( result != BankruptcyResult.Bankrupt )
                return result;
        }

        return BankruptcyResult.Bankrupt;
    }

    /// <summary>
    /// 주문의 최소 조립 가능성 확인
    /// </summary>
    /// <param name="order">확인할 주문</param>
    /// <returns>주문 기준 파산 판정 결과</returns>
    BankruptcyResult CheckOrder ( CustomerOrder order )
    {
        //최소 조립 파츠 4개조차 사용할 수 없는 주문 제외
        if ( order.MaxPartCount > 0 &&
            order.MaxPartCount < MinimumPartTypes.Length )
            return BankruptcyResult.Bankrupt;

        //몸통, 눈, 코, 입 타입별 후보 생성
        var optionGroups = new List<List<MinimumPartOption>>( );

        for ( int i = 0; i < MinimumPartTypes.Length; i++ )
        {
            List<MinimumPartOption> options =
                CreateMinimumPartOptions( order, MinimumPartTypes [ i ] );

            //필수 타입 후보가 하나라도 없으면 제작 불가
            if ( options.Count == 0 )
                return BankruptcyResult.Bankrupt;

            optionGroups.Add( options );
        }

        //현재 인벤토리만으로 가능한지 확인
        if ( CanCreateMinimumAssembly(
            optionGroups, order.MaxCraftCost, PartSupplyType.Inventory ) )
            return BankruptcyResult.CanCraftWithInventory;

        //현재 상점 재고 구매까지 포함해 확인
        if ( CanCreateMinimumAssembly(
            optionGroups, order.MaxCraftCost, PartSupplyType.Purchase ) )
            return BankruptcyResult.CanCraftAfterPurchase;

        //빠른 재입고와 구매까지 포함해 확인
        if ( CanCreateMinimumAssembly(
            optionGroups, order.MaxCraftCost, PartSupplyType.QuickRestock ) )
            return BankruptcyResult.CanCraftAfterQuickRestock;

        return BankruptcyResult.Bankrupt;
    }

    /// <summary>
    /// 필수 파츠 타입의 확보 후보 생성
    /// </summary>
    /// <param name="order">확인할 주문</param>
    /// <param name="partType">필수 파츠 타입</param>
    /// <returns>사용 가능한 파츠 후보</returns>
    List<MinimumPartOption> CreateMinimumPartOptions (
        CustomerOrder order, PartType partType )
    {
        var options = new List<MinimumPartOption>( );

        foreach ( ShopItemModel itemModel in _shopModel.Items )
        {
            //해당 타입의 파츠 상품만 확인
            if ( itemModel.Item.Data is not PartsData part ||
                part.PartType != partType )
                continue;

            //제외 테마가 포함된 파츠는 후보에서 제외
            if ( HasExcludedTheme( part, order.ExcludedThemes ) )
                continue;

            //현재 상태에서 확보 가능한 파츠만 추가
            if ( TryCreateMinimumPartOption(
                itemModel, part, out MinimumPartOption option ) )
                options.Add( option );
        }

        return options;
    }

    /// <summary>
    /// 파츠 확보 방법과 필요 비용 생성
    /// </summary>
    /// <param name="itemModel">파츠 상품 모델</param>
    /// <param name="part">파츠 데이터</param>
    /// <param name="option">생성한 파츠 후보</param>
    /// <returns>파츠 확보 가능 여부</returns>
    bool TryCreateMinimumPartOption (
        ShopItemModel itemModel, PartsData part,
        out MinimumPartOption option )
    {
        option = null;

        //현재 인벤토리 파츠 사용
        if ( _inventoryModel.GetQuantity( part.Id ) > 0 )
        {
            option = new MinimumPartOption
            {
                Part = part,
                SupplyType = PartSupplyType.Inventory,
                RequiredBudget = 0f,
                QuickRestockCount = 0
            };

            return true;
        }

        //잠긴 상품은 구매와 빠른 재입고 대상에서 제외
        if ( _playStateModel.IsUnlocked( part.Id ) == false )
            return false;

        ShopItem item = itemModel.Item;

        //현재 상점 재고 구매
        if ( item.RemainingStock > 0 )
        {
            option = new MinimumPartOption
            {
                Part = part,
                SupplyType = PartSupplyType.Purchase,
                RequiredBudget = item.CurrentPrice,
                QuickRestockCount = 0
            };

            return true;
        }

        //품절 상품의 빠른 재입고 가능 여부와 이용료 확인
        if ( _quickRestockModel.CheckRestock(
            part.Id, out float fee ) != QuickRestockResult.Success )
            return false;

        option = new MinimumPartOption
        {
            Part = part,
            SupplyType = PartSupplyType.QuickRestock,
            RequiredBudget = fee + item.CurrentPrice,
            QuickRestockCount = 1
        };

        return true;
    }

    /// <summary>
    /// 허용한 확보 방법으로 최소 조립 가능 여부 확인
    /// </summary>
    /// <param name="optionGroups">필수 타입별 파츠 후보</param>
    /// <param name="maxCraftCost">최대 제작 코스트</param>
    /// <param name="maximumSupplyType">허용할 최대 확보 방법</param>
    /// <returns>최소 조립 가능 여부</returns>
    bool CanCreateMinimumAssembly (
        List<List<MinimumPartOption>> optionGroups,
        int maxCraftCost, PartSupplyType maximumSupplyType )
    {
        int remainingRestockCount =
            _quickRestockModel.DailyLimit -
            _quickRestockModel.TodayCount;

        return FindMinimumAssembly(
            optionGroups, maxCraftCost,
            maximumSupplyType, remainingRestockCount,
            0, 0, 0f, 0 );
    }

    /// <summary>
    /// 필수 타입별 파츠 조합 탐색
    /// </summary>
    /// <param name="optionGroups">필수 타입별 파츠 후보</param>
    /// <param name="maxCraftCost">최대 제작 코스트</param>
    /// <param name="maximumSupplyType">허용할 최대 확보 방법</param>
    /// <param name="remainingRestockCount">남은 빠른 재입고 횟수</param>
    /// <param name="groupIndex">현재 확인할 타입 순서</param>
    /// <param name="craftCost">현재 조합 제작 코스트</param>
    /// <param name="requiredBudget">현재 조합 필요 자금</param>
    /// <param name="restockCount">현재 조합 빠른 재입고 횟수</param>
    /// <returns>조건을 만족하는 조합 존재 여부</returns>
    bool FindMinimumAssembly (
        List<List<MinimumPartOption>> optionGroups,
        int maxCraftCost, PartSupplyType maximumSupplyType,
        int remainingRestockCount, int groupIndex,
        int craftCost, float requiredBudget, int restockCount )
    {
        //몸통, 눈, 코, 입 후보 선택 완료
        if ( groupIndex >= optionGroups.Count )
            return true;

        List<MinimumPartOption> options = optionGroups [ groupIndex ];

        for ( int i = 0; i < options.Count; i++ )
        {
            MinimumPartOption option = options [ i ];

            //현재 단계에서 허용하지 않는 확보 방법 제외
            if ( option.SupplyType > maximumSupplyType )
                continue;

            int nextCraftCost = craftCost + option.Part.CraftCost;
            float nextBudget = requiredBudget + option.RequiredBudget;
            int nextRestockCount = restockCount + option.QuickRestockCount;

            //주문 최대 제작 코스트 초과 조합 제외
            if ( nextCraftCost > maxCraftCost )
                continue;

            //현재 자금으로 확보할 수 없는 조합 제외
            if ( nextBudget > _playStateModel.Budget )
                continue;

            //남은 빠른 재입고 횟수를 초과하는 조합 제외
            if ( nextRestockCount > remainingRestockCount )
                continue;

            //다음 필수 파츠 타입 탐색
            if ( FindMinimumAssembly(
                optionGroups, maxCraftCost,
                maximumSupplyType, remainingRestockCount,
                groupIndex + 1, nextCraftCost,
                nextBudget, nextRestockCount ) )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 파츠의 제외 테마 포함 여부 확인
    /// </summary>
    /// <param name="part">확인할 파츠</param>
    /// <param name="excludedThemes">제외 테마 목록</param>
    /// <returns>제외 테마 포함 여부</returns>
    bool HasExcludedTheme (
        PartsData part, IReadOnlyList<PartTheme> excludedThemes )
    {
        for ( int i = 0; i < part.Themes.Count; i++ )
        {
            for ( int j = 0; j < excludedThemes.Count; j++ )
            {
                if ( part.Themes [ i ] == excludedThemes [ j ] )
                    return true;
            }
        }

        return false;
    }
}
