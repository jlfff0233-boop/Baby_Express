using UnityEngine;

/// <summary>
/// 눈 모양 타입
/// </summary>
public enum EyeType
{
    None,        //없음
    Animal,      //동물 눈
    Human,      //인간 눈
    Demon,       //악마 눈(역안)
}

/// <summary>
/// 눈 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "EyeData" , menuName = "PartsSettings/EyeData" )]
public class EyeData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] EyeType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Eye;

    /// <summary>
    /// 눈 모양 타입
    /// </summary>
    public EyeType EyeType => _type;
}
