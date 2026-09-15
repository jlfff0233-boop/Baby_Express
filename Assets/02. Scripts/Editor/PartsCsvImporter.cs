using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 파츠 CSV를 파츠 에셋과 상품 데이터 맵으로 생성 또는 갱신
/// </summary>
public static class PartsCsvImporter
{
    const string MenuPath =
        "Tools/Baby Express/데이터/파츠 CSV 가져오기";
    const string OutputRoot =
        "Assets/03. Configs/_PurchasableData";
    const string IconRoot =
        "Assets/99. Assets/!_BodyParts";
    const string PartsFileName = "Parts.csv";
    const string SpeciesUnlockFileName = "SpeciesUnlock.csv";

    /// <summary>
    /// CSV 파츠 한 행의 생성 값
    /// </summary>
    class PartValues
    {
        public string Id;
        public string Species;
        public PartType PartType;
        public string Name;
        public ProductType ProductType;
        public float BasePrice;
        public int BaseStockQuantity;
        public int BaseRestockSpan;
        public int CraftCost;
        public List<PartTheme> Themes = new List<PartTheme>( );
        public string Description;
        public string IconKey;
    }

    /// <summary>
    /// 헤더 검증이 끝난 CSV 데이터
    /// </summary>
    class CsvTable
    {
        public Dictionary<string, int> Columns;
        public List<List<string>> Rows;
    }

    /// <summary>
    /// 외부 파츠 CSV를 선택해 파츠 데이터 생성
    /// </summary>
    [MenuItem( MenuPath )]
    static void Import ()
    {
        string sourcePath = EditorUtility.OpenFilePanel(
            "파츠 CSV 선택", string.Empty, "csv" );

        if ( string.IsNullOrEmpty( sourcePath ) ) return;

        try
        {
            List<PartValues> values = ReadParts( sourcePath );
            HashSet<string> initialSpecies =
                ReadInitialSpecies( sourcePath );

            FindDataMaps(
                out PurchasableDataMap dataMap,
                out PurchasableDataMap startMap );

            Dictionary<string, Sprite> sprites = BuildSpriteMap( );
            var generatedDatas = new List<PartsData>( );
            var initialDatas = new List<PartsData>( );
            var missingIcons = new List<string>( );
            int createdCount = 0;
            int updatedCount = 0;

            //배치 편집 전에 종족별 출력 폴더를 한 번만 생성
            EnsureSpeciesFolders( values );

            AssetDatabase.StartAssetEditing( );

            try
            {
                for ( int i = 0; i < values.Count; i++ )
                {
                    PartValues currentValues = values [ i ];
                    PartsData data = CreateOrLoadData(
                        currentValues, ref createdCount, ref updatedCount );

                    Sprite icon = FindSprite(
                        sprites, currentValues.Species,
                        currentValues.PartType, currentValues.IconKey );

                    if ( icon == null )
                        missingIcons.Add( currentValues.Id );

                    UpdateData( data, currentValues, icon );
                    generatedDatas.Add( data );

                    if ( initialSpecies.Contains( currentValues.Species ) )
                        initialDatas.Add( data );
                }

                ReplacePartDatas( dataMap, generatedDatas );
                ReplacePartDatas( startMap, initialDatas );
            }
            finally
            {
                AssetDatabase.StopAssetEditing( );
            }

            AssetDatabase.SaveAssets( );
            AssetDatabase.Refresh( );

            Debug.Log(
                $"파츠 CSV 적용 완료, 생성 {createdCount}개, " +
                $"갱신 {updatedCount}개, 전체 맵 {generatedDatas.Count}개, " +
                $"초기 맵 {initialDatas.Count}개" );

            if ( missingIcons.Count > 0 )
            {
                Debug.LogWarning(
                    "이미지를 찾지 못한 파츠는 기존 아이콘을 유지하거나 " +
                    $"빈 상태로 생성했습니다: {string.Join( ", ", missingIcons )}" );
            }
        }
        catch ( Exception exception )
        {
            Debug.LogError(
                $"파츠 CSV 가져오기 실패: {exception.Message}" );
        }
    }

    /// <summary>
    /// 파츠 CSV 읽기와 행 검증
    /// </summary>
    /// <param name="sourcePath">파츠 CSV 경로</param>
    /// <returns>검증한 파츠 값 목록</returns>
    static List<PartValues> ReadParts ( string sourcePath )
    {
        CsvTable table = ReadCsv(
            sourcePath,
            "Id", "Species", "PartType", "Name", "ProductType",
            "BasePrice", "BaseStockQuantity", "BaseRestockSpan",
            "CraftCost", "TraitThemes", "SpeciesTheme",
            "Description", "IconKey" );

        var values = new List<PartValues>( );
        var ids = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            List<string> row = table.Rows [ i ];
            int rowNumber = i + 2;
            string id = RequiredText(
                table, row, "Id", rowNumber );

            if ( ids.Add( id ) == false )
                throw RowError( rowNumber, $"중복 Id입니다: {id}" );

            ProductType productType = EnumValue<ProductType>(
                table, row, "ProductType", rowNumber );

            if ( productType != ProductType.BabyPart )
            {
                throw RowError(
                    rowNumber,
                    $"파츠 ProductType은 BabyPart여야 합니다: {productType}" );
            }

            var currentValues = new PartValues
            {
                Id = id,
                Species = RequiredText(
                    table, row, "Species", rowNumber ),
                PartType = EnumValue<PartType>(
                    table, row, "PartType", rowNumber ),
                Name = RequiredText(
                    table, row, "Name", rowNumber ),
                ProductType = productType,
                BasePrice = FloatValue(
                    table, row, "BasePrice", 0f, rowNumber ),
                BaseStockQuantity = IntValue(
                    table, row, "BaseStockQuantity", 0, rowNumber ),
                BaseRestockSpan = IntValue(
                    table, row, "BaseRestockSpan", 1, rowNumber ),
                CraftCost = IntValue(
                    table, row, "CraftCost", 0, rowNumber ),
                Description = Text(
                    table, row, "Description" ),
                IconKey = Text(
                    table, row, "IconKey" ),
            };

            AddThemes(
                currentValues.Themes,
                Text( table, row, "TraitThemes" ),
                Text( table, row, "SpeciesTheme" ),
                rowNumber );

            values.Add( currentValues );
        }

        return values;
    }

    /// <summary>
    /// 파츠 CSV 묶음의 초기 해금 종족 읽기
    /// </summary>
    /// <param name="partsPath">파츠 CSV 경로</param>
    /// <returns>초기 해금 종족 이름 집합</returns>
    static HashSet<string> ReadInitialSpecies ( string partsPath )
    {
        string sourceRoot = Directory.GetParent(
            Path.GetDirectoryName( partsPath ) )?.FullName;
        string unlockPath = Path.Combine(
            sourceRoot ?? string.Empty,
            "02_UnlockResearch", SpeciesUnlockFileName );

        if ( File.Exists( unlockPath ) == false )
        {
            unlockPath = Path.Combine(
                Path.GetDirectoryName( partsPath ) ?? string.Empty,
                SpeciesUnlockFileName );
        }

        if ( File.Exists( unlockPath ) == false )
        {
            throw new FileNotFoundException(
                $"초기 상품 맵 생성에 필요한 {SpeciesUnlockFileName}이 없습니다." );
        }

        CsvTable table = ReadCsv(
            unlockPath, "Stage", "Species" );
        var species = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            List<string> row = table.Rows [ i ];

            if ( Text( table, row, "Stage" ).Equals(
                "Initial", StringComparison.OrdinalIgnoreCase ) == false )
            {
                continue;
            }

            species.Add( RequiredText(
                table, row, "Species", i + 2 ) );
        }

        if ( species.Count == 0 )
            throw new FormatException( "Initial 해금 종족이 없습니다." );

        return species;
    }

    /// <summary>
    /// 성질 테마와 종족 테마를 파츠 테마 목록에 추가
    /// </summary>
    /// <param name="themes">추가할 테마 목록</param>
    /// <param name="traitText">성질 테마 문자열</param>
    /// <param name="speciesText">종족 테마 문자열</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    static void AddThemes (
        ICollection<PartTheme> themes,
        string traitText, string speciesText, int rowNumber )
    {
        if ( string.IsNullOrWhiteSpace( traitText ) == false )
        {
            string [ ] traitNames = traitText.Split( '|' );

            for ( int i = 0; i < traitNames.Length; i++ )
            {
                PartTheme theme = ParseTheme(
                    traitNames [ i ], rowNumber );

                if ( theme.IsTraitTheme( ) == false )
                {
                    throw RowError(
                        rowNumber,
                        $"성질 테마가 아닙니다: {traitNames [ i ]}" );
                }

                if ( themes.Contains( theme ) == false )
                    themes.Add( theme );
            }
        }

        PartTheme speciesTheme = ParseTheme(
            speciesText, rowNumber );

        if ( speciesTheme.IsSpeciesTheme( ) == false )
        {
            throw RowError(
                rowNumber,
                $"종족 테마가 아닙니다: {speciesText}" );
        }

        if ( themes.Contains( speciesTheme ) == false )
            themes.Add( speciesTheme );
    }

    /// <summary>
    /// CSV 테마 이름을 파츠 테마로 변환
    /// </summary>
    /// <param name="text">테마 이름</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <returns>변환한 파츠 테마</returns>
    static PartTheme ParseTheme ( string text, int rowNumber )
    {
        if ( Enum.TryParse(
                text.Trim( ), true, out PartTheme theme ) == false ||
            Enum.IsDefined( typeof( PartTheme ), theme ) == false )
        {
            throw RowError(
                rowNumber, $"PartTheme에 없는 값입니다: {text}" );
        }

        return theme;
    }

    /// <summary>
    /// 전체 상품 맵과 새 게임 초기 상품 맵 조회
    /// </summary>
    /// <param name="dataMap">전체 상품 데이터 맵</param>
    /// <param name="startMap">초기 해금 상품 데이터 맵</param>
    static void FindDataMaps (
        out PurchasableDataMap dataMap,
        out PurchasableDataMap startMap )
    {
        string [ ] startSettingGuids =
            AssetDatabase.FindAssets( "t:PlayStartSettingsData" );

        if ( startSettingGuids.Length != 1 )
        {
            throw new InvalidOperationException(
                "PlayStartSettingsData 에셋은 하나여야 합니다." );
        }

        string startSettingPath =
            AssetDatabase.GUIDToAssetPath( startSettingGuids [ 0 ] );
        PlayStartSettingsData startSettings =
            AssetDatabase.LoadAssetAtPath<PlayStartSettingsData>(
                startSettingPath );
        startMap = startSettings.UnlockedProductMap;

        if ( startMap == null )
            throw new InvalidOperationException( "초기 상품 데이터 맵이 없습니다." );

        string [ ] mapGuids =
            AssetDatabase.FindAssets( "t:PurchasableDataMap" );
        var candidates = new List<PurchasableDataMap>( );

        for ( int i = 0; i < mapGuids.Length; i++ )
        {
            PurchasableDataMap currentMap =
                AssetDatabase.LoadAssetAtPath<PurchasableDataMap>(
                    AssetDatabase.GUIDToAssetPath( mapGuids [ i ] ) );

            if ( currentMap != null && currentMap != startMap )
                candidates.Add( currentMap );
        }

        if ( candidates.Count != 1 )
        {
            throw new InvalidOperationException(
                "초기 상품 맵을 제외한 전체 상품 데이터 맵은 하나여야 합니다." );
        }

        dataMap = candidates [ 0 ];
    }

    /// <summary>
    /// 파츠 타입에 맞는 데이터 에셋 생성 또는 조회
    /// </summary>
    /// <param name="values">파츠 생성 값</param>
    /// <param name="createdCount">생성 에셋 수</param>
    /// <param name="updatedCount">갱신 에셋 수</param>
    /// <returns>생성 또는 조회한 파츠 데이터</returns>
    static PartsData CreateOrLoadData (
        PartValues values, ref int createdCount, ref int updatedCount )
    {
        string speciesFolder = $"{OutputRoot}/{values.Species}";
        string assetPath = $"{speciesFolder}/{values.Id}.asset";
        Type dataType = GetDataType( values.PartType );
        PartsData data =
            AssetDatabase.LoadAssetAtPath( assetPath, dataType ) as PartsData;
        UnityEngine.Object existing =
            AssetDatabase.LoadMainAssetAtPath( assetPath );

        if ( data == null && existing != null )
        {
            throw new InvalidOperationException(
                $"같은 경로에 다른 종류의 에셋이 있습니다: {assetPath}" );
        }

        if ( data == null )
        {
            data = ScriptableObject.CreateInstance( dataType ) as PartsData;
            data.name = values.Id;
            AssetDatabase.CreateAsset( data, assetPath );
            createdCount++;
        }
        else
        {
            Undo.RecordObject( data, "파츠 CSV 데이터 갱신" );
            updatedCount++;
        }

        return data;
    }

    /// <summary>
    /// 파츠 데이터 공통 값을 직렬화 필드에 적용
    /// </summary>
    /// <param name="data">갱신할 파츠 데이터</param>
    /// <param name="values">적용할 CSV 값</param>
    /// <param name="icon">찾은 파츠 이미지</param>
    static void UpdateData (
        PartsData data, PartValues values, Sprite icon )
    {
        var serializedData = new SerializedObject( data );

        serializedData.FindProperty( "_productType" ).enumValueIndex =
            ( int ) values.ProductType;
        serializedData.FindProperty( "_name" ).stringValue = values.Name;
        serializedData.FindProperty( "_id" ).stringValue = values.Id;
        serializedData.FindProperty( "_basePrice" ).floatValue =
            values.BasePrice;
        serializedData.FindProperty( "_baseStockQuantity" ).intValue =
            values.BaseStockQuantity;
        serializedData.FindProperty( "_baseRestockSpan" ).intValue =
            values.BaseRestockSpan;
        serializedData.FindProperty( "_disc" ).stringValue =
            values.Description;
        serializedData.FindProperty( "_craftCost" ).intValue =
            values.CraftCost;

        if ( icon != null )
            serializedData.FindProperty( "_icon" ).objectReferenceValue = icon;

        SerializedProperty themes =
            serializedData.FindProperty( "_themes" );
        themes.arraySize = values.Themes.Count;

        for ( int i = 0; i < values.Themes.Count; i++ )
        {
            themes.GetArrayElementAtIndex( i ).enumValueIndex =
                ( int ) values.Themes [ i ];
        }

        serializedData.ApplyModifiedProperties( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// 상품 데이터 맵의 기존 파츠를 생성 파츠 목록으로 교체
    /// </summary>
    /// <param name="dataMap">갱신할 상품 데이터 맵</param>
    /// <param name="partDatas">등록할 파츠 데이터 목록</param>
    static void ReplacePartDatas (
        PurchasableDataMap dataMap,
        IReadOnlyList<PartsData> partDatas )
    {
        var datas = new List<PurchasableData>( );

        if ( dataMap.PurchasableDatas != null )
        {
            for ( int i = 0; i < dataMap.PurchasableDatas.Count; i++ )
            {
                PurchasableData currentData =
                    dataMap.PurchasableDatas [ i ];

                if ( currentData != null &&
                    currentData is not PartsData )
                {
                    datas.Add( currentData );
                }
            }
        }

        for ( int i = 0; i < partDatas.Count; i++ )
            datas.Add( partDatas [ i ] );

        Undo.RecordObject( dataMap, "파츠 상품 데이터 맵 갱신" );

        var serializedMap = new SerializedObject( dataMap );
        SerializedProperty property =
            serializedMap.FindProperty( "_datas" );
        property.arraySize = datas.Count;

        for ( int i = 0; i < datas.Count; i++ )
        {
            property.GetArrayElementAtIndex( i ).objectReferenceValue =
                datas [ i ];
        }

        serializedMap.ApplyModifiedProperties( );
        EditorUtility.SetDirty( dataMap );
    }

    /// <summary>
    /// 파츠 이미지 파일명 검색용 맵 생성
    /// </summary>
    /// <returns>정규화 파일명별 파츠 이미지</returns>
    static Dictionary<string, Sprite> BuildSpriteMap ()
    {
        string [ ] guids =
            AssetDatabase.FindAssets( "t:Sprite", new [ ] { IconRoot } );
        var sprites = new Dictionary<string, Sprite>(
            StringComparer.OrdinalIgnoreCase );

        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath( guids [ i ] );
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>( path );

            if ( sprite == null ) continue;

            string key = NormalizeName(
                Path.GetFileNameWithoutExtension( path ) );

            if ( sprites.ContainsKey( key ) == false )
                sprites.Add( key, sprite );
        }

        return sprites;
    }

    /// <summary>
    /// 종족과 파츠 타입으로 대응 이미지 조회
    /// </summary>
    /// <param name="sprites">파일명별 이미지 맵</param>
    /// <param name="species">파츠 종족</param>
    /// <param name="partType">파츠 타입</param>
    /// <param name="iconKey">CSV 아이콘 키</param>
    /// <returns>찾은 이미지 또는 null</returns>
    static Sprite FindSprite (
        IReadOnlyDictionary<string, Sprite> sprites,
        string species, PartType partType, string iconKey )
    {
        var candidates = new List<string> { iconKey };
        string [ ] speciesNames = GetSpeciesFileNames( species );
        string [ ] partNames = GetPartFileNames( partType );

        for ( int i = 0; i < speciesNames.Length; i++ )
        {
            for ( int j = 0; j < partNames.Length; j++ )
                candidates.Add( speciesNames [ i ] + partNames [ j ] );
        }

        for ( int i = 0; i < candidates.Count; i++ )
        {
            if ( sprites.TryGetValue(
                NormalizeName( candidates [ i ] ), out Sprite sprite ) )
            {
                return sprite;
            }
        }

        return null;
    }

    /// <summary>
    /// CSV 종족 이름에 대응하는 이미지 파일명 후보 반환
    /// </summary>
    /// <param name="species">CSV 종족 이름</param>
    /// <returns>이미지 파일명 종족 후보</returns>
    static string [ ] GetSpeciesFileNames ( string species )
    {
        switch ( species )
        {
            case "Mushroom":
                return new [ ] { "Mushroom", "Fungus" };

            case "Mandragora":
                return new [ ] { "Mandragora", "Mandragdra" };

            default:
                return new [ ] { species };
        }
    }

    /// <summary>
    /// 파츠 타입에 대응하는 이미지 파일명 후보 반환
    /// </summary>
    /// <param name="partType">파츠 타입</param>
    /// <returns>이미지 파일명 파츠 후보</returns>
    static string [ ] GetPartFileNames ( PartType partType )
    {
        switch ( partType )
        {
            case PartType.Arms:
                return new [ ] { "Arms", "Arm" };

            case PartType.Legs:
                return new [ ] { "Legs", "Leg" };

            case PartType.Wings:
                return new [ ] { "Wings", "Wing" };

            case PartType.Claws:
                return new [ ] { "Claws", "Claw" };

            case PartType.Horns:
                return new [ ] { "Horns", "Horn" };

            case PartType.Nose:
                return new [ ] { "Nose", "Mose" };

            default:
                return new [ ] { partType.ToString( ) };
        }
    }

    /// <summary>
    /// 비교할 파일명에서 구분 문자 제거
    /// </summary>
    /// <param name="name">원본 파일명</param>
    /// <returns>영문과 숫자만 남긴 소문자 이름</returns>
    static string NormalizeName ( string name )
    {
        if ( string.IsNullOrWhiteSpace( name ) ) return string.Empty;

        var builder = new StringBuilder( );

        for ( int i = 0; i < name.Length; i++ )
        {
            if ( char.IsLetterOrDigit( name [ i ] ) )
                builder.Append( char.ToLowerInvariant( name [ i ] ) );
        }

        return builder.ToString( );
    }

    /// <summary>
    /// 파츠 타입에 대응하는 데이터 클래스 반환
    /// </summary>
    /// <param name="partType">파츠 타입</param>
    /// <returns>생성할 ScriptableObject 클래스</returns>
    static Type GetDataType ( PartType partType )
    {
        switch ( partType )
        {
            case PartType.Body: return typeof( BodyData );
            case PartType.Eye: return typeof( EyeData );
            case PartType.Nose: return typeof( NoseData );
            case PartType.Mouth: return typeof( MouthData );
            case PartType.Arms: return typeof( ArmData );
            case PartType.Legs: return typeof( LegData );
            case PartType.Hair: return typeof( HairData );
            case PartType.Wings: return typeof( WingData );
            case PartType.Tail: return typeof( TailData );
            case PartType.Claws: return typeof( ClawData );
            case PartType.Horns: return typeof( HornData );
            default:
                throw new ArgumentOutOfRangeException(
                    nameof( partType ), partType,
                    "지원하지 않는 파츠 타입입니다." );
        }
    }

    /// <summary>
    /// CSV에 포함된 종족별 출력 폴더를 중복 없이 준비
    /// </summary>
    /// <param name="values">전체 파츠 생성 값</param>
    static void EnsureSpeciesFolders ( IReadOnlyList<PartValues> values )
    {
        var speciesNames = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < values.Count; i++ )
        {
            if ( speciesNames.Add( values [ i ].Species ) == false )
                continue;

            EnsureFolder( $"{OutputRoot}/{values [ i ].Species}" );
        }
    }

    /// <summary>
    /// 에셋 출력 폴더가 없으면 단계별 생성
    /// </summary>
    /// <param name="folderPath">생성할 폴더 경로</param>
    static void EnsureFolder ( string folderPath )
    {
        if ( AssetDatabase.IsValidFolder( folderPath ) ) return;

        string parent = Path.GetDirectoryName( folderPath )
            ?.Replace( '\\', '/' );
        string folderName = Path.GetFileName( folderPath );

        if ( string.IsNullOrEmpty( parent ) ||
            AssetDatabase.IsValidFolder( parent ) == false )
        {
            throw new DirectoryNotFoundException(
                $"파츠 에셋 상위 폴더가 없습니다: {parent}" );
        }

        AssetDatabase.CreateFolder( parent, folderName );
    }

    /// <summary>
    /// CSV 파일 읽기와 필수 헤더 검증
    /// </summary>
    /// <param name="path">CSV 파일 경로</param>
    /// <param name="headers">필수 헤더 목록</param>
    /// <returns>헤더와 데이터 행</returns>
    static CsvTable ReadCsv ( string path, params string [ ] headers )
    {
        if ( File.Exists( path ) == false )
            throw new FileNotFoundException( $"CSV 파일이 없습니다: {path}" );

        List<List<string>> rows = ParseCsv(
            File.ReadAllText( path, Encoding.UTF8 ) );

        if ( rows.Count < 2 )
            throw new FormatException( $"CSV 데이터 행이 없습니다: {path}" );

        var columns = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase );

        for ( int i = 0; i < rows [ 0 ].Count; i++ )
        {
            string header = rows [ 0 ] [ i ]
                .Trim( ).TrimStart( '\uFEFF' );

            if ( string.IsNullOrEmpty( header ) ||
                columns.TryAdd( header, i ) == false )
            {
                throw new FormatException(
                    $"CSV 헤더가 비어 있거나 중복됩니다: {header}" );
            }
        }

        for ( int i = 0; i < headers.Length; i++ )
        {
            if ( columns.ContainsKey( headers [ i ] ) == false )
                throw new FormatException(
                    $"CSV 필수 헤더가 없습니다: {headers [ i ]}" );
        }

        rows.RemoveAt( 0 );
        rows.RemoveAll( IsEmptyRow );

        return new CsvTable
        {
            Columns = columns,
            Rows = rows,
        };
    }

    /// <summary>
    /// 따옴표와 줄바꿈을 포함한 CSV 원문 파싱
    /// </summary>
    /// <param name="text">CSV 원문</param>
    /// <returns>행과 열 목록</returns>
    static List<List<string>> ParseCsv ( string text )
    {
        var rows = new List<List<string>>( );
        var row = new List<string>( );
        var field = new StringBuilder( );
        bool inQuotes = false;

        for ( int i = 0; i < text.Length; i++ )
        {
            char current = text [ i ];

            if ( current == '"' )
            {
                if ( inQuotes && i + 1 < text.Length &&
                    text [ i + 1 ] == '"' )
                {
                    field.Append( '"' );
                    i++;
                    continue;
                }

                inQuotes = inQuotes == false;
                continue;
            }

            if ( current == ',' && inQuotes == false )
            {
                row.Add( field.ToString( ) );
                field.Clear( );
                continue;
            }

            if ( ( current == '\r' || current == '\n' ) &&
                inQuotes == false )
            {
                row.Add( field.ToString( ) );
                field.Clear( );
                rows.Add( row );
                row = new List<string>( );

                if ( current == '\r' && i + 1 < text.Length &&
                    text [ i + 1 ] == '\n' )
                {
                    i++;
                }

                continue;
            }

            field.Append( current );
        }

        if ( inQuotes )
            throw new FormatException( "CSV에 닫히지 않은 따옴표가 있습니다." );

        if ( field.Length > 0 || row.Count > 0 )
        {
            row.Add( field.ToString( ) );
            rows.Add( row );
        }

        return rows;
    }

    /// <summary>
    /// CSV 행이 모두 빈 값인지 확인
    /// </summary>
    /// <param name="row">확인할 CSV 행</param>
    /// <returns>빈 행 여부</returns>
    static bool IsEmptyRow ( List<string> row )
    {
        for ( int i = 0; i < row.Count; i++ )
        {
            if ( string.IsNullOrWhiteSpace( row [ i ] ) == false )
                return false;
        }

        return true;
    }

    /// <summary>
    /// CSV 행의 문자열 값 반환
    /// </summary>
    /// <param name="table">CSV 테이블</param>
    /// <param name="row">CSV 행</param>
    /// <param name="header">조회할 헤더</param>
    /// <returns>공백을 정리한 문자열</returns>
    static string Text (
        CsvTable table, IReadOnlyList<string> row, string header )
    {
        int index = table.Columns [ header ];
        return index < row.Count ? row [ index ].Trim( ) : string.Empty;
    }

    /// <summary>
    /// CSV 행의 필수 문자열 값 반환
    /// </summary>
    /// <param name="table">CSV 테이블</param>
    /// <param name="row">CSV 행</param>
    /// <param name="header">조회할 헤더</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <returns>비어 있지 않은 문자열</returns>
    static string RequiredText (
        CsvTable table, IReadOnlyList<string> row,
        string header, int rowNumber )
    {
        string value = Text( table, row, header );

        if ( string.IsNullOrEmpty( value ) )
            throw RowError( rowNumber, $"{header} 값이 비어 있습니다." );

        return value;
    }

    /// <summary>
    /// CSV 행의 정수 값 반환
    /// </summary>
    /// <param name="table">CSV 테이블</param>
    /// <param name="row">CSV 행</param>
    /// <param name="header">조회할 헤더</param>
    /// <param name="minimum">허용 최솟값</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <returns>검증한 정수</returns>
    static int IntValue (
        CsvTable table, IReadOnlyList<string> row,
        string header, int minimum, int rowNumber )
    {
        string text = Text( table, row, header );

        if ( int.TryParse(
                text, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int value ) == false ||
            value < minimum )
        {
            throw RowError(
                rowNumber,
                $"{header} 값은 {minimum} 이상의 정수여야 합니다: {text}" );
        }

        return value;
    }

    /// <summary>
    /// CSV 행의 실수 값 반환
    /// </summary>
    /// <param name="table">CSV 테이블</param>
    /// <param name="row">CSV 행</param>
    /// <param name="header">조회할 헤더</param>
    /// <param name="minimum">허용 최솟값</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <returns>검증한 실수</returns>
    static float FloatValue (
        CsvTable table, IReadOnlyList<string> row,
        string header, float minimum, int rowNumber )
    {
        string text = Text( table, row, header );

        if ( float.TryParse(
                text, NumberStyles.Float,
                CultureInfo.InvariantCulture, out float value ) == false ||
            value < minimum )
        {
            throw RowError(
                rowNumber,
                $"{header} 값은 {minimum} 이상의 숫자여야 합니다: {text}" );
        }

        return value;
    }

    /// <summary>
    /// CSV 행의 enum 값 반환
    /// </summary>
    /// <typeparam name="T">변환할 enum 타입</typeparam>
    /// <param name="table">CSV 테이블</param>
    /// <param name="row">CSV 행</param>
    /// <param name="header">조회할 헤더</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <returns>검증한 enum 값</returns>
    static T EnumValue<T> (
        CsvTable table, IReadOnlyList<string> row,
        string header, int rowNumber ) where T : struct, Enum
    {
        string text = Text( table, row, header );

        if ( Enum.TryParse( text, true, out T value ) == false ||
            Enum.IsDefined( typeof( T ), value ) == false )
        {
            throw RowError(
                rowNumber,
                $"{header} 값이 {typeof( T ).Name}에 없습니다: {text}" );
        }

        return value;
    }

    /// <summary>
    /// CSV 행 번호를 포함한 형식 오류 생성
    /// </summary>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <param name="message">오류 내용</param>
    /// <returns>CSV 형식 오류</returns>
    static FormatException RowError (
        int rowNumber, string message )
    {
        return new FormatException(
            $"{PartsFileName} {rowNumber}행: {message}" );
    }
}
