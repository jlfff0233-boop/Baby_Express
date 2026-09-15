using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이 씬의 결산과 다음 영업일 전환 처리
/// </summary>
public class PlaySceneDayHandler
{
    PlayStateModel _playStateModel;       //공용 플레이 상태
    BusinessDayModel _businessDayModel;       //영업일 상태
    DailyRecordModel _dailyRecordModel;       //일일 기록
    SettlementModel _settlementModel;       //결산 모델

    CustomerOrderModel _customerOrderModel;       //고객 주문 모델
    OrderGeneratorModel _orderGeneratorModel;       //주문 생성 모델
    DeliveryModel _deliveryModel;       //배송 모델
    EmployeeModel _employeeModel;       //직원 모델

    MaintenanceEffectModel _maintenanceEffectModel;       //정비 효과 모델
    ShopModel _shopModel;       //상점 모델
    AchvModel _achvModel;       //업적 모델

    DayPresenter _dayPresenter;       //영업일 프레젠터
    SettlementPresenter _settlementPresenter;       //결산 프레젠터
    OrderPresenter _orderPresenter;       //주문 프레젠터
    DeliveryPresenter _deliveryPresenter;       //배송 프레젠터
    PlaySceneNavHandler _sceneNavHandler;       //화면 전환 처리

    WeeklySettlementData _pendingWeeklySettlement;       //확인 대기 주간 결산
    bool _isEndingPending;       //현재 결산 확인 후 엔딩 표시 대기 여부


    /// <summary>
    /// 다음 영업일 시작 완료 이벤트
    /// </summary>
    public event Action OnNextDayStarted;

    /// <summary>
    /// 다음 영업일 전환 진행 상태 변경 이벤트
    /// </summary>
    public event Action<bool> OnNextDayTransitionChanged;


    /// <summary>
    /// 플레이 씬 영업일 처리 객체 생성
    /// </summary>
    /// <param name="playStateModel">공용 플레이 상태</param>
    /// <param name="businessDayModel">영업일 상태</param>
    /// <param name="dailyRecordModel">일일 기록 모델</param>
    /// <param name="settlementModel">결산 모델</param>
    /// <param name="customerOrderModel">고객 주문 모델</param>
    /// <param name="orderGeneratorModel">주문 생성 모델</param>
    /// <param name="deliveryModel">배송 모델</param>
    /// <param name="employeeModel">직원 모델</param>
    /// <param name="maintenanceEffectModel">정비 효과 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="achvModel">업적 모델</param>
    /// <param name="dayPresenter">영업일 프레젠터</param>
    /// <param name="settlementPresenter">결산 프레젠터</param>
    /// <param name="orderPresenter">주문 프레젠터</param>
    /// <param name="deliveryPresenter">배송 프레젠터</param>
    /// <param name="sceneNavHandler">화면 전환 처리 객체</param>
    public PlaySceneDayHandler (
        PlayStateModel playStateModel,
        BusinessDayModel businessDayModel,
        DailyRecordModel dailyRecordModel,
        SettlementModel settlementModel,
        CustomerOrderModel customerOrderModel,
        OrderGeneratorModel orderGeneratorModel,
        DeliveryModel deliveryModel,
        EmployeeModel employeeModel,
        MaintenanceEffectModel maintenanceEffectModel,
        ShopModel shopModel,
        AchvModel achvModel,
        DayPresenter dayPresenter,
        SettlementPresenter settlementPresenter,
        OrderPresenter orderPresenter,
        DeliveryPresenter deliveryPresenter,
        PlaySceneNavHandler sceneNavHandler )
    {
        _playStateModel = playStateModel;
        _businessDayModel = businessDayModel;
        _dailyRecordModel = dailyRecordModel;
        _settlementModel = settlementModel;
        _customerOrderModel = customerOrderModel;
        _orderGeneratorModel = orderGeneratorModel;
        _deliveryModel = deliveryModel;
        _employeeModel = employeeModel;
        _maintenanceEffectModel = maintenanceEffectModel;
        _shopModel = shopModel;
        _achvModel = achvModel;
        _dayPresenter = dayPresenter;
        _settlementPresenter = settlementPresenter;
        _orderPresenter = orderPresenter;
        _deliveryPresenter = deliveryPresenter;
        _sceneNavHandler = sceneNavHandler;
    }

    #region ----- 이벤트 연결 -----

    /// <summary>
    /// 영업일과 결산 이벤트 연결
    /// </summary>
    public void ConnectEvents ()
    {
        _settlementPresenter.OnDailyConfirmed += ConfirmDailySettlement;
        _settlementPresenter.OnWeeklyConfirmed += ConfirmWeeklySettlement;
        _settlementPresenter.OnSettlementDebug += DebugOpenSettlement;

        _orderPresenter.OnOrderCompleted += TryStartSettlement;
        _deliveryPresenter.OnDeliveryClosed += TryStartSettlement;

        _dayPresenter.OnDayFinished += OpenSettlement;
        _dayPresenter.OnBankruptcyDetected += _sceneNavHandler.ShowBankruptcy;
    }

    /// <summary>
    /// 영업일과 결산 이벤트 해제
    /// </summary>
    public void DisconnectEvents ()
    {
        _settlementPresenter.OnDailyConfirmed -= ConfirmDailySettlement;
        _settlementPresenter.OnWeeklyConfirmed -= ConfirmWeeklySettlement;
        _settlementPresenter.OnSettlementDebug -= DebugOpenSettlement;

        _orderPresenter.OnOrderCompleted -= TryStartSettlement;
        _deliveryPresenter.OnDeliveryClosed -= TryStartSettlement;

        _dayPresenter.OnDayFinished -= OpenSettlement;
        _dayPresenter.OnBankruptcyDetected -= _sceneNavHandler.ShowBankruptcy;
    }

    #endregion

    #region ----- 결산 시작 -----

    /// <summary>
    /// 현재 영업일 결산 시작 확인
    /// </summary>
    void TryStartSettlement ()
    {
        _dayPresenter.TryStartSettlement( );
    }

    /// <summary>
    /// 결산 시작 요청 처리
    /// </summary>
    /// <param name="reason">영업 종료 사유</param>
    void OpenSettlement ( DayEndReason reason )
    {
        TryOpenSettlement( reason );
    }

    /// <summary>
    /// 배송 도착과 주문 기한을 처리한 뒤 일일 결산 시작
    /// </summary>
    /// <param name="reason">영업 종료 사유</param>
    /// <returns>결산 시작 성공 여부</returns>
    bool TryOpenSettlement ( DayEndReason reason )
    {
        int totalDay = _playStateModel.TotalDay;

        //당일 도착한 배송부터 확정
        DeliveryProcessResult deliveryResult =
            _deliveryModel.ProcessArrivals( totalDay );

        if ( deliveryResult != DeliveryProcessResult.Success )
        {
            Debug.LogError(
                $"배송 도착 처리 실패: {deliveryResult}" );
            return false;
        }

        //배송 처리 후 남은 주문의 기한 종료 처리
        _customerOrderModel.ProcessEndOfDay( totalDay );

        //현재 고용 직원의 근무일 기록
        _employeeModel.RecordCurrentEmployeesWorkDay( totalDay );

        //주간 종료일이면 결산 전에 주급 지급
        if ( TryPayWeeklyWage( ) == false )
            return false;

        //일일 기록 확정 전 업적 달성 판정
        EvaluateAchvs( );

        DailyRecord dailyRecord =
            _dailyRecordModel.CompleteDay( reason, _playStateModel.Budget );

        DailySettlementData settlementData =
            _settlementModel.CreateDaily(
                dailyRecord, _customerOrderModel.Orders );

        _sceneNavHandler.ShowDailySettlement( settlementData );

        return true;
    }

    /// <summary>
    /// 현재 영업일 기록으로 업적 달성 단계와 엔딩 조건 판정
    /// </summary>
    void EvaluateAchvs ()
    {
        int totalDay = _playStateModel.TotalDay;

        //업적 판정 데이터 생성
        AchvEvalContext context = new AchvEvalContext(
            _dailyRecordModel.Records,
            _dailyRecordModel.CurrentRecord,
            totalDay,
            _settlementModel.WeeklySettlements.Count,
            totalDay % 7 == 0 );

        //업적 확인
        bool hasReachedAchv = _achvModel.Evaluate( context );

        //이번 결산에서 마지막 업적을 새로 달성한 경우에만 엔딩 대기
        _isEndingPending = hasReachedAchv && _achvModel.AreAllCompleted;
    }

    /// <summary>
    /// 주간 종료일의 직원 주급 지급
    /// </summary>
    /// <returns>주급 처리 성공 여부</returns>
    bool TryPayWeeklyWage ()
    {
        int totalDay = _playStateModel.TotalDay;

        if ( totalDay % 7 != 0 )
            return true;

        EmployeeResult result =
            _employeeModel.PayWeeklyWage( totalDay, out float weeklyWage );

        if ( result == EmployeeResult.Success )
        {
            Debug.Log( $"직원 주급 지급 완료: {weeklyWage:N0}G" );
            return true;
        }

        if ( result == EmployeeResult.InsufficientBudget )
        {
            Debug.LogWarning( $"직원 주급 부족: 필요 금액 {weeklyWage:N0}G" );

            _sceneNavHandler.ShowBankruptcy( );
            return false;
        }

        if ( result ==
            EmployeeResult.WeeklyWageAlreadyPaid )
        {
            Debug.LogWarning( "현재 주간 결산의 직원 주급을 이미 지급했습니다." );
            return false;
        }

        Debug.LogWarning( $"직원 주급 처리 실패: {result}" );

        return false;
    }

    #endregion

    #region ----- 결산 확인 -----

    /// <summary>
    /// 일일 결산 확인 처리
    /// </summary>
    void ConfirmDailySettlement ()
    {
        //주간 종료일이 아니라면 엔딩 또는 다음 영업일 처리
        if ( _playStateModel.TotalDay % 7 != 0 )
        {
            if ( TryShowEnding( ) == true )
                return;

            StartNextDay( );
            return;
        }

        _pendingWeeklySettlement =
            _settlementModel.CreateWeekly(
                _dailyRecordModel.Records,
                _customerOrderModel.Orders,
                _playStateModel.TotalDay,
                _employeeModel.TotalHireCount );

        _sceneNavHandler.ShowWeeklySettlement(
            _pendingWeeklySettlement );
    }

    /// <summary>
    /// 주간 결산 확인 처리
    /// </summary>
    void ConfirmWeeklySettlement ()
    {
        if ( _pendingWeeklySettlement == null )
            return;

        _orderGeneratorModel.ApplyWeeklyAdjustment(
            _pendingWeeklySettlement.NextWeekAdjustment );

        _pendingWeeklySettlement = null;

        if ( TryShowEnding( ) == true )
            return;

        StartNextDay( );
    }

    /// <summary>
    /// 현재 결산에서 달성한 엔딩 표시
    /// </summary>
    /// <returns>엔딩 표시 여부</returns>
    bool TryShowEnding ()
    {
        if ( _isEndingPending == false )
            return false;

        //같은 엔딩의 반복 표시 차단
        _isEndingPending = false;
        _sceneNavHandler.ShowEnding( );

        return true;
    }

    #endregion

    #region ----- 다음 영업일 -----

    /// <summary>
    /// 엔딩 화면을 닫고 다음 영업일부터 계속 진행
    /// </summary>
    public void ContinueAfterEnding ()
    {
        _sceneNavHandler.CloseEnding( );
        StartNextDay( );
    }

    /// <summary>
    /// 다음 영업일 시작
    /// </summary>
    void StartNextDay ()
    {
        OnNextDayTransitionChanged?.Invoke( true );

        try
        {
            int nextTotalDay = _playStateModel.TotalDay + 1;

            //정비 효과 적용 결과, 상태
            MaintenanceEffectResult maintenanceResult =
                _maintenanceEffectModel.ApplyNextDayEffects(
                    out IReadOnlyList<MaintenanceEffectChangeSet> appliedChanges );

            if ( maintenanceResult != MaintenanceEffectResult.Success )
            {
                Debug.LogWarning( $"다음 영업일 정비 효과 적용 실패: {maintenanceResult}" );
                return;
            }

            if ( _playStateModel.SetTotalDay( nextTotalDay ) == false )
            {
                MaintenanceEffectResult rollbackResult =
                    _maintenanceEffectModel.RollbackAppliedEffects( appliedChanges );

                if ( rollbackResult !=
                    MaintenanceEffectResult.Success )
                {
                    Debug.LogWarning( $"다음 영업일 정비 효과 복구 실패: {rollbackResult}" );
                }

                return;
            }

            //주문량 페널티 하루 경과
            _customerOrderModel.PassPenaltyDay( );

            //새 영업일 기준 일반 재입고
            _shopModel.RestockForDay( nextTotalDay );

            //제작과 빠른 재입고 일일 상태 초기화
            if ( _businessDayModel.ResetDay( ) == false )
            {
                Debug.LogWarning( "다음 영업일 상태 초기화 실패" );
                return;
            }

            //새 영업일 거래 기록 시작
            _dailyRecordModel.StartDay(
                nextTotalDay,
                _playStateModel.Budget,
                _businessDayModel.CraftLimit,
                _businessDayModel.QuickRestockLimit );

            //새 영업일 주문 생성
            _orderPresenter.GenerateDailyOrders( );

            _sceneNavHandler.CloseSettlementForNextDay( );

            //모든 다음 영업일 처리가 끝난 후 자동 저장 요청
            OnNextDayStarted?.Invoke( );
        }
        finally
        {
            //성공과 실패 모두 후속 가이드 시작 잠금 해제
            OnNextDayTransitionChanged?.Invoke( false );
        }
    }

    #endregion

    #region ----- 검증 -----

#if UNITY_EDITOR
    /// <summary>
    /// 모든 업적을 달성 처리하고 실제 일일 결산을 통해 엔딩 검증 시작
    /// </summary>
    public void DebugOpenEndingSettlement ()
    {
        _achvModel.DebugCompleteAll( );

        if ( TryOpenSettlement(
            DayEndReason.CraftLimitReached ) == false )
        {
            return;
        }

        //일일 결산 확인 후 엔딩이 표시되도록 대기 상태 설정
        _isEndingPending = _achvModel.AreAllCompleted;
    }
#endif

    /// <summary>
    /// 현재 영업일 실제 결산 흐름 확인
    /// </summary>
    void DebugOpenSettlement ()
    {
        OpenSettlement( DayEndReason.CraftLimitReached );
    }

    /// <summary>
    /// 배송 도착과 결산 확인을 포함한 다음 영업일 전환 검증
    /// </summary>
    public void DebugStartNextDay ()
    {
        int currentDay =
            _playStateModel.TotalDay;

        if ( TryOpenSettlement(
            DayEndReason.CraftLimitReached ) == false )
        {
            return;
        }

        ConfirmDailySettlement( );

        if ( _pendingWeeklySettlement != null )
            ConfirmWeeklySettlement( );

        //날짜 전환 실패 시 배송을 다시 처리하지 않음
        if ( _playStateModel.TotalDay == currentDay )
            return;

        //새 날짜에 도착 예정인 배송 즉시 확정
        DeliveryProcessResult deliveryResult =
            _deliveryModel.ProcessArrivals( _playStateModel.TotalDay );

        if ( deliveryResult !=
            DeliveryProcessResult.Success )
        {
            Debug.LogError( $"배송 도착 처리 실패: {deliveryResult}" );
        }
    }

    #endregion
}
