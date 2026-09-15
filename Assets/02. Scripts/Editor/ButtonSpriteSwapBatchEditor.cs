using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 선택한 UI 아래의 버튼을 Sprite Swap 방식으로 일괄 설정하는 Editor 도구
/// </summary>
public class ButtonSpriteSwapBatchEditor : EditorWindow
{
    Sprite _highlightedSprite;       //커서를 올렸을 때 스프라이트
    Sprite _pressedSprite;       //버튼을 눌렀을 때 스프라이트
    Sprite _disabledSprite;       //비활성 버튼 스프라이트

    /// <summary>
    /// 버튼 Sprite Swap 일괄 설정 창 열기
    /// </summary>
    [MenuItem( "Tools/Baby Express/UI/버튼 Sprite Swap 일괄 설정" )]
    static void OpenWindow ()
    {
        GetWindow<ButtonSpriteSwapBatchEditor>(
            "버튼 Sprite Swap 설정" );
    }

    /// <summary>
    /// 일괄 설정 창 표시
    /// </summary>
    void OnGUI ()
    {
        EditorGUILayout.LabelField(
            "버튼 상태 스프라이트",
            EditorStyles.boldLabel );

        _highlightedSprite =
            EditorGUILayout.ObjectField(
                "Highlighted",
                _highlightedSprite,
                typeof( Sprite ),
                false ) as Sprite;

        _pressedSprite =
            EditorGUILayout.ObjectField(
                "Pressed",
                _pressedSprite,
                typeof( Sprite ),
                false ) as Sprite;

        _disabledSprite =
            EditorGUILayout.ObjectField(
                "Disabled",
                _disabledSprite,
                typeof( Sprite ),
                false ) as Sprite;

        EditorGUILayout.Space( );

        EditorGUILayout.HelpBox(
            "Hierarchy의 Canvas 또는 Project의 프리팹을 선택한 뒤 실행합니다.\n" +
            "Normal Sprite는 현재 버튼 이미지를 그대로 유지합니다.",
            MessageType.Info );

        if ( GUILayout.Button( "선택 대상에 일괄 적용" ) )
            ApplySelectedObjects( );
    }

    /// <summary>
    /// 선택한 씬 오브젝트와 프리팹의 버튼 설정 일괄 변경
    /// </summary>
    void ApplySelectedObjects ()
    {
        if ( _highlightedSprite == null ||
            _pressedSprite == null ||
            _disabledSprite == null )
        {
            Debug.LogWarning(
                "버튼 상태 스프라이트를 모두 할당해 주세요." );
            return;
        }

        UnityEngine.Object [ ] selectedObjects =
            Selection.objects;

        var processedButtons = new HashSet<Button>( );
        var skippedButtons = new List<string>( );

        int changedCount = 0;

        for ( int i = 0; i < selectedObjects.Length; i++ )
        {
            if ( selectedObjects [ i ] is not GameObject root )
                continue;

            string assetPath =
                AssetDatabase.GetAssetPath( root );

            if ( assetPath.EndsWith( ".prefab" ) )
            {
                changedCount += ApplyPrefab(
                    assetPath, skippedButtons );

                continue;
            }

            changedCount += ApplySceneObject(
                root, processedButtons, skippedButtons );
        }

        AssetDatabase.SaveAssets( );

        Debug.Log(
            $"Button Sprite Swap 일괄 설정 완료: {changedCount}개" );

        if ( skippedButtons.Count > 0 )
        {
            Debug.LogWarning(
                "자기 Image가 없어 건너뛴 버튼:\n" +
                string.Join( "\n", skippedButtons ) );
        }
    }

    /// <summary>
    /// 선택한 씬 오브젝트 아래의 버튼 설정 변경
    /// </summary>
    /// <param name="root">검색할 최상위 오브젝트</param>
    /// <param name="processedButtons">이미 처리한 버튼</param>
    /// <param name="skippedButtons">건너뛴 버튼 이름 목록</param>
    /// <returns>변경한 버튼 수</returns>
    int ApplySceneObject (
        GameObject root,
        HashSet<Button> processedButtons,
        List<string> skippedButtons )
    {
        Button [ ] buttons =
            root.GetComponentsInChildren<Button>( true );

        int changedCount = 0;

        for ( int i = 0; i < buttons.Length; i++ )
        {
            Button button = buttons [ i ];

            if ( processedButtons.Add( button ) == false )
                continue;

            Undo.RecordObject(
                button, "버튼 Sprite Swap 일괄 설정" );

            if ( ApplyButton( button ) == false )
            {
                skippedButtons.Add(
                    GetButtonPath( button.transform ) );

                continue;
            }

            EditorUtility.SetDirty( button );
            changedCount++;
        }

        if ( changedCount > 0 &&
            root.scene.IsValid( ) )
        {
            EditorSceneManager.MarkSceneDirty(
                root.scene );
        }

        return changedCount;
    }

    /// <summary>
    /// 선택한 프리팹 내부의 버튼 설정 변경
    /// </summary>
    /// <param name="assetPath">프리팹 경로</param>
    /// <param name="skippedButtons">건너뛴 버튼 이름 목록</param>
    /// <returns>변경한 버튼 수</returns>
    int ApplyPrefab (
        string assetPath,
        List<string> skippedButtons )
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents( assetPath );

        int changedCount = 0;

        try
        {
            Button [ ] buttons =
                prefabRoot.GetComponentsInChildren<Button>( true );

            for ( int i = 0; i < buttons.Length; i++ )
            {
                Button button = buttons [ i ];

                if ( ApplyButton( button ) == false )
                {
                    skippedButtons.Add(
                        $"{assetPath}: {GetButtonPath( button.transform )}" );

                    continue;
                }

                EditorUtility.SetDirty( button );
                changedCount++;
            }

            if ( changedCount > 0 )
            {
                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot, assetPath );
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(
                prefabRoot );
        }

        return changedCount;
    }

    /// <summary>
    /// 버튼을 자기 Image를 사용하는 Sprite Swap 방식으로 설정
    /// </summary>
    /// <param name="button">설정할 버튼</param>
    /// <returns>설정 성공 여부</returns>
    bool ApplyButton ( Button button )
    {
        Image targetImage =
            button.GetComponent<Image>( );

        //자기 Image가 없는 특수 버튼은 임의 변경하지 않음
        if ( targetImage == null )
            return false;

        SpriteState spriteState =
            button.spriteState;

        spriteState.highlightedSprite =
            _highlightedSprite;
        spriteState.pressedSprite =
            _pressedSprite;
        spriteState.selectedSprite =
            _highlightedSprite;
        spriteState.disabledSprite =
            _disabledSprite;

        button.transition =
            Selectable.Transition.SpriteSwap;
        button.targetGraphic = targetImage;
        button.spriteState = spriteState;

        return true;
    }

    /// <summary>
    /// 버튼의 Hierarchy 경로 반환
    /// </summary>
    /// <param name="target">경로를 확인할 버튼 Transform</param>
    /// <returns>버튼 Hierarchy 경로</returns>
    string GetButtonPath ( Transform target )
    {
        string path = target.name;

        while ( target.parent != null )
        {
            target = target.parent;
            path = $"{target.name}/{path}";
        }

        return path;
    }
}