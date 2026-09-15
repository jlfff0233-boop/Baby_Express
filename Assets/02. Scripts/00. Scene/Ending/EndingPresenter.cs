using System;
using UnityEngine;

/// <summary>
/// 엔딩 프레젠터 - 총 결산 표시와 엔딩 이후 이동 요청 중재
/// </summary>
public class EndingPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] EndingView _endingView;       //엔딩 뷰

    PlayStateModel _playStateModel;       //공용 플레이 상태
    DailyRecordModel _dailyRecordModel;       //완료된 일일 기록
    SettlementModel _settlementModel;       //기존 결산 계산 모델
    EmployeeModel _employeeModel;       //직원 상태
    AchvModel _achvModel;       //업적 상태
    AchvDataMap _achvDataMap;       //전체 업적 데이터 맵

    EndingViewDataBuilder _viewDataBuilder =
        new EndingViewDataBuilder( );       //엔딩 총 결산 표시 데이터 생성기

    bool _isShowing;       //엔딩 화면 표시 여부


    /// <summary>
    /// 처음부터 다시 시작 요청 이벤트
    /// </summary>
    public event Action OnRestartRequested;

    /// <summary>
    /// 시작 화면 이동 요청 이벤트
    /// </summary>
    public event Action OnTitleRequested;

    /// <summary>
    /// 엔딩 이후 계속하기 요청 이벤트
    /// </summary>
    public event Action OnContinueRequested;

    #region ----- 초기화/이벤트 -----

    /// <summary>
    /// 엔딩 화면 입력 연결
    /// </summary>
    void Awake ()
    {
        _endingView.OnContinueSelected += Continue;
        _endingView.OnRestartSelected += Restart;
        _endingView.OnTitleSelected += MoveToTitle;
    }

    /// <summary>
    /// 엔딩 화면 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _endingView.OnContinueSelected -= Continue;
        _endingView.OnRestartSelected -= Restart;
        _endingView.OnTitleSelected -= MoveToTitle;
    }

    /// <summary>
    /// 엔딩 프레젠터 초기화
    /// </summary>
    /// <param name="playStateModel">공용 플레이 상태</param>
    /// <param name="dailyRecordModel">완료된 일일 기록 모델</param>
    /// <param name="settlementModel">기존 결산 계산 모델</param>
    /// <param name="employeeModel">직원 상태 모델</param>
    /// <param name="achvModel">업적 상태 모델</param>
    /// <param name="achvDataMap">전체 업적 데이터 맵</param>
    public void Init (
        PlayStateModel playStateModel,
        DailyRecordModel dailyRecordModel,
        SettlementModel settlementModel,
        EmployeeModel employeeModel,
        AchvModel achvModel,
        AchvDataMap achvDataMap )
    {
        _playStateModel = playStateModel;
        _dailyRecordModel = dailyRecordModel;
        _settlementModel = settlementModel;
        _employeeModel = employeeModel;
        _achvModel = achvModel;
        _achvDataMap = achvDataMap;

        _isShowing = false;
        _endingView.HideInstant( );
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 엔딩 총 결산 화면 표시
    /// </summary>
    public void ShowEnding ()
    {
        //엔딩 화면 중복 표시 차단
        if ( _isShowing == true ) return;

        EndingViewData viewData =
            _viewDataBuilder.Build(
                _settlementModel,
                _dailyRecordModel.Records,
                _playStateModel.TotalDay,
                _playStateModel.Budget,
                _employeeModel.TotalHireCount,
                _achvModel.CompletedCount,
                _achvDataMap.Datas.Count );

        _isShowing = true;
        _endingView.Show( viewData );
    }

    /// <summary>
    /// 엔딩 화면 숨김
    /// </summary>
    public void HideEnding ()
    {
        CloseEnding ( );
    }

    /// <summary>
    /// 엔딩 화면 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _isShowing = false;
        _endingView.HideInstant ( );
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 엔딩 이후 계속하기 요청
    /// </summary>
    void Continue ()
    {
        CloseEnding ( ( ) => OnContinueRequested?.Invoke ( ) );
    }

    /// <summary>
    /// 처음부터 다시 시작 요청
    /// </summary>
    void Restart ()
    {
        CloseEnding ( ( ) => OnRestartRequested?.Invoke ( ) );
    }

    /// <summary>
    /// 시작 화면 이동 요청
    /// </summary>
    void MoveToTitle ()
    {
        CloseEnding ( ( ) => OnTitleRequested?.Invoke ( ) );
    }

    /// <summary>
    /// 엔딩 화면을 닫고 후속 처리 실행
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CloseEnding ( Action onComplete = null )
    {
        //중복 입력 차단
        if ( _isShowing == false )
            return;

        _isShowing = false;
        _endingView.Hide ( onComplete );
    }

    #endregion
}
