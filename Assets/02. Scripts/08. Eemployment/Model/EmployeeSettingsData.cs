using UnityEngine;

/// <summary>
/// 직원 시스템의 공용 설정 데이터
/// </summary>
[CreateAssetMenu( menuName = "MaintenanceSettings/EmployeeSettingsData" )]
public class EmployeeSettingsData : ScriptableObject
{
    [Header( "----- 직원 설정 -----" )]
    [SerializeField] EmployeeDataMap _dataMap;       //전체 직원 데이터 맵
    [SerializeField, Min( 1 )] int _maxEmployeeCount = 3;       //최대 고용 수

    /// <summary>
    /// 전체 직원 데이터 맵
    /// </summary>
    public EmployeeDataMap DataMap => _dataMap;

    /// <summary>
    /// 전체 최대 고용 수
    /// </summary>
    public int MaxEmployeeCount => _maxEmployeeCount;
}