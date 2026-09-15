using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 정비 상태를 화면 표시 데이터로 변환
/// </summary>
public class MaintenanceViewDataBuilder
{
    MaintenanceModel _maintenanceModel;       //정비 상태 모델
    MaintenanceUpgradeModel _maintenanceUpgradeModel;       //정비 가능 여부 확인 모델
    ShopModel _shopModel;       //파츠 표시 이름 조회용 상점 모델
    int _totalDay;       //현재 누적 영업일
    WeeklyRating _weeklyRating;       //최근 주간 영업 평가

    #region ----- 초기화 -----

    /// <summary>
    /// 정비 표시 데이터 생성기 생성
    /// </summary>
    /// <param name="maintenanceModel">정비 상태 모델</param>
    /// <param name="maintenanceUpgradeModel">정비 가능 여부 확인 모델</param>
    /// <param name="shopModel">상점 모델</param>
    public MaintenanceViewDataBuilder (
        MaintenanceModel maintenanceModel,
        MaintenanceUpgradeModel maintenanceUpgradeModel,
        ShopModel shopModel )
    {
        _maintenanceModel = maintenanceModel;
        _maintenanceUpgradeModel = maintenanceUpgradeModel;
        _shopModel = shopModel;
    }

    /// <summary>
    /// 표시 데이터 생성 기준 설정
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    void SetContext ( int totalDay, WeeklyRating weeklyRating )
    {
        _totalDay = totalDay;
        _weeklyRating = weeklyRating;
    }

    #endregion

    #region ----- 표시 데이터 생성 -----

    /// <summary>
    /// 정비 슬롯 표시 데이터 목록 생성
    /// </summary>
    /// <param name="states">정비 상태 목록</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <returns>정렬된 정비 슬롯 표시 데이터 목록</returns>
    public IReadOnlyList<MaintenanceSlotViewData> CreateSlotViewDatas (
        IReadOnlyList<MaintenanceState> states,
        int totalDay, WeeklyRating weeklyRating )
    {
        SetContext( totalDay, weeklyRating );

        var sortedStates = new List<MaintenanceState>( states );
        sortedStates.Sort( CompareMaintenance );

        var viewDatas =
            new List<MaintenanceSlotViewData>( sortedStates.Count );

        for ( int i = 0; i < sortedStates.Count; i++ )
            viewDatas.Add( CreateSlotViewData( sortedStates [ i ] ) );

        return viewDatas;
    }

    /// <summary>
    /// 정비 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>정비 슬롯 표시 데이터</returns>
    MaintenanceSlotViewData CreateSlotViewData (
        MaintenanceState state )
    {
        MaintenanceResult result = GetNextResult( state );
        bool isLocked =
            result == MaintenanceResult.RequirementNotMet ||
            result == MaintenanceResult.InvalidData;

        string title =
            $"{state.Data.Name}(Lv.{state.PurchasedLevel}/{state.Data.MaxLevel})";

        string info =
            $"{GetDescriptionText( state )}\n" +
            $"{GetSlotStateText( state )}\n" +
            $"{GetSlotRequirementText( state )}";

        return new MaintenanceSlotViewData(
            state.Data.Id, state.Data.Icon, title, info,
            isLocked, state.IsMaxLevel );
    }

    /// <summary>
    /// 정비 상세 표시 데이터 생성
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <returns>정비 상세 표시 데이터</returns>
    public MaintenanceDetailViewData CreateDetailViewData (
        MaintenanceState state,
        int totalDay, WeeklyRating weeklyRating )
    {
        SetContext( totalDay, weeklyRating );
        MaintenanceLevelData nextLevelData =
            GetDisplayedNextLevelData( state );

        string info =
            $"{state.Data.Name}\n\n" +
            $"카테고리\n" +
            $"{GetProductTypeText( state.Data.ProductType )}\n\n" +
            $"설명\n" +
            $"{GetDescriptionText( state )}";

        MaintenanceResult result = GetNextResult( state );

        bool canUpgrade =
            state.IsOwned && result == MaintenanceResult.Success;

        return new MaintenanceDetailViewData(
            state.Data.Id, state.Data.Icon, info,
            GetCurrentEffectText( state ),
            GetNextEffectText( state, nextLevelData ),
            GetCostText( state, nextLevelData ),
            GetDetailRequirementText( state, nextLevelData ),
            GetApplyText( state.Data.ApplyType ),
            canUpgrade );
    }

    /// <summary>
    /// 정비 전체 현황 표시 데이터 생성
    /// </summary>
    /// <param name="totalDay">현재 누적 영업일</param>
    /// <param name="weeklyRating">최근 주간 영업 평가</param>
    /// <param name="employeeSummary">고용 현황</param>
    /// <returns>정비 전체 현황 표시 데이터</returns>
    public MaintenanceManagementViewData CreateManagementViewData (
        int totalDay, WeeklyRating weeklyRating, string employeeSummary )
    {
        SetContext( totalDay, weeklyRating );

        //시설, 연구, 편의 순서로 정비 항목 정렬
        var states =
            new List<MaintenanceState>( _maintenanceModel.States );
        states.Sort( CompareManagementMaintenance );

        var viewDatas =
            new List<MaintenanceEffectSlotViewData>( states.Count + 1 );

        //정비 항목 하나당 전체 탭 슬롯 하나 생성
        for ( int i = 0; i < states.Count; i++ )
            viewDatas.Add(
                CreateManagementSlotViewData( states [ i ] ) );

        //고용 현황은 마지막 독립 슬롯으로 표시
        viewDatas.Add(
            new MaintenanceEffectSlotViewData(
                MaintenanceEffectSlotCategory.Employee,
                "고용 현황", employeeSummary ) );

        return new MaintenanceManagementViewData( viewDatas );
    }

    #endregion

    #region ----- 전체 현황 슬롯 -----

    /// <summary>
    /// 개별 정비 항목의 전체 탭 슬롯 데이터 생성
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>정비 효과 슬롯 표시 데이터</returns>
    MaintenanceEffectSlotViewData CreateManagementSlotViewData (
        MaintenanceState state )
    {
        string title =
            $"{state.Data.Name}(Lv.{state.PurchasedLevel}/{state.Data.MaxLevel})";

        return new MaintenanceEffectSlotViewData(
            GetManagementCategory( state.Data.Type ),
            title, GetManagementInfoText( state ) );
    }

    /// <summary>
    /// 개별 정비 항목의 현재 효과와 다음 상태 문구 생성
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>효과와 진행 상태 문구</returns>
    string GetManagementInfoText ( MaintenanceState state )
    {
        string currentEffect = state.AppliedLevel < 1
            ? "현재 효과 없음"
            : GetInlineEffectText( GetAppliedEffects( state ) );

        if ( state.IsOwned == false )
            return $"{currentEffect}\n상점에서 최초 구매 필요";

        MaintenanceLevelData nextLevelData =
            GetDisplayedNextLevelData( state );

        if ( state.HasPendingEffect )
        {
            return $"{currentEffect}\n" +
                $"적용 대기: " +
                $"{GetInlineEffectText( nextLevelData.Effects )}";
        }

        if ( state.IsMaxLevel )
            return $"{currentEffect}\nMAX";

        if ( nextLevelData == null )
            return $"{currentEffect}\n다음 단계 정보 없음";

        if ( GetNextResult( state ) ==
            MaintenanceResult.RequirementNotMet )
        {
            return $"{currentEffect}\n" +
                $"잠금: " +
                $"{GetInlineRequirementText( nextLevelData.Requirements )}";
        }

        return $"{currentEffect}\n" +
            $"다음: {GetInlineEffectText( nextLevelData.Effects )} / " +
            $"{nextLevelData.UpgradeCost:N0}G";
    }

    /// <summary>
    /// 정비 분류를 전체 탭 슬롯 분류로 변환
    /// </summary>
    /// <param name="type">정비 분류</param>
    /// <returns>전체 탭 슬롯 분류</returns>
    MaintenanceEffectSlotCategory GetManagementCategory (
        MaintenanceType type )
    {
        switch ( type )
        {
            case MaintenanceType.Facility:
                return MaintenanceEffectSlotCategory.Facility;

            case MaintenanceType.Research:
                return MaintenanceEffectSlotCategory.Research;

            default:
                return MaintenanceEffectSlotCategory.Convenience;
        }
    }

    /// <summary>
    /// 전체 탭 정비 항목 정렬 순서 비교
    /// </summary>
    int CompareManagementMaintenance (
        MaintenanceState first,
        MaintenanceState second )
    {
        int typeOrder =
            first.Data.Type.CompareTo( second.Data.Type );

        if ( typeOrder != 0 )
            return typeOrder;

        return CompareMaintenance( first, second );
    }

    #endregion

    #region ----- 상세 문구 -----

    /// <summary>
    /// 현재 표시 단계의 정비 설명 반환
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>효과값을 반영한 정비 설명</returns>
    string GetDescriptionText ( MaintenanceState state )
    {
        int level = GetDisplayedNextLevel( state );

        if ( level < 1 ) level = state.AppliedLevel;

        return state.Data.GetDescription( level );
    }

    /// <summary>
    /// 현재 적용 효과 문구 반환
    /// </summary>
    string GetCurrentEffectText ( MaintenanceState state )
    {
        if ( state.AppliedLevel < 1 )
            return "- 현재 적용된 효과 없음";

        if ( state.Data.GetLevelData(
            state.AppliedLevel, out _ ) == false )
            return "효과 정보를 확인할 수 없음";

        return GetEffectTextList( GetAppliedEffects( state ) );
    }

    /// <summary>
    /// 다음 적용 효과 문구 반환
    /// </summary>
    string GetNextEffectText (
        MaintenanceState state, MaintenanceLevelData levelData )
    {
        if ( state.IsMaxLevel && state.HasPendingEffect == false )
            return "최대 단계";

        if ( levelData == null )
            return "- 다음 효과 없음";

        string effectText = GetEffectTextList( levelData.Effects );

        if ( state.HasPendingEffect )
            return $"Lv.{state.PurchasedLevel} 효과 적용 대기\n" +
                $"{effectText}\n다음 영업일부터 적용";

        return effectText;
    }

    /// <summary>
    /// 정비 비용 문구 반환
    /// </summary>
    string GetCostText (
        MaintenanceState state, MaintenanceLevelData levelData )
    {
        if ( state.IsOwned == false )
        {
            if ( _shopModel.TryGetItem(
                state.Data.Id, out ShopItemModel itemModel ) == false )
                return "상점 구매 비용 확인 불가";

            return $"{itemModel.Item.CurrentPrice:N0}G";
        }

        if ( state.HasPendingEffect )
            return $"Lv.{state.PurchasedLevel} 적용 후 구매 가능";

        if ( state.IsMaxLevel )
            return "MAX";

        if ( levelData == null )
            return "비용 확인 불가";

        float upgradeCost = _maintenanceUpgradeModel.GetAdjustedCost(
            state.Data, levelData.UpgradeCost );
        return $"{upgradeCost:N0}G";
    }

    /// <summary>
    /// 정비 상세 선행 조건 문구 반환
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <param name="levelData">다음 단계 데이터</param>
    /// <returns>현재 상태에 맞는 선행 조건 문구</returns>
    string GetDetailRequirementText (
        MaintenanceState state, MaintenanceLevelData levelData )
    {
        if ( state.IsOwned == false )
            return "상점에서 최초 구매 필요";

        if ( state.HasPendingEffect )
            return $"Lv.{state.PurchasedLevel} 적용 후 구매 가능";

        return GetRequirementText( levelData?.Requirements );
    }

    /// <summary>
    /// 슬롯 상태 문구 반환
    /// </summary>
    string GetSlotStateText ( MaintenanceState state )
    {
        if ( state.IsOwned == false )
            return "상점에서 구매 필요";

        if ( state.HasPendingEffect )
            return "다음 영업일부터 적용 대기";

        if ( state.IsMaxLevel )
            return "MAX";

        if ( state.Data.GetLevelData(
            state.NextLevel,
            out MaintenanceLevelData levelData ) == false )
            return "다음 단계 정보 없음";

        float upgradeCost = _maintenanceUpgradeModel.GetAdjustedCost(
            state.Data, levelData.UpgradeCost );
        return $"정비 비용: {upgradeCost:N0}G";
    }

    /// <summary>
    /// 슬롯 선행 조건 문구 반환
    /// </summary>
    string GetSlotRequirementText ( MaintenanceState state )
    {
        MaintenanceLevelData levelData =
            GetDisplayedNextLevelData( state );

        if ( levelData == null ) return string.Empty;

        return GetInlineRequirementText(
            levelData.Requirements );
    }

    /// <summary>
    /// 상세에 표시할 다음 단계 데이터 반환
    /// </summary>
    MaintenanceLevelData GetDisplayedNextLevelData (
        MaintenanceState state )
    {
        int level = GetDisplayedNextLevel( state );

        if ( level < 1 ) return null;

        return state.Data.GetLevelData(
            level, out MaintenanceLevelData levelData )
            ? levelData
            : null;
    }

    /// <summary>
    /// 상세에 표시할 다음 단계 반환
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>표시할 단계, 없으면 0</returns>
    int GetDisplayedNextLevel ( MaintenanceState state )
    {
        if ( state.HasPendingEffect ) return state.PurchasedLevel;
        if ( state.IsMaxLevel ) return 0;

        return state.NextLevel;
    }

    #endregion

    #region ----- 효과 문구 -----

    /// <summary>
    /// 현재 단계까지 실제 적용된 효과 목록 생성
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>누적 해금과 현재 단계 최종 효과 목록</returns>
    IReadOnlyList<EffectData> GetAppliedEffects ( MaintenanceState state )
    {
        var effects = new List<EffectData>( );

        //적용 레벨만큼 반복
        for ( int level = 1; level <= state.AppliedLevel; level++ )
        {
            //레벨 데이터 가져오기
            if ( state.Data.GetLevelData(
                level, out MaintenanceLevelData levelData ) == false ||
                levelData.Effects == null )
                continue;

            //효과 개수만큼 반복
            for ( int i = 0; i < levelData.Effects.Count; i++ )
            {
                EffectData effect = levelData.Effects [ i ];

                if ( effect == null ) continue;

                //파츠와 정보 해금은 이전 단계까지 누적 표시
                if ( effect.Type == MaintenanceEffectType.PartUnlock ||
                    effect.Type == MaintenanceEffectType.InformationUnlock )
                {
                    effects.Add( effect );
                    continue;
                }

                //수치 효과와 테마 연구는 현재 단계 최종값만 표시
                if ( level == state.AppliedLevel )
                    effects.Add( effect );
            }
        }

        return effects;
    }

    /// <summary>
    /// 정비 효과 목록 문구 반환
    /// </summary>
    string GetEffectTextList ( IReadOnlyList<EffectData> effects )
    {
        if ( effects == null || effects.Count == 0 )
            return "- 효과 없음";

        var text = new StringBuilder( );

        for ( int i = 0; i < effects.Count; i++ )
            text.AppendLine( $"- {GetEffectText( effects [ i ] )}" );

        return text.ToString( ).TrimEnd( );
    }

    /// <summary>
    /// 개별 정비 효과 문구 반환
    /// </summary>
    string GetEffectText ( EffectData effect )
    {
        switch ( effect.Type )
        {
            case MaintenanceEffectType.InventoryCapacity:
                return $"인벤토리 최대 용량 {effect.Value:0}";

            case MaintenanceEffectType.PartMaxStockBonus:
                return $"파츠 최대 재고 +{effect.Value:0}";

            case MaintenanceEffectType.RestockSpan:
                return $"일반 재입고 간격 {effect.Value:0}일";

            case MaintenanceEffectType.OrderWaitingLimit:
                return $"수락 대기 주문 한도 {effect.Value:0}개";

            case MaintenanceEffectType.CraftQuota:
                return $"일일 제작 할당량 {effect.Value:0}건";

            case MaintenanceEffectType.DeliverySpanReduction:
                return $"배송 소요일 {effect.Value:0}일 단축";

            case MaintenanceEffectType.PartUnlock:
                return $"{GetPartName( effect.TargetId )} 파츠 해금";

            case MaintenanceEffectType.InformationUnlock:
                return GetInformationUnlockText( effect.TargetId );

            case MaintenanceEffectType.ThemeScoreBonus:
                if ( Enum.TryParse( effect.TargetId, true,
                    out PartTheme theme ) && theme.IsTraitTheme( ) )
                {
                    return $"{theme.GetDisplayName( )} 테마 점수 " +
                        $"+{effect.Value:0.##}";
                }
                return $"{effect.TargetId} 테마 점수 " +
                    $"+{effect.Value:0.##}";

            default:
                return "확인할 수 없는 효과";
        }
    }

    /// <summary>
    /// 파츠 아이디의 표시 이름 반환
    /// </summary>
    /// <param name="partId">파츠 아이디</param>
    /// <returns>파츠 표시 이름</returns>
    string GetPartName ( string partId )
    {
        if ( _shopModel.TryGetItem( partId, out ShopItemModel itemModel ) )
            return itemModel.Item.Data.Name;

        return partId;
    }

    /// <summary>
    /// 정보 표시 해금 효과 문구 반환
    /// </summary>
    /// <param name="informationId">정보 표시 해금 아이디</param>
    /// <returns>정보 표시 해금 효과 문구</returns>
    string GetInformationUnlockText ( string informationId )
    {
        switch ( informationId )
        {
            case InformationUnlockId.OrderDeadlineAlert:
                return "주문 기한 임박 알림 해금";

            case InformationUnlockId.MissingPartsDisplay:
                return "부족 파츠 표시 해금";

            case InformationUnlockId.DeliveryEstimatedArrival:
                return "배송 예상 완료일 표시 해금";

            default:
                return $"{informationId} 정보 해금";
        }
    }

    /// <summary>
    /// 정비 효과 한 줄 문구 반환
    /// </summary>
    string GetInlineEffectText (
        IReadOnlyList<EffectData> effects )
    {
        if ( effects == null || effects.Count == 0 )
            return "효과 없음";

        var text = new StringBuilder( );

        for ( int i = 0; i < effects.Count; i++ )
        {
            if ( i > 0 ) text.Append( ", " );
            text.Append( GetEffectText( effects [ i ] ) );
        }

        return text.ToString( );
    }

    #endregion

    #region ----- 상품 카테고리 문구 -----

    /// <summary>
    /// 상품 카테고리 문구 반환
    /// </summary>
    /// <param name="productType">상품 타입</param>
    /// <returns>상품 카테고리 문구</returns>
    string GetProductTypeText ( ProductType productType )
    {
        switch ( productType )
        {
            case ProductType.Expansion:
                return "시설";

            case ProductType.Furniture:
                return "인테리어";

            case ProductType.Equipment:
                return "장비";

            default:
                return "기타";
        }
    }

    #endregion

    #region ----- 선행 조건 문구 -----

    /// <summary>
    /// 선행 조건 목록 문구 반환
    /// </summary>
    string GetRequirementText (
        IReadOnlyList<MaintenanceRequirementData> requirements )
    {
        if ( requirements == null || requirements.Count == 0 )
            return "선행 조건 없음";

        var text = new StringBuilder( );

        for ( int i = 0; i < requirements.Count; i++ )
            text.AppendLine(
                $"- {GetRequirementText( requirements [ i ] )}" );

        return text.ToString( ).TrimEnd( );
    }

    /// <summary>
    /// 선행 조건 한 줄 문구 반환
    /// </summary>
    string GetInlineRequirementText (
        IReadOnlyList<MaintenanceRequirementData> requirements )
    {
        if ( requirements == null || requirements.Count == 0 )
            return "선행 조건 없음";

        var text = new StringBuilder( );

        for ( int i = 0; i < requirements.Count; i++ )
        {
            if ( i > 0 ) text.Append( ", " );
            text.Append(
                GetRequirementText( requirements [ i ] ) );
        }

        return text.ToString( );
    }

    /// <summary>
    /// 개별 선행 조건 문구 반환
    /// </summary>
    string GetRequirementText (
        MaintenanceRequirementData requirement )
    {
        switch ( requirement.Type )
        {
            case MaintenanceRequirementType.TotalDay:
                return $"누적 영업일 {requirement.RequiredValue}일";

            case MaintenanceRequirementType.WeeklyRating:
                return $"주간 영업 평가 " +
                    $"{GetRatingText( requirement.RequiredRating )} 이상";

            case MaintenanceRequirementType.MaintenanceLevel:
                return $"{GetMaintenanceName( requirement.RequiredMaintenanceId )} " +
                    $"Lv.{requirement.RequiredLevel}";

            default:
                return "확인할 수 없는 선행 조건";
        }
    }

    /// <summary>
    /// 선행 정비 이름 반환
    /// </summary>
    string GetMaintenanceName ( string id )
    {
        if ( _maintenanceModel.GetState(
            id, out MaintenanceState state ) )
            return state.Data.Name;

        return id;
    }

    #endregion

    #region ----- 공통 문구 -----

    /// <summary>
    /// 정비 적용 시점 문구 반환
    /// </summary>
    /// <param name="applyType">정비 효과 적용 시점</param>
    /// <returns>정비 적용 시점 문구</returns>
    string GetApplyText ( MaintenanceApplyType applyType )
    {
        return applyType == MaintenanceApplyType.Immediately
            ? "즉시 적용"
            : "다음 영업일부터 적용";
    }

    /// <summary>
    /// 주간 영업 평가 문구 반환
    /// </summary>
    /// <param name="rating">주간 영업 평가</param>
    /// <returns>주간 영업 평가 문구</returns>
    string GetRatingText ( WeeklyRating rating )
    {
        switch ( rating )
        {
            case WeeklyRating.Best:
                return "최우수";

            case WeeklyRating.Good:
                return "우수";

            case WeeklyRating.Normal:
                return "보통";

            case WeeklyRating.Caution:
                return "주의";

            case WeeklyRating.Danger:
                return "위험";

            default:
                return "평가 전";
        }
    }

    #endregion

    #region ----- 상태 조회 -----

    /// <summary>
    /// 다음 정비 단계 처리 결과 반환
    /// </summary>
    /// <param name="state">정비 상태</param>
    /// <returns>정비 처리 결과</returns>
    MaintenanceResult GetNextResult ( MaintenanceState state )
    {
        //업그레이드한 적 없으면 상점에서 최초 구매 필요
        if ( state.IsOwned == false )
            return _maintenanceModel.GetAcquireResult( state.Data.Id );

        //업그레이드 가능 여부 확인
        return _maintenanceUpgradeModel.GetUpgradeResult(
            state.Data.Id, _totalDay, _weeklyRating, out _ );
    }

    /// <summary>
    /// 정비 목록 정렬 순서 비교
    /// </summary>
    int CompareMaintenance (
        MaintenanceState first,
        MaintenanceState second )
    {
        int firstOrder = GetSortOrder( first );
        int secondOrder = GetSortOrder( second );

        if ( firstOrder != secondOrder )
            return firstOrder.CompareTo( secondOrder );

        return string.Compare(
            first.Data.Name,
            second.Data.Name,
            StringComparison.CurrentCulture );
    }

    /// <summary>
    /// 정비 상태별 정렬값 반환
    /// </summary>
    int GetSortOrder ( MaintenanceState state )
    {
        if ( state.IsMaxLevel ) return 2;

        MaintenanceResult result = GetNextResult( state );

        return result == MaintenanceResult.RequirementNotMet
            ? 1
            : 0;
    }

    #endregion
}
