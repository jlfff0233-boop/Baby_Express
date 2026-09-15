using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작 주문 목록 뷰 - 제작 가능 주문 표시와 선택, 제작 입력 전달
/// </summary>
public class CraftListView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] RectTransform _content;          //주문 슬롯 부모
    [SerializeField] ScrollRect _scrollRect;          //주문 목록 스크롤
    [SerializeField] TMP_Dropdown _sortDropdown;      //제작 주문 정렬 선택
    [SerializeField] Button _craftButton;             //제작 시작 버튼
    [SerializeField] Button _exitButton;              //제작 목록 나가기 버튼

    [Header ( "----- 프리팹 -----" )]
    [SerializeField] OrderSlotView _slotPrefab;       //제작 주문 슬롯

    /// <summary>
    /// 정렬 옵션
    /// </summary>
    static readonly string [ ] _sortOptions =
    {
        "마감일 빠른 순",
        "마감일 늦은 순",
        "특수 주문 상단",
        "일반 주문 상단",
    };

    /// <summary>
    /// 슬롯 뷰 딕셔너리(아이디, 주문 슬롯 뷰)
    /// </summary>
    Dictionary<string , OrderSlotView> _slotViews = new Dictionary<string , OrderSlotView> ( );
    PoolManager _poolManager;       //공용 주문 슬롯 풀 관리자

    /// <summary>
    /// 제작 주문 선택 이벤트
    /// </summary>
    public event Action<string> OnOrderSelected;

    /// <summary>
    /// 제작 주문 정렬 선택 이벤트
    /// </summary>
    public event Action<int> OnSortSelected;

    /// <summary>
    /// 제작 시작 이벤트
    /// </summary>
    public event Action OnCraftStarted;

    /// <summary>
    /// 제작 목록 나가기 이벤트
    /// </summary>
    public event Action OnExit;

    /// <summary>
    /// 지정 주문 슬롯의 튜토리얼 강조 대상 조회
    /// </summary>
    /// <param name="orderId">조회할 주문 아이디</param>
    /// <param name="target">주문 슬롯 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetSlotTarget (
        string orderId, out RectTransform target )
    {
        if ( _slotViews.TryGetValue(
            orderId, out OrderSlotView slotView ) == false )
        {
            target = null;
            return false;
        }

        target = slotView.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 제작 시작 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetCraftStartTarget ( out RectTransform target )
    {
        target = _craftButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 공용 풀 매니저 초기화
    /// </summary>
    public void InitializeRuntime ( )
    {
        _poolManager = GameManager.Instance.PoolManager;
    }

    /// <summary>
    /// 제작 목록 입력 연결
    /// </summary>
    void Awake ( )
    {
        //제작 시작과 취소 버튼에 공용 클릭 연출 연결
        _craftButton.BindClickHighlight( );
        _exitButton.BindClickHighlight( );

        _sortDropdown.SetOptions( _sortOptions );

        _sortDropdown.onValueChanged.AddListener( SelectSort );
        _craftButton.onClick.AddListener ( StartCraft );
        _exitButton.onClick.AddListener ( Exit );
    }

    /// <summary>
    /// 비활성화 전에 활성 제작 주문 슬롯 반환
    /// </summary>
    void OnDisable ( )
    {
        ClearOrders ( );
    }

    /// <summary>
    /// 제작 목록 입력 해제
    /// </summary>
    void OnDestroy ( )
    {
        _sortDropdown.onValueChanged.RemoveListener( SelectSort );
        _craftButton.onClick.RemoveListener ( StartCraft );
        _exitButton.onClick.RemoveListener ( Exit );
    }

    #region ----- 주문 슬롯 -----

    /// <summary>
    /// 제작 주문 목록 표시
    /// </summary>
    /// <param name="viewDatas">주문 슬롯 표시 데이터 목록</param>
    public void ShowOrders ( IReadOnlyList<OrderSlotViewData> viewDatas )
    {
        //기존 주문 슬롯 제거
        ClearOrders ( );
        //제작 시작 입력 초기화
        SetCraftInteractable ( false );

        //표시할 주문이 없으면 종료
        if ( viewDatas == null || viewDatas.Count == 0 ) return;

        //제작 주문 슬롯을 현재 정렬 순서대로 대여
        for ( int i = 0 ; i < viewDatas.Count ; i++ )
            CreateOrder ( viewDatas [ i ] , i );

        //목록 스크롤을 맨 위로 이동
        _scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 제작 주문 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    /// <param name="siblingIndex">현재 정렬 순서</param>
    void CreateOrder (
        OrderSlotViewData viewData , int siblingIndex )
    {
        if ( viewData == null || string.IsNullOrWhiteSpace ( viewData.OrderId ) )
            return;

        //같은 주문 슬롯 중복 생성 차단
        if ( _slotViews.ContainsKey ( viewData.OrderId ) )
            return;

        //주문 슬롯 프리팹 전용 풀에서 슬롯 대여
        GameObject slotObject = _poolManager.GetFromPool (
            _slotPrefab.gameObject , _content );
        OrderSlotView slotView =
            slotObject.GetComponent<OrderSlotView> ( );

        //현재 제작 주문 정렬 순서 적용
        slotView.transform.SetSiblingIndex ( siblingIndex );

        //주문 정보와 선택 입력 연결
        slotView.Init ( viewData );
        slotView.OnClickDetail += SelectOrder;

        _slotViews.Add ( viewData.OrderId , slotView );
    }

    /// <summary>
    /// 제작 주문 슬롯 전체 반환
    /// </summary>
    public void ClearOrders ( )
    {
        foreach ( OrderSlotView slotView in _slotViews.Values )
        {
            //선택 입력과 재사용 상태 초기화
            slotView.OnClickDetail -= SelectOrder;
            slotView.ResetForReuse ( );

            //현재 슬롯을 생성한 풀로 반환
            slotView.GetComponent<Poolable> ( ).ReturnToPool ( );
        }

        _slotViews.Clear ( );
    }

    /// <summary>
    /// 제작 주문 선택 표시 설정
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    public void SetSelectedOrder ( string orderId )
    {
        //선택 주문만 외곽선 표시
        foreach ( var slotView in _slotViews )
        {
            slotView.Value.SetSelected ( slotView.Key == orderId );
        }
    }

    #endregion

    #region ----- 제작 입력 -----

    /// <summary>
    /// 제작 주문 정렬 입력 전달
    /// </summary>
    /// <param name="sortIndex">선택한 정렬 번호</param>
    void SelectSort ( int sortIndex )
    {
        OnSortSelected?.Invoke( sortIndex );
    }

    /// <summary>
    /// 제작 시작 버튼 상태 설정
    /// </summary>
    /// <param name="isInteractable">입력 가능 여부</param>
    public void SetCraftInteractable ( bool isInteractable )
    {
        _craftButton.interactable = isInteractable;
    }

    /// <summary>
    /// 제작 주문 선택 입력 전달
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    void SelectOrder ( string orderId )
    {
        OnOrderSelected?.Invoke ( orderId );
    }

    /// <summary>
    /// 제작 시작 입력 전달
    /// </summary>
    void StartCraft ( )
    {
        OnCraftStarted?.Invoke ( );
    }

    /// <summary>
    /// 제작 목록 나가기 입력 전달
    /// </summary>
    void Exit ( )
    {
        OnExit?.Invoke ( );
    }

    #endregion
}
