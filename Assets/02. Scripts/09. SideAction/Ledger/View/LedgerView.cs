using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 가계부 뷰 - 주간 기록 목록과 선택 상세 입력 관리
/// </summary>
public class LedgerView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //가계부 패널 연출

    [Header( "----- 목록 -----" )]
    [SerializeField] Transform _slotContent;       //주간 기록 슬롯 Content
    [SerializeField] LedgerSlotView [ ] _slots;       //고정 주간 기록 슬롯
    [SerializeField] TMP_Dropdown _monthDropdown;       //조회 월 선택
    [SerializeField] TMP_Dropdown _yearDropdown;       //조회 연도 선택
    [SerializeField] Button _previousButton;       //이전 목록 버튼
    [SerializeField] Button _nextButton;       //다음 목록 버튼

    [Header( "----- 상세 -----" )]
    [SerializeField] GameObject _detailGroup;       //선택 주간 상세 그룹
    [SerializeField] TMP_Text _titleText;       //현재 상세 항목 제목
    [SerializeField] Transform _detailSlotContent;       //상세 슬롯 정렬 영역
    [SerializeField] SettlementSlotView _detailSlotPrefab;       //가계부 상세 슬롯 프리팹

    [Header( "----- 결산 아이콘 -----" )]
    [SerializeField] SettlementIconData _iconData;       //결산 공용 아이콘 데이터

    [Header( "----- 입력 -----" )]
    [SerializeField] Button _backButton;       //상세에서 목록 돌아가기 버튼
    [SerializeField] Button _closeButton;       //가계부 닫기 버튼

    PoolManager _poolManager;       //공용 슬롯 풀 관리자

    List<SettlementSlotView> _activeDetailSlots =
        new List<SettlementSlotView>( );       //현재 표시 중인 상세 슬롯

    /// <summary>
    /// 주차 선택 이벤트
    /// </summary>
    public event Action<int> OnWeekSelected;

    /// <summary>
    /// 연도 선택 이벤트
    /// </summary>
    public event Action<int> OnYearSelected;

    /// <summary>
    /// 월 선택 이벤트
    /// </summary>
    public event Action<int> OnMonthSelected;

    /// <summary>
    /// 이전 목록 요청 이벤트
    /// </summary>
    public event Action OnPreviousRequested;

    /// <summary>
    /// 다음 목록 요청 이벤트
    /// </summary>
    public event Action OnNextRequested;

    /// <summary>
    /// 목록 돌아가기 요청 이벤트
    /// </summary>
    public event Action OnBackRequested;

    /// <summary>
    /// 가계부 닫기 이벤트
    /// </summary>
    public event Action OnClosed;

    /// <summary>
    /// 가계부 상세 슬롯 풀과 아이콘 목록 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        _poolManager =
            GameManager.Instance.PoolManager;
    }

    /// <summary>
    /// 가계부 입력 연결
    /// </summary>
    void Awake ()
    {
        //닫기 버튼 영역을 상세와 슬롯보다 위에 표시
        _closeButton.transform.parent.SetAsLastSibling( );
        _backButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        for ( int i = 0; i < _slots.Length; i++ )
        {
            _slots [ i ].OnSelected += SelectWeek;
            _slots [ i ].Hide( );
        }

        _yearDropdown.onValueChanged.AddListener( SelectYear );
        _monthDropdown.onValueChanged.AddListener( SelectMonth );
        _previousButton.onClick.AddListener( ShowPrevious );
        _nextButton.onClick.AddListener( ShowNext );
        _backButton.onClick.AddListener( Back );
        _closeButton.onClick.AddListener( Close );
    }

    /// <summary>
    /// 가계부 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        for ( int i = 0; i < _slots.Length; i++ )
            _slots [ i ].OnSelected -= SelectWeek;

        _yearDropdown.onValueChanged.RemoveListener( SelectYear );
        _monthDropdown.onValueChanged.RemoveListener( SelectMonth );
        _previousButton.onClick.RemoveListener( ShowPrevious );
        _nextButton.onClick.RemoveListener( ShowNext );
        _backButton.onClick.RemoveListener( Back );
        _closeButton.onClick.RemoveListener( Close );
    }

    /// <summary>
    /// 조회 연도 목록 설정
    /// </summary>
    /// <param name="options">연도 문구 목록</param>
    /// <param name="selectedIndex">선택할 연도 인덱스</param>
    public void SetYearOptions (
        IReadOnlyList<string> options, int selectedIndex )
    {
        SetDropdownOptions( _yearDropdown, options, selectedIndex );
    }

    /// <summary>
    /// 조회 월 목록 설정
    /// </summary>
    /// <param name="options">월 문구 목록</param>
    /// <param name="selectedIndex">선택할 월 인덱스</param>
    public void SetMonthOptions (
        IReadOnlyList<string> options, int selectedIndex )
    {
        SetDropdownOptions( _monthDropdown, options, selectedIndex );
    }

    /// <summary>
    /// 주간 기록 목록 표시
    /// </summary>
    /// <param name="viewDatas">최신순 주간 목록 표시 데이터</param>
    public void ShowList (
        IReadOnlyList<WeeklyLedgerSummaryViewData> viewDatas )
    {
        gameObject.SetActive ( true );
        ClearDetailSlots( );
        ResetSlots( );

        _detailGroup.SetActive( false );
        SetDetailMode( false );

        int count = Mathf.Min( viewDatas.Count, _slots.Length );

        for ( int i = 0; i < count; i++ )
            _slots [ i ].Show( viewDatas [ i ] );

        _panelTween.Show( );
    }

    /// <summary>
    /// 기록 없음 화면 표시
    /// </summary>
    public void ShowEmpty ()
    {
        gameObject.SetActive ( true );
        ClearDetailSlots( );
        ResetSlots( );

        _detailGroup.SetActive( false );
        SetDetailMode( false );

        _panelTween.Show( );
    }

    /// <summary>
    /// 선택한 주간 기록 상세 표시
    /// </summary>
    /// <param name="section">표시할 결산 구역</param>
    public void ShowDetail ( SettlementSectionViewData section )
    {
        ClearDetailSlots( );

        _detailGroup.SetActive( true );
        SetDetailMode( true );

        _titleText.text =
            GetSectionTitle( section.SectionType );

        for ( int i = 0; i < section.Slots.Count; i++ )
            CreateDetailSlot( section.Slots [ i ] );
    }

    /// <summary>
    /// 상세에서 기존 목록으로 돌아가기
    /// </summary>
    public void ShowList ()
    {
        ClearDetailSlots( );

        _detailGroup.SetActive( false );
        SetDetailMode( false );
    }

    /// <summary>
    /// 이전과 다음 이동 버튼 상태 설정
    /// </summary>
    /// <param name="canShowPrevious">이전 대상 존재 여부</param>
    /// <param name="canShowNext">다음 대상 존재 여부</param>
    public void SetNavigation (
        bool canShowPrevious, bool canShowNext )
    {
        _previousButton.interactable = canShowPrevious;
        _nextButton.interactable = canShowNext;
    }

    /// <summary>
    /// 가계부 목록의 첫 선택 가능 주차를 튜토리얼 대상으로 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 주차 선택 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        target = null;

        if ( targetId != TutorialTargetId.LedgerWeekSelector )
            return false;

        for ( int i = 0; i < _slots.Length; i++ )
        {
            if ( _slots [ i ].CanUseTutorialTarget == false )
                continue;

            target = _slots [ i ].SelectTarget;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 선택한 주차 슬롯 강조
    /// </summary>
    /// <param name="week">선택한 결산 주차</param>
    public void SetSelectedWeek ( int week )
    {
        for ( int i = 0; i < _slots.Length; i++ )
            _slots [ i ].SetSelected( _slots [ i ].Week == week );
    }

    /// <summary>
    /// 가계부 숨김
    /// </summary>
    public void Hide ()
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 가계부 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        ClearDetailSlots( );

        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 가계부 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        ClearDetailSlots( );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 고정 주간 기록 슬롯 초기화
    /// </summary>
    void ResetSlots ()
    {
        for ( int i = 0; i < _slots.Length; i++ )
            _slots [ i ].Hide( );
    }

    /// <summary>
    /// Dropdown 선택 항목 설정
    /// </summary>
    /// <param name="dropdown">설정할 Dropdown</param>
    /// <param name="options">표시 문구 목록</param>
    /// <param name="selectedIndex">선택할 인덱스</param>
    void SetDropdownOptions (
        TMP_Dropdown dropdown,
        IReadOnlyList<string> options, int selectedIndex )
    {
        dropdown.ClearOptions( );

        var optionList = new List<string>( options.Count );

        for ( int i = 0; i < options.Count; i++ )
            optionList.Add( options [ i ] );

        dropdown.AddOptions( optionList );
        dropdown.SetValueWithoutNotify( selectedIndex );
        dropdown.RefreshShownValue( );
    }

    /// <summary>
    /// 목록과 상세 입력 상태 전환
    /// </summary>
    /// <param name="isDetail">상세 표시 여부</param>
    void SetDetailMode ( bool isDetail )
    {
        bool isList = isDetail == false;

        _slotContent.gameObject.SetActive( isList );
        _yearDropdown.gameObject.SetActive( isList );
        _monthDropdown.gameObject.SetActive( isList );

        //목록에서는 월, 상세에서는 항목 이동에 사용합니다.
        _previousButton.gameObject.SetActive( true );
        _nextButton.gameObject.SetActive( true );

        _backButton.gameObject.SetActive( isDetail );
    }

    /// <summary>
    /// 가계부 상세 슬롯 하나 생성
    /// </summary>
    /// <param name="viewData">결산 슬롯 표시 데이터</param>
    void CreateDetailSlot ( SettlementSlotViewData viewData )
    {
        GameObject instance =
            _poolManager.GetFromPool(
                _detailSlotPrefab.gameObject,
                _detailSlotContent );

        SettlementSlotView slotView =
            instance.GetComponent<SettlementSlotView>( );

        slotView.Init(
            viewData,
            _iconData.GetIcon( viewData.IconType ) );

        _activeDetailSlots.Add( slotView );
    }

    /// <summary>
    /// 현재 가계부 상세 슬롯 전체 반환
    /// </summary>
    void ClearDetailSlots ()
    {
        for ( int i = 0; i < _activeDetailSlots.Count; i++ )
        {
            SettlementSlotView slotView =
                _activeDetailSlots [ i ];

            slotView.ResetView( );
            slotView
                .GetComponent<Poolable>( )
                .ReturnToPool( );
        }

        _activeDetailSlots.Clear( );
    }

    /// <summary>
    /// 결산 구역에 대응하는 상세 제목 반환
    /// </summary>
    /// <param name="sectionType">결산 구역 종류</param>
    /// <returns>가계부 상세 제목</returns>
    string GetSectionTitle (
        SettlementSectionType sectionType )
    {
        switch ( sectionType )
        {
            case SettlementSectionType.Summary:
                return "[ 결산 요약 ]";

            case SettlementSectionType.Operation:
                return "[ 운영 현황 ]";

            case SettlementSectionType.OrderResult:
                return "[ 주문 결과 ]";

            case SettlementSectionType.Evaluation:
                return "[ 주문 평가 ]";

            case SettlementSectionType.Economy:
                return "[ 경제 현황 ]";

            case SettlementSectionType.PurchaseDetail:
                return "[ 구매 상세 ]";

            case SettlementSectionType.UsedPartDetail:
                return "[ 소비 파츠 상세 ]";

            case SettlementSectionType.QuickRestockDetail:
                return "[ 빠른 재입고 상세 ]";

            case SettlementSectionType.Employee:
                return "[ 직원 현황 ]";

            case SettlementSectionType.NextWeek:
                return "[ 다음 주 변화 ]";

            default:
                return "[ 상세 ]";
        }
    }

    /// <summary>
    /// 선택한 주차 전달
    /// </summary>
    /// <param name="week">선택한 결산 주차</param>
    void SelectWeek ( int week )
    {
        OnWeekSelected?.Invoke( week );
    }

    /// <summary>
    /// 조회 연도 선택 전달
    /// </summary>
    /// <param name="optionIndex">선택한 연도 인덱스</param>
    void SelectYear ( int optionIndex )
    {
        OnYearSelected?.Invoke( optionIndex );
    }

    /// <summary>
    /// 조회 월 선택 전달
    /// </summary>
    /// <param name="optionIndex">선택한 월 인덱스</param>
    void SelectMonth ( int optionIndex )
    {
        OnMonthSelected?.Invoke( optionIndex );
    }

    /// <summary>
    /// 이전 목록 요청 전달
    /// </summary>
    void ShowPrevious ()
    {
        OnPreviousRequested?.Invoke( );
    }

    /// <summary>
    /// 다음 목록 요청 전달
    /// </summary>
    void ShowNext ()
    {
        OnNextRequested?.Invoke( );
    }

    /// <summary>
    /// 상세에서 목록 돌아가기 요청 전달
    /// </summary>
    void Back ()
    {
        OnBackRequested?.Invoke( );
    }

    /// <summary>
    /// 가계부 닫기 요청
    /// </summary>
    void Close ()
    {
        OnClosed?.Invoke( );
    }
}
