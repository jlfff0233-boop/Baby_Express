using System;
using UnityEngine;

/// <summary>
/// 저장 확인 대기 작업
/// </summary>
public enum SavePendingAction
{
    None,       //확인 대기 작업 없음
    Overwrite,      //기존 저장 덮어쓰기
    Load,       //저장 데이터 불러오기
    Delete      //저장 데이터 삭제
}

/// <summary>
/// 저장 및 슬롯 데이터 삭제 중재
/// </summary>
public class SavePresenter : MonoBehaviour
{
    [SerializeField] SaveView _saveView;       //저장 뷰

    SaveManager _saveManager;       //저장 관리자
    PlaySceneSaveHandler _saveHandler;       //플레이 씬 저장 처리기

    int _pendingSlotNumber;       //확인 대기 슬롯 번호
    SavePendingAction _pendingAction;       //확인 대기 작업

    /// <summary>
    /// 저장 화면 닫기 요청 이벤트
    /// </summary>
    public event Action OnCloseRequested;

    /// <summary>
    /// 저장 데이터 불러오기 완료 이벤트
    /// </summary>
    public event Action OnLoaded;

    #region ----- 초기화 -----

    /// <summary>
    /// 저장 프레젠터 초기화
    /// </summary>
    /// <param name="saveManager">저장 관리자</param>
    /// <param name="saveHandler">Play 씬 저장 처리기</param>
    public void Init (
        SaveManager saveManager , PlaySceneSaveHandler saveHandler )
    {
        _saveManager = saveManager;
        _saveHandler = saveHandler;

        _saveView.OnClose += RequestClose;
        _saveView.OnSave += Save;
        _saveView.OnLoad += CheckLoad;
        _saveView.OnDelete += CheckDelete;
        _saveView.OnConfirmed += Confirm;
        _saveView.OnCanceled += Cancel;

        _saveView.HideInstant ( );
    }

    /// <summary>
    /// 저장 Presenter 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        _saveView.OnClose -= RequestClose;
        _saveView.OnSave -= Save;
        _saveView.OnLoad -= CheckLoad;
        _saveView.OnDelete -= CheckDelete;
        _saveView.OnConfirmed -= Confirm;
        _saveView.OnCanceled -= Cancel;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 저장 화면 표시
    /// </summary>
    public void Show ( )
    {
        Refresh ( );
        _saveView.Show ( );
    }

    /// <summary>
    /// 저장 화면 숨김 및 확인 대기 상태 초기화
    /// </summary>
    public void Hide ( )
    {
        _pendingSlotNumber = 0;
        _pendingAction = SavePendingAction.None;

        _saveView.HideConfirm ( );
        _saveView.Hide ( );
    }

    /// <summary>
    /// 저장 화면 즉시 숨김 및 확인 대기 상태 초기화
    /// </summary>
    public void HideInstant ( )
    {
        _pendingSlotNumber = 0;
        _pendingAction = SavePendingAction.None;

        _saveView.HideConfirm ( );
        _saveView.HideInstant ( );
    }

    /// <summary>
    /// 저장 화면 닫기 요청 전달
    /// </summary>
    void RequestClose ( )
    {
        OnCloseRequested?.Invoke ( );
    }

    #endregion

    #region ----- 저장 -----

    /// <summary>
    /// 선택 슬롯 저장 또는 덮어쓰기 확인
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    void Save ( int slotNumber )
    {
        SaveSlotData slotData = GetSlotData ( slotNumber );

        if ( slotData.State == SaveSlotState.Empty )
        {
            WriteSlot ( slotNumber );
            return;
        }

        _pendingSlotNumber = slotNumber;
        _pendingAction = SavePendingAction.Overwrite;

        _saveView.ShowConfirm (
            $"{slotNumber}번 슬롯을 덮어쓰시겠습니까?" );
    }

    /// <summary>
    /// 지정 슬롯에 현재 플레이 상태 저장
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    void WriteSlot ( int slotNumber )
    {
        SaveResult result = _saveHandler.Save ( slotNumber );

        if ( result != SaveResult.Success )
        {
            Debug.LogWarning (
                $"저장 실패: {result}" );
        }

        Refresh ( );
    }

    #endregion

    #region ----- 불러오기 -----

    /// <summary>
    /// 선택 슬롯 불러오기 확인
    /// </summary>
    /// <param name="slotNumber">불러올 슬롯 번호</param>
    void CheckLoad ( int slotNumber )
    {
        SaveSlotData slotData = GetSlotData ( slotNumber );

        if ( slotData.State != SaveSlotState.Valid )
            return;

        _pendingSlotNumber = slotNumber;
        _pendingAction = SavePendingAction.Load;

        _saveView.ShowConfirm (
            $"{slotNumber}번 저장 데이터를 불러오시겠습니까?\n" +
            "저장하지 않은 진행 상황은 사라집니다." );
    }

    /// <summary>
    /// 선택 슬롯의 플레이 상태 불러오기
    /// </summary>
    /// <param name="slotNumber">불러올 슬롯 번호</param>
    void Load ( int slotNumber )
    {
        SaveSlotData slotData = GetSlotData ( slotNumber );

        if ( slotData.State != SaveSlotState.Valid )
            return;

        SaveResult result = _saveHandler.Load ( slotNumber );

        if ( result != SaveResult.Success )
        {
            Debug.LogWarning ( $"불러오기 실패: {result}" );

            Refresh ( );
            return;
        }

        _saveView.Hide ( );
        OnLoaded?.Invoke ( );
    }

    #endregion

    #region ----- 삭제 -----

    /// <summary>
    /// 선택 슬롯 삭제 확인
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    void CheckDelete ( int slotNumber )
    {
        _pendingSlotNumber = slotNumber;
        _pendingAction = SavePendingAction.Delete;

        _saveView.ShowConfirm (
            $"{slotNumber}번 저장 데이터를 삭제하시겠습니까?" );
    }

    /// <summary>
    /// 지정 슬롯 저장 데이터 삭제
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    void Delete ( int slotNumber )
    {
        SaveResult result =
            _saveManager.DeleteSlot ( slotNumber );

        if ( result != SaveResult.Success )
        {
            Debug.LogWarning (
                $"저장 데이터 삭제 실패: {result}" );
        }

        Refresh ( );
    }

    #endregion

    #region ----- 확인/조회 -----

    /// <summary>
    /// 확인 대기 중인 저장 작업 실행
    /// </summary>
    void Confirm ( )
    {
        switch ( _pendingAction )
        {
            case SavePendingAction.Overwrite:       //덮어쓰기
                WriteSlot ( _pendingSlotNumber );
                break;

            case SavePendingAction.Load:        //불러오기
                Load ( _pendingSlotNumber );
                break;

            case SavePendingAction.Delete:      //삭제
                Delete ( _pendingSlotNumber );
                break;
        }

        _pendingSlotNumber = 0;
        _pendingAction = SavePendingAction.None;
    }

    /// <summary>
    /// 확인 대기 작업 취소
    /// </summary>
    void Cancel ( )
    {
        _pendingSlotNumber = 0;
        _pendingAction = SavePendingAction.None;
    }

    /// <summary>
    /// 저장 슬롯 목록 갱신
    /// </summary>
    void Refresh ( )
    {
        _saveView.SetSlots (
            _saveManager.GetSlotDatas ( ) , SaveSlotMode.Save );
    }

    /// <summary>
    /// 지정 번호의 저장 슬롯 데이터 조회
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <returns>저장 슬롯 데이터</returns>
    SaveSlotData GetSlotData ( int slotNumber )
    {
        return _saveManager.GetSlotDatas ( )
            [ slotNumber - 1 ];
    }

    #endregion
}
