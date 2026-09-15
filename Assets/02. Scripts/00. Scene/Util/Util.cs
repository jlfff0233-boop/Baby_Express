using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 공용 기능 제공
/// </summary>
public static class Util
{
    /// <summary>
    /// 지정 게임오브젝트에서 컴포넌트를 조회하거나 새로 추가
    /// </summary>
    /// <typeparam name="T">조회하거나 추가할 컴포넌트 타입</typeparam>
    /// <param name="gameObject">컴포넌트를 사용할 게임오브젝트</param>
    /// <returns>조회하거나 추가한 컴포넌트</returns>
    public static T GetOrAddComponent<T> ( GameObject gameObject ) where T : Component
    {
        T component = gameObject.GetComponent<T> ( );

        if ( component == null )
            component = gameObject.AddComponent<T> ( );

        return component;
    }

    /// <summary>
    /// 드롭다운 표시 항목과 선택값 설정
    /// </summary>
    /// <param name="dropdown">설정할 드롭다운</param>
    /// <param name="options">표시 문구 목록</param>
    /// <param name="selectedIndex">선택할 항목 인덱스</param>
    public static void SetDropdownOptions (
        TMP_Dropdown dropdown ,
        IReadOnlyList<string> options , int selectedIndex = 0 )
    {
        dropdown.ClearOptions ( );

        var optionList = new List<string> ( options.Count );

        for ( int i = 0 ; i < options.Count ; i++ )
            optionList.Add ( options [ i ] );

        dropdown.AddOptions ( optionList );
        dropdown.SetValueWithoutNotify ( selectedIndex );
        dropdown.RefreshShownValue ( );
    }
}
