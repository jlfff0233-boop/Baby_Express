using System.IO;

/// <summary>
/// 저장 파일과 주간 복구 파일의 경로 제공
/// </summary>
public class SavePathProvider
{
    public const int MinSlotNumber = 1;       //최소 저장 슬롯 번호
    public const int MaxSlotNumber = 5;       //최대 저장 슬롯 번호

    readonly string _saveDirectory;       //저장 파일 보관 경로

    /// <summary>
    /// 저장 파일 보관 경로
    /// </summary>
    public string SaveDirectory => _saveDirectory;

    /// <summary>
    /// 저장 경로 제공자 생성
    /// </summary>
    /// <param name="persistentDataPath">데이터 저장 경로</param>
    public SavePathProvider ( string persistentDataPath )
    {
        _saveDirectory = Path.Combine( persistentDataPath, "Saves" );
    }

    /// <summary>
    /// 사용할 수 있는 슬롯 번호인지 확인
    /// </summary>
    /// <param name="slotNumber">검사할 슬롯 번호</param>
    /// <returns>사용 가능한 슬롯 번호인지 여부</returns>
    public bool IsValidSlot ( int slotNumber )
    {
        return slotNumber >= MinSlotNumber &&
               slotNumber <= MaxSlotNumber;
    }

    /// <summary>
    /// 일반 저장 파일 경로 반환
    /// </summary>
    /// <param name="slotNumber">저장 슬롯 번호</param>
    /// <returns>일반 저장 파일 경로</returns>
    public string GetSaveFilePath ( int slotNumber )
    {
        return Path.Combine(
            _saveDirectory, $"save_slot_{slotNumber}.json" );
    }

    /// <summary>
    /// 주간 복구 파일 경로 반환
    /// </summary>
    /// <param name="slotNumber">저장 슬롯 번호</param>
    /// <returns>주간 복구 파일 경로</returns>
    public string GetWeekFilePath ( int slotNumber )
    {
        return Path.Combine(
            _saveDirectory, $"week_slot_{slotNumber}.json" );
    }

    /// <summary>
    /// 저장 검증에 사용할 임시 파일 경로 반환
    /// </summary>
    /// <param name="targetPath">원본 저장 파일 경로</param>
    /// <returns>임시 저장 파일 경로</returns>
    public string GetTempFilePath ( string targetPath )
    {
        return $"{targetPath}.tmp";
    }
}
