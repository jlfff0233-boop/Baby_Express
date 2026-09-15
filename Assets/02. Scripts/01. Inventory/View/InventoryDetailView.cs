using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 상세 뷰 - 아이템 상세 표시와 입력 전달
/// </summary>
public class InventoryDetailView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _icon;                  //아이템 아이콘
    [SerializeField] TMP_Text _nameText;           //아이템 이름
    [SerializeField] TMP_Text _quantityText;       //선택한 스택 수량
    [SerializeField] TMP_Text _descriptionText;    //아이템 설명

    [SerializeField] Button _shopButton;           //상점 이동 버튼
    [SerializeField] Button _useButton;            //아이템 사용 버튼
    [SerializeField] Button _craftButton;          //제작 이동 버튼
    [SerializeField] Button _sellButton;           //아이템 판매 버튼
    [SerializeField] Button _deleteButton;         //아이템 삭제 버튼
    [SerializeField] Button _closeButton;          //상세 패널 닫기 버튼

    string _slotId;                                //현재 슬롯 아이디
    string _itemId;                                //현재 아이템 아이디

    /// <summary>
    /// 상점 이동 요청 이벤트
    /// </summary>
    public event Action<string> OnMoveToShop;

    /// <summary>
    /// 아이템 사용 요청 이벤트
    /// </summary>
    public event Action<string> OnUse;

    /// <summary>
    /// 제작 이동 요청 이벤트
    /// </summary>
    public event Action<string> OnMoveToCraft;

    /// <summary>
    /// 아이템 판매 요청 이벤트
    /// </summary>
    public event Action<string> OnSell;

    /// <summary>
    /// 아이템 삭제 요청 이벤트
    /// </summary>
    public event Action<string> OnDelete;

    /// <summary>
    /// 상세 패널 닫기 요청 이벤트
    /// </summary>
    public event Action OnClose;


    /// <summary>
    /// 상세 패널 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        //인벤토리 상세 행동 버튼에 공용 클릭 연출 연결
        _shopButton.BindClickHighlight( );
        _useButton.BindClickHighlight( );
        _craftButton.BindClickHighlight( );
        _sellButton.BindClickHighlight( );
        _deleteButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        //상세 패널 버튼 연결
        _shopButton.onClick.AddListener( MoveToShop );
        _useButton.onClick.AddListener( Use );
        _craftButton.onClick.AddListener( MoveToCraft );
        _sellButton.onClick.AddListener( Sell );
        _deleteButton.onClick.AddListener( Delete );
        _closeButton.onClick.AddListener( ClosePanel );
    }

    /// <summary>
    /// 상세 패널 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        //상세 패널 버튼 연결 해제
        _shopButton.onClick.RemoveListener( MoveToShop );
        _useButton.onClick.RemoveListener( Use );
        _craftButton.onClick.RemoveListener( MoveToCraft );
        _sellButton.onClick.RemoveListener( Sell );
        _deleteButton.onClick.RemoveListener( Delete );
        _closeButton.onClick.RemoveListener( ClosePanel );
    }

    /// <summary>
    /// 인벤토리 상세 패널 표시
    /// </summary>
    /// <param name="viewData">상세 패널 표시 데이터</param>
    public void ShowPanel ( InventoryDetailViewData viewData )
    {
        //표시 데이터가 없으면 종료
        if ( viewData == null ) return;

        //상세 정보 갱신
        UpdateView( viewData );

        //상세 패널 활성화
        gameObject.SetActive( true );
    }

    /// <summary>
    /// 인벤토리 상세 패널 숨김
    /// </summary>
    public void HidePanel ()
    {
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 인벤토리 상세 표시 갱신
    /// </summary>
    /// <param name="viewData">상세 패널 표시 데이터</param>
    public void UpdateView ( InventoryDetailViewData viewData )
    {
        //표시 데이터가 없으면 종료
        if ( viewData == null ) return;

        //현재 슬롯과 아이템 아이디 저장
        _slotId = viewData.SlotId;
        _itemId = viewData.ItemId;

        //아이템 정보 표시
        _icon.SetIconSprite( viewData.Icon );
        _nameText.text = viewData.Name;
        _quantityText.text = viewData.SelectedStackQuantity.ToString( );
        _descriptionText.text = viewData.Desc;

        //현재 기능 상태 표시
        _shopButton.interactable = viewData.CanOpenShop;
        _useButton.interactable = viewData.CanUse;
        _craftButton.interactable = viewData.CanCraft;
        _sellButton.interactable = viewData.CanSell;
        _deleteButton.interactable = viewData.CanDelete;
    }

    /// <summary>
    /// 상점 이동 이벤트 발행
    /// </summary>
    void MoveToShop ()
    {
        OnMoveToShop?.Invoke( _itemId );
    }

    /// <summary>
    /// 아이템 사용 이벤트 발행
    /// </summary>
    void Use ()
    {
        OnUse?.Invoke( _itemId );
    }

    /// <summary>
    /// 제작 이동 이벤트 발행 
    /// </summary>
    void MoveToCraft ()
    {
        OnMoveToCraft?.Invoke( _itemId );
    }

    /// <summary>
    /// 아이템 판매 이벤트 발행
    /// </summary>
    void Sell ()
    {
        OnSell?.Invoke( _slotId );
    }

    /// <summary>
    /// 아이템 삭제 이벤트 발행
    /// </summary>
    void Delete ()
    {
        OnDelete?.Invoke( _slotId );
    }

    /// <summary>
    /// 상세 패널 닫기 이벤트 발행
    /// </summary>
    void ClosePanel ()
    {
        OnClose?.Invoke( );
    }
}
