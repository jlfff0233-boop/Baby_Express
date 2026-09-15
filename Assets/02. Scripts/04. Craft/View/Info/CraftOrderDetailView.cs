using TMPro;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 제작 주문 상세 뷰 - 주요 요구와 희망 사항 표시
/// </summary>
public class CraftOrderDetailView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [Header( "--- 주문 상세 ---" )]
    [SerializeField] TMP_Text _requirementText;       //주요 요구 사항
    [SerializeField] TMP_Text _wishText;      //희망 사항
    [SerializeField] TMP_Text _specialText;       //특수 조건

    [Header( "----- 문구 색상 -----" )]
    [SerializeField] Color _completedColor = new Color( 0.1f, 0.6f, 0.2f );     //달성 색상
    [SerializeField] Color _partialColor = new Color( 0.9f, 0.65f, 0.1f );      //일부 달성 색상
    [SerializeField] Color _failedColor = new Color( 0.8f, 0.15f, 0.15f );      //미달성 색상

    /// <summary>
    /// 제작 주문 조건의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">조회한 주문 조건 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget ( out RectTransform target )
    {
        target = transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 제작 주문 상세 표시
    /// </summary>
    /// <param name="requirements">주요 요구 결과</param>
    /// <param name="wishes">희망 결과</param>
    /// <param name="specials">특수 조건 결과</param>
    public void ShowDetail (
        IReadOnlyList<CraftConditionViewData> requirements,
        IReadOnlyList<CraftConditionViewData> wishes,
        IReadOnlyList<CraftReviewTextData> specials )
    {
        //주요 요구 표시
        _requirementText.text = CreateConditionText( requirements );

        //희망 사항 표시
        _wishText.text = CreateConditionText( wishes );

        //특수 조건 표시
        _specialText.text = CreateSpecialText( specials );
    }

    /// <summary>
    /// 제작 주문 상세 초기화
    /// </summary>
    public void ClearDetail ()
    {
        _requirementText.text = string.Empty;
        _wishText.text = string.Empty;
        _specialText.text = string.Empty;
    }

    /// <summary>
    /// 제작 조건 달성 수량과 색상 문구 생성
    /// </summary>
    string CreateConditionText (
        IReadOnlyList<CraftConditionViewData> conditions )
    {
        //조건이 없으면 기본 문구 반환
        if ( conditions == null || conditions.Count == 0 ) return "- 없음";

        var builder = new StringBuilder( );

        for ( int i = 0; i < conditions.Count; i++ )
        {
            CraftConditionViewData condition = conditions [ i ];
            Color color = GetColor( condition.State );
            string colorCode = ColorUtility.ToHtmlStringRGBA( color );
            string minimumText = condition.IsMinimum ? " 이상" : string.Empty;

            builder.Append( $"<color=#{colorCode}>- {condition.PartName} " );
            builder.Append( $"({condition.UsedQuantity} / " );
            builder.Append( $"{condition.RequiredQuantity}{minimumText})</color>" );

            if ( i < conditions.Count - 1 ) builder.AppendLine( );
        }

        return builder.ToString( );
    }

    /// <summary>
    /// 제작 조건 달성 상태별 색상 반환
    /// </summary>
    Color GetColor ( CraftConditionState state )
    {
        switch ( state )
        {
            case CraftConditionState.Completed: return _completedColor;
            case CraftConditionState.Partial: return _partialColor;
            default: return _failedColor;
        }
    }

    /// <summary>
    /// 특수 조건 상태별 색상 문구 생성
    /// </summary>
    string CreateSpecialText ( IReadOnlyList<CraftReviewTextData> specials )
    {
        if ( specials == null || specials.Count == 0 ) return "- 없음";

        var builder = new StringBuilder( );

        for ( int i = 0; i < specials.Count; i++ )
        {
            Color color = GetColor( specials [ i ].State );
            string colorCode = ColorUtility.ToHtmlStringRGBA( color );

            builder.Append( $"<color=#{colorCode}>{specials [ i ].Text}</color>" );

            if ( i < specials.Count - 1 ) builder.AppendLine( );
        }

        return builder.ToString( );
    }

    /// <summary>
    /// 특수 조건 상태별 색상 반환
    /// </summary>
    Color GetColor ( CraftReviewTextState state )
    {
        switch ( state )
        {
            case CraftReviewTextState.Positive: return _completedColor;
            case CraftReviewTextState.Partial: return _partialColor;
            case CraftReviewTextState.Negative: return _failedColor;
            default: return Color.black;
        }
    }
}
