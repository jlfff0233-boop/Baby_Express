using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카탈로그 파츠 슬롯 뷰
/// </summary>
public class CatalogSlotView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Button _selectButton;       //파츠 선택 버튼
    [SerializeField] Image _icon;       //파츠 아이콘
    [SerializeField] TMP_Text _nameText;       //파츠 이름
    [SerializeField] TMP_Text _partTypeText;       //파츠 타입
    [SerializeField] Image _lockIcon;       //잠금 아이콘

    string _id;       //현재 파츠 아이디
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출
    bool _isSelected;       //현재 선택 강조 상태
    bool _isLocked;       //현재 파츠 잠금 여부

    /// <summary>
    /// 현재 표시 중인 파츠 아이디
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// 현재 표시 중인 파츠 잠금 여부
    /// </summary>
    public bool IsLocked => _isLocked;

    /// <summary>
    /// 파츠 선택 이벤트
    /// </summary>
    public event Action<string> OnSelected;

    /// <summary>
    /// 비활성 상태에서도 사용할 슬롯 연출 초기화
    /// </summary>
    void InitializeRuntime ()
    {
        if ( _slotTween != null ) return;

        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );
    }

    /// <summary>
    /// 슬롯 입력 연결
    /// </summary>
    void Awake ()
    {
        InitializeRuntime( );
        _selectButton.onClick.AddListener( Select );
    }

    /// <summary>
    /// 슬롯 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _selectButton.onClick.RemoveListener( Select );
    }

    /// <summary>
    /// 카탈로그 파츠 슬롯 표시
    /// </summary>
    /// <param name="viewData">파츠 슬롯 표시 데이터</param>
    public void Show ( CatalogSlotViewData viewData )
    {
        gameObject.SetActive( true );

        //재사용 전 이전 선택 상태와 연출 초기화
        ResetForReuse( );

        _id = viewData.Id;
        _isLocked = viewData.IsLocked;

        _icon.SetIconSprite( viewData.Icon );
        _icon.color = viewData.IsLocked ? Color.black : Color.white;

        _nameText.text = viewData.DisplayName;
        _partTypeText.text = viewData.PartTypeText;
        _lockIcon.gameObject.SetActive( viewData.IsLocked );

        //잠긴 파츠도 잠금 상세를 확인할 수 있도록 선택 허용
        _selectButton.interactable = true;

        //같은 슬롯 인스턴스에서 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 카탈로그 슬롯 선택 강조 설정
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected ( bool isSelected )
    {
        //같은 선택 상태에서는 연출을 반복하지 않음
        if ( _isSelected == isSelected ) return;

        _isSelected = isSelected;

        if ( isSelected )
        {
            _slotTween.PlaySelected( );
            return;
        }

        //선택 해제 시 즉시 기본 상태로 복구
        _slotTween.ResetInstant( );
    }

    /// <summary>
    /// 재사용 전 카탈로그 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        InitializeRuntime( );
        _slotTween.ResetInstant( );

        _id = null;
        _isSelected = false;
        _isLocked = false;
    }

    /// <summary>
    /// 카탈로그 가이드에서 강조할 슬롯 영역 조회
    /// </summary>
    /// <param name="target">슬롯 선택 영역</param>
    /// <returns>활성 슬롯 영역 조회 성공 여부</returns>
    public bool TryGetTutorialTarget ( out RectTransform target )
    {
        target = _selectButton.transform as RectTransform;

        return gameObject.activeInHierarchy &&
            string.IsNullOrEmpty( _id ) == false &&
            target != null;
    }

    /// <summary>
    /// 사용하지 않는 카탈로그 슬롯 숨김
    /// </summary>
    public void Hide ()
    {
        ResetForReuse( );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 현재 파츠 선택
    /// </summary>
    void Select ()
    {
        OnSelected?.Invoke( _id );
    }
}
