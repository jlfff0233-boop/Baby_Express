using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 아이템 판매 뷰 - 판매 정보 표시와 입력 전달
/// </summary>
public class InventoryItemSellView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] Image _icon;                    //아이템 아이콘
    [SerializeField] TMP_Text _nameText;             //아이템 이름
    [SerializeField] TMP_Text _priceText;            //개당 판매 가격
    [SerializeField] TMP_Text _quantityText;         //보유 수량
    [SerializeField] TMP_Text _notificationText;     //예상 획득 금액 안내
    [SerializeField] TMP_InputField _inputField;     //판매 수량 입력

    [SerializeField] Button _minButton;              //최소 수량 버튼
    [SerializeField] Button _maxButton;              //최대 수량 버튼

    [SerializeField] Button _minusButton;            //수량 감소 버튼
    [SerializeField] Button _plusButton;             //수량 증가 버튼

    [SerializeField] Button _confirmButton;          //판매 버튼
    [SerializeField] Button _closeButton;            //판매 패널 닫기 버튼

    [SerializeField] PanelTweenView _panelTween;       //판매 패널 연출

    #region ----- 이벤트 -----
    /// <summary>
    /// 판매 수량 변경 요청 이벤트
    /// </summary>
    public event Action<int> OnQuantityChanged;

    /// <summary>
    /// 판매 수량 직접 설정 요청 이벤트
    /// </summary>
    public event Action<string> OnQuantitySet;

    /// <summary>
    /// 최소 판매 수량 요청 이벤트
    /// </summary>
    public event Action OnSellMin;

    /// <summary>
    /// 최대 판매 수량 요청 이벤트
    /// </summary>
    public event Action OnSellMax;

    /// <summary>
    /// 판매 확정 요청 이벤트
    /// </summary>
    public event Action OnSellConfirm;

    /// <summary>
    /// 판매 패널 닫기 요청 이벤트
    /// </summary>
    public event Action OnClose;
    #endregion

    /// <summary>
    /// 판매 패널 입력 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        //판매와 취소 버튼에 공용 클릭 연출 연결
        _confirmButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        _minButton.onClick.AddListener ( SelectMin );
        _minusButton.onClick.AddListener ( DecreaseQuantity );
        _plusButton.onClick.AddListener ( IncreaseQuantity );
        _maxButton.onClick.AddListener ( SelectMax );
        _confirmButton.onClick.AddListener ( Confirm );
        _closeButton.onClick.AddListener ( Close );
        _inputField.onEndEdit.AddListener ( SetQuantity );
    }

    /// <summary>
    /// 판매 패널 입력 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        _minButton.onClick.RemoveListener ( SelectMin );
        _minusButton.onClick.RemoveListener ( DecreaseQuantity );
        _plusButton.onClick.RemoveListener ( IncreaseQuantity );
        _maxButton.onClick.RemoveListener ( SelectMax );
        _confirmButton.onClick.RemoveListener ( Confirm );
        _closeButton.onClick.RemoveListener ( Close );
        _inputField.onEndEdit.RemoveListener ( SetQuantity );
    }

    /// <summary>
    /// 금액 표시 문자열 생성
    /// </summary>
    /// <param name="price">표시할 금액</param>
    /// <returns>소수점을 제외한 금액 문자열</returns>
    string FormatPrice ( float price )
    {
        return Math.Truncate ( price ).ToString ( "N0" );
    }

    /// <summary>
    /// 판매 패널 표시
    /// </summary>
    /// <param name="viewData">판매 패널 표시 데이터</param>
    public void ShowPanel ( InventoryItemSellViewData viewData )
    {
        //표시 데이터 확인
        if ( viewData == null ) return;

        gameObject.SetActive ( true );

        UpdateView ( viewData );
        _panelTween.Show ( );
    }

    /// <summary>
    /// 판매 패널 표시 갱신
    /// </summary>
    /// <param name="viewData">판매 패널 표시 데이터</param>
    public void UpdateView ( InventoryItemSellViewData viewData )
    {
        //표시 데이터 확인
        if ( viewData == null ) return;

        _icon.SetIconSprite( viewData.Icon );
        _nameText.text = viewData.ItemName;
        _priceText.text = $"판매 가격: {FormatPrice ( viewData.UnitPrice )}";
        _quantityText.text = $"보유 수량: {viewData.MaxQuantity}";
        _notificationText.text =
            $"{viewData.SelectedQuantity}개 판매 시 " +
            $"{FormatPrice ( viewData.TotalPrice )} 획득합니다.";

        //이벤트를 다시 발생시키지 않고 입력값 갱신
        _inputField.SetTextWithoutNotify ( viewData.SelectedQuantity.ToString ( ) );

        //현재 수량에 따라 버튼 상태 표시
        bool isMin = viewData.SelectedQuantity <= 1;
        bool isMax = viewData.SelectedQuantity >= viewData.MaxQuantity;

        _minButton.interactable = isMin == false;
        _minusButton.interactable = isMin == false;
        _plusButton.interactable = isMax == false;
        _maxButton.interactable = isMax == false;
    }

    /// <summary>
    /// 판매 패널 숨김
    /// </summary>
    public void HidePanel ( )
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 판매 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 판매 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 최소 판매 수량 요청
    /// </summary>
    void SelectMin ( )
    {
        OnSellMin?.Invoke ( );
    }

    /// <summary>
    /// 판매 수량 감소 요청
    /// </summary>
    void DecreaseQuantity ( )
    {
        OnQuantityChanged?.Invoke ( -1 );
    }

    /// <summary>
    /// 판매 수량 증가 요청
    /// </summary>
    void IncreaseQuantity ( )
    {
        OnQuantityChanged?.Invoke ( 1 );
    }

    /// <summary>
    /// 최대 판매 수량 요청
    /// </summary>
    void SelectMax ( )
    {
        OnSellMax?.Invoke ( );
    }

    /// <summary>
    /// 판매 수량 직접 설정 요청
    /// </summary>
    /// <param name="value">입력한 수량</param>
    void SetQuantity ( string value )
    {
        OnQuantitySet?.Invoke ( value );
    }

    /// <summary>
    /// 판매 확정 요청
    /// </summary>
    void Confirm ( )
    {
        OnSellConfirm?.Invoke ( );
    }

    /// <summary>
    /// 판매 패널 닫기 요청
    /// </summary>
    void Close ( )
    {
        OnClose?.Invoke ( );
    }
}
