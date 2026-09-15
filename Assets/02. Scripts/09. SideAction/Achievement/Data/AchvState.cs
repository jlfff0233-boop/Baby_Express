/// <summary>
/// 업적 달성과 보상 수령 런타임 상태
/// </summary>
public class AchvState
{
    /// <summary>
    /// 업적 아이디
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 마지막으로 달성한 단계 인덱스
    /// </summary>
    public int ReachedStageIndex { get; private set; }

    /// <summary>
    /// 마지막으로 보상을 수령한 단계 인덱스
    /// </summary>
    public int ClaimedStageIndex { get; private set; }

    /// <summary>
    /// 최종 단계 달성 여부
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// 신규 달성 여부
    /// </summary>
    public bool IsNew { get; private set; }

    /// <summary>
    /// 미수령 보상 존재 여부
    /// </summary>
    public bool HasUnclaimedReward =>
        ClaimedStageIndex < ReachedStageIndex;


    /// <summary>
    /// 업적 런타임 상태 생성
    /// </summary>
    /// <param name="id">업적 아이디</param>
    public AchvState ( string id )
    {
        Id = id;
        ReachedStageIndex = -1;
        ClaimedStageIndex = -1;
    }

    /// <summary>
    /// 달성한 마지막 단계 갱신
    /// </summary>
    /// <param name="stageIndex">새로 달성한 마지막 단계 인덱스</param>
    /// <param name="isCompleted">최종 단계 달성 여부</param>
    /// <returns>단계 갱신 성공 여부</returns>
    public bool ReachStage ( int stageIndex, bool isCompleted )
    {
        if ( stageIndex <= ReachedStageIndex )
            return false;

        ReachedStageIndex = stageIndex;
        IsCompleted = isCompleted;
        IsNew = true;
        return true;
    }

    /// <summary>
    /// 지정 단계의 보상 수령 완료 처리
    /// </summary>
    /// <param name="stageIndex">수령 완료할 단계 인덱스</param>
    /// <returns>수령 상태 갱신 성공 여부</returns>
    public bool MarkStageClaimed ( int stageIndex )
    {
        //단계 보상은 달성 순서대로만 수령
        if ( stageIndex != ClaimedStageIndex + 1 ||
            stageIndex > ReachedStageIndex )
            return false;

        ClaimedStageIndex = stageIndex;
        return true;
    }

    /// <summary>
    /// 신규 달성 확인 처리
    /// </summary>
    public void MarkViewed ()
    {
        IsNew = false;
    }

    /// <summary>
    /// 저장된 업적 런타임 상태 복구
    /// </summary>
    /// <param name="reachedStageIndex">마지막 달성 단계 인덱스</param>
    /// <param name="claimedStageIndex">마지막 보상 수령 단계 인덱스</param>
    /// <param name="isCompleted">최종 단계 달성 여부</param>
    /// <param name="isNew">신규 달성 여부</param>
    internal void Restore (
        int reachedStageIndex, int claimedStageIndex,
        bool isCompleted, bool isNew )
    {
        ReachedStageIndex = reachedStageIndex;
        ClaimedStageIndex = claimedStageIndex;
        IsCompleted = isCompleted;
        IsNew = isNew;
    }
}
