
/// <summary>
/// 현재 실제 적용된 설정값 관리 모델
/// </summary>
public class SettingsModel
{
    SettingsValue _appliedValue;       //현재 실제 적용값

    /// <summary>
    /// 현재 실제 적용값
    /// </summary>
    public SettingsValue AppliedValue => _appliedValue;

    /// <summary>
    /// 설정 모델 생성
    /// </summary>
    /// <param name="initialValue">최초 적용값</param>
    public SettingsModel ( SettingsValue initialValue )
    {
        _appliedValue = initialValue;
    }

    /// <summary>
    /// 실제 적용에 성공한 오디오 설정값 확정
    /// </summary>
    /// <param name="value">확정할 오디오 설정값</param>
    public void CommitAudio ( SettingsValue value )
    {
        _appliedValue = new SettingsValue(
            value.MasterVolume, value.BGMVolume, value.SFXVolume,
            value.IsMasterMuted, value.IsBGMMuted, value.IsSFXMuted,
            _appliedValue.IsFullScreen, _appliedValue.Resolution );
    }

    /// <summary>
    /// 확인한 화면 설정값 확정
    /// </summary>
    /// <param name="value">확정할 화면 설정값</param>
    public void CommitDisplay ( SettingsValue value )
    {
        _appliedValue = new SettingsValue(
            _appliedValue.MasterVolume, _appliedValue.BGMVolume, _appliedValue.SFXVolume,
            _appliedValue.IsMasterMuted, _appliedValue.IsBGMMuted, _appliedValue.IsSFXMuted,
            value.IsFullScreen, value.Resolution );
    }

    /// <summary>
    /// 현재 적용값으로 편집 상태 생성
    /// </summary>
    /// <returns>새로운 설정 편집 상태</returns>
    public SettingsEditState CreateEditState ()
    {
        return new SettingsEditState( _appliedValue );
    }
}
