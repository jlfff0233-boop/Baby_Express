using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 아이템 슬롯 - 아이콘, 이름, 수량 표시와 선택 입력 전달
/// </summary>
public class ItemSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _icon;                 //아이템 아이콘
    [SerializeField] TMP_Text _nameText;          //아이템 이름
    [SerializeField] TMP_Text _quantityText;      //슬롯 수량
    [SerializeField] Button _selectButton;        //슬롯 선택 버튼
    [SerializeField] RectTransform _tooltipPoint;     //툴팁 표시 위치

    string _slotId;                               //현재 슬롯 아이디
    string _itemId;                               //현재 아이템 아이디
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출
    bool _isSelected;       //현재 선택 강조 상태
    bool _isInitialized;       //비활성 생성 대응 초기화 여부

    /// <summary>
    /// 현재 슬롯에 표시 중인 아이템 아이디
    /// </summary>
    public string ItemId => _itemId;

    /// <summary>
    /// 아이템 선택 이벤트(슬롯 아이디, 아이템 아이디)
    /// </summary>
    public event Action<string, string> OnSelected;

    /// <summary>
    /// 슬롯 포인터 진입 이벤트(아이템 아이디)
    /// </summary>
    public event Action<string, RectTransform> OnPointerEntered;

    /// <summary>
    /// 슬롯 포인터 이탈 이벤트
    /// </summary>
    public event Action OnPointerExited;

    /// <summary>
    /// 슬롯 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        InitializeRuntime( );
    }

    /// <summary>
    /// 비활성 부모 아래에서 생성돼도 사용할 슬롯 의존성 초기화
    /// </summary>
    void InitializeRuntime ()
    {
        if ( _isInitialized ) return;

        //아이템 슬롯에 공용 등장과 선택 연출 연결
        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );

        //슬롯 선택 버튼 연결
        _selectButton.onClick.AddListener( SelectItem );

        _isInitialized = true;
    }

    /// <summary>
    /// 슬롯 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        if ( _isInitialized == false ) return;

        //슬롯 선택 버튼 연결 해제
        _selectButton.onClick.RemoveListener( SelectItem );
    }

    /// <summary>
    /// 아이템 슬롯 초기화
    /// </summary>
    /// <param name="viewData">아이템 슬롯 표시 데이터</param>
    public void Init ( ItemSlotViewData viewData )
    {
        //기존 호출부는 연출 없이 아이템 슬롯 표시
        Init( viewData, false );
    }

    /// <summary>
    /// 아이템 슬롯 초기화
    /// </summary>
    /// <param name="viewData">아이템 슬롯 표시 데이터</param>
    /// <param name="playAppear">등장 연출 여부</param>
    public void Init (
        ItemSlotViewData viewData, bool playAppear )
    {
        //아이템 정보와 재사용 상태 갱신
        UpdateView( viewData );

        //풀 인스턴스가 처음 생성된 경우에만 등장 연출 재생
        if ( playAppear )
            _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 아이템 슬롯 표시 갱신
    /// </summary>
    /// <param name="viewData">아이템 슬롯 표시 데이터</param>
    public void UpdateView ( ItemSlotViewData viewData )
    {
        //표시 데이터가 없으면 종료
        if ( viewData == null ) return;

        //비활성 제작 패널 아래에서 생성되면 Awake보다 초기화가 먼저 필요
        InitializeRuntime( );

        //일반 정보 갱신 전에 이전 슬롯 연출 제거
        _slotTween.ResetInstant( );

        //현재 슬롯과 아이템 아이디 저장
        _slotId = viewData.SlotId;
        _itemId = viewData.ItemId;

        //아이템 정보 표시
        _icon.SetIconSprite( viewData.Icon );
        _nameText.text = viewData.Name;
        _quantityText.text = viewData.Quantity.ToString( );
    }

    /// <summary>
    /// 아이템 슬롯 선택 강조 설정
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected ( bool isSelected )
    {
        //같은 선택 상태의 일반 갱신에는 연출을 반복하지 않음
        if ( _isSelected == isSelected ) return;

        _isSelected = isSelected;

        if ( isSelected )
        {
            //새로 선택한 아이템 슬롯을 짧게 강조
            _slotTween.PlaySelected( );
            return;
        }

        //선택 해제 시 진행 중인 연출을 즉시 복구
        _slotTween.ResetInstant( );
    }

    /// <summary>
    /// 아이템 슬롯 선택 입력 가능 여부 설정
    /// </summary>
    /// <param name="isEnabled">선택 입력 허용 여부</param>
    public void SetInputEnabled ( bool isEnabled )
    {
        _selectButton.interactable = isEnabled;
    }

    /// <summary>
    /// 풀 반환 전 아이템 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        //재사용 슬롯의 Tween과 표시 식별자 초기화
        _slotTween.ResetInstant( );
        _slotId = null;
        _itemId = null;
        _isSelected = false;

        //재사용 중 표시 중이던 툴팁 닫기 요청
        OnPointerExited?.Invoke( );
    }

    /// <summary>
    /// 아이템 선택 이벤트 발행
    /// </summary>
    void SelectItem ()
    {
        //현재 슬롯과 아이템 아이디 전달
        OnSelected?.Invoke( _slotId, _itemId );
    }

    /// <summary>
    /// 슬롯 포인터 진입 입력 전달
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerEnter ( PointerEventData eventData )
    {
        OnPointerEntered?.Invoke( _itemId, _tooltipPoint );
    }

    /// <summary>
    /// 슬롯 포인터 이탈 입력 전달
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerExit ( PointerEventData eventData )
    {
        OnPointerExited?.Invoke( );
    }
}
