using System;

/// <summary>
/// Day 2 일반 주문 후속 가이드 프레젠터
/// </summary>
public class OrderGuidePresenter : ITutorialGuideSection
{
    const int OrderGuideStartDay = 2;
    const int MultipleRequirementCount = 2;
    const string NormalOrderDialogueId = "Dialogue_Day2_NormalOrderGuide";

    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    PlayStateModel _playStateModel;       //현재 누적 영업일
    CustomerOrderModel _orderModel;       //주문 상태 조회 모델
    OrderPresenter _orderPresenter;       //주문 화면 행동 이벤트

    Func<string, bool> _playDialogue;       //공용 대화 재생 함수
    Func<TutorialGuideId, bool> _requestGuide;       //후속 가이드 요청 함수

    TutorialGuideId? _currentGuideId;       //현재 진행 중인 주문 가이드
    string _currentDialogueId;       //현재 재생한 주문 가이드 대화
    string _openedOrderId;       //현재 상세 화면에 표시된 주문

    bool _isOrderPanelOpened;       //주문 화면 표시 여부
    bool _isSubscribed;       //이벤트 연결 여부

    /// <summary>
    /// 주문 가이드 완료 이벤트
    /// </summary>
    public event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// Day 2 주문 후속 가이드 프레젠터 생성
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="playStateModel">현재 누적 영업일 모델</param>
    /// <param name="orderModel">주문 상태 조회 모델</param>
    /// <param name="orderPresenter">주문 화면 행동 프레젠터</param>
    /// <param name="playDialogue">공용 대화 재생 함수</param>
    /// <param name="requestGuide">후속 가이드 요청 함수</param>
    public OrderGuidePresenter (
        TutorialModel tutorialModel,
        PlayStateModel playStateModel,
        CustomerOrderModel orderModel,
        OrderPresenter orderPresenter,
        Func<string, bool> playDialogue,
        Func<TutorialGuideId, bool> requestGuide )
    {
        _tutorialModel = tutorialModel;
        _playStateModel = playStateModel;
        _orderModel = orderModel;
        _orderPresenter = orderPresenter;
        _playDialogue = playDialogue;
        _requestGuide = requestGuide;
    }

    /// <summary>
    /// 주문 후속 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>주문 가이드 담당 여부</returns>
    public bool Handles ( TutorialGuideId guideId )
    {
        return guideId == TutorialGuideId.NormalOrder;
    }

    /// <summary>
    /// 지정된 주문 후속 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 후속 가이드 아이디</param>
    /// <returns>대화 시작 여부</returns>
    public bool Begin ( TutorialGuideId guideId )
    {
        if ( CanBeginGuide( guideId ) == false )
            return false;

        _currentGuideId = guideId;
        _currentDialogueId = NormalOrderDialogueId;

        if ( _playDialogue( NormalOrderDialogueId ) )
            return true;

        Reset( );
        return false;
    }

    /// <summary>
    /// 주문 가이드 시작 조건 확인
    /// </summary>
    /// <param name="guideId">확인할 후속 가이드 아이디</param>
    /// <returns>현재 화면에서 시작 가능한지 여부</returns>
    bool CanBeginGuide ( TutorialGuideId guideId )
    {
        if ( _tutorialModel.CoreTutorialCompleted == false ||
            _playStateModel.TotalDay < OrderGuideStartDay ||
            _isOrderPanelOpened == false )
        {
            return false;
        }

        if ( guideId != TutorialGuideId.NormalOrder ||
            string.IsNullOrEmpty( _openedOrderId ) )
        {
            return false;
        }

        return HasMultipleRequirements( _openedOrderId );
    }

    /// <summary>
    /// 지정 주문에 주요 요구 사항이 두 개 이상 있는지 확인
    /// </summary>
    /// <param name="orderId">확인할 주문 아이디</param>
    /// <returns>복수 주요 요구 사항 존재 여부</returns>
    bool HasMultipleRequirements ( string orderId )
    {
        if ( _orderModel.GetOrder(
            orderId, out CustomerOrder order ) == false )
        {
            return false;
        }

        return order.Requirements.Count >= MultipleRequirementCount;
    }

    /// <summary>
    /// 주문 가이드 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>신호 처리 여부</returns>
    public bool HandleSignal ( string signalId )
    {
        return false;
    }

    /// <summary>
    /// 주문 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 주문 가이드 대화 처리 여부</returns>
    public bool HandleDialogueCompleted ( string dialogueId )
    {
        if ( _currentGuideId.HasValue == false ||
            dialogueId != _currentDialogueId )
        {
            return false;
        }

        TutorialGuideId completedGuideId =
            _currentGuideId.Value;

        _currentGuideId = null;
        _currentDialogueId = string.Empty;

        OnCompleted?.Invoke( completedGuideId );
        return true;
    }

    /// <summary>
    /// 주문 화면 행동 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _orderPresenter.OnPanelOpened += HandleOrderPanelOpened;
        _orderPresenter.OnPanelClosed += HandleOrderPanelClosed;
        _orderPresenter.OnDetailOpened += HandleOrderDetailOpened;

        _isSubscribed = true;
    }

    /// <summary>
    /// 주문 화면 행동 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _orderPresenter.OnPanelOpened -= HandleOrderPanelOpened;
        _orderPresenter.OnPanelClosed -= HandleOrderPanelClosed;
        _orderPresenter.OnDetailOpened -= HandleOrderDetailOpened;

        _isSubscribed = false;
    }

    /// <summary>
    /// Day 2 이후 주문 화면 첫 진입 안내 요청
    /// </summary>
    void HandleOrderPanelOpened ()
    {
        _isOrderPanelOpened = true;
    }

    /// <summary>
    /// 주문 화면 종료 상태 반영
    /// </summary>
    void HandleOrderPanelClosed ()
    {
        _isOrderPanelOpened = false;
        _openedOrderId = string.Empty;
    }

    /// <summary>
    /// 처음 확인한 복수 주요 요구 사항 주문의 조건 안내 요청
    /// </summary>
    /// <param name="orderId">상세 화면에 표시된 주문 아이디</param>
    void HandleOrderDetailOpened ( string orderId )
    {
        _openedOrderId = orderId;

        if ( HasMultipleRequirements( orderId ) == false ||
            CanRequestGuide( TutorialGuideId.NormalOrder ) == false )
        {
            return;
        }

        _requestGuide( TutorialGuideId.NormalOrder );
    }

    /// <summary>
    /// 지정 후속 가이드를 새로 요청할 수 있는지 확인
    /// </summary>
    /// <param name="guideId">요청할 후속 가이드 아이디</param>
    /// <returns>가이드 요청 가능 여부</returns>
    bool CanRequestGuide ( TutorialGuideId guideId )
    {
        return _tutorialModel.CoreTutorialCompleted &&
            _playStateModel.TotalDay >= OrderGuideStartDay &&
            _tutorialModel.HasShownGuide( guideId ) == false &&
            _currentGuideId != guideId;
    }

    /// <summary>
    /// 진행 중인 주문 가이드 임시 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _currentGuideId = null;
        _currentDialogueId = string.Empty;
    }
}
