using System.Collections.Generic;

/// <summary>
/// 직원 복구 상태 - 검증을 마친 주급 지급일과 직원별 상태 보관
/// </summary>
public class EmployeeRestoreState
{
    int _lastPaidTotalDay;       //최근 주급 지급 영업일
    Dictionary<string, EmployeeStateSaveData> _states;       //직원별 복구 상태

    /// <summary>
    /// 최근 주급 지급 영업일
    /// </summary>
    public int LastPaidTotalDay => _lastPaidTotalDay;

    /// <summary>
    /// 직원 복구 상태 생성
    /// </summary>
    /// <param name="lastPaidTotalDay">최근 주급 지급 영업일</param>
    /// <param name="states">직원별 복구 상태</param>
    public EmployeeRestoreState (
        int lastPaidTotalDay,
        Dictionary<string, EmployeeStateSaveData> states )
    {
        _lastPaidTotalDay = lastPaidTotalDay;
        _states = states;
    }

    /// <summary>
    /// 지정 직원 상태 보유 여부 확인
    /// </summary>
    /// <param name="employeeId">직원 아이디</param>
    /// <returns>직원 상태 보유 여부</returns>
    public bool ContainsEmployee ( string employeeId )
    {
        return _states.ContainsKey( employeeId );
    }

    /// <summary>
    /// 복구할 직원별 상태 생성
    /// </summary>
    /// <returns>직원별 복구 상태 복사본</returns>
    public Dictionary<string, EmployeeStateSaveData> CreateStates ()
    {
        return new Dictionary<string, EmployeeStateSaveData>( _states );
    }
}
