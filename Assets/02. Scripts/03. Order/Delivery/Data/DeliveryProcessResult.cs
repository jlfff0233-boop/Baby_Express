/// <summary>
/// 배송 처리 결과
/// </summary>
public enum DeliveryProcessResult
{
    Success,       //처리 성공
    InvalidSettings,       //잘못된 배송 설정
    InvalidOrder,       //배송할 수 없는 주문
    Expired,       //최종 납품 기한 초과
    CraftResultNotFound,       //확정 제작 결과 없음
    InvalidCraftResult,       //잘못된 확정 제작 결과
    AlreadyShipping,       //이미 배송 중인 주문
    AlreadyDelivered,       //이미 배송한 주문
    BudgetUpdateFailed,       //주문 대금 지급 실패
    HighGradeUpdateFailed,       //고등급 평가 수 변경 실패
    OrderUpdateFailed,       //주문 상태 변경 실패
    RollbackFailed,       //배송 처리 원상 복구 실패
    InvalidMethod,       //잘못된 배송 방식
    DirectDeliveryInUse,       //직접 배송 사용 중
    EmployeeUnavailable,       //배치 가능한 직원 없음
    NotAssigned,       //배치되지 않은 직원
    DeliveryCostUpdateFailed,       //배송비 처리 실패

}