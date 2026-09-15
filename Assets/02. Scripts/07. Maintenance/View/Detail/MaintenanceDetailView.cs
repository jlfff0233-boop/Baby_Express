using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 정비 상세 뷰 - 정비 상세 정보 표시와 입력 전달
/// </summary>
public class MaintenanceDetailView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //정비 상세 패널 연출
    [SerializeField] Image _icon;       //정비 아이콘
    [SerializeField] TMP_Text _infoText;       //이름, 카테고리, 설명
    [SerializeField] TMP_Text _currentEffectText;       //현재 효과
    [SerializeField] TMP_Text _nextEffectText;       //다음 효과
    [SerializeField] TMP_Text _costText;       //정비 비용
    [SerializeField] TMP_Text _requirementText;       //선행 조건
    [SerializeField] TMP_Text _applyText;       //적용 시점
    [SerializeField] Button _upgradeButton;       //정비 버튼
    [SerializeField] Button _cancelButton;       //돌아가기 버튼
    [SerializeField] Button _closeButton;       //상세 닫기 버튼

    string _maintenanceId;       //현재 정비 아이디
    Color _upgradeButtonColor;       //정비 버튼 기본 색상

    /// <summary>
    /// 정비 구매 입력 이벤트
    /// </summary>
    public event Action<string> OnUpgrade;

    /// <summary>
    /// 상세 패널 닫기 이벤트
    /// </summary>
    public event Action OnClose;

    /// <summary>
    /// 정비 상세 화면의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 상세 정보 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.MaintenanceCurrentLevel:
                target = _currentEffectText.transform as RectTransform;
                return target != null;

            case TutorialTargetId.MaintenanceNextLevel:
                target = _nextEffectText.transform as RectTransform;
                return target != null;

            default:
                target = null;
                return false;
        }
    }

    /// <summary>
    /// 정비 상세 입력 연결
    /// </summary>
    void Awake ()
    {
        _upgradeButtonColor = _upgradeButton.targetGraphic.color;

        //정비와 닫기 버튼에 공용 클릭 연출 연결
        _upgradeButton.BindClickHighlight( );
        _cancelButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        _upgradeButton.onClick.AddListener( Upgrade );
        _cancelButton.onClick.AddListener( Close );
        _closeButton.onClick.AddListener( Close );
    }

    /// <summary>
    /// 정비 상세 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _upgradeButton.onClick.RemoveListener( Upgrade );
        _cancelButton.onClick.RemoveListener( Close );
        _closeButton.onClick.RemoveListener( Close );
    }

    /// <summary>
    /// 정비 상세 패널 표시
    /// </summary>
    /// <param name="viewData">정비 상세 표시 데이터</param>
    public void ShowPanel ( MaintenanceDetailViewData viewData )
    {
        gameObject.SetActive ( true );
        UpdateView ( viewData );
        _panelTween.Show ( );
    }

    /// <summary>
    /// 정비 상세 정보 갱신
    /// </summary>
    /// <param name="viewData">정비 상세 표시 데이터</param>
    public void UpdateView ( MaintenanceDetailViewData viewData )
    {
        _maintenanceId = viewData.Id;
        _icon.SetIconSprite( viewData.Icon );
        _infoText.text = viewData.Info;
        _currentEffectText.text = viewData.CurrentEffect;
        _nextEffectText.text = viewData.NextEffect;
        _costText.text = viewData.Cost;
        _requirementText.text = viewData.Requirement;
        _applyText.text = viewData.Apply;
        UpdateUpgradeButton( viewData.CanUpgrade );
    }

    /// <summary>
    /// 정비 가능 여부에 맞춰 버튼 입력과 색상 갱신
    /// </summary>
    /// <param name="canUpgrade">정비 가능 여부</param>
    void UpdateUpgradeButton ( bool canUpgrade )
    {
        _upgradeButton.interactable = canUpgrade;
        _upgradeButton.targetGraphic.color = canUpgrade
            ? _upgradeButtonColor
            : _upgradeButton.colors.disabledColor;
    }

    /// <summary>
    /// 정비 상세 패널 숨김
    /// </summary>
    public void HidePanel ()
    {
        _maintenanceId = null;
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 정비 상세 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _maintenanceId = null;
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 정비 상세 패널 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 정비 구매 입력 전달
    /// </summary>
    void Upgrade ()
    {
        OnUpgrade?.Invoke( _maintenanceId );
    }

    /// <summary>
    /// 상세 패널 닫기 입력 전달
    /// </summary>
    void Close ()
    {
        OnClose?.Invoke( );
    }
}
