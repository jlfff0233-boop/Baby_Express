using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 결산 표시 데이터 생성기 - 계산된 결산 결과를 화면 문구로 변환
/// </summary>
public class SettlementViewDataBuilder
{
    #region ----- 가계부 상세 문구 -----

    /// <summary>
    /// 구매 상세 문구 생성
    /// </summary>
    /// <param name="details">상품별 구매 상세</param>
    /// <param name="purchaseExpense">구매 총액</param>
    /// <returns>구매 상세 문구</returns>
    string CreatePurchaseDetail (
        IReadOnlyList<ItemSettlement> details, float purchaseExpense )
    {
        var text = new StringBuilder( );

        text.AppendLine( "[ 구매 상세 ]" );
        AppendItemDetails( text, details, string.Empty );
        text.AppendLine( );
        text.Append( $"구매 총액: {purchaseExpense:N0}G" );

        return text.ToString( );
    }

    /// <summary>
    /// 제작 소비 파츠 상세 문구 생성
    /// </summary>
    /// <param name="details">파츠별 제작 소비 상세</param>
    /// <returns>제작 소비 파츠 상세 문구</returns>
    string CreateUsedPartDetail (
        IReadOnlyList<ItemSettlement> details )
    {
        var text = new StringBuilder( );
        float basePriceTotal = GetItemPriceTotal( details );

        text.AppendLine( "[ 소비 파츠 상세 ]" );
        AppendItemDetails( text, details, "기준가 " );
        text.AppendLine( );
        text.Append( $"소비 파츠 기준가 합계: {basePriceTotal:N0}G" );

        return text.ToString( );
    }

    #endregion

    #region ----- 주간 결산 -----

    /// <summary>
    /// 가계부 주간 목록 표시 데이터 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <param name="displayWeek">선택 연도에서 표시할 주차</param>
    /// <returns>가계부 주간 목록 표시 데이터</returns>
    public WeeklyLedgerSummaryViewData CreateWeeklyLedgerSummary (
        WeeklySettlementData data, int displayWeek )
    {
        return new WeeklyLedgerSummaryViewData(
            data.Week, displayWeek, true,
            GetWeeklyRatingText( data.Rating ),
            GetIncomeText( data.Economy.TotalIncome ),
            GetExpenseText( data.Economy.TotalExpense ),
            GetSignedMoney( data.Economy.NetProfit ) );
    }

    /// <summary>
    /// 미결산 가계부 주간 목록 표시 데이터 생성
    /// </summary>
    /// <param name="week">누적 결산 주차</param>
    /// <param name="displayWeek">선택 연도에서 표시할 주차</param>
    /// <returns>미결산 가계부 주간 목록 표시 데이터</returns>
    public WeeklyLedgerSummaryViewData CreatePendingWeeklyLedgerSummary (
        int week, int displayWeek )
    {
        return new WeeklyLedgerSummaryViewData(
            week, displayWeek, false,
            "-", "-", "-", "-" );
    }

    /// <summary>
    /// 가계부 주간 상세 표시 데이터 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <param name="displayWeek">선택 연도에서 표시할 주차</param>
    /// <param name="startMonth">결산 시작 월</param>
    /// <param name="startDay">결산 시작 일</param>
    /// <param name="endMonth">결산 종료 월</param>
    /// <param name="endDay">결산 종료 일</param>
    /// <returns>가계부 주간 상세 표시 데이터</returns>
    public WeeklySettlementViewData CreateWeeklyLedger (
        WeeklySettlementData data, int displayWeek,
        int startMonth, int startDay, int endMonth, int endDay )
    {
        return new WeeklySettlementViewData
        {
            Title = $"가계부 / {displayWeek}주차",
            Summary = CreateWeeklySummary(
                data, startMonth, startDay, endMonth, endDay ),
            Operation = CreateWeeklyOperation( data ),
            OrderResult = CreateWeeklyOrderResult( data ),

            Economy = CreateWeeklyEconomy( data ),
            Employee = CreateWeeklyEmployee( data ),
            PurchaseDetail = CreatePurchaseDetail(
                data.PurchaseDetails, data.Economy.PurchaseExpense ),
            UsedPartDetail = CreateUsedPartDetail( data.UsedPartDetails ),
            NextWeek = CreateLedgerNextWeek( data.NextWeekAdjustment )
        };
    }

    /// <summary>
    /// 주간 결산 요약 문구 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <param name="startMonth">결산 시작 월</param>
    /// <param name="startDay">결산 시작 일</param>
    /// <param name="endMonth">결산 종료 월</param>
    /// <param name="endDay">결산 종료 일</param>
    /// <returns>주간 결산 요약 문구</returns>
    string CreateWeeklySummary (
        WeeklySettlementData data,
        int startMonth, int startDay, int endMonth, int endDay )
    {
        var text = new StringBuilder( );

        text.AppendLine( "[ 주간 결산 ]" );
        text.AppendLine( $"제{data.Week}주" );
        text.AppendLine(
            $"기간: {startMonth}월 {startDay}일 ~ " +
            $"{endMonth}월 {endDay}일" );
        text.AppendLine( $"주간 시작 자금: {data.StartBudget:N0}G" );
        text.AppendLine(
            $"주간 영업 평가: {GetWeeklyRatingText( data.Rating )}" );
        text.AppendLine(
            $"주간 순이익: {GetSignedMoney( data.Economy.NetProfit )}" );
        text.Append( $"최종 보유 자금: {data.EndBudget:N0}G" );

        return text.ToString( );
    }

    /// <summary>
    /// 주간 운영 현황 문구 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 운영 현황 문구</returns>
    string CreateWeeklyOperation ( WeeklySettlementData data )
    {
        OperationSettlement operation = data.Operation;
        var text = new StringBuilder( );

        text.AppendLine( "[ 운영 현황 ]" );
        text.AppendLine(
            $"제작 완료: {operation.CraftCompletedCount} / " +
            $"{operation.CraftLimit}" );
        text.AppendLine(
            $"취소/폐기 제작물: {operation.DiscardedCraftCount}" );
        text.AppendLine( $"배송 출발: {operation.DeliveryStartedCount}" );
        text.AppendLine( $"도착/납품: {data.Orders.DeliveredCount}" );
        text.AppendLine( $"제작 진행: {operation.ProductionOrderCount}" );
        text.AppendLine( $"배송 대기: {operation.PendingDeliveryCount}" );
        text.AppendLine( $"배송 중: {operation.InDeliveryCount}" );
        text.Append(
            $"빠른 재입고: {operation.QuickRestockCount} / " +
            $"{operation.QuickRestockLimit}" );

        return text.ToString( );
    }

    /// <summary>
    /// 주간 주문 결과와 평가 문구 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 주문 결과와 평가 문구</returns>
    string CreateWeeklyOrderResult ( WeeklySettlementData data )
    {
        OrderSettlement orders = data.Orders;
        EvaluationSettlement evaluation = data.Evaluation;
        float normalRate = orders.DeliveredCount > 0
            ? ( float ) orders.NormalDeliveryCount / orders.DeliveredCount
            : 0f;
        float lateRate = orders.DeliveredCount > 0
            ? ( float ) orders.LateDeliveryCount / orders.DeliveredCount
            : 0f;

        var text = new StringBuilder( );

        text.AppendLine( "[ 주문 결과 ]" );
        text.AppendLine( $"정상 배송: {orders.NormalDeliveryCount}" );
        text.AppendLine( $"지연 배송: {orders.LateDeliveryCount}" );
        text.AppendLine(
            $"정상 / 지연 배송률: {normalRate:P1} / {lateRate:P1}" );
        text.AppendLine( $"직접 거절: {orders.RejectedCount}" );
        text.AppendLine( $"자동 거절: {orders.AutoRejectedCount}" );
        text.AppendLine( $"주문 취소: {orders.CancelledCount}" );
        text.AppendLine( $"최종 실패: {orders.FailedCount}" );
        text.AppendLine( $"평가 대상 주문: {data.EvaluationOrderCount}" );
        text.AppendLine(
            $"평가 자료: {( data.HasEnoughData ? "충족" : "자료 부족" )}" );
        text.AppendLine( );

        text.AppendLine( "[ 주문 평가 ]" );
        text.AppendLine( $"주간 평가 점수: {data.WeeklyScore:0.#}" );
        text.AppendLine(
            $"등급: S {evaluation.SGradeCount} / " +
            $"A {evaluation.AGradeCount} / " +
            $"B {evaluation.BGradeCount} / " +
            $"C {evaluation.CGradeCount} / " +
            $"D {evaluation.DGradeCount}" );
        text.Append(
            $"주요 요구: {evaluation.CompletedRequirementCount} / " +
            $"{evaluation.RequirementCount} 달성 " +
            $"({evaluation.RequirementCompletionRate:P1})" );

        return text.ToString( );
    }

    /// <summary>
    /// 주간 경제 요약 문구 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 경제 요약 문구</returns>
    string CreateWeeklyEconomy ( WeeklySettlementData data )
    {
        var text = new StringBuilder( );
        AppendEconomy( text, data.Economy );
        return text.ToString( );
    }

    /// <summary>
    /// 주간 직원 현황 문구 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <returns>주간 직원 현황 문구</returns>
    string CreateWeeklyEmployee ( WeeklySettlementData data )
    {
        var text = new StringBuilder( );

        text.AppendLine( "[ 직원 현황 ]" );
        text.AppendLine( $"고용 직원: {data.HiredEmployeeCount}명" );
        text.Append(
            $"지급 주급: {GetExpenseText( data.Economy.EmployeeWeeklyExpense )}" );

        return text.ToString( );
    }

    /// <summary>
    /// 과거 주간 주문 변화 문구 생성
    /// </summary>
    /// <param name="adjustment">해당 주차에 확정된 다음 주 보정</param>
    /// <returns>과거 주간 주문 변화 문구</returns>
    string CreateLedgerNextWeek ( WeeklyOrderAdjustment adjustment )
    {
        var text = new StringBuilder( );

        text.AppendLine( "[ 다음 주 변화 ]" );

        if ( adjustment.IsApplied )
        {
            text.Append(
                $"일일 주문 수: " +
                $"{GetCountCorrectionText( adjustment.OrderCountCorrection )}" );
        }
        else
        {
            text.Append( "주간 보정: 변화 없음" );
        }

        return text.ToString( );
    }

    #endregion

    #region ----- 상세 문구 -----

    /// <summary>
    /// 경제 요약 문구 추가
    /// </summary>
    /// <param name="text">문구 생성기</param>
    /// <param name="economy">경제 결산 결과</param>
    void AppendEconomy (
        StringBuilder text, EconomySettlement economy )
    {
        text.AppendLine( "[ 수입 ]" );
        text.AppendLine(
            $"주문 대금: {GetIncomeText( economy.OrderRewardIncome )}" );
        text.AppendLine(
            $"인벤토리 판매: " +
            $"{GetIncomeText( economy.InventorySaleIncome )}" );
        text.AppendLine( $"기타 수입: {GetIncomeText( economy.OtherIncome )}" );
        text.AppendLine( $"총수입: {GetIncomeText( economy.TotalIncome )}" );
        text.AppendLine( );

        text.AppendLine( "[ 지출 ]" );
        text.AppendLine(
            $"상점 구매: {GetExpenseText( economy.PurchaseExpense )}" );
        text.AppendLine(
            $"빠른 재입고: {GetExpenseText( economy.QuickRestockExpense )}" );
        text.AppendLine(
            $"직접 배송비: {GetExpenseText( economy.DeliveryExpense )}" );
        text.AppendLine(
            $"직원 고용비: {GetExpenseText( economy.EmployeeHireExpense )}" );
        text.AppendLine(
            $"직원 주급: {GetExpenseText( economy.EmployeeWeeklyExpense )}" );
        text.AppendLine( $"기타 지출: {GetExpenseText( economy.OtherExpense )}" );
        text.AppendLine( $"총지출: {GetExpenseText( economy.TotalExpense )}" );
        text.AppendLine( );

        text.AppendLine( "[ 순이익 ]" );
        text.Append( GetSignedMoney( economy.NetProfit ) );
    }

    /// <summary>
    /// 상품별 상세 문구 추가
    /// </summary>
    /// <param name="text">문구 생성기</param>
    /// <param name="details">상품별 상세 목록</param>
    /// <param name="pricePrefix">금액 앞에 표시할 문구</param>
    void AppendItemDetails (
        StringBuilder text,
        IReadOnlyList<ItemSettlement> details,
        string pricePrefix )
    {
        if ( details.Count == 0 )
        {
            text.AppendLine( "없음" );
            return;
        }

        for ( int i = 0; i < details.Count; i++ )
        {
            ItemSettlement detail = details [ i ];

            text.AppendLine(
                $"{detail.Data.Name} x{detail.Quantity}: " +
                $"{pricePrefix}{detail.PriceTotal:N0}G" );
        }
    }

    /// <summary>
    /// 상품 상세 금액 합계 계산
    /// </summary>
    /// <param name="details">상품별 상세 목록</param>
    /// <returns>상품 상세 금액 합계</returns>
    float GetItemPriceTotal ( IReadOnlyList<ItemSettlement> details )
    {
        float total = 0f;

        for ( int i = 0; i < details.Count; i++ )
            total += details [ i ].PriceTotal;

        return total;
    }

    #endregion

    #region ----- 공통 문구 -----

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
        if ( value < 0f ) return $"-{Math.Abs( value ):N0}G";

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
        if ( correction < 0 ) return correction.ToString( );

        return "변화 없음";
    }

    #endregion
}
