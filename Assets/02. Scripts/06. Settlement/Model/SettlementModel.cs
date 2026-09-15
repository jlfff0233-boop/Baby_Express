using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 결산 모델 - 일일 기록과 현재 주문 상태 집계
/// </summary>
public class SettlementModel
{
    WeeklySettlementSettingData _weeklySettings;       //주간 결산 평가 설정
    List<WeeklySettlementData> _weeklySettlements =
        new List<WeeklySettlementData>( );       //완료된 주간 결산 목록
    bool _hasNotification;       //새 주간 결산 알림 여부

    /// <summary>
    /// 완료된 주간 결산 목록
    /// </summary>
    public IReadOnlyList<WeeklySettlementData> WeeklySettlements =>
        _weeklySettlements;

    /// <summary>
    /// 새 주간 결산 알림 여부
    /// </summary>
    public bool HasNotification => _hasNotification;

    /// <summary>
    /// 새 주간 결산 알림 상태 변경 이벤트
    /// </summary>
    public event Action<bool> OnNotificationChanged;

    /// <summary>
    /// 결산 모델 생성
    /// </summary>
    /// <param name="weeklySettings">주간 결산 평가 설정</param>
    public SettlementModel ( WeeklySettlementSettingData weeklySettings )
    {
        _weeklySettings = weeklySettings;
    }

    /// <summary>
    /// 일일 결산 데이터 생성
    /// </summary>
    /// <param name="record">확정된 일일 기록</param>
    /// <param name="currentOrders">현재 전체 주문 목록</param>
    /// <returns>일일 결산 계산 결과</returns>
    public DailySettlementData CreateDaily (
        DailyRecord record, IReadOnlyCollection<CustomerOrder> currentOrders )
    {
        return new DailySettlementData
        {
            TotalDay = record.TotalDay,
            EndReason = record.EndReason,
            StartBudget = record.StartBudget,
            EndBudget = record.EndBudget,

            //일일 운영 기록 생성
            Operation = CreateOperationSummary( record, currentOrders ),
            //주문 결과 기록 생성
            Orders = CreateOrderSummary( record.OrderRecords ),
            //주문 평가 기록 생성
            Evaluation = CreateEvaluationSummary( record.OrderRecords ),
            //수익, 지출 기록 생성
            Economy = CreateEconomySummary( record ),

            //상품별 상세 기록 생성
            PurchaseDetails = CreateItemDetails( record.PurchaseRecords ),
            //파츠별 제작 소비 상세 기록 생성
            UsedPartDetails = CreateUsedPartDetails( record.CraftRecords ),
            //상품별 빠른 재입고 상세 기록 생성
            QuickRestockDetails = CreateQuickRestockDetails( record.QuickRestockRecords )
        };
    }

    #region ----- 주간 결산 -----
    /// <summary>
    /// 주간 결산 데이터 생성
    /// </summary>
    /// <param name="records">완료된 전체 일일 기록</param>
    /// <param name="currentOrders">현재 전체 주문 목록</param>
    /// <param name="endTotalDay">주간 결산 종료 누적 영업일</param>
    /// <param name="hiredEmployeeCount">결산 당시 고용 직원 수</param>
    /// <returns>주간 결산 계산 결과</returns>
    public WeeklySettlementData CreateWeekly (
        IReadOnlyList<DailyRecord> records,
        IReadOnlyCollection<CustomerOrder> currentOrders,
        int endTotalDay, int hiredEmployeeCount )
    {
        int startTotalDay = endTotalDay - 6;
        List<DailyRecord> weeklyRecords =
            GetWeeklyRecords( records, startTotalDay, endTotalDay );

        var orderRecords = new List<DailyOrderRecord>( );
        var purchaseRecords = new List<DailyItemRecord>( );
        var craftRecords = new List<DailyCraftRecord>( );
        var quickRestockRecords = new List<DailyQuickRestockRecord>( );

        //주간 계산에 사용할 일일 원본 기록 분류
        for ( int i = 0; i < weeklyRecords.Count; i++ )
        {
            DailyRecord record = weeklyRecords [ i ];

            orderRecords.AddRange( record.OrderRecords );
            purchaseRecords.AddRange( record.PurchaseRecords );
            craftRecords.AddRange( record.CraftRecords );
            quickRestockRecords.AddRange( record.QuickRestockRecords );
        }

        float weeklyScore = CalculateWeeklyScore( orderRecords );
        bool hasEnoughData = orderRecords.Count >= _weeklySettings.MinimumOrderCount;

        WeeklyRatingSetting ratingSetting = hasEnoughData
            ? GetRatingSetting( weeklyScore )
            : null;
        bool canApplyRating = hasEnoughData && ratingSetting != null;

        WeeklySettlementData weeklySettlement = new WeeklySettlementData
        {
            Week = ( endTotalDay - 1 ) / 7 + 1,
            StartTotalDay = startTotalDay,
            EndTotalDay = endTotalDay,

            StartBudget = weeklyRecords [ 0 ].StartBudget,
            EndBudget = weeklyRecords [ weeklyRecords.Count - 1 ].EndBudget,

            //결산 당시 고용 직원 수 저장
            HiredEmployeeCount = hiredEmployeeCount,

            //주간 운영 결과 생성
            Operation = CreateWeeklyOperation( weeklyRecords, currentOrders ),
            //주간 주문 결과 생성
            Orders = CreateOrderSummary( orderRecords ),
            //주간 주문 평가 생성
            Evaluation = CreateEvaluationSummary( orderRecords ),
            //주간 경제 결과 생성
            Economy = CreateWeeklyEconomy( weeklyRecords ),

            EvaluationOrderCount = orderRecords.Count,
            HasEnoughData = hasEnoughData,
            WeeklyScore = weeklyScore,
            Rating = canApplyRating
                ? ratingSetting.Rating
                : WeeklyRating.Insufficient,
            NextWeekAdjustment = canApplyRating
                ? CreateWeeklyAdjustment( ratingSetting )
                : new WeeklyOrderAdjustment( ),

            //주간 상품 상세 생성
            PurchaseDetails = CreateItemDetails( purchaseRecords ),
            UsedPartDetails = CreateUsedPartDetails( craftRecords ),
            QuickRestockDetails =
                CreateQuickRestockDetails( quickRestockRecords )
        };

        //과거 결산 조회를 위해 결과 보관
        _weeklySettlements.Add( weeklySettlement );

        //새 가계부 기록 알림 전달
        _hasNotification = true;
        OnNotificationChanged?.Invoke( true );

        return weeklySettlement;
    }

    /// <summary>
    /// 새 주간 결산 확인 처리
    /// </summary>
    public void MarkNotificationViewed ()
    {
        if ( _hasNotification == false ) return;

        _hasNotification = false;
        OnNotificationChanged?.Invoke( false );
    }

    /// <summary>
    /// 주간 결산 대상 일일 기록 조회
    /// </summary>
    /// <param name="records">완료된 전체 일일 기록</param>
    /// <param name="startTotalDay">주간 시작 누적 영업일</param>
    /// <param name="endTotalDay">주간 종료 누적 영업일</param>
    /// <returns>주간 결산 대상 일일 기록</returns>
    List<DailyRecord> GetWeeklyRecords (
        IReadOnlyList<DailyRecord> records, int startTotalDay, int endTotalDay )
    {
        var weeklyRecords = new List<DailyRecord>( );

        for ( int i = 0; i < records.Count; i++ )
        {
            DailyRecord record = records [ i ];

            //현재 주간 범위에 포함된 기록만 추가
            if ( record.TotalDay >= startTotalDay &&
                record.TotalDay <= endTotalDay )
            {
                weeklyRecords.Add( record );
            }
        }

        return weeklyRecords;
    }

    /// <summary>
    /// 주간 운영 현황 생성
    /// </summary>
    /// <param name="records">주간 일일 기록</param>
    /// <param name="currentOrders">현재 전체 주문 목록</param>
    /// <returns>주간 운영 현황</returns>
    OperationSettlement CreateWeeklyOperation (
        IReadOnlyList<DailyRecord> records,
        IReadOnlyCollection<CustomerOrder> currentOrders )
    {
        var operation = new OperationSettlement( );

        //일일 운영 기록 합산
        for ( int i = 0; i < records.Count; i++ )
        {
            DailyRecord record = records [ i ];

            operation.CraftCompletedCount += record.CraftRecords.Count;     //제작 완료 횟수
            operation.CraftLimit += record.CraftLimit;      //일일 제작 할당량
            operation.DeliveryStartedCount += record.DeliveryStartedOrderIds.Count;     //배송 출발 수
            operation.QuickRestockCount += record.QuickRestockRecords.Count;        //빠른 재입고 횟수
            operation.QuickRestockLimit += record.QuickRestockLimit;        //빠른 재입고 제한

            //취소, 폐기된 제작물 합산
            for ( int j = 0; j < record.CraftRecords.Count; j++ )
            {
                if ( record.CraftRecords [ j ].IsDiscarded )
                    operation.DiscardedCraftCount++;
            }
        }

        //주말 기준 이월 주문 수 집계
        foreach ( CustomerOrder order in currentOrders )
        {
            if ( order.ProgressState == OrderProgressState.Production )
                operation.ProductionOrderCount++;
            else if ( order.ProgressState == OrderProgressState.Crafted )
                operation.PendingDeliveryCount++;
            else if ( order.ProgressState == OrderProgressState.Shipping )
                operation.InDeliveryCount++;
        }

        return operation;
    }

    /// <summary>
    /// 주간 경제 결과 생성
    /// </summary>
    /// <param name="records">주간 일일 기록</param>
    /// <returns>주간 경제 결과</returns>
    EconomySettlement CreateWeeklyEconomy (
        IReadOnlyList<DailyRecord> records )
    {
        var weeklyEconomy = new EconomySettlement( );

        //일일 경제 결과를 주간 결과로 합산
        for ( int i = 0; i < records.Count; i++ )
        {
            EconomySettlement dailyEconomy =
                CreateEconomySummary( records [ i ] );

            weeklyEconomy.OrderRewardIncome +=
                dailyEconomy.OrderRewardIncome;      //주문 수익
            weeklyEconomy.InventorySaleIncome +=
                dailyEconomy.InventorySaleIncome;      //인벤토리 판매 수익
            weeklyEconomy.OtherIncome +=
                dailyEconomy.OtherIncome;      //기타 수익

            weeklyEconomy.PurchaseExpense +=
                dailyEconomy.PurchaseExpense;       //구매 지출
            weeklyEconomy.QuickRestockExpense +=
                dailyEconomy.QuickRestockExpense;       //빠른 재입고 지출
            weeklyEconomy.DeliveryExpense +=
                dailyEconomy.DeliveryExpense;       //배송비
            weeklyEconomy.EmployeeHireExpense +=
                dailyEconomy.EmployeeHireExpense;       //고용비
            weeklyEconomy.EmployeeWeeklyExpense +=
                dailyEconomy.EmployeeWeeklyExpense;     //주급
            weeklyEconomy.OtherExpense +=
                dailyEconomy.OtherExpense;      //기타 지출
        }

        return weeklyEconomy;
    }

    /// <summary>
    /// 주간 평가 점수 계산
    /// </summary>
    /// <param name="records">주간 종료 주문 기록</param>
    /// <returns>주간 평가 평균 점수</returns>
    float CalculateWeeklyScore (
        IReadOnlyList<DailyOrderRecord> records )
    {
        if ( records.Count == 0 ) return 0f;

        float totalScore = 0f;

        for ( int i = 0; i < records.Count; i++ )
            totalScore += GetOrderScore( records [ i ] );

        return totalScore / records.Count;
    }

    /// <summary>
    /// 주문 종료 결과 점수 반환
    /// </summary>
    /// <param name="record">종료 주문 기록</param>
    /// <returns>주문 종료 결과 점수</returns>
    float GetOrderScore ( DailyOrderRecord record )
    {
        switch ( record.Outcome )
        {
            case OrderOutcome.NormalDelivery:
                return _weeklySettings.GetGradeScore( record.Grade );

            case OrderOutcome.LateDelivery:
                return _weeklySettings.GetGradeScore( record.Grade ) *
                    _weeklySettings.LateDeliveryRate;

            case OrderOutcome.Rejected:
                return _weeklySettings.RejectedScore;

            case OrderOutcome.AutoRejected:
                return _weeklySettings.AutoRejectedScore;

            case OrderOutcome.Cancelled:
                return _weeklySettings.CancelledScore;

            case OrderOutcome.Failed:
                return _weeklySettings.FailedScore;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// 주간 점수에 맞는 평가 설정 조회
    /// </summary>
    /// <param name="weeklyScore">주간 평가 점수</param>
    /// <returns>주간 평가 설정</returns>
    WeeklyRatingSetting GetRatingSetting ( float weeklyScore )
    {
        IReadOnlyList<WeeklyRatingSetting> settings =
            _weeklySettings.RatingSettings;

        //평가 설정이 없으면 주간 보정을 적용하지 않음
        if ( settings == null || settings.Count == 0 )
        {
            Debug.LogError( "주간 결산 평가 설정에 평가별 주문 생성 설정이 없습니다." );
            return null;
        }

        //Inspector에 등록된 최소 점수가 높은 순서로 확인
        for ( int i = 0; i < settings.Count; i++ )
        {
            if ( weeklyScore >= settings [ i ].MinimumScore )
                return settings [ i ];
        }

        return settings [ settings.Count - 1 ];
    }

    /// <summary>
    /// 다음 주 주문 생성 보정 생성
    /// </summary>
    /// <param name="setting">주간 평가 설정</param>
    /// <returns>다음 주 주문 생성 보정</returns>
    WeeklyOrderAdjustment CreateWeeklyAdjustment (
        WeeklyRatingSetting setting )
    {
        return new WeeklyOrderAdjustment
        {
            IsApplied = true,
            OrderCountCorrection = setting.OrderCountCorrection,
            EasyWeight = setting.EasyWeight,
            NormalWeight = setting.NormalWeight,
            HardWeight = setting.HardWeight
        };
    }
    #endregion

    #region ----- 운영 현황 -----

    /// <summary>
    /// 일일 운영 현황 생성
    /// </summary>
    /// <param name="record">확정된 일일 기록</param>
    /// <param name="currentOrders">현재 전체 주문 목록</param>
    /// <returns>일일 운영 현황</returns>
    OperationSettlement CreateOperationSummary (
        DailyRecord record, IReadOnlyCollection<CustomerOrder> currentOrders )
    {
        int discardedCraftCount = 0;
        int productionOrderCount = 0;
        int pendingDeliveryCount = 0;
        int inDeliveryCount = 0;

        //취소, 폐기된 제작물 수 집계
        for ( int i = 0; i < record.CraftRecords.Count; i++ )
        {
            if ( record.CraftRecords [ i ].IsDiscarded )
                discardedCraftCount++;
        }

        //현재 제작 진행과 배송 대기, 배송 중 주문 집계
        foreach ( CustomerOrder order in currentOrders )
        {
            if ( order.ProgressState == OrderProgressState.Production )
                productionOrderCount++;
            else if ( order.ProgressState == OrderProgressState.Crafted )
                pendingDeliveryCount++;
            else if ( order.ProgressState == OrderProgressState.Shipping )
                inDeliveryCount++;
        }

        return new OperationSettlement
        {
            CraftCompletedCount = record.CraftRecords.Count,
            CraftLimit = record.CraftLimit,

            DiscardedCraftCount = discardedCraftCount,
            DeliveryStartedCount = record.DeliveryStartedOrderIds.Count,
            ProductionOrderCount = productionOrderCount,
            PendingDeliveryCount = pendingDeliveryCount,
            InDeliveryCount = inDeliveryCount,

            QuickRestockCount = record.QuickRestockRecords.Count,
            QuickRestockLimit = record.QuickRestockLimit
        };
    }

    #endregion

    #region ----- 주문 결과 -----

    /// <summary>
    /// 일일 주문 결과 생성
    /// </summary>
    /// <param name="records">일일 종료 주문 기록</param>
    /// <returns>일일 주문 결과</returns>
    OrderSettlement CreateOrderSummary (
        IReadOnlyList<DailyOrderRecord> records )
    {
        var summary = new OrderSettlement( );

        for ( int i = 0; i < records.Count; i++ )
        {
            switch ( records [ i ].Outcome )
            {
                case OrderOutcome.NormalDelivery:
                    summary.NormalDeliveryCount++;
                    break;

                case OrderOutcome.LateDelivery:
                    summary.LateDeliveryCount++;
                    break;

                case OrderOutcome.Rejected:
                    summary.RejectedCount++;
                    break;

                case OrderOutcome.AutoRejected:
                    summary.AutoRejectedCount++;
                    break;

                case OrderOutcome.Cancelled:
                    summary.CancelledCount++;
                    break;

                case OrderOutcome.Failed:
                    summary.FailedCount++;
                    break;
            }
        }

        return summary;
    }

    #endregion

    #region ----- 주문 평가 -----

    /// <summary>
    /// 일일 주문 평가 생성
    /// </summary>
    /// <param name="records">일일 종료 주문 기록</param>
    /// <returns>일일 주문 평가</returns>
    EvaluationSettlement CreateEvaluationSummary (
        IReadOnlyList<DailyOrderRecord> records )
    {
        var summary = new EvaluationSettlement( );

        for ( int i = 0; i < records.Count; i++ )
        {
            DailyOrderRecord record = records [ i ];

            //배송 평가가 없는 거절, 취소, 실패 제외
            if ( record.HasEvaluation == false ) continue;

            //달성한 요구사항 수 합산
            summary.CompletedRequirementCount += record.CompletedRequirementCount;
            //전체 요구사항 수 합산
            summary.RequirementCount += record.RequirementCount;

            //달성한 희망사항 수 합산
            summary.CompletedWishCount += record.CompletedWishCount;
            //전체 희망사항 수 합산
            summary.WishCount += record.WishCount;

            //배송 평가 등급 수 합산
            AddGradeCount( summary, record.Grade );
        }

        return summary;
    }

    /// <summary>
    /// 배송 평가 등급 수 추가
    /// </summary>
    /// <param name="summary">일일 주문 평가</param>
    /// <param name="grade">추가할 배송 평가 등급</param>
    void AddGradeCount (
        EvaluationSettlement summary, DeliveryGrade grade )
    {
        switch ( grade )
        {
            case DeliveryGrade.S:
                summary.SGradeCount++;
                break;

            case DeliveryGrade.A:
                summary.AGradeCount++;
                break;

            case DeliveryGrade.B:
                summary.BGradeCount++;
                break;

            case DeliveryGrade.C:
                summary.CGradeCount++;
                break;

            case DeliveryGrade.D:
                summary.DGradeCount++;
                break;
        }
    }

    #endregion

    #region ----- 수익/지출 결과 -----

    /// <summary>
    /// 일일 경제 결과 생성
    /// </summary>
    /// <param name="record">확정된 일일 기록</param>
    /// <returns>일일 경제 결과</returns>
    EconomySettlement CreateEconomySummary ( DailyRecord record )
    {
        var summary = new EconomySettlement( );

        //배송 완료 주문 대금 합산
        for ( int i = 0; i < record.OrderRecords.Count; i++ )
            summary.OrderRewardIncome +=
                record.OrderRecords [ i ].OrderReward;

        //인벤토리 판매 수익 합산
        for ( int i = 0;
            i < record.InventorySaleIncomes.Count; i++ )
        {
            summary.InventorySaleIncome += record.InventorySaleIncomes [ i ];
        }

        //기타 수입 합산
        for ( int i = 0; i < record.OtherIncomes.Count; i++ )
            summary.OtherIncome += record.OtherIncomes [ i ];

        //상점 구매 지출 합산
        for ( int i = 0; i < record.PurchaseRecords.Count; i++ )
            summary.PurchaseExpense += record.PurchaseRecords [ i ].PriceTotal;

        //빠른 재입고 이용료 합산
        for ( int i = 0;
            i < record.QuickRestockRecords.Count; i++ )
        {
            summary.QuickRestockExpense += record.QuickRestockRecords [ i ].Fee;
        }

        //직접 배송비 합산
        for ( int i = 0; i < record.DeliveryExpenses.Count; i++ )
            summary.DeliveryExpense += record.DeliveryExpenses [ i ];

        //직원 고용비 합산
        for ( int i = 0; i < record.EmployeeHireExpenses.Count; i++ )
        {
            summary.EmployeeHireExpense +=
                record.EmployeeHireExpenses [ i ];
        }

        //직원 주급 합산
        for ( int i = 0; i < record.EmployeeWageExpenses.Count; i++ )
        {
            summary.EmployeeWeeklyExpense +=
                record.EmployeeWageExpenses [ i ];
        }

        //기타 지출 합산
        for ( int i = 0; i < record.OtherExpenses.Count; i++ )
            summary.OtherExpense += record.OtherExpenses [ i ];

        return summary;
    }

    #endregion

    #region ----- 상품 상세 -----

    /// <summary>
    /// 상품별 구매 상세 생성
    /// </summary>
    /// <param name="records">일일 구매 기록</param>
    /// <returns>상품별 구매 상세</returns>
    List<ItemSettlement> CreateItemDetails (
        IReadOnlyList<DailyItemRecord> records )
    {
        var details = new Dictionary<string, ItemSettlement>( );

        for ( int i = 0; i < records.Count; i++ )
        {
            DailyItemRecord record = records [ i ];

            AddItemDetail(
                details, record.Data, record.Quantity, record.PriceTotal );
        }

        return new List<ItemSettlement>( details.Values );
    }

    /// <summary>
    /// 파츠별 제작 소비 상세 생성
    /// </summary>
    /// <param name="craftRecords">일일 제작 완료 기록</param>
    /// <returns>파츠별 제작 소비 상세</returns>
    List<ItemSettlement> CreateUsedPartDetails (
        IReadOnlyList<DailyCraftRecord> craftRecords )
    {
        var details = new Dictionary<string, ItemSettlement>( );

        //폐기 여부와 관계없이 실제 소비한 파츠 전체 집계
        for ( int i = 0; i < craftRecords.Count; i++ )
        {
            IReadOnlyList<DailyItemRecord> usedParts =
                craftRecords [ i ].UsedParts;

            for ( int j = 0; j < usedParts.Count; j++ )
            {
                DailyItemRecord usedPart = usedParts [ j ];

                AddItemDetail(
                    details, usedPart.Data,
                    usedPart.Quantity, usedPart.PriceTotal );
            }
        }

        return new List<ItemSettlement>( details.Values );
    }

    /// <summary>
    /// 상품별 빠른 재입고 상세 생성
    /// </summary>
    /// <param name="records">일일 빠른 재입고 기록</param>
    /// <returns>상품별 빠른 재입고 상세</returns>
    List<ItemSettlement> CreateQuickRestockDetails (
        IReadOnlyList<DailyQuickRestockRecord> records )
    {
        var details = new Dictionary<string, ItemSettlement>( );

        for ( int i = 0; i < records.Count; i++ )
        {
            DailyQuickRestockRecord record = records [ i ];

            //수량은 해당 상품의 빠른 재입고 이용 횟수
            AddItemDetail( details, record.Data, 1, record.Fee );
        }

        return new List<ItemSettlement>( details.Values );
    }

    /// <summary>
    /// 상품별 수량과 금액 추가
    /// </summary>
    /// <param name="details">상품별 집계 목록</param>
    /// <param name="data">집계할 상품 데이터</param>
    /// <param name="quantity">추가 수량</param>
    /// <param name="price">추가 금액</param>
    void AddItemDetail (
        IDictionary<string, ItemSettlement> details,
        PurchasableData data, int quantity, float price )
    {
        if ( details.TryGetValue(
            data.Id, out ItemSettlement summary ) )
        {
            summary.Quantity += quantity;
            summary.PriceTotal += price;
            return;
        }

        details.Add( data.Id, new ItemSettlement
        {
            Data = data,
            Quantity = quantity,
            PriceTotal = price
        } );
    }

    #endregion

    #region ----- 저장/복구 -----

    /// <summary>
    /// 주간 결산 저장 데이터 생성
    /// </summary>
    /// <returns>결산 모델 세이브 데이터</returns>
    public SettlementSaveData CreateSaveData ()
    {
        return new SettlementSaveData(
            _weeklySettlements, _hasNotification );
    }

    /// <summary>
    /// 검증된 과거 주간 결산 상태 복구
    /// </summary>
    /// <param name="restoreState">주간 결산 복구 상태</param>
    public void Restore ( SettlementRestoreState restoreState )
    {
        _weeklySettlements =
            restoreState.CreateWeeklySettlements( );
        _hasNotification = restoreState.HasNotification;
    }

    #endregion
}
