using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카탈로그 파츠 상세 뷰
/// </summary>
public class CatalogDetailView : MonoBehaviour
{
    [Header( "----- 표시 -----" )]
    [SerializeField] Image _icon;       //파츠 아이콘
    [SerializeField] TMP_Text _nameText;       //파츠 이름
    [SerializeField] TMP_Text _partTypeText;       //파츠 타입
    [SerializeField] TMP_Text _craftCostText;       //제작 코스트
    [SerializeField] TMP_Text _themeText;       //파츠 테마
    [SerializeField] TMP_Text _conflictThemeText;       //상극 테마
    [SerializeField] TMP_Text _descriptionText;       //파츠 설명

    [SerializeField] TMP_Text _unlockText;       //해금 상태
    [SerializeField] TMP_Text _quantityText;       //보유 수량

    [Header( "----- 입력 -----" )]
    [SerializeField] Button _closeButton;       //상세 닫기 버튼
    [SerializeField] Button _shopButton;       //상점 이동 버튼

    string _id;       //현재 파츠 아이디
    bool _isLocked;       //현재 파츠 잠금 여부

    /// <summary>
    /// 현재 해금 파츠 상세 표시 여부
    /// </summary>
    public bool IsShowingUnlockedPart =>
        gameObject.activeInHierarchy && _isLocked == false;

    /// <summary>
    /// 상세 닫기 이벤트
    /// </summary>
    public event Action OnClosed;

    /// <summary>
    /// 선택한 파츠의 상점 이동 이벤트
    /// </summary>
    public event Action<string> OnMoveToShop;

    /// <summary>
    /// 상세 입력 연결
    /// </summary>
    void Awake ()
    {
        _closeButton.onClick.AddListener( Close );
        _shopButton.onClick.AddListener( MoveToShop );
    }

    /// <summary>
    /// 상세 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _closeButton.onClick.RemoveListener( Close );
        _shopButton.onClick.RemoveListener( MoveToShop );
    }

    /// <summary>
    /// 선택한 파츠 상세 표시
    /// </summary>
    /// <param name="viewData">파츠 상세 표시 데이터</param>
    public void Show ( CatalogDetailViewData viewData )
    {
        _id = viewData.Id;
        _isLocked = viewData.IsLocked;

        gameObject.SetActive( true );

        _icon.SetIconSprite( viewData.Icon );
        _icon.color = viewData.IsLocked ? Color.black : Color.white;

        _nameText.text = viewData.DisplayName;
        _descriptionText.text = viewData.DescriptionText;
        _partTypeText.text = viewData.PartTypeText;
        _craftCostText.text = viewData.CraftCostText;
        _themeText.text = viewData.ThemeText;
        _conflictThemeText.text = viewData.ConflictThemeText;
        _unlockText.text = viewData.UnlockText;
        _quantityText.text = viewData.QuantityText;

        //해금된 파츠만 상점 상품 상세로 이동 가능
        _shopButton.interactable = viewData.IsLocked == false;
    }

    /// <summary>
    /// 파츠 상세 숨김
    /// </summary>
    public void Hide ()
    {
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 카탈로그 가이드의 상점 이동 버튼 영역 조회
    /// </summary>
    /// <param name="target">상점 이동 버튼 영역</param>
    /// <returns>활성화된 버튼 영역 조회 성공 여부</returns>
    public bool TryGetShopTutorialTarget ( out RectTransform target )
    {
        target = _shopButton.transform as RectTransform;

        return IsShowingUnlockedPart &&
            _shopButton.interactable && target != null;
    }

    /// <summary>
    /// 상세 닫기 요청
    /// </summary>
    void Close ()
    {
        OnClosed?.Invoke( );
    }

    /// <summary>
    /// 현재 파츠의 상점 이동 요청
    /// </summary>
    void MoveToShop ()
    {
        OnMoveToShop?.Invoke( _id );
    }
}
