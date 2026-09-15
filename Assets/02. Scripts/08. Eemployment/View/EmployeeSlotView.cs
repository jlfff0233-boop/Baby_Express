using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 직원 슬롯 뷰 - 직원 카드 표시와 고용, 해고 입력 전달
/// </summary>
public class EmployeeSlotView : MonoBehaviour
{
    [Header( "----- 직원 정보 -----" )]
    [SerializeField] Image _portraitImage;       //직원 초상화
    [SerializeField] TMP_Text _nameText;       //직원 이름
    [SerializeField] TMP_Text _jobText;       //직원 업무
    [SerializeField] TMP_Text _costText;       //고용비와 주급
    [SerializeField] TMP_Text _guideText;       //상태 안내

    [Header( "----- 입력 -----" )]
    [SerializeField] Button _actionButton;       //고용 또는 해고 버튼
    [SerializeField] TMP_Text _actionText;       //입력 버튼 문구

    string _employeeId;       //직원 아이디
    SlotTweenView _slotTween;       //직원 카드 등장과 재사용 연출

    /// <summary>
    /// 직원 처리 입력 이벤트(직원 아이디)
    /// </summary>
    public event Action<string> OnAction;

    /// <summary>
    /// 직원 입력과 카드 연출 연결
    /// </summary>
    void Awake ()
    {
        //직원 카드에 공용 슬롯 연출 연결
        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );

        //고용과 해고 버튼에 공용 클릭 연출 연결
        _actionButton.BindClickHighlight( );

        _actionButton.onClick.AddListener( SelectAction );
    }

    /// <summary>
    /// 직원 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _actionButton.onClick.RemoveListener( SelectAction );
    }

    /// <summary>
    /// 직원 카드 초기화
    /// </summary>
    /// <param name="viewData">직원 카드 표시 데이터</param>
    public void Init ( EmployeeSlotViewData viewData )
    {
        //이전 직원 식별자와 실행 중인 연출 초기화
        ResetForReuse( );

        _employeeId = viewData.EmployeeId;

        //고용 화면용 초상화와 원본 Sprite 비율 적용
        _portraitImage.SetIconSprite( viewData.Portrait );
        _nameText.text = viewData.Name;
        _jobText.text = viewData.Job;
        _costText.text = viewData.Cost;
        _guideText.text = viewData.Guide;
        _actionText.text = viewData.Action;
        _actionButton.interactable = viewData.IsInteractable;

        //같은 풀 인스턴스에서 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 현재 직원 카드 내부의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 직원 카드 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        target = targetId switch
        {
            TutorialTargetId.GuideEmployeeCard =>
                transform as RectTransform,
            TutorialTargetId.EmployeeCost =>
                _costText.transform as RectTransform,
            TutorialTargetId.EmployeeEffect =>
                _jobText.transform as RectTransform,
            _ => null
        };

        //등장 연출의 임시 투명도가 강조 연출의 복구 기준으로 저장되지 않게 완료
        if ( target != null )
            _slotTween.ResetInstant( );

        return gameObject.activeInHierarchy &&
            string.IsNullOrEmpty( _employeeId ) == false &&
            target != null;
    }

    /// <summary>
    /// 풀 반환 전 직원 카드 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        //트윈과 직원 식별자 초기화
        _slotTween.ResetInstant( );
        _employeeId = null;
    }

    /// <summary>
    /// 직원 고용 또는 해고 입력 전달
    /// </summary>
    void SelectAction ()
    {
        OnAction?.Invoke( _employeeId );
    }
}
