using System;
using System.Collections.Generic;

/// <summary>
/// 특수 주문 생성기 - 개수 제한과 목표, 제외 테마 생성
/// </summary>
public class OrderSpecialGenerator
{
    /// <summary>
    /// 생성할 특수 조건 종류
    /// </summary>
    public enum ConditionType
    {
        None,       //특수 조건 없음
        PartCount,      //전체 파츠 개수 제한
        TargetTheme,        //목표 테마
        ExcludedTheme,      //제외 테마
    }

    static readonly PartTheme [ ] ThemeValues =
        ( PartTheme [ ] ) Enum.GetValues( typeof( PartTheme ) );       //테마 순회값

    Random _random;       //주문 생성 모델과 공유하는 무작위 생성기
    OrderSpecialSettingsData _settings;       //특수 주문 생성 설정

    /// <summary>
    /// 특수 주문 생성기 초기화
    /// </summary>
    /// <param name="random">주문 생성 모델과 공유하는 무작위 생성기</param>
    /// <param name="settings">특수 주문 생성 설정 데이터</param>
    public OrderSpecialGenerator ( Random random, OrderSpecialSettingsData settings )
    {
        _random = random ?? throw new ArgumentNullException( nameof( random ) );
        _settings = settings;
    }

    #region ----- 특수 조건 선택 -----
    /// <summary>
    /// 가중치에 따른 특수 조건 종류 선택
    /// </summary>
    /// <returns>선택한 특수 조건 종류</returns>
    public ConditionType SelectCondition ()
    {
        int selectedWeight = _random.Next(
            _settings.NoConditionWeight + _settings.PartCountWeight +
            _settings.TargetThemeWeight + _settings.ExcludedThemeWeight );

        if ( selectedWeight < _settings.NoConditionWeight )
            return ConditionType.None;

        selectedWeight -= _settings.NoConditionWeight;

        if ( selectedWeight < _settings.PartCountWeight )
            return ConditionType.PartCount;

        selectedWeight -= _settings.PartCountWeight;

        if ( selectedWeight < _settings.TargetThemeWeight )
            return ConditionType.TargetTheme;

        return ConditionType.ExcludedTheme;
    }

    /// <summary>
    /// 최소 해결 수량에 맞는 최대 파츠 수 생성
    /// </summary>
    /// <param name="minimumPartCount">최소 해결 파츠 수</param>
    /// <returns>생성한 최대 파츠 수</returns>
    public int CreateMaxPartCount ( int minimumPartCount )
    {
        int buffer = _random.Next(
            _settings.PartCountBufferRange.Min,
            _settings.PartCountBufferRange.Max + 1 );

        return minimumPartCount + buffer;
    }
    #endregion

    #region ----- 목표 테마 -----
    /// <summary>
    /// 활성 가능한 목표 테마 선택
    /// </summary>
    /// <param name="unlockedParts">해금된 파츠 목록</param>
    /// <param name="requirements">주요 요구 사항 목록</param>
    /// <param name="wishes">희망 사항 목록</param>
    /// <param name="targetTheme">선택한 목표 테마</param>
    /// <returns>목표 테마 선택 성공 여부</returns>
    public bool TrySelectTargetTheme (
        IReadOnlyList<PartsData> unlockedParts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes,
        out PartTheme targetTheme )
    {
        var candidates = new List<PartTheme>( );

        //실제 활성 가능한 테마만 후보에 추가
        for ( int i = 0; i < ThemeValues.Length; i++ )
        {
            PartTheme theme = ThemeValues [ i ];

            if ( theme.GetCategory( ) != PartThemeCategory.None &&
                CanActivateTheme(
                unlockedParts, requirements, wishes, theme ) )
                candidates.Add( theme );
        }

        if ( candidates.Count == 0 )
        {
            targetTheme = PartTheme.None;
            return false;
        }

        targetTheme = candidates [ _random.Next( candidates.Count ) ];
        return true;
    }

    /// <summary>
    /// 목표 테마 활성 가능 여부 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="requirements">주요 요구 사항 목록</param>
    /// <param name="wishes">희망 사항 목록</param>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>목표 테마 활성 가능 여부</returns>
    bool CanActivateTheme (
        IReadOnlyList<PartsData> parts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes, PartTheme theme )
    {
        int themedRequiredTypeCount = 0;
        int themedOptionalTypeCount = 0;
        HashSet<PartType> usedOptionalTypes = GetConditionOptionalTypes(
            parts, requirements, wishes );

        //모든 파츠 타입의 테마 선택 가능 여부 확인
        for ( int i = 0; i < ( int ) PartType.Count; i++ )
        {
            PartType partType = ( PartType ) i;
            bool hasTheme = partType == PartType.Body
                ? HasAvailableBodyTheme( parts, requirements, wishes, theme )
                : HasPartTheme( parts, partType, theme );

            if ( hasTheme == false ) continue;

            if ( IsMinimumPartType( partType ) )
                themedRequiredTypeCount++;
            else
            {
                themedOptionalTypeCount++;
                usedOptionalTypes.Add( partType );
            }
        }

        //테마 파츠 타입을 모두 사용했을 때 전체 사용 타입의 절반 초과 여부 반환
        int usedTypeCount = 4 + usedOptionalTypes.Count;
        int themedTypeCount = themedRequiredTypeCount + themedOptionalTypeCount;
        return themedTypeCount * 2 > usedTypeCount;
    }

    /// <summary>
    /// 주문 조건에서 반드시 사용하는 추가 파츠 타입 생성
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="requirements">주요 요구 사항 목록</param>
    /// <param name="wishes">희망 사항 목록</param>
    /// <returns>주문 조건에서 반드시 사용하는 추가 파츠 타입</returns>
    HashSet<PartType> GetConditionOptionalTypes (
        IReadOnlyList<PartsData> parts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes )
    {
        var partTypes = new HashSet<PartType>( );

        AddConditionOptionalTypes( partTypes, parts, requirements );
        AddConditionOptionalTypes( partTypes, parts, wishes );
        return partTypes;
    }

    /// <summary>
    /// 주문 조건의 추가 파츠 타입 반영
    /// </summary>
    /// <param name="partTypes">추가 파츠 타입 집계</param>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="conditions">확인할 주문 조건 목록</param>
    void AddConditionOptionalTypes (
        HashSet<PartType> partTypes, IReadOnlyList<PartsData> parts,
        IReadOnlyList<OrderPartCondition> conditions )
    {
        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( TryGetPart( parts, conditions [ i ].PartId, out PartsData part ) &&
                IsMinimumPartType( part.PartType ) == false )
                partTypes.Add( part.PartType );
        }
    }

    /// <summary>
    /// 몸통의 목표 테마 선택 가능 여부 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="requirements">주요 요구 사항 목록</param>
    /// <param name="wishes">희망 사항 목록</param>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>몸통의 해당 테마 선택 가능 여부</returns>
    bool HasAvailableBodyTheme (
        IReadOnlyList<PartsData> parts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes, PartTheme theme )
    {
        //몸통 조건이 있으면 지정된 몸통의 테마 확인
        if ( TryGetConditionBody( parts, requirements, out PartsData body ) ||
            TryGetConditionBody( parts, wishes, out body ) )
            return ContainsTheme( body.Themes, theme );

        return HasPartTheme( parts, PartType.Body, theme );
    }
    #endregion

    #region ----- 제외 테마 -----
    /// <summary>
    /// 제외 테마와 사용 가능 파츠 선택
    /// </summary>
    /// <param name="unlockedParts">해금된 파츠 목록</param>
    /// <param name="requirements">주요 요구 사항 목록</param>
    /// <param name="wishes">희망 사항 목록</param>
    /// <param name="excludedTheme">선택한 제외 테마</param>
    /// <param name="availableParts">제외 테마 없이 사용할 수 있는 파츠 목록</param>
    /// <returns>제외 테마 선택 성공 여부</returns>
    public bool TrySelectExcludedTheme (
        IReadOnlyList<PartsData> unlockedParts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes,
        out PartTheme excludedTheme, out List<PartsData> availableParts )
    {
        var candidates = new List<PartTheme>( );

        //조건 파츠와 충돌하지 않는 테마만 후보에 추가
        for ( int i = 0; i < ThemeValues.Length; i++ )
        {
            PartTheme theme = ThemeValues [ i ];

            if ( theme.GetCategory( ) != PartThemeCategory.None &&
                HasTheme( unlockedParts, theme ) &&
                HasConditionTheme( unlockedParts, requirements, theme ) == false &&
                HasConditionTheme( unlockedParts, wishes, theme ) == false &&
                HasMinimumPartsWithoutTheme( unlockedParts, theme ) )
                candidates.Add( theme );
        }

        if ( candidates.Count == 0 )
        {
            excludedTheme = PartTheme.None;
            availableParts = null;
            return false;
        }

        excludedTheme = candidates [ _random.Next( candidates.Count ) ];
        availableParts = GetPartsWithoutTheme( unlockedParts, excludedTheme );
        return true;
    }

    /// <summary>
    /// 주문 조건 파츠의 테마 포함 여부 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="conditions">확인할 주문 조건 목록</param>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>주문 조건 파츠의 테마 포함 여부</returns>
    bool HasConditionTheme (
        IReadOnlyList<PartsData> parts,
        IReadOnlyList<OrderPartCondition> conditions, PartTheme theme )
    {
        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( TryGetPart( parts, conditions [ i ].PartId, out PartsData part ) &&
                ContainsTheme( part.Themes, theme ) )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 제외 테마 없이 최소 제작 타입 구성 가능 여부 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="theme">제외할 파츠 테마</param>
    /// <returns>제외 테마 없이 최소 제작 타입 구성 가능 여부</returns>
    bool HasMinimumPartsWithoutTheme (
        IReadOnlyList<PartsData> parts, PartTheme theme )
    {
        return HasPartWithoutTheme( parts, PartType.Body, theme ) &&
            HasPartWithoutTheme( parts, PartType.Eye, theme ) &&
            HasPartWithoutTheme( parts, PartType.Nose, theme ) &&
            HasPartWithoutTheme( parts, PartType.Mouth, theme );
    }

    /// <summary>
    /// 제외 테마가 없는 파츠 목록 생성
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="theme">제외할 파츠 테마</param>
    /// <returns>제외 테마가 없는 파츠 목록</returns>
    List<PartsData> GetPartsWithoutTheme (
        IReadOnlyList<PartsData> parts, PartTheme theme )
    {
        var result = new List<PartsData>( );

        for ( int i = 0; i < parts.Count; i++ )
        {
            PartsData part = parts [ i ];

            if ( part != null && ContainsTheme( part.Themes, theme ) == false )
                result.Add( part );
        }

        return result;
    }
    #endregion

    #region ----- 파츠 조회 -----
    /// <summary>
    /// 주문 조건에 포함된 몸통 조회
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="conditions">확인할 주문 조건 목록</param>
    /// <param name="body">조회한 몸통 파츠</param>
    /// <returns>몸통 파츠 조회 성공 여부</returns>
    bool TryGetConditionBody (
        IReadOnlyList<PartsData> parts,
        IReadOnlyList<OrderPartCondition> conditions, out PartsData body )
    {
        for ( int i = 0; i < conditions.Count; i++ )
        {
            //조건 파츠 조회 후 몸통 타입 확인
            if ( TryGetPart( parts, conditions [ i ].PartId, out PartsData part ) &&
                part.PartType == PartType.Body )
            {
                //조회한 몸통 반환
                body = part;
                return true;
            }
        }

        body = null;
        return false;
    }

    /// <summary>
    /// 파츠 타입의 테마 보유 여부 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="partType">확인할 파츠 타입</param>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>해당 타입 파츠의 테마 보유 여부</returns>
    bool HasPartTheme (
        IReadOnlyList<PartsData> parts, PartType partType, PartTheme theme )
    {
        for ( int i = 0; i < parts.Count; i++ )
        {
            PartsData part = parts [ i ];

            if ( part != null && part.PartType == partType &&
                ContainsTheme( part.Themes, theme ) )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 파츠 목록의 테마 포함 여부 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>파츠 목록의 테마 포함 여부</returns>
    bool HasTheme ( IReadOnlyList<PartsData> parts, PartTheme theme )
    {
        for ( int i = 0; i < parts.Count; i++ )
        {
            if ( parts [ i ] != null && ContainsTheme( parts [ i ].Themes, theme ) )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 해당 타입에서 제외 테마가 아닌 파츠 확인
    /// </summary>
    /// <param name="parts">사용 가능한 파츠 목록</param>
    /// <param name="partType">확인할 파츠 타입</param>
    /// <param name="theme">제외할 파츠 테마</param>
    /// <returns>해당 타입의 제외 테마 아닌 파츠 존재 여부</returns>
    bool HasPartWithoutTheme (
        IReadOnlyList<PartsData> parts, PartType partType, PartTheme theme )
    {
        for ( int i = 0; i < parts.Count; i++ )
        {
            PartsData part = parts [ i ];

            if ( part != null && part.PartType == partType &&
                ContainsTheme( part.Themes, theme ) == false )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 파츠 아이디로 데이터 조회
    /// </summary>
    /// <param name="parts">조회할 파츠 목록</param>
    /// <param name="partId">조회할 파츠 아이디</param>
    /// <param name="part">조회한 파츠 데이터</param>
    /// <returns>파츠 데이터 조회 성공 여부</returns>
    bool TryGetPart (
        IReadOnlyList<PartsData> parts, string partId, out PartsData part )
    {
        for ( int i = 0; i < parts.Count; i++ )
        {
            if ( parts [ i ] != null && parts [ i ].Id == partId )
            {
                part = parts [ i ];
                return true;
            }
        }

        part = null;
        return false;
    }

    /// <summary>
    /// 최소 제작 필수 파츠 타입 여부 확인
    /// </summary>
    /// <param name="partType">확인할 파츠 타입</param>
    /// <returns>최소 제작 필수 파츠 타입 여부</returns>
    bool IsMinimumPartType ( PartType partType )
    {
        return partType == PartType.Body || partType == PartType.Eye ||
            partType == PartType.Nose || partType == PartType.Mouth;
    }

    /// <summary>
    /// 대상 테마 포함 여부 확인
    /// </summary>
    /// <param name="themes">파츠 테마 목록</param>
    /// <param name="targetTheme">확인할 파츠 테마</param>
    /// <returns>테마 포함 여부</returns>
    bool ContainsTheme (
        IReadOnlyList<PartTheme> themes, PartTheme targetTheme )
    {
        for ( int i = 0; i < themes.Count; i++ )
        {
            if ( themes [ i ] == targetTheme ) return true;
        }

        return false;
    }
    #endregion
}
