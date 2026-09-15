using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대화 설정 데이터
/// </summary>
[CreateAssetMenu(
    menuName = "DialogueSettings/DialogueData" )]
public class DialogueData : ScriptableObject
{
    [SerializeField] string _id;       //대화 고유 아이디
    [SerializeField] bool _canSkip;       //대화 건너뛰기 허용 여부

    [Header( "----- 대화 줄 -----" )]
    [SerializeField]
    List<DialogueLine> _lines =
        new List<DialogueLine>( );       //순서대로 재생할 대화 줄

    /// <summary>
    /// 대화 고유 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 대화 건너뛰기 허용 여부
    /// </summary>
    public bool CanSkip => _canSkip;

    /// <summary>
    /// 지정된 순번의 대화 줄 반환
    /// </summary>
    /// <param name="index">대화 줄 순번</param>
    /// <param name="line">반환할 대화 줄</param>
    /// <returns>대화 줄 존재 여부</returns>
    public bool TryGetLine ( int index, out DialogueLine line )
    {
        if ( index < 0 || index >= _lines.Count )
        {
            //라인 비우기
            line = null;
            return false;
        }

        //대사 설정
        line = _lines [ index ];
        return true;
    }
}