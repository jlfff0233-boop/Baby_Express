using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카탈로그 프레젠터 - 검색, 필터, 페이지와 파츠 상세 표시 중재
/// </summary>
public class CatalogPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] CatalogView _catalogView;       //카탈로그 뷰

    PurchasableDataMap _dataMap;       //구매 가능 상품 데이터 맵
    InventoryModel _inventoryModel;       //인벤토리 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델

    CatalogListBuilder _listBuilder;       //카탈로그 목록 빌더
    CatalogViewDataBuilder _viewDataBuilder;       //표시 데이터 빌더
    CatalogFilter _filter;       //현재 검색과 필터 상태

    /// <summary>
    /// 현재 조건의 전체 파츠 목록
    /// </summary>
    IReadOnlyList<PartsData> _filteredParts = Array.Empty<PartsData>( );
    /// <summary>
    /// 필터 버튼 순서에 맞춘 테마 목록
    /// </summary>
    List<PartTheme> _themeOptions = new List<PartTheme>( );

    int _pageIndex;       //현재 페이지 인덱스
    CatalogCategoryType _selectedCategory;       //현재 선택한 분류
    bool _isGuideMode;       //잠금 파츠 선두 배치 가이드 여부

    /// <summary>
    /// 카탈로그 닫기 요청 이벤트
    /// </summary>
    public event Action OnCloseRequested;

    /// <summary>
    /// 선택한 파츠의 상점 이동 요청 이벤트(파츠 아이디)
    /// </summary>
    public event Action<string> OnMoveToShopRequested;

    /// <summary>
    /// 새 카탈로그 정보 알림 상태 변경 이벤트(성공 여부)
    /// </summary>
    public event Action<bool> OnNotificationChanged;

    /// <summary>
    /// 카탈로그 파츠 선택 완료 이벤트(파츠 아이디)
    /// </summary>
    public event Action<string> OnPartSelected;

    /// <summary>
    /// 새 카탈로그 정보 알림 여부
    /// </summary>
    public bool HasNotification => HasNewParts( );

    #region ----- 초기화/이벤트 -----
    /// <summary>
    /// 카탈로그 프레젠터 초기화
    /// </summary>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="scoreSettings">제작 점수 설정 데이터</param>
    public void Init (
        PurchasableDataMap dataMap,
        InventoryModel inventoryModel,
        PlayStateModel playStateModel,
        CraftScoreSettingsData scoreSettings )
    {
        _dataMap = dataMap;
        _inventoryModel = inventoryModel;
        _playStateModel = playStateModel;

        _listBuilder = new CatalogListBuilder( );
        _viewDataBuilder =
            new CatalogViewDataBuilder( scoreSettings );
        _filter = new CatalogFilter( );

        _playStateModel.OnItemUnlockChanged += ChangeItemUnlock;

        _catalogView.SetThemeOptions( CreateThemeOptions( ) );
        _catalogView.HideInstant( );
    }

    /// <summary>
    /// 성질과 종족 테마 필터 버튼 항목 생성
    /// </summary>
    /// <returns>전체 항목을 포함한 테마 표시 문자열 목록</returns>
    List<string> CreateThemeOptions ()
    {
        _themeOptions.Clear( );

        var optionTexts = new List<string> { "전체" };
        PartTheme [ ] themes =
            ( PartTheme [ ] ) Enum.GetValues( typeof( PartTheme ) );

        for ( int i = 0; i < themes.Length; i++ )
        {
            PartTheme theme = themes [ i ];
            PartThemeCategory category = theme.GetCategory( );

            //현재 사용하지 않는 레거시 테마는 필터 목록에서 제외
            if ( category == PartThemeCategory.None ) continue;

            _themeOptions.Add( theme );

            string categoryText = category == PartThemeCategory.Trait
                ? "성질"
                : "종족";

            optionTexts.Add(
                $"[{categoryText}] {theme.GetDisplayName( )}" );
        }

        return optionTexts;
    }

    /// <summary>
    /// 카탈로그 입력 연결
    /// </summary>
    void Awake ()
    {
        _catalogView.OnSearchChanged += ChangeSearch;
        _catalogView.OnSortSelected += SelectSort;
        _catalogView.OnCategorySelected += SelectCategory;
        _catalogView.OnFilterSelected += SelectFilter;
        _catalogView.OnSlotSelected += SelectPart;
        _catalogView.OnPreviousRequested += ShowPrevious;
        _catalogView.OnNextRequested += ShowNext;
        _catalogView.OnDetailClosed += CloseDetail;
        _catalogView.OnClosed += RequestClose;
        _catalogView.OnMoveToShop += MoveToShop;
    }

    /// <summary>
    /// 카탈로그 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _playStateModel != null )
            _playStateModel.OnItemUnlockChanged -= ChangeItemUnlock;

        _catalogView.OnSearchChanged -= ChangeSearch;
        _catalogView.OnSortSelected -= SelectSort;
        _catalogView.OnCategorySelected -= SelectCategory;
        _catalogView.OnFilterSelected -= SelectFilter;
        _catalogView.OnSlotSelected -= SelectPart;
        _catalogView.OnPreviousRequested -= ShowPrevious;
        _catalogView.OnNextRequested -= ShowNext;
        _catalogView.OnDetailClosed -= CloseDetail;
        _catalogView.OnClosed -= RequestClose;
        _catalogView.OnMoveToShop -= MoveToShop;
    }
    #endregion

    #region ----- 화면 표시 -----
    /// <summary>
    /// 최신 파츠 상태로 카탈로그 표시
    /// </summary>
    public void Show ()
    {

        MarkAllViewed( );       //신규 파츠 확인 처리
        ResetState( );      //상태 초기화
        _catalogView.ResetInput( );     //검색창 및 필터 초기화
        RefreshList( );     //목록 갱신

    }

    /// <summary>
    /// 카탈로그 가이드용 목록 정렬 사용 여부 설정
    /// </summary>
    /// <param name="isEnabled">잠금 파츠 선두 배치 여부</param>
    public void SetGuideMode ( bool isEnabled )
    {
        _isGuideMode = isEnabled;
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
        return _catalogView.TryGetTutorialTarget(
            targetId, out target );
    }

    /// <summary>
    /// 카탈로그 가이드에서 사용할 해금 파츠 상세 표시 보장
    /// </summary>
    /// <returns>해금 파츠 상세 표시 성공 여부</returns>
    public bool EnsureGuideDetail ()
    {
        //해금 파츠 상세 표시
        if ( _catalogView.IsShowingUnlockedDetail( ) == true )
            return true;

        //해금 파츠 아이디 조회
        if ( _catalogView.TryGetFirstUnlockedPartId(
            out string partId ) == false )
        {
            return false;
        }

        //선택 파츠 상세 표시
        SelectPart( partId );
        //상세 표시 여부 반환
        return _catalogView.IsShowingUnlockedDetail( );
    }

    /// <summary>
    /// 카탈로그 가이드의 목록 대상 표시 보장
    /// </summary>
    public void EnsureGuideList ()
    {
        _catalogView.ShowListMode( );
    }

    /// <summary>
    /// 상품 해금 변경 시 카탈로그 알림 상태 전달
    /// </summary>
    /// <param name="id">변경된 상품 아이디</param>
    /// <param name="isUnlocked">해금 여부</param>
    void ChangeItemUnlock ( string id, bool isUnlocked )
    {
        //파츠 타입인 상품 데이터 조회
        if ( _dataMap.TryGetData(
            id, out PurchasableData data ) == false ||
            data is not PartsData )
        {
            return;
        }

        //카탈로그 변경 이벤트 발행
        OnNotificationChanged?.Invoke( HasNotification );
    }

    /// <summary>
    /// 카탈로그에 포함되는 새 파츠 존재 여부 확인
    /// </summary>
    /// <returns>새 파츠 존재 여부</returns>
    bool HasNewParts ()
    {
        //미확인 해금 상품 아이디 확인
        foreach ( string id in _playStateModel.NewUnlockedIds )
        {
            //파츠 타입인 상품 데이터 가져오기
            if ( _dataMap.TryGetData(
                id, out PurchasableData data ) && data is PartsData )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 카탈로그에 포함되는 모든 새 파츠 확인 처리
    /// </summary>
    void MarkAllViewed ()
    {
        //상품 데이터 목록 가져오기
        IReadOnlyList<PurchasableData> datas =
            _dataMap.PurchasableDatas;

        for ( int i = 0; i < datas.Count; i++ )
        {
            //파츠 타입 상품 해금 처리
            if ( datas [ i ] is PartsData )
                _playStateModel.MarkUnlockViewed( datas [ i ].Id );
        }

        OnNotificationChanged?.Invoke( false );
    }

    /// <summary>
    /// 카탈로그 숨김
    /// </summary>
    public void Hide ()
    {
        _catalogView.Hide( );
    }

    /// <summary>
    /// 카탈로그 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _catalogView.HideInstant( );
    }

    /// <summary>
    /// 카탈로그 상태 초기화
    /// </summary>
    void ResetState ()
    {
        _filter = new CatalogFilter( );
        _pageIndex = 0;
        _selectedCategory = CatalogCategoryType.Part;
    }

    /// <summary>
    /// 현재 검색과 필터 조건으로 전체 목록 갱신
    /// </summary>
    void RefreshList ()
    {
        //필터링된 파츠 목록
        _filteredParts = _listBuilder.Build(
            _dataMap.PurchasableDatas,
            _inventoryModel, _playStateModel, _filter );

        //가이드 중이라면 가이드용 파츠 최상위 정렬
        if ( _isGuideMode == true )
            _filteredParts = CreateGuideOrderedParts( _filteredParts );

        //인덱스 보정
        ClampPageIndex( );
        //현재 페이지 표시 갱신
        RefreshPage( );
    }

    /// <summary>
    /// 기존 정렬을 유지하며 잠금 파츠 하나만 선두로 이동
    /// </summary>
    /// <param name="parts">기존 정렬이 적용된 파츠 목록</param>
    /// <returns>카탈로그 가이드용 파츠 목록</returns>
    IReadOnlyList<PartsData> CreateGuideOrderedParts (
        IReadOnlyList<PartsData> parts )
    {
        int lockedIndex = -1;

        for ( int i = 0; i < parts.Count; i++ )
        {
            //이미 해금된 상품이면 다음으로
            if ( _playStateModel
                .IsUnlocked( parts [ i ].Id ) == true )
                continue;

            lockedIndex = i;
            break;
        }

        //미해금 상품이 없거나 이미 가장 앞에 있다면
        if ( lockedIndex <= 0 )
            return parts;

        //정렬된 목록 생성
        var orderedParts = new List<PartsData>( parts );
        //미해금 파츠 가져오기
        PartsData lockedPart = orderedParts [ lockedIndex ];

        //미해금 파츠 기존 인덱스 정리 후 가장 앞으로 이동
        orderedParts.RemoveAt( lockedIndex );
        orderedParts.Insert( 0, lockedPart );

        //정렬된 목록 반환
        return orderedParts;
    }

    /// <summary>
    /// 현재 페이지 슬롯 표시
    /// </summary>
    void RefreshPage ()
    {
        int capacity = _catalogView.SlotCapacity;
        int startIndex = _pageIndex * capacity;
        //기본 용량과 필터링 이후의 값 중 더 작은 값
        int endIndex = Mathf.Min(
            startIndex + capacity, _filteredParts.Count );

        var viewDatas =
            new List<CatalogSlotViewData>( endIndex - startIndex );

        for ( int i = startIndex; i < endIndex; i++ )
        {
            //파츠 데이터 가져오기
            PartsData part = _filteredParts [ i ];
            //해금 여부 확인
            bool isUnlocked = _playStateModel.IsUnlocked( part.Id );

            viewDatas.Add(
                _viewDataBuilder.CreateSlot( part, isUnlocked ) );
        }

        //목록 보여주기
        _catalogView.ShowList( viewDatas );
        //페이지 이동 버튼 설정
        _catalogView.SetPageNavigation(
            _pageIndex > 0, endIndex < _filteredParts.Count );
    }

    /// <summary>
    /// 현재 페이지 인덱스를 필터 결과 범위로 보정
    /// </summary>
    void ClampPageIndex ()
    {
        if ( _filteredParts.Count == 0 )
        {
            _pageIndex = 0;
            return;
        }

        int lastPageIndex =
            ( _filteredParts.Count - 1 ) / _catalogView.SlotCapacity;

        _pageIndex = Mathf.Clamp( _pageIndex, 0, lastPageIndex );
    }
    #endregion

    #region ----- 검색/필터/정렬 -----
    /// <summary>
    /// 검색어 변경
    /// </summary>
    /// <param name="searchText">입력한 검색어</param>
    void ChangeSearch ( string searchText )
    {
        _filter.SearchText = searchText;
        RefreshFilterResult( );
    }

    /// <summary>
    /// 정렬 타입 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 인덱스</param>
    void SelectSort ( int optionIndex )
    {
        _filter.SortType = ( CatalogSortType ) optionIndex;
        RefreshFilterResult( );
    }

    /// <summary>
    /// 필터 분류 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 인덱스</param>
    void SelectCategory ( int optionIndex )
    {
        _selectedCategory = ( CatalogCategoryType ) optionIndex;

        //분류를 바꾸면 이전 분류의 필터를 함께 해제
        _filter.PartType = null;
        _filter.Theme = null;

        RefreshFilterResult( );
    }

    /// <summary>
    /// 현재 분류의 파츠 또는 테마 필터 선택
    /// </summary>
    /// <param name="optionIndex">선택한 옵션 인덱스</param>
    void SelectFilter ( int optionIndex )
    {
        //선택한 분류가 파츠 타입이면
        if ( _selectedCategory == CatalogCategoryType.Part )
        {
            _filter.PartType = optionIndex == 0
                ? null      //파츠 전체
                : ( PartType? ) ( optionIndex - 1 );        //선택한 파츠 타입
            _filter.Theme = null;
        }
        //테마 타입이면
        else
        {
            _filter.Theme = optionIndex == 0
                ? null      //테마 전체
                : _themeOptions [ optionIndex - 1 ];        //선택 테마 타입
            _filter.PartType = null;
        }

        RefreshFilterResult( );
    }

    /// <summary>
    /// 검색이나 필터 변경 후 첫 페이지 표시
    /// </summary>
    void RefreshFilterResult ()
    {
        _pageIndex = 0;

        RefreshList( );
    }
    #endregion

    #region ----- 페이지 -----
    /// <summary>
    /// 이전 카탈로그 페이지 표시
    /// </summary>
    void ShowPrevious ()
    {
        if ( _pageIndex <= 0 ) return;

        _pageIndex--;

        RefreshPage( );
    }

    /// <summary>
    /// 다음 카탈로그 페이지 표시
    /// </summary>
    void ShowNext ()
    {
        int nextStartIndex =
            ( _pageIndex + 1 ) * _catalogView.SlotCapacity;

        if ( nextStartIndex >= _filteredParts.Count ) return;

        _pageIndex++;

        RefreshPage( );
    }
    #endregion

    #region ----- 상세 -----
    /// <summary>
    /// 선택한 파츠 상세 표시
    /// </summary>
    /// <param name="id">선택한 파츠 아이디</param>
    void SelectPart ( string id )
    {
        //파츠 데이터 가져오기
        if ( GetFilteredPart( id, out PartsData part ) == false )
            return;

        //해금 여부 확인
        bool isUnlocked = _playStateModel.IsUnlocked( id );
        //수량 확인
        int quantity = _inventoryModel.GetQuantity( id );

        //파츠 상세뷰 데이터 가져오기
        CatalogDetailViewData viewData =
            _viewDataBuilder.CreateDetail( part, isUnlocked, quantity );

        //파츠 상세 표시
        _catalogView.ShowDetail( viewData );
        OnPartSelected?.Invoke( id );
    }

    /// <summary>
    /// 현재 필터 결과에서 파츠 조회
    /// </summary>
    /// <param name="id">조회할 파츠 아이디</param>
    /// <param name="part">조회한 파츠 데이터</param>
    /// <returns>조회 성공 여부</returns>
    bool GetFilteredPart ( string id, out PartsData part )
    {
        for ( int i = 0; i < _filteredParts.Count; i++ )
        {
            if ( _filteredParts [ i ].Id != id ) continue;

            //필터링에 걸린 파츠 조회
            part = _filteredParts [ i ];
            return true;
        }

        part = null;
        return false;
    }

    /// <summary>
    /// 상세를 닫고 현재 목록으로 복귀
    /// </summary>
    void CloseDetail ()
    {
        _catalogView.ShowListMode( );
    }
    #endregion

    #region ----- 닫기 -----
    /// <summary>
    /// 선택한 파츠의 상점 이동 요청 전달
    /// </summary>
    /// <param name="id">선택한 파츠 아이디</param>
    void MoveToShop ( string id )
    {
        OnMoveToShopRequested?.Invoke( id );
    }

    /// <summary>
    /// 카탈로그 닫기 요청 전달
    /// </summary>
    void RequestClose ()
    {
        OnCloseRequested?.Invoke( );
    }
    #endregion
}
