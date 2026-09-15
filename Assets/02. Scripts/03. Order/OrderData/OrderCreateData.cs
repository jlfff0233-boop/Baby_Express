using System.Collections.Generic;

/// <summary>
/// 주문 생성 데이터 - 새 주문 생성에 필요한 값 전달
/// </summary>
public class OrderCreateData
{
    public int CreatedNumber { get; set; }      //누적 생성 번호
    public string Id { get; set; }      //주문 아이디
    public string Title { get; set; }       //주문 제목
    public int MaxCraftCost { get; set; }        //최대 제작 코스트

    public IReadOnlyList<OrderPartCondition> Requirements { get; set; }       //주요 요구 사항
    public IReadOnlyList<OrderPartCondition> Wishes { get; set; }       //희망 사항

    public OrderSpecialType SpecialType { get; set; }       //특수 주문 종류
    public int MaxPartCount { get; set; }       //최대 전체 파츠 개수, 0이면 제한 없음
    public IReadOnlyList<PartTheme> TargetThemes { get; set; }       //목표 테마 목록
    public IReadOnlyList<PartTheme> ExcludedThemes { get; set; }       //제외 테마 목록

    public int CreatedTotalDay { get; set; }     //주문 생성 누적 영업일
    public int AcceptDue { get; set; }       //수락 마감일
    public int DeliveryDue { get; set; }     //납품 마감일
    public int MaxDelay { get; set; }        //지연 허용 일수

    public OrderDifficulty Difficulty { get; set; }      //주문 난이도
}
