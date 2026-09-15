using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 업적 슬롯 뷰
/// </summary>
public class AchvSlotView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _icon;       //업적 종류 아이콘
    [SerializeField] TMP_Text _achvText;       //업적 이름과 설명
    [SerializeField] TMP_Text _rewardText;       //보상 안내
    [SerializeField] Image _progressBar;       //진행 게이지
    [SerializeField] TMP_Text _progressText;       //숫자 진행도
    [SerializeField] Button _claimButton;       //보상 획득 버튼

    string _id;       //현재 표시 중인 업적 아이디
    bool _canClaim;       //현재 보상 수령 가능 여부
    bool _isCompleted;       //현재 최종 단계 달성 여부
    bool _isMultiStage;       //현재 여러 단계 업적 여부
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출

    /// <summary>
    /// 현재 보상 수령 가능 여부
    /// </summary>
    public bool CanClaim => _canClaim;

    /// <summary>
    /// 현재 진행 중인 업적 여부
    /// </summary>
    public bool IsProgressing =>
        string.IsNullOrEmpty( _id ) == false &&
        _isCompleted == false && _canClaim == false;

    /// <summary>
    /// 현재 여러 단계 업적 여부
    /// </summary>
    public bool IsMultiStage => _isMultiStage;

    /// <summary>
    /// 업적 보상 수령 이벤트
    /// </summary>
    public event Action<string> OnClaim;


    /// <summary>
    /// 비활성 상태에서도 사용할 슬롯 연출 초기화
    /// </summary>
    void InitializeRuntime ()
    {
        if ( _slotTween != null ) return;

        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );
    }

    /// <summary>
    /// 업적 슬롯 입력 연결
    /// </summary>
    void Awake ()
    {
        InitializeRuntime( );
        _claimButton.onClick.AddListener( Claim );
    }

    /// <summary>
    /// 업적 슬롯 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _claimButton.onClick.RemoveListener( Claim );
    }

    #region ----- 화면 표시 -----

    /// <summary>
    /// 업적 종류 아이콘 표시 갱신
    /// </summary>
    /// <param name="icon">표시할 아이콘</param>
    void SetIcon ( Sprite icon )
    {
        if ( _icon == null ) return;

        _icon.SetIconSprite( icon );
        _icon.gameObject.SetActive( icon != null );
    }

    /// <summary>
    /// 업적 슬롯 표시
    /// </summary>
    /// <param name="viewData">업적 슬롯 표시 데이터</param>
    /// <param name="resolvedIcon">공용 종류를 실제 Sprite로 변환한 아이콘</param>
    public void Show (
        AchvViewData viewData, Sprite resolvedIcon )
    {
        gameObject.SetActive( true );

        //재사용 전 이전 연출과 식별자 초기화
        ResetForReuse( );

        _id = viewData.Id;
        _canClaim = viewData.CanClaim;
        _isCompleted = viewData.IsCompleted;
        _isMultiStage = viewData.IsMultiStage;

        SetIcon( resolvedIcon );

        _achvText.text =
            $"{viewData.DisplayName}\n{viewData.DescriptionText}";

        _rewardText.text = viewData.RewardText;
        _progressBar.fillAmount = viewData.ProgressRate;
        _progressText.text = viewData.ProgressText;

        //수령 가능 여부는 버튼 활성 상태로만 표시
        _claimButton.interactable = viewData.CanClaim;

        //같은 슬롯 인스턴스에서 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 재사용 전 업적 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        InitializeRuntime( );
        _slotTween.ResetInstant( );
        _id = null;
        _canClaim = false;
        _isCompleted = false;
        _isMultiStage = false;
        SetIcon( null );
    }

    /// <summary>
    /// 현재 업적 슬롯 내부의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 튜토리얼 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>활성 대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        target = null;

        if ( gameObject.activeInHierarchy == false ||
            string.IsNullOrEmpty( _id ) )
        {
            return false;
        }

        switch ( targetId )
        {
            case TutorialTargetId.AchievementProgress:
                target = _progressBar.rectTransform;
                break;

            case TutorialTargetId.AchievementStage:
                target = transform as RectTransform;
                break;

            case TutorialTargetId.AchievementReward:
                target = _rewardText.transform as RectTransform;
                break;

            case TutorialTargetId.AchievementClaimButton
                when _canClaim && _claimButton.interactable:
                target = _claimButton.transform as RectTransform;
                break;
        }

        return target != null;
    }

    /// <summary>
    /// 사용하지 않는 업적 슬롯 숨김
    /// </summary>
    public void Hide ()
    {
        ResetForReuse( );
        gameObject.SetActive( false );
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 현재 업적 보상 수령 요청
    /// </summary>
    void Claim ()
    {
        //수령한 업적 슬롯을 짧게 강조
        _slotTween.PlaySelected( );

        OnClaim?.Invoke( _id );
    }

    #endregion
}
