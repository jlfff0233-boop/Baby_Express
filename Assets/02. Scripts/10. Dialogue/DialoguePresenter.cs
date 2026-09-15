using System;
using UnityEngine;

/// <summary>
/// 공용 대화 프레젠터 - 대화 진행과 외부 연출 신호 전달 중재
/// </summary>
public class DialoguePresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] DialogueView _dialogueView;       //공용 대화 뷰

    DialogueData _currentData;       //현재 재생 중인 대화 데이터
    IDialogueOutput _currentOutput;       //현재 대화 출력 뷰
    int _lineIndex;       //현재 대화 줄 순번
    bool _isClosing;       //대화 패널 퇴장 중 여부

    /// <summary>
    /// 대화 시작 이벤트
    /// </summary>
    public event Action<string> OnStarted;

    /// <summary>
    /// 대화 활성화 상태 이벤트
    /// </summary>
    public event Action<bool> OnToggled;

    /// <summary>
    /// 연출 신호 이벤트
    /// </summary>
    public event Action<string> OnSignal;

    /// <summary>
    /// 대화 완료 이벤트
    /// </summary>
    public event Action<string, bool> OnCompleted;

    /// <summary>
    /// 현재 대화 재생 여부
    /// </summary>
    public bool IsPlaying => _currentData != null;

    #region ----- 초기화/이벤트 -----

    /// <summary>
    /// 현재 대화 출력 뷰 연결
    /// </summary>
    /// <param name="output">연결할 대화 출력 뷰</param>
    void ConnectOutput ( IDialogueOutput output )
    {
        DisconnectOutput( );

        _currentOutput = output;
        _currentOutput.InitRuntime( );

        _currentOutput.OnNext += Next;
        _currentOutput.OnSkip += Skip;
    }

    /// <summary>
    /// 현재 대화 출력 뷰 연결 해제
    /// </summary>
    void DisconnectOutput ()
    {
        if ( _currentOutput == null ) return;

        //인터페이스 참조는 파괴된 Unity 오브젝트도 null이 아닐 수 있음
        if ( _currentOutput is UnityEngine.Object outputObject &&
            outputObject == null )
        {
            _currentOutput = null;
            return;
        }

        _currentOutput.OnNext -= Next;
        _currentOutput.OnSkip -= Skip;

        _currentOutput = null;
    }

    /// <summary>
    /// 기본 대화 뷰 초기화
    /// </summary>
    void Awake ()
    {
        _dialogueView.InitRuntime( );
        _dialogueView.HideInstant( );
    }

    /// <summary>
    /// 현재 대화 출력 뷰 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        DisconnectOutput( );
    }

    #endregion

    #region ----- 대화 진행 -----

    /// <summary>
    /// 기본 대화 뷰를 사용해 지정된 대화 재생 시작
    /// </summary>
    /// <param name="data">재생할 대화 데이터</param>
    /// <returns>대화 시작 성공 여부</returns>
    public bool BeginDialogue ( DialogueData data )
    {
        return BeginDialogue( data, _dialogueView );
    }

    /// <summary>
    /// 지정된 출력 뷰를 사용해 대화 재생 시작
    /// </summary>
    /// <param name="data">재생할 대화 데이터</param>
    /// <param name="output">대사를 표시할 출력 뷰</param>
    /// <returns>대화 시작 성공 여부</returns>
    public bool BeginDialogue (
        DialogueData data,
        IDialogueOutput output )
    {
        //현재 대화 또는 퇴장 연출이 끝난 뒤 새 대화 시작
        if ( data == null ||
            output == null ||
            _currentData != null ||
            _isClosing )
        {
            return false;
        }

        ConnectOutput( output );

        _currentData = data;
        _lineIndex = 0;

        _currentOutput.Show( data.CanSkip );

        OnStarted?.Invoke( data.Id );
        OnToggled?.Invoke( true );

        PlayCurrentLine( );
        return true;
    }

    /// <summary>
    /// 현재 대화를 완료 이벤트 없이 즉시 종료
    /// </summary>
    public void CancelDialogue ()
    {
        bool wasPlaying = _currentData != null || _isClosing;

        //인터페이스 참조가 가리키는 Unity 오브젝트의 파괴 여부 확인
        bool canHideCurrentOutput =
            _currentOutput != null &&
            ( _currentOutput is not UnityEngine.Object outputObject ||
                outputObject != null );

        if ( canHideCurrentOutput )
            _currentOutput.HideInstant( );

        DisconnectOutput( );

        if ( _dialogueView != null )
            _dialogueView.HideInstant( );
        _currentData = null;
        _lineIndex = 0;
        _isClosing = false;

        if ( wasPlaying )
            OnToggled?.Invoke( false );
    }

    /// <summary>
    /// 현재 순번의 대화 줄 재생
    /// </summary>
    void PlayCurrentLine ()
    {
        //연출 신호는 외부에 전달한 뒤 다음 대화 줄까지 계속 진행
        while ( _currentData.TryGetLine(
            _lineIndex, out DialogueLine line ) )
        {
            if ( line.LineType == DialogueLineType.Signal )
            {
                OnSignal?.Invoke( line.SignalId );
                _lineIndex++;
                continue;
            }

            _currentOutput.DisplayLine( line );
            return;
        }

        CloseDialogue( false );
    }

    /// <summary>
    /// 현재 문장 완성 또는 다음 대사 진행
    /// </summary>
    void Next ()
    {
        if ( _currentData == null ) return;

        if ( _currentOutput.IsTyping )
        {
            _currentOutput.CompleteTyping( );
            return;
        }

        _lineIndex++;
        PlayCurrentLine( );
    }

    /// <summary>
    /// 허용된 대화 전체 건너뛰기
    /// </summary>
    void Skip ()
    {
        if ( _currentData == null ||
            _currentData.CanSkip == false )
        {
            return;
        }

        CloseDialogue( true );
    }

    /// <summary>
    /// 현재 대화 종료와 패널 퇴장
    /// </summary>
    /// <param name="wasSkipped">전체 건너뛰기 여부</param>
    void CloseDialogue ( bool wasSkipped )
    {
        if ( _currentData == null || _isClosing )
            return;

        string dialogueId = _currentData.Id;

        _currentData = null;
        _isClosing = true;

        _currentOutput.Hide(
            () => CompleteDialogue( dialogueId, wasSkipped ) );
    }

    /// <summary>
    /// 대화 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="dialogueId">완료한 대화 아이디</param>
    /// <param name="wasSkipped">전체 건너뛰기 여부</param>
    void CompleteDialogue ( string dialogueId, bool wasSkipped )
    {
        DisconnectOutput( );

        _isClosing = false;

        OnToggled?.Invoke( false );
        OnCompleted?.Invoke( dialogueId, wasSkipped );
    }

    #endregion
}
