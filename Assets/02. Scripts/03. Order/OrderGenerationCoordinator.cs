using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 생성 중재자 - 생성 수 계산과 주문 모델 등록 순서 관리
/// </summary>
public class OrderGenerationCoordinator
{
    CustomerOrderModel _orderModel;       //고객 주문 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    ShopModel _shopModel;                 //상점 모델
    OrderGeneratorModel _generatorModel;  //주문 생성 모델
    TutorialModel _tutorialModel;       //튜토리얼 진행 상태 모델
    TutorialDay1Data _tutorialDay1Data;       //Day 1 고정 주문 데이터

    /// <summary>
    /// 주문 생성 중재자 생성
    /// </summary>
    /// <param name="orderModel">고객 주문 모델</param>
    /// <param name="playStateModel">플레이 상태 모델</param>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="generatorModel">주문 생성 모델</param>
    /// <param name="tutorialModel">튜토리얼 진행 상태 모델</param>
    /// <param name="tutorialDay1Data">Day 1 고정 주문 데이터</param>
    public OrderGenerationCoordinator (
        CustomerOrderModel orderModel, PlayStateModel playStateModel,
        ShopModel shopModel, OrderGeneratorModel generatorModel,
        TutorialModel tutorialModel,
        TutorialDay1Data tutorialDay1Data )
    {
        _orderModel = orderModel;
        _playStateModel = playStateModel;
        _shopModel = shopModel;
        _generatorModel = generatorModel;
        _tutorialModel = tutorialModel;
        _tutorialDay1Data = tutorialDay1Data;
    }

    /// <summary>
    /// 현재 영업일 신규 주문 생성
    /// </summary>
    public void GenerateDailyOrders ()
    {
        //Day 1 핵심 튜토리얼 중에는 고정 주문만 생성
        if ( _tutorialModel.CoreTutorialCompleted == false )
        {
            GenerateTutorialOrder( );
            return;
        }

        //핵심 튜토리얼 종료 후 고정 주문 보호 해제
        _orderModel.SetOrderProtected(
            _tutorialDay1Data.OrderId, false );

        int count = _generatorModel.GetDailyOrderCount(
            _orderModel.WaitingCount,
            _orderModel.WaitingLimit,
            _orderModel.OrderReduction );

        GenerateOrders( count );
    }

    /// <summary>
    /// Day 1 튜토리얼 고정 주문 생성
    /// </summary>
    void GenerateTutorialOrder ()
    {
        //핵심 튜토리얼 동안 거절과 기한 종료에서 고정 주문 보호
        _orderModel.SetOrderProtected(
            _tutorialDay1Data.OrderId, true );

        //저장 복구나 재초기화 시 동일 주문 중복 생성 방지
        if ( _orderModel.GetOrder(
            _tutorialDay1Data.OrderId, out _ ) )
        {
            return;
        }

        OrderCreateData createData =
            _tutorialDay1Data.CreateOrderData(
                _orderModel.NextCreatedNumber,
                _playStateModel.TotalDay );

        var order = new CustomerOrder( createData );

        OrderResult result = _orderModel.AddOrder( order );

        if ( result != OrderResult.Success )
        {
            Debug.LogWarning(
                $"튜토리얼 주문 생성 실패: {result}" );
        }
    }

    /// <summary>
    /// 지정 수량의 주문 생성
    /// </summary>
    /// <param name="count">생성할 주문 수</param>
    public void GenerateOrders ( int count )
    {
        if ( count <= 0 ) return;

        IReadOnlyList<PartsData> unlockedParts = GetUnlockedParts( );

        for ( int i = 0; i < count; i++ )
        {
            if ( _orderModel.WaitingCount >= _orderModel.WaitingLimit ) return;

            //고객 주문 모델이 소유한 다음 생성 번호 사용
            int createdNumber =
                _orderModel.NextCreatedNumber;

            //주문 생성 결과 가져오기
            OrderGenerateResult generateResult =
                _generatorModel.GenerateOrder(
                    _playStateModel.TotalDay, createdNumber,
                    unlockedParts,
                    _playStateModel.HighGradeEvaluationCount,
                    0, 0, out CustomerOrder order );

            if ( generateResult != OrderGenerateResult.Success )
            {
                Debug.Log( $"주문 생성 실패: {generateResult}" );
                return;
            }

            //주문 추가 결과
            OrderResult addResult = _orderModel.AddOrder( order );

            if ( addResult != OrderResult.Success )
            {
                Debug.Log( $"주문 추가 실패: {addResult}" );
                return;
            }
        }
    }

    /// <summary>
    /// 현재 해금된 파츠 목록 조회
    /// </summary>
    /// <returns>현재 해금된 파츠 목록</returns>
    public IReadOnlyList<PartsData> GetUnlockedParts ()
    {
        IReadOnlyList<PartsData> parts = _shopModel.GetAllParts( );
        var unlockedParts = new List<PartsData>( );

        for ( int i = 0; i < parts.Count; i++ )
        {
            PartsData part = parts [ i ];

            if ( part != null && _playStateModel.IsUnlocked( part.Id ) )
                unlockedParts.Add( part );
        }

        return unlockedParts;
    }
}
