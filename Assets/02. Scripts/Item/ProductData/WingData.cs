using UnityEngine;

/// <summary>
/// 날개 모양 타입
/// </summary>
public enum WingType
{
    None,       //없음
    Bird,       //새 날개
    Bat,        //박쥐 날개
    Skeleton,       //뼈 날개
}

/// <summary>
/// 날개 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "WingData" , menuName = "PartsSettings/WingData" )]
public class WingData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] WingType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Wings;

    /// <summary>
    /// 날개 모양 타입
    /// </summary>
    public WingType WingType => _type;
}
