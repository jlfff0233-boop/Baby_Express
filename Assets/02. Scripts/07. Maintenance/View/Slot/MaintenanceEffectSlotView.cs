using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 정비 전체 탭 개별 효과 슬롯 뷰
/// </summary>
public class MaintenanceEffectSlotView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _sortIcon;       //정비 분류 아이콘
    [SerializeField] TMP_Text _titleText;       //업그레이드 항목과 단계
    [SerializeField] TMP_Text _infoText;       //현재 효과와 다음 상태

    [Header( "----- 분류 아이콘 -----" )]
    [SerializeField] Sprite _facilityIcon;       //시설 아이콘
    [SerializeField] Sprite _researchIcon;       //연구 아이콘
    [SerializeField] Sprite _convenienceIcon;       //편의 아이콘
    [SerializeField] Sprite _employeeIcon;       //고용 아이콘

    SlotTweenView _slotTween;       //슬롯 등장과 재사용 연출

    /// <summary>
    /// 정비 효과 슬롯 연출 연결
    /// </summary>
    void Awake ()
    {
        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );
    }

    /// <summary>
    /// 정비 효과 슬롯 표시
    /// </summary>
    /// <param name="viewData">정비 효과 슬롯 표시 데이터</param>
    public void Show ( MaintenanceEffectSlotViewData viewData )
    {
        //이전 슬롯 표시와 실행 중인 연출 초기화
        ResetForReuse( );

        _sortIcon.SetIconSprite( GetSortIcon( viewData.Category ) );
        _titleText.text = viewData.Title;
        _infoText.text = viewData.Info;

        //같은 풀 인스턴스에서 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 풀 반환 전 효과 슬롯 상태 초기화
    /// </summary>
    public void ResetForReuse ()
    {
        //트윈과 이전 표시 데이터 초기화
        _slotTween.ResetInstant( );
        _sortIcon.SetIconSprite( null );
        _titleText.text = string.Empty;
        _infoText.text = string.Empty;
    }

    /// <summary>
    /// 슬롯 분류에 맞는 아이콘 반환
    /// </summary>
    /// <param name="category">슬롯 분류</param>
    /// <returns>정비 분류 아이콘</returns>
    Sprite GetSortIcon (
        MaintenanceEffectSlotCategory category )
    {
        switch ( category )
        {
            case MaintenanceEffectSlotCategory.Facility:
                return _facilityIcon;

            case MaintenanceEffectSlotCategory.Research:
                return _researchIcon;

            case MaintenanceEffectSlotCategory.Convenience:
                return _convenienceIcon;

            default:
                return _employeeIcon;
        }
    }
}
