using System.Collections.Generic;

/// <summary>
/// Play 씬 저장 데이터의 시스템 간 참조 검증
/// </summary>
public class PlaySceneSaveValidator
{
    /// <summary>
    /// 전체 Play 씬 저장 데이터의 참조 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveFileData">검증할 저장 파일 데이터</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore ( SaveFileData saveFileData )
    {
        if ( saveFileData == null )
            return false;

        return CanRestoreEmployeeAssignments(
            saveFileData.DeliveryData,
            saveFileData.EmployeeData );
    }

    /// <summary>
    /// 직원 저장 상태와 배송 배치 상태 일치 여부 확인
    /// </summary>
    /// <param name="deliveryData">배송 세이브 데이터</param>
    /// <param name="employeeData">직원 세이브 데이터</param>
    /// <returns>직원 배송 상태 복구 가능 여부</returns>
    bool CanRestoreEmployeeAssignments (
        DeliverySaveData deliveryData,
        EmployeeSaveData employeeData )
    {
        //데이터 확인
        if ( deliveryData?.Schedules == null ||
            deliveryData.Results == null ||
            employeeData?.States == null )
        {
            return false;
        }

        //직원 상태 세이브 데이터 딕셔너리 가져오기
        if ( TryCreateEmployeeMap(
            employeeData,
            out Dictionary<string, EmployeeStateSaveData> employees ) == false )
        {
            return false;
        }

        var completedOrderIds = new HashSet<string>( );

        //배송 결과 세이브 데이터 순회
        foreach ( DeliveryResultSaveData resultData in deliveryData.Results )
        {
            //완료 주문 아이디 추가
            if ( resultData == null ||
                completedOrderIds.Add( resultData.OrderId ) == false )
            {
                return false;
            }
        }

        var assignedEmployeeIds = new HashSet<string>( );

        //배송 일정 세이브 데이터 순회
        foreach ( DeliveryScheduleSaveData scheduleData in deliveryData.Schedules )
        {
            if ( scheduleData == null )
                return false;

            //출발하지 않았거나 직원 배송이 아니면 현재 직원 배치가 아님
            if ( scheduleData.DepartureTotalDay <= 0 ||
                scheduleData.Method != DeliveryMethod.Employee )
                continue;

            //배송 결과가 있으면 도착 완료 후 직원 배치가 해제된 상태
            if ( completedOrderIds.Contains(
                scheduleData.OrderId ) == true )
                continue;

            //직원 상태 세이브 데이터 가져오기
            if ( employees.TryGetValue(
                scheduleData.EmployeeId,
                out EmployeeStateSaveData employeeState ) == false ||
                employeeState.IsHired == false ||
                assignedEmployeeIds.Add( scheduleData.EmployeeId ) == false )
            {
                return false;
            }
        }

        foreach ( EmployeeStateSaveData employeeState in employeeData.States )
        {
            //배송 중 여부
            bool hasActiveDelivery =
                assignedEmployeeIds.Contains( employeeState.EmployeeId );

            if ( employeeState.IsAssigned != hasActiveDelivery )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 직원 세이브 데이터 맵 생성
    /// </summary>
    /// <param name="saveData">직원 세이브 데이터</param>
    /// <param name="employees">직원 아이디별 세이브 상태</param>
    /// <returns>직원 세이브 데이터 맵 생성 성공 여부</returns>
    bool TryCreateEmployeeMap (
        EmployeeSaveData saveData,
        out Dictionary<string, EmployeeStateSaveData> employees )
    {
        employees =
            new Dictionary<string, EmployeeStateSaveData>( );

        foreach ( EmployeeStateSaveData stateData in saveData.States )
        {
            if ( stateData == null ||
                string.IsNullOrWhiteSpace( stateData.EmployeeId ) == true ||
                employees.TryAdd( stateData.EmployeeId, stateData ) == false )
            {
                employees = null;
                return false;
            }
        }

        return true;
    }
}
