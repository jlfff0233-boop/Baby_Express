using System;
using System.Collections.Generic;

/// <summary>
/// 완료된 일일 기록을 엔딩 총 결산 표시 데이터로 변환
/// </summary>
public class EndingViewDataBuilder
{
    /// <summary>
    /// 엔딩 슬롯 종류
    /// </summary>
    enum EndingSlotType
    {
        Operation,      //운영 결과
        Economy,        //경제 결과
        Order,          //주문 평가
        Craft,          //제작과 배송
        Achievement,        //업적
    }

    /// <summary>
    /// 엔딩 결산 화면 표시 데이터 생성
    /// </summary>
    /// <param name="settlementModel">기존 결산 계산 모델</param>
    /// <param name="records">완료된 전체 일일 기록</param>
    /// <param name="totalBusinessDay">총 운영 영업일</param>
    /// <param name="finalBudget">최종 보유 골드</param>
    /// <param name="hiredEmployeeCount">최종 고용 직원 수</param>
    /// <param name="completedAchvCount">달성한 업적 수</param>
    /// <param name="totalAchvCount">전체 업적 수</param>
    /// <returns>엔딩 총 결산 화면 표시 데이터</returns>
    public EndingViewData Build (
        SettlementModel settlementModel,
        IReadOnlyList<DailyRecord> records,
        int totalBusinessDay,
        float finalBudget,
        int hiredEmployeeCount,
        int completedAchvCount,
        int totalAchvCount )
    {
        var viewData = new EndingViewData
        {
            TotalBusinessDay = totalBusinessDay,
            FinalBudget = finalBudget,
            HiredEmployeeCount = hiredEmployeeCount,
            CompletedAchvCount = completedAchvCount,
            TotalAchvCount = totalAchvCount
        };

        for ( int i = 0; i < records.Count; i++ )
        {
            DailyRecord record = records [ i ];

            DailySettlementData dailyData =
                settlementModel.CreateDaily(
                    record, Array.Empty<CustomerOrder>( ) );

            AddDailySettlement( viewData, dailyData );
            AddEmployeeDeliveries( viewData, record.OrderRecords );
        }

        CreateSlotViewDatas( viewData );

        return viewData;
    }

    /// <summary>
    /// 누적 결과를 엔딩 슬롯 표시 데이터로 변환
    /// </summary>
    /// <param name="viewData">엔딩 총 결산 표시 데이터</param>
    void CreateSlotViewDatas ( EndingViewData viewData )
    {
        viewData.OperationSlot = new EndingSlotViewData(
            "운영 결과",
            GetMedalGrade( EndingSlotType.Operation, viewData ),
            new EndingInfoLineViewData [ ]
            {
                new EndingInfoLineViewData(
                    $"총 운영 기간: {viewData.TotalBusinessDay}일" ),
                new EndingInfoLineViewData(
                    $"최종 보유 골드: {viewData.FinalBudget:N0}G" ),
                new EndingInfoLineViewData(
                    $"최종 고용 직원: {viewData.HiredEmployeeCount}명" ),
            } );

        EndingTextTone profitTone = viewData.NetProfit >= 0f
            ? EndingTextTone.Income
            : EndingTextTone.Expense;

        viewData.EconomySlot = new EndingSlotViewData(
            "경제 결과",
            GetMedalGrade( EndingSlotType.Economy, viewData ),
            new EndingInfoLineViewData [ ]
            {
                new EndingInfoLineViewData(
                    $"누적 총수입: {viewData.TotalIncome:N0}G",
                    EndingTextTone.Income ),
                new EndingInfoLineViewData(
                    $"누적 총지출: {viewData.TotalExpense:N0}G",
                    EndingTextTone.Expense ),
                new EndingInfoLineViewData(
                    $"누적 순이익: {viewData.NetProfit:N0}G",
                    profitTone ),
            } );

        viewData.OrderSlot = new EndingSlotViewData(
            "주문 평가",
            GetMedalGrade( EndingSlotType.Order, viewData ),
            new EndingInfoLineViewData [ ]
            {
                new EndingInfoLineViewData(
                    $"완료 주문: {viewData.CompletedOrderCount}건" ),
                new EndingInfoLineViewData(
                    $"S등급: {viewData.SGradeCount}건" ),
                new EndingInfoLineViewData(
                    $"A등급: {viewData.AGradeCount}건" ),
                new EndingInfoLineViewData(
                    $"B등급: {viewData.BGradeCount}건" ),
                new EndingInfoLineViewData(
                    $"C등급: {viewData.CGradeCount}건" ),
                new EndingInfoLineViewData(
                    $"D등급: {viewData.DGradeCount}건" ),
            } );

        viewData.CraftSlot = new EndingSlotViewData(
            "제작 및 배송",
            GetMedalGrade( EndingSlotType.Craft, viewData ),
            new EndingInfoLineViewData [ ]
            {
                new EndingInfoLineViewData(
                    $"제작 완료: {viewData.CraftCompletedCount}건" ),
                new EndingInfoLineViewData(
                    $"사용 파츠: {viewData.UsedPartCount}개" ),
                new EndingInfoLineViewData(
                    $"직원 배송: {viewData.EmployeeDeliveryCount}건" ),
            } );

        viewData.AchievementSlot = new EndingSlotViewData(
            "업적",
            GetMedalGrade( EndingSlotType.Achievement, viewData ),
            new EndingInfoLineViewData [ ]
            {
                new EndingInfoLineViewData(
                    $"달성 업적: {viewData.CompletedAchvCount} / " +
                    $"{viewData.TotalAchvCount}" ),
            } );
    }

    /// <summary>
    /// 엔딩 항목의 실적 메달 등급 반환
    /// </summary>
    /// <param name="slotType">엔딩 슬롯 종류</param>
    /// <param name="viewData">엔딩 총 결산 표시 데이터</param>
    /// <returns>실적 메달 등급</returns>
    EndingMedalGrade GetMedalGrade (
        EndingSlotType slotType, EndingViewData viewData )
    {
        //세부 평가 기준 확정 전까지 모든 항목을 금메달로 표시
        return EndingMedalGrade.Gold;
    }

    /// <summary>
    /// 일일 결산 결과를 엔딩 결산에 합산
    /// </summary>
    /// <param name="viewData">합산할 엔딩 총 결산 데이터</param>
    /// <param name="dailyData">합산 대상 일일 결산 데이터</param>
    void AddDailySettlement (
        EndingViewData viewData,
        DailySettlementData dailyData )
    {
        viewData.TotalIncome +=
            dailyData.Economy.TotalIncome;      //총 수입
        viewData.TotalExpense +=
            dailyData.Economy.TotalExpense;     //총 지출

        viewData.CompletedOrderCount +=
            dailyData.Orders.DeliveredCount;        //납품 횟수

        viewData.SGradeCount +=
            dailyData.Evaluation.SGradeCount;
        viewData.AGradeCount +=
            dailyData.Evaluation.AGradeCount;
        viewData.BGradeCount +=
            dailyData.Evaluation.BGradeCount;
        viewData.CGradeCount +=
            dailyData.Evaluation.CGradeCount;
        viewData.DGradeCount +=
            dailyData.Evaluation.DGradeCount;

        viewData.CraftCompletedCount +=
            dailyData.Operation.CraftCompletedCount;        //제작 완료 횟수

        //제작에 실제 사용한 파츠 수량 합산
        for ( int i = 0; i < dailyData.UsedPartDetails.Count; i++ )
        {
            viewData.UsedPartCount +=
                dailyData.UsedPartDetails [ i ].Quantity;       //사용 파츠 총 개수
        }
    }

    /// <summary>
    /// 직원 배송 횟수 증가(엔딩 결산용)
    /// </summary>
    /// <param name="viewData">합산할 엔딩 결산 데이터</param>
    /// <param name="orderRecords">일일 종료 주문 기록</param>
    void AddEmployeeDeliveries (
        EndingViewData viewData,
        IReadOnlyList<DailyOrderRecord> orderRecords )
    {
        for ( int i = 0; i < orderRecords.Count; i++ )
        {
            //주문 기록 가져오기
            DailyOrderRecord record = orderRecords [ i ];

            //배송 평가가 있고 직원 배송일 때
            if ( record.HasEvaluation == true &&
                record.DeliveryMethod == DeliveryMethod.Employee )
            {
                //직원 배송 횟수 증가
                viewData.EmployeeDeliveryCount++;
            }
        }
    }
}
