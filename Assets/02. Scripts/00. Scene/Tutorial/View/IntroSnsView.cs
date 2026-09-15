using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// SNS 인트로 뷰 - 본문 확대, 이동, 문장 강조
/// </summary>
public class IntroSnsView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _contentText;       //SNS 본문

    [Header( "----- 확대와 이동 -----" )]
    [SerializeField, Min( 1f )]
    float _zoomScale = 1.5f;       //확대 배율

    [SerializeField]
    Vector2 _rightFocusPosition;       //오른쪽 이동 완료 위치

    [SerializeField, Min( 0f )]
    float _moveDuration = 0.45f;       //확대와 이동 시간

    [SerializeField, Min( 0f )]
    float _letterInterval = 0.03f;       //본문 글자 출력 간격

    [Header( "----- 마지막 강조 -----" )]
    [SerializeField, Min( 0f )]
    float _finalPunchScale = 0.15f;       //마지막 문장 강조 크기

    [SerializeField, Min( 0f )]
    float _finalPunchDuration = 0.3f;       //마지막 문장 강조 시간

    RectTransform _sceneRect;       //SNS 장면 화면
    Vector2 _originalPosition;       //화면 원래 위치
    Vector3 _originalScale;       //화면 원래 크기
    Vector2 _originalPivot;       //화면 원래 중심점
    Tween _motionTween;       //확대 또는 이동 연출
    Tween _finalTween;       //마지막 문장 강조 연출
    Coroutine _typingRoutine;       //본문 출력 코루틴
    bool _isTyping;       //본문 출력 여부
    bool _emphasizeAfterTyping;       //본문 출력 후 마지막 강조 여부

    /// <summary>
    /// SNS 본문 원래 위치와 크기 보관
    /// </summary>
    void Awake ()
    {
        _sceneRect = ( RectTransform ) transform;
        _originalPosition = _sceneRect.anchoredPosition;
        _originalScale = _sceneRect.localScale;
        _originalPivot = _sceneRect.pivot;

        ResetView( );
    }

    /// <summary>
    /// 비활성화 시 SNS 표시 상태 초기화
    /// </summary>
    void OnDisable ()
    {
        ResetView( );
    }

    /// <summary>
    /// 현재 확대 또는 이동 연출 여부
    /// </summary>
    public bool IsAnimating =>
        _motionTween != null ||
        _finalTween != null ||
        _isTyping;

    /// <summary>
    /// SNS 문장 추가 표시
    /// </summary>
    /// <param name="content">추가할 문장</param>
    public void AddContent ( string content )
    {
        StopTyping( );

        _contentText.maxVisibleCharacters = int.MaxValue;
        _contentText.ForceMeshUpdate( );

        int visibleCharacterCount =
            _contentText.textInfo.characterCount;

        if ( string.IsNullOrEmpty( _contentText.text ) )
            _contentText.text = content;
        else
            _contentText.text += $"\n{content}";

        _contentText.ForceMeshUpdate( );

        int characterCount =
            _contentText.textInfo.characterCount;

        if ( _letterInterval <= 0f ||
            characterCount <= visibleCharacterCount )
        {
            _contentText.maxVisibleCharacters = int.MaxValue;
            _isTyping = false;
            return;
        }

        _contentText.maxVisibleCharacters =
            visibleCharacterCount;

        _isTyping = true;
        _typingRoutine = StartCoroutine(
            TypingRoutine(
                visibleCharacterCount + 1,
                characterCount ) );
    }

    /// <summary>
    /// SNS 본문 영역 확대
    /// </summary>
    public void ZoomIn ()
    {
        KillMotionTween( );
        SetFocusPivot( );

        _motionTween = _sceneRect
            .DOScale(
                _originalScale * _zoomScale, _moveDuration )
            .SetUpdate( true )
            .SetEase( Ease.OutCubic )
            .OnComplete( CompleteMotion );
    }

    /// <summary>
    /// 확대된 SNS 본문을 오른쪽 위치로 이동
    /// </summary>
    public void MoveRight ()
    {
        KillMotionTween( );

        _motionTween = _sceneRect
            .DOAnchorPos(
                _rightFocusPosition, _moveDuration )
            .SetUpdate( true )
            .SetEase( Ease.InOutCubic )
            .OnComplete( CompleteMotion );
    }

    /// <summary>
    /// 마지막 문장 두둥 강조
    /// </summary>
    public void EmphasizeFinal ()
    {
        if ( _isTyping )
        {
            _emphasizeAfterTyping = true;
            return;
        }

        StartFinalTween( );
    }

    /// <summary>
    /// 현재 SNS 연출 즉시 완성
    /// </summary>
    public void CompleteAnimation ()
    {
        if ( _motionTween != null )
            _motionTween.Complete( true );

        CompleteTyping( );

        if ( _finalTween != null )
            _finalTween.Complete( true );
    }

    /// <summary>
    /// SNS 표시 상태 초기화
    /// </summary>
    public void ResetView ()
    {
        KillMotionTween( );
        KillFinalTween( );
        StopTyping( );

        _sceneRect.pivot = _originalPivot;
        _sceneRect.anchoredPosition = _originalPosition;
        _sceneRect.localScale = _originalScale;

        _contentText.text = string.Empty;
        _contentText.maxVisibleCharacters = 0;

        _isTyping = false;
        _emphasizeAfterTyping = false;
    }

    /// <summary>
    /// SNS 본문 출력을 즉시 완성
    /// </summary>
    void CompleteTyping ()
    {
        StopTyping( );

        _contentText.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;

        if ( _emphasizeAfterTyping == false ) return;

        _emphasizeAfterTyping = false;
        StartFinalTween( );
    }

    /// <summary>
    /// SNS 본문을 한 글자씩 표시
    /// </summary>
    /// <param name="startIndex">출력을 시작할 글자 순번</param>
    /// <param name="characterCount">전체 글자 수</param>
    IEnumerator TypingRoutine (
        int startIndex, int characterCount )
    {
        WaitForSecondsRealtime interval =
            new WaitForSecondsRealtime( _letterInterval );

        for ( int i = startIndex; i <= characterCount; i++ )
        {
            _contentText.maxVisibleCharacters = i;
            yield return interval;
        }

        _typingRoutine = null;
        _isTyping = false;

        if ( _emphasizeAfterTyping == false ) yield break;

        _emphasizeAfterTyping = false;
        StartFinalTween( );
    }

    /// <summary>
    /// SNS 텍스트 위치를 장면 확대 중심점으로 설정
    /// </summary>
    void SetFocusPivot ()
    {
        Rect sceneArea = _sceneRect.rect;
        RectTransform contentRect =
            _contentText.rectTransform;

        Vector3 focusWorldPosition =
            contentRect.TransformPoint( contentRect.rect.center );

        Vector2 focusLocalPosition =
            _sceneRect.InverseTransformPoint(
                focusWorldPosition );

        Vector2 focusPivot = new Vector2(
            Mathf.InverseLerp(
                sceneArea.xMin, sceneArea.xMax,
                focusLocalPosition.x ),
            Mathf.InverseLerp(
                sceneArea.yMin, sceneArea.yMax,
                focusLocalPosition.y ) );

        Vector2 pivotOffset =
            focusPivot - _sceneRect.pivot;

        _sceneRect.pivot = focusPivot;
        _sceneRect.anchoredPosition +=
            Vector2.Scale( pivotOffset, sceneArea.size );
    }

    /// <summary>
    /// 마지막 문장 장면 강조 시작
    /// </summary>
    void StartFinalTween ()
    {
        KillFinalTween( );

        _finalTween = _sceneRect
            .DOPunchScale(
                Vector3.one * _finalPunchScale,
                _finalPunchDuration, 5, 0.5f )
            .SetUpdate( true )
            .OnComplete( CompleteFinal );
    }

    /// <summary>
    /// 확대 또는 이동 연출 완료 처리
    /// </summary>
    void CompleteMotion ()
    {
        _motionTween = null;
    }

    /// <summary>
    /// 마지막 문장 강조 완료 처리
    /// </summary>
    void CompleteFinal ()
    {
        _finalTween = null;
    }

    /// <summary>
    /// 확대 또는 이동 연출 제거
    /// </summary>
    void KillMotionTween ()
    {
        if ( _motionTween == null ) return;

        _motionTween.Kill( );
        _motionTween = null;
    }

    /// <summary>
    /// 마지막 문장 강조 연출 제거
    /// </summary>
    void KillFinalTween ()
    {
        if ( _finalTween == null ) return;

        _finalTween.Kill( );
        _finalTween = null;
    }

    /// <summary>
    /// SNS 본문 출력 중지
    /// </summary>
    void StopTyping ()
    {
        if ( _typingRoutine == null ) return;

        StopCoroutine( _typingRoutine );
        _typingRoutine = null;
    }
}
