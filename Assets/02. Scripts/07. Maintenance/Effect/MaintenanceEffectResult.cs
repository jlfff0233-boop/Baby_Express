
/// <summary>
/// 정비 효과 처리 결과
/// </summary>
public enum MaintenanceEffectResult
{
    Success,        //효과 처리 성공
    NotFound,       //정비 항목 없음
    NoPendingEffect,        //적용 대기 효과 없음
    InvalidLevelData,       //잘못된 단계 데이터
    InvalidEffect,      //잘못된 효과 데이터
    UnsupportedEffect,      //아직 지원하지 않는 효과
    ApplyFailed,        //담당 모델 적용 실패
    StateUpdateFailed,      //적용 단계 변경 실패
    RollbackFailed,     //실패 후 원상 복구 실패
}
