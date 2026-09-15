/// <summary>
/// 정비 업그레이드 구매 처리
/// </summary>
public class MaintenanceUpgradeModel
{
    MaintenanceModel _maintenanceModel;       //정비 단계 상태
    MaintenanceEffectModel _maintenanceEffectModel;       //정비 효과 적용 모델
    PlayStateModel _playStateModel;       //플레이 자금 상태
    EmployeeModel _employeeModel;       //직원 효과 모델

    #region ----- 초기화 -----

    /// <summary>
    /// 정비 상위 단계 구매 모델 생성
    /// </summary>
    /// <param name="maintenanceModel">정비 모델</param>
    /// <param name="maintenanceEffectModel">정비 효과 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="employeeModel">직원 효과 모델</param>
    public MaintenanceUpgradeModel (
        MaintenanceModel maintenanceModel,
        MaintenanceEffectModel maintenanceEffectModel,
        PlayStateModel playStateModel,
        EmployeeModel employeeModel )
    {
        _maintenanceModel = maintenanceModel;
        _maintenanceEffectModel = maintenanceEffectModel;
        _playStateModel = playStateModel;
        _employeeModel = employeeModel;
    }

    #endregion

    #region ----- 구매 검증 -----

    /// <summary>
    /// 직원 효과가 적용된 정비 비용 계산
    /// </summary>
    /// <param name="data">정비 데이터</param>
    /// <param name="baseCost">기본 정비 비용</param>
    /// <returns>직원 효과가 적용된 정비 비용</returns>
    public float GetAdjustedCost (
        MaintenanceData data, float baseCost )
    {
        if ( data == null ) return baseCost;

        EmployeeEffectType effectType;

        switch ( data.Type )
        {
            case MaintenanceType.Facility:
            case MaintenanceType.Convenience:
                effectType = EmployeeEffectType.FacilityMaintCostRate;
                break;

            case MaintenanceType.Research:
                effectType = EmployeeEffectType.ResearchMaintCostRate;
                break;

            default:
                return baseCost;
        }

        return baseCost * _employeeModel.GetEffectValue( effectType );
    }

    /// <summary>
    /// 정비 업그레이드 가능 여부 확인
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <param name="upgradeCost">다음 단계 구매 비용</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult GetUpgradeResult (
        string id, int totalDay, WeeklyRating weeklyRating, out float upgradeCost )
    {
        upgradeCost = 0f;

        //정비 단계와 선행 조건 확인
        MaintenanceResult result =
            _maintenanceModel.GetUpgradeResult( id, totalDay, weeklyRating );

        if ( result != MaintenanceResult.Success ) return result;

        //다음 단계 데이터 조회
        if ( _maintenanceModel.GetNextLevelData(
            id, out MaintenanceLevelData levelData ) == false )
            return MaintenanceResult.InvalidData;

        if ( _maintenanceModel.GetState(
            id, out MaintenanceState state ) == false )
            return MaintenanceResult.InvalidData;

        upgradeCost = GetAdjustedCost(
            state.Data, levelData.UpgradeCost );

        //잘못된 구매 비용 차단
        if ( float.IsNaN( upgradeCost ) ||
            float.IsInfinity( upgradeCost ) ||
            upgradeCost < 0f )
            return MaintenanceResult.InvalidData;

        //현재 자금 확인
        if ( _playStateModel.CanSpendBudget( upgradeCost ) == false )
            return MaintenanceResult.InsufficientBudget;

        return MaintenanceResult.Success;
    }

    #endregion

    #region ----- 상위 단계 구매 -----

    /// <summary>
    /// 정비 상위 단계 구매
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <param name="paidCost">지불한 정비 비용</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult Upgrade (
        string id, int totalDay,
        WeeklyRating weeklyRating,
        out float paidCost )
    {
        //정비 구매 조건 전체 확인
        MaintenanceResult result = GetUpgradeResult(
            id, totalDay, weeklyRating, out paidCost );

        if ( result != MaintenanceResult.Success ) return result;

        //정비 비용 차감
        if ( paidCost > 0f &&
            _playStateModel.SpendBudget( paidCost ) == false )
            return MaintenanceResult.InsufficientBudget;

        //정비 구매 단계 상승
        result = _maintenanceModel.Upgrade( id, totalDay, weeklyRating );

        //결과가 성공이 아니라면
        if ( result != MaintenanceResult.Success )
        {
            //단계 상승 실패 시 정비 비용 복구
            if ( paidCost > 0f &&
                _playStateModel.AddBudget( paidCost ) == false )
                return MaintenanceResult.RollbackFailed;

            paidCost = 0f;
            return result;
        }

        //즉시 적용 정비 효과 처리
        MaintenanceEffectResult effectResult =
            _maintenanceEffectModel.ApplyImmediate( id );

        //결과가 성공이라면
        if ( effectResult == MaintenanceEffectResult.Success )
            return MaintenanceResult.Success;

        //효과 내부 원상 복구 실패 시 추가 상태 변경 차단
        if ( effectResult == MaintenanceEffectResult.RollbackFailed )
            return MaintenanceResult.RollbackFailed;

        //효과 적용 실패 시 구매 단계와 비용 복구
        if ( _maintenanceEffectModel.RollbackImmediate( id ) !=
            MaintenanceEffectResult.Success )
            return MaintenanceResult.RollbackFailed;

        //업그레이드 값 복구 실패 시
        if ( _maintenanceModel.RollbackUpgrade( id ) !=
            MaintenanceResult.Success )
            return MaintenanceResult.RollbackFailed;

        //자금 복구 실패 시
        if ( paidCost > 0f &&
            _playStateModel.AddBudget( paidCost ) == false )
            return MaintenanceResult.RollbackFailed;

        paidCost = 0f;
        return MaintenanceResult.EffectApplyFailed;
    }

    #endregion
}
