using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 결산 슬롯 뷰 - 아이콘과 묘사 문구 표시
/// </summary>
public class SettlementSlotView : MonoBehaviour
{
    [Header( "----- 기본 표시 -----" )]
    [SerializeField] Image _icon;       //결산 항목 아이콘
    [SerializeField] TMP_Text _descriptionText;       //결산 항목 묘사 문구

    [Header( "----- 문구 색상 -----" )]
    [SerializeField] Color _defaultTextColor = Color.black;       //기본 문구 색상
    [SerializeField] Color _incomeTextColor = new Color( 0.2f, 0.65f, 0.25f );       //수입 문구 색상
    [SerializeField] Color _expenseTextColor = new Color( 0.85f, 0.2f, 0.2f );       //지출 문구 색상

    /// <summary>
    /// 결산 슬롯 표시 초기화
    /// </summary>
    /// <param name="viewData">결산 슬롯 표시 데이터</param>
    /// <param name="resolvedIcon">공용 종류를 실제 Sprite로 변환한 아이콘</param>
    public void Init (
        SettlementSlotViewData viewData, Sprite resolvedIcon )
    {
        ResetView( );

        //상품 아이콘이 있으면 공용 아이콘보다 우선 사용합니다.
        Sprite displayIcon =
            viewData.Icon != null
                ? viewData.Icon
                : resolvedIcon;

        SetIcon( displayIcon );

        _descriptionText.text = viewData.Description;
        _descriptionText.color = GetTextColor( viewData.TextTone );

        ApplyAdditionalView( viewData );
    }

    /// <summary>
    /// 풀에서 재사용하기 전 슬롯 표시 초기화
    /// </summary>
    public void ResetView ()
    {
        _icon.SetIconSprite( null );
        _icon.gameObject.SetActive( false );

        _descriptionText.text = string.Empty;
        _descriptionText.color = _defaultTextColor;

        ResetAdditionalView( );
    }

    /// <summary>
    /// 결산 슬롯 아이콘 표시
    /// </summary>
    /// <param name="icon">표시할 아이콘</param>
    void SetIcon ( Sprite icon )
    {
        _icon.SetIconSprite( icon );
        _icon.gameObject.SetActive( icon != null );
    }

    /// <summary>
    /// 파생 슬롯의 추가 표시 적용
    /// </summary>
    /// <param name="viewData">결산 슬롯 표시 데이터</param>
    protected virtual void ApplyAdditionalView (
        SettlementSlotViewData viewData )
    {
    }

    /// <summary>
    /// 파생 슬롯의 추가 표시 초기화
    /// </summary>
    protected virtual void ResetAdditionalView ( )
    {
    }

    /// <summary>
    /// 결산 문구 종류에 맞는 색상 반환
    /// </summary>
    /// <param name="textTone">결산 문구 색상 종류</param>
    /// <returns>적용할 문구 색상</returns>
    Color GetTextColor ( SettlementTextTone textTone )
    {
        switch ( textTone )
        {
            case SettlementTextTone.Income:
                return _incomeTextColor;

            case SettlementTextTone.Expense:
                return _expenseTextColor;

            default:
                return _defaultTextColor;
        }
    }
}
