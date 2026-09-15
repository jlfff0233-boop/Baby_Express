using System.Collections.Generic;

/// <summary>
/// 직원 저장 복구 처리 - 저장 데이터 검증과 직원 복구 상태 생성
/// </summary>
public class EmployeeSaveRestorer
{
    /// <summary>
    /// 직원별 저장 상태 검증
    /// </summary>
    /// <param name="stateData">직원별 저장 상태</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <param name="employeeModel">직원 모델</param>
    /// <returns>직원별 저장 상태 정상 여부</returns>
    bool IsValidState (
        EmployeeStateSaveData stateData,
        int totalDay,
        EmployeeModel employeeModel )
    {
        return stateData != null &&
            string.IsNullOrWhiteSpace(
                stateData.EmployeeId ) == false &&
            employeeModel.GetState(
                stateData.EmployeeId, out _ ) == true &&
            ( stateData.IsAssigned == false ||
                stateData.IsHired == true ) &&
            stateData.UnpaidWorkedDayCount >= 0 &&
            stateData.UnpaidWorkedDayCount <= totalDay &&
            stateData.LastWorkedTotalDay >= 0 &&
            stateData.LastWorkedTotalDay <= totalDay &&
            ( stateData.UnpaidWorkedDayCount == 0 ||
                stateData.LastWorkedTotalDay > 0 );
    }

    /// <summary>
    /// 직원 저장 데이터를 검증하고 복구 상태 생성
    /// </summary>
    /// <param name="saveData">직원 저장 데이터</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <param name="employeeModel">직원 모델</param>
    /// <param name="restoreState">생성한 직원 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        EmployeeSaveData saveData, int totalDay,
        EmployeeModel employeeModel,
        out EmployeeRestoreState restoreState )
    {
        restoreState = null;

        //직원 저장 데이터의 공통 값 검증
        if ( saveData?.States == null ||
            employeeModel == null ||
            totalDay <= 0 ||
            saveData.LastPaidTotalDay < 0 ||
            saveData.LastPaidTotalDay > totalDay )
        {
            return false;
        }

        //검증을 마친 직원별 상태 보관
        var states =
            new Dictionary<string, EmployeeStateSaveData>( );
        int hiredCount = 0;

        for ( int i = 0; i < saveData.States.Count; i++ )
        {
            //저장된 직원만 복구하고 신규 직원은 초기 상태로 유지
            EmployeeStateSaveData stateData =
                saveData.States [ i ];

            //직원 상태를 검증하고 직원 아이디 기준으로 추가
            if ( IsValidState(
                stateData, totalDay, employeeModel ) == false ||
                states.TryAdd(
                    stateData.EmployeeId, stateData ) == false )
            {
                return false;
            }

            if ( stateData.IsHired == true )
                hiredCount++;
        }

        //최대 고용 인원을 초과한 저장 상태 차단
        if ( hiredCount > employeeModel.MaxEmployeeCount )
            return false;

        //검증을 마친 직원 복구 상태 생성
        restoreState = new EmployeeRestoreState(
            saveData.LastPaidTotalDay, states );

        return true;
    }
}
