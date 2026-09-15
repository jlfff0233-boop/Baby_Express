using System;
using UnityEngine;

/// <summary>
/// 인트로 프레젠터 - 공용 대화와 인트로 화면 및 완료 상태 중재
/// </summary>
public class IntroPresenter : MonoBehaviour
{
    const string StorkSignalId = "Intro_Stork";       //황새 장면 신호
    const string ArticleHeaderSignalId = "Intro_ArticleHeader";       //기사 제목 신호
    const string ArticleBodySignalId = "Intro_ArticleBody";       //기사 본문 신호
    const string SnsZoomSignalId = "Intro_SnsZoom";       //SNS 확대 신호
    const string SnsMoveRightSignalId = "Intro_SnsMoveRight";       //SNS 오른쪽 이동 신호
    const string SnsFinalSignalId = "Intro_SnsFinal";       //SNS 마지막 문장 신호

    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] DialoguePresenter _dialoguePresenter;       //공용 대화 프레젠터
    [SerializeField] IntroView _introView;       //인트로 뷰

    [Header ( "----- 인트로 데이터 -----" )]
    [SerializeField] DialogueData _introDialogue;       //인트로 대화 데이터

    TutorialModel _tutorialModel;       //튜토리얼 진행 모델
    bool _isPlayingIntro;       //인트로 재생 여부

    /// <summary>
    /// 인트로 완료 이벤트
    /// </summary>
    public event Action OnCompleted;

    /// <summary>
    /// 대화 결과 이벤트 연결
    /// </summary>
    void OnEnable ( )
    {
        _dialoguePresenter.OnSignal += HandleSignal;
        _dialoguePresenter.OnCompleted += HandleDialogueCompleted;
    }

    /// <summary>
    /// 대화 결과 이벤트 해제
    /// </summary>
    void OnDisable ( )
    {
        _dialoguePresenter.OnSignal -= HandleSignal;
        _dialoguePresenter.OnCompleted -= HandleDialogueCompleted;
    }

    /// <summary>
    /// 인트로 진행 모델 연결
    /// </summary>
    /// <param name="tutorialModel">튜토리얼 진행 모델</param>
    public void Init ( TutorialModel tutorialModel )
    {
        _tutorialModel = tutorialModel;
    }

    /// <summary>
    /// 인트로가 완료되지 않은 경우 재생 시작
    /// </summary>
    /// <returns>인트로 시작 여부</returns>
    public bool TryBeginIntro ( )
    {
        if ( _tutorialModel.IntroCompleted ||
            _dialoguePresenter.IsPlaying )
        {
            return false;
        }

        _isPlayingIntro = true;

        //대화 시작에 실패하면 인트로 화면도 원래 상태로 복구
        if ( _dialoguePresenter.BeginDialogue ( _introDialogue , _introView ) == false )
        {
            _isPlayingIntro = false;
            _introView.HideInstant ( );
            return false;
        }

        return true;
    }

    /// <summary>
    /// 인트로 대화의 연출 신호 처리
    /// </summary>
    /// <param name="signalId">연출 신호 아이디</param>
    void HandleSignal ( string signalId )
    {
        if ( _isPlayingIntro == false ) return;

        switch ( signalId )
        {
            case StorkSignalId:
                _introView.ShowScene ( IntroSceneType.Stork );
                break;

            case ArticleHeaderSignalId:
                _introView.ShowScene ( IntroSceneType.Article );
                _introView.SetArticleLineType ( IntroArticleLineType.Header );
                break;

            case ArticleBodySignalId:
                _introView.SetArticleLineType ( IntroArticleLineType.Body );
                break;

            case SnsZoomSignalId:
                _introView.ShowScene ( IntroSceneType.Sns );
                _introView.ZoomInSns ( );
                break;

            case SnsMoveRightSignalId:
                _introView.MoveSnsRight ( );
                break;

            case SnsFinalSignalId:
                _introView.SetSnsLineType ( IntroSnsLineType.Final );
                break;
        }
    }

    /// <summary>
    /// 인트로 대화 종료 후 완료 상태 반영
    /// </summary>
    /// <param name="dialogueId">종료한 대화 아이디</param>
    /// <param name="wasSkipped">전체 건너뛰기 여부</param>
    void HandleDialogueCompleted ( string dialogueId , bool wasSkipped )
    {
        if ( _isPlayingIntro == false || dialogueId != _introDialogue.Id )
        {
            return;
        }

        _isPlayingIntro = false;

        //일반 종료와 건너뛰기 모두 같은 인트로 완료 상태로 처리
        _tutorialModel.CompleteIntro ( );

        OnCompleted?.Invoke ( );
    }
}
