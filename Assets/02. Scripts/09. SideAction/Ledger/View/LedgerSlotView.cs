using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 가계부 주간 기록 슬롯 뷰
/// </summary>
public class LedgerSlotView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Button _selectButton;       //주차 선택 버튼
    [SerializeField] Outline _selectedOutline;       //선택 테두리

    [Header( "----- 표시 -----" )]
    [SerializeField] TMP_Text _weekText;       //주차
    [SerializeField] TMP_Text _ratingText;       //주간 평가
    [SerializeField] TMP_Text _incomeText;       //총수입
    [SerializeField] TMP_Text _expenseText;       //총지출
    [SerializeField] TMP_Text _profitText;       //순이익

    int _week;       //표시 중인 결산 주차
    SlotTweenView _slotTween;       //주간 기록 슬롯 등장과 선택 연출
    bool _isSelected;       //현재 선택 강조 상태

    /// <summary>
    /// 표시 중인 누적 결산 주차
    /// </summary>
    public int Week => _week;

    /// <summary>
    /// 현재 주차 선택 버튼 영역
    /// </summary>
    public RectTransform SelectTarget =>
        _selectButton.transform as RectTransform;

    /// <summary>
    /// 현재 주차 슬롯의 튜토리얼 대상 사용 가능 여부
    /// </summary>
    public bool CanUseTutorialTarget =>
        gameObject.activeInHierarchy &&
        _selectButton.interactable &&
        SelectTarget != null;

    /// <summary>
    /// 주차 선택 이벤트
    /// </summary>
    public event Action<int> OnSelected;

    /// <summary>
    /// 슬롯 입력과 연출 연결
    /// </summary>
    void Awake ()
    {
        //고정 주간 기록 슬롯에 공용 연출 연결
        InitSlotTween( );

        _selectButton.onClick.AddListener( Select );
    }

    /// <summary>
    /// 슬롯 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _selectButton.onClick.RemoveListener( Select );
    }

    /// <summary>
    /// 주간 기록 표시
    /// </summary>
    /// <param name="viewData">주간 목록 표시 데이터</param>
    public void Show ( WeeklyLedgerSummaryViewData viewData )
    {
        //이전에 표시한 주차와 선택 연출 초기화
        ResetForReuse( );

        _week = viewData.Week;

        gameObject.SetActive( true );
        _weekText.text = $"{viewData.DisplayWeek}주차";
        _ratingText.text = $"주간 평가: {viewData.Rating}";
        _incomeText.text = $"총수입: {viewData.TotalIncome}";
        _expenseText.text = $"총지출: {viewData.TotalExpense}";
        _profitText.text = $"순이익: {viewData.NetProfit}";
        _selectButton.interactable = viewData.IsCompleted;

        //고정 슬롯이 처음 사용된 경우에만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 사용하지 않는 주간 슬롯 숨김
    /// </summary>
    public void Hide ()
    {
        //주차와 실행 중인 연출 초기화
        ResetForReuse( );

        _selectButton.interactable = false;
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 슬롯 선택 상태 표시
    /// </summary>
    /// <param name="selected">선택 여부</param>
    public void SetSelected ( bool selected )
    {
        //선택 테두리는 현재 상태에 맞게 항상 갱신
        _selectedOutline.enabled = selected;

        //같은 선택 상태의 일반 갱신에는 연출을 반복하지 않음
        if ( _isSelected == selected ) return;

        _isSelected = selected;

        if ( selected )
        {
            //선택한 주간 기록 슬롯을 짧게 강조
            _slotTween.PlaySelected( );
            return;
        }

        //선택 해제 시 진행 중인 연출 즉시 복구
        _slotTween.ResetInstant( );
    }

    /// <summary>
    /// 고정 슬롯 재사용 전 표시 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        //부모 뷰의 Awake가 먼저 실행된 경우 연출 컴포넌트 준비
        InitSlotTween( );

        //트윈과 주차 및 선택 상태 초기화
        _slotTween.ResetInstant( );
        _week = 0;
        _isSelected = false;
        _selectedOutline.enabled = false;
    }

    /// <summary>
    /// 공용 슬롯 연출 컴포넌트 초기화
    /// </summary>
    void InitSlotTween ()
    {
        if ( _slotTween != null ) return;

        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );
    }

    /// <summary>
    /// 현재 주차 선택
    /// </summary>
    void Select ()
    {
        OnSelected?.Invoke( _week );
    }
}
