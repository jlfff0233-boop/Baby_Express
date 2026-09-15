using System;
using System.Collections.Generic;

/// <summary>
/// 제작 파츠 목록 정렬 방식
/// </summary>
public enum CraftPartSortType
{
    PartType,       //파츠 타입 순
    Name,       //이름 순
    QuantityDescending,       //수량 많은 순
    CostAscending,       //코스트 낮은 순
}

/// <summary>
/// 제작 파츠 목록 프레젠터 - 보유 수량과 배치 가능 수량 표시 중재
/// </summary>
public class CraftPartListPresenter
{
    CraftModel _craftModel;               //제작 모델
    InventoryModel _inventoryModel;       //인벤토리 모델
    PartsSelectView _partsSelectView;     //파츠 선택 뷰
    CraftPartsViewDataBuilder _viewDataBuilder;       //파츠 표시 데이터 빌더
    /// <summary>
    /// 현재 제작 주문 조회
    /// </summary>
    Func<CustomerOrder> _getSelectedOrder;

    CraftPartSortType _selectedSortType =
        CraftPartSortType.PartType;       //현재 파츠 정렬 방식

    bool _isSubscribed;                   //이벤트 연결 여부

    /// <summary>
    /// 제작 파츠 목록 프레젠터 생성
    /// </summary>
    /// <param name="craftModel">제작 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="partsSelectView">파츠 선택 뷰</param>
    /// <param name="viewDataBuilder">제작 파츠 표시 데이터 생성기</param>
    /// <param name="getSelectedOrder">현재 제작 주문 조회 함수</param>
    public CraftPartListPresenter (
        CraftModel craftModel, InventoryModel inventoryModel,
        PartsSelectView partsSelectView,
        CraftPartsViewDataBuilder viewDataBuilder,
        Func<CustomerOrder> getSelectedOrder )
    {
        _craftModel = craftModel;
        _inventoryModel = inventoryModel;
        _partsSelectView = partsSelectView;
        _viewDataBuilder = viewDataBuilder;
        _getSelectedOrder = getSelectedOrder;
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 인벤토리 변경 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        //인벤토리 갱신 이벤트 연결
        _inventoryModel.OnInventoryChanged += Refresh;
        //정렬 방식 변경 이벤트 변경
        _partsSelectView.OnSortSelected += SelectSort;
        _isSubscribed = true;
    }

    /// <summary>
    /// 인벤토리 변경 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _inventoryModel.OnInventoryChanged -= Refresh;
        _partsSelectView.OnSortSelected -= SelectSort;
        _isSubscribed = false;
    }
    #endregion

    #region ----- 파츠 슬롯 -----
    /// <summary>
    /// 보유 파츠 슬롯 전체 갱신
    /// </summary>
    public void Refresh ()
    {
        //제작 주문이 없으면 슬롯 초기화
        if ( _getSelectedOrder( ) == null )
        {
            Clear( );
            return;
        }

        //인벤토리 아이템 목록 생성
        var partItems = new List<InventoryItem>( );

        foreach ( InventoryItem item in _inventoryModel.Items )
        {
            //파츠가 아니면 다음 아이템 확인
            if ( item.Data is not PartsData partData ) continue;

            //현재 배치 가능한 남은 수량 계산
            int remainingQuantity = item.Quantity -
                _craftModel.GetPartQuantity( partData.Id );

            //남은 수량이 없으면 슬롯에 표시하지 않음
            if ( remainingQuantity <= 0 ) continue;

            partItems.Add( item );
        }

        //선택한 기준으로 순서 정렬
        partItems.Sort( CompareParts );

        //슬롯 데이터 뷰 목록 생성
        var viewDatas = new List<ItemSlotViewData>( partItems.Count );

        for ( int i = 0; i < partItems.Count; i++ )
        {
            //인벤토리 아이템 가져오기
            InventoryItem item = partItems [ i ];
            //파츠 데이터 가져오기
            PartsData partData = item.Data as PartsData;
            //남은 수량 가져오기
            int remainingQuantity = GetRemainingQuantity( item );

            //뷰 설정
            viewDatas.Add(
                _viewDataBuilder
                .CreatePartSlot( partData, remainingQuantity ) );
        }

        //파츠 선택 슬롯 표시
        _partsSelectView.ShowParts( viewDatas );
    }

    /// <summary>
    /// 파츠 선택 목록 초기화
    /// </summary>
    public void Clear ()
    {
        _partsSelectView.ClearParts( );
    }

    /// <summary>
    /// 파츠 목록 정렬 방식 변경
    /// </summary>
    /// <param name="sortIndex">선택한 정렬 번호</param>
    void SelectSort ( int sortIndex )
    {
        if ( sortIndex < 0 ||
            sortIndex > ( int ) CraftPartSortType.CostAscending )
        {
            return;
        }

        _selectedSortType = ( CraftPartSortType ) sortIndex;
        Refresh( );
    }

    /// <summary>
    /// 현재 정렬 방식에 따른 파츠 비교
    /// </summary>
    /// <param name="first">첫 번째 파츠 아이템</param>
    /// <param name="second">두 번째 파츠 아이템</param>
    /// <returns>정렬 비교값</returns>
    int CompareParts ( InventoryItem first, InventoryItem second )
    {
        PartsData firstData = first.Data as PartsData;
        PartsData secondData = second.Data as PartsData;
        int result;

        switch ( _selectedSortType )
        {
            case CraftPartSortType.Name:
                result = string.Compare(
                    firstData.Name, secondData.Name,
                    StringComparison.CurrentCulture );
                break;

            case CraftPartSortType.QuantityDescending:
                result = GetRemainingQuantity( second )
                    .CompareTo( GetRemainingQuantity( first ) );
                break;

            case CraftPartSortType.CostAscending:
                result = firstData.CraftCost
                    .CompareTo( secondData.CraftCost );
                break;

            default:
                result = firstData.PartType
                    .CompareTo( secondData.PartType );
                break;
        }

        //같은 우선순위는 이름과 아이디 순으로 고정
        if ( result == 0 )
            result = string.Compare(
                firstData.Name, secondData.Name,
                StringComparison.CurrentCulture );

        if ( result == 0 )
            result = string.CompareOrdinal(
                firstData.Id, secondData.Id );

        return result;
    }

    /// <summary>
    /// 현재 제작물에 배치하고 남은 파츠 수량 계산
    /// </summary>
    /// <param name="item">수량을 계산할 인벤토리 아이템</param>
    /// <returns>현재 배치 가능한 남은 수량</returns>
    int GetRemainingQuantity ( InventoryItem item )
    {
        return item.Quantity -
            _craftModel.GetPartQuantity( item.Data.Id );
    }
    #endregion
}
