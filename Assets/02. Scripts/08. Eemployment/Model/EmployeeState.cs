/// <summary>
/// 직원별 고용과 배치 상태
/// </summary>
public class EmployeeState
{
    EmployeeData _data;       //직원 설정 데이터
    bool _isHired;       //고용 상태
    bool _isAssigned;       //배송 배치 상태

    int _unpaidDayCount;       //주급을 지급하지 않은 고용 일수
    int _lastWorkedTotalDay;       //최근 근무 기록(누적 영업일 기준)

    /// <summary>
    /// 직원 설정 데이터
    /// </summary>
    public EmployeeData Data => _data;

    /// <summary>
    /// 고용 여부
    /// </summary>
    public bool IsHired => _isHired;

    /// <summary>
    /// 주급을 지급하지 않은 고용 일수
    /// </summary>
    public int UnpaidWorkedDayCount => _unpaidDayCount;

    /// <summary>
    /// 배송 배치 여부
    /// </summary>
    public bool IsAssigned => _isAssigned;

    /// <summary>
    /// 현재 배치 가능 여부
    /// </summary>
    public bool IsAvailable => _isHired && _isAssigned == false;

    /// <summary>
    /// 현재 직원의 전체 주급
    /// </summary>
    public float TotalWeeklyWage => _isHired ? _data.WeeklyWage : 0f;

    /// <summary>
    /// 현재 직원의 전체 배송 슬롯 증가량
    /// </summary>
    public int TotalDeliverySlotIncrease => _isHired ? _data.DeliverySlotIncrease : 0;

    /// <summary>
    /// 최근 근무 기록(누적 영업일 기준)
    /// </summary>
    public int LastWorkedTotalDay => _lastWorkedTotalDay;

    /// <summary>
    /// 직원 런타임 상태 생성
    /// </summary>
    /// <param name="data">직원 설정 데이터</param>
    public EmployeeState ( EmployeeData data )
    {
        _data = data;
    }

    /// <summary>
    /// 직원 고용
    /// </summary>
    /// <returns>고용 성공 여부</returns>
    public bool Hire ()
    {
        if ( _isHired ) return false;

        _isHired = true;
        return true;
    }

    /// <summary>
    /// 배치되지 않은 직원 해고
    /// </summary>
    /// <returns>해고 성공 여부</returns>
    public bool Fire ()
    {
        if ( _isHired == false || _isAssigned )
            return false;

        _isHired = false;
        return true;
    }

    /// <summary>
    /// 배송 직원 배치
    /// </summary>
    /// <returns>배치 성공 여부</returns>
    public bool Assign ()
    {
        if ( IsAvailable == false ) return false;

        _isAssigned = true;
        return true;
    }

    /// <summary>
    /// 배송 직원 배치 해제
    /// </summary>
    /// <returns>배치 해제 성공 여부</returns>
    public bool Release ()
    {
        if ( _isAssigned == false ) return false;

        _isAssigned = false;
        return true;
    }

    /// <summary>
    /// 현재 영업일을 고용 일수로 기록
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <returns>새 고용 일수 기록 여부</returns>
    public bool RecordWorkedDay ( int totalDay )
    {
        if ( totalDay <= 0 || _lastWorkedTotalDay == totalDay )
            return false;

        _lastWorkedTotalDay = totalDay;
        _unpaidDayCount++;
        return true;
    }

    /// <summary>
    /// 주급 지급 완료 후 미지급 고용 일수 초기화
    /// </summary>
    public void ClearUnpaidWorkedDays ()
    {
        _unpaidDayCount = 0;
    }

    /// <summary>
    /// 저장된 직원 상태 복구
    /// </summary>
    /// <param name="isHired">고용 여부</param>
    /// <param name="isAssigned">배송 배치 여부</param>
    /// <param name="unpaidWorkedDayCount">미지급 근무일 수</param>
    /// <param name="lastWorkedTotalDay">마지막 근무 기록 영업일</param>
    internal void Restore (
        bool isHired, bool isAssigned,
        int unpaidWorkedDayCount,
        int lastWorkedTotalDay )
    {
        _isHired = isHired;
        _isAssigned = isAssigned;
        _unpaidDayCount = unpaidWorkedDayCount;
        _lastWorkedTotalDay = lastWorkedTotalDay;
    }
}
