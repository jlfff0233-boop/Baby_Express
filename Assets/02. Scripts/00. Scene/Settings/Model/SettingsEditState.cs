
/// <summary>
/// 설정 화면에서 편집 중인 임시 설정 상태
/// </summary>
public class SettingsEditState
{
    /// <summary>
    /// 편집 중인 마스터 볼륨
    /// </summary>
    public float MasterVolume { get; set; }

    /// <summary>
    /// 편집 중인 배경 음악 볼륨
    /// </summary>
    public float BGMVolume { get; set; }

    /// <summary>
    /// 편집 중인 효과음 볼륨
    /// </summary>
    public float SFXVolume { get; set; }

    /// <summary>
    /// 편집 중인 마스터 음소거 여부
    /// </summary>
    public bool IsMasterMuted { get; set; }

    /// <summary>
    /// 편집 중인 배경 음악 음소거 여부
    /// </summary>
    public bool IsBGMMuted { get; set; }

    /// <summary>
    /// 편집 중인 효과음 음소거 여부
    /// </summary>
    public bool IsSFXMuted { get; set; }

    /// <summary>
    /// 편집 중인 전체 화면 여부
    /// </summary>
    public bool IsFullScreen { get; set; }

    /// <summary>
    /// 편집 중인 해상도
    /// </summary>
    public SettingsResolution Resolution { get; set; }

    /// <summary>
    /// 현재 적용값을 복사해 편집 상태 생성
    /// </summary>
    /// <param name="value">현재 적용된 설정값</param>
    public SettingsEditState ( SettingsValue value )
    {
        CopyFrom( value );
    }

    /// <summary>
    /// 적용된 설정값을 편집 상태로 복사
    /// </summary>
    /// <param name="value">복사할 설정값</param>
    public void CopyFrom ( SettingsValue value )
    {
        MasterVolume = value.MasterVolume;
        BGMVolume = value.BGMVolume;
        SFXVolume = value.SFXVolume;

        IsMasterMuted = value.IsMasterMuted;
        IsBGMMuted = value.IsBGMMuted;
        IsSFXMuted = value.IsSFXMuted;

        IsFullScreen = value.IsFullScreen;
        Resolution = value.Resolution;
    }

    /// <summary>
    /// 적용된 설정값의 화면 항목만 편집 상태로 복사
    /// </summary>
    /// <param name="value">복사할 적용 설정값</param>
    public void CopyDisplayFrom ( SettingsValue value )
    {
        IsFullScreen = value.IsFullScreen;
        Resolution = value.Resolution;
    }

    /// <summary>
    /// 현재 편집 상태를 적용용 설정값으로 생성
    /// </summary>
    /// <returns>적용할 전체 설정값</returns>
    public SettingsValue CreateValue ()
    {
        return new SettingsValue(
            MasterVolume, BGMVolume, SFXVolume,
            IsMasterMuted, IsBGMMuted, IsSFXMuted,
            IsFullScreen, Resolution );
    }
}
