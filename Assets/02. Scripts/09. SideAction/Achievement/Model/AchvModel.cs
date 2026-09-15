using System;
using System.Collections.Generic;

/// <summary>
/// 업적 모델 - 업적 상태와 결산 시점 달성 판정 모델
/// </summary>
public class AchvModel
{
    AchvDataMap _dataMap;       //전체 업적 설정
    AchvProgressResolver _progressResolver = new AchvProgressResolver( );       //진행도 계산기

    /// <summary>
    /// 업적별 런타임 상태(업적 아이디, 업적 상태)
    /// </summary>
    Dictionary<string, AchvState> _states = new Dictionary<string, AchvState>( );

    #region ----- 프로퍼티/이벤트 -----

    /// <summary>
    /// 읽기 전용 업적 상태
    /// </summary>
    public IReadOnlyDictionary<string, AchvState> States => _states;

    /// <summary>
    /// 신규 달성 또는 미수령 보상 알림 존재 여부
    /// </summary>
    public bool HasNotification
    {
        get
        {
            foreach ( AchvState state in _states.Values )
            {
                if ( state.IsNew || state.HasUnclaimedReward )
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 수령하지 않은 업적 보상 존재 여부
    /// </summary>
    public bool HasUnclaimedReward
    {
        get
        {
            foreach ( AchvState state in _states.Values )
            {
                if ( state.HasUnclaimedReward )
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 업적 단계 달성 이벤트(업적 아이디)
    /// </summary>
    public event Action<string> OnReached;

    /// <summary>
    /// 업적 알림 상태 변경 이벤트
    /// </summary>
    public event Action<bool> OnNotificationChanged;

    /// <summary>
    /// 업적 단계 보상 수령 완료 이벤트(업적 아이디)
    /// </summary>
    public event Action<string> OnRewardClaimed;

    /// <summary>
    /// 최종 완료한 업적 개수
    /// </summary>
    public int CompletedCount
    {
        get
        {
            int count = 0;

            foreach ( AchvState state in _states.Values )
            {
                //완료 개수 더하기
                if ( state.IsCompleted == true )
                    count++;
            }

            return count;
        }
    }

    /// <summary>
    /// 등록된 모든 업적의 최종 단계 달성 여부
    /// </summary>
    public bool AreAllCompleted =>
        _states.Count > 0 && CompletedCount == _states.Count;

    #endregion

    #region ----- 초기화 -----

    /// <summary>
    /// 업적 모델 생성
    /// </summary>
    /// <param name="dataMap">전체 업적 설정 데이터 맵</param>
    public AchvModel ( AchvDataMap dataMap )
    {
        _dataMap = dataMap;

        Init( );
    }

    /// <summary>
    /// 업적 설정별 초기 런타임 상태 생성
    /// </summary>
    void Init ()
    {
        _states.Clear( );

        for ( int i = 0; i < _dataMap.Datas.Count; i++ )
        {
            AchvData data = _dataMap.Datas [ i ];

            _states.Add( data.Id, new AchvState( data.Id ) );
        }
    }

    #endregion

    #region ----- 조회/달성 판정 -----

    /// <summary>
    /// 지정 업적 상태 조회
    /// </summary>
    /// <param name="id">업적 아이디</param>
    /// <param name="state">조회한 업적 상태</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetState ( string id, out AchvState state )
    {
        return _states.TryGetValue( id, out state );
    }

    /// <summary>
    /// 지정 업적의 현재 진행도 계산
    /// </summary>
    /// <param name="data">업적 설정 데이터</param>
    /// <param name="context">업적 판정 원본</param>
    /// <returns>현재 누적 진행도</returns>
    public int GetProgress ( AchvData data, AchvEvalContext context )
    {
        return _progressResolver.Resolve( data.ProgressType, context );
    }

    /// <summary>
    /// 현재 기록을 기준으로 모든 업적 달성 단계 판정
    /// </summary>
    /// <param name="context">업적 판정 원본</param>
    /// <returns>새로 달성한 업적 존재 여부</returns>
    public bool Evaluate ( AchvEvalContext context )
    {
        bool prevNotification = HasNotification;
        bool hasReachedAchv = false;

        for ( int i = 0; i < _dataMap.Datas.Count; i++ )
        {
            //업적 데이터 및 상태 가져오기
            AchvData data = _dataMap.Datas [ i ];
            AchvState state = _states [ data.Id ];

            //진행도 계산 및 달성 가능한 마지막 단계 조회
            int progress = GetProgress( data, context );
            int reachedStageIndex = GetReachedStageIndex(
                data, state.ReachedStageIndex, progress );

            //달성 가능한 단계가 이미 달성한 단계 인덱스보다 작거나 같을 때
            if ( reachedStageIndex <= state.ReachedStageIndex )
                continue;

            //완료 여부
            bool isCompleted =
                reachedStageIndex == data.Stages.Count - 1;

            //달성 단계 갱신
            state.ReachStage( reachedStageIndex, isCompleted );
            AdvanceNoRewardStages( data, state );

            hasReachedAchv = true;
            OnReached?.Invoke( data.Id );
        }

        //알림 상태 변경 이벤트 전달
        NotificationChanged( prevNotification );
        return hasReachedAchv;
    }

    /// <summary>
    /// 모든 신규 업적 확인 처리
    /// </summary>
    public void MarkAllViewed ()
    {
        bool prevNotification = HasNotification;

        foreach ( AchvState state in _states.Values )
            state.MarkViewed( );

        NotificationChanged( prevNotification );
    }

    #endregion

    #region ----- 보상 수령 상태 -----

    /// <summary>
    /// 지정 업적의 다음 미수령 단계 조회
    /// </summary>
    /// <param name="id">업적 아이디</param>
    /// <param name="stage">다음 미수령 단계 데이터</param>
    /// <returns>미수령 단계 조회 성공 여부</returns>
    public bool TryGetNextClaimStage ( string id, out AchvStageData stage )
    {
        stage = null;

        //업적 데이터와 런타임 상태 조회
        if ( _dataMap.TryGetData( id, out AchvData data ) == false ||
            _states.TryGetValue( id, out AchvState state ) == false )
            return false;

        //다음 미수령 단계 인덱스 계산
        int stageIndex = state.ClaimedStageIndex + 1;

        //다음 단계가 아직 달성되지 않았거나 단계 목록 범위를 벗어나면 중단
        if ( stageIndex > state.ReachedStageIndex || stageIndex >= data.Stages.Count )
            return false;

        //다음 미수령 단계 설정
        stage = data.Stages [ stageIndex ];
        return HasReward( stage );
    }

    /// <summary>
    /// 지정 업적의 다음 단계 보상 수령 완료 처리
    /// </summary>
    /// <param name="id">업적 아이디</param>
    /// <returns>수령 상태 갱신 성공 여부</returns>
    public bool MarkNextStageClaimed ( string id )
    {
        //업적 데이터와 런타임 상태 조회
        if ( _dataMap.TryGetData( id, out AchvData data ) == false ||
            _states.TryGetValue( id, out AchvState state ) == false )
            return false;

        bool prevNotification = HasNotification;
        int stageIndex = state.ClaimedStageIndex + 1;

        //보상 수령 완료 처리
        if ( state.MarkStageClaimed( stageIndex ) == false )
            return false;

        //연속된 무보상 달성 단계는 별도 입력 없이 통과
        AdvanceNoRewardStages( data, state );

        //보상 수령 및 알림 상태 변경 이벤트 전달
        OnRewardClaimed?.Invoke( id );
        NotificationChanged( prevNotification );
        return true;
    }

    #endregion

    #region ----- 내부 보조 -----

    /// <summary>
    /// 현재 진행도로 달성 가능한 마지막 단계 조회
    /// </summary>
    /// <param name="data">업적 설정 데이터</param>
    /// <param name="currentStageIndex">현재 달성 단계 인덱스</param>
    /// <param name="progress">현재 누적 진행도</param>
    /// <returns>달성 가능한 마지막 단계 인덱스</returns>
    int GetReachedStageIndex (
        AchvData data, int currentStageIndex, int progress )
    {
        //현재 달성 단계 설정
        int reachedStageIndex = currentStageIndex;


        for ( int i = currentStageIndex + 1; i < data.Stages.Count; i++ )
        {
            //진행도가 목표값보다 작으면 중단
            if ( progress < data.Stages [ i ].TargetValue )
                break;

            //단계 인덱스 설정
            reachedStageIndex = i;
        }

        return reachedStageIndex;
    }

    /// <summary>
    /// 실제 보상이 없는 달성 단계를 수령 완료 처리
    /// </summary>
    /// <param name="data">업적 설정 데이터</param>
    /// <param name="state">업적 런타임 상태</param>
    void AdvanceNoRewardStages ( AchvData data, AchvState state )
    {
        int stageIndex = state.ClaimedStageIndex + 1;

        while ( stageIndex <= state.ReachedStageIndex )
        {
            AchvStageData stage = data.Stages [ stageIndex ];

            if ( HasReward( stage ) )
                return;

            state.MarkStageClaimed( stageIndex );
            stageIndex++;
        }
    }

    /// <summary>
    /// 업적 단계의 실제 보상 존재 여부 확인
    /// </summary>
    /// <param name="stage">확인할 업적 단계</param>
    /// <returns>실제 보상 존재 여부</returns>
    bool HasReward ( AchvStageData stage )
    {
        for ( int i = 0; i < stage.Rewards.Count; i++ )
        {
            if ( stage.Rewards [ i ].Type != AchvRewardType.None )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 실제 알림 상태가 달라진 경우 변경 이벤트 전달
    /// </summary>
    /// <param name="previousNotification">변경 전 알림 상태</param>
    void NotificationChanged ( bool previousNotification )
    {
        bool currentNotification = HasNotification;

        if ( previousNotification == currentNotification )
            return;

        OnNotificationChanged?.Invoke( currentNotification );
    }

    #endregion

#if UNITY_EDITOR
    #region ----- 검증 -----

    /// <summary>
    /// 검증을 위해 등록된 모든 업적의 최종 단계 달성 처리
    /// </summary>
    /// <returns>새로 달성 처리한 업적 존재 여부</returns>
    public bool DebugCompleteAll ()
    {
        bool previousNotification = HasNotification;
        bool hasReachedAchv = false;

        for ( int i = 0; i < _dataMap.Datas.Count; i++ )
        {
            AchvData data = _dataMap.Datas [ i ];
            AchvState state = _states [ data.Id ];

            if ( state.IsCompleted == true )
                continue;

            int lastStageIndex = data.Stages.Count - 1;

            if ( state.ReachStage(
                lastStageIndex, true ) == false )
            {
                continue;
            }

            AdvanceNoRewardStages( data, state );
            hasReachedAchv = true;
            OnReached?.Invoke( data.Id );
        }

        NotificationChanged( previousNotification );
        return hasReachedAchv;
    }

    #endregion
#endif

    #region ----- 저장 복구 -----

    /// <summary>
    /// 업적 모델 세이브 데이터 생성
    /// </summary>
    /// <returns>업적 모델 세이브 데이터</returns>
    public AchvSaveData CreateSaveData ()
    {
        return new AchvSaveData( States );
    }

    /// <summary>
    /// 업적 저장 데이터 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveData">복구할 업적 데이터</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore ( AchvSaveData saveData )
    {
        if ( saveData?.States == null )
        {
            return false;
        }

        var ids = new HashSet<string>( );

        foreach ( AchvStateSaveData stateData in saveData.States )
        {
            //저장된 업적만 검증하고 신규 업적은 초기 상태로 유지
            if ( stateData == null ||
                string.IsNullOrWhiteSpace( stateData.Id ) == true ||
                ids.Add( stateData.Id ) == false ||
                _dataMap.TryGetData(
                    stateData.Id, out AchvData data ) == false ||
                TryGetState( stateData.Id, out _ ) == false ||
                IsValidSaveState( stateData, data ) == false )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 저장 데이터로 업적 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 업적 데이터</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore ( AchvSaveData saveData )
    {
        //현재 상태를 변경하기 전에 전체 데이터 검사
        if ( CanRestore( saveData ) == false )
            return false;

        foreach ( AchvStateSaveData stateData in saveData.States )
        {
            TryGetState( stateData.Id, out AchvState state );

            state.Restore(
                stateData.ReachedStageIndex,
                stateData.ClaimedStageIndex,
                stateData.IsCompleted,
                stateData.IsNew );
        }

        return true;
    }

    /// <summary>
    /// 업적 저장 상태의 단계 관계 검증
    /// </summary>
    /// <param name="stateData">검증할 업적 상태</param>
    /// <param name="data">업적 설정 데이터</param>
    /// <returns>유효한 저장 상태 여부</returns>
    bool IsValidSaveState (
        AchvStateSaveData stateData, AchvData data )
    {
        int lastStageIndex = data.Stages.Count - 1;

        if ( stateData.ReachedStageIndex < -1 ||
            stateData.ReachedStageIndex > lastStageIndex ||
            stateData.ClaimedStageIndex < -1 ||
            stateData.ClaimedStageIndex >
            stateData.ReachedStageIndex )
        {
            return false;
        }

        //완료 상태는 실제 최종 단계 달성과 일치해야 함
        bool isCompleted =
            stateData.ReachedStageIndex == lastStageIndex;

        if ( stateData.IsCompleted != isCompleted )
            return false;

        //달성한 단계가 없으면 신규 상태일 수 없음
        if ( stateData.IsNew == true &&
            stateData.ReachedStageIndex < 0 )
        {
            return false;
        }

        return true;
    }

    #endregion
}
