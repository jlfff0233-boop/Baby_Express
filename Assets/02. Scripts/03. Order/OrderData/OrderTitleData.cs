using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 제목 설정 데이터
/// </summary>
[CreateAssetMenu ( fileName = "OrderTitleData" , menuName = "OrderSettings/TitleData" )]
public class OrderTitleData : ScriptableObject
{
    [Header ( "----- 주문 제목 -----" )]
    [SerializeField] string [ ] _adjectives;       //주문 제목 형용사
    [SerializeField] string [ ] _requests;       //주문 요청 문구

    /// <summary>
    /// 주문 제목 형용사 목록
    /// </summary>
    public IReadOnlyList<string> Adjectives => _adjectives;

    /// <summary>
    /// 주문 요청 문구 목록
    /// </summary>
    public IReadOnlyList<string> Requests => _requests;
}
