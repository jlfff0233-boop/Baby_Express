using System;
using System.Collections.Generic;

/// <summary>
/// 확정 제작 결과 저장 복구 처리 - 저장 데이터 검증과 제작 결과 복구 상태 생성
/// </summary>
public class CraftCompleteSaveRestorer
{
    #region ----- 제작 결과 복구 -----

    /// <summary>
    /// 확정 제작 결과 저장 데이터를 검증하고 복구 상태 생성
    /// </summary>
    /// <param name="saveData">제작 저장 데이터</param>
    /// <param name="orderData">주문 저장 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="restoreState">생성한 확정 제작 결과 복구 상태</param>
    /// <returns>복구 상태 생성 성공 여부</returns>
    public bool TryCreate (
        CraftSaveData saveData,
        OrderSaveData orderData,
        PurchasableDataMap dataMap,
        out CraftCompleteRestoreState restoreState )
    {
        restoreState = null;

        //저장 데이터를 검증하고 주문별 제작 결과 생성
        if ( TryCreateRestoredResults(
            saveData, orderData, dataMap,
            out Dictionary<string, CraftResult> restoredResults ) == false )
        {
            return false;
        }

        //검증을 마친 제작 결과 복구 상태 생성
        restoreState =
            new CraftCompleteRestoreState( restoredResults );

        return true;
    }

    /// <summary>
    /// 저장 데이터를 주문별 확정 제작 결과로 변환
    /// </summary>
    /// <param name="saveData">제작 저장 데이터</param>
    /// <param name="orderData">주문 저장 데이터</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="restoredResults">복구한 주문별 제작 결과</param>
    /// <returns>제작 결과 생성 성공 여부</returns>
    bool TryCreateRestoredResults (
        CraftSaveData saveData, OrderSaveData orderData, PurchasableDataMap dataMap,
        out Dictionary<string, CraftResult> restoredResults )
    {
        restoredResults = null;

        //데이터 확인
        if ( saveData?.Results == null || orderData?.Orders == null || dataMap == null )
        {
            return false;
        }

        //주문 저장 데이터맵 생성
        if ( TryCreateOrderSaveDataMap(
            orderData,
            out Dictionary<string, CustomerOrderSaveData> orders ) == false )
        {
            return false;
        }

        //복구할 주문별 제작 결과 생성
        var pendingResults =
            new Dictionary<string, CraftResult>( );

        for ( int i = 0; i < saveData.Results.Count; i++ )
        {
            //제작 결과 저장 데이터 가져오기
            CraftResultSaveData resultData = saveData.Results [ i ];

            //제작 결과를 생성하고 주문 아이디 기준으로 추가
            if ( TryCreateCraftResult(
                resultData, orders, dataMap, out CraftResult result ) == false ||
                pendingResults.TryAdd( result.OrderId, result ) == false )
            {
                return false;
            }
        }

        foreach ( CustomerOrderSaveData order in orders.Values )
        {
            //주문에 대응하는 제작 결과 존재 여부 확인
            bool hasResult =
                pendingResults.ContainsKey( order.OrderId );

            //제작 완료 이후의 주문은 확정 제작 결과 필수
            bool requiresResult =
                order.ProgressState == OrderProgressState.Crafted ||
                order.ProgressState == OrderProgressState.Shipping ||
                ( order.ProgressState == OrderProgressState.Closed &&
                ( order.Outcome == OrderOutcome.NormalDelivery ||
                order.Outcome == OrderOutcome.LateDelivery ) );

            //주문 진행 상태와 제작 결과 보유 상태 불일치 차단
            if ( requiresResult != hasResult )
                return false;
        }

        restoredResults = pendingResults;
        return true;
    }

    /// <summary>
    /// 주문 저장 데이터맵 생성
    /// </summary>
    /// <param name="saveData">주문 저장 데이터</param>
    /// <param name="orders">생성한 주문 저장 데이터맵</param>
    /// <returns>데이터맵 생성 성공 여부</returns>
    bool TryCreateOrderSaveDataMap (
        OrderSaveData saveData,
        out Dictionary<string, CustomerOrderSaveData> orders )
    {
        orders =
            new Dictionary<string, CustomerOrderSaveData>( );

        if ( saveData?.Orders == null )
            return false;

        for ( int i = 0; i < saveData.Orders.Count; i++ )
        {
            //주문 저장 데이터 가져오기
            CustomerOrderSaveData order = saveData.Orders [ i ];

            //주문 아이디를 확인하고 데이터맵에 추가
            if ( order == null ||
                string.IsNullOrWhiteSpace( order.OrderId ) == true ||
                orders.TryAdd( order.OrderId, order ) == false )
            {
                orders = null;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 제작 결과 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="saveData">제작 결과 저장 데이터</param>
    /// <param name="orders">주문 저장 데이터맵</param>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="result">생성한 제작 결과</param>
    /// <returns>제작 결과 생성 성공 여부</returns>
    bool TryCreateCraftResult (
        CraftResultSaveData saveData,
        IReadOnlyDictionary<string, CustomerOrderSaveData> orders,
        PurchasableDataMap dataMap,
        out CraftResult result )
    {
        result = null;

        //데이터 확인
        if ( saveData == null ||
            string.IsNullOrWhiteSpace( saveData.OrderId ) == true ||
            saveData.RootPlacementNumber <= 0 ||
            saveData.PlacedParts == null ||
            saveData.PlacedParts.Count == 0 ||
            saveData.ReviewData == null ||
            orders.ContainsKey( saveData.OrderId ) == false )
        {
            return false;
        }

        var placementNumbers = new HashSet<int>( );
        var placedParts =
            new List<PlacedPartData>( saveData.PlacedParts.Count );

        bool hasRootBody = false;

        for ( int i = 0; i < saveData.PlacedParts.Count; i++ )
        {
            PlacedPartSaveData partData = saveData.PlacedParts [ i ];

            //배치 번호 중복을 확인하고 파츠 데이터 조회
            if ( IsValidPlacedPartData( partData ) == false ||
                placementNumbers.Add( partData.PlacementNumber ) == false ||
                dataMap.TryGetData(
                    partData.PartId, out PurchasableData product ) == false ||
                product is not PartsData part )
            {
                return false;
            }

            //배치 파츠 데이터 복구
            var placedPart = new PlacedPartData(
                partData.PlacementNumber,
                part,
                partData.PartIndex,
                partData.LocalPosition,
                partData.Scale );

            //배치 파츠 회전 설정
            placedPart.SetRotation( partData.Rotation );
            //배치 파츠 목록에 추가
            placedParts.Add( placedPart );

            //몸통 배치 확인
            if ( partData.PlacementNumber == saveData.RootPlacementNumber )
            {
                if ( part.PartType != PartType.Body )
                    return false;

                hasRootBody = true;
            }
        }

        //몸통과 제작 판정 데이터 확인
        if ( hasRootBody == false ||
            TryCreateReviewData(
                saveData.ReviewData, out CraftReviewData reviewData ) == false ||
            reviewData.UsedPartCount != placedParts.Count )
        {
            return false;
        }

        result = new CraftResult
        {
            OrderId = saveData.OrderId,
            RootPlacementNumber = saveData.RootPlacementNumber,
            PlacedParts = placedParts,
            ReviewData = reviewData
        };

        return true;
    }

    /// <summary>
    /// 배치 파츠 데이터 검사
    /// </summary>
    /// <param name="saveData">배치 파츠 저장 데이터</param>
    /// <returns>배치 파츠 데이터 정상 여부</returns>
    bool IsValidPlacedPartData ( PlacedPartSaveData saveData )
    {
        //데이터 확인
        return saveData != null &&
            saveData.PlacementNumber > 0 &&
            saveData.PartIndex >= 0 &&
            string.IsNullOrWhiteSpace( saveData.PartId ) == false &&
            IsFinite( saveData.LocalPosition.x ) &&
            IsFinite( saveData.LocalPosition.y ) &&
            IsFinite( saveData.Rotation ) &&
            IsFinite( saveData.Scale ) &&
            saveData.Scale > 0f;
    }

    #endregion

    #region ----- 제작 판정 복구 -----

    /// <summary>
    /// 제작 판정 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="saveData">제작 판정 저장 데이터</param>
    /// <param name="reviewData">제작 판정 데이터</param>
    /// <returns>제작 판정 데이터 생성 성공 여부</returns>
    bool TryCreateReviewData (
        CraftReviewSaveData saveData, out CraftReviewData reviewData )
    {
        reviewData = null;

        //데이터 확인
        if ( saveData == null ||
            saveData.RequirementResults == null ||
            saveData.WishResults == null ||
            saveData.ThemeResults == null ||
            saveData.TargetThemeResults == null ||
            saveData.ExcludedThemeResults == null ||
            saveData.ScoreResult == null ||
            saveData.UsedCraftCost < 0 ||
            saveData.UsedPartCount <= 0 )
        {
            return false;
        }

        //저장 데이터를 런타임 데이터로 변환
        if ( TryCreateConditionResults( saveData.RequirementResults,
                out List<CraftConditionResult> requirements ) == false ||
            TryCreateConditionResults( saveData.WishResults,
                out List<CraftConditionResult> wishes ) == false ||
            TryCreateThemeResults( saveData.ThemeResults,
                out List<CraftThemeResult> themes ) == false ||
            TryCreateTargetThemeResults( saveData.TargetThemeResults,
                out List<CraftTargetThemeResult> targetThemes ) == false ||
            TryCreateExcludedThemeResults( saveData.ExcludedThemeResults,
                out List<CraftExcludedThemeResult> excludedThemes ) == false ||
            TryCreateScoreResult( saveData.ScoreResult,
                out CraftScoreResult scoreResult ) == false )
        {
            return false;
        }

        //제작 판정 데이터 생성
        reviewData = new CraftReviewData
        {
            IsAssemblyCompleted = saveData.IsAssemblyCompleted,
            IsCostExceeded = saveData.IsCostExceeded,
            UsedCraftCost = saveData.UsedCraftCost,
            UsedPartCount = saveData.UsedPartCount,
            IsPartCountExceeded = saveData.IsPartCountExceeded,

            RequirementResults = requirements,
            WishResults = wishes,
            ThemeResults = themes,
            TargetThemeResults = targetThemes,
            ExcludedThemeResults = excludedThemes,
            ScoreResult = scoreResult
        };

        return true;
    }

    /// <summary>
    /// 제작 조건 판정 결과 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="saveDatas">제작 조건 판정 결과 저장 데이터</param>
    /// <param name="results">제작 조건 결과 데이터</param>
    /// <returns>제작 조건 결과 생성 성공 여부</returns>
    bool TryCreateConditionResults (
        IReadOnlyList<CraftConditionResultSaveData> saveDatas,
        out List<CraftConditionResult> results )
    {
        results = new List<CraftConditionResult>( saveDatas.Count );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            //저장 데이터 가져오기
            CraftConditionResultSaveData data = saveDatas [ i ];

            //데이터 확인
            if ( data == null || string.IsNullOrWhiteSpace( data.PartId ) == true ||
                data.RequiredQuantity <= 0 ||
                data.UsedQuantity < 0 ||
                IsFinite( data.ScoreChanged ) == false )
            {
                results = null;
                return false;
            }

            //결과에 추가
            results.Add( new CraftConditionResult
            {
                PartId = data.PartId,
                RequiredQuantity = data.RequiredQuantity,
                UsedQuantity = data.UsedQuantity,
                IsCompleted = data.IsCompleted,
                ScoreChanged = data.ScoreChanged
            } );
        }

        return true;
    }

    /// <summary>
    /// 테마 판정 결과 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="saveDatas">테마 판정 결과 저장 데이터</param>
    /// <param name="results">테마 판정 결과</param>
    /// <returns>테마 결과 생성 성공 여부</returns>
    bool TryCreateThemeResults (
        IReadOnlyList<CraftThemeResultSaveData> saveDatas,
        out List<CraftThemeResult> results )
    {
        results = new List<CraftThemeResult>( saveDatas.Count );
        var themes = new HashSet<PartTheme>( );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            //저장 데이터 가져오기
            CraftThemeResultSaveData data = saveDatas [ i ];

            //데이터 확인
            if ( data == null || Enum.IsDefined( typeof( PartTheme ), data.Theme ) == false ||
                themes.Add( data.Theme ) == false ||
                IsFinite( data.BaseScore ) == false ||
                IsFinite( data.TargetBonus ) == false ||
                IsFinite( data.ResearchBonus ) == false )
            {
                results = null;
                return false;
            }

            //테마 판정 결과 추가
            results.Add( new CraftThemeResult
            {
                Theme = data.Theme,
                UsedTypeCount = data.UsedTypeCount,
                TotalTypeCount = data.TotalTypeCount,
                IsCompleted = data.IsCompleted,
                IsTargetTheme = data.IsTargetTheme,
                BaseScore = data.BaseScore,
                TargetBonus = data.TargetBonus,
                ResearchBonus = data.ResearchBonus
            } );
        }

        return true;
    }

    /// <summary>
    /// 목표 테마 판정 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="saveDatas">목표 테마 판정 저장 데이터</param>
    /// <param name="results">목표 테마 판정 데이터</param>
    /// <returns>목표 테마 결과 생성 성공 여부</returns>
    bool TryCreateTargetThemeResults (
        IReadOnlyList<CraftTargetThemeResultSaveData> saveDatas,
        out List<CraftTargetThemeResult> results )
    {
        results = new List<CraftTargetThemeResult>( saveDatas.Count );

        var themes = new HashSet<PartTheme>( );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            //저장 데이터 가져오기
            CraftTargetThemeResultSaveData data = saveDatas [ i ];

            //데이터 확인
            if ( data == null || Enum.IsDefined( typeof( PartTheme ), data.Theme ) == false ||
                themes.Add( data.Theme ) == false ||
                data.UsedTypeCount < 0 ||
                data.TotalTypeCount < 0 ||
                data.UsedTypeCount > data.TotalTypeCount )
            {
                results = null;
                return false;
            }

            //목표 테마 판정 결과 추가
            results.Add( new CraftTargetThemeResult
            {
                Theme = data.Theme,
                UsedTypeCount = data.UsedTypeCount,
                TotalTypeCount = data.TotalTypeCount,
                IsActive = data.IsActive,
                IsCompleted = data.IsCompleted
            } );
        }

        return true;
    }

    /// <summary>
    /// 제외 테마 판정 결과 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="saveDatas">제외 테마 판정 결과 저장 데이터</param>
    /// <param name="results">제외 테마 판정 결과</param>
    /// <returns>제외 테마 결과 생성 성공 여부</returns>
    bool TryCreateExcludedThemeResults (
        IReadOnlyList<CraftExcludedThemeResultSaveData> saveDatas,
        out List<CraftExcludedThemeResult> results )
    {
        results = new List<CraftExcludedThemeResult>( saveDatas.Count );

        var themes = new HashSet<PartTheme>( );

        for ( int i = 0; i < saveDatas.Count; i++ )
        {
            //저장 데이터 가져오기
            CraftExcludedThemeResultSaveData data = saveDatas [ i ];

            //데이터 확인
            if ( data == null || data.UsedPartCount < 0 ||
                Enum.IsDefined( typeof( PartTheme ), data.Theme ) == false ||
                themes.Add( data.Theme ) == false )
            {
                results = null;
                return false;
            }

            //제외 테마 결과 추가
            results.Add( new CraftExcludedThemeResult
            {
                Theme = data.Theme,
                UsedPartCount = data.UsedPartCount
            } );
        }

        return true;
    }

    /// <summary>
    /// 점수 결과 저장 데이터를 런타임 데이터로 변환
    /// </summary>
    /// <param name="data">점수 결과 저장 데이터</param>
    /// <param name="result">점수 결과</param>
    /// <returns>점수 결과 생성 성공 여부</returns>
    bool TryCreateScoreResult (
        CraftScoreSaveData data, out CraftScoreResult result )
    {
        result = null;

        //데이터 확인
        if ( data == null ||
            IsFinite( data.AssemblyScore ) == false ||
            IsFinite( data.RequirementScore ) == false ||
            IsFinite( data.WishScore ) == false ||
            IsFinite( data.ThemeBaseScore ) == false ||
            IsFinite( data.TargetThemeBonus ) == false ||
            IsFinite( data.ThemeResearchBonus ) == false ||
            IsFinite( data.ThemeSubtotal ) == false ||
            IsFinite( data.ThemeRate ) == false ||
            IsFinite( data.ThemeReduction ) == false ||
            IsFinite( data.ThemeLimit ) == false ||
            IsFinite( data.ThemeLimitReduction ) == false ||
            data.ConflictCount < 0 ||
            IsFinite( data.ConflictPenalty ) == false ||
            IsFinite( data.ThemeScore ) == false ||
            IsFinite( data.RawScore ) == false )
        {
            return false;
        }

        //점수 결과 생성
        result = new CraftScoreResult
        {
            AssemblyScore = data.AssemblyScore,
            RequirementScore = data.RequirementScore,
            WishScore = data.WishScore,
            ThemeBaseScore = data.ThemeBaseScore,
            TargetThemeBonus = data.TargetThemeBonus,
            ThemeResearchBonus = data.ThemeResearchBonus,
            ThemeSubtotal = data.ThemeSubtotal,
            ThemeRate = data.ThemeRate,
            ThemeReduction = data.ThemeReduction,
            ThemeLimit = data.ThemeLimit,
            ThemeLimitReduction = data.ThemeLimitReduction,
            ConflictCount = data.ConflictCount,
            ConflictPenalty = data.ConflictPenalty,
            ThemeScore = data.ThemeScore,
            RawScore = data.RawScore,
            FinalScore = data.FinalScore
        };

        return true;
    }

    /// <summary>
    /// 유한한 수인지 확인
    /// </summary>
    /// <param name="value">확인할 값</param>
    /// <returns>유한한 값 여부</returns>
    bool IsFinite ( float value )
    {
        return float.IsNaN( value ) == false && float.IsInfinity( value ) == false;
    }

    #endregion
}
