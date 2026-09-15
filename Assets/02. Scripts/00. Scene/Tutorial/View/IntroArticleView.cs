using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 기사 인트로 뷰 - 기사 제목 강조, 본문 글자 출력
/// </summary>
public class IntroArticleView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _headerText;       //기사 제목
    [SerializeField] TMP_Text _bodyText;       //기사 본문

    [Header( "----- 연출 설정 -----" )]
    [SerializeField, Range( 0.1f, 1f )]
    float _headerStartScale = 0.5f;       //제목 등장 시작 크기

    [SerializeField, Min( 0f )]
    float _headerDuration = 0.35f;       //제목 강조 시간

    [SerializeField, Min( 0f )]
    float _letterInterval = 0.03f;       //본문 글자 출력 간격

    RectTransform _headerRect;       //기사 제목 위치
    Vector3 _headerOriginalScale;       //기사 제목 원래 크기
    Tween _headerTween;       //기사 제목 연출
    Coroutine _typingRoutine;       //본문 출력 코루틴
    TMP_Text _typingText;       //현재 글자 출력 대상
    bool _isTyping;       //현재 글자 출력 여부

    /// <summary>
    /// 기사 제목 원래 크기 보관
    /// </summary>
    void Awake ()
    {
        _headerRect = _headerText.rectTransform;
        _headerOriginalScale = _headerRect.localScale;

        ResetView( );
    }

    /// <summary>
    /// 비활성화 시 기사 표시 상태 초기화
    /// </summary>
    void OnDisable ()
    {
        ResetView( );
    }

    /// <summary>
    /// 현재 기사 글자 출력 또는 제목 강조 여부
    /// </summary>
    public bool IsTyping =>
        _headerTween != null || _isTyping;

    /// <summary>
    /// 기사 제목 두둥 연출
    /// </summary>
    /// <param name="header">표시할 기사 제목</param>
    public void ShowHeader ( string header )
    {
        KillHeaderTween( );
        StopTyping( );

        _headerText.text = header;
        _headerText.gameObject.SetActive( true );
        _headerText.maxVisibleCharacters = 0;
        _headerText.ForceMeshUpdate( );

        _headerRect.localScale =
            _headerOriginalScale * _headerStartScale;

        _headerTween = _headerRect
            .DOScale( _headerOriginalScale, _headerDuration )
            .SetUpdate( true )
            .SetEase( Ease.OutBack )
            .OnComplete( CompleteHeader );

        StartTyping( _headerText );
    }

    /// <summary>
    /// 기사 본문 글자 출력 시작
    /// </summary>
    /// <param name="body">표시할 기사 본문</param>
    public void ShowBody ( string body )
    {
        KillHeaderTween( );
        StopTyping( );

        _bodyText.gameObject.SetActive( true );
        _bodyText.text = body;
        _bodyText.maxVisibleCharacters = 0;
        _bodyText.ForceMeshUpdate( );

        StartTyping( _bodyText );
    }

    /// <summary>
    /// 현재 기사 본문 즉시 완성
    /// </summary>
    public void CompleteTyping ()
    {
        if ( _headerTween != null )
            _headerTween.Complete( true );

        StopTyping( );

        if ( _typingText != null )
            _typingText.maxVisibleCharacters = int.MaxValue;

        _typingText = null;
        _isTyping = false;
    }

    /// <summary>
    /// 기사 표시 상태 초기화
    /// </summary>
    public void ResetView ()
    {
        KillHeaderTween( );
        StopTyping( );

        _headerText.text = string.Empty;
        _bodyText.text = string.Empty;
        _headerText.maxVisibleCharacters = 0;
        _bodyText.maxVisibleCharacters = 0;

        _headerRect.localScale = _headerOriginalScale;
        _typingText = null;
        _isTyping = false;
    }

    /// <summary>
    /// 지정된 기사 텍스트의 글자 출력 시작
    /// </summary>
    /// <param name="text">글자를 출력할 텍스트</param>
    void StartTyping ( TMP_Text text )
    {
        _typingText = text;

        int characterCount =
            text.textInfo.characterCount;

        if ( _letterInterval <= 0f || characterCount == 0 )
        {
            text.maxVisibleCharacters = int.MaxValue;
            _typingText = null;
            _isTyping = false;
            return;
        }

        _isTyping = true;
        _typingRoutine =
            StartCoroutine( TypingRoutine( characterCount ) );
    }

    /// <summary>
    /// 기사 본문을 한 글자씩 표시
    /// </summary>
    /// <param name="characterCount">전체 글자 수</param>
    IEnumerator TypingRoutine ( int characterCount )
    {
        WaitForSecondsRealtime interval =
            new WaitForSecondsRealtime( _letterInterval );

        for ( int i = 1; i <= characterCount; i++ )
        {
            _typingText.maxVisibleCharacters = i;
            yield return interval;
        }

        _typingRoutine = null;
        _typingText = null;
        _isTyping = false;
    }

    /// <summary>
    /// 기사 제목 연출 완료 처리
    /// </summary>
    void CompleteHeader ()
    {
        _headerTween = null;
    }

    /// <summary>
    /// 기사 제목 연출 제거
    /// </summary>
    void KillHeaderTween ()
    {
        if ( _headerTween == null ) return;

        _headerTween.Kill( );
        _headerTween = null;
    }

    /// <summary>
    /// 기사 본문 출력 중지
    /// </summary>
    void StopTyping ()
    {
        if ( _typingRoutine == null ) return;

        StopCoroutine( _typingRoutine );
        _typingRoutine = null;
    }
}
