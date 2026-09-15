using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 공용 대화 뷰 - 대사 출력과 캐릭터 이미지 표시 및 입력 전달
/// </summary>
public class DialogueView : MonoBehaviour, IDialogueOutput
{
    const string PlayerSpeakerName = "Player";

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //대화 패널 연출
    [SerializeField] TMP_Text _speakerNameText;       //화자 이름 텍스트
    [SerializeField] TMP_Text _speechText;       //대사 본문 텍스트
    [SerializeField] Image _leftPortrait;       //왼쪽 캐릭터 이미지
    [SerializeField] Image _rightPortrait;       //오른쪽 캐릭터 이미지
    [SerializeField] Button _nextButton;       //다음 대사 버튼
    [SerializeField] Button _skipButton;       //전체 건너뛰기 버튼

    [Header( "----- 출력 설정 -----" )]
    [SerializeField, Min( 0f )]
    float _letterInterval = 0.03f;       //한 글자 출력 간격

    [SerializeField, Range( 0f, 1f )]
    float _playerPortraitBrightness = 0.65f;       //플레이어 대사 중 상대 초상화 밝기

    Coroutine _typingRoutine;       //현재 실행 중인 글자 출력 코루틴
    Canvas _rootCanvas;       //튜토리얼 안내보다 위에 표시할 루트 Canvas
    GameObject _speakerNameRoot;       //화자 이름 프레임
    Color _leftPortraitColor;       //왼쪽 초상화 기본 색상
    Color _rightPortraitColor;       //오른쪽 초상화 기본 색상
    bool _isTyping;       //현재 문장 출력 여부
    bool _isInitialized;       //런타임 초기화 여부

    /// <summary>
    /// 다음 대사 입력 이벤트
    /// </summary>
    public event Action OnNext;

    /// <summary>
    /// 대화 건너뛰기 입력 이벤트
    /// </summary>
    public event Action OnSkip;

    /// <summary>
    /// 현재 문장 출력 여부
    /// </summary>
    public bool IsTyping => _isTyping;

    #region ----- 초기화/이벤트 -----

    /// <summary>
    /// 대화 뷰 런타임 초기화
    /// </summary>
    void Awake ()
    {
        InitRuntime( );
    }

    /// <summary>
    /// 대화 뷰 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _isInitialized == false ) return;

        _nextButton.onClick.RemoveListener( SelectNext );
        _skipButton.onClick.RemoveListener( SelectSkip );
    }

    /// <summary>
    /// 비활성화 시 글자 출력 상태 초기화
    /// </summary>
    void OnDisable ()
    {
        StopTyping( );
        _isTyping = false;
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 런타임 의존성 초기화
    /// </summary>
    public void InitRuntime ()
    {
        if ( _isInitialized ) return;

        _rootCanvas = GetComponentInParent<Canvas>( true )?.rootCanvas;
        _speakerNameRoot = _speakerNameText.transform.parent.gameObject;
        _leftPortraitColor = _leftPortrait.color;
        _rightPortraitColor = _rightPortrait.color;

        _nextButton.onClick.AddListener( SelectNext );
        _skipButton.onClick.AddListener( SelectSkip );

        _isInitialized = true;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 대화 패널 표시
    /// </summary>
    /// <param name="canSkip">전체 건너뛰기 허용 여부</param>
    public void Show ( bool canSkip )
    {
        InitRuntime( );

        //즉시 숨김 이후에도 대화 뷰 전체를 다시 표시
        gameObject.SetActive( true );

        _skipButton.gameObject.SetActive( canSkip );
        UpdatePortrait( null, DialoguePortraitSide.None );
        _panelTween.Show( );
    }

    /// <summary>
    /// 현재 대화 줄 표시
    /// </summary>
    /// <param name="line">표시할 대화 줄</param>
    public void DisplayLine ( DialogueLine line )
    {
        _speakerNameText.text = line.SpeakerName;

        bool isPlayer = string.Equals(
            line.SpeakerName, PlayerSpeakerName,
            StringComparison.OrdinalIgnoreCase );

        _speakerNameRoot.SetActive( isPlayer == false );
        SetPortraitDimmed( isPlayer );

        //플레이어 대사에는 같은 대화의 직전 NPC 초상화를 유지
        if ( isPlayer == false )
            UpdatePortrait( line.Portrait, line.PortraitSide );

        StartTyping( line.Content );
    }

    /// <summary>
    /// 대화 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void Hide ( Action onComplete = null )
    {
        StopTyping( );
        _isTyping = false;

        _panelTween.Hide(
            ( ) => CompleteHide( onComplete ) );
    }

    /// <summary>
    /// 대화 패널 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        StopTyping( );
        _isTyping = false;

        _panelTween.SetVisible( false );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 대화 패널 퇴장 후 대화 뷰 전체 비활성화
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive( false );
        onComplete?.Invoke( );
    }

    /// <summary>
    /// 캐릭터 이미지 표시 갱신
    /// </summary>
    /// <param name="portrait">표시할 캐릭터 이미지</param>
    /// <param name="side">이미지 표시 위치</param>
    void UpdatePortrait (
        Sprite portrait, DialoguePortraitSide side )
    {
        bool showLeft =
            portrait != null &&
            side == DialoguePortraitSide.Left;

        bool showRight =
            portrait != null &&
            side == DialoguePortraitSide.Right;

        _leftPortrait.gameObject.SetActive( showLeft );
        _rightPortrait.gameObject.SetActive( showRight );

        if ( showLeft )
            _leftPortrait.SetIconSprite( portrait );

        if ( showRight )
            _rightPortrait.SetIconSprite( portrait );
    }

    /// <summary>
    /// 플레이어 대사 중 상대 캐릭터 초상화 밝기 조절
    /// </summary>
    /// <param name="isDimmed">어둡게 표시할지 여부</param>
    void SetPortraitDimmed ( bool isDimmed )
    {
        float brightness = isDimmed
            ? _playerPortraitBrightness
            : 1f;

        _leftPortrait.color = new Color(
            _leftPortraitColor.r * brightness,
            _leftPortraitColor.g * brightness,
            _leftPortraitColor.b * brightness,
            _leftPortraitColor.a );

        _rightPortrait.color = new Color(
            _rightPortraitColor.r * brightness,
            _rightPortraitColor.g * brightness,
            _rightPortraitColor.b * brightness,
            _rightPortraitColor.a );
    }

    #endregion

    #region ----- 글자 출력 -----

    /// <summary>
    /// 현재 문장 글자별 출력 시작
    /// </summary>
    /// <param name="speech">출력할 대사 본문</param>
    void StartTyping ( string speech )
    {
        StopTyping( );

        _speechText.text = speech;
        _speechText.maxVisibleCharacters = 0;
        _speechText.ForceMeshUpdate( );

        int characterCount =
            _speechText.textInfo.characterCount;

        if ( _letterInterval <= 0f || characterCount == 0 )
        {
            CompleteTyping( );
            return;
        }

        _isTyping = true;
        _typingRoutine = StartCoroutine( TypingRoutine( characterCount ) );
    }

    /// <summary>
    /// 대사 본문을 한 글자씩 표시
    /// </summary>
    /// <param name="characterCount">전체 표시 글자 수</param>
    IEnumerator TypingRoutine ( int characterCount )
    {
        WaitForSecondsRealtime interval =
            new WaitForSecondsRealtime( _letterInterval );

        for ( int i = 1; i <= characterCount; i++ )
        {
            _speechText.maxVisibleCharacters = i;
            yield return interval;
        }

        _typingRoutine = null;
        _isTyping = false;
    }

    /// <summary>
    /// 현재 문장을 즉시 전부 표시
    /// </summary>
    public void CompleteTyping ()
    {
        StopTyping( );

        _speechText.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;
    }

    /// <summary>
    /// 실행 중인 글자 출력 코루틴 정리
    /// </summary>
    void StopTyping ()
    {
        if ( _typingRoutine == null ) return;

        StopCoroutine( _typingRoutine );
        _typingRoutine = null;
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 다음 대사 입력 전달
    /// </summary>
    void SelectNext ()
    {
        OnNext?.Invoke( );
    }

    /// <summary>
    /// 대화 건너뛰기 입력 전달
    /// </summary>
    void SelectSkip ()
    {
        OnSkip?.Invoke( );
    }

    #endregion
}
