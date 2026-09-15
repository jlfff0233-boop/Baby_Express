using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 행동 뷰 - 메인 행동 버튼 입력 전달
/// </summary>
public class ActionView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Button _shopButton;           //상점 버튼
    [SerializeField] Button _craftButton;           //제작 버튼
    [SerializeField] Button _orderButton;       //주문 버튼
    [SerializeField] Button _maintenanceButton;     //정비 버튼
    [SerializeField] Button _inventoryButton;      //인벤토리 버튼


    /// <summary>
    /// 상점 열기 요청 이벤트
    /// </summary>
    public event Action OnShopOpen;

    /// <summary>
    /// 인벤토리 열기 요청 이벤트
    /// </summary>
    public event Action OnInventoryOpen;

    /// <summary>
    /// 제작 열기 이벤트
    /// </summary>
    public event Action OnCraftOpen;

    /// <summary>
    /// 주문 열기 이벤트
    /// </summary>
    public event Action OnOrderOpen;

    /// <summary>
    /// 정비 열기 이벤트
    /// </summary>
    public event Action OnMaintenanceOpen;

    /// <summary>
    /// 메인 행동 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 버튼 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        Button button;

        switch ( targetId )
        {
            case TutorialTargetId.OrderButton:
                button = _orderButton;
                break;

            case TutorialTargetId.ShopButton:
                button = _shopButton;
                break;

            case TutorialTargetId.InventoryButton:
                button = _inventoryButton;
                break;

            case TutorialTargetId.CraftButton:
                button = _craftButton;
                break;

            case TutorialTargetId.MaintenanceButton:
                button = _maintenanceButton;
                break;

            default:
                target = null;
                return false;
        }

        target = button.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 버튼 입력 연결
    /// </summary>
    void Awake ()
    {
        //메인 행동 버튼에 공용 클릭 연출 연결
        _shopButton.BindClickHighlight( );
        _inventoryButton.BindClickHighlight( );
        _craftButton.BindClickHighlight( );
        _orderButton.BindClickHighlight( );
        _maintenanceButton.BindClickHighlight( );

        _shopButton.onClick.AddListener( OpenShop );
        _inventoryButton.onClick.AddListener( OpenInventory );
        _craftButton.onClick.AddListener( OpenCraft );
        _orderButton.onClick.AddListener( OpenOrder );
        _maintenanceButton.onClick.AddListener( OpenMaintenance );
    }

    /// <summary>
    /// 버튼 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _shopButton.onClick.RemoveListener( OpenShop );
        _inventoryButton.onClick.RemoveListener( OpenInventory );
        _craftButton.onClick.RemoveListener( OpenCraft );
        _orderButton.onClick.RemoveListener( OpenOrder );
        _maintenanceButton.onClick.RemoveListener( OpenMaintenance );
    }

    /// <summary>
    /// 메인 행동 버튼 입력 가능 여부 설정
    /// </summary>
    /// <param name="interactable">입력 가능 여부</param>
    public void SetInteractable ( bool interactable )
    {
        _shopButton.interactable = interactable;
        _craftButton.interactable = interactable;
        _orderButton.interactable = interactable;
        _maintenanceButton.interactable = interactable;
        _inventoryButton.interactable = interactable;
    }

    /// <summary>
    /// Day 1 진행 단계에 따른 메인 행동 버튼 입력 설정
    /// </summary>
    /// <param name="canOpenOrder">주문 진입 가능 여부</param>
    /// <param name="canOpenShop">상점 진입 가능 여부</param>
    /// <param name="canOpenInventory">인벤토리 진입 가능 여부</param>
    /// <param name="canOpenCraft">제작 진입 가능 여부</param>
    /// <param name="canOpenMaintenance">정비 진입 가능 여부</param>
    public void SetTutorialAccess (
        bool canOpenOrder,
        bool canOpenShop,
        bool canOpenInventory,
        bool canOpenCraft,
        bool canOpenMaintenance )
    {
        _orderButton.interactable = canOpenOrder;
        _shopButton.interactable = canOpenShop;
        _inventoryButton.interactable = canOpenInventory;
        _craftButton.interactable = canOpenCraft;
        _maintenanceButton.interactable = canOpenMaintenance;
    }

    /// <summary>
    /// 상점 열기 요청
    /// </summary>
    void OpenShop ()
    {
        OnShopOpen?.Invoke( );
    }

    /// <summary>
    /// 인벤토리 열기 요청
    /// </summary>
    void OpenInventory ()
    {
        OnInventoryOpen?.Invoke( );
    }

    /// <summary>
    /// 제작 열기
    /// </summary>
    void OpenCraft ()
    {
        OnCraftOpen?.Invoke( );
    }

    /// <summary>
    /// 주문 열기
    /// </summary>
    void OpenOrder ()
    {
        OnOrderOpen?.Invoke( );
    }

    /// <summary>
    /// 정비 열기
    /// </summary>
    void OpenMaintenance ()
    {
        OnMaintenanceOpen?.Invoke( );
    }
}
