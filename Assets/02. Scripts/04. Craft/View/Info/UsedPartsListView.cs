using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사용 파츠 목록 뷰 - 사용한 파츠를 읽기 전용 아이템 슬롯으로 표시
/// </summary>
public class UsedPartsListView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] ItemSlotView _slotPrefab;       //아이템 슬롯 프리팹
    [SerializeField] RectTransform _content;         //사용 파츠 슬롯 부모

    List<ItemSlotView> _slotViews = new List<ItemSlotView>( );

    /// <summary>
    /// 사용 파츠 목록 초기화
    /// </summary>
    public void Init ()
    {
        ClearParts( );
    }

    #region ----- 사용 파츠 표시 -----
    /// <summary>
    /// 사용 파츠 슬롯 목록 표시
    /// </summary>
    /// <param name="viewDatas">사용 파츠 슬롯 표시 데이터 목록</param>
    public void ShowParts ( IReadOnlyList<ItemSlotViewData> viewDatas )
    {
        ClearParts( );

        if ( viewDatas == null || viewDatas.Count == 0 ) return;

        for ( int i = 0; i < viewDatas.Count; i++ )
        {
            ItemSlotView slotView = Instantiate( _slotPrefab, _content );
            slotView.Init( viewDatas [ i ] );
            slotView.SetInputEnabled( false );
            _slotViews.Add( slotView );
        }
    }

    /// <summary>
    /// 생성된 사용 파츠 슬롯 전체 제거
    /// </summary>
    public void ClearParts ()
    {
        for ( int i = 0; i < _slotViews.Count; i++ )
        {
            ItemSlotView slotView = _slotViews [ i ];

            if ( slotView != null )
                Destroy( slotView.gameObject );
        }

        _slotViews.Clear( );
    }
    #endregion
}
