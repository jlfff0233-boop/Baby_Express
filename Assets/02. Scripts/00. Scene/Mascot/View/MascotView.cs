using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인 화면 마스코트 뷰 - 배회와 팁 및 터치 연출 표시
/// </summary>
public class MascotView : MonoBehaviour
{
    [Header( "----- 이동 -----" )]
    [SerializeField] RectTransform _moveArea;       //마스코트 이동 가능 영역
    [SerializeField] RectTransform _moveRoot;       //위치를 변경할 마스코트 루트
    [SerializeField] RectTransform _visualRoot;       //터치 연출을 적용할 이미지 루트
    [SerializeField] RectTransform _renderer;       //이동 방향에 따라 반전할 이미지 렌더러

    [Header( "----- 팁 -----" )]
    [SerializeField] CanvasGroup _tipGroup;       //팁 말풍선 표시 그룹
    [SerializeField] TMP_Text _tipText;       //팁 문구

    [Header( "----- 입력 -----" )]
    [SerializeField] Button _touchButton;       //마스코트 터치 버튼

    [Header( "----- 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _tipFadeDuration = 0.15f;       //팁 페이드 시간

    [SerializeField, Min( 0f )]
    float _touchBounceScale = 0.15f;       //터치 시 추가 크기

    [SerializeField, Min( 0f )]
    float _touchBounceDuration = 0.35f;       //터치 연출 시간

    [SerializeField, Min( 0f )]
    float _visibilityDuration = 0.2f;       //대화 중 등장과 퇴장 시간

    Vector2 _startPosition;       //마스코트 시작 위치
    Vector3 _visualScale;       //마스코트 기본 크기
    Vector3 _rendererScale;       //마스코트 렌더러 기본 크기

    Tween _moveTween;       //현재 이동 연출
    Tween _tipTween;       //현재 팁 연출
    Tween _touchTween;       //현재 터치 연출
    Tween _visibilityTween;       //현재 등장 또는 퇴장 연출

    /// <summary>
    /// 마스코트 터치 이벤트
    /// </summary>
    public event Action OnTouched;

    /// <summary>
    /// 마스코트 전체 표시 상태 변경
    /// </summary>
    /// <param name="isVisible">표시 여부</param>
    public void SetVisible ( bool isVisible )
    {
        _visibilityTween?.Kill( );
        _visibilityTween = null;

        gameObject.SetActive( isVisible );

        if ( isVisible )
            _visualRoot.localScale = _visualScale;
    }

    /// <summary>
    /// 대화 상태에 맞춰 마스코트를 뿅 연출로 표시하거나 숨김
    /// </summary>
    /// <param name="isVisible">표시 여부</param>
    public void SetVisibleAnimated ( bool isVisible )
    {
        _visibilityTween?.Kill( );
        _visibilityTween = null;

        if ( isVisible )
        {
            gameObject.SetActive( true );
            _visualRoot.localScale = Vector3.zero;

            _visibilityTween = _visualRoot
                .DOScale( _visualScale, _visibilityDuration )
                .SetEase( Ease.OutBack )
                .SetUpdate( true )
                .OnComplete( CompleteShow );
            return;
        }

        if ( gameObject.activeSelf == false )
            return;

        HideTip( );
        _visualRoot.localScale = _visualScale;

        _visibilityTween = _visualRoot
            .DOScale( Vector3.zero, _visibilityDuration )
            .SetEase( Ease.InBack )
            .SetUpdate( true )
            .OnComplete( CompleteHide );
    }

    /// <summary>
    /// 마스코트 표시 상태와 입력 초기화
    /// </summary>
    void Awake ()
    {
        _startPosition = _moveRoot.anchoredPosition;
        _visualScale = _visualRoot.localScale;
        _rendererScale = _renderer.localScale;

        _tipGroup.alpha = 0f;
        _tipGroup.interactable = false;
        _tipGroup.blocksRaycasts = false;

        _touchButton.onClick.AddListener( Touch );
    }

    /// <summary>
    /// 비활성화할 때 실행 중인 연출과 표시 상태 복구
    /// </summary>
    void OnDisable ()
    {
        KillTweens( );

        _moveRoot.anchoredPosition = _startPosition;
        _visualRoot.localScale = _visualScale;
        _renderer.localScale = _rendererScale;
        _tipGroup.alpha = 0f;
    }

    /// <summary>
    /// 마스코트 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        _touchButton.onClick.RemoveListener( Touch );
    }

    /// <summary>
    /// 이동 영역 안의 임의 위치로 마스코트 이동
    /// </summary>
    /// <param name="duration">이동 시간</param>
    public void MoveRandom ( float duration )
    {
        _moveTween?.Kill( );

        Vector2 destination =
            CreateRandomPosition( );

        UpdateFacing( destination.x );

        _moveTween = _moveRoot
            .DOAnchorPos( destination, duration )
            .SetEase( Ease.InOutSine )
            .SetUpdate( true )
            .OnComplete( CompleteMove );
    }

    /// <summary>
    /// 지정 문구를 말풍선에 일정 시간 표시
    /// </summary>
    /// <param name="tip">표시할 팁 문구</param>
    /// <param name="displayDuration">팁 유지 시간</param>
    public void ShowTip (
        string tip, float displayDuration )
    {
        _tipTween?.Kill( );

        _tipText.text = tip;
        _tipGroup.alpha = 0f;

        _tipTween = DOTween.Sequence( )
            .SetUpdate( true )
            .Append(
                _tipGroup.DOFade( 1f, _tipFadeDuration ) )
            .AppendInterval( displayDuration )
            .Append(
                _tipGroup.DOFade( 0f, _tipFadeDuration ) )
            .OnComplete( CompleteTip );
    }

    /// <summary>
    /// 현재 마스코트 팁 즉시 숨김
    /// </summary>
    public void HideTip ()
    {
        _tipTween?.Kill( );
        _tipTween = null;
        _tipGroup.alpha = 0f;
    }

    /// <summary>
    /// 터치에 반응해 마스코트가 통통 튀는 연출 재생
    /// </summary>
    public void PlayTouchBounce ()
    {
        _touchTween?.Kill( );

        _visualRoot.localScale = _visualScale;

        _touchTween = _visualRoot
            .DOPunchScale(
                Vector3.one * _touchBounceScale,
                _touchBounceDuration,
                6, 0.7f )
            .SetUpdate( true )
            .OnComplete( CompleteTouch );
    }

    /// <summary>
    /// 이동 영역과 마스코트 크기를 반영한 임의 위치 생성
    /// </summary>
    /// <returns>마스코트가 이동할 위치</returns>
    Vector2 CreateRandomPosition ()
    {
        Rect areaRect = _moveArea.rect;
        Rect mascotRect = _moveRoot.rect;
        Vector2 pivot = _moveRoot.pivot;

        float minX =
            areaRect.xMin + mascotRect.width * pivot.x;
        float maxX =
            areaRect.xMax - mascotRect.width * ( 1f - pivot.x );

        float minY =
            areaRect.yMin + mascotRect.height * pivot.y;
        float maxY =
            areaRect.yMax - mascotRect.height * ( 1f - pivot.y );

        return new Vector2(
            UnityEngine.Random.Range( minX, maxX ),
            UnityEngine.Random.Range( minY, maxY ) );
    }

    /// <summary>
    /// 이동 목적지 방향에 맞춰 마스코트 렌더러 좌우 반전
    /// </summary>
    /// <param name="destinationX">이동 목적지 X 좌표</param>
    void UpdateFacing ( float destinationX )
    {
        float moveDelta =
            destinationX - _moveRoot.anchoredPosition.x;

        if ( Mathf.Approximately( moveDelta, 0f ) )
            return;

        Vector3 scale = _rendererScale;
        scale.x *= moveDelta > 0f ? 1f : -1f;

        _renderer.localScale = scale;
    }

    /// <summary>
    /// 마스코트 이동 연출 완료 처리
    /// </summary>
    void CompleteMove ()
    {
        _moveTween = null;
    }

    /// <summary>
    /// 팁 표시 연출 완료 처리
    /// </summary>
    void CompleteTip ()
    {
        _tipTween = null;
        _tipGroup.alpha = 0f;
    }

    /// <summary>
    /// 터치 연출 완료 처리
    /// </summary>
    void CompleteTouch ()
    {
        _touchTween = null;
        _visualRoot.localScale = _visualScale;
    }

    /// <summary>
    /// 마스코트 등장 연출 완료 처리
    /// </summary>
    void CompleteShow ()
    {
        _visibilityTween = null;
        _visualRoot.localScale = _visualScale;
    }

    /// <summary>
    /// 마스코트 퇴장 연출 완료 처리
    /// </summary>
    void CompleteHide ()
    {
        _visibilityTween = null;
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 마스코트 터치 이벤트 전달
    /// </summary>
    void Touch ()
    {
        OnTouched?.Invoke( );
    }

    /// <summary>
    /// 실행 중인 마스코트 연출 제거
    /// </summary>
    void KillTweens ()
    {
        _moveTween?.Kill( );
        _tipTween?.Kill( );
        _touchTween?.Kill( );
        _visibilityTween?.Kill( );

        _moveTween = null;
        _tipTween = null;
        _touchTween = null;
        _visibilityTween = null;
    }
}
