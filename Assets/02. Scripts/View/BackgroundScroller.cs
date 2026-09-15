using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배경 스크롤러 - 반복 텍스처의 UV 또는 화면 배경 위치를 이동
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    /// <summary>
    /// 배경 이동 방식
    /// </summary>
    enum BackgroundScrollMode
    {
        UvRepeat,       //반복 텍스처 UV 이동
        RectPingPong,       //화면 오브젝트 왕복 이동
        UvDirectional       //반복 텍스처 방향 이동
    }

    [Header( "----- 배경 -----" )]
    [SerializeField] RawImage _background;       //이동할 배경
    [SerializeField] BackgroundScrollMode _scrollMode;       //배경 이동 방식

    [Header( "----- UV 반복 이동 -----" )]
    [SerializeField, Min( 0f )]
    float _speed = 0.02f;       //초당 UV 이동량

    [SerializeField]
    Vector2 _uvMoveDirection = Vector2.one;       //UV 이동 방향

    [Header( "----- 오브젝트 왕복 이동 -----" )]
    [SerializeField]
    Vector2 _moveDirection = new Vector2( 1f, -0.35f );       //왕복 이동 방향

    [SerializeField, Min( 0f )]
    float _moveDistance = 30f;       //원래 위치 기준 최대 이동 거리

    [SerializeField, Min( 0.01f )]
    float _moveCycleDuration = 8f;       //한 번 왕복하는 시간

    RectTransform _rectTransform;       //이동할 배경 RectTransform
    Rect _startUvRect;       //배경 시작 UV 영역
    Vector2 _startPosition;       //배경 시작 위치
    float _offsetX;       //현재 가로 UV 이동량
    float _offsetY;       //현재 세로 UV 이동량
    float _moveElapsedTime;       //현재 왕복 이동 시간

    /// <summary>
    /// 배경의 시작 UV 영역과 위치 저장
    /// </summary>
    void Awake ()
    {
        _rectTransform = _background.rectTransform;
        _startUvRect = _background.uvRect;
        _startPosition = _rectTransform.anchoredPosition;
    }

    /// <summary>
    /// 활성화할 때 배경을 처음 위치로 복구
    /// </summary>
    void OnEnable ()
    {
        ResetPosition( );
    }

    /// <summary>
    /// 선택한 방식으로 배경 이동
    /// </summary>
    void Update ()
    {
        switch ( _scrollMode )
        {
            case BackgroundScrollMode.UvRepeat:
                UpdateUvRepeat( );
                break;

            case BackgroundScrollMode.RectPingPong:
                UpdateRectPingPong( );
                break;

            case BackgroundScrollMode.UvDirectional:
                UpdateUvDirectional( );
                break;
        }
    }

    /// <summary>
    /// 반복 텍스처의 UV를 가로 방향으로 이동
    /// </summary>
    void UpdateUvRepeat ()
    {
        //Mirror는 두 타일마다 원래 방향으로 돌아오므로 2를 반복 주기로 사용
        _offsetX = Mathf.Repeat(
            _offsetX + _speed * Time.unscaledDeltaTime,
            2f );

        Rect uvRect = _startUvRect;
        uvRect.x += _offsetX;
        _background.uvRect = uvRect;
    }

    /// <summary>
    /// 반복 텍스처의 UV를 지정 방향으로 이동
    /// </summary>
    void UpdateUvDirectional ()
    {
        Vector2 direction = _uvMoveDirection.normalized;

        _offsetX = Mathf.Repeat(
            _offsetX + direction.x * _speed * Time.unscaledDeltaTime,
            2f );

        _offsetY = Mathf.Repeat(
            _offsetY + direction.y * _speed * Time.unscaledDeltaTime,
            2f );

        Rect uvRect = _startUvRect;
        uvRect.position += new Vector2( _offsetX, _offsetY );
        _background.uvRect = uvRect;
    }

    /// <summary>
    /// 배경 오브젝트를 지정 방향으로 천천히 왕복 이동
    /// </summary>
    void UpdateRectPingPong ()
    {
        _moveElapsedTime += Time.unscaledDeltaTime;

        float cycle =
            _moveElapsedTime / _moveCycleDuration *
            Mathf.PI * 2f;

        float distance =
            Mathf.Sin( cycle ) * _moveDistance;

        _rectTransform.anchoredPosition =
            _startPosition +
            _moveDirection.normalized * distance;
    }

    /// <summary>
    /// 배경 UV와 화면 위치를 시작 상태로 복구
    /// </summary>
    void ResetPosition ()
    {
        _offsetX = 0f;
        _offsetY = 0f;
        _moveElapsedTime = 0f;

        _background.uvRect = _startUvRect;
        _rectTransform.anchoredPosition = _startPosition;
    }
}
