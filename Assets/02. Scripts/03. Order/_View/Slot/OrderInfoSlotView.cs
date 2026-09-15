using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주문 정보 슬롯 표시 데이터
/// </summary>
public class OrderInfoSlotViewData
{
    /// <summary>
    /// 표시 아이콘
    /// </summary>
    public Sprite Icon { get; }

    /// <summary>
    /// 정보 제목
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 정보 값
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 현재 개수
    /// </summary>
    public int CurrentCount { get; }

    /// <summary>
    /// 목표 개수
    /// </summary>
    public int TargetCount { get; }

    /// <summary>
    /// 주문 정보 슬롯 표시 데이터 생성
    /// </summary>
    public OrderInfoSlotViewData (
        Sprite icon, string title, string value,
        int currentCount = 0, int targetCount = 0 )
    {
        Icon = icon;
        Title = title;
        Value = value;
        CurrentCount = currentCount;
        TargetCount = targetCount;
    }
}

/// <summary>
/// 주문 정보 슬롯 뷰 - 아이콘과 정보 문구 표시
/// </summary>
public class OrderInfoSlotView : MonoBehaviour
{
    [SerializeField] protected Image _icon;       //정보 아이콘
    [SerializeField] protected TMP_Text _titleText;       //정보 제목
    [SerializeField] protected TMP_Text _valueText;       //정보 값

    /// <summary>
    /// 주문 정보 슬롯 갱신
    /// </summary>
    public virtual void SetData ( OrderInfoSlotViewData viewData )
    {
        if ( viewData.Icon != null )
            _icon.SetIconSprite( viewData.Icon );
        _titleText.text = viewData.Title;
        _valueText.text = viewData.Value;

        gameObject.SetActive( true );
    }

    /// <summary>
    /// 주문 정보 슬롯 표시 상태 설정
    /// </summary>
    public void SetVisible ( bool isVisible )
    {
        gameObject.SetActive( isVisible );
    }
}
