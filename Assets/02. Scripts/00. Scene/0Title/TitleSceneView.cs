using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 씬 뷰 - 타이틀 메인 버튼 입력과 화면 연출
/// </summary>
public class TitleSceneView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //타이틀 메인 패널 연출
    [SerializeField] Button _startButton;       //시작 버튼
    [SerializeField] Button _loadButton;        //불러오기 버튼
    [SerializeField] Button _settingsButton;        //설정 버튼
    [SerializeField] Button _quitButton;        //나가기 버튼

    [Header( "----- 타이틀 연출 -----" )]
    [SerializeField] RectTransform _titleLogo;       //타이틀 로고

    [SerializeField, Min( 0f )]
    float _logoShowDuration = 0.45f;       //로고 등장 시간

    [SerializeField]
    float _logoStartOffsetY = 80f;       //로고 등장 시작 높이

    [SerializeField, Range( 0f, 1f )]
    float _logoStartScale = 0.85f;       //로고 등장 시작 크기

    [SerializeField, Min( 0f )]
    float _logoFloatDistance = 10f;       //로고 반복 이동 거리

    [SerializeField, Min( 0f )]
    float _logoFloatDuration = 1.6f;       //로고 편도 이동 시간

    [Header( "----- 버튼 연출 -----" )]
    [SerializeField]
    float _buttonStartOffsetX = 80f;       //버튼 등장 시작 가로 위치

    [SerializeField, Min( 0f )]
    float _buttonShowDuration = 0.3f;       //버튼 등장 시간

    [SerializeField, Min( 0f )]
    float _buttonShowInterval = 0.08f;       //버튼 사이 등장 간격

    RectTransform[] _menuButtonRects;       //순차 등장할 메뉴 버튼
    Vector2[] _menuButtonPositions;       //메뉴 버튼 원래 위치
    Vector2 _titleLogoPosition;       //타이틀 로고 원래 위치
    Vector3 _titleLogoScale;       //타이틀 로고 원래 크기
    Sequence _showSequence;       //최초 등장 연출
    Sequence _logoFloatSequence;       //로고 반복 이동 연출
    bool _isAnimationInitialized;       //연출 기준값 저장 여부
    bool _hasPlayedShowAnimation;       //최초 등장 연출 재생 여부

    /// <summary>
    /// 시작 버튼 클릭 이벤트
    /// </summary>
    public event Action OnStartClicked;
    /// <summary>
    /// 불러오기 버튼 클릭 이벤트
    /// </summary>
    public event Action OnLoadClicked;
    /// <summary>
    /// 설정 버튼 클릭 이벤트
    /// </summary>
    public event Action OnSettingsClicked;
    /// <summary>
    /// 나가기 버튼 클릭 이벤트
    /// </summary>
    public event Action OnQuitClicked;

    /// <summary>
    /// 타이틀 연출에 사용할 원래 위치와 크기 저장
    /// </summary>
    void InitializeAnimation ()
    {
        if ( _isAnimationInitialized ) return;

        _menuButtonRects = new RectTransform[]
        {
            _startButton.transform as RectTransform,
            _loadButton.transform as RectTransform,
            _settingsButton.transform as RectTransform,
            _quitButton.transform as RectTransform
        };

        _menuButtonPositions =
            new Vector2[ _menuButtonRects.Length ];

        for ( int i = 0; i < _menuButtonRects.Length; i++ )
        {
            _menuButtonPositions[ i ] =
                _menuButtonRects[ i ].anchoredPosition;
        }

        _titleLogoPosition = _titleLogo.anchoredPosition;
        _titleLogoScale = _titleLogo.localScale;
        _isAnimationInitialized = true;
    }

    /// <summary>
    /// 실행 중인 타이틀 연출 제거
    /// </summary>
    void KillAnimation ()
    {
        _showSequence?.Kill( );
        _logoFloatSequence?.Kill( );

        _showSequence = null;
        _logoFloatSequence = null;
    }

    /// <summary>
    /// 로고와 버튼을 원래 상태로 복구
    /// </summary>
    void ResetAnimationState ()
    {
        if ( _isAnimationInitialized == false ) return;

        _titleLogo.anchoredPosition = _titleLogoPosition;
        _titleLogo.localScale = _titleLogoScale;

        for ( int i = 0; i < _menuButtonRects.Length; i++ )
        {
            _menuButtonRects[ i ].anchoredPosition =
                _menuButtonPositions[ i ];
        }
    }

    /// <summary>
    /// 타이틀 로고의 잔잔한 반복 이동 재생
    /// </summary>
    void StartLogoFloatAnimation ()
    {
        _logoFloatSequence?.Kill( );

        _titleLogo.anchoredPosition =
            _titleLogoPosition;

        _logoFloatSequence = DOTween.Sequence( )
            .Append(
                _titleLogo
                    .DOAnchorPosY(
                        _titleLogoPosition.y +
                        _logoFloatDistance,
                        _logoFloatDuration )
                    .SetEase( Ease.InOutSine ) )
            .Append(
                _titleLogo
                    .DOAnchorPosY(
                        _titleLogoPosition.y,
                        _logoFloatDuration )
                    .SetEase( Ease.InOutSine ) )
            .SetLoops( -1 )
            .SetUpdate( true );
    }

    /// <summary>
    /// 최초 등장 완료 후 로고 반복 이동 시작
    /// </summary>
    void CompleteShowAnimation ()
    {
        _showSequence = null;
        StartLogoFloatAnimation( );
    }

    /// <summary>
    /// 타이틀 최초 등장 연출 재생
    /// </summary>
    void PlayShowAnimation ()
    {
        KillAnimation( );
        ResetAnimationState( );

        _hasPlayedShowAnimation = true;

        //로고를 위쪽의 작은 크기에서 시작
        _titleLogo.anchoredPosition =
            _titleLogoPosition +
            Vector2.up * _logoStartOffsetY;

        _titleLogo.localScale =
            _titleLogoScale * _logoStartScale;

        //버튼을 오른쪽 바깥에서 시작
        for ( int i = 0; i < _menuButtonRects.Length; i++ )
        {
            _menuButtonRects[ i ].anchoredPosition =
                _menuButtonPositions[ i ] +
                Vector2.right * _buttonStartOffsetX;
        }

        _showSequence = DOTween.Sequence( )
            .SetUpdate( true );

        //로고가 아래로 내려오면서 원래 크기로 복구
        _showSequence.Insert(
            0f,
            _titleLogo
                .DOAnchorPos( _titleLogoPosition, _logoShowDuration )
                .SetEase( Ease.OutBack ) );

        _showSequence.Insert(
            0f,
            _titleLogo
                .DOScale( _titleLogoScale, _logoShowDuration )
                .SetEase( Ease.OutBack ) );

        //메뉴 버튼을 위에서부터 순서대로 표시
        for ( int i = 0; i < _menuButtonRects.Length; i++ )
        {
            float delay =
                0.12f + _buttonShowInterval * i;

            _showSequence.Insert(
                delay,
                _menuButtonRects[ i ]
                    .DOAnchorPos(
                        _menuButtonPositions[ i ],
                        _buttonShowDuration )
                    .SetEase( Ease.OutCubic ) );
        }

        _showSequence.OnComplete( CompleteShowAnimation );
    }

    /// <summary>
    /// 타이틀 연출 기준값 초기화
    /// </summary>
    void Awake ()
    {
        InitializeAnimation( );
    }

    /// <summary>
    /// 타이틀 메인 버튼 입력 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        _startButton.onClick.AddListener( ClickStart );
        _loadButton.onClick.AddListener( ClickLoad );
        _settingsButton.onClick.AddListener( ClickSettings );
        _quitButton.onClick.AddListener( ClickQuit );
    }

    /// <summary>
    /// 타이틀 메인 버튼 입력 이벤트와 실행 중인 연출 정리
    /// </summary>
    void OnDisable ()
    {
        _startButton.onClick.RemoveListener( ClickStart );
        _loadButton.onClick.RemoveListener( ClickLoad );
        _settingsButton.onClick.RemoveListener( ClickSettings );
        _quitButton.onClick.RemoveListener( ClickQuit );

        KillAnimation( );
        ResetAnimationState( );
    }

    /// <summary>
    /// 타이틀 메인 패널 표시
    /// </summary>
    public void Show ( )
    {
        InitializeAnimation( );

        //전체 패널은 확대하지 않고 정상 크기로 즉시 표시
        _panelTween.SetVisible( true );

        if ( _hasPlayedShowAnimation )
        {
            ResetAnimationState( );
            StartLogoFloatAnimation( );
            return;
        }

        PlayShowAnimation( );
    }

    /// <summary>
    /// 타이틀 메인 패널 숨김
    /// </summary>
    public void Hide ( )
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 타이틀 메인 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 타이틀 메인 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 시작 버튼 클릭 이벤트 발행
    /// </summary>
    void ClickStart ()
    {
        OnStartClicked?.Invoke( );
    }

    /// <summary>
    /// 불러오기 버튼 클릭 이벤트 발행
    /// </summary>
    void ClickLoad ()
    {
        OnLoadClicked?.Invoke( );
    }

    /// <summary>
    /// 설정 버튼 클릭 이벤트 발행
    /// </summary>
    void ClickSettings ()
    {
        OnSettingsClicked?.Invoke( );
    }

    /// <summary>
    /// 나가기 버튼 클릭 이벤트 발행
    /// </summary>
    void ClickQuit ()
    {
        OnQuitClicked?.Invoke( );
    }
}
