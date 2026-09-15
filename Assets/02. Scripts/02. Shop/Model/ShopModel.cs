using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// 상점 상품 정렬 방식
/// </summary>
public enum ShopSortType
{
    Default,                //기본 순서
    NameAscending,          //이름 오름차순
    NameDescending,         //이름 내림차순
    PriceAscending,         //가격 낮은 순
    PriceDescending,        //가격 높은 순
    StockAscending,         //남은 재고 적은 순
    StockDescending,        //남은 재고 많은 순
}

/// <summary>
/// 상점 모델 - 전체 상품 생성, 조회, 필터, 정렬, 상태 관리
/// </summary>
public class ShopModel
{

    /// <summary>
    /// 상품 모델 딕셔너리(상품 아이디, 상품 모델)
    /// </summary>
    Dictionary<string, ShopItemModel> _items = new Dictionary<string, ShopItemModel>( );
    int _partMaxStockBonus;       //파츠 최대 재고 보정값
    int _partRestockSpan;       //파츠 재입고 간격 정비값

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 상품 모델 목록(읽기 전용)
    /// </summary>
    public IReadOnlyCollection<ShopItemModel> Items => _items.Values;
    /// <summary>
    /// 등록 상품 수
    /// </summary>
    public int Count => _items.Count;
    /// <summary>
    /// 파츠 최대 재고 보정값
    /// </summary>
    public int PartMaxStockBonus => _partMaxStockBonus;

    /// <summary>
    /// 파츠 재입고 간격 정비값, 0이면 기본값
    /// </summary>
    public int PartRestockSpan => _partRestockSpan;

    #endregion

    #region ----- 초기화 -----
    /// <summary>
    /// 상점 모델 생성자
    /// </summary>
    /// <param name="dataMap">구매 가능 상품 데이터 맵</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    public ShopModel ( PurchasableDataMap dataMap, int totalDay )
    {
        if ( dataMap == null )
            throw new ArgumentNullException(
                nameof( dataMap ), "구매 가능 상품 데이터 없음" );

        Init( dataMap.PurchasableDatas, totalDay );
    }

    /// <summary>
    /// 상품 목록 초기화
    /// </summary>
    /// <param name="datas">구매 가능 상품 목록</param>
    /// <param name="totalDay">현재 누적 영업일</param>
    void Init ( IReadOnlyList<PurchasableData> datas, int totalDay )
    {
        _items.Clear( );

        if ( datas == null ) return;

        foreach ( PurchasableData data in datas )
        {
            if ( data == null || string.IsNullOrEmpty( data.Id ) ) continue;

            var item = new ShopItem( data, totalDay );
            var itemModel = new ShopItemModel( item );

            if ( _items.TryAdd( data.Id, itemModel ) == false )
                UnityEngine.Debug.LogWarning(
                    $"{data.Id}에 이미 상품이 존재합니다." );
        }
    }
    #endregion

    #region ----- 상품 조회 및 정렬 -----
    /// <summary>
    /// 상품 모델 조회
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="itemModel">상품 모델</param>
    /// <returns>상품 모델 조회 성공 여부</returns>
    public bool TryGetItem ( string id, out ShopItemModel itemModel )
    {
        //기본 반환값 설정
        itemModel = null;

        //빈 아이디 차단
        if ( string.IsNullOrEmpty( id ) ) return false;

        //아이디에 맞는 모델 반환
        return _items.TryGetValue( id, out itemModel );
    }

    /// <summary>
    /// 상품 목록 정렬
    /// </summary>
    /// <param name="items">정렬할 상품 목록</param>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>정렬된 상품 목록</returns>
    IReadOnlyList<ShopItemModel> SortItems ( IEnumerable<ShopItemModel> items, ShopSortType sortType )
    {
        //선택한 방식으로 상품 정렬
        switch ( sortType )
        {
            case ShopSortType.NameAscending:
                items = items.OrderBy( model => model.Item.Data.Name, StringComparer.Ordinal );
                break;

            case ShopSortType.NameDescending:
                items = items.OrderByDescending( model => model.Item.Data.Name, StringComparer.Ordinal );
                break;

            case ShopSortType.PriceAscending:
                items = items.OrderBy( model => model.Item.CurrentPrice );
                break;

            case ShopSortType.PriceDescending:
                items = items.OrderByDescending( model => model.Item.CurrentPrice );
                break;

            case ShopSortType.StockAscending:
                items = items.OrderBy( model => model.Item.RemainingStock );
                break;

            case ShopSortType.StockDescending:
                items = items.OrderByDescending( model => model.Item.RemainingStock );
                break;
        }

        //정렬 결과 반환
        return items.ToList( );
    }

    /// <summary>
    /// 전체 상품 목록 조회
    /// </summary>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>전체 상품 목록</returns>
    public IReadOnlyList<ShopItemModel> GetAllItems ( ShopSortType sortType )
    {
        //타입에 따라 정렬된 상품 목록 반환
        return SortItems( _items.Values, sortType );
    }

    /// <summary>
    /// 상품 타입별 목록 조회
    /// </summary>
    /// <param name="productType">상품 타입</param>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>상품 타입별 목록</returns>
    public IReadOnlyList<ShopItemModel> GetItems ( ProductType productType, ShopSortType sortType )
    {
        //같은 상품 타입만 조회
        IEnumerable<ShopItemModel> items = _items.Values
            .Where( model => model.Item.Data.ProductType == productType );

        //조회한 상품 정렬
        return SortItems( items, sortType );
    }

    /// <summary>
    /// 파츠 타입별 목록 조회
    /// </summary>
    /// <param name="partType">파츠 타입</param>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>파츠 타입별 목록</returns>
    public IReadOnlyList<ShopItemModel> GetParts ( PartType partType, ShopSortType sortType )
    {
        //같은 파츠 타입만 조회
        IEnumerable<ShopItemModel> items = _items.Values
            .Where( model => model.Item.Data is PartsData partsData &&
                partsData.PartType == partType );

        //조회한 파츠 정렬
        return SortItems( items, sortType );
    }

    /// <summary>
    /// 전체 파츠 데이터 조회
    /// </summary>
    /// <returns>전체 파츠 데이터 목록</returns>
    public IReadOnlyList<PartsData> GetAllParts ()
    {
        var parts = new List<PartsData>( );

        //파츠 데이터만 목록에 추가
        foreach ( var itemModel in _items.Values )
        {
            if ( itemModel.Item.Data is PartsData partsData )
                parts.Add( partsData );
        }

        return parts;
    }

    /// <summary>
    /// 시설 관련 상품 목록 조회
    /// </summary>
    /// <param name="sortType">정렬 방식</param>
    /// <returns>시설 관련 상품 목록</returns>
    public IReadOnlyList<ShopItemModel> GetFacilities ( ShopSortType sortType )
    {
        //시설, 장비, 가구 상품만 조회
        IEnumerable<ShopItemModel> items = _items.Values.Where(
            model =>
                model.Item.Data.ProductType == ProductType.Expansion ||
                model.Item.Data.ProductType == ProductType.Equipment ||
                model.Item.Data.ProductType == ProductType.Furniture
        );

        //조회한 상품 정렬
        return SortItems( items, sortType );
    }
    /// <summary>
    /// 장바구니에 추가할 수 있는 최대 수량 조회
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="cartQuantity">현재 장바구니 수량</param>
    /// <param name="quantity">추가 가능 수량</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetAddableQuantity ( string id, int cartQuantity, out int quantity )
    {
        quantity = 0;

        //잘못된 장바구니 수량 차단
        if ( cartQuantity < 0 ) return false;

        //상품 조회
        if ( TryGetItem( id, out var itemModel ) == false ) return false;

        //남은 재고에서 장바구니 수량 제외
        quantity = itemModel.Item.RemainingStock - cartQuantity;

        return quantity > 0;
    }


    #endregion

    #region ----- 구매 처리 -----
    /// <summary>
    /// 상품 구매 가능 여부 확인
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">구매 수량</param>
    /// <returns>상품 구매 가능 여부</returns>
    public bool CanPurchase ( string id, int quantity )
    {
        //상품 조회 후 구매 가능 여부 확인
        return TryGetItem( id, out var itemModel ) && itemModel.CanPurchase( quantity );
    }

    /// <summary>
    /// 구매 수량 변경
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="currentQuantity">현재 선택 수량</param>
    /// <param name="amount">변경 수량</param>
    /// <param name="changedQuantity">변경된 수량</param>
    /// <returns>변경 성공 여부</returns>
    public bool ChangePurchaseQuantity ( string id, int currentQuantity, int amount, out int changedQuantity )
    {
        //기본 반환 수량 설정
        changedQuantity = currentQuantity;

        //잘못된 현재 수량과 변경값 차단
        if ( currentQuantity <= 0 || amount == 0 ) return false;

        //정수 범위 초과를 방지하며 변경 수량 계산
        long quantity = ( long ) currentQuantity + amount;

        //1 미만 또는 정수 범위 초과 차단
        if ( quantity <= 0 || quantity > int.MaxValue ) return false;

        //변경 수량 구매 가능 여부 확인
        if ( CanPurchase( id, ( int ) quantity ) == false ) return false;

        //변경된 수량 반환
        changedQuantity = ( int ) quantity;

        return true;
    }

    /// <summary>
    /// 상품 재고 차감
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">차감할 수량</param>
    /// <returns>재고 차감 성공 여부</returns>
    public bool RemoveStock ( string id, int quantity )
    {
        //상품 조회 후 재고 차감
        return TryGetItem( id, out var itemModel ) && itemModel.RemoveStock( quantity );
    }
    #endregion

    #region ----- 장바구니 -----
    /// <summary>
    /// 장바구니 최종 수량 확인
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">설정할 장바구니 수량</param>
    /// <returns>설정 가능 여부</returns>
    public bool CanSetCartQuantity ( string id, int quantity )
    {
        //상품이 없으면 종료
        if ( TryGetItem( id, out _ ) == false ) return false;

        //수량 0은 장바구니 제거로 허용
        if ( quantity == 0 ) return true;

        //음수 입력 차단
        if ( quantity < 0 ) return false;

        //현재 판매 가능 수량 확인
        return CanPurchase( id, quantity );
    }

    /// <summary>
    /// 장바구니 수량 변경값 계산
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="currentQuantity">현재 장바구니 수량</param>
    /// <param name="amount">변경 수량</param>
    /// <param name="changedQuantity">변경된 수량</param>
    /// <returns>변경 가능 여부</returns>
    public bool ChangeCartQuantity ( string id, int currentQuantity, int amount, out int changedQuantity )
    {
        changedQuantity = currentQuantity;

        //변경 후 수량 계산
        long quantity = ( long ) currentQuantity + amount;

        //음수 또는 정수 범위 초과 거부
        if ( quantity < 0 || quantity > int.MaxValue ) return false;

        //변경 수량 검증
        if ( CanSetCartQuantity( id, ( int ) quantity ) == false ) return false;

        changedQuantity = ( int ) quantity;
        return true;
    }

    /// <summary>
    /// 장바구니 수량 추가 가능 여부 확인
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="cartQuantity">현재 장바구니 수량</param>
    /// <param name="addQuantity">추가할 수량</param>
    /// <returns>추가 가능 여부</returns>
    public bool CanAddToCart ( string id, int cartQuantity, int addQuantity )
    {
        //잘못된 장바구니 수량과 추가 수량 차단
        if ( cartQuantity < 0 || addQuantity <= 0 ) return false;

        //정수 범위 초과를 방지하며 최종 수량 계산
        long totalQuantity = ( long ) cartQuantity + addQuantity;

        //정수 범위 초과 차단
        if ( totalQuantity > int.MaxValue ) return false;

        //장바구니 최종 수량 구매 가능 여부 반환
        return CanPurchase( id, ( int ) totalQuantity );
    }
    #endregion

    #region ----- 재고 관리 -----
    /// <summary>
    /// 상품 부분 입고
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="quantity">추가 수량</param>
    /// <returns>상품 입고 성공 여부</returns>
    public bool AddStock ( string id, int quantity )
    {
        //상품 조회 후 재고 추가
        return TryGetItem( id, out var itemModel ) && itemModel.AddStock( quantity );
    }

    /// <summary>
    /// 상품 최대 재고 수량 증가
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <param name="amount">증가 수량</param>
    /// <returns>최대 재고 증가 성공 여부</returns>
    public bool ExpandMaxStock ( string id, int amount )
    {
        //조회 후 최대 재고 증가
        return TryGetItem( id, out var itemModel ) && itemModel.ExpandMaxStock( amount );
    }

    /// <summary>
    /// 모든 파츠 상품의 최대 재고 보정값 설정
    /// </summary>
    /// <param name="bonus">기본 최대 재고에 더할 최종 보정값</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetPartMaxStockBonus ( int bonus )
    {
        if ( bonus < 0 ) return false;

        //모든 파츠 상품의 최종 최대 재고 사전 검증
        foreach ( ShopItemModel itemModel in _items.Values )
        {
            if ( itemModel.Item.Data is not PartsData ) continue;

            int baseStock = itemModel.Item.Data.BaseStockQuantity;

            if ( baseStock <= 0 || baseStock > int.MaxValue - bonus ||
                baseStock + bonus < itemModel.Item.RemainingStock )
                return false;
        }

        //현재 재고를 채우지 않고 최대 재고만 변경
        foreach ( ShopItemModel itemModel in _items.Values )
        {
            if ( itemModel.Item.Data is not PartsData ) continue;

            if ( itemModel.SetMaxStock(
                itemModel.Item.Data.BaseStockQuantity + bonus ) == false )
                return false;
        }

        _partMaxStockBonus = bonus;
        return true;
    }

    /// <summary>
    /// 모든 파츠 상품의 다음 재입고 간격 설정
    /// </summary>
    /// <param name="span">최종 재입고 간격, 0이면 상품 기본값</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetPartRestockSpan ( int span )
    {
        //정비 적용값은 최소 3일 보장
        if ( span != 0 && span < 3 ) return false;

        //모든 파츠 상품의 적용 간격 사전 검증
        foreach ( ShopItemModel itemModel in _items.Values )
        {
            if ( itemModel.Item.Data is not PartsData ) continue;

            int appliedSpan = span == 0
                ? itemModel.Item.Data.BaseRestockSpan
                : span;

            if ( appliedSpan <= 0 ) return false;
        }

        //현재 재입고 예정일은 유지하고 다음 주기 간격만 예약
        foreach ( ShopItemModel itemModel in _items.Values )
        {
            if ( itemModel.Item.Data is not PartsData ) continue;

            int appliedSpan = span == 0
                ? itemModel.Item.Data.BaseRestockSpan
                : span;

            if ( itemModel.ScheduleRestockSpan( appliedSpan ) == false )
                return false;
        }

        _partRestockSpan = span;
        return true;
    }

    /// <summary>
    /// 개별 상품 재입고
    /// </summary>
    /// <param name="id">상품 아이디</param>
    /// <returns>상품 재입고 성공 여부</returns>
    public bool Restock ( string id )
    {
        //상품 조회
        if ( TryGetItem( id, out var itemModel ) == false ) return false;

        //남은 수량을 최대 재고 수량으로 갱신
        itemModel.Restock( );

        return true;
    }

    /// <summary>
    /// 전체 상품 재입고
    /// </summary>
    public void RestockAll ()
    {
        foreach ( var item in _items.Values )
        {
            item.Restock( );
        }
    }

    /// <summary>
    /// 지정 영업일의 일반 재입고 처리
    /// </summary>
    /// <param name="totalDay">새 누적 영업일</param>
    public void RestockForDay ( int totalDay )
    {
        foreach ( ShopItemModel itemModel in _items.Values )
            itemModel.RestockForDay( totalDay );
    }
    #endregion

    #region ----- 저장 복구 -----
    /// <summary>
    /// 상점 저장 데이터 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveData">복구할 상점 데이터</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore ( ShopSaveData saveData )
    {
        if ( saveData == null ||
            saveData.Items == null ||
            saveData.PartMaxStockBonus < 0 ||
            ( saveData.PartRestockSpan != 0 &&
            saveData.PartRestockSpan < 3 ) )
        {
            return false;
        }

        var ids = new HashSet<string>( );

        foreach ( ShopItemSaveData itemData in saveData.Items )
        {
            if ( itemData == null ||
                string.IsNullOrWhiteSpace( itemData.Id ) == true ||
                ids.Add( itemData.Id ) == false ||
                TryGetItem( itemData.Id, out _ ) == false ||
                itemData.CurrentPrice < 0f ||
                float.IsNaN( itemData.CurrentPrice ) == true ||
                float.IsInfinity( itemData.CurrentPrice ) == true ||
                itemData.MaxStock <= 0 ||
                itemData.RemainingStock < 0 ||
                itemData.RemainingStock > itemData.MaxStock ||
                itemData.RestockSpan < 0 ||
                itemData.NextRestockDay < 0 ||
                itemData.PendingRestockSpan < 0 )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 저장 데이터로 상점 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 상점 데이터</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore ( ShopSaveData saveData )
    {
        //현재 상태를 변경하기 전에 전체 데이터 검사
        if ( CanRestore( saveData ) == false )
            return false;

        foreach ( ShopItemSaveData itemData in saveData.Items )
        {
            //상품 모델 가져오기
            TryGetItem( itemData.Id, out ShopItemModel itemModel );

            //상품 상태 가져오기
            ShopItem item = itemModel.Item;

            item.SetPrice( itemData.CurrentPrice );
            item.SetMaxStock( itemData.MaxStock );
            item.SetStock( itemData.RemainingStock );

            item.SetRestockSpan( itemData.RestockSpan );
            item.SetNextRestockDay( itemData.NextRestockDay );
            item.SetPendingRestockSpan( itemData.PendingRestockSpan );
        }

        _partMaxStockBonus = saveData.PartMaxStockBonus;
        _partRestockSpan = saveData.PartRestockSpan;

        return true;
    }
    #endregion
}
