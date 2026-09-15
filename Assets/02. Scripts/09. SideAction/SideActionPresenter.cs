using UnityEngine;

using System;

/// <summary>
/// 현재 표시 중인 사이드 액션 화면
/// </summary>
enum SideActionPage
{
    None,       //표시 중인 화면 없음
    Ledger,       //가계부
    Catalog,       //카탈로그
    Achv,       //업적
    Settings,       //설정
    Save,       //저장
}

/// <summary>
/// 사이드 액션 프레젠터 - 서브 화면 진입과 단일 화면 전환 중재
/// </summary>
public class SideActionPresenter : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] SideActionView _sideActionView;       //사이드 액션 뷰
    [SerializeField] LedgerPresenter _ledgerPresenter;       //가계부 프레젠터
    [SerializeField] CatalogPresenter _catalogPresenter;       //카탈로그 프레젠터
    [SerializeField] AchvPresenter _achvPresenter;       //업적 프레젠터
    [SerializeField] SettingsPresenter _settingsPresenter;       //설정 프레젠터

    SideActionPage _currentPage;       //현재 표시 중인 사이드 액션
    AchvModel _achvModel;       //업적 상태와 알림 모델
    SettlementPresenter _settlementPresenter;       //주간 결산 확인 중재
    SavePresenter _savePresenter;       //수동 저장 중재

    /// <summary>
    /// 선택한 카탈로그 파츠의 상점 이동 이벤트
    /// </summary>
    public event Action<string> OnMoveToShop;

    /// <summary>
    /// 카탈로그 화면 표시 완료 이벤트
    /// </summary>
    public event Action OnCatalogOpened;

    /// <summary>
    /// 카탈로그 화면 닫기 요청 처리 이벤트
    /// </summary>
    public event Action OnCatalogClosed;

    /// <summary>
    /// 카탈로그 파츠 선택 이벤트
    /// </summary>
    public event Action<string> OnCatalogPartSelected;

    /// <summary>
    /// 가계부 화면 표시 완료 이벤트
    /// </summary>
    public event Action OnLedgerOpened;

    /// <summary>
    /// 가계부 화면 닫기 처리 이벤트
    /// </summary>
    public event Action OnLedgerClosed;

    /// <summary>
    /// 주간 결산 확인 완료 이벤트
    /// </summary>
    public event Action OnWeeklySettlementConfirmed;

    /// <summary>
    /// 업적 화면 표시 완료 이벤트
    /// </summary>
    public event Action OnAchievementOpened;

    /// <summary>
    /// 업적 화면 닫기 처리 이벤트
    /// </summary>
    public event Action OnAchievementClosed;

    /// <summary>
    /// 업적 보상 수령 완료 이벤트
    /// </summary>
    public event Action<string> OnAchievementRewardClaimed;

    /// <summary>
    /// 사이드 액션 프레젠터 초기화
    /// </summary>
    /// <param name="settlementModel">결산 모델</param>
    /// <param name="settlementPresenter">결산 화면 프레젠터</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="scoreSettings">제작 점수 설정 데이터</param>
    /// <param name="achvDataMap">업적 데이터 맵</param>
    /// <param name="achvModel">업적 모델</param>
    /// <param name="rewardProcessor">업적 보상 처리기</param>
    /// <param name="dailyRecordModel">일일 기록 모델</param>
    /// <param name="savePresenter">수동 저장 중재</param>
    public void Init (
        SettlementModel settlementModel ,
        SettlementPresenter settlementPresenter ,
        PlayStateModel playStateModel ,
        PurchasableDataMap dataMap , InventoryModel inventoryModel ,
        CraftScoreSettingsData scoreSettings ,
        AchvDataMap achvDataMap , AchvModel achvModel ,
        AchvRewardProcessor rewardProcessor ,
        DailyRecordModel dailyRecordModel ,
        SavePresenter savePresenter )
    {
        //알림 상태를 전달하기 전에 비활성 사이드 액션 뷰 초기화
        _sideActionView.InitializeRuntime ( );

        _ledgerPresenter.Init ( settlementModel , playStateModel );
        _catalogPresenter.Init (
            dataMap , inventoryModel , playStateModel , scoreSettings );

        _achvModel = achvModel;
        _settlementPresenter = settlementPresenter;
        _settlementPresenter.OnWeeklyConfirmed +=
            ConfirmWeeklySettlement;
        _achvPresenter.Init (
            achvDataMap , achvModel , rewardProcessor ,
            dailyRecordModel , playStateModel , settlementModel , dataMap );

        _settingsPresenter.Init ( GameManager.Instance.SettingsManager );

        _savePresenter = savePresenter;
        _savePresenter.OnCloseRequested += CloseSave;

        _sideActionView.SetAchvNotification ( achvModel.HasNotification );
        _sideActionView.SetLedgerNotification (
            _ledgerPresenter.HasNotification );
        _sideActionView.SetCatalogNotification (
            _catalogPresenter.HasNotification );
    }

    /// <summary>
    /// 메인 시스템 화면 진입 전 사이드 액션과 버튼 숨김
    /// </summary>
    public void HideForMainPanel ( )
    {
        CloseCurrent ( );
        _sideActionView.SetButtonsVisible ( false );
    }

    /// <summary>
    /// 메인 시스템 화면 종료 후 사이드 액션 버튼 표시
    /// </summary>
    public void ShowButtons ( )
    {
        _sideActionView.SetButtonsVisible ( true );
    }

    /// <summary>
    /// Day 1 사이드 액션 입력 제한 갱신
    /// </summary>
    /// <param name="isCoreTutorialCompleted">Day 1 핵심 튜토리얼 완료 여부</param>
    public void SetTutorialAccess (
        bool isCoreTutorialCompleted )
    {
        _sideActionView.SetTutorialAccess (
            isCoreTutorialCompleted );
    }

    /// <summary>
    /// 카탈로그 가이드용 목록 정렬 사용 여부 설정
    /// </summary>
    /// <param name="isEnabled">잠금 파츠 선두 배치 여부</param>
    public void SetCatalogGuideMode ( bool isEnabled )
    {
        _catalogPresenter.SetGuideMode ( isEnabled );
    }

    /// <summary>
    /// 카탈로그 가이드에서 사용할 해금 파츠 상세 표시 보장
    /// </summary>
    /// <returns>해금 파츠 상세 표시 성공 여부</returns>
    public bool EnsureCatalogGuideDetail ( )
    {
        return _currentPage == SideActionPage.Catalog &&
            _catalogPresenter.EnsureGuideDetail ( );
    }

    /// <summary>
    /// 사이드 액션과 카탈로그의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 튜토리얼 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <param name="arrowOffset">대상 기준 화살표 위치 보정</param>
    /// <returns>현재 활성 대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId ,
        out RectTransform target , out Vector2 arrowOffset )
    {
        arrowOffset = Vector2.zero;

        if ( _sideActionView.TryGetTutorialTarget (
            targetId , out target ) )
        {
            return true;
        }

        target = null;

        if ( _currentPage == SideActionPage.Catalog )
        {
            if ( IsCatalogListTarget ( targetId ) )
                _catalogPresenter.EnsureGuideList ( );

            return _catalogPresenter.TryGetTutorialTarget (
                targetId , out target );
        }

        if ( _currentPage == SideActionPage.Achv )
        {
            return _achvPresenter.TryGetTutorialTarget (
                targetId , out target );
        }

        if ( _currentPage == SideActionPage.Ledger )
        {
            return _ledgerPresenter.TryGetTutorialTarget (
                targetId , out target );
        }

        return false;
    }

    /// <summary>
    /// 카탈로그 목록 화면에서 표시하는 가이드 대상 여부 확인
    /// </summary>
    /// <param name="targetId">확인할 튜토리얼 대상 아이디</param>
    /// <returns>카탈로그 목록 대상 여부</returns>
    bool IsCatalogListTarget ( TutorialTargetId targetId )
    {
        return targetId == TutorialTargetId.CatalogLockedPart ||
            targetId == TutorialTargetId.CatalogPart ||
            targetId == TutorialTargetId.CatalogSearch ||
            targetId == TutorialTargetId.CatalogFilter;
    }

    /// <summary>
    /// 사이드 액션 입력 연결
    /// </summary>
    void Awake ( )
    {
        _currentPage = SideActionPage.None;

        _sideActionView.OnLedgerOpen += OpenLedger;
        _sideActionView.OnCatalogOpen += OpenCatalog;
        _sideActionView.OnAchievementOpen += OpenAchv;
        _sideActionView.OnSettingsOpen += OpenSettings;
        _ledgerPresenter.OnCloseRequested += CloseLedger;
        _ledgerPresenter.OnNotificationChanged += ChangeLedgerNotification;
        _catalogPresenter.OnCloseRequested += CloseCatalog;
        _catalogPresenter.OnMoveToShopRequested += MoveToShop;
        _catalogPresenter.OnNotificationChanged += ChangeCatalogNotification;
        _catalogPresenter.OnPartSelected += SelectCatalogPart;
        _achvPresenter.OnCloseRequested += CloseAchv;
        _achvPresenter.OnNotificationChanged += ChangeAchvNotification;
        _achvPresenter.OnRewardClaimed += ClaimAchievementReward;
        _settingsPresenter.OnCloseRequested += CloseSettings;
        _settingsPresenter.OnSaveRequested += OpenSave;
    }

    /// <summary>
    /// 사이드 액션 입력 해제
    /// </summary>
    void OnDestroy ( )
    {
        _sideActionView.OnLedgerOpen -= OpenLedger;
        _sideActionView.OnCatalogOpen -= OpenCatalog;
        _sideActionView.OnAchievementOpen -= OpenAchv;
        _sideActionView.OnSettingsOpen -= OpenSettings;
        _ledgerPresenter.OnCloseRequested -= CloseLedger;
        _ledgerPresenter.OnNotificationChanged -= ChangeLedgerNotification;
        _catalogPresenter.OnCloseRequested -= CloseCatalog;
        _catalogPresenter.OnMoveToShopRequested -= MoveToShop;
        _catalogPresenter.OnNotificationChanged -= ChangeCatalogNotification;
        _catalogPresenter.OnPartSelected -= SelectCatalogPart;
        _achvPresenter.OnCloseRequested -= CloseAchv;
        _achvPresenter.OnNotificationChanged -= ChangeAchvNotification;
        _achvPresenter.OnRewardClaimed -= ClaimAchievementReward;
        _settingsPresenter.OnCloseRequested -= CloseSettings;
        _settingsPresenter.OnSaveRequested -= OpenSave;

        if ( _settlementPresenter != null )
        {
            _settlementPresenter.OnWeeklyConfirmed -=
                ConfirmWeeklySettlement;
        }

        if ( _savePresenter != null )
            _savePresenter.OnCloseRequested -= CloseSave;
    }

    /// <summary>
    /// 가계부 표시
    /// </summary>
    void OpenLedger ( )
    {
        //기존 사이드 액션을 닫은 뒤 가계부 표시
        CloseCurrent ( );

        _currentPage = SideActionPage.Ledger;
        _ledgerPresenter.Show ( );
        OnLedgerOpened?.Invoke ( );
    }

    /// <summary>
    /// 가계부 닫기 요청 처리
    /// </summary>
    void CloseLedger ( )
    {
        if ( _currentPage != SideActionPage.Ledger ) return;

        CloseCurrent ( );
    }

    /// <summary>
    /// 가계부 버튼 알림 표시 상태 갱신
    /// </summary>
    /// <param name="hasNotification">새 주간 결산 기록 존재 여부</param>
    void ChangeLedgerNotification ( bool hasNotification )
    {
        _sideActionView.SetLedgerNotification ( hasNotification );
    }

    /// <summary>
    /// 주간 결산 확인 완료를 가계부 가이드에 전달
    /// </summary>
    void ConfirmWeeklySettlement ( )
    {
        OnWeeklySettlementConfirmed?.Invoke ( );
    }

    /// <summary>
    /// 카탈로그 표시
    /// </summary>
    void OpenCatalog ( )
    {
        //기존 사이드 액션을 닫은 뒤 카탈로그 표시
        CloseCurrent ( );

        _currentPage = SideActionPage.Catalog;
        _catalogPresenter.Show ( );
        OnCatalogOpened?.Invoke ( );
    }

    /// <summary>
    /// 카탈로그 닫기 요청 처리
    /// </summary>
    void CloseCatalog ( )
    {
        if ( _currentPage != SideActionPage.Catalog ) return;

        CloseCurrent ( );
    }

    /// <summary>
    /// 카탈로그 버튼 알림 표시 상태 갱신
    /// </summary>
    /// <param name="hasNotification">새 파츠 정보 존재 여부</param>
    void ChangeCatalogNotification ( bool hasNotification )
    {
        _sideActionView.SetCatalogNotification ( hasNotification );
    }

    /// <summary>
    /// 카탈로그 파츠 선택 이벤트 전달
    /// </summary>
    /// <param name="partId">선택한 파츠 아이디</param>
    void SelectCatalogPart ( string partId )
    {
        if ( _currentPage != SideActionPage.Catalog ) return;

        OnCatalogPartSelected?.Invoke ( partId );
    }

    /// <summary>
    /// 카탈로그를 닫고 선택한 파츠의 상점 이동 요청 전달
    /// </summary>
    /// <param name="id">선택한 파츠 아이디</param>
    void MoveToShop ( string id )
    {
        if ( _currentPage != SideActionPage.Catalog ) return;

        CloseCurrent ( );
        OnMoveToShop?.Invoke ( id );
    }

    /// <summary>
    /// 업적 표시
    /// </summary>
    void OpenAchv ( )
    {
        //기존 사이드 액션을 닫은 뒤 업적 표시
        CloseCurrent ( );

        _currentPage = SideActionPage.Achv;
        _achvPresenter.Show ( );
        OnAchievementOpened?.Invoke ( );
    }

    /// <summary>
    /// 업적 닫기 요청 처리
    /// </summary>
    void CloseAchv ( )
    {
        if ( _currentPage != SideActionPage.Achv ) return;

        CloseCurrent ( );
    }

    /// <summary>
    /// 업적 버튼 알림 표시 상태 갱신
    /// </summary>
    /// <param name="hasNotification">신규 업적 또는 미수령 보상 존재 여부</param>
    void ChangeAchvNotification ( bool hasNotification )
    {
        _sideActionView.SetAchvNotification ( hasNotification );
    }

    /// <summary>
    /// 업적 보상 수령 완료 이벤트 전달
    /// </summary>
    /// <param name="achievementId">보상을 수령한 업적 아이디</param>
    void ClaimAchievementReward ( string achievementId )
    {
        OnAchievementRewardClaimed?.Invoke ( achievementId );
    }

    /// <summary>
    /// 설정 화면 표시
    /// </summary>
    void OpenSettings ( )
    {
        //기존 사이드 액션을 닫은 뒤 설정 화면 표시
        CloseCurrent ( );

        _currentPage = SideActionPage.Settings;
        _settingsPresenter.Show ( );
    }

    /// <summary>
    /// 설정 화면 닫기 요청 처리
    /// </summary>
    void CloseSettings ( )
    {
        if ( _currentPage != SideActionPage.Settings ) return;

        CloseCurrent ( );
    }

    /// <summary>
    /// 설정 화면을 닫고 저장 화면 표시
    /// </summary>
    void OpenSave ( )
    {
        if ( _currentPage != SideActionPage.Settings ) return;

        _settingsPresenter.Hide ( );

        _currentPage = SideActionPage.Save;
        _savePresenter.Show ( );
    }

    /// <summary>
    /// 저장 화면 닫기 요청 처리
    /// </summary>
    void CloseSave ( )
    {
        if ( _currentPage != SideActionPage.Save ) return;

        CloseCurrent ( );
    }

    /// <summary>
    /// 현재 사이드 액션 화면 숨김
    /// </summary>
    void CloseCurrent ( )
    {
        bool wasLedger = _currentPage == SideActionPage.Ledger;
        bool wasCatalog = _currentPage == SideActionPage.Catalog;
        bool wasAchievement = _currentPage == SideActionPage.Achv;

        switch ( _currentPage )
        {
            case SideActionPage.Ledger:
                _ledgerPresenter.Hide ( );
                break;

            case SideActionPage.Catalog:
                _catalogPresenter.Hide ( );
                break;

            case SideActionPage.Achv:
                _achvPresenter.Hide ( );
                break;

            case SideActionPage.Settings:
                _settingsPresenter.Hide ( );
                break;

            case SideActionPage.Save:
                _savePresenter.Hide ( );
                break;
        }

        _currentPage = SideActionPage.None;
        _sideActionView.SetButtonsVisible ( true );

        if ( wasLedger )
            OnLedgerClosed?.Invoke ( );

        if ( wasCatalog )
            OnCatalogClosed?.Invoke ( );

        if ( wasAchievement )
            OnAchievementClosed?.Invoke ( );
    }

    /// <summary>
    /// 불러오기 후 사이드 액션 뷰 및 알림 상태 복구
    /// </summary>
    public void ResetAfterLoad ( )
    {
        CloseCurrentInstant ( );

        _sideActionView.SetAchvNotification ( _achvModel.HasNotification );
        _sideActionView.SetLedgerNotification ( _ledgerPresenter.HasNotification );
        _sideActionView.SetCatalogNotification ( _catalogPresenter.HasNotification );
    }

    /// <summary>
    /// 현재 사이드 액션 화면 즉시 숨김
    /// </summary>
    void CloseCurrentInstant ( )
    {
        bool wasLedger = _currentPage == SideActionPage.Ledger;
        bool wasCatalog = _currentPage == SideActionPage.Catalog;
        bool wasAchievement = _currentPage == SideActionPage.Achv;

        switch ( _currentPage )
        {
            case SideActionPage.Ledger:
                _ledgerPresenter.HideInstant ( );
                break;

            case SideActionPage.Catalog:
                _catalogPresenter.HideInstant ( );
                break;

            case SideActionPage.Achv:
                _achvPresenter.HideInstant ( );
                break;

            case SideActionPage.Settings:
                _settingsPresenter.HideInstant ( );
                break;

            case SideActionPage.Save:
                _savePresenter.HideInstant ( );
                break;
        }

        _currentPage = SideActionPage.None;
        _sideActionView.SetButtonsVisible ( true );

        if ( wasLedger )
            OnLedgerClosed?.Invoke ( );

        if ( wasCatalog )
            OnCatalogClosed?.Invoke ( );

        if ( wasAchievement )
            OnAchievementClosed?.Invoke ( );
    }
}
