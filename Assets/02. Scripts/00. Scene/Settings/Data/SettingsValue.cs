
/// <summary>
/// 실제 적용할 전체 설정값
/// </summary>
public class SettingsValue
{
    /// <summary>
    /// 마스터 볼륨
    /// </summary>
    public float MasterVolume { get; }

    /// <summary>
    /// 배경 음악 볼륨
    /// </summary>
    public float BGMVolume { get; }

    /// <summary>
    /// 효과음 볼륨
    /// </summary>
    public float SFXVolume { get; }

    /// <summary>
    /// 마스터 음소거 여부
    /// </summary>
    public bool IsMasterMuted { get; }

    /// <summary>
    /// 배경 음악 음소거 여부
    /// </summary>
    public bool IsBGMMuted { get; }

    /// <summary>
    /// 효과음 음소거 여부
    /// </summary>
    public bool IsSFXMuted { get; }

    /// <summary>
    /// 전체 화면 여부
    /// </summary>
    public bool IsFullScreen { get; }

    /// <summary>
    /// 적용 해상도
    /// </summary>
    public SettingsResolution Resolution { get; }

    /// <summary>
    /// 전체 설정값 생성
    /// </summary>
    /// <param name="masterVolume">마스터 볼륨</param>
    /// <param name="bgmVolume">배경 음악 볼륨</param>
    /// <param name="sfxVolume">효과음 볼륨</param>
    /// <param name="isMasterMuted">마스터 음소거 여부</param>
    /// <param name="isBGMMuted">배경 음악 음소거 여부</param>
    /// <param name="isSFXMuted">효과음 음소거 여부</param>
    /// <param name="isFullScreen">전체 화면 여부</param>
    /// <param name="resolution">적용 해상도</param>
    public SettingsValue (
        float masterVolume, float bgmVolume, float sfxVolume,
        bool isMasterMuted, bool isBGMMuted, bool isSFXMuted,
        bool isFullScreen, SettingsResolution resolution )
    {
        MasterVolume = masterVolume;
        BGMVolume = bgmVolume;
        SFXVolume = sfxVolume;
        IsMasterMuted = isMasterMuted;
        IsBGMMuted = isBGMMuted;
        IsSFXMuted = isSFXMuted;
        IsFullScreen = isFullScreen;
        Resolution = resolution;
    }
}
