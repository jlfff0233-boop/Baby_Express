using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 정비 슬롯 뷰 - 정비 정보 표시와 선택 입력 전달
/// </summary>
public class MaintenanceSlotView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _iconImage;       //정비 아이콘
    [SerializeField] TMP_Text _titleText;       //정비 이름과 단계
    [SerializeField] TMP_Text _infoText;       //정비 설명, 비용, 조건
    [SerializeField] Button _selectButton;       //정비 슬롯 선택 버튼
    [SerializeField] CanvasGroup _canvasGroup;       //슬롯 표시 상태

    string _maintenanceId;       //정비 아이디
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출
    bool _isSelected;       //현재 선택 강조 상태

    /// <summary>
    /// 정비 슬롯 선택 이벤트
    /// </summary>
    public event Action<string> OnSelected;

    /// <summary>
    /// 정비 슬롯 선택 입력과 연출 연결
    /// </summary>
    void Awake ()
    {
        //공용 슬롯 등장과 선택 연출 연결
        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );

        _selectButton.onClick.AddListener( Select );
    }

    /// <summary>
    /// 정비 슬롯 선택 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _selectButton.onClick.RemoveListener( Select );
    }

    /// <summary>
    /// 정비 슬롯 초기화
    /// </summary>
    /// <param name="viewData">정비 슬롯 표시 데이터</param>
    public void Init ( MaintenanceSlotViewData viewData )
    {
        //이전 정비 식별자와 선택 연출 초기화
        ResetForReuse( );

        _maintenanceId = viewData.Id;

        //정비 데이터의 아이콘과 정보를 표시
        _iconImage.SetIconSprite( viewData.Icon );
        _titleText.text = viewData.Title;
        _infoText.text = viewData.Info;

        //잠긴 정비는 흐리게 표시하지만 상세 확인은 허용
        float alpha = viewData.IsLocked ? 0.5f : 1f;
        _canvasGroup.alpha = alpha;
        _selectButton.interactable = true;

        //잠금 상태 Alpha를 연출 복구 기준으로 저장
        _slotTween.SetBaseAlpha( alpha );

        //같은 풀 인스턴스에서 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 정비 슬롯 선택 강조 설정
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected ( bool isSelected )
    {
        //같은 선택 상태의 일반 갱신에는 연출을 반복하지 않음
        if ( _isSelected == isSelected ) return;

        _isSelected = isSelected;

        if ( isSelected )
        {
            //선택한 정비 슬롯을 짧게 강조
            _slotTween.PlaySelected( );
            return;
        }

        //선택 해제 시 잠금 Alpha를 포함한 원래 상태로 복구
        _slotTween.ResetInstant( );
    }

    /// <summary>
    /// 풀 반환 전 정비 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        //Tween과 정비 식별자 및 선택 상태 초기화
        _slotTween.ResetInstant( );
        _maintenanceId = null;
        _isSelected = false;
    }

    /// <summary>
    /// 정비 슬롯 선택 전달
    /// </summary>
    void Select ()
    {
        OnSelected?.Invoke( _maintenanceId );
    }
}
