using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Day 2~7 튜토리얼 가이드 데이터 - 후속 가이드 대화 목록 관리
/// </summary>
[CreateAssetMenu(
    fileName = "TutorialGuideData",
    menuName = "TutorialSettings/TutorialGuideData" )]
public class TutorialGuideData : ScriptableObject
{
    [Header( "----- 가이드 대화 -----" )]
    [SerializeField]
    DialogueData [ ] _dialogues =
        Array.Empty<DialogueData>( );       //Day 2~7 가이드 대화 목록

    /// <summary>
    /// Day 2~7 가이드 대화 목록
    /// </summary>
    public IReadOnlyList<DialogueData> Dialogues => _dialogues;

    /// <summary>
    /// 대화 아이디로 후속 가이드 대화 데이터 조회
    /// </summary>
    /// <param name="dialogueId">조회할 대화 아이디</param>
    /// <param name="dialogue">조회한 대화 데이터</param>
    /// <returns>대화 존재 여부</returns>
    public bool TryGetDialogue (
        string dialogueId, out DialogueData dialogue )
    {
        for ( int i = 0; i < _dialogues.Length; i++ )
        {
            DialogueData currentDialogue = _dialogues [ i ];

            if ( currentDialogue != null &&
                currentDialogue.Id == dialogueId )
            {
                dialogue = currentDialogue;
                return true;
            }
        }

        dialogue = null;
        return false;
    }
}
