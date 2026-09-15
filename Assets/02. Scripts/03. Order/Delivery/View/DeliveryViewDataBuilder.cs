using System.Collections.Generic;
using System.Text;

/// <summary>
/// 배송 상태와 판정 결과를 배송 표시 데이터로 변환
/// </summary>
public class DeliveryViewDataBuilder
{
    PlayStateModel _playStateModel;       //날짜 표시용 플레이 상태
    EmployeeModel _employeeModel;       //배송 담당 직원 조회용 모델

    /// <summary>
    /// 배송 표시 데이터 생성기 생성
    /// </summary>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="employeeModel">직원 상태 모델</param>
    public DeliveryViewDataBuilder (
        PlayStateModel playStateModel , EmployeeModel employeeModel )
    {
        _playStateModel = playStateModel;
        _employeeModel = employeeModel;
    }

    /// <summary>
    /// 배송 표시 데이터 생성
    /// </summary>
    /// <param name="order">배송 주문</param>
    /// <param name="reviewViewData">제작 판정 표시 데이터</param>
    /// <param name="deliveryResult">배송 결과</param>
    /// <param name="isCompleted">배송 완료 여부</param>
    /// <returns>배송 표시 데이터</returns>
    public DeliveryViewData Create (
        CustomerOrder order ,
        CraftReviewViewData reviewViewData ,
        DeliveryResult deliveryResult ,
        bool isCompleted )
    {
        _playStateModel.GetDate (
            order.DeliveryDue , out int deliveryMonth , out int deliveryDay );

        string resultPrefix = isCompleted
            ? "배송 결과"
            : "예상 배송 결과";
        string gradePrefix = isCompleted
            ? "최종 등급"
            : "예상 등급";
        string rewardPrefix = isCompleted
            ? "최종 주문 대금"
            : "예상 주문 대금";

        _playStateModel.GetDate (
            deliveryResult.ArrivalTotalDay ,
            out int arrivalMonth , out int arrivalDay );

        bool showArrivalDate = isCompleted ||
            _playStateModel.IsUnlocked (
                InformationUnlockId.DeliveryEstimatedArrival );

        string arrivalDateText = string.Empty;

        if ( showArrivalDate )
        {
            string arrivalDatePrefix = isCompleted
                ? "도착일"
                : "예상 도착일";

            arrivalDateText =
                $"{arrivalDatePrefix}: {arrivalMonth}월 {arrivalDay}일\n";
        }

        string deliveryInfo =
            arrivalDateText +
            $"납품 마감일: {deliveryMonth}월 {deliveryDay}일\n" +
            $"{resultPrefix}: {GetOutcomeText ( deliveryResult.Outcome )}\n" +
            $"제작 점수: {deliveryResult.CraftScore:0} / " +
            $"{deliveryResult.StandardScore:0} " +
            $"({deliveryResult.AchievementRate:P0})\n" +
            $"{gradePrefix}: {deliveryResult.Grade}";

        if ( deliveryResult.IsMethodConfirmed )
            deliveryInfo += $"\n{CreateDeliveryMethodText ( deliveryResult )}";

        string themeText = CreateReviewText ( reviewViewData.ThemeResults );
        string themeInfo = themeText == "없음"
            ? "활성 테마: 없음"
            : $"활성 테마:\n{themeText}";

        string craftResultInfo =
            $"{GetConditionSummary ( "주요 요구 사항" , reviewViewData.RequirementResults )}\n" +
            $"{GetConditionSummary ( "희망 사항" , reviewViewData.WishResults )}\n" +
            themeInfo;

        string vipCorrection = order.SpecialType == OrderSpecialType.Vip
            ? $"VIP 보정: {GetRateText ( deliveryResult.VipRate )}"
            : "VIP 보정: 없음";
        string requirementCorrection = AreConditionsCompleted (
            reviewViewData.RequirementResults ) == false
            ? $"주요 요구 미달성 상한: {deliveryResult.RequirementRewardLimit:N0}G\n"
            : string.Empty;

        string rewardInfo =
            $"기본 주문 대금: {deliveryResult.BaseOrderReward:N0}G\n" +
            $"난이도 보정: {GetRateText ( deliveryResult.DifficultyRate )}\n" +
            $"등급 보정: {GetRateText ( deliveryResult.GradeRate )}\n" +
            $"배송 보정: {GetRateText ( deliveryResult.DeliveryRate )}\n" +
            $"{vipCorrection}\n" + requirementCorrection +
            $"{rewardPrefix}: {deliveryResult.FinalOrderReward:N0}G";

        int requirementCompletedCount = GetCompletedConditionCount(
            reviewViewData.RequirementResults );
        int wishCompletedCount = GetCompletedConditionCount(
            reviewViewData.WishResults );
        string specialCorrectionText = order.SpecialType == OrderSpecialType.Vip
            ? $"특수 주문 보정 난이도 {GetRateText( deliveryResult.DifficultyRate )} / " +
                $"VIP {GetRateText( deliveryResult.VipRate )}"
            : $"특수 주문 보정 난이도 " +
                GetRateText( deliveryResult.DifficultyRate );

        if ( AreConditionsCompleted( reviewViewData.RequirementResults ) == false )
        {
            specialCorrectionText +=
                $" / 보상 상한 {deliveryResult.RequirementRewardLimit:N0}G";
        }

        return new DeliveryViewData
        {
            OrderTitle = $"\" {order.Title} \"" ,
            CraftCost = $"(제작 코스트: {order.MaxCraftCost})" ,
            SpecialOrder = order.IsSpecial
                ? "[ 특수 주문 ]"
                : "[ 일반 주문 ]" ,
            SpecialCondition = reviewViewData.SpecialResults.Count > 0
                ? CreateReviewText( reviewViewData.SpecialResults )
                : "특수 조건 없음",
            CraftScoreText =
                $"{deliveryResult.CraftScore:0} / {deliveryResult.StandardScore:0}",
            ExpectedGradeText = deliveryResult.Grade.ToString( ),
            DeliveryResultText = GetOutcomeText( deliveryResult.Outcome ),
            RequirementCompletedCount = requirementCompletedCount,
            RequirementTotalCount = reviewViewData.RequirementResults.Count,
            WishCompletedCount = wishCompletedCount,
            WishTotalCount = reviewViewData.WishResults.Count,
            ActiveThemes = CreateActiveThemeViewDatas(
                reviewViewData.ThemeSlots ),
            BaseRewardText = $"{deliveryResult.BaseOrderReward:N0}G",
            GradeCorrectionText =
                $"등급 보정 {GetRateText( deliveryResult.GradeRate )}",
            DeliveryCorrectionText =
                $"배송 보정 {GetRateText( deliveryResult.DeliveryRate )}",
            SpecialCorrectionText = specialCorrectionText,
            ExpectedRewardText = $"{deliveryResult.FinalOrderReward:N0}G",

            DeliveryTitle = GetDeliveryTitle (
                deliveryResult.Outcome , isCompleted ) ,
            DeliveryInfo = deliveryInfo ,
            CraftResultInfo = craftResultInfo ,
            RewardInfo = rewardInfo
        };
    }

    /// <summary>
    /// 제작 조건 달성 수 반환
    /// </summary>
    /// <param name="conditions">제작 조건 표시 목록</param>
    /// <returns>달성한 조건 수</returns>
    int GetCompletedConditionCount (
        IReadOnlyList<CraftConditionViewData> conditions )
    {
        int completedCount = 0;

        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( conditions [ i ].State == CraftConditionState.Completed )
                completedCount++;
        }

        return completedCount;
    }

    /// <summary>
    /// 배송 제작 상세의 활성 테마 슬롯 데이터 생성
    /// </summary>
    /// <param name="themes">제작 테마 슬롯 데이터</param>
    /// <returns>활성 테마 슬롯 데이터</returns>
    IReadOnlyList<OrderInfoSlotViewData> CreateActiveThemeViewDatas (
        IReadOnlyList<CraftThemeSlotViewData> themes )
    {
        var viewDatas = new OrderInfoSlotViewData [ themes.Count ];

        for ( int i = 0; i < themes.Count; i++ )
        {
            CraftThemeSlotViewData theme = themes [ i ];
            viewDatas [ i ] = new OrderInfoSlotViewData(
                theme.Icon,
                theme.ThemeName,
                theme.IsCompleted ? "완성" : "활성" );
        }

        return viewDatas;
    }

    /// <summary>
    /// 배송 방식 툴팁 문구 생성
    /// </summary>
    /// <param name="method">배송 방식</param>
    /// <param name="result">배송 방식 사용 가능 결과</param>
    /// <param name="directDeliveryCost">직접 배송 고정 비용</param>
    /// <returns>배송 방식 툴팁 문구</returns>
    public string CreateMethodTooltip (
        DeliveryMethod method , DeliveryProcessResult result ,
        float directDeliveryCost )
    {
        if ( method == DeliveryMethod.Direct )
        {
            string slotText = result == DeliveryProcessResult.DirectDeliveryInUse
                ? "배송 슬롯: 1 / 1"
                : "배송 슬롯: 0 / 1";

            string tooltip =
                $"직접 배송\n" +
                $"배송비: {directDeliveryCost:N0}G\n" +
                slotText;

            if ( result == DeliveryProcessResult.Success )
            {
                return tooltip + "\n배송 직원 없이 출발합니다.";
            }

            if ( result == DeliveryProcessResult.DirectDeliveryInUse )
            {
                return tooltip +
                    "\n직접 배송 중입니다." +
                    "\n배송이 완료될 때까지 추가 배송이 불가능합니다.";
            }

            return tooltip + $"\n{CreateFailureText ( result )}";
        }

        if ( result != DeliveryProcessResult.Success )
            return $"직원 배송\n{CreateFailureText ( result )}";

        return "직원 배송\n" +
            "배치 가능한 배송 직원을 자동으로 배정합니다.\n" +
            "건당 배송비가 없습니다.";
    }

    /// <summary>
    /// 배송 실패 안내 문구 생성
    /// </summary>
    /// <param name="result">배송 처리 결과</param>
    /// <returns>배송 실패 안내 문구</returns>
    public string CreateFailureText ( DeliveryProcessResult result )
    {
        switch ( result )
        {
            case DeliveryProcessResult.InvalidSettings:
                return "배송 설정을 확인해 주세요.";

            case DeliveryProcessResult.InvalidOrder:
                return "배송할 수 없는 주문입니다.";

            case DeliveryProcessResult.Expired:
                return "최종 납품 기한이 지났습니다.";

            case DeliveryProcessResult.CraftResultNotFound:
                return "제작 결과를 찾을 수 없습니다.";

            case DeliveryProcessResult.InvalidCraftResult:
                return "제작 결과 정보가 올바르지 않습니다.";

            case DeliveryProcessResult.AlreadyDelivered:
                return "이미 배송한 주문입니다.";

            case DeliveryProcessResult.BudgetUpdateFailed:
                return "주문 대금 지급에 실패했습니다.";

            case DeliveryProcessResult.HighGradeUpdateFailed:
                return "평가 누적 수 변경에 실패했습니다.";

            case DeliveryProcessResult.OrderUpdateFailed:
                return "주문 상태 변경에 실패했습니다.";

            case DeliveryProcessResult.RollbackFailed:
                return "배송 처리 복구에 실패했습니다. 상태를 확인해 주세요.";

            case DeliveryProcessResult.AlreadyShipping:
                return "이미 배송 중인 주문입니다.";

            case DeliveryProcessResult.InvalidMethod:
                return "배송 방식을 확인해 주세요.";

            case DeliveryProcessResult.DirectDeliveryInUse:
                return "직접 배송 중입니다.";

            case DeliveryProcessResult.EmployeeUnavailable:
                return "현재 배치할 수 있는 직원이 없습니다.";

            case DeliveryProcessResult.DeliveryCostUpdateFailed:
                return "배송비가 부족합니다.";

            default:
                return "배송을 완료하지 못했습니다.";
        }
    }

    /// <summary>
    /// 확정 배송 방식 문구 생성
    /// </summary>
    /// <param name="deliveryResult">배송 결과</param>
    /// <returns>확정 배송 방식 문구</returns>
    string CreateDeliveryMethodText ( DeliveryResult deliveryResult )
    {
        if ( deliveryResult.Method == DeliveryMethod.Direct )
        {
            return "배송 방식: 직접 배송\n" +
                $"배송비: {deliveryResult.DeliveryCost:N0}G";
        }

        string employeeName = "확인 불가";

        if ( _employeeModel.GetState (
            deliveryResult.EmployeeId , out EmployeeState employeeState ) )
            employeeName = employeeState.Data.DisplayName;

        return "배송 방식: 직원 배송\n" +
            $"담당 직원: {employeeName}";
    }

    /// <summary>
    /// 제작 조건 달성 수 문구 반환
    /// </summary>
    /// <param name="title">조건 제목</param>
    /// <param name="conditions">제작 조건 표시 목록</param>
    /// <returns>조건 달성 수 문구</returns>
    string GetConditionSummary (
        string title ,
        IReadOnlyList<CraftConditionViewData> conditions )
    {
        int completedCount = 0;

        for ( int i = 0 ; i < conditions.Count ; i++ )
        {
            if ( conditions [ i ].State == CraftConditionState.Completed )
                completedCount++;
        }

        return $"{title}: {completedCount} / {conditions.Count}";
    }

    /// <summary>
    /// 제작 조건 전체 달성 여부 확인
    /// </summary>
    /// <param name="conditions">제작 조건 표시 목록</param>
    /// <returns>전체 달성 여부</returns>
    bool AreConditionsCompleted (
        IReadOnlyList<CraftConditionViewData> conditions )
    {
        for ( int i = 0 ; i < conditions.Count ; i++ )
        {
            if ( conditions [ i ].State != CraftConditionState.Completed )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 제작 판정 문구 목록을 줄바꿈 문자열로 변환
    /// </summary>
    /// <param name="reviewTexts">제작 판정 문구 목록</param>
    /// <returns>제작 판정 문구</returns>
    string CreateReviewText (
        IReadOnlyList<CraftReviewTextData> reviewTexts )
    {
        var text = new StringBuilder ( );

        for ( int i = 0 ; i < reviewTexts.Count ; i++ )
            text.AppendLine ( reviewTexts [ i ].Text );

        return text.ToString ( ).TrimEnd ( );
    }

    /// <summary>
    /// 배송 패널 상태 제목 반환
    /// </summary>
    /// <param name="outcome">배송 결과</param>
    /// <param name="isCompleted">배송 완료 여부</param>
    /// <returns>배송 패널 상태 제목</returns>
    string GetDeliveryTitle (
        OrderOutcome outcome , bool isCompleted )
    {
        if ( isCompleted )
        {
            return outcome == OrderOutcome.NormalDelivery
                ? "[ 정상 배송 완료 ]"
                : "[ 지연 배송 완료 ]";
        }

        return outcome == OrderOutcome.NormalDelivery
            ? "[ 정상 배송 예정 ]"
            : "[ 지연 배송 예정 ]";
    }

    /// <summary>
    /// 배송 결과 문구 반환
    /// </summary>
    /// <param name="outcome">배송 결과</param>
    /// <returns>배송 결과 문구</returns>
    string GetOutcomeText ( OrderOutcome outcome )
    {
        switch ( outcome )
        {
            case OrderOutcome.NormalDelivery:
                return "정상 배송";

            case OrderOutcome.LateDelivery:
                return "지연 배송";

            default:
                return "확인 불가";
        }
    }

    /// <summary>
    /// 보정 배율 문구 반환
    /// </summary>
    /// <param name="rate">보정 배율</param>
    /// <returns>보정 배율 문구</returns>
    string GetRateText ( float rate )
    {
        return $"x{rate:0.##}";
    }
}
