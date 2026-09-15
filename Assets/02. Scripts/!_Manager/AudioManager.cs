using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 공용 BGM, 효과음 재생과 오디오 설정 적용 관리
/// </summary>
public class AudioManager : MonoBehaviour
{
    const string MasterVolumeParameter = "MasterVolume";
    const string BGMVolumeParameter = "BGMVolume";
    const string SFXVolumeParameter = "SFXVolume";
    const string TitleSceneName = "Title";
    const string PlaySceneName = "Play";

    const float MinVolumeDecibel = -80f;

    AudioMixer _audioMixer;       //메인 오디오 믹서
    AudioMixerGroup _bgmMixerGroup;       //BGM 출력 그룹
    AudioMixerGroup _sfxMixerGroup;       //효과음 출력 그룹
    AudioClipMap _audioClipMap;       //종류별 음원 대응 데이터

    AudioSource _bgmSource;       //BGM 재생 소스
    AudioSource _sfxSource;       //효과음 재생 소스

    BGMType _sceneBGMType;       //현재 장면 배경 음악 종류
    bool _isEventBGMPlaying;       //이벤트 배경 음악 재생 여부
    bool _isInitialized;       //오디오 초기화 완료 여부

    readonly List<Button> _registeredButtons = new List<Button>( );
    readonly List<Toggle> _registeredToggles = new List<Toggle>( );
    readonly List<TMP_Dropdown> _registeredDropdowns =
        new List<TMP_Dropdown>( );

    #region ----- 초기화 / 이벤트 -----

    /// <summary>
    /// 오디오 설정 데이터와 재생 소스 및 공용 UI 효과음 초기화
    /// </summary>
    /// <param name="data">오디오 관리자 초기화 데이터</param>
    public void Init ( AudioManagerData data )
    {
        if ( _isInitialized ) return;

        if ( data == null )
        {
            Debug.LogError(
                "AudioManagerData를 Resources/Settings에 배치해 주세요." );
            return;
        }

        _audioMixer = data.AudioMixer;
        _bgmMixerGroup = data.BGMMixerGroup;
        _sfxMixerGroup = data.SFXMixerGroup;
        _audioClipMap = data.AudioClipMap;

        InitAudioSources( );
        PlaySceneBGM( SceneManager.GetActiveScene( ) );

        SceneManager.sceneLoaded += ChangeScene;

        RegisterSceneUIInputs( );

        _isInitialized = true;
    }

    /// <summary>
    /// 오디오 매니저 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        SceneManager.sceneLoaded -= ChangeScene;

        ClearUIInputs( );
    }

    /// <summary>
    /// 오디오 재생 소스 생성과 출력 그룹 연결
    /// </summary>
    void InitAudioSources ()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>( );
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.outputAudioMixerGroup = _bgmMixerGroup;

        _sfxSource = gameObject.AddComponent<AudioSource>( );
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
    }

    /// <summary>
    /// 씬 전환 후 장면 배경 음악과 공용 UI 입력 갱신
    /// </summary>
    /// <param name="scene">전환된 씬</param>
    /// <param name="loadMode">씬 불러오기 방식</param>
    void ChangeScene ( Scene scene, LoadSceneMode loadMode )
    {
        _sfxSource.Stop( );

        PlaySceneBGM( scene );

        ClearUIInputs( );
        RegisterSceneUIInputs( );
    }

    #endregion

    #region ----- 오디오 재생 -----

    /// <summary>
    /// 현재 장면에 맞는 배경 음악 재생
    /// </summary>
    /// <param name="scene">현재 장면</param>
    void PlaySceneBGM ( Scene scene )
    {
        _isEventBGMPlaying = false;

        switch ( scene.name )
        {
            case TitleSceneName:
                _sceneBGMType = BGMType.Title;
                break;

            case PlaySceneName:
                _sceneBGMType = BGMType.Play;
                break;

            default:
                return;
        }

        PlayBGM( _sceneBGMType );
    }

    /// <summary>
    /// 지정 종류의 배경 음악 재생
    /// </summary>
    /// <param name="type">재생할 배경 음악 종류</param>
    public void PlayBGM ( BGMType type )
    {
        AudioClip clip = _audioClipMap.GetBGMClip( type );

        if ( clip == null ) return;

        //같은 배경 음악이 재생 중이면 처음부터 다시 재생하지 않음
        if ( _bgmSource.clip == clip &&
            _bgmSource.isPlaying )
        {
            return;
        }

        _bgmSource.clip = clip;
        _bgmSource.Play( );
    }

    /// <summary>
    /// 이벤트 배경 음악 재생
    /// </summary>
    public void PlayEventBGM ()
    {
        _isEventBGMPlaying = true;
        PlayBGM( BGMType.Event );
    }

    /// <summary>
    /// 이벤트 이전의 장면 배경 음악 복구
    /// </summary>
    public void RestoreSceneBGM ()
    {
        if ( _isEventBGMPlaying == false ) return;

        _isEventBGMPlaying = false;
        PlayBGM( _sceneBGMType );
    }

    /// <summary>
    /// 현재 배경 음악 정지
    /// </summary>
    public void StopBGM ()
    {
        _bgmSource.Stop( );
        _bgmSource.clip = null;
    }

    /// <summary>
    /// 지정 종류의 효과음 일회 재생
    /// </summary>
    /// <param name="type">재생할 효과음 종류</param>
    public void PlaySFX ( SFXType type )
    {
        AudioClip clip = _audioClipMap.GetSFXClip( type );

        if ( clip == null ) return;

        _sfxSource.PlayOneShot( clip );
    }

    /// <summary>
    /// 자금 수입 효과음 재생
    /// </summary>
    public void PlayBudgetIncome ()
    {
        PlaySFX( SFXType.BudgetIncome );
    }

    /// <summary>
    /// 자금 지출 효과음 재생
    /// </summary>
    public void PlayBudgetExpense ()
    {
        PlaySFX( SFXType.BudgetExpense );
    }

    /// <summary>
    /// 기본 UI 클릭음 재생
    /// </summary>
    public void PlayButtonClick ()
    {
        PlaySFX( SFXType.Button );
    }

    #endregion

    #region ----- 설정 적용 -----

    /// <summary>
    /// 전체 오디오 볼륨과 음소거 설정 적용
    /// </summary>
    /// <param name="masterVolume">전체 음량</param>
    /// <param name="bgmVolume">배경 음악 음량</param>
    /// <param name="sfxVolume">효과음 음량</param>
    /// <param name="isMasterMuted">전체 음소거 여부</param>
    /// <param name="isBGMMuted">배경 음악 음소거 여부</param>
    /// <param name="isSFXMuted">효과음 음소거 여부</param>
    /// <returns>전체 설정 적용 성공 여부</returns>
    public bool TryApplySettings (
        float masterVolume, float bgmVolume, float sfxVolume,
        bool isMasterMuted, bool isBGMMuted, bool isSFXMuted )
    {
        if ( CanApplySettings( ) == false )
            return false;

        bool masterApplied = _audioMixer.SetFloat(
            MasterVolumeParameter,
            GetAppliedDecibel( masterVolume, isMasterMuted ) );

        bool bgmApplied = _audioMixer.SetFloat(
            BGMVolumeParameter,
            GetAppliedDecibel( bgmVolume, isBGMMuted ) );

        bool sfxApplied = _audioMixer.SetFloat(
            SFXVolumeParameter,
            GetAppliedDecibel( sfxVolume, isSFXMuted ) );

        return masterApplied && bgmApplied && sfxApplied;
    }

    /// <summary>
    /// 오디오 설정 파라미터 사용 가능 여부 확인
    /// </summary>
    /// <returns>전체 파라미터 사용 가능 여부</returns>
    bool CanApplySettings ()
    {
        return
            _audioMixer.GetFloat( MasterVolumeParameter, out _ ) &&
            _audioMixer.GetFloat( BGMVolumeParameter, out _ ) &&
            _audioMixer.GetFloat( SFXVolumeParameter, out _ );
    }

    /// <summary>
    /// 음소거 상태를 반영한 적용 데시벨 조회
    /// </summary>
    /// <param name="volume">0에서 1 사이의 볼륨</param>
    /// <param name="isMuted">음소거 여부</param>
    /// <returns>오디오 믹서에 적용할 데시벨</returns>
    float GetAppliedDecibel ( float volume, bool isMuted )
    {
        if ( isMuted )
            return MinVolumeDecibel;

        float normalizedVolume = Mathf.Clamp01( volume );

        if ( normalizedVolume <= 0f )
            return MinVolumeDecibel;

        return 20f * Mathf.Log10( normalizedVolume );
    }

    #endregion

    #region ----- 공용 UI 효과음 -----

    /// <summary>
    /// 현재 씬의 UI 클릭 입력에 기본 효과음 연결
    /// </summary>
    void RegisterSceneUIInputs ()
    {
        Button [ ] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None );

        Toggle [ ] toggles = FindObjectsByType<Toggle>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None );

        TMP_Dropdown [ ] dropdowns = FindObjectsByType<TMP_Dropdown>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None );

        for ( int i = 0; i < buttons.Length; i++ )
            RegisterButton( buttons [ i ] );

        for ( int i = 0; i < toggles.Length; i++ )
            RegisterToggle( toggles [ i ] );

        for ( int i = 0; i < dropdowns.Length; i++ )
            RegisterDropdown( dropdowns [ i ] );
    }

    /// <summary>
    /// 런타임 생성 버튼에 기본 클릭음 연결
    /// </summary>
    /// <param name="button">연결할 버튼</param>
    public void RegisterButton ( Button button )
    {
        if ( _registeredButtons.Contains( button ) )
            return;

        _registeredButtons.Add( button );
        button.onClick.AddListener( PlayButtonClick );
    }

    /// <summary>
    /// 런타임 생성 토글에 기본 클릭음 연결
    /// </summary>
    /// <param name="toggle">연결할 토글</param>
    public void RegisterToggle ( Toggle toggle )
    {
        if ( _registeredToggles.Contains( toggle ) )
            return;

        _registeredToggles.Add( toggle );
        toggle.onValueChanged.AddListener( PlayToggleClick );
    }

    /// <summary>
    /// 런타임 생성 Dropdown에 기본 클릭음 연결
    /// </summary>
    /// <param name="dropdown">연결할 Dropdown</param>
    public void RegisterDropdown ( TMP_Dropdown dropdown )
    {
        if ( _registeredDropdowns.Contains( dropdown ) )
            return;

        _registeredDropdowns.Add( dropdown );
        dropdown.onValueChanged.AddListener( PlayDropdownClick );
    }

    /// <summary>
    /// 토글 변경 시 기본 클릭음 재생
    /// </summary>
    /// <param name="isOn">변경된 토글 상태</param>
    void PlayToggleClick ( bool isOn )
    {
        PlayButtonClick( );
    }

    /// <summary>
    /// Dropdown 선택 시 기본 클릭음 재생
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 인덱스</param>
    void PlayDropdownClick ( int optionIndex )
    {
        PlayButtonClick( );
    }

    /// <summary>
    /// 연결된 UI 효과음 이벤트 전체 해제
    /// </summary>
    void ClearUIInputs ()
    {
        for ( int i = 0; i < _registeredButtons.Count; i++ )
        {
            if ( _registeredButtons [ i ] != null )
            {
                _registeredButtons [ i ].onClick.RemoveListener(
                    PlayButtonClick );
            }
        }

        for ( int i = 0; i < _registeredToggles.Count; i++ )
        {
            if ( _registeredToggles [ i ] != null )
            {
                _registeredToggles [ i ].onValueChanged.RemoveListener(
                    PlayToggleClick );
            }
        }

        for ( int i = 0; i < _registeredDropdowns.Count; i++ )
        {
            if ( _registeredDropdowns [ i ] != null )
            {
                _registeredDropdowns [ i ].onValueChanged.RemoveListener(
                    PlayDropdownClick );
            }
        }

        _registeredButtons.Clear( );
        _registeredToggles.Clear( );
        _registeredDropdowns.Clear( );
    }

    #endregion
}
