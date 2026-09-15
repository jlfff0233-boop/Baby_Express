using System.Collections.Generic;

/// <summary>
/// 일일 기록 복구 상태 - 검증을 마친 현재 영업일과 과거 일일 기록 보관
/// </summary>
public class DailyRecordRestoreState
{
    DailyRecord _currentRecord;       //현재 영업일 기록
    List<DailyRecord> _pastRecords;       //완료된 과거 일일 기록

    /// <summary>
    /// 현재 영업일 기록
    /// </summary>
    public DailyRecord CurrentRecord => _currentRecord;

    /// <summary>
    /// 일일 기록 복구 상태 생성
    /// </summary>
    /// <param name="currentRecord">현재 영업일 기록</param>
    /// <param name="pastRecords">완료된 과거 일일 기록</param>
    public DailyRecordRestoreState (
        DailyRecord currentRecord, List<DailyRecord> pastRecords )
    {
        _currentRecord = currentRecord;
        _pastRecords = pastRecords;
    }

    /// <summary>
    /// 복구할 과거 일일 기록 생성
    /// </summary>
    /// <returns>과거 일일 기록 복사본</returns>
    public List<DailyRecord> CreatePastRecords ()
    {
        return new List<DailyRecord>( _pastRecords );
    }
}
