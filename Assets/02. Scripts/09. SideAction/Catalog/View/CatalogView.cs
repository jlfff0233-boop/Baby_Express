using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 카탈로그 뷰 - 파츠 목록, 상세, 검색과 필터 입력 관리
/// </summary>
public class CatalogView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //카탈로그 패널 연출

    [Header ( "----- 목록 -----" )]
    [SerializeField] GameObject _listPanel;       //파츠 목록 패널
    [SerializeField] Transform _leftGroup;       //왼쪽 슬롯 그룹
    [SerializeField] Transform _rightGroup;       //오른쪽 슬롯 그룹
    [SerializeField] CatalogSlotView _slotPrefab;       //파츠 슬롯 프리팹

    [Header ( "----- 목록 입력 -----" )]
    [SerializeField] GameObject _actionBar;       //검색과 필터 그룹
    [SerializeField] TMP_InputField _searchInput;       //이름 검색
    [SerializeField] Button _partButton;       //파츠 분류 버튼
    [SerializeField] Button _themeButton;       //테마 분류 버튼
    [FormerlySerializedAs( "_partTypeDropdown" )]
    [SerializeField] TMP_Dropdown _filterDropdown;       //현재 분류의 세부 항목 선택
    [SerializeField] TMP_Dropdown _sortDropdown;       //정렬 선택

    [Header ( "----- 페이지 -----" )]
    [SerializeField] GameObject _pageButtonGroup;       //페이지 이동 버튼 그룹
    [SerializeField] Button _previousButton;       //이전 페이지 버튼
    [SerializeField] Button _nextButton;       //다음 페이지 버튼

    [Header ( "----- 상세 -----" )]
    [SerializeField] CatalogDetailView _detailView;       //파츠 상세 뷰

    [Header ( "----- 입력 -----" )]
    [SerializeField] Button _closeButton;       //카탈로그 닫기 버튼

    /// <summary>
    /// 정렬 옵션
    /// </summary>
    static readonly string [ ] _sortOptions =
    {
        "정렬",
        "이름 오름차순",
        "이름 내림차순",
        "아이디 오름차순",
        "아이디 내림차순",
        "해금 우선",
        "보유 우선",
    };

    List<CatalogSlotView> _slots = new List<CatalogSlotView> ( );       //현재 생성된 슬롯
    List<string> _partFilterOptions = new List<string>( );       //파츠 세부 분류 문구
    List<string> _themeFilterOptions = new List<string>( );       //테마 세부 분류 문구
    CatalogCategoryType _selectedCategory;       //현재 선택한 분류
    int _arrangedSlotCount = -1;       //마지막으로 배치한 슬롯 수
    int _arrangedLeftCapacity = -1;       //마지막으로 사용한 왼쪽 그룹 용량

    /// <summary>
    /// 페이지당 표시 가능한 슬롯 수
    /// </summary>
    public int SlotCapacity =>
        GetGroupCapacity ( _leftGroup ) +
        GetGroupCapacity ( _rightGroup );

    #region ----- 이벤트 -----
    /// <summary>
    /// 검색어 변경 이벤트(입력값)
    /// </summary>
    public event Action<string> OnSearchChanged;

    /// <summary>
    /// 정렬 선택 이벤트
    /// </summary>
    public event Action<int> OnSortSelected;

    /// <summary>
    /// 분류 선택 이벤트
    /// </summary>
    public event Action<int> OnCategorySelected;

    /// <summary>
    /// 현재 분류의 필터 선택 이벤트
    /// </summary>
    public event Action<int> OnFilterSelected;

    /// <summary>
    /// 파츠 슬롯 선택 이벤트
    /// </summary>
    public event Action<string> OnSlotSelected;

    /// <summary>
    /// 이전 페이지 요청 이벤트
    /// </summary>
    public event Action OnPreviousRequested;

    /// <summary>
    /// 다음 페이지 요청 이벤트
    /// </summary>
    public event Action OnNextRequested;

    /// <summary>
    /// 상세 닫기 이벤트
    /// </summary>
    public event Action OnDetailClosed;

    /// <summary>
    /// 카탈로그 닫기 이벤트
    /// </summary>
    public event Action OnClosed;

    /// <summary>
    /// 선택한 파츠의 상점 이동 이벤트
    /// </summary>
    public event Action<string> OnMoveToShop;
    #endregion

    #region ----- 이벤트 연결/해제 -----
    /// <summary>
    /// 카탈로그 입력 연결
    /// </summary>
    void Awake ( )
    {
        InitDropdowns ( );

        _searchInput.onValueChanged.AddListener ( ChangeSearch );
        _partButton.onClick.AddListener( SelectPartCategory );
        _themeButton.onClick.AddListener( SelectThemeCategory );
        _filterDropdown.onValueChanged.AddListener( SelectFilter );
        _sortDropdown.onValueChanged.AddListener ( SelectSort );

        _previousButton.onClick.AddListener ( ShowPrevious );
        _nextButton.onClick.AddListener ( ShowNext );
        _closeButton.onClick.AddListener ( Close );

        _detailView.OnClosed += CloseDetail;
        _detailView.OnMoveToShop += MoveToShop;

        ShowListMode ( );
    }

    /// <summary>
    /// 카탈로그 고정 드롭다운 표시 항목 초기화
    /// </summary>
    void InitDropdowns ( )
    {
        _partFilterOptions = new List<string> (
            ( int ) PartType.Count + 1 )
        {
            "전체",
        };

        for ( int i = 0 ; i < ( int ) PartType.Count ; i++ )
        {
            _partFilterOptions.Add(
                ( ( PartType ) i ).GetDisplayName( ) );
        }

        _sortDropdown.SetOptions ( _sortOptions );
        _filterDropdown.SetOptions( _partFilterOptions );

        _selectedCategory = CatalogCategoryType.Part;
        SetSelectedCategoryButton( );

        _partButton.BindClickHighlight( );
        _themeButton.BindClickHighlight( );
    }

    /// <summary>
    /// 카탈로그 입력 해제
    /// </summary>
    void OnDestroy ( )
    {
        _searchInput.onValueChanged.RemoveListener ( ChangeSearch );
        _partButton.onClick.RemoveListener( SelectPartCategory );
        _themeButton.onClick.RemoveListener( SelectThemeCategory );
        _filterDropdown.onValueChanged.RemoveListener( SelectFilter );
        _sortDropdown.onValueChanged.RemoveListener ( SelectSort );

        _previousButton.onClick.RemoveListener ( ShowPrevious );
        _nextButton.onClick.RemoveListener ( ShowNext );
        _closeButton.onClick.RemoveListener ( Close );

        _detailView.OnClosed -= CloseDetail;
        _detailView.OnMoveToShop -= MoveToShop;

        for ( int i = 0 ; i < _slots.Count ; i++ )
            _slots [ i ].OnSelected -= SelectSlot;
    }

    #endregion

    #region ----- 화면 표시 -----
    /// <summary>
    /// 카탈로그 숨김
    /// </summary>
    public void Hide ( )
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 카탈로그 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 현재 페이지의 파츠 슬롯 표시
    /// </summary>
    /// <param name="viewDatas">파츠 슬롯 표시 데이터 목록</param>
    public void ShowList (
        IReadOnlyList<CatalogSlotViewData> viewDatas )
    {
        gameObject.SetActive ( true );
        ShowListMode ( );

        EnsureSlotCount ( viewDatas.Count );
        ArrangeSlots ( );

        for ( int i = 0 ; i < viewDatas.Count ; i++ )
            _slots [ i ].Show ( viewDatas [ i ] );

        //현재 페이지에서 사용하지 않는 기존 슬롯 숨김
        for ( int i = viewDatas.Count ; i < _slots.Count ; i++ )
            _slots [ i ].Hide ( );

        _panelTween.Show ( );
    }

    /// <summary>
    /// 카탈로그 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 선택한 파츠 상세 표시
    /// </summary>
    /// <param name="viewData">파츠 상세 표시 데이터</param>
    public void ShowDetail ( CatalogDetailViewData viewData )
    {
        SetDetailMode ( );
        _detailView.Show ( viewData );
    }

    /// <summary>
    /// 카탈로그 가이드 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 튜토리얼 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>현재 활성 대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.CatalogLockedPart:
                return TryGetSlotTutorialTarget( true, out target );

            case TutorialTargetId.CatalogPart:
                return TryGetSlotTutorialTarget( false, out target );

            case TutorialTargetId.CatalogSearch:
                target = _searchInput.transform as RectTransform;
                return _searchInput.gameObject.activeInHierarchy &&
                    target != null;

            case TutorialTargetId.CatalogFilter:
                target = _filterDropdown.transform as RectTransform;
                return _filterDropdown.gameObject.activeInHierarchy &&
                    target != null;

            case TutorialTargetId.CatalogShopLink:
                return _detailView.TryGetShopTutorialTarget(
                    out target );

            default:
                target = null;
                return false;
        }
    }

    /// <summary>
    /// 현재 페이지의 첫 해금 파츠 아이디 조회
    /// </summary>
    /// <param name="partId">조회한 해금 파츠 아이디</param>
    /// <returns>해금 파츠 조회 성공 여부</returns>
    public bool TryGetFirstUnlockedPartId ( out string partId )
    {
        for ( int i = 0; i < _slots.Count; i++ )
        {
            CatalogSlotView slot = _slots [ i ];

            if ( slot.gameObject.activeInHierarchy == false ||
                slot.IsLocked || string.IsNullOrEmpty( slot.Id ) )
            {
                continue;
            }

            partId = slot.Id;
            return true;
        }

        partId = null;
        return false;
    }

    /// <summary>
    /// 현재 해금 파츠 상세 표시 여부 확인
    /// </summary>
    /// <returns>해금 파츠 상세 표시 여부</returns>
    public bool IsShowingUnlockedDetail ()
    {
        return _detailView.IsShowingUnlockedPart;
    }

    /// <summary>
    /// 상세를 닫고 기존 목록 표시
    /// </summary>
    public void ShowListMode ( )
    {
        _listPanel.SetActive ( true );
        _actionBar.SetActive ( true );
        _pageButtonGroup.SetActive ( true );

        _detailView.Hide ( );
    }

    /// <summary>
    /// 상세 화면 상태로 전환
    /// </summary>
    void SetDetailMode ( )
    {
        _listPanel.SetActive ( false );
        _actionBar.SetActive ( false );
        _pageButtonGroup.SetActive ( false );
    }

    /// <summary>
    /// 현재 페이지에서 잠금 상태가 일치하는 첫 슬롯 영역 조회
    /// </summary>
    /// <param name="isLocked">조회할 잠금 여부</param>
    /// <param name="target">조회한 슬롯 영역</param>
    /// <returns>슬롯 영역 조회 성공 여부</returns>
    bool TryGetSlotTutorialTarget (
        bool isLocked, out RectTransform target )
    {
        for ( int i = 0; i < _slots.Count; i++ )
        {
            CatalogSlotView slot = _slots [ i ];

            if ( slot.IsLocked != isLocked ) continue;

            if ( slot.TryGetTutorialTarget( out target ) )
                return true;
        }

        target = null;
        return false;
    }
    #endregion

    #region ----- 목록 관리 -----
    /// <summary>
    /// 현재 페이지 표시에 필요한 슬롯 수 확보
    /// </summary>
    /// <param name="count">필요한 슬롯 수</param>
    void EnsureSlotCount ( int count )
    {
        while ( _slots.Count < count )
            CreateSlot ( );
    }

    /// <summary>
    /// 재사용할 카탈로그 파츠 슬롯 생성
    /// </summary>
    void CreateSlot ( )
    {
        CatalogSlotView slot = Instantiate ( _slotPrefab , _leftGroup );

        slot.OnSelected += SelectSlot;
        _slots.Add ( slot );
        slot.Hide ( );
    }

    /// <summary>
    /// 왼쪽 그리드를 먼저 채우도록 재사용 슬롯 재배치
    /// </summary>
    void ArrangeSlots ( )
    {
        int leftCapacity = GetGroupCapacity ( _leftGroup );

        //슬롯 수와 그룹 용량이 그대로면 기존 배치 유지
        if ( _arrangedSlotCount == _slots.Count &&
            _arrangedLeftCapacity == leftCapacity )
            return;

        for ( int i = 0 ; i < _slots.Count ; i++ )
        {
            Transform parent = i < leftCapacity
                ? _leftGroup
                : _rightGroup;

            _slots [ i ].transform.SetParent ( parent , false );
        }

        _arrangedSlotCount = _slots.Count;
        _arrangedLeftCapacity = leftCapacity;
    }

    /// <summary>
    /// 현재 그리드 크기에 표시 가능한 슬롯 수 계산
    /// </summary>
    /// <param name="group">계산할 슬롯 그룹</param>
    /// <returns>그룹에 표시 가능한 슬롯 수</returns>
    int GetGroupCapacity ( Transform group )
    {
        RectTransform rectTransform = ( RectTransform ) group;
        GridLayoutGroup layoutGroup =
            group.GetComponent<GridLayoutGroup> ( );

        float width = rectTransform.rect.width -
            layoutGroup.padding.horizontal;
        float height = rectTransform.rect.height -
            layoutGroup.padding.vertical;

        int columnCount = Mathf.Max ( 1 , Mathf.FloorToInt (
            ( width + layoutGroup.spacing.x ) /
            ( layoutGroup.cellSize.x + layoutGroup.spacing.x ) ) );
        int rowCount = Mathf.Max ( 1 , Mathf.FloorToInt (
            ( height + layoutGroup.spacing.y ) /
            ( layoutGroup.cellSize.y + layoutGroup.spacing.y ) ) );

        if ( layoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount )
            columnCount = layoutGroup.constraintCount;
        else if ( layoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount )
            rowCount = layoutGroup.constraintCount;

        return columnCount * rowCount;
    }

    #endregion

    #region ----- 입력 상태 -----
    /// <summary>
    /// 테마 세부 분류 항목 설정
    /// </summary>
    /// <param name="optionTexts">전체 항목을 포함한 테마 표시 문자열 목록</param>
    public void SetThemeOptions ( List<string> optionTexts )
    {
        _themeFilterOptions = optionTexts;

        if ( _selectedCategory == CatalogCategoryType.Theme )
            SetFilterOptions( _themeFilterOptions );
    }

    /// <summary>
    /// 현재 분류에 사용할 세부 Dropdown 항목 표시
    /// </summary>
    /// <param name="optionTexts">Dropdown 순서에 맞춘 표시 문구</param>
    void SetFilterOptions (
        IReadOnlyList<string> optionTexts )
    {
        _filterDropdown.SetOptions( optionTexts );
        _filterDropdown.SetValueWithoutNotify( 0 );
        _filterDropdown.RefreshShownValue( );
    }

    /// <summary>
    /// 페이지 이동 버튼 상태 설정
    /// </summary>
    /// <param name="canShowPrevious">이전 페이지 존재 여부</param>
    /// <param name="canShowNext">다음 페이지 존재 여부</param>
    public void SetPageNavigation (
        bool canShowPrevious , bool canShowNext )
    {
        _previousButton.interactable = canShowPrevious;
        _nextButton.interactable = canShowNext;
    }

    /// <summary>
    /// 검색과 필터 입력 초기화
    /// </summary>
    public void ResetInput ( )
    {
        _searchInput.SetTextWithoutNotify ( string.Empty );
        _sortDropdown.SetValueWithoutNotify ( 0 );

        _sortDropdown.RefreshShownValue ( );

        _selectedCategory = CatalogCategoryType.Part;
        SetFilterOptions( _partFilterOptions );
        SetSelectedCategoryButton( );
    }

    /// <summary>
    /// 파츠와 테마 분류 버튼 선택 상태 표시
    /// </summary>
    void SetSelectedCategoryButton ()
    {
        _partButton.interactable =
            _selectedCategory != CatalogCategoryType.Part;
        _themeButton.interactable =
            _selectedCategory != CatalogCategoryType.Theme;
    }
    #endregion

    #region ----- 입력 전달 -----
    /// <summary>
    /// 검색어 변경 전달
    /// </summary>
    /// <param name="searchText">입력한 검색어</param>
    void ChangeSearch ( string searchText )
    {
        OnSearchChanged?.Invoke ( searchText );
    }

    /// <summary>
    /// 정렬 선택 전달
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 인덱스</param>
    void SelectSort ( int optionIndex )
    {
        OnSortSelected?.Invoke ( optionIndex );
    }

    /// <summary>
    /// 파츠 분류 선택 전달
    /// </summary>
    void SelectPartCategory ()
    {
        SelectCategory( CatalogCategoryType.Part );
    }

    /// <summary>
    /// 테마 분류 선택 전달
    /// </summary>
    void SelectThemeCategory ()
    {
        SelectCategory( CatalogCategoryType.Theme );
    }

    /// <summary>
    /// 분류 선택과 세부 Dropdown 교체
    /// </summary>
    /// <param name="category">선택한 분류</param>
    void SelectCategory ( CatalogCategoryType category )
    {
        _selectedCategory = category;

        SetFilterOptions(
            _selectedCategory == CatalogCategoryType.Part
                ? _partFilterOptions
                : _themeFilterOptions );

        SetSelectedCategoryButton( );
        OnCategorySelected?.Invoke( ( int ) category );
    }

    /// <summary>
    /// 현재 분류의 필터 선택 전달
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 인덱스</param>
    void SelectFilter ( int optionIndex )
    {
        OnFilterSelected?.Invoke( optionIndex );
    }

    /// <summary>
    /// 선택한 파츠 아이디 전달
    /// </summary>
    /// <param name="id">선택한 파츠 아이디</param>
    void SelectSlot ( string id )
    {
        //현재 표시 중인 파츠 아이디와 일치하는 슬롯만 강조
        for ( int i = 0; i < _slots.Count; i++ )
        {
            bool isSelected =
                _slots [ i ].gameObject.activeSelf &&
                _slots [ i ].Id == id;

            _slots [ i ].SetSelected( isSelected );
        }

        //실제 파츠 선택 처리는 프레젠터로 전달
        OnSlotSelected?.Invoke ( id );
    }

    /// <summary>
    /// 이전 페이지 요청 전달
    /// </summary>
    void ShowPrevious ( )
    {
        OnPreviousRequested?.Invoke ( );
    }

    /// <summary>
    /// 다음 페이지 요청 전달
    /// </summary>
    void ShowNext ( )
    {
        OnNextRequested?.Invoke ( );
    }

    /// <summary>
    /// 상세 닫기 요청 전달
    /// </summary>
    void CloseDetail ( )
    {
        OnDetailClosed?.Invoke ( );
    }

    /// <summary>
    /// 선택한 파츠의 상점 이동 요청 전달
    /// </summary>
    /// <param name="id">선택한 파츠 아이디</param>
    void MoveToShop ( string id )
    {
        OnMoveToShop?.Invoke ( id );
    }

    /// <summary>
    /// 카탈로그 닫기 요청 전달
    /// </summary>
    void Close ( )
    {
        OnClosed?.Invoke ( );
    }
    #endregion
}
