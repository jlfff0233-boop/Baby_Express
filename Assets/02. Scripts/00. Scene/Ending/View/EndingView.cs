using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 엔딩 뷰 - 총 결산 표시와 엔딩 이후 입력 전달
/// </summary>
public class EndingView : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //엔딩 패널 연출
    [SerializeField] WarningView _warningView;       //처음부터 다시 시작 경고 뷰

    [Header ( "----- 총 결산 슬롯 -----" )]
    [SerializeField] EndingSlotView _operationSlot;       //운영 결과 슬롯
    [SerializeField] EndingSlotView _economySlot;       //경제 결과 슬롯
    [SerializeField] EndingSlotView _orderSlot;       //주문 평가 슬롯
    [SerializeField] EndingSlotView _craftSlot;       //제작과 배송 슬롯
    [SerializeField] EndingSlotView _achievementSlot;       //업적 슬롯

    [Header ( "----- 메달 아이콘 -----" )]
    [SerializeField] Sprite _goldMedalIcon;       //금메달 아이콘
    [SerializeField] Sprite _silverMedalIcon;       //은메달 아이콘
    [SerializeField] Sprite _bronzeMedalIcon;       //동메달 아이콘

    [Header ( "----- 버튼 -----" )]
    [SerializeField] Button _restartButton;       //처음부터 다시 시작 버튼
    [SerializeField] Button _titleButton;       //시작 화면 버튼
    [SerializeField] Button _continueButton;       //이어서 하기 버튼


    #region ----- 초기화/이벤트 -----

    /// <summary>
    /// 처음부터 다시 시작 이벤트
    /// </summary>
    public event Action OnRestartSelected;

    /// <summary>
    /// 시작 화면 이동 이벤트
    /// </summary>
    public event Action OnTitleSelected;

    /// <summary>
    /// 엔딩 이후 계속하기 이벤트
    /// </summary>
    public event Action OnContinueSelected;

    /// <summary>
    /// 엔딩 화면 입력 연결
    /// </summary>
    void Awake ( )
    {
        _continueButton.onClick.AddListener ( SelectContinue );
        _restartButton.onClick.AddListener ( ShowRestartWarning );
        _titleButton.onClick.AddListener ( SelectTitle );

        _warningView.OnConfirmed += ConfirmRestart;
        _warningView.OnCanceled += CancelRestart;
    }

    /// <summary>
    /// 엔딩 화면 입력 해제
    /// </summary>
    void OnDestroy ( )
    {
        _continueButton.onClick.RemoveListener ( SelectContinue );
        _restartButton.onClick.RemoveListener ( ShowRestartWarning );
        _titleButton.onClick.RemoveListener ( SelectTitle );

        _warningView.OnConfirmed -= ConfirmRestart;
        _warningView.OnCanceled -= CancelRestart;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 엔딩 총 결산 화면 표시
    /// </summary>
    /// <param name="viewData">엔딩 총 결산 표시 데이터</param>
    public void Show ( EndingViewData viewData )
    {
        gameObject.SetActive ( true );
        _warningView.HideInstant ( );

        _operationSlot.Init(
            viewData.OperationSlot,
            GetMedalIcon( viewData.OperationSlot.MedalGrade ) );
        _economySlot.Init(
            viewData.EconomySlot,
            GetMedalIcon( viewData.EconomySlot.MedalGrade ) );
        _orderSlot.Init(
            viewData.OrderSlot,
            GetMedalIcon( viewData.OrderSlot.MedalGrade ) );
        _craftSlot.Init(
            viewData.CraftSlot,
            GetMedalIcon( viewData.CraftSlot.MedalGrade ) );
        _achievementSlot.Init(
            viewData.AchievementSlot,
            GetMedalIcon( viewData.AchievementSlot.MedalGrade ) );

        _panelTween.Show ( );
    }

    /// <summary>
    /// 실적 등급에 맞는 메달 아이콘 반환
    /// </summary>
    /// <param name="medalGrade">실적 메달 등급</param>
    /// <returns>메달 아이콘</returns>
    Sprite GetMedalIcon ( EndingMedalGrade medalGrade )
    {
        switch ( medalGrade )
        {
            case EndingMedalGrade.Silver:
                return _silverMedalIcon;

            case EndingMedalGrade.Bronze:
                return _bronzeMedalIcon;

            default:
                return _goldMedalIcon;
        }
    }

    /// <summary>
    /// 엔딩 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void Hide ( Action onComplete = null )
    {
        _warningView.HideInstant ( );
        _panelTween.Hide ( ( ) => CompleteHide ( onComplete ) );
    }

    /// <summary>
    /// 엔딩 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _warningView.HideInstant ( );
        _panelTween.SetVisible ( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 엔딩 패널 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive ( false );
        onComplete?.Invoke ( );
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 엔딩 이후 계속하기 입력 전달
    /// </summary>
    void SelectContinue ( )
    {
        OnContinueSelected?.Invoke ( );
    }

    /// <summary>
    /// 처음부터 다시 시작 경고 표시
    /// </summary>
    void ShowRestartWarning ( )
    {
        _warningView.Show (
            "처음부터 다시 시작" ,
            "현재 저장 슬롯의 데이터가 삭제됩니다.\n" +
            "처음부터 다시 시작하시겠습니까?" ,
            "처음부터" ,
            "돌아가기" );
    }

    /// <summary>
    /// 처음부터 다시 시작 확인 입력 전달
    /// </summary>
    void ConfirmRestart ( )
    {
        OnRestartSelected?.Invoke ( );
    }

    /// <summary>
    /// 처음부터 다시 시작 취소
    /// </summary>
    void CancelRestart ( )
    {
        _warningView.Hide ( );
    }

    /// <summary>
    /// 시작 화면 이동 입력 전달
    /// </summary>
    void SelectTitle ( )
    {
        OnTitleSelected?.Invoke ( );
    }

    #endregion
}
