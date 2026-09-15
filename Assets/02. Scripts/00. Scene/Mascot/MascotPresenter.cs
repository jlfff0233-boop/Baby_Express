using System.Collections;
using UnityEngine;

/// <summary>
/// 마스코트 프레젠터 - 배회와 팁 표시 흐름 중재
/// </summary>
public class MascotPresenter : MonoBehaviour
{
    [SerializeField] MascotSettingsData _settings;       //마스코트 표시 설정
    [SerializeField] MascotView _view;       //마스코트 뷰

    Coroutine _moveRoutine;       //배회 반복 작업
    Coroutine _tipRoutine;       //팁 표시 반복 작업
    int _lastTipIndex = -1;       //직전에 표시한 팁 번호
    bool _isInitialized;       //초기화 여부
    bool _isIntroPaused;       //인트로로 전체 동작을 멈춘 상태
    bool _isTipPaused;       //튜토리얼 등으로 팁 출력을 멈춘 상태
    bool _isDialogueHidden;       //대화 중 마스코트 숨김 상태

    /// <summary>
    /// 마스코트 입력과 반복 동작 초기화
    /// </summary>
    public void Init ()
    {
        if ( _isInitialized == true ) return;

        _isInitialized = true;
        _view.OnTouched += Touch;

        if ( _isIntroPaused == false )
            StartRoutines( );
    }

    /// <summary>
    /// 인트로 중 마스코트 전체 동작과 표시 상태 변경
    /// </summary>
    /// <param name="isPaused">인트로 일시 정지 여부</param>
    public void SetIntroPaused ( bool isPaused )
    {
        if ( _isIntroPaused == isPaused ) return;

        _isIntroPaused = isPaused;

        if ( _isIntroPaused == true )
        {
            StopRoutines( );
            _view.SetVisible( false );
            return;
        }

        _view.SetVisible( true );

        if ( _isInitialized && isActiveAndEnabled )
            StartRoutines( );
    }

    /// <summary>
    /// 마스코트 이동은 유지하고 팁 출력 상태만 변경
    /// </summary>
    /// <param name="isPaused">팁 출력 중지 여부</param>
    public void SetTipPaused ( bool isPaused )
    {
        if ( _isTipPaused == isPaused ) return;

        _isTipPaused = isPaused;

        if ( _isTipPaused == true )
        {
            StopTipRoutine( );
            _view.HideTip( );
            return;
        }

        if ( _isInitialized && isActiveAndEnabled )
            StartRoutines( );
    }

    /// <summary>
    /// 공용 대화 중 마스코트 표시 상태 변경
    /// </summary>
    /// <param name="isHidden">대화로 숨길지 여부</param>
    public void SetDialogueHidden ( bool isHidden )
    {
        if ( _isDialogueHidden == isHidden ) return;

        _isDialogueHidden = isHidden;

        //인트로의 즉시 숨김 상태가 대화 연출보다 우선
        if ( _isIntroPaused == true ) return;

        if ( _isDialogueHidden == true )
        {
            StopRoutines( );
            _view.SetVisibleAnimated( false );
            return;
        }

        _view.SetVisibleAnimated( true );

        if ( _isInitialized && isActiveAndEnabled )
            StartRoutines( );
    }

    /// <summary>
    /// 재활성화할 때 마스코트 반복 동작 재개
    /// </summary>
    void OnEnable ()
    {
        if ( _isInitialized && _isIntroPaused == false )
            StartRoutines( );
    }

    /// <summary>
    /// 비활성화할 때 마스코트 반복 동작 중단
    /// </summary>
    void OnDisable ()
    {
        StopRoutines( );
    }

    /// <summary>
    /// 마스코트 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _isInitialized )
            _view.OnTouched -= Touch;
    }

    /// <summary>
    /// 배회와 팁 표시 반복 작업 시작
    /// </summary>
    void StartRoutines ()
    {
        if ( _moveRoutine == null )
            _moveRoutine = StartCoroutine( MoveRoutine( ) );

        if ( _isTipPaused == false &&
            _tipRoutine == null &&
            _settings.Tips.Count > 0 )
        {
            _tipRoutine = StartCoroutine( TipRoutine( ) );
        }
    }

    /// <summary>
    /// 배회와 팁 표시 반복 작업 중단
    /// </summary>
    void StopRoutines ()
    {
        if ( _moveRoutine != null )
            StopCoroutine( _moveRoutine );

        _moveRoutine = null;
        StopTipRoutine( );
    }

    /// <summary>
    /// 마스코트 팁 반복 작업만 중단
    /// </summary>
    void StopTipRoutine ()
    {
        if ( _tipRoutine != null )
            StopCoroutine( _tipRoutine );

        _tipRoutine = null;
    }

    /// <summary>
    /// 임의 간격으로 마스코트 이동 반복
    /// </summary>
    IEnumerator MoveRoutine ()
    {
        while ( true )
        {
            float interval = Random.Range(
                _settings.MinMoveInterval, _settings.MaxMoveInterval );

            yield return new WaitForSecondsRealtime( interval );

            _view.MoveRandom( _settings.MoveDuration );

            yield return new WaitForSecondsRealtime(
                _settings.MoveDuration );
        }
    }

    /// <summary>
    /// 임의 간격으로 팁 문구 표시 반복
    /// </summary>
    IEnumerator TipRoutine ()
    {
        while ( true )
        {
            float interval = Random.Range(
                _settings.MinTipInterval,
                _settings.MaxTipInterval );

            yield return new WaitForSecondsRealtime( interval );

            int tipIndex = GetRandomTipIndex( );

            _view.ShowTip(
                _settings.Tips [ tipIndex ],
                _settings.TipDisplayDuration );

            yield return new WaitForSecondsRealtime(
                _settings.TipDisplayDuration );
        }
    }

    /// <summary>
    /// 직전 문구와 겹치지 않는 임의 팁 번호 반환
    /// </summary>
    /// <returns>표시할 팁 번호</returns>
    int GetRandomTipIndex ()
    {
        int tipCount = _settings.Tips.Count;
        int tipIndex = Random.Range( 0, tipCount );

        if ( tipCount > 1 &&
            tipIndex == _lastTipIndex )
        {
            tipIndex =
                ( tipIndex + Random.Range( 1, tipCount ) ) % tipCount;
        }

        _lastTipIndex = tipIndex;
        return tipIndex;
    }

    /// <summary>
    /// 마스코트 터치 연출 요청
    /// </summary>
    void Touch ()
    {
        _view.PlayTouchBounce( );
    }
}
