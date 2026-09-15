using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 이미지 비율 뷰 - 최초 표시 영역 안에서 Sprite 원본 비율 적용
/// </summary>
[RequireComponent( typeof( Image ) )]
public class UIImageAspectView : MonoBehaviour
{
    Image _image;       //비율을 적용할 이미지
    RectTransform _rectTransform;       //이미지 표시 영역
    Vector2 _maxSize;       //최초 설정된 최대 표시 크기
    bool _isInitialized;       //최대 표시 크기 저장 여부

    /// <summary>
    /// 최초 이미지 표시 영역 저장
    /// </summary>
    void Awake ()
    {
        InitRuntime( );
    }

    /// <summary>
    /// 비활성 상태에서도 이미지 표시 영역 초기화
    /// </summary>
    public void InitRuntime ()
    {
        if ( _isInitialized ) return;

        _image = GetComponent<Image>( );
        _rectTransform = _image.rectTransform;
        _maxSize = _rectTransform.rect.size;

        _isInitialized = true;
    }

    /// <summary>
    /// Sprite를 적용하고 최초 영역 안에서 원본 비율로 크기 조정
    /// </summary>
    /// <param name="sprite">표시할 Sprite</param>
    public void SetSprite ( Sprite sprite )
    {
        InitRuntime( );

        _image.sprite = sprite;

        if ( sprite == null )
        {
            ApplySize( _maxSize );
            return;
        }

        Vector2 spriteSize = sprite.rect.size;
        float fitScale = Mathf.Min(
            _maxSize.x / spriteSize.x,
            _maxSize.y / spriteSize.y );

        ApplySize( spriteSize * fitScale );
    }

    /// <summary>
    /// 이미지 RectTransform 크기 적용
    /// </summary>
    /// <param name="size">적용할 가로세로 크기</param>
    void ApplySize ( Vector2 size )
    {
        _rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, size.x );

        _rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, size.y );
    }
}
