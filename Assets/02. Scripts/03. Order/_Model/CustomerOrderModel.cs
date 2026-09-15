using System;
using System.Collections.Generic;

/// <summary>
/// 고객 주문 모델 - 주문 보관, 조회, 상태/기한 처리
/// </summary>
public class CustomerOrderModel
{
    public const int DefaultWaitingLimit = 5;      //기본 수락 대기 제한
    public const int MaxWaitingLimit = 20;     //최대 수락 대기 제한
    public const int MaxPenaltyDays = 14;       //주문량 페널티 최대 적용 일수

    int _waitingLimit;       //현재 수락 대기 제한

    OrderPenaltyType _penaltyType;       //현재 페널티 원인
    int _orderReduction;       //일일 주문 감소량
    int _penaltyRemainingDays;       //페널티 남은 일수
    int _nextCreatedNumber = 1;       //다음 주문 생성 번호

    /// <summary>
    /// 손님 주문 딕셔너리(주문 아이디, 주문 정보)
    /// </summary>
    Dictionary<string, CustomerOrder> _orders = new Dictionary<string, CustomerOrder>( );
    HashSet<string> _protectedOrderIds =
        new HashSet<string>( );       //거절과 기한 종료에서 보호할 주문

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 주문 리스트(읽기 전용)
    /// </summary>
    public IReadOnlyCollection<CustomerOrder> Orders => _orders.Values;

    /// <summary>
    /// 전체 주문 수
    /// </summary>
    public int TotalCount => _orders.Count;

    /// <summary>
    /// 대기 주문 수
    /// </summary>
    public int WaitingCount
    {
        get => GetOrderCount( OrderProgressState.Waiting );
    }

    /// <summary>
    /// 현재 수락 대기 제한 수
    /// </summary>
    public int WaitingLimit => _waitingLimit;

    /// <summary>
    /// 현재 주문량 페널티 원인
    /// </summary>
    public OrderPenaltyType PenaltyType => _penaltyType;

    /// <summary>
    /// 일일 주문 감소량
    /// </summary>
    public int OrderReduction => _orderReduction;

    /// <summary>
    /// 페널티 남은 일수
    /// </summary>
    public int PenaltyRemainingDays => _penaltyRemainingDays;

    /// <summary>
    /// 다음 주문 생성 번호
    /// </summary>
    public int NextCreatedNumber => _nextCreatedNumber;

    /// <summary>
    /// 주문량 페널티 적용 여부
    /// </summary>
    public bool IsPenaltyActive => _orderReduction > 0 && _penaltyRemainingDays > 0;

    /// <summary>
    /// 다음 영업일에 적용될 주문 감소량
    /// </summary>
    public int NextDayOrderReduction =>
        IsPenaltyActive && _penaltyRemainingDays > 1
            ? _orderReduction
            : 0;

    #endregion

    #region ----- 이벤트 -----
    /// <summary>
    /// 주문 변경 이벤트
    /// </summary>
    public event Action OnOrderChanged;

    /// <summary>
    /// 주문량 페널티 변경 이벤트
    /// </summary>
    public event Action OnPenaltyChanged;

    /// <summary>
    /// 배송 이외 주문 종료 이벤트(주문 아이디, 종료 결과)
    /// </summary>
    public event Action<string, OrderOutcome> OnOrderClosed;
    #endregion

    /// <summary>
    /// 고객 주문 모델 생성
    /// </summary>
    /// <param name="waitingLimit">수락 대기 제한</param>
    public CustomerOrderModel ( int waitingLimit = DefaultWaitingLimit )
    {
        _waitingLimit = waitingLimit;
    }

    #region ----- 조회 -----

    /// <summary>
    /// 주문 조회
    /// </summary>
    /// <param name="id">주문 아이디</param>
    /// <param name="order">조회한 주문</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetOrder ( string id, out CustomerOrder order )
    {
        //기본값 설정
        order = null;

        //잘못된 아이디 차단
        if ( string.IsNullOrWhiteSpace( id ) ) return false;

        //주문 아이디, 주문 반환
        return _orders.TryGetValue( id, out order );
    }

    /// <summary>
    /// 지정 주문의 보호 상태 설정
    /// </summary>
    /// <param name="id">보호할 주문 아이디</param>
    /// <param name="isProtected">보호 여부</param>
    public void SetOrderProtected (
        string id, bool isProtected )
    {
        if ( isProtected )
        {
            _protectedOrderIds.Add( id );
            return;
        }

        _protectedOrderIds.Remove( id );
    }

    /// <summary>
    /// 지정 주문의 보호 여부 확인
    /// </summary>
    /// <param name="id">확인할 주문 아이디</param>
    /// <returns>주문 보호 여부</returns>
    public bool IsOrderProtected ( string id )
    {
        return _protectedOrderIds.Contains( id );
    }

    /// <summary>
    /// 진행 상태별 주문 수 조회
    /// </summary>
    /// <param name="progress">진행 상태</param>
    /// <returns>상태에 맞는 주문 수</returns>
    public int GetOrderCount ( OrderProgressState progress )
    {
        int count = 0;

        //진행 상태가 같은 주문 수 계산
        foreach ( var order in _orders.Values )
        {
            if ( order.ProgressState == progress )
                count++;
        }

        return count;
    }

    /// <summary>
    /// 진행 상태별 주문 조회
    /// </summary>
    /// <param name="progress">진행 상태</param>
    /// <returns>상태에 맞는 주문 목록</returns>
    public IReadOnlyList<CustomerOrder> GetOrders ( OrderProgressState progress )
    {
        //주문 리스트 생성
        var orders = new List<CustomerOrder>( );

        //progress 상태인 주문만 리스트에 추가
        foreach ( var order in _orders.Values )
        {
            if ( order.ProgressState == progress )
                orders.Add( order );
        }

        return orders;
    }

    /// <summary>
    /// 진행 중 주문 조회
    /// </summary>
    /// <returns>제작 중, 배송 대기, 배송 중 주문 목록</returns>
    public IReadOnlyList<CustomerOrder> GetProgressOrders ()
    {
        var orders = new List<CustomerOrder>( );

        //아직 종료되지 않은 진행 주문 추가
        foreach ( CustomerOrder order in _orders.Values )
        {
            if ( order.ProgressState == OrderProgressState.Production ||
                order.ProgressState == OrderProgressState.Crafted ||
                order.ProgressState == OrderProgressState.Shipping )
                orders.Add( order );
        }

        //제작 중, 배송 대기, 배송 중 순서와 가까운 납품 기한으로 정렬
        orders.Sort( CompareProgressOrders );

        return orders;
    }

    /// <summary>
    /// 진행 주문 표시 순서 비교
    /// </summary>
    /// <param name="left">왼쪽 주문</param>
    /// <param name="right">오른쪽 주문</param>
    /// <returns>정렬 비교 결과</returns>
    int CompareProgressOrders ( CustomerOrder left, CustomerOrder right )
    {
        //제작 중, 배송 대기, 배송 중 순서로 표시
        int stateCompare = GetProgressStateOrder( left.ProgressState )
            .CompareTo( GetProgressStateOrder( right.ProgressState ) );
        if ( stateCompare != 0 ) return stateCompare;

        //같은 상태면 납품 기한이 가까운 주문 우선
        int dueCompare = left.DeliveryDue.CompareTo( right.DeliveryDue );
        if ( dueCompare != 0 ) return dueCompare;

        //같은 기한이면 먼저 생성된 주문 우선
        return left.CreatedNumber.CompareTo( right.CreatedNumber );
    }

    /// <summary>
    /// 진행 상태 표시 우선순위 반환
    /// </summary>
    /// <param name="progressState">주문 진행 상태</param>
    /// <returns>표시 우선순위</returns>
    int GetProgressStateOrder ( OrderProgressState progressState )
    {
        switch ( progressState )
        {
            case OrderProgressState.Production:
                return 0;

            case OrderProgressState.Crafted:
                return 1;

            case OrderProgressState.Shipping:
                return 2;

            default:
                return int.MaxValue;
        }
    }

    /// <summary>
    /// 최근 종료 주문 조회
    /// </summary>
    /// <param name="maxCount">최대 조회 개수</param>
    /// <returns>최근 종료 주문 목록</returns>
    public IReadOnlyList<CustomerOrder> GetRecentClosedOrders ( int maxCount )
    {
        //조회할 개수가 없으면 빈 목록 반환
        if ( maxCount <= 0 ) return Array.Empty<CustomerOrder>( );

        var orders = new List<CustomerOrder>( );

        //종료된 주문만 목록에 추가
        foreach ( var order in _orders.Values )
        {
            if ( order.ProgressState == OrderProgressState.Closed )
                orders.Add( order );
        }

        //최근 종료 주문부터 정렬
        orders.Sort( CompareClosedOrders );

        //최대 조회 개수만 반환
        if ( orders.Count > maxCount )
            orders.RemoveRange( maxCount, orders.Count - maxCount );

        return orders;
    }

    /// <summary>
    /// 종료 주문 최신순 비교
    /// </summary>
    /// <param name="left">왼쪽 주문</param>
    /// <param name="right">오른쪽 주문</param>
    /// <returns>정렬 비교 결과</returns>
    int CompareClosedOrders ( CustomerOrder left, CustomerOrder right )
    {
        //종료일이 다르면 최근 종료일 우선
        int dayCompare = right.ClosedTotalDay.CompareTo( left.ClosedTotalDay );
        if ( dayCompare != 0 ) return dayCompare;

        //같은 날이면 최근 생성 주문 우선
        return right.CreatedNumber.CompareTo( left.CreatedNumber );
    }

    #endregion

    #region ----- 주문 처리 -----

    /// <summary>
    /// 수락 대기 제한 설정
    /// </summary>
    /// <param name="limit">대기 제한</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetWaitingLimit ( int limit )
    {
        //허용 범위를 벗어나면 종료
        if ( limit < 1 || limit > MaxWaitingLimit ) return false;

        //같은 값은 추가 변경 없이 성공 처리
        if ( _waitingLimit == limit ) return true;

        _waitingLimit = limit;

        //주문 목록과 수량 표시 갱신
        NotifyChanged( );
        return true;
    }

    /// <summary>
    /// 수락 대기 주문 추가
    /// </summary>
    /// <param name="order">추가할 주문</param>
    /// <returns>주문 추가 결과</returns>
    public OrderResult AddOrder ( CustomerOrder order )
    {
        //이미 등록된 주문 차단
        if ( _orders.ContainsKey( order.OrderId ) )
            return OrderResult.Duplicate;

        //수락 대기 상태가 아닌 주문 차단
        if ( order.ProgressState != OrderProgressState.Waiting )
            return OrderResult.InvalidState;

        //수락 대기 목록 만석
        if ( WaitingCount >= _waitingLimit )
            return OrderResult.WaitingFull;

        //주문 딕셔너리에 추가
        _orders.Add( order.OrderId, order );

        //등록된 주문 다음 번호로 갱신
        _nextCreatedNumber = Math.Max(
            _nextCreatedNumber,
            order.CreatedNumber + 1 );

        NotifyChanged( );
        //추가 성공
        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 수락
    /// </summary>
    /// <param name="id">주문 아이디</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <returns>주문 수락 결과</returns>
    public OrderResult AcceptOrder ( string id, int totalDay )
    {
        //주문 조회
        if ( GetOrder( id, out var order ) == false )
            //데이터가 없다면
            return OrderResult.NotFound;

        //수락 대기 상태 확인
        if ( order.ProgressState != OrderProgressState.Waiting )
            //대기 중인 상태가 아니면
            return OrderResult.InvalidState;

        //수락 기한 확인
        if ( totalDay > order.AcceptDue )
            //수락 기한이 지났으면
            return OrderResult.Expired;

        //진행 상태 변경
        order.SetProgress( OrderProgressState.Production );

        NotifyChanged( );
        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 거절
    /// </summary>
    /// <param name="id">주문 아이디</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <returns>주문 거절 결과</returns>
    public OrderResult RejectOrder ( string id, int totalDay )
    {
        //주문 조회
        if ( GetOrder( id, out var order ) == false )
            //데이터가 없다면
            return OrderResult.NotFound;

        //튜토리얼 진행에 필요한 보호 주문은 거절 차단
        if ( IsOrderProtected( id ) )
            return OrderResult.InvalidState;

        //수락 대기 상태 확인
        if ( order.ProgressState != OrderProgressState.Waiting )
            //대기 중인 상태가 아니면
            return OrderResult.InvalidState;

        //수락 기한 확인
        if ( totalDay > order.AcceptDue )
            //수락 기한이 지났으면
            return OrderResult.Expired;

        //주문 종료
        order.Close( OrderOutcome.Rejected, totalDay );

        //직접 거절 결과 전달
        OnOrderClosed?.Invoke( id, OrderOutcome.Rejected );

        NotifyChanged( );
        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 제작 완료
    /// </summary>
    /// <param name="id">주문 아이디</param>
    /// <returns>제작 완료 처리 결과</returns>
    public OrderResult CompleteOrder ( string id )
    {
        //주문 조회
        if ( GetOrder( id, out var order ) == false )
            //데이터가 없으면
            return OrderResult.NotFound;

        //제작 상태 확인
        if ( order.ProgressState != OrderProgressState.Production )
            //제작 중이 아니면
            return OrderResult.InvalidState;

        //제작 완료 상태로 변경
        order.SetProgress( OrderProgressState.Crafted );

        NotifyChanged( );
        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 배송 출발 가능 여부 확인
    /// </summary>
    /// <param name="id">배송할 주문 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="order">배송할 주문</param>
    /// <returns>배송 출발 검사 결과</returns>
    public OrderResult CheckDeliveryStart (
        string id, int totalDay, out CustomerOrder order )
    {
        //주문 조회
        if ( GetOrder( id, out order ) == false )
            return OrderResult.NotFound;

        //배송 대기 상태 확인
        if ( order.ProgressState != OrderProgressState.Crafted )
            return OrderResult.InvalidState;

        //이미 최종 납품 기한이 지났으면 출발 불가
        if ( totalDay > order.FinalDeadline )
            return OrderResult.Expired;

        return OrderResult.Success;
    }

    /// <summary>
    /// 도착일 기준 배송 결과 확인
    /// </summary>
    /// <param name="order">배송 주문</param>
    /// <param name="arrivalTotalDay">예상 또는 실제 도착 누적 영업일</param>
    /// <param name="outcome">정상 또는 지연 배송 결과</param>
    /// <returns>배송 결과 검사 결과</returns>
    public OrderResult CheckDeliveryOutcome (
        CustomerOrder order, int arrivalTotalDay,
        out OrderOutcome outcome )
    {
        outcome = OrderOutcome.None;

        if ( order == null )
            return OrderResult.InvalidOrder;

        //배송 대기 또는 배송 중 주문만 확인
        if ( order.ProgressState != OrderProgressState.Crafted &&
            order.ProgressState != OrderProgressState.Shipping )
            return OrderResult.InvalidState;

        //최종 납품 기한 이후 도착 차단
        if ( arrivalTotalDay > order.FinalDeadline )
            return OrderResult.Expired;

        //도착일 기준 정상 또는 지연 배송 결정
        outcome = arrivalTotalDay <= order.DeliveryDue
            ? OrderOutcome.NormalDelivery
            : OrderOutcome.LateDelivery;

        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 배송 출발
    /// </summary>
    /// <param name="id">배송할 주문 아이디</param>
    /// <returns>배송 출발 처리 결과</returns>
    public OrderResult StartDelivery ( string id )
    {
        //주문 조회
        if ( GetOrder( id, out CustomerOrder order ) == false )
            return OrderResult.NotFound;

        //배송 대기 상태 확인
        if ( order.ProgressState != OrderProgressState.Crafted )
            return OrderResult.InvalidState;

        //배송 중 상태로 변경
        order.SetProgress( OrderProgressState.Shipping );

        NotifyChanged( );
        return OrderResult.Success;
    }

    /// <summary>
    /// 도착한 배송 주문 완료
    /// </summary>
    /// <param name="id">배송 주문 아이디</param>
    /// <param name="arrivalTotalDay">실제 도착 누적 영업일</param>
    /// <returns>배송 완료 처리 결과</returns>
    public OrderResult CompleteDelivery (
        string id, int arrivalTotalDay )
    {
        //주문 조회
        if ( GetOrder( id, out CustomerOrder order ) == false )
            return OrderResult.NotFound;

        //배송 중인 주문인지 확인
        if ( order.ProgressState != OrderProgressState.Shipping )
            return OrderResult.InvalidState;

        //도착일 기준 배송 결과 확인
        OrderResult result = CheckDeliveryOutcome(
            order, arrivalTotalDay, out OrderOutcome outcome );

        if ( result != OrderResult.Success )
            return result;

        //배송 결과로 주문 종료
        order.Close( outcome, arrivalTotalDay );

        NotifyChanged( );
        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 취소
    /// </summary>
    /// <param name="id">주문 아이디</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <returns>주문 취소 결과</returns>
    public OrderResult CancelOrder ( string id, int totalDay )
    {
        //주문 조회
        if ( GetOrder( id, out var order ) == false )
            return OrderResult.NotFound;

        //튜토리얼 진행에 필요한 보호 주문은 취소 차단
        if ( IsOrderProtected( id ) )
            return OrderResult.InvalidState;

        //취소할 수 있는 진행 상태 확인
        if ( order.ProgressState != OrderProgressState.Production &&
            order.ProgressState != OrderProgressState.Crafted )
            //제작 중이 아니거나 제작 완료가 아니면
            return OrderResult.InvalidState;

        //주문 취소 종료
        order.Close( OrderOutcome.Cancelled, totalDay );

        //주문 취소 결과 전달
        OnOrderClosed?.Invoke( id, OrderOutcome.Cancelled );

        NotifyChanged( );
        return OrderResult.Success;
    }

    /// <summary>
    /// 주문 변경 이벤트 발행
    /// </summary>
    void NotifyChanged ()
    {
        OnOrderChanged?.Invoke( );
    }
    #endregion

    #region ----- 주문량 페널티 -----

    /// <summary>
    /// 주문량 페널티 추가
    /// </summary>
    /// <param name="type">페널티 원인</param>
    /// <param name="reduction">일일 주문 감소량</param>
    /// <param name="days">적용 일수</param>
    /// <returns>추가 성공 여부</returns>
    public bool AddPenalty ( OrderPenaltyType type, int reduction, int days )
    {
        //적용할 수 없는 페널티 차단
        if ( type == OrderPenaltyType.None || reduction <= 0 || days <= 0 )
            return false;

        //기존 페널티와 원인이 다르면 복합 원인으로 변경
        if ( IsPenaltyActive && _penaltyType != type )
            _penaltyType = OrderPenaltyType.Mixed;
        else if ( IsPenaltyActive == false )
            _penaltyType = type;

        //감소량과 적용 일수 누적
        _orderReduction = Math.Max( _orderReduction, reduction );

        //페널티 합산 최대 14일
        _penaltyRemainingDays = Math.Min( MaxPenaltyDays, _penaltyRemainingDays + days );

        OnPenaltyChanged?.Invoke( );
        return true;
    }

    /// <summary>
    /// 주문량 페널티 하루 경과
    /// </summary>
    /// <returns>페널티 변경 여부</returns>
    public bool PassPenaltyDay ()
    {
        //적용 중인 페널티가 없으면 종료
        if ( IsPenaltyActive == false ) return false;

        _penaltyRemainingDays--;

        //남은 기간이 끝나면 페널티 초기화
        if ( _penaltyRemainingDays == 0 )
        {
            _penaltyType = OrderPenaltyType.None;
            _orderReduction = 0;
        }

        OnPenaltyChanged?.Invoke( );
        return true;
    }

    #endregion

    #region ----- 저장 복구 -----
    /// <summary>
    /// 검증된 복구 상태로 고객 주문 상태 복구
    /// </summary>
    /// <param name="restoreState">고객 주문 복구 상태</param>
    public void Restore ( CustomerOrderRestoreState restoreState )
    {
        //주문 아이디 기준 복구 딕셔너리 생성
        var restoredOrders =
            new Dictionary<string, CustomerOrder>( );

        foreach ( CustomerOrder order in restoreState.Orders )
        {
            restoredOrders.Add( order.OrderId, order );
        }

        //검증을 마친 주문 상태 적용
        _orders = restoredOrders;
        _waitingLimit = restoreState.WaitingLimit;
        _nextCreatedNumber = restoreState.NextCreatedNumber;

        //검증을 마친 주문량 페널티 상태 적용
        _penaltyType = restoreState.PenaltyType;
        _orderReduction = restoreState.OrderReduction;
        _penaltyRemainingDays = restoreState.PenaltyRemainingDays;

        //주문과 페널티 표시 갱신
        OnOrderChanged?.Invoke( );
        OnPenaltyChanged?.Invoke( );
    }
    #endregion

    #region ----- 영업 종료 -----

    /// <summary>
    /// 영업 종료 처리
    /// </summary>
    /// <param name="totalDay">누적 영업일</param>
    /// <returns>변경된 주문 존재 여부</returns>
    public bool ProcessEndOfDay ( int totalDay )
    {
        bool isChanged = false;

        //전체 주문 기한 확인
        foreach ( var order in _orders.Values )
        {
            //튜토리얼 진행에 필요한 주문은 자동 거절과 실패 처리 제외
            if ( IsOrderProtected( order.OrderId ) )
                continue;

            ///수락 기한이 끝난 대기 주문 자동 거절
            if ( order.ProgressState == OrderProgressState.Waiting &&
                order.AcceptDue <= totalDay )
            {
                order.Close( OrderOutcome.AutoRejected, totalDay );

                //자동 거절 결과 전달
                OnOrderClosed?.Invoke( order.OrderId, OrderOutcome.AutoRejected );

                isChanged = true;
                continue;
            }

            //최종 기한이 끝난 진행 주문 실패
            bool isActive =
                order.ProgressState == OrderProgressState.Production ||
                order.ProgressState == OrderProgressState.Crafted ||
                order.ProgressState == OrderProgressState.Shipping;

            if ( isActive && order.FinalDeadline <= totalDay )
            {
                order.Close( OrderOutcome.Failed, totalDay );

                //최종 기한 초과 결과 전달
                OnOrderClosed?.Invoke( order.OrderId, OrderOutcome.Failed );

                isChanged = true;
            }
        }

        //변경된 주문이 없으면 알리지 않음
        if ( isChanged == false )
            return false;

        NotifyChanged( );
        return true;
    }

    #endregion
}
