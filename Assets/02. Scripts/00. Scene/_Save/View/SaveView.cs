using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 저장 슬롯 목록과 확인 입력 뷰
/// </summary>
public class SaveView : MonoBehaviour
{
    [SerializeField] SaveSlotView [ ] _slots;       //저장 슬롯 뷰

    bool _isConfirmOpen;       //확인 패널 표시 여부
    Coroutine _scrollResetRoutine;       //스크롤 위치 초기화 코루틴

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //저장 패널 연출
    [SerializeField] Button _closeButton;       //저장 화면 닫기 버튼
    [SerializeField] PanelTweenView _confirmPanel;       //확인 패널 연출
    [SerializeField] TMP_Text _confirmText;       //확인 문구
    [SerializeField] Button _confirmButton;       //확인 버튼
    [SerializeField] Button _cancelButton;       //취소 버튼

    /// <summary>
    /// 저장 화면 닫기 이벤트
    /// </summary>
    public event Action OnClose;

    /// <summary>
    /// 슬롯 저장 입력 이벤트
    /// </summary>
    public event Action<int> OnSave;

    /// <summary>
    /// 슬롯 불러오기 입력 이벤트
    /// </summary>
    public event Action<int> OnLoad;

    /// <summary>
    /// 슬롯 삭제 입력 이벤트(슬롯 번호)
    /// </summary>
    public event Action<int> OnDelete;

    /// <summary>
    /// 확인 입력 이벤트
    /// </summary>
    public event Action OnConfirmed;

    /// <summary>
    /// 확인 취소 입력 이벤트
    /// </summary>
    public event Action OnCanceled;

    #region ----- 초기화 -----

    /// <summary>
    /// 저장 뷰 입력 이벤트 연결
    /// </summary>
    void Awake ()
    {
        //기존 Play 씬의 닫기 버튼 참조 보완
        if ( _closeButton == null )
            _closeButton = FindButton( "CloseDetailButton" );

        //저장 확인과 취소 버튼에 공용 클릭 연출 연결
        if ( _closeButton != null )
            _closeButton.BindClickHighlight( );

        _confirmButton.BindClickHighlight( );
        _cancelButton.BindClickHighlight( );

        for ( int i = 0; i < _slots.Length; i++ )
        {
            _slots [ i ].OnSave += Save;
            _slots [ i ].OnLoad += Load;
            _slots [ i ].OnDelete += Delete;
        }

        if ( _closeButton != null )
            _closeButton.onClick.AddListener( Close );

        _confirmButton.onClick.AddListener( Confirm );
        _cancelButton.onClick.AddListener( Cancel );

        HideConfirm( );
    }

    /// <summary>
    /// 저장 뷰 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        for ( int i = 0; i < _slots.Length; i++ )
        {
            _slots [ i ].OnSave -= Save;
            _slots [ i ].OnLoad -= Load;
            _slots [ i ].OnDelete -= Delete;
        }

        if ( _closeButton != null )
            _closeButton.onClick.RemoveListener( Close );

        _confirmButton.onClick.RemoveListener( Confirm );
        _cancelButton.onClick.RemoveListener( Cancel );
    }

    /// <summary>
    /// 현재 저장 화면의 이름으로 버튼 조회
    /// </summary>
    /// <param name="buttonName">찾을 버튼 오브젝트 이름</param>
    /// <returns>일치하는 버튼</returns>
    Button FindButton ( string buttonName )
    {
        Button [ ] buttons =
            GetComponentsInChildren<Button>( true );

        for ( int i = 0; i < buttons.Length; i++ )
        {
            if ( buttons [ i ].name == buttonName )
                return buttons [ i ];
        }

        return null;
    }

    #endregion

    #region ----- 표시 -----

    /// <summary>
    /// 전체 저장 슬롯 정보 표시
    /// </summary>
    /// <param name="slotDatas">전체 저장 슬롯 데이터</param>
    /// <param name="mode">저장 슬롯 사용 목적</param>
    public void SetSlots (
        IReadOnlyList<SaveSlotData> slotDatas,
        SaveSlotMode mode )
    {
        for ( int i = 0; i < _slots.Length; i++ )
            _slots [ i ].SetData( slotDatas [ i ], mode );
    }

    /// <summary>
    /// 사용자 확인 패널 표시
    /// </summary>
    /// <param name="message">확인 문구</param>
    public void ShowConfirm ( string message )
    {
        _confirmText.text = message;
        _isConfirmOpen = true;

        //CanvasGroup 입력 상태까지 함께 복구
        _confirmPanel.Show( );
    }

    /// <summary>
    /// 확인 패널 숨기기
    /// </summary>
    public void HideConfirm ()
    {
        _isConfirmOpen = false;
        _confirmPanel.SetVisible( false );
    }

    /// <summary>
    /// 저장 화면 표시
    /// </summary>
    public void Show ()
    {
        gameObject.SetActive ( true );
        ResetScrollsToTop( );
        _panelTween.Show( );
    }

    /// <summary>
    /// 저장 화면의 활성 스크롤을 최상단으로 이동
    /// </summary>
    void ResetScrollsToTop ()
    {
        if ( _scrollResetRoutine != null )
            StopCoroutine( _scrollResetRoutine );

        _scrollResetRoutine = StartCoroutine(
            ResetScrollsToTopRoutine( ) );
    }

    /// <summary>
    /// 레이아웃 갱신 이후 저장 화면 스크롤 위치 확정
    /// </summary>
    /// <returns>레이아웃 갱신 대기</returns>
    IEnumerator ResetScrollsToTopRoutine ()
    {
        yield return null;

        Canvas.ForceUpdateCanvases( );
        ScrollRect [ ] scrollRects =
            GetComponentsInChildren<ScrollRect>( false );

        for ( int i = 0; i < scrollRects.Length; i++ )
        {
            ScrollRect scrollRect = scrollRects [ i ];
            scrollRect.StopMovement( );

            if ( scrollRect.content != null )
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    scrollRect.content );

            scrollRect.verticalNormalizedPosition = 1f;
        }

        yield return new WaitForEndOfFrame( );

        for ( int i = 0; i < scrollRects.Length; i++ )
            scrollRects [ i ].verticalNormalizedPosition = 1f;

        _scrollResetRoutine = null;
    }

    /// <summary>
    /// 저장 화면 숨기기
    /// </summary>
    public void Hide ()
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 연출 없이 저장 화면 숨기기
    /// </summary>
    public void HideInstant ()
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 저장 화면 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }


    #endregion

    #region ----- 입력 -----

    /// <summary>
    /// 저장 화면 닫기 요청 전달
    /// </summary>
    void Close ()
    {
        OnClose?.Invoke( );
    }

    /// <summary>
    /// 선택 슬롯 저장 요청 전달
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    void Save ( int slotNumber )
    {
        if ( _isConfirmOpen ) return;

        OnSave?.Invoke( slotNumber );
    }

    /// <summary>
    /// 선택 슬롯 불러오기 요청 전달
    /// </summary>
    /// <param name="slotNumber">불러올 슬롯 번호</param>
    void Load ( int slotNumber )
    {
        if ( _isConfirmOpen ) return;

        OnLoad?.Invoke( slotNumber );
    }

    /// <summary>
    /// 슬롯 삭제 입력 전달
    /// </summary>
    /// <param name="slotNumber">입력한 슬롯 번호</param>
    void Delete ( int slotNumber )
    {
        if ( _isConfirmOpen ) return;

        OnDelete?.Invoke( slotNumber );
    }

    /// <summary>
    /// 확인 입력 전달
    /// </summary>
    void Confirm ()
    {
        HideConfirm( );
        OnConfirmed?.Invoke( );
    }

    /// <summary>
    /// 확인 취소 요청 전달
    /// </summary>
    void Cancel ()
    {
        HideConfirm( );
        OnCanceled?.Invoke( );
    }

    #endregion
}
