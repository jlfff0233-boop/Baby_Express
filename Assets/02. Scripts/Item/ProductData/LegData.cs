using UnityEngine;

/// <summary>
/// 다리 모양 타입
/// </summary>
public enum LegType
{
    None,       //없음
    Hoofed,     //발굽
    Paw,        //발바닥
    Pinnipeds,      //지느러미
    Insect,     //곤충
    Human,      //인간형
}

/// <summary>
/// 다리 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "LegData" , menuName = "PartsSettings/LegData" )]
public class LegData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] LegType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Legs;

    /// <summary>
    /// 다리 모양 타입
    /// </summary>
    public LegType LegType => _type;
}
