using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작 뷰 - 배치 파츠 생성과 표시, 제작 영역 입력 중계
/// </summary>
public class CraftView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;             //제작 패널 연출
    [SerializeField] Canvas _canvas;                         //제작 UI 캔버스
    [SerializeField] RectTransform _babyBuildArea;           //제작 영역
    [SerializeField] RectTransform _placedPartsRoot;         //배치 파츠 부모
    [SerializeField] Button _buildAreaButton;                //빈 제작 영역 선택 버튼
    [SerializeField] Button _backToListButton;       //제작 목록 돌아가기 버튼
    [SerializeField] Button _craftDoneButton;       //제작 완료 버튼
    [SerializeField] Button _craftExitButton;       //제작 화면 나가기 버튼

    [Header( "--- 제작 패널 ---" )]
    [SerializeField] GameObject _craftListPanel;      //제작 주문 목록 패널
    [SerializeField] GameObject _craftPanel;          //파츠 제작 패널

    [Header( "--- 사이드 패널 ---" )]
    [SerializeField] PartsSelectView _partsSelectView;          //파츠 선택 뷰
    [SerializeField] CraftInfoPanelView _craftInfoPanelView;        //제작 정보 패널 뷰

    [Header( "--- 제작 정보 ---" )]
    [SerializeField] GameObject _craftFailFrame;       //제작 실패 안내 프레임
    [SerializeField] TMP_Text _craftFailText;                    //제작 실패 안내 문구
    [SerializeField, Min( 0f )]
    float _craftFailureDuration = 2f;       //제작 실패 안내 유지 시간
    [SerializeField] TMP_Text _costLimitText;                    //현재 코스트와 최대 코스트

    [Header( "----- 프리팹 -----" )]
    [SerializeField] PartsView _partsViewPrefab;             //배치 파츠 프리팹

    [Header( "----- 파츠 생성 위치 -----" )]
    [SerializeField]
    Vector2 [ ] _spawnPositions =
    {
        Vector2.zero,
        new Vector2( 80f, 80f ),
        new Vector2( -80f, -80f ),
        new Vector2( 80f, -80f ),
        new Vector2( -80f, 80f )
    };

    int _nextSpawnIndex;                                    //다음 생성 위치 인덱스
    bool _isPartsSelectOpen;        //파츠 선택 패널 열림 여부
    bool _isCraftInfoOpen;      //제작 정보 패널 열림 여부
    bool _isPanelDisplayInitialized;       //사이드 패널 초기 표시 완료 여부
    Tween _craftFailureHideTween;       //제작 실패 안내 종료 예약

    /// <summary>
    /// 배치 번호별 파츠 뷰
    /// </summary>
    Dictionary<int, PartsView> _partsViews = new Dictionary<int, PartsView>( );

    /// <summary>
    /// 파츠 제작 패널 표시 여부
    /// </summary>
    public bool IsCraftPanelVisible =>
        gameObject.activeInHierarchy && _craftPanel.activeSelf;

    /// <summary>
    /// 파츠 선택 패널 열림 여부
    /// </summary>
    public bool IsPartsSelectOpen => _partsSelectView.IsOpen;

    /// <summary>
    /// 제작 정보 패널 열림 여부
    /// </summary>
    public bool IsCraftInfoOpen => _craftInfoPanelView.IsOpen;

    /// <summary>
    /// 배치 파츠의 튜토리얼 강조 대상 조회
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="target">배치 파츠 영역</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetPartTarget (
        int placementNumber, out RectTransform target )
    {
        if ( _partsViews.TryGetValue(
            placementNumber, out PartsView partsView ) == false )
        {
            target = null;
            return false;
        }

        target = partsView.transform as RectTransform;
        return target != null;
    }

    /// <summary>
    /// 제작 화면의 고정 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        switch ( targetId )
        {
            case TutorialTargetId.CraftCost:
                target = _costLimitText.rectTransform;
                break;

            case TutorialTargetId.CraftDoneButton:
                target = _craftDoneButton.transform as RectTransform;
                break;

            case TutorialTargetId.PartsSelectOpenButton:
                return _partsSelectView.TryGetOpenCloseTarget( out target );

            case TutorialTargetId.CraftInfoOpenButton:
                return _craftInfoPanelView.TryGetOpenCloseTarget( out target );

            case TutorialTargetId.CraftInfoSwitchButton:
                return _craftInfoPanelView.TryGetSwitchTarget( out target );

            default:
                target = null;
                return false;
        }

        return target != null;
    }

    #region ----- 이벤트 -----
    /// <summary>
    /// 배치 파츠 선택 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnPartSelected;

    /// <summary>
    /// 배치 파츠 드래그 시작 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnPartDragStarted;

    /// <summary>
    /// 배치 파츠 드래그 이동 이벤트(배치 번호, 제작 영역 로컬 좌표)
    /// </summary>
    public event Action<int, Vector2> OnPartDragged;

    /// <summary>
    /// 배치 파츠 드래그 종료 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnPartDragEnded;

    /// <summary>
    /// 선택 파츠 스케일 입력 이벤트(휠 방향)
    /// </summary>
    public event Action<int , float> OnPartScaleInput;

    /// <summary>
    /// 선택 파츠 핀치 스케일 입력 이벤트(배치 번호, 거리 비율)
    /// </summary>
    public event Action<int, float> OnPartPinchScaleInput;

    /// <summary>
    /// 배치 파츠 제거 이벤트(배치 번호)
    /// </summary>
    public event Action<int> OnPartRemoved;

    /// <summary>
    /// 빈 제작 영역 선택 이벤트
    /// </summary>
    public event Action OnEmptyAreaSelected;

    /// <summary>
    /// 제작 주문 목록 돌아가기 이벤트
    /// </summary>
    public event Action OnBackToList;

    /// <summary>
    /// 제작 화면 나가기 이벤트
    /// </summary>
    public event Action OnCraftClose;

    /// <summary>
    /// 제작 완료 이벤트
    /// </summary>
    public event Action OnCraftDone;

    /// <summary>
    /// 파츠 선택 패널 열기 또는 닫기 연출 완료 이벤트
    /// </summary>
    public event Action<bool> OnPartsSelectPanelChanged;

    /// <summary>
    /// 제작 정보 패널 열기 또는 닫기 연출 완료 이벤트
    /// </summary>
    public event Action<bool> OnCraftInfoPanelChanged;

    /// <summary>
    /// 제작 정보 패널에서 사용 파츠 표시 이벤트
    /// </summary>
    public event Action OnUsedPartsShown;
    #endregion

    /// <summary>
    /// 비활성 제작 사이드 패널 초기화
    /// </summary>
    public void InitializeRuntime ()
    {
        _partsSelectView.InitializeRuntime( );
        _craftInfoPanelView.InitializeRuntime( );
    }

    /// <summary>
    /// 제작 영역 입력 연결
    /// </summary>
    void Awake ()
    {
        //제작 처리 주요 버튼에 공용 클릭 연출 연결
        _backToListButton.BindClickHighlight( );
        _craftDoneButton.BindClickHighlight( );
        _craftExitButton.BindClickHighlight( );

        //제작 영역 입력 연결
        _buildAreaButton.onClick.AddListener( SelectEmptyArea );
        _backToListButton.onClick.AddListener( BackToList );
        _craftDoneButton.onClick.AddListener( CompleteCraft );
        _craftExitButton.onClick.AddListener( CloseCraft );

        //사이드 패널 입력 연결
        _partsSelectView.OnOpenClose += OpenClosePartsSelect;
        _craftInfoPanelView.OnOpenClose += OpenCloseCraftInfo;
        _partsSelectView.OnPanelTransitionCompleted +=
            HandlePartsSelectPanelChanged;
        _craftInfoPanelView.OnPanelTransitionCompleted +=
            HandleCraftInfoPanelChanged;
        _craftInfoPanelView.OnUsedPartsShown += HandleUsedPartsShown;
    }

    /// <summary>
    /// 제작 영역 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        //오브젝트 제거 전에 실패 안내 종료 예약 정리
        KillCraftFailureHideTween( );

        //제작 영역 입력 해제
        _buildAreaButton.onClick.RemoveListener( SelectEmptyArea );
        _backToListButton.onClick.RemoveListener( BackToList );
        _craftDoneButton.onClick.RemoveListener( CompleteCraft );
        _craftExitButton.onClick.RemoveListener( CloseCraft );

        //사이드 패널 입력 해제
        _partsSelectView.OnOpenClose -= OpenClosePartsSelect;
        _craftInfoPanelView.OnOpenClose -= OpenCloseCraftInfo;
        _partsSelectView.OnPanelTransitionCompleted -=
            HandlePartsSelectPanelChanged;
        _craftInfoPanelView.OnPanelTransitionCompleted -=
            HandleCraftInfoPanelChanged;
        _craftInfoPanelView.OnUsedPartsShown -= HandleUsedPartsShown;
    }

    #region ----- 배치 파츠 생성/제거 -----

    /// <summary>
    /// 배치 파츠 뷰 생성
    /// </summary>
    /// <param name="placedPart">배치 파츠 데이터</param>
    /// <param name="playAppear">신규 배치 등장 연출 여부</param>
    /// <returns>생성 성공 여부</returns>
    public bool CreatePart (
        PlacedPartData placedPart,
        bool playAppear )
    {
        //표시할 데이터가 없으면 생성x
        if ( placedPart == null ) return false;

        //같은 배치 번호의 뷰가 있으면 중복 생성x
        if ( _partsViews.ContainsKey( placedPart.PlacementNumber ) ) return false;

        //파츠 뷰 복제 생성
        PartsView partsView = Instantiate( _partsViewPrefab, _placedPartsRoot );

        //초기화
        partsView.Init(
            placedPart.PlacementNumber, placedPart.PartData.Icon,
            placedPart.LocalPosition, placedPart.Rotation,
            placedPart.Scale, placedPart.PartIndex,
            playAppear );

        //파츠 입력 연결
        SubscribePart( partsView );

        //배치 번호로 뷰 추가
        _partsViews.Add( placedPart.PlacementNumber, partsView );

        //파츠 생성 위치 순번 진행
        MoveNextSpawnPosition( );

        return true;
    }

    /// <summary>
    /// 배치 파츠 뷰 제거
    /// </summary>
    /// <param name="placementNumber">제거할 배치 번호</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemovePart ( int placementNumber )
    {
        //배치 번호 가져오기
        if ( _partsViews.TryGetValue( placementNumber, out PartsView partsView ) == false )
            return false;

        //파츠 입력 해제
        UnsubscribePart( partsView );

        //배치 상태에서 제거 후 오브젝트 제거
        _partsViews.Remove( placementNumber );
        Destroy( partsView.gameObject );

        return true;
    }

    /// <summary>
    /// 모든 배치 파츠 뷰 제거
    /// </summary>
    public void ClearParts ()
    {
        foreach ( PartsView partsView in _partsViews.Values )
        {
            //파츠 입력 해제 후 오브젝트 제거
            UnsubscribePart( partsView );
            Destroy( partsView.gameObject );
        }

        _partsViews.Clear( );
        _nextSpawnIndex = 0;
    }

    #endregion

    #region ----- 배치 파츠 표시 갱신 -----

    /// <summary>
    /// 배치 파츠 표시 데이터 갱신
    /// </summary>
    /// <param name="placedPart">갱신할 배치 파츠 데이터</param>
    /// <returns>갱신 성공 여부</returns>
    public bool UpdatePart ( PlacedPartData placedPart )
    {
        //갱신할 데이터가 없으면 종료
        if ( placedPart == null ) return false;

        //배치 번호에 해당하는 View 조회
        if ( _partsViews.TryGetValue (
            placedPart.PlacementNumber , out PartsView partsView ) == false )
            return false;

        //기존 등장이나 안착 연출을 제거하고 데이터 상태 즉시 표시
        partsView.ResetVisual( );

        //파츠 아이콘 갱신
        partsView.SetIcon ( placedPart.PartData.Icon );
        //파츠 위치 갱신
        partsView.SetPosition ( placedPart.LocalPosition );
        //파츠 회전값 갱신
        partsView.SetRotation ( placedPart.Rotation );
        //파츠 스케일 갱신
        partsView.SetScale ( placedPart.Scale );
        //파츠 앞뒤 순서 갱신
        partsView.SetPartIndex ( placedPart.PartIndex );

        return true;
    }

    /// <summary>
    /// 선택 파츠 표시 갱신
    /// </summary>
    /// <param name="placementNumber">선택 배치 번호, 0이면 선택 해제</param>
    public void SetSelectedPart ( int placementNumber )
    {
        foreach ( var pair in _partsViews )
        {
            bool isSelected = pair.Key == placementNumber;
            pair.Value.SetSelected( isSelected );
        }
    }

    /// <summary>
    /// 배치 파츠 위치 갱신
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="localPosition">제작 영역 기준 로컬 위치</param>
    public void SetPartPosition ( int placementNumber, Vector2 localPosition )
    {
        //배치 번호 가져오기
        if ( _partsViews.TryGetValue( placementNumber, out PartsView partsView ) == false )
            return;

        partsView.SetPosition( localPosition );
    }

    /// <summary>
    /// 배치 파츠 회전값 갱신
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="rotation">회전값</param>
    public void SetPartRotation ( int placementNumber, float rotation )
    {
        //배치 번호 가져오기
        if ( _partsViews.TryGetValue( placementNumber, out PartsView partsView ) == false )
            return;

        partsView.SetRotation( rotation );
    }

    /// <summary>
    /// 배치 파츠 균등 스케일 갱신
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="scale">균등 스케일</param>
    public void SetPartScale ( int placementNumber, float scale )
    {
        //배치 번호 가져오기
        if ( _partsViews.TryGetValue( placementNumber, out PartsView partsView ) == false )
            return;

        partsView.SetScale( scale );
    }

    /// <summary>
    /// 배치 파츠를 임시로 가장 앞에 표시
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    public void ShowPartInFront ( int placementNumber )
    {
        if ( _partsViews.TryGetValue( placementNumber, out PartsView partsView ) == false )
            return;

        partsView.ShowInFront( );
    }

    /// <summary>
    /// 앞뒤 순서 복구
    /// </summary>
    /// <param name="placedParts">현재 배치 파츠 목록</param>
    public void RestorePartIndexes ( IReadOnlyList<PlacedPartData> placedParts )
    {
        for ( int i = 0; i < placedParts.Count; i++ )
        {
            PlacedPartData placedPart = placedParts [ i ];

            if ( _partsViews.TryGetValue( placedPart.PlacementNumber, out PartsView partsView ) == false )
                continue;

            partsView.SetPartIndex( placedPart.PartIndex );
        }
    }

    #endregion

    #region ----- 제작 정보 표시 -----
    /// <summary>
    /// 현재 제작 코스트 표시 갱신
    /// </summary>
    /// <param name="currentCost">현재 제작 코스트</param>
    /// <param name="maxCost">주문 최대 제작 코스트</param>
    public void UpdateCost ( int currentCost, int maxCost )
    {
        _costLimitText.text = $"{currentCost} / {maxCost}";
    }

    /// <summary>
    /// 제작 실패 안내 문구 표시
    /// </summary>
    public void ShowCraftFailure ( string message )
    {
        //연속 실패 시 이전 종료 예약 제거
        KillCraftFailureHideTween( );

        _craftFailText.text = message;
        _craftFailFrame.SetActive( true );

        //지정 시간이 지나면 프레임과 문구를 함께 숨김
        _craftFailureHideTween = DOVirtual.DelayedCall(
                _craftFailureDuration,
                CompleteCraftFailure )
            .SetUpdate( true );
    }

    /// <summary>
    /// 제작 실패 안내 문구 초기화
    /// </summary>
    public void ClearCraftFailure ()
    {
        KillCraftFailureHideTween( );
        HideCraftFailure( );
    }

    /// <summary>
    /// 제작 실패 안내 종료 예약 완료 처리
    /// </summary>
    void CompleteCraftFailure ()
    {
        _craftFailureHideTween = null;
        HideCraftFailure( );
    }

    /// <summary>
    /// 제작 실패 프레임과 문구 숨김
    /// </summary>
    void HideCraftFailure ()
    {
        _craftFailText.text = string.Empty;
        _craftFailFrame.SetActive( false );
    }

    /// <summary>
    /// 실행 중인 제작 실패 안내 종료 예약 제거
    /// </summary>
    void KillCraftFailureHideTween ()
    {
        _craftFailureHideTween?.Kill( );
        _craftFailureHideTween = null;
    }
    #endregion

    #region ----- 파츠 생성 위치 -----

    /// <summary>
    /// 현재 파츠 생성 위치 반환
    /// </summary>
    /// <returns>제작 영역 기준 로컬 위치</returns>
    public Vector2 GetSpawnPosition ()
    {
        //스폰 위치 미지정 시 중앙 위치 반환
        if ( _spawnPositions == null || _spawnPositions.Length == 0 )
            return Vector2.zero;

        return _spawnPositions [ _nextSpawnIndex ];
    }

    /// <summary>
    /// 파츠 생성 위치 순번 진행
    /// </summary>
    void MoveNextSpawnPosition ()
    {
        //스폰 위치 미지정 시 종료
        if ( _spawnPositions == null || _spawnPositions.Length == 0 )
            return;

        //다음 생성 위치 인덱스 설정
        _nextSpawnIndex = ( _nextSpawnIndex + 1 ) % _spawnPositions.Length;
    }

    /// <summary>
    /// 파츠 생성 위치 순환 초기화
    /// </summary>
    public void ResetSpawnPosition ()
    {
        _nextSpawnIndex = 0;
    }

    #endregion

    #region ----- 파츠 입력 중계 -----

    /// <summary>
    /// 배치 파츠 입력 연결
    /// </summary>
    /// <param name="partsView">연결할 파츠 뷰</param>
    void SubscribePart ( PartsView partsView )
    {
        partsView.OnSelected += SelectPart;
        partsView.OnDragStarted += StartPartDrag;
        partsView.OnDragged += DragPart;
        partsView.OnDragEnded += EndPartDrag;
        partsView.OnScaleInput += ScalePart;
        partsView.OnPinchScaleInput += PinchScalePart;
        partsView.OnRemoved += RemovePartInput;
    }

    /// <summary>
    /// 배치 파츠 입력 해제
    /// </summary>
    /// <param name="partsView">해제할 파츠 뷰</param>
    void UnsubscribePart ( PartsView partsView )
    {
        partsView.OnSelected -= SelectPart;
        partsView.OnDragStarted -= StartPartDrag;
        partsView.OnDragged -= DragPart;
        partsView.OnDragEnded -= EndPartDrag;
        partsView.OnScaleInput -= ScalePart;
        partsView.OnPinchScaleInput -= PinchScalePart;
        partsView.OnRemoved -= RemovePartInput;
    }

    /// <summary>
    /// 배치 파츠 선택 입력 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    void SelectPart ( int placementNumber )
    {
        OnPartSelected?.Invoke( placementNumber );
    }

    /// <summary>
    /// 배치 파츠 드래그 시작 입력 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    void StartPartDrag ( int placementNumber )
    {
        OnPartDragStarted?.Invoke( placementNumber );
    }

    /// <summary>
    /// 배치 파츠 드래그 좌표 변환 후 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="screenPosition">화면 좌표</param>
    void DragPart ( int placementNumber, Vector2 screenPosition )
    {
        Camera eventCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        //화면 좌표를 제작 영역 기준 로컬 좌표로 변환
        bool isConverted = RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                _babyBuildArea, screenPosition, eventCamera,
                out Vector2 localPosition );

        if ( isConverted == false ) return;

        OnPartDragged?.Invoke( placementNumber, localPosition );
    }

    /// <summary>
    /// 배치 파츠 드래그 종료 입력 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    void EndPartDrag ( int placementNumber )
    {
        OnPartDragEnded?.Invoke( placementNumber );
    }

    /// <summary>
    /// 배치 파츠 스케일 입력 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="direction">휠 방향</param>
    void ScalePart ( int placementNumber, float direction )
    {
        OnPartScaleInput?.Invoke( placementNumber, direction );
    }

    /// <summary>
    /// 배치 파츠 핀치 스케일 입력 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="scaleRatio">이전 프레임 대비 터치 거리 비율</param>
    void PinchScalePart ( int placementNumber, float scaleRatio )
    {
        OnPartPinchScaleInput?.Invoke( placementNumber, scaleRatio );
    }

    /// <summary>
    /// 배치 파츠 제거 입력 중계
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    void RemovePartInput ( int placementNumber )
    {
        OnPartRemoved?.Invoke( placementNumber );
    }

    /// <summary>
    /// 빈 제작 영역 선택 입력 중계
    /// </summary>
    void SelectEmptyArea ()
    {
        OnEmptyAreaSelected?.Invoke( );
    }

    /// <summary>
    /// 제작 주문 목록 돌아가기 입력 중계
    /// </summary>
    void BackToList ()
    {
        OnBackToList?.Invoke( );
    }

    /// <summary>
    /// 제작 완료 입력 중계
    /// </summary>
    void CompleteCraft ()
    {
        OnCraftDone?.Invoke( );
    }

    /// <summary>
    /// 제작 화면 나가기 입력 중계
    /// </summary>
    void CloseCraft ()
    {
        OnCraftClose?.Invoke( );
    }

    #endregion

    #region ----- 제작 패널 -----
    /// <summary>
    /// 제작 주문 목록 패널 표시
    /// </summary>
    public void ShowCraftList ()
    {
        //이전 제작 실패 안내 초기화
        ClearCraftFailure( );

        //제작 화면 활성화
        gameObject.SetActive( true );
        InitPanelDisplay( );

        //제작 주문 목록만 표시
        _craftListPanel.SetActive( true );
        _craftPanel.SetActive( false );

        _panelTween.Show( );
    }

    /// <summary>
    /// 파츠 제작 패널 표시
    /// </summary>
    public void ShowCraftPanel ()
    {
        //제작 화면 활성화
        gameObject.SetActive( true );
        InitPanelDisplay( );

        //이전 제작 실패 안내 초기화
        ClearCraftFailure( );

        //제작 정보 패널 초기화
        SetCraftInfoOpen( false, false );
        _craftInfoPanelView.ShowOrderDetail( );

        //파츠 제작 패널만 표시
        _craftListPanel.SetActive( false );
        _craftPanel.SetActive( true );

        _panelTween.Show( );
    }

    /// <summary>
    /// 제작 화면 숨김
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    public void HidePanel ( Action onComplete = null )
    {
        //화면을 닫을 때 실패 안내와 종료 예약 초기화
        ClearCraftFailure( );

        _panelTween.Hide( ( ) => CompleteHide( onComplete ) );
    }

    /// <summary>
    /// 제작 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        //즉시 종료할 때 실패 안내와 종료 예약 초기화
        ClearCraftFailure( );

        _panelTween.SetVisible( false );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 제작 화면 퇴장 완료 처리
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CompleteHide ( Action onComplete )
    {
        gameObject.SetActive( false );
        onComplete?.Invoke( );
    }
    #endregion

    #region ----- 사이드 패널 -----
    /// <summary>
    /// 파츠 선택 패널 전환 완료 상태 전달
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    void HandlePartsSelectPanelChanged ( bool isOpen )
    {
        OnPartsSelectPanelChanged?.Invoke( isOpen );
    }

    /// <summary>
    /// 제작 정보 패널 전환 완료 상태 전달
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    void HandleCraftInfoPanelChanged ( bool isOpen )
    {
        OnCraftInfoPanelChanged?.Invoke( isOpen );
    }

    /// <summary>
    /// 사용 파츠 Content 표시 결과 전달
    /// </summary>
    void HandleUsedPartsShown ()
    {
        OnUsedPartsShown?.Invoke( );
    }

    /// <summary>
    /// 제작 화면 최초 표시 시 사이드 패널 초기화
    /// </summary>
    void InitPanelDisplay ()
    {
        if ( _isPanelDisplayInitialized )
            return;

        SetPartsSelectOpen( false, false );
        SetCraftInfoOpen( false, false );

        _isPanelDisplayInitialized = true;
    }

    /// <summary>
    /// 파츠 선택 패널 열기 닫기
    /// </summary>
    void OpenClosePartsSelect ()
    {
        SetPartsSelectOpen( _isPartsSelectOpen == false );
    }

    /// <summary>
    /// 파츠 선택 패널 열림 상태 설정
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    /// <param name="playAnimation">좌우 이동 연출 재생 여부</param>
    void SetPartsSelectOpen (
        bool isOpen, bool playAnimation = true )
    {
        _isPartsSelectOpen = isOpen;
        _partsSelectView.SetPanelOpen(
            isOpen, playAnimation );
    }

    /// <summary>
    /// 제작 정보 패널 열기 닫기
    /// </summary>
    void OpenCloseCraftInfo ()
    {
        SetCraftInfoOpen( _isCraftInfoOpen == false );
    }

    /// <summary>
    /// 제작 정보 패널 열림 상태 설정
    /// </summary>
    /// <param name="isOpen">패널 열림 여부</param>
    /// <param name="playAnimation">펼침 연출 재생 여부</param>
    void SetCraftInfoOpen (
        bool isOpen, bool playAnimation = true )
    {
        _isCraftInfoOpen = isOpen;

        _craftInfoPanelView.SetPanelOpen(
            isOpen, playAnimation );
    }
    #endregion
}
