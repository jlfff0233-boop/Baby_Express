using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 조작 결과
/// </summary>
public enum CraftActionResult
{
    Success,                //처리 성공
    InvalidOrder,           //잘못된 주문
    NoOrderSelected,        //선택한 주문 없음
    HasPlacedParts,         //기존 배치 파츠 존재
    InvalidPart,            //잘못된 파츠
    RootBodyRequired,       //루트 몸통 필요
    RootLocked,             //루트 몸통 이동, 크기, 순서 변경 불가
    BodyAlreadyPlaced,      //몸통이 이미 배치됨
    CostExceeded,           //최대 제작 코스트 초과
    PartNotFound,           //배치 파츠 없음
    OutsideRootArea,        //루트 몸통 부착 범위 밖
    InvalidPartIndex,       //잘못된 앞뒤 순서 인덱스
    InvalidScale,           //잘못된 스케일
    NotEnoughParts,         //보유 파츠 수량 부족
}

/// <summary>
/// 제작 모델 - 선택 주문과 현재 파츠 배치 상태 관리
/// </summary>
public class CraftModel
{
    public const float DefaultBodyScale = 4.5f;       //몸통 기본 스케일
    public const float DefaultPartScale = 2f;       //몸통 외 파츠 기본 스케일
    public const float MinPartScale = 0.5f;       //파츠 최소 스케일
    public const float MaxPartScale = 3f;       //파츠 최대 스케일
    public const float RootAttachmentHalfSize = 225f;       //루트 몸통 부착 범위 절반

    string _selectedOrderId;       //선택한 주문 아이디
    int _lastPlacementNumber;       //마지막으로 발급한 배치 번호
    int _rootPlacementNumber;       //루트 몸통 배치 번호, 0이면 없음
    int _currentCraftCost;       //현재 제작 코스트

    /// <summary>
    /// 배치된 파츠 리스트
    /// </summary>
    List<PlacedPartData> _placedParts = new List<PlacedPartData>( );
    /// <summary>
    /// 배치된 파츠 수량 딕셔너리(파츠 아이디, 개수)
    /// </summary>
    Dictionary<string, int> _partQuantities = new Dictionary<string, int>( );

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 선택한 주문 아이디
    /// </summary>
    public string SelectedOrderId => _selectedOrderId;

    /// <summary>
    /// 주문 선택 여부
    /// </summary>
    public bool HasSelectedOrder =>
        string.IsNullOrWhiteSpace( _selectedOrderId ) == false;

    /// <summary>
    /// 현재 배치 파츠 목록
    /// </summary>
    public IReadOnlyList<PlacedPartData> PlacedParts => _placedParts;

    /// <summary>
    /// 루트 몸통 배치 번호
    /// </summary>
    public int RootPlacementNumber => _rootPlacementNumber;

    /// <summary>
    /// 루트 몸통 존재 여부
    /// </summary>
    public bool HasRootBody => _rootPlacementNumber > 0;

    /// <summary>
    /// 현재 제작 코스트
    /// </summary>
    public int CurrentCraftCost => _currentCraftCost;
    #endregion

    #region ----- 주문 선택 -----
    /// <summary>
    /// 제작 주문 선택
    /// </summary>
    /// <param name="orderId">선택할 주문 아이디</param>
    /// <returns>주문 선택 결과</returns>
    public CraftActionResult SelectOrder ( string orderId )
    {
        //아이디가 이상하면 종료
        if ( string.IsNullOrWhiteSpace( orderId ) )
            return CraftActionResult.InvalidOrder;

        //같은 주문은 현재 배치를 유지
        if ( _selectedOrderId == orderId )
            return CraftActionResult.Success;

        //기존 배치가 있으면 주문을 임의로 변경하지 않음
        if ( _placedParts.Count > 0 )
            return CraftActionResult.HasPlacedParts;

        _selectedOrderId = orderId;
        return CraftActionResult.Success;
    }

    /// <summary>
    /// 제작 주문 선택 해제
    /// </summary>
    public void ClearOrder ()
    {
        ResetPlacements( );
        _selectedOrderId = null;
    }
    #endregion

    #region ----- 파츠 배치/편집 -----
    /// <summary>
    /// 파츠 배치
    /// </summary>
    /// <param name="partData">배치할 파츠 데이터</param>
    /// <param name="ownedQuantity">인벤토리 보유 수량</param>
    /// <param name="maxCraftCost">주문의 최대 제작 코스트</param>
    /// <param name="localPosition">작업 영역 기준 로컬 위치</param>
    /// <param name="placedPart">생성된 배치 파츠</param>
    /// <returns>파츠 배치 결과</returns>
    public CraftActionResult AddPart (
        PartsData partData , int ownedQuantity , int maxCraftCost ,
        Vector2 localPosition , out PlacedPartData placedPart )
    {
        //기본값 설정
        placedPart = null;

        //선택한 주문 없으면 종료
        if ( HasSelectedOrder == false )
            return CraftActionResult.NoOrderSelected;

        //파츠 데이터가 없으면 종료
        if ( partData == null || string.IsNullOrWhiteSpace ( partData.Id ) )
            return CraftActionResult.InvalidPart;

        //루트 몸통 배치 후 다른 몸통 추가 차단
        if ( HasRootBody && partData.PartType == PartType.Body )
            return CraftActionResult.BodyAlreadyPlaced;

        //루트 몸통 배치 전에는 몸통만 배치 가능
        if ( HasRootBody == false && partData.PartType != PartType.Body )
            return CraftActionResult.RootBodyRequired;

        //파츠 추가 시 최대 제작 코스트 초과 차단
        if ( _currentCraftCost + partData.CraftCost > maxCraftCost )
            return CraftActionResult.CostExceeded;

        //파츠 추가 시 보유 수량을 초과하면 종료
        if ( GetPartQuantity ( partData.Id ) >= ownedQuantity )
            return CraftActionResult.NotEnoughParts;

        //고유 배치 번호 발급
        int placementNumber = ++_lastPlacementNumber;
        //현재 목록의 마지막 순서 지정
        int partIndex = _placedParts.Count;
        //파츠 타입에 맞는 기본 스케일 설정
        float scale = partData.PartType == PartType.Body
            ? DefaultBodyScale
            : DefaultPartScale;

        //배치된 파츠 데이터 생성
        placedPart = new PlacedPartData (
            placementNumber , partData , partIndex , localPosition , scale );

        //리스트에 추가
        _placedParts.Add ( placedPart );
        //파츠 수량 증가
        AddPartQuantity ( partData.Id );
        //현재 제작 코스트 증가
        _currentCraftCost += partData.CraftCost;

        //최초 몸통을 루트 몸통으로 지정
        if ( partData.PartType == PartType.Body )
            _rootPlacementNumber = placementNumber;

        return CraftActionResult.Success;
    }

    /// <summary>
    /// 루트 몸통 교체
    /// </summary>
    /// <param name="partData">새 몸통 파츠 데이터</param>
    /// <param name="ownedQuantity">새 몸통 보유 수량</param>
    /// <param name="maxCraftCost">주문의 최대 제작 코스트</param>
    /// <param name="placedPart">교체된 루트 몸통</param>
    /// <returns>루트 몸통 교체 결과</returns>
    public CraftActionResult ReplaceRootBody (
        PartsData partData , int ownedQuantity , int maxCraftCost ,
        out PlacedPartData placedPart )
    {
        //기본값 설정
        placedPart = null;

        //선택한 주문이 없으면 종료
        if ( HasSelectedOrder == false )
            return CraftActionResult.NoOrderSelected;

        //몸통 파츠가 아니면 종료
        if ( partData == null ||
            string.IsNullOrWhiteSpace ( partData.Id ) ||
            partData.PartType != PartType.Body )
            return CraftActionResult.InvalidPart;

        //현재 루트 몸통 조회
        int rootIndex = FindPartIndex ( _rootPlacementNumber );

        if ( rootIndex < 0 )
            return CraftActionResult.PartNotFound;

        PlacedPartData currentBody = _placedParts [ rootIndex ];

        //같은 몸통이면 현재 루트 반환
        if ( currentBody.PartId == partData.Id )
        {
            placedPart = currentBody;
            return CraftActionResult.Success;
        }

        //현재 배치 수량이 보유 수량 이상이면 교체 불가
        if ( GetPartQuantity ( partData.Id ) >= ownedQuantity )
            return CraftActionResult.NotEnoughParts;

        //몸통 교체 후 제작 코스트 계산
        int replacedCraftCost =
            _currentCraftCost -
            currentBody.PartData.CraftCost +
            partData.CraftCost;

        //최대 제작 코스트를 넘으면 교체 불가
        if ( replacedCraftCost > maxCraftCost )
            return CraftActionResult.CostExceeded;

        //기존 배치 번호와 앞뒤 순서를 유지한 몸통 생성
        placedPart = new PlacedPartData (
            currentBody.PlacementNumber ,
            partData ,
            currentBody.PartIndex ,
            Vector2.zero ,
            DefaultBodyScale );

        //기존 몸통 회전값 유지
        placedPart.SetRotation ( currentBody.Rotation );

        //루트 몸통 데이터 교체
        _placedParts [ rootIndex ] = placedPart;

        //기존 몸통 수량 감소
        RemovePartQuantity ( currentBody.PartId );
        //새 몸통 수량 증가
        AddPartQuantity ( partData.Id );
        //교체된 제작 코스트 적용
        _currentCraftCost = replacedCraftCost;

        return CraftActionResult.Success;
    }

    /// <summary>
    /// 배치 파츠 위치 변경
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="localPosition">변경할 로컬 위치</param>
    /// <returns>위치 변경 결과</returns>
    public CraftActionResult MovePart (
        int placementNumber, Vector2 localPosition )
    {
        //배치된 파츠 데이터 가져오기
        if ( GetPlacedPart( placementNumber, out var placedPart ) == false )
            return CraftActionResult.PartNotFound;

        //루트 몸통은 이동 불가
        if ( placementNumber == _rootPlacementNumber )
            return CraftActionResult.RootLocked;

        //파츠 기준점이 루트 몸통 부착 범위 밖이면 종료
        if ( Mathf.Abs( localPosition.x ) > RootAttachmentHalfSize ||
            Mathf.Abs( localPosition.y ) > RootAttachmentHalfSize )
            return CraftActionResult.OutsideRootArea;

        //위치 설정
        placedPart.SetLocalPosition( localPosition );
        return CraftActionResult.Success;
    }

    /// <summary>
    /// 배치 파츠 회전
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="rotation">변경할 회전값</param>
    /// <returns>회전 결과</returns>
    public CraftActionResult RotatePart ( int placementNumber, float rotation )
    {
        //배치된 파츠 데이터 가져오기
        if ( GetPlacedPart( placementNumber, out var placedPart ) == false )
            return CraftActionResult.PartNotFound;

        //파츠 회전
        placedPart.SetRotation( rotation );
        return CraftActionResult.Success;
    }

    /// <summary>
    /// 배치 파츠 균등 스케일 변경
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="scale">변경할 스케일</param>
    /// <returns>스케일 변경 결과</returns>
    public CraftActionResult SetPartScale ( int placementNumber, float scale )
    {
        //배치된 파츠 데이터 가져오기
        if ( GetPlacedPart( placementNumber, out var placedPart ) == false )
            return CraftActionResult.PartNotFound;

        //루트 몸통은 스케일 변경 불가
        if ( placementNumber == _rootPlacementNumber )
            return CraftActionResult.RootLocked;

        //허용 스케일 범위를 벗어나면 종료
        if ( scale < MinPartScale || scale > MaxPartScale )
            return CraftActionResult.InvalidScale;

        //파츠 스케일 설정
        placedPart.SetScale( scale );
        return CraftActionResult.Success;
    }

    /// <summary>
    /// 배치 파츠 앞뒤 순서 설정
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="partIndex">변경할 앞뒤 순서 인덱스</param>
    /// <returns>순서 설정 결과</returns>
    public CraftActionResult SetPartIndex ( int placementNumber, int partIndex )
    {
        //인덱스 가져오기
        int currentIndex = FindPartIndex( placementNumber );

        if ( currentIndex < 0 )
            return CraftActionResult.PartNotFound;

        if ( partIndex < 0 || partIndex >= _placedParts.Count )
            return CraftActionResult.InvalidPartIndex;

        if ( currentIndex == partIndex )
            return CraftActionResult.Success;

        //루트 몸통의 앞뒤 순서 변경 차단
        if ( placementNumber == _rootPlacementNumber )
            return CraftActionResult.RootLocked;

        //일반 파츠를 루트 몸통보다 뒤로 이동하지 못하게 차단
        if ( HasRootBody && partIndex == 0 )
            return CraftActionResult.InvalidPartIndex;

        //파츠 데이터 가져오기
        PlacedPartData placedPart = _placedParts [ currentIndex ];

        //현재 인덱스에 있는 요소 제거 후 파츠 데이터 삽입
        _placedParts.RemoveAt( currentIndex );
        _placedParts.Insert( partIndex, placedPart );

        RefreshPartIndexes( );
        return CraftActionResult.Success;
    }

    /// <summary>
    /// 배치 파츠 제거
    /// </summary>
    /// <param name="placementNumber">제거할 배치 번호</param>
    /// <returns>파츠 제거 결과</returns>
    public CraftActionResult RemovePart ( int placementNumber )
    {
        int index = FindPartIndex( placementNumber );

        if ( index < 0 )
            return CraftActionResult.PartNotFound;

        //루트 몸통 제거 시 선택 주문을 유지하고 배치만 전체 초기화
        if ( placementNumber == _rootPlacementNumber )
        {
            ResetPlacements( );
            return CraftActionResult.Success;
        }

        PlacedPartData placedPart = _placedParts [ index ];

        _placedParts.RemoveAt( index );
        RemovePartQuantity( placedPart.PartId );
        _currentCraftCost -= placedPart.PartData.CraftCost;

        RefreshPartIndexes( );
        return CraftActionResult.Success;
    }
    #endregion

    #region ----- 배치 초기화/조회 -----
    /// <summary>
    /// 현재 배치 상태 초기화
    /// </summary>
    public void ResetPlacements ()
    {
        _placedParts.Clear( );
        _partQuantities.Clear( );

        _lastPlacementNumber = 0;
        _rootPlacementNumber = 0;
        _currentCraftCost = 0;
    }

    /// <summary>
    /// 배치 번호로 파츠 조회
    /// </summary>
    /// <param name="placementNumber">조회할 배치 번호</param>
    /// <param name="placedPart">조회한 배치 파츠</param>
    /// <returns>조회 성공 여부</returns>
    public bool GetPlacedPart ( int placementNumber, out PlacedPartData placedPart )
    {
        int index = FindPartIndex( placementNumber );

        if ( index < 0 )
        {
            placedPart = null;
            return false;
        }

        placedPart = _placedParts [ index ];
        return true;
    }

    /// <summary>
    /// 파츠 아이디별 현재 배치 수량 조회
    /// </summary>
    /// <param name="partId">조회할 파츠 아이디</param>
    /// <returns>현재 배치 수량</returns>
    public int GetPartQuantity ( string partId )
    {
        return _partQuantities.TryGetValue( partId, out int quantity )
            ? quantity
            : 0;
    }
    #endregion

    #region ----- 배치 내부 처리 -----
    /// <summary>
    /// 파츠 배치 수량 증가
    /// </summary>
    /// <param name="partId">증가할 파츠 아이디</param>
    void AddPartQuantity ( string partId )
    {
        if ( _partQuantities.ContainsKey( partId ) )
            _partQuantities [ partId ]++;
        else
            _partQuantities.Add( partId, 1 );
    }

    /// <summary>
    /// 파츠 배치 수량 감소
    /// </summary>
    /// <param name="partId">감소할 파츠 아이디</param>
    void RemovePartQuantity ( string partId )
    {
        _partQuantities [ partId ]--;

        if ( _partQuantities [ partId ] == 0 )
            _partQuantities.Remove( partId );
    }

    /// <summary>
    /// 배치 파츠 목록 인덱스 조회
    /// </summary>
    /// <param name="placementNumber">조회할 배치 번호</param>
    /// <returns>배치 목록 인덱스</returns>
    int FindPartIndex ( int placementNumber )
    {
        for ( int i = 0; i < _placedParts.Count; i++ )
        {
            if ( _placedParts [ i ].PlacementNumber == placementNumber )
                return i;
        }

        return -1;
    }

    /// <summary>
    /// 현재 목록 순서에 맞게 저장 앞뒤 순서 인덱스 갱신
    /// </summary>
    void RefreshPartIndexes ()
    {
        for ( int i = 0; i < _placedParts.Count; i++ )
        {
            _placedParts [ i ].SetPartIndex( i );
        }
    }
    #endregion
}
