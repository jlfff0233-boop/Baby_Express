using System;
using System.Collections.Generic;

/// <summary>
/// Day 1 핵심 튜토리얼 진행 단계
/// </summary>
public enum CoreTutorialStep
{
    OrderDetail,       //주문 상세 확인
    OrderAccept,       //주문 수락
    ShopPurchase,       //필수 파츠 구매
    InventoryCheck,       //구매한 파츠 확인
    Craft,       //아기 제작
    CraftComplete,       //제작 확정과 평가
    Delivery,       //직접 배송
    Settlement,       //첫 일일 결산
    Completed,       //핵심 튜토리얼 완료
}

/// <summary>
/// Day 2 이후 개별 가이드
/// </summary>
public enum TutorialGuideId
{
    NormalOrder,       //일반 주문 안내
    ShopStock,       //상점 재고 안내
    OutOfStock,       //품절 안내
    QuickRestock,       //빠른 재입고 안내
    Maintenance,       //정비 안내
    Catalog,       //카탈로그 안내
    Achievement,       //업적 안내
    SaveAndSettings,       //저장과 설정 안내
    WeeklySettlement,       //첫 주간 결산 안내
    Ledger,       //가계부 안내
    Employee,       //직원 안내
}

/// <summary>
/// 튜토리얼 모델 - 인트로와 핵심 튜토리얼 및 후속 가이드 진행 상태 관리
/// </summary>
public class TutorialModel
{
    bool _introCompleted;       //인트로 완료 여부
    CoreTutorialStep _currentCoreStep;       //현재 핵심 튜토리얼 단계

    HashSet<TutorialGuideId> _shownGuides =
        new HashSet<TutorialGuideId>( );       //표시를 완료한 후속 가이드

    /// <summary>
    /// 튜토리얼 진행 상태 변경 이벤트
    /// </summary>
    public event Action OnProgressChanged;

    /// <summary>
    /// 인트로 완료 여부
    /// </summary>
    public bool IntroCompleted => _introCompleted;

    /// <summary>
    /// 현재 핵심 튜토리얼 단계
    /// </summary>
    public CoreTutorialStep CurrentCoreStep =>
        _currentCoreStep;

    /// <summary>
    /// Day 1 핵심 튜토리얼 완료 여부
    /// </summary>
    public bool CoreTutorialCompleted =>
        _currentCoreStep == CoreTutorialStep.Completed;

    /// <summary>
    /// 전체 후속 가이드 완료 여부
    /// </summary>
    public bool AllGuidesCompleted =>
        HasShownGuide( TutorialGuideId.Ledger ) &&
        HasShownGuide( TutorialGuideId.Employee );

    /// <summary>
    /// 신규 튜토리얼 상태 생성
    /// </summary>
    public TutorialModel ()
    {
        //새 게임은 주문 상세 확인 단계부터 시작
        _currentCoreStep = CoreTutorialStep.OrderDetail;
    }

    #region ----- 진행 상태 -----

    /// <summary>
    /// 인트로 완료 처리
    /// </summary>
    /// <returns>상태 변경 여부</returns>
    public bool CompleteIntro ()
    {
        //이미 완료한 인트로의 중복 완료 처리 차단
        if ( _introCompleted )
            return false;

        //완료 상태를 먼저 반영한 뒤 저장 요청 전달
        _introCompleted = true;
        OnProgressChanged?.Invoke( );
        return true;
    }

    /// <summary>
    /// 현재 핵심 튜토리얼 단계를 완료하고 다음 단계로 진행
    /// </summary>
    /// <param name="completedStep">완료한 것으로 확인한 단계</param>
    /// <returns>단계 변경 여부</returns>
    public bool CompleteCoreStep (
        CoreTutorialStep completedStep )
    {
        //현재 단계와 다른 행동으로 진행 상태가 변경되는 것을 차단
        if ( _currentCoreStep != completedStep ||
            CoreTutorialCompleted )
        {
            return false;
        }

        //다음 핵심 단계로 이동
        _currentCoreStep++;

        //변경된 단계를 현재 저장 슬롯에 반영하도록 알림
        OnProgressChanged?.Invoke( );
        return true;
    }

    /// <summary>
    /// Day 1 핵심 튜토리얼 전체 완료 처리
    /// </summary>
    /// <returns>완료 상태 변경 여부</returns>
    public bool SkipCoreTutorial ()
    {
        if ( CoreTutorialCompleted == true ) return false;

        _currentCoreStep = CoreTutorialStep.Completed;
        OnProgressChanged?.Invoke( );
        return true;
    }

    /// <summary>
    /// 지정된 가이드 표시 완료 처리
    /// </summary>
    /// <param name="guideId">표시를 완료한 가이드</param>
    /// <returns>상태 변경 여부</returns>
    public bool MarkGuideShown ( TutorialGuideId guideId )
    {
        //이미 표시한 가이드는 저장 상태와 이벤트를 다시 변경하지 않음
        if ( _shownGuides.Add( guideId ) == false )
            return false;

        //새로 완료한 가이드 상태 저장 요청 전달
        OnProgressChanged?.Invoke( );
        return true;
    }

    /// <summary>
    /// 지정된 가이드 표시 완료 여부 확인
    /// </summary>
    /// <param name="guideId">확인할 가이드</param>
    /// <returns>가이드 표시 완료 여부</returns>
    public bool HasShownGuide ( TutorialGuideId guideId )
    {
        return _shownGuides.Contains( guideId );
    }

    #endregion

    #region ----- 저장과 복구 -----

    /// <summary>
    /// 현재 튜토리얼 상태를 저장 데이터로 변환
    /// </summary>
    /// <returns>튜토리얼 저장 데이터</returns>
    public TutorialSaveData CreateSaveData ()
    {
        //외부에서 런타임 컬렉션을 변경하지 못하도록 저장 데이터로 복사
        return new TutorialSaveData(
            _introCompleted, _currentCoreStep, _shownGuides );
    }

    /// <summary>
    /// 튜토리얼 저장 데이터 복구 가능 여부 확인
    /// </summary>
    /// <param name="saveData">복구할 튜토리얼 저장 데이터</param>
    /// <returns>복구 가능 여부</returns>
    public bool CanRestore ( TutorialSaveData saveData )
    {
        //튜토리얼 데이터가 없는 이전 저장 파일은 별도 기본값으로 복구
        if ( saveData == null )
            return true;

        if ( Enum.IsDefined(
            typeof( CoreTutorialStep ),
            saveData.CurrentCoreStep ) == false ||
            saveData.ShownGuides == null )
        {
            return false;
        }

        //정의되지 않은 가이드와 중복 저장 상태 검사
        var guideIds = new HashSet<TutorialGuideId>( );

        for ( int i = 0; i < saveData.ShownGuides.Count; i++ )
        {
            //가이드 데이터 가져오기
            TutorialGuideId guideId = saveData.ShownGuides [ i ];

            if ( Enum.IsDefined( typeof( TutorialGuideId ),
                guideId ) == false ||
                guideIds.Add( guideId ) == false )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 저장 데이터로 튜토리얼 상태 복구
    /// </summary>
    /// <param name="saveData">복구할 튜토리얼 저장 데이터</param>
    /// <returns>복구 성공 여부</returns>
    public bool Restore ( TutorialSaveData saveData )
    {
        if ( CanRestore( saveData ) == false )
            return false;

        //기존 저장에서는 진행 중인 플레이에 튜토리얼이 갑자기 시작되지 않게 완료 처리
        if ( saveData == null )
        {
            RestoreLegacySave( );
            return true;
        }

        _introCompleted = saveData.IntroCompleted;
        _currentCoreStep = saveData.CurrentCoreStep;

        //현재 상태를 비운 뒤 검증을 마친 가이드만 복구
        _shownGuides.Clear( );

        for ( int i = 0; i < saveData.ShownGuides.Count; i++ )
        {
            _shownGuides.Add( saveData.ShownGuides [ i ] );
        }

        return true;
    }

    /// <summary>
    /// 튜토리얼 데이터가 없는 이전 저장 파일 기본 상태 복구
    /// </summary>
    void RestoreLegacySave ()
    {
        //기존 저장 슬롯은 인트로와 Day 1 튜토리얼 완료로 취급
        _introCompleted = true;
        _currentCoreStep = CoreTutorialStep.Completed;

        //기존 플레이 중 가이드가 새로 나타나지 않게 전체 완료 처리
        _shownGuides.Clear( );

        Array guideIds =
            Enum.GetValues( typeof( TutorialGuideId ) );

        foreach ( TutorialGuideId guideId in guideIds )
            _shownGuides.Add( guideId );
    }

    #endregion
}
