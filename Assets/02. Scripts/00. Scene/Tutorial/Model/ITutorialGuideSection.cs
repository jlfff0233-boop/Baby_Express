using System;

/// <summary>
/// Day 2 이후 기능별 튜토리얼 가이드 공통 인터페이스
/// </summary>
public interface ITutorialGuideSection
{
    /// <summary>
    /// 현재 가이드 완료 이벤트
    /// </summary>
    event Action<TutorialGuideId> OnCompleted;

    /// <summary>
    /// 지정 가이드 담당 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 가이드 아이디</param>
    /// <returns>현재 기능의 담당 가이드 여부</returns>
    bool Handles ( TutorialGuideId guideId );

    /// <summary>
    /// 기능별 가이드 시작
    /// </summary>
    /// <param name="guideId">시작할 가이드 아이디</param>
    /// <returns>가이드 시작 성공 여부</returns>
    bool Begin ( TutorialGuideId guideId );

    /// <summary>
    /// 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>현재 기능에서 처리한 신호 여부</returns>
    bool HandleSignal ( string signalId );

    /// <summary>
    /// 가이드 대화 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <returns>현재 기능에서 처리한 대화 여부</returns>
    bool HandleDialogueCompleted ( string dialogueId );

    /// <summary>
    /// 담당 시스템 행동 이벤트 연결
    /// </summary>
    void SubscribeEvents ();

    /// <summary>
    /// 담당 시스템 행동 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ();

    /// <summary>
    /// 진행 중인 대기 상태와 임시 표시 상태 초기화
    /// </summary>
    void Reset ();
}