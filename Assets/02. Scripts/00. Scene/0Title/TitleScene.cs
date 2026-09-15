using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬 기능과 플레이 씬 진입 중재
/// </summary>
public class TitleScene : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TitleSceneView _titleSceneView;       //타이틀 메인 뷰
    [SerializeField] TitleStartPresenter _startPresenter;       //게임 시작 프레젠터
    [SerializeField] SettingsPresenter _settingsPresenter;       //설정 프레젠터

    #region ----- 초기화 -----

    /// <summary>
    /// 타이틀 시스템 초기화
    /// </summary>
    void Awake ()
    {
        _startPresenter.Init(
            GameManager.Instance.SaveManager );

        _settingsPresenter.Init(
            GameManager.Instance.SettingsManager );
    }

    /// <summary>
    /// 타이틀 메인 패널 최초 등장 연출
    /// </summary>
    void Start ( )
    {
        _titleSceneView.HideInstant( );
        _titleSceneView.Show( );
    }

    /// <summary>
    /// 타이틀 입력 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        _titleSceneView.OnStartClicked += OpenNewGame;
        _titleSceneView.OnLoadClicked += OpenLoad;
        _titleSceneView.OnSettingsClicked += OpenSettings;
        _titleSceneView.OnQuitClicked += Quit;
        _startPresenter.OnPlayStart += StartPlay;
        _settingsPresenter.OnCloseRequested += CloseSettings;
    }

    /// <summary>
    /// 타이틀 입력 이벤트 해제
    /// </summary>
    void OnDisable ()
    {
        _titleSceneView.OnStartClicked -= OpenNewGame;
        _titleSceneView.OnLoadClicked -= OpenLoad;
        _titleSceneView.OnSettingsClicked -= OpenSettings;
        _titleSceneView.OnQuitClicked -= Quit;
        _startPresenter.OnPlayStart -= StartPlay;
        _settingsPresenter.OnCloseRequested -= CloseSettings;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 새 게임 시작 또는 슬롯 선택 화면 표시
    /// </summary>
    void OpenNewGame ()
    {
        _settingsPresenter.Hide( );
        _startPresenter.ShowNewGame( );
    }

    /// <summary>
    /// 이어하기 슬롯 선택 화면 표시
    /// </summary>
    void OpenLoad ()
    {
        _settingsPresenter.Hide( );
        _startPresenter.ShowLoad( );
    }

    /// <summary>
    /// 설정 화면 표시
    /// </summary>
    void OpenSettings ()
    {
        _startPresenter.Hide( );
        _settingsPresenter.Show( );
    }

    /// <summary>
    /// 설정 화면 닫기
    /// </summary>
    void CloseSettings ()
    {
        _settingsPresenter.Hide( );
    }

    #endregion

    #region ----- 시작/종료 -----

    /// <summary>
    /// 플레이 씬 시작 요청을 보관하고 씬 이동
    /// </summary>
    /// <param name="request">플레이 씬 시작 요청</param>
    void StartPlay ( PlayStartRequest request )
    {
        GameManager.Instance.SetPlayStartRequest( request );

        SceneManager.LoadScene( "Play" );
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    void Quit ()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ( );
#endif
    }

    #endregion
}
