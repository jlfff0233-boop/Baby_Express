using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 제작 완료 뷰 - 제작 결과 표시와 완료 입력 중계
/// </summary>
public class CraftCompleteView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //제작 완료 패널 연출

    [Header( "----- 제작 조건 -----" )]
    [SerializeField] TMP_Text _requirementText;       //주요 요구 달성 상태
    [SerializeField] OrderInfoProgressSlotView [ ] _requirementSlots;       //주요 요구 사항 슬롯
    [SerializeField] GameObject _requirementEmpty;       //주요 요구 홀수 정렬용 빈 슬롯
    [SerializeField] TMP_Text _wishText;      //희망 사항 달성 상태
    [SerializeField] OrderInfoProgressSlotView [ ] _wishSlots;      //희망 사항 슬롯
    [SerializeField] GameObject _wishEmpty;       //희망 사항 홀수 정렬용 빈 슬롯
    [SerializeField] TMP_Text _specialText;       //특수 조건 결과
    [SerializeField] OrderInfoProgressSlotView [ ] _specialSlots;       //특수 조건 슬롯
    [SerializeField] GameObject _specialEmpty;       //특수 조건 홀수 정렬용 빈 슬롯

    [Header( "----- 테마와 점수 -----" )]
    [SerializeField] CraftThemeSlotView [ ] _themeCheckSlots;       //활성, 완성 테마 슬롯
    [SerializeField] GameObject _themeCheckEmpty;       //테마 상태 홀수 정렬용 빈 슬롯
    [SerializeField] CraftThemeSlotView [ ] _scoreCheckSlots;       //테마 점수 슬롯
    [SerializeField] GameObject _scoreCheckEmpty;       //테마 점수 홀수 정렬용 빈 슬롯
    [SerializeField] TMP_Text _finalScoreText;        //최종 제작 점수

    [Header( "----- 버튼 -----" )]
    [SerializeField] Button _closeButton;     //상단 닫기 버튼
    [SerializeField] Button _confirmButton;       //확인 버튼
    [FormerlySerializedAs( "_moveToOrderButton" )]
    [SerializeField] Button _deliveryButton;       //배송 버튼

    [Header( "----- 문구 색상 -----" )]
    [SerializeField] Color _normalColor = Color.black;      //일반 문구 색상
    [SerializeField] Color _positiveColor = new Color( 0.1f, 0.6f, 0.2f );     //달성, 추가 색상
    [SerializeField] Color _partialColor = new Color( 0.9f, 0.65f, 0.1f );      //일부 달성 색상
    [SerializeField] Color _negativeColor = new Color( 0.8f, 0.15f, 0.15f );       //미달성, 감소 색상
    [SerializeField] Color _finalColor = new Color( 0.15f, 0.3f, 0.8f );       //최종 점수 색상

    Coroutine _scrollResetRoutine;       //제작 완료 스크롤 최상단 이동 루틴

    /// <summary>
    /// 배송 화면 이동 이벤트
    /// </summary>
    public event Action OnDeliverySelected;

    /// <summary>
    /// 배송 이동 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetDeliveryTarget ( out RectTransform target )
    {
        target = _deliveryButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 제작 완료 입력 연결
    /// </summary>
    void Awake ()
    {
        //제작 결과 주요 버튼에 공용 클릭 연출 연결
        _closeButton.BindClickHighlight( );
        _confirmButton.BindClickHighlight( );
        _deliveryButton.BindClickHighlight( );

        _closeButton.onClick.AddListener( SelectDelivery );
        _confirmButton.onClick.AddListener( SelectDelivery );
        _deliveryButton.onClick.AddListener( SelectDelivery );
    }

    /// <summary>
    /// 제작 완료 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _closeButton.onClick.RemoveListener( SelectDelivery );
        _confirmButton.onClick.RemoveListener( SelectDelivery );
        _deliveryButton.onClick.RemoveListener( SelectDelivery );
    }

    /// <summary>
    /// 화면 비활성화 시 중단된 스크롤 루틴 참조 초기화
    /// </summary>
    void OnDisable ()
    {
        _scrollResetRoutine = null;
    }

    /// <summary>
    /// 제작 완료 결과 표시
    /// </summary>
    public void Show ( CraftReviewViewData viewData )
    {
        gameObject.SetActive ( true );

        //주요 요구와 희망 전체 달성 여부 확인
        bool requirementsCompleted = AreConditionsCompleted( viewData.RequirementResults );
        bool wishesCompleted = AreConditionsCompleted( viewData.WishResults );

        //주요 요구 결과 표시
        _requirementText.text = requirementsCompleted
            ? ": 달성"
            : ": 미달성";
        _requirementText.color = requirementsCompleted
            ? _positiveColor
            : _negativeColor;
        UpdateConditionSlots(
            _requirementSlots, viewData.RequirementSlots );
        UpdateEmptySlot( _requirementEmpty, viewData.RequirementSlots.Count );

        //희망 결과 표시
        _wishText.text = wishesCompleted
            ? ": 달성"
            : ": 미달성";
        _wishText.color = wishesCompleted
            ? _positiveColor
            : _negativeColor;
        UpdateConditionSlots( _wishSlots, viewData.WishSlots );
        UpdateEmptySlot( _wishEmpty, viewData.WishSlots.Count );

        //특수 조건 결과 표시
        _specialText.text = viewData.SpecialResults.Count > 0
            ? CreateText( viewData.SpecialResults )
            : "- 없음";
        _specialText.color = _normalColor;
        UpdateConditionSlots( _specialSlots, viewData.SpecialSlots );
        UpdateEmptySlot( _specialEmpty, viewData.SpecialSlots.Count );

        //테마 완성 상태와 점수 상태를 각각 고정 슬롯에 표시
        UpdateThemeSlots( _themeCheckSlots, viewData.ThemeSlots, false );
        UpdateEmptySlot( _themeCheckEmpty, viewData.ThemeSlots.Count );
        UpdateThemeSlots( _scoreCheckSlots, viewData.ThemeSlots, true );
        UpdateEmptySlot( _scoreCheckEmpty, viewData.ThemeSlots.Count );
        _finalScoreText.text = $"최종 제작 점수: {viewData.FinalScore}";
        _finalScoreText.color = _finalColor;

        ResetActiveScrollsToTop( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 제작 완료 화면의 활성 스크롤을 최상단으로 이동
    /// </summary>
    void ResetActiveScrollsToTop ()
    {
        if ( _scrollResetRoutine != null )
            StopCoroutine( _scrollResetRoutine );

        _scrollResetRoutine = StartCoroutine(
            ResetActiveScrollsToTopRoutine( ) );
    }

    /// <summary>
    /// 레이아웃 갱신 이후 제작 완료 스크롤 위치 확정
    /// </summary>
    /// <returns>레이아웃 갱신 대기</returns>
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
    /// 제작 완료 패널 숨김
    /// </summary>
    public void Hide ()
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 제작 완료 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 제작 완료 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 제작 조건 전체 달성 여부 확인
    /// </summary>
    bool AreConditionsCompleted (
        IReadOnlyList<CraftConditionViewData> conditions )
    {
        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( conditions [ i ].State != CraftConditionState.Completed )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 제작 테마 고정 슬롯 배열 갱신
    /// </summary>
    /// <param name="slots">갱신할 슬롯 배열</param>
    /// <param name="viewDatas">테마 슬롯 표시 데이터</param>
    /// <param name="showScore">점수 상태 표시 여부</param>
    void UpdateThemeSlots (
        CraftThemeSlotView [ ] slots,
        IReadOnlyList<CraftThemeSlotViewData> viewDatas,
        bool showScore )
    {
        int viewDataCount = viewDatas?.Count ?? 0;

        for ( int i = 0; i < slots.Length; i++ )
        {
            if ( i < viewDataCount )
                slots [ i ].SetData( viewDatas [ i ], showScore );
            else
                slots [ i ].Hide( );
        }
    }

    /// <summary>
    /// 제작 조건 고정 슬롯 배열 갱신
    /// </summary>
    /// <param name="slots">갱신할 슬롯 배열</param>
    /// <param name="viewDatas">조건 슬롯 표시 데이터</param>
    void UpdateConditionSlots (
        OrderInfoProgressSlotView [ ] slots,
        IReadOnlyList<OrderInfoSlotViewData> viewDatas )
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
    /// 조건 슬롯 개수에 맞춰 홀수 정렬용 빈 슬롯 표시
    /// </summary>
    /// <param name="emptySlot">정렬용 빈 슬롯 오브젝트</param>
    /// <param name="slotCount">표시할 실제 슬롯 개수</param>
    void UpdateEmptySlot ( GameObject emptySlot, int slotCount )
    {
        emptySlot.SetActive( slotCount > 0 && slotCount % 2 != 0 );
    }

    /// <summary>
    /// 상태별 색상을 적용한 줄바꿈 문구 생성
    /// </summary>
    string CreateText ( IReadOnlyList<CraftReviewTextData> texts )
    {
        var builder = new StringBuilder( );

        for ( int i = 0; i < texts.Count; i++ )
        {
            Color color = GetColor( texts [ i ].State );
            string colorCode = ColorUtility.ToHtmlStringRGBA( color );
            string displayText = texts [ i ].Text.Replace( " (", "\n(" );

            //특수 조건의 상세 괄호를 다음 줄에 표시해 자동 글자 축소 방지
            builder.Append( $"<color=#{colorCode}>{displayText}</color>" );

            if ( i < texts.Count - 1 )
                builder.AppendLine( );
        }

        return builder.ToString( );
    }

    /// <summary>
    /// 문구 상태별 색상 반환
    /// </summary>
    Color GetColor ( CraftReviewTextState state )
    {
        switch ( state )
        {
            case CraftReviewTextState.Positive: return _positiveColor;
            case CraftReviewTextState.Partial: return _partialColor;
            case CraftReviewTextState.Negative: return _negativeColor;
            default: return _normalColor;
        }
    }

    /// <summary>
    /// 배송 화면 이동 입력 중계
    /// </summary>
    void SelectDelivery ()
    {
        OnDeliverySelected?.Invoke( );
    }
}
