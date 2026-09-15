using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 슬롯 뷰 - 상품 정보, 품절 상태 표시, 선택 이벤트 발행
/// </summary>
public class ShopSlotView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _icon;       //상품 아이콘
    [SerializeField] TMP_Text _nameText;        //상품 이름
    [SerializeField] TMP_Text _stockText;       //남은 재고
    [SerializeField] TMP_Text _priceText;       //현재 가격
    [SerializeField] Button _selectButton;      //상품 선택 버튼
    [SerializeField] CanvasGroup _canvasGroup;       //캔버스 그룹

    string _id;     //상품 아이디
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출
    bool _isSelected;       //현재 선택 강조 상태

    /// <summary>
    /// 상품 선택 이벤트
    /// </summary>
    public event Action<string> OnSelected;

    /// <summary>
    /// 상품 선택 입력 연결
    /// </summary>
    void Awake ()
    {
        //상품 슬롯에 공용 등장과 선택 연출 연결
        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );

        //상품 선택 버튼 연결
        _selectButton.onClick.AddListener( SelectSlot );
    }

    /// <summary>
    /// 상품 슬롯 초기화
    /// </summary>
    /// <param name="viewData">상품 슬롯 표시 데이터</param>
    public void Init ( ShopSlotViewData viewData )
    {
        //상품 아이디 저장
        _id = viewData.Id;

        //상품 정보 표시
        SetIcon( viewData.Icon );
        SetName( viewData.Name );
        SetPrice( viewData.Price );
        SetStock( viewData.RemainingStock );
        SetState( viewData.IsSoldOut, viewData.IsLocked );

        //풀 인스턴스가 처음 생성된 경우에만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 아이콘 갱신
    /// </summary>
    /// <param name="icon">아이콘</param>
    public void SetIcon ( Sprite icon )
    {
        //상품 아이콘 표시
        _icon.SetIconSprite( icon );
    }

    /// <summary>
    /// 이름 갱신
    /// </summary>
    /// <param name="itemName">이름</param>
    public void SetName ( string itemName )
    {
        //상품 이름 표시
        _nameText.text = itemName;
    }

    /// <summary>
    /// 현재 가격 갱신
    /// </summary>
    /// <param name="price">현재 가격</param>
    public void SetPrice ( float price )
    {
        //소수점 아래를 제외한 가격 표시
        _priceText.text = Mathf.FloorToInt( price ).ToString( "N0" );
    }

    /// <summary>
    /// 남은 재고 갱신
    /// </summary>
    /// <param name="remainingStock">남은 재고</param>
    public void SetStock ( int remainingStock )
    {
        //남은 재고 표시
        _stockText.text = $"재고: {remainingStock}개";
    }

    /// <summary>
    /// 상품 슬롯 상태 표시
    /// </summary>
    /// <param name="isSoldOut">품절 여부</param>
    /// <param name="isLocked">잠금 여부</param>
    public void SetState ( bool isSoldOut, bool isLocked )
    {
        //품절 또는 잠금 상태는 회색으로 표시
        float alpha = isSoldOut || isLocked ? 0.5f : 1f;
        _canvasGroup.alpha = alpha;

        //상태 Alpha를 슬롯 연출의 복구 기준으로 갱신
        _slotTween.SetBaseAlpha( alpha );

        //상품 선택 가능 여부는 프레젠터에서 최종 판정
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _selectButton.interactable = true;
    }

    /// <summary>
    /// 상품 슬롯 선택 강조 설정
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected ( bool isSelected )
    {
        //같은 선택 상태의 일반 갱신에는 연출을 반복하지 않음
        if ( _isSelected == isSelected ) return;

        _isSelected = isSelected;

        if ( isSelected )
        {
            //새로 선택한 상품 슬롯을 짧게 강조
            _slotTween.PlaySelected( );
            return;
        }

        //선택 해제 시 진행 중인 연출을 즉시 복구
        _slotTween.ResetInstant( );
    }

    /// <summary>
    /// 풀 반환 전 상품 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        //재사용 슬롯의 Tween과 표시 식별자 초기화
        _slotTween.ResetInstant( );
        _id = null;
        _isSelected = false;
    }

    /// <summary>
    /// 상품 슬롯 갱신
    /// </summary>
    /// <param name="viewData">상품 슬롯 표시 데이터</param>
    public void UpdateView ( ShopSlotViewData viewData )
    {
        //상품 정보 갱신
        SetIcon( viewData.Icon );
        SetName( viewData.Name );
        SetPrice( viewData.Price );
        SetStock( viewData.RemainingStock );
        SetState( viewData.IsSoldOut, viewData.IsLocked );
    }

    /// <summary>
    /// 상품 선택 이벤트 발행
    /// </summary>
    public void SelectSlot ()
    {
        //선택 상품 아이디 전달
        OnSelected?.Invoke( _id );
    }
}
