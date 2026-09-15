using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 진행률 결산 슬롯 뷰 - 기본 결산 정보와 진행 바 표시
/// </summary>
public class SettlementProgressSlotView : SettlementSlotView
{
    [Header( "----- 진행률 표시 -----" )]
    [SerializeField] GameObject _progressFrame;       //진행 바 전체 오브젝트
    [SerializeField] Image _progressBar;       //진행률 Fill 이미지

    /// <summary>
    /// 결산 진행률 표시 적용
    /// </summary>
    /// <param name="viewData">결산 슬롯 표시 데이터</param>
    protected override void ApplyAdditionalView (
        SettlementSlotViewData viewData )
    {
        _progressFrame.SetActive( viewData.ShowProgress );
        _progressBar.fillAmount =
            Mathf.Clamp01( viewData.Progress );
    }

    /// <summary>
    /// 결산 진행률 표시 초기화
    /// </summary>
    protected override void ResetAdditionalView ( )
    {
        _progressFrame.SetActive( false );
        _progressBar.fillAmount = 0f;
    }
}
