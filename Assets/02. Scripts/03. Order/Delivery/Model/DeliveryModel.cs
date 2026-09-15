using System;
using System.Collections.Generic;

/// <summary>
/// 배송 모델 - 배송 예상과 확정 처리
/// </summary>
public class DeliveryModel
{
    /// <summary>
    /// 제작에 반드시 필요한 파츠 타입
    /// </summary>
    static readonly PartType [ ] MinimumPartTypes =
    {
        PartType.Body,
        PartType.Eye,
        PartType.Nose,
        PartType.Mouth
    };

    int _deliverySpanReduction;       //현재 배송 소요일 단축값

    DeliverySettingData _deliverySettings;       //배송 설정
    CraftScoreSettingsData _craftScoreSettings;       //제작 점수 설정
    CustomerOrderModel _orderModel;       //주문 모델
    CraftCompleteModel _craftCompleteModel;       //제작 완료 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    EmployeeModel _employeeModel;       //직원 고용과 배송 배치 상태

    Random _random = new Random( );       //배송 소요일 생성기

    Dictionary<string, PartsData> _parts = new Dictionary<string, PartsData>( );       //파츠 데이터
    Dictionary<string, DeliverySchedule> _deliverySchedules =
        new Dictionary<string, DeliverySchedule>( );       //주문별 배송 일정
    Dictionary<string, DeliveryResult> _deliveryResults =
        new Dictionary<string, DeliveryResult>( );       //배송 결과


    /// <summary>
    /// 현재 배송 소요일 단축값
    /// </summary>
    public int DeliverySpanReduction => _deliverySpanReduction;
    /// <summary>
    /// 기본 직접 배송 고정 비용
    /// </summary>
    public float DirectDeliveryCost => _deliverySettings.DirectDeliveryCost;

    /// <summary>
    /// 배송 출발 이벤트
    /// </summary>
    public event Action<string> OnDeliveryStarted;
    /// <summary>
    /// 배송 완료 이벤트
    /// </summary>
    public event Action<DeliveryResult> OnDeliveryCompleted;
    /// <summary>
    /// 직접 배송비 지출 이벤트
    /// </summary>
    public event Action<float> OnDeliveryCostPaid;


    /// <summary>
    /// 배송 주문 대금 계산용 파츠 집계
    /// </summary>
    class DeliveryPartSummary
    {
        /// <summary>
        /// 실제 파츠별 수량(파츠 아이디, 수량)
        /// </summary>
        public Dictionary<string, int> UsedPartQuantities { get; } = new Dictionary<string, int>( );

        /// <summary>
        /// 실제 파츠 데이터(파츠 아이디, 파츠 데이터)
        /// </summary>
        public Dictionary<string, PartsData> UsedParts { get; } = new Dictionary<string, PartsData>( );

        /// <summary>
        /// 사용한 모든 파츠의 기본 가격 합계
        /// </summary>
        public float BasePriceTotal { get; set; }
    }

    /// <summary>
    /// 배송 모델 생성
    /// </summary>
    /// <param name="deliverySettings">배송 설정 데이터</param>
    /// <param name="craftScoreSettings">제작 점수 설정 데이터</param>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="craftCompleteModel">제작 완료 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="employeeModel">직원 상태 모델</param>
    /// <param name="dataMap">상품 데이터 목록</param>
    public DeliveryModel (
        DeliverySettingData deliverySettings, CraftScoreSettingsData craftScoreSettings,
        CustomerOrderModel orderModel, CraftCompleteModel craftCompleteModel,
        PlayStateModel playStateModel, EmployeeModel employeeModel,
        PurchasableDataMap dataMap )
    {
        _deliverySettings = deliverySettings;
        _craftScoreSettings = craftScoreSettings;
        _orderModel = orderModel;
        _craftCompleteModel = craftCompleteModel;
        _playStateModel = playStateModel;
        _employeeModel = employeeModel;

        //배송 대금 계산에 사용할 파츠 데이터 등록
        AddParts( dataMap );
    }

    /// <summary>
    /// 상품 데이터 목록의 파츠 데이터 등록
    /// </summary>
    /// <param name="dataMap">상품 데이터 목록</param>
    void AddParts ( PurchasableDataMap dataMap )
    {
        if ( dataMap?.PurchasableDatas == null ) return;

        for ( int i = 0; i < dataMap.PurchasableDatas.Count; i++ )
        {
            if ( dataMap.PurchasableDatas [ i ] is PartsData part &&
                string.IsNullOrWhiteSpace( part.Id ) == false )
                _parts [ part.Id ] = part;
        }
    }

    /// <summary>
    /// 배송 소요일 단축값 설정
    /// </summary>
    /// <param name="reduction">적용할 배송 소요일 단축값</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetDeliverySpanReduction ( int reduction )
    {
        if ( reduction < 0 ) return false;

        _deliverySpanReduction = reduction;
        return true;
    }

    #region ----- 배송 예상 -----

    /// <summary>
    /// 주문 배송 예상 결과 계산
    /// </summary>
    /// <param name="orderId">배송할 주문 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="deliveryResult">배송 예상 결과</param>
    /// <returns>배송 예상 처리 결과</returns>
    public DeliveryProcessResult CalculatePreview (
        string orderId, int totalDay, out DeliveryResult deliveryResult )
    {
        deliveryResult = null;

        //배송과 제작 점수 설정 확인
        if ( _deliverySettings == null ||
            _deliverySettings.IsValid( ) == false ||
            _craftScoreSettings == null ||
            _parts.Count == 0 )
            return DeliveryProcessResult.InvalidSettings;

        //주문 조회
        if ( _orderModel.GetOrder(
            orderId, out CustomerOrder order ) == false )
            return DeliveryProcessResult.InvalidOrder;

        DeliverySchedule schedule;

        //배송 대기 주문의 출발 가능 여부 확인
        if ( order.ProgressState == OrderProgressState.Crafted )
        {
            OrderResult orderResult = _orderModel.CheckDeliveryStart(
                orderId, totalDay, out order );

            if ( orderResult != OrderResult.Success )
                return ConvertOrderResult( orderResult );

            schedule = GetOrCreateSchedule( orderId );
        }
        //배송 중 주문의 확정 일정 조회
        else if ( order.ProgressState == OrderProgressState.Shipping )
        {
            if ( _deliverySchedules.TryGetValue(
                orderId, out schedule ) == false ||
                schedule.IsStarted == false )
                return DeliveryProcessResult.InvalidOrder;
        }
        else
            return DeliveryProcessResult.InvalidOrder;

        //출발 전이면 현재 날짜 기준 예상 도착일 계산
        int departureTotalDay = schedule.IsStarted
            ? schedule.DepartureTotalDay
            : totalDay;
        int arrivalTotalDay = schedule.IsStarted
            ? schedule.ArrivalTotalDay
            : totalDay + schedule.DeliveryDays;

        DeliveryProcessResult result = CalculateDeliveryResult(
            order, departureTotalDay, arrivalTotalDay, out deliveryResult );

        if ( result == DeliveryProcessResult.Success && schedule.IsStarted )
            ApplyDeliveryMethod( deliveryResult, schedule );

        return result;
    }

    /// <summary>
    /// 지정 도착일 기준 배송 결과 계산
    /// </summary>
    /// <param name="order">배송 주문</param>
    /// <param name="departureTotalDay">배송 출발 누적 영업일</param>
    /// <param name="arrivalTotalDay">예상 또는 실제 도착 누적 영업일</param>
    /// <param name="deliveryResult">계산한 배송 결과</param>
    /// <returns>배송 결과 계산 처리 결과</returns>
    DeliveryProcessResult CalculateDeliveryResult (
        CustomerOrder order, int departureTotalDay,
        int arrivalTotalDay, out DeliveryResult deliveryResult )
    {
        deliveryResult = null;

        //도착일 기준 정상 또는 지연 배송 결과 확인
        OrderResult orderResult = _orderModel.CheckDeliveryOutcome(
            order, arrivalTotalDay, out OrderOutcome outcome );

        if ( orderResult != OrderResult.Success )
            return ConvertOrderResult( orderResult );

        //확정 제작 결과 조회
        if ( _craftCompleteModel.GetResult(
            order.OrderId, out CraftResult craftResult ) == false )
            return DeliveryProcessResult.CraftResultNotFound;

        //확정 제작 결과 확인
        if ( IsValidCraftResult( order.OrderId, craftResult ) == false )
            return DeliveryProcessResult.InvalidCraftResult;

        //실제 배치 파츠 집계
        if ( TryCreatePartSummary( craftResult.PlacedParts,
            out DeliveryPartSummary partSummary ) == false )
            return DeliveryProcessResult.InvalidCraftResult;

        //주문 기준점수 계산
        float standardScore = CalculateStandardScore( order );

        if ( standardScore <= 0f )
            return DeliveryProcessResult.InvalidSettings;

        //제작 점수 달성률과 배송 등급 계산
        float craftScore = craftResult.ReviewData.ScoreResult.FinalScore;
        float achievementRate = craftScore / standardScore;
        DeliveryGrade grade = _deliverySettings.GetGrade( achievementRate );

        //주문 조건으로 기본 주문 대금 계산
        if ( TryCalculateBaseOrderReward(
            order, craftResult.ReviewData, partSummary,
            out float baseOrderReward ) == false )
            return DeliveryProcessResult.InvalidCraftResult;

        //배송 보정 배율 조회
        float difficultyRate =
            _deliverySettings.GetDifficultyRate( order.Difficulty );
        float gradeRate =
            _deliverySettings.GetGradeRate( grade );
        float vipRate =
            _deliverySettings.GetVipRate( order.SpecialType );
        float deliveryRate =
            _deliverySettings.GetDeliveryRate( outcome );

        //최종 주문 대금 계산
        int finalOrderReward = CalculateFinalOrderReward(
            baseOrderReward, difficultyRate, gradeRate,
            vipRate, deliveryRate );

        int requirementRewardLimit = 0;

        //주요 요구 미달성 시 사용 파츠 기본가보다 낮게 지급
        if ( AreRequirementsCompleted(
            craftResult.ReviewData.RequirementResults ) == false )
        {
            requirementRewardLimit = CalculateRequirementRewardLimit(
                partSummary.BasePriceTotal );
            finalOrderReward = Math.Min(
                finalOrderReward, requirementRewardLimit );
        }

        deliveryResult = new DeliveryResult
        {
            OrderId = order.OrderId,
            DepartureTotalDay = departureTotalDay,
            ArrivalTotalDay = arrivalTotalDay,

            Outcome = outcome,
            CraftScore = craftScore,
            StandardScore = standardScore,

            AchievementRate = achievementRate,
            Grade = grade,
            BaseOrderReward = baseOrderReward,
            BasePriceTotal = partSummary.BasePriceTotal,
            RequirementRewardLimit = requirementRewardLimit,

            DifficultyRate = difficultyRate,
            GradeRate = gradeRate,
            VipRate = vipRate,
            DeliveryRate = deliveryRate,
            FinalOrderReward = finalOrderReward
        };

        return DeliveryProcessResult.Success;
    }

    /// <summary>
    /// 출발 시 확정한 배송 방식과 담당 정보 적용
    /// </summary>
    /// <param name="deliveryResult">배송 결과</param>
    /// <param name="schedule">확정된 배송 일정</param>
    void ApplyDeliveryMethod (
        DeliveryResult deliveryResult, DeliverySchedule schedule )
    {
        deliveryResult.IsMethodConfirmed = true;
        deliveryResult.Method = schedule.Method;
        deliveryResult.EmployeeId = schedule.EmployeeId;
        deliveryResult.DeliveryCost = schedule.DeliveryCost;
    }

    /// <summary>
    /// 주문별 배송 일정 조회 또는 생성
    /// </summary>
    /// <param name="orderId">배송 주문 아이디</param>
    /// <returns>배송 일정</returns>
    DeliverySchedule GetOrCreateSchedule ( string orderId )
    {
        if ( _deliverySchedules.TryGetValue(
            orderId, out DeliverySchedule schedule ) )
            return schedule;

        //최소와 최대 범위에서 기본 배송 소요일 생성
        int baseDeliveryDays = _random.Next(
            _deliverySettings.MinimumDeliveryDays,
            _deliverySettings.MaximumDeliveryDays + 1 );

        //현재 정비 효과를 적용하고 최소 배송 소요일 제한
        int deliveryDays = Math.Max(
            _deliverySettings.MinimumDeliveryDays,
            baseDeliveryDays - _deliverySpanReduction );

        schedule = new DeliverySchedule
        {
            OrderId = orderId,
            DeliveryDays = deliveryDays
        };

        _deliverySchedules.Add( orderId, schedule );
        return schedule;
    }

    /// <summary>
    /// 주문 배송 출발
    /// </summary>
    /// <param name="orderId">배송할 주문 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="method">선택한 배송 방식</param>
    /// <returns>배송 출발 처리 결과</returns>
    public DeliveryProcessResult StartDelivery (
        string orderId, int totalDay, DeliveryMethod method )
    {
        if ( string.IsNullOrWhiteSpace( orderId ) )
            return DeliveryProcessResult.InvalidOrder;

        if ( _deliveryResults.ContainsKey( orderId ) )
            return DeliveryProcessResult.AlreadyDelivered;

        //이미 출발한 배송 일정 확인
        if ( _deliverySchedules.TryGetValue(
            orderId, out DeliverySchedule savedSchedule ) &&
            savedSchedule.IsStarted )
            return DeliveryProcessResult.AlreadyShipping;

        //현재 출발 기준 배송 예상과 주문 상태 검증
        DeliveryProcessResult previewResult = CalculatePreview(
            orderId, totalDay, out DeliveryResult preview );

        if ( previewResult != DeliveryProcessResult.Success )
            return previewResult;

        //선택한 배송 방식 사용 가능 여부 확인
        DeliveryProcessResult methodResult =
            GetDeliveryMethodResult( method );

        if ( methodResult != DeliveryProcessResult.Success )
            return methodResult;

        string employeeId = null;
        float deliveryCost = 0f;

        //직원 배송은 배치 가능한 직원을 자동 선택
        if ( method == DeliveryMethod.Employee )
        {
            if ( _employeeModel.GetAvailableEmployee(
                EmployeeJobType.Delivery,
                out EmployeeState employeeState ) == false )
                return DeliveryProcessResult.EmployeeUnavailable;

            employeeId = employeeState.Data.Id;
        }
        else
        {
            deliveryCost = _deliverySettings.DirectDeliveryCost;
        }

        //직접 배송비를 먼저 차감
        if ( method == DeliveryMethod.Direct &&
            deliveryCost > 0f &&
            _playStateModel.SpendBudget( deliveryCost ) == false )
            return DeliveryProcessResult.DeliveryCostUpdateFailed;

        //자동 선택한 직원을 배송에 배치
        if ( method == DeliveryMethod.Employee &&
            _employeeModel.Assign(
                employeeId ) != EmployeeResult.Success )
            return DeliveryProcessResult.EmployeeUnavailable;

        DeliverySchedule schedule = _deliverySchedules [ orderId ];

        //확정된 배송 방식과 일정을 저장
        schedule.Method = method;
        schedule.EmployeeId = employeeId;
        schedule.DeliveryCost = deliveryCost;
        schedule.DepartureTotalDay = totalDay;
        schedule.ArrivalTotalDay = preview.ArrivalTotalDay;

        if ( _orderModel.StartDelivery( orderId ) != OrderResult.Success )
        {
            //주문 상태 변경 실패 시 배송 자원과 일정 복구
            bool isRestored = RestoreDeliveryMethod(
                method, employeeId, deliveryCost );

            schedule.Method = DeliveryMethod.Direct;
            schedule.EmployeeId = null;
            schedule.DeliveryCost = 0f;
            schedule.DepartureTotalDay = 0;
            schedule.ArrivalTotalDay = 0;

            return isRestored
                ? DeliveryProcessResult.OrderUpdateFailed
                : DeliveryProcessResult.RollbackFailed;
        }

        //성공한 직접 배송비만 일일 지출에 전달
        if ( deliveryCost > 0f )
            OnDeliveryCostPaid?.Invoke( deliveryCost );

        OnDeliveryStarted?.Invoke( orderId );

        //0일 배송은 출발 직후 같은 날 도착 처리
        if ( schedule.ArrivalTotalDay <= totalDay )
            return CompleteArrival( schedule, totalDay );

        return DeliveryProcessResult.Success;
    }

    /// <summary>
    /// 배송 방식 사용 가능 여부 확인
    /// </summary>
    /// <param name="method">배송 방식</param>
    /// <returns>배송 처리 결과</returns>
    public DeliveryProcessResult GetDeliveryMethodResult (
        DeliveryMethod method )
    {
        if ( method == DeliveryMethod.Direct )
        {
            if ( IsDirectDeliveryInUse( ) )
                return DeliveryProcessResult.DirectDeliveryInUse;

            if ( _playStateModel.CanSpendBudget(
                _deliverySettings.DirectDeliveryCost ) == false )
                return DeliveryProcessResult.DeliveryCostUpdateFailed;

            return DeliveryProcessResult.Success;
        }

        if ( method == DeliveryMethod.Employee )
        {
            return _employeeModel.GetAvailableEmployee(
                EmployeeJobType.Delivery, out _ )
                ? DeliveryProcessResult.Success
                : DeliveryProcessResult.EmployeeUnavailable;
        }

        return DeliveryProcessResult.InvalidMethod;
    }

    /// <summary>
    /// 기본 직접 배송 사용 여부 확인
    /// </summary>
    /// <returns>직접 배송 사용 여부</returns>
    bool IsDirectDeliveryInUse ()
    {
        foreach ( DeliverySchedule schedule in _deliverySchedules.Values )
        {
            //직접 배송 중인 일정이 있는지 확인
            if ( schedule.IsStarted &&
                schedule.Method == DeliveryMethod.Direct &&
                _deliveryResults.ContainsKey( schedule.OrderId ) == false )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 출발 실패 시 사용한 배송 자원 복구
    /// </summary>
    /// <param name="method">배송 방식</param>
    /// <param name="employeeId">배치한 직원 아이디</param>
    /// <param name="deliveryCost">차감한 배송비</param>
    /// <returns>복구 성공 여부</returns>
    bool RestoreDeliveryMethod (
        DeliveryMethod method, string employeeId, float deliveryCost )
    {
        //직접 배송일 때
        if ( method == DeliveryMethod.Direct )
        {
            //배송비 복구
            return deliveryCost <= 0f ||
                _playStateModel.AddBudget( deliveryCost );
        }

        //직원 배송 중 해제
        return _employeeModel.Release( employeeId ) == EmployeeResult.Success;
    }

    /// <summary>
    /// 지정 영업일에 도착한 배송 일괄 처리
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <returns>배송 도착 처리 결과</returns>
    public DeliveryProcessResult ProcessArrivals ( int totalDay )
    {
        foreach ( DeliverySchedule schedule in _deliverySchedules.Values )
        {
            //출발하지 않았거나 아직 도착일이 아니면 제외
            if ( schedule.IsStarted == false ||
                schedule.ArrivalTotalDay > totalDay )
                continue;

            //이미 완료한 배송 제외
            if ( _deliveryResults.ContainsKey( schedule.OrderId ) )
                continue;

            DeliveryProcessResult result = CompleteArrival(
                schedule, totalDay );

            //한 주문이라도 실패하면 결산 진행 중단
            if ( result != DeliveryProcessResult.Success )
                return result;
        }

        return DeliveryProcessResult.Success;
    }

    /// <summary>
    /// 도착한 주문 배송 확정
    /// </summary>
    /// <param name="schedule">확정할 배송 일정</param>
    /// <param name="totalDay">실제 도착 누적 영업일</param>
    /// <returns>배송 확정 처리 결과</returns>
    DeliveryProcessResult CompleteArrival (
        DeliverySchedule schedule, int totalDay )
    {
        if ( _orderModel.GetOrder(
            schedule.OrderId, out CustomerOrder order ) == false )
            return DeliveryProcessResult.InvalidOrder;

        //실제 도착일 기준 배송 결과 계산
        DeliveryProcessResult result = CalculateDeliveryResult(
            order, schedule.DepartureTotalDay,
            totalDay, out DeliveryResult pendingResult );

        if ( result != DeliveryProcessResult.Success )
            return result;

        //출발 시 확정한 배송 방식과 담당 정보 보존
        ApplyDeliveryMethod( pendingResult, schedule );

        //직원 배송 일정과 현재 배치 상태 확인
        if ( schedule.Method == DeliveryMethod.Employee )
        {
            //직원 배송 가능 상태 가져오기
            if ( _employeeModel.GetState(
                schedule.EmployeeId, out EmployeeState employeeState ) == false ||
                employeeState.IsAssigned == false )
                return DeliveryProcessResult.EmployeeUnavailable;
        }

        bool isHighGrade = IsHighGrade( pendingResult.Grade );

        //배송 확정에 필요한 플레이 상태 전체 사전 검증
        if ( _playStateModel.CanAddBudget(
            pendingResult.FinalOrderReward ) == false )
            return DeliveryProcessResult.BudgetUpdateFailed;

        if ( isHighGrade &&
            _playStateModel.CanAddHighGradeEvaluation( ) == false )
            return DeliveryProcessResult.HighGradeUpdateFailed;

        float previousBudget = _playStateModel.Budget;
        int previousHighGradeCount = _playStateModel.HighGradeEvaluationCount;

        //주문 상태 변경 이벤트 전에 확정 결과 등록
        DeliveryResult savedResult = CopyDeliveryResult( pendingResult );
        _deliveryResults.Add( schedule.OrderId, savedResult );

        //주문 대금 지급
        if ( pendingResult.FinalOrderReward > 0 &&
            _playStateModel.AddBudget(
                pendingResult.FinalOrderReward ) == false )
        {
            _deliveryResults.Remove( schedule.OrderId );
            return DeliveryProcessResult.BudgetUpdateFailed;
        }

        //B등급 이상 평가 누적
        if ( isHighGrade &&
            _playStateModel.AddHighGradeEvaluation( ) == false )
        {
            return FailDelivery(
                schedule.OrderId, previousBudget, previousHighGradeCount,
                DeliveryProcessResult.HighGradeUpdateFailed );
        }

        //배송 주문 종료
        if ( _orderModel.CompleteDelivery(
            schedule.OrderId, totalDay ) != OrderResult.Success )
        {
            return FailDelivery(
                schedule.OrderId, previousBudget,
                previousHighGradeCount,
                DeliveryProcessResult.OrderUpdateFailed );
        }

        //도착이 완료된 배송 직원 배치 해제
        if ( schedule.Method == DeliveryMethod.Employee &&
            _employeeModel.Release(
                schedule.EmployeeId ) != EmployeeResult.Success )
            return DeliveryProcessResult.RollbackFailed;

        //일일 기록에 배송 완료 전달
        OnDeliveryCompleted?.Invoke( CopyDeliveryResult( savedResult ) );

        return DeliveryProcessResult.Success;
    }

    /// <summary>
    /// 배송 처리 실패 상태 원상 복구
    /// </summary>
    /// <param name="orderId">배송 주문 아이디</param>
    /// <param name="previousBudget">이전 자금</param>
    /// <param name="previousHighGradeCount">이전 고등급 평가 수</param>
    /// <param name="failureResult">복구 성공 시 반환할 실패 결과</param>
    /// <returns>배송 처리 결과</returns>
    DeliveryProcessResult FailDelivery (
        string orderId, float previousBudget,
        int previousHighGradeCount,
        DeliveryProcessResult failureResult )
    {
        _deliveryResults.Remove( orderId );

        //변경된 플레이 상태를 배송 처리 전 값으로 복구
        bool budgetRestored =
            _playStateModel.Budget == previousBudget ||
            _playStateModel.SetBudget( previousBudget );
        bool highGradeRestored =
            _playStateModel.HighGradeEvaluationCount ==
            previousHighGradeCount ||
            _playStateModel.SetHighGradeEvaluationCount(
                previousHighGradeCount );

        return budgetRestored && highGradeRestored
            ? failureResult
            : DeliveryProcessResult.RollbackFailed;
    }

    /// <summary>
    /// B등급 이상 여부 확인
    /// </summary>
    /// <param name="grade">확인할 배송 등급</param>
    /// <returns>B등급 이상 여부</returns>
    bool IsHighGrade ( DeliveryGrade grade )
    {
        return grade == DeliveryGrade.S ||
            grade == DeliveryGrade.A ||
            grade == DeliveryGrade.B;
    }

    /// <summary>
    /// 주문 처리 결과를 배송 처리 결과로 변환
    /// </summary>
    /// <param name="orderResult">주문 처리 결과</param>
    /// <returns>배송 처리 결과</returns>
    DeliveryProcessResult ConvertOrderResult ( OrderResult orderResult )
    {
        if ( orderResult == OrderResult.Expired )
            return DeliveryProcessResult.Expired;

        return DeliveryProcessResult.InvalidOrder;
    }

    #endregion

    #region ----- 배송 결과 조회 -----

    /// <summary>
    /// 주문 배송 결과 보유 여부 확인
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <returns>배송 결과 보유 여부</returns>
    public bool HasDeliveryResult ( string orderId )
    {
        return string.IsNullOrWhiteSpace( orderId ) == false &&
            _deliveryResults.ContainsKey( orderId );
    }

    /// <summary>
    /// 주문 배송 결과 조회
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <param name="deliveryResult">조회한 배송 결과</param>
    /// <returns>배송 결과 조회 성공 여부</returns>
    public bool GetDeliveryResult (
        string orderId, out DeliveryResult deliveryResult )
    {
        deliveryResult = null;

        if ( string.IsNullOrWhiteSpace( orderId ) ) return false;

        if ( _deliveryResults.TryGetValue(
            orderId, out DeliveryResult savedResult ) == false )
            return false;

        deliveryResult = CopyDeliveryResult( savedResult );
        return true;
    }

    /// <summary>
    /// 배송 결과 복사
    /// </summary>
    /// <param name="source">복사할 배송 결과</param>
    /// <returns>복사한 배송 결과</returns>
    DeliveryResult CopyDeliveryResult ( DeliveryResult source )
    {
        return new DeliveryResult
        {
            IsMethodConfirmed = source.IsMethodConfirmed,
            OrderId = source.OrderId,
            DepartureTotalDay = source.DepartureTotalDay,
            ArrivalTotalDay = source.ArrivalTotalDay,

            Outcome = source.Outcome,
            CraftScore = source.CraftScore,
            StandardScore = source.StandardScore,
            AchievementRate = source.AchievementRate,
            Grade = source.Grade,

            BaseOrderReward = source.BaseOrderReward,
            BasePriceTotal = source.BasePriceTotal,
            RequirementRewardLimit = source.RequirementRewardLimit,

            DifficultyRate = source.DifficultyRate,
            GradeRate = source.GradeRate,
            VipRate = source.VipRate,
            DeliveryRate = source.DeliveryRate,
            FinalOrderReward = source.FinalOrderReward,

            Method = source.Method,
            EmployeeId = source.EmployeeId,
            DeliveryCost = source.DeliveryCost
        };
    }

    #endregion

    #region ----- 점수와 보상 계산 -----

    /// <summary>
    /// 주문별 평가 기준점수 계산
    /// </summary>
    /// <param name="order">배송할 주문</param>
    /// <returns>주문 평가 기준점수</returns>
    float CalculateStandardScore ( CustomerOrder order )
    {
        //조립 완성과 전체 주요 요구, 희망 달성 점수 합산
        return _craftScoreSettings.AssemblyCompletedScore +
            order.Requirements.Count *
            _craftScoreSettings.RequirementCompletedScore +
            order.Wishes.Count *
            _craftScoreSettings.WishCompletedScore;
    }

    /// <summary>
    /// 배송 배율을 적용한 최종 주문 대금 계산
    /// </summary>
    /// <param name="baseOrderReward">기본 주문 대금</param>
    /// <param name="difficultyRate">난이도 보정 배율</param>
    /// <param name="gradeRate">등급 보정 배율</param>
    /// <param name="vipRate">VIP 주문 보정 배율</param>
    /// <param name="deliveryRate">배송 결과 보정 배율</param>
    /// <returns>반올림된 최종 주문 대금</returns>
    int CalculateFinalOrderReward (
        float baseOrderReward, float difficultyRate,
        float gradeRate, float vipRate,
        float deliveryRate )
    {
        //모든 배송 보정 적용
        float reward = baseOrderReward *
            difficultyRate * gradeRate *
            vipRate * deliveryRate;

        int roundUnit = _deliverySettings.RewardRoundUnit;

        //지정 단위로 일반 반올림
        return ( int ) Math.Round( reward / roundUnit, MidpointRounding.AwayFromZero ) * roundUnit;
    }

    /// <summary>
    /// 주요 요구 전체 달성 여부 확인
    /// </summary>
    /// <param name="requirements">주요 요구 판정 목록</param>
    /// <returns>주요 요구 전체 달성 여부</returns>
    bool AreRequirementsCompleted (
        IReadOnlyList<CraftConditionResult> requirements )
    {
        if ( requirements == null ) return false;

        for ( int i = 0; i < requirements.Count; i++ )
        {
            if ( requirements [ i ] == null ||
                requirements [ i ].IsCompleted == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 주요 요구 미달성 주문 대금 상한 계산
    /// </summary>
    /// <param name="basePriceTotal">사용 파츠 기본 가격 합계</param>
    /// <returns>지급 단위로 내림한 주문 대금 상한</returns>
    int CalculateRequirementRewardLimit ( float basePriceTotal )
    {
        float limit = basePriceTotal *
            _deliverySettings.RequirementFailedRewardRate;
        int roundUnit = _deliverySettings.RewardRoundUnit;

        //반올림으로 파츠 기본가 이상이 되지 않도록 지정 단위로 내림
        return ( int ) Math.Floor( limit / roundUnit ) * roundUnit;
    }

    #endregion

    #region ----- 주문 대금 계산 -----

    /// <summary>
    /// 실제 배치 파츠 수량과 기본 가격 합계 집계
    /// </summary>
    /// <param name="placedParts">확정 배치 파츠 목록</param>
    /// <param name="partSummary">배치 파츠 집계</param>
    /// <returns>집계 성공 여부</returns>
    bool TryCreatePartSummary (
        IReadOnlyList<PlacedPartData> placedParts,
        out DeliveryPartSummary partSummary )
    {
        partSummary = null;

        if ( placedParts == null || placedParts.Count == 0 )
            return false;

        var summary = new DeliveryPartSummary( );

        for ( int i = 0; i < placedParts.Count; i++ )
        {
            PlacedPartData placedPart = placedParts [ i ];

            if ( placedPart?.PartData == null ||
                string.IsNullOrWhiteSpace( placedPart.PartData.Id ) )
                return false;

            PartsData part = placedPart.PartData;

            //잘못된 기본 가격 차단
            if ( float.IsNaN( part.BasePrice ) ||
                float.IsInfinity( part.BasePrice ) ||
                part.BasePrice < 0f )
                return false;

            //실제 파츠 수량 추가
            summary.UsedPartQuantities.TryGetValue( part.Id, out int quantity );
            summary.UsedPartQuantities [ part.Id ] = quantity + 1;
            summary.UsedParts [ part.Id ] = part;

            //사용 파츠의 기본 가격 추가
            summary.BasePriceTotal += part.BasePrice;
        }

        partSummary = summary;
        return true;
    }

    /// <summary>
    /// 주문 조건으로 기본 주문 대금 계산
    /// </summary>
    /// <param name="order">배송할 주문</param>
    /// <param name="review">확정 제작 판정</param>
    /// <param name="partSummary">실제 배치 파츠 집계</param>
    /// <param name="baseOrderReward">기본 주문 대금</param>
    /// <returns>기본 주문 대금 계산 성공 여부</returns>
    bool TryCalculateBaseOrderReward (
        CustomerOrder order, CraftReviewData review,
        DeliveryPartSummary partSummary,
        out float baseOrderReward )
    {
        baseOrderReward = 0f;

        var orderPartQuantities = new Dictionary<string, int>( );

        //모든 주요 요구 파츠 수량 반영
        if ( AddRequirementQuantities(
            order.Requirements, orderPartQuantities ) == false )
            return false;

        //달성한 희망 파츠 수량 반영
        if ( AddWishQuantities(
            review.WishResults,
            partSummary, orderPartQuantities ) == false )
            return false;

        //주문 조건에 포함되지 않은 최소 조립 타입 추가
        for ( int i = 0; i < MinimumPartTypes.Length; i++ )
        {
            PartType partType = MinimumPartTypes [ i ];

            if ( HasOrderPartType( orderPartQuantities, partType ) )
                continue;

            if ( TryGetCheapestUsedPart(
                partSummary, partType, out string partId ) == false )
                return false;

            AddOrderPartQuantity(
                orderPartQuantities, partId, 1 );
        }

        //주문 파츠별 가격 합산
        foreach ( var pair in orderPartQuantities )
        {
            if ( _parts.TryGetValue( pair.Key, out PartsData part ) == false )
                return false;

            baseOrderReward += part.BasePrice * pair.Value;
        }

        return true;
    }

    /// <summary>
    /// 주요 요구 파츠 수량 반영
    /// </summary>
    /// <param name="requirements">주요 요구 목록</param>
    /// <param name="orderPartQuantities">주문 파츠별 수량</param>
    /// <returns>수량 반영 성공 여부</returns>
    bool AddRequirementQuantities (
        IReadOnlyList<OrderPartCondition> requirements,
        Dictionary<string, int> orderPartQuantities )
    {
        if ( requirements == null ) return false;

        for ( int i = 0; i < requirements.Count; i++ )
        {
            OrderPartCondition requirement = requirements [ i ];

            //잘못된 주요 요구나 파츠 데이터 차단
            if ( requirement == null || string.IsNullOrWhiteSpace( requirement.PartId ) ||
                requirement.Quantity <= 0 ||
                _parts.ContainsKey( requirement.PartId ) == false )
                return false;

            AddOrderPartQuantity(
                orderPartQuantities,
                requirement.PartId, requirement.Quantity );
        }

        return true;
    }

    /// <summary>
    /// 달성한 희망 파츠 수량 반영
    /// </summary>
    /// <param name="wishes">희망 판정 목록</param>
    /// <param name="partSummary">실제 배치 파츠 집계</param>
    /// <param name="orderPartQuantities">주문 파츠별 수량</param>
    /// <returns>수량 반영 성공 여부</returns>
    bool AddWishQuantities (
        IReadOnlyList<CraftConditionResult> wishes,
        DeliveryPartSummary partSummary,
        Dictionary<string, int> orderPartQuantities )
    {
        if ( wishes == null ) return false;

        for ( int i = 0; i < wishes.Count; i++ )
        {
            CraftConditionResult wish = wishes [ i ];

            //희망 사항이 이상하거나 아이디가 이상하면 종료
            if ( wish == null || string.IsNullOrWhiteSpace( wish.PartId ) ||
                wish.RequiredQuantity <= 0 || wish.UsedQuantity < 0 )
                return false;

            //미달성 희망은 기본 주문 대금에서 제외
            if ( wish.IsCompleted == false ) continue;

            //실제 배치된 희망 파츠 확인
            if ( partSummary.UsedPartQuantities.TryGetValue(
                wish.PartId, out int usedQuantity ) == false ||
                wish.RequiredQuantity > usedQuantity )
                return false;

            AddOrderPartQuantity(
                orderPartQuantities, wish.PartId, wish.RequiredQuantity );
        }

        return true;
    }

    /// <summary>
    /// 주문 파츠별 수량 추가
    /// </summary>
    /// <param name="quantities">주문 파츠별 수량</param>
    /// <param name="partId">추가할 파츠 아이디</param>
    /// <param name="quantity">추가할 파츠 수량</param>
    void AddOrderPartQuantity (
        Dictionary<string, int> quantities,
        string partId, int quantity )
    {
        //같은 파츠 조건은 더 큰 수량만 유지
        if ( quantities.TryGetValue( partId, out int savedQuantity ) )
            quantities [ partId ] = Math.Max( savedQuantity, quantity );
        else
            quantities.Add( partId, quantity );
    }

    /// <summary>
    /// 주문 파츠의 필수 타입 포함 여부 확인
    /// </summary>
    /// <param name="orderPartQuantities">주문 파츠별 수량</param>
    /// <param name="partType">확인할 파츠 타입</param>
    /// <returns>파츠 타입 포함 여부</returns>
    bool HasOrderPartType (
        IReadOnlyDictionary<string, int> orderPartQuantities,
        PartType partType )
    {
        foreach ( var pair in orderPartQuantities )
        {
            //수량이 있고 확인할 타입과 같은 파츠면 포함
            if ( pair.Value > 0 && _parts.TryGetValue(
                pair.Key, out PartsData part ) && part.PartType == partType )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 사용한 특정 타입의 최저 가격 파츠 조회
    /// </summary>
    /// <param name="partSummary">실제 배치 파츠 집계</param>
    /// <param name="partType">조회할 파츠 타입</param>
    /// <param name="partId">조회한 파츠 아이디</param>
    /// <returns>파츠 조회 성공 여부</returns>
    bool TryGetCheapestUsedPart (
        DeliveryPartSummary partSummary,
        PartType partType, out string partId )
    {
        partId = null;
        float minimumPrice = float.MaxValue;

        //실제 사용한 파츠에서 같은 타입의 최저 가격 파츠 조회
        foreach ( var pair in partSummary.UsedParts )
        {
            PartsData part = pair.Value;

            //같은 타입의 더 저렴한 파츠 저장
            if ( part.PartType == partType && part.BasePrice < minimumPrice )
            {
                minimumPrice = part.BasePrice;
                partId = pair.Key;
            }
        }

        return partId != null;
    }

    #endregion

    #region ----- 제작 결과 확인 -----

    /// <summary>
    /// 확정 제작 결과 유효성 확인
    /// </summary>
    /// <param name="orderId">배송 주문 아이디</param>
    /// <param name="craftResult">확정 제작 결과</param>
    /// <returns>제작 결과 유효 여부</returns>
    bool IsValidCraftResult (
        string orderId, CraftResult craftResult )
    {
        return craftResult != null &&
            craftResult.OrderId == orderId &&
            craftResult.PlacedParts != null &&
            craftResult.PlacedParts.Count > 0 &&
            craftResult.ReviewData != null &&
            craftResult.ReviewData.IsAssemblyCompleted &&
            craftResult.ReviewData.RequirementResults != null &&
            craftResult.ReviewData.WishResults != null &&
            craftResult.ReviewData.ScoreResult != null;
    }

    #endregion

    #region ----- 저장/복구 -----

    /// <summary>
    /// 현재 배송 일정과 결과 저장 데이터 생성
    /// </summary>
    public DeliverySaveData CreateSaveData ()
    {
        //배송 일정 세이브 데이터 목록 생성
        var schedules = new List<DeliveryScheduleSaveData>( _deliverySchedules.Count );

        //배송 일정 목록에 배송 일정 세이브 데이터 추가
        foreach ( DeliverySchedule schedule in _deliverySchedules.Values )
            schedules.Add( new DeliveryScheduleSaveData( schedule ) );

        //배송 결과 세이브 데이터 목록 생성
        var results = new List<DeliveryResultSaveData>( _deliveryResults.Count );

        //배송 결과 목록에 결과 세이브 데이터 추가
        foreach ( DeliveryResult result in _deliveryResults.Values )
            results.Add( new DeliveryResultSaveData( result ) );

        //배송 세이브 데이터 반환
        return new DeliverySaveData( _deliverySpanReduction, schedules, results );
    }

    /// <summary>
    /// 검증된 배송 일정과 결과 복구
    /// </summary>
    /// <param name="restoreState">배송 복구 상태</param>
    public void Restore ( DeliveryRestoreState restoreState )
    {
        _deliverySpanReduction =
            restoreState.DeliverySpanReduction;
        _deliverySchedules =
            restoreState.CreateSchedules( );
        _deliveryResults =
            restoreState.CreateResults( );
    }

    #endregion
}
