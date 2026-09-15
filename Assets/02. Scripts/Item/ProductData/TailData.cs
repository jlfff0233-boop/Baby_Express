using UnityEngine;

/// <summary>
/// 꼬리 모양 타입
/// </summary>
public enum TailType
{
    None,       //없음
    Long,       //긺
    Short,      //짧음
}

/// <summary>
/// 꼬리 파츠 데이터
/// </summary>
[CreateAssetMenu ( fileName = "TailData" , menuName = "PartsSettings/TailData" )]
public class TailData : PartsData
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] TailType _type;

    /// <summary>
    /// 파츠 타입
    /// </summary>
    public override PartType PartType => PartType.Tail;

    /// <summary>
    /// 꼬리 모양 타입
    /// </summary>
    public TailType TailType => _type;
}
