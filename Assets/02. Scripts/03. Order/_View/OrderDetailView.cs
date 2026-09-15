using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주문 상세 뷰 - 주문 정보 갱신
/// </summary>
public class OrderDetailView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _titleText;      //주문 제목 텍스트
    [SerializeField] TMP_Text _costText;        //제작 코스트 텍스트
    [SerializeField] TMP_Text _orderDateText;       //주문일 텍스트
    [SerializeField] TMP_Text _deliveryText;        //납품일 텍스트
    [SerializeField] TMP_Text _finalDueText;       //지연 납품 기한 텍스트
    [SerializeField] TMP_Text _acceptDueText;       //남은 기한 텍스트
    [SerializeField] TMP_Text _stateText;       //주문 상태 텍스트

    [SerializeField] OrderInfoSlotView [ ] _requirementSlots;        //주요 요구 사항 슬롯 배열
    [SerializeField] OrderInfoSlotView [ ] _wishSlots;       //희망 사항 슬롯 배열
    [SerializeField] TMP_Text _specialText;       //특수 조건 텍스트

    [SerializeField] TMP_Text _confirmText;      //확인 버튼 텍스트
    [SerializeField] TMP_Text _cancelText;       //취소 버튼 텍스트

    [SerializeField] Button _confirmButton;      //수락 버튼
    [SerializeField] Button _cancelButton;      //거절 버튼
    [SerializeField] Button _closeButton;       //닫기 버튼
    [SerializeField] PanelTweenView _panelTween;       //주문 상세 패널 연출
    [SerializeField] Color _deadlineAlertColor = new Color( 0.85f, 0.2f, 0.2f, 1f );       //기한 임박 문구 색상

    string _orderId;        //현재 주문 아이디
    Color _normalDeadlineColor;       //일반 기한 문구 색상

    /// <summary>
    /// 확인 버튼 클릭 이벤트
    /// </summary>
    public event Action<string> OnConfirmed;
    /// <summary>
    /// 취소 버튼 클릭 이벤트
    /// </summary>
    public event Action<string> OnCanceled;
    /// <summary>
    /// 닫힘 버튼 클릭 이벤트
    /// </summary>
    public event Action OnClose;

    /// <summary>
    /// 주문 상세의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 상세 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.Requirement:
                target = _requirementSlots.Length > 0
                    ? _requirementSlots [ 0 ].transform.parent as RectTransform
                    : null;
                break;

            case TutorialTargetId.Wish:
                target = _wishSlots.Length > 0
                    ? _wishSlots [ 0 ].transform.parent as RectTransform
                    : null;
                break;

            case TutorialTargetId.OrderDeadline:
                target = _acceptDueText.transform as RectTransform;
                break;

            case TutorialTargetId.OrderAcceptButton:
                target = _confirmButton.transform as RectTransform;
                break;

            default:
                target = null;
                return false;
        }

        return target != null;
    }

    #region ----- 초기화/이벤트 연결 -----
    /// <summary>
    /// 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        //주문 처리 주요 버튼에 공용 클릭 연출 연결
        _confirmButton.BindClickHighlight( );
        _cancelButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        //프리팹의 기본 기한 문구 색상 저장
        _normalDeadlineColor = _acceptDueText.color;

        //상세 패널 버튼 연결
        _confirmButton.onClick.AddListener( Confirm );
        _cancelButton.onClick.AddListener( Cancel );
        _closeButton.onClick.AddListener( Close );
    }

    /// <summary>
    /// 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        //상세 패널 버튼 연결 해제
        _confirmButton.onClick.RemoveListener( Confirm );
        _cancelButton.onClick.RemoveListener( Cancel );
        _closeButton.onClick.RemoveListener( Close );
    }

    /// <summary>
    /// 주문 상세 뷰 초기화
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    public void Init ( OrderDetailViewData viewData )
    {
        //현재 표시 중인 주문 아이디 저장
        _orderId = viewData.OrderId;

        //상세 정보 표시
        SetTitleText( viewData );
        SetCostText( viewData );
        SetOrderDateText( viewData );
        SetDeliveryText( viewData );
        SetFinalDueText( viewData );
        SetDeadlineText( viewData );
        UpdateStateText( viewData );
        UpdateButtons( viewData );
        UpdateSlots( _requirementSlots, viewData.Requirements );
        UpdateSlots( _wishSlots, viewData.Wishes );
        SetSpecialText( viewData );
    }
    #endregion

    #region ----- 화면 표시 -----
    /// <summary>
    /// 주문 상세 패널 표시
    /// </summary>
    public void Show ( )
    {
        gameObject.SetActive ( true );
        _panelTween.Show ( );
    }

    /// <summary>
    /// 주문 상세 패널 숨김
    /// </summary>
    public void Hide ( )
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 주문 상세 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 주문 상세 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 주문 제목 텍스트 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void SetTitleText ( OrderDetailViewData viewData )
    {
        _titleText.text = viewData.OrderTitle;
    }

    /// <summary>
    /// 제작 코스트 텍스트 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void SetCostText ( OrderDetailViewData viewData )
    {
        _costText.text = $"제작 코스트: {viewData.MaxCraftCost}";
    }

    /// <summary>
    /// 주문일 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void SetOrderDateText ( OrderDetailViewData viewData )
    {
        _orderDateText.text = $"주문일: {viewData.CreatedMonth}월 {viewData.CreatedDay}일";
    }

    /// <summary>
    /// 납품일 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void SetDeliveryText ( OrderDetailViewData viewData )
    {
        //납품일 표시
        _deliveryText.text = $"납품일: {viewData.DeliveryMonth}월 {viewData.DeliveryDay}일";
    }

    /// <summary>
    /// 지연 납품 기한 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void SetFinalDueText ( OrderDetailViewData viewData )
    {
        //배송 대기 주문만 지연 납품 기한 표시
        _finalDueText.gameObject.SetActive( viewData.ShowFinalDue );

        if ( viewData.ShowFinalDue )
            _finalDueText.text = $"지연 납품: ~{viewData.FinalDueMonth}월 {viewData.FinalDueDay}일";
    }

    /// <summary>
    /// 남은 기한 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void SetDeadlineText ( OrderDetailViewData viewData )
    {
        //남은 기한 문구가 있는 주문만 표시
        bool showDeadline = string.IsNullOrEmpty( viewData.DeadlineText ) == false;
        _acceptDueText.gameObject.SetActive( showDeadline );

        //재사용되는 상세 뷰의 기한 문구 색상 갱신
        _acceptDueText.color = viewData.IsDeadlineImminent
            ? _deadlineAlertColor
            : _normalDeadlineColor;

        if ( showDeadline )
        {
            _acceptDueText.text = viewData.IsDeadlineImminent
                ? $"{viewData.DeadlineText} [임박]"
                : viewData.DeadlineText;
        }
    }

    /// <summary>
    /// 주문 상태 설정
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void UpdateStateText ( OrderDetailViewData viewData )
    {
        _stateText.text = $"상태: {viewData.OrderState}";
    }

    /// <summary>
    /// 주문 행동 버튼 갱신
    /// </summary>
    /// <param name="viewData">주문 상세 표시 데이터</param>
    void UpdateButtons ( OrderDetailViewData viewData )
    {
        //버튼 표시 여부 확인
        bool showConfirm = string.IsNullOrEmpty( viewData.ConfirmText ) == false;
        bool showCancel = string.IsNullOrEmpty( viewData.CancelText ) == false;

        //버튼 표시 상태 갱신
        _confirmButton.gameObject.SetActive( showConfirm );
        _cancelButton.gameObject.SetActive( showCancel );

        //표시할 버튼 문구 갱신
        if ( showConfirm ) _confirmText.text = viewData.ConfirmText;
        if ( showCancel ) _cancelText.text = viewData.CancelText;
    }

    /// <summary>
    /// 주문 정보 슬롯 배열 갱신
    /// </summary>
    /// <param name="slots">갱신할 슬롯 배열</param>
    /// <param name="viewDatas">슬롯 표시 데이터</param>
    void UpdateSlots (
        OrderInfoSlotView [ ] slots,
        System.Collections.Generic.IReadOnlyList<OrderInfoSlotViewData> viewDatas )
    {
        int viewDataCount = viewDatas?.Count ?? 0;

        for ( int i = 0; i < slots.Length; i++ )
        {
            bool isVisible = i < viewDataCount;
            slots [ i ].SetVisible( isVisible );

            if ( isVisible )
                slots [ i ].SetData( viewDatas [ i ] );
        }
    }

    /// <summary>
    /// 특수 조건 텍스트 설정
    /// </summary>
    void SetSpecialText ( OrderDetailViewData viewData )
    {
        _specialText.text = string.IsNullOrWhiteSpace( viewData.SpecialConditions )
            ? "- 없음"
            : viewData.SpecialConditions;
    }
    #endregion

    #region ----- 이벤트 발행 -----
    /// <summary>
    /// 확인 이벤트 발행
    /// </summary>
    void Confirm ()
    {
        //현재 주문 아이디 전달
        OnConfirmed?.Invoke( _orderId );
    }

    /// <summary>
    /// 취소 이벤트 발행
    /// </summary>
    void Cancel ()
    {
        //현재 주문 아이디 전달
        OnCanceled?.Invoke( _orderId );
    }

    /// <summary>
    /// 닫힘 이벤트 발행
    /// </summary>
    void Close ()
    {
        OnClose?.Invoke( );
    }
    #endregion
}
