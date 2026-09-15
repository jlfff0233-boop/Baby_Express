using System;
using System.Collections.Generic;

/// <summary>
/// 후속 튜토리얼 가이드 중재 - 중복 실행과 순서 및 완료 상태 관리
/// </summary>
public class TutorialGuideCoordinator
{
    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    Func<bool> _canStartGuide;       //현재 새 가이드 시작 가능 여부
    Action _clearGuideDisplay;       //화살표와 입력 차단 표시 초기화

    List<ITutorialGuideSection> _sections =
        new List<ITutorialGuideSection>( );       //기능별 가이드 프레젠터

    Queue<TutorialGuideId> _queuedGuides =
        new Queue<TutorialGuideId>( );       //실행 대기 가이드

    HashSet<TutorialGuideId> _queuedGuideIds =
        new HashSet<TutorialGuideId>( );       //중복 대기 차단용 아이디

    ITutorialGuideSection _currentSection;       //현재 가이드 담당 프레젠터
    TutorialGuideId? _currentGuideId;       //현재 실행 중인 가이드
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 현재 실행 중인 가이드 여부
    /// </summary>
    public bool HasActiveGuide => _currentGuideId.HasValue;

    /// <summary>
    /// 현재 실행 중인 가이드 아이디
    /// </summary>
    public TutorialGuideId? CurrentGuideId => _currentGuideId;

    /// <summary>
    /// 후속 가이드 활성 상태 변경 이벤트
    /// </summary>
    public event Action<bool> OnActiveChanged;

    /// <summary>
    /// 후속 튜토리얼 가이드 중재기 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="canStartGuide">대화와 다른 가이드 상태를 포함한 시작 가능 여부</param>
    /// <param name="clearGuideDisplay">화살표와 입력 차단 표시 초기화 함수</param>
    public TutorialGuideCoordinator (
        TutorialModel tutorialModel,
        Func<bool> canStartGuide,
        Action clearGuideDisplay )
    {
        _tutorialModel = tutorialModel;
        _canStartGuide = canStartGuide;
        _clearGuideDisplay = clearGuideDisplay;
    }

    #region ----- 기능별 가이드 등록 -----

    /// <summary>
    /// 기능별 가이드 프레젠터 등록
    /// </summary>
    /// <param name="section">등록할 기능별 가이드 프레젠터</param>
    public void RegisterSection ( ITutorialGuideSection section )
    {
        if ( section == null || _sections.Contains( section ) )
            return;

        _sections.Add( section );

        //실행 중 추가된 경우에도 완료 이벤트와 행동 이벤트를 즉시 연결
        if ( _isSubscribed )
            SubscribeSection( section );
    }

    /// <summary>
    /// 등록된 기능별 가이드 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        for ( int i = 0; i < _sections.Count; i++ )
            SubscribeSection( _sections [ i ] );

        _isSubscribed = true;
    }

    /// <summary>
    /// 등록된 기능별 가이드 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        for ( int i = 0; i < _sections.Count; i++ )
            UnsubscribeSection( _sections [ i ] );

        _isSubscribed = false;
    }

    /// <summary>
    /// 단일 기능별 가이드 이벤트 연결
    /// </summary>
    /// <param name="section">연결할 기능별 가이드 프레젠터</param>
    void SubscribeSection ( ITutorialGuideSection section )
    {
        //완료 이벤트를 먼저 연결해 Begin 직후 발생하는 완료도 놓치지 않음
        section.OnCompleted += CompleteGuide;
        section.SubscribeEvents( );
    }

    /// <summary>
    /// 단일 기능별 가이드 이벤트 해제
    /// </summary>
    /// <param name="section">해제할 기능별 가이드 프레젠터</param>
    void UnsubscribeSection ( ITutorialGuideSection section )
    {
        section.OnCompleted -= CompleteGuide;
        section.UnsubscribeEvents( );
    }

    #endregion

    #region ----- 가이드 시작 -----

    /// <summary>
    /// 후속 가이드 시작 또는 대기열 등록 요청
    /// </summary>
    /// <param name="guideId">요청할 가이드 아이디</param>
    /// <returns>시작하거나 대기열에 등록한 여부</returns>
    public bool RequestGuide ( TutorialGuideId guideId )
    {
        if ( _tutorialModel.CoreTutorialCompleted == false ||
            _tutorialModel.HasShownGuide( guideId ) ||
            _currentGuideId == guideId )
        {
            return false;
        }

        //대기 중인 가이드의 화면 조건이 다시 갖춰졌으면 즉시 재시도
        if ( _queuedGuideIds.Contains( guideId ) )
        {
            TryStartNext( );
            return true;
        }

        ITutorialGuideSection section = FindSection( guideId );

        if ( section == null ) return false;

        //대화·화살표·다른 가이드가 사용 중이면 순서만 보관
        if ( _currentGuideId.HasValue || _canStartGuide( ) == false )
        {
            EnqueueGuide( guideId );
            return true;
        }

        if ( BeginGuide( guideId, section ) )
            return true;

        //패널 비활성화 등으로 시작하지 못하면 다음 조건 확인까지 보류
        EnqueueGuide( guideId );
        return true;
    }

    /// <summary>
    /// 현재 실행 가능한 첫 번째 대기 가이드 시작
    /// </summary>
    /// <returns>대기 가이드 시작 여부</returns>
    public bool TryStartNext ()
    {
        if ( _currentGuideId.HasValue ||
            _queuedGuides.Count == 0 ||
            _canStartGuide( ) == false )
        {
            return false;
        }

        int pendingCount = _queuedGuides.Count;

        //현재 화면에서 시작할 수 없는 가이드 하나가 뒤 가이드를 막지 않게
        //이번 호출 시점의 대기 개수만큼만 순환 검사
        for ( int i = 0; i < pendingCount; i++ )
        {
            TutorialGuideId guideId = _queuedGuides.Peek( );

            if ( _tutorialModel.HasShownGuide( guideId ) )
            {
                DequeueGuide( );
                continue;
            }

            ITutorialGuideSection section = FindSection( guideId );

            if ( section == null )
            {
                DequeueGuide( );
                continue;
            }

            if ( BeginGuide( guideId, section ) )
            {
                DequeueGuide( );
                return true;
            }

            MoveFirstGuideToEnd( );
        }

        return false;
    }

    /// <summary>
    /// 기능별 가이드 실제 시작
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <param name="section">담당 기능별 가이드 프레젠터</param>
    /// <returns>시작 성공 여부</returns>
    bool BeginGuide (
        TutorialGuideId guideId, ITutorialGuideSection section )
    {
        _clearGuideDisplay( );

        //Begin 안에서 대화 신호가 즉시 발생해도 받을 수 있도록 먼저 선점
        _currentGuideId = guideId;
        _currentSection = section;
        OnActiveChanged?.Invoke( true );

        if ( section.Begin( guideId ) == true )
            return true;

        //시작 실패 시 현재 상태와 표시를 모두 원상 복구
        section.Reset( );
        _currentGuideId = null;
        _currentSection = null;
        _clearGuideDisplay( );
        OnActiveChanged?.Invoke( false );

        return false;
    }

    /// <summary>
    /// 지정 가이드 담당 프레젠터 조회
    /// </summary>
    /// <param name="guideId">조회할 가이드 아이디</param>
    /// <returns>담당 기능별 가이드 프레젠터</returns>
    ITutorialGuideSection FindSection ( TutorialGuideId guideId )
    {
        for ( int i = 0; i < _sections.Count; i++ )
        {
            if ( _sections [ i ].Handles( guideId ) )
                return _sections [ i ];
        }

        return null;
    }

    #endregion

    #region ----- 대화 전달 -----

    /// <summary>
    /// 현재 기능별 가이드에 대화 연출 신호 전달
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        if ( _currentSection == null || string.IsNullOrEmpty( signalId ) )
            return false;

        return _currentSection.HandleSignal( signalId );
    }

    /// <summary>
    /// 현재 기능별 가이드에 대화 완료 전달
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( _currentSection == null || string.IsNullOrEmpty( dialogueId ) )
            return false;

        return _currentSection.HandleDialogueCompleted( dialogueId );
    }

    #endregion

    #region ----- 완료와 초기화 -----

    /// <summary>
    /// 현재 기능별 가이드 완료 처리
    /// </summary>
    /// <param name="guideId">완료한 가이드 아이디</param>
    void CompleteGuide ( TutorialGuideId guideId )
    {
        if ( _currentGuideId != guideId || _currentSection == null )
            return;

        ITutorialGuideSection completedSection = _currentSection;

        //완료 이벤트 재진입 전에 현재 표시와 실행 상태부터 정리
        _clearGuideDisplay( );
        completedSection.Reset( );
        _currentGuideId = null;
        _currentSection = null;
        OnActiveChanged?.Invoke( false );

        _tutorialModel.MarkGuideShown( guideId );

        //대화 퇴장과 패널 전환이 끝난 상태에서만 다음 가이드 시작
        TryStartNext( );
    }

    /// <summary>
    /// 현재 실행 중인 후속 가이드 완료 처리
    /// </summary>
    /// <returns>완료할 활성 가이드 존재 여부</returns>
    public bool SkipCurrentGuide ()
    {
        if ( _currentGuideId.HasValue == false ||
            _currentSection == null )
        {
            return false;
        }

        CompleteGuide( _currentGuideId.Value );
        return true;
    }

    /// <summary>
    /// 실행 중인 가이드와 대기열 및 표시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _clearGuideDisplay( );

        for ( int i = 0; i < _sections.Count; i++ )
            _sections [ i ].Reset( );

        _currentGuideId = null;
        _currentSection = null;
        _queuedGuides.Clear( );
        _queuedGuideIds.Clear( );
        OnActiveChanged?.Invoke( false );
    }

    /// <summary>
    /// 가이드 대기열에 중복 없이 등록
    /// </summary>
    /// <param name="guideId">등록할 가이드 아이디</param>
    void EnqueueGuide ( TutorialGuideId guideId )
    {
        if ( _queuedGuideIds.Add( guideId ) == false )
            return;

        _queuedGuides.Enqueue( guideId );
    }

    /// <summary>
    /// 가장 먼저 등록된 가이드를 대기열에서 제거
    /// </summary>
    void DequeueGuide ()
    {
        if ( _queuedGuides.Count == 0 ) return;

        TutorialGuideId guideId = _queuedGuides.Dequeue( );
        _queuedGuideIds.Remove( guideId );
    }

    /// <summary>
    /// 현재 시작할 수 없는 첫 가이드를 대기열 마지막으로 이동
    /// </summary>
    void MoveFirstGuideToEnd ()
    {
        if ( _queuedGuides.Count == 0 ) return;

        TutorialGuideId guideId = _queuedGuides.Dequeue( );
        _queuedGuides.Enqueue( guideId );
    }

    #endregion
}
