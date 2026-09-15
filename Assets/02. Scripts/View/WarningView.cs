using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 경고 뷰 - 경고 내용과 확인 입력 표시
/// </summary>
public class WarningView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _titleText;       //경고 제목
    [SerializeField] TMP_Text _descriptionText;       //경고 설명
    [SerializeField] TMP_Text _confirmText;       //확인 버튼 문구
    [SerializeField] TMP_Text _cancelText;       //취소 버튼 문구
    [SerializeField] PanelTweenView _panelTween;       //경고 패널 연출

    [Header ( "----- 버튼 -----" )]
    [SerializeField] Button _confirmButton;       //확인 버튼
    [SerializeField] Button _cancelButton;       //취소 버튼

    /// <summary>
    /// 경고 확인 이벤트
    /// </summary>
    public event Action OnConfirmed;

    /// <summary>
    /// 경고 취소 이벤트
    /// </summary>
    public event Action OnCanceled;

    /// <summary>
    /// 입력 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        //확인과 취소 버튼에 공용 클릭 연출 연결
        _confirmButton.BindClickHighlight( );
        _cancelButton.BindClickHighlight( );

        _confirmButton.onClick.AddListener ( Confirm );
        _cancelButton.onClick.AddListener ( Cancel );
    }

    /// <summary>
    /// 입력 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        _confirmButton.onClick.RemoveListener ( Confirm );
        _cancelButton.onClick.RemoveListener ( Cancel );
    }

    /// <summary>
    /// 경고 패널 표시
    /// </summary>
    /// <param name="title">경고 제목</param>
    /// <param name="description">경고 설명</param>
    /// <param name="confirmText">확인 버튼 문구</param>
    /// <param name="cancelText">취소 버튼 문구</param>
    public void Show (
    string title , string description , string confirmText , string cancelText )
    {
        gameObject.SetActive ( true );

        _titleText.text = title;
        _descriptionText.text = description;
        _confirmText.text = confirmText;
        _cancelText.text = cancelText;

        //연출
        _panelTween.Show ( );
    }

    /// <summary>
    /// 경고 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 연출 완료 후 처리</param>
    public void Hide ( Action onComplete = null )
    {
        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 경고 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 경고 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    /// <summary>
    /// 경고 확인 이벤트 발행
    /// </summary>
    void Confirm ( )
    {
        OnConfirmed?.Invoke ( );
    }

    /// <summary>
    /// 경고 취소 이벤트 발행
    /// </summary>
    void Cancel ( )
    {
        OnCanceled?.Invoke ( );
    }
}
