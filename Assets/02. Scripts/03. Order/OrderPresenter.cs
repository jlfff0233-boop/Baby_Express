using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 프레젠터 - 주문 화면 생명주기와 기능 간 결과 중재
/// </summary>
public class OrderPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] CustomerOrderView _orderView;       //고객 주문 뷰

    [Header( "----- 상태 아이콘 -----" )]
    [SerializeField] OrderIconData _iconData;       //주문 공용 상태 아이콘 데이터

    CustomerOrderModel _orderModel;       //고객 주문 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    InventoryModel _inventoryModel;       //인벤토리 모델
    ShopModel _shopModel;                 //상점 모델
    OrderGeneratorModel _generatorModel;  //주문 생성 모델

    OrderViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기
    OrderGenerationCoordinator _generationCoordinator;       //주문 생성 중재자
    OrderListPresenter _listPresenter;       //주문 목록 프레젠터
    OrderDetailPresenter _detailPresenter;       //주문 상세 프레젠터

    bool _isInitialized;       //모델 전달 완료 여부
    bool _isSubscribed;        //이벤트 연결 여부

    /// <summary>
    /// 제작 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToCraft;

    /// <summary>
    /// 주문 처리 완료 이벤트
    /// </summary>
    public event Action OnOrderCompleted;

    /// <summary>
    /// 배송 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToDelivery;

    /// <summary>
    /// 배송 결과 화면 이동 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnMoveToDeliveryResult;

    /// <summary>
    /// 주문 화면 닫기 이벤트
    /// </summary>
    public event Action OnPanelClosed;

    /// <summary>
    /// 주문 패널 표시 이벤트
    /// </summary>
    public event Action OnPanelOpened;

    /// <summary>
    /// 주문 상세 표시 이벤트
    /// </summary>
    public event Action<string> OnDetailOpened;

    /// <summary>
    /// 주문 수락 완료 이벤트
    /// </summary>
    public event Action<string> OnOrderAccepted;

    #region ----- 초기화 -----
    /// <summary>
    /// 주문 프레젠터 초기화
    /// </summary>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="generatorModel">주문 생성 모델</param>
    /// <param name="craftCompleteModel">제작 확정 모델</param>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 고정 주문 데이터</param>
    public void Init (
        CustomerOrderModel orderModel, PlayStateModel playStateModel,
        InventoryModel inventoryModel, ShopModel shopModel,
        OrderGeneratorModel generatorModel,
        CraftCompleteModel craftCompleteModel,
        TutorialModel tutorialModel,
        TutorialDay1Data tutorialDay1Data )
    {
        //목록을 갱신하기 전에 비활성 주문 뷰 초기화
        _orderView.InitializeRuntime( );

        UnsubscribeEvents( );

        _orderModel = orderModel;
        _playStateModel = playStateModel;
        _inventoryModel = inventoryModel;
        _shopModel = shopModel;
        _generatorModel = generatorModel;

        _viewDataBuilder =
            new OrderViewDataBuilder( _iconData );

        _generationCoordinator = new OrderGenerationCoordinator(
            orderModel, playStateModel, shopModel, generatorModel,
            tutorialModel, tutorialDay1Data );

        _listPresenter = new OrderListPresenter(
            orderModel, playStateModel, _orderView, _viewDataBuilder );

        _detailPresenter = new OrderDetailPresenter(
            orderModel, playStateModel, inventoryModel, shopModel,
            craftCompleteModel, _orderView, _viewDataBuilder,
            () => _listPresenter.SelectedTab );

        _isInitialized = true;

        if ( isActiveAndEnabled ) SubscribeEvents( );

        GenerateDailyOrders( );
        _listPresenter.Reset( );
        _listPresenter.Refresh( );
        _detailPresenter.Hide( );
        _orderView.HideInstant( );
    }

    /// <summary>
    /// 주문 이벤트 연결
    /// </summary>
    private void OnEnable ()
    {
        if ( _isInitialized ) SubscribeEvents( );
    }

    /// <summary>
    /// 주문 이벤트 해제
    /// </summary>
    private void OnDisable ()
    {
        UnsubscribeEvents( );
    }

#if UNITY_EDITOR
    /// <summary>
    /// 주문 확인용 입력 처리
    /// </summary>
    private void Update ()
    {
        if ( _isInitialized == false ) return;

        if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
            _generationCoordinator.GenerateOrders( 1 );

        if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
            FillWaitingOrders( );

        if ( Input.GetKeyDown( KeyCode.Alpha5 ) )
            LogOrders( );

        if ( Input.GetKeyDown( KeyCode.Alpha6 ) )
            UnlockAllParts( );

        if ( Input.GetKeyDown( KeyCode.Alpha7 ) )
            LockHornParts( );

        if ( Input.GetKeyDown( KeyCode.Alpha8 ) )
            UnlockVipOrders( );

        if ( Input.GetKeyDown( KeyCode.Alpha9 ) )
            AddPenaltyDebug( );
    }
#endif
    #endregion

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 하위 프레젠터와 주문 화면 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        if ( _isSubscribed || _isInitialized == false ) return;

        _listPresenter.SubscribeEvents( );
        _detailPresenter.SubscribeEvents( );

        _listPresenter.OnTabChanged += _detailPresenter.Hide;
        _detailPresenter.OnMoveToCraft += MoveToCraft;
        _detailPresenter.OnMoveToDelivery += MoveToDelivery;
        _detailPresenter.OnMoveToResult += MoveToResult;
        _detailPresenter.OnDetailOpened += OpenDetail;
        _detailPresenter.OnOrderAccepted += AcceptOrder;
        _detailPresenter.OnOrderCompleted += CompleteOrderHandling;
        _orderView.OnOrderClose += ClosePanel;

        _isSubscribed = true;
    }

    /// <summary>
    /// 하위 프레젠터와 주문 화면 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _listPresenter.UnsubscribeEvents( );
        _detailPresenter.UnsubscribeEvents( );

        _listPresenter.OnTabChanged -= _detailPresenter.Hide;
        _detailPresenter.OnMoveToCraft -= MoveToCraft;
        _detailPresenter.OnMoveToDelivery -= MoveToDelivery;
        _detailPresenter.OnMoveToResult -= MoveToResult;
        _detailPresenter.OnDetailOpened -= OpenDetail;
        _detailPresenter.OnOrderAccepted -= AcceptOrder;
        _detailPresenter.OnOrderCompleted -= CompleteOrderHandling;
        _orderView.OnOrderClose -= ClosePanel;

        _isSubscribed = false;
    }

    /// <summary>
    /// 제작 화면 이동 요청 전달
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void MoveToCraft ( string orderId )
    {
        OnMoveToCraft?.Invoke( orderId );
    }

    /// <summary>
    /// 배송 화면 이동 요청 전달
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void MoveToDelivery ( string orderId )
    {
        OnMoveToDelivery?.Invoke( orderId );
    }

    /// <summary>
    /// 배송 결과 화면 이동 요청 전달
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void MoveToResult ( string orderId )
    {
        OnMoveToDeliveryResult?.Invoke( orderId );
    }

    /// <summary>
    /// 주문 상세 표시 결과 전달
    /// </summary>
    /// <param name="orderId">표시한 주문 아이디</param>
    void OpenDetail ( string orderId )
    {
        OnDetailOpened?.Invoke( orderId );
    }

    /// <summary>
    /// 주문 수락 결과 전달
    /// </summary>
    /// <param name="orderId">수락한 주문 아이디</param>
    void AcceptOrder ( string orderId )
    {
        OnOrderAccepted?.Invoke( orderId );
    }

    /// <summary>
    /// 주문 처리 완료 결과 전달
    /// </summary>
    void CompleteOrderHandling ()
    {
        OnOrderCompleted?.Invoke( );
    }
    #endregion

    #region ----- 주문 생성 -----
    /// <summary>
    /// 현재 영업일 신규 주문 생성
    /// </summary>
    public void GenerateDailyOrders ()
    {
        _generationCoordinator.GenerateDailyOrders( );
    }

    /// <summary>
    /// 지정 주문의 현재 진행 상태 조회
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <param name="progress">주문 진행 상태</param>
    /// <returns>주문 존재 여부</returns>
    public bool TryGetOrderProgress (
        string orderId, out OrderProgressState progress )
    {
        if ( _orderModel.GetOrder(
            orderId, out CustomerOrder order ) == false )
        {
            progress = default;
            return false;
        }

        progress = order.ProgressState;
        return true;
    }

    /// <summary>
    /// 주문 아이디에 대응하는 튜토리얼 강조 대상 조회
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <param name="target">현재 활성 슬롯 위치</param>
    /// <returns>강조 대상 조회 성공 여부</returns>
    public bool TryGetSlotTarget (
        string orderId, out RectTransform target )
    {
        target = null;

        if ( _orderView.GetSlotView(
            orderId, out OrderSlotView view ) == false )
        {
            return false;
        }

        target = view.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 주문 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        return _orderView.TryGetTutorialTarget(
            targetId, out target );
    }
    #endregion

    #region ----- 주문 패널 -----
    /// <summary>
    /// 주문 패널 표시
    /// </summary>
    public void ShowPanel ()
    {
        //진입 시 항상 대기 주문 목록이 먼저 보이게
        _detailPresenter.Hide( );
        _listPresenter.Reset( );
        _listPresenter.Refresh( );
        _orderView.Show( );

        OnPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 진행 중 주문 패널 표시
    /// </summary>
    public void ShowProducingPanel ()
    {
        _listPresenter.SelectTab( OrderTab.Producing );
        _orderView.Show( );
    }

    /// <summary>
    /// 진행 중 탭에서 지정 주문 상세 표시
    /// </summary>
    /// <param name="orderId">표시할 주문 아이디</param>
    public void ShowProducingOrder ( string orderId )
    {
        ShowProducingPanel( );
        _detailPresenter.ShowOrder( orderId );
    }

    /// <summary>
    /// 종료 주문 패널 표시
    /// </summary>
    public void ShowClosedPanel ()
    {
        _listPresenter.SelectTab( OrderTab.Closed );
        _orderView.Show( );
    }

    /// <summary>
    /// 사용자 입력으로 주문 화면 닫기
    /// </summary>
    void ClosePanel ()
    {
        HidePanel( () => OnPanelClosed?.Invoke( ) );
    }

    /// <summary>
    /// 주문 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        _detailPresenter?.Hide( );
        _orderView.Hide( onComplete );
    }

    /// <summary>
    /// 주문 패널 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _detailPresenter?.Hide( );
        _orderView.HideInstant( );
    }
    #endregion

#if UNITY_EDITOR
    #region ----- 테스트 -----
    /// <summary>
    /// 수락 대기 목록 채우기
    /// </summary>
    void FillWaitingOrders ()
    {
        _generationCoordinator.GenerateOrders(
            _orderModel.WaitingLimit - _orderModel.WaitingCount );

        Debug.Log(
            $"수락 대기: {_orderModel.WaitingCount} / " +
            $"{_orderModel.WaitingLimit}" );
    }

    /// <summary>
    /// 전체 파츠 해금
    /// </summary>
    void UnlockAllParts ()
    {
        IReadOnlyList<PartsData> parts = _shopModel.GetAllParts( );

        for ( int i = 0; i < parts.Count; i++ )
            _playStateModel.SetUnlocked( parts [ i ].Id, true );

        LogHardUnlockState( );
    }

    /// <summary>
    /// 뿔 파츠 잠금
    /// </summary>
    void LockHornParts ()
    {
        IReadOnlyList<PartsData> parts = _shopModel.GetAllParts( );

        for ( int i = 0; i < parts.Count; i++ )
        {
            if ( parts [ i ].PartType == PartType.Horns )
                _playStateModel.SetUnlocked( parts [ i ].Id, false );
        }

        LogHardUnlockState( );
    }

    /// <summary>
    /// 어려움 난이도 해금 상태 출력
    /// </summary>
    void LogHardUnlockState ()
    {
        bool isUnlocked = _generatorModel.IsHardUnlocked(
            _generationCoordinator.GetUnlockedParts( ) );
        Debug.Log( $"Hard 난이도 해금: {isUnlocked}" );
    }

    /// <summary>
    /// VIP 주문 해금 조건 충족
    /// </summary>
    void UnlockVipOrders ()
    {
        _playStateModel.SetHighGradeEvaluationCount( 50 );
        Debug.Log(
            $"B등급 이상 평가 누적 수: " +
            $"{_playStateModel.HighGradeEvaluationCount}" );
    }

    /// <summary>
    /// 전체 주문 상태 출력
    /// </summary>
    void LogOrders ()
    {
        Debug.Log(
            $"현재 누적 영업일: {_playStateModel.TotalDay} / " +
            $"전체 주문: {_orderModel.TotalCount} / " +
            $"수락 대기: {_orderModel.WaitingCount} / " +
            $"{_orderModel.WaitingLimit}" );

        foreach ( CustomerOrder order in _orderModel.Orders )
        {
            string [ ] requirements =
                _viewDataBuilder.CreatePartConditionTexts(
                    order.Requirements, false, false,
                    _inventoryModel, _shopModel );
            string [ ] wishes =
                _viewDataBuilder.CreatePartConditionTexts(
                    order.Wishes, true, false,
                    _inventoryModel, _shopModel );

            string requirementText = requirements.Length == 0
                ? "없음"
                : string.Join( ", ", requirements );
            string wishText = wishes.Length == 0
                ? "없음"
                : string.Join( ", ", wishes );

            Debug.Log(
                $"[{order.OrderId}] 난이도: {order.Difficulty} / " +
                $"상태: {order.ProgressState} / " +
                $"생성일: {order.CreatedTotalDay} / " +
                $"수락: {order.AcceptDue} / " +
                $"납품: {order.DeliveryDue} / " +
                $"최종: {order.FinalDeadline} / " +
                $"최대 코스트: {order.MaxCraftCost} / " +
                $"주요 요구: {requirementText} / " +
                $"희망 사항: {wishText} / " +
                $"특수 주문: {order.SpecialType}" );
        }
    }

    /// <summary>
    /// 주문량 페널티 추가
    /// </summary>
    void AddPenaltyDebug ()
    {
        _orderModel.AddPenalty( OrderPenaltyType.Reject, 1, 3 );

        Debug.Log(
            $"주문 감소량: {_orderModel.OrderReduction} / " +
            $"남은 기간: {_orderModel.PenaltyRemainingDays}일" );
    }
    #endregion
#endif
}
