/// <summary>
/// 정비 처리 결과
/// </summary>
public enum MaintenanceResult
{
    Success,        //처리 성공
    InvalidData,        //잘못된 정비 데이터
    NotFound,       //정비 항목 없음
    AlreadyOwned,       //이미 소유한 정비
    NotOwned,       //소유하지 않은 정비
    MaxLevel,       //최대 단계 도달
    RequirementNotMet,      //선행 조건 미달성
    EffectPending,      //이전 단계 효과 적용 대기
    InsufficientBudget,     //자금 부족
    EffectApplyFailed,      //정비 효과 적용 실패
    NoPendingEffect,        //적용 대기 효과 없음
    RollbackFailed,     //실패 후 원상 복구 실패
}
