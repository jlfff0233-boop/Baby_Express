using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파산 뷰 - 파산 안내 표시와 다시 시작 입력 전달
/// </summary>
public class BankruptcyView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //파산 패널 연출
    [SerializeField] TMP_Text _warningTitleText;       //경고 제목
    [SerializeField] TMP_Text _warningDescText;       //경고 설명
    [SerializeField] Button _restartButton;       //처음부터 다시 시작 버튼
    [SerializeField] Button _restartCurrentWeekButton;       //이번 주부터 다시 시작 버튼
    [SerializeField] Button _titleButton;       //시작 화면 버튼

    /// <summary>
    /// 처음부터 다시 시작 이벤트
    /// </summary>
    public event Action OnRestartSelected;

    /// <summary>
    /// 이번 주부터 다시 시작 이벤트
    /// </summary>
    public event Action OnRestartCurrentWeekSelected;

    /// <summary>
    /// 시작 화면 이동 이벤트
    /// </summary>
    public event Action OnTitleSelected;


    /// <summary>
    /// 파산 화면 입력 연결
    /// </summary>
    void Awake ( )
    {
        _restartButton.onClick.AddListener ( SelectRestart );
        _restartCurrentWeekButton.onClick.AddListener (
            SelectRestartCurrentWeek );
        _titleButton.onClick.AddListener ( SelectTitle );
    }

    /// <summary>
    /// 파산 화면 입력 해제
    /// </summary>
    void OnDestroy ( )
    {
        _restartButton.onClick.RemoveListener ( SelectRestart );
        _restartCurrentWeekButton.onClick.RemoveListener (
            SelectRestartCurrentWeek );
        _titleButton.onClick.RemoveListener ( SelectTitle );
    }

    /// <summary>
    /// 파산 화면 표시
    /// </summary>
    /// <param name="title">경고 제목</param>
    /// <param name="description">경고 설명</param>
    /// <param name="canRestartCurrentWeek">이번 주부터 다시 시작 가능 여부</param>
    public void Show (
        string title , string description , bool canRestartCurrentWeek )
    {
        //비활성화 상태로 시작한 뷰도 표시
        gameObject.SetActive ( true );

        //파산 안내 문구 표시
        _warningTitleText.text = title;
        _warningDescText.text = description;

        //주간 저장 기능 연결 전에는 버튼 비활성화
        _restartCurrentWeekButton.interactable =
            canRestartCurrentWeek;

        _panelTween.Show ( );
    }

    /// <summary>
    /// 파산 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void Hide ( Action onComplete = null )
    {
        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 파산 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 파산 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    /// <summary>
    /// 처음부터 다시 시작 입력 전달
    /// </summary>
    void SelectRestart ( )
    {
        OnRestartSelected?.Invoke ( );
    }

    /// <summary>
    /// 이번 주부터 다시 시작 입력 전달
    /// </summary>
    void SelectRestartCurrentWeek ( )
    {
        OnRestartCurrentWeekSelected?.Invoke ( );
    }

    /// <summary>
    /// 시작 화면 이동 입력 전달
    /// </summary>
    void SelectTitle ( )
    {
        OnTitleSelected?.Invoke ( );
    }
}
