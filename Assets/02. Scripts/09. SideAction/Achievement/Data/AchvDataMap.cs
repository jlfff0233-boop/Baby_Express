using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 업적 데이터 목록 관리
/// </summary>
[CreateAssetMenu( menuName = "AchievementSettings/AchievementDataMap" )]
public class AchvDataMap : ScriptableObject
{
    [Header( "----- 업적 데이터 -----" )]
    [SerializeField] AchvData [ ] _datas;       //전체 업적 데이터
    List<AchvData> _validDatas = new List<AchvData>( );       //검증을 통과한 업적 데이터

    /// <summary>
    /// 업적 데이터 딕셔너리(업적 아이디, 업적 데이터)
    /// </summary>
    Dictionary<string, AchvData> _dataMap = new Dictionary<string, AchvData>( );

    /// <summary>
    /// 읽기 전용 사용 가능 업적 데이터
    /// </summary>
    public IReadOnlyList<AchvData> Datas => _validDatas;

    /// <summary>
    /// 업적 데이터 맵 초기화
    /// </summary>
    void OnEnable ()
    {
        BuildDataMap( );
    }

    /// <summary>
    /// 지정 아이디의 업적 데이터 조회
    /// </summary>
    /// <param name="id">조회할 업적 아이디</param>
    /// <param name="data">조회한 업적 데이터</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetData ( string id, out AchvData data )
    {
        data = null;

        if ( string.IsNullOrWhiteSpace( id ) )
            return false;

        return _dataMap.TryGetValue( id, out data );
    }

    /// <summary>
    /// 업적 아이디 조회 맵 생성
    /// </summary>
    void BuildDataMap ()
    {
        _dataMap.Clear( );
        _validDatas.Clear( );

        for ( int i = 0; i < _datas.Length; i++ )
        {
            AchvData data = _datas [ i ];

            if ( IsValidData( data ) == false )
                continue;

            if ( _dataMap.TryAdd( data.Id, data ) == false )
            {
                Debug.LogError( $"업적 데이터 아이디가 중복되었습니다: {data.Id}", data );
                continue;
            }

            _validDatas.Add( data );
        }
    }

    /// <summary>
    /// 업적 데이터 필수 설정 검증
    /// </summary>
    /// <param name="data">검증할 업적 데이터</param>
    /// <returns>사용 가능한 데이터 여부</returns>
    bool IsValidData ( AchvData data )
    {
        if ( data == null )
        {
            Debug.LogError( "업적 데이터 목록에 빈 항목이 있습니다.", this );
            return false;
        }

        if ( string.IsNullOrWhiteSpace( data.Id ) )
        {
            Debug.LogError( "아이디가 비어 있는 업적 데이터가 있습니다.", data );
            return false;
        }

        if ( data.Stages.Count == 0 )
        {
            Debug.LogError( $"업적 단계가 비어 있습니다: {data.Id}", data );
            return false;
        }

        return AreValidStages( data );
    }

    /// <summary>
    /// 업적 단계 목표값과 보상 설정 검증
    /// </summary>
    /// <param name="data">검증할 업적 데이터</param>
    /// <returns>사용 가능한 단계 설정 여부</returns>
    bool AreValidStages ( AchvData data )
    {
        int previousTarget = 0;

        for ( int i = 0; i < data.Stages.Count; i++ )
        {
            AchvStageData stage = data.Stages [ i ];

            if ( stage == null )
            {
                Debug.LogError( $"업적 단계가 비어 있습니다: {data.Id}, 단계 {i + 1}", data );
                return false;
            }

            if ( stage.TargetValue <= previousTarget )
            {
                Debug.LogError(
                    $"업적 목표값은 이전 단계보다 커야 합니다: " +
                    $"{data.Id}, 단계 {i + 1}",
                    data );
                return false;
            }

            if ( AreValidRewards( data, stage, i ) == false )
                return false;

            previousTarget = stage.TargetValue;
        }

        return true;
    }

    /// <summary>
    /// 업적 단계 보상 설정 검증
    /// </summary>
    /// <param name="data">검증할 업적 데이터</param>
    /// <param name="stage">검증할 단계 데이터</param>
    /// <param name="stageIndex">검증할 단계 인덱스</param>
    /// <returns>사용 가능한 보상 설정 여부</returns>
    bool AreValidRewards (
        AchvData data, AchvStageData stage, int stageIndex )
    {
        for ( int i = 0; i < stage.Rewards.Count; i++ )
        {
            AchvRewardData reward = stage.Rewards [ i ];

            if ( reward == null )
            {
                Debug.LogError(
                    $"업적 보상이 비어 있습니다: " +
                    $"{data.Id}, 단계 {stageIndex + 1}",
                    data );
                return false;
            }

            if ( IsValidReward( reward ) )
                continue;

            Debug.LogError(
                $"업적 보상 설정이 올바르지 않습니다: " +
                $"{data.Id}, 단계 {stageIndex + 1}, 보상 {i + 1}",
                data );
            return false;
        }

        return true;
    }

    /// <summary>
    /// 업적 보상 종류별 필수값 검증
    /// </summary>
    /// <param name="reward">검증할 보상 데이터</param>
    /// <returns>사용 가능한 보상 여부</returns>
    bool IsValidReward ( AchvRewardData reward )
    {
        switch ( reward.Type )
        {
            case AchvRewardType.None:
                return true;

            case AchvRewardType.Gold:
                return reward.Amount > 0f;

            case AchvRewardType.PartUnlock:
            case AchvRewardType.ProductUnlock:
            case AchvRewardType.MaintenanceUnlock:
            case AchvRewardType.DecorationUnlock:
                return string.IsNullOrWhiteSpace( reward.TargetId ) == false;

            default:
                return false;
        }
    }
}
