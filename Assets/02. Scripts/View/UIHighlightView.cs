using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 강조 뷰 - 일회성 반짝임, 짧은 크기 강조, 반복 강조 연출
/// </summary>
public class UIHighlightView : MonoBehaviour
{
    RectTransform _target;       //강조할 UI
    CanvasGroup _canvasGroup;       //투명도 변경 대상
    Image _flashImage;       //반짝임 대상 이미지

    [Header( "----- 반짝임 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _flashDuration = 0.18f;       //반짝임이 나타나고 사라지는 시간

    [Header( "----- 크기 강조 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _punchDuration = 0.2f;       //크기 강조 시간
    [SerializeField, Min( 0f )]
    float _punchScale = 0.12f;       //원래 크기에서 추가할 크기 비율

    [Header( "----- 클릭 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _clickDuration = 0.16f;       //버튼 클릭 연출 시간
    [SerializeField, Range( 0f, 1f )]
    float _clickScale = 0.9f;       //버튼이 눌렸을 때 크기 비율

    [Header( "----- 반복 강조 연출 -----" )]
    [SerializeField, Min( 0f )]
    float _loopDuration = 0.5f;       //반복 연출 편도 시간
    [SerializeField] Vector2 _loopOffset = new Vector2( 0f, 10f );       //반복 이동 거리
    [SerializeField, Min( 0f )]
    float _loopScale = 1.08f;       //반복 연출 최대 크기 비율
    [SerializeField, Range( 0f, 1f )]
    float _loopMinAlpha = 0.65f;       //반복 연출 최소 투명도

    Vector2 _shownPosition;       //위치 이동 연출 시작 위치
    Vector3 _shownScale;       //대상의 원래 크기
    float _shownAlpha;       //대상의 원래 투명도
    Color _flashColor;       //반짝임 이미지의 원래 색상

    Tween _highlightTween;       //현재 실행 중인 강조 트윈
    Button _clickButton;       //클릭 연출을 적용할 버튼
    bool _isInitialized;       //컴포넌트 초기화 여부
    bool _movesPosition;       //현재 반복 연출의 위치 이동 여부
    bool _appearLoopMovesPosition;       //등장 이후 위치 반복 포함 여부

    /// <summary>
    /// 강조 대상의 원래 표시 상태 저장
    /// </summary>
    void Awake ()
    {
        Initialize( );
    }

    /// <summary>
    /// 비활성화 시 강조 연출 제거 및 원래 표시 상태 복구
    /// </summary>
    void OnDisable ()
    {
        if ( _isInitialized == false ) return;

        KillTween( );
        RestoreVisual( );
    }

    /// <summary>
    /// 제거 시 버튼 클릭 연출 연결 해제
    /// </summary>
    void OnDestroy ()
    {
        UnbindClick( );
    }

    /// <summary>
    /// 버튼 클릭 눌림과 복귀 연출 연결
    /// </summary>
    /// <param name="button">클릭 연출을 적용할 버튼</param>
    public void BindClick ( Button button )
    {
        Initialize( );

        //같은 버튼의 중복 연결 차단
        if ( _clickButton == button ) return;

        UnbindClick( );

        _clickButton = button;
        _clickButton.onClick.AddListener( PlayClick );
    }

    /// <summary>
    /// 버튼 클릭 연출 연결 해제
    /// </summary>
    public void UnbindClick ()
    {
        if ( _clickButton == null ) return;

        _clickButton.onClick.RemoveListener( PlayClick );
        _clickButton = null;
    }

    /// <summary>
    /// 반짝임 오버레이가 나타났다 사라지는 일회성 연출 재생
    /// </summary>
    public void PlayFlash ()
    {
        Initialize( );

        //이미지가 없는 Text와 정렬 영역은 크기 강조로 대체
        if ( _flashImage == null )
        {
            PlayPunch( );
            return;
        }

        Stop( );

        //반짝임 이미지를 투명한 상태에서 표시
        Color transparentColor = _flashColor;
        transparentColor.a = 0f;

        _flashImage.color = transparentColor;

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        //반짝임 이미지가 나타난 뒤 다시 사라지도록 순차 재생
        sequence.Append(
            _flashImage.DOFade(
                _flashColor.a, _flashDuration ) );

        sequence.Append(
            _flashImage.DOFade(
                0f, _flashDuration ) );

        sequence.OnComplete( CompleteHighlight );

        _highlightTween = sequence;
    }

    /// <summary>
    /// 대상이 짧게 커졌다 돌아오는 일회성 강조 연출 재생
    /// </summary>
    public void PlayPunch ()
    {
        Initialize( );
        Stop( );

        Vector3 punchScale = _shownScale * _punchScale;

        _highlightTween = _target
            .DOPunchScale(
                punchScale, _punchDuration, 4, 0.5f )
            .SetUpdate( true )
            .OnComplete( CompleteHighlight );
    }

    /// <summary>
    /// 작은 크기와 투명한 상태에서 등장한 뒤 반복 강조 재생
    /// </summary>
    /// <param name="movesPosition">등장 이후 위치 이동 포함 여부</param>
    public void PlayAppearLoop ( bool movesPosition = false )
    {
        Initialize( );
        Stop( );

        _appearLoopMovesPosition = movesPosition;

        //아이콘의 최초 등장 상태 설정
        _target.localScale = _shownScale * 0.7f;
        _canvasGroup.alpha = 0f;

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _target.DOScale(
                _shownScale, _punchDuration )
            .SetEase( Ease.OutBack ) );

        sequence.Join(
            _canvasGroup.DOFade(
                _shownAlpha, _punchDuration * 0.6f )
            .SetEase( Ease.OutQuad ) );

        sequence.OnComplete( CompleteAppearAndPlayLoop );

        _highlightTween = sequence;
    }

    /// <summary>
    /// 버튼이 눌렸다가 원래 크기로 튀어 오르는 클릭 연출 재생
    /// </summary>
    public void PlayClick ()
    {
        Initialize( );
        Stop( );

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _target.DOScale(
                _shownScale * _clickScale,
                _clickDuration * 0.4f )
            .SetEase( Ease.OutQuad ) );

        sequence.Append(
            _target.DOScale(
                _shownScale,
                _clickDuration * 0.6f )
            .SetEase( Ease.OutBack ) );

        sequence.OnComplete( CompleteHighlight );

        _highlightTween = sequence;
    }

    /// <summary>
    /// 투명도가 왕복하는 알림 아이콘 깜빡임 연출 재생
    /// </summary>
    public void PlayBlinkLoop ()
    {
        Initialize( );
        Stop( );

        _highlightTween = _canvasGroup
            .DOFade(
                _shownAlpha * _loopMinAlpha, _loopDuration )
            .SetEase( Ease.InOutSine )
            .SetLoops( -1, LoopType.Yoyo )
            .SetUpdate( true );
    }

    /// <summary>
    /// 크기와 투명도가 왕복하는 반복 강조 연출 재생
    /// </summary>
    /// <param name="movesPosition">위치 이동 포함 여부</param>
    public void PlayLoop ( bool movesPosition = false )
    {
        Initialize( );
        Stop( );

        _movesPosition = movesPosition;

        if ( _movesPosition == true )
            _shownPosition = _target.anchoredPosition;

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        //버튼과 알림 아이콘은 위치를 유지하고 크기와 투명도만 반복
        sequence.Append(
            _target.DOScale(
                _shownScale * _loopScale, _loopDuration )
            .SetEase( Ease.InOutSine ) );

        sequence.Join(
            _canvasGroup.DOFade(
                _shownAlpha * _loopMinAlpha, _loopDuration )
            .SetEase( Ease.InOutSine ) );

        //튜토리얼 화살표처럼 이동이 필요한 대상만 위치 반복 추가
        if ( _movesPosition == true )
        {
            sequence.Join(
                _target.DOAnchorPos(
                    _shownPosition + _loopOffset, _loopDuration )
                .SetEase( Ease.InOutSine ) );
        }

        sequence.SetLoops( -1, LoopType.Yoyo );

        _highlightTween = sequence;
    }

    /// <summary>
    /// 현재 강조 연출을 중단하고 원래 표시 상태로 복구
    /// </summary>
    public void Stop ()
    {
        Initialize( );
        KillTween( );

        _appearLoopMovesPosition = false;

        RestoreVisual( );
    }

    /// <summary>
    /// 강조 대상 컴포넌트와 원래 표시 상태 초기화
    /// </summary>
    void Initialize ()
    {
        if ( _isInitialized == true ) return;

        _target = transform as RectTransform;
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>( );
        _flashImage = GetComponent<Image>( );

        _shownScale = _target.localScale;
        _shownAlpha = _canvasGroup.alpha;

        //반짝임용 이미지가 있는 대상만 원래 색상 저장
        if ( _flashImage != null )
            _flashColor = _flashImage.color;

        _isInitialized = true;
    }

    /// <summary>
    /// 일회성 강조 연출 완료 처리
    /// </summary>
    void CompleteHighlight ()
    {
        _highlightTween = null;
        RestoreVisual( );
    }

    /// <summary>
    /// 등장 연출 완료 후 반복 강조 시작
    /// </summary>
    void CompleteAppearAndPlayLoop ()
    {
        bool movesPosition = _appearLoopMovesPosition;

        _highlightTween = null;
        _appearLoopMovesPosition = false;

        RestoreVisual( );
        PlayLoop( movesPosition );
    }

    /// <summary>
    /// 강조 대상의 원래 표시 상태 복구
    /// </summary>
    void RestoreVisual ()
    {
        //씬 종료 중 강조 대상이 먼저 제거된 경우 표시 복구 생략
        if ( _target == null || _canvasGroup == null )
        {
            _movesPosition = false;
            return;
        }

        if ( _movesPosition == true )
            _target.anchoredPosition = _shownPosition;

        _target.localScale = _shownScale;
        _canvasGroup.alpha = _shownAlpha;

        if ( _flashImage != null )
            _flashImage.color = _flashColor;

        _movesPosition = false;
    }

    /// <summary>
    /// 현재 실행 중인 강조 트윈 제거
    /// </summary>
    void KillTween ()
    {
        if ( _highlightTween == null ) return;

        _highlightTween.Kill( );
        _highlightTween = null;
    }
}
