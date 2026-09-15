using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장바구니 패널 표시 및 입력 관리
/// </summary>
public class CartView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;           //장바구니 패널 연출

    [Header( "--- 상품 목록 ---" )]
    [SerializeField] Transform _content;                //슬롯 생성 위치
    [SerializeField] CartItemView _itemPrefab;          //장바구니 슬롯 프리팹
    [SerializeField] TMP_Text _itemCountText;           //장바구니 상품 개수

    [Header( "--- 금액 표시 ---" )]
    [SerializeField] TMP_Text _totalPriceText;          //장바구니 총액
    [SerializeField] TMP_Text _expectedBudgetText;       //구매 후 예상 잔액

    [Header( "--- 입력 버튼 ---" )]
    [SerializeField] Button _clearButton;               //장바구니 비우기 버튼
    [SerializeField] Button _purchaseButton;            //구매 버튼
    [SerializeField] Button _closeButton;               //패널 닫기 버튼

    Dictionary<string, CartItemView> _itemViews = new( );

    public event Action<string, int> OnQuantityChange;
    public event Action<string, string> OnQuantitySet;
    public event Action<string> OnRemove;
    public event Action OnClear;
    public event Action OnPurchase;
    public event Action OnClose;

    /// <summary>
    /// 구매 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetPurchaseTarget ( out RectTransform target )
    {
        target = _purchaseButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 패널 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        //장바구니 주요 버튼에 공용 클릭 연출 연결
        _clearButton.BindClickHighlight( );
        _purchaseButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        //패널 버튼 연결
        _clearButton.onClick.AddListener( ClearCart );
        _purchaseButton.onClick.AddListener( Purchase );
        _closeButton.onClick.AddListener( Close );
    }

    /// <summary>
    /// 패널 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        //패널 버튼 연결 해제
        _clearButton.onClick.RemoveListener( ClearCart );
        _purchaseButton.onClick.RemoveListener( Purchase );
        _closeButton.onClick.RemoveListener( Close );

        //생성된 슬롯 이벤트 해제
        ClearItems( );
    }

    /// <summary>
    /// 장바구니 전체 표시 갱신
    /// </summary>
    /// <param name="items">장바구니 슬롯 표시 데이터</param>
    /// <param name="totalPrice">장바구니 총액</param>
    /// <param name="expectedBudget">구매 후 예상 잔액</param>
    public void RefreshItems ( IReadOnlyList<CartItemViewData> items, float totalPrice, float expectedBudget )
    {
        //기존 슬롯 제거
        ClearItems( );

        //현재 장바구니 슬롯 생성
        foreach ( var viewData in items )
        {
            CreateItem( viewData );
        }

        //소수점 제거
        double total = Math.Truncate( totalPrice );
        double budget = Math.Truncate( expectedBudget );

        //총액과 예상 잔액 표시
        _itemCountText.text = $"{items.Count}";
        _totalPriceText.text = $"총합: {total:N0}";
        _expectedBudgetText.text = $"예상 잔액: {budget:N0}";
    }

    /// <summary>
    /// 지정 상품 수량 표시 복구
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">기존 수량</param>
    public void UpdateItemQuantity ( string id, int quantity )
    {
        //생성된 슬롯 조회
        if ( _itemViews.TryGetValue( id, out var itemView ) == false )
        {
            return;
        }

        //기존 수량으로 복구
        itemView.UpdateQuantity( quantity );
    }

    /// <summary>
    /// 장바구니 패널 표시
    /// </summary>
    public void ShowPanel ()
    {
        gameObject.SetActive ( true );
        _panelTween.Show( );
    }

    /// <summary>
    /// 장바구니 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        _panelTween.Hide ( () => CompleteHide( onComplete ) );
    }

    /// <summary>
    /// 장바구니 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 장바구니 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke( );
    }

    /// <summary>
    /// 장바구니 슬롯 생성
    /// </summary>
    /// <param name="viewData">슬롯 표시 데이터</param>
    void CreateItem ( CartItemViewData viewData )
    {
        //슬롯 프리팹 생성
        var itemView = Instantiate( _itemPrefab, _content );

        //상품 정보 표시
        itemView.Init( viewData );

        //슬롯 입력 이벤트 연결
        itemView.OnQuantityChange += ChangeQuantity;
        itemView.OnQuantitySet += SetQuantity;
        itemView.OnRemove += RemoveItem;

        //상품 아이디 기준으로 슬롯 저장
        _itemViews.Add( viewData.Id, itemView );
    }

    /// <summary>
    /// 생성된 장바구니 슬롯 제거
    /// </summary>
    void ClearItems ()
    {
        //슬롯 이벤트 해제 및 오브젝트 제거
        foreach ( var itemView in _itemViews.Values )
        {
            itemView.OnQuantityChange -= ChangeQuantity;
            itemView.OnQuantitySet -= SetQuantity;
            itemView.OnRemove -= RemoveItem;

            Destroy( itemView.gameObject );
        }

        _itemViews.Clear( );
    }

    /// <summary>
    /// 상품 수량 변경 입력 중계
    /// </summary>
    void ChangeQuantity ( string id, int amount )
    {
        OnQuantityChange?.Invoke( id, amount );
    }

    /// <summary>
    /// 상품 수량 설정 입력 중계
    /// </summary>
    void SetQuantity ( string id, string value )
    {
        OnQuantitySet?.Invoke( id, value );
    }

    /// <summary>
    /// 상품 삭제 입력 중계
    /// </summary>
    void RemoveItem ( string id )
    {
        OnRemove?.Invoke( id );
    }

    /// <summary>
    /// 장바구니 비우기 입력 중계
    /// </summary>
    void ClearCart ()
    {
        OnClear?.Invoke( );
    }

    /// <summary>
    /// 구매 입력 중계
    /// </summary>
    void Purchase ()
    {
        OnPurchase?.Invoke( );
    }

    /// <summary>
    /// 패널 닫기 입력 중계
    /// </summary>
    void Close ()
    {
        OnClose?.Invoke( );
    }
}
