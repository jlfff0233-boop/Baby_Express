using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상태 뷰 - 날짜, 시간, 일일 제작 진행률, 자금을 표시, 연출
/// </summary>
public class StatusView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _timeIcon;       //시간 아이콘
    [SerializeField] TMP_Text _dateText;       //날짜 텍스트
    [SerializeField] TMP_Text _timeText;       //시간 텍스트
    [SerializeField] Image _progressBar;       //진행 바 이미지
    [SerializeField] TMP_Text _progressText;       //일일 제작 진행률
    [SerializeField] Image _budgetIcon;       //자금 아이콘
    [SerializeField] TMP_Text _budgetText;       //자금 텍스트
    [SerializeField] Image _budgetEffectIcon;       //자금 변경 연출용 코인
    [SerializeField] RectTransform _incomeStartPoint;       //수입 코인 시작 위치
    [SerializeField] RectTransform _budgetPoint;       //코인 도착 위치
    [SerializeField] RectTransform _expenseEndPoint;       //지출 코인 종료 위치
    [SerializeField] GameObject _tooltipIcon;       //툴팁 페널티 알림
    [SerializeField] TooltipArea _tooltip;       //툴팁 페널티 설명

    [Header( "----- 상태 변경 연출 -----" )]
    [SerializeField, Min( 0f )] float _textDuration = 0.18f;       //텍스트 교체 연출 시간
    [SerializeField, Min( 0f )] float _progressDuration = 0.3f;       //진행 바 변경 연출 시간
    [SerializeField, Min( 0f )] float _coinDuration = 0.45f;       //코인 이동 연출 시간
    [SerializeField, Min( 0f )] float _coinJumpPower = 30f;       //수입 코인 점프 높이
    [SerializeField, Min( 0f )] float _budgetChangeDuration = 0.6f;       //자금 차액 표시 시간
    [SerializeField, Min( 0f )] float _budgetChangeDistance = 50f;       //자금 차액 이동 거리
    [SerializeField] Color _incomeColor = Color.green;       //수입 강조 색상
    [SerializeField] Color _expenseColor = Color.red;       //지출 강조 색상

    TMP_Text _budgetChangeText;       //자금 차액 표시용 복제 텍스트

    RectTransform _dateRect;       //날짜 RectTransform
    RectTransform _timeRect;       //시간 RectTransform
    RectTransform _progressTextRect;       //진행률 텍스트 RectTransform
    RectTransform _budgetTextRect;       //자금 텍스트 RectTransform
    RectTransform _budgetEffectRect;       //연출용 코인 RectTransform
    RectTransform _budgetChangeRect;       //자금 차액 RectTransform

    Vector2 _budgetChangePosition;       //자금 차액 원래 위치

    Vector3 _dateScale;       //날짜 원래 크기
    Vector3 _timeScale;       //시간 원래 크기
    Vector3 _progressTextScale;       //진행률 텍스트 원래 크기
    Vector3 _budgetTextScale;       //자금 텍스트 원래 크기
    Vector3 _budgetIconScale;       //자금 아이콘 원래 크기
    Vector3 _budgetChangeScale;       //자금 차액 원래 크기
    Color _budgetTextColor;       //자금 텍스트 원래 색상

    float _displayBudget;       //현재 화면에 표시 중인 자금
    float _targetBudget;       //연출 완료 후 표시할 자금

    Tween _dateTween;       //날짜 변경 트윈
    Tween _timeTween;       //시간 변경 트윈
    Tween _progressTween;       //진행 바 트윈
    Tween _progressTextTween;       //진행률 텍스트 트윈
    Tween _budgetTween;       //자금 변경 트윈
    Tween _budgetChangeTween;       //자금 차액 이동과 사라짐 연출


    #region ----- 초기화 -----

    /// <summary>
    /// 기존 자금 텍스트를 복제해 차액 표시 텍스트 생성
    /// </summary>
    void CreateBudgetChangeText ()
    {
        _budgetChangeText =
            Instantiate(
                _budgetText,
                _budgetText.transform.parent );

        _budgetChangeText.name = "BudgetChangeText";
        _budgetChangeText.raycastTarget = false;

        _budgetChangeRect =
            _budgetChangeText.rectTransform;

        _budgetChangeRect.SetSiblingIndex(
            _budgetText.transform.GetSiblingIndex( ) + 1 );

        //상태 바의 기존 레이아웃을 변경하지 않도록 제외
        LayoutElement layoutElement =
            _budgetChangeText.gameObject
                .GetOrAddComponent<LayoutElement>( );

        layoutElement.ignoreLayout = true;

        _budgetChangePosition =
            _budgetChangeRect.anchoredPosition;
        _budgetChangeScale =
            _budgetChangeRect.localScale;

        _budgetChangeText.gameObject.SetActive( false );
    }

    /// <summary>
    /// 상태 표시 컴포넌트와 원래 표시 상태 초기화
    /// </summary>
    void Awake ()
    {
        _dateRect = _dateText.rectTransform;
        _timeRect = _timeText.rectTransform;
        _progressTextRect = _progressText.rectTransform;
        _budgetTextRect = _budgetText.rectTransform;
        _budgetEffectRect = _budgetEffectIcon.rectTransform;

        _dateScale = _dateRect.localScale;
        _timeScale = _timeRect.localScale;
        _progressTextScale = _progressTextRect.localScale;
        _budgetTextScale = _budgetTextRect.localScale;
        _budgetIconScale = _budgetIcon.rectTransform.localScale;
        _budgetTextColor = _budgetText.color;

        _budgetEffectIcon.gameObject.SetActive( false );

        //자금 차액 표시는 기존 텍스트를 런타임에 한 번 복제해 재사용
        CreateBudgetChangeText( );
    }

    /// <summary>
    /// 비활성화 시 상태 변경 연출 정리
    /// </summary>
    void OnDisable ()
    {
        KillTween( ref _dateTween );
        KillTween( ref _timeTween );
        KillTween( ref _progressTween );
        KillTween( ref _progressTextTween );
        CompleteBudgetTween( );
        KillBudgetChangeTween( );

        ResetDateVisual( );
        ResetTimeVisual( );
        ResetProgressVisual( );
    }

    #endregion

    #region ----- 날짜와 시간 -----

    /// <summary>
    /// 연출 없이 날짜 표시 설정
    /// </summary>
    /// <param name="month">현재 월</param>
    /// <param name="day">현재 일</param>
    public void SetDate ( int month, int day )
    {
        KillTween( ref _dateTween );
        ResetDateVisual( );

        _dateText.text = $"{month}월 {day}일";
    }

    /// <summary>
    /// 날짜 변경 연출 재생
    /// </summary>
    /// <param name="month">변경된 월</param>
    /// <param name="day">변경된 일</param>
    public void PlayDateChange ( int month, int day )
    {
        KillTween( ref _dateTween );
        ResetDateVisual( );

        //기존 날짜가 사라진 뒤 변경된 날짜가 다시 나타나도록 순차 재생
        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _dateRect.DOScale( Vector3.zero, _textDuration )
                .SetEase( Ease.InBack ) );

        sequence.AppendCallback(
            () => _dateText.text = $"{month}월 {day}일" );

        sequence.Append(
            _dateRect.DOScale( _dateScale, _textDuration )
                .SetEase( Ease.OutBack ) );

        sequence.OnComplete( ResetDateVisual );

        _dateTween = sequence;
    }

    /// <summary>
    /// 연출 없이 시간 표시 설정
    /// </summary>
    /// <param name="time">현재 시간</param>
    public void SetTime ( float time )
    {
        KillTween( ref _timeTween );
        ResetTimeVisual( );

        _timeText.text = GetTimeText( time );
    }

    /// <summary>
    /// 시간 변경 연출 재생
    /// </summary>
    /// <param name="time">변경된 시간</param>
    public void PlayTimeChange ( float time )
    {
        KillTween( ref _timeTween );
        ResetTimeVisual( );

        //시간 텍스트 교체, 시간 아이콘 강조
        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _timeRect.DOScale( Vector3.zero, _textDuration )
                .SetEase( Ease.InBack ) );

        sequence.AppendCallback(
            () => _timeText.text = GetTimeText( time ) );

        sequence.Append(
            _timeRect.DOScale( _timeScale, _textDuration )
                .SetEase( Ease.OutBack ) );

        sequence.Join(
            _timeIcon.rectTransform.DOPunchScale(
                Vector3.one * 0.1f, _textDuration, 4, 0.5f ) );

        sequence.OnComplete( ResetTimeVisual );

        _timeTween = sequence;
    }

    /// <summary>
    /// 시간 표시 문구 생성
    /// </summary>
    /// <param name="time">현재 시간</param>
    /// <returns>시와 분으로 변환한 문구</returns>
    string GetTimeText ( float time )
    {
        //부동소수점 오차가 분 표시에 반영되지 않도록 전체 분 반올림
        int totalMinutes =
            Mathf.RoundToInt( time * 60f );

        int hour = totalMinutes / 60 % 24;
        int minute = totalMinutes % 60;

        return $"{hour:00}:{minute:00}";
    }

    #endregion

    #region ----- 제작 진행률 -----

    /// <summary>
    /// 연출 없이 일일 제작 진행률 설정
    /// </summary>
    /// <param name="craftCount">오늘 제작 완료 수</param>
    /// <param name="craftLimit">일일 제작 할당량</param>
    /// <param name="progressRate">현재 제작 진행률</param>
    public void SetCraftProgress (
        int craftCount, int craftLimit, float progressRate )
    {
        KillTween( ref _progressTween );
        KillTween( ref _progressTextTween );
        ResetProgressVisual( );

        _progressText.text = $"{craftCount} / {craftLimit}";
        _progressBar.fillAmount = progressRate;
    }

    /// <summary>
    /// 일일 제작 진행률 변경 연출 재생
    /// </summary>
    /// <param name="craftCount">오늘 제작 완료 수</param>
    /// <param name="craftLimit">일일 제작 할당량</param>
    /// <param name="progressRate">현재 제작 진행률</param>
    public void PlayCraftProgress (
        int craftCount, int craftLimit, float progressRate )
    {
        KillTween( ref _progressTween );
        KillTween( ref _progressTextTween );
        ResetProgressVisual( );

        _progressText.text = $"{craftCount} / {craftLimit}";
        _progressTextRect.localScale =
            _progressTextScale * 0.8f;

        //진행 바는 현재 값에서 변경된 진행률까지 부드럽게 채움
        _progressTween = _progressBar
            .DOFillAmount( progressRate, _progressDuration )
            .SetEase( Ease.OutQuad )
            .SetUpdate( true );

        //진행률 문구를 강조하고 할당량 달성 시 한 번 더 튀어 오르게 표시
        Sequence textSequence = DOTween.Sequence( )
            .SetUpdate( true );

        textSequence.Append(
            _progressTextRect
                .DOScale( _progressTextScale, _textDuration )
                .SetEase( Ease.OutBack ) );

        if ( craftLimit > 0 && craftCount >= craftLimit )
        {
            textSequence.Append(
                _progressTextRect.DOPunchScale(
                    Vector3.one * 0.15f, _textDuration, 4, 0.5f ) );
        }

        textSequence.OnComplete( ResetProgressVisual );

        _progressTextTween = textSequence;
    }

    #endregion

    #region ----- 자금 -----

    /// <summary>
    /// 연출 없이 자금 표시 설정
    /// </summary>
    /// <param name="budget">현재 자금</param>
    public void SetBudget ( float budget )
    {
        CompleteBudgetTween( );
        KillBudgetChangeTween( );

        _displayBudget = budget;
        _targetBudget = budget;

        SetBudgetText( budget );
    }

    /// <summary>
    /// 자금 차액 연출 완료 처리
    /// </summary>
    void CompleteBudgetChangeText ()
    {
        _budgetChangeTween = null;

        ResetBudgetChangeVisual( );
        _budgetChangeText.gameObject.SetActive( false );
    }

    /// <summary>
    /// 자금 차액을 이동시키며 사라지게 표시
    /// </summary>
    /// <param name="changeAmount">변경된 자금 차액</param>
    void PlayBudgetChangeText ( float changeAmount )
    {
        KillBudgetChangeTween( );

        bool isIncome = changeAmount > 0f;
        string sign = isIncome ? "+" : "-";

        _budgetChangeText.text =
            $"{sign}{Math.Truncate( Mathf.Abs( changeAmount ) ):N0}";

        _budgetChangeText.color =
            isIncome ? _incomeColor : _expenseColor;

        _budgetChangeRect.anchoredPosition =
            _budgetChangePosition;
        _budgetChangeRect.localScale =
            _budgetChangeScale;

        _budgetChangeText.gameObject.SetActive( true );

        Vector2 endPosition =
            _budgetChangePosition +
            Vector2.up * (
                isIncome
                    ? _budgetChangeDistance
                    : -_budgetChangeDistance );

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        sequence.Append(
            _budgetChangeRect
                .DOAnchorPos(
                    endPosition,
                    _budgetChangeDuration )
                .SetEase( Ease.OutQuad ) );

        sequence.Join(
            _budgetChangeText
                .DOFade( 0f, _budgetChangeDuration )
                .SetEase( Ease.InQuad ) );

        sequence.OnComplete(
            CompleteBudgetChangeText );

        _budgetChangeTween = sequence;
    }

    /// <summary>
    /// 자금 변경 연출 재생
    /// </summary>
    /// <param name="budget">변경된 최종 자금</param>
    /// <param name="changeAmount">이전 자금과의 차액</param>
    public void PlayBudgetChange (
        float budget, float changeAmount )
    {
        KillBudgetTween( );

        _targetBudget = budget;

        bool isIncome = changeAmount > 0f;

        //변경된 실제 금액을 별도 텍스트로 표시
        PlayBudgetChangeText( changeAmount );

        Vector2 startPosition = GetBudgetEffectPosition(
            isIncome == true ? _incomeStartPoint : _budgetPoint );
        Vector2 endPosition = GetBudgetEffectPosition(
            isIncome == true ? _budgetPoint : _expenseEndPoint );

        //수입과 지출에 공용으로 사용하는 코인 연출 상태 준비
        _budgetEffectIcon.gameObject.SetActive( true );
        _budgetEffectRect.localScale = Vector3.one * 0.7f;
        _budgetEffectIcon.color = Color.white;
        _budgetEffectRect.anchoredPosition = startPosition;

        Color changeColor = isIncome
            ? _incomeColor
            : _expenseColor;

        Sequence sequence = DOTween.Sequence( )
            .SetUpdate( true );

        //수입은 코인이 자금 위치로 들어오고 지출은 자금 위치에서 빠져나감
        if ( isIncome )
        {
            sequence.Append(
                _budgetEffectRect.DOJumpAnchorPos(
                    endPosition, _coinJumpPower, 1, _coinDuration ) );
        }
        else
        {
            sequence.Append(
                _budgetEffectRect.DOAnchorPos(
                    endPosition, _coinDuration )
                    .SetEase( Ease.InQuad ) );
        }

        sequence.Join(
            _budgetEffectRect
                .DOScale( Vector3.one, _coinDuration )
                .SetEase( Ease.OutBack ) );

        //코인 이동 중 화면 자금을 최종 자금까지 연속 갱신
        sequence.Join(
            DOTween.To(
                () => _displayBudget,
                value =>
                {
                    _displayBudget = value;
                    SetBudgetText( value );
                },
                budget, _coinDuration )
            .SetEase( Ease.OutQuad ) );

        sequence.Join(
            _budgetText.DOColor(
                changeColor, _coinDuration * 0.5f ) );

        //코인 이동 완료 후 자금 아이콘과 문구를 강조하고 원래 색상으로 복구
        sequence.Append(
            _budgetIcon.rectTransform.DOPunchScale(
                Vector3.one * 0.15f, _textDuration, 4, 0.5f ) );

        sequence.Join(
            _budgetTextRect.DOPunchScale(
                Vector3.one * 0.12f, _textDuration, 4, 0.5f ) );

        sequence.Join(
            _budgetText.DOColor(
                _budgetTextColor, _textDuration ) );

        sequence.OnComplete( CompleteBudgetTween );

        _budgetTween = sequence;
    }

    /// <summary>
    /// 자금 연출 지점의 위치를 연출용 코인의 앵커 좌표로 변환
    /// </summary>
    /// <param name="point">변환할 자금 연출 지점</param>
    /// <returns>연출용 코인 부모 기준 앵커 좌표</returns>
    Vector2 GetBudgetEffectPosition ( RectTransform point )
    {
        Vector3 currentPosition = _budgetEffectRect.position;

        _budgetEffectRect.position = point.position;
        Vector2 anchoredPosition = _budgetEffectRect.anchoredPosition;
        _budgetEffectRect.position = currentPosition;

        return anchoredPosition;
    }

    /// <summary>
    /// 자금 표시 문구 갱신
    /// </summary>
    /// <param name="budget">표시할 자금</param>
    void SetBudgetText ( float budget )
    {
        _budgetText.text =
            Math.Truncate( budget ).ToString( "N0" );
    }

    #endregion

    #region ----- 연출 초기화 -----

    /// <summary>
    /// 자금 차액 표시 상태 초기화
    /// </summary>
    void ResetBudgetChangeVisual ()
    {
        _budgetChangeRect.anchoredPosition =
            _budgetChangePosition;
        _budgetChangeRect.localScale =
            _budgetChangeScale;

        Color color = _budgetChangeText.color;
        color.a = 1f;
        _budgetChangeText.color = color;
    }

    /// <summary>
    /// 실행 중인 자금 차액 연출 제거
    /// </summary>
    void KillBudgetChangeTween ()
    {
        KillTween( ref _budgetChangeTween );

        ResetBudgetChangeVisual( );
        _budgetChangeText.gameObject.SetActive( false );
    }

    /// <summary>
    /// 날짜 표시 상태 초기화
    /// </summary>
    void ResetDateVisual ()
    {
        _dateRect.localScale = _dateScale;
    }

    /// <summary>
    /// 시간 표시 상태 초기화
    /// </summary>
    void ResetTimeVisual ()
    {
        _timeRect.localScale = _timeScale;
        _timeIcon.rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 제작 진행률 표시 상태 초기화
    /// </summary>
    void ResetProgressVisual ()
    {
        _progressTextRect.localScale = _progressTextScale;
    }

    /// <summary>
    /// 실행 중인 자금 연출 제거
    /// </summary>
    void KillBudgetTween ()
    {
        KillTween( ref _budgetTween );

        //중단된 연출의 시각 요소만 원래 상태로 복구
        _budgetText.color = _budgetTextColor;
        _budgetTextRect.localScale = _budgetTextScale;
        _budgetIcon.rectTransform.localScale = _budgetIconScale;
        _budgetEffectIcon.gameObject.SetActive( false );
    }

    /// <summary>
    /// 자금 연출을 최종 상태로 완료
    /// </summary>
    void CompleteBudgetTween ()
    {
        KillTween( ref _budgetTween );

        //연출 중 비활성화되어도 최종 자금이 정확히 표시되도록 완료 처리
        _displayBudget = _targetBudget;
        SetBudgetText( _targetBudget );

        _budgetText.color = _budgetTextColor;
        _budgetTextRect.localScale = _budgetTextScale;
        _budgetIcon.rectTransform.localScale = _budgetIconScale;
        _budgetEffectIcon.gameObject.SetActive( false );
    }

    /// <summary>
    /// 실행 중인 Tween 제거
    /// </summary>
    /// <param name="tween">제거할 Tween</param>
    void KillTween ( ref Tween tween )
    {
        if ( tween == null ) return;

        tween.Kill( );
        tween = null;
    }

    #endregion

    /// <summary>
    /// 주문량 페널티 알림 갱신
    /// </summary>
    /// <param name="isActive">페널티 적용 여부</param>
    /// <param name="description">페널티 설명</param>
    public void UpdateOrderPenalty ( bool isActive, string description )
    {
        //적용 중이면 툴팁 설명 갱신
        if ( isActive )
            _tooltip.SetDescription( description );
        else
            _tooltip.Hide( );

        //알림 아이콘 표시 상태 갱신
        _tooltipIcon.SetActive( isActive );
    }
}
