using UnityEngine;

/// <summary>
/// 대화 줄 종류
/// </summary>
public enum DialogueLineType
{
    Speech,        //일반 대사
    Signal,        //연출 신호
}

/// <summary>
/// 캐릭터 이미지 표시 위치
/// </summary>
public enum DialoguePortraitSide
{
    None,       //캐릭터 이미지 없음
    Left,       //왼쪽 표시
    Right,      //오른쪽 표시
}

/// <summary>
/// 대화 한 줄 데이터
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [SerializeField] DialogueLineType _lineType;       //대화 줄 종류

    [Header( "----- 대사 -----" )]
    [SerializeField] string _speakerName;       //화자 이름
    [SerializeField, TextArea( 3, 8 )] string _content;       //대사 본문
    [SerializeField] Sprite _portrait;       //캐릭터 이미지
    [SerializeField] DialoguePortraitSide _portraitSide;       //이미지 표시 위치

    [Header( "----- 연출 신호 -----" )]
    [SerializeField] string _signalId;       //외부에 전달할 연출 신호 아이디

    /// <summary>
    /// 대화 줄 종류
    /// </summary>
    public DialogueLineType LineType => _lineType;

    /// <summary>
    /// 화자 이름
    /// </summary>
    public string SpeakerName => _speakerName;

    /// <summary>
    /// 대사 본문
    /// </summary>
    public string Content => _content;

    /// <summary>
    /// 캐릭터 이미지
    /// </summary>
    public Sprite Portrait => _portrait;

    /// <summary>
    /// 캐릭터 이미지 표시 위치
    /// </summary>
    public DialoguePortraitSide PortraitSide => _portraitSide;

    /// <summary>
    /// 연출 신호 아이디
    /// </summary>
    public string SignalId => _signalId;
}