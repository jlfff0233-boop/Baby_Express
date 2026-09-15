using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 판정 모델 - 제작 조건, 테마와 점수 계산
/// </summary>
public class CraftReviewModel
{
    static readonly PartTheme [ ] ThemeValues =
        ( PartTheme [ ] ) Enum.GetValues( typeof( PartTheme ) );

    CraftScoreSettingsData _settings;       //제작 점수 설정 데이터
    MaintenanceModel _maintenanceModel;       //적용된 테마 연구 조회용 정비 모델

    /// <summary>
    /// 제작에 사용한 파츠 집계(계산용)
    /// </summary>
    class CraftPartSummary
    {
        /// <summary>
        /// 파츠별 수량 딕셔너리(파츠 아이디, 수량)
        /// </summary>
        public Dictionary<string, int> PartQuantities { get; set; }

        /// <summary>
        /// 파츠 타입별 수량(파츠 타입, 수량)
        /// </summary>
        public Dictionary<PartType, int> TypeQuantities { get; set; }

        /// <summary>
        /// 테마별 파츠 종류 딕셔너리(파츠 테마, 파츠 종류 목록)
        /// </summary>
        public Dictionary<PartTheme, HashSet<PartType>> ThemeTypes { get; set; }

        /// <summary>
        /// 테마별 사용 파츠 수량 딕셔너리(파츠 테마, 수량)
        /// </summary>
        public Dictionary<PartTheme, int> ThemePartQuantities { get; set; }

        public int CraftCost { get; set; }        //사용 제작 코스트
        public int PartCount { get; set; }        //전체 사용 파츠 수
    }

    /// <summary>
    /// 제작 판정 모델 생성
    /// </summary>
    /// <param name="settings">제작 점수 설정 데이터</param>
    /// <param name="maintenanceModel">정비 모델</param>
    public CraftReviewModel (
        CraftScoreSettingsData settings, MaintenanceModel maintenanceModel )
    {
        _settings = settings;
        _maintenanceModel = maintenanceModel;
    }

    /// <summary>
    /// 현재 배치 제작 판정
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <param name="placedParts">현재 배치 파츠</param>
    /// <param name="rootPlacementNumber">루트 몸통 배치 번호</param>
    /// <param name="reviewData">구조 오류면 null, 제작 조건 미달이면 현재 상태의 판정 데이터</param>
    /// <returns>제작 판정 처리 결과</returns>
    public CraftReviewResult Review (
        CustomerOrder order, IReadOnlyList<PlacedPartData> placedParts,
        int rootPlacementNumber, out CraftReviewData reviewData )
    {
        reviewData = null;

        //제작 중인 주문 확인
        if ( order == null || order.ProgressState != OrderProgressState.Production )
            return CraftReviewResult.InvalidOrder;

        //파츠 집계 생성
        if ( TryCreatePartSummary( placedParts, out CraftPartSummary partSummary ) == false )
            return CraftReviewResult.InvalidParts;

        //루트 몸통과 최소 제작 조건 확인
        bool hasRootBody = HasRootBody( placedParts, rootPlacementNumber );
        bool isAssemblyCompleted = hasRootBody &&
            HasMinimumParts( partSummary.TypeQuantities );

        //최대 제작 코스트 초과 여부 확인
        bool isCostExceeded = partSummary.CraftCost > order.MaxCraftCost;

        //전체 파츠 개수 제한 초과 여부 확인
        bool isPartCountExceeded = order.MaxPartCount > 0 &&
            partSummary.PartCount > order.MaxPartCount;

        //주요 요구와 희망 판정
        List<CraftConditionResult> requirements =
            ReviewRequirements( order.Requirements, partSummary.PartQuantities );
        List<CraftConditionResult> wishes =
            ReviewWishes( order.Wishes, partSummary.PartQuantities );

        //활성 테마 판정
        List<CraftThemeResult> themes = ReviewThemes(
            order, partSummary.ThemeTypes, partSummary.TypeQuantities.Count );
        //목표 테마 판정
        List<CraftTargetThemeResult> targetThemes = ReviewTargetThemes(
            order.TargetThemes, partSummary.ThemeTypes,
            partSummary.TypeQuantities.Count );
        //제외 테마 판정
        List<CraftExcludedThemeResult> excludedThemes = ReviewExcludedThemes(
            order.ExcludedThemes, partSummary.ThemePartQuantities );

        //미완성 상태를 포함한 제작 판정 데이터 생성
        reviewData = new CraftReviewData
        {
            IsAssemblyCompleted = isAssemblyCompleted,
            IsCostExceeded = isCostExceeded,

            UsedCraftCost = partSummary.CraftCost,
            UsedPartCount = partSummary.PartCount,
            IsPartCountExceeded = isPartCountExceeded,

            RequirementResults = requirements,
            WishResults = wishes,
            ThemeResults = themes,
            TargetThemeResults = targetThemes,
            ExcludedThemeResults = excludedThemes
        };

        //현재 상태의 예상 점수 계산
        reviewData.ScoreResult = CalculateScore( order, reviewData );

        //최종 제작 가능 여부 반환
        return GetReviewResult( reviewData );
    }

    #region ----- 파츠 집계 -----
    /// <summary>
    /// 제작에 사용한 파츠 집계 생성
    /// </summary>
    /// <param name="placedParts">현재 배치 파츠 목록</param>
    /// <param name="partSummary">생성된 제작 파츠 집계</param>
    /// <returns>파츠 집계 생성 성공 여부</returns>
    bool TryCreatePartSummary ( IReadOnlyList<PlacedPartData> placedParts,
        out CraftPartSummary partSummary )
    {
        partSummary = null;

        //배치 목록 자체가 없으면 잘못된 데이터
        if ( placedParts == null ) return false;

        var summary = new CraftPartSummary
        {
            PartQuantities = new Dictionary<string, int>( ),
            TypeQuantities = new Dictionary<PartType, int>( ),
            ThemeTypes = new Dictionary<PartTheme, HashSet<PartType>>( ),
            ThemePartQuantities = new Dictionary<PartTheme, int>( )
        };

        for ( int i = 0; i < placedParts.Count; i++ )
        {
            PlacedPartData placedPart = placedParts [ i ];

            //잘못된 배치 파츠 차단
            if ( placedPart?.PartData == null ) return false;

            PartsData part = placedPart.PartData;

            //파츠 아이디와 타입 수량 집계
            AddQuantity( summary.PartQuantities, part.Id );
            AddQuantity( summary.TypeQuantities, part.PartType );

            //파츠 테마별 사용 타입 집계
            AddThemes( summary.ThemeTypes, summary.ThemePartQuantities, part );

            //사용 제작 코스트 합산
            summary.CraftCost += part.CraftCost;
            //전체 사용 파츠 수 증가
            summary.PartCount++;
        }

        partSummary = summary;
        return true;
    }

    /// <summary>
    /// 파츠의 테마별 타입과 수량 집계
    /// </summary>
    /// <param name="themeTypes">테마별 파츠 타입 목록</param>
    /// <param name="themePartQuantities">테마별 사용 파츠 수량</param>
    /// <param name="part">집계할 파츠</param>
    void AddThemes (
        Dictionary<PartTheme, HashSet<PartType>> themeTypes,
        Dictionary<PartTheme, int> themePartQuantities,
        PartsData part )
    {
        for ( int i = 0; i < part.Themes.Count; i++ )
        {
            PartTheme theme = part.Themes [ i ];

            //테마 없음은 집계하지 않음
            if ( theme == PartTheme.None ) continue;

            //처음 사용한 테마면 타입 목록 생성
            if ( themeTypes.TryGetValue( theme, out HashSet<PartType> types ) == false )
            {
                //해당 테마의 파츠 타입 추가
                types = new HashSet<PartType>( );
                themeTypes.Add( theme, types );
            }

            //같은 타입은 테마별 한 번만 추가
            types.Add( part.PartType );

            //테마별 사용 파츠 수량 추가
            if ( themePartQuantities.TryGetValue( theme, out int quantity ) )
                themePartQuantities [ theme ] = quantity + 1;
            else
                themePartQuantities.Add( theme, 1 );
        }
    }

    /// <summary>
    /// 파츠 아이디별 수량 추가
    /// </summary>
    /// <param name="quantities">파츠별 수량</param>
    /// <param name="partId">추가할 파츠 아이디</param>
    void AddQuantity ( Dictionary<string, int> quantities, string partId )
    {
        //딕셔너리에 파츠가 존재하면 수량 증가
        if ( quantities.TryGetValue( partId, out int quantity ) )
            quantities [ partId ] = quantity + 1;
        //없으면 파츠 새로 추가
        else
            quantities.Add( partId, 1 );
    }

    /// <summary>
    /// 파츠 타입별 수량 추가
    /// </summary>
    /// <param name="quantities">파츠 타입별 수량</param>
    /// <param name="partType">추가할 파츠 타입</param>
    void AddQuantity ( Dictionary<PartType, int> quantities, PartType partType )
    {
        //딕셔너리에 파츠 타입이 존재하면 수량 증가
        if ( quantities.TryGetValue( partType, out int quantity ) )
            quantities [ partType ] = quantity + 1;
        else
            quantities.Add( partType, 1 );
    }
    #endregion

    #region ----- 제작 조건 판정 -----
    /// <summary>
    /// 루트 몸통 배치 여부 확인
    /// </summary>
    /// <param name="placedParts">현재 배치 파츠 목록</param>
    /// <param name="rootPlacementNumber">루트 몸통 배치 번호</param>
    /// <returns>루트 몸통 배치 여부</returns>
    bool HasRootBody (
        IReadOnlyList<PlacedPartData> placedParts, int rootPlacementNumber )
    {
        //배치한 게 없으면 종료
        if ( rootPlacementNumber <= 0 ) return false;

        for ( int i = 0; i < placedParts.Count; i++ )
        {
            //배치한 파츠 데이터 가져오기
            PlacedPartData placedPart = placedParts [ i ];

            if ( placedPart.PlacementNumber == rootPlacementNumber &&
                placedPart.PartData.PartType == PartType.Body )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 최소 제작 조건 확인
    /// </summary>
    /// <param name="quantities">파츠 타입별 수량</param>
    /// <returns>최소 제작 조건 달성 여부</returns>
    bool HasMinimumParts ( IReadOnlyDictionary<PartType, int> quantities )
    {
        //몸통은 정확히 한 개, 눈, 코, 입은 각각 한 개 이상 사용
        return GetQuantity( quantities, PartType.Body ) == 1 &&
            GetQuantity( quantities, PartType.Eye ) >= 1 &&
            GetQuantity( quantities, PartType.Nose ) >= 1 &&
            GetQuantity( quantities, PartType.Mouth ) >= 1;
    }

    /// <summary>
    /// 주요 요구 사항 판정
    /// </summary>
    /// <param name="requirements">주요 요구 사항 목록</param>
    /// <param name="quantities">파츠별 사용 수량</param>
    /// <returns>주요 요구 사항 판정 결과 목록</returns>
    List<CraftConditionResult> ReviewRequirements (
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyDictionary<string, int> quantities )
    {
        var results = new List<CraftConditionResult>( requirements.Count );

        for ( int i = 0; i < requirements.Count; i++ )
        {
            OrderPartCondition requirement = requirements [ i ];
            int usedQuantity = GetQuantity( quantities, requirement.PartId );

            //주요 요구는 정확 수량일 때 달성
            bool isCompleted = usedQuantity == requirement.Quantity;

            results.Add( new CraftConditionResult
            {
                PartId = requirement.PartId,
                RequiredQuantity = requirement.Quantity,
                UsedQuantity = usedQuantity,

                IsCompleted = isCompleted,
                ScoreChanged = isCompleted
                    ? _settings.RequirementCompletedScore
                    : _settings.RequirementFailedScore
            } );
        }

        return results;
    }

    /// <summary>
    /// 희망 사항 판정
    /// </summary>
    /// <param name="wishes">희망 사항 목록</param>
    /// <param name="quantities">파츠별 사용 수량</param>
    /// <returns>희망 사항 판정 결과 목록</returns>
    List<CraftConditionResult> ReviewWishes (
        IReadOnlyList<OrderPartCondition> wishes,
        IReadOnlyDictionary<string, int> quantities )
    {
        var results = new List<CraftConditionResult>( wishes.Count );

        for ( int i = 0; i < wishes.Count; i++ )
        {
            OrderPartCondition wish = wishes [ i ];
            int usedQuantity = GetQuantity( quantities, wish.PartId );

            //희망은 최소 수량 이상일 때 달성
            bool isCompleted = usedQuantity >= wish.Quantity;

            results.Add( new CraftConditionResult
            {
                PartId = wish.PartId,
                RequiredQuantity = wish.Quantity,
                UsedQuantity = usedQuantity,
                IsCompleted = isCompleted,
                ScoreChanged = isCompleted ? _settings.WishCompletedScore : 0f
            } );
        }

        return results;
    }
    #endregion

    #region ----- 테마 판정 -----
    /// <summary>
    /// 활성 테마 판정
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <param name="themeTypes">테마별 파츠 타입 목록</param>
    /// <param name="usedTypeCount">사용한 전체 파츠 타입 수</param>
    /// <returns>활성 테마 판정 결과 목록</returns>
    List<CraftThemeResult> ReviewThemes (
        CustomerOrder order,
        IReadOnlyDictionary<PartTheme, HashSet<PartType>> themeTypes,
        int usedTypeCount )
    {
        var results = new List<CraftThemeResult>( );

        //enum 순서대로 확인해 표시 순서 고정
        for ( int i = 0; i < ThemeValues.Length; i++ )
        {
            PartTheme theme = ThemeValues [ i ];

            //테마 없음과 사용하지 않은 테마 제외
            if ( theme.GetCategory( ) == PartThemeCategory.None ||
                themeTypes.TryGetValue( theme, out HashSet<PartType> types ) == false )
                continue;

            //테마 종류 수량
            int themeTypeCount = types.Count;
            //달성 여부
            bool isCompleted = themeTypeCount == usedTypeCount;

            //사용 타입의 절반을 초과하지 않으면 비활성
            if ( isCompleted == false && themeTypeCount * 2 <= usedTypeCount )
                continue;

            //목표 테마 여부 확인
            bool isTargetTheme = ContainsTheme( order.TargetThemes, theme );
            //기본 점수
            float baseScore = isCompleted
                ? _settings.CompletedThemeScore
                : _settings.NormalThemeScore;
            //목표 테마 보너스 점수
            float targetBonus = isTargetTheme
                ? baseScore * ( _settings.TargetThemeRate - 1f )
                : 0f;
            //현재 적용된 테마 연구 보너스
            float researchBonus =
                _maintenanceModel.GetThemeScoreBonus( theme );

            results.Add( new CraftThemeResult
            {
                Theme = theme,
                UsedTypeCount = themeTypeCount,
                TotalTypeCount = usedTypeCount,
                IsCompleted = isCompleted,
                IsTargetTheme = isTargetTheme,
                BaseScore = baseScore,
                TargetBonus = targetBonus,
                ResearchBonus = researchBonus
            } );
        }

        return results;
    }

    /// <summary>
    /// 목표 테마 진행 상태 판정
    /// </summary>
    /// <param name="targetThemes">목표 테마 목록</param>
    /// <param name="themeTypes">테마별 파츠 타입 목록</param>
    /// <param name="usedTypeCount">사용한 전체 파츠 타입 수</param>
    /// <returns>목표 테마 진행 결과 목록</returns>
    List<CraftTargetThemeResult> ReviewTargetThemes (
        IReadOnlyList<PartTheme> targetThemes,
        IReadOnlyDictionary<PartTheme, HashSet<PartType>> themeTypes,
        int usedTypeCount )
    {
        var results = new List<CraftTargetThemeResult>( targetThemes.Count );

        for ( int i = 0; i < targetThemes.Count; i++ )
        {
            //파츠 테마 가져오기
            PartTheme theme = targetThemes [ i ];
            //테마 수량 가져오기
            int themeTypeCount = themeTypes.TryGetValue(
                theme, out HashSet<PartType> types )
                ? types.Count
                : 0;

            bool isCompleted = usedTypeCount > 0 && themeTypeCount == usedTypeCount;
            bool isActive = isCompleted || themeTypeCount * 2 > usedTypeCount;

            results.Add( new CraftTargetThemeResult
            {
                Theme = theme,
                UsedTypeCount = themeTypeCount,
                TotalTypeCount = usedTypeCount,
                IsActive = isActive,
                IsCompleted = isCompleted
            } );
        }

        return results;
    }

    /// <summary>
    /// 제외 테마 사용 상태 판정
    /// </summary>
    /// <param name="excludedThemes">제외 테마 목록</param>
    /// <param name="themePartQuantities">테마별 사용 파츠 수량</param>
    /// <returns>제외 테마 사용 결과 목록</returns>
    List<CraftExcludedThemeResult> ReviewExcludedThemes (
        IReadOnlyList<PartTheme> excludedThemes,
        IReadOnlyDictionary<PartTheme, int> themePartQuantities )
    {
        var results = new List<CraftExcludedThemeResult>( excludedThemes.Count );

        for ( int i = 0; i < excludedThemes.Count; i++ )
        {
            PartTheme theme = excludedThemes [ i ];
            int usedPartCount = themePartQuantities.TryGetValue(
                theme, out int quantity )
                ? quantity
                : 0;

            results.Add( new CraftExcludedThemeResult
            {
                Theme = theme,
                UsedPartCount = usedPartCount
            } );
        }

        return results;
    }
    #endregion

    #region ----- 점수 계산 -----
    /// <summary>
    /// 제작 점수 계산
    /// </summary>
    /// <param name="order">제작 주문</param>
    /// <param name="reviewData">현재 제작 판정 데이터</param>
    /// <returns>제작 점수 결과</returns>
    CraftScoreResult CalculateScore (
        CustomerOrder order, CraftReviewData reviewData )
    {
        //최소 제작 조건 달성 시에만 조립 점수 적용
        float assemblyScore = reviewData.IsAssemblyCompleted
            ? _settings.AssemblyCompletedScore
            : 0f;

        float requirementScore = SumConditionScores( reviewData.RequirementResults );
        float wishScore = SumConditionScores( reviewData.WishResults );

        float themeBaseScore = 0f;
        float targetThemeBonus = 0f;
        float themeResearchBonus = 0f;

        //테마 기본 점수와 목표, 연구 보너스 합산
        for ( int i = 0; i < reviewData.ThemeResults.Count; i++ )
        {
            CraftThemeResult theme = reviewData.ThemeResults [ i ];

            themeBaseScore += theme.BaseScore;
            targetThemeBonus += theme.TargetBonus;
            themeResearchBonus += theme.ResearchBonus;
        }

        float themeSubtotal =
            themeBaseScore + targetThemeBonus + themeResearchBonus;

        //활성 테마 수에 맞는 감쇠 적용
        float themeRate = GetThemeRate( reviewData.ThemeResults.Count );
        float reducedThemeScore = themeSubtotal * themeRate;
        float themeReduction = themeSubtotal - reducedThemeScore;

        //목표 테마 주문 여부에 맞는 상한 적용
        float themeLimit = order.TargetThemes.Count > 0
            ? _settings.TargetThemeLimit
            : _settings.NormalThemeLimit;

        float limitedThemeScore = Mathf.Min( reducedThemeScore, themeLimit );
        float themeLimitReduction = reducedThemeScore - limitedThemeScore;

        //활성화된 상극 테마 감점
        int conflictCount = CountConflicts( reviewData.ThemeResults );
        float conflictPenalty = conflictCount * _settings.ConflictScorePenalty;

        //상극 감점 후 테마 점수에 0점 하한 적용
        float themeScore = Mathf.Max( 0f, limitedThemeScore - conflictPenalty );

        //전체 제작 원점수 합산
        float rawScore = assemblyScore + requirementScore + wishScore + themeScore;

        //전체 계산 마지막에 반올림하고 0점 하한 적용
        int finalScore = Mathf.Max( 0, Mathf.RoundToInt( rawScore ) );

        //제작 결과 점수 반환
        return new CraftScoreResult
        {
            AssemblyScore = assemblyScore,
            RequirementScore = requirementScore,
            WishScore = wishScore,

            ThemeBaseScore = themeBaseScore,
            TargetThemeBonus = targetThemeBonus,
            ThemeResearchBonus = themeResearchBonus,
            ThemeSubtotal = themeSubtotal,
            ThemeRate = themeRate,
            ThemeReduction = themeReduction,
            ThemeLimit = themeLimit,
            ThemeLimitReduction = themeLimitReduction,

            ConflictCount = conflictCount,
            ConflictPenalty = conflictPenalty,
            ThemeScore = themeScore,
            RawScore = rawScore,
            FinalScore = finalScore
        };
    }

    /// <summary>
    /// 최종 제작 가능 여부 결정
    /// </summary>
    /// <param name="reviewData">현재 제작 판정 데이터</param>
    /// <returns>최종 제작 가능 여부 결과</returns>
    CraftReviewResult GetReviewResult ( CraftReviewData reviewData )
    {
        //최소 제작 조건을 먼저 확인
        if ( reviewData.IsAssemblyCompleted == false )
            return CraftReviewResult.MissingMinimumParts;

        //최대 제작 코스트 확인
        if ( reviewData.IsCostExceeded )
            return CraftReviewResult.CostExceeded;

        //최대 파츠 개수 제한 확인
        if ( reviewData.IsPartCountExceeded )
            return CraftReviewResult.PartCountExceeded;

        //제외 테마 사용 여부 확인
        for ( int i = 0; i < reviewData.ExcludedThemeResults.Count; i++ )
        {
            if ( reviewData.ExcludedThemeResults [ i ].IsUsed )
                return CraftReviewResult.ExcludedThemeUsed;
        }

        return CraftReviewResult.Success;
    }

    /// <summary>
    /// 제작 조건 변경 점수 합산
    /// </summary>
    /// <param name="results">제작 조건 판정 결과 목록</param>
    /// <returns>제작 조건 변경 점수 합계</returns>
    float SumConditionScores ( IReadOnlyList<CraftConditionResult> results )
    {
        float score = 0f;

        for ( int i = 0; i < results.Count; i++ )
            score += results [ i ].ScoreChanged;

        return score;
    }

    /// <summary>
    /// 활성 테마 수에 맞는 적용 비율 반환
    /// </summary>
    /// <param name="themeCount">활성 테마 수</param>
    /// <returns>활성 테마 적용 비율</returns>
    float GetThemeRate ( int themeCount )
    {
        if ( themeCount >= 3 ) return _settings.MultipleThemeRate;
        if ( themeCount == 2 ) return _settings.TwoThemeRate;

        return 1f;
    }

    /// <summary>
    /// 활성화된 상극 테마 쌍 수 계산
    /// </summary>
    /// <param name="themes">활성 테마 결과 목록</param>
    /// <returns>활성화된 상극 테마 쌍 수</returns>
    int CountConflicts ( IReadOnlyList<CraftThemeResult> themes )
    {
        int count = 0;

        //상극 목록은 중복 없이 한 방향으로 전달
        IReadOnlyList<ThemeConflict> themeConflicts = _settings.ThemeConflicts;

        for ( int i = 0; i < themeConflicts.Count; i++ )
        {
            ThemeConflict conflict = themeConflicts [ i ];

            if ( ContainsTheme( themes, conflict.First ) &&
                ContainsTheme( themes, conflict.Second ) )
                count++;
        }

        return count;
    }
    #endregion

    #region ----- 조회 -----
    /// <summary>
    /// 파츠 타입별 수량 조회
    /// </summary>
    /// <param name="quantities">파츠 타입별 수량</param>
    /// <param name="partType">조회할 파츠 타입</param>
    /// <returns>해당 파츠 타입의 수량</returns>
    int GetQuantity ( IReadOnlyDictionary<PartType, int> quantities, PartType partType )
    {
        return quantities.TryGetValue( partType, out int quantity )
            ? quantity
            : 0;
    }

    /// <summary>
    /// 파츠 아이디별 수량 조회
    /// </summary>
    /// <param name="quantities">파츠 아이디별 수량</param>
    /// <param name="partId">조회할 파츠 아이디</param>
    /// <returns>해당 파츠 아이디의 수량</returns>
    int GetQuantity ( IReadOnlyDictionary<string, int> quantities, string partId )
    {
        return quantities.TryGetValue( partId, out int quantity )
            ? quantity
            : 0;
    }

    /// <summary>
    /// 테마 목록 포함 여부 확인
    /// </summary>
    /// <param name="themes">파츠 테마 목록</param>
    /// <param name="target">확인할 파츠 테마</param>
    /// <returns>테마 포함 여부</returns>
    bool ContainsTheme ( IReadOnlyList<PartTheme> themes, PartTheme target )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ] == target )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 활성 테마 결과 포함 여부 확인
    /// </summary>
    /// <param name="themes">활성 테마 결과 목록</param>
    /// <param name="target">확인할 파츠 테마</param>
    /// <returns>활성 테마 결과 포함 여부</returns>
    bool ContainsTheme (
        IReadOnlyList<CraftThemeResult> themes,
        PartTheme target )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ].Theme == target )
                return true;
        }

        return false;
    }
    #endregion
}
