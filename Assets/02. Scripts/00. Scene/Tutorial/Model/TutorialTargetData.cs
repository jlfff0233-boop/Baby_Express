using UnityEngine;

/// <summary>
/// 튜토리얼 표시 방식
/// </summary>
public enum TutorialGuideMode
{
    Blocking,       //강조 대상 외 입력 차단
    Focus,       //입력 차단 없이 대상만 강조
}

/// <summary>
/// 튜토리얼 강조 대상
/// </summary>
public enum TutorialTargetId
{
    None,

    OrderButton,
    TutorialOrderSlot,
    OrderAcceptButton,

    ShopButton,
    RequiredPartSlot,
    PurchaseButton,

    InventoryButton,
    InventoryPartSlot,

    CraftButton,
    CraftOrderSlot,
    BodyPartSlot,
    EyePartSlot,
    NosePartSlot,
    MouthPartSlot,
    CraftDoneButton,

    DeliveryButton,
    DeliveryStartButton,

    SettlementConfirmButton,

    Requirement,
    Wish,

    AddCartButton,
    CartButton,

    //제작 튜토리얼 추가 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    CraftStartButton,
    CraftCost,
    OrderConditions,
    PlacedEyePart,

    //배송 튜토리얼 추가 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    DirectDelivery,
    DeliverySummary,

    //결산 튜토리얼 추가 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    SettlementOrderResult,
    SettlementEconomy,
    SettlementSummary,

    //제작 사이드 패널 입력 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    PartsSelectOpenButton,
    CraftInfoOpenButton,

    //주문 설명 추가 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    OrderDeadline,

    //상점 재고 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    ShopStockInfo,
    OutOfStockProduct,
    QuickRestockButton,

    //정비 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    MaintenanceButton,
    MaintenanceFacilityTab,
    MaintenanceResearchTab,
    MaintenanceConvenienceTab,
    MaintenanceEmployeeTab,
    MaintenanceCurrentLevel,
    MaintenanceNextLevel,

    //카탈로그 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    CatalogButton,
    CatalogLockedPart,
    CatalogPart,
    CatalogSearch,
    CatalogFilter,
    CatalogShopLink,

    //업적 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    AchievementButton,
    AchievementProgress,
    AchievementStage,
    AchievementReward,
    AchievementClaimButton,

    //직원 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    GuideEmployeeCard,
    EmployeeCost,
    EmployeeEffect,

    //주간 결산 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    WeeklyDeliverySummary,
    WeeklyGradeDistribution,
    WeeklyRequirementRate,
    WeeklyFinance,
    WeeklyBusinessGrade,

    //가계부 가이드 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    LedgerButton,
    LedgerWeekSelector,

    //사용 파츠 전환 입력 대상, 기존 직렬화 번호 유지를 위해 마지막에 배치
    CraftInfoSwitchButton,
}

/// <summary>
/// 튜토리얼 대상 연결 데이터
/// </summary>
public class TutorialTargetData
{
    TutorialTargetId _targetId;       //대상 아이디
    RectTransform _target;       //강조할 실제 UI
    Vector2 _arrowOffset;       //대상 기준 화살표 위치

    UIHighlightView _highlight;       //대상 강조 연출

    /// <summary>
    /// 대상 아이디
    /// </summary>
    public TutorialTargetId TargetId => _targetId;

    /// <summary>
    /// 강조할 실제 UI
    /// </summary>
    public RectTransform Target => _target;

    /// <summary>
    /// 대상 기준 화살표 위치
    /// </summary>
    public Vector2 ArrowOffset => _arrowOffset;

    /// <summary>
    /// 대상 강조 연출
    /// </summary>
    public UIHighlightView Highlight => _highlight;

    /// <summary>
    /// 런타임 튜토리얼 대상 데이터 생성
    /// </summary>
    /// <param name="targetId">대상 아이디</param>
    /// <param name="target">강조할 실제 UI</param>
    /// <param name="arrowOffset">대상 기준 화살표 위치 보정</param>
    public TutorialTargetData (
        TutorialTargetId targetId,
        RectTransform target,
        Vector2 arrowOffset )
    {
        _targetId = targetId;
        SetTarget( target, arrowOffset );
    }

    /// <summary>
    /// 런타임에 생성된 실제 강조 대상 연결
    /// </summary>
    /// <param name="target">현재 활성 UI 대상</param>
    /// <param name="arrowOffset">대상 기준 화살표 위치 보정</param>
    public void SetTarget (
        RectTransform target, Vector2 arrowOffset )
    {
        if ( _highlight != null )
            _highlight.Stop( );

        _target = target;
        _arrowOffset = arrowOffset;
        _highlight = null;

        if ( _target == null ) return;

        _highlight =
            _target.gameObject.GetOrAddComponent<UIHighlightView>( );
    }

}
