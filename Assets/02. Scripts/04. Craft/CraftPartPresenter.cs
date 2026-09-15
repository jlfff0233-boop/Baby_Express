using System;
using UnityEngine;

/// <summary>
/// 제작 파츠 프레젠터 - 파츠 배치와 편집 입력 중재
/// </summary>
public class CraftPartPresenter
{
    const float PartScaleStep = 0.1f;       //휠 한 번당 스케일 변경값

    CraftModel _craftModel;                 //제작 모델
    InventoryModel _inventoryModel;         //인벤토리 모델
    CraftView _craftView;                   //제작 뷰
    PartsSelectView _partsSelectView;       //파츠 선택 뷰
    /// <summary>
    /// 현재 제작 주문 조회
    /// </summary>
    Func<CustomerOrder> _getSelectedOrder;

    int _selectedPlacementNumber;           //선택 파츠 배치 번호
    int _draggingPlacementNumber;           //드래그 중인 파츠 배치 번호
    bool _wasPartDragged;                   //현재 드래그 중 실제 위치 변경 여부
    bool _isSubscribed;                     //이벤트 연결 여부

    /// <summary>
    /// 배치 파츠 변경 이벤트
    /// </summary>
    public event Action OnPartsChanged;

    /// <summary>
    /// 파츠 배치 완료 이벤트(파츠 아이디, 파츠 타입, 배치 번호)
    /// </summary>
    public event Action<string, PartType, int> OnPartPlaced;

    /// <summary>
    /// 배치 파츠 이동 완료 이벤트(파츠 아이디, 파츠 타입, 배치 번호)
    /// </summary>
    public event Action<string, PartType, int> OnPartDragCompleted;

    /// <summary>
    /// 배치 파츠 크기 변경 완료 이벤트(파츠 아이디, 파츠 타입, 배치 번호)
    /// </summary>
    public event Action<string, PartType, int> OnPartScaled;

    /// <summary>
    /// 제작 파츠 프레젠터 생성
    /// </summary>
    /// <param name="craftModel">제작 모델</param>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="craftView">제작 뷰</param>
    /// <param name="partsSelectView">파츠 선택 뷰</param>
    /// <param name="getSelectedOrder">현제 제작 주문 조회 함수</param>
    public CraftPartPresenter (
        CraftModel craftModel, InventoryModel inventoryModel,
        CraftView craftView, PartsSelectView partsSelectView,
        Func<CustomerOrder> getSelectedOrder )
    {
        _craftModel = craftModel;
        _inventoryModel = inventoryModel;
        _craftView = craftView;
        _partsSelectView = partsSelectView;
        _getSelectedOrder = getSelectedOrder;
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 파츠 선택과 편집 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _partsSelectView.OnPartSelected += PlacePart;
        _craftView.OnPartSelected += SelectPlacedPart;
        _craftView.OnPartDragStarted += StartPartDrag;
        _craftView.OnPartDragged += DragPart;
        _craftView.OnPartDragEnded += EndPartDrag;
        _craftView.OnPartScaleInput += ScalePart;
        _craftView.OnPartPinchScaleInput += PinchScalePart;
        _craftView.OnPartRemoved += RemovePlacedPart;
        _craftView.OnEmptyAreaSelected += ClearSelection;

        _isSubscribed = true;
    }

    /// <summary>
    /// 파츠 선택과 편집 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _partsSelectView.OnPartSelected -= PlacePart;
        _craftView.OnPartSelected -= SelectPlacedPart;
        _craftView.OnPartDragStarted -= StartPartDrag;
        _craftView.OnPartDragged -= DragPart;
        _craftView.OnPartDragEnded -= EndPartDrag;
        _craftView.OnPartScaleInput -= ScalePart;
        _craftView.OnPartPinchScaleInput -= PinchScalePart;
        _craftView.OnPartRemoved -= RemovePlacedPart;
        _craftView.OnEmptyAreaSelected -= ClearSelection;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 표시 상태 -----
    /// <summary>
    /// 현재 배치 파츠 표시 복원
    /// </summary>
    public void RestorePlacedParts ()
    {
        //기존 배치 파츠 뷰 제거
        _craftView.ClearParts( );

        //배치 파츠를 순서대로 생성
        for ( int i = 0; i < _craftModel.PlacedParts.Count; i++ )
        {
            _craftView.CreatePart(
                _craftModel.PlacedParts [ i ], false );
        }
    }

    /// <summary>
    /// 배치 파츠 표시와 입력 상태 초기화
    /// </summary>
    public void ResetDisplay ()
    {
        ClearSelection( );
        _craftView.ClearParts( );
        _craftView.ResetSpawnPosition( );
    }

    /// <summary>
    /// 배치 파츠 선택 해제
    /// </summary>
    public void ClearSelection ()
    {
        //선택과 드래그 상태 초기화
        _selectedPlacementNumber = 0;
        _draggingPlacementNumber = 0;
        _wasPartDragged = false;

        //선택 강조 해제
        _craftView.SetSelectedPart( 0 );
    }
    #endregion

    #region ----- 파츠 배치 -----
    /// <summary>
    /// 선택한 파츠 배치
    /// </summary>
    /// <param name="partId">배치할 파츠 아이디</param>
    void PlacePart ( string partId )
    {
        //선택한 주문 조회
        CustomerOrder selectedOrder = _getSelectedOrder( );

        //제작 주문이 없으면 종료
        if ( selectedOrder == null ) return;

        //인벤토리 파츠 조회
        if ( _inventoryModel.GetItem(
            partId, out InventoryItem item ) == false ||
            item.Data is not PartsData partData )
            return;

        //루트 몸통이 존재하면 몸통 교체 처리
        if ( partData.PartType == PartType.Body &&
            _craftModel.HasRootBody )
        {
            ReplaceRootBody( partData, item.Quantity, selectedOrder );
            return;
        }

        //현재 파츠 생성 위치 가져오기
        Vector2 spawnPosition = _craftView.GetSpawnPosition( );

        //파츠 배치
        CraftActionResult result = _craftModel.AddPart(
            partData, item.Quantity, selectedOrder.MaxCraftCost,
            spawnPosition, out PlacedPartData placedPart );

        //배치 실패 시 종료
        if ( result != CraftActionResult.Success ) return;

        //배치 파츠 View 생성
        if ( _craftView.CreatePart(
            placedPart, true ) == false )
        {
            //뷰 생성 실패 시 Model 배치 원상 복구
            _craftModel.RemovePart( placedPart.PlacementNumber );
            OnPartsChanged?.Invoke( );
            return;
        }

        //새로 배치한 파츠 선택
        SelectPlacedPart( placedPart.PlacementNumber );

        //제작 정보 갱신 요청
        OnPartsChanged?.Invoke( );

        //실제 배치에 성공한 파츠 전달
        OnPartPlaced?.Invoke(
            placedPart.PartId,
            placedPart.PartData.PartType,
            placedPart.PlacementNumber );
    }

    /// <summary>
    /// 루트 몸통 교체
    /// </summary>
    /// <param name="partData">새 몸통 파츠 데이터</param>
    /// <param name="ownedQuantity">새 몸통 보유 수량</param>
    /// <param name="selectedOrder">현재 제작 주문</param>
    void ReplaceRootBody (
        PartsData partData, int ownedQuantity,
        CustomerOrder selectedOrder )
    {
        //루트 몸통 교체
        CraftActionResult result = _craftModel.ReplaceRootBody(
            partData, ownedQuantity, selectedOrder.MaxCraftCost,
            out PlacedPartData replacedBody );

        //교체 실패 시 기존 상태 유지
        if ( result != CraftActionResult.Success ) return;

        //교체된 몸통 뷰 갱신
        if ( _craftView.UpdatePart( replacedBody ) == false )
            RestorePlacedParts( );

        //루트 몸통 선택 상태 유지
        SelectPlacedPart( replacedBody.PlacementNumber );

        //제작 정보 갱신 요청
        OnPartsChanged?.Invoke( );

        //교체된 몸통 배치 결과 전달
        OnPartPlaced?.Invoke(
            replacedBody.PartId,
            replacedBody.PartData.PartType,
            replacedBody.PlacementNumber );
    }

    /// <summary>
    /// 배치 파츠 선택
    /// </summary>
    /// <param name="placementNumber">선택할 배치 번호</param>
    void SelectPlacedPart ( int placementNumber )
    {
        //배치 번호 가져오기
        if ( _craftModel.GetPlacedPart(
            placementNumber, out PlacedPartData placedPart ) == false )
            return;

        //이전 드래그 상태 초기화
        _draggingPlacementNumber = 0;
        _craftView.RestorePartIndexes( _craftModel.PlacedParts );

        //선택 번호 저장과 강조 표시
        _selectedPlacementNumber = placedPart.PlacementNumber;
        _craftView.SetSelectedPart( _selectedPlacementNumber );
    }

    /// <summary>
    /// 배치 파츠 제거
    /// </summary>
    /// <param name="placementNumber">제거할 배치 번호</param>
    void RemovePlacedPart ( int placementNumber )
    {
        //루트 몸통 제거 여부 저장
        bool isRootBody = placementNumber == _craftModel.RootPlacementNumber;

        //배치 파츠 제거
        if ( _craftModel.RemovePart( placementNumber ) !=
            CraftActionResult.Success )
            return;

        //루트 몸통이면 전체 배치 표시 초기화
        if ( isRootBody )
        {
            _craftView.ClearParts( );
            ClearSelection( );
        }
        else
        {
            //제거한 배치 파츠 뷰 제거
            _craftView.RemovePart( placementNumber );

            //제거한 파츠가 선택 또는 드래그 중이면 선택 해제
            if ( _selectedPlacementNumber == placementNumber ||
                _draggingPlacementNumber == placementNumber )
                ClearSelection( );
        }

        //제작 정보 갱신 요청
        OnPartsChanged?.Invoke( );
    }
    #endregion

    #region ----- 파츠 이동/크기 -----
    /// <summary>
    /// 배치 파츠 드래그 시작
    /// </summary>
    /// <param name="placementNumber">드래그할 배치 번호</param>
    void StartPartDrag ( int placementNumber )
    {
        //현재 선택한 파츠가 아니면 종료
        if ( placementNumber != _selectedPlacementNumber ) return;

        //루트 몸통은 이동과 임시 앞 표시 차단
        if ( placementNumber == _craftModel.RootPlacementNumber ) return;

        //드래그 중인 파츠 저장
        _draggingPlacementNumber = placementNumber;
        _wasPartDragged = false;

        //드래그 파츠를 임시로 가장 앞에 표시
        _craftView.ShowPartInFront( placementNumber );
    }

    /// <summary>
    /// 배치 파츠 이동
    /// </summary>
    /// <param name="placementNumber">이동할 배치 번호</param>
    /// <param name="localPosition">제작 영역 기준 로컬 위치</param>
    void DragPart ( int placementNumber, Vector2 localPosition )
    {
        //현재 드래그 중인 파츠가 아니면 종료
        if ( placementNumber != _draggingPlacementNumber ) return;

        //Model의 배치 위치 변경
        CraftActionResult result =
            _craftModel.MovePart( placementNumber, localPosition );

        //위치 변경 성공 시 뷰와 실제 이동 상태 갱신
        if ( result == CraftActionResult.Success )
        {
            _craftView.SetPartPosition( placementNumber, localPosition );
            _wasPartDragged = true;
        }
    }

    /// <summary>
    /// 배치 파츠 드래그 종료
    /// </summary>
    /// <param name="placementNumber">드래그를 종료할 배치 번호</param>
    void EndPartDrag ( int placementNumber )
    {
        //현재 드래그 중인 파츠가 아니면 종료
        if ( placementNumber != _draggingPlacementNumber ) return;

        bool wasPartDragged = _wasPartDragged;

        //드래그 상태 초기화
        _draggingPlacementNumber = 0;
        _wasPartDragged = false;

        //저장된 앞뒤 순서로 표시 복구
        _craftView.RestorePartIndexes( _craftModel.PlacedParts );

        //실제 이동이 없었다면 튜토리얼 행동으로 처리하지 않음
        if ( wasPartDragged == false ) return;

        //배치 파츠 조회
        if ( _craftModel.GetPlacedPart(
            placementNumber, out PlacedPartData placedPart ) == false )
        {
            return;
        }

        OnPartDragCompleted?.Invoke(
            placedPart.PartId,
            placedPart.PartData.PartType,
            placedPart.PlacementNumber );
    }

    /// <summary>
    /// 선택 파츠 균등 스케일 변경
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="direction">휠 방향</param>
    void ScalePart ( int placementNumber, float direction )
    {
        //현재 선택한 파츠가 아니면 종료
        if ( placementNumber != _selectedPlacementNumber ) return;

        //루트 몸통은 스케일 변경 불가
        if ( placementNumber == _craftModel.RootPlacementNumber ) return;

        //휠 입력이 없으면 종료
        if ( Mathf.Approximately( direction, 0f ) ) return;

        //현재 배치 데이터 가져오기
        if ( _craftModel.GetPlacedPart(
            placementNumber, out PlacedPartData placedPart ) == false )
            return;

        //휠 방향에 따라 스케일 계산
        float scaleChange = direction > 0f
            ? PartScaleStep
            : -PartScaleStep;
        float scale = Mathf.Clamp(
            Mathf.Round( ( placedPart.Scale + scaleChange ) * 10f ) / 10f,
            CraftModel.MinPartScale,
            CraftModel.MaxPartScale );

        //최소값이나 최대값에 도달했으면 종료
        if ( Mathf.Approximately( scale, placedPart.Scale ) ) return;

        //스케일 변경
        CraftActionResult result =
            _craftModel.SetPartScale( placementNumber, scale );

        if ( result != CraftActionResult.Success ) return;

        //변경 성공 시 View와 튜토리얼 행동 상태 갱신
        _craftView.SetPartScale( placementNumber, scale );
        OnPartScaled?.Invoke(
            placedPart.PartId,
            placedPart.PartData.PartType,
            placedPart.PlacementNumber );
    }

    /// <summary>
    /// 선택 파츠의 핀치 비율만큼 균등 스케일 변경
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="scaleRatio">이전 프레임 대비 터치 거리 비율</param>
    void PinchScalePart ( int placementNumber, float scaleRatio )
    {
        if ( placementNumber != _selectedPlacementNumber ) return;
        if ( placementNumber == _craftModel.RootPlacementNumber ) return;
        if ( scaleRatio <= 0f || Mathf.Approximately( scaleRatio, 1f ) ) return;

        if ( _craftModel.GetPlacedPart(
            placementNumber, out PlacedPartData placedPart ) == false )
            return;

        float scale = Mathf.Clamp(
            placedPart.Scale * scaleRatio,
            CraftModel.MinPartScale,
            CraftModel.MaxPartScale );

        if ( Mathf.Approximately( scale, placedPart.Scale ) ) return;

        CraftActionResult result =
            _craftModel.SetPartScale( placementNumber, scale );

        if ( result != CraftActionResult.Success ) return;

        _craftView.SetPartScale( placementNumber, scale );
        OnPartScaled?.Invoke(
            placedPart.PartId,
            placedPart.PartData.PartType,
            placedPart.PlacementNumber );
    }
    #endregion
}
