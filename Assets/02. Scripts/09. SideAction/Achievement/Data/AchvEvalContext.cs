using System.Collections.Generic;

/// <summary>
/// 업적 진행도 판정에 사용하는 기존 기록 원본
/// </summary>
public class AchvEvalContext
{
    /// <summary>
    /// 이전에 확정된 일일 기록
    /// </summary>
    public IReadOnlyList<DailyRecord> CompletedRecords { get; }

    /// <summary>
    /// 현재 영업일의 미확정 기록
    /// </summary>
    public DailyRecord CurrentRecord { get; }

    /// <summary>
    /// 완료한 누적 영업일 수
    /// </summary>
    public int CompletedBusinessDayCount { get; }

    /// <summary>
    /// 이미 완료된 주간 결산 수
    /// </summary>
    public int CompletedWeeklySettlementCount { get; }

    /// <summary>
    /// 현재 영업일의 주간 종료 여부
    /// </summary>
    public bool IsWeekEnd { get; }


    /// <summary>
    /// 업적 판정 원본 생성
    /// </summary>
    /// <param name="completedRecords">이전에 확정된 일일 기록</param>
    /// <param name="currentRecord">현재 영업일의 미확정 기록</param>
    /// <param name="completedBusinessDayCount">완료한 누적 영업일 수</param>
    /// <param name="completedWeeklySettlementCount">이미 완료된 주간 결산 수</param>
    /// <param name="isWeekEnd">현재 영업일의 주간 종료 여부</param>
    public AchvEvalContext (
        IReadOnlyList<DailyRecord> completedRecords, DailyRecord currentRecord,
        int completedBusinessDayCount, int completedWeeklySettlementCount,
        bool isWeekEnd )
    {
        CompletedRecords = completedRecords;
        CurrentRecord = currentRecord;
        CompletedBusinessDayCount = completedBusinessDayCount;
        CompletedWeeklySettlementCount = completedWeeklySettlementCount;
        IsWeekEnd = isWeekEnd;
    }
}
