/// <summary>
/// 결산 페이지 표시 데이터 생성기 - 일일, 주간 결산 페이지 구성
/// </summary>
public class SettlementPageViewDataBuilder
{
    SettlementSlotViewDataBuilder _slotViewDataBuilder =
        new SettlementSlotViewDataBuilder ( ); //결산 슬롯 표시 데이터 생성기

    /// <summary>
    /// 일일 결산 슬롯 페이지 표시 데이터 생성
    /// </summary>
    /// <param name="data">일일 결산 계산 결과</param>
    /// <param name="month">결산 월</param>
    /// <param name="day">결산 일</param>
    /// <returns>일일 결산 페이지 표시 데이터</returns>
    public SettlementPageViewData CreateDailyPage (
        DailySettlementData data , int month , int day )
    {
        return new SettlementPageViewData
        {
            Title = $"일일 결산 / {month}월 {day}일" ,
            Sections =
            {
                _slotViewDataBuilder.CreateDailySummarySection( data ),
                _slotViewDataBuilder.CreateOperationSection( data.Operation, data.Orders ),
                _slotViewDataBuilder.CreateOrderResultSection( data.Orders ),
                _slotViewDataBuilder.CreateEvaluationSection( data.Evaluation ),
                _slotViewDataBuilder.CreateEconomySection( data.Economy ),
                _slotViewDataBuilder.CreatePurchaseSection(
                    data.PurchaseDetails, data.Economy.PurchaseExpense ),
                _slotViewDataBuilder.CreateUsedPartSection( data.UsedPartDetails ),
                _slotViewDataBuilder.CreateQuickRestockSection( data.QuickRestockDetails )
            }
        };
    }

    /// <summary>
    /// 주간 결산 슬롯 페이지 표시 데이터 생성
    /// </summary>
    /// <param name="data">주간 결산 계산 결과</param>
    /// <param name="startMonth">결산 시작 월</param>
    /// <param name="startDay">결산 시작 일</param>
    /// <param name="endMonth">결산 종료 월</param>
    /// <param name="endDay">결산 종료 일</param>
    /// <param name="nextDayReduction">다음 영업일 주문 감소량</param>
    /// <param name="minCount">최소 예상 주문 수</param>
    /// <param name="maxCount">최대 예상 주문 수</param>
    /// <returns>주간 결산 페이지 표시 데이터</returns>
    public SettlementPageViewData CreateWeeklyPage (
        WeeklySettlementData data ,
        int startMonth , int startDay ,
        int endMonth , int endDay ,
        int nextDayReduction ,
        int minCount , int maxCount )
    {
        return new SettlementPageViewData
        {
            Title = $"주간 결산 / 제{data.Week}주" ,
            Sections =
            {
                _slotViewDataBuilder.CreateWeeklySummarySection(
                    data,
                    startMonth, startDay,
                    endMonth, endDay ),
                _slotViewDataBuilder.CreateOperationSection(
                    data.Operation, data.Orders ),
                _slotViewDataBuilder.CreateWeeklyOrderResultSection( data ),
                _slotViewDataBuilder.CreateWeeklyEvaluationSection( data ),
                _slotViewDataBuilder.CreateEconomySection( data.Economy ),
                _slotViewDataBuilder.CreatePurchaseSection(
                    data.PurchaseDetails, data.Economy.PurchaseExpense ),
                _slotViewDataBuilder.CreateUsedPartSection( data.UsedPartDetails ),
                _slotViewDataBuilder.CreateQuickRestockSection( data.QuickRestockDetails ),
                _slotViewDataBuilder.CreateEmployeeSection( data ),
                _slotViewDataBuilder.CreateNextWeekSection(
                    data.NextWeekAdjustment, nextDayReduction, minCount, maxCount )
            }
        };
    }

    /// <summary>
    /// 가계부 주간 상세 페이지 표시 데이터 생성
    /// </summary>
    /// <param name="data">선택한 주간 결산 기록</param>
    /// <param name="displayWeek">선택 연도에서 표시할 주차</param>
    /// <param name="startMonth">결산 시작 월</param>
    /// <param name="startDay">결산 시작 일</param>
    /// <param name="endMonth">결산 종료 월</param>
    /// <param name="endDay">결산 종료 일</param>
    /// <returns>가계부 주간 상세 페이지 표시 데이터</returns>
    public SettlementPageViewData CreateWeeklyLedgerPage (
        WeeklySettlementData data , int displayWeek ,
        int startMonth , int startDay ,
        int endMonth , int endDay )
    {
        return new SettlementPageViewData
        {
            Title = $"가계부 / {displayWeek}주차" ,
            Sections =
            {
                _slotViewDataBuilder.CreateWeeklySummarySection(
                    data, startMonth, startDay, endMonth, endDay ),
                _slotViewDataBuilder.CreateOperationSection( data.Operation, data.Orders ),
                _slotViewDataBuilder.CreateWeeklyOrderResultSection( data ),
                _slotViewDataBuilder.CreateWeeklyEvaluationSection( data ),
                _slotViewDataBuilder.CreateEconomySection( data.Economy ),
                _slotViewDataBuilder.CreatePurchaseSection(
                    data.PurchaseDetails, data.Economy.PurchaseExpense ),
                _slotViewDataBuilder.CreateUsedPartSection( data.UsedPartDetails ),
                _slotViewDataBuilder.CreateEmployeeSection( data ),
                _slotViewDataBuilder.CreateLedgerNextWeekSection( data.NextWeekAdjustment )
            }
        };
    }
}
