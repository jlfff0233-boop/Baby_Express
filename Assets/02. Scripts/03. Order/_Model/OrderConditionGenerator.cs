using System;
using System.Collections.Generic;

/// <summary>
/// 주문 조건 생성기 - 주요 요구와 희망 생성
/// </summary>
public class OrderConditionGenerator
{
    const int MaxTotalConditionCount = 12;       //주요 요구와 희망을 합친 최대 개수

    Random _random;       //주문 생성 모델과 공유하는 무작위 생성기

    /// <summary>
    /// 주문 조건 생성기 생성
    /// </summary>
    /// <param name="random">공유할 무작위 생성기</param>
    public OrderConditionGenerator ( Random random )
    {
        _random = random ?? throw new ArgumentNullException( nameof( random ) );
    }

    #region ----- 주요 요구 사항 -----
    /// <summary>
    /// 주요 요구 사항 생성
    /// </summary>
    /// <param name="difficultyData">난이도 설정</param>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <param name="requirements">생성된 주요 요구</param>
    /// <returns>생성 성공 여부</returns>
    public bool CreateRequirements (
        OrderDifficultyData difficultyData,
        IReadOnlyList<PartsData> unlockedParts,
        out List<OrderPartCondition> requirements )
    {
        //요구 사항 리스트 생성
        requirements = new List<OrderPartCondition>( );

        //해금된 파츠 리스트 설정
        List<PartsData> candidates = GetParts( unlockedParts );
        //요구 사항 개수 설정
        int requirementCount = GetRandomValue( difficultyData.ConditionCountRange );

        //난이도의 최소 주요 요구 개수도 만들 수 없으면 실패
        if ( candidates.Count < difficultyData.ConditionCountRange.Min ) return false;

        //요구 사항 개수와 해금 파츠 리스트 길이 중 작은 값 사용
        requirementCount = Math.Min( requirementCount, candidates.Count );
        Shuffle( candidates );

        bool hasBody = false;

        //후보를 순회하며 필요한 주요 요구 수만큼 생성
        for ( int i = 0; i < candidates.Count &&
            requirements.Count < requirementCount; i++ )
        {
            //주요 요구 파츠 가져오기
            PartsData part = candidates [ i ];

            //몸통 주요 요구는 하나만 생성
            if ( part.PartType == PartType.Body && hasBody ) continue;

            //몸통은 하나만 배치할 수 있으므로 수량을 1로 고정
            int quantity = part.PartType == PartType.Body
                ? 1
                : GetRandomValue( difficultyData.ConditionQuantityRange );

            //파츠 아이디, 주요 요구 수량 추가
            requirements.Add( new OrderPartCondition( part.Id, quantity ) );

            //몸통 주요 요구 생성 여부 저장
            if ( part.PartType == PartType.Body ) hasBody = true;
        }

        return requirements.Count >= difficultyData.ConditionCountRange.Min;
    }
    #endregion

    #region ----- 희망 사항 -----
    /// <summary>
    /// 희망 사항 생성
    /// </summary>
    /// <param name="difficultyData">난이도 설정</param>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <param name="requirements">주요 요구</param>
    /// <param name="wishes">생성된 희망</param>
    /// <returns>생성 성공 여부</returns>
    public bool CreateWishes ( OrderDifficultyData difficultyData,
        IReadOnlyList<PartsData> unlockedParts,
        IReadOnlyList<OrderPartCondition> requirements,
        out List<OrderPartCondition> wishes )
    {
        wishes = new List<OrderPartCondition>( );

        //희망 사항 개수 설정
        int wishCount = GetRandomValue( difficultyData.ConditionCountRange );
        //해금 파츠 리스트 설정
        List<PartsData> candidates = GetParts( unlockedParts );

        //주요 요구와 같은 파츠 아이디 제외
        for ( int i = candidates.Count - 1; i >= 0; i-- )
        {
            if ( ContainsPartId( requirements, candidates [ i ].Id ) )
                candidates.RemoveAt( i );
        }

        //최대 조건 개수에서 주요 요구 사항 개수를 뺀 값과 파츠 길이 중 작은 값 사용
        int availableCount = Math.Min(
            MaxTotalConditionCount - requirements.Count, candidates.Count );

        //난이도의 최소 희망 개수도 만들 수 없으면 실패
        if ( availableCount < difficultyData.ConditionCountRange.Min ) return false;

        //희망 사항 개수 설정
        wishCount = Math.Min( wishCount, availableCount );
        Shuffle( candidates );

        //주요 요구의 몸통 포함 여부 확인
        bool hasBodyRequirement = HasPartType(
            requirements, unlockedParts, PartType.Body );
        bool hasBodyWish = false;

        //희망 사항 생성
        for ( int i = 0; i < candidates.Count && wishes.Count < wishCount; i++ )
        {
            //파츠 가져오기
            PartsData part = candidates [ i ];

            //주요 요구에 몸통이 있거나 몸통 희망을 이미 생성했으면 제외
            if ( part.PartType == PartType.Body &&
                ( hasBodyRequirement || hasBodyWish ) )
                continue;

            //몸통은 하나만 배치할 수 있으므로 수량을 1로 고정
            int quantity = part.PartType == PartType.Body
                ? 1
                : GetRandomValue( difficultyData.ConditionQuantityRange );

            //최소 구성만으로 충족되는 희망은 2개 이상으로 보정
            if ( IsWishAutomaticallyMet( part, quantity, unlockedParts ) )
            {
                //몸통 하나로 자동 달성되는 희망은 생성하지 않음
                if ( part.PartType == PartType.Body ) continue;

                quantity = Math.Max( 2, difficultyData.ConditionQuantityRange.Min );

                if ( quantity > difficultyData.ConditionQuantityRange.Max ) continue;
            }

            //파츠 아이디, 수량 추가
            wishes.Add( new OrderPartCondition( part.Id, quantity ) );

            //몸통 희망 생성 여부 저장
            if ( part.PartType == PartType.Body ) hasBodyWish = true;
        }

        return wishes.Count >= difficultyData.ConditionCountRange.Min;
    }

    /// <summary>
    /// 최소 제작 구성만으로 희망이 자동 달성되는지 확인
    /// </summary>
    /// <param name="part">희망 파츠</param>
    /// <param name="quantity">희망 수량</param>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <returns>자동 달성 여부</returns>
    bool IsWishAutomaticallyMet (
        PartsData part, int quantity,
        IReadOnlyList<PartsData> unlockedParts )
    {
        //수량이 2개 이상이거나 최소 제작 필수 타입이 아니면 달성 x
        if ( quantity > 1 || IsMinimumPartType( part.PartType ) == false ) return false;

        //유일한 선택지가 아니면 자동 달성 x
        for ( int i = 0; i < unlockedParts.Count; i++ )
        {
            //해금 파츠 가져오기
            PartsData candidate = unlockedParts [ i ];

            //같은 타입의 해금된 대체 파츠 확인
            if ( candidate != null &&
                candidate.PartType == part.PartType &&
                candidate.Id != part.Id )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 최소 제작에 필요한 파츠 타입인지 확인
    /// </summary>
    bool IsMinimumPartType ( PartType partType )
    {
        return partType == PartType.Body || partType == PartType.Eye ||
               partType == PartType.Nose || partType == PartType.Mouth;
    }

    /// <summary>
    /// 조건 목록의 파츠 아이디 포함 여부 확인
    /// </summary>
    bool ContainsPartId ( IReadOnlyList<OrderPartCondition> conditions, string partId )
    {
        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( conditions [ i ].PartId == partId )
                return true;
        }

        return false;
    }

    /// <summary>
    /// 주문 조건의 파츠 타입 포함 여부 확인
    /// </summary>
    bool HasPartType (
        IReadOnlyList<OrderPartCondition> conditions,
        IReadOnlyList<PartsData> parts, PartType partType )
    {
        //조건을 순회하며 파츠 데이터 확인
        for ( int i = 0; i < conditions.Count; i++ )
        {
            for ( int j = 0; j < parts.Count; j++ )
            {
                PartsData part = parts [ j ];

                //조건 아이디와 타입이 같으면 포함된 것으로 반환
                if ( part != null && part.Id == conditions [ i ].PartId &&
                    part.PartType == partType )
                    return true;
            }
        }

        return false;
    }
    #endregion

    #region ----- 파츠 후보 -----
    /// <summary>
    /// 중복과 빈 값을 제거한 파츠 후보 생성
    /// </summary>
    List<PartsData> GetParts ( IReadOnlyList<PartsData> parts )
    {
        var result = new List<PartsData>( );

        for ( int i = 0; i < parts.Count; i++ )
        {
            PartsData part = parts [ i ];

            if ( part != null && ContainsPart( result, part.Id ) == false )
                result.Add( part );
        }

        return result;
    }

    /// <summary>
    /// 파츠 목록의 아이디 포함 여부 확인
    /// </summary>
    bool ContainsPart ( IReadOnlyList<PartsData> parts, string partId )
    {
        for ( int i = 0; i < parts.Count; i++ )
        {
            if ( parts [ i ].Id == partId ) return true;
        }

        return false;
    }
    #endregion

    #region ----- 무작위 -----
    /// <summary>
    /// 정수 범위 안의 무작위 값 생성
    /// </summary>
    int GetRandomValue ( IntRange range )
    {
        return _random.Next( range.Min, range.Max + 1 );
    }

    /// <summary>
    /// 목록 순서 무작위 변경
    /// </summary>
    void Shuffle<T> ( IList<T> items )
    {
        for ( int i = items.Count - 1; i > 0; i-- )
        {
            int index = _random.Next( i + 1 );
            (items [ i ], items [ index ]) = (items [ index ], items [ i ]);
        }
    }
    #endregion
}
