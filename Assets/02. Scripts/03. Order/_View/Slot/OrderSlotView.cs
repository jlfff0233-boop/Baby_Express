using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주문 슬롯 뷰 - 주문 번호, 주문일, 상태별 기한
/// </summary>
public class OrderSlotView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _orderNumText;     //주문 번호
    [SerializeField] TMP_Text _orderDateText;       //주문일
    [SerializeField] TMP_Text _acceptDueText;       //상태별 기한
    [SerializeField] Image _stateIcon;      //주문 상태 아이콘
    [SerializeField] Button _detailButton;      //주문 상세 버튼
    [SerializeField] Outline _selectionOutline;       //제작 주문 선택 외곽선
    [SerializeField] GameObject _specialIcon;       //특수 주문 아이콘
    [SerializeField] UIHighlightView _specialIconHighlight;       //특수 주문 아이콘 연출
    [SerializeField] Color _deadlineAlertColor = new Color( 0.85f, 0.2f, 0.2f, 1f );       //기한 임박 문구 색상

    string _orderId;        //주문 아이디
    string _specialOrderId;       //현재 특수 아이콘을 표시한 주문 아이디
    Color _normalDeadlineColor;       //일반 기한 문구 색상
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출
    bool _isSelected;       //현재 선택 강조 상태

    /// <summary>
    /// 디테일 버튼 클릭 이벤트
    /// </summary>
    public event Action<string> OnClickDetail;


    /// <summary>
    /// 입력 이벤트와 슬롯 연출 연결
    /// </summary>
    void Awake ( )
    {
        //슬롯의 원래 기한 문구 색상 저장
        _normalDeadlineColor = _acceptDueText.color;

        //공용 슬롯 등장과 선택 연출 연결
        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView> ( );

        //상세 버튼 연결
        _detailButton.onClick.AddListener ( ClickDetail );
    }

    /// <summary>
    /// 입력 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        //상세 버튼 연결 해제
        _detailButton.onClick.RemoveListener ( ClickDetail );
    }

    /// <summary>
    /// 슬롯 비활성화 시 재사용 상태 초기화
    /// </summary>
    void OnDisable ( )
    {
        ResetForReuse ( );
    }

    /// <summary>
    /// 주문 슬롯 초기화
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    public void Init ( OrderSlotViewData viewData )
    {
        //이전 주문의 선택과 특수 주문 표시 상태 초기화
        ResetForReuse ( );

        //현재 주문 아이디 저장
        _orderId = viewData.OrderId;

        //슬롯 정보 표시
        SetOrderNumber ( viewData );
        SetOrderDate ( viewData );
        SetDeadline ( viewData );
        UpdateStateIcon ( viewData );
        UpdateSpecialState ( viewData );

        //같은 풀 인스턴스에서 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce ( );
    }

    /// <summary>
    /// 주문 슬롯 선택 표시 설정
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected ( bool isSelected )
    {
        //선택 외곽선이 있으면 현재 선택 상태 표시
        if ( _selectionOutline != null )
            _selectionOutline.enabled = isSelected;

        //같은 선택 상태의 일반 갱신에는 연출을 반복하지 않음
        if ( _isSelected == isSelected ) return;

        _isSelected = isSelected;

        if ( isSelected )
        {
            //새로 선택한 주문 슬롯을 짧게 강조
            _slotTween.PlaySelected ( );
            return;
        }

        //선택 해제 시 진행 중인 연출 즉시 복구
        _slotTween.ResetInstant ( );
    }

    /// <summary>
    /// 풀 반환 전 주문 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ( )
    {
        //슬롯 전체 연출과 주문 식별자 초기화
        _slotTween.ResetInstant ( );
        _orderId = null;
        _isSelected = false;

        //선택 외곽선 즉시 숨김
        if ( _selectionOutline != null )
            _selectionOutline.enabled = false;

        //이전 기한 색상과 특수 주문 연출 초기화
        _acceptDueText.color = _normalDeadlineColor;
        ResetSpecialState ( );
    }

    /// <summary>
    /// 주문 번호 설정
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    void SetOrderNumber ( OrderSlotViewData viewData )
    {
        _orderNumText.text = $"주문 번호: {viewData.OrderNumber}";
    }

    /// <summary>
    /// 주문일 설정
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    void SetOrderDate ( OrderSlotViewData viewData )
    {
        _orderDateText.text = $"주문일: {viewData.CreatedMonth}월 {viewData.CreatedDay}일";
    }

    /// <summary>
    /// 상태별 기한 설정
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    void SetDeadline ( OrderSlotViewData viewData )
    {
        //기한 임박 상태면 안내 문구와 강조 색상 적용
        _acceptDueText.text = viewData.IsDeadlineImminent
            ? $"{viewData.DeadlineText} [임박]"
            : viewData.DeadlineText;

        _acceptDueText.color = viewData.IsDeadlineImminent
            ? _deadlineAlertColor
            : _normalDeadlineColor;
    }

    /// <summary>
    /// 주문 상태 아이콘 설정
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    void UpdateStateIcon ( OrderSlotViewData viewData )
    {
        _stateIcon.SetIconSprite( viewData.IconSprite );
    }

    /// <summary>
    /// 특수 주문 표시 설정
    /// </summary>
    /// <param name="viewData">주문 슬롯 표시 데이터</param>
    void UpdateSpecialState ( OrderSlotViewData viewData )
    {
        if ( viewData.IsSpecial == false )
        {
            ResetSpecialState( );
            return;
        }

        //같은 특수 주문의 일반 갱신이면 등장 연출을 반복하지 않음
        if ( _specialOrderId == viewData.OrderId &&
            _specialIcon.activeSelf )
            return;

        ResetSpecialState( );

        _specialOrderId = viewData.OrderId;
        _specialIcon.SetActive( true );
        _specialIconHighlight.PlayAppearLoop( );
    }

    /// <summary>
    /// 특수 주문 아이콘과 실행 중인 연출 초기화
    /// </summary>
    void ResetSpecialState ()
    {
        if ( _specialIcon.activeSelf )
            _specialIconHighlight.Stop( );

        _specialOrderId = null;
        _specialIcon.SetActive( false );
    }

    /// <summary>
    /// 주문 상세 버튼 눌림 이벤트 발행
    /// </summary>
    void ClickDetail ( )
    {
        //현재 주문 아이디 전달
        OnClickDetail?.Invoke ( _orderId );
    }
}
