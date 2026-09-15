using System.Collections.Generic;

/// <summary>
/// 업적 단계 보상 검증과 수령 처리
/// </summary>
public class AchvRewardProcessor
{
    AchvModel _achvModel;       //업적 상태 모델
    PlayStateModel _playStateModel;       //자금과 해금 상태 모델
    DailyRecordModel _dailyRecordModel;       //현재 영업일 기록 모델
    PurchasableDataMap _purchasableDataMap;       //상품 데이터 원본

    #region ----- 초기화 -----

    /// <summary>
    /// 업적 보상 처리기 생성
    /// </summary>
    /// <param name="achvModel">업적 상태 모델</param>
    /// <param name="playStateModel">자금과 해금 상태 모델</param>
    /// <param name="dailyRecordModel">현재 영업일 기록 모델</param>
    /// <param name="purchasableDataMap">상품 데이터 원본</param>
    public AchvRewardProcessor (
        AchvModel achvModel,
        PlayStateModel playStateModel,
        DailyRecordModel dailyRecordModel,
        PurchasableDataMap purchasableDataMap )
    {
        _achvModel = achvModel;
        _playStateModel = playStateModel;
        _dailyRecordModel = dailyRecordModel;
        _purchasableDataMap = purchasableDataMap;
    }

    #endregion

    #region ----- 보상 수령 -----

    /// <summary>
    /// 지정 업적의 다음 미수령 단계 보상 수령
    /// </summary>
    /// <param name="id">업적 아이디</param>
    /// <returns>보상 수령 처리 결과</returns>
    public AchvRewardResult Claim ( string id )
    {
        //업적 상태 조회
        if ( _achvModel.TryGetState( id, out _ ) == false )
            return AchvRewardResult.NotFound;

        //업적 다음 단계 조회
        if ( _achvModel.TryGetNextClaimStage(
            id, out AchvStageData stage ) == false )
            return AchvRewardResult.NoUnclaimedReward;

        //보상 수령 가능 여부 확인
        if ( IsValidStageReward( stage, out float goldAmount ) == false )
            return AchvRewardResult.InvalidReward;

        //이전 상태 딕셔너리 생성
        var prevUnlockStates = new Dictionary<string, bool>( );

        //보상 대상의 기존 해금 상태 보관
        CaptureUnlockStates( stage, prevUnlockStates );

        float prevBudget = _playStateModel.Budget;
        bool isIncomeRecorded = false;

        //해당하는 단계의 모든 해금 보상 적용
        if ( ApplyUnlockRewards( stage ) == false )
        {
            //실패 시 보상 적용 전 상태로 원상 복구
            return RollbackOrReturn(
                prevBudget, prevUnlockStates, goldAmount, isIncomeRecorded,
                AchvRewardResult.RewardApplyFailed );
        }

        //골드 보상을 한 번에 지급하고 현재 영업일 수입으로 기록
        if ( goldAmount > 0f )
        {
            if ( _playStateModel.AddBudget( goldAmount ) == false )
            {
                //실패 시 보상 적용 전 상태로 원상 복구
                return RollbackOrReturn(
                    prevBudget, prevUnlockStates, goldAmount, isIncomeRecorded,
                    AchvRewardResult.RewardApplyFailed );
            }

            //보상을 기타 수입으로 기록
            _dailyRecordModel.RecordOtherIncome( goldAmount );
            isIncomeRecorded = true;
        }

        //보상 적용 성공 후에만 업적 수령 단계를 갱신
        if ( _achvModel.MarkNextStageClaimed( id ) == false )
        {
            //실패 시 보상 적용 전 상태로 원상 복구
            return RollbackOrReturn(
                prevBudget, prevUnlockStates, goldAmount, isIncomeRecorded,
                AchvRewardResult.StateUpdateFailed );
        }

        return AchvRewardResult.Success;
    }

    #endregion

    #region ----- 보상 검증 -----

    /// <summary>
    /// 단계의 전체 보상 데이터와 골드 추가 가능 여부 검증
    /// </summary>
    /// <param name="stage">검증할 업적 단계</param>
    /// <param name="goldAmount">단계의 골드 보상 합계</param>
    /// <returns>보상 적용 가능 여부</returns>
    bool IsValidStageReward ( AchvStageData stage, out float goldAmount )
    {
        goldAmount = 0f;
        bool hasReward = false;

        for ( int i = 0; i < stage.Rewards.Count; i++ )
        {
            AchvRewardData reward = stage.Rewards [ i ];

            switch ( reward.Type )
            {
                case AchvRewardType.None:
                    continue;

                case AchvRewardType.Gold:       //골드
                    if ( reward.Amount <= 0f )
                        return false;

                    goldAmount += reward.Amount;
                    hasReward = true;
                    break;

                case AchvRewardType.PartUnlock:     //파츠 해금
                    if ( TryGetPurchasableData(
                        reward.TargetId,
                        out PurchasableData partData ) == false ||
                        partData is not PartsData )
                        return false;

                    hasReward = true;
                    break;

                case AchvRewardType.ProductUnlock:      //상품 해금
                    if ( TryGetPurchasableData( reward.TargetId, out _ ) == false )
                        return false;

                    hasReward = true;
                    break;

                case AchvRewardType.MaintenanceUnlock:      //정비 구매 가능 상태 해금
                    if ( TryGetPurchasableData(
                        reward.TargetId,
                        out PurchasableData maintenanceData ) == false ||
                        maintenanceData is not MaintenanceData )
                        return false;

                    hasReward = true;
                    break;

                case AchvRewardType.DecorationUnlock:       //장식 해금 예약
                    if ( string.IsNullOrWhiteSpace( reward.TargetId ) )
                        return false;

                    hasReward = true;
                    break;

                default:
                    return false;
            }
        }

        if ( hasReward == false )
            return false;

        return goldAmount <= 0f ||
            _playStateModel.CanAddBudget( goldAmount );
    }

    #endregion

    #region ----- 해금 보상 처리 -----

    /// <summary>
    /// 보상 대상의 기존 해금 상태 보관
    /// </summary>
    /// <param name="stage">수령할 업적 단계</param>
    /// <param name="prevStates">대상별 기존 해금 상태</param>
    void CaptureUnlockStates (
        AchvStageData stage, Dictionary<string, bool> prevStates )
    {
        for ( int i = 0; i < stage.Rewards.Count; i++ )
        {
            //보상 가져오기
            AchvRewardData reward = stage.Rewards [ i ];

            //해금형 보상 여부 확인
            if ( IsUnlockReward( reward.Type ) == false )
                continue;

            //이전 상태 추가
            prevStates.TryAdd(
                reward.TargetId, _playStateModel.IsUnlocked( reward.TargetId ) );
        }
    }

    /// <summary>
    /// 단계에 포함된 모든 해금 보상 적용
    /// </summary>
    /// <param name="stage">수령할 업적 단계</param>
    /// <returns>해금 적용 성공 여부</returns>
    bool ApplyUnlockRewards ( AchvStageData stage )
    {
        for ( int i = 0; i < stage.Rewards.Count; i++ )
        {
            //보상 가져오기
            AchvRewardData reward = stage.Rewards [ i ];

            //해금형 보상 여부 확인
            if ( IsUnlockReward( reward.Type ) == false )
                continue;

            //보상 해금 상태 설정
            if ( _playStateModel.SetUnlocked( reward.TargetId, true ) == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 업적 보상 종류의 해금 상태 사용 여부 확인
    /// </summary>
    /// <param name="type">업적 보상 종류</param>
    /// <returns>해금 보상 여부</returns>
    bool IsUnlockReward ( AchvRewardType type )
    {
        switch ( type )
        {
            case AchvRewardType.PartUnlock:
            case AchvRewardType.ProductUnlock:
            case AchvRewardType.MaintenanceUnlock:
            case AchvRewardType.DecorationUnlock:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 상품 아이디에 해당하는 설정 데이터 조회
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="data">조회한 상품 데이터</param>
    /// <returns>조회 성공 여부</returns>
    bool TryGetPurchasableData ( string id, out PurchasableData data )
    {
        data = null;

        //상품 목록 순회
        for ( int i = 0; i < _purchasableDataMap.PurchasableDatas.Count; i++ )
        {
            //상품 가져오기
            PurchasableData current = _purchasableDataMap.PurchasableDatas [ i ];

            if ( current != null && current.Id == id )
            {
                //데이터 설정
                data = current;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region ----- 원상 복구 -----

    /// <summary>
    /// 보상 적용 전 상태로 원상 복구하고 최종 결과 반환
    /// </summary>
    /// <param name="prevBudget">수령 전 자금</param>
    /// <param name="prevUnlockStates">대상별 기존 해금 상태</param>
    /// <param name="goldAmount">기록한 골드 보상</param>
    /// <param name="isIncomeRecorded">기타 수입 기록 여부</param>
    /// <param name="failedResult">원래 실패 결과</param>
    /// <returns>실패 또는 원상 복구 실패 결과</returns>
    AchvRewardResult RollbackOrReturn (
        float prevBudget,
        IReadOnlyDictionary<string, bool> prevUnlockStates,
        float goldAmount, bool isIncomeRecorded,
        AchvRewardResult failedResult )
    {
        bool isRolledBack = true;

        //보상 수입 기록을 먼저 제거
        if ( isIncomeRecorded )
        {
            isRolledBack &= _dailyRecordModel.RollbackOtherIncome( goldAmount );
        }

        //수령 전 자금으로 복구
        isRolledBack &= _playStateModel.SetBudget( prevBudget );

        //각 대상의 기존 해금 상태 복구
        foreach ( var pair in prevUnlockStates )
        {
            isRolledBack &= _playStateModel.SetUnlocked( pair.Key, pair.Value );
        }

        return isRolledBack
            ? failedResult
            : AchvRewardResult.RollbackFailed;
    }

    #endregion
}
