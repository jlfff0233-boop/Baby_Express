using System.Collections.Generic;

/// <summary>
/// 정비 효과 적용 처리
/// </summary>
public class MaintenanceEffectModel
{

    PlayStateModel _playStateModel;       //상품 해금과 공용 플레이 상태
    CustomerOrderModel _orderModel;       //고객 주문 상태
    MaintenanceModel _maintenanceModel;       //정비 단계 상태
    InventoryModel _inventoryModel;       //인벤토리 상태
    ShopModel _shopModel;       //상점 상품 상태
    BusinessDayModel _businessDayModel;       //영업일과 제작 할당량 상태
    DeliveryModel _deliveryModel;       //배송 일정 상태

    #region ----- 초기화 -----

    /// <summary>
    /// 정비 효과 모델 생성
    /// </summary>
    /// <param name="maintenanceModel">정비 단계 상태 모델</param>
    /// <param name="inventoryModel">인벤토리 상태 모델</param>
    /// <param name="shopModel">상점 상품 상태 모델</param>
    /// <param name="orderModel">고객 주문 상태 모델</param>
    /// <param name="businessDayModel">영업일 상태 모델</param>
    /// <param name="deliveryModel">배송 상태 모델</param>
    /// <param name="playStateModel">공용 플레이 상태 모델</param>
    public MaintenanceEffectModel (
        MaintenanceModel maintenanceModel, InventoryModel inventoryModel,
        ShopModel shopModel, CustomerOrderModel orderModel,
        BusinessDayModel businessDayModel, DeliveryModel deliveryModel,
        PlayStateModel playStateModel )
    {
        _maintenanceModel = maintenanceModel;
        _inventoryModel = inventoryModel;
        _shopModel = shopModel;
        _orderModel = orderModel;
        _businessDayModel = businessDayModel;
        _deliveryModel = deliveryModel;
        _playStateModel = playStateModel;
    }

    #endregion

    #region ----- 효과 처리 -----

    /// <summary>
    /// 즉시 적용 정비 효과 처리
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 효과 처리 결과</returns>
    public MaintenanceEffectResult ApplyImmediate ( string id )
    {
        //정비 상태 조회
        if ( _maintenanceModel.GetState(
            id, out MaintenanceState state ) == false )
            return MaintenanceEffectResult.NotFound;

        //다음 영업일 적용 정비는 적용 대기 상태 유지
        if ( state.Data.ApplyType != MaintenanceApplyType.Immediately )
            return MaintenanceEffectResult.Success;

        return ApplyPendingEffect( id );
    }

    /// <summary>
    /// 다음 영업일부터 적용할 정비 효과 처리
    /// </summary>
    /// <param name="appliedChanges">적용 완료한 정비 효과 변경 목록</param>
    /// <returns>정비 효과 처리 결과</returns>
    public MaintenanceEffectResult ApplyNextDayEffects (
        out IReadOnlyList<MaintenanceEffectChangeSet> appliedChanges )
    {
        var preparedChanges = new List<MaintenanceEffectChangeSet>( );

        //적용 대기 중인 다음 영업일 효과를 먼저 모두 검증
        foreach ( MaintenanceState state in _maintenanceModel.States )
        {
            if ( state.Data.ApplyType != MaintenanceApplyType.NextDay ||
                state.HasPendingEffect == false )
                continue;

            MaintenanceEffectResult result = TryCreateEffectChangeSet(
                state.Data.Id, out MaintenanceEffectChangeSet changeSet );

            if ( result != MaintenanceEffectResult.Success )
            {
                appliedChanges = new List<MaintenanceEffectChangeSet>( );
                return result;
            }

            preparedChanges.Add( changeSet );
        }

        var completedChanges = new List<MaintenanceEffectChangeSet>(
            preparedChanges.Count );

        //검증을 마친 효과를 순서대로 적용
        for ( int i = 0; i < preparedChanges.Count; i++ )
        {
            MaintenanceEffectChangeSet changeSet = preparedChanges [ i ];
            MaintenanceEffectResult result = ApplyEffectChangeSet( changeSet );

            if ( result == MaintenanceEffectResult.Success )
            {
                completedChanges.Add( changeSet );
                continue;
            }

            //중간 실패 시 앞에서 적용한 효과를 역순 복구
            bool isRolledBack = RollbackAppliedEffects( completedChanges ) ==
                MaintenanceEffectResult.Success;

            appliedChanges = new List<MaintenanceEffectChangeSet>( );
            return isRolledBack ? result : MaintenanceEffectResult.RollbackFailed;
        }

        appliedChanges = completedChanges;
        return MaintenanceEffectResult.Success;
    }

    /// <summary>
    /// 적용 완료한 정비 효과 목록 복구
    /// </summary>
    /// <param name="appliedChanges">적용 완료한 정비 효과 변경 목록</param>
    /// <returns>정비 효과 처리 결과</returns>
    public MaintenanceEffectResult RollbackAppliedEffects (
        IReadOnlyList<MaintenanceEffectChangeSet> appliedChanges )
    {
        //나중에 적용된 효과부터 역순으로 복구
        for ( int i = appliedChanges.Count - 1; i >= 0; i-- )
        {
            //복구에 실패하면 앞 단계 상태까지 추가 변경하지 않음
            if ( RollbackAppliedEffect( appliedChanges [ i ] ) == false )
                return MaintenanceEffectResult.RollbackFailed;
        }

        return MaintenanceEffectResult.Success;
    }

    /// <summary>
    /// 현재 적용 대기 정비 효과 처리
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 효과 처리 결과</returns>
    public MaintenanceEffectResult ApplyPendingEffect ( string id )
    {
        MaintenanceEffectResult result = TryCreateEffectChangeSet(
            id, out MaintenanceEffectChangeSet changeSet );

        if ( result != MaintenanceEffectResult.Success ) return result;

        return ApplyEffectChangeSet( changeSet );
    }

    /// <summary>
    /// 적용 대기 중인 정비 효과 변경 내용 생성
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <param name="changeSet">생성한 정비 효과 변경 내용</param>
    /// <returns>정비 효과 처리 결과</returns>
    MaintenanceEffectResult TryCreateEffectChangeSet (
        string id, out MaintenanceEffectChangeSet changeSet )
    {
        changeSet = null;

        //정비 상태 조회
        if ( _maintenanceModel.GetState(
            id, out MaintenanceState state ) == false )
            return MaintenanceEffectResult.NotFound;

        //적용할 구매 단계가 없으면 종료
        if ( state.HasPendingEffect == false )
            return MaintenanceEffectResult.NoPendingEffect;

        //구매한 단계 데이터 조회
        if ( state.Data.GetLevelData(
            state.PurchasedLevel,
            out MaintenanceLevelData levelData ) == false )
            return MaintenanceEffectResult.InvalidLevelData;

        //현재 단계의 최종 효과값 생성
        MaintenanceEffectResult result = TryCreateEffectValues(
            levelData.Effects, out MaintenanceEffectValues effectValues );

        if ( result != MaintenanceEffectResult.Success )
            return result;

        //이미 해금된 파츠 재구매 차단
        foreach ( string partId in effectValues.PartUnlockIds )
        {
            if ( _playStateModel.IsUnlocked( partId ) )
                return MaintenanceEffectResult.InvalidEffect;
        }

        //이미 해금된 정보 재구매 차단
        foreach ( string informationId in effectValues.InformationUnlockIds )
        {
            if ( _playStateModel.IsUnlocked( informationId ) )
                return MaintenanceEffectResult.InvalidEffect;
        }

        //변경 전 상태를 포함한 효과 변경 내용 생성
        PreviousEffectValues previousValues =
            CreatePreviousEffectValues( effectValues );

        changeSet = new MaintenanceEffectChangeSet(
            id, effectValues, previousValues );
        return MaintenanceEffectResult.Success;
    }

    /// <summary>
    /// 검증을 마친 정비 효과 변경 내용 적용
    /// </summary>
    /// <param name="changeSet">적용할 정비 효과 변경 내용</param>
    /// <returns>정비 효과 처리 결과</returns>
    MaintenanceEffectResult ApplyEffectChangeSet (
        MaintenanceEffectChangeSet changeSet )
    {
        MaintenanceEffectValues effectValues = changeSet.EffectValues;
        PreviousEffectValues previousValues = changeSet.PreviousValues;

        //현재 단계 효과 적용
        MaintenanceEffectResult result = ApplyEffectValues(
            effectValues, previousValues );

        if ( result != MaintenanceEffectResult.Success )
            return result;

        //효과 적용 단계 갱신
        if ( _maintenanceModel.ApplyPurchasedLevel(
            changeSet.MaintenanceId ) ==
            MaintenanceResult.Success )
            return MaintenanceEffectResult.Success;

        //단계 갱신 실패 시 전체 효과 복구
        return RestoreEffectValues( effectValues, previousValues )
            ? MaintenanceEffectResult.StateUpdateFailed
            : MaintenanceEffectResult.RollbackFailed;
    }

    /// <summary>
    /// 적용 완료한 정비 효과 한 건 복구
    /// </summary>
    /// <param name="changeSet">복구할 정비 효과 변경 내용</param>
    /// <returns>전체 원상 복구 성공 여부</returns>
    bool RollbackAppliedEffect ( MaintenanceEffectChangeSet changeSet )
    {
        //담당 모델 상태를 먼저 효과 적용 전 값으로 복구
        if ( RestoreEffectValues(
            changeSet.EffectValues, changeSet.PreviousValues ) == false )
        {
            //복구 실패 시 적용 상태와 맞도록 현재 효과 재적용 시도
            ApplyEffectValues(
                changeSet.EffectValues, changeSet.PreviousValues );
            return false;
        }

        //정비 적용 단계 복구
        if ( _maintenanceModel.RollbackAppliedLevel(
            changeSet.MaintenanceId ) == MaintenanceResult.Success )
            return true;

        //단계 복구 실패 시 적용 단계와 맞도록 현재 효과 재적용
        ApplyEffectValues(
            changeSet.EffectValues, changeSet.PreviousValues );
        return false;
    }

    /// <summary>
    /// 현재 단계의 즉시 적용 효과 복구
    /// </summary>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 효과 처리 결과</returns>
    public MaintenanceEffectResult RollbackImmediate ( string id )
    {
        //정비 상태 조회
        if ( _maintenanceModel.GetState(
            id, out MaintenanceState state ) == false )
            return MaintenanceEffectResult.NotFound;

        //다음 영업일 효과와 아직 적용되지 않은 단계는 복구 대상이 아님
        if ( state.Data.ApplyType != MaintenanceApplyType.Immediately ||
            state.AppliedLevel != state.PurchasedLevel )
            return MaintenanceEffectResult.Success;

        //현재 단계 효과값 생성
        if ( state.Data.GetLevelData(
            state.AppliedLevel, out MaintenanceLevelData currentLevelData ) == false )
            return MaintenanceEffectResult.InvalidLevelData;

        //정비 결과 값 생성
        MaintenanceEffectResult result = TryCreateEffectValues(
            currentLevelData.Effects,
            out MaintenanceEffectValues currentValues );

        if ( result != MaintenanceEffectResult.Success ) return result;

        var previousValues = new MaintenanceEffectValues( );

        //이전 적용 단계가 있으면 해당 단계의 최종 효과값 생성
        if ( state.AppliedLevel > 1 )
        {
            if ( state.Data.GetLevelData(
                state.AppliedLevel - 1,
                out MaintenanceLevelData previousLevelData ) == false )
                return MaintenanceEffectResult.InvalidLevelData;

            result = TryCreateEffectValues(
                previousLevelData.Effects, out previousValues );

            if ( result != MaintenanceEffectResult.Success ) return result;
        }

        //첫 업그레이드 이전에는 기본값 사용
        //인벤토리 용량
        if ( currentValues.InventoryCapacity.HasValue
            && previousValues.InventoryCapacity.HasValue == false )
            previousValues.InventoryCapacity = InventoryModel.DefaultCapacity;
        //파츠당 최대 재고 보너스
        if ( currentValues.PartMaxStockBonus.HasValue
            && previousValues.PartMaxStockBonus.HasValue == false )
            previousValues.PartMaxStockBonus = 0;
        //재입고 간격
        if ( currentValues.RestockSpan.HasValue
            && previousValues.RestockSpan.HasValue == false )
            previousValues.RestockSpan = 0;
        //수락 대기 주문 제한
        if ( currentValues.OrderWaitingLimit.HasValue
            && previousValues.OrderWaitingLimit.HasValue == false )
            previousValues.OrderWaitingLimit = CustomerOrderModel.DefaultWaitingLimit;
        //일일 제작 할당량
        if ( currentValues.CraftQuota.HasValue
            && previousValues.CraftQuota.HasValue == false )
            previousValues.CraftQuota = _businessDayModel.BaseCraftLimit;
        //배송 소요일 단축값
        if ( currentValues.DeliverySpanReduction.HasValue
            && previousValues.DeliverySpanReduction.HasValue == false )
            previousValues.DeliverySpanReduction = 0;

        //적용 단계 복구 실패 시 되돌릴 현재 담당 모델 상태 저장
        PreviousEffectValues currentRuntimeValues =
            CreatePreviousEffectValues( currentValues );

        //현재 단계 효과를 제거한 이전 단계 상태 생성
        PreviousEffectValues rollbackValues =
            CreateRollbackValues(
                currentValues, previousValues,
                currentRuntimeValues );

        //이전 단계 효과값 복구
        if ( RestoreEffectValues(
            currentValues, rollbackValues ) == false )
        {
            //부분 복구 실패 시 현재 담당 모델 상태로 원상 복구
            RestoreEffectValues( currentValues, currentRuntimeValues );
            return MaintenanceEffectResult.RollbackFailed;
        }

        //적용 단계 복구
        if ( _maintenanceModel.RollbackAppliedLevel( id ) ==
            MaintenanceResult.Success )
            return MaintenanceEffectResult.Success;

        //적용 단계 복구 실패 시 현재 효과값 재적용
        return RestoreEffectValues( currentValues, currentRuntimeValues )
            ? MaintenanceEffectResult.StateUpdateFailed
            : MaintenanceEffectResult.RollbackFailed;
    }

    #endregion

    #region ----- 효과값 생성 -----

    /// <summary>
    /// 단계별 정비 효과값 생성
    /// </summary>
    /// <param name="effects">현재 단계 효과 목록</param>
    /// <param name="effectValues">생성한 정비 효과값</param>
    /// <returns>정비 효과 처리 결과</returns>
    MaintenanceEffectResult TryCreateEffectValues (
        IReadOnlyList<EffectData> effects, out MaintenanceEffectValues effectValues )
    {
        effectValues = new MaintenanceEffectValues( );

        if ( effects == null || effects.Count == 0 )
            return MaintenanceEffectResult.InvalidEffect;

        for ( int i = 0; i < effects.Count; i++ )
        {
            EffectData effect = effects [ i ];

            if ( effect == null )
                return MaintenanceEffectResult.InvalidEffect;

            switch ( effect.Type )
            {
                case MaintenanceEffectType.InventoryCapacity:
                    if ( TryGetIntegerValue( effect.Value, out int capacity ) == false ||
                        capacity <= 0 ||
                        effectValues.InventoryCapacity.HasValue )
                        return MaintenanceEffectResult.InvalidEffect;

                    effectValues.InventoryCapacity = capacity;
                    break;

                case MaintenanceEffectType.PartMaxStockBonus:
                    if ( TryGetIntegerValue( effect.Value, out int stockBonus ) == false ||
                        stockBonus < 0 ||
                        effectValues.PartMaxStockBonus.HasValue )
                        return MaintenanceEffectResult.InvalidEffect;

                    effectValues.PartMaxStockBonus = stockBonus;
                    break;

                case MaintenanceEffectType.RestockSpan:
                    if ( TryGetIntegerValue( effect.Value, out int restockSpan ) == false ||
                        restockSpan < 3 ||
                        effectValues.RestockSpan.HasValue )
                        return MaintenanceEffectResult.InvalidEffect;

                    effectValues.RestockSpan = restockSpan;
                    break;

                case MaintenanceEffectType.OrderWaitingLimit:
                    if ( TryGetIntegerValue( effect.Value, out int waitingLimit ) == false ||
                        waitingLimit < 1 ||
                        waitingLimit > CustomerOrderModel.MaxWaitingLimit ||
                        effectValues.OrderWaitingLimit.HasValue )
                        return MaintenanceEffectResult.InvalidEffect;

                    effectValues.OrderWaitingLimit = waitingLimit;
                    break;

                case MaintenanceEffectType.CraftQuota:
                    if ( TryGetIntegerValue( effect.Value, out int craftQuota ) == false ||
                        craftQuota <= 0 ||
                        effectValues.CraftQuota.HasValue )
                        return MaintenanceEffectResult.InvalidEffect;

                    effectValues.CraftQuota = craftQuota;
                    break;

                case MaintenanceEffectType.DeliverySpanReduction:
                    if ( TryGetIntegerValue(
                        effect.Value, out int deliveryReduction ) == false ||
                        deliveryReduction < 0 ||
                        effectValues.DeliverySpanReduction.HasValue )
                        return MaintenanceEffectResult.InvalidEffect;

                    effectValues.DeliverySpanReduction = deliveryReduction;
                    break;

                case MaintenanceEffectType.PartUnlock:
                    if ( TryAddPartUnlock(
                        effect, effectValues.PartUnlockIds ) == false )
                        return MaintenanceEffectResult.InvalidEffect;
                    break;

                case MaintenanceEffectType.ThemeScoreBonus:
                    if ( TryAddThemeScoreBonus(
                        effect, effectValues.ThemeScoreThemes ) == false )
                        return MaintenanceEffectResult.InvalidEffect;
                    break;

                case MaintenanceEffectType.InformationUnlock:
                    if ( TryAddInformationUnlock(
                        effect, effectValues.InformationUnlockIds ) == false )
                        return MaintenanceEffectResult.InvalidEffect;
                    break;

                default:
                    return MaintenanceEffectResult.UnsupportedEffect;
            }
        }

        return MaintenanceEffectResult.Success;
    }

    /// <summary>
    /// 파츠 해금 효과 추가
    /// </summary>
    /// <param name="effect">검증할 파츠 해금 효과</param>
    /// <param name="partUnlockIds">파츠 해금 아이디 목록</param>
    /// <returns>유효한 파츠 해금 효과 추가 여부</returns>
    bool TryAddPartUnlock (
        EffectData effect, HashSet<string> partUnlockIds )
    {
        string targetId = effect.TargetId;

        //파츠 해금은 수치값을 사용하지 않음
        if ( effect.Value != 0f ||
            string.IsNullOrWhiteSpace( targetId ) ||
            partUnlockIds.Add( targetId ) == false )
            return false;

        //등록된 실제 파츠인지 확인
        return _shopModel.TryGetItem(
            targetId, out ShopItemModel itemModel ) &&
            itemModel.Item.Data is PartsData;
    }

    /// <summary>
    /// 정보 표시 해금 효과 추가
    /// </summary>
    /// <param name="effect">검증할 정보 해금 효과</param>
    /// <param name="informationUnlockIds">정보 표시 해금 아이디 목록</param>
    /// <returns>유효한 정보 해금 효과 추가 여부</returns>
    bool TryAddInformationUnlock (
        EffectData effect, HashSet<string> informationUnlockIds )
    {
        //정보 해금은 대상 아이디만 사용하고 수치값은 사용하지 않음
        return effect.Value == 0f &&
            InformationUnlockId.IsDefined( effect.TargetId ) &&
            informationUnlockIds.Add( effect.TargetId );
    }

    /// <summary>
    /// 성질 테마 점수 효과 추가
    /// </summary>
    /// <param name="effect">검증할 테마 점수 효과</param>
    /// <param name="themeScoreThemes">점수 보너스 적용 대상 테마 목록</param>
    /// <returns>유효한 테마 점수 효과 추가 여부</returns>
    bool TryAddThemeScoreBonus (
        EffectData effect,
        HashSet<PartTheme> themeScoreThemes )
    {
        if ( float.IsNaN( effect.Value ) ||
            float.IsInfinity( effect.Value ) ||
            effect.Value <= 0f ||
            System.Enum.TryParse(
                effect.TargetId, true, out PartTheme theme ) == false ||
            theme.IsTraitTheme( ) == false )
            return false;

        return themeScoreThemes.Add( theme );
    }

    /// <summary>
    /// 정수 효과값 변환
    /// </summary>
    /// <param name="source">변환할 효과값</param>
    /// <param name="value">변환된 정수 효과값</param>
    /// <returns>정수 효과값 변환 성공 여부</returns>
    bool TryGetIntegerValue ( float source, out int value )
    {
        value = 0;

        if ( float.IsNaN( source ) ||
            float.IsInfinity( source ) ||
            source < int.MinValue ||
            source > int.MaxValue )
            return false;

        value = ( int ) source;
        return source == value;
    }

    #endregion

    #region ----- 이전 상태 생성 -----

    /// <summary>
    /// 정비 효과 적용 전 상태 생성
    /// </summary>
    /// <param name="effectValues">적용할 정비 효과값</param>
    /// <returns>효과 적용 전 담당 모델 상태</returns>
    PreviousEffectValues CreatePreviousEffectValues (
        MaintenanceEffectValues effectValues )
    {
        var previousValues = new PreviousEffectValues
        {
            InventoryCapacity = _inventoryModel.Capacity,
            PartMaxStockBonus = _shopModel.PartMaxStockBonus,
            RestockSpan = _shopModel.PartRestockSpan,
            OrderWaitingLimit = _orderModel.WaitingLimit,
            CraftQuota = _businessDayModel.NextCraftLimit,
            DeliverySpanReduction = _deliveryModel.DeliverySpanReduction
        };

        foreach ( string id in effectValues.PartUnlockIds )
        {
            previousValues.PartUnlockStates.Add(
                id, _playStateModel.IsUnlocked( id ) );
        }

        foreach ( string id in effectValues.InformationUnlockIds )
        {
            previousValues.InformationUnlockStates.Add(
                id, _playStateModel.IsUnlocked( id ) );
        }

        return previousValues;
    }

    /// <summary>
    /// 현재 단계 효과를 제거한 이전 단계 상태 생성
    /// </summary>
    /// <param name="currentValues">현재 단계 정비 효과값</param>
    /// <param name="previousValues">이전 단계 정비 효과값</param>
    /// <param name="currentRuntimeValues">현재 담당 모델 상태</param>
    /// <returns>현재 단계 복구 후 적용할 이전 상태</returns>
    PreviousEffectValues CreateRollbackValues (
        MaintenanceEffectValues currentValues,
        MaintenanceEffectValues previousValues,
        PreviousEffectValues currentRuntimeValues )
    {
        var rollbackValues = new PreviousEffectValues
        {
            InventoryCapacity = previousValues.InventoryCapacity ??
                currentRuntimeValues.InventoryCapacity,
            PartMaxStockBonus = previousValues.PartMaxStockBonus ??
                currentRuntimeValues.PartMaxStockBonus,
            RestockSpan = previousValues.RestockSpan ??
                currentRuntimeValues.RestockSpan,
            OrderWaitingLimit = previousValues.OrderWaitingLimit ??
                currentRuntimeValues.OrderWaitingLimit,
            CraftQuota = previousValues.CraftQuota ??
                currentRuntimeValues.CraftQuota,
            DeliverySpanReduction = previousValues.DeliverySpanReduction ??
                currentRuntimeValues.DeliverySpanReduction
        };

        //현재 단계에서 새로 해금한 파츠만 다시 잠금
        foreach ( string id in currentValues.PartUnlockIds )
            rollbackValues.PartUnlockStates.Add( id, false );

        //현재 단계에서 새로 해금한 정보 표시만 다시 잠금
        foreach ( string id in currentValues.InformationUnlockIds )
            rollbackValues.InformationUnlockStates.Add( id, false );

        return rollbackValues;
    }

    #endregion

    #region ----- 적용/복구 -----

    /// <summary>
    /// 현재 단계 효과 적용
    /// </summary>
    /// <param name="effectValues">적용할 정비 효과값</param>
    /// <param name="previousValues">적용 실패 시 복구할 이전 상태</param>
    /// <returns>정비 효과 처리 결과</returns>
    MaintenanceEffectResult ApplyEffectValues (
        MaintenanceEffectValues effectValues,
        PreviousEffectValues previousValues )
    {
        //현재 단계의 효과값을 담당 모델에 순서대로 적용
        bool isApplied =
            ( effectValues.InventoryCapacity.HasValue == false ||
                _inventoryModel.SetCapacity(
                    effectValues.InventoryCapacity.Value ) ) &&
            ( effectValues.PartMaxStockBonus.HasValue == false ||
                _shopModel.SetPartMaxStockBonus(
                    effectValues.PartMaxStockBonus.Value ) ) &&
            ( effectValues.RestockSpan.HasValue == false ||
                _shopModel.SetPartRestockSpan(
                    effectValues.RestockSpan.Value ) ) &&
            ( effectValues.OrderWaitingLimit.HasValue == false ||
                _orderModel.SetWaitingLimit(
                    effectValues.OrderWaitingLimit.Value ) ) &&
            ( effectValues.CraftQuota.HasValue == false ||
                _businessDayModel.SetNextCraftLimit(
                    effectValues.CraftQuota.Value ) ) &&
            ( effectValues.DeliverySpanReduction.HasValue == false ||
                _deliveryModel.SetDeliverySpanReduction(
                    effectValues.DeliverySpanReduction.Value ) ) &&
            SetUnlockStates( effectValues.PartUnlockIds, true ) &&
            SetUnlockStates( effectValues.InformationUnlockIds, true );

        if ( isApplied )
            return MaintenanceEffectResult.Success;

        //중간 적용 실패 시 변경된 담당 모델을 이전 상태로 복구
        return RestoreEffectValues( effectValues, previousValues )
            ? MaintenanceEffectResult.ApplyFailed
            : MaintenanceEffectResult.RollbackFailed;
    }

    /// <summary>
    /// 공용 해금 상태 일괄 설정
    /// </summary>
    /// <param name="ids">상태를 변경할 해금 아이디 목록</param>
    /// <param name="isUnlocked">적용할 해금 상태</param>
    /// <returns>전체 해금 상태 변경 성공 여부</returns>
    bool SetUnlockStates (
        IEnumerable<string> ids, bool isUnlocked )
    {
        foreach ( string id in ids )
        {
            if ( _playStateModel.SetUnlocked( id, isUnlocked ) == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 변경 전 정비 효과값 복구
    /// </summary>
    /// <param name="effectValues">복구 대상 효과 종류</param>
    /// <param name="previousValues">효과 적용 전 담당 모델 상태</param>
    /// <returns>전체 담당 모델 상태 복구 성공 여부</returns>
    bool RestoreEffectValues (
        MaintenanceEffectValues effectValues,
        PreviousEffectValues previousValues )
    {
        bool isRestored = true;

        if ( effectValues.InventoryCapacity.HasValue )
            isRestored &= _inventoryModel.SetCapacity(
                previousValues.InventoryCapacity );

        if ( effectValues.PartMaxStockBonus.HasValue )
            isRestored &= _shopModel.SetPartMaxStockBonus(
                previousValues.PartMaxStockBonus );

        if ( effectValues.RestockSpan.HasValue )
            isRestored &= _shopModel.SetPartRestockSpan(
                previousValues.RestockSpan );

        if ( effectValues.OrderWaitingLimit.HasValue )
            isRestored &= _orderModel.SetWaitingLimit(
                previousValues.OrderWaitingLimit );

        if ( effectValues.CraftQuota.HasValue )
            isRestored &= _businessDayModel.SetNextCraftLimit(
                previousValues.CraftQuota );

        if ( effectValues.DeliverySpanReduction.HasValue )
            isRestored &= _deliveryModel.SetDeliverySpanReduction(
                previousValues.DeliverySpanReduction );

        foreach ( var pair in previousValues.PartUnlockStates )
            isRestored &= _playStateModel.SetUnlocked(
                pair.Key, pair.Value );

        foreach ( var pair in previousValues.InformationUnlockStates )
        {
            isRestored &= _playStateModel.SetUnlocked(
                pair.Key, pair.Value );
        }

        return isRestored;
    }

    #endregion
}
