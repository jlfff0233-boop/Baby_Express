using System.Collections.Generic;

/// <summary>
/// 직원 상태를 직원 슬롯 표시 데이터로 변환
/// </summary>
public class EmployeeViewDataBuilder
{
    EmployeeModel _employeeModel;       //직원 상태 모델

    /// <summary>
    /// 직원 표시 데이터 생성기 생성
    /// </summary>
    /// <param name="employeeModel">직원 상태 모델</param>
    public EmployeeViewDataBuilder ( EmployeeModel employeeModel )
    {
        _employeeModel = employeeModel;
    }

    /// <summary>
    /// 전체 직원 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="weeklySettlementCount">완료된 주간 결산 수</param>
    /// <returns>직원 슬롯 표시 데이터 목록</returns>
    public IReadOnlyList<EmployeeSlotViewData> CreateViewDatas (
        int weeklySettlementCount )
    {
        var viewDatas = new List<EmployeeSlotViewData>( );

        foreach ( EmployeeState state in _employeeModel.States )
            viewDatas.Add( CreateViewData( state, weeklySettlementCount ) );

        return viewDatas;
    }

    /// <summary>
    /// 직원 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="state">직원 상태</param>
    /// <param name="weeklySettlementCount">완료된 주간 결산 수</param>
    /// <returns>직원 슬롯 표시 데이터</returns>
    EmployeeSlotViewData CreateViewData (
        EmployeeState state, int weeklySettlementCount )
    {
        bool isHired = state.IsHired;

        EmployeeResult result = isHired
            ? _employeeModel.GetFireResult( state.Data.Id )
            : _employeeModel.GetHireResult(
                state.Data.Id, weeklySettlementCount, out _ );

        return new EmployeeSlotViewData(
            state.Data.Id,
            state.Data.HirePortrait,
            state.Data.DisplayName,
            GetJobText( state.Data, isHired ),
            $"고용비: {state.Data.HireCost:N0}\n" +
            $"주급: {state.Data.WeeklyWage:N0}",
            GetGuideText( result, state.Data,
                isHired, state.IsAssigned ),
            isHired ? "해고" : "고용",
            result == EmployeeResult.Success );
    }

    /// <summary>
    /// 직원 직종 표시 문구 반환
    /// </summary>
    /// <param name="data">직원 설정 데이터</param>
    /// <param name="isHired">고용 여부</param>
    /// <returns>직종과 고용 효과 표시 문구</returns>
    string GetJobText ( EmployeeData data, bool isHired )
    {
        string job = data.JobType switch
        {
            EmployeeJobType.Delivery => "배송",
            EmployeeJobType.OperationsRunner => "심부름",
            EmployeeJobType.StockClerk => "재고 관리",
            EmployeeJobType.Agent => "주문 관리",
            EmployeeJobType.Engineer => "시설 관리",
            EmployeeJobType.Researcher => "연구",
            EmployeeJobType.Carrier => "제작 보조",
            EmployeeJobType.Accountant => "회계",
            _ => "알 수 없음"
        };

        string jobText = isHired
            ? $"담당 업무: {job}"
            : $"희망 업무: {job}";

        return $"{jobText}\n{GetEffectText( data )}";
    }

    /// <summary>
    /// 직원 효과 표시 문구 반환
    /// </summary>
    /// <param name="data">직원 설정 데이터</param>
    /// <returns>직원 효과 문구</returns>
    string GetEffectText ( EmployeeData data )
    {
        if ( data.UsesExpandedSettings == false )
            return $"배송 슬롯: +{data.DeliverySlotIncrease}";

        return data.EffectType switch
        {
            EmployeeEffectType.DeliverySlotBonus =>
                $"배송 슬롯: +{data.EffectValue:N0}",
            EmployeeEffectType.QuickRestockCostRate =>
                $"빠른 재입고 비용: {data.EffectValue:P0}",
            EmployeeEffectType.InventorySaleRate =>
                $"인벤토리 판매 가격: {data.EffectValue:P0}",
            EmployeeEffectType.DailyOrderMinimumBonus =>
                $"일일 최소 주문: +{data.EffectValue:N0}",
            EmployeeEffectType.FacilityMaintCostRate =>
                $"시설/편의 정비 비용: {data.EffectValue:P0}",
            EmployeeEffectType.ResearchMaintCostRate =>
                $"연구 정비 비용: {data.EffectValue:P0}",
            EmployeeEffectType.CraftQuotaBonus =>
                $"제작 할당량: +{data.EffectValue:N0}",
            EmployeeEffectType.WeeklySalaryRate =>
                $"전체 주급: {data.EffectValue:P0}",
            _ => "효과 없음"
        };
    }

    /// <summary>
    /// 직원 상태 안내 문구 반환
    /// </summary>
    /// <param name="result">직원 처리 결과</param>
    /// <param name="data">직원 설정 데이터</param>
    /// <param name="isHired">현재 고용 여부</param>
    /// <param name="isAssigned">배송 배치 여부</param>
    /// <returns>상태 안내 문구</returns>
    string GetGuideText (
        EmployeeResult result, EmployeeData data,
        bool isHired, bool isAssigned )
    {
        if ( isAssigned ) return "배송 중";

        if ( result == EmployeeResult.Success )
            return isHired ? "배송 가능" : "고용 가능";

        if ( result == EmployeeResult.Locked )
        {
            if ( data.UsesExpandedSettings == false )
            {
                return $"주간 결산 " +
                    $"{data.RequiredWeeklySettlementCount}회 후 해금";
            }

            return data.UnlockType switch
            {
                EmployeeUnlockType.FirstWeeklySettlement =>
                    "첫 주간 결산 후 해금",
                EmployeeUnlockType.TotalDay =>
                    $"{data.RequiredDay}일차에 해금",
                _ => "해금 조건 미달성"
            };
        }

        return GetResultText( result );
    }

    /// <summary>
    /// 직원 처리 결과 문구 반환
    /// </summary>
    /// <param name="result">직원 처리 결과</param>
    /// <returns>직원 처리 결과 문구</returns>
    public string GetResultText ( EmployeeResult result )
    {
        return result switch
        {
            EmployeeResult.InvalidData =>
                "직원 데이터를 확인해 주세요.",
            EmployeeResult.NotFound =>
                "직원 정보를 찾을 수 없습니다.",
            EmployeeResult.Locked =>
                "직원이 아직 해금되지 않았습니다.",
            EmployeeResult.AlreadyHired =>
                "이미 고용된 직원입니다.",
            EmployeeResult.NotHired =>
                "고용되지 않은 직원입니다.",
            EmployeeResult.MaxEmployeeCount =>
                "더 이상 직원을 고용할 수 없습니다.",
            EmployeeResult.AssignedEmployee =>
                "배송 배치 중에는 해고할 수 없습니다.",
            EmployeeResult.InsufficientBudget =>
                "자금이 부족합니다.",
            EmployeeResult.RollbackFailed =>
                "고용 실패 후 상태 복구에 실패했습니다.",
            _ => "직원 처리를 완료하지 못했습니다."
        };
    }

    /// <summary>
    /// 직원 관리 현황 문구 생성
    /// </summary>
    /// <returns>직원 관리 현황 문구</returns>
    public string CreateManagementSummary ()
    {
        return
            $"고용 직원: {_employeeModel.TotalHireCount} / " +
            $"{_employeeModel.MaxEmployeeCount}명\n" +
            $"배송 슬롯 증가: +{_employeeModel.TotalDeliverySlotIncrease}\n" +
            $"다음 주 예상 주급: {_employeeModel.TotalWeeklyWage:N0}G";
    }
}
