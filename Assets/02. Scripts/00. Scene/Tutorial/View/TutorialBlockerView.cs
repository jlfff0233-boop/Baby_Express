using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼 블록커 뷰 - 강조 대상 내부를 제외한 화면 입력 차단
/// </summary>
[RequireComponent( typeof( Image ) )]
public class TutorialBlockerView : MonoBehaviour, ICanvasRaycastFilter
{
    RectTransform _rectTransform;       //전체 입력 차단 영역
    RectTransform _target;       //현재 입력 허용 대상
    Vector2 _padding;       //대상 주변 입력 허용 여백
    bool _isFullScreenBlocking;       //허용 대상 없는 전체 입력 차단 여부

    /// <summary>
    /// 블록커 영역 초기화
    /// </summary>
    void Awake ()
    {
        _rectTransform = transform as RectTransform;
    }

    /// <summary>
    /// 강조 대상 외 입력 차단 시작
    /// </summary>
    /// <param name="target">입력을 허용할 대상</param>
    /// <param name="padding">대상 주변 입력 허용 여백</param>
    public void Show ( RectTransform target, Vector2 padding )
    {
        _target = target;
        _padding = padding;
        _isFullScreenBlocking = false;

        gameObject.SetActive( true );
    }

    /// <summary>
    /// 허용 대상 없이 화면 전체 입력 차단 시작
    /// </summary>
    public void ShowAll ( )
    {
        _target = null;
        _padding = Vector2.zero;
        _isFullScreenBlocking = true;

        gameObject.SetActive( true );
    }

    /// <summary>
    /// 입력 차단 종료
    /// </summary>
    public void Hide ()
    {
        _target = null;
        _isFullScreenBlocking = false;
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 현재 화면 위치가 블록커 입력을 받을지 판정
    /// </summary>
    /// <param name="screenPoint">현재 화면 입력 위치</param>
    /// <param name="eventCamera">UI 입력 판정 카메라</param>
    /// <returns>블록커 입력 수신 여부</returns>
    public bool IsRaycastLocationValid (
        Vector2 screenPoint, Camera eventCamera )
    {
        if ( _isFullScreenBlocking == true ) return true;
        if ( _target == null ) return false;

        if ( RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform, screenPoint, eventCamera,
            out Vector2 localPoint ) == false )
        {
            return true;
        }

        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                _rectTransform, _target );

        Rect allowedRect = new Rect(
            targetBounds.min.x - _padding.x,
            targetBounds.min.y - _padding.y,
            targetBounds.size.x + _padding.x * 2f,
            targetBounds.size.y + _padding.y * 2f );

        return allowedRect.Contains( localPoint ) == false;
    }
}
