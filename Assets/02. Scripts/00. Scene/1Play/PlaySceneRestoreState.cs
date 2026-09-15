/// <summary>
/// 플레이 씬 복구 상태 - 검증을 마친 시스템별 복구 상태 보관
/// </summary>
public class PlaySceneRestoreState
{
    /// <summary>
    /// 고객 주문 복구 상태
    /// </summary>
    public CustomerOrderRestoreState CustomerOrderState { get; }

    /// <summary>
    /// 확정 제작 결과 복구 상태
    /// </summary>
    public CraftCompleteRestoreState CraftCompleteState { get; }

    /// <summary>
    /// 직원 복구 상태
    /// </summary>
    public EmployeeRestoreState EmployeeState { get; }

    /// <summary>
    /// 배송 복구 상태
    /// </summary>
    public DeliveryRestoreState DeliveryState { get; }

    /// <summary>
    /// 일일 기록 복구 상태
    /// </summary>
    public DailyRecordRestoreState DailyRecordState { get; }

    /// <summary>
    /// 주간 결산 복구 상태
    /// </summary>
    public SettlementRestoreState SettlementState { get; }

    /// <summary>
    /// 플레이 씬 복구 상태 생성
    /// </summary>
    /// <param name="customerOrderState">고객 주문 복구 상태</param>
    /// <param name="craftCompleteState">확정 제작 결과 복구 상태</param>
    /// <param name="employeeState">직원 복구 상태</param>
    /// <param name="deliveryState">배송 복구 상태</param>
    /// <param name="dailyRecordState">일일 기록 복구 상태</param>
    /// <param name="settlementState">주간 결산 복구 상태</param>
    public PlaySceneRestoreState (
        CustomerOrderRestoreState customerOrderState,
        CraftCompleteRestoreState craftCompleteState,
        EmployeeRestoreState employeeState,
        DeliveryRestoreState deliveryState,
        DailyRecordRestoreState dailyRecordState,
        SettlementRestoreState settlementState )
    {
        CustomerOrderState = customerOrderState;
        CraftCompleteState = craftCompleteState;
        EmployeeState = employeeState;
        DeliveryState = deliveryState;
        DailyRecordState = dailyRecordState;
        SettlementState = settlementState;
    }
}
