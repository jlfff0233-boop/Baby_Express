using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 드롭다운 항목의 선택 여부를 글자 색상으로 표시
/// </summary>
[RequireComponent( typeof( Toggle ) )]
public class DropdownOptionColorView : MonoBehaviour
{
    [Header( "----- 색상 -----" )]
    [SerializeField] Color _normalColor = Color.white;       //일반 항목 색상
    [SerializeField] Color _selectedColor = Color.yellow;       //선택 항목 색상

    Toggle _toggle;       //드롭다운 항목 선택 상태
    TMP_Text _label;       //드롭다운 항목 문구

    /// <summary>
    /// 드롭다운 항목 선택 상태 연결
    /// </summary>
    void Awake ()
    {
        _toggle = GetComponent<Toggle>( );
        _label = transform.Find( "Item Label" )
            .GetComponent<TMP_Text>( );

        _toggle.onValueChanged.AddListener( SetSelected );
    }

    /// <summary>
    /// 항목이 표시될 때 현재 선택 색상 갱신
    /// </summary>
    void OnEnable ()
    {
        SetSelected( _toggle.isOn );
    }

    /// <summary>
    /// 드롭다운 항목 선택 상태 해제
    /// </summary>
    void OnDestroy ()
    {
        _toggle.onValueChanged.RemoveListener( SetSelected );
    }

    /// <summary>
    /// 선택 여부에 맞는 글자 색상 표시
    /// </summary>
    /// <param name="isSelected">현재 항목 선택 여부</param>
    void SetSelected ( bool isSelected )
    {
        _label.color =
            isSelected ? _selectedColor : _normalColor;
    }
}
