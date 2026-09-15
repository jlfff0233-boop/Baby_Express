using UnityEngine;

/// <summary>
/// 배경 음악과 효과음 종류별 음원 대응 데이터
/// </summary>
[CreateAssetMenu(
    menuName = "AudioSettings/AudioClipMap",
    fileName = "AudioClipMap" )]
public class AudioClipMap : ScriptableObject
{
    [Header( "----- 배경 음악 -----" )]
    [SerializeField] BGMClipEntry [ ] _bgmClips;       //배경 음악 대응 목록

    [Header( "----- 효과음 -----" )]
    [SerializeField] SFXClipEntry [ ] _sfxClips;       //효과음 대응 목록

    /// <summary>
    /// 지정 종류의 배경 음악 조회
    /// </summary>
    /// <param name="type">조회할 배경 음악 종류</param>
    /// <returns>대응하는 배경 음악 또는 미등록 시 null</returns>
    public AudioClip GetBGMClip ( BGMType type )
    {
        for ( int i = 0; i < _bgmClips.Length; i++ )
        {
            if ( _bgmClips [ i ].Type == type )
                return _bgmClips [ i ].Clip;
        }

        return null;
    }

    /// <summary>
    /// 지정 종류의 효과음 조회
    /// </summary>
    /// <param name="type">조회할 효과음 종류</param>
    /// <returns>대응하는 효과음 또는 미등록 시 null</returns>
    public AudioClip GetSFXClip ( SFXType type )
    {
        for ( int i = 0; i < _sfxClips.Length; i++ )
        {
            if ( _sfxClips [ i ].Type == type )
                return _sfxClips [ i ].Clip;
        }

        return null;
    }
}