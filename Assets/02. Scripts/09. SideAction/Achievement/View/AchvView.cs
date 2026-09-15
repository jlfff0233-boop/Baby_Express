using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 업적 목록과 분류 입력 뷰
/// </summary>
public class AchvView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //업적 패널 연출

    [Header( "----- 목록 -----" )]
    [SerializeField] SettlementIconData _iconData;       //결산 공용 아이콘 데이터
    [SerializeField] Transform _content;       //업적 슬롯 부모
    [SerializeField] AchvSlotView _slotPrefab;       //업적 슬롯 프리팹

    [Header( "----- 분류 입력 -----" )]
    [SerializeField] Button _allButton;       //전체 분류 버튼
    [SerializeField] Button _businessButton;       //영업 분류 버튼
    [SerializeField] Button _orderButton;       //주문 분류 버튼
    [SerializeField] Button _craftButton;       //제작 분류 버튼
    [SerializeField] Button _etcButton;       //기타 분류 버튼

    [Header( "----- 입력 -----" )]
    [SerializeField] Button _closeButton;       //업적 화면 닫기 버튼

    List<AchvSlotView> _slots = new List<AchvSlotView>( );       //생성 또는 배치된 업적 슬롯
    Coroutine _scrollResetRoutine;       //업적 목록 최상단 이동 루틴

    #region ----- 이벤트 -----

    /// <summary>
    /// 전체 분류 선택 이벤트
    /// </summary>
    public event Action OnAllSelected;

    /// <summary>
    /// 업적 분류 선택 이벤트
    /// </summary>
    public event Action<AchvCategory> OnCategorySelected;

    /// <summary>
    /// 업적 보상 수령 이벤트
    /// </summary>
    public event Action<string> OnClaim;

    /// <summary>
    /// 업적 화면 닫기 이벤트
    /// </summary>
    public event Action OnClosed;

    #endregion

    #region ----- 이벤트 연결/해제 -----

    /// <summary>
    /// 업적 화면 입력 연결
    /// </summary>
    void Awake ()
    {
        CollectExistingSlots( );

        _allButton.onClick.AddListener( SelectAll );
        _businessButton.onClick.AddListener( SelectBusiness );
        _orderButton.onClick.AddListener( SelectOrder );
        _craftButton.onClick.AddListener( SelectCraft );
        _etcButton.onClick.AddListener( SelectEtc );
        _closeButton.onClick.AddListener( Close );
    }

    /// <summary>
    /// 업적 화면 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _allButton.onClick.RemoveListener( SelectAll );
        _businessButton.onClick.RemoveListener( SelectBusiness );
        _orderButton.onClick.RemoveListener( SelectOrder );
        _craftButton.onClick.RemoveListener( SelectCraft );
        _etcButton.onClick.RemoveListener( SelectEtc );
        _closeButton.onClick.RemoveListener( Close );

        for ( int i = 0; i < _slots.Count; i++ )
            _slots [ i ].OnClaim -= Claim;
    }

    /// <summary>
    /// 화면 비활성화 시 중단된 스크롤 루틴 참조 초기화
    /// </summary>
    void OnDisable ()
    {
        _scrollResetRoutine = null;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 업적 목록 표시
    /// </summary>
    /// <param name="viewDatas">표시할 업적 목록</param>
    public void ShowList ( IReadOnlyList<AchvViewData> viewDatas )
    {
        gameObject.SetActive ( true );
        EnsureSlotCount( viewDatas.Count );

        for ( int i = 0; i < viewDatas.Count; i++ )
        {
            AchvViewData viewData = viewDatas [ i ];
            Sprite icon = _iconData != null
                ? _iconData.GetIcon( viewData.IconType )
                : null;

            _slots [ i ].Show( viewData, icon );
        }

        //현재 목록에서 사용하지 않는 기존 슬롯 숨김
        for ( int i = viewDatas.Count; i < _slots.Count; i++ )
            _slots [ i ].Hide( );

        ResetScrollToTop( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 업적 목록 스크롤을 레이아웃 갱신 후 최상단으로 이동
    /// </summary>
    void ResetScrollToTop ()
    {
        if ( _scrollResetRoutine != null )
            StopCoroutine( _scrollResetRoutine );

        _scrollResetRoutine = StartCoroutine(
            ResetScrollToTopRoutine( ) );
    }

    /// <summary>
    /// 활성 업적 스크롤의 최상단 위치 확정
    /// </summary>
    /// <returns>레이아웃 갱신 대기</returns>
    IEnumerator ResetScrollToTopRoutine ()
    {
        yield return null;

        Canvas.ForceUpdateCanvases( );
        ScrollRect scrollRect = _content.GetComponentInParent<ScrollRect>( );

        if ( scrollRect != null )
        {
            scrollRect.StopMovement( );
            LayoutRebuilder.ForceRebuildLayoutImmediate( scrollRect.content );
            scrollRect.verticalNormalizedPosition = 1f;

            yield return new WaitForEndOfFrame( );
            scrollRect.verticalNormalizedPosition = 1f;
        }

        _scrollResetRoutine = null;
    }

    /// <summary>
    /// 업적 화면 숨김
    /// </summary>
    public void Hide ()
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 업적 화면 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 현재 업적 목록에서 가이드에 사용할 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 튜토리얼 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>활성 대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        target = null;

        AchvSlotView fallback = null;

        for ( int i = 0; i < _slots.Count; i++ )
        {
            AchvSlotView slot = _slots [ i ];

            if ( slot.gameObject.activeInHierarchy == false )
                continue;

            fallback ??= slot;

            bool isMatched = targetId switch
            {
                TutorialTargetId.AchievementClaimButton => slot.CanClaim,
                TutorialTargetId.AchievementReward => slot.CanClaim,
                TutorialTargetId.AchievementStage => slot.IsMultiStage,
                TutorialTargetId.AchievementProgress => slot.IsProgressing,
                _ => false,
            };

            if ( isMatched &&
                slot.TryGetTutorialTarget( targetId, out target ) )
            {
                return true;
            }
        }

        //검증 데이터에 진행 중 또는 다단계 업적이 없어도 설명 대상은 유지
        return fallback != null &&
            fallback.TryGetTutorialTarget( targetId, out target );
    }

    /// <summary>
    /// 업적 화면 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    #endregion

    #region ----- 슬롯 관리 -----

    /// <summary>
    /// Content에 업적 슬롯 등록
    /// </summary>
    void CollectExistingSlots ()
    {
        AchvSlotView [ ] slots =
            _content.GetComponentsInChildren<AchvSlotView>( true );

        for ( int i = 0; i < slots.Length; i++ )
            AddSlot( slots [ i ] );
    }

    /// <summary>
    /// 목록 표시에 필요한 업적 슬롯 수 확보
    /// </summary>
    /// <param name="count">필요한 슬롯 수</param>
    void EnsureSlotCount ( int count )
    {
        while ( _slots.Count < count )
        {
            AchvSlotView slot = Instantiate( _slotPrefab, _content );

            AddSlot( slot );
        }
    }

    /// <summary>
    /// 재사용할 업적 슬롯 등록
    /// </summary>
    /// <param name="slot">등록할 업적 슬롯</param>
    void AddSlot ( AchvSlotView slot )
    {
        slot.OnClaim += Claim;
        _slots.Add( slot );
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 전체 분류 선택
    /// </summary>
    void SelectAll ()
    {
        OnAllSelected?.Invoke( );
    }

    /// <summary>
    /// 영업 분류 선택
    /// </summary>
    void SelectBusiness ()
    {
        OnCategorySelected?.Invoke( AchvCategory.Business );
    }

    /// <summary>
    /// 주문 분류 선택
    /// </summary>
    void SelectOrder ()
    {
        OnCategorySelected?.Invoke( AchvCategory.Order );
    }

    /// <summary>
    /// 제작 분류 선택
    /// </summary>
    void SelectCraft ()
    {
        OnCategorySelected?.Invoke( AchvCategory.Craft );
    }

    /// <summary>
    /// 기타 분류 선택
    /// </summary>
    void SelectEtc ()
    {
        OnCategorySelected?.Invoke( AchvCategory.Etc );
    }

    /// <summary>
    /// 업적 보상 수령 전달
    /// </summary>
    /// <param name="id">보상을 수령할 업적 아이디</param>
    void Claim ( string id )
    {
        OnClaim?.Invoke( id );
    }

    /// <summary>
    /// 업적 화면 닫기 전달
    /// </summary>
    void Close ()
    {
        OnClosed?.Invoke( );
    }

    #endregion
}
