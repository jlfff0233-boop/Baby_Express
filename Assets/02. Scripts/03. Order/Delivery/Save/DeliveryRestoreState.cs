using System.Collections.Generic;

/// <summary>
/// 배송 복구 상태 - 검증을 마친 배송 설정과 주문별 배송 데이터 보관
/// </summary>
public class DeliveryRestoreState
{
    int _deliverySpanReduction;       //배송 소요일 단축값
    Dictionary<string, DeliverySchedule> _deliverySchedules;       //주문별 배송 일정
    Dictionary<string, DeliveryResult> _deliveryResults;       //주문별 배송 결과

    /// <summary>
    /// 배송 소요일 단축값
    /// </summary>
    public int DeliverySpanReduction => _deliverySpanReduction;

    /// <summary>
    /// 배송 복구 상태 생성
    /// </summary>
    /// <param name="deliverySpanReduction">배송 소요일 단축값</param>
    /// <param name="deliverySchedules">주문별 배송 일정</param>
    /// <param name="deliveryResults">주문별 배송 결과</param>
    public DeliveryRestoreState (
        int deliverySpanReduction,
        Dictionary<string, DeliverySchedule> deliverySchedules,
        Dictionary<string, DeliveryResult> deliveryResults )
    {
        _deliverySpanReduction = deliverySpanReduction;
        _deliverySchedules = deliverySchedules;
        _deliveryResults = deliveryResults;
    }

    /// <summary>
    /// 복구할 배송 일정 생성
    /// </summary>
    /// <returns>배송 일정 복사본</returns>
    public Dictionary<string, DeliverySchedule> CreateSchedules ()
    {
        return new Dictionary<string, DeliverySchedule>( _deliverySchedules );
    }

    /// <summary>
    /// 복구할 배송 결과 생성
    /// </summary>
    /// <returns>배송 결과 복사본</returns>
    public Dictionary<string, DeliveryResult> CreateResults ()
    {
        return new Dictionary<string, DeliveryResult>( _deliveryResults );
    }
}
