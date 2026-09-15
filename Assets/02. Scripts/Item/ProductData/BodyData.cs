using UnityEngine;

/// <summary>
/// 신체 모양 타입
/// </summary>
public enum BodyType
{
    Slime,      //슬라임형
    Worm,       //애벌레형
    Animal,        //동물
    Human,      //인간
    Ghost,      //유령
}

/// <summary>
/// 신체 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "BodyData" , menuName = "PartsSettings/BodyData" )]
public class BodyData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] BodyType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Body;

    /// <summary>
    /// 신체 모양 타입
    /// </summary>
    public BodyType BodyType => _type;
}
