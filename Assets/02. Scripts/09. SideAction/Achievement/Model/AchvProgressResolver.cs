/// <summary>
/// 기존 영업 기록 기반 업적 진행도 계산
/// </summary>
public class AchvProgressResolver
{
    #region ----- 진행도 계산 -----

    /// <summary>
    /// 업적 진행도 종류에 맞는 현재 누적값 계산
    /// </summary>
    /// <param name="type">진행도 판정 종류</param>
    /// <param name="context">업적 판정 원본</param>
    /// <returns>현재 누적 진행도</returns>
    public int Resolve ( AchvProgressType type, AchvEvalContext context )
    {
        switch ( type )
        {
            case AchvProgressType.BusinessDayCount:     //누적 영업일
                return context.CompletedBusinessDayCount;

            case AchvProgressType.WeeklySettlementCount:        //완료한 주간 결산 횟수
                return context.CompletedWeeklySettlementCount +
                    ( context.IsWeekEnd ? 1 : 0 );

            default:
                return SumRecordProgress( type, context );
        }
    }

    #endregion

    #region ----- 기록 집계 -----

    /// <summary>
    /// 완료 기록과 현재 기록의 진행도 합산
    /// </summary>
    /// <param name="type">진행도 판정 종류</param>
    /// <param name="context">업적 판정 원본</param>
    /// <returns>기록 기반 누적 진행도</returns>
    int SumRecordProgress ( AchvProgressType type, AchvEvalContext context )
    {
        int progress = 0;

        //일일 기록 순회
        for ( int i = 0; i < context.CompletedRecords.Count; i++ )
        {
            //진행도 더하기
            progress += GetRecordProgress( type, context.CompletedRecords [ i ] );
        }

        progress += GetRecordProgress( type, context.CurrentRecord );
        return progress;
    }

    /// <summary>
    /// 일일 기록 하나의 업적 진행도 계산
    /// </summary>
    /// <param name="type">진행도 판정 종류</param>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>일일 기록 진행도</returns>
    int GetRecordProgress ( AchvProgressType type, DailyRecord record )
    {
        switch ( type )
        {
            case AchvProgressType.EmployeeHireCount:        //고용 횟수
                return record.EmployeeHireCount;

            case AchvProgressType.DeliveryCount:        //배송 횟수
                return CountDeliveries( record );

            case AchvProgressType.SGradeCount:      //S등급 개수
                return CountSGrades( record );

            case AchvProgressType.CraftCount:       //제작 횟수
                return record.CraftRecords.Count;

            case AchvProgressType.ActivatedThemeCraftCount:     //활성 테마 제작 횟수
                return CountActivatedThemeCrafts( record );

            case AchvProgressType.CompleteThemeCraftCount:      //완성 테마 제작 횟수
                return CountCompleteThemeCrafts( record );

            case AchvProgressType.EmployeeDeliveryCount:        //직원 배송 횟수
                return CountEmployeeDeliveries( record );

            case AchvProgressType.CumulativeNetProfit:      //누적 순이익
                return GetNetProfit( record );

            case AchvProgressType.DeadlineDayOnTimeDeliveryCount:       //마감일 정상 납품
                return CountDeadlineDayDeliveries( record );

            case AchvProgressType.LateDeliveryCount:        //지연 납품 횟수
                return CountLateDeliveries( record );

            case AchvProgressType.AOrHigherGradeCount:      //A등급 이상 납품
                return CountAOrHigherGrades( record );

            case AchvProgressType.AllRequirementsDeliveryCount:        //주요 요구 전체 달성
                return CountCompletedRequirementsDeliveries( record );

            case AchvProgressType.AllWishesDeliveryCount:       //희망 사항 전체 달성
                return CountCompletedWishesDeliveries( record );

            case AchvProgressType.PerfectCustomCraftCount:      //모든 주문 제작 조건 달성
                return CountPerfectCustomCrafts( record );

            case AchvProgressType.OppositeThemeDeliveryCount:       //상극 테마 제작물 납품
                return CountOppositeThemeDeliveries( record );

            default:
                return 0;
        }
    }

    #endregion

    #region ----- 종류별 개수 계산 -----

    /// <summary>
    /// 정상과 지연 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>납품 완료 수</returns>
    int CountDeliveries ( DailyRecord record )
    {
        int count = 0;

        //주문 기록 순회
        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            //결과 가져오기
            OrderOutcome outcome = record.OrderRecords [ i ].Outcome;

            //정상 납품이거나 지연 납품일 때
            if ( outcome == OrderOutcome.NormalDelivery ||
                outcome == OrderOutcome.LateDelivery )
                count++;
        }

        return count;
    }

    /// <summary>
    /// S등급 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>S등급 납품 수</returns>
    int CountSGrades ( DailyRecord record )
    {
        int count = 0;

        //주문 기록 순회
        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            //주문 가져오기
            DailyOrderRecord order = record.OrderRecords [ i ];

            //납품한 주문이 S등급일 때
            if ( order.HasEvaluation && order.Grade == DeliveryGrade.S )
                count++;
        }

        return count;
    }

    /// <summary>
    /// 일반 테마가 활성화된 제작 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>테마 활성 제작 수</returns>
    int CountActivatedThemeCrafts ( DailyRecord record )
    {
        int count = 0;

        //제작 기록 순회
        for ( int i = 0; i < record.CraftRecords.Count; i++ )
        {
            //활성 테마가 있다면
            if ( record.CraftRecords [ i ].HasActivatedTheme )
                count++;
        }

        return count;
    }

    /// <summary>
    /// 완성 테마를 달성한 제작 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>완성 테마 제작 수</returns>
    int CountCompleteThemeCrafts ( DailyRecord record )
    {
        int count = 0;

        //제작 기록 순회
        for ( int i = 0; i < record.CraftRecords.Count; i++ )
        {
            //완성 테마가 있다면
            if ( record.CraftRecords [ i ].HasCompleteTheme )
                count++;
        }

        return count;
    }

    /// <summary>
    /// 직원 배송 완료 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>직원 배송 완료 수</returns>
    int CountEmployeeDeliveries ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            DailyOrderRecord order = record.OrderRecords [ i ];

            if ( IsDelivered( order ) &&
                order.DeliveryMethod == DeliveryMethod.Employee )
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 일일 순이익 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>정수 단위 일일 순이익</returns>
    int GetNetProfit ( DailyRecord record )
    {
        float income = SumOrderRewards( record ) +
            SumValues( record.InventorySaleIncomes ) +
            SumValues( record.OtherIncomes );

        float expense = SumItemPrices( record.PurchaseRecords ) +
            SumRestockFees( record.QuickRestockRecords ) +
            SumValues( record.DeliveryExpenses ) +
            SumValues( record.EmployeeHireExpenses ) +
            SumValues( record.EmployeeWageExpenses ) +
            SumValues( record.OtherExpenses );

        return ( int ) ( income - expense );
    }

    /// <summary>
    /// 마감일 당일 정상 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>마감일 당일 정상 납품 수</returns>
    int CountDeadlineDayDeliveries ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            if ( record.OrderRecords [ i ].IsDeadlineDayNormalDelivery )
                count++;
        }

        return count;
    }

    /// <summary>
    /// 지연 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>지연 납품 수</returns>
    int CountLateDeliveries ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            if ( record.OrderRecords [ i ].Outcome == OrderOutcome.LateDelivery )
                count++;
        }

        return count;
    }

    /// <summary>
    /// A등급 이상 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>A등급 이상 납품 수</returns>
    int CountAOrHigherGrades ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            DailyOrderRecord order = record.OrderRecords [ i ];

            if ( order.HasEvaluation &&
                ( order.Grade == DeliveryGrade.S ||
                order.Grade == DeliveryGrade.A ) )
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 모든 주요 요구를 달성한 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>주요 요구 전체 달성 납품 수</returns>
    int CountCompletedRequirementsDeliveries ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            DailyOrderRecord order = record.OrderRecords [ i ];

            if ( IsDelivered( order ) && order.RequirementCount > 0 &&
                order.CompletedRequirementCount == order.RequirementCount )
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 모든 희망 사항을 달성한 납품 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>희망 사항 전체 달성 납품 수</returns>
    int CountCompletedWishesDeliveries ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            DailyOrderRecord order = record.OrderRecords [ i ];

            if ( IsDelivered( order ) && order.WishCount > 0 &&
                order.CompletedWishCount == order.WishCount )
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 모든 주문 제작 조건을 달성한 제작 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>모든 주문 제작 조건 달성 수</returns>
    int CountPerfectCustomCrafts ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.CraftRecords.Count; i++ )
        {
            DailyCraftRecord craft = record.CraftRecords [ i ];

            if ( craft.AreAllRequirementsCompleted &&
                craft.AreAllWishesCompleted && craft.IsTargetThemeCompleted )
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 상극 테마 제작물을 납품한 수 계산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>상극 테마 제작물 납품 수</returns>
    int CountOppositeThemeDeliveries ( DailyRecord record )
    {
        int count = 0;

        for ( int i = 0; i < record.CraftRecords.Count; i++ )
        {
            DailyCraftRecord craft = record.CraftRecords [ i ];

            if ( craft.ThemeConflictCount > 0 &&
                HasDeliveredOrder( record, craft.OrderId ) )
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 주문이 정상 또는 지연 납품됐는지 확인
    /// </summary>
    /// <param name="order">확인할 주문 기록</param>
    /// <returns>납품 완료 여부</returns>
    bool IsDelivered ( DailyOrderRecord order )
    {
        return order.Outcome == OrderOutcome.NormalDelivery ||
            order.Outcome == OrderOutcome.LateDelivery;
    }

    /// <summary>
    /// 주문 아이디에 해당하는 납품 기록 확인
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <param name="orderId">확인할 주문 아이디</param>
    /// <returns>납품 기록 존재 여부</returns>
    bool HasDeliveredOrder ( DailyRecord record, string orderId )
    {
        for ( int i = 0; i < record.OrderRecords.Count; i++ )
        {
            DailyOrderRecord order = record.OrderRecords [ i ];

            if ( order.OrderId == orderId && IsDelivered( order ) )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 주문 보상 수익 합산
    /// </summary>
    /// <param name="record">확인할 일일 기록</param>
    /// <returns>주문 보상 합계</returns>
    float SumOrderRewards ( DailyRecord record )
    {
        float total = 0f;

        for ( int i = 0; i < record.OrderRecords.Count; i++ )
            total += record.OrderRecords [ i ].OrderReward;

        return total;
    }

    /// <summary>
    /// 금액 목록 합산
    /// </summary>
    /// <param name="values">합산할 금액 목록</param>
    /// <returns>금액 합계</returns>
    float SumValues ( System.Collections.Generic.IReadOnlyList<float> values )
    {
        float total = 0f;

        for ( int i = 0; i < values.Count; i++ )
            total += values [ i ];

        return total;
    }

    /// <summary>
    /// 상품 구매 금액 합산
    /// </summary>
    /// <param name="records">구매 기록 목록</param>
    /// <returns>구매 금액 합계</returns>
    float SumItemPrices (
        System.Collections.Generic.IReadOnlyList<DailyItemRecord> records )
    {
        float total = 0f;

        for ( int i = 0; i < records.Count; i++ )
            total += records [ i ].PriceTotal;

        return total;
    }

    /// <summary>
    /// 빠른 재입고 비용 합산
    /// </summary>
    /// <param name="records">빠른 재입고 기록 목록</param>
    /// <returns>빠른 재입고 비용 합계</returns>
    float SumRestockFees (
        System.Collections.Generic.IReadOnlyList<DailyQuickRestockRecord> records )
    {
        float total = 0f;

        for ( int i = 0; i < records.Count; i++ )
            total += records [ i ].Fee;

        return total;
    }

    #endregion
}
