using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인트로 장면 종류
/// </summary>
public enum IntroSceneType
{
    None,       //표시 장면 없음
    Stork,       //황새 장면
    Article,       //기사 장면
    Sns,       //SNS 장면
}

/// <summary>
/// 기사 대사 표시 위치
/// </summary>
public enum IntroArticleLineType
{
    Header,       //기사 제목
    Body,       //기사 본문
}

/// <summary>
/// SNS 대사 표시 종류
/// </summary>
public enum IntroSnsLineType
{
    Normal,       //일반 문장
    Final,       //마지막 강조 문장
}

/// <summary>
/// 인트로 뷰 - 인트로 장면 표시와 대사별 전용 연출 담당
/// </summary>
public class IntroView : MonoBehaviour, IDialogueOutput
{
    [Header( "----- 공용 컴포넌트 -----" )]
    [SerializeField] CanvasGroup _canvasGroup;       //인트로 전체 표시 그룹
    [SerializeField] RectTransform _sceneViewport;       //장면 전환 기준 영역
    [SerializeField] Button _nextButton;       //다음 대사 입력 버튼
    [SerializeField] Button _skipButton;       //전체 건너뛰기 버튼
    [SerializeField] TMP_Text _clickToSkipText;       //클릭 진행 안내

    [Header( "----- 인트로 장면 -----" )]
    [SerializeField] RectTransform _storkScene;       //황새 장면 루트
    [SerializeField] RectTransform _articleScene;       //기사 장면 루트
    [SerializeField] RectTransform _snsScene;       //SNS 장면 루트

    [Header( "----- 장면별 뷰 -----" )]
    [SerializeField] IntroStorkView _storkView;       //황새 연출 뷰
    [SerializeField] IntroArticleView _articleView;       //기사 연출 뷰
    [SerializeField] IntroSnsView _snsView;       //SNS 연출 뷰

    [Header( "----- 장면 전환 설정 -----" )]
    [SerializeField, Min( 0f )]
    float _transitionDuration = 0.3f;       //장면 이동 시간

    [SerializeField, Range( 0f, 1f )]
    float _clickGuideMinAlpha = 0.3f;       //클릭 안내 최소 투명도

    [SerializeField, Min( 0f )]
    float _clickGuideBlinkDuration = 0.6f;       //클릭 안내 점멸 시간

    IntroSceneType _currentSceneType;       //현재 표시 장면 종류
    IntroArticleLineType _articleLineType;       //현재 기사 대사 위치
    IntroSnsLineType _snsLineType;       //현재 SNS 대사 종류
    RectTransform _currentScene;       //현재 표시 장면 루트
    Tween _sceneTween;       //현재 장면 이동 연출
    Tween _clickGuideTween;       //클릭 진행 안내 점멸 연출
    Action _pendingSceneAction;       //장면 입장 후 실행할 대사 연출
    DialogueLine _pendingFirstStorkLine;       //첫 클릭까지 대기할 황새 대사
    bool _hasShownStorkLine;       //황새 대사 표시 시작 여부
    bool _isInitialized;       //런타임 입력 초기화 여부

    /// <summary>
    /// 다음 대사 입력 이벤트
    /// </summary>
    public event Action OnNext;

    /// <summary>
    /// 전체 건너뛰기 입력 이벤트
    /// </summary>
    public event Action OnSkip;

    /// <summary>
    /// 현재 문장 또는 장면 연출 진행 여부
    /// </summary>
    public bool IsTyping =>
        _sceneTween != null ||
        ( _currentSceneType == IntroSceneType.Stork &&
            _storkView.IsTyping ) ||
        ( _currentSceneType == IntroSceneType.Article &&
            _articleView.IsTyping ) ||
        ( _currentSceneType == IntroSceneType.Sns &&
            _snsView.IsAnimating );

    #region ----- 장면 조회/초기화 -----

    /// <summary>
    /// 장면 종류에 대응하는 장면 루트 반환
    /// </summary>
    /// <param name="sceneType">조회할 장면 종류</param>
    /// <returns>장면 루트</returns>
    RectTransform GetScene ( IntroSceneType sceneType )
    {
        switch ( sceneType )
        {
            case IntroSceneType.Stork:
                return _storkScene;

            case IntroSceneType.Article:
                return _articleScene;

            case IntroSceneType.Sns:
                return _snsScene;
        }

        return null;
    }

    /// <summary>
    /// 장면 전환에 사용할 화면 너비 반환
    /// </summary>
    /// <returns>현재 장면 영역 너비</returns>
    float GetSceneWidth ()
    {
        return _sceneViewport.rect.width;
    }

    /// <summary>
    /// 인트로 장면 표시 상태 초기화
    /// </summary>
    void ResetScenes ()
    {
        _storkScene.anchoredPosition = Vector2.zero;
        _articleScene.anchoredPosition = Vector2.zero;
        _snsScene.anchoredPosition = Vector2.zero;

        _storkScene.gameObject.SetActive( false );
        _articleScene.gameObject.SetActive( false );
        _snsScene.gameObject.SetActive( false );

        _currentSceneType = IntroSceneType.None;
        _articleLineType = IntroArticleLineType.Header;
        _snsLineType = IntroSnsLineType.Normal;
        _currentScene = null;
        _pendingSceneAction = null;
        _pendingFirstStorkLine = null;
        _hasShownStorkLine = false;
    }

    /// <summary>
    /// 실행 중인 장면 이동 연출 제거
    /// </summary>
    void KillSceneTween ()
    {
        if ( _sceneTween == null ) return;

        _sceneTween.Kill( );
        _sceneTween = null;
        _pendingSceneAction = null;
    }

    /// <summary>
    /// 최초 인트로 화면과 입력 초기화
    /// </summary>
    void Awake ()
    {
        InitRuntime( );
        HideInstant( );
    }

    /// <summary>
    /// 비활성화 시 실행 중인 장면 연출 제거
    /// </summary>
    void OnDisable ()
    {
        KillSceneTween( );
        KillClickGuideTween( );
    }

    /// <summary>
    /// 인트로 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _isInitialized == false ) return;

        _nextButton.onClick.RemoveListener( SelectNext );
        _skipButton.onClick.RemoveListener( SelectSkip );
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 런타임 입력 초기화
    /// </summary>
    public void InitRuntime ()
    {
        if ( _isInitialized ) return;

        _nextButton.onClick.AddListener( SelectNext );
        _skipButton.onClick.AddListener( SelectSkip );

        _isInitialized = true;
    }

    #endregion

    #region ----- 장면/대사 표시 -----

    /// <summary>
    /// 지정된 인트로 장면으로 전환
    /// </summary>
    /// <param name="sceneType">표시할 장면 종류</param>
    public void ShowScene ( IntroSceneType sceneType )
    {
        if ( sceneType == IntroSceneType.None ||
            sceneType == _currentSceneType )
        {
            return;
        }

        KillSceneTween( );

        RectTransform previousScene = _currentScene;
        RectTransform nextScene = GetScene( sceneType );
        float sceneWidth = GetSceneWidth( );

        _currentSceneType = sceneType;
        _currentScene = nextScene;

        nextScene.gameObject.SetActive( true );
        nextScene.anchoredPosition =
            new Vector2( sceneWidth, 0f );

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        if ( previousScene != null )
        {
            sequence.Join(
                previousScene
                    .DOAnchorPosX( -sceneWidth, _transitionDuration )
                    .SetEase( Ease.InCubic ) );
        }

        sequence.Join(
            nextScene
                .DOAnchorPosX( 0f, _transitionDuration )
                .SetEase( Ease.OutCubic ) );

        _sceneTween = sequence
            .OnComplete(
                () => CompleteSceneChange( previousScene ) );
    }

    /// <summary>
    /// 기사 대사의 다음 표시 위치 설정
    /// </summary>
    /// <param name="lineType">기사 대사 표시 위치</param>
    public void SetArticleLineType (
        IntroArticleLineType lineType )
    {
        _articleLineType = lineType;
    }

    /// <summary>
    /// SNS 대사의 다음 표시 종류 설정
    /// </summary>
    /// <param name="lineType">SNS 대사 표시 종류</param>
    public void SetSnsLineType ( IntroSnsLineType lineType )
    {
        _snsLineType = lineType;
    }

    /// <summary>
    /// SNS 본문 영역 확대
    /// </summary>
    public void ZoomInSns ()
    {
        if ( _sceneTween != null )
        {
            _pendingSceneAction += _snsView.ZoomIn;
            return;
        }

        _snsView.ZoomIn( );
    }

    /// <summary>
    /// SNS 본문 영역을 오른쪽 위치로 이동
    /// </summary>
    public void MoveSnsRight ()
    {
        _snsView.MoveRight( );
    }

    /// <summary>
    /// 대화 한 줄을 현재 인트로 장면에 표시
    /// </summary>
    /// <param name="line">표시할 대화 줄</param>
    public void DisplayLine ( DialogueLine line )
    {
        //장면이 도착한 뒤 첫 문장의 글자 출력 연출 시작
        if ( _sceneTween != null )
        {
            _pendingSceneAction +=
                () => DisplayLine( line );
            return;
        }

        switch ( _currentSceneType )
        {
            case IntroSceneType.Stork:
                if ( _hasShownStorkLine == false )
                {
                    _pendingFirstStorkLine = line;
                    return;
                }

                _storkView.DisplayLine( line );
                break;

            case IntroSceneType.Article:
                DisplayArticleLine( line.Content );
                break;

            case IntroSceneType.Sns:
                DisplaySnsLine( line.Content );
                break;
        }
    }

    /// <summary>
    /// 기사 대사를 현재 표시 위치에 적용
    /// </summary>
    /// <param name="content">표시할 기사 대사</param>
    void DisplayArticleLine ( string content )
    {
        if ( _articleLineType == IntroArticleLineType.Header )
        {
            _articleView.ShowHeader( content );
            return;
        }

        _articleView.ShowBody( content );
    }

    /// <summary>
    /// SNS 대사를 추가하고 필요한 강조 연출 실행
    /// </summary>
    /// <param name="content">추가할 SNS 대사</param>
    void DisplaySnsLine ( string content )
    {
        _snsView.AddContent( content );

        if ( _snsLineType != IntroSnsLineType.Final )
            return;

        _snsView.EmphasizeFinal( );
        _snsLineType = IntroSnsLineType.Normal;
    }

    /// <summary>
    /// 장면 전환 완료 처리
    /// </summary>
    /// <param name="previousScene">퇴장한 이전 장면</param>
    void CompleteSceneChange ( RectTransform previousScene )
    {
        _sceneTween = null;

        if ( previousScene != null )
            previousScene.gameObject.SetActive( false );

        Action pendingAction = _pendingSceneAction;
        _pendingSceneAction = null;
        pendingAction?.Invoke( );
    }

    #endregion

    #region ----- 대화 출력 규격 -----

    /// <summary>
    /// 인트로 화면 표시
    /// </summary>
    /// <param name="canSkip">전체 건너뛰기 허용 여부</param>
    public void Show ( bool canSkip )
    {
        InitRuntime( );
        KillSceneTween( );

        gameObject.SetActive( true );
        ResetScenes( );

        _skipButton.gameObject.SetActive( canSkip );

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        StartClickGuideTween( );
    }

    /// <summary>
    /// 현재 문장 또는 장면 연출 즉시 완성
    /// </summary>
    public void CompleteTyping ()
    {
        if ( _sceneTween != null )
        {
            _sceneTween.Complete( true );
            TryShowFirstStorkLine( );
            return;
        }

        if ( _currentSceneType == IntroSceneType.Stork &&
            _storkView.IsTyping )
        {
            _storkView.CompleteTyping( );
        }

        if ( _currentSceneType == IntroSceneType.Article &&
            _articleView.IsTyping )
        {
            _articleView.CompleteTyping( );
        }

        if ( _currentSceneType == IntroSceneType.Sns &&
            _snsView.IsAnimating )
        {
            _snsView.CompleteAnimation( );
        }
    }

    /// <summary>
    /// 현재 인트로 장면을 밀어낸 뒤 화면 숨김
    /// </summary>
    /// <param name="onComplete">숨김 완료 후 실행할 함수</param>
    public void Hide ( Action onComplete = null )
    {
        KillSceneTween( );
        KillClickGuideTween( );

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        if ( _currentScene == null )
        {
            CompleteHide( onComplete );
            return;
        }

        float sceneWidth = GetSceneWidth( );

        _sceneTween = _currentScene
            .DOAnchorPosX( -sceneWidth, _transitionDuration )
            .SetUpdate( true )
            .SetEase( Ease.InCubic )
            .OnComplete( () => CompleteHide( onComplete ) );
    }

    /// <summary>
    /// 인트로 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        KillSceneTween( );
        KillClickGuideTween( );
        ResetScenes( );

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        gameObject.SetActive( false );
    }

    /// <summary>
    /// 인트로 화면 숨김 완료 처리
    /// </summary>
    /// <param name="onComplete">숨김 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        _sceneTween = null;
        ResetScenes( );

        _canvasGroup.alpha = 0f;
        gameObject.SetActive( false );

        onComplete?.Invoke( );
    }

    /// <summary>
    /// 클릭 진행 안내 점멸 시작
    /// </summary>
    void StartClickGuideTween ()
    {
        KillClickGuideTween( );

        Color color = _clickToSkipText.color;
        color.a = 1f;
        _clickToSkipText.color = color;

        _clickGuideTween = _clickToSkipText
            .DOFade(
                _clickGuideMinAlpha,
                _clickGuideBlinkDuration )
            .SetUpdate( true )
            .SetEase( Ease.InOutSine )
            .SetLoops( -1, LoopType.Yoyo );
    }

    /// <summary>
    /// 클릭 진행 안내 점멸 제거
    /// </summary>
    void KillClickGuideTween ()
    {
        if ( _clickGuideTween == null ) return;

        _clickGuideTween.Kill( );
        _clickGuideTween = null;
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 다음 대사 입력 전달
    /// </summary>
    void SelectNext ()
    {
        //첫 입력은 자동 생성 대신 첫 황새 말풍선 표시에 사용
        if ( TryShowFirstStorkLine( ) ) return;

        OnNext?.Invoke( );
    }

    /// <summary>
    /// 대기 중인 첫 황새 말풍선을 현재 입력으로 표시
    /// </summary>
    /// <returns>첫 말풍선 표시 여부</returns>
    bool TryShowFirstStorkLine ()
    {
        if ( _pendingFirstStorkLine == null )
            return false;

        DialogueLine firstLine =
            _pendingFirstStorkLine;

        _pendingFirstStorkLine = null;
        _hasShownStorkLine = true;

        _storkView.DisplayLine( firstLine );
        return true;
    }

    /// <summary>
    /// 전체 건너뛰기 입력 전달
    /// </summary>
    void SelectSkip ()
    {
        OnSkip?.Invoke( );
    }

    #endregion
}
