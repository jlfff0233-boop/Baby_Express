using UnityEngine;

/// <summary>
/// 털 타입
/// </summary>
public enum HairType
{
    None,       //없음
    Long,       //장모
    Short,      //단모
}

/// <summary>
/// 털 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "HairData" , menuName = "PartsSettings/HairData" )]
public class HairData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] HairType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Hair;

    /// <summary>
    /// 털 타입
    /// </summary>
    public HairType HairType => _type;
}
