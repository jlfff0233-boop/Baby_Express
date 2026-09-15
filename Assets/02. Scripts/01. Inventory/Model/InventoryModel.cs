using System;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 정렬 타입
/// </summary>
public enum InventorySortType
{
    Default,                //기본 순서
    NameAscending,          //이름 오름차순
    NameDescending,         //이름 내림차순
    PriceAscending,         //가격 낮은 순
    PriceDescending,        //가격 높은 순
    QuantityAscending,      //수량 적은 순
    QuantityDescending,     //수량 많은 순
}

/// <summary>
/// 인벤토리 모델 - 보유 아이템, 수량, 스택, 슬롯 용량 관리
/// </summary>
public class InventoryModel
{
    public const int MaxStackQuantity = 99;       //슬롯 하나의 최대 수량
    public const int DefaultCapacity = 50;        //기본 슬롯 용량

    /// <summary>
    /// 인벤토리 아이템 딕셔너리(아이디, 인벤토리 아이템)
    /// </summary>
    Dictionary<string, InventoryItem> _items = new Dictionary<string, InventoryItem>( );
    InventoryListBuilder _listBuilder;                 //슬롯 아이디 조회용 목록 빌더
    int _capacity;                                //현재 최대 슬롯 수

    /// <summary>
    /// 보유 상품 목록(읽기 전용)
    /// </summary>
    public IReadOnlyCollection<InventoryItem> Items => _items.Values;

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 보유 상품 종류 수
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// 현재 최대 슬롯 수
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// 현재 사용 슬롯 수
    /// </summary>
    public int UsedSlotCount => CalculateUsedSlots( );

    /// <summary>
    /// 현재 남은 슬롯 수
    /// </summary>
    public int RemainingSlotCount => _capacity - UsedSlotCount;

    /// <summary>
    /// 인벤토리 비어 있음 여부
    /// </summary>
    public bool IsEmpty => _items.Count == 0;
    #endregion

    /// <summary>
    /// 인벤토리 변경 이벤트
    /// </summary>
    public event Action OnInventoryChanged;

    /// <summary>
    /// 인벤토리 용량 변경 이벤트(용량)
    /// </summary>
    public event Action<int> OnCapacityChanged;

    /// <summary>
    /// 인벤토리 모델 생성
    /// </summary>
    /// <param name="capacity">초기 용량</param>
    public InventoryModel ( int capacity = DefaultCapacity )
    {
        //잘못된 개수 차단
        if ( capacity <= 0 )
            throw new ArgumentOutOfRangeException( nameof( capacity ) );

        _listBuilder = new InventoryListBuilder( );
        _capacity = capacity;
    }

    #region ----- 용량 계산 -----
    /// <summary>
    /// 아이템 데이터 확인
    /// </summary>
    /// <param name="data">확인할 아이템 데이터</param>
    /// <returns>유효 여부</returns>
    bool IsValidData ( PurchasableData data )
    {
        //아이템과 아이템 아이디 확인
        return data != null && string.IsNullOrWhiteSpace( data.Id ) == false;
    }

    /// <summary>
    /// 슬롯 용량을 사용하는 아이템인지 확인
    /// </summary>
    /// <param name="data">확인할 아이템 데이터</param>
    /// <returns>슬롯 용량 사용 여부</returns>
    bool UsesCapacity ( PurchasableData data )
    {
        //시설 관련 아이템은 인벤토리에 표시하지만 슬롯 용량을 사용하지 않음
        return data != null &&
            data.ProductType != ProductType.Expansion &&
            data.ProductType != ProductType.Equipment &&
            data.ProductType != ProductType.Furniture;
    }

    /// <summary>
    /// 수량에 필요한 스택 수 계산
    /// </summary>
    /// <param name="quantity">전체 아이템 수량</param>
    /// <returns>필요한 스택 수</returns>
    int CalculateStackCount ( long quantity )
    {
        //보유 수량이 없으면 슬롯을 사용하지 않음
        if ( quantity <= 0 ) return 0;

        //나머지가 있으면 스택 하나 추가
        return ( int ) ( ( quantity + MaxStackQuantity - 1 ) / MaxStackQuantity );
    }

    /// <summary>
    /// 현재 사용 슬롯 수 계산
    /// </summary>
    /// <returns>현재 사용 슬롯 수</returns>
    int CalculateUsedSlots ()
    {
        int usedSlotCount = 0;

        //용량을 사용하는 아이템별 필요한 스택 수 합산
        foreach ( var item in _items.Values )
        {
            if ( UsesCapacity( item.Data ) )
                usedSlotCount += CalculateStackCount( item.Quantity );
        }

        return usedSlotCount;
    }

    /// <summary>
    /// 예상 보유 수량의 전체 사용 슬롯 수 계산
    /// </summary>
    /// <param name="quantities">아이템별 예상 전체 수량</param>
    /// <param name="datas">아이템별 원본 데이터</param>
    /// <returns>예상 사용 슬롯 수</returns>
    long CalculateUsedSlots (
        IReadOnlyDictionary<string, long> quantities,
        IReadOnlyDictionary<string, PurchasableData> datas )
    {
        long usedSlotCount = 0;

        //용량을 사용하는 아이템별 예상 스택 수 합산
        foreach ( var pair in quantities )
        {
            if ( datas.TryGetValue( pair.Key, out var data ) && UsesCapacity( data ) )
                usedSlotCount += CalculateStackCount( pair.Value );
        }

        return usedSlotCount;
    }

    /// <summary>
    /// 아이템 추가 후 예상 수량 생성
    /// </summary>
    /// <param name="amounts">추가할 아이템 목록</param>
    /// <param name="quantities">아이템별 추가 후 전체 수량</param>
    /// <param name="datas">아이템별 원본 데이터</param>
    /// <returns>생성 성공 여부</returns>
    bool SimulateQuantities ( IReadOnlyList<InventoryItemAmount> amounts,
        out Dictionary<string, long> quantities,
        out Dictionary<string, PurchasableData> datas )
    {
        //기본값 설정
        quantities = new Dictionary<string, long>( );
        datas = new Dictionary<string, PurchasableData>( );

        //추가할 아이템이 없으면 종료
        if ( amounts == null || amounts.Count == 0 ) return false;

        //현재 인벤토리 상태 복사
        foreach ( var item in _items.Values )
        {
            quantities.Add( item.Data.Id, item.Quantity );
            datas.Add( item.Data.Id, item.Data );
        }

        //추가 후 수량 계산
        foreach ( var amount in amounts )
        {
            //잘못된 요청 차단
            if ( amount == null || IsValidData( amount.Data ) == false || amount.Quantity <= 0 )
                return false;

            string id = amount.Data.Id;

            //같은 아이디의 다른 아이템 데이터 차단
            if ( datas.TryGetValue( id, out var savedData ) &&
                ReferenceEquals( savedData, amount.Data ) == false )
                return false;

            //새 아이템 데이터 등록
            if ( datas.ContainsKey( id ) == false )
                datas.Add( id, amount.Data );

            //현재 또는 앞선 요청 수량 조회
            quantities.TryGetValue( id, out long currentQuantity );

            long changedQuantity = currentQuantity + amount.Quantity;

            //아이템별 전체 수량 정수 범위 초과 차단
            if ( changedQuantity > int.MaxValue ) return false;

            quantities [ id ] = changedQuantity;
        }

        return true;
    }

    /// <summary>
    /// 인벤토리 변경 이벤트 발행
    /// </summary>
    void NotifyChanged ()
    {
        OnInventoryChanged?.Invoke( );
    }
    #endregion

    #region ----- 아이템/스택 조회 -----
    /// <summary>
    /// 아이템 보유 여부 확인
    /// </summary>
    /// <param name="id">아이템 아이디</param>
    /// <returns>보유 여부</returns>
    public bool HasItem ( string id )
    {
        //빈 아이디 차단
        if ( string.IsNullOrWhiteSpace( id ) ) return false;

        return _items.ContainsKey( id );
    }

    /// <summary>
    /// 요구 수량 보유 여부 확인
    /// </summary>
    /// <param name="id">아이템 아이디</param>
    /// <param name="quantity">요구 수량</param>
    /// <returns>보유 여부</returns>
    public bool HasQuantity ( string id, int quantity )
    {
        //잘못된 요구 수량 차단
        if ( quantity <= 0 ) return false;

        //아이템 조회 후 수량 비교
        return GetItem( id, out var item ) && item.Quantity >= quantity;
    }

    /// <summary>
    /// 인벤토리 아이템 조회
    /// </summary>
    /// <param name="id">아이템 아이디</param>
    /// <param name="item">조회한 인벤토리 아이템</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetItem ( string id, out InventoryItem item )
    {
        item = null;

        //빈 아이디 차단
        if ( string.IsNullOrWhiteSpace( id ) ) return false;

        return _items.TryGetValue( id, out item );
    }

    /// <summary>
    /// 슬롯 아이디로 인벤토리 스택 조회
    /// </summary>
    /// <param name="slotId">조회할 슬롯 아이디</param>
    /// <param name="stack">조회한 인벤토리 스택</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetStack ( string slotId, out InventoryStack stack )
    {
        return _listBuilder.GetStack( Items, slotId, out stack );
    }

    /// <summary>
    /// 아이템 전체 보유 수량 조회
    /// </summary>
    /// <param name="id">아이템 아이디</param>
    /// <returns>전체 보유 수량</returns>
    public int GetQuantity ( string id )
    {
        //보유 아이템이 없으면 0 반환
        if ( GetItem( id, out var item ) == false ) return 0;

        return item.Quantity;
    }
    #endregion

    #region ----- 아이템 추가 -----
    /// <summary>
    /// 단일 아이템 추가 가능 여부 확인
    /// </summary>
    /// <param name="data">추가할 아이템 데이터</param>
    /// <param name="quantity">추가 수량</param>
    /// <returns>추가 가능 여부</returns>
    public bool CanAddItem ( PurchasableData data, int quantity )
    {
        //인벤토리 아이템 수량 배열 생성
        var amounts = new InventoryItemAmount [ ]
        {
            new InventoryItemAmount( data, quantity )
        };

        return CanAddItems( amounts );
    }

    /// <summary>
    /// 여러 아이템 추가 가능 여부 확인
    /// </summary>
    /// <param name="amounts">추가할 아이템 목록</param>
    /// <returns>추가 가능 여부</returns>
    public bool CanAddItems ( IReadOnlyList<InventoryItemAmount> amounts )
    {
        //아이템 추가 후 예상 수량 생성
        if ( SimulateQuantities( amounts, out var quantities, out var datas ) == false )
            return false;

        //예상 사용 슬롯 수와 현재 최대 슬롯 수 비교
        return CalculateUsedSlots( quantities, datas ) <= _capacity;
    }

    /// <summary>
    /// 단일 아이템 추가
    /// </summary>
    /// <param name="data">추가할 아이템 데이터</param>
    /// <param name="quantity">추가 수량</param>
    /// <returns>추가 성공 여부</returns>
    public bool AddItem ( PurchasableData data, int quantity )
    {
        //인벤토리 아이템 수량 배열 생성
        var amounts = new InventoryItemAmount [ ]
        {
            new InventoryItemAmount( data, quantity )
        };

        return AddItems( amounts );
    }

    /// <summary>
    /// 여러 아이템 일괄 추가
    /// </summary>
    /// <param name="amounts">추가할 아이템 목록</param>
    /// <returns>추가 성공 여부</returns>
    public bool AddItems ( IReadOnlyList<InventoryItemAmount> amounts )
    {
        //아이템 추가 후 예상 수량 생성
        if ( SimulateQuantities( amounts, out var quantities, out var datas ) == false )
            return false;

        //전체 인벤토리 용량 확인
        if ( CalculateUsedSlots( quantities, datas ) > _capacity )
            return false;

        //최종 수량 인벤토리에 반영
        foreach ( var pair in quantities )
        {
            string id = pair.Key;
            int finalQuantity = ( int ) pair.Value;

            //기존에 있던 아이템이면 증가분만 반영
            if ( _items.TryGetValue( id, out var item ) )
            {
                int addQuantity = finalQuantity - item.Quantity;

                if ( addQuantity > 0 )
                    item.AddQuantity( addQuantity );
            }
            else
            {
                //새 아이템 생성
                _items.Add( id, new InventoryItem( datas [ id ], finalQuantity ) );
            }
        }

        //이벤트 발행
        NotifyChanged( );
        return true;
    }
    #endregion

    #region ----- 아이템 제거 -----
    /// <summary>
    /// 아이템 수량 제거
    /// </summary>
    /// <param name="id">아이템 아이디</param>
    /// <param name="quantity">제거 수량</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveItem ( string id, int quantity )
    {
        //아이템이 없거나 제거할 수량이 없으면 종료
        if ( GetItem( id, out var item ) == false ||
            item.CanRemoveQuantity( quantity ) == false )
            return false;

        //아이템 수량 제거
        item.RemoveQuantity( quantity );

        //보유 수량이 없으면 아이템 항목 제거
        if ( item.Quantity == 0 )
            _items.Remove( id );

        NotifyChanged( );
        return true;
    }

    /// <summary>
    /// 여러 아이템 일괄 제거 가능 여부 확인
    /// </summary>
    /// <param name="amounts">제거할 아이템 목록</param>
    /// <returns>제거 가능 여부</returns>
    public bool CanRemoveItems ( IReadOnlyList<InventoryItemAmount> amounts )
    {
        return TryCreateRemovalQuantities( amounts, out _ );
    }

    /// <summary>
    /// 여러 아이템 일괄 제거
    /// </summary>
    /// <param name="amounts">제거할 아이템 목록</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveItems ( IReadOnlyList<InventoryItemAmount> amounts )
    {
        //전체 아이템 제거 가능 여부 확인
        if ( TryCreateRemovalQuantities(
            amounts, out Dictionary<string, int> removeQuantities ) == false )
            return false;

        //검증된 아이템 수량 일괄 제거
        foreach ( var pair in removeQuantities )
        {
            InventoryItem item = _items [ pair.Key ];

            item.RemoveQuantity( pair.Value );

            //보유 수량이 없으면 아이템 항목 제거
            if ( item.Quantity == 0 )
                _items.Remove( pair.Key );
        }

        //전체 제거 완료 후 한 번만 변경 알림
        NotifyChanged( );
        return true;
    }

    /// <summary>
    /// 아이템별 최종 제거 수량 생성
    /// </summary>
    /// <param name="amounts">제거할 아이템 목록</param>
    /// <param name="removeQuantities">아이템별 제거 수량</param>
    /// <returns>전체 제거 가능 여부</returns>
    bool TryCreateRemovalQuantities (
        IReadOnlyList<InventoryItemAmount> amounts,
        out Dictionary<string, int> removeQuantities )
    {
        removeQuantities = new Dictionary<string, int>( );

        //제거할 아이템이 없으면 종료
        if ( amounts == null || amounts.Count == 0 ) return false;

        for ( int i = 0; i < amounts.Count; i++ )
        {
            InventoryItemAmount amount = amounts [ i ];

            //잘못된 아이템 데이터와 수량 차단
            if ( amount == null || IsValidData( amount.Data ) == false ||
                amount.Quantity <= 0 )
                return false;

            string id = amount.Data.Id;

            //보유하지 않았거나 같은 아이디의 다른 데이터면 제거 불가
            if ( _items.TryGetValue( id, out InventoryItem item ) == false ||
                ReferenceEquals( item.Data, amount.Data ) == false )
                return false;

            //같은 아이템의 제거 요청 수량 합산
            removeQuantities.TryGetValue( id, out int currentQuantity );

            long changedQuantity = ( long ) currentQuantity + amount.Quantity;

            if ( changedQuantity > int.MaxValue ) return false;

            removeQuantities [ id ] = ( int ) changedQuantity;
        }

        //아이템별 최종 제거 수량 확인
        foreach ( var pair in removeQuantities )
        {
            if ( _items [ pair.Key ].CanRemoveQuantity( pair.Value ) == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// 선택한 스택에서 수량 제거 가능 여부 확인
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="quantity">제거 수량</param>
    /// <returns>제거 가능 여부</returns>
    public bool CanRemoveFromStack ( string slotId, int quantity )
    {
        //선택한 스택 범위 안의 수량만 제거 가능
        return GetStack( slotId, out var stack ) &&
            quantity > 0 && quantity <= stack.Quantity;
    }

    /// <summary>
    /// 선택한 스택에서 수량 제거
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="quantity">제거 수량</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveFromStack ( string slotId, int quantity )
    {
        //선택한 스택과 제거 수량 확인
        if ( GetStack( slotId, out var stack ) == false ||
            quantity <= 0 || quantity > stack.Quantity )
            return false;

        return RemoveItem( stack.ItemId, quantity );
    }
    #endregion

    #region ----- 용량 확장 -----
    /// <summary>
    /// 인벤토리 용량 설정
    /// </summary>
    /// <param name="capacity">변경할 최대 슬롯 수</param>
    /// <returns>용량 설정 성공 여부</returns>
    public bool SetCapacity ( int capacity )
    {
        //현재 사용 슬롯 개수보다 작은 값 차단
        if ( capacity <= 0 || capacity < UsedSlotCount )
            return false;

        //같은 용량은 추가 변경 없이 성공 처리
        if ( _capacity == capacity ) return true;

        _capacity = capacity;

        //변경된 최대 용량 전달
        OnCapacityChanged?.Invoke( _capacity );
        return true;
    }

    /// <summary>
    /// 인벤토리 용량 확장
    /// </summary>
    /// <param name="amount">추가 슬롯 수</param>
    /// <returns>확장 성공 여부</returns>
    public bool ExpandCapacity ( int amount )
    {
        //잘못된 추가량과 정수 범위 초과 차단
        if ( amount <= 0 || _capacity > int.MaxValue - amount )
            return false;

        _capacity += amount;

        //변경된 최대 용량 전달
        OnCapacityChanged?.Invoke( _capacity );
        return true;
    }
    #endregion

    #region ----- 저장 복구 -----
    /// <summary>
    /// 인벤토리 저장 데이터 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveData">복구할 인벤토리 데이터</param>
    /// <param name="dataMap">상품 데이터 맵</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore (
        InventorySaveData saveData,
        PurchasableDataMap dataMap )
    {
        if ( saveData == null ||
            dataMap == null ||
            saveData.Capacity <= 0 ||
            saveData.Items == null )
        {
            return false;
        }

        var ids = new HashSet<string>( );
        long usedSlotCount = 0;

        foreach ( InventoryItemSaveData itemData in saveData.Items )
        {
            if ( itemData == null ||
                string.IsNullOrWhiteSpace( itemData.Id ) == true ||
                itemData.Quantity <= 0 ||
                ids.Add( itemData.Id ) == false ||
                dataMap.TryGetData(
                    itemData.Id, out PurchasableData data ) == false )
            {
                return false;
            }

            //용량을 사용하는 아이템만 필요한 슬롯 수 계산
            if ( UsesCapacity( data ) == true )
                usedSlotCount += CalculateStackCount( itemData.Quantity );
        }

        return usedSlotCount <= saveData.Capacity;
    }

    /// <summary>
    /// 저장 데이터로 인벤토리 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 인벤토리 데이터</param>
    /// <param name="dataMap">상품 데이터 맵</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore (
        InventorySaveData saveData, PurchasableDataMap dataMap )
    {
        //현재 상태를 변경하기 전에 전체 데이터 검사
        if ( CanRestore( saveData, dataMap ) == false )
            return false;

        var restoredItems = new Dictionary<string, InventoryItem>( );

        //검증된 저장 데이터로 새 아이템 목록 생성
        foreach ( InventoryItemSaveData itemData in saveData.Items )
        {
            dataMap.TryGetData( itemData.Id, out PurchasableData data );

            restoredItems.Add(
                itemData.Id, new InventoryItem( data, itemData.Quantity ) );
        }

        bool capacityChanged = _capacity != saveData.Capacity;

        _items = restoredItems;
        _capacity = saveData.Capacity;

        if ( capacityChanged == true )
            OnCapacityChanged?.Invoke( _capacity );

        NotifyChanged( );
        return true;
    }
    #endregion
}
