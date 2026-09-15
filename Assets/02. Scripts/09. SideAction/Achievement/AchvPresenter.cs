using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 목록, 진행도와 보상 수령 중재 
/// </summary>
public class AchvPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] AchvView _achvView;       //업적 뷰

    AchvDataMap _dataMap;       //전체 업적 데이터
    AchvModel _achvModel;       //업적 상태와 달성 모델
    AchvRewardProcessor _rewardProcessor;       //업적 보상 처리기
    DailyRecordModel _dailyRecordModel;       //일일 기록 모델
    PlayStateModel _playStateModel;       //플레이 상태 모델
    SettlementModel _settlementModel;       //결산 기록 모델

    AchvListBuilder _listBuilder;       //업적 목록 생성기
    AchvViewDataBuilder _viewDataBuilder;       //업적 표시 데이터 생성기

    AchvCategory? _selectedCategory;       //현재 선택한 업적 분류

    /// <summary>
    /// 업적 화면 닫기 요청 이벤트
    /// </summary>
    public event Action OnCloseRequested;

    /// <summary>
    /// 업적 알림 상태 변경 이벤트(변경 성공 여부)
    /// </summary>
    public event Action<bool> OnNotificationChanged;

    /// <summary>
    /// 업적 보상 수령 완료 이벤트(업적 아이디)
    /// </summary>
    public event Action<string> OnRewardClaimed;


    #region ----- 초기화/이벤트 -----

    /// <summary>
    /// 업적 프레젠터 초기화
    /// </summary>
    public void Init (
        AchvDataMap dataMap, AchvModel achvModel,
        AchvRewardProcessor rewardProcessor,
        DailyRecordModel dailyRecordModel,
        PlayStateModel playStateModel,
        SettlementModel settlementModel,
        PurchasableDataMap purchasableDataMap )
    {
        _dataMap = dataMap;
        _achvModel = achvModel;
        _rewardProcessor = rewardProcessor;
        _dailyRecordModel = dailyRecordModel;
        _playStateModel = playStateModel;
        _settlementModel = settlementModel;

        _listBuilder = new AchvListBuilder( );
        _viewDataBuilder =
            new AchvViewDataBuilder( purchasableDataMap );

        _achvModel.OnNotificationChanged += ChangeNotification;

        _achvView.HideInstant( );
    }

    /// <summary>
    /// 업적 뷰 입력 연결
    /// </summary>
    void Awake ()
    {
        _achvView.OnAllSelected += SelectAll;
        _achvView.OnCategorySelected += SelectCategory;
        _achvView.OnClaim += Claim;
        _achvView.OnClosed += RequestClose;
    }

    /// <summary>
    /// 업적 입력과 모델 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        _achvView.OnAllSelected -= SelectAll;
        _achvView.OnCategorySelected -= SelectCategory;
        _achvView.OnClaim -= Claim;
        _achvView.OnClosed -= RequestClose;

        if ( _achvModel != null )
            _achvModel.OnNotificationChanged -= ChangeNotification;
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 최신 업적 상태로 전체 목록 표시
    /// </summary>
    public void Show ()
    {
        _selectedCategory = null;

        //신규 상태를 표시한 뒤 확인 상태로 변경
        RefreshList( );
        _achvModel.MarkAllViewed( );
    }

    /// <summary>
    /// 업적 화면 숨김
    /// </summary>
    public void Hide ()
    {
        _achvView.Hide( );
    }

    /// <summary>
    /// 업적 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _achvView.HideInstant( );
    }

    /// <summary>
    /// 현재 업적 화면의 튜토리얼 대상 조회
    /// </summary>
    /// <param name="targetId">조회할 튜토리얼 대상 아이디</param>
    /// <param name="target">조회한 UI 영역</param>
    /// <returns>활성 대상 조회 성공 여부</returns>
    public bool TryGetTutorialTarget (
        TutorialTargetId targetId, out RectTransform target )
    {
        return _achvView.TryGetTutorialTarget(
            targetId, out target );
    }

    /// <summary>
    /// 현재 분류의 업적 목록 갱신
    /// </summary>
    void RefreshList ()
    {
        //업적 목록 생성
        IReadOnlyList<AchvData> datas =
            _listBuilder.Build( _dataMap.Datas, _selectedCategory );

        //미수령 보상이 있는 업적을 항상 최상단에 배치
        datas = BuildClaimableFirstOrder( datas );

        //원본 생성
        AchvEvalContext context = CreateCurrentContext( );

        //뷰 데이터 목록 생성
        var viewDatas = new List<AchvViewData>( datas.Count );

        for ( int i = 0; i < datas.Count; i++ )
        {
            //데이터 및 상태 가져오기
            AchvData data = datas [ i ];
            AchvState state = _achvModel.States [ data.Id ];

            //상태 가져오기
            int progress = _achvModel.GetProgress( data, context );

            //데이터 추가
            viewDatas.Add( _viewDataBuilder.Build( data, state, progress ) );
        }

        _achvView.ShowList( viewDatas );
    }

    /// <summary>
    /// 미수령 보상이 있는 업적을 기존 순서를 유지하며 최상단에 배치
    /// </summary>
    /// <param name="datas">카테고리 필터가 적용된 업적 목록</param>
    /// <returns>미수령 보상 우선 업적 목록</returns>
    IReadOnlyList<AchvData> BuildClaimableFirstOrder (
        IReadOnlyList<AchvData> datas )
    {
        var ordered = new List<AchvData>( datas.Count );

        //미수령 보상이 있는 업적을 먼저 추가
        for ( int i = 0; i < datas.Count; i++ )
        {
            AchvData data = datas [ i ];

            if ( _achvModel.States [ data.Id ].HasUnclaimedReward )
                ordered.Add( data );
        }

        //나머지 업적은 기존 순서대로 추가
        for ( int i = 0; i < datas.Count; i++ )
        {
            AchvData data = datas [ i ];

            if ( _achvModel.States [ data.Id ].HasUnclaimedReward == false )
                ordered.Add( data );
        }

        return ordered;
    }

    /// <summary>
    /// 현재 업적 진행도 판정 원본 생성
    /// </summary>
    /// <returns>현재 업적 진행도 판정 원본</returns>
    AchvEvalContext CreateCurrentContext ()
    {
        //업적 판정 데이터 생성
        return new AchvEvalContext(
            _dailyRecordModel.Records,
            _dailyRecordModel.CurrentRecord,
            _playStateModel.TotalDay - 1,
            _settlementModel.WeeklySettlements.Count,
            false );
    }

    #endregion

    #region ----- 분류 선택 -----

    /// <summary>
    /// 전체 업적 선택
    /// </summary>
    void SelectAll ()
    {
        _selectedCategory = null;
        RefreshList( );
    }

    /// <summary>
    /// 지정 업적 분류 선택
    /// </summary>
    /// <param name="category">선택한 업적 분류</param>
    void SelectCategory ( AchvCategory category )
    {
        _selectedCategory = category;
        RefreshList( );
    }

    #endregion

    #region ----- 보상 수령 -----

    /// <summary>
    /// 지정 업적의 다음 미수령 단계 보상 수령
    /// </summary>
    /// <param name="id">보상을 수령할 업적 아이디</param>
    void Claim ( string id )
    {
        //업적 결과 가져오기
        AchvRewardResult result = _rewardProcessor.Claim( id );

        //결과가 성공이라면
        if ( result == AchvRewardResult.Success )
        {
            //가이드에 수령 완료 전달
            OnRewardClaimed?.Invoke( id );
            RefreshList( );
            return;
        }

        Debug.LogWarning( $"업적 보상 수령 실패: {result}" );
    }

    #endregion

    #region ----- 알림/닫기 -----

    /// <summary>
    /// 업적 알림 상태 변경 전달
    /// </summary>
    /// <param name="hasNotification">알림 존재 여부</param>
    void ChangeNotification ( bool hasNotification )
    {
        OnNotificationChanged?.Invoke( hasNotification );
    }

    /// <summary>
    /// 업적 화면 닫기 요청 전달
    /// </summary>
    void RequestClose ()
    {
        OnCloseRequested?.Invoke( );
    }

    #endregion
}
