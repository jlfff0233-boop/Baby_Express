using System.Text;
using UnityEngine;

/// <summary>
/// 업적 슬롯 표시 데이터 생성기
/// </summary>
public class AchvViewDataBuilder
{
    PurchasableDataMap _dataMap;       //해금 보상 대상 데이터

    /// <summary>
    /// 업적 슬롯 표시 데이터 생성기 생성자
    /// </summary>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    public AchvViewDataBuilder ( PurchasableDataMap dataMap )
    {
        _dataMap = dataMap;
    }

    /// <summary>
    /// 업적 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="data">업적 설정 데이터</param>
    /// <param name="state">업적 런타임 상태</param>
    /// <param name="progress">현재 누적 진행도</param>
    /// <returns>업적 슬롯 표시 데이터</returns>
    public AchvViewData Build (
        AchvData data, AchvState state, int progress )
    {
        int stageIndex = GetDisplayStageIndex( data, state );
        AchvStageData stage = data.Stages [ stageIndex ];

        bool isHidden =
            data.IsHidden && state.ReachedStageIndex < 0;

        int displayProgress =
            Mathf.Min( progress, stage.TargetValue );

        float progressRate = Mathf.Clamp01(
            ( float ) displayProgress / stage.TargetValue );

        return new AchvViewData(
            data.Id,
            data.Category,
            GetIconType( data.ProgressType ),
            isHidden ? "?" : GetDisplayName( data, stage ),
            isHidden ? "?" : GetDescription( data, stage ),
            $"{displayProgress} / {stage.TargetValue}",
            GetRewardText( stage ),
            progressRate,
            isHidden,
            state.HasUnclaimedReward,
            state.IsCompleted,
            state.IsNew,
            data.Stages.Count > 1 );
    }

    /// <summary>
    /// 업적 진행 종류에 맞는 공용 결산 아이콘 종류 조회
    /// </summary>
    /// <param name="progressType">업적 진행도 판정 종류</param>
    /// <returns>업적 슬롯에 표시할 아이콘 종류</returns>
    SettlementIconType GetIconType ( AchvProgressType progressType )
    {
        switch ( progressType )
        {
            case AchvProgressType.BusinessDayCount:
                return SettlementIconType.Business;

            case AchvProgressType.WeeklySettlementCount:
                return SettlementIconType.NextWeek;

            case AchvProgressType.EmployeeHireCount:
            case AchvProgressType.EmployeeDeliveryCount:
                return SettlementIconType.Employee;

            case AchvProgressType.DeliveryCount:
            case AchvProgressType.DeadlineDayOnTimeDeliveryCount:
            case AchvProgressType.LateDeliveryCount:
            case AchvProgressType.OppositeThemeDeliveryCount:
                return SettlementIconType.Delivery;

            case AchvProgressType.SGradeCount:
                return SettlementIconType.GoldTrophy;

            case AchvProgressType.AOrHigherGradeCount:
                return SettlementIconType.SilverTrophy;

            case AchvProgressType.AllRequirementsDeliveryCount:
                return SettlementIconType.Requirement;

            case AchvProgressType.AllWishesDeliveryCount:
                return SettlementIconType.Wish;

            case AchvProgressType.CumulativeNetProfit:
                return SettlementIconType.Profit;

            case AchvProgressType.CraftCount:
            case AchvProgressType.ActivatedThemeCraftCount:
            case AchvProgressType.CompleteThemeCraftCount:
            case AchvProgressType.PerfectCustomCraftCount:
                return SettlementIconType.Craft;

            default:
                return SettlementIconType.None;
        }
    }

    /// <summary>
    /// 슬롯에 표시할 업적 단계 인덱스 조회
    /// </summary>
    /// <param name="data">업적 설정 데이터</param>
    /// <param name="state">업적 런타임 상태</param>
    /// <returns>표시할 단계 인덱스</returns>
    int GetDisplayStageIndex ( AchvData data, AchvState state )
    {
        int nextClaimIndex = state.ClaimedStageIndex + 1;

        //미수령 보상이 있으면 가장 오래된 미수령 단계 표시
        if ( nextClaimIndex <= state.ReachedStageIndex )
            return nextClaimIndex;

        int nextStageIndex = state.ReachedStageIndex + 1;

        //진행 중이면 다음 목표 단계 표시
        if ( nextStageIndex < data.Stages.Count )
            return nextStageIndex;

        //모든 단계를 달성했으면 마지막 단계 표시
        return data.Stages.Count - 1;
    }

    /// <summary>
    /// 단계에 맞는 업적 표시 이름 조회
    /// </summary>
    string GetDisplayName (
        AchvData data, AchvStageData stage )
    {
        return string.IsNullOrWhiteSpace( stage.DisplayName )
            ? data.DisplayName
            : stage.DisplayName;
    }

    /// <summary>
    /// 단계에 맞는 업적 설명 조회
    /// </summary>
    string GetDescription (
        AchvData data, AchvStageData stage )
    {
        return string.IsNullOrWhiteSpace( stage.Description )
            ? data.Description
            : stage.Description;
    }

    /// <summary>
    /// 단계별 보상 안내 생성
    /// </summary>
    /// <param name="stage">표시할 업적 단계</param>
    /// <returns>보상 안내</returns>
    string GetRewardText ( AchvStageData stage )
    {
        var text = new StringBuilder( );

        for ( int i = 0; i < stage.Rewards.Count; i++ )
        {
            string rewardText =
                GetRewardText( stage.Rewards [ i ] );

            if ( string.IsNullOrEmpty( rewardText ) )
                continue;

            if ( text.Length > 0 )
                text.Append( '\n' );

            text.Append( rewardText );
        }

        return text.Length > 0
            ? text.ToString( )
            : "보상: 없음";
    }

    /// <summary>
    /// 보상 종류별 안내 생성
    /// </summary>
    /// <param name="reward">보상 데이터</param>
    /// <returns>보상 안내</returns>
    string GetRewardText ( AchvRewardData reward )
    {
        switch ( reward.Type )
        {
            case AchvRewardType.Gold:
                return $"보상: 골드 {reward.Amount:N0}G";

            case AchvRewardType.PartUnlock:
                return
                    $"보상: 파츠 해금 {GetTargetName( reward.TargetId )}";

            case AchvRewardType.ProductUnlock:
                return
                    $"보상: 상품 해금 {GetTargetName( reward.TargetId )}";

            case AchvRewardType.MaintenanceUnlock:
                return
                    $"보상: 정비 구매 가능 해금 " +
                    GetTargetName( reward.TargetId );

            case AchvRewardType.DecorationUnlock:
                return $"보상: 장식 해금 예약 {reward.TargetId}";

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 해금 대상의 표시 이름 조회
    /// </summary>
    /// <param name="id">해금 대상 아이디</param>
    /// <returns>해금 대상 표시 이름</returns>
    string GetTargetName ( string id )
    {
        for ( int i = 0; i < _dataMap.PurchasableDatas.Count; i++ )
        {
            PurchasableData data =
                _dataMap.PurchasableDatas [ i ];

            if ( data != null && data.Id == id )
                return data.Name;
        }

        //장식처럼 아직 실제 데이터가 없는 대상은 아이디 표시
        return id;
    }
}
