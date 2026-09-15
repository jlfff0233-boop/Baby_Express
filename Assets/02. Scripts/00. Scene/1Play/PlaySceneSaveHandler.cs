using UnityEngine;

/// <summary>
/// 플레이씬 저장 핸들러 - 플레이 씬의 저장 데이터 생성과 기본 상태 복구 처리
/// </summary>
public class PlaySceneSaveHandler
{
    SaveManager _saveManager;       //저장 관리자
    PlayStateModel _playStateModel;       //공용 플레이 모델
    PlaySceneSaveDataBuilder _saveDataBuilder;       //전체 저장 데이터 생성기
    PlaySceneSaveRestorer _saveRestorer;       //전체 저장 검증과 복구 처리기

    /// <summary>
    /// 플레이 씬 저장 핸들러 생성
    /// </summary>
    /// <param name="saveManager">저장 관리자</param>
    /// <param name="playStateModel">공용 플레이 모델</param>
    /// <param name="saveDataBuilder">전체 저장 데이터 생성기</param>
    /// <param name="saveRestorer">전체 저장 검증과 복구 처리기</param>
    public PlaySceneSaveHandler (
        SaveManager saveManager, PlayStateModel playStateModel,
        PlaySceneSaveDataBuilder saveDataBuilder,
        PlaySceneSaveRestorer saveRestorer )
    {
        _saveManager = saveManager;
        _playStateModel = playStateModel;
        _saveDataBuilder = saveDataBuilder;
        _saveRestorer = saveRestorer;
    }

    /// <summary>
    /// 현재 플레이 상태를 지정 슬롯에 저장
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    /// <returns>저장 결과</returns>
    public SaveResult Save ( int slotNumber )
    {
        SaveFileData saveFileData =
            _saveDataBuilder.Create( slotNumber );

        return _saveManager.WriteSlot( slotNumber, saveFileData );
    }

    /// <summary>
    /// 새 게임 상태 저장
    /// </summary>
    /// <param name="slotNumber">사용할 슬롯 번호</param>
    /// <returns>저장 결과</returns>
    public SaveResult SaveNewGame ( int slotNumber )
    {
        SaveFileData saveFileData =
            _saveDataBuilder.Create( slotNumber );

        SaveResult result =
            _saveManager.WriteSlot( slotNumber, saveFileData );

        if ( result != SaveResult.Success )
            return result;

        SaveResult snapshotResult = SaveWeekSnapshot( saveFileData );

        if ( snapshotResult == SaveResult.Success )
            return SaveResult.Success;

        //새 스냅샷 기록 실패 시 이전 플레이의 스냅샷 제거
        SaveResult deleteResult =
            _saveManager.DeleteWeekSnapshot( );

        if ( deleteResult != SaveResult.Success &&
            deleteResult != SaveResult.NotFound )
        {
            Debug.LogWarning(
                $"이전 주간 시작 상태 삭제 실패: {deleteResult}" );
        }

        return snapshotResult;
    }

    /// <summary>
    /// 현재 사용 중인 슬롯에 플레이 상태 자동 저장
    /// </summary>
    public void AutoSave ()
    {
        //활성 슬롯이 없으면 자동 저장하지 않음
        if ( _saveManager.HasActiveSlot == false )
            return;

        int slotNumber = _saveManager.ActiveSlotNumber;

        //저장 파일 데이터 생성
        SaveFileData saveFileData =
            _saveDataBuilder.Create( slotNumber );

        //지정 슬롯에 저장
        SaveResult result =
            _saveManager.WriteSlot( slotNumber, saveFileData );

        if ( result != SaveResult.Success )
        {
            Debug.LogWarning( $"자동 저장 실패: {result}" );
            return;
        }

        //현재 상태 기록
        if ( IsWeekStart( ) == true )
            SaveWeekSnapshot( saveFileData );
    }

    /// <summary>
    /// 현재 상태를 주간 시작 스냅샷으로 저장
    /// </summary>
    /// <param name="saveFileData">저장 데이터</param>
    /// <returns>주간 시작 상태 저장 결과</returns>
    SaveResult SaveWeekSnapshot ( SaveFileData saveFileData )
    {
        //현재 상태 기록
        SaveResult result =
            _saveManager.WriteWeekSnapshot( saveFileData );

        if ( result != SaveResult.Success )
            Debug.LogWarning( $"주간 시작 상태 저장 실패: {result}" );

        return result;
    }

    /// <summary>
    /// 현재 영업일이 새로운 주의 시작인지 확인
    /// </summary>
    /// <returns>1일, 8일, 15일 이후 주간 시작 여부</returns>
    bool IsWeekStart ()
    {
        return
            ( _playStateModel.TotalDay - 1 ) % 7 == 0;
    }

    /// <summary>
    /// 지정 슬롯의 플레이 상태 복구
    /// </summary>
    /// <param name="slotNumber">불러올 슬롯 번호</param>
    /// <returns>불러오기 결과</returns>
    public SaveResult Load ( int slotNumber )
    {
        //저장 데이터 가져오기
        SaveResult result = _saveManager.ReadSlot( slotNumber, out SaveFileData saveFileData );

        //실패 시 결과 반환
        if ( result != SaveResult.Success )
            return result;

        //저장 데이터를 검증하고 복구 상태 생성
        if ( _saveRestorer.TryCreate(
            saveFileData, out PlaySceneRestoreState restoreState ) == false )
        {
            _saveManager.MarkInvalidSlot( slotNumber );
            return SaveResult.InvalidData;
        }

        //저장 데이터 복구
        if ( _saveRestorer.Restore(
            saveFileData, restoreState ) == false )
        {
            _saveManager.MarkInvalidSlot( slotNumber );
            return SaveResult.InvalidData;
        }

        //전체 복구가 끝난 슬롯만 현재 슬롯으로 지정
        result = _saveManager.SelectSlot( slotNumber );

        if ( result != SaveResult.Success )
            return result;

        //성공 결과 반환
        return SaveResult.Success;
    }

    /// <summary>
    /// 현재 주 시작 상태로 복구 가능한지 확인
    /// </summary>
    /// <returns>현재 주 시작 상태 복구 가능 여부</returns>
    public bool CanRestartCurrentWeek ()
    {
        SaveResult result =
            _saveManager.ReadWeekSnapshot( out SaveFileData saveFileData );

        if ( result != SaveResult.Success )
            return false;

        return
            IsCurrentWeekSnapshot( saveFileData ) &&
            _saveRestorer.TryCreate( saveFileData, out _ );
    }

    /// <summary>
    /// 현재 주 시작 상태로 플레이 복구
    /// </summary>
    /// <returns>주간 시작 상태 복구 결과</returns>
    public SaveResult RestartCurrentWeek ()
    {
        SaveResult result =
            _saveManager.ReadWeekSnapshot(
                out SaveFileData saveFileData );

        if ( result != SaveResult.Success )
            return result;

        //현재 주에 해당하지 않는 스냅샷 차단
        if ( IsCurrentWeekSnapshot( saveFileData ) == false )
            return SaveResult.InvalidData;

        //저장 데이터를 검증하고 복구 상태 생성
        if ( _saveRestorer.TryCreate(
            saveFileData, out PlaySceneRestoreState restoreState ) == false )
            return SaveResult.InvalidData;

        int slotNumber = _saveManager.ActiveSlotNumber;

        //런타임 변경 전에 주간 시작 상태를 저장에 반영
        SaveResult saveResult =
            _saveManager.WriteSlot( slotNumber, saveFileData );

        if ( saveResult != SaveResult.Success )
        {
            Debug.LogWarning( $"주간 시작 상태 저장 실패: {saveResult}" );
            return saveResult;
        }

        //저장 완료 후 주간 시작 상태 복구
        if ( _saveRestorer.Restore(
            saveFileData, restoreState ) == false )
            return SaveResult.InvalidData;

        return SaveResult.Success;
    }

    /// <summary>
    /// 저장 데이터가 현재 주의 시작 상태인지 확인
    /// </summary>
    /// <param name="saveFileData">확인할 주간 시작 저장 데이터</param>
    /// <returns>현재 주 시작 상태 여부</returns>
    bool IsCurrentWeekSnapshot ( SaveFileData saveFileData )
    {
        int currentWeekStartDay =
            ( ( _playStateModel.TotalDay - 1 ) / 7 * 7 ) + 1;

        return
            saveFileData.Data.SlotNumber ==
                _saveManager.ActiveSlotNumber &&
            saveFileData.Data.TotalDay == currentWeekStartDay &&
            ( saveFileData.Data.TotalDay - 1 ) % 7 == 0;
    }

}
