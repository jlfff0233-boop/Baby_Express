using System;

/// <summary>
/// 저장 슬롯 데이터
/// </summary>
public class SaveSlotData
{
    /// <summary>
    /// 슬롯 번호
    /// </summary>
    public int SlotNumber { get; }

    /// <summary>
    /// 슬롯 상태
    /// </summary>
    public SaveSlotState State { get; }

    /// <summary>
    /// 누적 영업일
    /// </summary>
    public int TotalDay { get; }

    /// <summary>
    /// 달성한 업적 개수
    /// </summary>
    public int AchievedAchvCount { get; }

    /// <summary>
    /// 저장 당시 보유 골드
    /// </summary>
    public float Budget { get; }

    /// <summary>
    /// UTC 기준 저장 시각
    /// </summary>
    public DateTime SavedAtUtc { get; }

    /// <summary>
    /// 손상된 슬롯에 표시할 오류 정보
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// 정상 데이터인지 여부
    /// </summary>
    public bool HasData => State == SaveSlotState.Valid;

    /// <summary>
    /// 저장 슬롯 표시 정보 생성
    /// </summary>
    /// <param name="slotNumber">슬롯 번호</param>
    /// <param name="state">슬롯 상태</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <param name="achievedAchvCount">달성한 업적 개수</param>
    /// <param name="budget">저장 당시 보유 골드</param>
    /// <param name="savedAtUtc">UTC 기준 저장 시각</param>
    /// <param name="errorMessage">손상된 슬롯 오류 정보</param>
    SaveSlotData (
        int slotNumber, SaveSlotState state,
        int totalDay, int achievedAchvCount, float budget,
        DateTime savedAtUtc, string errorMessage )
    {
        SlotNumber = slotNumber;
        State = state;
        TotalDay = totalDay;
        AchievedAchvCount = achievedAchvCount;
        Budget = budget;
        SavedAtUtc = savedAtUtc;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 비어 있는 슬롯 정보를 생성
    /// </summary>
    /// <param name="slotNumber">슬롯 번호</param>
    /// <returns>비어 있는 저장 슬롯 데이터</returns>
    public static SaveSlotData CreateEmpty ( int slotNumber )
    {
        return new SaveSlotData(
            slotNumber,
            SaveSlotState.Empty,
            0,
            0,
            0f,
            DateTime.MinValue,
            string.Empty );
    }

    /// <summary>
    /// 정상적인 저장 슬롯 정보 생성
    /// </summary>
    /// <param name="data">저장 파일 기본 정보</param>
    /// <param name="savedAtUtc">UTC 기준 저장 시각</param>
    /// <returns>정상적인 저장 슬롯 데이터</returns>
    public static SaveSlotData CreateValid (
        SaveData data, DateTime savedAtUtc )
    {
        return new SaveSlotData(
            data.SlotNumber,
            SaveSlotState.Valid,
            data.TotalDay,
            data.AchievedAchvCount,
            data.Budget,
            savedAtUtc,
            string.Empty );
    }

    /// <summary>
    /// 손상된 저장 슬롯 정보 생성
    /// </summary>
    /// <param name="slotNumber">슬롯 번호</param>
    /// <param name="errorMessage">손상 원인 표시 문구</param>
    /// <returns>손상된 저장 슬롯 데이터</returns>
    public static SaveSlotData CreateCorrupted (
        int slotNumber, string errorMessage )
    {
        return new SaveSlotData(
            slotNumber,
            SaveSlotState.Corrupted,
            0,
            0,
            0f,
            DateTime.MinValue,
            errorMessage );
    }
}
