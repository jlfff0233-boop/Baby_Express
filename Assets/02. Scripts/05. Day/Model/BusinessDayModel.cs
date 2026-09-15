using System;

/// <summary>
/// 영업 종료 사유
/// </summary>
public enum DayEndReason
{
    None,       //영업 종료 대기 없음
    CraftLimitReached,       //일일 제작 할당량 달성
    NoAvailableOrders,       //처리할 주문 소진
}

/// <summary>
/// 영업 모델 - 일일 제작 할당량과 영업 종료 상태 관리
/// </summary>
public class BusinessDayModel
{
    const float BusinessStartTime = 9f;       //영업 시작 시각
    const float BusinessDuration = 12f;       //하루 영업 시간

    int _craftCount;       //오늘 제작 완료 수
    int _craftLimit;       //일일 제작 할당량
    bool _isEndStarted;       //결산 시작 여부
    DayEndReason _endReason;       //영업 종료 사유

    int _quickRestockCount;       //오늘 빠른 재입고 횟수
    int _quickRestockLimit;       //일일 빠른 재입고 최대 횟수

    int _baseCraftLimit;       //기본 제작 할당량
    int _nextCraftLimit;       //다음 영업일부터 적용할 제작 할당량
    EmployeeModel _employeeModel;       //직원 효과 모델

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 오늘 제작 완료 수
    /// </summary>
    public int CraftCount => _craftCount;

    /// <summary>
    /// 현재 직원 제작 할당량 보너스
    /// </summary>
    public int EmployeeCraftBonus => Math.Max( 0, ( int ) Math.Round(
        _employeeModel.GetEffectValue(
            EmployeeEffectType.CraftQuotaBonus ),
        MidpointRounding.AwayFromZero ) );

    /// <summary>
    /// 일일 제작 할당량
    /// </summary>
    public int CraftLimit => _craftLimit + EmployeeCraftBonus;

    /// <summary>
    /// 결산 시작 여부
    /// </summary>
    public bool IsEndStarted => _isEndStarted;

    /// <summary>
    /// 영업 종료 사유
    /// </summary>
    public DayEndReason EndReason => _endReason;

    /// <summary>
    /// 영업 종료 대기 여부
    /// </summary>
    public bool IsEndPending => _endReason != DayEndReason.None;

    /// <summary>
    /// 추가 제작 가능 여부
    /// </summary>
    public bool CanCraft => IsEndPending == false && _craftCount < CraftLimit;

    /// <summary>
    /// 오늘 제작 진행률
    /// </summary>
    public float CraftProgressRate => ( float ) _craftCount / CraftLimit;

    /// <summary>
    /// 현재 제작 진행률에 따른 영업 시각
    /// </summary>
    public float BusinessTime =>
        BusinessStartTime +
        BusinessDuration * CraftProgressRate;

    /// <summary>
    /// 오늘 빠른 재입고 횟수
    /// </summary>
    public int QuickRestockCount => _quickRestockCount;

    /// <summary>
    /// 일일 빠른 재입고 최대 횟수
    /// </summary>
    public int QuickRestockLimit => _quickRestockLimit;

    /// <summary>
    /// 빠른 재입고 가능 여부
    /// </summary>
    public bool CanQuickRestock => _quickRestockCount < _quickRestockLimit;

    /// <summary>
    /// 기본 일일 제작 할당량
    /// </summary>
    public int BaseCraftLimit => _baseCraftLimit;

    /// <summary>
    /// 다음 영업일부터 적용할 제작 할당량
    /// </summary>
    public int NextCraftLimit =>
        _nextCraftLimit + EmployeeCraftBonus;

    #endregion

    /// <summary>
    /// 제작 진행률 변경 이벤트(제작 완료 수, 제작 할당량)
    /// </summary>
    public event Action<int, int> OnCraftProgressChanged;

    /// <summary>
    /// 결산 요청 이벤트
    /// </summary>
    public event Action<DayEndReason> OnDayFinished;

    /// <summary>
    /// 영업일 모델 생성
    /// </summary>
    /// <param name="craftCount">오늘 제작 완료 수</param>
    /// <param name="craftLimit">일일 제작 할당량</param>
    /// <param name="quickRestockCount">오늘 빠른 재입고 횟수</param>
    /// <param name="quickRestockLimit">일일 빠른 재입고 최대 횟수</param>
    /// <param name="employeeModel">직원 효과 모델</param>
    public BusinessDayModel ( int craftCount, int craftLimit,
        int quickRestockCount, int quickRestockLimit,
        EmployeeModel employeeModel )
    {
        if ( craftLimit <= 0 )
            throw new ArgumentOutOfRangeException( nameof( craftLimit ) );
        if ( craftCount < 0 || craftCount > craftLimit )
            throw new ArgumentOutOfRangeException( nameof( craftCount ) );
        if ( quickRestockLimit <= 0 )
            throw new ArgumentOutOfRangeException( nameof( quickRestockLimit ) );
        if ( quickRestockCount < 0 || quickRestockCount > quickRestockLimit )
            throw new ArgumentOutOfRangeException( nameof( quickRestockCount ) );

        _craftCount = craftCount;
        _craftLimit = craftLimit;

        _baseCraftLimit = craftLimit;
        _nextCraftLimit = craftLimit;

        _quickRestockCount = quickRestockCount;
        _quickRestockLimit = quickRestockLimit;
        _employeeModel = employeeModel;

        _employeeModel.OnEmployeeChanged += RefreshCraftLimit;
        
        _endReason = craftCount >= CraftLimit
            ? DayEndReason.CraftLimitReached
            : DayEndReason.None;
        _isEndStarted = false;
    }

    #region ----- 제작 할당량 -----

    /// <summary>
    /// 제작 완료 수 추가
    /// </summary>
    public void AddCraftCompletion ()
    {
        //제작 불가 상태면 변경하지 않음
        if ( CanCraft == false ) return;

        _craftCount++;
        OnCraftProgressChanged?.Invoke( _craftCount, CraftLimit );

        //할당량을 모두 사용하면 영업 종료 요청
        if ( _craftCount >= CraftLimit )
            RequestEnd( DayEndReason.CraftLimitReached );
    }

    /// <summary>
    /// 현재 영업일 제작 할당량 설정
    /// </summary>
    /// <param name="craftLimit">현재 영업일 제작 할당량</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetCurrentCraftLimit ( int craftLimit )
    {
        if ( craftLimit <= 0 ||
            craftLimit < _craftCount ||
            _isEndStarted )
        {
            return false;
        }

        _craftLimit = craftLimit;
        _endReason = _craftCount >= CraftLimit
            ? DayEndReason.CraftLimitReached
            : DayEndReason.None;

        OnCraftProgressChanged?.Invoke(
            _craftCount, CraftLimit );
        return true;
    }

    #endregion

    #region ----- 빠른 재입고 -----

    /// <summary>
    /// 오늘 빠른 재입고 횟수 추가
    /// </summary>
    public void AddQuickRestock ()
    {
        //일일 최대 횟수에 도달했으면 변경하지 않음
        if ( CanQuickRestock == false ) return;

        _quickRestockCount++;
    }

    #endregion

    #region ----- 영업 종료 -----

    /// <summary>
    /// 영업 종료 대기 설정
    /// </summary>
    /// <param name="reason">영업 종료 사유</param>
    public void RequestEnd ( DayEndReason reason )
    {
        //잘못된 사유와 중복 요청 차단
        if ( reason == DayEndReason.None || IsEndPending ) return;

        //결산 전까지 영업 종료 사유만 저장
        _endReason = reason;
    }

    /// <summary>
    /// 대기 중인 영업 종료의 결산 시작
    /// </summary>
    /// <returns>결산 시작 여부</returns>
    public bool StartEnd ()
    {
        //종료 대기 상태가 아니거나 이미 시작했으면 차단
        if ( IsEndPending == false || _isEndStarted ) return false;

        _isEndStarted = true;
        OnDayFinished?.Invoke( _endReason );
        return true;
    }

    /// <summary>
    /// 다음 영업일부터 적용할 제작 할당량 설정
    /// </summary>
    /// <param name="craftLimit">최종 제작 할당량</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetNextCraftLimit ( int craftLimit )
    {
        if ( craftLimit <= 0 ) return false;

        //같은 값은 추가 변경 없이 성공 처리
        if ( _nextCraftLimit == craftLimit ) return true;

        _nextCraftLimit = craftLimit;
        return true;
    }

    /// <summary>
    /// 다음 영업일 상태 초기화
    /// </summary>
    /// <returns>초기화 성공 여부</returns>
    public bool ResetDay ()
    {
        if ( _nextCraftLimit <= 0 ) return false;

        _craftCount = 0;
        _craftLimit = _nextCraftLimit;
        _quickRestockCount = 0;

        _endReason = DayEndReason.None;
        _isEndStarted = false;

        OnCraftProgressChanged?.Invoke( _craftCount, CraftLimit );
        return true;
    }

    /// <summary>
    /// 직원 변경 후 제작 할당량 상태와 표시 갱신
    /// </summary>
    /// <param name="employeeId">변경된 직원 아이디</param>
    void RefreshCraftLimit ( string employeeId )
    {
        if ( _isEndStarted == false )
        {
            if ( _craftCount >= CraftLimit )
                _endReason = DayEndReason.CraftLimitReached;
            else if ( _endReason == DayEndReason.CraftLimitReached )
                _endReason = DayEndReason.None;
        }

        OnCraftProgressChanged?.Invoke( _craftCount, CraftLimit );
    }

    #endregion

    #region ----- 데이터 복구 -----

    /// <summary>
    /// 저장된 영업일 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 영업일 저장 데이터</param>
    public void Restore ( BusinessDaySaveData saveData )
    {
        _craftCount = saveData.CraftCount;
        _craftLimit = saveData.CraftLimit -
            saveData.EmployeeCraftBonus;
        _baseCraftLimit = saveData.BaseCraftLimit;
        _nextCraftLimit = saveData.NextCraftLimit -
            saveData.EmployeeCraftBonus;

        _quickRestockCount = saveData.QuickRestockCount;
        _quickRestockLimit = saveData.QuickRestockLimit;

        _isEndStarted = saveData.IsEndStarted;
        _endReason = saveData.EndReason;
    }
    #endregion
}
