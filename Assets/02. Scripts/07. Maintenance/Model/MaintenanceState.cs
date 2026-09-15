/// <summary>
/// 정비 항목 런타임 상태
/// </summary>
public class MaintenanceState
{
    MaintenanceData _data;       //정비 설정 데이터
    int _purchasedLevel;      //구매한 정비 단계
    int _appliedLevel;        //실제 적용된 정비 단계

    /// <summary>
    /// 정비 설정 데이터
    /// </summary>
    public MaintenanceData Data => _data;

    /// <summary>
    /// 구매한 정비 단계
    /// </summary>
    public int PurchasedLevel => _purchasedLevel;

    /// <summary>
    /// 실제 적용된 정비 단계
    /// </summary>
    public int AppliedLevel => _appliedLevel;

    /// <summary>
    /// 정비 소유 여부
    /// </summary>
    public bool IsOwned => _purchasedLevel > 0;

    /// <summary>
    /// 최대 단계 도달 여부
    /// </summary>
    public bool IsMaxLevel => _purchasedLevel >= _data.MaxLevel;

    /// <summary>
    /// 다음 적용 대기 여부
    /// </summary>
    public bool HasPendingEffect => _purchasedLevel > _appliedLevel;

    /// <summary>
    /// 다음 정비 단계
    /// </summary>
    public int NextLevel => _purchasedLevel + 1;

    /// <summary>
    /// 정비 상태 생성
    /// </summary>
    /// <param name="data">정비 설정 데이터</param>
    public MaintenanceState ( MaintenanceData data )
    {
        _data = data;
    }

    /// <summary>
    /// 다음 정비 단계 구매
    /// </summary>
    /// <returns>단계 변경 성공 여부</returns>
    public bool Upgrade ()
    {
        //최대 단계면 변경하지 않음
        if ( IsMaxLevel ) return false;

        _purchasedLevel++;
        return true;
    }

    /// <summary>
    /// 구매한 정비 단계까지 효과 적용
    /// </summary>
    /// <returns>적용 단계 변경 성공 여부</returns>
    public bool ApplyPurchasedLevel ()
    {
        //적용할 새 단계가 없으면 변경하지 않음
        if ( HasPendingEffect == false ) return false;

        _appliedLevel = _purchasedLevel;
        return true;
    }

    /// <summary>
    /// 적용 대기 중인 마지막 구매 단계 복구
    /// </summary>
    /// <returns>구매 단계 복구 성공 여부</returns>
    public bool RollbackUpgrade ()
    {
        //이미 적용된 단계는 구매 단계에서 제거하지 않음
        if ( HasPendingEffect == false ) return false;

        _purchasedLevel--;
        return true;
    }

    /// <summary>
    /// 마지막 적용 단계 복구
    /// </summary>
    /// <returns>적용 단계 복구 성공 여부</returns>
    public bool RollbackAppliedLevel ()
    {
        //적용된 단계가 없으면 종료
        if ( _appliedLevel <= 0 ) return false;

        _appliedLevel--;
        return true;
    }

    /// <summary>
    /// 저장된 정비 단계 복구
    /// </summary>
    /// <param name="purchasedLevel">구매한 정비 단계</param>
    /// <param name="appliedLevel">실제 적용된 정비 단계</param>
    internal void Restore (
        int purchasedLevel, int appliedLevel )
    {
        _purchasedLevel = purchasedLevel;
        _appliedLevel = appliedLevel;
    }

}
