using System;

/// <summary>
/// 대화 출력 뷰 공용 규격
/// </summary>
public interface IDialogueOutput
{
    /// <summary>
    /// 다음 대사 입력 이벤트
    /// </summary>
    event Action OnNext;

    /// <summary>
    /// 전체 건너뛰기 입력 이벤트
    /// </summary>
    event Action OnSkip;

    /// <summary>
    /// 현재 문장 또는 연출 진행 여부
    /// </summary>
    bool IsTyping { get; }

    /// <summary>
    /// 런타임 입력 연결
    /// </summary>
    void InitRuntime ();

    /// <summary>
    /// 대화 출력 화면 표시
    /// </summary>
    /// <param name="canSkip">전체 건너뛰기 허용 여부</param>
    void Show ( bool canSkip );

    /// <summary>
    /// 대화 한 줄 표시
    /// </summary>
    /// <param name="line">표시할 대화 줄</param>
    void DisplayLine ( DialogueLine line );

    /// <summary>
    /// 현재 문장 또는 연출 즉시 완성
    /// </summary>
    void CompleteTyping ();

    /// <summary>
    /// 대화 출력 화면 숨김
    /// </summary>
    /// <param name="onComplete">숨김 완료 후 실행할 함수</param>
    void Hide ( Action onComplete = null );

    /// <summary>
    /// 대화 출력 화면 즉시 숨김
    /// </summary>
    void HideInstant ();
}