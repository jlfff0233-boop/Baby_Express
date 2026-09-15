using UnityEngine;

/// <summary>
/// 입 모양 타입
/// </summary>
public enum MouthType
{
    None,       //없음
    Carnivore,        //육식
    Herbivore,      //초식
    Human,      //인간
}

/// <summary>
/// 입 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "MouthData" , menuName = "PartsSettings/MouthData" )]
public class MouthData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] MouthType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Mouth;

    /// <summary>
    /// 입 모양 타입
    /// </summary>
    public MouthType MouthType => _type;
}
