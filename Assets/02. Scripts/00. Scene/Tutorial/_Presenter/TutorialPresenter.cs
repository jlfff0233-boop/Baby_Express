using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 프레젠터 - 공용 대화와 기능별 튜토리얼 진행 중재
/// </summary>
public class TutorialPresenter : MonoBehaviour
{
    const string EmployeeGuideTargetId = "Employee_0_Dullahan_0";

    delegate bool TutorialTargetGetter (
        TutorialTargetId targetId, out RectTransform target );

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] DialoguePresenter _dialoguePresenter;       //공용 대화 프레젠터
    [SerializeField] TutorialView _tutorialView;       //튜토리얼 표시 뷰

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 데이터
    TutorialGuideData _tutorialGuideData;       //Day 2~7 후속 가이드 데이터
    TutorialGuideCoordinator _guideCoordinator;       //후속 가이드 실행 중재기

    TutorialOrderPresenter _orderSection;       //주문 튜토리얼 프레젠터
    TutorialShopPresenter _shopSection;       //상점 튜토리얼 프레젠터
    TutorialCraftPresenter _craftSection;       //제작 튜토리얼 프레젠터
    TutorialDeliveryPresenter _deliverySection;       //배송 튜토리얼 프레젠터
    OrderGuidePresenter _orderGuideSection;       //Day 2 주문 후속 가이드 프레젠터
    ShopGuidePresenter _shopGuideSection;       //Day 3 상점 후속 가이드 프레젠터
    MaintenanceGuidePresenter _maintenanceGuideSection;       //Day 4 정비 후속 가이드 프레젠터
    CatalogGuidePresenter _catalogGuideSection;       //Day 5 카탈로그 후속 가이드 프레젠터
    AchievementGuidePresenter _achievementGuideSection;       //누적 영업일 2일차 업적 후속 가이드 프레젠터
    EmployeeGuidePresenter _employeeGuideSection;       //Day 7 직원 후속 가이드 프레젠터
    WeeklySettlementGuidePresenter _weeklyGuideSection;       //첫 주간 결산 후속 가이드 프레젠터
    LedgerGuidePresenter _ledgerGuideSection;       //첫 주간 결산 이후 가계부 가이드 프레젠터
    List<ITutorialSection> _sections =
        new List<ITutorialSection>( );       //기능별 튜토리얼 프레젠터

    string _currentDialogueId;       //현재 재생 중인 튜토리얼 대화
    string _queuedDialogueId;       //현재 대화 종료 후 이어서 재생할 대화
    bool _isInitialized;       //모델과 기능별 프레젠터 연결 여부
    bool _isSubscribed;       //공용 이벤트 연결 여부
    bool _isFollowupGuideStartBlocked;       //영업일 전환 중 후속 가이드 시작 잠금 여부
    bool _wasIntroCompleted;       //인트로 완료 상태 확인용
    bool _isSkipping;       //스킵 중복 실행 차단 여부

    /// <summary>
    /// Day 1 핵심 튜토리얼 완료 이벤트
    /// </summary>
    public event Action OnCoreTutorialCompleted;

    /// <summary>
    /// 공용 대화 활성화 상태 이벤트
    /// </summary>
    public event Action<bool> OnDialogueToggled;

    /// <summary>
    /// 활성화 이후 튜토리얼 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        if ( _isInitialized ) SubscribeEvents( );
    }

    /// <summary>
    /// 비활성화할 때 튜토리얼 이벤트 해제
    /// </summary>
    void OnDisable ()
    {
        UnsubscribeEvents( );
        ResetDisplay( );
        _guideCoordinator?.Reset( );

        if ( _tutorialView != null )
            _tutorialView.SetSkipVisible( false );
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 스페이스바로 현재 튜토리얼 또는 가이드 건너뛰기
    /// </summary>
    void Update ()
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
            SkipCurrent( );
    }
#endif

    /// <summary>
    /// 튜토리얼 데이터와 기능별 프레젠터 연결
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 튜토리얼 데이터</param>
    /// <param name="tutorialGuideData">Day 2~7 후속 가이드 데이터</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="actionView">메인 행동 뷰</param>
    /// <param name="orderPresenter">주문 프레젠터</param>
    /// <param name="shopPresenter">상점 프레젠터</param>
    /// <param name="inventoryPresenter">인벤토리 프레젠터</param>
    /// <param name="craftPresenter">제작 프레젠터</param>
    /// <param name="deliveryPresenter">배송 프레젠터</param>
    /// <param name="settlementPresenter">결산 프레젠터</param>
    /// <param name="settlementModel">결산 기록 모델</param>
    /// <param name="maintenancePresenter">정비 프레젠터</param>
    /// <param name="sideActionPresenter">사이드 액션 프레젠터</param>
    /// <param name="achvModel">업적 상태 모델</param>
    /// <param name="shopModel">상점 재고 상태 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="orderModel">주문 모델</param>
    public void Init (
        TutorialModel tutorialModel,
        TutorialDay1Data tutorialDay1Data,
        TutorialGuideData tutorialGuideData,
        PlayStateModel playStateModel,
        ActionView actionView,
        OrderPresenter orderPresenter,
        ShopPresenter shopPresenter,
        InventoryPresenter inventoryPresenter,
        CraftPresenter craftPresenter,
        DeliveryPresenter deliveryPresenter,
        SettlementPresenter settlementPresenter,
        SettlementModel settlementModel,
        MaintenancePresenter maintenancePresenter,
        SideActionPresenter sideActionPresenter,
        AchvModel achvModel,
        ShopModel shopModel,
        InventoryModel inventoryModel,
        CustomerOrderModel orderModel )
    {
        UnsubscribeEvents( );
        _guideCoordinator?.Reset( );
        _tutorialView.InitRuntime( );
        _tutorialView.SetSkipVisible( false );

        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
        _tutorialGuideData = tutorialGuideData;
        _wasIntroCompleted = tutorialModel.IntroCompleted;

        _guideCoordinator = new TutorialGuideCoordinator(
            _tutorialModel, CanStartGuide, ClearGuideDisplay );

        RegisterFixedTargets(
            actionView, orderPresenter, shopPresenter,
            craftPresenter, deliveryPresenter,
            settlementPresenter, maintenancePresenter );

        InitializeSections(
            orderPresenter, shopPresenter,
            inventoryPresenter, craftPresenter,
            deliveryPresenter, settlementPresenter,
            inventoryModel, orderModel );

        InitializeGuideSections(
            playStateModel, orderModel, orderPresenter,
            shopModel, shopPresenter, maintenancePresenter,
            settlementPresenter, settlementModel,
            sideActionPresenter, achvModel );

        _isInitialized = true;

        if ( isActiveAndEnabled ) SubscribeEvents( );
    }

    /// <summary>
    /// 기존 시스템 뷰 참조를 고정 튜토리얼 대상으로 자동 등록
    /// </summary>
    /// <param name="actionView">메인 행동 뷰</param>
    /// <param name="orderPresenter">주문 프레젠터</param>
    /// <param name="shopPresenter">상점 프레젠터</param>
    /// <param name="craftPresenter">제작 프레젠터</param>
    /// <param name="deliveryPresenter">배송 프레젠터</param>
    /// <param name="settlementPresenter">결산 프레젠터</param>
    /// <param name="maintenancePresenter">정비 프레젠터</param>
    void RegisterFixedTargets (
        ActionView actionView,
        OrderPresenter orderPresenter,
        ShopPresenter shopPresenter,
        CraftPresenter craftPresenter,
        DeliveryPresenter deliveryPresenter,
        SettlementPresenter settlementPresenter,
        MaintenancePresenter maintenancePresenter )
    {
        RegisterTargets(
            actionView.TryGetTutorialTarget,
            TutorialTargetId.OrderButton,
            TutorialTargetId.ShopButton,
            TutorialTargetId.InventoryButton,
            TutorialTargetId.CraftButton,
            TutorialTargetId.MaintenanceButton );

        RegisterTargets(
            orderPresenter.TryGetTutorialTarget,
            TutorialTargetId.Requirement,
            TutorialTargetId.Wish,
            TutorialTargetId.OrderDeadline,
            TutorialTargetId.OrderAcceptButton );

        RegisterTargets(
            shopPresenter.TryGetTutorialTarget,
            TutorialTargetId.AddCartButton,
            TutorialTargetId.CartButton,
            TutorialTargetId.PurchaseButton,
            TutorialTargetId.QuickRestockButton );

        RegisterTargets(
            craftPresenter.TryGetTutorialTarget,
            TutorialTargetId.CraftStartButton,
            TutorialTargetId.CraftCost,
            TutorialTargetId.OrderConditions,
            TutorialTargetId.CraftDoneButton,
            TutorialTargetId.DeliveryButton,
            TutorialTargetId.PartsSelectOpenButton,
            TutorialTargetId.CraftInfoOpenButton,
            TutorialTargetId.CraftInfoSwitchButton );

        RegisterTargets(
            deliveryPresenter.TryGetTutorialTarget,
            TutorialTargetId.DirectDelivery,
            TutorialTargetId.DeliverySummary,
            TutorialTargetId.DeliveryStartButton );

        RegisterTargets(
            maintenancePresenter.TryGetTutorialTarget,
            TutorialTargetId.MaintenanceFacilityTab,
            TutorialTargetId.MaintenanceResearchTab,
            TutorialTargetId.MaintenanceConvenienceTab,
            TutorialTargetId.MaintenanceEmployeeTab,
            TutorialTargetId.MaintenanceCurrentLevel,
            TutorialTargetId.MaintenanceNextLevel );

        if ( settlementPresenter.TryGetConfirmTarget(
            out RectTransform settlementConfirmTarget ) )
        {
            _tutorialView.RegisterTarget(
                TutorialTargetId.SettlementConfirmButton,
                settlementConfirmTarget,
                Vector2.zero );
        }
    }

    /// <summary>
    /// 같은 시스템에서 제공하는 고정 UI 대상을 일괄 등록
    /// </summary>
    /// <param name="targetGetter">대상 아이디별 UI 조회 함수</param>
    /// <param name="targetIds">등록할 대상 아이디 목록</param>
    void RegisterTargets (
        TutorialTargetGetter targetGetter,
        params TutorialTargetId [ ] targetIds )
    {
        for ( int i = 0; i < targetIds.Length; i++ )
        {
            TutorialTargetId targetId = targetIds [ i ];

            if ( targetGetter(
                targetId, out RectTransform target ) == false )
            {
                continue;
            }

            _tutorialView.RegisterTarget(
                targetId, target, Vector2.zero );
        }
    }

    /// <summary>
    /// 기능별 튜토리얼 프레젠터 생성
    /// </summary>
    /// <param name="orderPresenter">주문 프레젠터</param>
    /// <param name="shopPresenter">상점 프레젠터</param>
    /// <param name="inventoryPresenter">인벤토리 프레젠터</param>
    /// <param name="craftPresenter">제작 프레젠터</param>
    /// <param name="deliveryPresenter">배송 프레젠터</param>
    /// <param name="settlementPresenter">결산 프레젠터</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="orderModel">주문 모델</param>
    void InitializeSections (
        OrderPresenter orderPresenter,
        ShopPresenter shopPresenter,
        InventoryPresenter inventoryPresenter,
        CraftPresenter craftPresenter,
        DeliveryPresenter deliveryPresenter,
        SettlementPresenter settlementPresenter,
        InventoryModel inventoryModel,
        CustomerOrderModel orderModel )
    {
        _sections.Clear( );

        _orderSection = new TutorialOrderPresenter(
            _tutorialModel, _tutorialDay1Data, _tutorialView,
            orderPresenter, PlayDialogue, QueueDialogue,
            CompleteStep );

        _shopSection = new TutorialShopPresenter(
            _tutorialModel, _tutorialDay1Data, _tutorialView,
            shopPresenter, inventoryModel,
            PlayDialogue, QueueDialogue, CompleteStep );

        _sections.Add( _orderSection );
        _sections.Add( _shopSection );
        _sections.Add( new TutorialInventoryPresenter(
            _tutorialModel, _tutorialDay1Data, _tutorialView,
            inventoryPresenter, craftPresenter, inventoryModel,
            PlayDialogue, QueueDialogue, CompleteStep ) );
        _craftSection = new TutorialCraftPresenter(
            _tutorialModel, _tutorialDay1Data, _tutorialView,
            craftPresenter, inventoryModel, orderModel,
            PlayDialogue, QueueDialogue, CompleteStep );

        _deliverySection = new TutorialDeliveryPresenter(
            _tutorialModel, _tutorialDay1Data, _tutorialView,
            deliveryPresenter, orderModel,
            PlayDialogue, CompleteStep );

        _sections.Add( _craftSection );
        _sections.Add( _deliverySection );
        _sections.Add( new TutorialSettlementPresenter(
            _tutorialModel, _tutorialView, settlementPresenter,
            PlayDialogue, CompleteStep ) );
    }

    /// <summary>
    /// Day 2 이후 기능별 후속 가이드 프레젠터 생성과 등록
    /// </summary>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="orderModel">주문 상태 조회 모델</param>
    /// <param name="orderPresenter">주문 행동 결과 프레젠터</param>
    /// <param name="shopModel">상점 재고 상태 모델</param>
    /// <param name="shopPresenter">상점 행동 결과 프레젠터</param>
    /// <param name="maintenancePresenter">정비 행동 결과 프레젠터</param>
    /// <param name="settlementPresenter">결산 행동 결과 프레젠터</param>
    /// <param name="settlementModel">완료된 결산 기록 모델</param>
    /// <param name="sideActionPresenter">사이드 액션 프레젠터</param>
    /// <param name="achvModel">업적 상태 모델</param>
    void InitializeGuideSections (
        PlayStateModel playStateModel,
        CustomerOrderModel orderModel,
        OrderPresenter orderPresenter,
        ShopModel shopModel,
        ShopPresenter shopPresenter,
        MaintenancePresenter maintenancePresenter,
        SettlementPresenter settlementPresenter,
        SettlementModel settlementModel,
        SideActionPresenter sideActionPresenter,
        AchvModel achvModel )
    {
        _orderGuideSection = new OrderGuidePresenter(
            _tutorialModel, playStateModel, orderModel,
            orderPresenter,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _orderGuideSection );

        _shopGuideSection = new ShopGuidePresenter(
            _tutorialModel, playStateModel, shopModel,
            shopPresenter, _tutorialView,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _shopGuideSection );

        _maintenanceGuideSection = new MaintenanceGuidePresenter(
            _tutorialModel, playStateModel,
            maintenancePresenter, _tutorialView,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _maintenanceGuideSection );

        _catalogGuideSection = new CatalogGuidePresenter(
            _tutorialModel, playStateModel,
            sideActionPresenter, _tutorialView,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _catalogGuideSection );

        _achievementGuideSection = new AchievementGuidePresenter(
            _tutorialModel, playStateModel, achvModel,
            sideActionPresenter, _tutorialView,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _achievementGuideSection );

        _employeeGuideSection = new EmployeeGuidePresenter(
            _tutorialModel, playStateModel,
            maintenancePresenter, _tutorialView,
            EmployeeGuideTargetId,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _employeeGuideSection );

        _weeklyGuideSection = new WeeklySettlementGuidePresenter(
            _tutorialModel, settlementPresenter,
            _tutorialView, PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _weeklyGuideSection );

        _ledgerGuideSection = new LedgerGuidePresenter(
            _tutorialModel, settlementModel,
            sideActionPresenter, _tutorialView,
            PlayDialogue, RequestGuide );

        _guideCoordinator.RegisterSection(
            _ledgerGuideSection );
    }

    /// <summary>
    /// 공용 대화와 기능별 튜토리얼 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _dialoguePresenter.OnSignal += HandleSignal;
        _dialoguePresenter.OnToggled += HandleDialogueToggled;
        _dialoguePresenter.OnCompleted += HandleDialogueCompleted;
        _tutorialView.OnSkip += HandleSkip;
        _guideCoordinator.OnActiveChanged += HandleGuideActiveChanged;

        for ( int i = 0; i < _sections.Count; i++ )
        {
            _sections [ i ].SubscribeEvents( );
        }

        _tutorialModel.OnProgressChanged += HandleProgressChanged;
        _guideCoordinator.SubscribeEvents( );

        _isSubscribed = true;
    }

    /// <summary>
    /// 공용 대화와 기능별 튜토리얼 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _guideCoordinator?.UnsubscribeEvents( );

        _dialoguePresenter.OnSignal -= HandleSignal;
        _dialoguePresenter.OnToggled -= HandleDialogueToggled;
        _dialoguePresenter.OnCompleted -= HandleDialogueCompleted;
        _tutorialView.OnSkip -= HandleSkip;
        _guideCoordinator.OnActiveChanged -= HandleGuideActiveChanged;

        for ( int i = 0; i < _sections.Count; i++ )
        {
            _sections [ i ].UnsubscribeEvents( );
        }

        _tutorialModel.OnProgressChanged -= HandleProgressChanged;

        _isSubscribed = false;
    }

    #region ----- 시작과 복구 -----

    /// <summary>
    /// 불러온 인트로 완료 상태를 현재 기억값에 반영
    /// </summary>
    public void RefreshIntroState ()
    {
        _wasIntroCompleted = _tutorialModel.IntroCompleted;
    }

    /// <summary>
    /// 인트로 완료 이후 현재 튜토리얼 단계 시작 또는 복구
    /// </summary>
    /// <returns>튜토리얼 시작 여부</returns>
    public bool BeginOrResume ()
    {
        if ( _tutorialModel.IntroCompleted == false )
        {
            RefreshSkipButton( );
            return false;
        }

        if ( _tutorialModel.CoreTutorialCompleted == true )
        {
            bool started = _guideCoordinator.TryStartNext( );
            RefreshSkipButton( );
            return started;
        }

        RefreshSkipButton( );

        //인트로 완료 이벤트가 겹쳐도 현재 튜토리얼 대화를 다시 시작하지 않음
        if ( string.IsNullOrEmpty( _currentDialogueId ) == false )
            return true;

        //불러오기 이후 같은 단계를 다시 시작하지 않게 완료 상태 기억
        _wasIntroCompleted = true;

        _orderSection.Recover( );
        _shopSection.Recover( );
        _craftSection.Recover( );
        _deliverySection.Recover( );
        return BeginCurrentStep( );
    }

    /// <summary>
    /// 현재 단계 담당 프레젠터를 통해 행동 유도 시작
    /// </summary>
    /// <returns>단계 시작 여부</returns>
    bool BeginCurrentStep ()
    {
        ResetDisplay( );

        ITutorialSection section = GetCurrentSection( );

        if ( section == null ) return false;

        return section.Begin(
            _tutorialModel.CurrentCoreStep );
    }

    /// <summary>
    /// 현재 핵심 단계 담당 튜토리얼 프레젠터 조회
    /// </summary>
    /// <returns>기능별 튜토리얼 프레젠터</returns>
    ITutorialSection GetCurrentSection ()
    {
        CoreTutorialStep currentStep =
            _tutorialModel.CurrentCoreStep;

        for ( int i = 0; i < _sections.Count; i++ )
        {
            if ( _sections [ i ].Handles( currentStep ) )
                return _sections [ i ];
        }

        return null;
    }

    #endregion

    #region ----- 대화와 강조 -----

    /// <summary>
    /// 지정된 후속 튜토리얼 가이드 시작 또는 대기 요청
    /// </summary>
    /// <param name="guideId">요청할 후속 가이드 아이디</param>
    /// <returns>시작하거나 대기열에 등록한 여부</returns>
    public bool RequestGuide ( TutorialGuideId guideId )
    {
        if ( _isInitialized == false || _guideCoordinator == null )
            return false;

        return _guideCoordinator.RequestGuide( guideId );
    }

    /// <summary>
    /// 영업일 전환 중 후속 가이드 시작 잠금 상태 적용
    /// </summary>
    /// <param name="isBlocked">후속 가이드 시작 잠금 여부</param>
    public void SetFollowupGuideStartBlocked ( bool isBlocked )
    {
        _isFollowupGuideStartBlocked = isBlocked;

        if ( isBlocked == false )
            _guideCoordinator?.TryStartNext( );
    }

    /// <summary>
    /// 기능별 후속 가이드 프레젠터 등록
    /// </summary>
    /// <param name="section">등록할 기능별 후속 가이드 프레젠터</param>
    public void RegisterGuideSection ( ITutorialGuideSection section )
    {
        if ( _isInitialized == false || _guideCoordinator == null )
            return;

        _guideCoordinator.RegisterSection( section );
    }

    /// <summary>
    /// 현재 대화 종료 후 지정 대화를 이어서 재생
    /// </summary>
    /// <param name="dialogueId">재생할 대화 아이디</param>
    void QueueDialogue ( string dialogueId )
    {
        if ( string.IsNullOrEmpty( _currentDialogueId ) )
        {
            PlayDialogue( dialogueId );
            return;
        }

        _queuedDialogueId = dialogueId;
    }

    /// <summary>
    /// 현재 튜토리얼 구간에 해당하는 대화 재생
    /// </summary>
    /// <param name="dialogueId">대화 아이디</param>
    /// <returns>재생 시작 여부</returns>
    bool PlayDialogue ( string dialogueId )
    {
        if ( TryGetDialogue(
            dialogueId, out DialogueData dialogue ) == false )
        {
            return false;
        }

        //대화 중에는 행동 대상을 열어 두지 않고 게임 화면 입력 전체 차단
        _tutorialView.ShowInputBlocker( );
        _currentDialogueId = dialogueId;

        if ( _dialoguePresenter.BeginDialogue( dialogue ) )
            return true;

        _currentDialogueId = string.Empty;
        _tutorialView.HideInstant( );
        return false;
    }

    /// <summary>
    /// 현재 튜토리얼 구간의 데이터에서 대화 조회
    /// </summary>
    /// <param name="dialogueId">조회할 대화 아이디</param>
    /// <param name="dialogue">조회한 대화 데이터</param>
    /// <returns>대화 존재 여부</returns>
    bool TryGetDialogue (
        string dialogueId, out DialogueData dialogue )
    {
        if ( _tutorialModel.CoreTutorialCompleted == false )
        {
            if ( _tutorialDay1Data != null )
            {
                return _tutorialDay1Data.TryGetDialogue(
                    dialogueId, out dialogue );
            }
        }
        else if ( _tutorialGuideData != null )
        {
            return _tutorialGuideData.TryGetDialogue(
                dialogueId, out dialogue );
        }

        dialogue = null;
        return false;
    }

    /// <summary>
    /// 공용 대화 활성화 상태 전달
    /// </summary>
    /// <param name="isPlaying">현재 대화 재생 여부</param>
    void HandleDialogueToggled ( bool isPlaying )
    {
        OnDialogueToggled?.Invoke( isPlaying );
    }

    /// <summary>
    /// 현재 튜토리얼 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <param name="wasSkipped">전체 건너뛰기 여부</param>
    void HandleDialogueCompleted (
        string dialogueId, bool wasSkipped )
    {
        if ( dialogueId != _currentDialogueId ) return;

        _currentDialogueId = string.Empty;

        if ( string.IsNullOrEmpty( _queuedDialogueId ) == false )
        {
            string queuedDialogueId = _queuedDialogueId;
            _queuedDialogueId = string.Empty;

            PlayDialogue( queuedDialogueId );
            return;
        }

        if ( _tutorialModel.CoreTutorialCompleted )
        {
            _guideCoordinator.HandleDialogueCompleted( dialogueId );
            _guideCoordinator.TryStartNext( );
            return;
        }

        ITutorialSection section = GetCurrentSection( );
        section?.HandleDialogueCompleted( dialogueId );
    }

    /// <summary>
    /// 현재 기능별 튜토리얼에 대화 연출 신호 전달
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    void HandleSignal ( string signalId )
    {
        //현재 튜토리얼 대화에서 발생한 신호만 행동 유도에 사용
        if ( string.IsNullOrEmpty( _currentDialogueId ) ) return;

        if ( _tutorialModel.CoreTutorialCompleted )
        {
            _guideCoordinator.HandleSignal( signalId );
            return;
        }

        ITutorialSection section = GetCurrentSection( );
        section?.HandleSignal( signalId );
    }

    #endregion

    #region ----- 단계 진행 -----

    /// <summary>
    /// 현재 핵심 단계 완료 후 다음 단계 시작
    /// </summary>
    /// <param name="step">완료한 단계</param>
    void CompleteStep ( CoreTutorialStep step )
    {
        if ( _tutorialModel.CompleteCoreStep( step ) == false ) return;

        ResetDisplay( );

        if ( _tutorialModel.CoreTutorialCompleted )
        {
            OnCoreTutorialCompleted?.Invoke( );
            return;
        }

        BeginCurrentStep( );
    }

    /// <summary>
    /// 튜토리얼 뷰의 스킵 요청 처리
    /// </summary>
    void HandleSkip ()
    {
        SkipCurrent( );
    }

    /// <summary>
    /// 현재 Day 1 튜토리얼 또는 실행 중인 후속 가이드 완료 처리
    /// </summary>
    void SkipCurrent ()
    {
        if ( _isInitialized == false ||
            _tutorialModel.IntroCompleted == false ||
            _isSkipping == true )
        {
            return;
        }

        _isSkipping = true;

        try
        {
            if ( _tutorialModel.CoreTutorialCompleted == false )
            {
                _deliverySection?.ResetGuideState( );
                ResetDisplay( );

                if ( _tutorialModel.SkipCoreTutorial( ) == true )
                    OnCoreTutorialCompleted?.Invoke( );

                RefreshSkipButton( );
                return;
            }

            //실행 중인 후속 가이드가 없으면 일반 게임 화면을 건드리지 않음
            if ( _guideCoordinator.HasActiveGuide == false )
                return;

            ResetDisplay( );
            _guideCoordinator.SkipCurrentGuide( );
            RefreshSkipButton( );
        }
        finally
        {
            _isSkipping = false;
        }
    }

    /// <summary>
    /// 후속 가이드 활성 상태에 맞춰 스킵 버튼 갱신
    /// </summary>
    /// <param name="isActive">후속 가이드 실행 여부</param>
    void HandleGuideActiveChanged ( bool isActive )
    {
        RefreshSkipButton( );
    }

    /// <summary>
    /// 현재 튜토리얼 진행 상태에 맞춰 스킵 버튼 표시
    /// </summary>
    void RefreshSkipButton ()
    {
        if ( _tutorialView == null || _tutorialModel == null )
            return;

        bool hasActiveGuide = _guideCoordinator != null &&
            _guideCoordinator.HasActiveGuide == true;
        bool isVisible = _tutorialModel.IntroCompleted == true &&
            ( _tutorialModel.CoreTutorialCompleted == false ||
            hasActiveGuide == true );

        _tutorialView.SetSkipVisible( isVisible );
    }

    /// <summary>
    /// 인트로 완료 상태 변경 확인
    /// </summary>
    void HandleProgressChanged ()
    {
        RefreshSkipButton( );

        if ( _wasIntroCompleted == true ||
            _tutorialModel.IntroCompleted == false )
        {
            return;
        }

        _wasIntroCompleted = true;
        BeginOrResume( );
    }

    /// <summary>
    /// 다른 대화와 가이드가 없을 때 후속 가이드 시작 가능 여부 확인
    /// </summary>
    /// <returns>후속 가이드 시작 가능 여부</returns>
    bool CanStartGuide ()
    {
        return _isInitialized &&
            _tutorialModel.CoreTutorialCompleted &&
            _isFollowupGuideStartBlocked == false &&
            string.IsNullOrEmpty( _currentDialogueId ) &&
            string.IsNullOrEmpty( _queuedDialogueId ) &&
            _dialoguePresenter.IsPlaying == false;
    }

    /// <summary>
    /// 후속 가이드가 공유하는 화살표와 입력 차단 표시 초기화
    /// </summary>
    void ClearGuideDisplay ()
    {
        if ( _tutorialView != null )
            _tutorialView.HideInstant( );
    }

    /// <summary>
    /// 현재 튜토리얼 표시 상태 초기화
    /// </summary>
    void ResetDisplay ()
    {
        if ( string.IsNullOrEmpty( _currentDialogueId ) == false &&
            _dialoguePresenter != null )
        {
            _dialoguePresenter.CancelDialogue( );
        }

        if ( _tutorialView != null )
            _tutorialView.HideInstant( );

        _currentDialogueId = string.Empty;
        _queuedDialogueId = string.Empty;
    }

    #endregion
}
