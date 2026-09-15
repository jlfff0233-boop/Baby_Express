using System;
using UnityEngine;

/// <summary>
/// 배경 음악 종류와 음원 대응 데이터
/// </summary>
[Serializable]
public struct BGMClipEntry
{
    [SerializeField] BGMType _type;       //배경 음악 종류
    [SerializeField] AudioClip _clip;       //재생할 음원

    /// <summary>
    /// 배경 음악 종류
    /// </summary>
    public BGMType Type => _type;

    /// <summary>
    /// 재생할 배경 음악
    /// </summary>
    public AudioClip Clip => _clip;
}

/// <summary>
/// 효과음 종류와 음원 대응 데이터
/// </summary>
[Serializable]
public struct SFXClipEntry
{
    [SerializeField] SFXType _type;       //효과음 종류
    [SerializeField] AudioClip _clip;       //재생할 음원

    /// <summary>
    /// 효과음 종류
    /// </summary>
    public SFXType Type => _type;

    /// <summary>
    /// 재생할 효과음
    /// </summary>
    public AudioClip Clip => _clip;
}