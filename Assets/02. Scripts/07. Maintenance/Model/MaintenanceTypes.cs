
/// <summary>
/// 정비 종류
/// </summary>
public enum MaintenanceType
{
    Facility,       //시설
    Research,       //연구
    Convenience,        //편의
}

/// <summary>
/// 정비 효과 적용 시점
/// </summary>
public enum MaintenanceApplyType
{
    Immediately,        //즉시 적용
    NextDay,        //다음 영업일부터 적용
}

/// <summary>
/// 정비 효과 종류
/// </summary>
public enum MaintenanceEffectType
{
    InventoryCapacity,       //인벤토리 최대 용량
    PartMaxStockBonus,       //파츠 최대 재고 보정값
    RestockSpan,       //파츠 재입고 간격
    OrderWaitingLimit,       //수락 대기 주문 한도
    CraftQuota,       //다음 영업일 제작 할당량
    DeliverySpanReduction,       //배송 소요일 단축값
    PartUnlock,       //파츠 해금
    InformationUnlock,       //정보 표시 해금
    ThemeScoreBonus,        //테마 점수 보너스
}

/// <summary>
/// 정비 화면 탭
/// </summary>
enum MaintenanceTab
{
    Management,       //관리
    Facility,       //시설
    Research,       //연구
    Convenience,       //편의
    Employee,       //고용
}

/// <summary>
/// 정비 선행 조건 종류
/// </summary>
public enum MaintenanceRequirementType
{
    TotalDay,       //누적 영업일
    WeeklyRating,      //주간 영업 평가
    MaintenanceLevel,      //선행 정비 단계
}

/// <summary>
/// 정보 표시 해금 아이디
/// </summary>
public static class InformationUnlockId
{
    public const string OrderDeadlineAlert =
        "Info_OrderDeadlineAlert";       //주문 기한 임박 알림

    public const string MissingPartsDisplay =
        "Info_MissingPartsDisplay";       //부족 파츠 표시

    public const string DeliveryEstimatedArrival =
        "Info_DeliveryEstimatedArrival";       //배송 예상 완료일 표시

    public static bool IsDefined ( string id )
    {
        return id == OrderDeadlineAlert ||
            id == MissingPartsDisplay ||
            id == DeliveryEstimatedArrival;
    }
}
