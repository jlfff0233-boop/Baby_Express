using System;

/// <summary>
/// 제작 정보 프레젠터 - 사용 파츠, 판정, 코스트 표시 중재
/// </summary>
public class CraftInfoPresenter
{
    CraftModel _craftModel;               //제작 모델
    CraftReviewModel _reviewModel;        //제작 판정 모델
    CraftReviewViewBuilder _reviewViewBuilder;       //제작 판정 표시 데이터 생성기
    CraftPartsViewDataBuilder _partsViewDataBuilder; //파츠 표시 데이터 빌더
    UsedPartsListView _usedPartsListView; //사용 파츠 목록 뷰
    CraftOrderDetailView _orderDetailView;           //제작 주문 상세 뷰
    CraftPreviewView _previewView;         //제작 테마와 점수 미리보기 뷰
    CraftView _craftView;                  //제작 뷰
    /// <summary>
    /// 현재 제작 주문 조회
    /// </summary>
    Func<CustomerOrder> _getSelectedOrder;

    /// <summary>
    /// 제작 정보 프레젠터 생성
    /// </summary>
    /// <param name="craftModel">제작 모델</param>
    /// <param name="reviewModel">제작 판정 모델</param>
    /// <param name="reviewViewBuilder">판정 표시 데이터 생성기</param>
    /// <param name="partsViewDataBuilder">제작 파츠 표시 데이터 생성기</param>
    /// <param name="usedPartsListView">사용한 파츠 목록 뷰</param>
    /// <param name="orderDetailView">주문 상세 뷰</param>
    /// <param name="previewView">제작 미리보기 뷰</param>
    /// <param name="craftView">제작 뷰</param>
    /// <param name="getSelectedOrder">현재 제작 주문 조회 함수</param>
    public CraftInfoPresenter (
        CraftModel craftModel, CraftReviewModel reviewModel,
        CraftReviewViewBuilder reviewViewBuilder,
        CraftPartsViewDataBuilder partsViewDataBuilder,
        UsedPartsListView usedPartsListView,
        CraftOrderDetailView orderDetailView,
        CraftPreviewView previewView, CraftView craftView,
        Func<CustomerOrder> getSelectedOrder )
    {
        _craftModel = craftModel;
        _reviewModel = reviewModel;
        _reviewViewBuilder = reviewViewBuilder;
        _partsViewDataBuilder = partsViewDataBuilder;
        _usedPartsListView = usedPartsListView;
        _orderDetailView = orderDetailView;
        _previewView = previewView;
        _craftView = craftView;
        _getSelectedOrder = getSelectedOrder;
    }

    /// <summary>
    /// 현재 제작 정보 전체 갱신
    /// </summary>
    public void Refresh ()
    {
        RefreshUsedParts( );
        RefreshCraftReview( );
        RefreshCost( );
    }

    /// <summary>
    /// 제작 정보 표시 초기화
    /// </summary>
    public void Clear ()
    {
        _usedPartsListView.ClearParts( );
        _orderDetailView.ClearDetail( );
        _previewView.Clear( );
        _craftView.UpdateCost( 0, 0 );
    }

    #region ----- 제작 판정 표시 -----
    /// <summary>
    /// 현재 제작 판정 표시 갱신
    /// </summary>
    void RefreshCraftReview ()
    {
        //선택 주문 조회
        CustomerOrder selectedOrder = _getSelectedOrder( );

        //선택 주문이 없으면 제작 판정 표시 초기화
        if ( selectedOrder == null )
        {
            _orderDetailView.ClearDetail( );
            _previewView.Clear( );
            return;
        }

        //현재 배치 제작 판정
        _reviewModel.Review(
            selectedOrder, _craftModel.PlacedParts,
            _craftModel.RootPlacementNumber, out CraftReviewData reviewData );

        //구조 오류면 제작 판정 표시 초기화
        if ( reviewData == null )
        {
            _orderDetailView.ClearDetail( );
            _previewView.Clear( );
            return;
        }

        //공용 제작 판정 표시 데이터 생성
        CraftReviewViewData viewData =
            _reviewViewBuilder.Create( selectedOrder, reviewData );

        //주문 조건과 테마, 점수 미리보기 표시
        _orderDetailView.ShowDetail(
            viewData.RequirementResults,
            viewData.WishResults,
            viewData.SpecialResults );

        //미리보기 뷰 표시
        _previewView.Show( viewData );
    }
    #endregion

    #region ----- 사용 파츠 표시 -----
    /// <summary>
    /// 사용 파츠 목록 갱신
    /// </summary>
    void RefreshUsedParts ()
    {
        //사용 파츠 목록 표시
        _usedPartsListView.ShowParts(
            _partsViewDataBuilder.CreateUsedParts(
                _craftModel.PlacedParts, _craftModel ) );
    }
    #endregion

    #region ----- 제작 코스트 -----
    /// <summary>
    /// 현재 제작 코스트 표시 갱신
    /// </summary>
    void RefreshCost ()
    {
        CustomerOrder selectedOrder = _getSelectedOrder( );

        //제작 주문이 없으면 초기값 표시
        if ( selectedOrder == null )
        {
            _craftView.UpdateCost( 0, 0 );
            return;
        }

        //현재 코스트와 주문 최대 코스트 표시
        _craftView.UpdateCost(
            _craftModel.CurrentCraftCost,
            selectedOrder.MaxCraftCost );
    }
    #endregion
}
