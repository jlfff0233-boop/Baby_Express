using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 설정 편집, 적용과 화면 흐름 중재
/// </summary>
public class SettingsPresenter : MonoBehaviour
{
    const float DisplayConfirmDuration = 10f;       //화면 설정 확인 제한 시간

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] SettingsView _settingsView;       //설정 뷰

    SettingsManager _settingsManager;       //공용 설정 관리자
    SettingsEditState _editState;       //현재 임시 편집 상태
    Coroutine _displayConfirmCoroutine;       //화면 설정 확인 타이머
    bool _isDisplayPreviewing;       //화면 설정 임시 적용 여부
    bool _canEditDisplay;       //현재 플랫폼 화면 설정 지원 여부

    IReadOnlyList<SettingsResolution> _resolutions =
        Array.Empty<SettingsResolution>( );       //지원 해상도 목록

    /// <summary>
    /// 저장 화면 열기 요청 이벤트
    /// </summary>
    public event Action OnSaveRequested;

    /// <summary>
    /// 설정 화면 닫기 요청 이벤트
    /// </summary>
    public event Action OnCloseRequested;

    #region ----- 초기화/이벤트 -----

    /// <summary>
    /// 설정 프레젠터 초기화
    /// </summary>
    /// <param name="settingsManager">공용 설정 관리자</param>
    public void Init ( SettingsManager settingsManager )
    {
        _settingsManager = settingsManager;

        _settingsView.HideInstant( );
    }

    /// <summary>
    /// 설정 뷰 입력 연결
    /// </summary>
    void Awake ()
    {
        _settingsView.OnMasterVolumeChanged += ChangeMasterVolume;
        _settingsView.OnBGMVolumeChanged += ChangeBGMVolume;
        _settingsView.OnSFXVolumeChanged += ChangeSFXVolume;
        _settingsView.OnMasterMuteChanged += ChangeMasterMute;
        _settingsView.OnBGMMuteChanged += ChangeBGMMute;
        _settingsView.OnSFXMuteChanged += ChangeSFXMute;
        _settingsView.OnWindowModeChanged += ChangeWindowMode;
        _settingsView.OnResolutionChanged += ChangeResolution;
        _settingsView.OnDisplayConfirm += ConfirmDisplay;
        _settingsView.OnDisplayCancel += CancelDisplay;
        _settingsView.OnSaveOpen += RequestSave;
        _settingsView.OnApply += Apply;
        _settingsView.OnClose += RequestClose;
    }

    /// <summary>
    /// 설정 뷰 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _settingsView.OnMasterVolumeChanged -= ChangeMasterVolume;
        _settingsView.OnBGMVolumeChanged -= ChangeBGMVolume;
        _settingsView.OnSFXVolumeChanged -= ChangeSFXVolume;
        _settingsView.OnMasterMuteChanged -= ChangeMasterMute;
        _settingsView.OnBGMMuteChanged -= ChangeBGMMute;
        _settingsView.OnSFXMuteChanged -= ChangeSFXMute;
        _settingsView.OnWindowModeChanged -= ChangeWindowMode;
        _settingsView.OnResolutionChanged -= ChangeResolution;
        _settingsView.OnDisplayConfirm -= ConfirmDisplay;
        _settingsView.OnDisplayCancel -= CancelDisplay;
        _settingsView.OnSaveOpen -= RequestSave;
        _settingsView.OnApply -= Apply;
        _settingsView.OnClose -= RequestClose;

        StopDisplayConfirmCoroutine( );
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 현재 적용값으로 설정 화면 표시
    /// </summary>
    public void Show ()
    {
        _editState = _settingsManager.CreateEditState( );
        _isDisplayPreviewing = false;
        _canEditDisplay = Application.isMobilePlatform == false;

        _settingsView.Show( );
        _settingsView.HideDisplayConfirm( );
        _settingsView.SetDisplayVisible( _canEditDisplay );
        _settingsView.SetVolumes(
            _editState.MasterVolume, _editState.BGMVolume, _editState.SFXVolume );
        _settingsView.SetMutes(
            _editState.IsMasterMuted, _editState.IsBGMMuted, _editState.IsSFXMuted );

        if ( _canEditDisplay == false )
        {
            _resolutions = Array.Empty<SettingsResolution>( );
            return;
        }

        _resolutions =
            SettingsResolutionProvider.GetSupportedResolutions( );

        int resolutionIndex =
            FindResolutionIndex( _editState.Resolution );

        _settingsView.SetResolutionOptions( _resolutions );
        _settingsView.SetWindowMode( _editState.IsFullScreen );
        _settingsView.SetResolutionIndex( resolutionIndex );
    }

    /// <summary>
    /// 설정 화면을 숨기고 임시 편집 상태 폐기
    /// </summary>
    public void Hide ()
    {
        ResetEditState ( );
        _settingsView.Hide( );
    }

    /// <summary>
    /// 설정 화면을 즉시 숨기고 임시 편집 상태 폐기
    /// </summary>
    public void HideInstant ( )
    {
        ResetEditState ( );
        _settingsView.HideInstant ( );
    }

    /// <summary>
    /// 설정 임시 편집 상태 초기화
    /// </summary>
    void ResetEditState ( )
    {
        if ( _isDisplayPreviewing )
            CancelDisplay ( );
        else
            StopDisplayConfirmCoroutine ( );

        _editState = null;
    }

    /// <summary>
    /// 현재 설정 해상도의 Dropdown 인덱스 조회
    /// </summary>
    /// <param name="resolution">찾을 해상도</param>
    /// <returns>일치하는 해상도 인덱스</returns>
    int FindResolutionIndex ( SettingsResolution resolution )
    {
        for ( int i = 0; i < _resolutions.Count; i++ )
        {
            if ( _resolutions [ i ].IsSame( resolution ) )
                return i;
        }

        return 0;
    }

    #endregion

    #region ----- 화면 설정 확인 -----

    /// <summary>
    /// 실행 중인 화면 설정 확인 타이머 중지
    /// </summary>
    void StopDisplayConfirmCoroutine ()
    {
        if ( _displayConfirmCoroutine == null )
            return;

        StopCoroutine( _displayConfirmCoroutine );
        _displayConfirmCoroutine = null;
    }

    /// <summary>
    /// 화면 설정 확인 흐름 종료
    /// </summary>
    void CloseDisplayConfirm ()
    {
        _isDisplayPreviewing = false;
        StopDisplayConfirmCoroutine( );
        _settingsView.HideDisplayConfirm( );
    }

    /// <summary>
    /// 화면 설정을 마지막 확정값으로 복구하고 입력 표시 갱신
    /// </summary>
    void RestoreDisplay ()
    {
        _settingsManager.RestoreDisplay( _editState );

        int resolutionIndex =
            FindResolutionIndex( _editState.Resolution );

        _settingsView.SetWindowMode( _editState.IsFullScreen );
        _settingsView.SetResolutionIndex( resolutionIndex );
    }

    /// <summary>
    /// 임시 적용한 화면 설정을 이전 확정값으로 복구
    /// </summary>
    void CancelDisplay ()
    {
        if ( _isDisplayPreviewing == false )
            return;

        RestoreDisplay( );
        CloseDisplayConfirm( );
    }

    /// <summary>
    /// 임시 적용한 화면 설정 확정
    /// </summary>
    void ConfirmDisplay ()
    {
        if ( _isDisplayPreviewing == false )
            return;

        _settingsManager.ConfirmDisplay( _editState );
        CloseDisplayConfirm( );
    }

    /// <summary>
    /// 화면 설정 확인 제한 시간 계산
    /// </summary>
    /// <returns>화면 설정 확인 코루틴</returns>
    IEnumerator CountDisplayConfirmTime ()
    {
        float remainingTime = DisplayConfirmDuration;

        while ( remainingTime > 0f )
        {
            int remainingSeconds = Mathf.CeilToInt( remainingTime );

            _settingsView.SetDisplayConfirmTime( remainingSeconds );

            yield return null;
            remainingTime -= Time.unscaledDeltaTime;
        }

        _displayConfirmCoroutine = null;
        CancelDisplay( );
    }

    /// <summary>
    /// 현재 화면 편집값을 임시 적용하고 확인 제한 시간 시작
    /// </summary>
    void BeginDisplayPreview ()
    {
        if ( _canEditDisplay == false )
            return;

        _settingsManager.PreviewDisplay( _editState );
        _isDisplayPreviewing = true;

        StopDisplayConfirmCoroutine( );
        _settingsView.ShowDisplayConfirm(
            Mathf.CeilToInt( DisplayConfirmDuration ) );

        _displayConfirmCoroutine =
            StartCoroutine( CountDisplayConfirmTime( ) );
    }

    #endregion

    #region ----- 설정 편집 -----

    /// <summary>
    /// 마스터 볼륨 편집값 변경
    /// </summary>
    /// <param name="volume">변경할 볼륨</param>
    void ChangeMasterVolume ( float volume )
    {
        _editState.MasterVolume = volume;
    }

    /// <summary>
    /// 배경 음악 볼륨 편집값 변경
    /// </summary>
    /// <param name="volume">변경할 볼륨</param>
    void ChangeBGMVolume ( float volume )
    {
        _editState.BGMVolume = volume;
    }

    /// <summary>
    /// 효과음 볼륨 편집값 변경
    /// </summary>
    /// <param name="volume">변경할 볼륨</param>
    void ChangeSFXVolume ( float volume )
    {
        _editState.SFXVolume = volume;
    }

    /// <summary>
    /// 마스터 음소거 편집값 변경
    /// </summary>
    /// <param name="isMuted">음소거 여부</param>
    void ChangeMasterMute ( bool isMuted )
    {
        _editState.IsMasterMuted = isMuted;
    }

    /// <summary>
    /// 배경 음악 음소거 편집값 변경
    /// </summary>
    /// <param name="isMuted">음소거 여부</param>
    void ChangeBGMMute ( bool isMuted )
    {
        _editState.IsBGMMuted = isMuted;
    }

    /// <summary>
    /// 효과음 음소거 편집값 변경
    /// </summary>
    /// <param name="isMuted">음소거 여부</param>
    void ChangeSFXMute ( bool isMuted )
    {
        _editState.IsSFXMuted = isMuted;
    }

    /// <summary>
    /// 화면 모드 편집값 변경
    /// </summary>
    /// <param name="isFullScreen">전체 화면 여부</param>
    void ChangeWindowMode ( bool isFullScreen )
    {
        _editState.IsFullScreen = isFullScreen;
        BeginDisplayPreview( );
    }

    /// <summary>
    /// 해상도 편집값 변경
    /// </summary>
    /// <param name="optionIndex">선택한 해상도 인덱스</param>
    void ChangeResolution ( int optionIndex )
    {
        _editState.Resolution = _resolutions [ optionIndex ];
        BeginDisplayPreview( );
    }

    #endregion

    #region ----- 적용/닫기 -----

    /// <summary>
    /// 저장 화면 열기 요청 전달
    /// </summary>
    void RequestSave ()
    {
        OnSaveRequested?.Invoke( );
    }

    /// <summary>
    /// 현재 오디오 편집값을 실제 설정으로 적용하고 설정 화면 닫기
    /// </summary>
    void Apply ()
    {
        if ( _settingsManager.TryApplyAudio( _editState ) == false )
            return;

        RequestClose( );
    }

    /// <summary>
    /// 편집값을 폐기하고 설정 화면 닫기 요청
    /// </summary>
    void RequestClose ()
    {
        OnCloseRequested?.Invoke ( );
    }

    #endregion
}
