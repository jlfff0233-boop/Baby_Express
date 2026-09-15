using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 툴팁 뷰 - 설명 문구 표시
/// </summary>
public class TooltipView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _text;       //툴팁 설명

    [Header( "----- 화면 경계 -----" )]
    [SerializeField] float _edgePadding = 10f;      //화면 경계 여백

    [Header( "----- 등장과 퇴장 연출 -----" )]
    [SerializeField, Range( 0f, 1f )]
    float _appearScale = 0.9f;       //등장 시작 크기
    [SerializeField, Range( 0f, 1f )]
    float _hideScale = 0.95f;       //퇴장 완료 크기
    [SerializeField, Min( 0f )]
    float _showDuration = 0.15f;       //등장 연출 시간
    [SerializeField, Min( 0f )]
    float _hideDuration = 0.1f;       //퇴장 연출 시간

    RectTransform _rectTransform;      //툴팁 RectTransform
    RectTransform _canvasRect;     //최상위 Canvas RectTransform
    CanvasGroup _canvasGroup;       //투명도와 입력 상태
    Vector3 [ ] _tooltipCorners = new Vector3 [ 4 ];        //툴팁 월드 좌표 모서리
    Vector3 [ ] _canvasCorners = new Vector3 [ 4 ];     //Canvas 월드 좌표 모서리
    Vector3 _shownScale;       //툴팁 기본 크기
    Tween _displayTween;       //현재 표시 연출
    bool _isInitialized;       //런타임 초기화 여부

    /// <summary>
    /// 툴팁 표시 컴포넌트와 기본 상태 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        if ( _isInitialized ) return;

        _rectTransform = GetComponent<RectTransform>( );
        Canvas canvas = GetComponentInParent<Canvas>( true );
        _canvasRect =
            canvas.rootCanvas.GetComponent<RectTransform>( );

        _canvasGroup =
            gameObject.GetOrAddComponent<CanvasGroup>( );

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _shownScale = _rectTransform.localScale;
        _isInitialized = true;
    }

    /// <summary>
    /// 실행 중인 툴팁 표시 연출 제거
    /// </summary>
    void KillDisplayTween ()
    {
        if ( _displayTween == null ) return;

        _displayTween.Kill( );
        _displayTween = null;
    }

    /// <summary>
    /// 툴팁 기본 표시 상태 복구
    /// </summary>
    void RestoreVisual ()
    {
        _rectTransform.localScale = _shownScale;
        _canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 툴팁 설명과 레이아웃 표시 준비
    /// </summary>
    /// <param name="description">표시할 설명</param>
    void PrepareShow ( string description )
    {
        InitializeRuntime( );
        KillDisplayTween( );

        gameObject.SetActive( true );
        RestoreVisual( );

        _text.text = description;

        //위치 보정 전에 실제 문구와 레이아웃 크기 갱신
        _text.ForceMeshUpdate( );
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            _rectTransform );
        Canvas.ForceUpdateCanvases( );
    }

    /// <summary>
    /// 툴팁 등장 완료 처리
    /// </summary>
    void CompleteShow ()
    {
        _displayTween = null;
        RestoreVisual( );
    }

    /// <summary>
    /// 툴팁 등장 연출 재생
    /// </summary>
    void PlayShow ()
    {
        _rectTransform.localScale =
            _shownScale * _appearScale;
        _canvasGroup.alpha = 0f;

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _rectTransform
                .DOScale( _shownScale, _showDuration )
                .SetEase( Ease.OutBack ) );

        sequence.Join(
            _canvasGroup
                .DOFade( 1f, _showDuration )
                .SetEase( Ease.OutQuad ) );

        sequence.OnComplete( CompleteShow );

        _displayTween = sequence;
    }

    /// <summary>
    /// 툴팁 런타임 상태 초기화
    /// </summary>
    void Awake ()
    {
        InitializeRuntime( );
    }

    /// <summary>
    /// 비활성화 시 실행 중인 연출과 표시 상태 초기화
    /// </summary>
    void OnDisable ()
    {
        if ( _isInitialized == false ) return;

        KillDisplayTween( );
        RestoreVisual( );
    }

    /// <summary>
    /// 툴팁 표시
    /// </summary>
    /// <param name="description">표시할 설명</param>
    public void Show ( string description )
    {
        PrepareShow( description );
        PlayShow( );
    }

    /// <summary>
    /// 지정 위치에 툴팁 표시
    /// </summary>
    /// <param name="description">표시할 설명</param>
    /// <param name="position">툴팁 표시 월드 좌표</param>
    public void Show ( string description, Vector3 position )
    {
        PrepareShow( description );

        //레이아웃 크기 계산 후 요청 위치와 화면 경계 보정
        transform.position = position;

        KeepInsideCanvas( );
        PlayShow( );
    }

    /// <summary>
    /// 지정 위치에 툴팁 표시
    /// </summary>
    /// <param name="description">표시할 설명</param>
    /// <param name="tooltipPoint">툴팁 표시 위치</param>
    public void Show ( string description, RectTransform tooltipPoint )
    {
        Show( description, tooltipPoint.position );
    }

    /// <summary>
    /// 툴팁을 Canvas 화면 경계 안으로 이동
    /// </summary>
    void KeepInsideCanvas ()
    {
        //툴팁과 Canvas 모서리 좌표 가져오기
        _rectTransform.GetWorldCorners( _tooltipCorners );
        _canvasRect.GetWorldCorners( _canvasCorners );

        //Canvas 기준 로컬 좌표로 변환
        Vector3 tooltipBottomLeft = _canvasRect.InverseTransformPoint( _tooltipCorners [ 0 ] );
        Vector3 tooltipTopRight = _canvasRect.InverseTransformPoint( _tooltipCorners [ 2 ] );
        Vector3 canvasBottomLeft = _canvasRect.InverseTransformPoint( _canvasCorners [ 0 ] );
        Vector3 canvasTopRight = _canvasRect.InverseTransformPoint( _canvasCorners [ 2 ] );
        Vector2 offset = Vector2.zero;

        //좌우 화면 경계 보정
        if ( tooltipBottomLeft.x < canvasBottomLeft.x + _edgePadding )
            offset.x = canvasBottomLeft.x + _edgePadding - tooltipBottomLeft.x;
        else if ( tooltipTopRight.x > canvasTopRight.x - _edgePadding )
            offset.x = canvasTopRight.x - _edgePadding - tooltipTopRight.x;

        //상하 화면 경계 보정
        if ( tooltipBottomLeft.y < canvasBottomLeft.y + _edgePadding )
            offset.y = canvasBottomLeft.y + _edgePadding - tooltipBottomLeft.y;
        else if ( tooltipTopRight.y > canvasTopRight.y - _edgePadding )
            offset.y = canvasTopRight.y - _edgePadding - tooltipTopRight.y;

        //Canvas 로컬 보정값을 월드 위치에 적용
        transform.position += _canvasRect.TransformVector( offset );
    }

    /// <summary>
    /// 툴팁 퇴장 완료 처리
    /// </summary>
    void CompleteHide ()
    {
        _displayTween = null;
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 툴팁을 페이드 아웃하며 숨김
    /// </summary>
    public void Hide ()
    {
        InitializeRuntime( );

        if ( gameObject.activeSelf == false )
            return;

        KillDisplayTween( );

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _rectTransform
                .DOScale(
                    _shownScale * _hideScale,
                    _hideDuration )
                .SetEase( Ease.InQuad ) );

        sequence.Join(
            _canvasGroup
                .DOFade( 0f, _hideDuration )
                .SetEase( Ease.InQuad ) );

        sequence.OnComplete( CompleteHide );

        _displayTween = sequence;
    }
}
