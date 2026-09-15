using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 저장 매니저 - 현재 저장 슬롯과 저장 파일 접근 관리
/// </summary>
public class SaveManager : MonoBehaviour
{
    public const int SlotCount = 5;       //전체 저장 슬롯 개수

    SaveFileHandler _fileHandler;       //저장 파일 처리기
    HashSet<int> _invalidSlotNumbers;       //심층 검증에 실패한 슬롯 번호
    int _activeSlotNumber;       //현재 사용 중인 슬롯 번호

    /// <summary>
    /// 사용 중 슬롯 번호
    /// </summary>
    public int ActiveSlotNumber => _activeSlotNumber;

    /// <summary>
    /// 선택 슬롯 존재 여부
    /// </summary>
    public bool HasActiveSlot => _activeSlotNumber > 0;

    #region ----- 초기화 -----

    /// <summary>
    /// 저장 경로와 파일 처리기 초기화
    /// </summary>
    public void Init ()
    {
        SavePathProvider pathProvider =
            new SavePathProvider( Application.persistentDataPath );

        _fileHandler = new SaveFileHandler( pathProvider );
        _invalidSlotNumbers = new HashSet<int>( );
        _activeSlotNumber = 0;
    }

    #endregion

    #region ----- 슬롯 관리 -----

    /// <summary>
    /// 전체 저장 슬롯의 표시 정보 반환
    /// </summary>
    /// <returns>전체 저장 슬롯 표시 데이터</returns>
    public IReadOnlyList<SaveSlotData> GetSlotDatas ()
    {
        //슬롯 데이터 리스트 생성
        List<SaveSlotData> slotDatas = new List<SaveSlotData>( SlotCount );

        for ( int slotNumber = 1; slotNumber <= SlotCount; slotNumber++ )
        {
            //심층 검증 실패 슬롯은 동일한 오류 문구로 표시
            if ( _invalidSlotNumbers.Contains( slotNumber ) )
            {
                slotDatas.Add( SaveSlotData.CreateCorrupted(
                    slotNumber, SaveFileHandler.InvalidDataMessage ) );
                continue;
            }

            slotDatas.Add( _fileHandler.GetSlotData( slotNumber ) );
        }

        return slotDatas;
    }

    /// <summary>
    /// 저장 슬롯 선택
    /// </summary>
    /// <param name="slotNumber">선택할 슬롯 번호</param>
    /// <returns>저장 슬롯 선택 결과</returns>
    public SaveResult SelectSlot ( int slotNumber )
    {
        //잘못된 슬롯 번호 차단
        if ( slotNumber < 1 || slotNumber > SlotCount )
            return SaveResult.InvalidSlot;

        //사용 중 슬롯 번호 설정
        _activeSlotNumber = slotNumber;
        return SaveResult.Success;
    }

    /// <summary>
    /// 현재 선택된 슬롯 해제
    /// </summary>
    public void ClearActiveSlot ()
    {
        _activeSlotNumber = 0;
    }

    /// <summary>
    /// 심층 검증에 실패한 슬롯을 현재 실행 중 손상 상태로 기록
    /// </summary>
    /// <param name="slotNumber">검증에 실패한 슬롯 번호</param>
    public void MarkInvalidSlot ( int slotNumber )
    {
        if ( slotNumber < 1 || slotNumber > SlotCount )
            return;

        _invalidSlotNumbers.Add( slotNumber );
    }

    #endregion

    #region ----- 일반 저장 -----

    /// <summary>
    /// 지정한 슬롯의 저장 파일 조회
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <param name="saveFileData">조회한 저장 파일 데이터</param>
    /// <returns>저장 파일 조회 결과</returns>
    public SaveResult ReadSlot (
        int slotNumber, out SaveFileData saveFileData )
    {
        return _fileHandler.Read( slotNumber, out saveFileData );
    }

    /// <summary>
    /// 지정한 슬롯에 저장하고 사용 중 슬롯 지정
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    /// <param name="saveFileData">기록할 저장 파일 데이터</param>
    /// <returns>저장 파일 기록 결과</returns>
    public SaveResult WriteSlot (
        int slotNumber, SaveFileData saveFileData )
    {
        //저장 결과 기록
        SaveResult result = _fileHandler.Write(
            slotNumber, saveFileData );

        if ( result == SaveResult.Success )
        {
            _invalidSlotNumbers.Remove( slotNumber );
            _activeSlotNumber = slotNumber;
        }

        return result;
    }

    /// <summary>
    /// 지정 슬롯의 저장 데이터 삭제
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    /// <returns>저장 파일 삭제 결과</returns>
    public SaveResult DeleteSlot ( int slotNumber )
    {
        //저장 데이터 파일 삭제
        SaveResult result = _fileHandler.Delete( slotNumber );

        //저장 파일 오류 시 주간 파일을 유지
        if ( result != SaveResult.Success &&
            result != SaveResult.NotFound )
        {
            return result;
        }

        if ( _activeSlotNumber == slotNumber )
            _activeSlotNumber = 0;

        _invalidSlotNumbers.Remove( slotNumber );

        //주간 저장 데이터 파일과 복구 파일 삭제
        SaveResult weekResult =
            _fileHandler.DeleteWeekSnapshot( slotNumber );

        if ( weekResult != SaveResult.Success &&
            weekResult != SaveResult.NotFound )
        {
            return weekResult;
        }

        //두 파일이 모두 없던 슬롯은 기존 결과 유지
        if ( result == SaveResult.NotFound &&
            weekResult == SaveResult.NotFound )
        {
            return SaveResult.NotFound;
        }

        return SaveResult.Success;
    }

    #endregion

    #region ----- 주간 복구 저장 -----

    /// <summary>
    /// 현재 슬롯의 주간 복구 파일 조회
    /// </summary>
    /// <param name="saveFileData">조회한 주간 복구 파일 데이터</param>
    /// <returns>주간 복구 파일 조회 결과</returns>
    public SaveResult ReadWeekSnapshot ( out SaveFileData saveFileData )
    {
        //사용 중 슬롯이 없다면
        if ( HasActiveSlot == false )
        {
            saveFileData = null;
            return SaveResult.InvalidSlot;
        }

        //주간 복구 파일 데이터 반환
        return _fileHandler.ReadWeekSnapshot(
            _activeSlotNumber, out saveFileData );
    }

    /// <summary>
    /// 현재 슬롯의 주간 복구 파일 저장
    /// </summary>
    /// <param name="saveFileData">기록할 주간 복구 파일 데이터</param>
    /// <returns>주간 복구 파일 기록 결과</returns>
    public SaveResult WriteWeekSnapshot ( SaveFileData saveFileData )
    {
        //사용 중 슬롯이 없다면
        if ( HasActiveSlot == false )
            return SaveResult.InvalidSlot;

        //주간 복구 파일 저장
        return _fileHandler.WriteWeekSnapshot(
            _activeSlotNumber, saveFileData );
    }

    /// <summary>
    /// 현재 슬롯의 주간 복구 파일 삭제
    /// </summary>
    /// <returns>주간 복구 파일 삭제 결과</returns>
    public SaveResult DeleteWeekSnapshot ()
    {
        if ( HasActiveSlot == false )
            return SaveResult.InvalidSlot;

        return _fileHandler.DeleteWeekSnapshot(
            _activeSlotNumber );
    }

    #endregion
}
