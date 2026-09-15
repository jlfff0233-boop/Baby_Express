using UnityEngine;

/// <summary>
/// 발톱 모양 타입
/// </summary>
public enum ClawType
{
    None,       //없음
    Blunt,      //뭉툭한
    Sharp,      //날카로움
}

/// <summary>
/// 발톱 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "ClawData" , menuName = "PartsSettings/ClawData" )]
public class ClawData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] ClawType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Claws;

    /// <summary>
    /// 발톱 모양 타입
    /// </summary>
    public ClawType ClawType => _type;
}
