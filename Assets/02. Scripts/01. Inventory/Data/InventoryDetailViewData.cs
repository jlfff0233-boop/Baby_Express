using UnityEngine;

/// <summary>
/// 인벤토리 상세 패널 표시 데이터
/// </summary>
public class InventoryDetailViewData
{
    public string SlotId { get; }             //선택한 슬롯 아이디
    public string ItemId { get; }             //아이템 아이디
    public Sprite Icon { get; }               //아이템 아이콘
    public string Name { get; }               //아이템 이름
    public string Desc { get; }        //아이템 설명
    public int SelectedStackQuantity { get; }         //선택한 스택 수량
    public bool CanOpenShop { get; }          //상점 이동 가능 여부
    public bool CanUse { get; }               //사용 가능 여부
    public bool CanCraft { get; }             //제작 가능 여부
    public bool CanSell { get; }              //판매 가능 여부
    public bool CanDelete { get; }            //삭제 가능 여부

    /// <summary>
    /// 인벤토리 상세 패널 표시 데이터 생성
    /// </summary>
    /// <param name="slotId">선택한 슬롯 아이디</param>
    /// <param name="itemId">아이템 아이디</param>
    /// <param name="icon">아이콘</param>
    /// <param name="name">이름</param>
    /// <param name="desc">설명</param>
    /// <param name="stackQuantity">선택한 스택 수량</param>
    /// <param name="canOpenShop">상점 이동 가능 여부</param>
    /// <param name="canUse">사용 가능 여부</param>
    /// <param name="canCraft">제작 가능 여부</param>
    /// <param name="canSell">판매 가능 여부</param>
    /// <param name="canDelete">삭제 가능 여부</param>
    public InventoryDetailViewData (
        string slotId, string itemId, Sprite icon, string name, string desc,
        int stackQuantity, bool canOpenShop, bool canUse, bool canCraft,
        bool canSell, bool canDelete )
    {
        SlotId = slotId;
        ItemId = itemId;
        Icon = icon;
        Name = name;
        Desc = desc;
        SelectedStackQuantity = stackQuantity;
        CanOpenShop = canOpenShop;
        CanUse = canUse;
        CanCraft = canCraft;
        CanSell = canSell;
        CanDelete = canDelete;
    }
}
