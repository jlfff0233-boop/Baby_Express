using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 제작 미리보기 뷰 - 활성 테마와 점수 표시
/// </summary>
public class CraftPreviewView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _themeText;      //활성, 완성 테마
    [SerializeField] TMP_Text _scoreText;      //점수 보정
    [SerializeField] TMP_Text _expectedScoreText;      //예상 제작 점수

    [Header( "----- 문구 색상 -----" )]
    [SerializeField] Color _normalColor = Color.black;      //일반 문구 색상
    [SerializeField] Color _positiveColor = new Color( 0.1f, 0.6f, 0.2f );     //점수 추가 색상
    [SerializeField] Color _partialColor = new Color( 0.9f, 0.65f, 0.1f );      //일부 달성 색상
    [SerializeField] Color _negativeColor = new Color( 0.8f, 0.15f, 0.15f );       //점수 감소 색상
    [SerializeField] Color _expectedScoreColor = new Color( 0.15f, 0.3f, 0.8f );       //예상 점수 색상

    /// <summary>
    /// 제작 미리보기 표시
    /// </summary>
    public void Show ( CraftReviewViewData viewData )
    {
        //테마와 점수 보정 표시
        _themeText.text = CreateText( viewData.ThemeResults, "- 활성 테마 없음" );
        _scoreText.text = CreateText( viewData.ScoreResults, "- 점수 보정 없음" );

        //예상 제작 점수 표시
        _expectedScoreText.text = $"예상 제작 점수: {viewData.FinalScore}";
        _expectedScoreText.color = _expectedScoreColor;
    }

    /// <summary>
    /// 제작 미리보기 초기화
    /// </summary>
    public void Clear ()
    {
        _themeText.text = "- 활성 테마 없음";
        _scoreText.text = "- 점수 보정 없음";
        _expectedScoreText.text = "예상 제작 점수: 0";
        _expectedScoreText.color = _expectedScoreColor;
    }

    /// <summary>
    /// 상태별 색상을 적용한 줄바꿈 문구 생성
    /// </summary>
    string CreateText ( IReadOnlyList<CraftReviewTextData> texts, string emptyText )
    {
        //표시 결과가 없으면 기본 문구 반환
        if ( texts == null || texts.Count == 0 ) return emptyText;

        var builder = new StringBuilder( );

        for ( int i = 0; i < texts.Count; i++ )
        {
            Color color = GetColor( texts [ i ].State );
            string colorCode = ColorUtility.ToHtmlStringRGBA( color );

            builder.Append( $"<color=#{colorCode}>{texts [ i ].Text}</color>" );

            if ( i < texts.Count - 1 ) builder.AppendLine( );
        }

        return builder.ToString( );
    }

    /// <summary>
    /// 문구 상태별 색상 반환
    /// </summary>
    Color GetColor ( CraftReviewTextState state )
    {
        switch ( state )
        {
            case CraftReviewTextState.Positive: return _positiveColor;
            case CraftReviewTextState.Partial: return _partialColor;
            case CraftReviewTextState.Negative: return _negativeColor;
            default: return _normalColor;
        }
    }
}
