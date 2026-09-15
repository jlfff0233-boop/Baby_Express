using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 황새 인트로 뷰 - 황새 말풍선 생성, 정리
/// </summary>
public class IntroStorkView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField]
    IntroSpeechBubbleView _speechBubblePrefab;       //말풍선 원본

    [SerializeField] RectTransform _leftBubbleRoot;       //왼쪽 생성 위치
    [SerializeField] RectTransform _rightBubbleRoot;       //오른쪽 생성 위치

    [Header( "----- 연출 설정 -----" )]
    [SerializeField, Min( 0f )]
    float _previousBubbleMoveY = 90f;       //기존 말풍선 이동 거리

    [SerializeField, Min( 0f )]
    float _bubbleMoveDuration = 0.2f;       //기존 말풍선 이동 시간

    readonly List<IntroSpeechBubbleView> _leftBubbles =
        new List<IntroSpeechBubbleView>( );       //왼쪽 말풍선 목록

    readonly List<IntroSpeechBubbleView> _rightBubbles =
        new List<IntroSpeechBubbleView>( );       //오른쪽 말풍선 목록

    IntroSpeechBubbleView _currentBubble;       //현재 출력 중인 말풍선

    /// <summary>
    /// 현재 말풍선 등장 또는 대사 출력 여부
    /// </summary>
    public bool IsTyping =>
        _currentBubble != null && _currentBubble.IsTyping;

    /// <summary>
    /// 비활성화 시 생성한 말풍선 정리
    /// </summary>
    void OnDisable ()
    {
        Clear( );
    }

    /// <summary>
    /// 황새 대사 말풍선 표시
    /// </summary>
    /// <param name="line">표시할 대화 줄</param>
    public void DisplayLine ( DialogueLine line )
    {
        if ( line.PortraitSide == DialoguePortraitSide.Left )
        {
            CreateBubble(
                line.Content, _leftBubbleRoot, _leftBubbles );

            return;
        }

        if ( line.PortraitSide == DialoguePortraitSide.Right )
        {
            CreateBubble(
                line.Content, _rightBubbleRoot, _rightBubbles );
        }
    }

    /// <summary>
    /// 생성한 말풍선 전체 정리
    /// </summary>
    public void Clear ()
    {
        ClearBubbles( _leftBubbles );
        ClearBubbles( _rightBubbles );

        _currentBubble = null;
    }

    /// <summary>
    /// 현재 말풍선 등장과 대사 출력을 즉시 완성
    /// </summary>
    public void CompleteTyping ()
    {
        if ( _currentBubble == null ) return;

        _currentBubble.CompleteTyping( );
    }

    /// <summary>
    /// 기존 말풍선을 이동한 뒤 새 말풍선 생성
    /// </summary>
    /// <param name="speech">새 말풍선 대사</param>
    /// <param name="root">말풍선 생성 위치</param>
    /// <param name="bubbles">현재 방향 말풍선 목록</param>
    void CreateBubble (
        string speech, RectTransform root,
        List<IntroSpeechBubbleView> bubbles )
    {
        MovePreviousBubbles( bubbles );

        _currentBubble =
            Instantiate( _speechBubblePrefab, root );

        _currentBubble.Show( speech );
        bubbles.Add( _currentBubble );
    }

    /// <summary>
    /// 기존 말풍선을 위로 이동
    /// </summary>
    /// <param name="bubbles">이동할 말풍선 목록</param>
    void MovePreviousBubbles (
        List<IntroSpeechBubbleView> bubbles )
    {
        for ( int i = 0; i < bubbles.Count; i++ )
        {
            bubbles [ i ].MoveUp(
                _previousBubbleMoveY,
                _bubbleMoveDuration );
        }
    }

    /// <summary>
    /// 지정된 말풍선 목록 정리
    /// </summary>
    /// <param name="bubbles">정리할 말풍선 목록</param>
    void ClearBubbles (
        List<IntroSpeechBubbleView> bubbles )
    {
        for ( int i = 0; i < bubbles.Count; i++ )
        {
            if ( bubbles [ i ] != null )
                Destroy( bubbles [ i ].gameObject );
        }

        bubbles.Clear( );
    }
}
