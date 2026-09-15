using UnityEngine;

/// <summary>
/// 팔 모양 타입
/// </summary>
public enum ArmType
{
    None,       //없음
    Hoofed,     //발굽
    Paw,        //발바닥
    Pinnipeds,      //지느러미
    Insect,     //곤충
    Human,      //인간형
}

/// <summary>
/// 팔 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "ArmData" , menuName = "PartsSettings/ArmData" )]
public class ArmData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] ArmType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Arms;

    /// <summary>
    /// 팔 모양 타입
    /// </summary>
    public ArmType ArmType => _type;
}
