using System;
using System.Collections.Generic;

/// <summary>
/// 결산 슬롯 표시 데이터 생성기 - 결산 결과를 구역별 슬롯 데이터로 변환
/// </summary>
public class SettlementSlotViewDataBuilder
{
    #region ----- 결산 구역 -----

    /// <summary>
    /// 일일 결산 요약 구역 생성
    /// </summary>
    /// <param name="data">일일 결산 계산 결과</param>
    /// <returns>일일 결산 요약 구역</returns>
    public SettlementSectionViewData CreateDailySummarySection (
        DailySettlementData data )
    {
        return CreateSection (
            SettlementSectionType.Summary ,
            CreateSlot (
                SettlementIconType.Business ,
                $"영업 종료: {GetEndReasonText ( data.EndReason )}" ) ,
            CreateSlot (
                SettlementIconType.Budget ,
                $"영업 시작 자금: {data.StartBudget:N0}G" ) ,
            CreateProfitSlot (
                "일일 순이익" , data.Economy.NetProfit ) ,
            CreateSlot (
                SettlementIconType.Budget ,
                $"최종 보유 자금: {data.EndBudget:N0}G" ) );
    }

    /// <summary>
    /// 주간 결산 요약 구역 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <param name="startMonth">결산 시작 월</param>
    /// <param name="startDay">결산 시작 일</param>
    /// <param name="endMonth">결산 종료 월</param>
    /// <param name="endDay">결산 종료 일</param>
    /// <returns>주간 결산 요약 구역</returns>
    public SettlementSectionViewData CreateWeeklySummarySection (
        WeeklySettlementData data ,
        int startMonth , int startDay ,
        int endMonth , int endDay )
    {
        return CreateSection (
            SettlementSectionType.Summary ,
            CreateSlot (
                SettlementIconType.Business ,
                $"기간: {startMonth}월 {startDay}일 ~ " +
                $"{endMonth}월 {endDay}일" ) ,
            CreateSlot (
                SettlementIconType.Order ,
                $"주간 영업 평가: " +
                $"{GetWeeklyRatingText ( data.Rating )}" ) ,
            CreateSlot (
                SettlementIconType.Budget ,
                $"주간 시작 자금: {data.StartBudget:N0}G" ) ,
            CreateProfitSlot (
                "주간 순이익" , data.Economy.NetProfit ) ,
            CreateSlot (
                SettlementIconType.Budget ,
                $"최종 보유 자금: {data.EndBudget:N0}G" ) );
    }

    /// <summary>
    /// 운영 현황 구역 생성
    /// </summary>
    /// <param name="operation">운영 현황</param>
    /// <param name="orders">주문 결과</param>
    /// <returns>운영 현황 구역</returns>
    public SettlementSectionViewData CreateOperationSection (
        OperationSettlement operation ,
        OrderSettlement orders )
    {
        return CreateSection (
            SettlementSectionType.Operation ,
            CreateProgressSlot (
                SettlementIconType.Craft ,
                $"제작 완료: {operation.CraftCompletedCount} / " +
                $"{operation.CraftLimit}" ,
                operation.CraftCompletedCount ,
                operation.CraftLimit ) ,
            CreateSlot (
                SettlementIconType.Craft ,
                $"취소/폐기 제작물: " +
                $"{operation.DiscardedCraftCount}" ) ,
            CreateSlot (
                SettlementIconType.Craft ,
                $"현재 제작 진행 주문: " +
                $"{operation.ProductionOrderCount}" ) ,
            CreateSlot (
                SettlementIconType.Delivery ,
                $"배송 출발 [{operation.DeliveryStartedCount}]  " +
                $"도착 [{orders.DeliveredCount}]  " +
                $"대기 [{operation.PendingDeliveryCount}]  " +
                $"배송 중 [{operation.InDeliveryCount}]" ) ,
            CreateProgressSlot (
                SettlementIconType.Restock ,
                $"빠른 재입고: {operation.QuickRestockCount} / " +
                $"{operation.QuickRestockLimit}" ,
                operation.QuickRestockCount ,
                operation.QuickRestockLimit ) );
    }

    /// <summary>
    /// 일일 주문 결과 구역 생성
    /// </summary>
    /// <param name="orders">주문 결과</param>
    /// <returns>일일 주문 결과 구역</returns>
    public SettlementSectionViewData CreateOrderResultSection (
        OrderSettlement orders )
    {
        return CreateSection (
            SettlementSectionType.OrderResult ,
            CreateSlot (
                SettlementIconType.Delivery ,
                $"정상 배송 [{orders.NormalDeliveryCount}]  " +
                $"지연 배송 [{orders.LateDeliveryCount}]" ) ,
            CreateSlot (
                SettlementIconType.Order ,
                $"직접 거절 [{orders.RejectedCount}]  " +
                $"자동 거절 [{orders.AutoRejectedCount}]" ) ,
            CreateSlot (
                SettlementIconType.Order ,
                $"주문 취소 [{orders.CancelledCount}]  " +
                $"최종 실패 [{orders.FailedCount}]" ) );
    }

    /// <summary>
    /// 주간 주문 결과 구역 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 주문 결과 구역</returns>
    public SettlementSectionViewData CreateWeeklyOrderResultSection (
        WeeklySettlementData data )
    {
        OrderSettlement orders = data.Orders;

        return CreateSection (
            SettlementSectionType.OrderResult ,
            CreateProgressSlot (
                SettlementIconType.Delivery ,
                $"정상 배송: {orders.NormalDeliveryCount} / " +
                $"{orders.DeliveredCount} " +
                $"(지연 {orders.LateDeliveryCount})" ,
                orders.NormalDeliveryCount ,
                orders.DeliveredCount ) ,
            CreateSlot (
                SettlementIconType.Order ,
                $"직접 거절 [{orders.RejectedCount}]  " +
                $"자동 거절 [{orders.AutoRejectedCount}]  " +
                $"취소 [{orders.CancelledCount}]  " +
                $"실패 [{orders.FailedCount}]" ) ,
            CreateSlot (
                SettlementIconType.Order ,
                $"평가 대상 주문: {data.EvaluationOrderCount} / " +
                $"{( data.HasEnoughData ? "자료 충족" : "자료 부족" )}" ) );
    }

    #endregion

    #region ----- 슬롯 결산 상세 -----

    /// <summary>
    /// 일일 주문 평가 구역 생성
    /// </summary>
    /// <param name="evaluation">주문 평가 결과</param>
    /// <returns>일일 주문 평가 구역</returns>
    public SettlementSectionViewData CreateEvaluationSection (
        EvaluationSettlement evaluation )
    {
        return CreateSection (
            SettlementSectionType.Evaluation ,
            CreateGradeSlot ( evaluation ) ,
            CreateProgressSlot (
                SettlementIconType.Requirement ,
                $"주요 요구: " +
                $"{evaluation.CompletedRequirementCount} / " +
                $"{evaluation.RequirementCount} 달성 " +
                $"({evaluation.RequirementCompletionRate:P1})" ,
                evaluation.CompletedRequirementCount ,
                evaluation.RequirementCount ) ,
            CreateProgressSlot (
                SettlementIconType.Wish ,
                $"희망 사항: " +
                $"{evaluation.CompletedWishCount} / " +
                $"{evaluation.WishCount} 달성 " +
                $"({evaluation.WishCompletionRate:P1})" ,
                evaluation.CompletedWishCount ,
                evaluation.WishCount ) );
    }

    /// <summary>
    /// 주간 주문 평가 구역 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 주문 평가 구역</returns>
    public SettlementSectionViewData CreateWeeklyEvaluationSection (
        WeeklySettlementData data )
    {
        SettlementSectionViewData section =
            CreateEvaluationSection ( data.Evaluation );

        section.Slots.Insert (
            0 ,
            CreateProgressSlot (
                SettlementIconType.Order ,
                $"주간 평가 점수: {data.WeeklyScore:0.#} / 100" ,
                data.WeeklyScore ,
                100f ) );

        return section;
    }

    /// <summary>
    /// 주문 등급 분포 슬롯 생성
    /// </summary>
    /// <param name="evaluation">주문 평가 결과</param>
    /// <returns>등급 분포 슬롯</returns>
    SettlementSlotViewData CreateGradeSlot (
        EvaluationSettlement evaluation )
    {
        return CreateSlot (
            GetGradeIconType ( evaluation ) ,
            $"S등급 [{evaluation.SGradeCount}]  " +
            $"A등급 [{evaluation.AGradeCount}]  " +
            $"B등급 [{evaluation.BGradeCount}]  " +
            $"C등급 [{evaluation.CGradeCount}]  " +
            $"D등급 [{evaluation.DGradeCount}]" );
    }

    /// <summary>
    /// 최다 주문 등급에 맞는 트로피 아이콘 종류 반환
    /// </summary>
    /// <param name="evaluation">주문 평가 결과</param>
    /// <returns>트로피 아이콘 종류</returns>
    SettlementIconType GetGradeIconType (
        EvaluationSettlement evaluation )
    {
        int highestCount = Math.Max (
            Math.Max ( evaluation.SGradeCount , evaluation.AGradeCount ) ,
            Math.Max ( evaluation.BGradeCount , Math.Max ( evaluation.CGradeCount , evaluation.DGradeCount ) ) );

        if ( highestCount <= 0 )
            return SettlementIconType.UnratedTrophy;

        int highestGradeCount = 0;

        if ( evaluation.SGradeCount == highestCount )
            highestGradeCount++;

        if ( evaluation.AGradeCount == highestCount )
            highestGradeCount++;

        if ( evaluation.BGradeCount == highestCount )
            highestGradeCount++;

        if ( evaluation.CGradeCount == highestCount )
            highestGradeCount++;

        if ( evaluation.DGradeCount == highestCount )
            highestGradeCount++;

        //동률 또는 C, D등급 최다는 미정 아이콘 사용
        if ( highestGradeCount != 1 )
            return SettlementIconType.UnratedTrophy;

        if ( evaluation.SGradeCount == highestCount )
            return SettlementIconType.GoldTrophy;

        if ( evaluation.AGradeCount == highestCount )
            return SettlementIconType.SilverTrophy;

        if ( evaluation.BGradeCount == highestCount )
            return SettlementIconType.BronzeTrophy;

        return SettlementIconType.UnratedTrophy;
    }

    /// <summary>
    /// 경제 결산 구역 생성
    /// </summary>
    /// <param name="economy">경제 결산 결과</param>
    /// <returns>경제 결산 구역</returns>
    public SettlementSectionViewData CreateEconomySection (
        EconomySettlement economy )
    {
        return CreateSection (
            SettlementSectionType.Economy ,
            CreateIncomeSlot (
                "주문 대금" , economy.OrderRewardIncome ) ,
            CreateIncomeSlot (
                "인벤토리 판매" , economy.InventorySaleIncome ) ,
            CreateIncomeSlot (
                "기타 수입" , economy.OtherIncome ) ,
            CreateIncomeSlot (
                "총수입" , economy.TotalIncome ) ,
            CreateExpenseSlot (
                "상점 구매" , economy.PurchaseExpense ) ,
            CreateExpenseSlot (
                "빠른 재입고" , economy.QuickRestockExpense ) ,
            CreateExpenseSlot (
                "직접 배송비" , economy.DeliveryExpense ) ,
            CreateExpenseSlot (
                "직원 고용비" , economy.EmployeeHireExpense ) ,
            CreateExpenseSlot (
                "직원 주급" , economy.EmployeeWeeklyExpense ) ,
            CreateExpenseSlot (
                "기타 지출" , economy.OtherExpense ) ,
            CreateExpenseSlot (
                "총지출" , economy.TotalExpense ) ,
            CreateProfitSlot (
                "순이익" , economy.NetProfit ) );
    }

    /// <summary>
    /// 구매 상세 구역 생성
    /// </summary>
    /// <param name="details">상품별 구매 상세</param>
    /// <param name="purchaseExpense">구매 총액</param>
    /// <returns>구매 상세 구역</returns>
    public SettlementSectionViewData CreatePurchaseSection (
        IReadOnlyList<ItemSettlement> details ,
        float purchaseExpense )
    {
        SettlementSectionViewData section =
            CreateSection ( SettlementSectionType.PurchaseDetail );

        if ( details.Count == 0 )
        {
            section.Slots.Add ( CreateSlot ( SettlementIconType.Expense , "구매 내역 없음" ) );
        }
        else
        {
            for ( int i = 0 ; i < details.Count ; i++ )
            {
                ItemSettlement detail = details [ i ];

                section.Slots.Add ( new SettlementSlotViewData
                {
                    Icon = detail.Data.Icon ,
                    Description =
                            $"{detail.Data.Name} x{detail.Quantity}: " +
                            $"-{detail.PriceTotal:N0}G" ,
                    TextTone = SettlementTextTone.Expense
                } );
            }
        }

        section.Slots.Add ( CreateExpenseSlot ( "구매 총액" , purchaseExpense ) );

        return section;
    }

    /// <summary>
    /// 사용 파츠 상세 구역 생성
    /// </summary>
    /// <param name="details">파츠별 사용 상세</param>
    /// <returns>사용 파츠 상세 구역</returns>
    public SettlementSectionViewData CreateUsedPartSection (
        IReadOnlyList<ItemSettlement> details )
    {
        SettlementSectionViewData section =
            CreateSection ( SettlementSectionType.UsedPartDetail );

        if ( details.Count == 0 )
        {
            section.Slots.Add ( CreateSlot ( SettlementIconType.Craft , "사용한 파츠 없음" ) );

            return section;
        }

        for ( int i = 0 ; i < details.Count ; i++ )
        {
            ItemSettlement detail = details [ i ];

            section.Slots.Add ( new SettlementSlotViewData
            {
                Icon = detail.Data.Icon ,
                Description =
                        $"{detail.Data.Name} x{detail.Quantity}: " +
                        $"기준가 {detail.PriceTotal:N0}G"
            } );
        }

        section.Slots.Add (
            CreateSlot (
                SettlementIconType.Craft ,
                $"사용 파츠 기준가 합계: " +
                $"{GetItemPriceTotal ( details ):N0}G" ) );

        return section;
    }

    /// <summary>
    /// 빠른 재입고 상세 구역 생성
    /// </summary>
    /// <param name="details">상품별 빠른 재입고 상세</param>
    /// <returns>빠른 재입고 상세 구역</returns>
    public SettlementSectionViewData CreateQuickRestockSection (
        IReadOnlyList<ItemSettlement> details )
    {
        SettlementSectionViewData section =
            CreateSection ( SettlementSectionType.QuickRestockDetail );

        if ( details.Count == 0 )
        {
            section.Slots.Add ( CreateSlot ( SettlementIconType.Restock , "빠른 재입고 내역 없음" ) );

            return section;
        }

        for ( int i = 0 ; i < details.Count ; i++ )
        {
            ItemSettlement detail = details [ i ];

            section.Slots.Add (
                new SettlementSlotViewData
                {
                    Icon = detail.Data.Icon ,
                    Description =
                        $"{detail.Data.Name}: " +
                        $"{detail.Quantity}회 / " +
                        $"-{detail.PriceTotal:N0}G" ,
                    TextTone = SettlementTextTone.Expense
                } );
        }

        return section;
    }

    /// <summary>
    /// 주간 직원 현황 구역 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 직원 현황 구역</returns>
    public SettlementSectionViewData CreateEmployeeSection (
        WeeklySettlementData data )
    {
        return CreateSection (
            SettlementSectionType.Employee ,
            CreateSlot (
                SettlementIconType.Employee ,
                $"고용 직원: {data.HiredEmployeeCount}명" ) ,
            CreateExpenseSlot (
                "지급 주급" ,
                data.Economy.EmployeeWeeklyExpense ) );
    }

    /// <summary>
    /// 가계부 다음 주 변화 구역 생성
    /// </summary>
    /// <param name="adjustment">해당 주차에 확정된 다음 주 보정</param>
    /// <returns>가계부 다음 주 변화 구역</returns>
    public SettlementSectionViewData CreateLedgerNextWeekSection (
        WeeklyOrderAdjustment adjustment )
    {
        string adjustmentText =
            adjustment.IsApplied
                ? GetCountCorrectionText (
                    adjustment.OrderCountCorrection )
                : "변화 없음";

        return CreateSection (
            SettlementSectionType.NextWeek ,
            CreateSlot ( SettlementIconType.NextWeek , $"일일 주문 수 보정: {adjustmentText}" ) );
    }

    /// <summary>
    /// 다음 주 변화 구역 생성
    /// </summary>
    /// <param name="adjustment">다음 주 주문 생성 보정</param>
    /// <param name="nextDayReduction">다음 영업일 주문 감소량</param>
    /// <param name="minCount">최소 예상 주문 수</param>
    /// <param name="maxCount">최대 예상 주문 수</param>
    /// <returns>다음 주 변화 구역</returns>
    public SettlementSectionViewData CreateNextWeekSection (
        WeeklyOrderAdjustment adjustment ,
        int nextDayReduction ,
        int minCount , int maxCount )
    {
        string adjustmentText =
            adjustment.IsApplied
                ? GetCountCorrectionText (
                    adjustment.OrderCountCorrection )
                : "변화 없음";

        string reductionText =
            nextDayReduction > 0
                ? $"-{nextDayReduction}"
                : "없음";

        return CreateSection (
            SettlementSectionType.NextWeek ,
            CreateSlot (
                SettlementIconType.NextWeek ,
                $"일일 주문 수 보정: {adjustmentText}" ) ,
            CreateSlot (
                SettlementIconType.NextWeek ,
                $"단기 주문 감소량: {reductionText}" ) ,
            CreateSlot (
                SettlementIconType.NextWeek ,
                $"예상 주문량: 하루 {minCount}~{maxCount}개" ) );
    }

    /// <summary>
    /// 결산 구역 표시 데이터 생성
    /// </summary>
    /// <param name="sectionType">결산 구역 종류</param>
    /// <param name="slots">구역에 표시할 슬롯</param>
    /// <returns>결산 구역 표시 데이터</returns>
    SettlementSectionViewData CreateSection (
        SettlementSectionType sectionType ,
        params SettlementSlotViewData [ ] slots )
    {
        SettlementSectionViewData section =
            new SettlementSectionViewData
            {
                SectionType = sectionType
            };

        for ( int i = 0 ; i < slots.Length ; i++ )
            section.Slots.Add ( slots [ i ] );

        return section;
    }

    /// <summary>
    /// 기본 결산 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="iconType">결산 아이콘 종류</param>
    /// <param name="description">슬롯 묘사 문구</param>
    /// <returns>기본 결산 슬롯 표시 데이터</returns>
    SettlementSlotViewData CreateSlot (
        SettlementIconType iconType ,
        string description )
    {
        return new SettlementSlotViewData
        {
            IconType = iconType ,
            Description = description ,
            TextTone = SettlementTextTone.Default
        };
    }

    /// <summary>
    /// 진행 바 결산 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="iconType">결산 아이콘 종류</param>
    /// <param name="description">슬롯 묘사 문구</param>
    /// <param name="currentValue">현재값</param>
    /// <param name="maxValue">최대값</param>
    /// <returns>진행 바 결산 슬롯 표시 데이터</returns>
    SettlementSlotViewData CreateProgressSlot (
        SettlementIconType iconType ,
        string description ,
        float currentValue ,
        float maxValue )
    {
        return new SettlementSlotViewData
        {
            IconType = iconType ,
            Description = description ,
            TextTone = SettlementTextTone.Default ,
            ShowProgress = true ,
            Progress = maxValue > 0f
                ? currentValue / maxValue
                : 0f
        };
    }

    /// <summary>
    /// 수입 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="label">수입 항목 이름</param>
    /// <param name="value">수입 금액</param>
    /// <returns>수입 슬롯 표시 데이터</returns>
    SettlementSlotViewData CreateIncomeSlot (
        string label , float value )
    {
        return new SettlementSlotViewData
        {
            IconType = SettlementIconType.Income ,
            Description =
                $"{label}: {GetIncomeText ( value )}" ,
            TextTone = SettlementTextTone.Income
        };
    }

    /// <summary>
    /// 지출 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="label">지출 항목 이름</param>
    /// <param name="value">지출 금액</param>
    /// <returns>지출 슬롯 표시 데이터</returns>
    SettlementSlotViewData CreateExpenseSlot (
        string label , float value )
    {
        return new SettlementSlotViewData
        {
            IconType = SettlementIconType.Expense ,
            Description =
                $"{label}: {GetExpenseText ( value )}" ,
            TextTone = SettlementTextTone.Expense
        };
    }

    /// <summary>
    /// 순이익 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="label">순이익 항목 이름</param>
    /// <param name="value">순이익 금액</param>
    /// <returns>순이익 슬롯 표시 데이터</returns>
    SettlementSlotViewData CreateProfitSlot (
        string label , float value )
    {
        SettlementTextTone textTone =
            value < 0f
                ? SettlementTextTone.Expense
                : SettlementTextTone.Income;

        return new SettlementSlotViewData
        {
            IconType = SettlementIconType.Profit ,
            Description =
                $"{label}: {GetSignedMoney ( value )}" ,
            TextTone = textTone
        };
    }

    #endregion

    #region ----- 공용 표시 문구 -----

    /// <summary>
    /// 상품 상세 금액 합계 계산
    /// </summary>
    /// <param name="details">상품별 상세 목록</param>
    /// <returns>상품 상세 금액 합계</returns>
    float GetItemPriceTotal (
        IReadOnlyList<ItemSettlement> details )
    {
        float total = 0f;

        for ( int i = 0 ; i < details.Count ; i++ )
            total += details [ i ].PriceTotal;

        return total;
    }

    /// <summary>
    /// 영업 종료 사유 문구 반환
    /// </summary>
    /// <param name="reason">영업 종료 사유</param>
    /// <returns>영업 종료 사유 문구</returns>
    string GetEndReasonText ( DayEndReason reason )
    {
        switch ( reason )
        {
            case DayEndReason.CraftLimitReached:
                return "제작 할당량 달성";

            case DayEndReason.NoAvailableOrders:
                return "처리 가능 주문 소진";

            default:
                return "영업 종료";
        }
    }

    /// <summary>
    /// 수입 금액 문구 반환
    /// </summary>
    /// <param name="value">수입 금액</param>
    /// <returns>수입 금액 문구</returns>
    string GetIncomeText ( float value )
    {
        return value > 0f
            ? $"+{value:N0}G"
            : "0G";
    }

    /// <summary>
    /// 지출 금액 문구 반환
    /// </summary>
    /// <param name="value">지출 금액</param>
    /// <returns>지출 금액 문구</returns>
    string GetExpenseText ( float value )
    {
        return value > 0f
            ? $"-{value:N0}G"
            : "0G";
    }

    /// <summary>
    /// 증감 금액 문구 반환
    /// </summary>
    /// <param name="value">증감 금액</param>
    /// <returns>증감 금액 문구</returns>
    string GetSignedMoney ( float value )
    {
        if ( value > 0f ) return $"+{value:N0}G";
        if ( value < 0f ) return $"-{Math.Abs ( value ):N0}G";

        return "0G";
    }

    /// <summary>
    /// 주간 영업 평가 문구 반환
    /// </summary>
    /// <param name="rating">주간 영업 평가</param>
    /// <returns>주간 영업 평가 문구</returns>
    string GetWeeklyRatingText ( WeeklyRating rating )
    {
        switch ( rating )
        {
            case WeeklyRating.Best:
                return "최상";

            case WeeklyRating.Good:
                return "우수";

            case WeeklyRating.Normal:
                return "보통";

            case WeeklyRating.Caution:
                return "주의";

            case WeeklyRating.Danger:
                return "위험";

            default:
                return "자료 부족";
        }
    }

    /// <summary>
    /// 주문 수 보정 문구 반환
    /// </summary>
    /// <param name="correction">주문 수 보정값</param>
    /// <returns>주문 수 보정 문구</returns>
    string GetCountCorrectionText ( int correction )
    {
        if ( correction > 0 ) return $"+{correction}";
        if ( correction < 0 ) return correction.ToString ( );

        return "변화 없음";
    }

    #endregion

}
