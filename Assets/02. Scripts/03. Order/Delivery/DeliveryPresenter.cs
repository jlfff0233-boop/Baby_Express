using System;
using UnityEngine;

/// <summary>
/// 배송 프레젠터 - 배송 예상, 확정, 결과 표시 중재
/// </summary>
public class DeliveryPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] DeliveryView _deliveryView;       //배송 뷰

    DeliveryModel _deliveryModel;       //배송 모델
    CustomerOrderModel _orderModel;       //주문 모델
    CraftCompleteModel _craftCompleteModel;       //제작 완료 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    CraftReviewViewBuilder _reviewViewBuilder;       //제작 판정 표시 데이터 생성기
    DeliveryViewDataBuilder _viewDataBuilder;       //배송 표시 데이터 생성기


    string _selectedOrderId;       //현재 배송 주문 아이디
    bool _isDelivering;       //배송 처리 중 여부
    bool _isTutorialMethodSelectionPending;       //튜토리얼 배송 방식 선택 대기 여부
    bool _hasSelectedMethod;       //현재 화면에서 배송 방식을 직접 선택한 여부
    DeliveryMethod _selectedMethod;       //현재 선택한 배송 방식

    /// <summary>
    /// 배송 진행 화면 종료 이벤트
    /// </summary>
    public event Action OnDeliveryClosed;

    /// <summary>
    /// 배송 예상 화면 표시 이벤트
    /// </summary>
    public event Action<string> OnPreviewShown;

    /// <summary>
    /// 배송 출발 성공 이벤트
    /// </summary>
    public event Action<string, DeliveryMethod> OnDeliveryStarted;

    /// <summary>
    /// 사용자가 배송 방식을 직접 선택한 이벤트
    /// </summary>
    public event Action<DeliveryMethod> OnDeliveryMethodSelected;

    /// <summary>
    /// 배송 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        return _deliveryView.TryGetTutorialTarget(
            targetId, out target );
    }

    /// <summary>
    /// 배송 입력 연결
    /// </summary>
    void Awake ()
    {
        _deliveryView.OnDelivered += Deliver;
        _deliveryView.OnCanceled += CloseDelivery;
        _deliveryView.OnMethodChanged += SelectDeliveryMethod;
    }

    /// <summary>
    /// 배송 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _deliveryView.OnDelivered -= Deliver;
        _deliveryView.OnCanceled -= CloseDelivery;
        _deliveryView.OnMethodChanged -= SelectDeliveryMethod;
    }

    /// <summary>
    /// 배송 프레젠터 초기화
    /// </summary>
    /// <param name="deliveryModel">배송 모델</param>
    /// <param name="orderModel">주문 모델</param>
    /// <param name="craftCompleteModel">제작 완료 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="reviewViewBuilder">제작 판정 표시 데이터 생성기</param>
    /// <param name="employeeModel">고용 모델</param>
    public void Init (
        DeliveryModel deliveryModel,
        CustomerOrderModel orderModel,
        CraftCompleteModel craftCompleteModel,
        PlayStateModel playStateModel,
        CraftReviewViewBuilder reviewViewBuilder,
        EmployeeModel employeeModel )
    {
        _deliveryModel = deliveryModel;
        _orderModel = orderModel;
        _craftCompleteModel = craftCompleteModel;
        _playStateModel = playStateModel;
        _reviewViewBuilder = reviewViewBuilder;
        _viewDataBuilder = new DeliveryViewDataBuilder( playStateModel, employeeModel );

        _selectedOrderId = null;
        _isDelivering = false;
        _isTutorialMethodSelectionPending = false;
        _hasSelectedMethod = false;
        _selectedMethod = DeliveryMethod.Direct;

        _deliveryView.HideInstant( );
    }

    #region ----- 배송 화면 -----

    /// <summary>
    /// 주문 배송 화면 표시
    /// </summary>
    /// <param name="orderId">표시할 배송 주문 아이디</param>
    public void ShowDelivery ( string orderId )
    {
        if ( GetDeliveryData(
            orderId, out CustomerOrder order,
            out CraftResult craftResult ) == false )
            return;

        //현재 배송 일정 기준 예상 결과 계산
        DeliveryProcessResult result = _deliveryModel.CalculatePreview(
            orderId, _playStateModel.TotalDay,
            out DeliveryResult deliveryResult );

        if ( result != DeliveryProcessResult.Success )
        {
            Debug.LogWarning( $"배송 예상 계산 실패: {result}" );
            return;
        }

        CraftReviewViewData reviewViewData =
            _reviewViewBuilder.Create(
                order, craftResult.ReviewData );

        DeliveryViewData viewData = _viewDataBuilder.Create(
            order, reviewViewData,
            deliveryResult, false );

        //배송 중 주문은 읽기 전용으로 표시
        if ( order.ProgressState == OrderProgressState.Shipping )
        {
            _selectedOrderId = null;

            //실제 출발에 사용한 배송 방식 표시
            _hasSelectedMethod = true;
            _selectedMethod = deliveryResult.Method;

            _deliveryView.ShowShipping ( viewData );

            //현재 배송 슬롯 상태와 툴팁 즉시 갱신
            RefreshDeliveryMethod ( );
            return;
        }

        //배송 대기 주문은 출발 입력 허용
        _selectedOrderId = orderId;
        _hasSelectedMethod = false;

        _deliveryView.ShowPreview( viewData );
        RefreshDeliveryMethod( );

        OnPreviewShown?.Invoke( orderId );
    }

    /// <summary>
    /// 배송 완료 결과 화면 표시
    /// </summary>
    /// <param name="orderId">배송 완료 주문 아이디</param>
    public void ShowDeliveryResult ( string orderId )
    {
        if ( GetDeliveryData(
            orderId, out CustomerOrder order,
            out CraftResult craftResult ) == false )
            return;

        if ( _deliveryModel.GetDeliveryResult(
            orderId, out DeliveryResult deliveryResult ) == false )
            return;

        _selectedOrderId = null;
        _hasSelectedMethod = true;
        _selectedMethod = deliveryResult.Method;

        //확정 제작 판정과 배송 결과를 읽기 전용으로 표시
        CraftReviewViewData reviewViewData =
            _reviewViewBuilder.Create( order, craftResult.ReviewData );

        _deliveryView.ShowResult(
            _viewDataBuilder.Create(
                order, reviewViewData,
                deliveryResult, true ) );
        RefreshDeliveryMethod( );
    }

    /// <summary>
    /// 지정 주문의 배송 예상 화면 표시 여부 확인
    /// </summary>
    /// <param name="orderId">확인할 주문 아이디</param>
    /// <returns>배송 예상 화면 표시 여부</returns>
    public bool IsShowingPreview ( string orderId )
    {
        return string.IsNullOrWhiteSpace( orderId ) == false &&
            _selectedOrderId == orderId;
    }

    /// <summary>
    /// 사용 가능한 배송 방식 선택
    /// </summary>
    /// <param name="method">선택할 배송 방식</param>
    /// <returns>배송 방식 선택 여부</returns>
    public bool TrySelectDeliveryMethod ( DeliveryMethod method )
    {
        if ( string.IsNullOrWhiteSpace( _selectedOrderId ) ||
            _deliveryModel.GetDeliveryMethodResult( method ) !=
                DeliveryProcessResult.Success )
        {
            return false;
        }

        _selectedMethod = method;
        _hasSelectedMethod = true;
        RefreshDeliveryMethod( );
        return true;
    }

    /// <summary>
    /// 튜토리얼 배송 방식 직접 선택 대기 상태 설정
    /// </summary>
    /// <param name="isPending">직접 선택 대기 여부</param>
    public void SetTutorialMethodSelectionPending ( bool isPending )
    {
        _isTutorialMethodSelectionPending = isPending;

        if ( isPending )
            _hasSelectedMethod = false;

        _deliveryView.SetTutorialMethodSelectionPending( isPending );

        if ( isPending == false &&
            string.IsNullOrWhiteSpace( _selectedOrderId ) == false )
        {
            RefreshDeliveryMethod( );
        }
    }

    /// <summary>
    /// 배송 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HideDelivery ( Action onComplete = null )
    {
        _isTutorialMethodSelectionPending = false;
        _deliveryView.SetTutorialMethodSelectionPending( false );
        _selectedOrderId = null;
        _hasSelectedMethod = false;
        _isDelivering = false;
        _deliveryView.Hide ( onComplete );
    }

    /// <summary>
    /// 배송 화면 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _isTutorialMethodSelectionPending = false;
        _deliveryView.SetTutorialMethodSelectionPending( false );
        _selectedOrderId = null;
        _hasSelectedMethod = false;
        _isDelivering = false;
        _deliveryView.HideInstant ( );
    }

    #endregion

    #region ----- 배송 확정 -----

    /// <summary>
    /// 현재 주문 배송 출발
    /// </summary>
    void Deliver ()
    {
        if ( _isTutorialMethodSelectionPending == true ||
            _hasSelectedMethod == false ||
            _isDelivering == true ||
            string.IsNullOrWhiteSpace( _selectedOrderId ) )
            return;

        string orderId = _selectedOrderId;
        DeliveryMethod method = _selectedMethod;

        _isDelivering = true;

        DeliveryProcessResult result = _deliveryModel.StartDelivery(
            orderId,
            _playStateModel.TotalDay,
            method );

        _isDelivering = false;

        if ( result != DeliveryProcessResult.Success )
        {
            _deliveryView.ShowFailure(
                _viewDataBuilder.CreateFailureText( result ) );
            return;
        }

        OnDeliveryStarted?.Invoke( orderId, method );

        //배송 출발 후 배송 화면 종료
        HideDelivery ( ( ) => OnDeliveryClosed?.Invoke ( ) );
    }

    /// <summary>
    /// 배송 표시용 주문과 확정 제작 결과 조회
    /// </summary>
    /// <param name="orderId">배송 주문 아이디</param>
    /// <param name="order">조회한 주문</param>
    /// <param name="craftResult">조회한 확정 제작 결과</param>
    /// <returns>조회 성공 여부</returns>
    bool GetDeliveryData (
        string orderId, out CustomerOrder order,
        out CraftResult craftResult )
    {
        craftResult = null;

        if ( _orderModel.GetOrder( orderId, out order ) == false )
            return false;

        return _craftCompleteModel.GetResult(
            orderId, out craftResult );
    }

    /// <summary>
    /// 배송 화면을 닫고 영업 종료 확인
    /// </summary>
    void CloseDelivery ()
    {
        //배송 예정 화면 여부 저장
        bool wasPreview =
            string.IsNullOrWhiteSpace( _selectedOrderId ) == false;

        //읽기 전용 배송 결과 화면은 영업 종료를 확인하지 않음
        if ( wasPreview )
            HideDelivery ( ( ) => OnDeliveryClosed?.Invoke ( ) );
        else
            HideDelivery ( );
    }
    /// <summary>
    /// 배송 방식 선택
    /// </summary>
    /// <param name="method">선택한 배송 방식</param>
    void SelectDeliveryMethod ( DeliveryMethod method )
    {
        bool wasTutorialSelectionPending =
            _isTutorialMethodSelectionPending;

        if ( TrySelectDeliveryMethod( method ) == false )
        {
            if ( wasTutorialSelectionPending == true )
                SetTutorialMethodSelectionPending( true );

            return;
        }

        if ( wasTutorialSelectionPending == true )
        {
            _isTutorialMethodSelectionPending = false;
            _deliveryView.SetTutorialMethodSelectionPending( false );
        }

        OnDeliveryMethodSelected?.Invoke( method );
    }

    /// <summary>
    /// 배송 방식 선택 상태 갱신
    /// </summary>
    void RefreshDeliveryMethod ()
    {
        //직접 배송 가능 여부 확인
        DeliveryProcessResult directResult =
            _deliveryModel.GetDeliveryMethodResult(
                DeliveryMethod.Direct );

        //직원 배송 가능 여부 확인
        DeliveryProcessResult employeeResult =
            _deliveryModel.GetDeliveryMethodResult(
                DeliveryMethod.Employee );

        bool canUseDirect =
            directResult == DeliveryProcessResult.Success;
        bool canUseEmployee =
            employeeResult == DeliveryProcessResult.Success;

        string directTooltip = _viewDataBuilder.CreateMethodTooltip(
            DeliveryMethod.Direct, directResult,
            _deliveryModel.DirectDeliveryCost );
        string employeeTooltip = _viewDataBuilder.CreateMethodTooltip(
            DeliveryMethod.Employee, employeeResult,
            _deliveryModel.DirectDeliveryCost );

        _deliveryView.SetDeliveryMethod(
            _selectedMethod, _hasSelectedMethod,
            canUseDirect, canUseEmployee,
            directTooltip, employeeTooltip );
    }

    #endregion

}
