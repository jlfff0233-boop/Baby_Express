using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 사이드 액션 뷰 - 서브 버튼 입력 전달
/// </summary>
public class SideActionView : MonoBehaviour
{
    [Header( "----- 입력 -----" )]
    [SerializeField] Button _achievementButton;       //업적 버튼
    [SerializeField] Button _catalogButton;       //카탈로그 버튼
    [SerializeField] Button _ledgerButton;       //가계부 버튼
    [SerializeField] Button _settingsButton;       //설정 버튼

    [Header( "----- 출력 -----" )]
    [SerializeField] GameObject _ledgerNotificationIcon;       //새 가계부 기록 알림 아이콘
    [SerializeField] GameObject _catalogNotificationIcon;       //새 카탈로그 정보 알림 아이콘
    [SerializeField] GameObject _achvNotificationIcon;       //신규 업적 알림 아이콘

    UIHighlightView _ledgerButtonHighlight;       //가계부 버튼 강조 연출
    UIHighlightView _ledgerNotificationHighlight;       //가계부 알림 아이콘 강조 연출
    UIHighlightView _catalogButtonHighlight;       //카탈로그 버튼 강조 연출
    UIHighlightView _catalogNotificationHighlight;       //카탈로그 알림 아이콘 강조 연출
    UIHighlightView _achvButtonHighlight;       //업적 버튼 강조 연출
    UIHighlightView _achvNotificationHighlight;       //업적 알림 아이콘 강조 연출
    UIHighlightView _settingsButtonHighlight;       //설정 버튼 클릭 연출

    bool _hasLedgerNotification;       //가계부 알림 존재 여부
    bool _hasCatalogNotification;       //카탈로그 알림 존재 여부
    bool _hasAchvNotification;       //업적 알림 존재 여부

    /// <summary>
    /// 가계부 열기 이벤트
    /// </summary>
    public event Action OnLedgerOpen;

    /// <summary>
    /// 카탈로그 열기 이벤트
    /// </summary>
    public event Action OnCatalogOpen;

    /// <summary>
    /// 업적 열기 이벤트
    /// </summary>
    public event Action OnAchievementOpen;

    /// <summary>
    /// 설정 열기 이벤트
    /// </summary>
    public event Action OnSettingsOpen;

    /// <summary>
    /// 비활성 상태에서도 사용할 강조 컴포넌트 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        InitHighlightViews( );
    }

    /// <summary>
    /// 사이드 액션 입력 연결
    /// </summary>
    void Awake ()
    {
        _ledgerButton.onClick.AddListener( OpenLedger );
        _catalogButton.onClick.AddListener( OpenCatalog );
        _achievementButton.onClick.AddListener( OpenAchievement );
        _settingsButton.onClick.AddListener( OpenSettings );
    }

    /// <summary>
    /// 사이드 액션 버튼과 알림 아이콘의 강조 컴포넌트 초기화
    /// </summary>
    void InitHighlightViews ()
    {
        _ledgerButtonHighlight =
            _ledgerButton.gameObject.GetOrAddComponent<UIHighlightView>( );
        _ledgerNotificationHighlight =
            _ledgerNotificationIcon.GetOrAddComponent<UIHighlightView>( );

        _catalogButtonHighlight =
            _catalogButton.gameObject.GetOrAddComponent<UIHighlightView>( );
        _catalogNotificationHighlight =
            _catalogNotificationIcon.GetOrAddComponent<UIHighlightView>( );

        _achvButtonHighlight =
            _achievementButton.gameObject.GetOrAddComponent<UIHighlightView>( );
        _achvNotificationHighlight =
            _achvNotificationIcon.GetOrAddComponent<UIHighlightView>( );

        _settingsButtonHighlight =
            _settingsButton.gameObject.GetOrAddComponent<UIHighlightView>( );
    }

    /// <summary>
    /// 사이드 액션 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _ledgerButton.onClick.RemoveListener( OpenLedger );
        _catalogButton.onClick.RemoveListener( OpenCatalog );
        _achievementButton.onClick.RemoveListener( OpenAchievement );
        _settingsButton.onClick.RemoveListener( OpenSettings );
    }

    /// <summary>
    /// 업적 버튼의 신규 업적 알림 표시 상태 설정
    /// </summary>
    /// <param name="isVisible">알림 표시 여부</param>
    public void SetAchvNotification ( bool isVisible )
    {
        _hasAchvNotification = isVisible;
        UpdateAchvHighlight( );
    }

    /// <summary>
    /// 가계부 버튼의 새 기록 알림 표시 상태 설정
    /// </summary>
    /// <param name="isVisible">알림 표시 여부</param>
    public void SetLedgerNotification ( bool isVisible )
    {
        _hasLedgerNotification = isVisible;
        UpdateLedgerHighlight( );
    }

    /// <summary>
    /// 카탈로그 버튼의 새 정보 알림 표시 상태 설정
    /// </summary>
    /// <param name="isVisible">알림 표시 여부</param>
    public void SetCatalogNotification ( bool isVisible )
    {
        _hasCatalogNotification = isVisible;
        UpdateCatalogHighlight( );
    }

    /// <summary>
    /// 사이드 액션 버튼 표시 상태 설정
    /// </summary>
    /// <param name="isVisible">버튼 표시 여부</param>
    public void SetButtonsVisible ( bool isVisible )
    {
        _achievementButton.gameObject.SetActive( isVisible );
        _catalogButton.gameObject.SetActive( isVisible );
        _ledgerButton.gameObject.SetActive( isVisible );
        _settingsButton.gameObject.SetActive( isVisible );

        //버튼을 다시 표시하면 유지 중인 알림 강조도 재생
        UpdateLedgerHighlight( );
        UpdateCatalogHighlight( );
        UpdateAchvHighlight( );
    }

    /// <summary>
    /// Day 1 동안 아직 배우지 않은 사이드 액션 입력 제한
    /// </summary>
    /// <param name="isCoreTutorialCompleted">Day 1 핵심 튜토리얼 완료 여부</param>
    public void SetTutorialAccess (
        bool isCoreTutorialCompleted )
    {
        _achievementButton.interactable =
            isCoreTutorialCompleted;
        _catalogButton.interactable =
            isCoreTutorialCompleted;
        _ledgerButton.interactable =
            isCoreTutorialCompleted;

        //접근성을 위한 설정 화면은 Day 1에도 허용
        _settingsButton.interactable = true;
    }

    /// <summary>
    /// 사이드 액션 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 튜토리얼 대상 아이디</param>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        if ( targetId == TutorialTargetId.LedgerButton )
        {
            target = _ledgerButton.transform as RectTransform;
            return target != null;
        }

        if ( targetId == TutorialTargetId.CatalogButton )
        {
            target = _catalogButton.transform as RectTransform;
            return target != null;
        }

        if ( targetId == TutorialTargetId.AchievementButton )
        {
            target = _achievementButton.transform as RectTransform;
            return target != null;
        }

        target = null;
        return false;
    }

    /// <summary>
    /// 가계부 버튼과 알림 아이콘의 강조 표시 갱신
    /// </summary>
    void UpdateLedgerHighlight ()
    {
        UpdateHighlight(
            _ledgerButton, _ledgerNotificationIcon,
            _ledgerNotificationHighlight, _hasLedgerNotification );
    }

    /// <summary>
    /// 카탈로그 버튼과 알림 아이콘의 강조 표시 갱신
    /// </summary>
    void UpdateCatalogHighlight ()
    {
        UpdateHighlight(
            _catalogButton, _catalogNotificationIcon,
            _catalogNotificationHighlight, _hasCatalogNotification );
    }

    /// <summary>
    /// 업적 버튼과 알림 아이콘의 강조 표시 갱신
    /// </summary>
    void UpdateAchvHighlight ()
    {
        UpdateHighlight(
            _achievementButton, _achvNotificationIcon,
            _achvNotificationHighlight, _hasAchvNotification );
    }

    /// <summary>
    /// 사이드 액션 알림 아이콘의 표시와 깜빡임 갱신
    /// </summary>
    /// <param name="button">강조할 사이드 액션 버튼</param>
    /// <param name="notificationIcon">알림 아이콘</param>
    /// <param name="iconHighlight">아이콘 강조 연출</param>
    /// <param name="hasNotification">알림 존재 여부</param>
    void UpdateHighlight (
        Button button, GameObject notificationIcon,
        UIHighlightView iconHighlight, bool hasNotification )
    {
        notificationIcon.SetActive( hasNotification );

        bool canPlay = hasNotification == true &&
            button.gameObject.activeInHierarchy == true;

        if ( canPlay == true )
        {
            iconHighlight.PlayBlinkLoop( );
            return;
        }

        iconHighlight.Stop( );
    }

    /// <summary>
    /// 가계부 열기 요청
    /// </summary>
    void OpenLedger ()
    {
        _ledgerButtonHighlight.PlayClick( );
        OnLedgerOpen?.Invoke( );
    }

    /// <summary>
    /// 카탈로그 열기 요청
    /// </summary>
    void OpenCatalog ()
    {
        _catalogButtonHighlight.PlayClick( );
        OnCatalogOpen?.Invoke( );
    }

    /// <summary>
    /// 업적 열기 요청
    /// </summary>
    void OpenAchievement ()
    {
        _achvButtonHighlight.PlayClick( );
        OnAchievementOpen?.Invoke( );
    }

    /// <summary>
    /// 설정 열기 요청
    /// </summary>
    void OpenSettings ()
    {
        _settingsButtonHighlight.PlayClick( );
        OnSettingsOpen?.Invoke( );
    }
}
