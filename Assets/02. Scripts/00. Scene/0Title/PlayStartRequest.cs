/// <summary>
/// 플레이 씬 시작 방식
/// </summary>
public enum PlayStartMode
{
    NewGame,       //새 게임 시작
    Load       //저장 데이터 불러오기
}

/// <summary>
/// 타이틀에서 플레이 씬으로 전달할 시작 요청
/// </summary>
public class PlayStartRequest
{
    /// <summary>
    /// 플레이 씬 시작 방식
    /// </summary>
    public PlayStartMode Mode { get; }

    /// <summary>
    /// 사용할 저장 슬롯 번호
    /// </summary>
    public int SlotNumber { get; }

    /// <summary>
    /// 플레이 씬 시작 요청 생성
    /// </summary>
    /// <param name="mode">플레이 씬 시작 방식</param>
    /// <param name="slotNumber">사용할 저장 슬롯 번호</param>
    public PlayStartRequest (
        PlayStartMode mode, int slotNumber )
    {
        Mode = mode;
        SlotNumber = slotNumber;
    }
}