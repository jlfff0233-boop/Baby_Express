using UnityEngine;

/// <summary>
/// 코 모양 타입
/// </summary>
public enum NoseType
{
    None,       //없음
    Animal,        //동물 코
    Human,      //인간 코
    TwoHoles,       //구멍만 있음
}

/// <summary>
/// 코 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "NoseData" , menuName = "PartsSettings/NoseData" )]
public class NoseData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] NoseType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Nose;

    /// <summary>
    /// 코 모양 타입
    /// </summary>
    public NoseType NoseType => _type;
}
