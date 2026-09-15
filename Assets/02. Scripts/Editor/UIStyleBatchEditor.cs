using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 Sprite Swap 스타일 규칙
/// </summary>
[System.Serializable]
public class ButtonSpriteStyleRule
{
    [SerializeField] string _styleName;       //Inspector에서 구분할 스타일 이름
    [SerializeField] Sprite _sourceSprite;       //현재 버튼을 판별할 기준 스프라이트
    [SerializeField] string _nameKeyword;       //버튼 이름에 포함될 판별 문자
    [SerializeField] Sprite _normalSprite;       //기본 상태 스프라이트
    [SerializeField] Sprite _highlightedSprite;       //커서를 올린 상태 스프라이트
    [SerializeField] Sprite _pressedSprite;       //눌린 상태 스프라이트
    [SerializeField] Sprite _selectedSprite;       //선택 상태 스프라이트
    [SerializeField] Sprite _disabledSprite;       //비활성 상태 스프라이트
    [SerializeField] bool _setSize;       //버튼 크기 적용 여부
    [SerializeField] Vector2 _size;       //적용할 버튼 가로와 세로 크기
    [SerializeField] bool _setPixelsPerUnitMultiplier;       //픽셀 배율 적용 여부
    [SerializeField, Min( 0.01f )]
    float _pixelsPerUnitMultiplier = 1f;       //Image 픽셀 배율

    /// <summary>
    /// 스타일 이름
    /// </summary>
    public string StyleName => _styleName;

    /// <summary>
    /// 현재 버튼 판별용 스프라이트
    /// </summary>
    public Sprite SourceSprite => _sourceSprite;

    /// <summary>
    /// 버튼 이름 판별 문자
    /// </summary>
    public string NameKeyword => _nameKeyword;

    /// <summary>
    /// 기본 상태 스프라이트
    /// </summary>
    public Sprite NormalSprite => _normalSprite;

    /// <summary>
    /// 커서를 올린 상태 스프라이트
    /// </summary>
    public Sprite HighlightedSprite => _highlightedSprite;

    /// <summary>
    /// 눌린 상태 스프라이트
    /// </summary>
    public Sprite PressedSprite => _pressedSprite;

    /// <summary>
    /// 선택 상태 스프라이트
    /// </summary>
    public Sprite SelectedSprite => _selectedSprite;

    /// <summary>
    /// 비활성 상태 스프라이트
    /// </summary>
    public Sprite DisabledSprite => _disabledSprite;

    /// <summary>
    /// 버튼 크기 적용 여부
    /// </summary>
    public bool SetSize => _setSize;

    /// <summary>
    /// 적용할 버튼 가로와 세로 크기
    /// </summary>
    public Vector2 Size => _size;

    /// <summary>
    /// 픽셀 배율 적용 여부
    /// </summary>
    public bool SetPixelsPerUnitMultiplier =>
        _setPixelsPerUnitMultiplier;

    /// <summary>
    /// Image 픽셀 배율
    /// </summary>
    public float PixelsPerUnitMultiplier =>
        _pixelsPerUnitMultiplier;
}

/// <summary>
/// 일반 Image 스프라이트 교체 규칙
/// </summary>
[System.Serializable]
public class ImageSpriteReplaceRule
{
    [SerializeField] string _ruleName;       //Inspector에서 구분할 규칙 이름
    [SerializeField] Sprite _sourceSprite;       //교체 전 스프라이트
    [SerializeField] Sprite _targetSprite;       //교체 후 스프라이트
    [SerializeField] bool _setSliced;       //교체 후 Sliced 타입 적용 여부

    /// <summary>
    /// 규칙 이름
    /// </summary>
    public string RuleName => _ruleName;

    /// <summary>
    /// 교체 전 스프라이트
    /// </summary>
    public Sprite SourceSprite => _sourceSprite;

    /// <summary>
    /// 교체 후 스프라이트
    /// </summary>
    public Sprite TargetSprite => _targetSprite;

    /// <summary>
    /// Sliced 타입 적용 여부
    /// </summary>
    public bool SetSliced => _setSliced;
}

/// <summary>
/// TMP 글꼴 교체 규칙
/// </summary>
[System.Serializable]
public class TMPFontReplaceRule
{
    [SerializeField] string _ruleName;       //Inspector에서 구분할 규칙 이름
    [SerializeField] TMP_FontAsset _sourceFont;       //교체 전 글꼴
    [SerializeField] TMP_FontAsset _targetFont;       //교체 후 글꼴

    /// <summary>
    /// 규칙 이름
    /// </summary>
    public string RuleName => _ruleName;

    /// <summary>
    /// 교체 전 글꼴
    /// </summary>
    public TMP_FontAsset SourceFont => _sourceFont;

    /// <summary>
    /// 교체 후 글꼴
    /// </summary>
    public TMP_FontAsset TargetFont => _targetFont;
}

/// <summary>
/// UI 스타일 일괄 적용 규칙 저장 설정
/// </summary>
[FilePath(
    "ProjectSettings/BabyExpressUIStyleBatchSettings.asset",
    FilePathAttribute.Location.ProjectFolder )]
class UIStyleBatchSettings :
    ScriptableSingleton<UIStyleBatchSettings>
{
    [SerializeField]
    List<ButtonSpriteStyleRule> _buttonStyles =
        new List<ButtonSpriteStyleRule>( );       //버튼 스타일 규칙

    [SerializeField]
    List<ImageSpriteReplaceRule> _imageRules =
        new List<ImageSpriteReplaceRule>( );       //일반 Image 교체 규칙

    [SerializeField]
    List<TMPFontReplaceRule> _fontRules =
        new List<TMPFontReplaceRule>( );       //TMP 글꼴 교체 규칙

    /// <summary>
    /// 버튼 스타일 규칙
    /// </summary>
    public List<ButtonSpriteStyleRule> ButtonStyles => _buttonStyles;

    /// <summary>
    /// 일반 Image 교체 규칙
    /// </summary>
    public List<ImageSpriteReplaceRule> ImageRules => _imageRules;

    /// <summary>
    /// TMP 글꼴 교체 규칙
    /// </summary>
    public List<TMPFontReplaceRule> FontRules => _fontRules;

    /// <summary>
    /// 저장된 규칙 존재 여부
    /// </summary>
    public bool HasRules =>
        _buttonStyles.Count > 0 ||
        _imageRules.Count > 0 ||
        _fontRules.Count > 0;

    /// <summary>
    /// 기존 Editor 창에 남은 규칙을 최초 한 번 이전
    /// </summary>
    /// <param name="buttonStyles">버튼 스타일 규칙</param>
    /// <param name="imageRules">일반 Image 교체 규칙</param>
    /// <param name="fontRules">TMP 글꼴 교체 규칙</param>
    public void ImportWindowRules (
        List<ButtonSpriteStyleRule> buttonStyles,
        List<ImageSpriteReplaceRule> imageRules,
        List<TMPFontReplaceRule> fontRules )
    {
        _buttonStyles = buttonStyles;
        _imageRules = imageRules;
        _fontRules = fontRules;

        SaveSettings( );
    }

    /// <summary>
    /// 현재 UI 스타일 규칙을 프로젝트 설정에 저장
    /// </summary>
    public void SaveSettings ()
    {
        Save( true );
    }
}

/// <summary>
/// 선택한 씬 UI와 프리팹의 버튼, 이미지, 글꼴 스타일 일괄 적용 도구
/// </summary>
public class UIStyleBatchEditor : EditorWindow
{
    //기존 창 직렬화 값이 남아 있으면 프로젝트 설정으로 한 번 이전합니다.
    [SerializeField]
    List<ButtonSpriteStyleRule> _buttonStyles =
        new List<ButtonSpriteStyleRule>( );       //버튼 스타일 규칙

    [SerializeField]
    List<ImageSpriteReplaceRule> _imageRules =
        new List<ImageSpriteReplaceRule>( );       //일반 Image 교체 규칙

    [SerializeField]
    List<TMPFontReplaceRule> _fontRules =
        new List<TMPFontReplaceRule>( );       //TMP 글꼴 교체 규칙

    UIStyleBatchSettings _settings;       //프로젝트에 저장할 UI 스타일 설정
    SerializedObject _serializedSettings;       //UI 스타일 설정 직렬화 객체
    Vector2 _scrollPosition;       //Editor 창 스크롤 위치

    /// <summary>
    /// UI 스타일 일괄 적용 창 열기
    /// </summary>
    [MenuItem( "Tools/Baby Express/UI/UI 스타일 일괄 적용" )]
    static void OpenWindow ()
    {
        GetWindow<UIStyleBatchEditor>(
            "UI 스타일 일괄 적용" );
    }

    /// <summary>
    /// 저장 설정과 직렬화 객체 초기화
    /// </summary>
    void OnEnable ()
    {
        _settings = UIStyleBatchSettings.instance;

        //기존 창에만 남은 규칙이 있으면 새 프로젝트 설정으로 이전
        if ( _settings.HasRules == false &&
            HasWindowRules( ) )
        {
            _settings.ImportWindowRules(
                _buttonStyles, _imageRules, _fontRules );
        }

        _serializedSettings =
            new SerializedObject( _settings );
    }

    /// <summary>
    /// 창을 닫거나 스크립트가 다시 컴파일되기 전에 설정 저장
    /// </summary>
    void OnDisable ()
    {
        if ( _settings != null )
            _settings.SaveSettings( );
    }

    /// <summary>
    /// 기존 Editor 창에 이전할 규칙이 남아 있는지 확인
    /// </summary>
    /// <returns>기존 규칙 존재 여부</returns>
    bool HasWindowRules ()
    {
        return
            _buttonStyles.Count > 0 ||
            _imageRules.Count > 0 ||
            _fontRules.Count > 0;
    }

    /// <summary>
    /// UI 스타일 규칙과 실행 버튼 표시
    /// </summary>
    void OnGUI ()
    {
        _serializedSettings.Update( );

        _scrollPosition = EditorGUILayout.BeginScrollView(
            _scrollPosition );

        EditorGUILayout.HelpBox(
            "Hierarchy의 Canvas 또는 Project의 프리팹을 선택한 뒤 실행합니다. " +
            "현재 스프라이트나 글꼴이 규칙의 교체 전 항목과 일치하는 대상만 변경합니다.",
            MessageType.Info );

        EditorGUILayout.PropertyField(
            _serializedSettings.FindProperty( "_buttonStyles" ),
            new GUIContent( "버튼 스타일 규칙" ), true );

        EditorGUILayout.Space( );

        EditorGUILayout.PropertyField(
            _serializedSettings.FindProperty( "_imageRules" ),
            new GUIContent( "패널 / 일반 Image 교체 규칙" ), true );

        EditorGUILayout.Space( );

        EditorGUILayout.PropertyField(
            _serializedSettings.FindProperty( "_fontRules" ),
            new GUIContent( "TMP 글꼴 교체 규칙" ), true );

        if ( _serializedSettings.ApplyModifiedProperties( ) )
            _settings.SaveSettings( );

        EditorGUILayout.Space( );

        if ( GUILayout.Button( "선택 대상에 일괄 적용" ) )
            ApplySelectedObjects( );

        EditorGUILayout.EndScrollView( );
    }

    /// <summary>
    /// 선택한 씬 오브젝트와 프리팹에 등록된 UI 스타일 규칙 적용
    /// </summary>
    void ApplySelectedObjects ()
    {
        UnityEngine.Object[] selectedObjects =
            Selection.objects;

        if ( selectedObjects.Length == 0 )
        {
            Debug.LogWarning(
                "UI 스타일을 적용할 Canvas 또는 프리팹을 선택해 주세요." );
            return;
        }

        var processedButtons = new HashSet<Button>( );
        var processedImages = new HashSet<Image>( );
        var processedTexts = new HashSet<TMP_Text>( );
        var skippedButtons = new List<string>( );

        int buttonCount = 0;
        int imageCount = 0;
        int fontCount = 0;

        for ( int i = 0; i < selectedObjects.Length; i++ )
        {
            if ( selectedObjects [ i ] is not GameObject root )
                continue;

            string assetPath =
                AssetDatabase.GetAssetPath( root );

            if ( assetPath.EndsWith( ".prefab" ) )
            {
                ApplyPrefab(
                    assetPath,
                    ref buttonCount, ref imageCount, ref fontCount,
                    skippedButtons );

                continue;
            }

            bool changed = ApplyRoot(
                root, true,
                processedButtons, processedImages, processedTexts,
                ref buttonCount, ref imageCount, ref fontCount,
                skippedButtons );

            if ( changed && root.scene.IsValid( ) )
                EditorSceneManager.MarkSceneDirty( root.scene );
        }

        AssetDatabase.SaveAssets( );

        Debug.Log(
            $"UI 스타일 일괄 적용 완료: " +
            $"버튼 {buttonCount}개 / 이미지 {imageCount}개 / 글꼴 {fontCount}개" );

        if ( skippedButtons.Count > 0 )
        {
            Debug.LogWarning(
                "자기 Image가 없어 건너뛴 버튼:\n" +
                string.Join( "\n", skippedButtons ) );
        }
    }

    /// <summary>
    /// 프리팹 내부의 UI 스타일 변경
    /// </summary>
    /// <param name="assetPath">프리팹 경로</param>
    /// <param name="buttonCount">변경한 버튼 수</param>
    /// <param name="imageCount">변경한 이미지 수</param>
    /// <param name="fontCount">변경한 글꼴 수</param>
    /// <param name="skippedButtons">건너뛴 버튼 목록</param>
    void ApplyPrefab (
        string assetPath,
        ref int buttonCount, ref int imageCount, ref int fontCount,
        List<string> skippedButtons )
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents( assetPath );

        try
        {
            var processedButtons = new HashSet<Button>( );
            var processedImages = new HashSet<Image>( );
            var processedTexts = new HashSet<TMP_Text>( );

            bool changed = ApplyRoot(
                prefabRoot, false,
                processedButtons, processedImages, processedTexts,
                ref buttonCount, ref imageCount, ref fontCount,
                skippedButtons, assetPath );

            if ( changed )
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
    }

    /// <summary>
    /// 지정한 UI 루트 아래의 버튼, 이미지, 글꼴 규칙 적용
    /// </summary>
    /// <param name="root">적용할 UI 최상위 오브젝트</param>
    /// <param name="recordUndo">실행 취소 기록 여부</param>
    /// <param name="processedButtons">이미 처리한 버튼</param>
    /// <param name="processedImages">이미 처리한 이미지</param>
    /// <param name="processedTexts">이미 처리한 TMP 텍스트</param>
    /// <param name="buttonCount">변경한 버튼 수</param>
    /// <param name="imageCount">변경한 이미지 수</param>
    /// <param name="fontCount">변경한 글꼴 수</param>
    /// <param name="skippedButtons">건너뛴 버튼 목록</param>
    /// <param name="assetPath">프리팹 경로</param>
    /// <returns>변경 여부</returns>
    bool ApplyRoot (
        GameObject root, bool recordUndo,
        HashSet<Button> processedButtons,
        HashSet<Image> processedImages,
        HashSet<TMP_Text> processedTexts,
        ref int buttonCount, ref int imageCount, ref int fontCount,
        List<string> skippedButtons,
        string assetPath = "" )
    {
        bool changed = false;

        Button[] buttons =
            root.GetComponentsInChildren<Button>( true );

        for ( int i = 0; i < buttons.Length; i++ )
        {
            Button button = buttons [ i ];

            if ( processedButtons.Add( button ) == false )
                continue;

            bool buttonChanged = ApplyButton(
                button, recordUndo, skippedButtons, assetPath );

            if ( buttonChanged == false )
                continue;

            buttonCount++;
            changed = true;
        }

        Image[] images =
            root.GetComponentsInChildren<Image>( true );

        for ( int i = 0; i < images.Length; i++ )
        {
            Image image = images [ i ];

            if ( processedImages.Add( image ) == false ||
                image.GetComponent<Button>( ) != null )
            {
                continue;
            }

            bool spriteChanged =
                ApplyImage( image, recordUndo );

            if ( spriteChanged == false )
            {
                continue;
            }

            imageCount++;
            changed = true;
        }

        TMP_Text[] texts =
            root.GetComponentsInChildren<TMP_Text>( true );

        for ( int i = 0; i < texts.Length; i++ )
        {
            TMP_Text text = texts [ i ];

            if ( processedTexts.Add( text ) == false ||
                ApplyFont( text, recordUndo ) == false )
            {
                continue;
            }

            fontCount++;
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// 현재 기본 스프라이트와 일치하는 버튼 스타일 적용
    /// </summary>
    /// <param name="button">변경할 버튼</param>
    /// <param name="recordUndo">실행 취소 기록 여부</param>
    /// <param name="skippedButtons">건너뛴 버튼 목록</param>
    /// <param name="assetPath">프리팹 경로</param>
    /// <returns>변경 여부</returns>
    bool ApplyButton (
        Button button, bool recordUndo,
        List<string> skippedButtons, string assetPath )
    {
        Image targetImage =
            button.GetComponent<Image>( );

        if ( targetImage == null )
        {
            string buttonPath =
                GetObjectPath( button.transform );

            skippedButtons.Add(
                string.IsNullOrEmpty( assetPath )
                    ? buttonPath
                    : $"{assetPath}: {buttonPath}" );

            return false;
        }

        ButtonSpriteStyleRule style =
            FindButtonStyle( button, targetImage.sprite );

        if ( style == null )
            return false;

        RecordObject( button, recordUndo );
        RecordObject( targetImage, recordUndo );

        RectTransform buttonRect =
            button.transform as RectTransform;

        if ( style.SetSize && buttonRect != null )
            RecordObject( buttonRect, recordUndo );

        SpriteState spriteState =
            button.spriteState;

        spriteState.highlightedSprite =
            style.HighlightedSprite;
        spriteState.pressedSprite =
            style.PressedSprite;
        spriteState.selectedSprite =
            style.SelectedSprite != null
                ? style.SelectedSprite
                : style.HighlightedSprite;
        spriteState.disabledSprite =
            style.DisabledSprite;

        button.transition =
            Selectable.Transition.SpriteSwap;
        button.targetGraphic = targetImage;
        button.spriteState = spriteState;

        //Image의 Source Image를 기본 상태 스프라이트로 함께 교체합니다.
        targetImage.overrideSprite = null;
        targetImage.sprite = style.NormalSprite;

        if ( style.SetPixelsPerUnitMultiplier )
        {
            //버튼 Image의 픽셀 배율을 스타일 값으로 통일합니다.
            targetImage.pixelsPerUnitMultiplier =
                style.PixelsPerUnitMultiplier;
        }

        targetImage.SetAllDirty( );

        if ( style.SetSize && buttonRect != null )
        {
            //같은 스타일의 버튼 크기를 지정한 가로와 세로로 통일합니다.
            buttonRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, style.Size.x );
            buttonRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical, style.Size.y );

            EditorUtility.SetDirty( buttonRect );
        }

        EditorUtility.SetDirty( targetImage );
        EditorUtility.SetDirty( button );

        return true;
    }

    /// <summary>
    /// 현재 스프라이트에 대응하는 버튼 스타일 반환
    /// </summary>
    /// <param name="button">판별할 버튼</param>
    /// <param name="sourceSprite">현재 버튼 스프라이트</param>
    /// <returns>일치하는 버튼 스타일</returns>
    ButtonSpriteStyleRule FindButtonStyle (
        Button button, Sprite sourceSprite )
    {
        for ( int i = 0; i < _settings.ButtonStyles.Count; i++ )
        {
            ButtonSpriteStyleRule style =
                _settings.ButtonStyles [ i ];

            if ( style.SourceSprite != sourceSprite ||
                style.SourceSprite == null ||
                style.NormalSprite == null )
            {
                continue;
            }

            //이름 조건이 있으면 버튼 이름까지 일치하는 규칙만 사용합니다.
            if ( string.IsNullOrWhiteSpace( style.NameKeyword ) == false &&
                button.name.IndexOf(
                    style.NameKeyword,
                    System.StringComparison.OrdinalIgnoreCase ) < 0 )
            {
                continue;
            }

            return style;
        }

        return null;
    }

    /// <summary>
    /// 현재 스프라이트와 일치하는 일반 Image 교체 규칙 적용
    /// </summary>
    /// <param name="image">변경할 Image</param>
    /// <param name="recordUndo">실행 취소 기록 여부</param>
    /// <returns>변경 여부</returns>
    bool ApplyImage ( Image image, bool recordUndo )
    {
        for ( int i = 0; i < _settings.ImageRules.Count; i++ )
        {
            ImageSpriteReplaceRule rule =
                _settings.ImageRules [ i ];

            if ( rule.SourceSprite == null ||
                rule.TargetSprite == null ||
                image.sprite != rule.SourceSprite )
            {
                continue;
            }

            RecordObject( image, recordUndo );

            image.sprite = rule.TargetSprite;

            if ( rule.SetSliced )
                image.type = Image.Type.Sliced;

            EditorUtility.SetDirty( image );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 현재 글꼴과 일치하는 TMP 글꼴 교체 규칙 적용
    /// </summary>
    /// <param name="text">변경할 TMP 텍스트</param>
    /// <param name="recordUndo">실행 취소 기록 여부</param>
    /// <returns>변경 여부</returns>
    bool ApplyFont ( TMP_Text text, bool recordUndo )
    {
        //제목 용도로 구분한 텍스트만 글꼴을 교체합니다.
        if ( text.name.IndexOf(
            "Title", System.StringComparison.OrdinalIgnoreCase ) < 0 )
        {
            return false;
        }

        for ( int i = 0; i < _settings.FontRules.Count; i++ )
        {
            TMPFontReplaceRule rule =
                _settings.FontRules [ i ];

            if ( rule.SourceFont == null ||
                rule.TargetFont == null ||
                text.font != rule.SourceFont )
            {
                continue;
            }

            RecordObject( text, recordUndo );

            text.font = rule.TargetFont;

            EditorUtility.SetDirty( text );
            return true;
        }

        return false;
    }

    /// <summary>
    /// 씬 오브젝트 실행 취소 기록
    /// </summary>
    /// <param name="target">변경할 오브젝트</param>
    /// <param name="recordUndo">실행 취소 기록 여부</param>
    void RecordObject (
        UnityEngine.Object target, bool recordUndo )
    {
        if ( recordUndo )
        {
            Undo.RecordObject(
                target, "UI 스타일 일괄 적용" );
        }
    }

    /// <summary>
    /// 오브젝트의 Hierarchy 경로 반환
    /// </summary>
    /// <param name="target">경로를 확인할 Transform</param>
    /// <returns>Hierarchy 경로</returns>
    string GetObjectPath ( Transform target )
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
