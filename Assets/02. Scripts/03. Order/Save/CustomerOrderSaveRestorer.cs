using System;
using System.Collections.Generic;

/// <summary>
/// 고객 주문 저장 복구 처리 - 저장 데이터 검증과 고객 주문 복구 상태 생성
/// </summary>
public class CustomerOrderSaveRestorer
{
    /// <summary>
    /// 고객 주문 저장 데이터를 검증하고 복구 상태 생성
    /// </summary>
    /// <param name="saveData">복구할 고객 주문 저장 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="restoreState">생성한 고객 주문 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        OrderSaveData saveData,
        PurchasableDataMap dataMap,
        out CustomerOrderRestoreState restoreState )
    {
        restoreState = null;

        //데이터 확인
        if ( saveData == null || dataMap == null ||
            saveData.WaitingLimit < 1 ||
            saveData.WaitingLimit > CustomerOrderModel.MaxWaitingLimit ||
            saveData.NextCreatedNumber < 1 ||
            saveData.Orders == null ||
            IsValidPenaltyData( saveData ) == false )
        {
            return false;
        }

        var orderIds = new HashSet<string>( );
        var createdNumbers = new HashSet<int>( );
        var orders = new List<CustomerOrder>( saveData.Orders.Count );

        int waitingCount = 0;
        int maxCreatedNumber = 0;

        foreach ( CustomerOrderSaveData orderData in saveData.Orders )
        {
            //주문 데이터 검증과 아이디, 생성 번호 중복 확인
            if ( IsValidOrderData( orderData, dataMap ) == false ||
                orderIds.Add( orderData.OrderId ) == false ||
                createdNumbers.Add( orderData.CreatedNumber ) == false )
            {
                return false;
            }

            //수락 대기 주문 수 집계
            if ( orderData.ProgressState == OrderProgressState.Waiting )
                waitingCount++;

            //가장 큰 주문 생성 번호 기록
            maxCreatedNumber = Math.Max(
                maxCreatedNumber, orderData.CreatedNumber );

            //검증을 마친 주문 생성
            orders.Add( CreateOrder( orderData ) );
        }

        //수락 대기 제한과 다음 생성 번호 확인
        if ( waitingCount > saveData.WaitingLimit ||
            saveData.NextCreatedNumber <= maxCreatedNumber )
        {
            return false;
        }

        //검증을 마친 고객 주문 복구 상태 생성
        restoreState = new CustomerOrderRestoreState(
            orders,
            saveData.WaitingLimit,
            saveData.NextCreatedNumber,
            saveData.PenaltyType,
            saveData.OrderReduction,
            saveData.PenaltyRemainingDays );

        return true;
    }

    /// <summary>
    /// 저장 데이터로 고객 주문 생성
    /// </summary>
    /// <param name="saveData">고객 주문 저장 데이터</param>
    /// <returns>생성한 고객 주문</returns>
    CustomerOrder CreateOrder ( CustomerOrderSaveData saveData )
    {
        //저장 데이터로 주문 생성 정보 구성
        var createData = new OrderCreateData
        {
            CreatedNumber = saveData.CreatedNumber,
            Id = saveData.OrderId,
            Title = saveData.Title,
            MaxCraftCost = saveData.MaxCraftCost,

            Requirements = CreateConditions( saveData.Requirements ),
            Wishes = CreateConditions( saveData.Wishes ),

            SpecialType = saveData.SpecialType,
            MaxPartCount = saveData.MaxPartCount,
            TargetThemes = new List<PartTheme>( saveData.TargetThemes ),
            ExcludedThemes = new List<PartTheme>( saveData.ExcludedThemes ),

            CreatedTotalDay = saveData.CreatedTotalDay,
            AcceptDue = saveData.AcceptDue,
            DeliveryDue = saveData.DeliveryDue,
            MaxDelay = saveData.MaxDelay,

            Difficulty = saveData.Difficulty
        };

        var order = new CustomerOrder( createData );

        //저장된 진행 상태와 종료 결과 복구
        order.RestoreState(
            saveData.ProgressState, saveData.Outcome, saveData.ClosedTotalDay );

        return order;
    }

    /// <summary>
    /// 저장 데이터로 주문 파츠 조건 목록 생성
    /// </summary>
    /// <param name="saveDatas">조건 저장 데이터 목록</param>
    /// <returns>생성한 주문 파츠 조건 목록</returns>
    List<OrderPartCondition> CreateConditions (
        IReadOnlyList<OrderPartConditionSaveData> saveDatas )
    {
        var conditions =
            new List<OrderPartCondition>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            OrderPartConditionSaveData saveData = saveDatas [ i ];

            //주문 파츠 조건 생성
            conditions.Add( new OrderPartCondition(
                saveData.PartId, saveData.Quantity ) );
        }

        return conditions;
    }

    /// <summary>
    /// 주문량 페널티 저장 상태 확인
    /// </summary>
    /// <param name="saveData">고객 주문 모델 저장 데이터</param>
    /// <returns>유효 여부</returns>
    bool IsValidPenaltyData ( OrderSaveData saveData )
    {
        if ( Enum.IsDefined(
            typeof( OrderPenaltyType ), saveData.PenaltyType ) == false )
        {
            return false;
        }

        //페널티가 없으면 관련 수치도 없어야 함
        if ( saveData.PenaltyType == OrderPenaltyType.None )
        {
            return saveData.OrderReduction == 0 &&
                saveData.PenaltyRemainingDays == 0;
        }

        return saveData.OrderReduction > 0 &&
            saveData.PenaltyRemainingDays > 0 &&
            saveData.PenaltyRemainingDays <=
            CustomerOrderModel.MaxPenaltyDays;
    }

    /// <summary>
    /// 고객 주문 저장 상태 확인
    /// </summary>
    /// <param name="saveData">검증할 고객 주문 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <returns>유효 여부</returns>
    bool IsValidOrderData (
        CustomerOrderSaveData saveData,
        PurchasableDataMap dataMap )
    {
        //기본 주문 데이터 확인
        if ( saveData == null ||
            saveData.CreatedNumber < 1 ||
            string.IsNullOrWhiteSpace( saveData.OrderId ) == true ||
            string.IsNullOrWhiteSpace( saveData.Title ) == true ||
            saveData.MaxCraftCost <= 0 ||
            saveData.Requirements == null ||
            saveData.Wishes == null ||
            saveData.TargetThemes == null ||
            saveData.ExcludedThemes == null ||
            saveData.MaxPartCount < 0 ||
            saveData.CreatedTotalDay < 1 ||
            saveData.AcceptDue < saveData.CreatedTotalDay ||
            saveData.DeliveryDue < saveData.AcceptDue ||
            saveData.MaxDelay < 0 )
        {
            return false;
        }

        //주문 분류와 진행 상태 확인
        if ( Enum.IsDefined( typeof( OrderSpecialType ),
                saveData.SpecialType ) == false ||
            Enum.IsDefined( typeof( OrderDifficulty ),
                saveData.Difficulty ) == false ||
            Enum.IsDefined( typeof( OrderProgressState ),
                saveData.ProgressState ) == false ||
            Enum.IsDefined( typeof( OrderOutcome ),
                saveData.Outcome ) == false )
        {
            return false;
        }

        //주문 조건과 테마 목록 확인
        if ( IsValidConditions(
                saveData.Requirements, dataMap ) == false ||
            IsValidConditions(
                saveData.Wishes, dataMap ) == false ||
            IsValidThemes( saveData.TargetThemes ) == false ||
            IsValidThemes( saveData.ExcludedThemes ) == false )
        {
            return false;
        }

        bool isClosed =
            saveData.ProgressState == OrderProgressState.Closed;

        //종료 주문은 결과와 종료일이 있어야 함
        if ( isClosed == true )
        {
            return saveData.Outcome != OrderOutcome.None &&
                saveData.ClosedTotalDay >= saveData.CreatedTotalDay;
        }

        //진행 중인 주문은 종료 결과가 없어야 함
        return saveData.Outcome == OrderOutcome.None &&
            saveData.ClosedTotalDay == 0;
    }

    /// <summary>
    /// 주문 파츠 조건 목록 확인
    /// </summary>
    /// <param name="saveDatas">검증할 조건 목록</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <returns>유효 여부</returns>
    bool IsValidConditions (
        IReadOnlyList<OrderPartConditionSaveData> saveDatas,
        PurchasableDataMap dataMap )
    {
        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            OrderPartConditionSaveData saveData = saveDatas [ i ];

            //조건의 파츠 데이터와 수량 확인
            if ( saveData == null ||
                string.IsNullOrWhiteSpace( saveData.PartId ) == true ||
                saveData.Quantity <= 0 ||
                dataMap.TryGetData(
                    saveData.PartId, out PurchasableData data ) == false ||
                data is PartsData == false )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 주문 테마 목록 확인
    /// </summary>
    /// <param name="themes">검증할 테마 목록</param>
    /// <returns>유효 여부</returns>
    bool IsValidThemes ( IReadOnlyList<PartTheme> themes )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            //정의되지 않았거나 비어 있는 테마 차단
            if ( Enum.IsDefined(
                typeof( PartTheme ),
                themes [ i ] ) == false ||
                themes [ i ] == PartTheme.None )
            {
                return false;
            }
        }

        return true;
    }
}
