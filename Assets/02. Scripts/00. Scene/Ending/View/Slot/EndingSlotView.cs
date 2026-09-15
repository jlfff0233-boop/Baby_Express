using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 엔딩 슬롯 뷰 - 메달과 누적 기록 표시
/// </summary>
public class EndingSlotView : MonoBehaviour
{
    [Header( "----- 표시 -----" )]
    [SerializeField] Image _medalIcon;       //실적 메달 아이콘
    [SerializeField] TMP_Text _titleText;        //슬롯 제목
    [SerializeField] TMP_Text _infoText;     //누적 기록

    [Header( "----- 문구 색상 -----" )]
    [SerializeField] Color _defaultTextColor = Color.black;      //기본 문구 색상
    [SerializeField] Color _incomeTextColor = new Color( 0.2f, 0.65f, 0.25f );     //수입 문구 색상
    [SerializeField] Color _expenseTextColor = new Color( 0.85f, 0.2f, 0.2f );      //지출 문구 색상

    /// <summary>
    /// 엔딩 슬롯 표시
    /// </summary>
    /// <param name="viewData">엔딩 슬롯 표시 데이터</param>
    /// <param name="medalIcon">등급에 맞는 메달 아이콘</param>
    public void Init (
        EndingSlotViewData viewData, Sprite medalIcon )
    {
        _medalIcon.SetIconSprite( medalIcon );
        _titleText.text = viewData.Title;
        _infoText.text = CreateInfoText( viewData.InfoLines );
    }

    /// <summary>
    /// 줄별 색상을 적용한 누적 기록 문구 생성
    /// </summary>
    /// <param name="infoLines">엔딩 기록 목록</param>
    /// <returns>색상이 적용된 누적 기록 문구</returns>
    string CreateInfoText (
        IReadOnlyList<EndingInfoLineViewData> infoLines )
    {
        var text = new StringBuilder( );

        for ( int i = 0; i < infoLines.Count; i++ )
        {
            EndingInfoLineViewData infoLine = infoLines [ i ];
            Color color = GetTextColor( infoLine.TextTone );
            string colorCode = ColorUtility.ToHtmlStringRGBA( color );

            text.Append( $"<color=#{colorCode}>{infoLine.Text}</color>" );

            if ( i < infoLines.Count - 1 )
                text.AppendLine( );
        }

        return text.ToString( );
    }

    /// <summary>
    /// 엔딩 기록 종류에 맞는 문구 색상 반환
    /// </summary>
    /// <param name="textTone">엔딩 기록 문구 색상 종류</param>
    /// <returns>적용할 문구 색상</returns>
    Color GetTextColor ( EndingTextTone textTone )
    {
        switch ( textTone )
        {
            case EndingTextTone.Income:
                return _incomeTextColor;

            case EndingTextTone.Expense:
                return _expenseTextColor;

            default:
                return _defaultTextColor;
        }
    }
}
