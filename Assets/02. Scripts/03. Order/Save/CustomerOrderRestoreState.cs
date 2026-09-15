using System.Collections.Generic;

/// <summary>
/// 고객 주문 복구 상태 - 검증을 마친 주문과 주문량 페널티 상태 보관
/// </summary>
public class CustomerOrderRestoreState
{
    /// <summary>
    /// 복구할 주문 목록
    /// </summary>
    public IReadOnlyList<CustomerOrder> Orders { get; }

    /// <summary>
    /// 복구할 수락 대기 제한
    /// </summary>
    public int WaitingLimit { get; }

    /// <summary>
    /// 복구할 다음 주문 생성 번호
    /// </summary>
    public int NextCreatedNumber { get; }

    /// <summary>
    /// 복구할 주문량 페널티 원인
    /// </summary>
    public OrderPenaltyType PenaltyType { get; }

    /// <summary>
    /// 복구할 일일 주문 감소량
    /// </summary>
    public int OrderReduction { get; }

    /// <summary>
    /// 복구할 페널티 남은 일수
    /// </summary>
    public int PenaltyRemainingDays { get; }

    /// <summary>
    /// 고객 주문 복구 상태 생성
    /// </summary>
    /// <param name="orders">복구할 주문 목록</param>
    /// <param name="waitingLimit">복구할 수락 대기 제한</param>
    /// <param name="nextCreatedNumber">복구할 다음 주문 생성 번호</param>
    /// <param name="penaltyType">복구할 주문량 페널티 원인</param>
    /// <param name="orderReduction">복구할 일일 주문 감소량</param>
    /// <param name="penaltyRemainingDays">복구할 페널티 남은 일수</param>
    public CustomerOrderRestoreState (
        IReadOnlyList<CustomerOrder> orders,
        int waitingLimit,
        int nextCreatedNumber,
        OrderPenaltyType penaltyType,
        int orderReduction,
        int penaltyRemainingDays )
    {
        Orders = orders;
        WaitingLimit = waitingLimit;
        NextCreatedNumber = nextCreatedNumber;
        PenaltyType = penaltyType;
        OrderReduction = orderReduction;
        PenaltyRemainingDays = penaltyRemainingDays;
    }
}
