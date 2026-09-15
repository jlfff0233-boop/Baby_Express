using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 손님 주문 뷰 - 주문 내용 갱신
/// </summary>
public class CustomerOrderView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //주문 패널 연출
    [SerializeField] Transform _content;       //주문 슬롯 생성 위치
    [SerializeField] OrderSlotView _slotPrefab;      //주문 슬롯 프리팹
    [SerializeField] OrderDetailView _detailView;       //주문 상세 뷰
    [SerializeField] WarningView _warningView;       //공용 경고 뷰
    [SerializeField] TMP_Text _orderCountText;       //현재 탭 주문 수
    [SerializeField] TooltipArea _countTooltip;       //주문 수 툴팁

    [Header ( "--- 버튼 ---" )]
    [SerializeField] Button _waitingButton;     //수락 대기 버튼
    [SerializeField] Button _producingButton;       //제작 중 버튼
    [SerializeField] Button _closedButton;      //종료 주문 버튼
    [SerializeField] Button _closeButton;       //주문 닫기 버튼

    [Header ( "--- 탭 색상 ---" )]
    [SerializeField] Color _normalTabColor = Color.white;       //기본 탭 색상
    [SerializeField] Color _selectedTabColor = Color.gray;       //선택 탭 색상
    [SerializeField] Color _highlightedTabColor = Color.white;       //마우스 강조 색상

    /// <summary>
    /// 주문 슬롯 딕셔너리(주문 아이디, 주문 뷰)
    /// </summary>
    Dictionary<string , OrderSlotView> _slots = new Dictionary<string , OrderSlotView> ( );
    PoolManager _poolManager;       //공용 주문 슬롯 풀 관리자

    #region ----- 이벤트 -----
    /// <summary>
    /// 대기 중 선택 이벤트
    /// </summary>
    public event Action OnWaitingSelected;
    /// <summary>
    /// 제작 중 선택 이벤트
    /// </summary>
    public event Action OnProducingSelected;
    /// <summary>
    /// 완료 주문 선택 이벤트
    /// </summary>
    public event Action OnClosedSelected;
    /// <summary>
    /// 주문 닫기 이벤트
    /// </summary>
    public event Action OnOrderClose;
    /// <summary>
    /// 주문 슬롯 선택 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnSlotSelected;
    /// <summary>
    /// 주문 확인 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnOrderConfirmed;
    /// <summary>
    /// 주문 취소 이벤트(주문 아이디)
    /// </summary>
    public event Action<string> OnOrderCanceled;
    /// <summary>
    /// 주문 상세 닫기 이벤트
    /// </summary>
    public event Action OnDetailClose;
    /// <summary>
    /// 경고 확인 이벤트
    /// </summary>
    public event Action OnWarningConfirmed;
    /// <summary>
    /// 경고 취소 이벤트
    /// </summary>
    public event Action OnWarningCanceled;
    #endregion

    /// <summary>
    /// 비활성 상태에서도 사용할 공용 풀 매니저 초기화
    /// </summary>
    public void InitializeRuntime ( )
    {
        _poolManager = GameManager.Instance.PoolManager;
    }

    /// <summary>
    /// 입력 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        //주문 탭 버튼 연결
        _waitingButton.onClick.AddListener ( SelectWaiting );
        _producingButton.onClick.AddListener ( SelectProducing );
        _closedButton.onClick.AddListener ( SelectClosed );

        //주문 닫기 입력 연결
        _closeButton.onClick.AddListener ( Close );

        //상세 패널 입력 연결
        _detailView.OnConfirmed += ConfirmOrder;
        _detailView.OnCanceled += CancelOrder;
        _detailView.OnClose += CloseDetail;

        //경고 패널 입력 연결
        _warningView.OnConfirmed += ConfirmWarning;
        _warningView.OnCanceled += CancelWarning;

        //상세 패널 초기 숨김
        _detailView.HideInstant ( );

        //경고 패널 초기 숨김
        _warningView.HideInstant ( );
    }

    /// <summary>
    /// 비활성화 전에 활성 주문 슬롯 반환
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
        //주문 탭 버튼 연결 해제
        _waitingButton.onClick.RemoveListener ( SelectWaiting );
        _producingButton.onClick.RemoveListener ( SelectProducing );
        _closedButton.onClick.RemoveListener ( SelectClosed );

        //주문 닫기 입력 해제
        _closeButton.onClick.RemoveListener ( Close );

        //상세 패널 입력 연결 해제
        _detailView.OnConfirmed -= ConfirmOrder;
        _detailView.OnCanceled -= CancelOrder;
        _detailView.OnClose -= CloseDetail;

        //경고 패널 입력 해제
        _warningView.OnConfirmed -= ConfirmWarning;
        _warningView.OnCanceled -= CancelWarning;

    }

    #region ----- 주문 패널 -----
    /// <summary>
    /// 닫기 이벤트 발행
    /// </summary>
    public void Close ( )
    {
        OnOrderClose?.Invoke ( );
    }

    /// <summary>
    /// 주문 패널 표시
    /// </summary>
    public void Show ( )
    {
        gameObject.SetActive ( true );
        _panelTween.Show ( );
    }

    /// <summary>
    /// 주문 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void Hide ( Action onComplete = null )
    {
        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 주문 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 주문 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    /// <summary>
    /// 현재 탭 주문 수 갱신
    /// </summary>
    /// <param name="count">현재 주문 수</param>
    /// <param name="total">비교할 전체 주문 수</param>
    /// <param name="description">주문 수 설명</param>
    public void UpdateOrderCount ( int count , int total , string description )
    {
        //현재 탭 주문 수 표시
        _orderCountText.text = $"{count} / {total}";

        //현재 탭에 맞는 툴팁 설명 설정
        _countTooltip.SetDescription ( description );

        //이전 탭 툴팁 숨김
        _countTooltip.Hide ( );
    }
    #endregion

    #region ----- 카테고리 -----
    /// <summary>
    /// 대기 중 버튼 선택 이벤트 발행
    /// </summary>
    public void SelectWaiting ( )
    {
        OnWaitingSelected?.Invoke ( );
    }

    /// <summary>
    /// 제작 중 버튼 선택 이벤트 발행
    /// </summary>
    public void SelectProducing ( )
    {
        OnProducingSelected?.Invoke ( );
    }

    /// <summary>
    /// 완료 주문 버튼 선택 이벤트 발행
    /// </summary>
    public void SelectClosed ( )
    {
        OnClosedSelected?.Invoke ( );
    }

    /// <summary>
    /// 선택한 주문 탭 표시
    /// </summary>
    /// <param name="selectedTab">현재 선택 탭</param>
    public void UpdateSelectedTab ( OrderTab selectedTab )
    {
        //선택 상태에 맞게 탭 색상 갱신
        UpdateTabColor ( _waitingButton , selectedTab == OrderTab.Waiting );
        UpdateTabColor ( _producingButton , selectedTab == OrderTab.Producing );
        UpdateTabColor ( _closedButton , selectedTab == OrderTab.Closed );
    }

    /// <summary>
    /// 주문 탭 색상 갱신
    /// </summary>
    /// <param name="button">갱신할 탭 버튼</param>
    /// <param name="isSelected">선택 여부</param>
    void UpdateTabColor ( Button button , bool isSelected )
    {
        ColorBlock colors = button.colors;
        colors.normalColor = isSelected ? _selectedTabColor : _normalTabColor;
        colors.highlightedColor = isSelected ? _selectedTabColor : _highlightedTabColor;
        colors.selectedColor = isSelected ? _selectedTabColor : _normalTabColor;
        button.colors = colors;
    }

    #endregion

    #region ----- 주문 슬롯 -----
    /// <summary>
    /// 주문 슬롯 선택 이벤트 발행
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    public void SelectSlot ( string orderId )
    {
        OnSlotSelected?.Invoke ( orderId );
    }

    /// <summary>
    /// 주문 슬롯을 공용 풀에서 가져와 표시
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    /// <param name="siblingIndex">현재 정렬 순서</param>
    void CreateSlot (
        OrderSlotViewData viewData , int siblingIndex )
    {
        //표시 데이터와 주문 아이디 확인
        if ( viewData == null || string.IsNullOrEmpty ( viewData.OrderId ) ) return;

        //현재 활성 목록의 주문 아이디 중복 생성 차단
        if ( _slots.ContainsKey ( viewData.OrderId ) ) return;

        //주문 슬롯 프리팹 전용 풀에서 슬롯 대여
        GameObject slotObject = _poolManager.GetFromPool (
            _slotPrefab.gameObject , _content );
        OrderSlotView view =
            slotObject.GetComponent<OrderSlotView> ( );

        //현재 주문 정렬 순서 적용
        view.transform.SetSiblingIndex ( siblingIndex );

        //주문 슬롯 초기화
        view.Init ( viewData );

        //주문 선택 이벤트 연결
        view.OnClickDetail += SelectSlot;

        //주문 딕셔너리에 추가
        _slots.Add ( viewData.OrderId , view );
    }

    /// <summary>
    /// 주문 슬롯을 공용 풀로 반환
    /// </summary>
    /// <param name="view">제거할 주문 슬롯</param>
    void RemoveSlot ( OrderSlotView view )
    {
        //주문 선택 이벤트 해제
        view.OnClickDetail -= SelectSlot;

        //주문 식별자와 실행 중인 연출 초기화
        view.ResetForReuse ( );

        //현재 슬롯을 생성한 풀로 반환
        view.GetComponent<Poolable> ( ).ReturnToPool ( );
    }

    /// <summary>
    /// 현재 활성 주문 슬롯 전체 반환
    /// </summary>
    void ClearSlots ( )
    {
        //현재 활성 슬롯을 공용 풀로 반환
        foreach ( OrderSlotView view in _slots.Values )
            RemoveSlot ( view );

        _slots.Clear ( );
    }

    /// <summary>
    /// 주문 슬롯 목록 생성
    /// </summary>
    /// <param name="viewDatas">주문 슬롯 표시 데이터 목록</param>
    public void CreateSlotList ( IReadOnlyList<OrderSlotViewData> viewDatas )
    {
        //기존 활성 주문 슬롯을 공용 풀로 반환
        ClearSlots ( );

        //표시할 주문이 없으면 종료
        if ( viewDatas == null || viewDatas.Count == 0 ) return;

        //현재 정렬 순서대로 주문 슬롯 대여
        for ( int i = 0 ; i < viewDatas.Count ; i++ )
            CreateSlot ( viewDatas [ i ] , i );
    }

    /// <summary>
    /// 주문 아이디에 대응하는 현재 활성 슬롯 조회
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <param name="view">조회한 주문 슬롯</param>
    /// <returns>슬롯 조회 성공 여부</returns>
    public bool GetSlotView (
        string orderId, out OrderSlotView view )
    {
        view = null;

        if ( string.IsNullOrEmpty( orderId ) ) return false;

        return _slots.TryGetValue( orderId, out view );
    }

    /// <summary>
    /// 주문 상세의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 상세 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        return _detailView.TryGetTutorialTarget(
            targetId, out target );
    }

    #endregion

    #region ----- 주문 상세 패널 -----
    /// <summary>
    /// 주문 확인 이벤트 중계
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void ConfirmOrder ( string orderId )
    {
        OnOrderConfirmed?.Invoke ( orderId );
    }

    /// <summary>
    /// 주문 취소 이벤트 중계
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    void CancelOrder ( string orderId )
    {
        OnOrderCanceled?.Invoke ( orderId );
    }

    /// <summary>
    /// 주문 상세 패널 표시
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    public void ShowDetail ( OrderDetailViewData viewData )
    {
        //선택한 주문 슬롯 표시
        SetSelectedOrder ( viewData.OrderId );

        //주문 상세 정보 초기화
        _detailView.Init ( viewData );

        //상세 패널 표시
        _detailView.Show ( );
    }

    /// <summary>
    /// 주문 상세 패널 숨기기
    /// </summary>
    public void HideDetail ( )
    {
        //주문 슬롯 선택 표시 초기화
        SetSelectedOrder ( null );

        //상세 패널 숨김
        _detailView.Hide ( );
    }

    /// <summary>
    /// 주문 슬롯 선택 표시 설정
    /// </summary>
    /// <param name="orderId">선택한 주문 아이디</param>
    void SetSelectedOrder ( string orderId )
    {
        //선택 주문만 외곽선 표시
        foreach ( var slot in _slots )
        {
            slot.Value.SetSelected ( slot.Key == orderId );
        }
    }

    /// <summary>
    /// 상세 패널 닫기 이벤트 중계
    /// </summary>
    void CloseDetail ( )
    {
        OnDetailClose?.Invoke ( );
    }

    #endregion

    #region ----- 경고 패널 -----
    /// <summary>
    /// 경고 패널 표시
    /// </summary>
    /// <param name="title">경고 제목</param>
    /// <param name="description">경고 설명</param>
    /// <param name="confirmText">확인 버튼 문구</param>
    /// <param name="cancelText">취소 버튼 문구</param>
    public void ShowWarning ( string title , string description , string confirmText , string cancelText )
    {
        _warningView.Show ( title , description , confirmText , cancelText );
    }

    /// <summary>
    /// 경고 패널 숨김
    /// </summary>
    public void HideWarning ( )
    {
        _warningView.Hide ( );
    }

    /// <summary>
    /// 경고 확인 이벤트 중계
    /// </summary>
    void ConfirmWarning ( )
    {
        OnWarningConfirmed?.Invoke ( );
    }

    /// <summary>
    /// 경고 취소 이벤트 중계
    /// </summary>
    void CancelWarning ( )
    {
        OnWarningCanceled?.Invoke ( );
    }
    #endregion

}
