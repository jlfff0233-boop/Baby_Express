using System.Collections.Generic;
using System.Text;

/// <summary>
/// 제작 파츠 표시 데이터 빌더
/// </summary>
public class CraftPartsViewDataBuilder
{
    /// <summary>
    /// 파츠 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="partData">파츠 데이터</param>
    /// <param name="quantity">표시 수량</param>
    /// <returns>파츠 슬롯 표시 데이터</returns>
    public ItemSlotViewData CreatePartSlot (
        PartsData partData, int quantity )
    {
        //파츠 아이디를 슬롯 아이디로 함께 사용
        return new ItemSlotViewData(
            partData.Id, partData.Id,
            partData.Icon, partData.Name, quantity,
            $"테마: {GetThemeText( partData.Themes )}\n제작 코스트: {partData.CraftCost}" );
    }

    /// <summary>
    /// 사용 파츠 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="placedParts">현재 배치 파츠 목록</param>
    /// <param name="craftModel">제작 모델</param>
    /// <returns>사용 파츠 슬롯 표시 데이터 목록</returns>
    public IReadOnlyList<ItemSlotViewData> CreateUsedParts (
        IReadOnlyList<PlacedPartData> placedParts, CraftModel craftModel )
    {
        //같은 파츠는 수량을 합쳐 하나의 슬롯으로 표시
        var viewDatas = new List<ItemSlotViewData>( );
        var addedPartIds = new HashSet<string>( );

        for ( int i = 0; i < placedParts.Count; i++ )
        {
            //배치한 파츠 데이터 가져오기
            PartsData partData = placedParts [ i ].PartData;

            //이미 표시한 파츠 아이디면 다음 파츠 확인
            if ( addedPartIds.Add( partData.Id ) == false ) continue;

            //현재 파츠 수량을 슬롯 표시 데이터로 변환
            int quantity = craftModel.GetPartQuantity( partData.Id );
            viewDatas.Add( CreatePartSlot( partData, quantity ) );
        }

        return viewDatas;
    }

    /// <summary>
    /// 파츠 테마 표시 문자열 생성
    /// </summary>
    /// <param name="themes">파츠 테마 목록</param>
    /// <returns>파츠 테마 표시 문자열</returns>
    string GetThemeText ( IReadOnlyList<PartTheme> themes )
    {
        var text = new StringBuilder( );

        for ( int i = 0; i < themes.Count; i++ )
        {
            //테마 없음은 표시에서 제외
            if ( themes [ i ] == PartTheme.None ) continue;

            //두 번째 테마부터 구분자 추가
            if ( text.Length > 0 ) text.Append( " / " );

            text.Append( themes [ i ].GetDisplayName( ) );
        }

        //표시할 테마가 없으면 없음 반환
        return text.Length > 0 ? text.ToString( ) : "없음";
    }
}
