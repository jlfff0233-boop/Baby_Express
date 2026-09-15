using System;
using System.Collections.Generic;

/// <summary>
/// 플레이 상태 모델 - 날짜, 시간, 진행률, 현재 자금 관리
/// </summary>
public class PlayStateModel
{
    const int DaysPerYear = 364;       //1년 영업일 수

    static readonly int [ ] DaysPerMonth =
    {
        31, 30, 30,
        31, 30, 30,
        31, 30, 30,
        31, 30, 30
    };

    int _month;       //현재 표시 월
    int _day;       //현재 표시 일
    float _time;       //현재 시간
    int _totalDay;       //누적 영업일

    int _highGradeEvaluationCount;       //B등급 이상 평가 누적 수
    float _budget;                 //현재 자금
    HashSet<string> _unlockedIds = new HashSet<string>( );       //해금된 상품 아이디
    HashSet<string> _newUnlockedIds = new HashSet<string>( );       //아직 확인하지 않은 해금 상품 아이디

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 현재 월
    /// </summary>
    public int Month => _month;

    /// <summary>
    /// 현재 일
    /// </summary>
    public int Day => _day;

    /// <summary>
    /// 현재 시간
    /// </summary>
    public float Time => _time;

    /// <summary>
    /// 누적 영업일
    /// </summary>
    public int TotalDay => _totalDay;

    /// <summary>
    /// 현재 연도
    /// </summary>
    public int Year => (_totalDay - 1) / DaysPerYear + 1;

    /// <summary>
    /// 연간 주차 수
    /// </summary>
    public int WeeksPerYear => DaysPerYear / 7;

    /// <summary>
    /// B등급 이상 평가 누적 수
    /// </summary>
    public int HighGradeEvaluationCount => _highGradeEvaluationCount;

    /// <summary>
    /// 현재 자금
    /// </summary>
    public float Budget => _budget;

    /// <summary>
    /// 해금된 상품 아이디 목록
    /// </summary>
    public IReadOnlyCollection<string> UnlockedIds => _unlockedIds;

    /// <summary>
    /// 아직 확인하지 않은 해금 상품 아이디 목록
    /// </summary>
    public IReadOnlyCollection<string> NewUnlockedIds => _newUnlockedIds;
    #endregion

    #region ----- 이벤트 -----
    /// <summary>
    /// 날짜 변경 이벤트(월, 일)
    /// </summary>
    public event Action<int, int> OnDateChanged;

    /// <summary>
    /// 시간 변경 이벤트(시간)
    /// </summary>
    public event Action<float> OnTimeChanged;

    /// <summary>
    /// 자금 변경 이벤트(자금)
    /// </summary>
    public event Action<float> OnBudgetChanged;

    /// <summary>
    /// 상품 해금 상태 변경 이벤트
    /// </summary>
    public event Action<string, bool> OnItemUnlockChanged;

    #endregion

    #region ----- 초기화 -----

    /// <summary>
    /// 0 이상인 유효한 값인지 확인
    /// </summary>
    /// <param name="value">확인할 값</param>
    /// <returns>유효 여부</returns>
    bool IsValidValue ( float value )
    {
        //숫자로 표현할 수 있고 무한대가 아니면서 0보다 크나면 true
        return float.IsNaN( value ) == false &&
            float.IsInfinity( value ) == false &&
            value >= 0f;
    }

    /// <summary>
    /// 플레이 상태 모델 생성
    /// </summary>
    /// <param name="time">시간</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <param name="highGradeEvaluationCount">B등급 이상 평가 누적 수</param>
    /// <param name="budget">현재 자금</param>
    public PlayStateModel ( float time, int totalDay,
        int highGradeEvaluationCount, float budget )
    {
        if ( IsValidValue( time ) == false )
            throw new ArgumentOutOfRangeException( nameof( time ) );
        if ( totalDay <= 0 )
            throw new ArgumentOutOfRangeException( nameof( totalDay ) );
        if ( highGradeEvaluationCount < 0 )
            throw new ArgumentOutOfRangeException(
                nameof( highGradeEvaluationCount ) );
        if ( IsValidValue( budget ) == false )
            throw new ArgumentOutOfRangeException( nameof( budget ) );

        _time = time;
        _totalDay = totalDay;
        _highGradeEvaluationCount = highGradeEvaluationCount;
        _budget = budget;

        //누적 영업일로 표시 날짜 계산
        UpdateDate( );
    }

    #endregion

    #region ----- 날짜와 시간 -----

    /// <summary>
    /// 누적 영업일을 월과 일로 변환
    /// </summary>
    /// <param name="totalDay">변환할 누적 영업일</param>
    /// <param name="month">변환된 월</param>
    /// <param name="day">변환된 일</param>
    /// <returns>변환 성공 여부</returns>
    public bool GetDate ( int totalDay, out int month, out int day )
    {
        month = 0;
        day = 0;

        //잘못된 누적 영업일 차단
        if ( totalDay <= 0 ) return false;

        //현재 연도의 경과 일수 계산
        int dayOfYear = (totalDay - 1) % DaysPerYear;
        month = 1;

        //경과 일수가 포함된 월 조회
        for ( int i = 0; i < DaysPerMonth.Length; i++ )
        {
            if ( dayOfYear < DaysPerMonth [ i ] )
            {
                day = dayOfYear + 1;
                return true;
            }

            dayOfYear -= DaysPerMonth [ i ];
            month++;
        }

        return false;
    }

    /// <summary>
    /// 누적 영업일이 속한 연도 반환
    /// </summary>
    /// <param name="totalDay">변환할 누적 영업일</param>
    /// <returns>표시 연도, 잘못된 영업일이면 0</returns>
    public int GetYear ( int totalDay )
    {
        if ( totalDay <= 0 ) return 0;

        return ( totalDay - 1 ) / DaysPerYear + 1;
    }

    /// <summary>
    /// 누적 영업일이 속한 연도의 주차 반환
    /// </summary>
    /// <param name="totalDay">변환할 누적 영업일</param>
    /// <returns>연도 내 주차, 잘못된 영업일이면 0</returns>
    public int GetWeekOfYear ( int totalDay )
    {
        if ( totalDay <= 0 ) return 0;

        int dayOfYear = ( totalDay - 1 ) % DaysPerYear;
        return dayOfYear / 7 + 1;
    }

    /// <summary>
    /// 현재 표시 날짜 갱신
    /// </summary>
    void UpdateDate ()
    {
        GetDate( _totalDay, out _month, out _day );
    }

    /// <summary>
    /// 시간 설정
    /// </summary>
    /// <param name="time">시간</param>
    /// <returns>시간 설정 성공 여부</returns>
    public bool SetTime ( float time )
    {
        //잘못된 시간 차단
        if ( IsValidValue( time ) == false ) return false;

        _time = time;

        OnTimeChanged?.Invoke( _time );
        return true;
    }

    /// <summary>
    /// 누적 영업일 설정
    /// </summary>
    /// <param name="totalDay">누적 영업일</param>
    /// <returns>누적 영업일 설정 성공 여부</returns>
    public bool SetTotalDay ( int totalDay )
    {
        //잘못된 값 차단
        if ( totalDay <= 0 ) return false;

        //totalDay가 현재 값과 같으면 변경 없이 진행
        if ( _totalDay == totalDay ) return true;

        //누적 영업일과 표시 날짜 갱신
        _totalDay = totalDay;
        UpdateDate( );

        //기존 날짜 이벤트 유지
        OnDateChanged?.Invoke( _month, _day );
        return true;
    }

    #endregion

    #region ----- 평가와 해금 -----

    /// <summary>
    /// B등급 이상 평가 누적 수 설정
    /// </summary>
    /// <param name="count">설정할 누적 수</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetHighGradeEvaluationCount ( int count )
    {
        if ( count < 0 ) return false;

        _highGradeEvaluationCount = count;
        return true;
    }

    /// <summary>
    /// B등급 이상 평가 누적 수 증가 가능 여부 확인
    /// </summary>
    /// <returns>증가 가능 여부</returns>
    public bool CanAddHighGradeEvaluation ( )
    {
        return _highGradeEvaluationCount < int.MaxValue;
    }

    /// <summary>
    /// B등급 이상 평가 누적 수 증가
    /// </summary>
    /// <returns>증가 성공 여부</returns>
    public bool AddHighGradeEvaluation ( )
    {
        if ( CanAddHighGradeEvaluation( ) == false ) return false;

        _highGradeEvaluationCount++;
        return true;
    }

    /// <summary>
    /// 상품 해금 여부 확인
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <returns>해금 여부</returns>
    public bool IsUnlocked ( string id )
    {
        return string.IsNullOrEmpty( id ) == false && _unlockedIds.Contains( id );
    }

    /// <summary>
    /// 상품 해금 상태 설정
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="isUnlocked">해금 여부</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetUnlocked ( string id, bool isUnlocked )
    {
        //빈 아이디 차단
        if ( string.IsNullOrEmpty( id ) ) return false;

        //해금 상태 변경
        bool isChanged = isUnlocked
            ? _unlockedIds.Add( id )
            : _unlockedIds.Remove( id );

        //기존 상태와 같으면 변경 이벤트를 발행하지 않음
        if ( isChanged == false ) return true;

        //새 해금은 확인 대기 목록에 추가하고 해제된 상품은 목록에서도 제거
        if ( isUnlocked == true )
            _newUnlockedIds.Add( id );
        else
            _newUnlockedIds.Remove( id );

        OnItemUnlockChanged?.Invoke( id, isUnlocked );
        return true;
    }

    /// <summary>
    /// 지정 상품의 새 해금 확인 처리
    /// </summary>
    /// <param name="id">확인한 상품 아이디</param>
    public void MarkUnlockViewed ( string id )
    {
        _newUnlockedIds.Remove( id );
    }

    /// <summary>
    /// 모든 새 해금 상품 확인 처리
    /// </summary>
    public void MarkAllUnlocksViewed ()
    {
        _newUnlockedIds.Clear( );
    }

    #endregion

    #region ----- 자금 -----

    /// <summary>
    /// 자금 지출 가능 여부 확인
    /// </summary>
    /// <param name="amount">지출량</param>
    /// <returns>자금 지출 가능 여부</returns>
    public bool CanSpendBudget ( float amount )
    {
        //잘못된 금액 차단
        if ( IsValidValue( amount ) == false ) return false;

        return _budget >= amount;
    }

    /// <summary>
    /// 자금 지출
    /// </summary>
    /// <param name="amount">지출량</param>
    /// <returns>자금 지출 성공 여부</returns>
    public bool SpendBudget ( float amount )
    {
        //0 이하의 지출과 자금 부족 차단
        if ( amount <= 0f || CanSpendBudget( amount ) == false )
            return false;

        _budget -= amount;

        OnBudgetChanged?.Invoke( _budget );
        return true;
    }

    /// <summary>
    /// 자금 추가 가능 여부 확인
    /// </summary>
    /// <param name="amount">추가량</param>
    /// <returns>추가 가능 여부</returns>
    public bool CanAddBudget ( float amount )
    {
        if ( IsValidValue( amount ) == false ) return false;

        return IsValidValue( _budget + amount );
    }

    /// <summary>
    /// 자금 설정
    /// </summary>
    /// <param name="budget">설정할 자금</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetBudget ( float budget )
    {
        if ( IsValidValue( budget ) == false ) return false;
        if ( _budget == budget ) return true;

        _budget = budget;
        OnBudgetChanged?.Invoke( _budget );
        return true;
    }

    /// <summary>
    /// 자금 추가
    /// </summary>
    /// <param name="amount">추가량</param>
    /// <returns>자금 추가 성공 여부</returns>
    public bool AddBudget ( float amount )
    {
        //잘못된 추가 금액 차단
        if ( amount <= 0f || CanAddBudget( amount ) == false )
            return false;

        return SetBudget( _budget + amount );
    }

    #endregion

    #region ----- 데이터 복구 -----

    /// <summary>
    /// 저장된 공용 플레이 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 저장 데이터</param>
    public void Restore ( SaveData saveData )
    {
        _time = saveData.Time;
        _totalDay = saveData.TotalDay;
        _highGradeEvaluationCount =
            saveData.HighGradeEvaluationCount;
        _budget = saveData.Budget;

        _unlockedIds.Clear( );
        _newUnlockedIds.Clear( );

        for ( int i = 0; i < saveData.UnlockedIds.Count; i++ )
            _unlockedIds.Add( saveData.UnlockedIds [ i ] );

        for ( int i = 0; i < saveData.NewUnlockedIds.Count; i++ )
            _newUnlockedIds.Add( saveData.NewUnlockedIds [ i ] );

        //누적 영업일을 기준으로 표시 날짜 복구
        UpdateDate( );
    }

    #endregion
}
