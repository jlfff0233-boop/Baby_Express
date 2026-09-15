using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// 플레이씬 - 플레이 시스템 생성과 연결 및 화면 흐름 중재
/// </summary>
//각 View의 Awake 이후 모델과 프레젠터를 연결하도록 실행 순서 지정
[DefaultExecutionOrder( 100 )]
public class PlayScene : MonoBehaviour
{
    const int MaintenanceProductUnlockDay = 4;

    [Header( "----- 설정 데이터 -----" )]
    [FormerlySerializedAs( "_dataMap" )]
    [SerializeField] PurchasableDataMap _purchasableDataMap;       //구매 가능 상품 데이터 맵
    [SerializeField] AchvDataMap _achvDataMap;       //업적 데이터 맵
    [SerializeField] PlayStartSettingsData _playStartSettings;       //새 게임 초기 설정
    [SerializeField] DaySettingsData _daySettings;       //영업일 기본 설정
    [SerializeField] OrderGenerationSettingsData _orderGenerationSettings;       //주문 생성 설정
    [SerializeField] CraftScoreSettingsData _craftScoreSettings;       //제작 점수 설정
    [SerializeField] DeliverySettingData _deliverySettings;       //배송 설정
    [SerializeField] WeeklySettlementSettingData _weeklySettlementSettings;       //주간 결산 평가 설정
    [SerializeField] EmployeeSettingsData _employeeSettings;       //직원 공용 설정
    [SerializeField] TutorialDay1Data _tutorialDay1Data;       //Day 1 튜토리얼 설정
    [SerializeField] TutorialGuideData _tutorialGuideData;       //Day 2~7 후속 가이드 설정

    [Header( "----- 프레젠터 -----" )]
    [SerializeField] IntroPresenter _introPresenter;       //인트로 프레젠터
    [SerializeField] TopBarPresenter _topBarPresenter;       //상단바 프레젠터
    [SerializeField] ShopPresenter _shopPresenter;      //상점 프레젠터
    [SerializeField] InventoryPresenter _inventoryPresenter;        //인벤토리 프레젠터
    [SerializeField] OrderPresenter _orderPresenter;      //주문 프레젠터
    [SerializeField] CraftPresenter _craftPresenter;      //제작 프레젠터
    [SerializeField] DeliveryPresenter _deliveryPresenter;       //배송 프레젠터
    [SerializeField] DayPresenter _dayPresenter;       //영업일 프레젠터
    [SerializeField] SettlementPresenter _settlementPresenter;       //결산 프레젠터
    [SerializeField] BankruptcyPresenter _bankruptcyPresenter;       //파산 프레젠터
    [SerializeField] MaintenancePresenter _maintenancePresenter;       //정비 프레젠터
    [SerializeField] SideActionPresenter _sideActionPresenter;       //사이드 액션 프레젠터
    [SerializeField] SavePresenter _savePresenter;       //수동 저장 중재
    [SerializeField] EndingPresenter _endingPresenter;       //엔딩 프레젠터
    [SerializeField] MascotPresenter _mascotPresenter;       //메인 마스코트 프레젠터
    [SerializeField] TutorialPresenter _tutorialPresenter;       //Day 1 튜토리얼 프레젠터

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] ActionView _actionView;       //메인 행동 뷰

    #region ---- 모델 -----
    PlayStateModel _playStateModel;       //상품 해금과 공용 플레이 상태

    ShopModel _shopModel;                 //상점 모델
    CartModel _cartModel;                 //장바구니 모델
    PurchaseModel _purchaseModel;         //구매 모델
    QuickRestockModel _quickRestockModel;       //빠른 재입고 모델

    InventoryModel _inventoryModel;       //인벤토리 모델
    InventoryItemActionModel _itemActionModel;              //인벤토리 판매와 삭제 모델

    CustomerOrderModel _customerOrderModel;       //고객 주문 모델
    OrderGeneratorModel _orderGeneratorModel;       //주문 생성 모델
    DeliveryModel _deliveryModel;       //배송 모델

    CraftModel _craftModel;       //제작 모델
    CraftReviewModel _craftReviewModel;      //제작 판정 모델
    CraftCompleteModel _craftCompleteModel;      //제작 확정 모델
    CraftReviewViewBuilder _craftReviewViewBuilder;       //제작 판정 표시 데이터 생성기

    BusinessDayModel _businessDayModel;       //영업일 모델
    DailyRecordModel _dailyRecordModel;       //일일 기록 모델
    SettlementModel _settlementModel;       //결산 모델
    BankruptcyModel _bankruptcyModel;       //파산 모델

    AchvModel _achvModel;       //업적 상태와 달성 모델
    AchvRewardProcessor _achvRewardProcessor;       //업적 보상 처리기

    MaintenanceModel _maintenanceModel;       //정비 모델
    MaintenanceEffectModel _maintenanceEffectModel;       //정비 효과 모델
    MaintenanceUpgradeModel _maintenanceUpgradeModel;       //정비 업그레이드 모델
    EmployeeModel _employeeModel;       //직원 고용과 배치 상태 모델
    TutorialModel _tutorialModel;       //인트로와 튜토리얼 진행 상태

    PlaySceneNavHandler _sceneNavHandler;       //플레이 씬 화면 전환 처리
    PlaySceneRecordHandler _sceneRecordHandler;       //플레이 씬 일일 기록 처리
    PlaySceneDayHandler _sceneDayHandler;       //플레이 씬 결산과 영업일 전환 처리
    PlaySceneSaveHandler _sceneSaveHandler;       //플레이 씬 저장 처리
    PlaySceneLoadHandler _sceneLoadHandler;       //불러오기 후 화면 복구 처리
    PlaySceneStartHandler _sceneStartHandler;       //플레이 씬 시작 처리

    #endregion

    #region ----- 시작 -----
    /// <summary>
    /// 활성 풀 오브젝트를 반환하고 지정 씬으로 이동
    /// </summary>
    /// <param name="sceneName">이동할 씬 이름</param>
    void LoadSceneAfterPoolReturn ( string sceneName )
    {
        GameManager.Instance.PoolManager.ReturnAll( );
        SceneManager.LoadScene( sceneName );
    }

    /// <summary>
    /// 플레이 시스템 생성 및 연결
    /// </summary>
    void Awake ()
    {
        CreateModels( );
        InitPresenters( );
        CreateHandlers( );

        SaveResult startResult =
            _sceneStartHandler.ApplyStartRequest( );

        if ( startResult != SaveResult.Success )
        {
            Debug.LogWarning(
                $"플레이 시작 실패: {startResult}" );

            LoadSceneAfterPoolReturn( "Title" );
            return;
        }

        RefreshMaintenanceProductUnlocks( );
        _introPresenter.TryBeginIntro( );
        RefreshMascotState( );
    }

    /// <summary>
    /// 화면 흐름 이벤트 연결 이후 튜토리얼 시작 또는 복구
    /// </summary>
    void Start ()
    {
        ResumeTutorial( );
    }

    /// <summary>
    /// 화면 이동 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        _sceneNavHandler.ConnectEvents( );
        _sceneRecordHandler.ConnectEvents( );
        _sceneDayHandler.ConnectEvents( );

        //튜토리얼 단계 변경 즉시 현재 슬롯에 저장
        _tutorialModel.OnProgressChanged += _sceneSaveHandler.AutoSave;

        //파산 화면의 다시 시작 입력 연결
        _bankruptcyPresenter.OnRestartRequested += RestartGame;
        //불러오기 이벤트 중재
        _bankruptcyPresenter
            .OnRestartCurrentWeekRequested += RestartCurrentWeek;
        //시작 화면 이동 이벤트 연결
        _bankruptcyPresenter.OnTitleRequested += MoveToTitle;

        _savePresenter.OnLoaded += RefreshAfterLoad;
        _playStateModel.OnDateChanged += RefreshMaintenanceProductUnlocks;
        _sceneDayHandler.OnNextDayStarted += _sceneSaveHandler.AutoSave;
        _sceneDayHandler.OnNextDayTransitionChanged +=
            _tutorialPresenter.SetFollowupGuideStartBlocked;

        //인트로 종료 후 튜토리얼 상태에 맞춰 마스코트 팁 갱신
        _introPresenter.OnCompleted += CompleteIntro;

        //핵심 튜토리얼 완료 후 마스코트 팁 출력 재개
        _tutorialPresenter.OnCoreTutorialCompleted +=
            RefreshMascotState;

        //공용 대화 중 마스코트 뿅 퇴장과 등장 처리
        _tutorialPresenter.OnDialogueToggled += SetMascotDialogueHidden;

        //엔딩 이후 입력 연결
        _endingPresenter.OnContinueRequested +=
            _sceneDayHandler.ContinueAfterEnding;
        _endingPresenter.OnRestartRequested += RestartGame;
        _endingPresenter.OnTitleRequested += MoveToTitle;
    }

    /// <summary>
    /// 화면 이동 이벤트 해제
    /// </summary>
    void OnDisable ()
    {
        _sceneDayHandler.DisconnectEvents( );
        _sceneRecordHandler.DisconnectEvents( );
        _sceneNavHandler.DisconnectEvents( );

        //튜토리얼 진행 상태 저장 요청 해제
        _tutorialModel.OnProgressChanged -= _sceneSaveHandler.AutoSave;

        //파산 화면의 다시 시작 입력 해제
        _bankruptcyPresenter.OnRestartRequested -= RestartGame;
        _bankruptcyPresenter
            .OnRestartCurrentWeekRequested -= RestartCurrentWeek;
        _bankruptcyPresenter.OnTitleRequested -= MoveToTitle;

        _savePresenter.OnLoaded -= RefreshAfterLoad;
        _playStateModel.OnDateChanged -= RefreshMaintenanceProductUnlocks;
        _sceneDayHandler.OnNextDayStarted -= _sceneSaveHandler.AutoSave;
        _sceneDayHandler.OnNextDayTransitionChanged -=
            _tutorialPresenter.SetFollowupGuideStartBlocked;

        //인트로 완료 연결 해제
        _introPresenter.OnCompleted -= CompleteIntro;

        //핵심 튜토리얼 완료 연결 해제
        _tutorialPresenter.OnCoreTutorialCompleted -=
            RefreshMascotState;

        //공용 대화의 마스코트 표시 연결 해제
        _tutorialPresenter.OnDialogueToggled -= SetMascotDialogueHidden;

        //엔딩 이후 입력 해제
        _endingPresenter.OnContinueRequested -=
            _sceneDayHandler.ContinueAfterEnding;
        _endingPresenter.OnRestartRequested -= RestartGame;
        _endingPresenter.OnTitleRequested -= MoveToTitle;
    }

    /// <summary>
    /// 불러온 화면과 인트로 진행 상태 복구
    /// </summary>
    void RefreshAfterLoad ()
    {
        //일반 플레이 화면을 먼저 복구
        _sceneLoadHandler.RefreshAfterLoad( );
        RefreshMaintenanceProductUnlocks( );

        //불러온 인트로 완료 기억값을 실제 저장 상태와 동기화
        _tutorialPresenter.RefreshIntroState( );

        //미완료 인트로를 먼저 요청한 뒤 완료된 튜토리얼 단계 복구
        _introPresenter.TryBeginIntro( );
        RefreshMascotState( );
        ResumeTutorial( );
    }

    /// <summary>
    /// 인트로 완료 후 튜토리얼 상태에 맞춰 마스코트 팁 갱신
    /// </summary>
    void CompleteIntro ()
    {
        RefreshMascotState( );
        ResumeTutorial( );
    }

    /// <summary>
    /// 인트로와 핵심 튜토리얼 상태에 맞춰 마스코트 동작 갱신
    /// </summary>
    void RefreshMascotState ()
    {
        bool isIntroPlaying =
            _tutorialModel.IntroCompleted == false;

        bool isTutorialPlaying =
            _tutorialModel.CoreTutorialCompleted == false;

        _mascotPresenter.SetTipPaused(
            isIntroPlaying || isTutorialPlaying );

        _mascotPresenter.SetIntroPaused(
            isIntroPlaying );
    }

    /// <summary>
    /// 공용 대화 활성화 상태에 맞춰 마스코트 표시 변경
    /// </summary>
    /// <param name="isDialoguePlaying">현재 대화 재생 여부</param>
    void SetMascotDialogueHidden ( bool isDialoguePlaying )
    {
        _mascotPresenter.SetDialogueHidden( isDialoguePlaying );
    }

    /// <summary>
    /// 현재 튜토리얼 단계와 결산 화면 복구
    /// </summary>
    void ResumeTutorial ()
    {
        _tutorialPresenter.BeginOrResume( );

        //결산 단계 저장 복구 시 실제 결산 화면도 다시 요청
        if ( _tutorialModel.CurrentCoreStep ==
            CoreTutorialStep.Settlement &&
            _settlementPresenter.IsShowingDaily == false )
        {
            _dayPresenter.TryStartSettlement( );
        }
    }

    /// <summary>
    /// 플레이에 사용할 모델 생성
    /// </summary>
    void CreateModels ()
    {
        //공통 플레이 상태 생성
        _playStateModel = new PlayStateModel(
            _playStartSettings.Time,
            _playStartSettings.TotalDay,
            _playStartSettings.HighGradeEvaluationCount,
            _playStartSettings.Budget );

        //직원 고용과 배치 상태 생성
        _employeeModel = new EmployeeModel(
            _employeeSettings.DataMap.Datas,
            _playStateModel, _employeeSettings.MaxEmployeeCount );

        //인트로와 튜토리얼 진행 상태 생성
        _tutorialModel = new TutorialModel( );

        //일일 제작 할당량 상태 생성
        _businessDayModel = new BusinessDayModel(
            0, _daySettings.CraftLimit,
            0, _daySettings.QuickRestockLimit,
            _employeeModel );

        //Day 1만 튜토리얼 제작 할당량 사용
        _businessDayModel.SetCurrentCraftLimit(
            _tutorialDay1Data.CraftLimit );

        //현재 영업일의 제작/주문/거래 기록 생성
        _dailyRecordModel = new DailyRecordModel(
            _playStateModel.TotalDay, _playStateModel.Budget,
            _businessDayModel.CraftLimit, _businessDayModel.QuickRestockLimit );

        //일일 기록을 일일, 주간 결산 데이터로 집계하는 결산 모델 생성
        _settlementModel = new SettlementModel( _weeklySettlementSettings );

        //업적 상태와 수동 보상 처리기 생성
        _achvModel = new AchvModel( _achvDataMap );
        _achvRewardProcessor = new AchvRewardProcessor(
            _achvModel, _playStateModel, _dailyRecordModel, _purchasableDataMap );

        //정비 단계와 적용 상태를 관리하는 정비 모델 생성
        _maintenanceModel = new MaintenanceModel( _purchasableDataMap );

        //초기 상품 해금 상태 적용
        InitUnlockedProducts( );
        _playStateModel.MarkAllUnlocksViewed( );

        //상점과 장바구니 모델 생성
        _shopModel = new ShopModel(
            _purchasableDataMap, _playStateModel.TotalDay );
        _cartModel = new CartModel( );

        //선택 상품 빠른 재입고 모델 생성
        _quickRestockModel = new QuickRestockModel(
            _shopModel, _playStateModel, _businessDayModel,
            _employeeModel, _daySettings.QuickRestockFeeRate );

        //인벤토리 모델 생성
        _inventoryModel = new InventoryModel( );
        _itemActionModel = new InventoryItemActionModel(
            _inventoryModel, _playStateModel, _employeeModel );

        //고객 주문 모델 생성
        _customerOrderModel = new CustomerOrderModel( );

        //주문 생성 모델 생성
        _orderGeneratorModel = new OrderGeneratorModel(
            _orderGenerationSettings, _employeeModel );

        //제작 모델 생성
        _craftModel = new CraftModel( );

        //제작 점수 설정과 현재 정비 단계를 사용하는 판정 모델 생성
        _craftReviewModel = new CraftReviewModel( _craftScoreSettings, _maintenanceModel );

        //제작 확정 모델 생성
        _craftCompleteModel = new CraftCompleteModel(
            _craftModel, _craftReviewModel, _inventoryModel, _customerOrderModel );

        //제작 판정 표시 데이터 생성기 생성
        _craftReviewViewBuilder = new CraftReviewViewBuilder(
            _shopModel, _purchasableDataMap, _craftScoreSettings );

        //배송 예상과 확정을 처리하는 배송 모델 생성
        _deliveryModel = new DeliveryModel(
            _deliverySettings, _craftScoreSettings,
            _customerOrderModel, _craftCompleteModel,
            _playStateModel, _employeeModel, _purchasableDataMap );

        //정비 효과 적용 모델 생성
        _maintenanceEffectModel = new MaintenanceEffectModel(
            _maintenanceModel, _inventoryModel,
            _shopModel, _customerOrderModel,
            _businessDayModel, _deliveryModel, _playStateModel );

        //정비 상위 단계 구매 모델 생성
        _maintenanceUpgradeModel = new MaintenanceUpgradeModel(
            _maintenanceModel, _maintenanceEffectModel,
            _playStateModel, _employeeModel );

        //현재 가격과 장바구니 수량을 사용하는 구매 모델 생성
        _purchaseModel = new PurchaseModel(
            _shopModel, _cartModel, _inventoryModel,
            _playStateModel, _maintenanceModel, _maintenanceEffectModel );

        //진행 주문의 최소 조립 가능성을 계산하는 파산 모델 생성
        _bankruptcyModel = new BankruptcyModel(
            _customerOrderModel, _inventoryModel,
            _shopModel, _quickRestockModel, _playStateModel );
    }

    /// <summary>
    /// 프레젠터에 공용 모델 연결
    /// </summary>
    void InitPresenters ()
    {
        //메인 화면 마스코트 연결
        _mascotPresenter.Init( );

        _introPresenter.Init( _tutorialModel );

        _shopPresenter.Init(
            _shopModel, _cartModel, _purchaseModel,
            _playStateModel, _quickRestockModel, _maintenanceModel );

        //인벤토리 프레젠터 연결
        _inventoryPresenter.Init( _inventoryModel, _itemActionModel );

        //상단바 프레젠터 연결
        _topBarPresenter.Init(
            _playStateModel, _businessDayModel,
            _customerOrderModel, _orderGeneratorModel,
            GameManager.Instance.AudioManager );

        //주문 프레젠터 연결
        _orderPresenter.Init(
            _customerOrderModel, _playStateModel, _inventoryModel,
            _shopModel, _orderGeneratorModel, _craftCompleteModel,
            _tutorialModel, _tutorialDay1Data );

        //정비 가이드 연결 전에 정비 프레젠터 초기화
        _maintenancePresenter.Init(
            _maintenanceModel, _maintenanceUpgradeModel,
            _shopModel, _playStateModel, _settlementModel, _employeeModel );

        //튜토리얼과 후속 가이드 진행 연결
        _tutorialPresenter.Init(
            _tutorialModel, _tutorialDay1Data,
            _tutorialGuideData, _playStateModel,
            _actionView,
            _orderPresenter,
            _shopPresenter, _inventoryPresenter,
            _craftPresenter, _deliveryPresenter,
            _settlementPresenter, _settlementModel,
            _maintenancePresenter,
            _sideActionPresenter,
            _achvModel,
            _shopModel,
            _inventoryModel,
            _customerOrderModel );

        //제작 프레젠터 연결
        _craftPresenter.Init(
            _craftModel, _craftReviewModel, _craftCompleteModel,
            _inventoryModel, _customerOrderModel,
            _playStateModel, _businessDayModel, _shopModel,
            _craftReviewViewBuilder );

        //배송 프레젠터 연결
        _deliveryPresenter.Init(
            _deliveryModel, _customerOrderModel,
            _craftCompleteModel, _playStateModel,
            _craftReviewViewBuilder, _employeeModel );

        //영업일 프레젠터 연결
        _dayPresenter.Init( _businessDayModel, _customerOrderModel, _bankruptcyModel );

        //파산 프레젠터 연결
        _bankruptcyPresenter.Init( );

        //결산 프레젠터 연결
        _settlementPresenter.Init(
            _playStateModel, _customerOrderModel, _orderGeneratorModel );

        //사이드 액션 프레젠터 연결
        _sideActionPresenter.Init(
            _settlementModel, _settlementPresenter,
            _playStateModel,
            _purchasableDataMap, _inventoryModel, _craftScoreSettings,
            _achvDataMap, _achvModel,
            _achvRewardProcessor, _dailyRecordModel,
            _savePresenter );

        //엔딩 프레젠터 연결
        _endingPresenter.Init(
            _playStateModel, _dailyRecordModel, _settlementModel,
            _employeeModel, _achvModel, _achvDataMap );
    }

    /// <summary>
    /// 플레이 씬 처리 객체 생성
    /// </summary>
    void CreateHandlers ()
    {
        _sceneNavHandler = new PlaySceneNavHandler(
            _actionView, _shopPresenter, _inventoryPresenter,
            _orderPresenter, _craftPresenter, _deliveryPresenter,
            _maintenancePresenter, _settlementPresenter, _bankruptcyPresenter,
            _endingPresenter, _sideActionPresenter,
            _tutorialModel );

        _sceneRecordHandler = new PlaySceneRecordHandler(
            _dailyRecordModel, _purchaseModel, _itemActionModel,
            _quickRestockModel, _craftCompleteModel, _deliveryModel,
            _customerOrderModel, _employeeModel );

        _sceneDayHandler = new PlaySceneDayHandler(
            _playStateModel, _businessDayModel, _dailyRecordModel,
            _settlementModel, _customerOrderModel, _orderGeneratorModel,
            _deliveryModel, _employeeModel, _maintenanceEffectModel,
            _shopModel, _achvModel, _dayPresenter,
            _settlementPresenter, _orderPresenter, _deliveryPresenter, _sceneNavHandler );

        //현재 시스템 상태를 저장 파일 데이터로 변환하는 생성기 구성
        var saveDataBuilder = new PlaySceneSaveDataBuilder(
            _playStateModel, _businessDayModel, _achvModel,
            _inventoryModel, _shopModel, _customerOrderModel,
            _orderGeneratorModel, _craftCompleteModel,
            _deliveryModel, _dailyRecordModel, _settlementModel,
            _maintenanceModel, _employeeModel, _tutorialModel );

        //저장 데이터 전체 검증과 시스템별 복구 처리기 구성
        var saveRestorer = new PlaySceneSaveRestorer(
            _playStateModel, _businessDayModel, _achvModel,
            _inventoryModel, _shopModel, _purchasableDataMap,
            _customerOrderModel, _orderGeneratorModel,
            _craftCompleteModel, _deliveryModel,
            _dailyRecordModel, _settlementModel,
            _maintenanceModel, _employeeModel, _tutorialModel );

        //파일 입출력과 슬롯 흐름만 담당하는 저장 핸들러 구성
        _sceneSaveHandler = new PlaySceneSaveHandler(
            GameManager.Instance.SaveManager,
            _playStateModel,
            saveDataBuilder,
            saveRestorer );

        //파산 화면의 주간 시작 상태 복구 가능 여부 연결
        _bankruptcyPresenter.SetWeekRestartCheck(
            _sceneSaveHandler.CanRestartCurrentWeek );

        _sceneLoadHandler = new PlaySceneLoadHandler(
            _sceneNavHandler, _topBarPresenter );

        _sceneStartHandler = new PlaySceneStartHandler(
            GameManager.Instance, _sceneSaveHandler, _sceneLoadHandler );

        _savePresenter.Init(
            GameManager.Instance.SaveManager, _sceneSaveHandler );

    }

    /// <summary>
    /// 새 게임 초기 해금 상품 설정
    /// </summary>
    void InitUnlockedProducts ()
    {
        IReadOnlyList<PurchasableData> products =
            _playStartSettings.UnlockedProductMap.PurchasableDatas;

        //새 게임 초기 설정에 등록된 상품 해금
        for ( int i = 0; i < products.Count; i++ )
        {
            PurchasableData data = products [ i ];

            if ( data != null )
                _playStateModel.SetUnlocked( data.Id, true );
        }
    }

    /// <summary>
    /// 현재 날짜가 정비 소개 시점이면 정비 상품을 해금
    /// </summary>
    void RefreshMaintenanceProductUnlocks ()
    {
        if ( _playStateModel.TotalDay < MaintenanceProductUnlockDay )
            return;

        IReadOnlyList<PurchasableData> products =
            _purchasableDataMap.PurchasableDatas;

        for ( int i = 0; i < products.Count; i++ )
        {
            if ( products [ i ] is MaintenanceData data )
                _playStateModel.SetUnlocked( data.Id, true );
        }
    }

    /// <summary>
    /// 날짜 변경 후 정비 상품 해금 상태 갱신
    /// </summary>
    /// <param name="month">변경된 월</param>
    /// <param name="day">변경된 일</param>
    void RefreshMaintenanceProductUnlocks ( int month, int day )
    {
        RefreshMaintenanceProductUnlocks( );
    }
    #endregion

    #region ----- 게임 오버 -----

    /// <summary>
    /// 실제 파산 상태 생성과 판정 검증
    /// </summary>
    [ContextMenu( "파산 상태 생성 및 검증" )]
    void DebugCreateBankruptcyState ()
    {
        if ( Application.isPlaying == false )
        {
            Debug.Log( "Play Mode에서 확인해 주세요." );
            return;
        }

        //진행 주문이 없으면 현재 대기 주문 생성
        if ( _customerOrderModel.GetOrderCount(
            OrderProgressState.Production ) == 0 &&
            _customerOrderModel.GetOrderCount(
            OrderProgressState.Waiting ) == 0 )
            _orderPresenter.GenerateDailyOrders( );

        //수락 가능한 주문을 모두 진행 주문으로 전환
        IReadOnlyList<CustomerOrder> waitingOrders =
            _customerOrderModel.GetOrders( OrderProgressState.Waiting );

        for ( int i = 0; i < waitingOrders.Count; i++ )
            _customerOrderModel.AcceptOrder(
                waitingOrders [ i ].OrderId, _playStateModel.TotalDay );

        //진행 주문을 만들 수 없으면 파산 상태 생성을 중단
        if ( _customerOrderModel.GetOrderCount(
            OrderProgressState.Production ) == 0 )
        {
            Debug.LogWarning(
                "[파산 상태 생성 실패] 진행 주문을 만들 수 없습니다." );
            return;
        }

        //현재 보유한 모든 파츠 제거 요청 생성
        var removeParts = new List<InventoryItemAmount>( );

        foreach ( InventoryItem item in _inventoryModel.Items )
        {
            if ( item.Data is PartsData )
                removeParts.Add(
                    new InventoryItemAmount( item.Data, item.Quantity ) );
        }

        //현재 보유 파츠 전체 제거
        if ( removeParts.Count > 0 )
            _inventoryModel.RemoveItems( removeParts );

        //상점의 모든 파츠를 품절 상태로 변경
        foreach ( ShopItemModel itemModel in _shopModel.Items )
        {
            if ( itemModel.Item.Data is PartsData )
                itemModel.Item.SetStock( 0 );
        }

        //오늘 빠른 재입고 횟수 모두 소진
        while ( _businessDayModel.CanQuickRestock )
            _businessDayModel.AddQuickRestock( );

        //일반 구매도 불가능하도록 현재 자금 제거
        _playStateModel.SetBudget( 0f );

        BankruptcyResult result =
            _bankruptcyModel.CheckBankruptcy( );

        //파산 판정에 사용한 전체 상태 출력
        Debug.Log( CreateBankruptcyStateLog( result ) );

        //정상 영업 상태 검사로 실제 파산 화면 요청
        _dayPresenter.TryStartSettlement( );
    }

    /// <summary>
    /// 파산 검증용 전체 상태 문구 생성
    /// </summary>
    /// <param name="result">최종 파산 판정 결과</param>
    /// <returns>파산 검증용 전체 상태 문구</returns>
    string CreateBankruptcyStateLog ( BankruptcyResult result )
    {
        var text = new StringBuilder( );

        text.AppendLine( "[파산 상태 생성 완료]" );
        text.AppendLine( $"현재 자금: {_playStateModel.Budget:N0}G" );
        text.AppendLine( );

        //주문 진행 상태별 개수 출력
        text.AppendLine( "[주문 상태]" );
        text.AppendLine(
            $"수락 대기: {_customerOrderModel.GetOrderCount( OrderProgressState.Waiting )}" );
        text.AppendLine(
            $"진행 중: {_customerOrderModel.GetOrderCount( OrderProgressState.Production )}" );
        text.AppendLine(
            $"제작 완료: {_customerOrderModel.GetOrderCount( OrderProgressState.Crafted )}" );
        text.AppendLine(
            $"종료: {_customerOrderModel.GetOrderCount( OrderProgressState.Closed )}" );
        text.AppendLine( );

        //인벤토리 전체 아이템과 파츠 수량 집계
        int inventoryQuantity = 0;
        int partKindCount = 0;
        int partQuantity = 0;
        var inventoryItems = new StringBuilder( );

        foreach ( InventoryItem item in _inventoryModel.Items )
        {
            inventoryQuantity += item.Quantity;
            inventoryItems.AppendLine(
                $"- {item.Data.Name}({item.Data.Id}): {item.Quantity}개" );

            if ( item.Data is PartsData )
            {
                partKindCount++;
                partQuantity += item.Quantity;
            }
        }

        text.AppendLine( "[인벤토리 상태]" );
        text.AppendLine( $"전체 아이템 종류: {_inventoryModel.Count}" );
        text.AppendLine( $"전체 아이템 수량: {inventoryQuantity}" );
        text.AppendLine( $"보유 파츠 종류: {partKindCount}" );
        text.AppendLine( $"보유 파츠 수량: {partQuantity}" );
        text.Append( inventoryItems.Length > 0
            ? inventoryItems.ToString( )
            : "- 보유 아이템 없음\n" );
        text.AppendLine( );

        //상점 파츠별 현재 재고와 해금 상태 출력
        int shopPartCount = 0;
        int soldOutPartCount = 0;
        int totalPartStock = 0;
        var shopParts = new StringBuilder( );

        foreach ( ShopItemModel itemModel in _shopModel.Items )
        {
            ShopItem item = itemModel.Item;

            if ( item.Data is not PartsData ) continue;

            shopPartCount++;
            totalPartStock += item.RemainingStock;

            if ( item.IsSoldOut ) soldOutPartCount++;

            shopParts.AppendLine(
                $"- {item.Data.Name}({item.Id}): " +
                $"재고 {item.RemainingStock} / {item.MaxStock}, " +
                $"가격 {item.CurrentPrice:N0}G, " +
                $"해금 {_playStateModel.IsUnlocked( item.Id )}" );
        }

        text.AppendLine( "[상점 파츠 재고]" );
        text.AppendLine( $"전체 파츠 상품: {shopPartCount}" );
        text.AppendLine( $"품절 파츠 상품: {soldOutPartCount}" );
        text.AppendLine( $"전체 남은 재고: {totalPartStock}" );
        text.Append( shopParts.Length > 0
            ? shopParts.ToString( )
            : "- 파츠 상품 없음\n" );
        text.AppendLine( );

        //빠른 재입고 사용 상태 출력
        int remainingRestockCount =
            _quickRestockModel.DailyLimit -
            _quickRestockModel.TodayCount;

        text.AppendLine( "[빠른 재입고 상태]" );
        text.AppendLine(
            $"사용 횟수: {_quickRestockModel.TodayCount} / {_quickRestockModel.DailyLimit}" );
        text.AppendLine( $"남은 횟수: {remainingRestockCount}" );
        text.AppendLine( $"추가 이용 가능: {_businessDayModel.CanQuickRestock}" );
        text.AppendLine( );

        text.AppendLine( "[최종 판정]" );
        text.Append( result );

        return text.ToString( );
    }

    /// <summary>
    /// 플레이를 처음부터 다시 시작
    /// </summary>
    void RestartGame ()
    {
        int slotNumber =
            GameManager.Instance.SaveManager.ActiveSlotNumber;

        if ( slotNumber <= 0 )
        {
            Debug.LogWarning(
                "처음부터 다시 시작할 저장 슬롯이 없습니다." );
            return;
        }

        //현재 슬롯을 사용하는 새 게임 시작 요청 보관
        GameManager.Instance.SetPlayStartRequest(
            new PlayStartRequest( PlayStartMode.NewGame, slotNumber ) );

        LoadSceneAfterPoolReturn(
            SceneManager.GetActiveScene( ).name );
    }

    /// <summary>
    /// 현재 주의 시작 상태로 복구
    /// </summary>
    void RestartCurrentWeek ()
    {
        SaveResult result =
            _sceneSaveHandler.RestartCurrentWeek( );

        if ( result != SaveResult.Success )
        {
            Debug.LogWarning(
                $"이번 주부터 다시 시작 실패: {result}" );
            return;
        }

        //복구된 모델 상태를 플레이 화면에 반영
        _sceneLoadHandler.RefreshAfterLoad( );
    }

    /// <summary>
    /// 현재 저장 데이터를 유지하고 시작 화면으로 이동
    /// </summary>
    void MoveToTitle ()
    {
        LoadSceneAfterPoolReturn( "Title" );
    }
    #endregion

    #region ----- 검증 -----
#if UNITY_EDITOR
    /// <summary>
    /// 숫자키 3으로 현재 영업일 전체 처리를 완료하고 다음 영업일 시작
    /// </summary>
    void Update ()
    {
        if ( Input.GetKeyDown( KeyCode.Alpha3 ) )
            _sceneDayHandler.DebugStartNextDay( );
    }

    /// <summary>
    /// 모든 업적 달성과 결산 이후 엔딩 표시 검증
    /// </summary>
    [ContextMenu( "전체 업적 달성 및 엔딩 결산 검증" )]
    void DebugEndingSettlement ()
    {
        if ( Application.isPlaying == false )
        {
            Debug.Log( "Play Mode에서 확인해 주세요." );
            return;
        }

        _sceneDayHandler.DebugOpenEndingSettlement( );
    }
#endif

    #endregion

}
