using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 정비 뷰 - 탭, 정비 목록, 전체 현황과 상세 패널 표시
/// </summary>
public class MaintenanceView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //정비 패널 연출
    [SerializeField] GameObject _maintenancePanel;       //정비 패널
    [SerializeField] Button _closeButton;       //정비 닫기 버튼

    [Header ( "--- 탭 ---" )]
    [SerializeField] Button _managementButton;       //전체 탭
    [SerializeField] Button _facilityButton;       //시설 탭
    [SerializeField] Button _researchButton;       //연구 탭
    [SerializeField] Button _convenienceButton;       //편의 탭
    [SerializeField] Button _employeeButton;       //고용 탭

    [Header ( "--- 상세 패널 ---" )]
    [SerializeField] MaintenanceDetailView _detailView;       //정비 상세 뷰

    [Header ( "--- Content ---" )]
    [SerializeField] GameObject _managementContent;       //전체 Content
    [SerializeField] Transform _defaultContent;       //공용 정비 목록 Content
    [SerializeField] GameObject _employeeContent;       //직원 Content
    [SerializeField] ScrollRect _scrollRect;       //정비 목록 스크롤
    [SerializeField] MaintenanceSlotView _slotPrefab;       //정비 슬롯 프리팹
    [SerializeField] EmployeeSlotView _employeeSlotPrefab;       //직원 슬롯 프리팹

    [Header ( "--- 전체 현황 ---" )]
    [SerializeField]
    MaintenanceEffectSlotView _effectSlotPrefab;       //전체 탭 효과 슬롯 프리팹

    List<MaintenanceEffectSlotView> _effectSlotViews =
        new List<MaintenanceEffectSlotView> ( );       //활성 효과 슬롯

    /// <summary>
    /// 활성 정비 슬롯 딕셔너리(정비 아이디, 정비 슬롯)
    /// </summary>
    Dictionary<string , MaintenanceSlotView> _slotViews =
        new Dictionary<string , MaintenanceSlotView> ( );

    /// <summary>
    /// 활성 직원 슬롯 딕셔너리(직원 아이디, 직원 슬롯)
    /// </summary>
    Dictionary<string , EmployeeSlotView> _employeeSlotViews =
        new Dictionary<string , EmployeeSlotView> ( );

    PoolManager _poolManager;       //공용 정비 슬롯 풀 관리자

    #region ----- 이벤트 -----

    /// <summary>
    /// 전체 탭 선택 이벤트
    /// </summary>
    public event Action OnManagementSelected;

    /// <summary>
    /// 시설 탭 선택 이벤트
    /// </summary>
    public event Action OnFacilitySelected;

    /// <summary>
    /// 연구 탭 선택 이벤트
    /// </summary>
    public event Action OnResearchSelected;

    /// <summary>
    /// 편의 탭 선택 이벤트
    /// </summary>
    public event Action OnConvenienceSelected;

    /// <summary>
    /// 고용 탭 선택 이벤트
    /// </summary>
    public event Action OnEmployeeSelected;

    /// <summary>
    /// 정비 항목 선택 이벤트
    /// </summary>
    public event Action<string> OnMaintenanceSelected;

    /// <summary>
    /// 정비 구매 입력 이벤트
    /// </summary>
    public event Action<string> OnUpgrade;

    /// <summary>
    /// 정비 화면 닫기 이벤트
    /// </summary>
    public event Action OnClose;

    /// <summary>
    /// 직원 처리 입력 이벤트(직원 아이디)
    /// </summary>
    public event Action<string> OnEmployeeAction;

    /// <summary>
    /// 정비 상세 닫기 이벤트
    /// </summary>
    public event Action OnDetailClose;
    #endregion

    /// <summary>
    /// 정비 화면의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId , out RectTransform target )
    {
        if ( targetId == TutorialTargetId.MaintenanceCurrentLevel ||
            targetId == TutorialTargetId.MaintenanceNextLevel )
        {
            return _detailView.TryGetTutorialTarget (
                targetId , out target );
        }

        Button button;

        switch ( targetId )
        {
            case TutorialTargetId.MaintenanceFacilityTab:
                button = _facilityButton;
                break;

            case TutorialTargetId.MaintenanceResearchTab:
                button = _researchButton;
                break;

            case TutorialTargetId.MaintenanceConvenienceTab:
                button = _convenienceButton;
                break;

            case TutorialTargetId.MaintenanceEmployeeTab:
                button = _employeeButton;
                break;

            default:
                target = null;
                return false;
        }

        target = button.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 지정 직원 카드 내부의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="employeeId">조회할 직원 아이디</param>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 직원 카드 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetEmployeeTutorialTarget (
        string employeeId , TutorialTargetId targetId ,
        out RectTransform target )
    {
        target = null;

        if ( string.IsNullOrEmpty ( employeeId ) ||
            _employeeSlotViews.TryGetValue (
                employeeId , out EmployeeSlotView slot ) == false )
        {
            return false;
        }

        return slot.TryGetTutorialTarget ( targetId , out target );
    }

    /// <summary>
    /// 현재 날짜의 고용 탭 입력 가능 여부 설정
    /// </summary>
    /// <param name="interactable">고용 탭 입력 허용 여부</param>
    public void SetEmployeeInteractable ( bool interactable )
    {
        _employeeButton.interactable = interactable;
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 공용 풀 매니저 초기화
    /// </summary>
    public void InitializeRuntime ( )
    {
        _poolManager = GameManager.Instance.PoolManager;
    }

    /// <summary>
    /// 정비 입력 연결
    /// </summary>
    void Awake ( )
    {
        _managementButton.onClick.AddListener ( SelectManagement );
        _facilityButton.onClick.AddListener ( SelectFacility );
        _researchButton.onClick.AddListener ( SelectResearch );
        _convenienceButton.onClick.AddListener ( SelectConvenience );
        _employeeButton.onClick.AddListener ( SelectEmployee );
        _closeButton.onClick.AddListener ( Close );

        _detailView.OnUpgrade += Upgrade;
        _detailView.OnClose += CloseDetail;
    }

    /// <summary>
    /// 비활성화 전에 활성 정비 관련 슬롯 반환
    /// </summary>
    void OnDisable ( )
    {
        ClearEffectSlots ( );
        ClearSlots ( );
        ClearEmployeeSlots ( );
    }

    /// <summary>
    /// 정비 입력 해제
    /// </summary>
    void OnDestroy ( )
    {
        _managementButton.onClick.RemoveListener ( SelectManagement );
        _facilityButton.onClick.RemoveListener ( SelectFacility );
        _researchButton.onClick.RemoveListener ( SelectResearch );
        _convenienceButton.onClick.RemoveListener ( SelectConvenience );
        _employeeButton.onClick.RemoveListener ( SelectEmployee );
        _closeButton.onClick.RemoveListener ( Close );

        _detailView.OnUpgrade -= Upgrade;
        _detailView.OnClose -= CloseDetail;

    }

    #region ----- 패널 표시 -----

    /// <summary>
    /// 정비 화면 표시
    /// </summary>
    public void ShowPanel ( )
    {
        gameObject.SetActive ( true );
        _maintenancePanel.SetActive ( true );

        //기본 전체 탭 상태를 먼저 확정
        SetContent ( true , false , false );
        _detailView.HideInstant ( );

        _panelTween.Show ( );
    }

    /// <summary>
    /// 정비 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        ClearEffectSlots ( );
        ClearSlots ( );
        ClearEmployeeSlots ( );
        _detailView.HideInstant ( );

        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 정비 화면 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        ClearEffectSlots ( );
        ClearSlots ( );
        ClearEmployeeSlots ( );
        _detailView.HideInstant ( );

        _panelTween.SetVisible ( false );
        _maintenancePanel.SetActive ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 정비 화면 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        _maintenancePanel.SetActive ( false );
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    /// <summary>
    /// 정비 전체 현황 표시
    /// </summary>
    /// <param name="viewData">정비 전체 현황 표시 데이터</param>
    public void ShowManagement (
        MaintenanceManagementViewData viewData )
    {
        SetContent ( true , false , false );       //전체 탭 표시
        ClearEffectSlots ( );
        ClearSlots ( );
        ClearEmployeeSlots ( );

        //업그레이드 항목 하나당 효과 슬롯 하나 표시
        for ( int i = 0 ; i < viewData.Effects.Count ; i++ )
            CreateEffectSlot ( viewData.Effects [ i ] , i );

        _detailView.HideInstant ( );
        ResetScroll ( );
    }

    /// <summary>
    /// 전체 탭 효과 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">효과 슬롯 표시 데이터</param>
    /// <param name="siblingIndex">현재 표시 순서</param>
    void CreateEffectSlot (
        MaintenanceEffectSlotViewData viewData ,
        int siblingIndex )
    {
        GameObject slotObject = _poolManager.GetFromPool (
            _effectSlotPrefab.gameObject ,
            _managementContent.transform );
        MaintenanceEffectSlotView slot =
            slotObject.GetComponent<MaintenanceEffectSlotView> ( );

        slot.transform.SetSiblingIndex ( siblingIndex );
        slot.Show ( viewData );

        _effectSlotViews.Add ( slot );
    }

    /// <summary>
    /// 정비 목록 표시
    /// </summary>
    /// <param name="viewDatas">정비 슬롯 표시 데이터 목록</param>
    public void ShowMaintenanceList (
        IReadOnlyList<MaintenanceSlotViewData> viewDatas )
    {
        SetContent ( false , true , false );
        ClearEffectSlots ( );
        ClearSlots ( );
        ClearEmployeeSlots ( );

        //현재 정렬 순서대로 정비 슬롯 대여
        for ( int i = 0 ; i < viewDatas.Count ; i++ )
            CreateSlot ( viewDatas [ i ] , i );

        _detailView.HideInstant ( );
        ResetScroll ( );
    }

    /// <summary>
    /// 정비 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">정비 슬롯 표시 데이터</param>
    /// <param name="siblingIndex">현재 정렬 순서</param>
    void CreateSlot (
        MaintenanceSlotViewData viewData , int siblingIndex )
    {
        //같은 정비 아이디의 활성 슬롯 중복 생성 차단
        if ( _slotViews.ContainsKey ( viewData.Id ) )
            return;

        //정비 슬롯 프리팹 전용 풀에서 슬롯 대여
        GameObject slotObject = _poolManager.GetFromPool (
            _slotPrefab.gameObject , _defaultContent );
        MaintenanceSlotView slot =
            slotObject.GetComponent<MaintenanceSlotView> ( );

        //현재 정비 목록 순서 적용
        slot.transform.SetSiblingIndex ( siblingIndex );

        //정비 정보와 선택 입력 연결
        slot.Init ( viewData );
        slot.OnSelected += SelectMaintenance;

        _slotViews.Add ( viewData.Id , slot );
    }

    /// <summary>
    /// 고용 Content 표시
    /// </summary>
    /// <param name="viewDatas">직원 슬롯 표시 데이터 목록</param>
    public void ShowEmployeeContent (
        IReadOnlyList<EmployeeSlotViewData> viewDatas )
    {
        SetContent ( false , false , true );
        ClearEffectSlots ( );
        ClearSlots ( );
        ClearEmployeeSlots ( );

        //현재 직원 표시 순서대로 카드 대여
        for ( int i = 0 ; i < viewDatas.Count ; i++ )
            CreateEmployeeSlot ( viewDatas [ i ] , i );

        _detailView.HideInstant ( );
        ResetScroll ( );
    }

    /// <summary>
    /// 직원 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">직원 슬롯 표시 데이터</param>
    /// <param name="siblingIndex">현재 표시 순서</param>
    void CreateEmployeeSlot (
        EmployeeSlotViewData viewData , int siblingIndex )
    {
        //같은 직원 아이디의 활성 카드 중복 생성 차단
        if ( _employeeSlotViews.ContainsKey ( viewData.EmployeeId ) )
            return;

        //직원 슬롯 프리팹 전용 풀에서 카드 대여
        GameObject slotObject = _poolManager.GetFromPool (
            _employeeSlotPrefab.gameObject ,
            _employeeContent.transform );
        EmployeeSlotView slot =
            slotObject.GetComponent<EmployeeSlotView> ( );

        //현재 직원 목록 순서 적용
        slot.transform.SetSiblingIndex ( siblingIndex );

        //직원 정보와 고용 및 해고 입력 연결
        slot.Init ( viewData );
        slot.OnAction += SelectEmployeeAction;

        _employeeSlotViews.Add ( viewData.EmployeeId , slot );
    }

    /// <summary>
    /// 현재 활성 직원 슬롯 전체 반환
    /// </summary>
    void ClearEmployeeSlots ( )
    {
        foreach ( EmployeeSlotView slot in _employeeSlotViews.Values )
        {
            //게임 종료 중 풀 오브젝트가 먼저 파괴된 경우 건너뜀
            if ( slot == null ) continue;

            //직원 처리 입력과 재사용 상태 초기화
            slot.OnAction -= SelectEmployeeAction;
            slot.ResetForReuse ( );

            //현재 슬롯을 생성한 풀로 반환
            slot.GetComponent<Poolable> ( ).ReturnToPool ( );
        }

        _employeeSlotViews.Clear ( );
    }

    /// <summary>
    /// 정비 상세 패널 표시
    /// </summary>
    /// <param name="viewData">정비 상세 표시 데이터</param>
    public void ShowDetail ( MaintenanceDetailViewData viewData )
    {
        _detailView.ShowPanel ( viewData );
    }

    /// <summary>
    /// 정비 상세 패널 숨김
    /// </summary>
    public void HideDetail ( )
    {
        //현재 선택 슬롯 강조 해제
        SetSelectedSlot ( null );

        _detailView.HidePanel ( );
    }

    /// <summary>
    /// 정비 화면 표시 여부
    /// </summary>
    /// <returns>정비 화면 표시 여부</returns>
    public bool IsShowing ( )
    {
        return _maintenancePanel != null &&
            _maintenancePanel.activeSelf;
    }

    #endregion

    #region ----- 내부 표시 처리 -----

    /// <summary>
    /// 정비 Content 표시 상태 설정
    /// </summary>
    void SetContent ( bool showManagement , bool showDefault , bool showEmployee )
    {
        _managementContent.SetActive ( showManagement );
        _defaultContent.gameObject.SetActive ( showDefault );
        _employeeContent.SetActive ( showEmployee );

        //선택한 탭의 Content를 현재 스크롤 대상으로 교체
        _scrollRect.content = showManagement
            ? ( RectTransform ) _managementContent.transform
            : showEmployee
                ? ( RectTransform ) _employeeContent.transform
                : ( RectTransform ) _defaultContent;
    }

    /// <summary>
    /// 현재 활성 효과 슬롯 전체 반환
    /// </summary>
    void ClearEffectSlots ( )
    {
        for ( int i = 0 ; i < _effectSlotViews.Count ; i++ )
        {
            MaintenanceEffectSlotView slot =
                _effectSlotViews [ i ];

            //게임 종료 중 풀 오브젝트가 먼저 파괴된 경우 건너뜀
            if ( slot == null ) continue;

            slot.ResetForReuse ( );
            slot.GetComponent<Poolable> ( ).ReturnToPool ( );
        }

        _effectSlotViews.Clear ( );
    }

    /// <summary>
    /// 현재 활성 정비 슬롯 전체 반환
    /// </summary>
    void ClearSlots ( )
    {
        foreach ( MaintenanceSlotView slot in _slotViews.Values )
        {
            //게임 종료 중 풀 오브젝트가 먼저 파괴된 경우 건너뜀
            if ( slot == null ) continue;

            //선택 입력과 재사용 상태 초기화
            slot.OnSelected -= SelectMaintenance;
            slot.ResetForReuse ( );

            //현재 슬롯을 생성한 풀로 반환
            slot.GetComponent<Poolable> ( ).ReturnToPool ( );
        }

        _slotViews.Clear ( );
    }

    /// <summary>
    /// 정비 목록 스크롤 최상단 이동
    /// </summary>
    void ResetScroll ( )
    {
        Canvas.ForceUpdateCanvases ( );
        _scrollRect.verticalNormalizedPosition = 1f;
    }

    #endregion

    #region ----- 입력 전달 -----

    void SelectManagement ( ) => OnManagementSelected?.Invoke ( );

    void SelectFacility ( ) => OnFacilitySelected?.Invoke ( );

    void SelectResearch ( ) => OnResearchSelected?.Invoke ( );

    void SelectConvenience ( ) => OnConvenienceSelected?.Invoke ( );

    void SelectEmployee ( ) => OnEmployeeSelected?.Invoke ( );

    /// <summary>
    /// 정비 슬롯 선택 입력 전달
    /// </summary>
    /// <param name="id">정비 아이디</param>
    void SelectMaintenance ( string id )
    {
        //선택한 정비 아이디의 슬롯만 강조
        SetSelectedSlot ( id );

        OnMaintenanceSelected?.Invoke ( id );
    }

    /// <summary>
    /// 선택한 정비 슬롯 강조 설정
    /// </summary>
    /// <param name="id">선택한 정비 아이디</param>
    void SetSelectedSlot ( string id )
    {
        foreach ( var slot in _slotViews )
            slot.Value.SetSelected ( slot.Key == id );
    }

    void Upgrade ( string id )
    {
        OnUpgrade?.Invoke ( id );
    }

    void Close ( )
    {
        OnClose?.Invoke ( );
    }

    /// <summary>
    /// 직원 처리 입력 전달
    /// </summary>
    /// <param name="id">직원 아이디</param>
    void SelectEmployeeAction ( string id )
    {
        OnEmployeeAction?.Invoke ( id );
    }

    /// <summary>
    /// 정비 상세 닫기 입력 전달
    /// </summary>
    void CloseDetail ( )
    {
        OnDetailClose?.Invoke ( );
    }
    #endregion
}
