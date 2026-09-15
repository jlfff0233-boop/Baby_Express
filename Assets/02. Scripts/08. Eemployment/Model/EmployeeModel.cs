using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직원 처리 결과
/// </summary>
public enum EmployeeResult
{
    Success,        //처리 성공
    InvalidData,        //잘못된 직원 데이터
    NotFound,       //직원 항목 없음

    Locked,     //해금 조건 미달성
    AlreadyHired,       //이미 고용된 직원
    NotHired,       //고용되지 않은 직원

    MaxEmployeeCount,       //전체 최대 고용 수 도달
    AssignedEmployee,       //배송 배치 중인 직원

    InsufficientBudget,     //자금 부족
    RollbackFailed,     //실패 후 원상 복구 실패
    NotAssigned,       //배송에 배치되지 않은 직원

    WeeklyWageAlreadyPaid,       //현재 주급 지급일에 이미 지급함
}


/// <summary>
/// 직원 모델 - 직원 설정과 고용, 배치 상태 관리
/// </summary>
public class EmployeeModel
{
    Dictionary<string, EmployeeState> _states =
        new Dictionary<string, EmployeeState>( );       //직원별 런타임 상태

    PlayStateModel _playStateModel;       //플레이 자금 상태

    int _maxEmployeeCount;       //전체 최대 고용 수
    int _lastPaidTotalDay;       //최근 주급 지급일(누적 영업일 기준)

    /// <summary>
    /// 전체 직원 상태 목록
    /// </summary>
    public IReadOnlyCollection<EmployeeState> States => _states.Values;
    /// <summary>
    /// 최근 주급 지급일(누적 영업일 기준)
    /// </summary>
    public int LastPaidTotalDay => _lastPaidTotalDay;

    #region ----- 프로퍼티 -----

    /// <summary>
    /// 전체 최대 고용 수
    /// </summary>
    public int MaxEmployeeCount => _maxEmployeeCount;

    /// <summary>
    /// 현재 전체 고용 수
    /// </summary>
    public int TotalHireCount
    {
        get
        {
            int count = 0;

            foreach ( EmployeeState state in _states.Values )
            {
                if ( state.IsHired ) count++;
            }

            return count;
        }
    }

    /// <summary>
    /// 현재 전체 주급
    /// </summary>
    public float TotalWeeklyWage
    {
        get
        {
            float wage = 0f;

            foreach ( EmployeeState state in _states.Values )
                wage += state.TotalWeeklyWage;

            return wage * GetEffectValue(
                EmployeeEffectType.WeeklySalaryRate );
        }
    }

    /// <summary>
    /// 현재 전체 배송 슬롯 증가량
    /// </summary>
    public int TotalDeliverySlotIncrease
    {
        get
        {
            float increase = GetEffectValue(
                EmployeeEffectType.DeliverySlotBonus );

            foreach ( EmployeeState state in _states.Values )
            {
                if ( state.Data.UsesExpandedSettings == false )
                    increase += state.TotalDeliverySlotIncrease;
            }

            return Mathf.RoundToInt( increase );
        }
    }

    #endregion

    /// <summary>
    /// 직원 상태 변경 이벤트(직원 아이디)
    /// </summary>
    public event Action<string> OnEmployeeChanged;

    /// <summary>
    /// 직원 고용비 지출 이벤트(고용비)
    /// </summary>
    public event Action<float> OnHireCostPaid;

    /// <summary>
    /// 직원 고용 성공 이벤트
    /// </summary>
    public event Action OnEmployeeHired;

    /// <summary>
    /// 직원 주급 지출 이벤트(지급 주급)
    /// </summary>
    public event Action<float> OnWeeklyWagePaid;

    #region ----- 초기화 -----

    /// <summary>
    /// 직원 모델 생성
    /// </summary>
    /// <param name="datas">직원 설정 데이터 목록</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="maxEmployeeCount">전체 최대 고용 수</param>
    public EmployeeModel (
        IReadOnlyList<EmployeeData> datas, PlayStateModel playStateModel,
        int maxEmployeeCount )
    {
        _playStateModel = playStateModel;
        _maxEmployeeCount = maxEmployeeCount;

        Init( datas );
    }

    /// <summary>
    /// 직원 상태 초기화
    /// </summary>
    /// <param name="datas">직원 설정 데이터 목록</param>
    void Init ( IReadOnlyList<EmployeeData> datas )
    {
        _states.Clear( );

        for ( int i = 0; i < datas.Count; i++ )
        {
            EmployeeData data = datas [ i ];

            //게임 규칙 계산에 필요한 설정값만 검증
            if ( IsValidData( data ) == false )
            {
                Debug.LogWarning( "잘못된 직원 설정 데이터입니다." );
                continue;
            }

            if ( _states.TryAdd(
                data.Id, new EmployeeState( data ) ) == false )
            {
                Debug.LogWarning(
                    $"중복된 직원 아이디입니다: {data.Id}" );
            }
        }
    }

    #endregion

    #region ----- 효과 조회 -----

    /// <summary>
    /// 현재 고용 직원의 지정 효과 합계 또는 배율 조회
    /// </summary>
    /// <param name="effectType">조회할 직원 효과 종류</param>
    /// <returns>보너스 효과는 합계, 비율 효과는 누적 배율</returns>
    public float GetEffectValue ( EmployeeEffectType effectType )
    {
        bool isRate = IsRateEffect( effectType );
        float value = isRate ? 1f : 0f;

        foreach ( EmployeeState state in _states.Values )
        {
            EmployeeData data = state.Data;

            if ( state.IsHired == false ||
                data.UsesExpandedSettings == false ||
                data.EffectType != effectType )
                continue;

            value = isRate
                ? value * data.EffectValue
                : value + data.EffectValue;
        }

        return value;
    }

    /// <summary>
    /// 현재 고용 직원이 지정 효과를 보유하는지 확인
    /// </summary>
    /// <param name="effectType">확인할 직원 효과 종류</param>
    /// <returns>지정 효과 보유 여부</returns>
    public bool HasEffect ( EmployeeEffectType effectType )
    {
        foreach ( EmployeeState state in _states.Values )
        {
            if ( state.IsHired &&
                state.Data.UsesExpandedSettings &&
                state.Data.EffectType == effectType )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 비율형 직원 효과 여부 확인
    /// </summary>
    /// <param name="effectType">확인할 직원 효과 종류</param>
    /// <returns>비율형 효과 여부</returns>
    bool IsRateEffect ( EmployeeEffectType effectType )
    {
        return effectType == EmployeeEffectType.QuickRestockCostRate ||
            effectType == EmployeeEffectType.InventorySaleRate ||
            effectType == EmployeeEffectType.FacilityMaintCostRate ||
            effectType == EmployeeEffectType.ResearchMaintCostRate ||
            effectType == EmployeeEffectType.WeeklySalaryRate;
    }

    #endregion

    #region ----- 고용/해고 -----

    /// <summary>
    /// 직원 고용
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <param name="weeklySettlementCount">완료된 주간 결산 수</param>
    /// <param name="paidCost">지불한 고용비</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult Hire (
        string id, int weeklySettlementCount, out float paidCost )
    {
        //고용 결과 및 고용비 가져오기
        EmployeeResult result = GetHireResult(
            id, weeklySettlementCount, out paidCost );

        if ( result != EmployeeResult.Success ) return result;

        EmployeeState state = _states [ id ];

        //고용비 차감 후 고용 상태 변경
        if ( paidCost > 0f &&
            _playStateModel.SpendBudget( paidCost ) == false )
            return EmployeeResult.InsufficientBudget;

        if ( state.Hire( ) == false )
        {
            //고용 실패 시 차감한 비용 복구
            if ( paidCost > 0f &&
                _playStateModel.AddBudget( paidCost ) == false )
                return EmployeeResult.RollbackFailed;

            paidCost = 0f;
            return EmployeeResult.AlreadyHired;
        }

        //고용일도 고용 일수에 포함
        state.RecordWorkedDay( _playStateModel.TotalDay );

        //실제 지출한 고용비만 일일 기록으로 전달
        if ( paidCost > 0f )
            OnHireCostPaid?.Invoke( paidCost );

        OnEmployeeHired?.Invoke( );
        OnEmployeeChanged?.Invoke( id );
        return EmployeeResult.Success;
    }

    /// <summary>
    /// 배치되지 않은 직원 해고
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult Fire ( string id )
    {
        EmployeeResult result = GetFireResult( id );

        if ( result != EmployeeResult.Success ) return result;

        //직원 상태 가져오기
        EmployeeState state = _states [ id ];

        if ( state.Fire( ) == false )
            return EmployeeResult.AssignedEmployee;

        //해고한 영업일도 고용 일수에 포함
        state.RecordWorkedDay( _playStateModel.TotalDay );

        //해고 시 고용비와 지급한 주급은 반환하지 않음
        OnEmployeeChanged?.Invoke( id );
        return EmployeeResult.Success;
    }

    #endregion

    #region ----- 주급 -----

    /// <summary>
    /// 현재 고용 중인 직원의 오늘 고용 일수 기록
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    public void RecordCurrentEmployeesWorkDay ( int totalDay )
    {
        if ( totalDay <= 0 ) return;

        foreach ( EmployeeState state in _states.Values )
        {
            if ( state.IsHired )
                state.RecordWorkedDay( totalDay );
        }
    }

    /// <summary>
    /// 현재까지 발생한 미지급 비례 주급 계산
    /// </summary>
    /// <returns>1G 단위로 반올림한 전체 주급</returns>
    public float CalculateUnpaidWeeklyWage ()
    {
        float totalWage = 0f;

        foreach ( EmployeeState state in _states.Values )
        {
            if ( state.UnpaidWorkedDayCount <= 0 ) continue;

            totalWage += state.Data.WeeklyWage *
                state.UnpaidWorkedDayCount / 7f;
        }

        //전체 주급 배율 적용 후 합계를 한 번만 반올림
        totalWage *= GetEffectValue(
            EmployeeEffectType.WeeklySalaryRate );
        return Mathf.Round( totalWage );
    }

    /// <summary>
    /// 주간 결산일의 비례 주급 일괄 지급
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyWage">계산된 전체 주급</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult PayWeeklyWage ( int totalDay, out float weeklyWage )
    {
        weeklyWage = 0f;

        if ( totalDay <= 0 || totalDay % 7 != 0 )
            return EmployeeResult.InvalidData;

        if ( _lastPaidTotalDay == totalDay )
            return EmployeeResult.WeeklyWageAlreadyPaid;

        weeklyWage = CalculateUnpaidWeeklyWage( );

        //전체 주급을 지급할 수 없으면 미지급 일수를 그대로 유지
        if ( weeklyWage > 0f &&
            _playStateModel.CanSpendBudget( weeklyWage ) == false )
            return EmployeeResult.InsufficientBudget;

        if ( weeklyWage > 0f &&
            _playStateModel.SpendBudget( weeklyWage ) == false )
            return EmployeeResult.InsufficientBudget;

        _lastPaidTotalDay = totalDay;

        //전체 금액 지급 성공 후에만 미지급 일수 초기화
        foreach ( EmployeeState state in _states.Values )
            state.ClearUnpaidWorkedDays( );

        if ( weeklyWage > 0f )
            OnWeeklyWagePaid?.Invoke( weeklyWage );

        return EmployeeResult.Success;
    }

    #endregion

    #region ----- 배송 배치 -----

    /// <summary>
    /// 배송 직원 배치
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult Assign ( string id )
    {
        EmployeeResult result = GetAssignResult( id );

        if ( result != EmployeeResult.Success ) return result;

        if ( _states [ id ].Assign( ) == false )
            return EmployeeResult.AssignedEmployee;

        OnEmployeeChanged?.Invoke( id );
        return EmployeeResult.Success;
    }

    /// <summary>
    /// 배송 직원 배치 해제
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult Release ( string id )
    {
        if ( GetState( id, out EmployeeState state ) == false )
            return EmployeeResult.NotFound;

        if ( state.IsAssigned == false )
            return EmployeeResult.NotAssigned;

        if ( state.Release( ) == false )
            return EmployeeResult.NotAssigned;

        OnEmployeeChanged?.Invoke( id );
        return EmployeeResult.Success;
    }

    /// <summary>
    /// 배송 직원 배치 가능 여부 확인
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult GetAssignResult ( string id )
    {
        if ( GetState( id, out EmployeeState state ) == false )
            return EmployeeResult.NotFound;

        if ( state.Data.JobType != EmployeeJobType.Delivery )
            return EmployeeResult.InvalidData;

        if ( state.IsHired == false )
            return EmployeeResult.NotHired;

        if ( state.IsAssigned )
            return EmployeeResult.AssignedEmployee;

        return EmployeeResult.Success;
    }

    #endregion

    #region ----- 처리 검증 -----

    /// <summary>
    /// 직원 고용 가능 여부 확인
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <param name="weeklySettlementCount">완료된 주간 결산 수</param>
    /// <param name="hireCost">직원 고용비</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult GetHireResult (
        string id, int weeklySettlementCount, out float hireCost )
    {
        hireCost = 0f;

        if ( weeklySettlementCount < 0 )
            return EmployeeResult.InvalidData;

        if ( GetState( id, out EmployeeState state ) == false )
            return EmployeeResult.NotFound;

        if ( IsUnlocked( state, weeklySettlementCount ) == false )
            return EmployeeResult.Locked;

        if ( state.IsHired )
            return EmployeeResult.AlreadyHired;

        if ( _maxEmployeeCount <= 0 )
            return EmployeeResult.InvalidData;

        if ( TotalHireCount >= _maxEmployeeCount )
            return EmployeeResult.MaxEmployeeCount;

        hireCost = state.Data.HireCost;

        if ( _playStateModel.CanSpendBudget( hireCost ) == false )
            return EmployeeResult.InsufficientBudget;

        return EmployeeResult.Success;
    }

    /// <summary>
    /// 직원 해고 가능 여부 확인
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <returns>직원 처리 결과</returns>
    public EmployeeResult GetFireResult ( string id )
    {
        if ( GetState( id, out EmployeeState state ) == false )
            return EmployeeResult.NotFound;

        if ( state.IsHired == false )
            return EmployeeResult.NotHired;

        if ( state.IsAssigned )
            return EmployeeResult.AssignedEmployee;

        return EmployeeResult.Success;
    }

    #endregion

    #region ----- 직원 조회 -----

    /// <summary>
    /// 배치 가능한 직원 조회
    /// </summary>
    /// <param name="jobType">직원 직종</param>
    /// <param name="state">조회한 직원 상태</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetAvailableEmployee (
        EmployeeJobType jobType, out EmployeeState state )
    {
        state = null;

        foreach ( EmployeeState employeeState in _states.Values )
        {
            if ( employeeState.Data.JobType == jobType &&
                employeeState.IsAvailable )
            {
                state = employeeState;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 직원 상태 조회
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <param name="state">조회한 직원 상태</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetState ( string id, out EmployeeState state )
    {
        state = null;

        if ( string.IsNullOrWhiteSpace( id ) ) return false;

        return _states.TryGetValue( id, out state );
    }

    /// <summary>
    /// 직원 해금 여부 확인
    /// </summary>
    /// <param name="id">직원 아이디</param>
    /// <param name="weeklySettlementCount">완료된 주간 결산 수</param>
    /// <returns>해금 여부</returns>
    public bool IsUnlocked ( string id, int weeklySettlementCount )
    {
        return GetState( id, out EmployeeState state ) &&
            IsUnlocked( state, weeklySettlementCount );
    }

    /// <summary>
    /// 직종별 배치 가능한 직원 수 조회
    /// </summary>
    /// <param name="jobType">직원 직종</param>
    /// <returns>배치 가능한 직원 수</returns>
    public int GetAvailableCount ( EmployeeJobType jobType )
    {
        int count = 0;

        foreach ( EmployeeState state in _states.Values )
        {
            if ( state.Data.JobType == jobType && state.IsAvailable )
                count++;
        }

        return count;
    }

    /// <summary>
    /// 직원 해금 여부 확인
    /// </summary>
    /// <param name="state">직원 상태</param>
    /// <param name="weeklySettlementCount">완료된 주간 결산 수</param>
    /// <returns>해금 여부</returns>
    bool IsUnlocked (
        EmployeeState state, int weeklySettlementCount )
    {
        EmployeeData data = state.Data;

        //기존 직원 에셋은 이전 주간 결산 조건을 그대로 사용
        if ( data.UsesExpandedSettings == false )
        {
            return weeklySettlementCount >=
                data.RequiredWeeklySettlementCount;
        }

        switch ( data.UnlockType )
        {
            case EmployeeUnlockType.FirstWeeklySettlement:
                return weeklySettlementCount > 0;
            case EmployeeUnlockType.TotalDay:
                return _playStateModel.TotalDay >= data.RequiredDay;
            default:
                return false;
        }
    }

    /// <summary>
    /// 직원 설정 데이터 유효 여부 확인
    /// </summary>
    /// <param name="data">직원 설정 데이터</param>
    /// <returns>유효 여부</returns>
    bool IsValidData ( EmployeeData data )
    {
        if ( data == null ||
            string.IsNullOrWhiteSpace( data.Id ) ||
            data.HireCost < 0f ||
            float.IsNaN( data.HireCost ) ||
            float.IsInfinity( data.HireCost ) ||
            data.WeeklyWage < 0f ||
            float.IsNaN( data.WeeklyWage ) ||
            float.IsInfinity( data.WeeklyWage ) )
            return false;

        if ( data.UsesExpandedSettings )
        {
            return data.RequiredDay >= 0 &&
                data.EffectValue >= 0f &&
                float.IsNaN( data.EffectValue ) == false &&
                float.IsInfinity( data.EffectValue ) == false;
        }

        return
            data.DeliverySlotIncrease >= 0 &&
            data.RequiredWeeklySettlementCount >= 0;
    }

    #endregion

    #region ----- 저장 복구 -----

    /// <summary>
    /// 직원 모델 세이브 데이터 생성
    /// </summary>
    /// <returns>직원 모델 세이브 데이터</returns>
    public EmployeeSaveData CreateSaveData ()
    {
        return new EmployeeSaveData( _lastPaidTotalDay, States );
    }

    /// <summary>
    /// 검증된 직원 상태 복구
    /// </summary>
    /// <param name="restoreState">직원 복구 상태</param>
    public void Restore ( EmployeeRestoreState restoreState )
    {
        Dictionary<string, EmployeeStateSaveData> states =
            restoreState.CreateStates( );

        foreach ( EmployeeStateSaveData stateData in states.Values )
        {
            EmployeeState state = _states [ stateData.EmployeeId ];

            state.Restore(
                stateData.IsHired,
                stateData.IsAssigned,
                stateData.UnpaidWorkedDayCount,
                stateData.LastWorkedTotalDay );
        }

        _lastPaidTotalDay = restoreState.LastPaidTotalDay;
    }

    #endregion
}
