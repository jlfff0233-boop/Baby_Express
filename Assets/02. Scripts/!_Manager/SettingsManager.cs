using UnityEngine;

/// <summary>
/// 실행 중 공용 설정 상태와 실제 설정 적용 관리
/// </summary>
public class SettingsManager : MonoBehaviour
{
    SettingsModel _model;       //현재 실제 적용 설정
    SettingsApplier _applier;       //실제 설정 적용기

    /// <summary>
    /// 현재 공용 설정 모델
    /// </summary>
    public SettingsModel Model => _model;

    /// <summary>
    /// 현재 실행 환경을 기준으로 설정 관리자 초기화
    /// </summary>
    /// <param name="audioManager">공용 오디오 관리자</param>
    public void Init ( AudioManager audioManager )
    {
        SettingsResolution resolution =
            new SettingsResolution( Screen.width, Screen.height );

        SettingsValue initialValue = new SettingsValue(
            1f, 1f, 1f,
            false, false, false,
            Screen.fullScreen, resolution );

        _model = new SettingsModel( initialValue );
        _applier = new SettingsApplier( audioManager );
    }

    /// <summary>
    /// 현재 적용값을 복사한 편집 상태 생성
    /// </summary>
    /// <returns>새로운 설정 편집 상태</returns>
    public SettingsEditState CreateEditState ()
    {
        return _model.CreateEditState( );
    }

    /// <summary>
    /// 현재 오디오 편집값 적용 및 확정
    /// </summary>
    /// <param name="editState">현재 설정 편집 상태</param>
    /// <returns>오디오 설정 적용 성공 여부</returns>
    public bool TryApplyAudio ( SettingsEditState editState )
    {
        SettingsValue value = editState.CreateValue( );

        if ( _applier.TryApplyAudio( value ) == false )
            return false;

        //실제 환경에 적용된 뒤 오디오 설정값 확정
        _model.CommitAudio( value );

        return true;
    }

    /// <summary>
    /// 현재 화면 편집값 임시 적용
    /// </summary>
    /// <param name="editState">현재 설정 편집 상태</param>
    public void PreviewDisplay ( SettingsEditState editState )
    {
        _applier.ApplyDisplay( editState.CreateValue( ) );
    }

    /// <summary>
    /// 임시 적용 중인 화면 설정 확정
    /// </summary>
    /// <param name="editState">현재 설정 편집 상태</param>
    public void ConfirmDisplay ( SettingsEditState editState )
    {
        _model.CommitDisplay( editState.CreateValue( ) );
    }

    /// <summary>
    /// 마지막 확정 화면 설정으로 복구
    /// </summary>
    /// <param name="editState">현재 설정 편집 상태</param>
    public void RestoreDisplay ( SettingsEditState editState )
    {
        SettingsValue appliedValue = _model.AppliedValue;

        _applier.ApplyDisplay( appliedValue );
        editState.CopyDisplayFrom( appliedValue );
    }
}
