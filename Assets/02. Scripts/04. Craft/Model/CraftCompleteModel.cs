using System;
using System.Collections.Generic;

/// <summary>
/// 제작 확정 처리 결과
/// </summary>
public enum CraftCompleteResult
{
    Success,        //제작 확정 성공
    InvalidOrder,       //잘못된 주문
    InvalidParts,       //잘못된 배치 파츠
    MissingMinimumParts,        //최소 제작 조건 미달성
    CostExceeded,       //최대 제작 코스트 초과
    PartCountExceeded,      //최대 파츠 개수 초과
    ExcludedThemeUsed,      //제외 테마 사용
    NotEnoughParts,     //인벤토리 파츠 부족
    AlreadyCompleted,       //이미 확정된 주문
    InventoryUpdateFailed,      //인벤토리 소비 실패
    InventoryRollbackFailed,        //인벤토리 복구 실패
    OrderUpdateFailed,      //주문 상태 변경 실패
}

/// <summary>
/// 제작 확정 모델 - 파츠 소비, 주문 상태 변경과 제작 결과 저장
/// </summary>
public class CraftCompleteModel
{
    CraftModel _craftModel;      //현재 제작 편집 상태
    CraftReviewModel _reviewModel;       //제작 판정 모델
    InventoryModel _inventoryModel;      //인벤토리 모델
    CustomerOrderModel _orderModel;      //주문 모델

    /// <summary>
    /// 주문별 확정 제작 결과
    /// </summary>
    Dictionary<string , CraftResult> _craftResults = new Dictionary<string , CraftResult> ( );

    /// <summary>
    /// 제작 확정 완료 이벤트
    /// </summary>
    public event Action<CraftResult> OnCraftCompleted;

    /// <summary>
    /// 제작 확정 모델 생성
    /// </summary>
    /// <param name="craftModel">제작 모델</param>
    /// <param name="reviewModel">제작 판정 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="orderModel">고객 주문 모델</param>
    public CraftCompleteModel (
        CraftModel craftModel , CraftReviewModel reviewModel ,
        InventoryModel inventoryModel , CustomerOrderModel orderModel )
    {
        _craftModel = craftModel;
        _reviewModel = reviewModel;
        _inventoryModel = inventoryModel;
        _orderModel = orderModel;
    }

    #region ----- 제작 확정 -----
    /// <summary>
    /// 현재 제작물 확정
    /// </summary>
    /// <param name="craftResult">확정된 제작 결과</param>
    /// <returns>제작 확정 처리 결과</returns>
    public CraftCompleteResult Complete ( out CraftResult craftResult )
    {
        craftResult = null;

        //선택한 제작 주문 확인
        if ( _craftModel.HasSelectedOrder == false )
            return CraftCompleteResult.InvalidOrder;

        string orderId = _craftModel.SelectedOrderId;

        //주문 조회
        if ( _orderModel.GetOrder ( orderId , out CustomerOrder order ) == false )
            return CraftCompleteResult.InvalidOrder;

        //같은 주문의 중복 제작 확정 차단
        if ( _craftResults.ContainsKey ( orderId ) )
            return CraftCompleteResult.AlreadyCompleted;

        //제작 중 상태 확인
        if ( order.ProgressState != OrderProgressState.Production )
            return CraftCompleteResult.InvalidOrder;

        //현재 배치 최종 판정
        CraftReviewResult reviewResult = _reviewModel.Review (
            order , _craftModel.PlacedParts ,
            _craftModel.RootPlacementNumber ,
            out CraftReviewData reviewData );

        //제작 판정 실패 결과 변환
        CraftCompleteResult result = ConvertReviewResult ( reviewResult );

        if ( result != CraftCompleteResult.Success )
            return result;

        //인벤토리에서 소비할 파츠 목록 생성
        if ( TryCreateUsedParts (
            _craftModel.PlacedParts ,
            out List<InventoryItemAmount> usedParts ) == false )
            return CraftCompleteResult.InvalidParts;

        //전체 파츠 소비 가능 여부 사전 확인
        if ( _inventoryModel.CanRemoveItems ( usedParts ) == false )
            return CraftCompleteResult.NotEnoughParts;

        //현재 배치와 판정을 복사한 임시 결과 생성
        CraftResult pendingResult = new CraftResult
        {
            OrderId = orderId ,
            RootPlacementNumber = _craftModel.RootPlacementNumber ,
            PlacedParts = CopyPlacedParts ( _craftModel.PlacedParts ) ,
            ReviewData = reviewData
        };

        //인벤토리 파츠 일괄 소비
        if ( _inventoryModel.RemoveItems ( usedParts ) == false )
            return CraftCompleteResult.InventoryUpdateFailed;

        //주문 상태 변경 이벤트보다 먼저 제작 결과 등록
        _craftResults.Add ( orderId , pendingResult );

        //주문 제작 완료 상태로 변경
        if ( _orderModel.CompleteOrder ( orderId ) != OrderResult.Success )
        {
            //임시 등록한 제작 결과 제거
            _craftResults.Remove ( orderId );

            //소비한 인벤토리 파츠 복구
            if ( _inventoryModel.AddItems ( usedParts ) == false )
                return CraftCompleteResult.InventoryRollbackFailed;

            return CraftCompleteResult.OrderUpdateFailed;
        }

        //완료된 편집 상태 초기화
        _craftModel.ClearOrder ( );

        craftResult = pendingResult;

        //성공한 제작 결과를 일일 기록으로 전달
        OnCraftCompleted?.Invoke ( craftResult );

        return CraftCompleteResult.Success;
    }

    /// <summary>
    /// 제작 판정 결과를 제작 확정 결과로 변환
    /// </summary>
    /// <param name="reviewResult">제작 판정 처리 결과</param>
    /// <returns>제작 확정 처리 결과</returns>
    CraftCompleteResult ConvertReviewResult ( CraftReviewResult reviewResult )
    {
        switch ( reviewResult )
        {
            case CraftReviewResult.Success:
                return CraftCompleteResult.Success;

            case CraftReviewResult.InvalidOrder:
                return CraftCompleteResult.InvalidOrder;

            case CraftReviewResult.InvalidParts:
                return CraftCompleteResult.InvalidParts;

            case CraftReviewResult.MissingMinimumParts:
                return CraftCompleteResult.MissingMinimumParts;

            case CraftReviewResult.CostExceeded:
                return CraftCompleteResult.CostExceeded;

            case CraftReviewResult.PartCountExceeded:
                return CraftCompleteResult.PartCountExceeded;

            case CraftReviewResult.ExcludedThemeUsed:
                return CraftCompleteResult.ExcludedThemeUsed;

            default:
                return CraftCompleteResult.InvalidParts;
        }
    }
    #endregion

    #region ----- 제작 결과 조회 -----
    /// <summary>
    /// 주문의 확정 제작 결과 보유 여부 확인
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <returns>확정 제작 결과 보유 여부</returns>
    public bool HasResult ( string orderId )
    {
        if ( string.IsNullOrWhiteSpace ( orderId ) ) return false;

        return _craftResults.ContainsKey ( orderId );
    }

    /// <summary>
    /// 주문의 확정 제작 결과 조회
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <param name="craftResult">조회한 확정 제작 결과</param>
    /// <returns>확정 제작 결과 조회 성공 여부</returns>
    public bool GetResult ( string orderId , out CraftResult craftResult )
    {
        craftResult = null;

        if ( string.IsNullOrWhiteSpace ( orderId ) ) return false;

        return _craftResults.TryGetValue ( orderId , out craftResult );
    }

    /// <summary>
    /// 주문의 확정 제작 결과 폐기
    /// </summary>
    /// <param name="orderId">주문 아이디</param>
    /// <returns>확정 제작 결과 폐기 성공 여부</returns>
    public bool DiscardResult ( string orderId )
    {
        if ( string.IsNullOrWhiteSpace ( orderId ) ) return false;

        return _craftResults.Remove ( orderId );
    }
    #endregion

    #region ----- 확정 데이터 생성 -----
    /// <summary>
    /// 배치 파츠를 인벤토리 소비 목록으로 변환
    /// </summary>
    /// <param name="placedParts">확정할 배치 파츠 목록</param>
    /// <param name="usedParts">인벤토리에서 소비할 아이템 목록</param>
    /// <returns>소비 목록 생성 성공 여부</returns>
    bool TryCreateUsedParts (
        IReadOnlyList<PlacedPartData> placedParts ,
        out List<InventoryItemAmount> usedParts )
    {
        usedParts = null;

        if ( placedParts == null || placedParts.Count == 0 )
            return false;

        var quantities = new Dictionary<string , int> ( );
        var parts = new Dictionary<string , PartsData> ( );

        for ( int i = 0 ; i < placedParts.Count ; i++ )
        {
            PlacedPartData placedPart = placedParts [ i ];

            if ( placedPart?.PartData == null )
                return false;

            PartsData part = placedPart.PartData;

            parts [ part.Id ] = part;
            quantities.TryGetValue ( part.Id , out int quantity );
            quantities [ part.Id ] = quantity + 1;
        }

        usedParts = new List<InventoryItemAmount> ( quantities.Count );

        //아이템별 최종 소비 수량 생성
        foreach ( var pair in quantities )
        {
            usedParts.Add (
                new InventoryItemAmount (
                    parts [ pair.Key ] , pair.Value ) );
        }

        return true;
    }

    /// <summary>
    /// 확정할 배치 파츠 목록 복사
    /// </summary>
    /// <param name="placedParts">확정할 배치 파츠 목록</param>
    /// <returns>복사한 배치 파츠 목록</returns>
    List<PlacedPartData> CopyPlacedParts (
        IReadOnlyList<PlacedPartData> placedParts )
    {
        var copies = new List<PlacedPartData> ( placedParts.Count );

        for ( int i = 0 ; i < placedParts.Count ; i++ )
        {
            PlacedPartData source = placedParts [ i ];

            var copy = new PlacedPartData (
                source.PlacementNumber , source.PartData ,
                source.PartIndex , source.LocalPosition ,
                source.Scale );

            //생성자에서 받지 않는 회전값 복사
            copy.SetRotation ( source.Rotation );

            copies.Add ( copy );
        }

        return copies;
    }
    #endregion

    #region ----- 저장/복구 -----

    /// <summary>
    /// 현재 확정 제작 결과 저장 데이터 생성
    /// </summary>
    /// <returns>제작 세이브 데이터</returns>
    public CraftSaveData CreateSaveData ( )
    {
        //리스트 생성
        var results = new List<CraftResultSaveData> ( _craftResults.Count );

        //결과 추가
        foreach ( CraftResult result in _craftResults.Values )
            results.Add ( new CraftResultSaveData ( result ) );

        //세이브 데이터 반환
        return new CraftSaveData ( results );
    }

    /// <summary>
    /// 검증된 복구 상태로 확정 제작 결과 복구
    /// </summary>
    /// <param name="restoreState">확정 제작 결과 복구 상태</param>
    public void Restore ( CraftCompleteRestoreState restoreState )
    {
        //검증을 마친 주문별 확정 제작 결과 적용
        _craftResults = restoreState.CreateResults ( );
    }

    #endregion
}
