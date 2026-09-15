using System;
using UnityEngine;

/// <summary>
/// 파산 프레젠터 - 파산 화면과 다시 시작 입력 중재
/// </summary>
public class BankruptcyPresenter : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] BankruptcyView _bankruptcyView;       //파산 뷰

    bool _isShowing;       //파산 화면 표시 여부
    /// <summary>
    /// 이번 주부터 다시 시작 가능 여부 확인
    /// </summary>
    Func<bool> _canRestartCurrentWeek;

    /// <summary>
    /// 처음부터 다시 시작 요청 이벤트
    /// </summary>
    public event Action OnRestartRequested;

    /// <summary>
    /// 이번 주부터 다시 시작 요청 이벤트
    /// </summary>
    public event Action OnRestartCurrentWeekRequested;

    /// <summary>
    /// 시작 화면 이동 요청 이벤트
    /// </summary>
    public event Action OnTitleRequested;


    /// <summary>
    /// 파산 화면 입력 연결
    /// </summary>
    void Awake ()
    {
        //처음부터 다시 시작 이벤트 연결
        _bankruptcyView.OnRestartSelected += Restart;
        //이번 주부터 다시 시작 이벤트 연결
        _bankruptcyView.OnRestartCurrentWeekSelected += RestartCurrentWeek;
        //타이틀로 이동 이벤트 연결
        _bankruptcyView.OnTitleSelected += MoveToTitle;
    }

    /// <summary>
    /// 파산 화면 입력 해제
    /// </summary>
    void OnDestroy ()
    {
        _bankruptcyView.OnRestartSelected -= Restart;
        _bankruptcyView.OnRestartCurrentWeekSelected -= RestartCurrentWeek;
        _bankruptcyView.OnTitleSelected -= MoveToTitle;
    }

    /// <summary>
    /// 파산 프레젠터 초기화
    /// </summary>
    public void Init ()
    {
        _isShowing = false;
        _bankruptcyView.HideInstant( );
    }

    /// <summary>
    /// 이번 주부터 다시 시작 가능 여부 확인
    /// </summary>
    /// <param name="canRestartCurrentWeek">주간 시작 상태 복구 가능 여부 확인 함수</param>
    public void SetWeekRestartCheck (
        Func<bool> canRestartCurrentWeek )
    {
        _canRestartCurrentWeek = canRestartCurrentWeek;
    }

    /// <summary>
    /// 파산 화면 표시
    /// </summary>
    public void ShowBankruptcy ()
    {
        //파산 화면 중복 표시 차단
        if ( _isShowing == true ) return;

        _isShowing = true;

        bool canRestartCurrentWeek = _canRestartCurrentWeek( );

        _bankruptcyView.Show(
            "파산했습니다.",
            "현재 자금과 보유 자원으로\n진행 주문을 제작할 수 없습니다.",
            canRestartCurrentWeek );
    }

    /// <summary>
    /// 파산 화면 숨김
    /// </summary>
    public void HideBankruptcy ()
    {
        CloseBankruptcy( );
    }

    /// <summary>
    /// 파산 화면 즉시 숨김
    /// </summary>
    public void HideInstant ()
    {
        _isShowing = false;
        _bankruptcyView.HideInstant( );
    }

    /// <summary>
    /// 처음부터 다시 시작 요청
    /// </summary>
    void Restart ()
    {
        CloseBankruptcy( () => OnRestartRequested?.Invoke( ) );
    }

    /// <summary>
    /// 이번 주부터 다시 시작 요청
    /// </summary>
    void RestartCurrentWeek ()
    {
        CloseBankruptcy(
            () => OnRestartCurrentWeekRequested?.Invoke( ) );
    }

    /// <summary>
    /// 시작 화면 이동 요청
    /// </summary>
    void MoveToTitle ()
    {
        CloseBankruptcy( () => OnTitleRequested?.Invoke( ) );
    }

    /// <summary>
    /// 파산 화면을 닫고 후속 처리 실행
    /// </summary>
    /// <param name="onComplete">퇴장 완료 후 실행할 함수</param>
    void CloseBankruptcy ( Action onComplete = null )
    {
        //중복 입력 차단
        if ( _isShowing == false )
            return;

        _isShowing = false;
        _bankruptcyView.Hide( onComplete );
    }
}
