using UnityEngine;

/// <summary>
/// 주문 생성에 필요한 공통 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "OrderSettings/GenerationSettingsData" )]
public class OrderGenerationSettingsData : ScriptableObject
{
    [Header( "----- 주문 생성 설정 -----" )]
    [SerializeField] OrderTitleData _titleData;       //주문 제목 설정
    [SerializeField] OrderDifficultyDataMap _difficultyDataMap;       //주문 난이도 데이터 맵
    [SerializeField] OrderSpecialSettingsData _specialSettings;       //특수 주문 설정

    /// <summary>
    /// 주문 제목 설정
    /// </summary>
    public OrderTitleData TitleData => _titleData;

    /// <summary>
    /// 주문 난이도 데이터 맵
    /// </summary>
    public OrderDifficultyDataMap DifficultyDataMap => _difficultyDataMap;

    /// <summary>
    /// 특수 주문 설정
    /// </summary>
    public OrderSpecialSettingsData SpecialSettings => _specialSettings;
}
