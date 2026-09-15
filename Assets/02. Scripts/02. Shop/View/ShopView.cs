using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 상점 뷰 - 상품 슬롯 생성, 조회, 갱신, 선택 중계
/// </summary>
public class ShopView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //상점 패널 연출
    [SerializeField] Transform _content;       //판매 슬롯 생성 위치
    [SerializeField] ShopSlotView _slotPrefab;      //상점 슬롯 프리팹
    [SerializeField] ShopDetailView _detailView;       //상품 상세 뷰
    [SerializeField] WarningView _warningView;       //빠른 재입고 경고 뷰

    [SerializeField] Button _allButton;        //전체 상품 선택 버튼
    [SerializeField] Button _partButton;       //파츠 선택 버튼
    [SerializeField] Button _facilityButton;       //시설 선택 버튼
    [SerializeField] Button _consumableButton;     //소모용품 선택 버튼
    [SerializeField] TMP_Dropdown _filterDropdown;     //현재 카테고리 분류 선택
    [SerializeField] TMP_Dropdown _sortDropdown;       //상품 정렬 선택
    [SerializeField] ScrollRect _scrollRect;        //상품 목록 스크롤
    [SerializeField] Button _closeButton;       //상점 닫기 버튼

    [SerializeField] Button _cartButton;       //장바구니 열기 버튼

    /// <summary>
    /// 전체 옵션
    /// </summary>
    static readonly string [ ] _allOptions =
    {
        "전체",
    };

    /// <summary>
    /// 시설 옵션
    /// </summary>
    static readonly string [ ] _facilityOptions =
    {
        "시설 전체",
        "장비",
        "가구",
        "확장",
    };

    /// <summary>
    /// 소모용품 옵션
    /// </summary>
    static readonly string [ ] _consumableOptions =
    {
        "소모용품 전체",
    };

    /// <summary>
    /// 정렬 옵션
    /// </summary>
    static readonly string [ ] _sortOptions =
    {
        "기본",
        "이름 오름차순",
        "이름 내림차순",
        "가격 낮은 순",
        "가격 높은 순",
        "수량 적은 순",
        "수량 많은 순",
    };

    /// <summary>
    /// 상품 슬롯 딕셔너리(아이디, 상품 뷰)
    /// </summary>
    Dictionary<string , ShopSlotView> _views = new Dictionary<string , ShopSlotView> ( );
    List<string> _partOptions;        //파츠 분류 옵션
    ShopCategoryType _selectedCategory;       //현재 메인 카테고리
    PoolManager _poolManager;       //공용 슬롯 오브젝트 풀 관리자

    #region ----- 이벤트 -----
    /// <summary>
    /// 상점 닫기 이벤트
    /// </summary>
    public event Action OnClose;
    /// <summary>
    /// 장바구니 열기 이벤트
    /// </summary>
    public event Action OnCartOpen;

    /// <summary>
    /// 상품 선택 이벤트(아이디)
    /// </summary>
    public event Action<string> OnSlotSelected;
    /// <summary>
    /// 전체 상품 선택 이벤트
    /// </summary>
    public event Action OnAllSelected;

    /// <summary>
    /// 파츠 타입 선택 이벤트(타입)
    /// </summary>
    public event Action<int> OnPartSelected;

    /// <summary>
    /// 시설 타입 선택 이벤트(타입)
    /// </summary>
    public event Action<int> OnFacilitySelected;

    /// <summary>
    /// 소모용품 선택 이벤트
    /// </summary>
    public event Action OnConsumableSelected;

    /// <summary>
    /// 상품 정렬 선택 이벤트(정렬 타입)
    /// </summary>
    public event Action<int> OnSortSelected;

    /// <summary>
    /// 상세 패널 수량 변경 이벤트(아이디, 수량)
    /// </summary>
    public event Action<string , int> OnDetailQuantityChanged;

    /// <summary>
    /// 상세 패널 수량 설정 이벤트(아이디, 입력값)
    /// </summary>
    public event Action<string , string> OnDetailQuantitySet;

    /// <summary>
    /// 상세 패널 최소 수량 설정 이벤트(아이디)
    /// </summary>
    public event Action<string> OnDetailQuantityMin;

    /// <summary>
    /// 상세 패널 최대 수량 설정 이벤트(아이디)
    /// </summary>
    public event Action<string> OnDetailQuantityMax;

    /// <summary>
    /// 상세 패널 상품 장바구니 추가 이벤트(아이디, 입력값)
    /// </summary>
    public event Action<string , string> OnDetailAddCart;

    /// <summary>
    /// 상세 상품 빠른 재입고 이벤트
    /// </summary>
    public event Action<string> OnDetailQuickRestock;

    /// <summary>
    /// 빠른 재입고 경고 확인 이벤트
    /// </summary>
    public event Action OnQuickRestockConfirmed;

    /// <summary>
    /// 빠른 재입고 경고 취소 이벤트
    /// </summary>
    public event Action OnQuickRestockCanceled;

    /// <summary>
    /// 상세 패널 닫기 요청 이벤트
    /// </summary>
    public event Action OnDetailClose;
    #endregion

    /// <summary>
    /// 비활성 상태에서도 사용할 런타임 값 초기화
    /// </summary>
    public void InitializeRuntime ( )
    {
        _poolManager = GameManager.Instance.PoolManager;
        InitDropdowns ( );
    }

    /// <summary>
    /// 입력 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        InitDropdowns ( );

        //메인 카테고리 버튼 연결
        _allButton.onClick.AddListener ( SelectAllCategory );
        _partButton.onClick.AddListener ( SelectPartCategory );
        _facilityButton.onClick.AddListener ( SelectFacilityCategory );
        _consumableButton.onClick.AddListener ( SelectConsumableCategory );

        //분류와 정렬 드롭다운 연결
        _filterDropdown.onValueChanged.AddListener ( SelectFilter );
        _sortDropdown.onValueChanged.AddListener ( SelectSort );

        //상세 패널 입력 연결
        _detailView.OnQuantityChanged += ChangeDetailQuantity;
        _detailView.OnQuantitySet += SetDetailQuantity;
        _detailView.OnQuantityMin += SetDetailMin;
        _detailView.OnQuantityMax += SetDetailMax;
        _detailView.OnAddCart += AddDetailToCart;
        _detailView.OnClose += CloseDetailPanel;
        _detailView.OnQuickRestock += QuickRestock;

        //빠른 재입고 경고 입력 연결
        _warningView.OnConfirmed += ConfirmQuickRestock;
        _warningView.OnCanceled += CancelQuickRestock;
        _warningView.HideInstant ( );

        //상세 패널 초기 숨김
        _detailView.HideInstant ( );

        //상점 닫기 버튼 연결
        _closeButton.onClick.AddListener ( ClosePanel );

        //장바구니 열기 버튼 연결
        _cartButton.onClick.AddListener ( OpenCart );
    }

    /// <summary>
    /// 상점 드롭다운 표시 항목 초기화
    /// </summary>
    void InitDropdowns ( )
    {
        //이미 준비한 드롭다운을 다시 초기화하지 않음
        if ( _partOptions != null ) return;

        _partOptions = new List<string> ( ( int ) PartType.Count + 1 )
        {
            "파츠 전체",
        };

        for ( int i = 0 ; i < ( int ) PartType.Count ; i++ )
        {
            _partOptions.Add (
                ( ( PartType ) i ).GetDisplayName ( ) );
        }

        SetFilterOptions ( _selectedCategory );
        _sortDropdown.SetOptions ( _sortOptions );
    }

    /// <summary>
    /// 비활성화 전에 활성 상품 슬롯 반환
    /// </summary>
    void OnDisable ( )
    {
        ClearSlots ( );
    }

    /// <summary>
    /// 입력 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        //메인 카테고리 버튼 연결 해제
        _allButton.onClick.RemoveListener ( SelectAllCategory );
        _partButton.onClick.RemoveListener ( SelectPartCategory );
        _facilityButton.onClick.RemoveListener ( SelectFacilityCategory );
        _consumableButton.onClick.RemoveListener ( SelectConsumableCategory );

        //분류와 정렬 드롭다운 연결 해제
        _filterDropdown.onValueChanged.RemoveListener ( SelectFilter );
        _sortDropdown.onValueChanged.RemoveListener ( SelectSort );

        //상세 패널 입력 연결 해제
        _detailView.OnQuantityChanged -= ChangeDetailQuantity;
        _detailView.OnQuantitySet -= SetDetailQuantity;
        _detailView.OnQuantityMin -= SetDetailMin;
        _detailView.OnQuantityMax -= SetDetailMax;
        _detailView.OnAddCart -= AddDetailToCart;
        _detailView.OnClose -= CloseDetailPanel;
        _detailView.OnQuickRestock -= QuickRestock;

        _warningView.OnConfirmed -= ConfirmQuickRestock;
        _warningView.OnCanceled -= CancelQuickRestock;

        //상점 닫기 버튼 연결 해제
        _closeButton.onClick.RemoveListener ( ClosePanel );

        //장바구니 열기 버튼 연결 해제
        _cartButton.onClick.RemoveListener ( OpenCart );

    }

    #region ----- 상품 슬롯 -----
    /// <summary>
    /// 상품 선택 이벤트 발행
    /// </summary>
    /// <param name="id">선택한 상품 아이디</param>
    void SelectSlot ( string id )
    {
        //현재 표시 중인 상품 아이디와 일치하는 슬롯만 강조
        foreach ( var pair in _views )
        {
            bool isSelected =
                pair.Value.gameObject.activeSelf &&
                pair.Key == id;

            pair.Value.SetSelected( isSelected );
        }

        //실제 상품 선택 처리는 Presenter로 전달
        OnSlotSelected?.Invoke ( id );
    }

    /// <summary>
    /// 상품 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">상품 표시 데이터</param>
    /// <param name="siblingIndex">현재 정렬 순서</param>
    void CreateSlot (
        ShopSlotViewData viewData, int siblingIndex )
    {
        //표시 데이터/아이디 null 확인
        if ( viewData == null || string.IsNullOrEmpty ( viewData.Id ) ) return;

        //현재 활성 목록의 상품 아이디 중복 생성 차단
        if ( _views.ContainsKey( viewData.Id ) ) return;

        //프리팹 전용 공용 풀에서 상품 슬롯 대여
        GameObject slotObject = _poolManager.GetFromPool(
            _slotPrefab.gameObject, _content );
        ShopSlotView view = slotObject.GetComponent<ShopSlotView>( );
        view.transform.SetSiblingIndex( siblingIndex );

        //상품 데이터와 인스턴스 최초 등장 연출 초기화
        view.Init( viewData );

        //상품 선택 이벤트 연결
        view.OnSelected += SelectSlot;

        //상품을 딕셔너리에 추가
        _views.Add ( viewData.Id , view );

    }

    /// <summary>
    /// 상품 슬롯을 공용 풀로 반환
    /// </summary>
    /// <param name="view">제거할 상품 뷰</param>
    void RemoveSlot ( ShopSlotView view )
    {
        //씬 종료 중 이미 파괴된 슬롯은 별도 반환하지 않음
        if ( view == null ) return;

        //상품 선택 이벤트 연결 해제
        view.OnSelected -= SelectSlot;

        //재사용 상태 초기화 후 생성된 풀로 반환
        view.ResetForReuse( );
        Poolable poolable = view.GetComponent<Poolable>( );

        if ( poolable != null )
            poolable.ReturnToPool( );
    }

    /// <summary>
    /// 현재 활성 상품 슬롯 전체 반환
    /// </summary>
    void ClearSlots ( )
    {
        //현재 활성 상품 슬롯을 공용 풀로 반환
        foreach ( var view in _views.Values )
        {
            RemoveSlot ( view );
        }

        //활성 상품 슬롯 조회 정보 제거
        _views.Clear ( );
    }

    /// <summary>
    /// 상품 슬롯 목록 생성
    /// </summary>
    /// <param name="viewDatas">상품 표시 데이터 목록</param>
    public void CreateSlotList ( IReadOnlyList<ShopSlotViewData> viewDatas )
    {
        //기존 활성 슬롯을 공용 풀로 반환
        ClearSlots( );

        //표시할 데이터가 없으면 종료
        if ( viewDatas == null || viewDatas.Count == 0 ) return;

        //표시 데이터별 공용 풀 슬롯 대여와 초기화
        for ( int i = 0 ; i < viewDatas.Count ; i++ )
            CreateSlot( viewDatas [ i ], i );

        //상품 목록 스크롤을 맨 위로 이동
        if ( _scrollRect != null )
            _scrollRect.verticalNormalizedPosition = 1f;

    }

    /// <summary>
    /// 상품 슬롯 조회
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="view">조회한 상품 슬롯</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetSlotView ( string id , out ShopSlotView view )
    {
        //기본 반환값 설정
        view = null;

        //빈 아이디 차단
        if ( string.IsNullOrEmpty ( id ) ) return false;

        //아이디에 맞는 상품 슬롯 조회
        return _views.TryGetValue ( id , out view );
    }

    /// <summary>
    /// 지정 상품 슬롯을 화면 안으로 이동하고 강조 위치 반환
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="target">화면 안으로 이동한 슬롯 위치</param>
    /// <returns>대상 슬롯 조회 성공 여부</returns>
    public bool FocusSlot ( string id, out RectTransform target )
    {
        target = null;

        if ( GetSlotView( id, out ShopSlotView view ) == false )
            return false;

        target = view.transform as RectTransform;

        if ( target == null ) return false;

        MoveSlotIntoViewport( target );
        return true;
    }

    /// <summary>
    /// 화면 밖 상품 슬롯이 보이도록 스크롤 위치 조정
    /// </summary>
    /// <param name="target">이동할 상품 슬롯</param>
    void MoveSlotIntoViewport ( RectTransform target )
    {
        if ( _scrollRect == null ||
            _scrollRect.content == null ||
            _scrollRect.viewport == null )
        {
            return;
        }

        Canvas.ForceUpdateCanvases( );
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            _scrollRect.content );
        Canvas.ForceUpdateCanvases( );

        RectTransform viewport = _scrollRect.viewport;
        Bounds contentBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport, _scrollRect.content );
        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport, target );

        float hiddenHeight =
            contentBounds.size.y - viewport.rect.height;

        if ( hiddenHeight <= 0f ) return;

        float centerOffset =
            viewport.rect.center.y - targetBounds.center.y;

        _scrollRect.StopMovement( );
        _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            _scrollRect.verticalNormalizedPosition -
            centerOffset / hiddenHeight );

        Canvas.ForceUpdateCanvases( );
    }

    /// <summary>
    /// 상점 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.AddCartButton:
                return _detailView.TryGetAddCartTarget( out target );

            case TutorialTargetId.CartButton:
                target = _cartButton.transform as RectTransform;
                return target != null;

            case TutorialTargetId.QuickRestockButton:
                return _detailView.TryGetTutorialTarget(
                    targetId, out target );

            default:
                target = null;
                return false;
        }
    }

    /// <summary>
    /// 상품 슬롯 갱신
    /// </summary>
    /// <param name="viewData">상품 표시 데이터</param>
    /// <returns>갱신 성공 여부</returns>
    public bool UpdateSlot ( ShopSlotViewData viewData )
    {
        //표시 데이터와 상품 아이디 확인
        if ( viewData == null || string.IsNullOrEmpty ( viewData.Id ) ) return false;

        //아이디에 맞는 상품 슬롯 조회
        if ( GetSlotView ( viewData.Id , out var view ) == false ) return false;

        //상품 슬롯 표시 갱신
        view.UpdateView ( viewData );

        return true;
    }
    #endregion

    #region ----- 카테고리 -----
    /// <summary>
    /// 전체 상품 카테고리 선택
    /// </summary>
    void SelectAllCategory ( )
    {
        SelectCategory ( ShopCategoryType.All );
    }

    /// <summary>
    /// 파츠 카테고리 선택
    /// </summary>
    void SelectPartCategory ( )
    {
        SelectCategory ( ShopCategoryType.Part );
    }

    /// <summary>
    /// 시설 카테고리 선택
    /// </summary>
    void SelectFacilityCategory ( )
    {
        SelectCategory ( ShopCategoryType.Facility );
    }

    /// <summary>
    /// 소모용품 카테고리 선택
    /// </summary>
    void SelectConsumableCategory ( )
    {
        SelectCategory ( ShopCategoryType.Consumable );
    }

    /// <summary>
    /// 메인 카테고리 선택과 전체 분류 적용
    /// </summary>
    /// <param name="category">선택한 메인 카테고리</param>
    public void SelectCategory ( ShopCategoryType category )
    {
        _selectedCategory = category;

        //선택 카테고리에 맞는 분류 옵션과 전체 분류 표시
        SetFilterOptions ( category );

        //버튼 선택 직후 해당 카테고리 전체 목록 요청
        PublishFilterSelection ( 0 );
    }

    /// <summary>
    /// 현재 카테고리에 맞는 분류 옵션 설정
    /// </summary>
    /// <param name="category">현재 메인 카테고리</param>
    void SetFilterOptions ( ShopCategoryType category )
    {
        switch ( category )
        {
            case ShopCategoryType.Part:
                _filterDropdown.SetOptions ( _partOptions );
                _filterDropdown.interactable = true;
                break;

            case ShopCategoryType.Facility:
                _filterDropdown.SetOptions ( _facilityOptions );
                _filterDropdown.interactable = true;
                break;

            case ShopCategoryType.Consumable:
                _filterDropdown.SetOptions ( _consumableOptions );
                _filterDropdown.interactable = false;
                break;

            default:
                _filterDropdown.SetOptions ( _allOptions );
                _filterDropdown.interactable = false;
                break;
        }
    }

    /// <summary>
    /// 공용 분류 선택
    /// </summary>
    /// <param name="optionIndex">선택한 분류 번호</param>
    void SelectFilter ( int optionIndex )
    {
        PublishFilterSelection ( optionIndex );
    }

    /// <summary>
    /// 현재 메인 카테고리에 맞는 목록 이벤트 발행
    /// </summary>
    /// <param name="optionIndex">선택한 분류 번호</param>
    void PublishFilterSelection ( int optionIndex )
    {
        switch ( _selectedCategory )
        {
            case ShopCategoryType.Part:
                OnPartSelected?.Invoke ( optionIndex );
                break;

            case ShopCategoryType.Facility:
                OnFacilitySelected?.Invoke ( optionIndex );
                break;

            case ShopCategoryType.Consumable:
                OnConsumableSelected?.Invoke ( );
                break;

            default:
                OnAllSelected?.Invoke ( );
                break;
        }
    }

    /// <summary>
    /// 상품 정렬 선택
    /// </summary>
    /// <param name="optionIndex">선택한 Dropdown 옵션 번호</param>
    void SelectSort ( int optionIndex )
    {
        //선택한 정렬 번호 전달
        OnSortSelected?.Invoke ( optionIndex );
    }
    #endregion

    #region ----- 상점 패널 -----

    /// <summary>
    /// 상점 닫기
    /// </summary>
    void ClosePanel ( )
    {
        OnClose?.Invoke ( );
    }

    /// <summary>
    /// 상점 패널 표시
    /// </summary>
    public void ShowPanel ( )
    {
        gameObject.SetActive ( true );
        _panelTween.Show ( );
    }

    /// <summary>
    /// 상점 선택 표시 초기화
    /// </summary>
    public void ResetSelection ( )
    {
        //전체 카테고리와 기본 정렬로 초기화
        _selectedCategory = ShopCategoryType.All;
        SetFilterOptions ( _selectedCategory );
        _sortDropdown.SetValueWithoutNotify ( 0 );

        //이전 상품 상세와 빠른 재입고 경고 숨김
        HideItemDetail ( );
        HideQuickRestockWarning ( );
    }

    /// <summary>
    /// 상점 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        _detailView.HideInstant ( );
        _warningView.HideInstant ( );

        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 상점 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _detailView.HideInstant ( );
        _warningView.HideInstant ( );

        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 상점 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    #endregion

    #region ----- 상품 상세 패널 -----
    /// <summary>
    /// 상품 상세 패널 표시
    /// </summary>
    /// <param name="viewData">상품 상세 표시 데이터</param>
    public void ShowItemDetail ( ShopDetailViewData viewData )
    {
        //상품 상세 표시
        _detailView.ShowPanel ( viewData );
    }

    /// <summary>
    /// 상품 상세 패널 숨김
    /// </summary>
    public void HideItemDetail ( )
    {
        //상품 상세 숨김
        _detailView.HidePanel ( );
    }

    /// <summary>
    /// 상품 상세 수량 표시 갱신
    /// </summary>
    /// <param name="quantity">표시할 수량</param>
    public void UpdateDetailQuantity ( int quantity )
    {
        //상세 수량 갱신
        _detailView.UpdateQuantity ( quantity );
    }

    /// <summary>
    /// 상세 수량 변경 요청 중계
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="amount">변경 수량</param>
    void ChangeDetailQuantity ( string id , int amount )
    {
        //상세 수량 변경 요청 전달
        OnDetailQuantityChanged?.Invoke ( id , amount );
    }

    /// <summary>
    /// 상세 수량 설정 요청 중계
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="value">입력값</param>
    void SetDetailQuantity ( string id , string value )
    {
        //상세 수량 설정 요청 전달
        OnDetailQuantitySet?.Invoke ( id , value );
    }

    /// <summary>
    /// 상세 패널 최소 수량 설정 중계
    /// </summary>
    void SetDetailMin ( string id )
    {
        OnDetailQuantityMin?.Invoke ( id );
    }

    /// <summary>
    /// 상세 패널 최대 수량 설정 중계
    /// </summary>
    void SetDetailMax ( string id )
    {
        OnDetailQuantityMax?.Invoke ( id );
    }

    /// <summary>
    /// 상세 상품 장바구니 추가 요청 중계
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="value">수량 입력값</param>
    void AddDetailToCart ( string id , string value )
    {
        //장바구니 추가 요청 전달
        OnDetailAddCart?.Invoke ( id , value );
    }

    /// <summary>
    /// 상세 상품 빠른 재입고 요청 중계
    /// </summary>
    /// <param name="id">상품 아이디</param>
    void QuickRestock ( string id )
    {
        OnDetailQuickRestock?.Invoke ( id );
    }

    /// <summary>
    /// 빠른 재입고 경고 확인 입력 중계
    /// </summary>
    void ConfirmQuickRestock ( )
    {
        OnQuickRestockConfirmed?.Invoke ( );
    }

    /// <summary>
    /// 빠른 재입고 경고 취소 입력 중계
    /// </summary>
    void CancelQuickRestock ( )
    {
        OnQuickRestockCanceled?.Invoke ( );
    }

    /// <summary>
    /// 빠른 재입고 경고 표시
    /// </summary>
    /// <param name="title">경고 제목</param>
    /// <param name="description">경고 설명</param>
    /// <param name="confirmText">확인 버튼 문구</param>
    /// <param name="cancelText">취소 버튼 문구</param>
    public void ShowQuickRestockWarning (
        string title , string description ,
        string confirmText , string cancelText )
    {
        _warningView.Show ( title , description , confirmText , cancelText );
    }

    /// <summary>
    /// 빠른 재입고 경고 숨김
    /// </summary>
    public void HideQuickRestockWarning ( )
    {
        _warningView.Hide ( );
    }

    /// <summary>
    /// 상세 패널 닫기 요청 중계
    /// </summary>
    void CloseDetailPanel ( )
    {
        //상세 패널 닫기 요청 전달
        OnDetailClose?.Invoke ( );
    }
    #endregion

    #region ----- 장바구니 -----
    /// <summary>
    /// 장바구니 열기 요청
    /// </summary>
    void OpenCart ( )
    {
        OnCartOpen?.Invoke ( );
    }
    #endregion
}
