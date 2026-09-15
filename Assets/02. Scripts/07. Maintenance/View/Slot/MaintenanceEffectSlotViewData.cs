/// <summary>
/// 정비 전체 탭 슬롯 분류
/// </summary>
public enum MaintenanceEffectSlotCategory
{
    Facility,       //시설
    Research,       //연구
    Convenience,       //편의
    Employee,       //고용
}

/// <summary>
/// 정비 전체 탭 효과 슬롯 표시 데이터
/// </summary>
public class MaintenanceEffectSlotViewData
{
    /// <summary>
    /// 슬롯 분류
    /// </summary>
    public MaintenanceEffectSlotCategory Category { get; }

    /// <summary>
    /// 업그레이드 항목 이름과 단계
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 현재 효과와 다음 상태 안내
    /// </summary>
    public string Info { get; }

    /// <summary>
    /// 정비 효과 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="category">슬롯 분류</param>
    /// <param name="title">업그레이드 항목 이름과 단계</param>
    /// <param name="info">현재 효과와 다음 상태 안내</param>
    public MaintenanceEffectSlotViewData (
        MaintenanceEffectSlotCategory category,
        string title, string info )
    {
        Category = category;
        Title = title;
        Info = info;
    }
}
