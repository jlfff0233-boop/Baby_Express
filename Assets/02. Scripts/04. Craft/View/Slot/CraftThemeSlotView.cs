using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작 테마 슬롯 표시 데이터
/// </summary>
public class CraftThemeSlotViewData
{
    /// <summary>
    /// 테마 아이콘
    /// </summary>
    public Sprite Icon { get; }

    /// <summary>
    /// 테마 이름
    /// </summary>
    public string ThemeName { get; }

    /// <summary>
    /// 테마 사용 타입 수
    /// </summary>
    public int UsedTypeCount { get; }

    /// <summary>
    /// 전체 사용 타입 수
    /// </summary>
    public int TotalTypeCount { get; }

    /// <summary>
    /// 완성 테마 여부
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// 상극 표시 여부
    /// </summary>
    public bool IsConflict { get; }

    /// <summary>
    /// 상극 관계 문구
    /// </summary>
    public string ConflictText { get; }

    /// <summary>
    /// 표시 점수
    /// </summary>
    public float Score { get; }

    /// <summary>
    /// 제작 테마 슬롯 표시 데이터 생성
    /// </summary>
    public CraftThemeSlotViewData (
        Sprite icon, string themeName,
        int usedTypeCount, int totalTypeCount,
        bool isCompleted, bool isConflict,
        string conflictText, float score )
    {
        Icon = icon;
        ThemeName = themeName;
        UsedTypeCount = usedTypeCount;
        TotalTypeCount = totalTypeCount;
        IsCompleted = isCompleted;
        IsConflict = isConflict;
        ConflictText = conflictText;
        Score = score;
    }
}

/// <summary>
/// 제작 테마 슬롯 뷰 - 테마 진행도와 완성 및 점수 상태 표시
/// </summary>
public class CraftThemeSlotView : MonoBehaviour
{
    [Header( "----- 테마 정보 -----" )]
    [SerializeField] Image _themeIcon;       //테마 아이콘
    [SerializeField] TMP_Text _themeText;       //테마 이름
    [SerializeField] TMP_Text _countText;       //테마 진행 개수
    [SerializeField] Image _progressBar;       //테마 진행 바

    [Header( "----- 완성 상태 -----" )]
    [SerializeField] Image _activeCheckIcon;       //완성 또는 상극 아이콘
    [SerializeField] TMP_Text _activeCheckText;       //완성 또는 상극 문구
    [SerializeField] Sprite _conflictIcon;       //상극 아이콘

    [Header( "----- 점수 상태 -----" )]
    [SerializeField] Image _scoreCheckIcon;       //점수 증감 아이콘
    [SerializeField] TMP_Text _scoreCheckText;       //점수 증감 문구
    [SerializeField] Sprite _negativeScoreIcon;       //감점 아이콘

    Sprite _completeIcon;       //프리팹 기본 완성 아이콘
    Sprite _positiveScoreIcon;       //프리팹 기본 추가 점수 아이콘

    /// <summary>
    /// 프리팹에 지정된 기본 아이콘 저장
    /// </summary>
    void Awake ()
    {
        _completeIcon = _activeCheckIcon.sprite;
        _positiveScoreIcon = _scoreCheckIcon.sprite;
    }

    /// <summary>
    /// 테마 슬롯 갱신
    /// </summary>
    /// <param name="viewData">테마 표시 데이터</param>
    /// <param name="showScore">점수 영역 표시 여부</param>
    public void SetData (
        CraftThemeSlotViewData viewData, bool showScore )
    {
        gameObject.SetActive( true );

        if ( viewData.Icon != null )
            _themeIcon.SetIconSprite( viewData.Icon );
        _themeText.text = viewData.ThemeName;
        _countText.text =
            $"{viewData.UsedTypeCount} / {viewData.TotalTypeCount}";

        _progressBar.fillAmount = viewData.TotalTypeCount > 0
            ? Mathf.Clamp01(
                ( float ) viewData.UsedTypeCount /
                viewData.TotalTypeCount )
            : 0f;

        UpdateState( viewData, showScore );
    }

    /// <summary>
    /// 완성 및 상극과 점수 상태 갱신
    /// </summary>
    void UpdateState (
        CraftThemeSlotViewData viewData, bool showScore )
    {
        //테마 체크 영역에서는 완성 여부만 표시
        if ( showScore == false )
        {
            SetCompletionState( viewData.IsCompleted );
            SetScoreVisible( false );
            return;
        }

        //점수 체크 영역에서 상극이면 양쪽 표시를 상극 상태로 교체
        if ( viewData.IsConflict )
        {
            _activeCheckIcon.gameObject.SetActive( true );
            _activeCheckText.gameObject.SetActive( true );
            _activeCheckIcon.SetIconSprite( _conflictIcon );
            _activeCheckText.text = viewData.ConflictText;

            _scoreCheckIcon.SetIconSprite( _negativeScoreIcon );
            _scoreCheckText.text = $"{viewData.Score:0}";
            SetScoreVisible( true );
            return;
        }

        SetCompletionState( viewData.IsCompleted );

        _scoreCheckIcon.SetIconSprite( _positiveScoreIcon );
        _scoreCheckText.text = viewData.Score >= 0f
            ? $"+{viewData.Score:0}"
            : $"{viewData.Score:0}";

        SetScoreVisible( true );
    }

    /// <summary>
    /// 완성 상태 표시
    /// </summary>
    void SetCompletionState ( bool isCompleted )
    {
        _activeCheckIcon.gameObject.SetActive( isCompleted );
        _activeCheckText.gameObject.SetActive( isCompleted );

        _activeCheckIcon.SetIconSprite( _completeIcon );
        _activeCheckText.text = "완성";
    }

    /// <summary>
    /// 점수 상태 표시
    /// </summary>
    void SetScoreVisible ( bool isVisible )
    {
        _scoreCheckIcon.gameObject.SetActive( isVisible );
        _scoreCheckText.gameObject.SetActive( isVisible );
    }

    /// <summary>
    /// 사용하지 않는 테마 슬롯 숨김
    /// </summary>
    public void Hide ()
    {
        SetCompletionState( false );
        SetScoreVisible( false );

        _progressBar.fillAmount = 0f;
        gameObject.SetActive( false );
    }
}
