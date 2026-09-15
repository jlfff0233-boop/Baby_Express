using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 주문 조건 종류
/// </summary>
public enum TutorialOrderConditionType
{
    Requirement,       //주요 요구
    Wish,       //희망 사항
}

/// <summary>
/// 튜토리얼 주문 파츠 조건 데이터
/// </summary>
[Serializable]
public class TutorialOrderConditionData
{
    [SerializeField] TutorialOrderConditionType _conditionType;       //조건 종류
    [SerializeField] string _partId;       //조건 파츠 아이디
    [SerializeField, Min( 1 )] int _quantity = 1;       //필요 수량

    /// <summary>
    /// 조건 종류
    /// </summary>
    public TutorialOrderConditionType ConditionType =>
        _conditionType;

    /// <summary>
    /// 조건 파츠 아이디
    /// </summary>
    public string PartId => _partId;

    /// <summary>
    /// 필요 수량
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// 런타임 주문 조건 생성
    /// </summary>
    /// <returns>주문 파츠 조건</returns>
    public OrderPartCondition CreateCondition ()
    {
        return new OrderPartCondition( _partId, _quantity );
    }
}

/// <summary>
/// 튜토리얼 구매와 제작 가이드 데이터
/// </summary>
[Serializable]
public class TutorialPurchaseGuideData
{
    [SerializeField, Min( 0 )] int _sequence;       //가이드 진행 순서
    [SerializeField] string _partId;       //대상 파츠 아이디
    [SerializeField] bool _requiredForProgress;       //진행에 반드시 필요한 파츠 여부
    [SerializeField, TextArea] string _note;       //데이터 설명

    /// <summary>
    /// 가이드 진행 순서
    /// </summary>
    public int Sequence => _sequence;

    /// <summary>
    /// 대상 파츠 아이디
    /// </summary>
    public string PartId => _partId;

    /// <summary>
    /// 진행에 반드시 필요한 파츠 여부
    /// </summary>
    public bool RequiredForProgress =>
        _requiredForProgress;

    /// <summary>
    /// 데이터 설명
    /// </summary>
    public string Note => _note;
}

/// <summary>
/// Day 1 튜토리얼 설정 데이터 - 고정 주문과 구매 및 대화 데이터 관리
/// </summary>
[CreateAssetMenu(
    fileName = "TutorialDay1Data",
    menuName = "TutorialSettings/TutorialDay1Data" )]
public class TutorialDay1Data : ScriptableObject
{
    [Header( "----- 고정 주문 -----" )]
    [SerializeField] string _orderId;       //튜토리얼 주문 아이디
    [SerializeField] string _title;       //주문 제목
    [SerializeField] OrderDifficulty _difficulty;       //주문 난이도
    [SerializeField, Min( 0 )] int _acceptDays;       //수락 가능 기간
    [SerializeField, Min( 0 )] int _deliveryDays;       //수락 이후 납품 기간
    [SerializeField, Min( 0 )] int _delayDays;       //지연 허용 기간
    [SerializeField, Min( 0 )] int _maxCraftCost;       //최대 제작 코스트
    [SerializeField] OrderSpecialType _specialType;       //특수 주문 종류
    [SerializeField, Min( 0 )] int _actualTravelDays;       //튜토리얼 실제 배송 소요일
    [SerializeField, Min( 1 )] int _craftLimit = 1;       //Day 1 제작 할당량

    [Header( "----- 주문 조건 -----" )]
    [SerializeField]
    TutorialOrderConditionData [ ] _conditions =
        Array.Empty<TutorialOrderConditionData>( );       //주요 요구와 희망 사항

    [Header( "----- 구매와 제작 가이드 -----" )]
    [SerializeField]
    TutorialPurchaseGuideData [ ] _purchaseGuides =
        Array.Empty<TutorialPurchaseGuideData>( );       //순서별 대상 파츠

    [Header( "----- 대화 -----" )]
    [SerializeField]
    DialogueData [ ] _dialogues =
        Array.Empty<DialogueData>( );       //Day 1 진행 대화 목록

    /// <summary>
    /// 튜토리얼 주문 아이디
    /// </summary>
    public string OrderId => _orderId;

    /// <summary>
    /// 튜토리얼 실제 배송 소요일
    /// </summary>
    public int ActualTravelDays => _actualTravelDays;

    /// <summary>
    /// Day 1 제작 할당량
    /// </summary>
    public int CraftLimit => _craftLimit;

    /// <summary>
    /// 튜토리얼 주문 조건 목록
    /// </summary>
    public IReadOnlyList<TutorialOrderConditionData> Conditions => _conditions;

    /// <summary>
    /// 구매와 제작 가이드 목록
    /// </summary>
    public IReadOnlyList<TutorialPurchaseGuideData> PurchaseGuides => _purchaseGuides;

    /// <summary>
    /// Day 1 대화 목록
    /// </summary>
    public IReadOnlyList<DialogueData> Dialogues => _dialogues;

    /// <summary>
    /// 고정 튜토리얼 주문 생성 데이터 구성
    /// </summary>
    /// <param name="createdNumber">누적 주문 생성 번호</param>
    /// <param name="createdTotalDay">주문 생성 누적 영업일</param>
    /// <returns>런타임 주문 생성 데이터</returns>
    public OrderCreateData CreateOrderData (
        int createdNumber, int createdTotalDay )
    {
        var requirements = new List<OrderPartCondition>( );
        var wishes = new List<OrderPartCondition>( );

        //CSV 조건을 주요 요구와 희망 사항으로 분리
        for ( int i = 0; i < _conditions.Length; i++ )
        {
            TutorialOrderConditionData condition = _conditions [ i ];

            if ( condition.ConditionType ==
                TutorialOrderConditionType.Requirement )
            {
                requirements.Add( condition.CreateCondition( ) );
                continue;
            }

            wishes.Add( condition.CreateCondition( ) );
        }

        int acceptDue = createdTotalDay + _acceptDays;
        int deliveryDue = acceptDue + _deliveryDays;

        return new OrderCreateData
        {
            CreatedNumber = createdNumber,
            Id = _orderId,
            Title = _title,
            Difficulty = _difficulty,
            MaxCraftCost = _maxCraftCost,

            Requirements = requirements,
            Wishes = wishes,

            SpecialType = _specialType,
            MaxPartCount = 0,
            TargetThemes = Array.Empty<PartTheme>( ),
            ExcludedThemes = Array.Empty<PartTheme>( ),

            CreatedTotalDay = createdTotalDay,
            AcceptDue = acceptDue,
            DeliveryDue = deliveryDue,
            MaxDelay = _delayDays,
        };
    }

    /// <summary>
    /// 지정 순서의 구매와 제작 가이드 조회
    /// </summary>
    /// <param name="sequence">조회할 가이드 순서</param>
    /// <param name="guide">조회한 가이드 데이터</param>
    /// <returns>가이드 존재 여부</returns>
    public bool TryGetPurchaseGuide (
        int sequence, out TutorialPurchaseGuideData guide )
    {
        for ( int i = 0; i < _purchaseGuides.Length; i++ )
        {
            if ( _purchaseGuides [ i ].Sequence != sequence )
                continue;

            guide = _purchaseGuides [ i ];
            return true;
        }

        guide = null;
        return false;
    }

    /// <summary>
    /// 대화 아이디로 Day 1 대화 데이터 조회
    /// </summary>
    /// <param name="dialogueId">조회할 대화 아이디</param>
    /// <param name="dialogue">조회한 대화 데이터</param>
    /// <returns>대화 존재 여부</returns>
    public bool TryGetDialogue (
        string dialogueId, out DialogueData dialogue )
    {
        for ( int i = 0; i < _dialogues.Length; i++ )
        {
            DialogueData currentDialogue = _dialogues [ i ];

            if ( currentDialogue != null &&
                currentDialogue.Id == dialogueId )
            {
                dialogue = currentDialogue;
                return true;
            }
        }

        dialogue = null;
        return false;
    }
}
