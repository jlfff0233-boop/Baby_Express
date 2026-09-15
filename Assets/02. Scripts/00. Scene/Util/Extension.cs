using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 공용 확장 기능 제공
/// </summary>
public static class Extension
{
    /// <summary>
    /// 파츠 타입 한글 이름 반환
    /// </summary>
    /// <param name="partType">파츠 타입</param>
    /// <returns>파츠 타입 한글 이름</returns>
    public static string GetDisplayName ( this PartType partType )
    {
        switch ( partType )
        {
            case PartType.Body: return "몸통";
            case PartType.Eye: return "눈";
            case PartType.Nose: return "코";
            case PartType.Mouth: return "입";
            case PartType.Arms: return "팔";
            case PartType.Legs: return "다리";
            case PartType.Hair: return "털";
            case PartType.Wings: return "날개";
            case PartType.Tail: return "꼬리";
            case PartType.Claws: return "발톱";
            case PartType.Horns: return "뿔";
            default: return string.Empty;
        }
    }

    /// <summary>
    /// 파츠 테마 분류 반환
    /// </summary>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>파츠 테마 분류</returns>
    public static PartThemeCategory GetCategory ( this PartTheme theme )
    {
        switch ( theme )
        {
            case PartTheme.Soft:
            case PartTheme.Round:
            case PartTheme.Hard:
            case PartTheme.Fluffy:
            case PartTheme.Shiny:
            case PartTheme.Pointy:
            case PartTheme.Slippery:
                return PartThemeCategory.Trait;

            case PartTheme.Slime:
            case PartTheme.Werewolf:
            case PartTheme.Vampire:
            case PartTheme.Bicorn:
            case PartTheme.Gargoyle:
            case PartTheme.Imp:
            case PartTheme.Kraken:
            case PartTheme.Mandragora:
            case PartTheme.Ghost:
            case PartTheme.Mushroom:
            case PartTheme.Goat:
            case PartTheme.Moth:
                return PartThemeCategory.Species;

            default:
                return PartThemeCategory.None;
        }
    }

    /// <summary>
    /// 성질 테마 여부 확인
    /// </summary>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>성질 테마 여부</returns>
    public static bool IsTraitTheme ( this PartTheme theme )
    {
        return theme.GetCategory( ) == PartThemeCategory.Trait;
    }

    /// <summary>
    /// 종족 테마 여부 확인
    /// </summary>
    /// <param name="theme">확인할 파츠 테마</param>
    /// <returns>종족 테마 여부</returns>
    public static bool IsSpeciesTheme ( this PartTheme theme )
    {
        return theme.GetCategory( ) == PartThemeCategory.Species;
    }

    /// <summary>
    /// 파츠 테마 한글 이름 반환
    /// </summary>
    /// <param name="theme">파츠 테마</param>
    /// <returns>파츠 테마 한글 이름</returns>
    public static string GetDisplayName ( this PartTheme theme )
    {
        switch ( theme )
        {
            case PartTheme.Soft: return "말랑";
            case PartTheme.Round: return "동글";
            case PartTheme.Sharp: return "뾰족";
            case PartTheme.Hard: return "단단";
            case PartTheme.Cute: return "귀여움";
            case PartTheme.Intimidating: return "위압";
            case PartTheme.Horrible: return "끔찍";
            case PartTheme.Rough: return "거칠";
            case PartTheme.Fluffy: return "복슬";
            case PartTheme.Shiny: return "반짝";
            case PartTheme.Heavy: return "묵직";
            case PartTheme.Playful: return "장난";
            case PartTheme.Dreamy: return "몽환";
            case PartTheme.Pointy: return "뾰족";
            case PartTheme.Slippery: return "미끌";

            case PartTheme.Slime: return "슬라임";
            case PartTheme.Werewolf: return "웨어울프";
            case PartTheme.Vampire: return "뱀파이어";
            case PartTheme.Bicorn: return "바이콘";
            case PartTheme.Gargoyle: return "가고일";
            case PartTheme.Imp: return "임프";
            case PartTheme.Kraken: return "크라켄";
            case PartTheme.Mandragora: return "만드라고라";
            case PartTheme.Ghost: return "고스트";
            case PartTheme.Mushroom: return "펑거스";
            case PartTheme.Goat: return "바포메트";
            case PartTheme.Moth: return "모스맨";
            default: return string.Empty;
        }
    }

    /// <summary>
    /// 게임오브젝트에서 컴포넌트를 조회하거나 새로 추가
    /// </summary>
    /// <typeparam name="T">조회하거나 추가할 컴포넌트 타입</typeparam>
    /// <param name="gameObject">컴포넌트를 사용할 게임오브젝트</param>
    /// <returns>조회하거나 추가한 컴포넌트</returns>
    public static T GetOrAddComponent<T> ( this GameObject gameObject ) where T : Component
    {
        return Util.GetOrAddComponent<T>( gameObject );
    }

    /// <summary>
    /// 버튼에 공용 클릭 눌림과 복귀 연출 연결
    /// </summary>
    /// <param name="button">클릭 연출을 적용할 버튼</param>
    public static void BindClickHighlight ( this Button button )
    {
        UIHighlightView highlight =
            button.gameObject.GetOrAddComponent<UIHighlightView>( );

        highlight.BindClick( button );
    }

    /// <summary>
    /// UI 아이콘 Sprite와 원본 비율 적용
    /// </summary>
    /// <param name="image">아이콘을 표시할 이미지</param>
    /// <param name="sprite">표시할 Sprite</param>
    public static void SetIconSprite (
        this Image image, Sprite sprite )
    {
        UIImageAspectView aspectView =
            image.gameObject.GetOrAddComponent<UIImageAspectView>( );

        aspectView.SetSprite( sprite );
    }

    /// <summary>
    /// 드롭다운 표시 항목과 선택값 설정
    /// </summary>
    /// <param name="dropdown">설정할 드롭다운</param>
    /// <param name="options">표시 문구 목록</param>
    /// <param name="selectedIndex">선택할 항목 인덱스</param>
    public static void SetOptions (
        this TMP_Dropdown dropdown ,
        IReadOnlyList<string> options , int selectedIndex = 0 )
    {
        Util.SetDropdownOptions ( dropdown , options , selectedIndex );
    }
}
