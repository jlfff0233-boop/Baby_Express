using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 패널 트윈 뷰 - 패널의 확대 등장과 수축 퇴장 연출
/// </summary>
public class PanelTweenView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] RectTransform _panel;       //크기를 변경할 패널
    [SerializeField] CanvasGroup _canvasGroup;       //패널 입력 관리

    [Header ( "----- 등장 연출 -----" )]
    [SerializeField, Min ( 0f )] float _showDuration = 0.25f;       //등장 연출 시간
    [SerializeField] Ease _showEase = Ease.OutBack;       //등장 Ease

    [Header ( "----- 퇴장 연출 -----" )]
    [SerializeField, Min ( 0f )]
    float _hideDuration = 0.18f;       //퇴장 연출 시간
    [SerializeField] Ease _hideEase = Ease.InBack;       //퇴장 Ease

    [Header ( "----- 크기 -----" )]
    [SerializeField, Range ( 0f , 0.2f )]
    float _hiddenScale;       //숨김 상태 크기 비율

    Vector3 _shownScale;       //패널의 원래 표시 크기
    Tween _scaleTween;       //현재 실행 중인 크기 Tween
    Action _hideCompleted;       //패널 퇴장 완료 처리
    bool _isInitialized;       //패널 원래 크기 저장 여부
    bool _isHiding;       //패널 퇴장 연출 진행 여부

    /// <summary>
    /// 패널 원래 크기 저장
    /// </summary>
    void Awake ( )
    {
        Initialize ( );
    }

    /// <summary>
    /// 비활성화 시 실행 중인 연출과 표시 상태 초기화
    /// </summary>
    void OnDisable ( )
    {
        KillTween ( );

        if ( _isInitialized == false ) return;

        if ( _panel != null )
            _panel.localScale = _shownScale;

        if ( _canvasGroup != null )
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 패널 확대 등장 연출
    /// </summary>
    public void Show ( )
    {
        Initialize ( );

        //이미 정상적으로 열린 상태면 다시 연출하지 않음
        if ( gameObject.activeSelf &&
            _scaleTween == null &&
            _canvasGroup.interactable )
        {
            return;
        }

        //기존 트윈 종료
        KillTween ( );

        //실제 패널 등장 연출에서만 효과음 재생
        GameManager.Instance.AudioManager.PlaySFX ( SFXType.Panel );

        gameObject.SetActive ( true );

        //패널과 패널 뒤의 UI 입력 차단
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;

        //크기 설정
        _panel.localScale = _shownScale * _hiddenScale;

        //연출
        _scaleTween = _panel
            .DOScale ( _shownScale , _showDuration )
            .SetEase ( _showEase )
            .SetUpdate ( true )
            .OnComplete ( CompleteShow );
    }

    /// <summary>
    /// 패널 수축 퇴장 연출
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 작업</param>
    public void Hide ( Action onComplete = null )
    {
        Initialize ( );

        //비활성화 시 즉시 끄기
        if ( gameObject.activeInHierarchy == false )
        {
            SetVisible ( false );
            onComplete?.Invoke ( );
            return;
        }

        //이미 퇴장 중이면 연출과 효과음을 다시 시작하지 않음
        if ( _isHiding )
        {
            _hideCompleted += onComplete;
            return;
        }

        //기존 트윈 종료
        KillTween ( );

        _isHiding = true;

        //실제 패널 퇴장 연출에서만 효과음 재생
        GameManager.Instance.AudioManager.PlaySFX ( SFXType.Panel );

        //패널과 패널 뒤의 UI 입력 차단
        _hideCompleted = onComplete;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;

        //연출
        _scaleTween = _panel
            .DOScale ( _shownScale * _hiddenScale , _hideDuration )
            .SetEase ( _hideEase )
            .SetUpdate ( true )
            .OnComplete ( CompleteHide );
    }

    /// <summary>
    /// 패널 표시 상태 설정
    /// </summary>
    /// <param name="isVisible">패널 표시 여부</param>
    public void SetVisible ( bool isVisible )
    {
        Initialize ( );
        KillTween ( );

        _panel.localScale = _shownScale;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;

        gameObject.SetActive ( isVisible );
    }

    /// <summary>
    /// 패널 원래 크기 저장
    /// </summary>
    void Initialize ( )
    {
        if ( _isInitialized ) return;

        _shownScale = _panel.localScale;
        _isInitialized = true;
    }

    /// <summary>
    /// 패널 등장 완료 처리
    /// </summary>
    void CompleteShow ( )
    {
        _scaleTween = null;
        _isHiding = false;

        _panel.localScale = _shownScale;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        _scaleTween = null;
        _isHiding = false;

        //비활성화 시 OnDisable에서 필드가 정리되므로 먼저 보관
        Action hideCompleted = _hideCompleted;
        _hideCompleted = null;

        gameObject.SetActive ( false );
        hideCompleted?.Invoke ( );
    }

    /// <summary>
    /// 현재 실행 중인 패널 연출 제거
    /// </summary>
    void KillTween ( )
    {
        //기존 트윈 종료
        if ( _scaleTween != null )
        {
            _scaleTween.Kill ( );
            _scaleTween = null;
        }

        _hideCompleted = null;
        _isHiding = false;
    }
}
