using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작 정보 패널 뷰 - 패널 표시와 사용 파츠, 테마와 점수, 주문 상세 전환 입력 처리
/// </summary>
public class CraftInfoPanelView : MonoBehaviour
{
    /// <summary>
    /// 제작 정보 Content 종류
    /// </summary>
    enum InfoContent
    {
        OrderDetail,        //주문 상세
        UsedParts,      //사용 파츠
        ThemeScore,     //테마와 점수
    }

    [Header( "----- 컴포넌트 -----" )]
    [Header( "--- Content ---" )]
    [SerializeField] GameObject _usedPartsContent;       //사용 파츠 Content
    [SerializeField] GameObject _themeScoreContent;      //테마와 점수 Content
    [SerializeField] GameObject _orderDetailContent;     //주문 상세 Content

    [Header( "--- 패널 ---" )]
    [SerializeField] Button _openCloseButton;        //패널 열기, 닫기 버튼
    [SerializeField] Button _switchButton;       //Content 전환 버튼
    [SerializeField] TMP_Text _openCloseButtonText;      //패널 열기, 닫기 버튼 문구
    [SerializeField] TMP_Text _switchButtonText;     //Content 전환 버튼 문구
    [SerializeField] CanvasGroup _contentCanvasGroup;       //Scroll View 내용 표시 그룹

    [Header( "--- 펼침 연출 ---" )]
    [SerializeField, Min( 0f )]
    float _openDuration = 0.25f;       //종이가 펼쳐지는 시간
    [SerializeField, Min( 0f )]
    float _closeDuration = 0.2f;       //종이가 접히는 시간
    [SerializeField, Min( 0f )]
    float _contentFadeDuration = 0.08f;       //내용 표시와 숨김 시간

    RectTransform _rectTransform;     //제작 정보 패널 RectTransform
    float _openHeight;       //패널이 완전히 열린 높이
    Sequence _panelTween;       //현재 실행 중인 패널 트윈
    bool _isOpen;     //제작 정보 패널 열림 여부
    InfoContent _currentContent = InfoContent.OrderDetail;      //현재 표시 Content

    /// <summary>
    /// 패널 열기, 닫기 이벤트
    /// </summary>
    public event Action OnOpenClose;

    /// <summary>
    /// 패널 열기 또는 닫기 연출 완료 이벤트
    /// </summary>
    public event Action<bool> OnPanelTransitionCompleted;

    /// <summary>
    /// 사용 파츠 Content 표시 이벤트
    /// </summary>
    public event Action OnUsedPartsShown;

    /// <summary>
    /// 현재 제작 정보 패널 열림 여부
    /// </summary>
    public bool IsOpen => _isOpen;

    /// <summary>
    /// 제작 정보 패널 열기 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">패널 열기 버튼 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetOpenCloseTarget ( out RectTransform target )
    {
        target = _openCloseButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// Content 전환 버튼의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="target">Content 전환 버튼 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetSwitchTarget ( out RectTransform target )
    {
        target = _switchButton.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 비활성 상태에서도 사용할 패널 크기와 입력 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        //현재 Inspector 높이를 열린 상태 기준으로 저장
        _rectTransform = GetComponent<RectTransform>( );
        _openHeight = _rectTransform.sizeDelta.y;

        //패널 입력 연결
        _openCloseButton.onClick.AddListener( OpenClosePanel );
        _switchButton.onClick.AddListener( SwitchContent );

        //주문 상세 Content를 기본으로 표시
        SetContent( InfoContent.OrderDetail );
    }

    /// <summary>
    /// 제작 정보 패널 활성화 시 주문 상세로 초기화
    /// </summary>
    void OnEnable ()
    {
        ShowOrderDetail( );
    }

    /// <summary>
    /// 비활성화 시 실행 중인 패널 연출 제거
    /// </summary>
    void OnDisable ()
    {
        KillPanelTween( );
    }

    /// <summary>
    /// 제작 정보 패널 입력과 연출 해제
    /// </summary>
    void OnDestroy ()
    {
        KillPanelTween( );

        //패널 입력 해제
        _openCloseButton.onClick.RemoveListener( OpenClosePanel );
        _switchButton.onClick.RemoveListener( SwitchContent );
    }

    #region ----- Content 표시 -----
    /// <summary>
    /// 주문 상세 Content 표시
    /// </summary>
    public void ShowOrderDetail ()
    {
        SetContent( InfoContent.OrderDetail );
    }

    /// <summary>
    /// 사용 파츠 Content 표시
    /// </summary>
    public void ShowUsedParts ()
    {
        SetContent( InfoContent.UsedParts );
    }

    /// <summary>
    /// Content 전환
    /// </summary>
    void SwitchContent ()
    {
        //닫힌 패널은 주문 상세를 유지한 상태로 열기
        if ( _isOpen == false )
        {
            ShowOrderDetail( );
            OnOpenClose?.Invoke( );
            return;
        }

        //다음 Content로 전환
        SetContent( GetNextContent( ) );
    }

    /// <summary>
    /// 다음에 표시할 Content 반환
    /// </summary>
    InfoContent GetNextContent ()
    {
        if ( _currentContent == InfoContent.UsedParts )
            return InfoContent.ThemeScore;

        if ( _currentContent == InfoContent.ThemeScore )
            return InfoContent.OrderDetail;

        return InfoContent.UsedParts;
    }

    /// <summary>
    /// 표시할 Content 설정
    /// </summary>
    /// <param name="content">표시할 Content</param>
    void SetContent ( InfoContent content )
    {
        _currentContent = content;

        //선택한 Content만 표시
        _usedPartsContent.SetActive( content == InfoContent.UsedParts );
        _themeScoreContent.SetActive( content == InfoContent.ThemeScore );
        _orderDetailContent.SetActive( content == InfoContent.OrderDetail );

        if ( content == InfoContent.UsedParts )
            OnUsedPartsShown?.Invoke( );

        //현재 패널과 Content 상태에 맞는 버튼 문구 갱신
        UpdateButtonTexts( );
    }
    #endregion

    #region ----- 패널 표시 -----
    /// <summary>
    /// 패널 열기, 닫기 입력 중계
    /// </summary>
    void OpenClosePanel ()
    {
        //닫힌 패널을 일반 열기 버튼으로 열면 주문 상세부터 표시
        if ( _isOpen == false )
            ShowOrderDetail( );

        OnOpenClose?.Invoke( );
    }

    /// <summary>
    /// 제작 정보 패널 열림 상태 설정
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    /// <param name="playAnimation">펼침 연출 재생 여부</param>
    public void SetPanelOpen (
        bool isOpen, bool playAnimation = true )
    {
        _isOpen = isOpen;

        KillPanelTween( );

        if ( playAnimation == false )
        {
            SetPanelOpenInstant( isOpen );
            return;
        }

        if ( isOpen )
            PlayOpen( );
        else
            PlayClose( );

        //현재 패널과 Content 상태에 맞는 버튼 문구 갱신
        UpdateButtonTexts( );
    }

    /// <summary>
    /// 제작 정보 패널을 즉시 열린 상태 또는 닫힌 상태로 설정
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    void SetPanelOpenInstant ( bool isOpen )
    {
        //버튼이 유지되도록 패널 오브젝트는 활성 상태로 사용
        gameObject.SetActive( true );

        SetPanelHeight( isOpen ? _openHeight : 0f );

        _contentCanvasGroup.alpha = isOpen ? 1f : 0f;
        _contentCanvasGroup.interactable = isOpen;
        _contentCanvasGroup.blocksRaycasts = isOpen;

        UpdateButtonTexts( );
        OnPanelTransitionCompleted?.Invoke( _isOpen );
    }

    /// <summary>
    /// 종이가 위로 펼쳐진 뒤 내용 표시
    /// </summary>
    void PlayOpen ()
    {
        gameObject.SetActive( true );

        _contentCanvasGroup.interactable = false;
        _contentCanvasGroup.blocksRaycasts = false;

        _panelTween = DOTween.Sequence( )
            .SetUpdate( true )
            .Append(
                _rectTransform.DOSizeDelta(
                    new Vector2( _rectTransform.sizeDelta.x, _openHeight ), _openDuration )
                .SetEase( Ease.OutCubic ) )
            .Append(
                _contentCanvasGroup.DOFade( 1f, _contentFadeDuration ) )
            .OnComplete( CompleteOpen );
    }

    /// <summary>
    /// 내용을 숨긴 뒤 종이를 아래로 접음
    /// </summary>
    void PlayClose ()
    {
        _contentCanvasGroup.interactable = false;
        _contentCanvasGroup.blocksRaycasts = false;

        _panelTween = DOTween.Sequence( )
            .SetUpdate( true )
            .Append(
                _contentCanvasGroup.DOFade( 0f, _contentFadeDuration ) )
            .Append(
                _rectTransform.DOSizeDelta(
                    new Vector2(
                        _rectTransform.sizeDelta.x, 0f ), _closeDuration )
                .SetEase( Ease.InCubic ) )
            .OnComplete( CompleteClose );
    }

    /// <summary>
    /// 패널 열기 완료 처리
    /// </summary>
    void CompleteOpen ()
    {
        _panelTween = null;

        _contentCanvasGroup.alpha = 1f;
        _contentCanvasGroup.interactable = true;
        _contentCanvasGroup.blocksRaycasts = true;
        OnPanelTransitionCompleted?.Invoke( true );
    }

    /// <summary>
    /// 패널 닫기 완료 후 닫힌 높이 고정
    /// </summary>
    void CompleteClose ()
    {
        _panelTween = null;

        SetPanelHeight( 0f );
        OnPanelTransitionCompleted?.Invoke( false );
    }

    /// <summary>
    /// 제작 정보 패널 높이 즉시 설정
    /// </summary>
    /// <param name="height">설정할 패널 높이</param>
    void SetPanelHeight ( float height )
    {
        Vector2 panelSize = _rectTransform.sizeDelta;
        panelSize.y = height;

        _rectTransform.sizeDelta = panelSize;
    }

    /// <summary>
    /// 실행 중인 패널 트윈 제거
    /// </summary>
    void KillPanelTween ()
    {
        if ( _panelTween == null ) return;

        _panelTween.Kill( );
        _panelTween = null;
    }

    /// <summary>
    /// 현재 패널과 Content 상태에 맞는 버튼 문구 갱신
    /// </summary>
    void UpdateButtonTexts ()
    {
        //패널 상태 표시
        _openCloseButtonText.text = _isOpen ? "닫기" : "열기";

        //닫힌 상태에서는 처음 표시할 주문 상세 안내
        if ( _isOpen == false )
        {
            _switchButtonText.text = "주문 상세";
            return;
        }

        //열린 상태에서는 다음에 표시할 Content 안내
        if ( _currentContent == InfoContent.UsedParts )
            _switchButtonText.text = "테마/점수";
        else if ( _currentContent == InfoContent.ThemeScore )
            _switchButtonText.text = "주문 상세";
        else
            _switchButtonText.text = "사용 파츠";
    }
    #endregion
}
