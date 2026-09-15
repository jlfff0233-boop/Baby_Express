using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 오디오 관리자 초기화에 사용하는 참조 데이터
/// </summary>
[CreateAssetMenu( menuName = "AudioSettings/AudioManagerData" )]
public class AudioManagerData : ScriptableObject
{
    [Header( "----- 오디오 믹서 -----" )]
    [SerializeField] AudioMixer _audioMixer;       //메인 오디오 믹서
    [SerializeField] AudioMixerGroup _bgmMixerGroup;       //BGM 출력 그룹
    [SerializeField] AudioMixerGroup _sfxMixerGroup;       //효과음 출력 그룹

    [Header( "----- 음원 데이터 -----" )]
    [SerializeField] AudioClipMap _audioClipMap;       //종류별 음원 대응 데이터


    /// <summary>
    /// 메인 오디오 믹서
    /// </summary>
    public AudioMixer AudioMixer => _audioMixer;

    /// <summary>
    /// BGM 출력 그룹
    /// </summary>
    public AudioMixerGroup BGMMixerGroup => _bgmMixerGroup;

    /// <summary>
    /// 효과음 출력 그룹
    /// </summary>
    public AudioMixerGroup SFXMixerGroup => _sfxMixerGroup;

    /// <summary>
    /// 종류별 음원 대응 데이터
    /// </summary>
    public AudioClipMap AudioClipMap => _audioClipMap;
}
