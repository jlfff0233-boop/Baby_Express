using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 판정 표시 데이터 생성기
/// </summary>
public class CraftReviewViewBuilder
{
    ShopModel _shopModel;       //파츠 이름 조회용 상점 모델
    PurchasableDataMap _dataMap;       //테마 연구 아이콘 조회용 상품 데이터맵
    CraftScoreSettingsData _scoreSettings;       //상극 테마 표시용 제작 점수 설정

    /// <summary>
    /// 제작 판정 표시 데이터 생성기 초기화
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="dataMap">구매 가능 상품 데이터맵</param>
    /// <param name="scoreSettings">제작 점수 설정 데이터</param>
    public CraftReviewViewBuilder (
        ShopModel shopModel, PurchasableDataMap dataMap,
        CraftScoreSettingsData scoreSettings )
    {
        _shopModel = shopModel;
        _dataMap = dataMap;
        _scoreSettings = scoreSettings;
    }

    /// <summary>
    /// 제작 판정 표시 데이터 생성
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <param name="reviewData">제작 판정 결과</param>
    /// <returns>제작 판정 표시 데이터</returns>
    public CraftReviewViewData Create ( CustomerOrder order, CraftReviewData reviewData )
    {
        return new CraftReviewViewData
        {
            RequirementResults = CreateConditionViewDatas(
                reviewData.RequirementResults, false ),     //주요 요구 사항 결과
            RequirementSlots = CreateConditionSlotViewDatas(
                reviewData.RequirementResults, false ),     //주요 요구 슬롯 결과
            WishResults = CreateConditionViewDatas(
                reviewData.WishResults, true ),     //희망 사항 결과
            WishSlots = CreateConditionSlotViewDatas(
                reviewData.WishResults, true ),     //희망 사항 슬롯 결과
            SpecialResults = CreateSpecialViewDatas( order, reviewData ),       //특수 조건 결과
            SpecialSlots = CreateSpecialSlotViewDatas( order, reviewData ),        //특수 조건 슬롯 결과
            ThemeResults = CreateThemeViewDatas( reviewData.ThemeResults ),     //테마 결과
            ThemeSlots = CreateThemeSlotViewDatas( reviewData.ThemeResults ),      //테마 슬롯 결과
            ScoreResults = CreateScoreViewDatas( reviewData.ScoreResult ),      //점수 결과
            FinalScore = reviewData.ScoreResult.FinalScore      //최종 점수
        };
    }

    /// <summary>
    /// 제작 조건 표시 데이터 목록 생성
    /// </summary>
    /// <param name="results">제작 조건 판정 목록</param>
    /// <param name="isMinimum">최소 수량 조건 여부</param>
    /// <returns>제작 조건 표시 데이터 목록</returns>
    List<CraftConditionViewData> CreateConditionViewDatas (
        IReadOnlyList<CraftConditionResult> results, bool isMinimum )
    {
        var viewDatas = new List<CraftConditionViewData>( results.Count );

        for ( int i = 0; i < results.Count; i++ )
        {
            CraftConditionResult result = results [ i ];

            viewDatas.Add( new CraftConditionViewData(
                GetPartIcon( result.PartId ),
                GetPartName( result.PartId ), result.UsedQuantity,
                result.RequiredQuantity, isMinimum,
                result.ScoreChanged, GetConditionState( result ) ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 제작 조건 달성 상태 반환
    /// </summary>
    /// <param name="result">제작 조건 판정 결과</param>
    /// <returns>제작 조건 달성 상태</returns>
    CraftConditionState GetConditionState ( CraftConditionResult result )
    {
        //조건을 달성했으면 완료
        if ( result.IsCompleted ) return CraftConditionState.Completed;

        //필요 수량 일부만 배치했으면 일부 달성
        if ( result.UsedQuantity > 0 &&
            result.UsedQuantity < result.RequiredQuantity )
            return CraftConditionState.Partial;

        //0개 또는 주요 요구 초과 배치는 미달성
        return CraftConditionState.Failed;
    }

    /// <summary>
    /// 특수 주문 표시 데이터 목록 생성
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <param name="reviewData">제작 판정 결과</param>
    /// <returns>특수 주문 표시 데이터 목록</returns>
    List<CraftReviewTextData> CreateSpecialViewDatas (
        CustomerOrder order, CraftReviewData reviewData )
    {
        var viewDatas = new List<CraftReviewTextData>( );

        //VIP 주문 표시
        if ( order.SpecialType == OrderSpecialType.Vip )
        {
            viewDatas.Add( new CraftReviewTextData(
                "- VIP 주문", CraftReviewTextState.Normal ) );
        }

        //전체 파츠 개수 제한 표시
        if ( order.MaxPartCount > 0 )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 전체 파츠 {reviewData.UsedPartCount} / {order.MaxPartCount} 이하",
                reviewData.IsPartCountExceeded
                    ? CraftReviewTextState.Negative
                    : CraftReviewTextState.Positive ) );
        }

        //목표 테마 진행 상태 표시
        for ( int i = 0; i < reviewData.TargetThemeResults.Count; i++ )
        {
            CraftTargetThemeResult result = reviewData.TargetThemeResults [ i ];
            int requiredTypeCount = result.TotalTypeCount / 2 + 1;
            CraftReviewTextState state = result.IsActive
                ? CraftReviewTextState.Positive
                : result.UsedTypeCount > 0
                    ? CraftReviewTextState.Partial
                    : CraftReviewTextState.Negative;

            viewDatas.Add( new CraftReviewTextData(
                $"- {result.Theme.GetDisplayName( )} 테마 목표 " +
                $"({result.UsedTypeCount} / {requiredTypeCount}, 보너스 조건)", state ) );
        }

        //제외 테마 사용 상태 표시
        for ( int i = 0; i < reviewData.ExcludedThemeResults.Count; i++ )
        {
            CraftExcludedThemeResult result = reviewData.ExcludedThemeResults [ i ];
            string stateText = result.IsUsed
                ? $"{result.UsedPartCount}개 사용"
                : "미사용";

            viewDatas.Add( new CraftReviewTextData(
                $"- {result.Theme.GetDisplayName( )} 테마 제외 ({stateText})",
                result.IsUsed
                    ? CraftReviewTextState.Negative
                    : CraftReviewTextState.Positive ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 특수 조건 슬롯 표시 데이터 목록 생성
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <param name="reviewData">제작 판정 결과</param>
    /// <returns>특수 조건 슬롯 표시 데이터 목록</returns>
    List<OrderInfoSlotViewData> CreateSpecialSlotViewDatas (
        CustomerOrder order, CraftReviewData reviewData )
    {
        var viewDatas = new List<OrderInfoSlotViewData>( );

        if ( order.SpecialType == OrderSpecialType.Vip )
        {
            viewDatas.Add( new OrderInfoSlotViewData(
                null, "VIP 주문", "달성", 1, 1 ) );
        }

        if ( order.MaxPartCount > 0 )
        {
            viewDatas.Add( new OrderInfoSlotViewData(
                null, "전체 파츠 개수",
                $"{reviewData.UsedPartCount} / {order.MaxPartCount} 이하",
                reviewData.UsedPartCount,
                order.MaxPartCount ) );
        }

        for ( int i = 0; i < reviewData.TargetThemeResults.Count; i++ )
        {
            CraftTargetThemeResult result = reviewData.TargetThemeResults [ i ];
            int requiredTypeCount = result.TotalTypeCount / 2 + 1;

            viewDatas.Add( new OrderInfoSlotViewData(
                null,
                $"{result.Theme.GetDisplayName( )} 테마 목표",
                $"{result.UsedTypeCount} / {requiredTypeCount}",
                result.UsedTypeCount,
                requiredTypeCount ) );
        }

        for ( int i = 0; i < reviewData.ExcludedThemeResults.Count; i++ )
        {
            CraftExcludedThemeResult result = reviewData.ExcludedThemeResults [ i ];
            int completedCount = result.IsUsed ? 0 : 1;

            viewDatas.Add( new OrderInfoSlotViewData(
                null,
                $"{result.Theme.GetDisplayName( )} 테마 제외",
                result.IsUsed ? $"{result.UsedPartCount}개 사용" : "미사용",
                completedCount,
                1 ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 활성, 완성 테마 표시 데이터 목록 생성
    /// </summary>
    /// <param name="results">테마 판정 결과 목록</param>
    /// <returns>테마 표시 데이터 목록</returns>
    List<CraftReviewTextData> CreateThemeViewDatas (
        IReadOnlyList<CraftThemeResult> results )
    {
        var viewDatas = new List<CraftReviewTextData>( );

        for ( int i = 0; i < results.Count; i++ )
        {
            CraftThemeResult result = results [ i ];
            string stateText = result.IsCompleted ? "완성" : "활성";
            string targetText =
                result.IsTargetTheme ? ", 목표" : string.Empty;
            string researchText = result.ResearchBonus > 0f
                ? $", 연구 {GetScoreText( result.ResearchBonus )}"
                : string.Empty;

            viewDatas.Add( new CraftReviewTextData(
                $"- {result.Theme.GetDisplayName( )}" +
                $"({stateText}{targetText}) " +
                $"{GetScoreText( result.BaseScore )}{researchText}",
                CraftReviewTextState.Positive ) );
        }

        //활성 테마가 없으면 일반 문구 표시
        if ( viewDatas.Count == 0 )
        {
            viewDatas.Add( new CraftReviewTextData(
                "없음", CraftReviewTextState.Normal ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 제작 조건 슬롯 표시 데이터 목록 생성
    /// </summary>
    /// <param name="results">제작 조건 판정 목록</param>
    /// <param name="isMinimum">최소 수량 조건 여부</param>
    /// <returns>제작 조건 슬롯 표시 데이터 목록</returns>
    List<OrderInfoSlotViewData> CreateConditionSlotViewDatas (
        IReadOnlyList<CraftConditionResult> results, bool isMinimum )
    {
        var viewDatas = new List<OrderInfoSlotViewData>( results.Count );

        for ( int i = 0; i < results.Count; i++ )
        {
            CraftConditionResult result = results [ i ];
            string minimumText = isMinimum ? " 이상" : string.Empty;

            viewDatas.Add( new OrderInfoSlotViewData(
                GetPartIcon( result.PartId ),
                GetPartName( result.PartId ),
                $"{result.UsedQuantity} / {result.RequiredQuantity}{minimumText}",
                result.UsedQuantity,
                result.RequiredQuantity ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 테마 슬롯 표시 데이터 목록 생성
    /// </summary>
    /// <param name="results">테마 판정 결과 목록</param>
    /// <returns>테마 슬롯 표시 데이터 목록</returns>
    List<CraftThemeSlotViewData> CreateThemeSlotViewDatas (
        IReadOnlyList<CraftThemeResult> results )
    {
        var viewDatas = new List<CraftThemeSlotViewData>( results.Count );

        for ( int i = 0; i < results.Count; i++ )
        {
            CraftThemeResult result = results [ i ];
            bool isConflict = TryGetConflictPenalty(
                result.Theme, results, out float conflictPenalty );

            viewDatas.Add( new CraftThemeSlotViewData(
                GetThemeIcon( result.Theme ),
                result.Theme.GetDisplayName( ),
                result.UsedTypeCount,
                result.TotalTypeCount,
                result.IsCompleted,
                isConflict,
                isConflict ? "상극" : string.Empty,
                isConflict ? -conflictPenalty : result.AppliedScore ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 테마 연구 상품에 등록된 아이콘 조회
    /// </summary>
    /// <param name="theme">조회할 파츠 테마</param>
    /// <returns>테마 아이콘, 연구 데이터가 없으면 null</returns>
    Sprite GetThemeIcon ( PartTheme theme )
    {
        Sprite configuredIcon = _scoreSettings.GetThemeIcon( theme );

        //종족 테마처럼 별도 등록한 대표 이미지가 있으면 우선 사용
        if ( configuredIcon != null ) return configuredIcon;

        string researchId = $"Mtc_Research_Theme_{theme}";

        if ( _dataMap.TryGetData(
            researchId, out PurchasableData data ) &&
            data is MaintenanceData )
        {
            return data.Icon;
        }

        return null;
    }

    /// <summary>
    /// 현재 테마에 표시할 상극 감점 조회
    /// </summary>
    /// <param name="theme">확인할 테마</param>
    /// <param name="results">활성 테마 결과 목록</param>
    /// <param name="penalty">상극 감점</param>
    /// <returns>상극 표시 여부</returns>
    bool TryGetConflictPenalty (
        PartTheme theme, IReadOnlyList<CraftThemeResult> results,
        out float penalty )
    {
        penalty = 0f;
        IReadOnlyList<ThemeConflict> conflicts = _scoreSettings.ThemeConflicts;

        for ( int i = 0; i < conflicts.Count; i++ )
        {
            ThemeConflict conflict = conflicts [ i ];

            //한 상극 쌍이 두 슬롯에 중복 표시되지 않도록 두 번째 테마에만 표시
            if ( conflict.Second == theme &&
                ContainsTheme( results, conflict.First ) )
                penalty += _scoreSettings.ConflictScorePenalty;
        }

        return penalty > 0f;
    }

    /// <summary>
    /// 활성 테마 결과에 지정한 테마가 있는지 확인
    /// </summary>
    bool ContainsTheme (
        IReadOnlyList<CraftThemeResult> results, PartTheme theme )
    {
        for ( int i = 0; i < results.Count; i++ )
        {
            if ( results [ i ].Theme == theme ) return true;
        }

        return false;
    }

    /// <summary>
    /// 점수 보정 표시 데이터 목록 생성
    /// </summary>
    /// <param name="score">제작 점수 결과</param>
    /// <returns>점수 보정 표시 데이터 목록</returns>
    List<CraftReviewTextData> CreateScoreViewDatas ( CraftScoreResult score )
    {
        var viewDatas = new List<CraftReviewTextData>( );

        //조립 완성 점수
        if ( score.AssemblyScore > 0f )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 조립 완성 {GetScoreText( score.AssemblyScore )}",
                CraftReviewTextState.Positive ) );
        }

        //실제 적용된 목표 테마 보정만 표시
        if ( score.TargetThemeBonus > 0f )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 목표 테마 보정 {GetScoreText( score.TargetThemeBonus )}",
                CraftReviewTextState.Positive ) );
        }

        //실제 적용된 테마 연구 보너스만 표시
        if ( score.ThemeResearchBonus > 0f )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 테마 연구 보너스 " +
                GetScoreText( score.ThemeResearchBonus ),
                CraftReviewTextState.Positive ) );
        }

        //실제 발생한 복수 테마 감쇠만 표시
        if ( score.ThemeReduction > 0f )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 복수 테마 감쇠 {GetScoreText( -score.ThemeReduction )}",
                CraftReviewTextState.Negative ) );
        }

        //실제 발생한 테마 상한 감소만 표시
        if ( score.ThemeLimitReduction > 0f )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 테마 점수 상한 {GetScoreText( -score.ThemeLimitReduction )}",
                CraftReviewTextState.Negative ) );
        }

        //실제 발생한 상극 감점만 표시
        if ( score.ConflictPenalty > 0f )
        {
            viewDatas.Add( new CraftReviewTextData(
                $"- 상극 테마 {score.ConflictCount}쌍 " +
                GetScoreText( -score.ConflictPenalty ),
                CraftReviewTextState.Negative ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 파츠 아이디의 표시 이름 반환
    /// </summary>
    /// <param name="partId">파츠 아이디</param>
    /// <returns>파츠 표시 이름</returns>
    string GetPartName ( string partId )
    {
        if ( _shopModel.TryGetItem( partId, out ShopItemModel itemModel ) )
            return itemModel.Item.Data.Name;

        return partId;
    }

    /// <summary>
    /// 파츠 아이디의 표시 아이콘 반환
    /// </summary>
    /// <param name="partId">파츠 아이디</param>
    /// <returns>파츠 표시 아이콘</returns>
    Sprite GetPartIcon ( string partId )
    {
        if ( _shopModel.TryGetItem( partId, out ShopItemModel itemModel ) )
            return itemModel.Item.Data.Icon;

        return null;
    }

    /// <summary>
    /// 점수 증감 문구 반환
    /// </summary>
    /// <param name="score">점수 증감량</param>
    /// <returns>점수 증감 문구</returns>
    string GetScoreText ( float score )
    {
        return score > 0f
            ? $"+{score:0.##}"
            : $"{score:0.##}";
    }
}
