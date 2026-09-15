using System;
using UnityEngine;

/// <summary>
/// 상단바 프레젠터 - 플레이 상태 Model과 상태 View 중재
/// </summary>
public class TopBarPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] StatusView _statusView;       //상태 뷰

    PlayStateModel _playStateModel;                //플레이 상태 모델
    CustomerOrderModel _orderModel;       //고객 주문 모델
    OrderGeneratorModel _generatorModel;       //주문 생성 모델
    BusinessDayModel _businessDayModel;       //영업일 모델

    AudioManager _audioManager;       //공용 오디오 관리자

    float _currentBudget;       //마지막으로 반영한 실제 자금
    bool _isInitialized;                           //모델 전달 완료 여부
    bool _isSubscribed;                            //이벤트 연결 여부

    #region ----- 시작 -----

    /// <summary>
    /// 상단바 모델과 오디오 관리자 연결
    /// </summary>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="businessDayModel">영업일 모델</param>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="generatorModel">주문 생성 모델</param>
    /// <param name="audioManager">공용 오디오 관리자</param>
    public void Init (
        PlayStateModel playStateModel,
        BusinessDayModel businessDayModel,
        CustomerOrderModel orderModel,
        OrderGeneratorModel generatorModel,
        AudioManager audioManager )
    {
        UnsubscribeEvents( );

        _playStateModel = playStateModel;
        _businessDayModel = businessDayModel;
        _orderModel = orderModel;
        _generatorModel = generatorModel;
        _audioManager = audioManager;

        _currentBudget = _playStateModel.Budget;
        _isInitialized = true;

        if ( isActiveAndEnabled )
            SubscribeEvents( );

        Refresh( );
    }

    /// <summary>
    /// 상단바 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        //모델 전달이 끝난 경우에만 연결
        if ( _isInitialized )
            SubscribeEvents( );
    }

    /// <summary>
    /// 상단바 이벤트 해제
    /// </summary>
    void OnDisable ()
    {
        UnsubscribeEvents( );
    }

#if UNITY_EDITOR
    /// <summary>
    /// 숫자키 4로 자금 수입 연출 검증
    /// </summary>
    void Update ()
    {
        if ( Input.GetKeyDown( KeyCode.Alpha4 ) )
            DebugBudgetIncome( );
    }
#endif

    #endregion

    #region ----- 이벤트 연결 -----

    /// <summary>
    /// 플레이 상태 변경 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        //중복 연결과 초기화 전 연결 차단
        if ( _isSubscribed || _isInitialized == false ) return;

        _playStateModel.OnDateChanged += UpdateDate;
        _playStateModel.OnBudgetChanged += UpdateBudget;

        _businessDayModel.OnCraftProgressChanged += UpdateCraftProgress;
        _orderModel.OnPenaltyChanged += UpdateOrderPenalty;

        _isSubscribed = true;
    }

    /// <summary>
    /// 플레이 상태 변경 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        //연결되지 않은 이벤트 해제 차단
        if ( _isSubscribed == false ) return;

        _playStateModel.OnDateChanged -= UpdateDate;
        _playStateModel.OnBudgetChanged -= UpdateBudget;

        _businessDayModel.OnCraftProgressChanged -= UpdateCraftProgress;
        _orderModel.OnPenaltyChanged -= UpdateOrderPenalty;

        _isSubscribed = false;
    }

    #endregion

    #region ----- 상태 표시 -----

    /// <summary>
    /// 현재 플레이 상태 전체 즉시 표시
    /// </summary>
    public void Refresh ()
    {
        _currentBudget = _playStateModel.Budget;

        _statusView.SetDate(
            _playStateModel.Month, _playStateModel.Day );

        //제작 진행률을 기준으로 현재 영업 시각 계산
        _statusView.SetTime( _businessDayModel.BusinessTime );

        _statusView.SetCraftProgress(
            _businessDayModel.CraftCount,
            _businessDayModel.CraftLimit,
            _businessDayModel.CraftProgressRate );

        _statusView.SetBudget( _currentBudget );

        UpdateOrderPenalty( );
    }

    /// <summary>
    /// 날짜 변경 연출 요청
    /// </summary>
    /// <param name="month">변경된 월</param>
    /// <param name="day">변경된 일</param>
    void UpdateDate ( int month, int day )
    {
        _statusView.PlayDateChange( month, day );
    }

    /// <summary>
    /// 일일 제작 진행률과 영업 시각 변경 연출 요청
    /// </summary>
    /// <param name="craftCount">오늘 제작 완료 수</param>
    /// <param name="craftLimit">일일 제작 할당량</param>
    void UpdateCraftProgress ( int craftCount, int craftLimit )
    {
        //제작 진행률에 비례한 영업 시각 표시
        _statusView.PlayTimeChange(
            _businessDayModel.BusinessTime );

        _statusView.PlayCraftProgress(
            craftCount,
            craftLimit,
            _businessDayModel.CraftProgressRate );
    }

    /// <summary>
    /// 자금 변경 방향에 맞는 표시와 효과음 요청
    /// </summary>
    /// <param name="budget">변경된 실제 자금</param>
    void UpdateBudget ( float budget )
    {
        float changeAmount = budget - _currentBudget;

        if ( Mathf.Approximately( changeAmount, 0f ) )
            return;

        bool isIncome = changeAmount > 0f;
        _currentBudget = budget;

        _statusView.PlayBudgetChange(
            budget, changeAmount );

        if ( isIncome )
            _audioManager.PlayBudgetIncome( );
        else
            _audioManager.PlayBudgetExpense( );
    }


    /// <summary>
    /// 주문량 페널티 원인 문구 조회
    /// </summary>
    /// <param name="type">페널티 원인</param>
    /// <returns>표시할 원인 문구</returns>
    string GetPenaltyReason ( OrderPenaltyType type )
    {
        switch ( type )
        {
            case OrderPenaltyType.Reject:
                return "주문 거절";

            case OrderPenaltyType.AutoReject:
                return "주문 자동 거절";

            case OrderPenaltyType.Cancel:
                return "주문 취소";

            case OrderPenaltyType.Fail:
                return "주문 실패";

            case OrderPenaltyType.Mixed:
                return "복합 원인";

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 주문량 페널티 알림 갱신
    /// </summary>
    void UpdateOrderPenalty ()
    {
        //적용 중인 페널티가 없으면 알림 숨김
        if ( _orderModel.IsPenaltyActive == false )
        {
            _statusView.UpdateOrderPenalty( false, string.Empty );
            return;
        }

        //페널티 적용 후 예상 주문량 계산
        _generatorModel.GetExpectedOrderRange(
            _orderModel.OrderReduction, out int minCount, out int maxCount );

        string reason = GetPenaltyReason( _orderModel.PenaltyType );
        string description =
            $"원인: {reason}\n" +
            $"주문 감소량: 하루 {_orderModel.OrderReduction}개\n" +
            $"남은 기간: {_orderModel.PenaltyRemainingDays}일\n" +
            $"예상 주문량: 하루 {minCount}~{maxCount}개";

        _statusView.UpdateOrderPenalty( true, description );
    }

#if UNITY_EDITOR
    /// <summary>
    /// 검증용 자금 추가
    /// </summary>
    void DebugBudgetIncome ()
    {
        _playStateModel.AddBudget( 1000f );
    }
#endif

    #endregion
}
