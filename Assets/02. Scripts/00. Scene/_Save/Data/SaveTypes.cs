
/// <summary>
/// 저장 슬롯의 현재 상태
/// </summary>
public enum SaveSlotState
{
    Empty,      //저장 데이터 없음
    Valid,      //정상 데이터
    Corrupted       //손상된 데이터
}

/// <summary>
/// 저장 슬롯 사용 목적
/// </summary>
public enum SaveSlotMode
{
    NewGame,        //새 게임 시작
    Load,       //타이틀 저장 데이터 불러오기
    Save        //플레이 중 저장/불러오기 관리
}

/// <summary>
/// 저장 파일 작업 결과
/// </summary>
public enum SaveResult
{
    Success,        //작업 성공
    InvalidSlot,        //잘못된 슬롯 번호
    NotFound,       //저장 파일 없음
    InvalidData,        //복구할 수 없는 저장 데이터
    ReadFailed,     //파일 읽기 실패
    WriteFailed,        //파일 쓰기 실패
    DeleteFailed        //파일 삭제 실패
}
