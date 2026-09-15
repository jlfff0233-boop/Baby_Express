using UnityEngine;

/// <summary>
/// 직원 종족
/// </summary>
public enum EmployeeSpecies
{
    Dullahan,       //듀라한
    Imp,       //임프
    Goblin,       //고블린
    Succubus,       //서큐버스
    Gremlin,       //그렘린
    Witch,       //마녀
    Gargoyle,       //가고일
    Lich,       //리치
}

/// <summary>
/// 직원 직종
/// </summary>
public enum EmployeeJobType
{
    Delivery,       //배송
    OperationsRunner,       //심부름
    StockClerk,     //재고 담당
    Agent,      //주문 담당
    Engineer,       //기술
    Researcher,     //연구
    Carrier,        //운반
    Accountant,     //회계
}

/// <summary>
/// 직원 해금 조건 종류
/// </summary>
public enum EmployeeUnlockType
{
    FirstWeeklySettlement,       //첫 주간 결산 완료
    TotalDay,       //누적 영업일 도달
}

/// <summary>
/// 직원 효과 종류
/// </summary>
public enum EmployeeEffectType
{
    DeliverySlotBonus,       //배송 슬롯 증가
    QuickRestockCostRate,       //빠른 재입고 비용 배율
    InventorySaleRate,       //인벤토리 판매 가격 배율
    DailyOrderMinimumBonus,       //일일 최소 주문 수 증가
    FacilityMaintCostRate,       //시설 유지비 배율
    ResearchMaintCostRate,       //연구 유지비 배율
    CraftQuotaBonus,       //일일 제작 할당량 증가
    WeeklySalaryRate,       //전체 주급 배율
}

/// <summary>
/// 직원 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "MaintenanceSettings/EmployeeData" )]
public class EmployeeData : ScriptableObject
{
    [Header( "----- 직원 정보 -----" )]
    [SerializeField] string _id;       //직원 아이디
    [SerializeField] EmployeeSpecies _species;       //직원 종족
    [SerializeField] string _firstName;       //영문 이름
    [SerializeField] string _lastName;       //영문 성
    [SerializeField] string _displayName;       //한글 표시 이름
    [SerializeField] string _name;       //기존 직원 이름
    [SerializeField] EmployeeJobType _jobType;       //직원 직종
    [SerializeField] Sprite _dialoguePortrait;       //대화용 전신 초상화
    [SerializeField] Sprite _hirePortrait;       //고용 화면용 초상화

    [Header( "----- 고용 설정 -----" )]
    [SerializeField] float _hireCost;       //최초 고용비
    [SerializeField] float _weeklyWage;       //직원 주급
    [SerializeField] EmployeeUnlockType _unlockType;       //해금 조건 종류
    [SerializeField] int _requiredDay;       //해금 요구 누적 영업일
    [SerializeField] EmployeeEffectType _effectType;       //직원 효과 종류
    [SerializeField] float _effectValue;       //직원 효과값
    [SerializeField] int _deliverySlotIncrease;       //직원 배송 슬롯 증가량
    [SerializeField] int _requiredWeeklySettlementCount;       //해금 요구 주간 결산 수

    /// <summary>
    /// 직원 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 직원 종족
    /// </summary>
    public EmployeeSpecies Species => _species;

    /// <summary>
    /// 영문 이름
    /// </summary>
    public string FirstName => _firstName;

    /// <summary>
    /// 영문 성
    /// </summary>
    public string LastName => _lastName;

    /// <summary>
    /// 영문 전체 이름
    /// </summary>
    public string EnglishName => $"{_firstName} {_lastName}".Trim( );

    /// <summary>
    /// 한글 표시 이름
    /// </summary>
    public string DisplayName => string.IsNullOrWhiteSpace( _displayName )
        ? _name
        : _displayName;

    /// <summary>
    /// 직원 이름
    /// </summary>
    public string Name => DisplayName;

    /// <summary>
    /// 직원 직종
    /// </summary>
    public EmployeeJobType JobType => _jobType;

    /// <summary>
    /// 대화용 전신 초상화
    /// </summary>
    public Sprite DialoguePortrait => _dialoguePortrait;

    /// <summary>
    /// 고용 화면용 초상화
    /// </summary>
    public Sprite HirePortrait => _hirePortrait;

    /// <summary>
    /// 직원 1명 고용비
    /// </summary>
    public float HireCost => _hireCost;

    /// <summary>
    /// 직원 주급
    /// </summary>
    public float WeeklyWage => _weeklyWage;

    /// <summary>
    /// 직원 해금 조건 종류
    /// </summary>
    public EmployeeUnlockType UnlockType => _unlockType;

    /// <summary>
    /// 해금 요구 누적 영업일
    /// </summary>
    public int RequiredDay => _requiredDay;

    /// <summary>
    /// 직원 효과 종류
    /// </summary>
    public EmployeeEffectType EffectType => _effectType;

    /// <summary>
    /// 직원 효과값
    /// </summary>
    public float EffectValue => _effectValue;

    /// <summary>
    /// 확장 직원 설정 사용 여부
    /// </summary>
    public bool UsesExpandedSettings =>
        string.IsNullOrWhiteSpace( _displayName ) == false ||
        _requiredDay > 0 ||
        Mathf.Approximately( _effectValue, 0f ) == false;

    /// <summary>
    /// 직원 배송 슬롯 증가량
    /// </summary>
    public int DeliverySlotIncrease => _deliverySlotIncrease;

    /// <summary>
    /// 해금 요구 주간 결산 수
    /// </summary>
    public int RequiredWeeklySettlementCount => _requiredWeeklySettlementCount;
}
