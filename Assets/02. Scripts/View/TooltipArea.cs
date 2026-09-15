using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 툴팁 입력 - 포인터 진입과 이탈 처리
/// </summary>
public class TooltipArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header( "----- 표시 내용 -----" )]
    [TextArea]
    [SerializeField] string _description;       //툴팁 설명

    RectTransform _rectTransform;       //입력 영역 RectTransform
    TooltipView _tooltip;       //Canvas 공용 툴팁 뷰
    Vector3 [ ] _worldCorners = new Vector3 [ 4 ];       //입력 영역 월드 좌표 모서리

    /// <summary>
    /// 입력 영역과 Canvas 공용 툴팁 초기화
    /// </summary>
    void InitializeRuntime ()
    {
        if ( _tooltip != null ) return;

        _rectTransform = GetComponent<RectTransform>( );

        Canvas canvas =
            GetComponentInParent<Canvas>( true );

        _tooltip = FindNearestTooltip(
            canvas.rootCanvas.transform );
    }

    /// <summary>
    /// 현재 입력 영역에서 가장 가까운 부모 영역의 툴팁 조회
    /// </summary>
    /// <param name="rootCanvas">탐색을 종료할 최상위 Canvas</param>
    /// <returns>가장 가까운 부모 영역에 포함된 툴팁</returns>
    TooltipView FindNearestTooltip ( Transform rootCanvas )
    {
        Transform current = transform;

        while ( current != null )
        {
            TooltipView tooltip =
                current.GetComponentInChildren<TooltipView>( true );

            if ( tooltip != null )
                return tooltip;

            if ( current == rootCanvas )
                break;

            current = current.parent;
        }

        return null;
    }

    /// <summary>
    /// 툴팁 입력 상태 초기화
    /// </summary>
    void Awake ()
    {
        InitializeRuntime( );
    }

    /// <summary>
    /// 툴팁 설명 설정
    /// </summary>
    /// <param name="description">표시할 설명</param>
    public void SetDescription ( string description )
    {
        _description = description;
    }

    /// <summary>
    /// 툴팁 강제 숨김
    /// </summary>
    public void Hide ()
    {
        InitializeRuntime( );
        _tooltip.Hide( );
    }

    /// <summary>
    /// 포인터 진입 처리
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerEnter ( PointerEventData eventData )
    {
        InitializeRuntime( );

        //입력 영역의 실제 중심 좌표 계산
        _rectTransform.GetWorldCorners( _worldCorners );
        Vector3 center = ( _worldCorners [ 0 ] + _worldCorners [ 2 ] ) * 0.5f;

        _tooltip.Show( _description, center );
    }

    /// <summary>
    /// 포인터 이탈 처리
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerExit ( PointerEventData eventData )
    {
        _tooltip.Hide( );
    }

    /// <summary>
    /// 입력 영역 비활성화 처리
    /// </summary>
    void OnDisable ()
    {
        //씬 종료 중 이미 제거된 공용 툴팁에는 접근하지 않음
        if ( _tooltip != null )
            _tooltip.Hide( );
    }
}
