using UnityEngine;

/// <summary>
/// 뿔 타입
/// </summary>
public enum HornType
{
    None,       //없음
    Goat,       //염소 뿔
    Unicorn,        //유니콘 뿔
}

/// <summary>
/// 뿔 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "HornData" , menuName = "PartsSettings/HornData" )]
public class HornData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] HornType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Horns;

    /// <summary>
    /// 뿔 타입
    /// </summary>
    public HornType HornType => _type;
}
