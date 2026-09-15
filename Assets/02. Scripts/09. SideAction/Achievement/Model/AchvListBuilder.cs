using System.Collections.Generic;

/// <summary>
/// 업적 분류별 목록 생성기
/// </summary>
public class AchvListBuilder
{
    /// <summary>
    /// 선택한 분류에 맞는 업적 목록 생성
    /// </summary>
    /// <param name="datas">전체 업적 데이터</param>
    /// <param name="category">선택한 업적 분류, null이면 전체</param>
    /// <returns>분류 조건에 맞는 업적 목록</returns>
    public IReadOnlyList<AchvData> Build (
        IReadOnlyList<AchvData> datas, AchvCategory? category )
    {
        if ( category.HasValue == false )
            return datas;

        var filteredDatas = new List<AchvData>( );

        for ( int i = 0; i < datas.Count; i++ )
        {
            //업적 데이터 가져오기
            AchvData data = datas [ i ];

            //데이터 카테고리와 카테고리 값이 같으면
            if ( data.Category == category.Value )
                filteredDatas.Add( data );
        }

        return filteredDatas;
    }
}