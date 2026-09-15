using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상품 상세 뷰 - 상품 상세 표시와 수량 입력 전달
/// </summary>
public class ShopDetailView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] Image _icon;       //상품 아이콘
    [SerializeField] TMP_Text _nameText;        //상품 이름
    [SerializeField] TMP_Text _priceText;       //현재 가격
    [SerializeField] TMP_Text _stockText;       //남은 재고
    [SerializeField] TMP_Text _descText;     //상품 설명

    [SerializeField] TMP_InputField _quantityInput;     //구매 수량
    [SerializeField] Button _plusButton;        //수량 증가 버튼
    [SerializeField] Button _minusButton;       //수량 감소 버튼
    [SerializeField] Button _minButton;         //최소 수량 버튼
    [SerializeField] Button _maxButton;         //최대 수량 버튼
    [SerializeField] Button _quickRestockButton;       //빠른 재입고 버튼

    [SerializeField] Button _addCartButton;     //장바구니 추가 버튼
    [SerializeField] Button _closeButton;       //닫기 버튼
    [SerializeField] PanelTweenView _panelTween;       //상품 상세 패널 연출

    string _id;     //현재 상품 아이디

    /// <summary>
    /// 장바구니 추가 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetAddCartTarget ( out RectTransform target )
    {
        target = _addCartButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 상품 상세 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.QuickRestockButton:
                target = _quickRestockButton.transform as RectTransform;
                return target != null;

            default:
                target = null;
                return false;
        }
    }

    #region ----- 이벤트 -----
    /// <summary>
    /// 수량 변경 이벤트(아이디, 변경 수량)
    /// </summary>
    public event Action<string , int> OnQuantityChanged;

    /// <summary>
    /// 수량 설정 이벤트(아이디, 입력값)
    /// </summary>
    public event Action<string , string> OnQuantitySet;

    /// <summary>
    /// 최소 수량 설정 이벤트
    /// </summary>
    public event Action<string> OnQuantityMin;

    /// <summary>
    /// 최대 수량 설정 이벤트
    /// </summary>
    public event Action<string> OnQuantityMax;

    /// <summary>
    /// 장바구니 추가 이벤트(아이디, 입력값)
    /// </summary>
    public event Action<string , string> OnAddCart;

    /// <summary>
    /// 빠른 재입고 요청 이벤트
    /// </summary>
    public event Action<string> OnQuickRestock;

    /// <summary>
    /// 상세 패널 닫기 이벤트
    /// </summary>
    public event Action OnClose;
    #endregion

    /// <summary>
    /// 입력 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        //상품 처리 주요 버튼에 공용 클릭 연출 연결
        _addCartButton.BindClickHighlight( );
        _quickRestockButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        //수량 입력 연결
        _minusButton.onClick.AddListener ( DecreaseQuantity );
        _plusButton.onClick.AddListener ( IncreaseQuantity );
        _quantityInput.onEndEdit.AddListener ( SetQuantity );

        //장바구니와 닫기 입력 연결
        _addCartButton.onClick.AddListener ( AddToCart );
        _closeButton.onClick.AddListener ( Close );

        //최소/최대 수량 입력 연결
        _minButton.onClick.AddListener ( SetMinQuantity );
        _maxButton.onClick.AddListener ( SetMaxQuantity );

        //빠른 재입고 입력 연결
        _quickRestockButton.onClick.AddListener ( QuickRestock );
    }

    /// <summary>
    /// 입력 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        //수량 입력 연결 해제
        _minusButton.onClick.RemoveListener ( DecreaseQuantity );
        _plusButton.onClick.RemoveListener ( IncreaseQuantity );
        _quantityInput.onEndEdit.RemoveListener ( SetQuantity );

        //장바구니와 닫기 입력 연결 해제
        _addCartButton.onClick.RemoveListener ( AddToCart );
        _closeButton.onClick.RemoveListener ( Close );

        //최소/최대 수량 입력 연결 해제
        _minButton.onClick.RemoveListener ( SetMinQuantity );
        _maxButton.onClick.RemoveListener ( SetMaxQuantity );

        //빠른 재입고 입력 연결 해제
        _quickRestockButton.onClick.RemoveListener ( QuickRestock );
    }

    /// <summary>
    /// 수량 1 증가 이벤트 발행
    /// </summary>
    void IncreaseQuantity ( )
    {
        //상품 아이디와 증가 수량 전달
        OnQuantityChanged?.Invoke ( _id , 1 );
    }

    /// <summary>
    /// 수량 1 감소 이벤트 발행
    /// </summary>
    void DecreaseQuantity ( )
    {
        //상품 아이디와 감소 수량 전달
        OnQuantityChanged?.Invoke ( _id , -1 );
    }

    /// <summary>
    /// 수량 설정 이벤트 발행
    /// </summary>
    /// <param name="value">입력한 수량 문자열</param>
    void SetQuantity ( string value )
    {
        //상품 아이디와 입력값 전달
        OnQuantitySet?.Invoke ( _id , value );
    }

    /// <summary>
    /// 최소 수량 설정 요청
    /// </summary>
    void SetMinQuantity ( )
    {
        //현재 상품 아이디 전달
        OnQuantityMin?.Invoke ( _id );
    }

    /// <summary>
    /// 최대 수량 설정 요청
    /// </summary>
    void SetMaxQuantity ( )
    {
        //현재 상품 아이디 전달
        OnQuantityMax?.Invoke ( _id );
    }

    /// <summary>
    /// 장바구니 추가 이벤트 발행
    /// </summary>
    void AddToCart ( )
    {
        //상품 아이디와 현재 입력값 전달
        OnAddCart?.Invoke ( _id , _quantityInput.text );
    }

    /// <summary>
    /// 현재 상품 빠른 재입고
    /// </summary>
    void QuickRestock ( )
    {
        OnQuickRestock?.Invoke ( _id );
    }

    /// <summary>
    /// 상세 패널 닫기 이벤트 발행
    /// </summary>
    void Close ( )
    {
        //닫기 이벤트 발행
        OnClose?.Invoke ( );
    }

    /// <summary>
    /// 상품 상세 패널 표시
    /// </summary>
    /// <param name="viewData">상품 상세 표시 데이터</param>
    public void ShowPanel ( ShopDetailViewData viewData )
    {
        //표시 데이터가 없으면 종료
        if ( viewData == null ) return;

        gameObject.SetActive ( true );

        //현재 상품 아이디 저장
        _id = viewData.Id;

        //상품 상세 정보 표시
        _icon.SetIconSprite( viewData.Icon );
        _nameText.text = viewData.Name;
        _descText.text = viewData.Desc;

        float price = Mathf.FloorToInt ( viewData.Price );
        _priceText.text = $"가격: {price:N0}";
        _stockText.text = $"재고: {viewData.RemainingStock}개";

        //잠긴 상품은 상세 정보만 표시하고 구매 입력 비활성화
        bool canPurchase = viewData.IsLocked == false;

        //구매 여부에 따라 활성화 상태 변경
        _quantityInput.interactable = canPurchase;
        _plusButton.interactable = canPurchase;
        _minusButton.interactable = canPurchase;
        _minButton.interactable = canPurchase;
        _maxButton.interactable = canPurchase;
        _addCartButton.interactable = canPurchase;

        //해금된 일반 상품만 빠른 재입고 입력 활성화
        _quickRestockButton.interactable = canPurchase && viewData.CanQuickRestock;

        //초기 수량 표시
        UpdateQuantity ( 1 );

        //상세 패널 표시
        _panelTween.Show ( );
    }

    /// <summary>
    /// 상품 상세 패널 숨김
    /// </summary>
    public void HidePanel ( )
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 상품 상세 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 상품 상세 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 수량 표시 갱신
    /// </summary>
    /// <param name="quantity">표시할 수량</param>
    public void UpdateQuantity ( int quantity )
    {
        //입력 이벤트 없이 수량 표시
        _quantityInput.SetTextWithoutNotify ( quantity.ToString ( ) );
    }
}
