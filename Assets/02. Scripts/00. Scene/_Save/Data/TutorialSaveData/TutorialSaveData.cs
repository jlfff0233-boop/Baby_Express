using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 진행 저장 데이터
/// </summary>
[Serializable]
public class TutorialSaveData
{
    [SerializeField] bool _introCompleted;       //인트로 완료 여부
    [SerializeField] CoreTutorialStep _currentCoreStep;       //현재 핵심 튜토리얼 단계
    [SerializeField] List<TutorialGuideId> _shownGuides;       //표시를 완료한 가이드

    /// <summary>
    /// 인트로 완료 여부
    /// </summary>
    public bool IntroCompleted => _introCompleted;

    /// <summary>
    /// 현재 핵심 튜토리얼 단계
    /// </summary>
    public CoreTutorialStep CurrentCoreStep =>
        _currentCoreStep;

    /// <summary>
    /// 표시를 완료한 후속 가이드
    /// </summary>
    public IReadOnlyList<TutorialGuideId> ShownGuides =>
        _shownGuides;

    /// <summary>
    /// 튜토리얼 진행 저장 데이터 생성
    /// </summary>
    /// <param name="introCompleted">인트로 완료 여부</param>
    /// <param name="currentCoreStep">현재 핵심 튜토리얼 단계</param>
    /// <param name="shownGuides">표시를 완료한 가이드</param>
    public TutorialSaveData (
        bool introCompleted,
        CoreTutorialStep currentCoreStep,
        IEnumerable<TutorialGuideId> shownGuides )
    {
        _introCompleted = introCompleted;
        _currentCoreStep = currentCoreStep;
        _shownGuides = new List<TutorialGuideId>( shownGuides );
    }
}
