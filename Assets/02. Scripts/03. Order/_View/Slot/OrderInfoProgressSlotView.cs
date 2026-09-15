using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주문 정보 진행 슬롯 뷰 - 달성 개수와 진행 바 표시
/// </summary>
public class OrderInfoProgressSlotView : OrderInfoSlotView
{
    [SerializeField] Image _progressBar;       //달성 진행 바

    /// <summary>
    /// 주문 정보와 진행 상태 갱신
    /// </summary>
    public override void SetData ( OrderInfoSlotViewData viewData )
    {
        base.SetData( viewData );

        SetProgress( viewData.CurrentCount, viewData.TargetCount );
    }

    /// <summary>
    /// 고정된 제목과 아이콘을 유지하고 진행 상태만 갱신
    /// </summary>
    public void SetProgress ( int currentCount, int targetCount )
    {
        _valueText.text = $"{currentCount} / {targetCount}";

        _progressBar.fillAmount = targetCount > 0
            ? UnityEngine.Mathf.Clamp01(
                ( float ) currentCount / targetCount )
            : 0f;

        gameObject.SetActive( true );
    }
}
