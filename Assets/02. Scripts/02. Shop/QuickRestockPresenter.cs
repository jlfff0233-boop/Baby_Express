using System;

/// <summary>
/// 빠른 재입고 프레젠터 - 확인, 취소, 실패 안내와 실행 중재
/// </summary>
public class QuickRestockPresenter
{
    ShopModel _shopModel;       //상점 모델
    QuickRestockModel _quickRestockModel;       //빠른 재입고 모델
    ShopView _shopView;         //상점 뷰
    ShopViewDataBuilder _viewDataBuilder;       //표시 데이터 생성기

    string _pendingItemId;       //확인 대기 중인 상품 아이디
    bool _isSubscribed;          //이벤트 연결 여부

    /// <summary>
    /// 빠른 재입고 성공 이벤트(상품 아이디)
    /// </summary>
    public event Action<string> OnRestocked;

    /// <summary>
    /// 빠른 재입고 프레젠터 생성
    /// </summary>
    /// <param name="shopModel">상점 모델</param>
    /// <param name="quickRestockModel">빠른 재입고 모델</param>
    /// <param name="shopView">상점 뷰</param>
    public QuickRestockPresenter (
        ShopModel shopModel, QuickRestockModel quickRestockModel,
        ShopView shopView )
    {
        _shopModel = shopModel;
        _quickRestockModel = quickRestockModel;
        _shopView = shopView;
        _viewDataBuilder = new ShopViewDataBuilder( );
    }

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 빠른 재입고 입력 이벤트 연결
    /// </summary>
    public void SubscribeEvents ()
    {
        if ( _isSubscribed ) return;

        _shopView.OnDetailQuickRestock += Open;
        _shopView.OnQuickRestockConfirmed += Confirm;
        _shopView.OnQuickRestockCanceled += Cancel;

        _isSubscribed = true;
    }

    /// <summary>
    /// 빠른 재입고 입력 이벤트 해제
    /// </summary>
    public void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _shopView.OnDetailQuickRestock -= Open;
        _shopView.OnQuickRestockConfirmed -= Confirm;
        _shopView.OnQuickRestockCanceled -= Cancel;

        _isSubscribed = false;
    }
    #endregion

    #region ----- 빠른 재입고 -----
    /// <summary>
    /// 빠른 재입고 확인 대기 상태 초기화
    /// </summary>
    public void Reset ()
    {
        _pendingItemId = null;
    }

    /// <summary>
    /// 빠른 재입고 상태와 경고 숨김
    /// </summary>
    public void Hide ()
    {
        Reset( );
        _shopView.HideQuickRestockWarning( );
    }

    /// <summary>
    /// 선택 상품 빠른 재입고 확인 표시
    /// </summary>
    /// <param name="itemId">상품 아이디</param>
    void Open ( string itemId )
    {
        QuickRestockResult result =
            _quickRestockModel.CheckRestock( itemId, out float fee );

        if ( result != QuickRestockResult.Success )
        {
            ShowFailure( result );
            return;
        }

        if ( _shopModel.TryGetItem(
            itemId, out ShopItemModel itemModel ) == false )
        {
            ShowFailure( QuickRestockResult.InvalidItem );
            return;
        }

        _pendingItemId = itemId;

        string description =
            $"{itemModel.Item.Data.Name}의 재고를 최대치까지 채웁니다.\n" +
            $"이용료: {fee:N0}G\n" +
            $"오늘 빠른 재입고: {_quickRestockModel.TodayCount} / " +
            $"{_quickRestockModel.DailyLimit}";

        _shopView.ShowQuickRestockWarning(
            "빠른 재입고", description, "재입고", "취소" );
    }

    /// <summary>
    /// 빠른 재입고 확정
    /// </summary>
    void Confirm ()
    {
        if ( string.IsNullOrEmpty( _pendingItemId ) )
        {
            _shopView.HideQuickRestockWarning( );
            return;
        }

        string itemId = _pendingItemId;
        _pendingItemId = null;
        _shopView.HideQuickRestockWarning( );

        QuickRestockResult result =
            _quickRestockModel.Restock( itemId, out _ );

        if ( result != QuickRestockResult.Success )
        {
            ShowFailure( result );
            return;
        }

        OnRestocked?.Invoke( itemId );
    }

    /// <summary>
    /// 빠른 재입고 취소
    /// </summary>
    void Cancel ()
    {
        Hide( );
    }

    /// <summary>
    /// 빠른 재입고 실패 안내
    /// </summary>
    /// <param name="result">빠른 재입고 처리 결과</param>
    void ShowFailure ( QuickRestockResult result )
    {
        _pendingItemId = null;

        _shopView.ShowQuickRestockWarning(
            "빠른 재입고 실패",
            _viewDataBuilder.GetQuickRestockFailureText( result ),
            "확인", "돌아가기" );
    }
    #endregion
}
