/// <summary>
/// 플레이 씬 시작 핸들러 - 새 게임 저장과 기존 데이터 복구 처리
/// </summary>
public class PlaySceneStartHandler
{
    GameManager _gameManager;       //공용 게임 관리자
    PlaySceneSaveHandler _saveHandler;       //플레이 상태 저장과 복구 처리기
    PlaySceneLoadHandler _loadHandler;       //불러오기 후 화면 복구 처리기

    /// <summary>
    /// 플레이 씬 시작 처리기 생성
    /// </summary>
    /// <param name="gameManager">게임 관리자</param>
    /// <param name="saveHandler">플레이 상태 저장과 복구 처리기</param>
    /// <param name="loadHandler">불러오기 후 화면 복구 처리기</param>
    public PlaySceneStartHandler (
        GameManager gameManager,
        PlaySceneSaveHandler saveHandler,
        PlaySceneLoadHandler loadHandler )
    {
        _gameManager = gameManager;
        _saveHandler = saveHandler;
        _loadHandler = loadHandler;
    }

    /// <summary>
    /// 새 게임 또는 불러오기 실행
    /// </summary>
    /// <returns>플레이 시작 결과</returns>
    public SaveResult ApplyStartRequest ()
    {
        //Play 씬 직접 실행 시 생성된 초기 상태 유지
        if ( _gameManager.HasPlayStartRequest == false )
            return SaveResult.Success;

        PlayStartRequest request =
            _gameManager.TakePlayStartRequest( );

        //이전 플레이에서 사용한 활성 슬롯 해제
        _gameManager.SaveManager.ClearActiveSlot( );

        switch ( request.Mode )
        {
            case PlayStartMode.NewGame:
                return _saveHandler.SaveNewGame( request.SlotNumber );
            case PlayStartMode.Load:
                return Load( request.SlotNumber );

            default:
                return SaveResult.InvalidData;
        }
    }

    /// <summary>
    /// 선택 슬롯의 플레이 상태와 화면 복구
    /// </summary>
    /// <param name="slotNumber">불러올 슬롯 번호</param>
    /// <returns>불러오기 결과</returns>
    SaveResult Load ( int slotNumber )
    {
        SaveResult result = _saveHandler.Load( slotNumber );

        if ( result != SaveResult.Success )
            return result;

        _loadHandler.RefreshAfterLoad( );

        return SaveResult.Success;
    }
}