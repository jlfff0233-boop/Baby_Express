using System;
using System.Collections.Generic;

/// <summary>
/// 주간 결산 저장 복구 처리 - 저장 데이터 검증과 결산 복구 상태 생성
/// </summary>
public class SettlementSaveRestorer
{
    #region ----- 결산 값 검증 -----

    /// <summary>
    /// 유한 실수 여부 확인
    /// </summary>
    /// <param name="value">확인할 실수</param>
    /// <returns>유한 실수 여부</returns>
    bool IsFinite ( float value )
    {
        return float.IsNaN( value ) == false &&
            float.IsInfinity( value ) == false;
    }

    /// <summary>
    /// 음수가 아닌 유한한 금액 여부 확인
    /// </summary>
    /// <param name="amount">확인할 금액</param>
    /// <returns>유효한 금액 여부</returns>
    bool IsValidAmount ( float amount )
    {
        return amount >= 0f && IsFinite( amount );
    }

    /// <summary>
    /// 주간 결산 기본 정보 유효 여부 확인
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>기본 정보 유효 여부</returns>
    bool IsValidBasicData (
        WeeklySettlementSaveData saveData )
    {
        return saveData.Week > 0 &&
            saveData.StartTotalDay > 0 &&
            saveData.EndTotalDay >= saveData.StartTotalDay &&
            IsFinite( saveData.StartBudget ) &&
            IsFinite( saveData.EndBudget ) &&
            saveData.HiredEmployeeCount >= 0;
    }

    /// <summary>
    /// 주간 운영 현황 유효 여부 확인
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>운영 현황 유효 여부</returns>
    bool IsValidOperationData (
        WeeklySettlementSaveData saveData )
    {
        return
            saveData.ProductionOrderCount >= 0 &&
            saveData.CraftCompletedCount >= 0 &&
            saveData.CraftLimit > 0 &&
            saveData.DiscardedCraftCount >= 0 &&
            saveData.DiscardedCraftCount <=
                saveData.CraftCompletedCount &&

            saveData.DeliveryStartedCount >= 0 &&
            saveData.PendingDeliveryCount >= 0 &&
            saveData.InDeliveryCount >= 0 &&

            saveData.QuickRestockCount >= 0 &&
            saveData.QuickRestockLimit > 0 &&
            saveData.QuickRestockCount <=
                saveData.QuickRestockLimit;
    }

    /// <summary>
    /// 주간 주문 결과 유효 여부 확인
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주문 결과 유효 여부</returns>
    bool IsValidOrderData (
        WeeklySettlementSaveData saveData )
    {
        return
            saveData.NormalDeliveryCount >= 0 &&
            saveData.LateDeliveryCount >= 0 &&
            saveData.RejectedCount >= 0 &&
            saveData.AutoRejectedCount >= 0 &&
            saveData.CancelledCount >= 0 &&
            saveData.FailedCount >= 0;
    }

    /// <summary>
    /// 주간 주문 평가 유효 여부 확인
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주문 평가 유효 여부</returns>
    bool IsValidEvaluationData (
        WeeklySettlementSaveData saveData )
    {
        return
            saveData.CompletedRequirementCount >= 0 &&
            saveData.RequirementCount >= 0 &&
            saveData.CompletedRequirementCount <=
                saveData.RequirementCount &&

            saveData.CompletedWishCount >= 0 &&
            saveData.WishCount >= 0 &&
            saveData.CompletedWishCount <=
                saveData.WishCount &&

            saveData.SGradeCount >= 0 &&
            saveData.AGradeCount >= 0 &&
            saveData.BGradeCount >= 0 &&
            saveData.CGradeCount >= 0 &&
            saveData.DGradeCount >= 0;
    }

    /// <summary>
    /// 주간 경제 결과 유효 여부 확인
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>경제 결과 유효 여부</returns>
    bool IsValidEconomyData (
        WeeklySettlementSaveData saveData )
    {
        return
            IsValidAmount(
                saveData.OrderRewardIncome ) &&
            IsValidAmount(
                saveData.InventorySaleIncome ) &&
            IsValidAmount( saveData.OtherIncome ) &&
            IsValidAmount(
                saveData.PurchaseExpense ) &&
            IsValidAmount(
                saveData.QuickRestockExpense ) &&
            IsValidAmount(
                saveData.DeliveryExpense ) &&
            IsValidAmount(
                saveData.EmployeeHireExpense ) &&
            IsValidAmount(
                saveData.EmployeeWeeklyExpense ) &&
            IsValidAmount( saveData.OtherExpense );
    }

    /// <summary>
    /// 주간 평가와 다음 주 보정 유효 여부 확인
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주간 평가와 보정 유효 여부</returns>
    bool IsValidWeeklyEvaluationData (
        WeeklySettlementSaveData saveData )
    {
        //저장 데이터 확인
        if ( saveData.EvaluationOrderCount < 0 ||
            IsFinite( saveData.WeeklyScore ) == false ||
            Enum.IsDefined( typeof( WeeklyRating ), saveData.Rating ) == false )
        {
            return false;
        }

        //다음 주 주간 보정이 없다면
        if ( saveData.IsAdjustmentApplied == false )
        {
            return saveData.OrderCountCorrection == 0 &&
                saveData.EasyWeight == 0 &&
                saveData.NormalWeight == 0 &&
                saveData.HardWeight == 0;
        }

        //난이도 가중치 확인
        if ( saveData.EasyWeight < 0 ||
            saveData.NormalWeight < 0 ||
            saveData.HardWeight < 0 )
        {
            return false;
        }

        //가중치 합계
        long totalWeight =
            ( long ) saveData.EasyWeight + saveData.NormalWeight + saveData.HardWeight;

        return totalWeight > 0 && totalWeight <= int.MaxValue;
    }

    #endregion

    #region ----- 주간 결산 복구 -----

    /// <summary>
    /// 결산 상품 상세 저장 데이터를 런타임 상세 목록으로 변환
    /// </summary>
    /// <param name="saveDatas">결산 상품 상세 저장 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="settlements">변환한 결산 상품 상세 목록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateItemSettlements (
        IReadOnlyList<SettlementItemSaveData> saveDatas,
        PurchasableDataMap dataMap,
        out List<ItemSettlement> settlements )
    {
        settlements = null;

        if ( saveDatas == null )
            return false;

        var restoredSettlements =
            new List<ItemSettlement>( saveDatas.Count );

        var itemIds = new HashSet<string>( );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            //저장 데이터 가져오기
            SettlementItemSaveData saveData = saveDatas [ i ];

            //데이터 확인 및 상품 데이터 가져오기
            if ( saveData == null || saveData.Quantity <= 0 ||
                IsValidAmount( saveData.PriceTotal ) == false ||
                itemIds.Add( saveData.ItemId ) == false ||
                dataMap.TryGetData( saveData.ItemId,
                    out PurchasableData data ) == false )
            {
                return false;
            }

            restoredSettlements.Add( new ItemSettlement
            {
                Data = data,
                Quantity = saveData.Quantity,
                PriceTotal = saveData.PriceTotal
            } );
        }

        settlements = restoredSettlements;
        return true;
    }

    /// <summary>
    /// 주간 운영 현황 복구 데이터 생성
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주간 운영 현황</returns>
    OperationSettlement CreateOperationSettlement (
        WeeklySettlementSaveData saveData )
    {
        return new OperationSettlement
        {
            ProductionOrderCount =
                saveData.ProductionOrderCount,
            CraftCompletedCount =
                saveData.CraftCompletedCount,
            CraftLimit = saveData.CraftLimit,
            DiscardedCraftCount =
                saveData.DiscardedCraftCount,

            DeliveryStartedCount =
                saveData.DeliveryStartedCount,
            PendingDeliveryCount =
                saveData.PendingDeliveryCount,
            InDeliveryCount =
                saveData.InDeliveryCount,

            QuickRestockCount =
                saveData.QuickRestockCount,
            QuickRestockLimit =
                saveData.QuickRestockLimit
        };
    }

    /// <summary>
    /// 주간 주문 결과 복구 데이터 생성
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주간 주문 결과</returns>
    OrderSettlement CreateOrderSettlement (
        WeeklySettlementSaveData saveData )
    {
        return new OrderSettlement
        {
            NormalDeliveryCount =
                saveData.NormalDeliveryCount,
            LateDeliveryCount =
                saveData.LateDeliveryCount,
            RejectedCount = saveData.RejectedCount,
            AutoRejectedCount =
                saveData.AutoRejectedCount,
            CancelledCount = saveData.CancelledCount,
            FailedCount = saveData.FailedCount
        };
    }

    /// <summary>
    /// 주간 주문 평가 복구 데이터 생성
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주간 주문 평가</returns>
    EvaluationSettlement CreateEvaluationSettlement (
        WeeklySettlementSaveData saveData )
    {
        return new EvaluationSettlement
        {
            CompletedRequirementCount =
                saveData.CompletedRequirementCount,
            RequirementCount =
                saveData.RequirementCount,

            CompletedWishCount =
                saveData.CompletedWishCount,
            WishCount = saveData.WishCount,

            SGradeCount = saveData.SGradeCount,
            AGradeCount = saveData.AGradeCount,
            BGradeCount = saveData.BGradeCount,
            CGradeCount = saveData.CGradeCount,
            DGradeCount = saveData.DGradeCount
        };
    }

    /// <summary>
    /// 주간 경제 결과 복구 데이터 생성
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>주간 경제 결과</returns>
    EconomySettlement CreateEconomySettlement (
        WeeklySettlementSaveData saveData )
    {
        return new EconomySettlement
        {
            OrderRewardIncome =
                saveData.OrderRewardIncome,
            InventorySaleIncome =
                saveData.InventorySaleIncome,
            OtherIncome = saveData.OtherIncome,

            PurchaseExpense =
                saveData.PurchaseExpense,
            QuickRestockExpense =
                saveData.QuickRestockExpense,
            DeliveryExpense =
                saveData.DeliveryExpense,
            EmployeeHireExpense =
                saveData.EmployeeHireExpense,
            EmployeeWeeklyExpense =
                saveData.EmployeeWeeklyExpense,
            OtherExpense = saveData.OtherExpense
        };
    }

    /// <summary>
    /// 다음 주 주문 생성 보정 복구 데이터 생성
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <returns>다음 주 주문 생성 보정</returns>
    WeeklyOrderAdjustment CreateWeeklyOrderAdjustment (
        WeeklySettlementSaveData saveData )
    {
        return new WeeklyOrderAdjustment
        {
            IsApplied = saveData.IsAdjustmentApplied,
            OrderCountCorrection =
                saveData.OrderCountCorrection,
            EasyWeight = saveData.EasyWeight,
            NormalWeight = saveData.NormalWeight,
            HardWeight = saveData.HardWeight
        };
    }

    /// <summary>
    /// 주간 결산 저장 데이터를 런타임 결산 데이터로 변환
    /// </summary>
    /// <param name="saveData">주간 결산 저장 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="settlement">변환한 주간 결산 데이터</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateWeeklySettlement (
        WeeklySettlementSaveData saveData,
        PurchasableDataMap dataMap,
        out WeeklySettlementData settlement )
    {
        settlement = null;

        //데이터 확인 및
        //구매 상세, 사용 파츠 상세, 빠른 재입고 상세 가져오기
        if ( IsValidBasicData( saveData ) == false ||
            IsValidOperationData( saveData ) == false ||
            IsValidOrderData( saveData ) == false ||
            IsValidEvaluationData( saveData ) == false ||
            IsValidEconomyData( saveData ) == false ||
            IsValidWeeklyEvaluationData( saveData ) == false ||
            TryCreateItemSettlements( saveData.PurchaseDetails, dataMap,
                out List<ItemSettlement> purchaseDetails ) == false ||
            TryCreateItemSettlements( saveData.UsedPartDetails, dataMap,
                out List<ItemSettlement> usedPartDetails ) == false ||
            TryCreateItemSettlements( saveData.QuickRestockDetails, dataMap,
                out List<ItemSettlement> quickRestockDetails ) == false )
        {
            return false;
        }

        settlement = new WeeklySettlementData
        {
            Week = saveData.Week,
            StartTotalDay = saveData.StartTotalDay,
            EndTotalDay = saveData.EndTotalDay,

            StartBudget = saveData.StartBudget,
            EndBudget = saveData.EndBudget,
            HiredEmployeeCount = saveData.HiredEmployeeCount,

            Operation = CreateOperationSettlement( saveData ),
            Orders = CreateOrderSettlement( saveData ),
            Evaluation = CreateEvaluationSettlement( saveData ),
            Economy = CreateEconomySettlement( saveData ),

            EvaluationOrderCount = saveData.EvaluationOrderCount,
            HasEnoughData = saveData.HasEnoughData,
            WeeklyScore = saveData.WeeklyScore,
            Rating = saveData.Rating,
            NextWeekAdjustment =
                CreateWeeklyOrderAdjustment( saveData ),

            PurchaseDetails = purchaseDetails,
            UsedPartDetails = usedPartDetails,
            QuickRestockDetails = quickRestockDetails
        };

        return true;
    }

    /// <summary>
    /// 주간 결산 저장 데이터를 런타임 결산 목록으로 변환
    /// </summary>
    /// <param name="saveData">결산 모델 저장 데이터</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="settlements">변환한 과거 주간 결산 목록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateRestoredSettlements (
        SettlementSaveData saveData, int totalDay,
        PurchasableDataMap dataMap,
        out List<WeeklySettlementData> settlements )
    {
        settlements = null;

        int expectedSettlementCount = ( totalDay - 1 ) / 7;

        //과거 주간 결산 데이터 확인
        if ( saveData?.WeeklySettlements == null ||
            saveData.WeeklySettlements.Count != expectedSettlementCount ||
            totalDay < 1 || dataMap == null )
        {
            return false;
        }

        var restoredSettlements =
            new List<WeeklySettlementData>( saveData.WeeklySettlements.Count );

        for ( int i = 0; i < saveData.WeeklySettlements.Count; i++ )
        {
            //저장 데이터 가져오기
            WeeklySettlementSaveData settlementData =
                saveData.WeeklySettlements [ i ];

            int expectedWeek = i + 1;
            int expectedStartTotalDay = ( expectedWeek - 1 ) * 7 + 1;
            int expectedEndTotalDay = expectedWeek * 7;

            //주차와 영업일 범위가 실제 진행 순서와 일치하는지 확인
            if ( settlementData == null ||
                settlementData.Week != expectedWeek ||
                settlementData.StartTotalDay != expectedStartTotalDay ||
                settlementData.EndTotalDay != expectedEndTotalDay ||
                settlementData.EndTotalDay >= totalDay ||
                TryCreateWeeklySettlement( settlementData, dataMap,
                    out WeeklySettlementData settlement ) == false )
            {
                return false;
            }

            restoredSettlements.Add( settlement );
        }

        settlements = restoredSettlements;
        return true;
    }

    /// <summary>
    /// 주간 결산 저장 데이터를 검증하고 복구 상태 생성
    /// </summary>
    /// <param name="saveData">결산 모델 저장 데이터</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="restoreState">생성한 주간 결산 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        SettlementSaveData saveData, int totalDay,
        PurchasableDataMap dataMap,
        out SettlementRestoreState restoreState )
    {
        restoreState = null;

        if ( TryCreateRestoredSettlements(
            saveData, totalDay, dataMap,
            out List<WeeklySettlementData> settlements ) == false )
        {
            return false;
        }

        restoreState =
            new SettlementRestoreState(
                settlements, saveData.HasNotification );

        return true;
    }

    #endregion
}
