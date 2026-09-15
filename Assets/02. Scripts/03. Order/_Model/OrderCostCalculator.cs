using System;
using System.Collections.Generic;

/// <summary>
/// 제작 코스트 계산기 - 최소 해결 구성과 최대 제작 코스트 계산
/// </summary>
public class OrderCostCalculator
{
    /// <summary>
    /// 최소 제작 코스트 여윳값
    /// </summary>
    const int MinCraftCostBuffer = 10;

    #region ----- 최대 제작 코스트 -----
    /// <summary>
    /// 주문 최대 제작 코스트 계산
    /// </summary>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <param name="requirements">주요 요구</param>
    /// <param name="wishes">희망</param>
    /// <param name="targetCost">난이도와 보정이 반영된 목표 코스트</param>
    /// <param name="maxCraftCost">최대 제작 코스트</param>
    /// <param name="minimumPartCount">최소 해결 파츠 수</param>
    /// <returns>계산 성공 여부</returns>
    public bool GetMaxCraftCost (
        IReadOnlyList<PartsData> unlockedParts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes,
        int targetCost,
        out int maxCraftCost, out int minimumPartCount )
    {
        maxCraftCost = 0;
        minimumPartCount = 0;

        //최소 제작 코스트와 파츠 수 가져오기
        if ( GetMinimumCraftCost(
            unlockedParts, requirements, wishes,
            out int minimumCost, out minimumPartCount ) == false )
            return false;

        //코스트 추갓값
        int costBuffer = Math.Max( MinCraftCostBuffer, targetCost - minimumCost );
        //최대 제작 코스트 반올림
        maxCraftCost = RoundUpToTen( minimumCost + costBuffer );
        return true;
    }

    /// <summary>
    /// 제작 코스트를 10단위로 올림
    /// </summary>
    int RoundUpToTen ( int value )
    {
        return ( value + 9 ) / 10 * 10;
    }
    #endregion

    #region ----- 최소 해결 코스트 -----
    /// <summary>
    /// 주요 요구와 희망을 충족하는 최소 제작 코스트 계산
    /// </summary>
    /// <param name="unlockedParts">해금 파츠 목록</param>
    /// <param name="requirements">주요 요구 목록</param>
    /// <param name="wishes">희망 목록</param>
    /// <param name="minimumCost">최소 제작 코스트</param>
    /// <param name="minimumPartCount">최소 해결 파츠 수</param>
    /// <returns>계산 성공 여부</returns>
    bool GetMinimumCraftCost (
        IReadOnlyList<PartsData> unlockedParts,
        IReadOnlyList<OrderPartCondition> requirements,
        IReadOnlyList<OrderPartCondition> wishes,
        out int minimumCost, out int minimumPartCount )
    {
        //파츠별 최소 필요 수량
        var partQuantities = new Dictionary<string, int>( );
        //파츠 타입별 최소 제작 수량
        var typeQuantities = new Dictionary<PartType, int>( );
        minimumCost = 0;
        minimumPartCount = 0;

        //주요 요구와 희망의 최소 필요 수량 반영
        ApplyConditions( partQuantities, requirements );
        ApplyConditions( partQuantities, wishes );

        //조건 파츠의 코스트와 타입별 수량 계산
        foreach ( var condition in partQuantities )
        {
            //해금 파츠 가져오기
            if ( GetPart( unlockedParts, condition.Key, out PartsData part ) == false )
                return false;

            //코스트 추가
            minimumCost += part.CraftCost * condition.Value;
            //파츠 수 추가
            minimumPartCount += condition.Value;
            //해당 파츠 타입의 제작 수량 추가
            AddQuantity( typeQuantities, part.PartType, condition.Value );
        }

        //제작에 반드시 필요한 파츠 타입
        PartType [ ] minimumTypes =
        {
            PartType.Body,
            PartType.Eye,
            PartType.Nose,
            PartType.Mouth
        };

        for ( int i = 0; i < minimumTypes.Length; i++ )
        {
            //필수 파츠 타입 가져오기
            PartType partType = minimumTypes [ i ];

            //조건에서 이미 추가한 타입이면 다음 타입 확인
            if ( GetQuantity( typeQuantities, partType ) > 0 ) continue;
            //현재 타입의 최저 제작 코스트 조회
            if ( GetMinimumCost( unlockedParts, partType, out int partCost ) == false )
                return false;

            //필수 파츠의 최저 코스트 추가
            minimumCost += partCost;
            //필수 파츠 수 추가
            minimumPartCount++;
        }

        return true;
    }

    /// <summary>
    /// 파츠 조건의 최소 필요 수량 반영
    /// </summary>
    /// <param name="quantities">파츠별 최소 필요 수량</param>
    /// <param name="conditions">반영할 주문 조건 목록</param>
    void ApplyConditions ( Dictionary<string, int> quantities,
        IReadOnlyList<OrderPartCondition> conditions )
    {
        for ( int i = 0; i < conditions.Count; i++ )
        {
            //주문 조건 가져오기
            OrderPartCondition condition = conditions [ i ];

            //같은 파츠 조건이 있으면 더 큰 수량으로 갱신
            if ( quantities.TryGetValue( condition.PartId, out int quantity ) )
                quantities [ condition.PartId ] = Math.Max( quantity, condition.Quantity );
            else
                //새 파츠 조건이면 최소 필요 수량 추가
                quantities.Add( condition.PartId, condition.Quantity );
        }
    }

    /// <summary>
    /// 파츠 타입 수량 추가
    /// </summary>
    /// <param name="quantities">파츠 타입별 수량</param>
    /// <param name="partType">추가할 파츠 타입</param>
    /// <param name="quantity">추가할 수량</param>
    void AddQuantity ( Dictionary<PartType, int> quantities,
        PartType partType, int quantity )
    {
        //같은 파츠 타입이 있으면 수량 누적
        if ( quantities.ContainsKey( partType ) )
            quantities [ partType ] += quantity;
        else
            //새 파츠 타입이면 수량 추가
            quantities.Add( partType, quantity );
    }

    /// <summary>
    /// 파츠 타입 수량 조회
    /// </summary>
    /// <param name="quantities">파츠 타입별 수량</param>
    /// <param name="partType">조회할 파츠 타입</param>
    /// <returns>파츠 타입 수량</returns>
    int GetQuantity (
        IReadOnlyDictionary<PartType, int> quantities, PartType partType )
    {
        //저장된 수량을 반환하고 없으면 0 반환
        return quantities.TryGetValue( partType, out int quantity ) ? quantity : 0;
    }

    /// <summary>
    /// 파츠 타입별 최소 제작 코스트 조회
    /// </summary>
    /// <param name="parts">조회할 파츠 목록</param>
    /// <param name="partType">조회할 파츠 타입</param>
    /// <param name="minimumCost">최소 제작 코스트</param>
    /// <returns>조회 성공 여부</returns>
    bool GetMinimumCost (
        IReadOnlyList<PartsData> parts, PartType partType, out int minimumCost )
    {
        //최소 제작 코스트 초기화
        minimumCost = int.MaxValue;

        for ( int i = 0; i < parts.Count; i++ )
        {
            //현재 파츠 가져오기
            PartsData part = parts [ i ];

            //같은 타입의 더 낮은 제작 코스트 저장
            if ( part != null && part.PartType == partType &&
                part.CraftCost < minimumCost )
                minimumCost = part.CraftCost;
        }

        //같은 타입의 파츠를 찾았으면 성공
        return minimumCost != int.MaxValue;
    }

    /// <summary>
    /// 파츠 아이디로 파츠 데이터 조회
    /// </summary>
    /// <param name="parts">조회할 파츠 목록</param>
    /// <param name="partId">조회할 파츠 아이디</param>
    /// <param name="part">조회한 파츠 데이터</param>
    /// <returns>조회 성공 여부</returns>
    bool GetPart ( IReadOnlyList<PartsData> parts, string partId, out PartsData part )
    {
        for ( int i = 0; i < parts.Count; i++ )
        {
            //같은 아이디의 파츠 확인
            if ( parts [ i ] != null && parts [ i ].Id == partId )
            {
                part = parts [ i ];
                return true;
            }
        }

        part = null;
        return false;
    }
    #endregion
}
