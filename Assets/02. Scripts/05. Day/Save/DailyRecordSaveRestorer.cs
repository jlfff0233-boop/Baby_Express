using System;
using System.Collections.Generic;

/// <summary>
/// 일일 기록 저장 복구 처리 - 저장 데이터 검증과 일일 기록 복구 상태 생성
/// </summary>
public class DailyRecordSaveRestorer
{
    #region ----- 공통 값 검증 -----

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
    /// 음수가 아닌 유한한 수인지 확인
    /// </summary>
    /// <param name="amount">확인할 금액</param>
    /// <returns>유효한 금액 여부</returns>
    bool IsValidAmount ( float amount )
    {
        return amount >= 0f && IsFinite( amount ) == true;
    }

    /// <summary>
    /// 주문 아이디 저장 목록 복사
    /// </summary>
    /// <param name="saveDatas">주문 아이디 저장 데이터</param>
    /// <param name="orderIds">주문 아이디 목록</param>
    /// <returns>복사 성공 여부</returns>
    bool TryCopyOrderIds (
        IReadOnlyList<string> saveDatas,
        out List<string> orderIds )
    {
        orderIds = new List<string>( saveDatas.Count );
        var addedIds = new HashSet<string>( );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            string orderId = saveDatas [ i ];

            if ( string.IsNullOrWhiteSpace( orderId ) == true ||
                addedIds.Add( orderId ) == false )
            {
                return false;
            }

            orderIds.Add( orderId );
        }

        return true;
    }

    /// <summary>
    /// 일일 수입과 지출 금액 목록 검증 후 복사
    /// </summary>
    /// <param name="saveDatas">일일 수입과 지출 금액 저장 데이터</param>
    /// <param name="amounts">복사한 일일 수입과 지출 금액 목록</param>
    /// <returns>복사 성공 여부</returns>
    bool TryCopyRecordAmounts (
        IReadOnlyList<float> saveDatas,
        out List<float> amounts )
    {
        amounts = new List<float>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            float amount = saveDatas [ i ];

            if ( IsValidAmount( amount ) == false )
                return false;

            amounts.Add( amount );
        }

        return true;
    }

    #endregion

    #region ----- 세부 기록 복구 -----

    /// <summary>
    /// 상품 저장 목록을 런타임 기록으로 변환
    /// </summary>
    /// <param name="saveDatas">상품 기록 저장 데이터</param>
    /// <param name="dataMap">상품 데이터맵</param>
    /// <param name="records">상품 기록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateItemRecords (
        IReadOnlyList<DailyItemRecordSaveData> saveDatas,
        PurchasableDataMap dataMap,
        out List<DailyItemRecord> records )
    {
        records = new List<DailyItemRecord>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            DailyItemRecordSaveData saveData = saveDatas [ i ];

            if ( saveData == null || saveData.Quantity <= 0 ||
                IsValidAmount( saveData.PriceTotal ) == false ||
                dataMap.TryGetData(
                    saveData.ItemId, out PurchasableData data ) == false )
            {
                return false;
            }

            records.Add( new DailyItemRecord
            {
                Data = data,
                Quantity = saveData.Quantity,
                PriceTotal = saveData.PriceTotal
            } );
        }

        return true;
    }

    /// <summary>
    /// 빠른 재입고 저장 목록을 런타임 기록으로 변환
    /// </summary>
    /// <param name="saveDatas">빠른 재입고 기록 저장 데이터</param>
    /// <param name="dataMap">상품 데이터맵</param>
    /// <param name="records">빠른 재입고 기록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateQuickRestockRecords (
        IReadOnlyList<DailyQuickRestockRecordSaveData> saveDatas,
        PurchasableDataMap dataMap,
        out List<DailyQuickRestockRecord> records )
    {
        records = new List<DailyQuickRestockRecord>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            DailyQuickRestockRecordSaveData saveData = saveDatas [ i ];

            if ( saveData == null ||
                IsValidAmount( saveData.Fee ) == false ||
                dataMap.TryGetData(
                    saveData.ItemId, out PurchasableData data ) == false )
            {
                return false;
            }

            records.Add( new DailyQuickRestockRecord
            {
                Data = data,
                Fee = saveData.Fee
            } );
        }

        return true;
    }

    /// <summary>
    /// 제작 기록 저장 목록을 런타임 기록으로 변환
    /// </summary>
    /// <param name="saveDatas">제작 기록 저장 데이터</param>
    /// <param name="dataMap">상품 데이터맵</param>
    /// <param name="records">일일 제작 기록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateCraftRecords (
        IReadOnlyList<DailyCraftRecordSaveData> saveDatas,
        PurchasableDataMap dataMap,
        out List<DailyCraftRecord> records )
    {
        records = new List<DailyCraftRecord>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            DailyCraftRecordSaveData saveData = saveDatas [ i ];

            if ( saveData == null ||
                string.IsNullOrWhiteSpace( saveData.OrderId ) == true ||
                saveData.ThemeConflictCount < 0 ||
                saveData.UsedParts == null ||
                TryCreateItemRecords(
                    saveData.UsedParts, dataMap,
                    out List<DailyItemRecord> usedParts ) == false )
            {
                return false;
            }

            records.Add( new DailyCraftRecord
            {
                OrderId = saveData.OrderId,
                IsDiscarded = saveData.IsDiscarded,
                HasActivatedTheme = saveData.HasActivatedTheme,
                HasCompleteTheme = saveData.HasCompleteTheme,
                ThemeConflictCount = saveData.ThemeConflictCount,
                IsTargetThemeCompleted = saveData.IsTargetThemeCompleted,
                AreAllRequirementsCompleted = saveData.AreAllRequirementsCompleted,
                AreAllWishesCompleted = saveData.AreAllWishesCompleted,
                UsedParts = usedParts
            } );
        }

        return true;
    }

    /// <summary>
    /// 종료 주문 저장 목록을 런타임 기록으로 변환
    /// </summary>
    /// <param name="saveDatas">주문 기록 저장 데이터</param>
    /// <param name="records">주문 기록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateOrderRecords (
        IReadOnlyList<DailyOrderRecordSaveData> saveDatas,
        out List<DailyOrderRecord> records )
    {
        records = new List<DailyOrderRecord>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            DailyOrderRecordSaveData saveData = saveDatas [ i ];

            if ( saveData == null ||
                string.IsNullOrWhiteSpace( saveData.OrderId ) == true ||
                Enum.IsDefined(
                    typeof( OrderOutcome ), saveData.Outcome ) == false ||
                Enum.IsDefined(
                    typeof( DeliveryGrade ), saveData.Grade ) == false ||
                Enum.IsDefined(
                    typeof( DeliveryMethod ), saveData.DeliveryMethod ) == false ||
                saveData.RequirementCount < 0 ||
                saveData.CompletedRequirementCount < 0 ||
                saveData.CompletedRequirementCount > saveData.RequirementCount ||
                saveData.WishCount < 0 ||
                saveData.CompletedWishCount < 0 ||
                saveData.CompletedWishCount > saveData.WishCount ||
                saveData.OrderReward < 0 )
            {
                return false;
            }

            records.Add( new DailyOrderRecord
            {
                OrderId = saveData.OrderId,
                Outcome = saveData.Outcome,

                HasEvaluation = saveData.HasEvaluation,
                Grade = saveData.Grade,

                DeliveryMethod = saveData.DeliveryMethod,
                EmployeeId = saveData.EmployeeId,
                IsDeadlineDayNormalDelivery = saveData.IsDeadlineDayNormalDelivery,

                CompletedRequirementCount = saveData.CompletedRequirementCount,
                RequirementCount = saveData.RequirementCount,

                CompletedWishCount = saveData.CompletedWishCount,
                WishCount = saveData.WishCount,
                OrderReward = saveData.OrderReward
            } );
        }

        return true;
    }

    #endregion

    #region ----- 일일 기록 복구 -----

    /// <summary>
    /// 영업일 저장 기록을 런타임 기록으로 변환
    /// </summary>
    /// <param name="saveData">영업일 원본 기록 저장 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="record">변환한 영업일 기록</param>
    /// <returns>변환 성공 여부</returns>
    bool TryCreateRecord (
        DailyRecordSaveData saveData,
        PurchasableDataMap dataMap,
        out DailyRecord record )
    {
        record = null;

        if ( saveData == null || saveData.TotalDay < 1 ||
            Enum.IsDefined(
                typeof( DayEndReason ), saveData.EndReason ) == false ||
            IsFinite( saveData.StartBudget ) == false ||
            IsFinite( saveData.EndBudget ) == false ||
            saveData.CraftLimit <= 0 ||
            saveData.QuickRestockLimit <= 0 ||
            saveData.EmployeeHireCount < 0 ||
            saveData.CraftRecords == null ||
            saveData.DeliveryStartedOrderIds == null ||
            saveData.OrderRecords == null ||
            saveData.PurchaseRecords == null ||
            saveData.InventorySaleIncomes == null ||
            saveData.QuickRestockRecords == null ||
            saveData.DeliveryExpenses == null ||
            saveData.EmployeeHireExpenses == null ||
            saveData.EmployeeWageExpenses == null ||
            saveData.OtherIncomes == null ||
            saveData.OtherExpenses == null )
        {
            return false;
        }

        if ( TryCreateCraftRecords(
            saveData.CraftRecords, dataMap,
            out List<DailyCraftRecord> craftRecords ) == false ||
            TryCopyOrderIds(
                saveData.DeliveryStartedOrderIds,
                out List<string> deliveryStartedOrderIds ) == false ||
            TryCreateOrderRecords(
                saveData.OrderRecords,
                out List<DailyOrderRecord> orderRecords ) == false ||
            TryCreateItemRecords(
                saveData.PurchaseRecords, dataMap,
                out List<DailyItemRecord> purchaseRecords ) == false ||
            TryCopyRecordAmounts(
                saveData.InventorySaleIncomes,
                out List<float> inventorySaleIncomes ) == false ||
            TryCreateQuickRestockRecords(
                saveData.QuickRestockRecords, dataMap,
                out List<DailyQuickRestockRecord> quickRestockRecords ) == false ||
            TryCopyRecordAmounts(
                saveData.DeliveryExpenses,
                out List<float> deliveryExpenses ) == false ||
            TryCopyRecordAmounts(
                saveData.EmployeeHireExpenses,
                out List<float> employeeHireExpenses ) == false ||
            TryCopyRecordAmounts(
                saveData.EmployeeWageExpenses,
                out List<float> employeeWageExpenses ) == false ||
            TryCopyRecordAmounts(
                saveData.OtherIncomes,
                out List<float> otherIncomes ) == false ||
            TryCopyRecordAmounts(
                saveData.OtherExpenses,
                out List<float> otherExpenses ) == false )
        {
            return false;
        }

        record = new DailyRecord
        {
            TotalDay = saveData.TotalDay,
            EndReason = saveData.EndReason,

            StartBudget = saveData.StartBudget,
            EndBudget = saveData.EndBudget,

            CraftLimit = saveData.CraftLimit,
            QuickRestockLimit = saveData.QuickRestockLimit,
            EmployeeHireCount = saveData.EmployeeHireCount,

            CraftRecords = craftRecords,
            DeliveryStartedOrderIds = deliveryStartedOrderIds,
            OrderRecords = orderRecords,

            PurchaseRecords = purchaseRecords,
            InventorySaleIncomes = inventorySaleIncomes,
            QuickRestockRecords = quickRestockRecords,

            DeliveryExpenses = deliveryExpenses,
            EmployeeHireExpenses = employeeHireExpenses,
            EmployeeWageExpenses = employeeWageExpenses,

            OtherIncomes = otherIncomes,
            OtherExpenses = otherExpenses
        };

        return true;
    }

    /// <summary>
    /// 일일 기록 저장 데이터를 검증하고 복구 상태 생성
    /// </summary>
    /// <param name="saveData">일일 기록 모델 저장 데이터</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="restoreState">생성한 일일 기록 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        DailyRecordModelSaveData saveData, int totalDay,
        PurchasableDataMap dataMap,
        out DailyRecordRestoreState restoreState )
    {
        restoreState = null;

        if ( saveData?.CurrentRecord == null ||
            saveData.PastRecords == null ||
            dataMap == null )
        {
            return false;
        }

        DailyRecordSaveData currentData = saveData.CurrentRecord;

        //현재 영업일은 아직 결산되지 않은 기록만 허용
        if ( currentData.TotalDay != totalDay ||
            currentData.EndReason != DayEndReason.None ||
            TryCreateRecord(
                currentData, dataMap,
                out DailyRecord currentRecord ) == false )
        {
            return false;
        }

        var pastRecords =
            new List<DailyRecord>( saveData.PastRecords.Count );
        int previousDay = 0;

        for ( int i = 0; i < saveData.PastRecords.Count; i++ )
        {
            DailyRecordSaveData recordData =
                saveData.PastRecords [ i ];

            //과거 기록은 현재 영업일 이전의 결산 완료 기록만 허용
            if ( recordData == null ||
                recordData.TotalDay <= previousDay ||
                recordData.TotalDay >= totalDay ||
                recordData.EndReason == DayEndReason.None ||
                TryCreateRecord(
                    recordData, dataMap,
                    out DailyRecord record ) == false )
            {
                return false;
            }

            pastRecords.Add( record );
            previousDay = recordData.TotalDay;
        }

        restoreState = new DailyRecordRestoreState(
            currentRecord, pastRecords );

        return true;
    }

    #endregion
}
