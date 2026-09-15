using System.Collections.Generic;

/// <summary>
/// 정비 전체 현황 표시 데이터
/// </summary>
public class MaintenanceManagementViewData
{
    /// <summary>
    /// 개별 업그레이드 효과 슬롯 목록
    /// </summary>
    public IReadOnlyList<MaintenanceEffectSlotViewData> Effects { get; }

    /// <summary>
    /// 정비 전체 현황 표시 데이터 생성
    /// </summary>
    /// <param name="effects">개별 업그레이드 효과 슬롯 목록</param>
    public MaintenanceManagementViewData (
        IReadOnlyList<MaintenanceEffectSlotViewData> effects )
    {
        Effects = effects;
    }
}
