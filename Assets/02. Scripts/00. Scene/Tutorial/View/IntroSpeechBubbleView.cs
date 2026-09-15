using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인트로 말풍선 뷰 - 황새 대사 표시, 이동 연출
/// </summary>
public class IntroSpeechBubbleView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] CanvasGroup _canvasGroup;       //말풍선 표시 그룹
    [SerializeField] TMP_Text _speechText;       //말풍선 대사

    [Header( "----- 연출 설정 -----" )]
    [SerializeField, Range( 0.1f, 1f )]
    float _startScale = 0.7f;       //등장 시작 크기

    [SerializeField, Min( 0f )]
    float _showDuration = 0.2f;       //등장 시간

    [SerializeField, Min( 0f )]
    float _showMoveY = 40f;       //등장 시작 아래쪽 거리

    [SerializeField, Min( 0f )]
    float _letterInterval = 0.03f;       //대사 글자 출력 간격

    RectTransform _rectTransform;       //말풍선 위치
    Vector3 _originalScale;       //말풍선 원래 크기
    Vector2 _showPosition;       //말풍선 등장 완료 위치
    Sequence _showSequence;       //등장 연출
    Tween _moveTween;       //이동 연출
    Coroutine _typingRoutine;       //대사 출력 코루틴
    bool _isTyping;       //대사 출력 여부

    /// <summary>
    /// 현재 말풍선 등장 또는 대사 출력 여부
    /// </summary>
    public bool IsTyping =>
        _showSequence != null || _isTyping;

    /// <summary>
    /// 최초 크기와 위치 컴포넌트 보관
    /// </summary>
    void Awake ()
    {
        _rectTransform = ( RectTransform ) transform;
        _originalScale = _rectTransform.localScale;

        //말풍선 뒤의 인트로 진행 버튼이 입력을 받도록 표시 입력 차단
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _speechText.raycastTarget = false;
    }

    /// <summary>
    /// 비활성화 시 실행 중인 연출 제거
    /// </summary>
    void OnDisable ()
    {
        KillTweens( );
    }

    /// <summary>
    /// 말풍선 대사 표시
    /// </summary>
    /// <param name="speech">표시할 대사</param>
    public void Show ( string speech )
    {
        KillTweens( );

        _speechText.text = speech;
        _speechText.maxVisibleCharacters = 0;
        _speechText.ForceMeshUpdate( );

        //텍스트 크기가 반영된 말풍선 위치에서 등장 연출 시작
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            _rectTransform );

        _canvasGroup.alpha = 0f;
        _showPosition = _rectTransform.anchoredPosition;
        _rectTransform.anchoredPosition =
            _showPosition + Vector2.down * _showMoveY;
        _rectTransform.localScale =
            _originalScale * _startScale;

        _showSequence = DOTween.Sequence( )
            .SetUpdate( true )
            .Join( _canvasGroup.DOFade( 1f, _showDuration ) )
            .Join(
                _rectTransform
                    .DOAnchorPos( _showPosition, _showDuration )
                    .SetEase( Ease.OutCubic ) )
            .Join(
                _rectTransform
                    .DOScale( _originalScale, _showDuration )
                    .SetEase( Ease.OutBack ) )
            .OnComplete( CompleteShow );

        GameManager.Instance.AudioManager.PlaySFX(
            SFXType.Panel );

        StartTyping( );
    }

    /// <summary>
    /// 현재 말풍선 등장과 대사 출력을 즉시 완성
    /// </summary>
    public void CompleteTyping ()
    {
        if ( _showSequence != null )
            _showSequence.Complete( true );

        StopTyping( );

        _speechText.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;
    }

    /// <summary>
    /// 기존 말풍선을 위로 이동
    /// </summary>
    /// <param name="distance">위로 이동할 거리</param>
    /// <param name="duration">이동 시간</param>
    public void MoveUp ( float distance, float duration )
    {
        if ( _moveTween != null )
        {
            _moveTween.Kill( );
            _moveTween = null;
        }

        float targetY =
            _rectTransform.anchoredPosition.y + distance;

        _moveTween = _rectTransform
            .DOAnchorPosY( targetY, duration )
            .SetUpdate( true )
            .SetEase( Ease.OutCubic )
            .OnComplete( CompleteMove );
    }

    /// <summary>
    /// 등장 연출 완료 처리
    /// </summary>
    void CompleteShow ()
    {
        _showSequence = null;
    }

    /// <summary>
    /// 이동 연출 완료 처리
    /// </summary>
    void CompleteMove ()
    {
        _moveTween = null;
    }

    /// <summary>
    /// 말풍선 대사 글자 출력 시작
    /// </summary>
    void StartTyping ()
    {
        int characterCount =
            _speechText.textInfo.characterCount;

        if ( _letterInterval <= 0f || characterCount == 0 )
        {
            _speechText.maxVisibleCharacters = int.MaxValue;
            _isTyping = false;
            return;
        }

        _isTyping = true;
        _typingRoutine =
            StartCoroutine( TypingRoutine( characterCount ) );
    }

    /// <summary>
    /// 말풍선 대사를 한 글자씩 표시
    /// </summary>
    /// <param name="characterCount">전체 글자 수</param>
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
    /// 말풍선 대사 출력 중지
    /// </summary>
    void StopTyping ()
    {
        if ( _typingRoutine == null ) return;

        StopCoroutine( _typingRoutine );
        _typingRoutine = null;
    }

    /// <summary>
    /// 실행 중인 말풍선 연출 제거
    /// </summary>
    void KillTweens ()
    {
        StopTyping( );
        _isTyping = false;

        if ( _showSequence != null )
        {
            _showSequence.Kill( );
            _showSequence = null;
        }

        if ( _moveTween != null )
        {
            _moveTween.Kill( );
            _moveTween = null;
        }
    }
}
