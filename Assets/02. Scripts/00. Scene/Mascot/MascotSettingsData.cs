using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 화면 마스코트의 이동과 팁 표시 설정
/// </summary>
[CreateAssetMenu(
    fileName = "MascotSettingsData",
    menuName = "Play/MascotSettingsData" )]
public class MascotSettingsData : ScriptableObject
{
    [Header( "----- 팁 -----" )]
    [SerializeField, TextArea]
    List<string> _tips = new List<string>( );       //마스코트 팁 문구

    [SerializeField, Min( 0f )]
    float _minTipInterval = 8f;       //팁 최소 표시 간격

    [SerializeField, Min( 0f )]
    float _maxTipInterval = 16f;       //팁 최대 표시 간격

    [SerializeField, Min( 0f )]
    float _tipDisplayDuration = 3f;       //팁 표시 시간

    [Header( "----- 이동 -----" )]
    [SerializeField, Min( 0f )]
    float _minMoveInterval = 1f;       //다음 이동까지 최소 대기 시간

    [SerializeField, Min( 0f )]
    float _maxMoveInterval = 3f;       //다음 이동까지 최대 대기 시간

    [SerializeField, Min( 0.01f )]
    float _moveDuration = 2f;       //한 번 이동하는 시간

    /// <summary>
    /// 팁 문구 목록
    /// </summary>
    public IReadOnlyList<string> Tips => _tips;

    /// <summary>
    /// 팁 최소 표시 간격
    /// </summary>
    public float MinTipInterval => _minTipInterval;

    /// <summary>
    /// 팁 최대 표시 간격
    /// </summary>
    public float MaxTipInterval => _maxTipInterval;

    /// <summary>
    /// 팁 표시 시간
    /// </summary>
    public float TipDisplayDuration => _tipDisplayDuration;

    /// <summary>
    /// 이동 최소 대기 시간
    /// </summary>
    public float MinMoveInterval => _minMoveInterval;

    /// <summary>
    /// 이동 최대 대기 시간
    /// </summary>
    public float MaxMoveInterval => _maxMoveInterval;

    /// <summary>
    /// 한 번 이동하는 시간
    /// </summary>
    public float MoveDuration => _moveDuration;
}