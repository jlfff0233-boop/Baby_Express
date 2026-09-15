using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장바구니 상품 슬롯 표시 및 입력 관리
/// </summary>
public class CartItemView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [Header("--- 정보 ---")]
    [SerializeField] TMP_Text _nameText;               //상품 이름
    [SerializeField] TMP_Text _priceText;              //현재 가격
    [SerializeField] TMP_Text _subtotalText;           //상품 소계
    [SerializeField] TMP_InputField _quantityInput;    //현재 수량

    [Header("--- 수량 ---")]
    [SerializeField] Button _plusButton;               //수량 증가 버튼
    [SerializeField] Button _minusButton;              //수량 감소 버튼
    [SerializeField] Button _removeButton;             //상품 삭제 버튼

    string _id;                                        //현재 상품 아이디

    public event Action<string, int> OnQuantityChange;
    public event Action<string, string> OnQuantitySet;
    public event Action<string> OnRemove;

    /// <summary>
    /// 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        //수량 입력 연결
        _minusButton.onClick.AddListener( DecreaseQuantity );
        _plusButton.onClick.AddListener( IncreaseQuantity );
        _quantityInput.onEndEdit.AddListener( SetQuantity );

        //상품 삭제 연결
        _removeButton.onClick.AddListener( RemoveItem );
    }

    /// <summary>
    /// 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        //수량 입력 해제
        _minusButton.onClick.RemoveListener( DecreaseQuantity );
        _plusButton.onClick.RemoveListener( IncreaseQuantity );
        _quantityInput.onEndEdit.RemoveListener( SetQuantity );

        //상품 삭제 해제
        _removeButton.onClick.RemoveListener( RemoveItem );
    }

    /// <summary>
    /// 장바구니 슬롯 초기화
    /// </summary>
    /// <param name="viewData">슬롯 표시 데이터</param>
    public void Init ( CartItemViewData viewData )
    {
        //현재 상품 정보 저장
        _id = viewData.Id;

        //상품 정보 표시
        _nameText.text = viewData.Name;
        _priceText.text = Mathf.FloorToInt( viewData.Price ).ToString( "N0" );
        _subtotalText.text = Mathf.FloorToInt( viewData.Subtotal ).ToString( "N0" );

        //현재 수량 표시
        UpdateQuantity( viewData.Quantity );
    }

    /// <summary>
    /// 수량 표시 갱신
    /// </summary>
    /// <param name="quantity">표시할 장바구니 수량</param>
    public void UpdateQuantity ( int quantity )
    {
        //이벤트를 발생시키지 않고 표시값만 변경
        _quantityInput.SetTextWithoutNotify( quantity.ToString( ) );
    }

    /// <summary>
    /// 수량 1 증가 요청
    /// </summary>
    void IncreaseQuantity ()
    {
        //상품 아이디와 변경량 전달
        OnQuantityChange?.Invoke( _id, 1 );
    }

    /// <summary>
    /// 수량 1 감소 요청
    /// </summary>
    void DecreaseQuantity ()
    {
        //상품 아이디와 변경량 전달
        OnQuantityChange?.Invoke( _id, -1 );
    }

    /// <summary>
    /// 입력한 최종 수량 설정 요청
    /// </summary>
    /// <param name="value">입력 문자열</param>
    void SetQuantity ( string value )
    {
        //변환과 검증은 Presenter와 Model에 요청
        OnQuantitySet?.Invoke( _id, value );
    }

    /// <summary>
    /// 장바구니 상품 삭제 요청
    /// </summary>
    void RemoveItem ()
    {
        //현재 상품 아이디 전달
        OnRemove?.Invoke( _id );
    }
}