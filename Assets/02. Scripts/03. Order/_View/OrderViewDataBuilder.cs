using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 표시 데이터 생성기 - 주문 상태를 슬롯과 상세 표시값으로 변환
/// </summary>
public class OrderViewDataBuilder
{
    const int DeadlineAlertRemainingDay = 1;       //기한 임박 표시 기준

    OrderIconData _iconData;       //주문 공용 상태 아이콘 데이터

    /// <summary>
    /// 주문 표시 데이터 생성기 생성
    /// </summary>
    /// <param name="iconData">주문 공용 상태 아이콘 데이터</param>
    public OrderViewDataBuilder ( OrderIconData iconData )
    {
        _iconData = iconData;
    }

    #region ----- 주문 목록 -----
    /// <summary>
    /// 주문 슬롯 표시 데이터 목록 생성
    /// </summary>
    /// <param name="orders">표시할 주문 목록</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <returns>주문 슬롯 표시 데이터 목록</returns>
    public IReadOnlyList<OrderSlotViewData> CreateSlots (
        IReadOnlyList<CustomerOrder> orders,
        PlayStateModel playStateModel )
    {
        var viewDatas = new List<OrderSlotViewData>( orders.Count );

        for ( int i = 0; i < orders.Count; i++ )
            viewDatas.Add( CreateSlot( orders [ i ], playStateModel ) );

        return viewDatas;
    }

    /// <summary>
    /// 주문 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <returns>주문 슬롯 표시 데이터</returns>
    OrderSlotViewData CreateSlot (
        CustomerOrder order, PlayStateModel playStateModel )
    {
        playStateModel.GetDate(
            order.CreatedTotalDay, out int createdMonth, out int createdDay );

        return new OrderSlotViewData(
            order.OrderId, order.CreatedNumber.ToString( ),
            createdMonth, createdDay,
            GetSlotDeadlineText( order, playStateModel ),
            GetStateIcon( order ), order.IsSpecial,
            IsDeadlineImminent( order, playStateModel ) );
    }

    /// <summary>
    /// 주문 상태별 슬롯 기한 문구 생성
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <returns>상태별 기한 문구</returns>
    string GetSlotDeadlineText (
        CustomerOrder order, PlayStateModel playStateModel )
    {
        int totalDay;
        string label;
        bool showDueMark = true;

        switch ( order.ProgressState )
        {
            case OrderProgressState.Waiting:
                totalDay = order.AcceptDue;
                label = "수락 기한";
                break;

            case OrderProgressState.Production:
                totalDay = order.DeliveryDue;
                label = "납품 기한";
                break;

            case OrderProgressState.Crafted:
                totalDay = order.FinalDeadline;
                label = "최종 기한";
                break;

            case OrderProgressState.Shipping:
                return "배송 중";

            default:
                totalDay = order.ClosedTotalDay;
                label = "종료일";
                showDueMark = false;
                break;
        }

        playStateModel.GetDate( totalDay, out int month, out int day );

        return showDueMark
            ? $"{label}: ~{month}월 {day}일"
            : $"{label}: {month}월 {day}일";
    }
    #endregion

    #region ----- 주문 상세 -----
    /// <summary>
    /// 주문 상세 표시 데이터 생성
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <returns>주문 상세 표시 데이터</returns>
    public OrderDetailViewData CreateDetail (
        CustomerOrder order, PlayStateModel playStateModel,
        InventoryModel inventoryModel, ShopModel shopModel )
    {
        playStateModel.GetDate(
            order.CreatedTotalDay, out int createdMonth, out int createdDay );
        playStateModel.GetDate(
            order.DeliveryDue, out int deliveryMonth, out int deliveryDay );
        playStateModel.GetDate(
            order.FinalDeadline, out int finalDueMonth, out int finalDueDay );

        bool showMissingParts = playStateModel.IsUnlocked(
            InformationUnlockId.MissingPartsDisplay );

        GetActionTexts(
            order.ProgressState, out string confirmText,
            out string cancelText );

        return new OrderDetailViewData
        {
            OrderId = order.OrderId,
            OrderTitle = $"\" {order.Title} \"",
            MaxCraftCost = order.MaxCraftCost,
            CreatedMonth = createdMonth,
            CreatedDay = createdDay,
            DeliveryMonth = deliveryMonth,
            DeliveryDay = deliveryDay,
            DeadlineText = GetDeadlineText( order, playStateModel ),
            IsDeadlineImminent =
                IsDeadlineImminent( order, playStateModel ),
            FinalDueMonth = finalDueMonth,
            FinalDueDay = finalDueDay,
            ShowFinalDue =
                order.ProgressState == OrderProgressState.Crafted,
            OrderState = GetStateText( order, playStateModel ),
            ConfirmText = confirmText,
            CancelText = cancelText,
            Requirements = CreatePartConditionViewDatas(
                order.Requirements, false, showMissingParts,
                inventoryModel, shopModel ),
            Wishes = CreatePartConditionViewDatas(
                order.Wishes, true, showMissingParts,
                inventoryModel, shopModel ),
            SpecialConditions = CreateSpecialConditionText( order )
        };
    }

    /// <summary>
    /// 주문 상태별 상세 버튼 문구 반환
    /// </summary>
    /// <param name="progressState">주문 진행 상태</param>
    /// <param name="confirmText">확인 버튼 문구</param>
    /// <param name="cancelText">취소 버튼 문구</param>
    void GetActionTexts (
        OrderProgressState progressState,
        out string confirmText, out string cancelText )
    {
        confirmText = string.Empty;
        cancelText = string.Empty;

        switch ( progressState )
        {
            case OrderProgressState.Waiting:
                confirmText = "수락";
                cancelText = "거절";
                break;

            case OrderProgressState.Production:
                confirmText = "제작하기";
                cancelText = "주문 취소";
                break;

            case OrderProgressState.Crafted:
                confirmText = "배송";
                cancelText = "주문 취소";
                break;
        }
    }

    /// <summary>
    /// 주문 특수 조건 표시 문구 생성
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <returns>특수 조건 표시 문구</returns>
    string CreateSpecialConditionText ( CustomerOrder order )
    {
        var texts = new List<string>( );

        if ( order.SpecialType == OrderSpecialType.Vip )
            texts.Add( "특수: VIP 주문" );

        if ( order.MaxPartCount > 0 )
            texts.Add( $"특수: 전체 파츠 {order.MaxPartCount}개 이하" );

        for ( int i = 0; i < order.TargetThemes.Count; i++ )
        {
            texts.Add(
                $"특수: {order.TargetThemes [ i ].GetDisplayName( )} 테마 목표" );
        }

        for ( int i = 0; i < order.ExcludedThemes.Count; i++ )
        {
            texts.Add(
                $"특수: {order.ExcludedThemes [ i ].GetDisplayName( )} 테마 제외" );
        }

        return string.Join( "\n", texts );
    }

    /// <summary>
    /// 현재 주문 기한 문구 생성
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <returns>현재 주문 기한 문구</returns>
    string GetDeadlineText (
        CustomerOrder order, PlayStateModel playStateModel )
    {
        if ( order.ProgressState == OrderProgressState.Closed )
            return string.Empty;

        int currentTotalDay = playStateModel.TotalDay;
        int remainDay =
            order.GetCurrentDeadline( currentTotalDay ) - currentTotalDay;

        if ( order.ProgressState == OrderProgressState.Waiting )
        {
            return remainDay < 0
                ? "수락 마감: 만료"
                : $"수락 마감: D-{remainDay}";
        }

        if ( currentTotalDay <= order.DeliveryDue )
            return $"납품 마감: D-{remainDay}";

        return remainDay < 0
            ? "최종 실패 기한: 만료"
            : $"최종 실패: D-{remainDay}";
    }

    /// <summary>
    /// 주문 기한 임박 여부 확인
    /// </summary>
    /// <param name="order">확인할 주문</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <returns>기한 임박 여부</returns>
    bool IsDeadlineImminent (
        CustomerOrder order, PlayStateModel playStateModel )
    {
        if ( playStateModel.IsUnlocked(
            InformationUnlockId.OrderDeadlineAlert ) == false )
            return false;

        if ( order.ProgressState == OrderProgressState.Shipping ||
            order.ProgressState == OrderProgressState.Closed )
            return false;

        int currentTotalDay = playStateModel.TotalDay;
        int remainDay =
            order.GetCurrentDeadline( currentTotalDay ) - currentTotalDay;

        return remainDay >= 0 && remainDay <= DeadlineAlertRemainingDay;
    }

    /// <summary>
    /// 파츠 조건 표시 문구 생성
    /// </summary>
    /// <param name="conditions">파츠 조건 목록</param>
    /// <param name="showMinimum">이상 문구 표시 여부</param>
    /// <param name="showMissingParts">부족 파츠 표시 여부</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <returns>파츠 조건 표시 문구</returns>
    public string [ ] CreatePartConditionTexts (
        IReadOnlyList<OrderPartCondition> conditions,
        bool showMinimum, bool showMissingParts,
        InventoryModel inventoryModel, ShopModel shopModel )
    {
        if ( conditions == null || conditions.Count == 0 )
            return Array.Empty<string>( );

        var texts = new string [ conditions.Count ];

        for ( int i = 0; i < conditions.Count; i++ )
        {
            OrderPartCondition condition = conditions [ i ];
            string minimumText = showMinimum ? " 이상" : string.Empty;
            string partName = condition.PartId;

            if ( shopModel.TryGetItem(
                condition.PartId, out var itemModel ) )
                partName = itemModel.Item.Data.Name;

            int missingQuantity = showMissingParts
                ? Mathf.Max( 0, condition.Quantity -
                    inventoryModel.GetQuantity( condition.PartId ) )
                : 0;

            string missingText = missingQuantity > 0
                ? $" / 부족 {missingQuantity}개"
                : string.Empty;

            texts [ i ] =
                $"{partName} {condition.Quantity}개" +
                $"{minimumText}{missingText}";
        }

        return texts;
    }

    /// <summary>
    /// 파츠 조건 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="conditions">파츠 조건 목록</param>
    /// <param name="showMinimum">이상 문구 표시 여부</param>
    /// <param name="showMissingParts">부족 파츠 표시 여부</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <returns>파츠 조건 슬롯 표시 데이터</returns>
    IReadOnlyList<OrderInfoSlotViewData> CreatePartConditionViewDatas (
        IReadOnlyList<OrderPartCondition> conditions,
        bool showMinimum, bool showMissingParts,
        InventoryModel inventoryModel, ShopModel shopModel )
    {
        if ( conditions == null || conditions.Count == 0 )
            return Array.Empty<OrderInfoSlotViewData>( );

        var viewDatas = new OrderInfoSlotViewData [ conditions.Count ];

        for ( int i = 0; i < conditions.Count; i++ )
        {
            OrderPartCondition condition = conditions [ i ];
            string partName = condition.PartId;
            Sprite icon = null;

            if ( shopModel.TryGetItem(
                condition.PartId, out ShopItemModel itemModel ) )
            {
                partName = itemModel.Item.Data.Name;
                icon = itemModel.Item.Data.Icon;
            }

            int missingQuantity = showMissingParts
                ? Mathf.Max( 0, condition.Quantity -
                    inventoryModel.GetQuantity( condition.PartId ) )
                : 0;
            string minimumText = showMinimum ? " 이상" : string.Empty;
            string missingText = missingQuantity > 0
                ? $" / 부족 {missingQuantity}개"
                : string.Empty;

            viewDatas [ i ] = new OrderInfoSlotViewData(
                icon,
                partName,
                $"x {condition.Quantity}{minimumText}{missingText}" );
        }

        return viewDatas;
    }
    #endregion

    #region ----- 상태 표시 -----
    /// <summary>
    /// 주문 상태 아이콘 조회
    /// </summary>
    /// <param name="order">조회할 주문</param>
    /// <returns>주문 상태 아이콘</returns>
    Sprite GetStateIcon ( CustomerOrder order )
    {
        return _iconData.GetIcon( order.ProgressState );
    }

    /// <summary>
    /// 주문 상태 표시 문구 조회
    /// </summary>
    /// <param name="order">표시할 주문</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <returns>주문 상태 표시 문구</returns>
    string GetStateText (
        CustomerOrder order, PlayStateModel playStateModel )
    {
        switch ( order.ProgressState )
        {
            case OrderProgressState.Waiting:
                return "수락 대기 중";
            case OrderProgressState.Production:
                return "제작 중";
            case OrderProgressState.Crafted:
                return "배송 대기";
            case OrderProgressState.Shipping:
                return "배송 중";
        }

        playStateModel.GetDate(
            order.ClosedTotalDay, out int closedMonth, out int closedDay );
        string closedDate = $"({closedMonth}월 {closedDay}일)";

        switch ( order.Outcome )
        {
            case OrderOutcome.NormalDelivery:
                return $"정상 납품 {closedDate}";
            case OrderOutcome.LateDelivery:
                return $"지연 납품 {closedDate}";
            case OrderOutcome.Rejected:
                return $"거절 {closedDate}";
            case OrderOutcome.AutoRejected:
                return $"자동 거절 {closedDate}";
            case OrderOutcome.Cancelled:
                return $"주문 취소 {closedDate}";
            case OrderOutcome.Failed:
                return $"주문 실패 {closedDate}";
            default:
                return "종료";
        }
    }
    #endregion
}
