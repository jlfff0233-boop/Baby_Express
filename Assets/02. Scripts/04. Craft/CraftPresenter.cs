using System;
using UnityEngine;

/// <summary>
/// 제작 프레젠터 - 제작 뷰와 기능 간 결과 중재
/// </summary>
public class CraftPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] CraftView _craftView;                  //제작 뷰
    [SerializeField] CraftListView _craftListView;          //제작 주문 목록 뷰
    [SerializeField] PartsSelectView _partsSelectView;      //파츠 선택 뷰
    [SerializeField] UsedPartsListView _usedPartsListView;  //사용 파츠 목록 뷰
    [SerializeField] CraftOrderDetailView _orderDetailView; //제작 주문 상세 뷰
    [SerializeField] CraftPreviewView _previewView;         //제작 테마와 점수 미리보기 뷰
    [SerializeField] CraftCompleteView _completeView;       //제작 완료 뷰
    [SerializeField] OrderIconData _orderIconData;          //주문 공용 상태 아이콘 데이터

    CraftModel _craftModel;                    //제작 모델
    CustomerOrderModel _orderModel;            //주문 모델

    CraftOrderListPresenter _orderListPresenter;       //제작 주문 목록 프레젠터
    CraftPartPresenter _partPresenter;                   //파츠 편집 프레젠터
    CraftPartListPresenter _partListPresenter;           //파츠 목록 프레젠터
    CraftInfoPresenter _infoPresenter;                   //제작 정보 프레젠터
    CraftCompletePresenter _completePresenter;           //제작 확정 프레젠터

    CustomerOrder _selectedOrder;              //현재 제작 주문
    bool _isInitialized;                       //모델 전달 완료 여부
    bool _isSubscribed;                        //이벤트 연결 여부

    /// <summary>
    /// 제작 완료 주문 배송 화면 이동 이벤트
    /// </summary>
    public event Action<string> OnMoveToDelivery;

    /// <summary>
    /// 제작 확정 성공 이벤트
    /// </summary>
    public event Action<string> OnCraftSucceeded;

    /// <summary>
    /// 제작 화면 닫기 이벤트
    /// </summary>
    public event Action OnPanelClosed;

    /// <summary>
    /// 제작 주문 목록 표시 이벤트
    /// </summary>
    public event Action OnListOpened;

    /// <summary>
    /// 제작 목록 주문 선택 이벤트
    /// </summary>
    public event Action<string> OnOrderSelected;

    /// <summary>
    /// 제작 주문 편집 화면 진입 이벤트
    /// </summary>
    public event Action<string> OnCraftOpened;

    /// <summary>
    /// 제작 시작 요청 이벤트
    /// </summary>
    public event Action<string> OnCraftStartRequested;

    /// <summary>
    /// 제작 시작 실패 이벤트
    /// </summary>
    public event Action<string> OnCraftStartFailed;

    /// <summary>
    /// 제작 파츠 배치 완료 이벤트
    /// </summary>
    public event Action<string, PartType, int> OnPartPlaced;

    /// <summary>
    /// 제작 파츠 이동 완료 이벤트
    /// </summary>
    public event Action<string, PartType, int> OnPartDragCompleted;

    /// <summary>
    /// 제작 파츠 크기 변경 완료 이벤트
    /// </summary>
    public event Action<string, PartType, int> OnPartScaled;

    /// <summary>
    /// 파츠 선택 패널 열림 연출 완료 이벤트
    /// </summary>
    public event Action OnPartsSelectPanelOpened;

    /// <summary>
    /// 제작 정보 패널 열림 연출 완료 이벤트
    /// </summary>
    public event Action OnCraftInfoPanelOpened;

    /// <summary>
    /// 제작 정보 패널의 사용 파츠 표시 이벤트
    /// </summary>
    public event Action OnUsedPartsShown;

    /// <summary>
    /// 현재 파츠 선택 패널 열림 여부
    /// </summary>
    public bool IsPartsSelectOpen => _craftView.IsPartsSelectOpen;

    /// <summary>
    /// 현재 제작 정보 패널 열림 여부
    /// </summary>
    public bool IsCraftInfoOpen => _craftView.IsCraftInfoOpen;

    #region ----- 시작 -----
    /// <summary>
    /// 제작 모델 연결
    /// </summary>
    /// <param name="craftModel">제작 모델</param>
    /// <param name="reviewModel">제작 판정 모델</param>
    /// <param name="completeModel">제작 확정 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="orderModel">주문 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="businessDayModel">영업일 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="reviewViewBuilder">제작 판정 표시 데이터 생성기</param>
    public void Init (
        CraftModel craftModel, CraftReviewModel reviewModel,
        CraftCompleteModel completeModel, InventoryModel inventoryModel,
        CustomerOrderModel orderModel, PlayStateModel playStateModel,
        BusinessDayModel businessDayModel,
        ShopModel shopModel, CraftReviewViewBuilder reviewViewBuilder )
    {
        //비활성 제작 뷰와 주문 목록 뷰 먼저 초기화
        _craftView.InitializeRuntime( );
        _craftListView.InitializeRuntime( );

        //기존 이벤트 연결 해제
        UnsubscribeEvents( );

        //플레이씬에서 생성한 모델 전달
        _craftModel = craftModel;
        _orderModel = orderModel;

        //비활성 Content의 사용 파츠 목록 뷰 초기화
        _usedPartsListView.Init( );

        var orderListBuilder = new CraftOrderListBuilder(
            playStateModel, _orderIconData );
        var partsViewDataBuilder = new CraftPartsViewDataBuilder( );

        _orderListPresenter = new CraftOrderListPresenter(
            orderModel, craftModel, _craftListView, orderListBuilder );

        _partPresenter = new CraftPartPresenter(
            craftModel, inventoryModel, _craftView, _partsSelectView,
            () => _selectedOrder );

        _partListPresenter = new CraftPartListPresenter(
            craftModel, inventoryModel, _partsSelectView,
            partsViewDataBuilder, () => _selectedOrder );

        _infoPresenter = new CraftInfoPresenter(
            craftModel, reviewModel, reviewViewBuilder,
            partsViewDataBuilder, _usedPartsListView,
            _orderDetailView, _previewView, _craftView,
            () => _selectedOrder );

        _completePresenter = new CraftCompletePresenter(
            completeModel, businessDayModel, reviewViewBuilder,
            _craftView, _completeView, () => _selectedOrder );

        _isInitialized = true;

        //활성화된 상태면 즉시 이벤트 연결
        if ( isActiveAndEnabled ) SubscribeEvents( );

        //제작 화면 표시 초기화
        _selectedOrder = null;
        _orderListPresenter.Reset( );
        _partPresenter.ResetDisplay( );
        _partListPresenter.Clear( );
        _infoPresenter.Clear( );
        _completePresenter.ResetInstant( );
        _craftView.ClearCraftFailure( );

        //게임 시작 시 제작 화면 숨김
        _craftView.HideInstant( );
    }

    /// <summary>
    /// 제작 입력 이벤트 연결
    /// </summary>
    private void OnEnable ()
    {
        //모델 전달이 끝난 경우에만 연결
        if ( _isInitialized ) SubscribeEvents( );
    }

    /// <summary>
    /// 제작 입력 이벤트 해제
    /// </summary>
    private void OnDisable ()
    {
        UnsubscribeEvents( );
    }
    #endregion

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 하위 프레젠터와 제작 화면 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        //중복 연결과 초기화 전 연결 차단
        if ( _isSubscribed || _isInitialized == false ) return;

        _orderListPresenter.SubscribeEvents( );
        _partPresenter.SubscribeEvents( );
        _partListPresenter.SubscribeEvents( );
        _completePresenter.SubscribeEvents( );

        _orderListPresenter.OnCraftSelected += StartSelectedCraft;
        _orderListPresenter.OnOrderSelected += HandleOrderSelected;
        _partPresenter.OnPartsChanged += RefreshCraftDisplay;
        _partPresenter.OnPartPlaced += HandlePartPlaced;
        _partPresenter.OnPartDragCompleted += HandlePartDragCompleted;
        _partPresenter.OnPartScaled += HandlePartScaled;
        _completePresenter.OnCraftSucceeded += HandleCraftSucceeded;
        _completePresenter.OnMoveToDelivery += MoveToDelivery;

        _craftView.OnBackToList += OpenCraftList;
        _craftView.OnCraftClose += ClosePanel;
        _craftView.OnPartsSelectPanelChanged +=
            HandlePartsSelectPanelChanged;
        _craftView.OnCraftInfoPanelChanged +=
            HandleCraftInfoPanelChanged;
        _craftView.OnUsedPartsShown += HandleUsedPartsShown;
        _craftListView.OnExit += ClosePanel;
        _orderModel.OnOrderChanged += HandleOrderChanged;

        _isSubscribed = true;
    }

    /// <summary>
    /// 하위 프레젠터와 제작 화면 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        //연결 전이면 종료
        if ( _isSubscribed == false ) return;

        _orderListPresenter.UnsubscribeEvents( );
        _partPresenter.UnsubscribeEvents( );
        _partListPresenter.UnsubscribeEvents( );
        _completePresenter.UnsubscribeEvents( );

        _orderListPresenter.OnCraftSelected -= StartSelectedCraft;
        _orderListPresenter.OnOrderSelected -= HandleOrderSelected;
        _partPresenter.OnPartsChanged -= RefreshCraftDisplay;
        _partPresenter.OnPartPlaced -= HandlePartPlaced;
        _partPresenter.OnPartDragCompleted -= HandlePartDragCompleted;
        _partPresenter.OnPartScaled -= HandlePartScaled;
        _completePresenter.OnCraftSucceeded -= HandleCraftSucceeded;
        _completePresenter.OnMoveToDelivery -= MoveToDelivery;

        _craftView.OnBackToList -= OpenCraftList;
        _craftView.OnCraftClose -= ClosePanel;
        _craftView.OnPartsSelectPanelChanged -=
            HandlePartsSelectPanelChanged;
        _craftView.OnCraftInfoPanelChanged -=
            HandleCraftInfoPanelChanged;
        _craftView.OnUsedPartsShown -= HandleUsedPartsShown;
        _craftListView.OnExit -= ClosePanel;
        _orderModel.OnOrderChanged -= HandleOrderChanged;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 제작 주문 -----
    /// <summary>
    /// 제작 주문 슬롯 강조 대상 조회
    /// </summary>
    /// <param name="orderId">조회할 주문 아이디</param>
    /// <param name="target">주문 슬롯 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetOrderSlotTarget (
        string orderId, out RectTransform target )
    {
        return _craftListView.TryGetSlotTarget(
            orderId, out target );
    }

    /// <summary>
    /// 제작 파츠 슬롯 강조 대상 조회
    /// </summary>
    /// <param name="partId">조회할 파츠 아이디</param>
    /// <param name="target">파츠 슬롯 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetPartSlotTarget (
        string partId, out RectTransform target )
    {
        return _partsSelectView.TryGetSlotTarget(
            partId, out target );
    }

    /// <summary>
    /// 배치 파츠 강조 대상 조회
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="target">배치 파츠 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetPlacedPartTarget (
        int placementNumber, out RectTransform target )
    {
        return _craftView.TryGetPartTarget(
            placementNumber, out target );
    }

    /// <summary>
    /// 제작 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.CraftStartButton:
                return _craftListView.TryGetCraftStartTarget( out target );

            case TutorialTargetId.OrderConditions:
                return _orderDetailView.TryGetTutorialTarget( out target );

            case TutorialTargetId.DeliveryButton:
                return _completeView.TryGetDeliveryTarget( out target );

            default:
                return _craftView.TryGetTutorialTarget(
                    targetId, out target );
        }
    }

    /// <summary>
    /// 지정 타입 파츠가 현재 배치됐는지 확인
    /// </summary>
    /// <param name="partType">확인할 파츠 타입</param>
    /// <returns>파츠 배치 여부</returns>
    public bool HasPlacedPartType ( PartType partType )
    {
        for ( int i = 0; i < _craftModel.PlacedParts.Count; i++ )
        {
            if ( _craftModel.PlacedParts [ i ].PartData.PartType ==
                partType )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 제작 주문 목록 열기
    /// </summary>
    public void OpenCraftList ()
    {
        //선택과 드래그 상태 초기화
        _partPresenter.ClearSelection( );

        //주문 선택을 복구하지 않고 Production 주문 목록 갱신
        _orderListPresenter.Refresh( false );

        //제작 주문 목록 표시
        _craftView.ShowCraftList( );

        OnListOpened?.Invoke( );
    }

    /// <summary>
    /// 사용자 입력으로 제작 화면 닫기
    /// </summary>
    void ClosePanel ()
    {
        HidePanel ( ( ) => OnPanelClosed?.Invoke ( ) );
    }

    /// <summary>
    /// 제작 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        //선택과 드래그 상태 초기화
        _partPresenter.ClearSelection( );

        //제작 완료 표시 상태 초기화
        _completePresenter.ResetInstant( );

        _craftView.HidePanel( onComplete );
    }

    /// <summary>
    /// 제작 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _partPresenter.ClearSelection( );
        _completePresenter.ResetInstant( );
        _craftView.HideInstant( );
    }

    /// <summary>
    /// 제작 주문 열기
    /// </summary>
    /// <param name="orderId">제작할 주문 아이디</param>
    /// <returns>주문 선택 결과</returns>
    public CraftActionResult OpenCraft ( string orderId )
    {
        //주문 조회
        if ( _orderModel.GetOrder(
            orderId, out CustomerOrder order ) == false )
            return CraftActionResult.InvalidOrder;

        //제작 중인 주문이 아니면 종료
        if ( order.ProgressState != OrderProgressState.Production )
            return CraftActionResult.InvalidOrder;

        //제작 주문 선택
        CraftActionResult result = _craftModel.SelectOrder( orderId );

        if ( result != CraftActionResult.Success ) return result;

        //현재 제작 주문 저장
        _selectedOrder = order;
        //제작 목록 선택 주문 동기화
        _orderListPresenter.SetSelection( orderId );

        //파츠 제작 패널 표시
        _craftView.ShowCraftPanel( );

        //제작 화면 전체 갱신
        _partPresenter.ClearSelection( );
        _partPresenter.RestorePlacedParts( );
        _partListPresenter.Refresh( );
        _infoPresenter.Refresh( );

        return CraftActionResult.Success;
    }

    /// <summary>
    /// 선택한 주문 제작 시작
    /// </summary>
    void StartSelectedCraft ( string orderId )
    {
        //패널 전환 전에 제작 시작 입력을 먼저 전달
        OnCraftStartRequested?.Invoke( orderId );

        CraftActionResult result = OpenCraft( orderId );

        //제작 진입 성공 결과 전달
        if ( result == CraftActionResult.Success )
        {
            OnCraftOpened?.Invoke( orderId );
            return;
        }

        //시도한 목록 선택을 지우고 실제 제작 주문으로 복구
        _orderListPresenter.ClearSelection( );
        _orderListPresenter.RestoreSelection( );
        OnCraftStartFailed?.Invoke( orderId );
    }

    /// <summary>
    /// 주문 변경에 따른 제작 편집 상태 갱신
    /// </summary>
    void HandleOrderChanged ()
    {
        //제작 확정 중 발생한 주문 상태 변경은 확정 프레젠터에서 처리
        if ( _completePresenter.IsCompleting ) return;

        //현재 편집 중인 주문이 없으면 종료
        if ( _selectedOrder == null ) return;

        //현재 주문이 계속 제작 중이면 편집 상태 유지
        if ( IsProductionOrder( _selectedOrder.OrderId ) ) return;

        //실제 제작 화면 표시 여부 저장
        bool returnToCraftList = _craftView.IsCraftPanelVisible;

        //종료된 주문의 편집 상태 초기화
        _craftModel.ClearOrder( );

        //프레젠터와 제작 표시 상태 초기화
        ResetCraftDisplayState( );

        //제작 중 화면에서 주문이 종료됐다면 제작 목록으로 복귀
        if ( returnToCraftList )
            _craftView.ShowCraftList( );
    }

    /// <summary>
    /// 제작 중 주문 여부 확인
    /// </summary>
    bool IsProductionOrder ( string orderId )
    {
        if ( string.IsNullOrWhiteSpace( orderId ) ) return false;

        //주문 상태 반환
        return _orderModel.GetOrder( orderId, out CustomerOrder order )
            && order.ProgressState == OrderProgressState.Production;
    }
    #endregion

    #region ----- 기능 간 결과 중재 -----
    /// <summary>
    /// 제작 확정 성공 결과 중계
    /// </summary>
    void HandleCraftSucceeded ()
    {
        string orderId = _selectedOrder?.OrderId;

        ResetCraftDisplayState( );

        if ( string.IsNullOrWhiteSpace( orderId ) ) return;

        OnCraftSucceeded?.Invoke( orderId );
    }

    /// <summary>
    /// 제작 목록 주문 선택 결과 중계
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    void HandleOrderSelected ( string orderId )
    {
        OnOrderSelected?.Invoke( orderId );
    }

    /// <summary>
    /// 파츠 선택 패널 전환 완료 결과 중계
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    void HandlePartsSelectPanelChanged ( bool isOpen )
    {
        if ( isOpen )
            OnPartsSelectPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 제작 정보 패널 전환 완료 결과 중계
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    void HandleCraftInfoPanelChanged ( bool isOpen )
    {
        if ( isOpen )
            OnCraftInfoPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 제작 정보 패널의 사용 파츠 표시 결과 중계
    /// </summary>
    void HandleUsedPartsShown ()
    {
        OnUsedPartsShown?.Invoke( );
    }

    /// <summary>
    /// 제작 파츠 배치 결과 중계
    /// </summary>
    /// <param name="partId">배치한 파츠 아이디</param>
    /// <param name="partType">배치한 파츠 타입</param>
    /// <param name="placementNumber">배치 번호</param>
    void HandlePartPlaced (
        string partId, PartType partType, int placementNumber )
    {
        OnPartPlaced?.Invoke(
            partId, partType, placementNumber );
    }

    /// <summary>
    /// 제작 파츠 이동 결과 중계
    /// </summary>
    /// <param name="partId">이동한 파츠 아이디</param>
    /// <param name="partType">이동한 파츠 타입</param>
    /// <param name="placementNumber">배치 번호</param>
    void HandlePartDragCompleted (
        string partId, PartType partType, int placementNumber )
    {
        OnPartDragCompleted?.Invoke(
            partId, partType, placementNumber );
    }

    /// <summary>
    /// 제작 파츠 크기 변경 결과 중계
    /// </summary>
    /// <param name="partId">크기를 변경한 파츠 아이디</param>
    /// <param name="partType">크기를 변경한 파츠 타입</param>
    /// <param name="placementNumber">배치 번호</param>
    void HandlePartScaled (
        string partId, PartType partType, int placementNumber )
    {
        OnPartScaled?.Invoke(
            partId, partType, placementNumber );
    }

    /// <summary>
    /// 파츠 변경 결과를 제작 목록과 정보 표시에 반영
    /// </summary>
    void RefreshCraftDisplay ()
    {
        _partListPresenter.Refresh( );
        _infoPresenter.Refresh( );
    }

    /// <summary>
    /// 제작 Presenter와 화면 표시 상태 초기화
    /// </summary>
    void ResetCraftDisplayState ()
    {
        _selectedOrder = null;
        _orderListPresenter.ClearSelection( );
        _partPresenter.ResetDisplay( );
        _partListPresenter.Clear( );
        _infoPresenter.Clear( );
        _craftView.ClearCraftFailure( );
    }

    /// <summary>
    /// 제작 완료 주문 배송 화면 이동
    /// </summary>
    void MoveToDelivery ( string orderId )
    {
        OnMoveToDelivery?.Invoke( orderId );
    }
    #endregion
}
