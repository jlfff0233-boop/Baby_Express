
/// <summary>
/// 주문 난이도
/// </summary>
public enum OrderDifficulty
{
    Easy,       //쉬움
    Normal,     //보통
    Hard,       //어려움
}

/// <summary>
/// 특수 주문 종류
/// </summary>
public enum OrderSpecialType
{
    None,       //일반 주문
    Vip,        //VIP 주문
}

/// <summary>
/// 주문 진행 상태
/// </summary>
public enum OrderProgressState
{
    Waiting,       //수락 대기
    Production,       //제작 요망
    Crafted,       //배송 대기
    Closed,       //종료
    Shipping,       //배송 중
}

/// <summary>
/// 주문 결과
/// </summary>
public enum OrderOutcome
{
    None,       //미정
    NormalDelivery,     //정상 납품
    LateDelivery,       //기한 초과 납품
    Rejected,       //거절
    AutoRejected,       //자동 거절
    Cancelled,      //취소
    Failed,     //실패
}

/// <summary>
/// 주문량 페널티 원인
/// </summary>
public enum OrderPenaltyType
{
    None,       //페널티 없음
    Reject,     //직접 거절
    AutoReject,     //자동 거절
    Cancel,     //주문 취소
    Fail,       //최종 실패
    Mixed,      //복합 원인
}

/// <summary>
/// 주문 처리 결과
/// </summary>
public enum OrderResult
{
    Success,    //추가 성공
    InvalidOrder,       //잘못된 주문
    Duplicate,      //중복
    NotFound,       //없음
    WaitingFull,        //대기 목록 만석
    InvalidState,       //처리할 수 없는 상태
    Expired,        //기한 만료
}

/// <summary>
/// 주문 생성 결과
/// </summary>
public enum OrderGenerateResult
{
    Success,        //생성 성공
    InvalidSettings,        //잘못된 생성 설정
    Unsolvable,        //제작 가능한 주문 조건 생성 실패
}

/// <summary>
/// 주문 목록 탭
/// </summary>
public enum OrderTab
{
    Waiting,        //수락 대기
    Producing,      //진행 중
    Closed,         //종료 주문
}
