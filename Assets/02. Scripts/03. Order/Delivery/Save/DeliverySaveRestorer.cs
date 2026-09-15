using System;
using System.Collections.Generic;

/// <summary>
/// 배송 저장 복구 처리 - 저장 데이터 검증과 배송 복구 상태 생성
/// </summary>
public class DeliverySaveRestorer
{
    #region ----- 배송 상태 복구 -----

    /// <summary>
    /// 배송 저장 데이터를 검증하고 복구 상태 생성
    /// </summary>
    /// <param name="saveData">배송 저장 데이터</param>
    /// <param name="orderData">주문 저장 데이터</param>
    /// <param name="employeeState">직원 복구 상태</param>
    /// <param name="restoreState">생성한 배송 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        DeliverySaveData saveData,
        OrderSaveData orderData,
        EmployeeRestoreState employeeState,
        out DeliveryRestoreState restoreState )
    {
        restoreState = null;

        //배송 일정, 배송 결과 가져오기
        if ( TryCreateRestoredState(
            saveData, orderData, employeeState,
            out Dictionary<string, DeliverySchedule> schedules,
            out Dictionary<string, DeliveryResult> results ) == false )
        {
            return false;
        }

        restoreState = new DeliveryRestoreState(
            saveData.DeliverySpanReduction, schedules, results );

        return true;
    }

    /// <summary>
    /// 배송 저장 데이터를 주문별 배송 일정과 결과로 변환
    /// </summary>
    /// <param name="saveData">배송 저장 데이터</param>
    /// <param name="orderData">주문 저장 데이터</param>
    /// <param name="employeeState">직원 복구 상태</param>
    /// <param name="schedules">배송 일정</param>
    /// <param name="results">배송 결과</param>
    /// <returns>배송 상태 생성 성공 여부</returns>
    bool TryCreateRestoredState (
        DeliverySaveData saveData, OrderSaveData orderData,
        EmployeeRestoreState employeeState,
        out Dictionary<string, DeliverySchedule> schedules,
        out Dictionary<string, DeliveryResult> results )
    {
        schedules = null;
        results = null;

        //데이터 확인 후 고객 주문 세이브 데이터 가져오기
        if ( saveData?.Schedules == null ||
            saveData.Results == null ||
            saveData.DeliverySpanReduction < 0 ||
            employeeState == null ||
            TryCreateOrderSaveDataMap(
                orderData,
                out Dictionary<string, CustomerOrderSaveData> orders ) == false )
        {
            return false;
        }

        var pendingSchedules =
            new Dictionary<string, DeliverySchedule>( );

        for ( int i = 0; i < saveData.Schedules.Count; i++ )
        {
            DeliveryScheduleSaveData data = saveData.Schedules [ i ];

            //배송 일정 가져오기 및 딕셔너리에 추가
            if ( TryCreateSchedule(
                data, orders, employeeState,
                out DeliverySchedule schedule ) == false ||
                pendingSchedules.TryAdd( schedule.OrderId, schedule ) == false )
            {
                return false;
            }
        }

        var pendingResults =
            new Dictionary<string, DeliveryResult>( );

        for ( int i = 0; i < saveData.Results.Count; i++ )
        {
            DeliveryResultSaveData data = saveData.Results [ i ];

            //배송 결과 가져오기 및 딕셔너리에 추가
            if ( TryCreateDeliveryResult(
                data, pendingSchedules,
                out DeliveryResult result ) == false ||
                pendingResults.TryAdd( result.OrderId, result ) == false )
            {
                return false;
            }
        }

        int directShippingCount = 0;

        foreach ( DeliverySchedule schedule in pendingSchedules.Values )
        {
            //현재 진행 중인 직접 배송만 슬롯 활성 대상으로 계산
            if ( schedule.IsStarted == true &&
                schedule.Method == DeliveryMethod.Direct &&
                pendingResults.ContainsKey( schedule.OrderId ) == false )
            {
                directShippingCount++;
            }
        }

        if ( directShippingCount > 1 ||
            ValidateOrderDeliveryStates(
                orders, pendingSchedules, pendingResults ) == false )
        {
            return false;
        }

        schedules = pendingSchedules;
        results = pendingResults;
        return true;
    }

    /// <summary>
    /// 주문 저장 데이터 맵 생성
    /// </summary>
    /// <param name="saveData">주문 저장 데이터</param>
    /// <param name="orders">주문별 저장 데이터</param>
    /// <returns>주문 저장 데이터 맵 생성 성공 여부</returns>
    bool TryCreateOrderSaveDataMap (
        OrderSaveData saveData,
        out Dictionary<string, CustomerOrderSaveData> orders )
    {
        orders = new Dictionary<string, CustomerOrderSaveData>( );

        if ( saveData?.Orders == null )
            return false;

        for ( int i = 0; i < saveData.Orders.Count; i++ )
        {
            CustomerOrderSaveData order = saveData.Orders [ i ];

            if ( order == null ||
                string.IsNullOrWhiteSpace( order.OrderId ) == true ||
                orders.TryAdd( order.OrderId, order ) == false )
            {
                orders = null;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 배송 일정 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="data">배송 일정 저장 데이터</param>
    /// <param name="orders">주문별 저장 데이터</param>
    /// <param name="employeeState">직원 복구 상태</param>
    /// <param name="schedule">생성한 배송 일정</param>
    /// <returns>배송 일정 생성 성공 여부</returns>
    bool TryCreateSchedule (
        DeliveryScheduleSaveData data,
        IReadOnlyDictionary<string, CustomerOrderSaveData> orders,
        EmployeeRestoreState employeeState,
        out DeliverySchedule schedule )
    {
        schedule = null;

        if ( data == null ||
            string.IsNullOrWhiteSpace( data.OrderId ) == true ||
            orders.ContainsKey( data.OrderId ) == false ||
            data.DeliveryDays < 0 ||
            data.DepartureTotalDay < 0 ||
            data.ArrivalTotalDay < 0 ||
            IsFiniteSaveValue( data.DeliveryCost ) == false ||
            data.DeliveryCost < 0f ||
            Enum.IsDefined( typeof( DeliveryMethod ), data.Method ) == false )
        {
            return false;
        }

        bool isStarted = data.DepartureTotalDay > 0;

        if ( isStarted == false )
        {
            if ( data.ArrivalTotalDay != 0 ||
                data.DeliveryCost != 0f ||
                string.IsNullOrEmpty( data.EmployeeId ) == false )
            {
                return false;
            }
        }
        else if ( data.ArrivalTotalDay !=
            data.DepartureTotalDay + data.DeliveryDays )
        {
            return false;
        }

        if ( data.Method == DeliveryMethod.Direct )
        {
            if ( string.IsNullOrEmpty( data.EmployeeId ) == false )
                return false;
        }
        else if ( isStarted == true )
        {
            if ( string.IsNullOrWhiteSpace( data.EmployeeId ) == true ||
                employeeState.ContainsEmployee( data.EmployeeId ) == false ||
                data.DeliveryCost != 0f )
            {
                return false;
            }
        }

        schedule = new DeliverySchedule
        {
            OrderId = data.OrderId,
            DeliveryDays = data.DeliveryDays,
            DepartureTotalDay = data.DepartureTotalDay,
            ArrivalTotalDay = data.ArrivalTotalDay,
            Method = data.Method,
            EmployeeId = data.EmployeeId,
            DeliveryCost = data.DeliveryCost
        };

        return true;
    }

    /// <summary>
    /// 배송 결과 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="data">배송 결과 저장 데이터</param>
    /// <param name="schedules">주문별 배송 일정</param>
    /// <param name="result">생성한 배송 결과</param>
    /// <returns>배송 결과 생성 성공 여부</returns>
    bool TryCreateDeliveryResult (
        DeliveryResultSaveData data,
        IReadOnlyDictionary<string, DeliverySchedule> schedules,
        out DeliveryResult result )
    {
        result = null;

        if ( data == null ||
            string.IsNullOrWhiteSpace( data.OrderId ) == true ||
            data.IsMethodConfirmed == false ||
            Enum.IsDefined( typeof( OrderOutcome ), data.Outcome ) == false ||
            Enum.IsDefined( typeof( DeliveryGrade ), data.Grade ) == false ||
            Enum.IsDefined( typeof( DeliveryMethod ), data.Method ) == false ||
            data.FinalOrderReward < 0 ||
            data.RequirementRewardLimit < 0 ||
            IsValidDeliveryNumbers( data ) == false ||
            schedules.TryGetValue(
                data.OrderId, out DeliverySchedule schedule ) == false ||
            schedule.IsStarted == false )
        {
            return false;
        }

        if ( data.Outcome != OrderOutcome.NormalDelivery &&
            data.Outcome != OrderOutcome.LateDelivery )
        {
            return false;
        }

        if ( schedule.DepartureTotalDay != data.DepartureTotalDay ||
            schedule.ArrivalTotalDay != data.ArrivalTotalDay ||
            schedule.Method != data.Method ||
            schedule.EmployeeId != data.EmployeeId ||
            schedule.DeliveryCost != data.DeliveryCost )
        {
            return false;
        }

        result = new DeliveryResult
        {
            IsMethodConfirmed = data.IsMethodConfirmed,
            OrderId = data.OrderId,
            DepartureTotalDay = data.DepartureTotalDay,
            ArrivalTotalDay = data.ArrivalTotalDay,
            Outcome = data.Outcome,

            CraftScore = data.CraftScore,
            StandardScore = data.StandardScore,
            AchievementRate = data.AchievementRate,
            Grade = data.Grade,

            BaseOrderReward = data.BaseOrderReward,
            BasePriceTotal = data.BasePriceTotal,
            RequirementRewardLimit = data.RequirementRewardLimit,

            DifficultyRate = data.DifficultyRate,
            GradeRate = data.GradeRate,
            VipRate = data.VipRate,
            DeliveryRate = data.DeliveryRate,

            FinalOrderReward = data.FinalOrderReward,
            Method = data.Method,
            EmployeeId = data.EmployeeId,
            DeliveryCost = data.DeliveryCost
        };

        return true;
    }

    #endregion

    #region ----- 배송 상태 검증 -----

    /// <summary>
    /// 배송 결과 수치 검증
    /// </summary>
    /// <param name="data">배송 결과 저장 데이터</param>
    /// <returns>배송 결과 수치 정상 여부</returns>
    bool IsValidDeliveryNumbers ( DeliveryResultSaveData data )
    {
        return data.DepartureTotalDay > 0 &&
            data.ArrivalTotalDay >= data.DepartureTotalDay &&
            IsFiniteSaveValue( data.CraftScore ) == true &&
            IsFiniteSaveValue( data.StandardScore ) == true &&
            IsFiniteSaveValue( data.AchievementRate ) == true &&
            IsFiniteSaveValue( data.BaseOrderReward ) == true &&
            IsFiniteSaveValue( data.BasePriceTotal ) == true &&
            IsFiniteSaveValue( data.DifficultyRate ) == true &&
            IsFiniteSaveValue( data.GradeRate ) == true &&
            IsFiniteSaveValue( data.VipRate ) == true &&
            IsFiniteSaveValue( data.DeliveryRate ) == true &&
            IsFiniteSaveValue( data.DeliveryCost ) == true;
    }

    /// <summary>
    /// 주문 진행 상태와 배송 데이터 관계 검증
    /// </summary>
    /// <param name="orders">주문별 저장 데이터</param>
    /// <param name="schedules">주문별 배송 일정</param>
    /// <param name="results">주문별 배송 결과</param>
    /// <returns>주문과 배송 데이터 관계 정상 여부</returns>
    bool ValidateOrderDeliveryStates (
        IReadOnlyDictionary<string, CustomerOrderSaveData> orders,
        IReadOnlyDictionary<string, DeliverySchedule> schedules,
        IReadOnlyDictionary<string, DeliveryResult> results )
    {
        foreach ( CustomerOrderSaveData order in orders.Values )
        {
            schedules.TryGetValue(
                order.OrderId, out DeliverySchedule schedule );
            results.TryGetValue(
                order.OrderId, out DeliveryResult result );

            bool hasSchedule = schedule != null;
            bool hasResult = result != null;

            if ( order.ProgressState == OrderProgressState.Crafted )
            {
                if ( hasResult == true ||
                    ( hasSchedule == true && schedule.IsStarted == true ) )
                {
                    return false;
                }

                continue;
            }

            if ( order.ProgressState == OrderProgressState.Shipping )
            {
                if ( hasSchedule == false ||
                    schedule.IsStarted == false ||
                    hasResult == true )
                {
                    return false;
                }

                continue;
            }

            bool isDelivered =
                order.ProgressState == OrderProgressState.Closed &&
                ( order.Outcome == OrderOutcome.NormalDelivery ||
                order.Outcome == OrderOutcome.LateDelivery );

            if ( isDelivered == true )
            {
                if ( hasSchedule == false ||
                    schedule.IsStarted == false ||
                    hasResult == false ||
                    result.Outcome != order.Outcome )
                {
                    return false;
                }

                continue;
            }

            if ( hasSchedule == true || hasResult == true )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 유한한 저장 수치인지 확인
    /// </summary>
    /// <param name="value">확인할 수치</param>
    /// <returns>유한한 수치 여부</returns>
    bool IsFiniteSaveValue ( float value )
    {
        return float.IsNaN( value ) == false &&
            float.IsInfinity( value ) == false;
    }

    #endregion
}
