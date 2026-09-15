using System.Collections.Generic;

/// <summary>
/// 주간 결산 복구 상태 - 검증을 마친 과거 주간 결산 목록 보관
/// </summary>
public class SettlementRestoreState
{
    List<WeeklySettlementData> _weeklySettlements;       //과거 주간 결산 목록
    bool _hasNotification;       //새 주간 결산 알림 여부

    /// <summary>
    /// 새 주간 결산 알림 여부
    /// </summary>
    public bool HasNotification => _hasNotification;

    /// <summary>
    /// 주간 결산 복구 상태 생성
    /// </summary>
    /// <param name="weeklySettlements">과거 주간 결산 목록</param>
    /// <param name="hasNotification">새 주간 결산 알림 여부</param>
    public SettlementRestoreState (
        List<WeeklySettlementData> weeklySettlements,
        bool hasNotification )
    {
        _weeklySettlements = weeklySettlements;
        _hasNotification = hasNotification;
    }

    /// <summary>
    /// 복구할 과거 주간 결산 목록 생성
    /// </summary>
    /// <returns>과거 주간 결산 목록 복사본</returns>
    public List<WeeklySettlementData> CreateWeeklySettlements ()
    {
        return new List<WeeklySettlementData>( _weeklySettlements );
    }
}
