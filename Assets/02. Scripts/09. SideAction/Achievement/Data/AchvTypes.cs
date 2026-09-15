
/// <summary>
/// 업적 분류
/// </summary>
public enum AchvCategory
{
    Business = 0,       //영업
    Order = 1,      //주문
    Craft = 2,      //제작
    Etc = 3        //기타
}

/// <summary>
/// 업적 진행도 판정 종류
/// </summary>
public enum AchvProgressType
{
    BusinessDayCount = 0,       //누적 영업일
    WeeklySettlementCount = 1,      //완료한 주간 결산
    EmployeeHireCount = 2,      //성공한 직원 고용
    DeliveryCount = 3,      //완료한 납품
    SGradeCount = 4,      //S등급 납품
    CraftCount = 5,      //완료한 제작
    ActivatedThemeCraftCount = 6,       //일반 테마 활성 제작
    CompleteThemeCraftCount = 7,     //완성 테마 제작
    EmployeeDeliveryCount = 8,       //직원 배송
    CumulativeNetProfit = 9,      //누적 순이익
    DeadlineDayOnTimeDeliveryCount = 10,        //마감일 당일 정상 납품
    LateDeliveryCount = 11,       //지연 납품
    AOrHigherGradeCount = 12,     //A등급 이상 납품
    AllRequirementsDeliveryCount = 13,      //주요 요구 전체 달성 납품
    AllWishesDeliveryCount = 14,      //희망 사항 전체 달성 납품
    PerfectCustomCraftCount = 15,     //모든 주문 제작 조건 달성
    OppositeThemeDeliveryCount = 16       //상극 테마 활성 제작물 납품
}

/// <summary>
/// 업적 보상 종류
/// </summary>
public enum AchvRewardType
{
    None = 0,       //보상 없음
    Gold = 1,       //골드
    PartUnlock = 2,      //파츠 해금
    ProductUnlock = 3,       //상품 해금
    MaintenanceUnlock = 4,       //정비 구매 가능 해금
    DecorationUnlock = 5     //장식 해금 예약
}

/// <summary>
/// 업적 보상 수령 처리 결과
/// </summary>
public enum AchvRewardResult
{
    Success,        //수령 성공
    NotFound,        //업적을 찾을 수 없음
    NoUnclaimedReward,      //미수령 보상 없음
    InvalidReward,      //잘못된 보상 데이터
    RewardApplyFailed,        //보상 적용 실패
    StateUpdateFailed,       //수령 상태 갱신 실패
    RollbackFailed       //보상 원상 복구 실패
}
