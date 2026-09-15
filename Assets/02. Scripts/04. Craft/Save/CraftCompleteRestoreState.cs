using System.Collections.Generic;

/// <summary>
/// 제작 결과 복구 상태 - 검증을 마친 주문별 제작 결과 보관
/// </summary>
public class CraftCompleteRestoreState
{
    Dictionary<string, CraftResult> _craftResults;       //주문별 확정 제작 결과

    /// <summary>
    /// 제작 결과 복구 상태 생성
    /// </summary>
    /// <param name="craftResults">주문 제작 결과</param>
    public CraftCompleteRestoreState (
        Dictionary<string, CraftResult> craftResults )
    {
        _craftResults = craftResults;
    }

    /// <summary>
    /// 복구할 제작 결과 생성
    /// </summary>
    /// <returns>제작 결과 복사본</returns>
    public Dictionary<string, CraftResult> CreateResults ()
    {
        return new Dictionary<string, CraftResult>( _craftResults );
    }
}
