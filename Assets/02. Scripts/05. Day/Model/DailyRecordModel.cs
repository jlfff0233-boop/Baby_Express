using System;
using System.Collections.Generic;

/// <summary>
/// 일일 제작/주문/거래 기록 모델
/// </summary>
public class DailyRecordModel
{
    DailyRecord _currentRecord;       //현재 영업일 기록
    List<DailyRecord> _records = new List<DailyRecord>( );       //완료된 영업일 기록

    /// <summary>
    /// 현재 영업일 기록
    /// </summary>
    public DailyRecord CurrentRecord => _currentRecord;
    /// <summary>
    /// 완료된 영업일 기록
    /// </summary>
    public IReadOnlyList<DailyRecord> Records => _records;


    /// <summary>
    /// 일일 기록 모델 생성
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="startBudget">영업 시작 자금</param>
    /// <param name="craftLimit">일일 제작 할당량</param>
    /// <param name="quickRestockLimit">빠른 재입고 최대 횟수</param>
    public DailyRecordModel (
        int totalDay, float startBudget, int craftLimit, int quickRestockLimit )
    {
        StartDay( totalDay, startBudget, craftLimit, quickRestockLimit );
    }

    /// <summary>
    /// 새 영업일 기록 시작
    /// </summary>
    /// <param name="totalDay">새 누적 영업일</param>
    /// <param name="startBudget">영업 시작 자금</param>
    /// <param name="craftLimit">일일 제작 할당량</param>
    /// <param name="quickRestockLimit">빠른 재입고 최대 횟수</param>
    public void StartDay (
        int totalDay, float startBudget, int craftLimit, int quickRestockLimit )
    {
        _currentRecord = new DailyRecord
        {
            TotalDay = totalDay,
            StartBudget = startBudget,
            EndBudget = startBudget,

            CraftLimit = craftLimit,
            QuickRestockLimit = quickRestockLimit,
            EndReason = DayEndReason.None
        };
    }

    #region ----- 제작 기록 -----

    /// <summary>
    /// 제작 완료와 소비 파츠 기록
    /// </summary>
    /// <param name="craftResult">확정 제작 결과</param>
    public void RecordCraft ( CraftResult craftResult )
    {
        var partRecords = new Dictionary<string, DailyItemRecord>( );
        CraftReviewData reviewData = craftResult.ReviewData;

        //같은 파츠를 상품별 수량과 기본 가격으로 합산
        for ( int i = 0; i < craftResult.PlacedParts.Count; i++ )
        {
            PartsData part = craftResult.PlacedParts [ i ].PartData;

            if ( partRecords.TryGetValue( part.Id, out DailyItemRecord record ) )
            {
                record.Quantity++;
                record.PriceTotal += part.BasePrice;
                continue;
            }

            partRecords.Add( part.Id, new DailyItemRecord
            {
                Data = part,
                Quantity = 1,
                PriceTotal = part.BasePrice
            } );
        }

        _currentRecord.CraftRecords.Add( new DailyCraftRecord
        {
            OrderId = craftResult.OrderId,
            HasActivatedTheme = reviewData.ThemeResults.Count > 0,
            HasCompleteTheme = HasCompleteTheme( reviewData.ThemeResults ),
            ThemeConflictCount = reviewData.ScoreResult.ConflictCount,
            IsTargetThemeCompleted =
                HasCompletedTargetTheme( reviewData.TargetThemeResults ),
            AreAllRequirementsCompleted =
                AreAllConditionsCompleted( reviewData.RequirementResults ),
            AreAllWishesCompleted =
                AreAllConditionsCompleted( reviewData.WishResults ),
            UsedParts = new List<DailyItemRecord>( partRecords.Values )
        } );
    }

    /// <summary>
    /// 제작 완료 주문의 제작물 취소/폐기 기록
    /// </summary>
    /// <param name="orderId">취소/폐기한 주문 아이디</param>
    /// <returns>기록 성공 여부</returns>
    public bool RecordCraftDiscard ( string orderId )
    {
        for ( int i = 0; i < _currentRecord.CraftRecords.Count; i++ )
        {
            DailyCraftRecord record = _currentRecord.CraftRecords [ i ];

            if ( record.OrderId != orderId ) continue;

            record.IsDiscarded = true;
            return true;
        }

        return false;
    }

    #endregion

    #region ----- 주문 기록 -----

    /// <summary>
    /// 배송 출발 기록
    /// </summary>
    /// <param name="orderId">배송을 시작한 주문 아이디</param>
    public void RecordDeliveryStarted ( string orderId )
    {
        //같은 주문의 배송 출발 중복 기록 차단
        if ( _currentRecord.DeliveryStartedOrderIds.Contains( orderId ) ) return;

        _currentRecord.DeliveryStartedOrderIds.Add( orderId );
    }

    /// <summary>
    /// 배송 완료 주문 결과와 평가 기록
    /// </summary>
    /// <param name="deliveryResult">확정 배송 결과</param>
    /// <param name="reviewData">확정 제작 판정 데이터</param>
    /// <param name="deliveryDue">주문의 납품 마감 누적 영업일</param>
    public void RecordDelivery (
        DeliveryResult deliveryResult, CraftReviewData reviewData,
        int deliveryDue )
    {
        //같은 주문의 배송 완료 중복 기록 차단
        for ( int i = 0;
            i < _currentRecord.OrderRecords.Count;
            i++ )
        {
            if ( _currentRecord.OrderRecords [ i ].OrderId ==
                deliveryResult.OrderId )
                return;
        }

        //주요 요구 달성 수 계산
        int completedRequirementCount =
            CountCompletedConditions( reviewData.RequirementResults );

        //희망 달성 수 계산
        int completedWishCount =
            CountCompletedConditions( reviewData.WishResults );

        _currentRecord.OrderRecords.Add( new DailyOrderRecord
        {
            OrderId = deliveryResult.OrderId,
            Outcome = deliveryResult.Outcome,

            HasEvaluation = true,
            Grade = deliveryResult.Grade,
            DeliveryMethod = deliveryResult.Method,
            EmployeeId = deliveryResult.EmployeeId,
            IsDeadlineDayNormalDelivery =
                deliveryResult.Outcome == OrderOutcome.NormalDelivery &&
                deliveryResult.ArrivalTotalDay == deliveryDue,

            CompletedRequirementCount = completedRequirementCount,
            RequirementCount = reviewData.RequirementResults.Count,
            CompletedWishCount = completedWishCount,
            WishCount = reviewData.WishResults.Count,

            OrderReward = deliveryResult.FinalOrderReward
        } );
    }

    /// <summary>
    /// 배송 평가가 없는 종료 주문 결과 기록
    /// </summary>
    /// <param name="orderId">종료 주문 아이디</param>
    /// <param name="outcome">거절/취소/최종 실패 결과</param>
    public void RecordOrderResult ( string orderId, OrderOutcome outcome )
    {
        _currentRecord.OrderRecords.Add( new DailyOrderRecord
        {
            OrderId = orderId,
            Outcome = outcome
        } );
    }

    #endregion

    #region ----- 경제 기록 -----

    /// <summary>
    /// 상점 구매 기록
    /// </summary>
    /// <param name="data">구매한 상품 데이터</param>
    /// <param name="quantity">구매 수량</param>
    /// <param name="expense">상품 구매 지출</param>
    public void RecordPurchase (
        PurchasableData data, int quantity, float expense )
    {
        _currentRecord.PurchaseRecords.Add( new DailyItemRecord
        {
            Data = data,
            Quantity = quantity,
            PriceTotal = expense
        } );
    }

    /// <summary>
    /// 인벤토리 판매 수익 기록
    /// </summary>
    /// <param name="income">판매 수익</param>
    public void RecordInventorySale ( float income )
    {
        _currentRecord.InventorySaleIncomes.Add( income );
    }

    /// <summary>
    /// 빠른 재입고 기록
    /// </summary>
    /// <param name="data">재입고한 상품 데이터</param>
    /// <param name="fee">빠른 재입고 이용료</param>
    public void RecordQuickRestock ( PurchasableData data, float fee )
    {
        _currentRecord.QuickRestockRecords.Add( new DailyQuickRestockRecord
        {
            Data = data,
            Fee = fee
        } );
    }

    /// <summary>
    /// 직접 배송비 기록
    /// </summary>
    /// <param name="expense">실제 지출한 배송비</param>
    public void RecordDeliveryExpense ( float expense )
    {
        _currentRecord.DeliveryExpenses.Add( expense );
    }

    /// <summary>
    /// 직원 고용비 기록
    /// </summary>
    /// <param name="expense">실제 지출한 고용비</param>
    public void RecordEmployeeHireExpense ( float expense )
    {
        _currentRecord.EmployeeHireExpenses.Add( expense );
    }

    /// <summary>
    /// 직원 고용 성공 기록
    /// </summary>
    public void RecordEmployeeHire ()
    {
        _currentRecord.EmployeeHireCount++;
    }

    /// <summary>
    /// 직원 주급 기록
    /// </summary>
    /// <param name="expense">실제 지급한 주급</param>
    public void RecordEmployeeWageExpense ( float expense )
    {
        _currentRecord.EmployeeWageExpenses.Add( expense );
    }

    /// <summary>
    /// 기타 수입 기록
    /// </summary>
    /// <param name="income">기타 수입</param>
    public void RecordOtherIncome ( float income )
    {
        _currentRecord.OtherIncomes.Add( income );
    }

    /// <summary>
    /// 마지막으로 기록한 기타 수입 원상 복구
    /// </summary>
    /// <param name="income">복구할 기타 수입</param>
    /// <returns>원상 복구 성공 여부</returns>
    public bool RollbackOtherIncome ( float income )
    {
        List<float> incomes = _currentRecord.OtherIncomes;
        int lastIndex = incomes.Count - 1;

        //다른 기타 수입을 잘못 제거하지 않도록 마지막 기록만 확인
        if ( lastIndex < 0 || incomes [ lastIndex ] != income )
            return false;

        incomes.RemoveAt( lastIndex );
        return true;
    }

    /// <summary>
    /// 기타 지출 기록
    /// </summary>
    /// <param name="expense">기타 지출</param>
    public void RecordOtherExpense ( float expense )
    {
        _currentRecord.OtherExpenses.Add( expense );
    }

    #endregion

    #region ----- 기록 확정 -----

    /// <summary>
    /// 현재 영업일 기록 확정
    /// </summary>
    /// <param name="endReason">영업 종료 사유</param>
    /// <param name="endBudget">영업 종료 자금</param>
    /// <returns>확정된 일일 기록</returns>
    public DailyRecord CompleteDay ( DayEndReason endReason, float endBudget )
    {
        _currentRecord.EndReason = endReason;
        _currentRecord.EndBudget = endBudget;

        //주간 결산과 과거 조회를 위해 완료 기록 보관
        _records.Add( _currentRecord );

        return _currentRecord;
    }

    #endregion

    #region ----- 저장/복구 -----

    /// <summary>
    /// 현재 영업일과 과거 영업일 기록의 저장 데이터 생성
    /// </summary>
    /// <returns>일일 기록 모델 세이브 데이터</returns>
    public DailyRecordModelSaveData CreateSaveData ()
    {
        var records = new List<DailyRecordSaveData>( _records.Count );

        for ( int i = 0; i < _records.Count; i++ )
        {
            records.Add( new DailyRecordSaveData( _records [ i ] ) );
        }

        return new DailyRecordModelSaveData(
            new DailyRecordSaveData( _currentRecord ), records );
    }

    /// <summary>
    /// 검증된 일일 기록 상태 복구
    /// </summary>
    /// <param name="restoreState">일일 기록 복구 상태</param>
    public void Restore ( DailyRecordRestoreState restoreState )
    {
        _currentRecord = restoreState.CurrentRecord;
        _records = restoreState.CreatePastRecords( );
    }

    #endregion

    /// <summary>
    /// 완성 테마 포함 여부 확인
    /// </summary>
    /// <param name="themes">활성 테마 결과 목록</param>
    /// <returns>완성 테마 포함 여부</returns>
    bool HasCompleteTheme ( IReadOnlyList<CraftThemeResult> themes )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ].IsCompleted )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 완성한 목표 테마 포함 여부 확인
    /// </summary>
    /// <param name="themes">목표 테마 결과 목록</param>
    /// <returns>완성한 목표 테마 포함 여부</returns>
    bool HasCompletedTargetTheme (
        IReadOnlyList<CraftTargetThemeResult> themes )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ].IsCompleted )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 하나 이상 존재하는 제작 조건의 전체 달성 여부 확인
    /// </summary>
    /// <param name="conditions">제작 조건 결과 목록</param>
    /// <returns>전체 조건 달성 여부</returns>
    bool AreAllConditionsCompleted (
        IReadOnlyList<CraftConditionResult> conditions )
    {
        if ( conditions.Count == 0 ) return false;

        return CountCompletedConditions( conditions ) == conditions.Count;
    }

    /// <summary>
    /// 달성한 제작 조건 수 계산
    /// </summary>
    /// <param name="conditions">제작 조건 결과 목록</param>
    /// <returns>달성한 조건 수</returns>
    int CountCompletedConditions (
        IReadOnlyList<CraftConditionResult> conditions )
    {
        int completedCount = 0;

        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( conditions [ i ].IsCompleted )
                completedCount++;
        }

        return completedCount;
    }
}
