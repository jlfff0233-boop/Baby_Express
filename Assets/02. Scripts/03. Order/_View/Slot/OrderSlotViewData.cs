using UnityEngine;

/// <summary>
/// 주문 슬롯 뷰 데이터
/// </summary>
public class OrderSlotViewData
{
    public string OrderId { get; }       //주문 아이디
    public string OrderNumber { get; }     //주문 번호
    public int CreatedMonth { get; }       //생성 월
    public int CreatedDay { get; }     //생성 일
    public string DeadlineText { get; }        //상태별 기한 문구
    public Sprite IconSprite { get; }      //아이콘
    public bool IsSpecial { get; }       //특수 주문 여부
    public bool IsDeadlineImminent { get; }       //기한 임박 여부


    /// <summary>
    /// 주문 슬롯 뷰 데이터 생성자
    /// </summary>
    /// <param name="id">주문 아이디</param>
    /// <param name="orderNumber">누적 주문 번호</param>
    /// <param name="createdMonth">생성 월</param>
    /// <param name="createdDay">생성 일</param>
    /// <param name="deadlineText">상태별 기한 문구</param>
    /// <param name="icon">아이콘</param>
    /// <param name="isSpecial">특수 주문 여부</param>
    /// <param name="isDeadlineImminent">기한 임박 여부</param>
    public OrderSlotViewData ( string id , string orderNumber , int createdMonth , int createdDay ,
        string deadlineText , Sprite icon , bool isSpecial , bool isDeadlineImminent = false )
    {
        OrderId = id;
        OrderNumber = orderNumber;
        CreatedMonth = createdMonth;
        CreatedDay = createdDay;

        DeadlineText = deadlineText;
        IconSprite = icon;
        IsSpecial = isSpecial;
        IsDeadlineImminent = isDeadlineImminent;
    }
}
