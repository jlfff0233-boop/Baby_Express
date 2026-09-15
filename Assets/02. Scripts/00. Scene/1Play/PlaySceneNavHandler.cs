/// <summary>
/// 플레이 씬의 화면 전환과 화면 표시 상태 중재
/// </summary>
public class PlaySceneNavHandler
{
    ActionView _actionView;       //메인 행동 뷰
    ShopPresenter _shopPresenter;       //상점 프레젠터
    InventoryPresenter _inventoryPresenter;       //인벤토리 프레젠터
    OrderPresenter _orderPresenter;       //주문 프레젠터
    CraftPresenter _craftPresenter;       //제작 프레젠터
    DeliveryPresenter _deliveryPresenter;       //배송 프레젠터
    MaintenancePresenter _maintenancePresenter;       //정비 프레젠터
    SettlementPresenter _settlementPresenter;       //결산 프레젠터
    BankruptcyPresenter _bankruptcyPresenter;       //파산 프레젠터
    SideActionPresenter _sideActionPresenter;       //사이드 액션 프레젠터
    EndingPresenter _endingPresenter;       //엔딩 프레젠터
    TutorialModel _tutorialModel;       //Day 1 튜토리얼 진행 상태

    /// <summary>
    /// 플레이 씬 화면 전환 중재 객체 생성
    /// </summary>
    /// <param name="actionView">메인 행동 뷰</param>
    /// <param name="shopPresenter">상점 프레젠터</param>
    /// <param name="inventoryPresenter">인벤토리 프레젠터</param>
    /// <param name="orderPresenter">주문 프레젠터</param>
    /// <param name="craftPresenter">제작 프레젠터</param>
    /// <param name="deliveryPresenter">배송 프레젠터</param>
    /// <param name="maintenancePresenter">정비 프레젠터</param>
    /// <param name="settlementPresenter">결산 프레젠터</param>
    /// <param name="bankruptcyPresenter">파산 프레젠터</param>
    /// <param name="endingPresenter">엔딩 프레젠터</param>
    /// <param name="sideActionPresenter">사이드 액션 프레젠터</param>
    /// <param name="tutorialModel">Day 1 튜토리얼 진행 상태 모델</param>
    public PlaySceneNavHandler (
        ActionView actionView,
        ShopPresenter shopPresenter,
        InventoryPresenter inventoryPresenter,
        OrderPresenter orderPresenter,
        CraftPresenter craftPresenter,
        DeliveryPresenter deliveryPresenter,
        MaintenancePresenter maintenancePresenter,
        SettlementPresenter settlementPresenter,
        BankruptcyPresenter bankruptcyPresenter,
        EndingPresenter endingPresenter,
        SideActionPresenter sideActionPresenter,
        TutorialModel tutorialModel )
    {
        _actionView = actionView;
        _shopPresenter = shopPresenter;
        _inventoryPresenter = inventoryPresenter;
        _orderPresenter = orderPresenter;
        _craftPresenter = craftPresenter;
        _deliveryPresenter = deliveryPresenter;
        _maintenancePresenter = maintenancePresenter;
        _settlementPresenter = settlementPresenter;
        _bankruptcyPresenter = bankruptcyPresenter;
        _endingPresenter = endingPresenter;
        _sideActionPresenter = sideActionPresenter;
        _tutorialModel = tutorialModel;
    }

    #region ----- 이벤트 연결 -----

    /// <summary>
    /// 화면 전환 이벤트 연결
    /// </summary>
    public void ConnectEvents ()
    {
        _actionView.OnShopOpen += OpenShop;
        _actionView.OnInventoryOpen += OpenInventory;
        _actionView.OnCraftOpen += OpenCraftList;
        _actionView.OnOrderOpen += OpenOrder;
        _actionView.OnMaintenanceOpen += OpenMaintenance;

        _shopPresenter.OnPanelClosed += _sideActionPresenter.ShowButtons;
        _inventoryPresenter.OnPanelClosed += _sideActionPresenter.ShowButtons;
        _orderPresenter.OnPanelClosed += _sideActionPresenter.ShowButtons;
        _craftPresenter.OnPanelClosed += _sideActionPresenter.ShowButtons;
        _maintenancePresenter.OnPanelClosed += _sideActionPresenter.ShowButtons;

        _inventoryPresenter.OnMoveToShop += OpenShopItem;
        _sideActionPresenter.OnMoveToShop += OpenShopItem;
        _maintenancePresenter.OnMoveToShop += OpenMaintenanceShop;
        _maintenancePresenter.OnMoveToGuideDetail += OpenMaintenanceGuideDetail;

        _orderPresenter.OnMoveToCraft += OpenCraftOrder;
        _orderPresenter.OnMoveToDelivery += OpenDelivery;
        _orderPresenter.OnMoveToDeliveryResult += OpenDeliveryResult;

        _craftPresenter.OnMoveToDelivery += OpenCraftDelivery;

        _tutorialModel.OnProgressChanged += RefreshTutorialAccess;
        RefreshTutorialAccess( );
    }

    /// <summary>
    /// 화면 전환 이벤트 해제
    /// </summary>
    public void DisconnectEvents ()
    {
        _actionView.OnShopOpen -= OpenShop;
        _actionView.OnInventoryOpen -= OpenInventory;
        _actionView.OnCraftOpen -= OpenCraftList;
        _actionView.OnOrderOpen -= OpenOrder;
        _actionView.OnMaintenanceOpen -= OpenMaintenance;

        _shopPresenter.OnPanelClosed -= _sideActionPresenter.ShowButtons;
        _inventoryPresenter.OnPanelClosed -= _sideActionPresenter.ShowButtons;
        _orderPresenter.OnPanelClosed -= _sideActionPresenter.ShowButtons;
        _craftPresenter.OnPanelClosed -= _sideActionPresenter.ShowButtons;
        _maintenancePresenter.OnPanelClosed -= _sideActionPresenter.ShowButtons;

        _inventoryPresenter.OnMoveToShop -= OpenShopItem;
        _sideActionPresenter.OnMoveToShop -= OpenShopItem;
        _maintenancePresenter.OnMoveToShop -= OpenMaintenanceShop;
        _maintenancePresenter.OnMoveToGuideDetail -=
            OpenMaintenanceGuideDetail;

        _orderPresenter.OnMoveToCraft -= OpenCraftOrder;
        _orderPresenter.OnMoveToDelivery -= OpenDelivery;
        _orderPresenter.OnMoveToDeliveryResult -= OpenDeliveryResult;

        _craftPresenter.OnMoveToDelivery -= OpenCraftDelivery;

        _tutorialModel.OnProgressChanged -= RefreshTutorialAccess;
    }

    #endregion

    #region ----- 메인 화면 -----

    /// <summary>
    /// 상점 패널 열기
    /// </summary>
    void OpenShop ()
    {
        if ( CanOpenFrom( CoreTutorialStep.ShopPurchase ) == false )
            return;

        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _shopPresenter.ShowPanel( );
    }

    /// <summary>
    /// 정비 가이드에서 시설 카테고리 상점 열기
    /// </summary>
    void OpenMaintenanceShop ()
    {
        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HideInstant( );

        _sideActionPresenter.HideForMainPanel( );
        _shopPresenter.ShowFacilityPanel( );
    }

    /// <summary>
    /// 정비 가이드에서 첫 시설 정비 상세 열기
    /// </summary>
    void OpenMaintenanceGuideDetail ()
    {
        _shopPresenter.HideInstant( );
        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _maintenancePresenter.ShowGuideDetail( );
    }

    /// <summary>
    /// 선택한 상품의 상점 상세 열기
    /// </summary>
    /// <param name="itemId">선택한 상품 아이디</param>
    void OpenShopItem ( string itemId )
    {
        if ( CanOpenFrom( CoreTutorialStep.ShopPurchase ) == false )
            return;

        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _shopPresenter.ShowProduct( itemId );
    }

    /// <summary>
    /// 인벤토리 패널 열기
    /// </summary>
    void OpenInventory ()
    {
        if ( CanOpenFrom( CoreTutorialStep.InventoryCheck ) == false )
            return;

        _shopPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _inventoryPresenter.ShowPanel( );
    }

    /// <summary>
    /// 주문 패널 열기
    /// </summary>
    void OpenOrder ()
    {
        if ( CanOpenFrom( CoreTutorialStep.OrderDetail ) == false )
            return;

        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _orderPresenter.ShowPanel( );
    }

    /// <summary>
    /// 제작 주문 목록 열기
    /// </summary>
    void OpenCraftList ()
    {
        //인벤토리 확인 단계는 제작 목록 진입을 기준으로 완료되므로
        //해당 단계부터 제작 화면 진입을 허용
        if ( CanOpenFrom( CoreTutorialStep.InventoryCheck ) == false )
            return;

        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _craftPresenter.OpenCraftList( );
    }

    /// <summary>
    /// 정비 패널 열기
    /// </summary>
    void OpenMaintenance ()
    {
        if ( _tutorialModel.CoreTutorialCompleted == false )
            return;

        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );

        _sideActionPresenter.HideForMainPanel( );
        _maintenancePresenter.ShowPanel( );
    }

    /// <summary>
    /// 불러오기 후 열려 있던 화면을 닫고 메인 화면 입력 복구
    /// </summary>
    public void ResetAfterLoad ()
    {
        _shopPresenter.HideInstant( );
        _inventoryPresenter.HideInstant( );
        _orderPresenter.HideInstant( );
        _craftPresenter.HideInstant( );
        _deliveryPresenter.HideInstant( );
        _maintenancePresenter.HideInstant( );
        _settlementPresenter.HideInstant( );
        _bankruptcyPresenter.HideInstant( );
        _endingPresenter.HideInstant( );

        _sideActionPresenter.ResetAfterLoad( );
        RefreshTutorialAccess( );
    }

    /// <summary>
    /// 현재 Day 1 진행 단계에 따른 화면 접근 상태 갱신
    /// </summary>
    public void RefreshTutorialAccess ()
    {
        bool isCompleted =
            _tutorialModel.CoreTutorialCompleted;

        _actionView.SetTutorialAccess(
            CanOpenFrom( CoreTutorialStep.OrderDetail ),
            CanOpenFrom( CoreTutorialStep.ShopPurchase ),
            CanOpenFrom( CoreTutorialStep.InventoryCheck ),
            //제작 목록을 열어야 인벤토리 확인 단계가 완료됨
            CanOpenFrom( CoreTutorialStep.InventoryCheck ),
            isCompleted );

        _sideActionPresenter.SetTutorialAccess(
            isCompleted );
    }

    /// <summary>
    /// 지정 단계부터 화면 진입 가능 여부 확인
    /// </summary>
    /// <param name="requiredStep">화면 진입에 필요한 최초 단계</param>
    /// <returns>현재 화면 진입 가능 여부</returns>
    bool CanOpenFrom ( CoreTutorialStep requiredStep )
    {
        return _tutorialModel.CoreTutorialCompleted ||
            ( int ) _tutorialModel.CurrentCoreStep >=
            ( int ) requiredStep;
    }

    #endregion

    #region ----- 제작/배송 -----

    /// <summary>
    /// 선택한 주문의 제작 화면 열기
    /// </summary>
    /// <param name="orderId">제작할 주문 아이디</param>
    void OpenCraftOrder ( string orderId )
    {
        if ( CanOpenFrom( CoreTutorialStep.Craft ) == false )
            return;

        CraftActionResult result =
            _craftPresenter.OpenCraft( orderId );

        //제작 진입에 실패하면 기존 화면 유지
        if ( result != CraftActionResult.Success )
            return;

        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );
        _orderPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );
        _sideActionPresenter.HideForMainPanel( );
    }

    /// <summary>
    /// 주문 배송 화면 열기
    /// </summary>
    /// <param name="orderId">배송할 주문 아이디</param>
    void OpenDelivery ( string orderId )
    {
        //제작 결과에서 배송 화면으로 이동해야 배송 단계가 완료됨
        if ( CanOpenFrom( CoreTutorialStep.CraftComplete ) == false )
            return;

        _deliveryPresenter.ShowDelivery( orderId );
    }

    /// <summary>
    /// 완료 주문의 배송 결과 화면 열기
    /// </summary>
    /// <param name="orderId">배송 완료 주문 아이디</param>
    void OpenDeliveryResult ( string orderId )
    {
        _deliveryPresenter.ShowDeliveryResult( orderId );
    }

    /// <summary>
    /// 제작 완료 주문의 배송 화면 열기
    /// </summary>
    /// <param name="orderId">배송할 주문 아이디</param>
    void OpenCraftDelivery ( string orderId )
    {
        //제작 결과에서 배송 화면으로 이동해야 배송 단계가 완료됨
        if ( CanOpenFrom( CoreTutorialStep.CraftComplete ) == false )
            return;

        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );
        _sideActionPresenter.HideForMainPanel( );

        _orderPresenter.ShowProducingPanel( );
        _deliveryPresenter.ShowDelivery( orderId );
    }

    #endregion

    #region ----- 결산/파산 -----

    /// <summary>
    /// 일일 결산 화면 표시
    /// </summary>
    /// <param name="settlementData">표시할 일일 결산 데이터</param>
    public void ShowDailySettlement (
        DailySettlementData settlementData )
    {
        HideForSettlement( );
        _settlementPresenter.ShowDaily( settlementData );
    }

    /// <summary>
    /// 주간 결산 화면 표시
    /// </summary>
    /// <param name="settlementData">표시할 주간 결산 데이터</param>
    public void ShowWeeklySettlement (
        WeeklySettlementData settlementData )
    {
        _settlementPresenter.ShowWeekly( settlementData );
    }

    /// <summary>
    /// 결산 화면을 닫고 메인 화면 입력 복구
    /// </summary>
    public void CloseSettlement ()
    {
        _settlementPresenter.Hide( );
        _sideActionPresenter.ShowButtons( );
    }

    /// <summary>
    /// 다음 영업일 전환 시 결산 화면을 즉시 닫고 메인 입력 복구
    /// </summary>
    public void CloseSettlementForNextDay ()
    {
        _settlementPresenter.HideInstant( );
        _sideActionPresenter.ShowButtons( );
    }

    /// <summary>
    /// 파산 화면 표시
    /// </summary>
    public void ShowBankruptcy ()
    {
        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );

        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );

        _maintenancePresenter.HidePanel( );
        _deliveryPresenter.HideDelivery( );

        _settlementPresenter.Hide( );
        _sideActionPresenter.HideForMainPanel( );
        _actionView.SetInteractable( false );

        _bankruptcyPresenter.ShowBankruptcy( );
    }

    /// <summary>
    /// 결산 표시 전 기존 플레이 화면 정리
    /// </summary>
    void HideForSettlement ()
    {
        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );

        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _deliveryPresenter.HideDelivery( );
        _sideActionPresenter.HideForMainPanel( );
    }

    /// <summary>
    /// 엔딩 총 결산 화면 표시
    /// </summary>
    public void ShowEnding ()
    {
        _shopPresenter.HidePanel( );
        _inventoryPresenter.HidePanel( );

        _orderPresenter.HidePanel( );
        _craftPresenter.HidePanel( );
        _maintenancePresenter.HidePanel( );

        _deliveryPresenter.HideDelivery( );
        _settlementPresenter.Hide( );

        _bankruptcyPresenter.HideBankruptcy( );
        _sideActionPresenter.HideForMainPanel( );

        _actionView.SetInteractable( false );

        _endingPresenter.ShowEnding( );
    }

    /// <summary>
    /// 엔딩 화면을 닫고 메인 화면 입력 복구
    /// </summary>
    public void CloseEnding ()
    {
        _endingPresenter.HideEnding( );
        _actionView.SetInteractable( true );
        _sideActionPresenter.ShowButtons( );
    }

    #endregion
}
