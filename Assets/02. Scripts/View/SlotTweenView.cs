using DG.Tweening;
using UnityEngine;

/// <summary>
/// 공용 슬롯 등장, 선택, 재사용 초기화 연출
/// </summary>
public class SlotTweenView : MonoBehaviour
{
    [Header( "----- 등장 연출 -----" )]
    [SerializeField, Range( 0f, 1f )]
    float _appearScale = 0.85f;       //등장 시작 크기 비율
    [SerializeField, Min( 0f )]
    float _appearDuration = 0.18f;       //등장 연출 시간

    [Header( "----- 선택 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _selectScale = 0.08f;       //선택 시 추가 크기 비율
    [SerializeField, Min( 0f )]
    float _selectDuration = 0.16f;       //선택 강조 시간

    RectTransform _target;       //연출 대상
    CanvasGroup _canvasGroup;       //투명도 연출 대상
    Vector3 _baseScale;       //기본 크기
    float _baseAlpha = 1f;       //상태 표시를 포함한 기본 투명도
    Tween _tween;       //현재 실행 중인 슬롯 연출
    bool _isInitialized;       //초기화 여부
    bool _hasAppeared;       //인스턴스 최초 등장 연출 완료 여부

    /// <summary>
    /// 슬롯 기본 표시 상태 저장
    /// </summary>
    void Awake ()
    {
        Initialize( );
    }

    /// <summary>
    /// 비활성화 시 실행 중인 연출과 표시 상태 초기화
    /// </summary>
    void OnDisable ()
    {
        if ( _isInitialized == false ) return;

        ResetInstant( );
    }

    /// <summary>
    /// 슬롯의 상태 표시 투명도 저장
    /// </summary>
    /// <param name="alpha">잠금과 품절 상태가 반영된 투명도</param>
    public void SetBaseAlpha ( float alpha )
    {
        Initialize( );
        KillTween( );

        _baseAlpha = alpha;
        RestoreVisual( );
    }

    /// <summary>
    /// 작은 크기와 투명한 상태에서 슬롯 등장
    /// </summary>
    public void PlayAppear ()
    {
        PrepareTween( );

        _target.localScale = _baseScale * _appearScale;
        _canvasGroup.alpha = 0f;

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _target.DOScale(
                _baseScale, _appearDuration )
            .SetEase( Ease.OutBack ) );

        sequence.Join(
            _canvasGroup.DOFade(
                _baseAlpha, _appearDuration * 0.7f )
            .SetEase( Ease.OutQuad ) );

        sequence.OnComplete( CompleteTween );

        _tween = sequence;
    }

    /// <summary>
    /// 풀 인스턴스가 처음 생성된 경우에만 등장 연출 재생
    /// </summary>
    public void PlayAppearOnce ()
    {
        if ( _hasAppeared ) return;

        _hasAppeared = true;
        PlayAppear( );
    }

    /// <summary>
    /// 선택한 슬롯을 짧게 확대 강조
    /// </summary>
    public void PlaySelected ()
    {
        PrepareTween( );

        _tween = _target
            .DOPunchScale(
                _baseScale * _selectScale,
                _selectDuration, 4, 0.5f )
            .SetUpdate( true )
            .OnComplete( CompleteTween );
    }

    /// <summary>
    /// 선택 해제 또는 재사용 전에 표시 상태 즉시 복구
    /// </summary>
    public void ResetInstant ()
    {
        Initialize( );
        KillTween( );
        RestoreVisual( );
    }

    /// <summary>
    /// 연출 컴포넌트와 기본 상태 초기화
    /// </summary>
    void Initialize ()
    {
        if ( _isInitialized == true ) return;

        _target = transform as RectTransform;
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>( );

        _baseScale = _target.localScale;
        _baseAlpha = _canvasGroup.alpha;
        _isInitialized = true;
    }

    /// <summary>
    /// 신규 연출 시작 전 이전 상태 정리
    /// </summary>
    void PrepareTween ()
    {
        Initialize( );
        KillTween( );
        RestoreVisual( );
    }

    /// <summary>
    /// 슬롯 연출 완료 처리
    /// </summary>
    void CompleteTween ()
    {
        _tween = null;
        RestoreVisual( );
    }

    /// <summary>
    /// 슬롯의 기본 크기와 투명도 복구
    /// </summary>
    void RestoreVisual ()
    {
        if ( _target != null )
            _target.localScale = _baseScale;

        if ( _canvasGroup != null )
            _canvasGroup.alpha = _baseAlpha;
    }

    /// <summary>
    /// 실행 중인 슬롯 연출 제거
    /// </summary>
    void KillTween ()
    {
        if ( _tween == null ) return;

        _tween.Kill( );
        _tween = null;
    }
}
