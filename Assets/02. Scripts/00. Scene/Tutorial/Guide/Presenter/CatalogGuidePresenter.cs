using System;
using UnityEngine;

/// <summary>
/// Day 5 카탈로그 후속 가이드 프레젠터
/// </summary>
public class CatalogGuidePresenter : ITutorialGuideSection
{
    const int GuideStartDay = 5;
    const float SearchFilterPunchInterval = 0.45f;

    const string IntroDialogueId = "Dialogue_Day5_CatalogIntro";

    const string GuideDialogueId = "Dialogue_Day5_CatalogGuide";

    enum GuidePhase
    {
        None,               //미진행
        Intro,              //카탈로그 소개
        WaitingForPanel,    //카탈로그 진입 대기
        Guide,              //카탈로그 내부 설명
    }

    TutorialModel _tutorialModel;               //튜토리얼 진행 상태
    PlayStateModel _playStateModel;             //현재 누적 영업일
    SideActionPresenter _sideActionPresenter;   //카탈로그 화면과 대상 중재
    TutorialView _tutorialView;                 //공용 강조 표시 뷰

    /// <summary>
    /// 대화 재생 함수(가이드 아이디, 재생 성공 여부)
    /// </summary>
    Func<string, bool> _playDialogue;
    /// <summary>
    /// 후속 가이드 요청 함수(가이드 아이디, 요청 성공 여부)
    /// </summary>
    Func<TutorialGuideId, bool> _requestGuide;

    GuidePhase _phase;                           //현재 카탈로그 가이드 단계
    string _currentDialogueId;                   //현재 재생 중인 대화
    bool _isCatalogOpened;                       //카탈로그 표시 여부
    bool _isGuideOrderingApplied;                //튜토리얼 전용 목록 정렬 적용 여부
    bool _isSearchFilterPunchQueued;             //검색/필터 순차 강조 예약 여부
    bool _isSubscribed;                          //이벤트 연결 여부

    /// <summary>
    /// 카탈로그 가이드 완료 이벤트(가이드 아이디)
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// Day 5 카탈로그 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="sideActionPresenter">사이드 액션 프레젠터</param>
    /// <param name="tutorialView">공용 튜토리얼 강조 표시 뷰</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public CatalogGuidePresenter (
        TutorialModel tutorialModel,
        PlayStateModel playStateModel,
        SideActionPresenter sideActionPresenter,
        TutorialView tutorialView,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _playStateModel = playStateModel;
        _sideActionPresenter = sideActionPresenter;
        _tutorialView = tutorialView;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 카탈로그 가이드 요청 가능 여부 확인
    /// </summary>
    /// <returns>가이드 요청 가능 여부</returns>
    bool CanRequestGuide ()
    {
        //핵심 튜토리얼 완료 여부 확인,
        //시작일 확인, 가이드 완료 여부 확인, 현재 단계 확인
        return _tutorialModel.CoreTutorialCompleted &&
            _playStateModel.TotalDay >= GuideStartDay &&
            _tutorialModel.HasShownGuide(
                TutorialGuideId.Catalog ) == false &&
            _phase == GuidePhase.None;
    }

    /// <summary>
    /// 카탈로그 가이드 실제 시작 가능 여부 확인
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <returns>가이드 시작 가능 여부</returns>
    bool CanBeginGuide ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Catalog &&
            CanRequestGuide( );
    }

    /// <summary>
    /// 카탈로그 가이드를 중복 없이 요청
    /// </summary>
    void RequestCatalogGuide ()
    {
        if ( CanRequestGuide( ) == false )
            return;

        _requestGuide( TutorialGuideId.Catalog );
    }

    /// <summary>
    /// 카탈로그 소개 대사 재생
    /// </summary>
    /// <returns>대사 재생 성공 여부</returns>
    bool PlayIntroDialogue ()
    {
        ApplyGuideOrdering( );

        _phase = GuidePhase.Intro;
        _currentDialogueId = IntroDialogueId;

        //인트로 대사 재생 성공 시
        if ( _playDialogue( IntroDialogueId ) == true )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 카탈로그 내부 가이드 대사 재생
    /// </summary>
    /// <returns>대사 재생 성공 여부</returns>
    bool PlayGuideDialogue ()
    {
        //카탈로그가 열리지 않았다면
        if ( _isCatalogOpened == false )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;
            return false;
        }

        //메인 카탈로그 버튼에 남은 화살표 제거
        _tutorialView.HideInstant( );

        _phase = GuidePhase.Guide;
        _currentDialogueId = GuideDialogueId;

        //가이드 대사 재생 성공 시
        if ( _playDialogue( GuideDialogueId ) == true )
            return true;

        _phase = GuidePhase.WaitingForPanel;
        _currentDialogueId = string.Empty;
        return false;
    }

    /// <summary>
    /// 튜토리얼 동안 잠금 파츠 하나를 첫 페이지 선두에 배치
    /// </summary>
    void ApplyGuideOrdering ()
    {
        if ( _isGuideOrderingApplied )
            return;

        //목록 정렬
        _sideActionPresenter.SetCatalogGuideMode( true );
        _isGuideOrderingApplied = true;
    }

    /// <summary>
    /// 카탈로그 목록을 기존 정렬 상태로 복구
    /// </summary>
    void RestoreGuideOrdering ()
    {
        //가이드용 정렬이 되어 있지 않다면
        if ( _isGuideOrderingApplied == false )
            return;

        //목록 정렬 복구
        _sideActionPresenter.SetCatalogGuideMode( false );
        _isGuideOrderingApplied = false;
    }

    /// <summary>
    /// 현재 카탈로그 UI를 공용 튜토리얼 대상으로 등록
    /// </summary>
    /// <param name="targetId">등록할 카탈로그 대상 아이디</param>
    /// <returns>대상 등록 성공 여부</returns>
    bool RegisterTarget ( TutorialTargetId targetId )
    {
        //가이드용 타겟 가져오기
        if ( _sideActionPresenter.TryGetTutorialTarget(
            targetId,
            out RectTransform target,
            out Vector2 arrowOffset ) == false )
        {
            return false;
        }

        //튜토리얼뷰에 등록
        return _tutorialView.RegisterTarget(
            targetId, target, arrowOffset );
    }

    /// <summary>
    /// 지정 카탈로그 대상에 화살표를 표시
    /// </summary>
    /// <param name="targetId">강조할 대상 아이디</param>
    /// <returns>강조 표시 성공 여부</returns>
    bool ShowTarget ( TutorialTargetId targetId )
    {
        //등록된 대상이 없다면 실패
        if ( RegisterTarget( targetId ) == false )
            return false;

        //등록된 대상 연출 재생
        return _tutorialView.ShowTarget(
            targetId, TutorialGuideMode.Focus );
    }

    /// <summary>
    /// 지정 카탈로그 대상을 화살표 없이 한 번 확대
    /// </summary>
    /// <param name="targetId">강조할 대상 아이디</param>
    /// <returns>확대 강조 성공 여부</returns>
    bool PlayTargetPunch ( TutorialTargetId targetId )
    {
        _tutorialView.HideInstant( );

        if ( RegisterTarget( targetId ) == false )
            return false;

        return _tutorialView.PlayTargetPunch( targetId );
    }

    /// <summary>
    /// 검색창과 필터를 순서대로 확대 강조
    /// </summary>
    /// <returns>순차 강조 시작 여부</returns>
    bool PlaySearchFilterSequence ()
    {
        _tutorialView.HideInstant( );

        //등록 성공 여부
        bool hasSearch = RegisterTarget( TutorialTargetId.CatalogSearch );
        bool hasFilter = RegisterTarget( TutorialTargetId.CatalogFilter );

        //둘 다 실패 시 종료
        if ( hasSearch == false && hasFilter == false )
            return false;

        //검색창 등록 실패 시 필터만 강조
        if ( hasSearch == false )
            return _tutorialView.PlayTargetPunch(
                TutorialTargetId.CatalogFilter );

        //필터 등록 실패 시 검색창만 강조
        if ( hasFilter == false )
            return _tutorialView.PlayTargetPunch(
                TutorialTargetId.CatalogSearch );

        //강조 예약 여부 확인
        _isSearchFilterPunchQueued = true;

        //검색창, 필터 순서대로 강조 연출 재생
        return _tutorialView.PlayTargetPunchSequence(
            SearchFilterPunchInterval,
            TutorialTargetId.CatalogSearch,
            TutorialTargetId.CatalogFilter );
    }

    /// <summary>
    /// 연속된 필터 신호의 중복 강조를 방지
    /// </summary>
    /// <returns>필터 신호 처리 여부</returns>
    bool HandleFilterPunch ()
    {
        //강조 연출 예약이 있다면
        if ( _isSearchFilterPunchQueued == true )
        {
            _isSearchFilterPunchQueued = false;
            return true;
        }

        //필터 강조 연출 재생
        return PlayTargetPunch( TutorialTargetId.CatalogFilter );
    }

    /// <summary>
    /// 해금 파츠 상세를 보장하고 상점 이동 버튼을 강조
    /// </summary>
    /// <returns>상점 이동 버튼 강조 성공 여부</returns>
    bool PlayShopLinkPunch ()
    {
        //파츠 상세 표시
        if ( _sideActionPresenter.EnsureCatalogGuideDetail( ) == false )
            return false;

        //상점 버튼 강조 연출 재생
        return PlayTargetPunch( TutorialTargetId.CatalogShopLink );
    }

    /// <summary>
    /// 현재 카탈로그 가이드를 완료
    /// </summary>
    void CompleteGuide ()
    {
        //뷰 숨기기
        _tutorialView.HideInstant( );
        //기존 정렬 상태 복구
        RestoreGuideOrdering( );

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;
        _isSearchFilterPunchQueued = false;

        //카탈로그 가이드 완료 이벤트 발행
        OnCompleted?.Invoke( TutorialGuideId.Catalog );
    }

    /// <summary>
    /// 카탈로그 후속 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>카탈로그 가이드 담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.Catalog;
    }

    /// <summary>
    /// Day 5 카탈로그 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 후속 가이드 아이디</param>
    /// <returns>가이드 시작 성공 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        //가이드 시작 여부 확인
        if ( CanBeginGuide( guideId ) == false )
            return false;

        //인트로 대사 재생
        return PlayIntroDialogue( );
    }

    /// <summary>
    /// 카탈로그 가이드 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        switch ( signalId )
        {
            case "Highlight_CatalogButton"
                when _phase == GuidePhase.Intro:
                //카탈로그 버튼 연출
                return ShowTarget( TutorialTargetId.CatalogButton );

            case "Highlight_LockedCatalogPart"
                when _phase == GuidePhase.Guide:
                //잠금 파츠 연출
                return PlayTargetPunch( TutorialTargetId.CatalogLockedPart );

            case "Highlight_CatalogPart"
                when _phase == GuidePhase.Guide:
                //파츠 연출
                return ShowTarget( TutorialTargetId.CatalogPart );

            case "Highlight_CatalogSearch"
                when _phase == GuidePhase.Guide:
                //검색창, 필터 강조 연출
                return PlaySearchFilterSequence( );

            case "Highlight_CatalogFilter"
                when _phase == GuidePhase.Guide:
                //중복 연출 방지
                return HandleFilterPunch( );

            case "Highlight_CatalogShopLink"
                when _phase == GuidePhase.Guide:
                //상세 표시 및 상점 버튼 연출
                return PlayShopLinkPunch( );

            default:
                return false;
        }
    }

    /// <summary>
    /// 카탈로그 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 카탈로그 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        //완료한 아이디와 현재 아이디가 다르면
        if ( dialogueId != _currentDialogueId )
            return false;

        if ( _phase == GuidePhase.Intro )
        {
            _phase = GuidePhase.WaitingForPanel;
            _currentDialogueId = string.Empty;

            //카탈로그가 열린 상태면 가이드 대사 재생
            if ( _isCatalogOpened == true )
                PlayGuideDialogue( );

            return true;
        }

        if ( _phase != GuidePhase.Guide )
            return false;

        //가이드 완료
        CompleteGuide( );
        return true;
    }

    /// <summary>
    /// 날짜와 카탈로그 화면 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed )
            return;

        //가이드 시작 조건 확인 이벤트 연결
        _playStateModel
            .OnDateChanged += HandleDateChanged;
        //카탈로그 진입 이벤트 연결
        _sideActionPresenter
            .OnCatalogOpened += HandleCatalogOpened;
        //카탈로그 닫힘 이벤트 연결
        _sideActionPresenter
            .OnCatalogClosed += HandleCatalogClosed;
        //파츠 선택 이벤트 연결
        _sideActionPresenter
            .OnCatalogPartSelected += HandleCatalogPartSelected;

        _isSubscribed = true;

        //Day 5 이상 세이브를 불러온 경우에도 최초 안내 요청
        RequestCatalogGuide( );
    }

    /// <summary>
    /// 날짜와 카탈로그 화면 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false )
            return;

        _playStateModel.OnDateChanged -= HandleDateChanged;
        _sideActionPresenter.OnCatalogOpened -= HandleCatalogOpened;
        _sideActionPresenter.OnCatalogClosed -= HandleCatalogClosed;
        _sideActionPresenter.OnCatalogPartSelected -= HandleCatalogPartSelected;

        _isSubscribed = false;
    }

    /// <summary>
    /// 날짜 변경 후 Day 5 카탈로그 안내 조건 확인
    /// </summary>
    /// <param name="month">변경된 월</param>
    /// <param name="day">변경된 일</param>
    void HandleDateChanged ( int month, int day )
    {
        RequestCatalogGuide( );
    }

    /// <summary>
    /// 카탈로그 진입 후 내부 설명 시작
    /// </summary>
    void HandleCatalogOpened ()
    {
        _isCatalogOpened = true;

        if ( _phase == GuidePhase.WaitingForPanel )
            PlayGuideDialogue( );
    }

    /// <summary>
    /// 카탈로그 종료 시 진행 중인 안내 정리
    /// </summary>
    void HandleCatalogClosed ()
    {
        _isCatalogOpened = false;
        _tutorialView.HideInstant( );

        //내부 설명 중 화면을 닫았다면 해당 안내는 확인한 것으로 처리
        if ( _phase == GuidePhase.Guide )
            CompleteGuide( );
    }

    /// <summary>
    /// 카탈로그 파츠 선택 후 슬롯 화살표 제거
    /// </summary>
    /// <param name="partId">선택한 파츠 아이디</param>
    void HandleCatalogPartSelected ( string partId )
    {
        if ( _phase != GuidePhase.Guide )
            return;

        _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 진행 중인 카탈로그 가이드 임시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        RestoreGuideOrdering( );

        _phase = GuidePhase.None;
        _currentDialogueId = string.Empty;
        _isCatalogOpened = false;
        _isSearchFilterPunchQueued = false;
    }
}
