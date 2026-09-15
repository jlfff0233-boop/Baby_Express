using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 인벤토리 프레젠터 - 인벤토리 화면 생명주기와 외부 이동 중재
/// </summary>
public class InventoryPresenter : MonoBehaviour
{
    [Header( "----- 설정 데이터 -----" )]
    [FormerlySerializedAs( "_dataMap" )]
    [SerializeField] PurchasableDataMap _purchasableDataMap;       //구매 가능 상품 데이터 맵

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] InventoryView _inventoryView;      //인벤토리 뷰

#if UNITY_EDITOR
    [Header( "----- 테스트 -----" )]
    [SerializeField] bool _addTestItems;                //임시 아이템 추가 여부
#endif

    InventoryModel _inventoryModel;                     //인벤토리 모델
    InventoryListPresenter _listPresenter;              //인벤토리 목록 프레젠터
    InventoryItemActionPresenter _actionPresenter;      //아이템 처리 프레젠터

    bool _isInitialized;       //모델 전달 완료 여부
    bool _isSubscribed;        //이벤트 연결 여부

    /// <summary>
    /// 상점 이동 요청 이벤트(선택한 아이템 아이디)
    /// </summary>
    public event Action<string> OnMoveToShop;

    /// <summary>
    /// 제작 이동 요청 이벤트(선택한 아이템 아이디)
    /// </summary>
    public event Action<string> OnMoveToCraft;

    /// <summary>
    /// 인벤토리 화면 닫기 이벤트
    /// </summary>
    public event Action OnPanelClosed;

    /// <summary>
    /// 인벤토리 패널 표시 이벤트
    /// </summary>
    public event Action OnPanelOpened;

    /// <summary>
    /// 인벤토리 아이템 상세 표시 이벤트
    /// </summary>
    public event Action<string> OnItemSelected;

    #region ----- 시작 -----
    /// <summary>
    /// 인벤토리 모델 연결
    /// </summary>
    /// <param name="inventoryModel">인벤토리 모델</param>
    /// <param name="itemActionModel">아이템 판매와 삭제 모델</param>
    public void Init ( InventoryModel inventoryModel,
        InventoryItemActionModel itemActionModel )
    {
        //비활성 인벤토리 뷰의 런타임 의존성 먼저 초기화
        _inventoryView.InitializeRuntime( );

        UnsubscribeEvents( );

        _inventoryModel = inventoryModel;

        //목록과 아이템 처리 흐름을 담당 프레젠터에 전달
        _listPresenter = new InventoryListPresenter(
            inventoryModel, _inventoryView );
        _actionPresenter = new InventoryItemActionPresenter(
            inventoryModel, itemActionModel, _inventoryView );

#if UNITY_EDITOR
        //선택한 경우 비어 있는 인벤토리에만 테스트 아이템 추가
        if ( _addTestItems && _inventoryModel.IsEmpty )
            AddTestItems( );
#endif

        _isInitialized = true;

        if ( isActiveAndEnabled ) SubscribeEvents( );

        _inventoryView.HideInstant( );
    }

    /// <summary>
    /// 인벤토리 이벤트 연결
    /// </summary>
    void OnEnable ()
    {
        if ( _isInitialized ) SubscribeEvents( );
    }

    /// <summary>
    /// 인벤토리 이벤트 해제
    /// </summary>
    void OnDisable ()
    {
        UnsubscribeEvents( );
    }

    /// <summary>
    /// 인벤토리 목록 표시 시작
    /// </summary>
    void Start ()
    {
        if ( _isInitialized == false ) return;

        _listPresenter.Refresh( );
        _actionPresenter.HidePanels( );
    }
    #endregion

    #region ----- 이벤트 연결 -----
    /// <summary>
    /// 인벤토리와 하위 프레젠터 이벤트 연결
    /// </summary>
    void SubscribeEvents ()
    {
        if ( _isSubscribed || _isInitialized == false ) return;

        _listPresenter.SubscribeEvents( );
        _actionPresenter.SubscribeEvents( );

        _actionPresenter.OnMoveToShop += MoveToShop;
        _actionPresenter.OnMoveToCraft += MoveToCraft;
        _actionPresenter.OnItemSelected += SelectItem;
        _inventoryView.OnClose += ClosePanel;

        _isSubscribed = true;
    }

    /// <summary>
    /// 인벤토리와 하위 프레젠터 이벤트 해제
    /// </summary>
    void UnsubscribeEvents ()
    {
        if ( _isSubscribed == false ) return;

        _listPresenter.UnsubscribeEvents( );
        _actionPresenter.UnsubscribeEvents( );

        _actionPresenter.OnMoveToShop -= MoveToShop;
        _actionPresenter.OnMoveToCraft -= MoveToCraft;
        _actionPresenter.OnItemSelected -= SelectItem;
        _inventoryView.OnClose -= ClosePanel;

        _isSubscribed = false;
    }

    /// <summary>
    /// 상점 이동 요청 전달
    /// </summary>
    /// <param name="itemId">선택한 아이템 아이디</param>
    void MoveToShop ( string itemId )
    {
        OnMoveToShop?.Invoke( itemId );
    }

    /// <summary>
    /// 제작 이동 요청 전달
    /// </summary>
    /// <param name="itemId">선택한 아이템 아이디</param>
    void MoveToCraft ( string itemId )
    {
        OnMoveToCraft?.Invoke( itemId );
    }

    /// <summary>
    /// 아이템 상세 표시 결과 전달
    /// </summary>
    /// <param name="itemId">확인한 아이템 아이디</param>
    void SelectItem ( string itemId )
    {
        OnItemSelected?.Invoke( itemId );
    }
    #endregion

    #region ----- 인벤토리 패널 -----
    /// <summary>
    /// 인벤토리 화면 표시 여부
    /// </summary>
    public bool IsShowing => _inventoryView.IsShowing;

    /// <summary>
    /// 인벤토리 패널 표시
    /// </summary>
    public void ShowPanel ()
    {
        _inventoryView.ShowPanel( );
        _listPresenter.Refresh( );

        OnPanelOpened?.Invoke( );
    }

    /// <summary>
    /// 사용자 입력으로 인벤토리 화면 닫기
    /// </summary>
    void ClosePanel ()
    {
        HidePanel ( ( ) => OnPanelClosed?.Invoke ( ) );
    }

    /// <summary>
    /// 인벤토리 패널 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        _actionPresenter?.HidePanels( );
        _inventoryView.HidePanel ( onComplete );
    }

    /// <summary>
    /// 인벤토리 패널 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _inventoryView.HideInstant ( );
    }

    /// <summary>
    /// 아이템 아이디에 대응하는 현재 슬롯 강조 대상 조회
    /// </summary>
    /// <param name="itemId">아이템 아이디</param>
    /// <param name="target">현재 활성 슬롯 위치</param>
    /// <returns>강조 대상 조회 성공 여부</returns>
    public bool TryGetItemSlotTarget (
        string itemId, out RectTransform target )
    {
        return _inventoryView.FocusSlot(
            itemId, out target );
    }
    #endregion

#if UNITY_EDITOR
    #region ----- 테스트 -----
    /// <summary>
    /// 인벤토리 표시 확인용 아이템 추가
    /// </summary>
    void AddTestItems ()
    {
        if ( _purchasableDataMap == null ||
            _purchasableDataMap.PurchasableDatas == null )
        {
            return;
        }

        IReadOnlyList<PurchasableData> datas =
            _purchasableDataMap.PurchasableDatas;
        int [ ] quantities = { 1, 99, 100, 199 };
        var amounts = new List<InventoryItemAmount>( );

        //사용 가능한 데이터만 테스트 목록에 추가
        for ( int i = 0; i < quantities.Length && i < datas.Count; i++ )
        {
            if ( datas [ i ] == null ) continue;

            amounts.Add( new InventoryItemAmount( datas [ i ], quantities [ i ] ) );
        }

        if ( amounts.Count > 0 )
            _inventoryModel.AddItems( amounts );
    }
    #endregion
#endif
}
