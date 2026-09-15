using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 직원 데이터 목록과 아이디 조회 관리
/// </summary>
[CreateAssetMenu( menuName = "MaintenanceSettings/EmployeeDataMap" )]
public class EmployeeDataMap : ScriptableObject
{
    [Header( "----- 직원 데이터 -----" )]
    [SerializeField] EmployeeData [ ] _datas;       //전체 직원 데이터

    List<EmployeeData> _validDatas =
        new List<EmployeeData>( );       //사용 가능한 직원 데이터

    Dictionary<string, EmployeeData> _dataMap =
        new Dictionary<string, EmployeeData>( );       //직원 아이디 조회 맵

    /// <summary>
    /// 사용 가능한 직원 데이터 목록
    /// </summary>
    public IReadOnlyList<EmployeeData> Datas => _validDatas;

    /// <summary>
    /// 직원 데이터 조회 맵 초기화
    /// </summary>
    void OnEnable ()
    {
        BuildDataMap( );
    }

    /// <summary>
    /// 지정 아이디의 직원 데이터 조회
    /// </summary>
    /// <param name="id">조회할 직원 아이디</param>
    /// <param name="data">조회한 직원 데이터</param>
    /// <returns>조회 성공 여부</returns>
    public bool TryGetData ( string id, out EmployeeData data )
    {
        data = null;

        if ( string.IsNullOrWhiteSpace( id ) )
            return false;

        return _dataMap.TryGetValue( id, out data );
    }

    /// <summary>
    /// 사용 가능한 직원 데이터 조회 맵 생성
    /// </summary>
    void BuildDataMap ()
    {
        _validDatas.Clear( );
        _dataMap.Clear( );

        for ( int i = 0; i < _datas.Length; i++ )
        {
            EmployeeData data = _datas [ i ];

            if ( IsValidData( data ) == false )
                continue;

            if ( _dataMap.TryAdd( data.Id, data ) == false )
            {
                Debug.LogError(
                    $"직원 데이터 아이디가 중복되었습니다: {data.Id}", data );
                continue;
            }

            _validDatas.Add( data );
        }
    }

    /// <summary>
    /// 직원 데이터 필수 설정 확인
    /// </summary>
    /// <param name="data">확인할 직원 데이터</param>
    /// <returns>사용 가능한 데이터 여부</returns>
    bool IsValidData ( EmployeeData data )
    {
        if ( data == null )
        {
            Debug.LogError(
                "직원 데이터 목록에 빈 항목이 있습니다.", this );
            return false;
        }

        if ( string.IsNullOrWhiteSpace( data.Id ) )
        {
            Debug.LogError(
                "아이디가 비어 있는 직원 데이터가 있습니다.", data );
            return false;
        }

        return true;
    }
}