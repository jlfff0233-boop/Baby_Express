using UnityEngine;

/// <summary>
/// 오디오와 화면 설정 실제 적용
/// </summary>
public class SettingsApplier
{
    AudioManager _audioManager;       //오디오 설정 적용 관리자

    /// <summary>
    /// 설정 적용기 생성
    /// </summary>
    /// <param name="audioManager">공용 오디오 관리자</param>
    public SettingsApplier ( AudioManager audioManager )
    {
        _audioManager = audioManager;
    }

    /// <summary>
    /// 오디오 설정 적용
    /// </summary>
    /// <param name="value">적용할 설정값</param>
    /// <returns>오디오 설정 적용 성공 여부</returns>
    public bool TryApplyAudio ( SettingsValue value )
    {
        bool audioApplied = _audioManager.TryApplySettings(
            value.MasterVolume, value.BGMVolume, value.SFXVolume,
            value.IsMasterMuted, value.IsBGMMuted, value.IsSFXMuted );

        if ( audioApplied == false )
        {
            Debug.LogWarning( "오디오 믹서의 설정 파라미터를 확인해 주세요." );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 화면 모드와 해상도 설정 적용
    /// </summary>
    /// <param name="value">적용할 설정값</param>
    public void ApplyDisplay ( SettingsValue value )
    {
        FullScreenMode screenMode = value.IsFullScreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(
            value.Resolution.Width,
            value.Resolution.Height,
            screenMode );
    }
}
