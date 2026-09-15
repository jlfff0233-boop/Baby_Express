using DG.Tweening;
using UnityEngine;

/// <summary>
/// 게임 전체에서 유지되는 공용 매니저 관리
/// </summary>
public class GameManager : Singleton<GameManager>
{
    const string AudioManagerDataPath = "Settings/AudioManagerData";

    AudioManager _audioManager;       //오디오 관리자
    SettingsManager _settingsManager;       //설정 관리자
    SaveManager _saveManager;       //저장 관리자
    PoolManager _poolManager;       //공용 오브젝트 풀 관리자
    PlayStartRequest _playStartRequest;       //플레이 씬 시작 요청

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 오디오 관리자
    /// </summary>
    public AudioManager AudioManager
    {
        get
        {
            if ( _audioManager == null )
                InitAudioManager( );

            return _audioManager;
        }
    }

    /// <summary>
    /// 설정 관리자
    /// </summary>
    public SettingsManager SettingsManager
    {
        get
        {
            if ( _settingsManager == null )
                InitSettingsManager( );

            return _settingsManager;
        }
    }

    /// <summary>
    /// 저장 관리자
    /// </summary>
    public SaveManager SaveManager
    {
        get
        {
            if ( _saveManager == null )
                InitSaveManager( );

            return _saveManager;
        }
    }

    /// <summary>
    /// 공용 오브젝트 풀 관리자
    /// </summary>
    public PoolManager PoolManager
    {
        get
        {
            if ( _poolManager == null )
                InitPoolManager( );

            return _poolManager;
        }
    }

    /// <summary>
    /// 플레이 씬 시작 요청 존재 여부
    /// </summary>
    public bool HasPlayStartRequest => _playStartRequest != null;

    #endregion

    #region ----- 시작/초기화 -----

    /// <summary>
    /// 씬 UI가 연출을 생성하기 전에 DOTween 용량 확보
    /// </summary>
    [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
    static void ConfigureTweenCapacity ()
    {
        DOTween.SetTweensCapacity( 500, 200 );
    }

    protected override void Awake ()
    {
        base.Awake( );

        if ( IsPrimaryInstance == false )
            return;

        InitAudioManager( );
        InitSettingsManager( );
        InitSaveManager( );
        InitPoolManager( );
    }

    /// <summary>
    /// 오디오 매니저 생성 및 초기화
    /// </summary>
    void InitAudioManager ()
    {
        if ( _audioManager != null )
            return;

        AudioManagerData audioManagerData =
            Resources.Load<AudioManagerData>(
                AudioManagerDataPath );

        _audioManager =
            gameObject.GetOrAddComponent<AudioManager>( );

        _audioManager.Init( audioManagerData );
    }

    /// <summary>
    /// 세팅 매니저 생성 및 초기화
    /// </summary>
    void InitSettingsManager ()
    {
        if ( _settingsManager != null )
            return;

        if ( _audioManager == null )
            InitAudioManager( );

        _settingsManager =
            gameObject.GetOrAddComponent<SettingsManager>( );

        _settingsManager.Init( _audioManager );
    }

    /// <summary>
    /// 저장 매니저 생성 및 초기화
    /// </summary>
    void InitSaveManager ()
    {
        if ( _saveManager != null )
            return;

        _saveManager =
            gameObject.GetOrAddComponent<SaveManager>( );

        _saveManager.Init( );
    }

    /// <summary>
    /// 공용 오브젝트 풀 매니저 생성
    /// </summary>
    void InitPoolManager ()
    {
        if ( _poolManager != null )
            return;

        _poolManager =
            gameObject.GetOrAddComponent<PoolManager>( );
    }
    #endregion

    #region ----- 플레이 시작 요청 -----

    /// <summary>
    /// 플레이 씬 시작 요청 보관
    /// </summary>
    /// <param name="request">보관할 플레이 씬 시작 요청</param>
    public void SetPlayStartRequest ( PlayStartRequest request )
    {
        _playStartRequest = request;
    }

    /// <summary>
    /// 보관된 플레이 씬 시작 요청을 반환하고 제거
    /// </summary>
    /// <returns>Play 씬 시작 요청</returns>
    public PlayStartRequest TakePlayStartRequest ()
    {
        PlayStartRequest request = _playStartRequest;

        _playStartRequest = null;

        return request;
    }

    #endregion
}
