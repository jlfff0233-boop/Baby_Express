using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정비 모델 - 정비 항목의 소유, 단계, 적용 상태 관리
/// </summary>
public class MaintenanceModel
{
    /// <summary>
    /// 정비 상태 딕셔너리(정비 아이디, 정비 상태)
    /// </summary>
    Dictionary<string, MaintenanceState> _states =
        new Dictionary<string, MaintenanceState>( );

    /// <summary>
    /// 정비 상태 목록
    /// </summary>
    public IReadOnlyCollection<MaintenanceState> States => _states.Values;

    /// <summary>
    /// 정비 상태 변경 이벤트
    /// </summary>
    public event Action<string> OnMaintenanceChanged;

    #region ----- 초기화 -----

    /// <summary>
    /// 정비 모델 생성
    /// </summary>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    public MaintenanceModel ( PurchasableDataMap dataMap )
    {
        Init( dataMap );
    }

    /// <summary>
    /// 정비 상태 초기화
    /// </summary>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    void Init ( PurchasableDataMap dataMap )
    {
        _states.Clear( );

        //등록된 정비 상품으로 초기 상태 생성
        for ( int i = 0; i < dataMap.PurchasableDatas.Count; i++ )
        {
            if ( dataMap.PurchasableDatas [ i ] is not MaintenanceData data )
                continue;

            //아이디가 없거나 단계 데이터가 없으면 제외
            if ( string.IsNullOrWhiteSpace( data.Id ) || data.MaxLevel < 1 )
                continue;

            //같은 아이디의 정비가 있으면 제외
            if ( _states.TryAdd(
                data.Id, new MaintenanceState( data ) ) == false )
            {
                Debug.LogWarning(
                    $"중복된 정비 아이디입니다: {data.Id}" );
            }
        }
    }

    #endregion

    #region ----- 정비 조회 -----

    /// <summary>
    /// 정비 상태 조회
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="state">조회한 정비 상태</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetState ( string id, out MaintenanceState state )
    {
        //기본 값 설정
        state = null;

        //아이디 확인
        if ( string.IsNullOrWhiteSpace( id ) ) return false;

        //상태 반환
        return _states.TryGetValue( id, out state );
    }

    /// <summary>
    /// 정비 종류별 상태 목록 조회
    /// </summary>
    /// <param name="type">정비 종류</param>
    /// <returns>정비 상태 목록</returns>
    public IReadOnlyList<MaintenanceState> GetStates (
        MaintenanceType type )
    {
        var states = new List<MaintenanceState>( );

        //정비 상태 딕셔너리 조회
        foreach ( MaintenanceState state in _states.Values )
        {
            //정비 종류가 같으면 목록에 추가
            if ( state.Data.Type == type )
                states.Add( state );
        }

        //상태 목록 반환
        return states;
    }

    /// <summary>
    /// 다음 정비 단계 데이터 조회
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="levelData">다음 단계 데이터</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetNextLevelData (
        string id, out MaintenanceLevelData levelData )
    {
        //기본값 설정
        levelData = null;

        //정비 상태 가져오기
        if ( GetState( id, out MaintenanceState state ) == false )
            return false;

        //이미 최대 단계면 종료
        if ( state.IsMaxLevel ) return false;

        //조회 성공 여부, 레벨 데이터 반환
        return state.Data.GetLevelData( state.NextLevel, out levelData );
    }

    /// <summary>
    /// 현재 적용된 성질 테마 연구 보너스 반환
    /// </summary>
    /// <param name="theme">조회할 성질 테마</param>
    /// <returns>현재 적용 단계의 최종 연구 보너스</returns>
    public float GetThemeScoreBonus ( PartTheme theme )
    {
        //성질 테마가 아니면 연구 보너스를 적용하지 않음
        if ( theme.IsTraitTheme( ) == false )
            return 0f;

        float bonus = 0f;

        //정비 상태 딕셔너리 순회
        foreach ( MaintenanceState state in _states.Values )
        {
            //실제 적용된 단계가 없는 정비는 제외
            if ( state.AppliedLevel < 1 ||
                state.Data.GetLevelData( state.AppliedLevel,
                out MaintenanceLevelData levelData ) == false )
                continue;

            for ( int i = 0; i < levelData.Effects.Count; i++ )
            {
                //효과 가져오기
                EffectData effect = levelData.Effects [ i ];

                //효과가 없거나 테마 점수 보너스 타입이 아니면
                if ( effect == null || effect.Type != MaintenanceEffectType.ThemeScoreBonus )
                    continue;

                //현재 조회 테마와 같은 효과만 확인
                if ( Enum.TryParse(
                    effect.TargetId, true,
                    out PartTheme effectTheme ) == false ||
                    effectTheme != theme )
                    continue;

                //같은 테마가 중복 등록돼도 현재 단계 최종값만 사용
                bonus = Mathf.Max( bonus, effect.Value );
            }
        }

        return bonus;
    }

    /// <summary>
    /// 파츠를 해금하는 연구와 요구 단계 조회
    /// </summary>
    /// <param name="partId">해금 대상 파츠 아이디</param>
    /// <param name="researchState">파츠 해금 연구 상태</param>
    /// <param name="requiredLevel">파츠 해금 요구 단계</param>
    /// <returns>파츠 해금 연구 조회 성공 여부</returns>
    public bool TryGetPartUnlockRequirement (
        string partId,
        out MaintenanceState researchState,
        out int requiredLevel )
    {
        //기본값 설정
        researchState = null;
        requiredLevel = 0;

        //아이디가 이상하면 종료
        if ( string.IsNullOrWhiteSpace( partId ) ) return false;

        foreach ( MaintenanceState state in _states.Values )
        {
            //연구 타입이 아니면 이번 순회 건너뛰기
            if ( state.Data.Type != MaintenanceType.Research ) continue;

            //최대 레벨만큼 반복
            for ( int level = 1; level <= state.Data.MaxLevel; level++ )
            {
                //정비 레벨 가져오기
                if ( state.Data.GetLevelData(
                    level, out MaintenanceLevelData levelData ) == false ||
                    levelData.Effects == null )
                    continue;

                //효과 개수만큼 반복
                for ( int i = 0; i < levelData.Effects.Count; i++ )
                {
                    //효과 가져오기
                    EffectData effect = levelData.Effects [ i ];

                    //효과가 null이거나 타입이 틀리거나 아이디가 정확히 맞지 않으면 이번 순회 건너뛰기
                    if ( effect == null ||
                        effect.Type != MaintenanceEffectType.PartUnlock ||
                        StringComparer.Ordinal.Equals( effect.TargetId, partId ) == false )
                        continue;

                    //연구 설정
                    researchState = state;
                    requiredLevel = level;
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region ----- 정비 단계 -----

    /// <summary>
    /// 최초 정비 구매 가능 여부 확인
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult GetAcquireResult ( string id )
    {
        //정비 상태 조회
        if ( GetState( id, out MaintenanceState state ) == false )
            return MaintenanceResult.NotFound;

        //1단계가 없는 정비 데이터 차단
        if ( state.Data.GetLevelData( 1, out _ ) == false )
            return MaintenanceResult.InvalidData;

        //이미 구매한 정비 차단
        if ( state.IsOwned )
            return MaintenanceResult.AlreadyOwned;

        return MaintenanceResult.Success;
    }

    /// <summary>
    /// 최초 정비 구매 등록
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult Acquire ( string id )
    {
        //최초 구매 가능 여부 확인
        MaintenanceResult result = GetAcquireResult( id );
        if ( result != MaintenanceResult.Success ) return result;

        //정비 구매 단계를 1단계로 변경
        MaintenanceState state = _states [ id ];
        if ( state.Upgrade( ) == false )
            return MaintenanceResult.InvalidData;

        //정비 상태 변경 전달
        OnMaintenanceChanged?.Invoke( id );
        return MaintenanceResult.Success;
    }

    /// <summary>
    /// 적용 대기 중인 마지막 구매 단계 복구
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult RollbackUpgrade ( string id )
    {
        //정비 상태 조회
        if ( GetState( id, out MaintenanceState state ) == false )
            return MaintenanceResult.NotFound;

        //마지막 구매 단계 복구
        if ( state.RollbackUpgrade( ) == false )
            return MaintenanceResult.InvalidData;

        //정비 상태 변경 전달
        OnMaintenanceChanged?.Invoke( id );
        return MaintenanceResult.Success;
    }

    /// <summary>
    /// 마지막 적용 단계 복구
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult RollbackAppliedLevel ( string id )
    {
        //정비 상태 조회
        if ( GetState( id, out MaintenanceState state ) == false )
            return MaintenanceResult.NotFound;

        //마지막 적용 단계 복구
        if ( state.RollbackAppliedLevel( ) == false )
            return MaintenanceResult.InvalidData;

        //정비 상태 변경 전달
        OnMaintenanceChanged?.Invoke( id );
        return MaintenanceResult.Success;
    }

    /// <summary>
    /// 다음 정비 단계 구매 가능 여부 확인
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult GetUpgradeResult (
        string id, int totalDay, WeeklyRating weeklyRating )
    {
        //정비 상태가 없으면 종료
        if ( GetState( id, out MaintenanceState state ) == false )
            return MaintenanceResult.NotFound;

        //업그레이드한 적 없는 정비 종료
        if ( state.IsOwned == false )
            return MaintenanceResult.NotOwned;

        //이전 구매 단계의 효과 적용 전 추가 구매 차단
        if ( state.HasPendingEffect )
            return MaintenanceResult.EffectPending;

        //최대 단계 정비라면 종료
        if ( state.IsMaxLevel )
            return MaintenanceResult.MaxLevel;

        //다음 단계 데이터 조회
        if ( state.Data.GetLevelData(
            state.NextLevel, out MaintenanceLevelData levelData ) == false )
            return MaintenanceResult.InvalidData;

        //다음 단계 선행 조건 확인
        if ( MeetsRequirements(
            levelData.Requirements, totalDay, weeklyRating ) == false )
            return MaintenanceResult.RequirementNotMet;

        return MaintenanceResult.Success;
    }

    /// <summary>
    /// 다음 정비 단계 구매 등록
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult Upgrade (
        string id, int totalDay, WeeklyRating weeklyRating )
    {
        //정비 결과 가져오기
        MaintenanceResult result =
            GetUpgradeResult( id, totalDay, weeklyRating );

        //성공이 아니면
        if ( result != MaintenanceResult.Success ) return result;

        MaintenanceState state = _states [ id ];

        if ( state.Upgrade( ) == false )
            return MaintenanceResult.MaxLevel;

        OnMaintenanceChanged?.Invoke( id );
        return MaintenanceResult.Success;
    }

    /// <summary>
    /// 구매한 정비 단계 효과 적용 완료
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 처리 결과</returns>
    public MaintenanceResult ApplyPurchasedLevel ( string id )
    {
        if ( GetState( id, out MaintenanceState state ) == false )
            return MaintenanceResult.NotFound;

        if ( state.IsOwned == false )
            return MaintenanceResult.NotOwned;

        if ( state.ApplyPurchasedLevel( ) == false )
            return MaintenanceResult.NoPendingEffect;

        OnMaintenanceChanged?.Invoke( id );
        return MaintenanceResult.Success;
    }

    #endregion

    #region ----- 선행 조건 -----

    /// <summary>
    /// 정비 선행 조건 달성 여부 확인
    /// </summary>
    /// <param name="requirements">정비 선행 조건 목록</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <returns>전체 선행 조건 달성 여부</returns>
    bool MeetsRequirements (
        IReadOnlyList<MaintenanceRequirementData> requirements,
        int totalDay, WeeklyRating weeklyRating )
    {
        //선행 조건이 없는 정비라면
        if ( requirements == null ) return true;

        for ( int i = 0; i < requirements.Count; i++ )
        {
            //선행 조건 달성 확인(요구 수치, 누적 영업일, 주간 평가)
            if ( MeetsRequirement(
                requirements [ i ], totalDay, weeklyRating ) == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 개별 정비 선행 조건 달성 여부 확인
    /// </summary>
    /// <param name="requirement">확인할 선행 조건</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <returns>선행 조건 달성 여부</returns>
    bool MeetsRequirement (
        MaintenanceRequirementData requirement,
        int totalDay, WeeklyRating weeklyRating )
    {
        if ( requirement == null ) return false;

        //선행 조건 타입 확인
        switch ( requirement.Type )
        {
            case MaintenanceRequirementType.TotalDay:
                //누적 영업일이 요구 수치보다 크거나 같으면 달성
                return totalDay >= requirement.RequiredValue;

            case MaintenanceRequirementType.WeeklyRating:
                //주간 평가 값이 요구 수치보다 크거나 같으면 달성
                return GetRatingRank( weeklyRating ) >=
                    GetRatingRank( requirement.RequiredRating );

            case MaintenanceRequirementType.MaintenanceLevel:
                //현재 레벨이 요구 레벨보다 크거나 같으면 달성
                return GetState( requirement.RequiredMaintenanceId,
                    out MaintenanceState requiredState ) &&
                    requiredState.AppliedLevel >= requirement.RequiredLevel;

            default:
                return false;
        }
    }

    /// <summary>
    /// 주간 영업 평가 비교값 반환
    /// </summary>
    /// <param name="rating">주간 영업 평가</param>
    /// <returns>평가 비교값</returns>
    int GetRatingRank ( WeeklyRating rating )
    {
        switch ( rating )
        {
            case WeeklyRating.Best:
                return 5;

            case WeeklyRating.Good:
                return 4;

            case WeeklyRating.Normal:
                return 3;

            case WeeklyRating.Caution:
                return 2;

            case WeeklyRating.Danger:
                return 1;

            default:
                return 0;
        }
    }

    #endregion

    #region ----- 저장 복구 -----

    /// <summary>
    /// 정비 모델 세이브 데이터 생성
    /// </summary>
    /// <returns>정비 모델 세이브 데이터</returns>
    public MaintenanceSaveData CreateSaveData ()
    {
        return new MaintenanceSaveData( States );
    }

    /// <summary>
    /// 정비 저장 데이터 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveData">복구할 정비 데이터</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore ( MaintenanceSaveData saveData )
    {
        //데이터 확인
        if ( saveData?.States == null )
        {
            return false;
        }

        var ids = new HashSet<string>( );

        //정비 상태 세이브 데이터 가져오기
        foreach ( MaintenanceStateSaveData stateData in saveData.States )
        {
            //저장된 정비만 검증하고 신규 정비는 초기 상태로 유지
            if ( stateData == null ||
                string.IsNullOrWhiteSpace( stateData.MaintenanceId ) == true ||
                ids.Add( stateData.MaintenanceId ) == false ||
                GetState( stateData.MaintenanceId,
                    out MaintenanceState state ) == false ||
                stateData.PurchasedLevel < 0 ||
                stateData.PurchasedLevel > state.Data.MaxLevel ||
                stateData.AppliedLevel < 0 ||
                stateData.AppliedLevel > stateData.PurchasedLevel )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 저장 데이터로 정비 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 정비 데이터</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore ( MaintenanceSaveData saveData )
    {
        //현재 상태를 변경하기 전에 전체 데이터 검사
        if ( CanRestore( saveData ) == false )
            return false;

        foreach ( MaintenanceStateSaveData stateData in saveData.States )
        {
            //정비 상태 가져오기
            GetState(
                stateData.MaintenanceId, out MaintenanceState state );

            //복구
            state.Restore(
                stateData.PurchasedLevel, stateData.AppliedLevel );
        }

        return true;
    }

    #endregion
}
