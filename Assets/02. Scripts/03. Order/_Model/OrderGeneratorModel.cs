using System;
using System.Collections.Generic;

/// <summary>
/// 주문 생성 모델 - 난이도와 해금 파츠를 이용한 주문 생성
/// </summary>
public class OrderGeneratorModel
{
    const int MaxRetryCount = 20;       //최대 생성 재시도 횟수
    const int MinDailyOrderCount = 3;       //일일 최소 주문 수
    const int MaxDailyOrderCount = 5;       //일일 최대 주문 수

    OrderTitleData _titleData;       //주문 제목 데이터
    List<OrderDifficultyData> _difficultySettings;       //난이도 설정 목록
    OrderSpecialSettingsData _specialSettings;       //특수 주문 생성 설정
    Random _random;       //랜덤 생성기
    OrderConditionGenerator _conditionGenerator;       //주요 요구와 희망 생성기
    OrderSpecialGenerator _specialGenerator;       //특수 조건 생성기
    OrderCostCalculator _costCalculator;       //최대 제작 코스트 계산기
    EmployeeModel _employeeModel;       //직원 효과 모델
    WeeklyOrderAdjustment _weeklyAdjustment = new WeeklyOrderAdjustment( );       //현재 주간 주문 생성 보정

    /// <summary>
    /// 주문 생성 모델 생성
    /// </summary>
    /// <param name="settings">주문 생성 설정</param>
    /// <param name="employeeModel">직원 효과 모델</param>
    public OrderGeneratorModel (
        OrderGenerationSettingsData settings,
        EmployeeModel employeeModel )
    {
        _titleData = settings.TitleData;
        _difficultySettings =
            new List<OrderDifficultyData>( settings.DifficultyDataMap.Datas );
        _specialSettings = settings.SpecialSettings;
        _random = new Random( );
        _conditionGenerator = new OrderConditionGenerator( _random );
        _specialGenerator = new OrderSpecialGenerator( _random, _specialSettings );
        _costCalculator = new OrderCostCalculator( );
        _employeeModel = employeeModel;
    }

    #region ----- 주문량 -----

    /// <summary>
    /// 현재 주문 생성 보정을 저장 데이터로 변환
    /// </summary>
    /// <returns>주문 생성 모델 저장 데이터</returns>
    public OrderGeneratorSaveData CreateSaveData ()
    {
        return new OrderGeneratorSaveData(
            _weeklyAdjustment.IsApplied,
            _weeklyAdjustment.OrderCountCorrection,
            _weeklyAdjustment.EasyWeight,
            _weeklyAdjustment.NormalWeight,
            _weeklyAdjustment.HardWeight );
    }

    /// <summary>
    /// 주문 생성 저장 데이터 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveData">복구할 주문 생성 데이터</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore (
        OrderGeneratorSaveData saveData )
    {
        if ( saveData == null )
            return false;

        //보정이 없으면 기본 상태 그대로 허용
        if ( saveData.IsApplied == false )
            return true;

        if ( saveData.EasyWeight < 0 ||
            saveData.NormalWeight < 0 ||
            saveData.HardWeight < 0 )
        {
            return false;
        }

        long totalWeight =
            ( long ) saveData.EasyWeight +
            saveData.NormalWeight +
            saveData.HardWeight;

        return totalWeight > 0 &&
            totalWeight <= int.MaxValue;
    }

    /// <summary>
    /// 저장 데이터로 주문 생성 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 주문 생성 데이터</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore (
        OrderGeneratorSaveData saveData )
    {
        if ( CanRestore( saveData ) == false )
            return false;

        _weeklyAdjustment =
            new WeeklyOrderAdjustment
            {
                IsApplied = saveData.IsApplied,
                OrderCountCorrection =
                    saveData.OrderCountCorrection,
                EasyWeight = saveData.EasyWeight,
                NormalWeight = saveData.NormalWeight,
                HardWeight = saveData.HardWeight
            };

        return true;
    }

    /// <summary>
    /// 다음 주 주문 생성 보정 적용
    /// </summary>
    /// <param name="adjustment">적용할 주간 주문 생성 보정</param>
    public void ApplyWeeklyAdjustment ( WeeklyOrderAdjustment adjustment )
    {
        //외부 결산 데이터와 분리하여 현재 보정값 저장
        _weeklyAdjustment = new WeeklyOrderAdjustment
        {
            IsApplied = adjustment.IsApplied,
            OrderCountCorrection = adjustment.OrderCountCorrection,
            EasyWeight = adjustment.EasyWeight,
            NormalWeight = adjustment.NormalWeight,
            HardWeight = adjustment.HardWeight
        };
    }

    /// <summary>
    /// 직원 효과가 적용된 일일 최소 주문 보너스 조회
    /// </summary>
    /// <returns>일일 최소 주문 추가값</returns>
    int GetDailyMinimumBonus ()
    {
        return Math.Max( 0, ( int ) Math.Round(
            _employeeModel.GetEffectValue(
                EmployeeEffectType.DailyOrderMinimumBonus ),
            MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// 주문량 페널티 적용 후 일일 주문 생성 수 계산
    /// </summary>
    /// <param name="waitingCount">현재 수락 대기 주문 수</param>
    /// <param name="waitingLimit">최대 수락 대기 주문 수</param>
    /// <param name="orderReduction">주문량 페널티 감소량</param>
    /// <returns>실제 생성할 주문 수</returns>
    public int GetDailyOrderCount (
        int waitingCount, int waitingLimit, int orderReduction )
    {
        //수락 대기 빈자리 계산
        int availableCount = Math.Max( 0, waitingLimit - waitingCount );

        if ( availableCount == 0 ) return 0;

        //기본 일일 주문 수에 주간 보정과 주문량 페널티 적용
        int minimumCount = MinDailyOrderCount + GetDailyMinimumBonus( );
        int maximumCount = Math.Max( minimumCount, MaxDailyOrderCount );
        int randomCount = _random.Next( minimumCount, maximumCount + 1 );
        int weeklyCorrection = _weeklyAdjustment.IsApplied
            ? _weeklyAdjustment.OrderCountCorrection
            : 0;
        int correctedCount = Math.Max(
            0, randomCount + weeklyCorrection - orderReduction );

        return Math.Min( correctedCount, availableCount );
    }

    /// <summary>
    /// 현재 보정 적용 후 예상 주문량 범위 계산
    /// </summary>
    /// <param name="reduction">일일 주문 감소량</param>
    /// <param name="minCount">예상 최소 주문 수</param>
    /// <param name="maxCount">예상 최대 주문 수</param>
    public void GetExpectedOrderRange (
        int reduction, out int minCount, out int maxCount )
    {
        GetExpectedOrderRange(
            reduction, _weeklyAdjustment, out minCount, out maxCount );
    }

    /// <summary>
    /// 지정 보정 적용 후 예상 주문량 범위 계산
    /// (다음 주 예상값 계산 가능)
    /// </summary>
    /// <param name="reduction">일일 주문 감소량</param>
    /// <param name="adjustment">적용할 주간 주문 생성 보정</param>
    /// <param name="minCount">예상 최소 주문 수</param>
    /// <param name="maxCount">예상 최대 주문 수</param>
    public void GetExpectedOrderRange (
        int reduction, WeeklyOrderAdjustment adjustment,
        out int minCount, out int maxCount )
    {
        int weeklyCorrection = adjustment.IsApplied
            ? adjustment.OrderCountCorrection
            : 0;

        //주간 보정과 주문 감소량을 적용한 예상 범위 계산
        int minimumBonus = GetDailyMinimumBonus( );

        minCount = Math.Max(
            0, MinDailyOrderCount + minimumBonus +
            weeklyCorrection - reduction );
        maxCount = Math.Max(
            minCount, MaxDailyOrderCount +
            weeklyCorrection - reduction );
    }

    #endregion

    #region ----- 주문 생성 -----
    /// <summary>
    /// 주문 생성
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="createdNumber">누적 생성 번호</param>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <param name="highGradeEvaluationCount">B등급 이상 평가 누적 수</param>
    /// <param name="dayCostBonus">영업일 최대 코스트 추가값</param>
    /// <param name="upgradeCostBonus">업그레이드 최대 코스트 추가값</param>
    /// <param name="order">생성된 주문</param>
    /// <returns>주문 생성 결과</returns>
    public OrderGenerateResult GenerateOrder ( int totalDay, int createdNumber,
        IReadOnlyList<PartsData> unlockedParts, int highGradeEvaluationCount,
        int dayCostBonus, int upgradeCostBonus, out CustomerOrder order )
    {
        //기본값 설정
        order = null;

        //생성 설정 확인
        if ( IsValidSettings( totalDay, createdNumber, unlockedParts,
            highGradeEvaluationCount, dayCostBonus, upgradeCostBonus ) == false )
            return OrderGenerateResult.InvalidSettings;

        //사용 가능한 주문 제목 생성
        string title = CreateTitle( );
        if ( string.IsNullOrEmpty( title ) ) return OrderGenerateResult.InvalidSettings;

        //현재 해금 상태에 맞는 난이도 후보 생성
        List<OrderDifficultyData> difficultyPool = CreateDifficultyPool( unlockedParts );

        //생성 가능한 난이도가 없으면 종료
        if ( difficultyPool.Count == 0 )
            return OrderGenerateResult.InvalidSettings;

        //해결 가능한 주문이 나올 때까지 제한 횟수만큼 생성
        for ( int i = 0; i < MaxRetryCount; i++ )
        {
            OrderDifficultyData difficultyData = SelectDifficulty( difficultyPool );

            //주요 요구 사항 생성
            if ( _conditionGenerator.CreateRequirements(
                difficultyData, unlockedParts, out var requirements ) == false )
                continue;

            //희망 사항 생성
            if ( _conditionGenerator.CreateWishes(
                difficultyData, unlockedParts, requirements, out var wishes ) == false )
                continue;

            //특수 조건 종류 선택
            OrderSpecialGenerator.ConditionType conditionType =
                _specialGenerator.SelectCondition( );
            IReadOnlyList<PartsData> costParts = unlockedParts;
            List<PartsData> availableParts = null;
            PartTheme targetTheme = PartTheme.None;
            PartTheme excludedTheme = PartTheme.None;

            //목표 테마 후보 선택
            if ( conditionType == OrderSpecialGenerator.ConditionType.TargetTheme &&
                _specialGenerator.TrySelectTargetTheme(
                    unlockedParts, requirements, wishes, out targetTheme ) == false )
                continue;

            //제외 테마와 해당 테마가 없는 파츠 후보 선택
            if ( conditionType == OrderSpecialGenerator.ConditionType.ExcludedTheme &&
                _specialGenerator.TrySelectExcludedTheme(
                    unlockedParts, requirements, wishes,
                    out excludedTheme, out availableParts ) == false )
                continue;

            if ( conditionType == OrderSpecialGenerator.ConditionType.ExcludedTheme )
                costParts = availableParts;

            //난이도별 목표 코스트 계산
            int targetCost = GetRandomValue( difficultyData.CraftCostRange ) +
                dayCostBonus + upgradeCostBonus;

            //최소 해결 구성과 최대 제작 코스트 계산
            if ( _costCalculator.GetMaxCraftCost(
                costParts, requirements, wishes, targetCost,
                out int maxCraftCost, out int minimumPartCount ) == false )
                continue;

            //개수 제한 주문이면 최소 해결 수량 기준 제한 생성
            int maxPartCount = conditionType == OrderSpecialGenerator.ConditionType.PartCount
                ? _specialGenerator.CreateMaxPartCount( minimumPartCount )
                : 0;

            //현재 플레이 상태에 따라 일반 또는 VIP 주문 추첨
            OrderSpecialType specialType = SelectVipType( highGradeEvaluationCount );

            //주문 기간 결정
            int acceptDue = totalDay + GetRandomValue( difficultyData.AcceptSpan );
            int deliveryDue = acceptDue + GetRandomValue( difficultyData.DeliverySpan );
            int maxDelay = GetRandomValue( difficultyData.DelaySpan );

            //주문 생성 데이터 구성
            var createData = new OrderCreateData
            {
                CreatedNumber = createdNumber,
                Id = $"Order_{totalDay}_{createdNumber}_{difficultyData.Difficulty}",
                Title = title,
                MaxCraftCost = maxCraftCost,

                Requirements = requirements,
                Wishes = wishes,

                SpecialType = specialType,
                MaxPartCount = maxPartCount,
                TargetThemes = targetTheme == PartTheme.None
                    ? Array.Empty<PartTheme>( )
                    : new [ ] { targetTheme },
                ExcludedThemes = excludedTheme == PartTheme.None
                    ? Array.Empty<PartTheme>( )
                    : new [ ] { excludedTheme },

                CreatedTotalDay = totalDay,
                AcceptDue = acceptDue,
                DeliveryDue = deliveryDue,
                MaxDelay = maxDelay,

                Difficulty = difficultyData.Difficulty
            };

            order = new CustomerOrder( createData );
            return OrderGenerateResult.Success;
        }

        return OrderGenerateResult.Unsolvable;
    }

    /// <summary>
    /// 주문 생성 설정 확인
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="createdNumber">누적 생성 번호</param>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <param name="highGradeEvaluationCount">B등급 이상 평가 누적 수</param>
    /// <param name="dayCostBonus">영업일 코스트 추가값</param>
    /// <param name="upgradeCostBonus">업그레이드 코스트 추가값</param>
    /// <returns>사용 가능한 설정 여부</returns>
    bool IsValidSettings (
        int totalDay, int createdNumber,
        IReadOnlyList<PartsData> unlockedParts,
        int highGradeEvaluationCount, int dayCostBonus, int upgradeCostBonus )
    {
        //기본 입력 확인
        if ( totalDay < 1 || createdNumber < 1 ||
            highGradeEvaluationCount < 0 || dayCostBonus < 0 || upgradeCostBonus < 0 ) return false;

        //제목 설정 확인
        if ( _titleData == null ) return false;

        //특수 주문 설정 확인
        if ( _specialSettings == null || _specialSettings.IsValid( ) == false )
            return false;

        //난이도 설정 확인
        if ( _difficultySettings.Count == 0 ) return false;

        //기본 제작 파츠 해금 여부 확인
        return HasPartType( unlockedParts, PartType.Body ) &&
               HasPartType( unlockedParts, PartType.Eye ) &&
               HasPartType( unlockedParts, PartType.Nose ) &&
               HasPartType( unlockedParts, PartType.Mouth );
    }

    /// <summary>
    /// 주문 제목 생성
    /// </summary>
    /// <returns>생성된 주문 제목</returns>
    string CreateTitle ()
    {
        List<string> adjectives = GetTexts( _titleData.Adjectives );
        List<string> requests = GetTexts( _titleData.Requests );

        //사용 가능한 제목 조합이 없으면 생성하지 않음
        if ( adjectives.Count == 0 || requests.Count == 0 ) return string.Empty;

        return $"{adjectives [ _random.Next( adjectives.Count ) ]} 아기를 {requests [ _random.Next( requests.Count ) ]}";
    }

    #endregion

    #region ----- 난이도/특수 주문 -----
    /// <summary>
    /// 현재 해금 상태에 맞는 난이도 후보 생성
    /// </summary>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <returns>사용 가능한 난이도 목록</returns>
    List<OrderDifficultyData> CreateDifficultyPool ( IReadOnlyList<PartsData> unlockedParts )
    {
        var difficultyPool = new List<OrderDifficultyData>( );

        //사용 가능한 난이도만 후보에 추가
        for ( int i = 0; i < _difficultySettings.Count; i++ )
        {
            OrderDifficultyData data = _difficultySettings [ i ];

            if ( data != null && data.IsValid( ) && CanUseDifficulty( data.Difficulty, unlockedParts ) )
                difficultyPool.Add( data );
        }

        return difficultyPool;
    }
    /// <summary>
    /// 주문 난이도 사용 가능 여부 확인
    /// </summary>
    /// <param name="difficulty">확인할 난이도</param>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <returns>사용 가능 여부</returns>
    bool CanUseDifficulty ( OrderDifficulty difficulty, IReadOnlyList<PartsData> unlockedParts )
    {
        switch ( difficulty )
        {
            //쉬움과 보통은 기본 사용
            case OrderDifficulty.Easy:
            case OrderDifficulty.Normal:
                return true;

            //어려움은 모든 파츠 타입 해금 후 사용
            case OrderDifficulty.Hard:
                return IsHardUnlocked( unlockedParts );

            default:
                return false;
        }
    }

    /// <summary>
    /// 모든 파츠 타입 해금 여부 확인
    /// </summary>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <returns>전체 파츠 타입 해금 여부</returns>
    public bool IsHardUnlocked ( IReadOnlyList<PartsData> unlockedParts )
    {
        //Count를 제외한 모든 파츠 타입 확인
        for ( int i = 0; i < ( int ) PartType.Count; i++ )
        {
            if ( HasPartType( unlockedParts, ( PartType ) i ) == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 가중치에 따른 난이도 선택
    /// </summary>
    /// <param name="difficultyPool">사용 가능한 난이도 목록</param>
    /// <returns>선택한 난이도 설정</returns>
    OrderDifficultyData SelectDifficulty (
        IReadOnlyList<OrderDifficultyData> difficultyPool )
    {
        int totalWeight = 0;

        //사용 가능한 난이도의 현재 전체 가중치 계산
        for ( int i = 0; i < difficultyPool.Count; i++ )
            totalWeight += GetDifficultyWeight( difficultyPool [ i ] );

        int selectedWeight = _random.Next( totalWeight );

        //선택값에 해당하는 난이도 반환
        for ( int i = 0; i < difficultyPool.Count; i++ )
        {
            OrderDifficultyData data = difficultyPool [ i ];
            int weight = GetDifficultyWeight( data );

            if ( selectedWeight < weight )
                return data;

            selectedWeight -= weight;
        }

        return difficultyPool [ difficultyPool.Count - 1 ];
    }

    /// <summary>
    /// 현재 주간 보정을 적용한 난이도 가중치 반환
    /// </summary>
    /// <param name="difficultyData">난이도 설정 데이터</param>
    /// <returns>현재 난이도 가중치</returns>
    int GetDifficultyWeight ( OrderDifficultyData difficultyData )
    {
        //주간 보정 전에는 기본 난이도 가중치 사용
        if ( _weeklyAdjustment.IsApplied == false )
            return difficultyData.Weight;

        switch ( difficultyData.Difficulty )
        {
            case OrderDifficulty.Easy:
                return _weeklyAdjustment.EasyWeight;

            case OrderDifficulty.Normal:
                return _weeklyAdjustment.NormalWeight;

            case OrderDifficulty.Hard:
                return _weeklyAdjustment.HardWeight;

            default:
                return 0;
        }
    }

    /// <summary>
    /// 특수 주문 선택
    /// </summary>
    /// <param name="highGradeEvaluationCount">B등급 이상 평가 누적 수</param>
    /// <returns>선택한 특수 주문 종류</returns>
    OrderSpecialType SelectVipType ( int highGradeEvaluationCount )
    {
        //VIP 해금 전에는 일반 주문만 생성
        if ( highGradeEvaluationCount < _specialSettings.VipRequiredGradeCount )
            return OrderSpecialType.None;

        int totalWeight = _specialSettings.RegularWeight + _specialSettings.VipWeight;
        int selectedWeight = _random.Next( totalWeight );

        //일반 주문 가중치 범위면 일반 주문
        if ( selectedWeight < _specialSettings.RegularWeight )
            return OrderSpecialType.None;

        return OrderSpecialType.Vip;
    }

    #endregion

    #region ----- 파츠 조회 -----
    /// <summary>
    /// 특정 파츠 타입 존재 여부 확인
    /// </summary>
    /// <param name="parts">해금 파츠 목록</param>
    /// <param name="partType">확인할 파츠 타입</param>
    /// <returns>파츠 타입 존재 여부</returns>
    bool HasPartType ( IReadOnlyList<PartsData> parts, PartType partType )
    {
        if ( parts == null ) return false;

        for ( int i = 0; i < parts.Count; i++ )
        {
            if ( parts [ i ] != null &&
                 parts [ i ].PartType == partType )
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region ----- 공용 보조 함수 -----
    /// <summary>
    /// 사용 가능한 문자열 목록 생성
    /// </summary>
    /// <param name="texts">원본 문자열 목록</param>
    /// <returns>빈 문자열을 제외한 목록</returns>
    List<string> GetTexts ( IReadOnlyList<string> texts )
    {
        var validTexts = new List<string>( );

        if ( texts == null ) return validTexts;

        for ( int i = 0; i < texts.Count; i++ )
        {
            if ( string.IsNullOrWhiteSpace( texts [ i ] ) == false )
                validTexts.Add( texts [ i ] );
        }

        return validTexts;
    }

    /// <summary>
    /// 설정 범위에서 무작위 값 선택
    /// </summary>
    /// <param name="range">선택할 범위</param>
    /// <returns>선택한 값</returns>
    int GetRandomValue ( IntRange range )
    {
        return _random.Next( range.Min, range.Max + 1 );
    }

    #endregion

}
