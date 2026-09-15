using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타이틀의 새 게임과 이어하기 슬롯 선택 중재
/// </summary>
public class TitleStartPresenter : MonoBehaviour
{
    /// <summary>
    /// 타이틀 슬롯 확인 대기 작업
    /// </summary>
    enum PendingAction
    {
        None,       //확인 대기 작업 없음
        NewGame,       //기존 슬롯에서 새 게임 시작
        Delete       //저장 데이터 삭제
    }

    [SerializeField] SaveView _saveView;       //타이틀 저장 슬롯 뷰

    SaveManager _saveManager;       //저장 관리자
    SaveSlotMode _currentMode;       //현재 슬롯 화면 사용 목적
    PendingAction _pendingAction;       //확인 대기 작업
    int _pendingSlotNumber;       //확인 대기 슬롯 번호

    /// <summary>
    /// Play 씬 시작 요청 이벤트
    /// </summary>
    public event Action<PlayStartRequest> OnPlayStart;

    #region ----- 초기화 -----

    /// <summary>
    /// 타이틀 게임 시작 프레젠터 초기화
    /// </summary>
    /// <param name="saveManager">저장 관리자</param>
    public void Init ( SaveManager saveManager )
    {
        _saveManager = saveManager;
        _currentMode = SaveSlotMode.NewGame;
        _pendingAction = PendingAction.None;
        _pendingSlotNumber = 0;

        _saveView.HideInstant( );
    }

    /// <summary>
    /// 저장 슬롯 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        _saveView.OnClose += Hide;
        _saveView.OnSave += StartNewGame;
        _saveView.OnLoad += Load;
        _saveView.OnDelete += CheckDelete;
        _saveView.OnConfirmed += Confirm;
        _saveView.OnCanceled += Cancel;
    }

    /// <summary>
    /// 저장 슬롯 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        _saveView.OnClose -= Hide;
        _saveView.OnSave -= StartNewGame;
        _saveView.OnLoad -= Load;
        _saveView.OnDelete -= CheckDelete;
        _saveView.OnConfirmed -= Confirm;
        _saveView.OnCanceled -= Cancel;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 새 게임 슬롯 선택 또는 첫 슬롯 즉시 시작
    /// </summary>
    public void ShowNewGame ()
    {
        //사용 중인 슬롯이 하나도 없으면 1번 슬롯으로 바로 시작
        if ( HasOccupiedSlot( ) == false )
        {
            RequestPlayStart( PlayStartMode.NewGame, 1 );
            return;
        }

        _currentMode = SaveSlotMode.NewGame;
        Refresh( );
        _saveView.Show( );
    }

    /// <summary>
    /// 이어하기 슬롯 선택 화면 표시
    /// </summary>
    public void ShowLoad ()
    {
        _currentMode = SaveSlotMode.Load;
        Refresh( );
        _saveView.Show( );
    }

    /// <summary>
    /// 슬롯 선택 화면 숨기기
    /// </summary>
    public void Hide ()
    {
        ResetPendingAction( );
        _saveView.HideConfirm( );
        _saveView.Hide( );
    }

    /// <summary>
    /// 현재 사용 목적에 맞게 슬롯 목록 갱신
    /// </summary>
    void Refresh ()
    {
        _saveView.SetSlots(
            _saveManager.GetSlotDatas( ),
            _currentMode );
    }

    #endregion

    #region ----- 새 게임/이어하기 -----

    /// <summary>
    /// 선택 슬롯으로 새 게임 시작 또는 덮어쓰기 확인
    /// </summary>
    /// <param name="slotNumber">선택한 슬롯 번호</param>
    void StartNewGame ( int slotNumber )
    {
        if ( _currentMode != SaveSlotMode.NewGame )
            return;

        //세이브 슬롯 데이터 가져오기
        SaveSlotData slotData = GetSlotData( slotNumber );

        if ( slotData.State == SaveSlotState.Empty )
        {
            RequestPlayStart( PlayStartMode.NewGame, slotNumber );
            return;
        }

        _pendingAction = PendingAction.NewGame;
        _pendingSlotNumber = slotNumber;

        _saveView.ShowConfirm(
            $"{slotNumber}번 저장 데이터를 삭제하고 " +
            "새 게임을 시작하시겠습니까?" );
    }

    /// <summary>
    /// 선택 슬롯으로 이어하기
    /// </summary>
    /// <param name="slotNumber">선택한 슬롯 번호</param>
    void Load ( int slotNumber )
    {
        if ( _currentMode != SaveSlotMode.Load )
            return;

        SaveSlotData slotData = GetSlotData( slotNumber );

        if ( slotData.State != SaveSlotState.Valid )
            return;

        RequestPlayStart( PlayStartMode.Load, slotNumber );
    }

    /// <summary>
    /// 플레이 씬 시작 요청
    /// </summary>
    /// <param name="mode">플레이 씬 시작 방식</param>
    /// <param name="slotNumber">사용할 저장 슬롯 번호</param>
    void RequestPlayStart ( PlayStartMode mode, int slotNumber )
    {
        OnPlayStart?.Invoke( new PlayStartRequest( mode, slotNumber ) );
    }

    #endregion

    #region ----- 삭제/확인 -----

    /// <summary>
    /// 선택 슬롯 삭제 확인
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    void CheckDelete ( int slotNumber )
    {
        _pendingAction = PendingAction.Delete;
        _pendingSlotNumber = slotNumber;

        _saveView.ShowConfirm(
            $"{slotNumber}번 저장 데이터를 삭제하시겠습니까?" );
    }

    /// <summary>
    /// 확인 대기 작업 실행
    /// </summary>
    void Confirm ()
    {
        switch ( _pendingAction )
        {
            case PendingAction.NewGame:
                RequestPlayStart(
                    PlayStartMode.NewGame, _pendingSlotNumber );
                break;

            case PendingAction.Delete:
                Delete( _pendingSlotNumber );
                break;
        }

        ResetPendingAction( );
    }

    /// <summary>
    /// 확인 대기 작업 취소
    /// </summary>
    void Cancel ()
    {
        ResetPendingAction( );
    }

    /// <summary>
    /// 선택 슬롯 저장 데이터 삭제
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    void Delete ( int slotNumber )
    {
        SaveResult result =
            _saveManager.DeleteSlot( slotNumber );

        if ( result != SaveResult.Success )
        {
            Debug.LogWarning(
                $"저장 데이터 삭제 실패: {result}" );
        }

        Refresh( );
    }

    /// <summary>
    /// 확인 대기 상태 초기화
    /// </summary>
    void ResetPendingAction ()
    {
        _pendingAction = PendingAction.None;
        _pendingSlotNumber = 0;
    }

    #endregion

    #region ----- 조회 -----

    /// <summary>
    /// 비어 있지 않은 저장 슬롯 존재 여부 조회
    /// </summary>
    /// <returns>정상 또는 손상된 저장 슬롯 존재 여부</returns>
    bool HasOccupiedSlot ()
    {
        IReadOnlyList<SaveSlotData> slotDatas =
            _saveManager.GetSlotDatas( );

        for ( int i = 0; i < slotDatas.Count; i++ )
        {
            if ( slotDatas [ i ].State != SaveSlotState.Empty )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 지정 번호의 저장 슬롯 데이터 조회
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <returns>저장 슬롯 데이터</returns>
    SaveSlotData GetSlotData ( int slotNumber )
    {
        return _saveManager.GetSlotDatas( ) [ slotNumber - 1 ];
    }

    #endregion
}
