using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배송 뷰 - 배송 예상 표시와 입력 중계
/// </summary>
public class DeliveryView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //배송 패널 연출

    [Header( "----- 주문 정보 -----" )]
    [SerializeField] TMP_Text _titleText;       //배송 패널 제목
    [SerializeField] TMP_Text _orderTitleText;      //주문 제목
    [SerializeField] TMP_Text _specialOrderText;        //특수 주문
    [SerializeField] TMP_Text _specialConditionText;       //특수 조건

    [Header( "----- 제작 결과 -----" )]
    [SerializeField] TMP_Text _craftScoreText;       //제작 점수
    [SerializeField] TMP_Text _expectedGradeText;        //예상 또는 확정 등급
    [SerializeField] TMP_Text _deliveryResultText;       //배송 결과

    [Header( "----- 제작 상세 -----" )]
    [SerializeField] OrderInfoProgressSlotView _requirementCheckSlot;       //주요 요구 달성 슬롯
    [SerializeField] OrderInfoProgressSlotView _wishCheckSlot;      //희망 사항 달성 슬롯
    [SerializeField] OrderInfoSlotView [ ] _activeThemeSlots;       //활성 테마 슬롯 배열

    [Header( "----- 보상 내역 -----" )]
    [SerializeField] TMP_Text _baseRewardText;       //기본 보상
    [SerializeField] TMP_Text _gradeCorrectionText;      //등급 보정
    [SerializeField] TMP_Text _deliveryCorrectionText;       //배송 보정
    [SerializeField] TMP_Text _specialCorrectionText;        //특수 보정
    [SerializeField] TMP_Text _expectedRewardText;       //예상 또는 최종 보상

    [Header( "----- 배송 방식 -----" )]
    [SerializeField] ToggleGroup _deliveryToggleGroup;       //배송 방식 단일 선택 그룹
    [SerializeField] Toggle _directDeliveryToggle;       //직접 배송 토글
    [SerializeField] Toggle _employeeDeliveryToggle;       //직원 배송 토글
    [SerializeField] TooltipArea _directDeliveryTooltip;       //직접 배송 안내 툴팁
    [SerializeField] TooltipArea _employeeDeliveryTooltip;       //직원 배송 안내 툴팁

    [Header( "----- 버튼 -----" )]
    [SerializeField] Button _closeButton;        //상단 닫기 버튼
    [SerializeField] Button _deliveryButton;     //배송 버튼
    [SerializeField] Button _cancelButton;       //돌아가기 버튼
    [SerializeField] TMP_Text _deliveryButtonText;       //배송 버튼 문구
    [SerializeField] TMP_Text _cancelButtonText;     //돌아가기 버튼 문구
    [SerializeField] Color _disabledButtonColor =
        new Color( 0.55f, 0.55f, 0.55f, 0.65f );       //배송 불가 버튼 배경 색상
    [SerializeField] Color _disabledButtonTextColor =
        new Color( 0.45f, 0.45f, 0.45f, 1f );       //배송 불가 버튼 문구 색상

    Color _deliveryButtonColor;       //배송 가능 버튼 배경 원본 색상
    Color _deliveryButtonTextColor;       //배송 가능 버튼 문구 원본 색상
    Coroutine _scrollResetRoutine;       //활성 스크롤 최상단 이동 루틴

    /// <summary>
    /// 배송 실행 이벤트
    /// </summary>
    public event Action OnDelivered;
    /// <summary>
    /// 배송 방식 선택 이벤트
    /// </summary>
    public event Action<DeliveryMethod> OnMethodChanged;
    /// <summary>
    /// 배송 화면 돌아가기 이벤트
    /// </summary>
    public event Action OnCanceled;

    /// <summary>
    /// 배송 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.DirectDelivery:
                target = _directDeliveryToggle.transform as RectTransform;
                break;

            case TutorialTargetId.DeliverySummary:
                target = _expectedRewardText.rectTransform.parent
                    as RectTransform;
                break;

            case TutorialTargetId.DeliveryStartButton:
                target = _deliveryButton.transform as RectTransform;
                break;

            default:
                target = null;
                return false;
        }

        return target != null;
    }

    /// <summary>
    /// 튜토리얼 배송 방식 선택 대기 상태 적용
    /// </summary>
    /// <param name="isPending">사용자 배송 방식 선택 대기 여부</param>
    public void SetTutorialMethodSelectionPending ( bool isPending )
    {
        _deliveryToggleGroup.allowSwitchOff = isPending;

        if ( isPending == false )
            return;

        //직접 선택하도록 기존 선택 상태와 배송 실행 입력 제거
        _directDeliveryToggle.SetIsOnWithoutNotify( false );
        _employeeDeliveryToggle.SetIsOnWithoutNotify( false );
        SetDeliveryButtonInteractable( false );
    }


    /// <summary>
    /// 배송 입력 연결
    /// </summary>
    void Awake ()
    {
        //배송과 취소 버튼에 공용 클릭 연출 연결
        _closeButton.BindClickHighlight( );
        _deliveryButton.BindClickHighlight( );
        _cancelButton.BindClickHighlight( );

        _closeButton.onClick.AddListener( Close );
        _deliveryButton.onClick.AddListener( Deliver );
        _cancelButton.onClick.AddListener( Cancel );
        _deliveryButtonColor = _deliveryButton.image.color;
        _deliveryButtonTextColor = _deliveryButtonText.color;

        _deliveryToggleGroup.allowSwitchOff = false;
        _directDeliveryToggle.group = _deliveryToggleGroup;
        _employeeDeliveryToggle.group = _deliveryToggleGroup;

        _directDeliveryToggle.onValueChanged.AddListener( SelectDirectDelivery );
        _employeeDeliveryToggle.onValueChanged.AddListener( SelectEmployeeDelivery );
    }

    /// <summary>
    /// 배송 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _closeButton.onClick.RemoveListener( Close );
        _deliveryButton.onClick.RemoveListener( Deliver );
        _cancelButton.onClick.RemoveListener( Cancel );

        _directDeliveryToggle.onValueChanged.RemoveListener( SelectDirectDelivery );
        _employeeDeliveryToggle.onValueChanged.RemoveListener( SelectEmployeeDelivery );
    }

    /// <summary>
    /// 화면 비활성화 시 중단된 스크롤 루틴 참조 초기화
    /// </summary>
    void OnDisable ()
    {
        _scrollResetRoutine = null;
    }

    /// <summary>
    /// 배송 예상 표시
    /// </summary>
    /// <param name="viewData">배송 표시 데이터</param>
    public void ShowPreview ( DeliveryViewData viewData )
    {
        //배송 뷰 활성화
        gameObject.SetActive( true );

        UpdateTexts( viewData );

        _titleText.text = "배송 정보";
        _deliveryButtonText.text = "배송 출발";
        _cancelButtonText.text = "돌아가기";

        _deliveryButton.gameObject.SetActive( true );
        _cancelButton.gameObject.SetActive( true );

        ResetActiveScrollsToTop( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 배송 중 정보 표시
    /// </summary>
    /// <param name="viewData">배송 표시 데이터</param>
    public void ShowShipping ( DeliveryViewData viewData )
    {
        //배송 뷰 활성화
        gameObject.SetActive( true );

        UpdateTexts( viewData );

        _titleText.text = "배송 중";
        _cancelButtonText.text = "돌아가기";

        //배송 중에는 중복 출발 차단
        _deliveryButton.gameObject.SetActive( false );
        _cancelButton.gameObject.SetActive( true );

        ResetActiveScrollsToTop( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 배송 완료 결과 표시
    /// </summary>
    /// <param name="viewData">배송 표시 데이터</param>
    public void ShowResult ( DeliveryViewData viewData )
    {
        //배송 뷰 활성화
        gameObject.SetActive( true );

        UpdateTexts( viewData );

        _titleText.text = "배송 결과";
        _cancelButtonText.text = "돌아가기";

        _deliveryButton.gameObject.SetActive( false );
        _cancelButton.gameObject.SetActive( true );

        ResetActiveScrollsToTop( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 배송 실패 안내 표시
    /// </summary>
    /// <param name="message">배송 실패 안내 문구</param>
    public void ShowFailure ( string message )
    {
        _deliveryResultText.text = message;
    }

    /// <summary>
    /// 배송 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void Hide ( Action onComplete = null )
    {
        _panelTween.Hide( () => CompleteHide( onComplete ) );
    }

    /// <summary>
    /// 배송 패널 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 배송 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive( false );
        onComplete?.Invoke( );
    }

    /// <summary>
    /// 배송 정보 갱신
    /// </summary>
    /// <param name="viewData">배송 표시 데이터</param>
    void UpdateTexts ( DeliveryViewData viewData )
    {
        //주문 정보 표시
        _orderTitleText.text = viewData.OrderTitle;
        _specialOrderText.text = viewData.SpecialOrder;
        _specialConditionText.text = viewData.SpecialCondition;

        //제작 결과 표시
        _craftScoreText.text = viewData.CraftScoreText;
        _expectedGradeText.text = viewData.ExpectedGradeText;
        _deliveryResultText.text = viewData.DeliveryResultText;

        //고정된 제목과 아이콘을 유지하고 달성 수치만 갱신
        _requirementCheckSlot.SetProgress(
            viewData.RequirementCompletedCount,
            viewData.RequirementTotalCount );
        _wishCheckSlot.SetProgress(
            viewData.WishCompletedCount, viewData.WishTotalCount );
        UpdateActiveThemeSlots( viewData.ActiveThemes );

        //보상 항목의 고정 제목을 유지하고 값만 갱신
        _baseRewardText.text = viewData.BaseRewardText;
        _gradeCorrectionText.text = viewData.GradeCorrectionText;
        _deliveryCorrectionText.text = viewData.DeliveryCorrectionText;
        //특수 주문 보정 항목을 줄별로 표시해 자동 글자 축소 방지
        _specialCorrectionText.text =
            viewData.SpecialCorrectionText.Replace( " / ", "\n" );
        _expectedRewardText.text = viewData.ExpectedRewardText;
    }

    /// <summary>
    /// 활성 테마 고정 슬롯 배열 갱신
    /// </summary>
    /// <param name="viewDatas">활성 테마 슬롯 표시 데이터</param>
    void UpdateActiveThemeSlots (
        IReadOnlyList<OrderInfoSlotViewData> viewDatas )
    {
        int viewDataCount = viewDatas?.Count ?? 0;

        for ( int i = 0; i < _activeThemeSlots.Length; i++ )
        {
            bool isVisible = i < viewDataCount;
            _activeThemeSlots [ i ].SetVisible( isVisible );

            if ( isVisible )
                _activeThemeSlots [ i ].SetData( viewDatas [ i ] );
        }
    }

    /// <summary>
    /// 배송 방식 선택 상태 표시
    /// </summary>
    /// <param name="method">현재 선택한 배송 방식</param>
    /// <param name="canUseDirect">직접 배송 가능 여부</param>
    /// <param name="canUseEmployee">직원 배송 가능 여부</param>
    /// <param name="directTooltip">직접 배송 안내</param>
    /// <param name="employeeTooltip">직원 배송 안내</param>
    public void SetDeliveryMethod (
        DeliveryMethod method,
        bool hasSelection,
        bool canUseDirect, bool canUseEmployee,
        string directTooltip, string employeeTooltip )
    {
        _directDeliveryToggle.interactable = canUseDirect;
        _employeeDeliveryToggle.interactable = canUseEmployee;

        //배송 대기 화면 진입 직후에는 두 방식을 모두 해제
        if ( hasSelection == false )
        {
            _deliveryToggleGroup.allowSwitchOff = true;
            _directDeliveryToggle.SetIsOnWithoutNotify( false );
            _employeeDeliveryToggle.SetIsOnWithoutNotify( false );
            _deliveryToggleGroup.allowSwitchOff = false;
        }
        else if ( method == DeliveryMethod.Direct )
        {
            _employeeDeliveryToggle.SetIsOnWithoutNotify( false );
            _directDeliveryToggle.SetIsOnWithoutNotify( true );
        }
        else
        {
            _directDeliveryToggle.SetIsOnWithoutNotify( false );
            _employeeDeliveryToggle.SetIsOnWithoutNotify( true );
        }

        //토글별 배송 방식 안내 갱신
        _directDeliveryTooltip.SetDescription( directTooltip );
        _employeeDeliveryTooltip.SetDescription( employeeTooltip );
        _directDeliveryTooltip.Hide( );
        _employeeDeliveryTooltip.Hide( );

        bool canDeliver = hasSelection &&
            ( method == DeliveryMethod.Direct
                ? canUseDirect
                : canUseEmployee );

        SetDeliveryButtonInteractable( canDeliver );
    }

    /// <summary>
    /// 배송 버튼의 입력과 비활성 시각 상태 동시 적용
    /// </summary>
    /// <param name="isInteractable">배송 가능 여부</param>
    void SetDeliveryButtonInteractable ( bool isInteractable )
    {
        _deliveryButton.interactable = isInteractable;
        _deliveryButton.image.color = isInteractable
            ? _deliveryButtonColor
            : _disabledButtonColor;
        _deliveryButtonText.color = isInteractable
            ? _deliveryButtonTextColor
            : _disabledButtonTextColor;
    }

    /// <summary>
    /// 현재 배송 화면의 활성 스크롤을 최상단으로 이동
    /// </summary>
    void ResetActiveScrollsToTop ()
    {
        if ( _scrollResetRoutine != null )
            StopCoroutine( _scrollResetRoutine );

        _scrollResetRoutine = StartCoroutine(
            ResetActiveScrollsToTopRoutine( ) );
    }

    /// <summary>
    /// 레이아웃 갱신 이후 활성 스크롤 최상단 위치 확정
    /// </summary>
    /// <returns>스크롤 위치 갱신 대기</returns>
    IEnumerator ResetActiveScrollsToTopRoutine ()
    {
        yield return null;

        Canvas.ForceUpdateCanvases( );
        ScrollRect [ ] scrollRects =
            GetComponentsInChildren<ScrollRect>( false );

        for ( int i = 0; i < scrollRects.Length; i++ )
        {
            ScrollRect scrollRect = scrollRects [ i ];
            scrollRect.StopMovement( );

            if ( scrollRect.content != null )
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    scrollRect.content );

            scrollRect.verticalNormalizedPosition = 1f;
        }

        yield return new WaitForEndOfFrame( );

        for ( int i = 0; i < scrollRects.Length; i++ )
            scrollRects [ i ].verticalNormalizedPosition = 1f;

        _scrollResetRoutine = null;
    }

    /// <summary>
    /// 직접 배송 선택 입력 전달
    /// </summary>
    /// <param name="isOn">토글 선택 여부</param>
    void SelectDirectDelivery ( bool isOn )
    {
        if ( isOn )
            OnMethodChanged?.Invoke( DeliveryMethod.Direct );
    }

    /// <summary>
    /// 직원 배송 선택 입력 전달
    /// </summary>
    /// <param name="isOn">토글 선택 여부</param>
    void SelectEmployeeDelivery ( bool isOn )
    {
        if ( isOn )
            OnMethodChanged?.Invoke( DeliveryMethod.Employee );
    }

    /// <summary>
    /// 배송 입력 전달
    /// </summary>
    void Deliver ()
    {
        OnDelivered?.Invoke( );
    }

    /// <summary>
    /// 배송 화면 돌아가기 입력 전달
    /// </summary>
    void Cancel ()
    {
        OnCanceled?.Invoke( );
    }

    /// <summary>
    /// 배송 화면 닫기 입력 전달
    /// </summary>
    void Close ()
    {
        OnCanceled?.Invoke( );
    }
}
