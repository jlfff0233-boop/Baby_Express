
/// <summary>
/// 저장 데이터 불러오기 후 플레이 화면 복구 처리
/// </summary>
public class PlaySceneLoadHandler
{
    PlaySceneNavHandler _navHandler;       //플레이 화면 전환 처리기
    TopBarPresenter _topBarPresenter;       //상단바 프레젠터

    /// <summary>
    /// 플레이 화면 복구 처리기 생성
    /// </summary>
    /// <param name="navHandler">플레이 화면 전환 처리기</param>
    /// <param name="topBarPresenter">상단바 프레젠터</param>
    public PlaySceneLoadHandler (
        PlaySceneNavHandler navHandler,
        TopBarPresenter topBarPresenter )
    {
        _navHandler = navHandler;
        _topBarPresenter = topBarPresenter;
    }

    /// <summary>
    /// 불러온 플레이 상태를 현재 화면에 반영
    /// </summary>
    public void RefreshAfterLoad ()
    {
        _navHandler.ResetAfterLoad( );
        _topBarPresenter.Refresh( );
    }
}