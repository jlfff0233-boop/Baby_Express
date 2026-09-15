/// <summary>
/// 기능별 튜토리얼 처리 공통 인터페이스
/// </summary>
public interface ITutorialSection
{
    /// <summary>
    /// 지정 핵심 단계 담당 여부 확인
    /// </summary>
    /// <param name="step">확인할 핵심 튜토리얼 단계</param>
    /// <returns>현재 기능의 담당 단계 여부</returns>
    bool Handles ( CoreTutorialStep step );

    /// <summary>
    /// 기능별 튜토리얼 단계 시작
    /// </summary>
    /// <param name="step">시작할 핵심 튜토리얼 단계</param>
    /// <returns>단계 시작 여부</returns>
    bool Begin ( CoreTutorialStep step );

    /// <summary>
    /// 대화 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    /// <returns>현재 기능에서 처리한 신호 여부</returns>
    bool HandleSignal ( string signalId );

    /// <summary>
    /// 대화 완료 처리
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
}
