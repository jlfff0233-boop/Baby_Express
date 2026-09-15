using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Day 1 튜토리얼 CSV를 데이터 에셋으로 생성 또는 갱신
/// </summary>
public static class TutorialCsvImporter
{
    const string Day1MenuPath =
        "Tools/Baby Express/데이터/Day 1 튜토리얼 CSV 가져오기";
    const string GuideMenuPath =
        "Tools/Baby Express/데이터/Day 2~7 튜토리얼 대화 CSV 가져오기";
    const string OrderFileName = "TutorialOrder.csv";
    const string ConditionFileName = "TutorialConditions.csv";
    const string PurchaseGuideFileName = "TutorialPurchaseGuide.csv";
    const string DialogueFileName = "TutorialDialogue.csv";

    /// <summary>
    /// 튜토리얼 주문 CSV 값
    /// </summary>
    class OrderValues
    {
        public string Id;
        public string Title;
        public OrderDifficulty Difficulty;
        public int AcceptDays;
        public int DeliveryDays;
        public int DelayDays;
        public int MaxCraftCost;
        public OrderSpecialType SpecialType;
        public int ActualTravelDays;
    }

    /// <summary>
    /// 튜토리얼 주문 조건 CSV 값
    /// </summary>
    class ConditionValues
    {
        public TutorialOrderConditionType Type;
        public string PartId;
        public int Quantity;
    }

    /// <summary>
    /// 튜토리얼 구매 가이드 CSV 값
    /// </summary>
    class PurchaseGuideValues
    {
        public int Sequence;
        public string PartId;
        public bool RequiredForProgress;
        public string Note;
    }

    /// <summary>
    /// 튜토리얼 대화 한 줄 CSV 값
    /// </summary>
    class DialogueLineValues
    {
        public int Index;
        public DialogueLineType Type;
        public string Speaker;
        public string Content;
        public DialoguePortraitSide PortraitSide;
        public string SignalId;
    }

    /// <summary>
    /// 튜토리얼 대화 CSV 묶음
    /// </summary>
    class DialogueValues
    {
        public string Id;
        public List<DialogueLineValues> Lines =
            new List<DialogueLineValues>( );
    }

    /// <summary>
    /// CSV 한 행의 이름 기반 값 조회
    /// </summary>
    class CsvRow
    {
        readonly string _fileName;
        readonly int _rowNumber;
        readonly IReadOnlyDictionary<string, int> _columns;
        readonly IReadOnlyList<string> _values;

        public CsvRow (
            string fileName, int rowNumber,
            IReadOnlyDictionary<string, int> columns,
            IReadOnlyList<string> values )
        {
            _fileName = fileName;
            _rowNumber = rowNumber;
            _columns = columns;
            _values = values;
        }

        /// <summary>
        /// 빈 값을 허용하는 문자열 반환
        /// </summary>
        public string Text ( string header )
        {
            int index = _columns [ header ];
            return index < _values.Count ? _values [ index ].Trim( ) : string.Empty;
        }

        /// <summary>
        /// 필수 문자열 반환
        /// </summary>
        public string RequiredText ( string header )
        {
            string value = Text( header );

            if ( string.IsNullOrEmpty( value ) )
                throw Error( $"{header} 값이 비어 있습니다." );

            return value;
        }

        /// <summary>
        /// 지정 최솟값 이상의 정수 반환
        /// </summary>
        public int Int ( string header, int minimum )
        {
            string text = Text( header );

            if ( int.TryParse( text, out int value ) == false ||
                value < minimum )
            {
                throw Error(
                    $"{header} 값은 {minimum} 이상의 정수여야 합니다: {text}" );
            }

            return value;
        }

        /// <summary>
        /// bool 값 반환
        /// </summary>
        public bool Bool ( string header )
        {
            string text = Text( header );

            if ( bool.TryParse( text, out bool value ) == false )
                throw Error( $"{header} 값은 True 또는 False여야 합니다: {text}" );

            return value;
        }

        /// <summary>
        /// enum 값 반환
        /// </summary>
        public T EnumValue<T> ( string header ) where T : struct, Enum
        {
            string text = Text( header );

            if ( Enum.TryParse( text, true, out T value ) == false ||
                Enum.IsDefined( typeof( T ), value ) == false )
            {
                throw Error(
                    $"{header} 값이 {typeof( T ).Name}에 없습니다: {text}" );
            }

            return value;
        }

        /// <summary>
        /// 현재 행 위치를 포함한 CSV 오류 생성
        /// </summary>
        public FormatException Error ( string message )
        {
            return new FormatException(
                $"{_fileName} {_rowNumber}행: {message}" );
        }
    }

    /// <summary>
    /// 헤더 검증이 끝난 CSV 데이터
    /// </summary>
    class CsvTable
    {
        public List<CsvRow> Rows = new List<CsvRow>( );

        /// <summary>
        /// 지정 CSV 읽기와 필수 헤더 검증
        /// </summary>
        public static CsvTable Read (
            string sourceFolder, string fileName, params string [ ] headers )
        {
            string path = Path.Combine( sourceFolder, fileName );

            if ( File.Exists( path ) == false )
                throw new FileNotFoundException( $"CSV 파일이 없습니다: {fileName}" );

            List<List<string>> sourceRows = ParseCsv( ReadCsvText( path ) );

            if ( sourceRows.Count < 2 )
                throw new FormatException( $"{fileName}에 데이터 행이 없습니다." );

            var columns = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase );

            for ( int i = 0; i < sourceRows [ 0 ].Count; i++ )
            {
                string header = sourceRows [ 0 ] [ i ]
                    .Trim( ).TrimStart( '\uFEFF' );

                if ( string.IsNullOrEmpty( header ) ||
                    columns.TryAdd( header, i ) == false )
                {
                    throw new FormatException(
                        $"{fileName}의 헤더가 비어 있거나 중복됩니다: {header}" );
                }
            }

            for ( int i = 0; i < headers.Length; i++ )
            {
                if ( columns.ContainsKey( headers [ i ] ) == false )
                    throw new FormatException(
                        $"{fileName}에 필수 헤더가 없습니다: {headers [ i ]}" );
            }

            var table = new CsvTable( );

            for ( int i = 1; i < sourceRows.Count; i++ )
            {
                if ( IsEmptyRow( sourceRows [ i ] ) ) continue;

                table.Rows.Add( new CsvRow(
                    fileName, i + 1, columns, sourceRows [ i ] ) );
            }

            return table;
        }

        /// <summary>
        /// 외부 편집기가 열어 둔 CSV를 공유 읽기 방식으로 불러오기
        /// </summary>
        /// <param name="path">읽을 CSV 파일 경로</param>
        /// <returns>CSV 전체 문자열</returns>
        static string ReadCsvText ( string path )
        {
            using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete );
            using var reader = new StreamReader(
                stream, Encoding.UTF8, true );

            return reader.ReadToEnd( );
        }

        /// <summary>
        /// 따옴표와 줄바꿈을 포함한 CSV 원문 파싱
        /// </summary>
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
                    if ( inQuotes && i + 1 < text.Length && text [ i + 1 ] == '"' )
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

                    if ( current == '\r' &&
                        i + 1 < text.Length && text [ i + 1 ] == '\n' )
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
        /// CSV 빈 행 여부 확인
        /// </summary>
        static bool IsEmptyRow ( IReadOnlyList<string> row )
        {
            for ( int i = 0; i < row.Count; i++ )
            {
                if ( string.IsNullOrWhiteSpace( row [ i ] ) == false )
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 외부 CSV 폴더를 선택해 튜토리얼 데이터 생성
    /// </summary>
    [MenuItem( Day1MenuPath )]
    static void ImportDay1 ()
    {
        string sourceFolder = EditorUtility.OpenFolderPanel(
            "Day 1 튜토리얼 CSV 폴더 선택", string.Empty, string.Empty );

        if ( string.IsNullOrEmpty( sourceFolder ) ) return;

        try
        {
            OrderValues order = ReadOrder( sourceFolder );
            List<ConditionValues> conditions =
                ReadConditions( sourceFolder, order.Id );
            List<PurchaseGuideValues> purchaseGuides =
                ReadPurchaseGuides( sourceFolder );
            List<DialogueValues> dialogues =
                ReadDialogues( sourceFolder, DialogueFileName );

            string dataAssetPath = EditorUtility.SaveFilePanelInProject(
                "Day 1 튜토리얼 데이터 저장",
                "TutorialDay1Data",
                "asset",
                "대화 데이터도 선택한 폴더에 함께 생성됩니다." );

            if ( string.IsNullOrEmpty( dataAssetPath ) ) return;

            string outputFolder = Path.GetDirectoryName( dataAssetPath )
                ?.Replace( '\\', '/' );

            if ( string.IsNullOrEmpty( outputFolder ) ||
                AssetDatabase.IsValidFolder( outputFolder ) == false )
            {
                throw new DirectoryNotFoundException(
                    $"저장 폴더가 없습니다: {outputFolder}" );
            }

            CreateAssets(
                dataAssetPath, outputFolder, order,
                conditions, purchaseGuides, dialogues,
                out int createdCount, out int updatedCount );

            WarnMissingParts( conditions, purchaseGuides );

            Debug.Log(
                $"튜토리얼 CSV 적용 완료, 생성 {createdCount}개, " +
                $"갱신 {updatedCount}개, 대화 {dialogues.Count}개" );
        }
        catch ( Exception exception )
        {
            Debug.LogError( $"튜토리얼 CSV 가져오기 실패: {exception.Message}" );
        }
    }

    /// <summary>
    /// Day 2~7 대화 CSV를 선택해 후속 가이드 데이터 생성
    /// </summary>
    [MenuItem( GuideMenuPath )]
    static void ImportGuides ()
    {
        string sourcePath = EditorUtility.OpenFilePanel(
            "Day 2~7 튜토리얼 대화 CSV 선택",
            string.Empty, "csv" );

        if ( string.IsNullOrEmpty( sourcePath ) ) return;

        try
        {
            string sourceFolder = Path.GetDirectoryName( sourcePath );
            string sourceFileName = Path.GetFileName( sourcePath );
            List<DialogueValues> dialogues =
                ReadDialogues( sourceFolder, sourceFileName );

            string dataAssetPath = EditorUtility.SaveFilePanelInProject(
                "Day 2~7 튜토리얼 가이드 데이터 저장",
                "TutorialGuideData",
                "asset",
                "대화 데이터도 선택한 폴더에 함께 생성됩니다." );

            if ( string.IsNullOrEmpty( dataAssetPath ) ) return;

            string outputFolder = GetOutputFolder( dataAssetPath );

            CreateGuideAssets(
                dataAssetPath, outputFolder, dialogues,
                out int createdCount, out int updatedCount );

            Debug.Log(
                $"Day 2~7 튜토리얼 CSV 적용 완료, 생성 {createdCount}개, " +
                $"갱신 {updatedCount}개, 대화 {dialogues.Count}개" );
        }
        catch ( Exception exception )
        {
            Debug.LogError(
                $"Day 2~7 튜토리얼 CSV 가져오기 실패: {exception.Message}" );
        }
    }

    /// <summary>
    /// 저장할 데이터 에셋 경로에서 유효한 출력 폴더 반환
    /// </summary>
    /// <param name="dataAssetPath">저장할 데이터 에셋 경로</param>
    /// <returns>대화 에셋 출력 폴더</returns>
    static string GetOutputFolder ( string dataAssetPath )
    {
        string outputFolder = Path.GetDirectoryName( dataAssetPath )
            ?.Replace( '\\', '/' );

        if ( string.IsNullOrEmpty( outputFolder ) ||
            AssetDatabase.IsValidFolder( outputFolder ) == false )
        {
            throw new DirectoryNotFoundException(
                $"저장 폴더가 없습니다: {outputFolder}" );
        }

        return outputFolder;
    }

    #region ----- CSV 데이터 구성 -----

    /// <summary>
    /// Day 1 고정 주문 값 읽기
    /// </summary>
    static OrderValues ReadOrder ( string sourceFolder )
    {
        CsvTable table = CsvTable.Read(
            sourceFolder, OrderFileName,
            "Id", "Title", "Difficulty", "AcceptDays",
            "DeliveryDays", "DelayDays", "MaxCraftCost",
            "SpecialCondition", "ActualTravelDays" );

        if ( table.Rows.Count != 1 )
            throw new FormatException(
                $"{OrderFileName}에는 주문 한 건만 있어야 합니다." );

        CsvRow row = table.Rows [ 0 ];

        return new OrderValues
        {
            Id = row.RequiredText( "Id" ),
            Title = row.RequiredText( "Title" ),
            Difficulty = row.EnumValue<OrderDifficulty>( "Difficulty" ),
            AcceptDays = row.Int( "AcceptDays", 0 ),
            DeliveryDays = row.Int( "DeliveryDays", 0 ),
            DelayDays = row.Int( "DelayDays", 0 ),
            MaxCraftCost = row.Int( "MaxCraftCost", 0 ),
            SpecialType = row.EnumValue<OrderSpecialType>( "SpecialCondition" ),
            ActualTravelDays = row.Int( "ActualTravelDays", 0 ),
        };
    }

    /// <summary>
    /// Day 1 주문 조건 값 읽기
    /// </summary>
    static List<ConditionValues> ReadConditions (
        string sourceFolder, string orderId )
    {
        CsvTable table = CsvTable.Read(
            sourceFolder, ConditionFileName,
            "OrderId", "ConditionType", "PartId", "Quantity" );
        var conditions = new List<ConditionValues>( );
        var keys = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            CsvRow row = table.Rows [ i ];
            string currentOrderId = row.RequiredText( "OrderId" );
            TutorialOrderConditionType type =
                row.EnumValue<TutorialOrderConditionType>( "ConditionType" );
            string partId = row.RequiredText( "PartId" );

            if ( currentOrderId != orderId )
                throw row.Error(
                    $"주문 아이디가 고정 주문과 다릅니다: {currentOrderId}" );

            if ( keys.Add( $"{type}:{partId}" ) == false )
                throw row.Error( $"주문 조건이 중복됩니다: {type}:{partId}" );

            conditions.Add( new ConditionValues
            {
                Type = type,
                PartId = partId,
                Quantity = row.Int( "Quantity", 1 ),
            } );
        }

        return conditions;
    }

    /// <summary>
    /// Day 1 구매 가이드 값 읽기
    /// </summary>
    static List<PurchaseGuideValues> ReadPurchaseGuides (
        string sourceFolder )
    {
        CsvTable table = CsvTable.Read(
            sourceFolder, PurchaseGuideFileName,
            "Sequence", "PartId", "RequiredForProgress", "Note" );
        var guides = new List<PurchaseGuideValues>( );
        var sequences = new HashSet<int>( );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            CsvRow row = table.Rows [ i ];
            int sequence = row.Int( "Sequence", 0 );

            if ( sequences.Add( sequence ) == false )
                throw row.Error( $"진행 순서가 중복됩니다: {sequence}" );

            guides.Add( new PurchaseGuideValues
            {
                Sequence = sequence,
                PartId = row.RequiredText( "PartId" ),
                RequiredForProgress = row.Bool( "RequiredForProgress" ),
                Note = row.Text( "Note" ),
            } );
        }

        guides.Sort( ( left, right ) =>
            left.Sequence.CompareTo( right.Sequence ) );

        for ( int i = 0; i < guides.Count; i++ )
        {
            if ( guides [ i ].Sequence != i )
                throw new FormatException(
                    $"{PurchaseGuideFileName}의 진행 순서는 0부터 연속되어야 합니다." );
        }

        return guides;
    }

    /// <summary>
    /// 튜토리얼 대화 값 읽기
    /// </summary>
    /// <param name="sourceFolder">CSV 원본 폴더</param>
    /// <param name="fileName">대화 CSV 파일명</param>
    /// <returns>대화 아이디별 CSV 값</returns>
    static List<DialogueValues> ReadDialogues (
        string sourceFolder, string fileName )
    {
        CsvTable table = CsvTable.Read(
            sourceFolder, fileName,
            "DialogueId", "LineIndex", "LineType", "Speaker",
            "Content", "PortraitSide", "SignalId" );
        var dialogues = new List<DialogueValues>( );
        var dialogueMap = new Dictionary<string, DialogueValues>(
            StringComparer.Ordinal );
        var lineKeys = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            CsvRow row = table.Rows [ i ];
            string id = row.RequiredText( "DialogueId" );
            int index = row.Int( "LineIndex", 0 );
            DialogueLineType type =
                row.EnumValue<DialogueLineType>( "LineType" );
            string content = row.Text( "Content" );
            string signalId = row.Text( "SignalId" );

            if ( type == DialogueLineType.Speech &&
                string.IsNullOrEmpty( content ) )
            {
                throw row.Error( "대사가 비어 있습니다." );
            }

            if ( type == DialogueLineType.Signal &&
                string.IsNullOrEmpty( signalId ) )
            {
                throw row.Error( "연출 신호가 비어 있습니다." );
            }

            if ( lineKeys.Add( $"{id}:{index}" ) == false )
                throw row.Error( $"대화 순번이 중복됩니다: {id}:{index}" );

            if ( dialogueMap.TryGetValue(
                id, out DialogueValues dialogue ) == false )
            {
                dialogue = new DialogueValues { Id = id };
                dialogueMap.Add( id, dialogue );
                dialogues.Add( dialogue );
            }

            dialogue.Lines.Add( new DialogueLineValues
            {
                Index = index,
                Type = type,
                Speaker = row.Text( "Speaker" ),
                Content = content,
                PortraitSide =
                    row.EnumValue<DialoguePortraitSide>( "PortraitSide" ),
                SignalId = signalId,
            } );
        }

        for ( int i = 0; i < dialogues.Count; i++ )
        {
            DialogueValues dialogue = dialogues [ i ];
            dialogue.Lines.Sort( ( left, right ) =>
                left.Index.CompareTo( right.Index ) );

            for ( int j = 0; j < dialogue.Lines.Count; j++ )
            {
                if ( dialogue.Lines [ j ].Index != j )
                    throw new FormatException(
                        $"{fileName}의 {dialogue.Id} 순번은 " +
                        "0부터 연속되어야 합니다." );
            }
        }

        return dialogues;
    }

    #endregion

    #region ----- 에셋 생성 -----

    /// <summary>
    /// 튜토리얼과 대화 에셋 생성 또는 갱신
    /// </summary>
    static void CreateAssets (
        string dataAssetPath, string outputFolder,
        OrderValues order, IReadOnlyList<ConditionValues> conditions,
        IReadOnlyList<PurchaseGuideValues> guides,
        IReadOnlyList<DialogueValues> dialogues,
        out int createdCount, out int updatedCount )
    {
        createdCount = 0;
        updatedCount = 0;

        AssetDatabase.StartAssetEditing( );

        try
        {
            List<DialogueData> dialogueAssets = CreateDialogueAssets(
                outputFolder, dialogues,
                ref createdCount, ref updatedCount );

            TutorialDay1Data tutorialData =
                GetOrCreateAsset<TutorialDay1Data>(
                    dataAssetPath, "TutorialDay1Data",
                    ref createdCount, ref updatedCount );

            UpdateTutorialData(
                tutorialData, order, conditions,
                guides, dialogueAssets );
        }
        finally
        {
            AssetDatabase.StopAssetEditing( );
        }

        AssetDatabase.SaveAssets( );
        AssetDatabase.Refresh( );
    }

    /// <summary>
    /// Day 2~7 후속 가이드와 대화 에셋 생성 또는 갱신
    /// </summary>
    /// <param name="dataAssetPath">가이드 데이터 에셋 저장 경로</param>
    /// <param name="outputFolder">대화 에셋 저장 폴더</param>
    /// <param name="dialogues">CSV 대화 목록</param>
    /// <param name="createdCount">신규 생성 에셋 수</param>
    /// <param name="updatedCount">갱신 에셋 수</param>
    static void CreateGuideAssets (
        string dataAssetPath, string outputFolder,
        IReadOnlyList<DialogueValues> dialogues,
        out int createdCount, out int updatedCount )
    {
        createdCount = 0;
        updatedCount = 0;

        AssetDatabase.StartAssetEditing( );

        try
        {
            List<DialogueData> dialogueAssets = CreateDialogueAssets(
                outputFolder, dialogues,
                ref createdCount, ref updatedCount );

            TutorialGuideData guideData =
                GetOrCreateAsset<TutorialGuideData>(
                    dataAssetPath, "TutorialGuideData",
                    ref createdCount, ref updatedCount );

            UpdateGuideData( guideData, dialogueAssets );
        }
        finally
        {
            AssetDatabase.StopAssetEditing( );
        }

        AssetDatabase.SaveAssets( );
        AssetDatabase.Refresh( );
    }

    /// <summary>
    /// 대화 목록을 개별 에셋으로 생성 또는 갱신
    /// </summary>
    /// <param name="outputFolder">대화 에셋 저장 폴더</param>
    /// <param name="dialogues">CSV 대화 목록</param>
    /// <param name="createdCount">누적 신규 생성 에셋 수</param>
    /// <param name="updatedCount">누적 갱신 에셋 수</param>
    /// <returns>생성하거나 갱신한 대화 에셋 목록</returns>
    static List<DialogueData> CreateDialogueAssets (
        string outputFolder,
        IReadOnlyList<DialogueValues> dialogues,
        ref int createdCount, ref int updatedCount )
    {
        var dialogueAssets = new List<DialogueData>( dialogues.Count );

        for ( int i = 0; i < dialogues.Count; i++ )
        {
            DialogueValues values = dialogues [ i ];
            string assetPath = $"{outputFolder}/{values.Id}.asset";
            DialogueData dialogue = GetOrCreateAsset<DialogueData>(
                assetPath, values.Id,
                ref createdCount, ref updatedCount );

            UpdateDialogue( dialogue, values );
            dialogueAssets.Add( dialogue );
        }

        return dialogueAssets;
    }

    /// <summary>
    /// 지정 경로의 데이터 에셋 반환 또는 신규 생성
    /// </summary>
    static T GetOrCreateAsset<T> (
        string assetPath, string assetName,
        ref int createdCount, ref int updatedCount )
        where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );

        if ( asset != null )
        {
            Undo.RecordObject( asset, "튜토리얼 CSV 데이터 갱신" );
            updatedCount++;
            return asset;
        }

        if ( AssetDatabase.LoadMainAssetAtPath( assetPath ) != null )
            throw new InvalidOperationException(
                $"같은 경로에 다른 종류의 에셋이 있습니다: {assetPath}" );

        asset = ScriptableObject.CreateInstance<T>( );
        asset.name = assetName;
        AssetDatabase.CreateAsset( asset, assetPath );
        createdCount++;
        return asset;
    }

    /// <summary>
    /// Day 1 튜토리얼 데이터에 CSV 값 적용
    /// </summary>
    static void UpdateTutorialData (
        TutorialDay1Data data, OrderValues order,
        IReadOnlyList<ConditionValues> conditions,
        IReadOnlyList<PurchaseGuideValues> guides,
        IReadOnlyList<DialogueData> dialogues )
    {
        var serializedData = new SerializedObject( data );

        serializedData.FindProperty( "_orderId" ).stringValue = order.Id;
        serializedData.FindProperty( "_title" ).stringValue = order.Title;
        serializedData.FindProperty( "_difficulty" ).enumValueIndex =
            ( int )order.Difficulty;
        serializedData.FindProperty( "_acceptDays" ).intValue = order.AcceptDays;
        serializedData.FindProperty( "_deliveryDays" ).intValue = order.DeliveryDays;
        serializedData.FindProperty( "_delayDays" ).intValue = order.DelayDays;
        serializedData.FindProperty( "_maxCraftCost" ).intValue = order.MaxCraftCost;
        serializedData.FindProperty( "_specialType" ).enumValueIndex =
            ( int )order.SpecialType;
        serializedData.FindProperty( "_actualTravelDays" ).intValue =
            order.ActualTravelDays;

        UpdateConditions(
            serializedData.FindProperty( "_conditions" ), conditions );
        UpdatePurchaseGuides(
            serializedData.FindProperty( "_purchaseGuides" ), guides );

        SerializedProperty dialogueProperty =
            serializedData.FindProperty( "_dialogues" );
        dialogueProperty.arraySize = dialogues.Count;

        for ( int i = 0; i < dialogues.Count; i++ )
            dialogueProperty.GetArrayElementAtIndex( i )
                .objectReferenceValue = dialogues [ i ];

        serializedData.ApplyModifiedProperties( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// Day 2~7 후속 가이드 데이터에 대화 목록 적용
    /// </summary>
    /// <param name="data">갱신할 후속 가이드 데이터</param>
    /// <param name="dialogues">연결할 대화 에셋 목록</param>
    static void UpdateGuideData (
        TutorialGuideData data,
        IReadOnlyList<DialogueData> dialogues )
    {
        var serializedData = new SerializedObject( data );
        SerializedProperty dialogueProperty =
            serializedData.FindProperty( "_dialogues" );

        dialogueProperty.arraySize = dialogues.Count;

        for ( int i = 0; i < dialogues.Count; i++ )
        {
            dialogueProperty.GetArrayElementAtIndex( i )
                .objectReferenceValue = dialogues [ i ];
        }

        serializedData.ApplyModifiedProperties( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// 주문 조건 배열에 CSV 값 적용
    /// </summary>
    static void UpdateConditions (
        SerializedProperty property,
        IReadOnlyList<ConditionValues> conditions )
    {
        property.arraySize = conditions.Count;

        for ( int i = 0; i < conditions.Count; i++ )
        {
            ConditionValues values = conditions [ i ];
            SerializedProperty element = property.GetArrayElementAtIndex( i );

            element.FindPropertyRelative( "_conditionType" ).enumValueIndex =
                ( int )values.Type;
            element.FindPropertyRelative( "_partId" ).stringValue = values.PartId;
            element.FindPropertyRelative( "_quantity" ).intValue = values.Quantity;
        }
    }

    /// <summary>
    /// 구매 가이드 배열에 CSV 값 적용
    /// </summary>
    static void UpdatePurchaseGuides (
        SerializedProperty property,
        IReadOnlyList<PurchaseGuideValues> guides )
    {
        property.arraySize = guides.Count;

        for ( int i = 0; i < guides.Count; i++ )
        {
            PurchaseGuideValues values = guides [ i ];
            SerializedProperty element = property.GetArrayElementAtIndex( i );

            element.FindPropertyRelative( "_sequence" ).intValue = values.Sequence;
            element.FindPropertyRelative( "_partId" ).stringValue = values.PartId;
            element.FindPropertyRelative( "_requiredForProgress" ).boolValue =
                values.RequiredForProgress;
            element.FindPropertyRelative( "_note" ).stringValue = values.Note;
        }
    }

    /// <summary>
    /// 대화 에셋에 CSV 값 적용하고 기존 초상화 유지
    /// </summary>
    static void UpdateDialogue ( DialogueData data, DialogueValues values )
    {
        var serializedData = new SerializedObject( data );
        SerializedProperty lines = serializedData.FindProperty( "_lines" );
        var portraits = new Dictionary<int, UnityEngine.Object>( );

        //같은 줄 순번에 할당된 기존 초상화를 재임포트 이후에도 유지
        for ( int i = 0; i < lines.arraySize; i++ )
        {
            UnityEngine.Object portrait = lines.GetArrayElementAtIndex( i )
                .FindPropertyRelative( "_portrait" ).objectReferenceValue;

            if ( portrait != null ) portraits.Add( i, portrait );
        }

        serializedData.FindProperty( "_id" ).stringValue = values.Id;
        lines.arraySize = values.Lines.Count;

        for ( int i = 0; i < values.Lines.Count; i++ )
        {
            DialogueLineValues line = values.Lines [ i ];
            SerializedProperty element = lines.GetArrayElementAtIndex( i );

            element.FindPropertyRelative( "_lineType" ).enumValueIndex =
                ( int )line.Type;
            element.FindPropertyRelative( "_speakerName" ).stringValue = line.Speaker;
            element.FindPropertyRelative( "_content" ).stringValue = line.Content;
            element.FindPropertyRelative( "_portraitSide" ).enumValueIndex =
                ( int )line.PortraitSide;
            element.FindPropertyRelative( "_signalId" ).stringValue = line.SignalId;

            if ( portraits.TryGetValue(
                line.Index, out UnityEngine.Object portrait ) )
            {
                element.FindPropertyRelative( "_portrait" )
                    .objectReferenceValue = portrait;
            }
        }

        serializedData.ApplyModifiedProperties( );
        EditorUtility.SetDirty( data );
    }

    #endregion

    /// <summary>
    /// 아직 생성되지 않은 튜토리얼 파츠를 경고로 표시
    /// </summary>
    static void WarnMissingParts (
        IReadOnlyList<ConditionValues> conditions,
        IReadOnlyList<PurchaseGuideValues> guides )
    {
        var registeredIds = new HashSet<string>( StringComparer.Ordinal );
        string [ ] guids = AssetDatabase.FindAssets( "t:PartsData" );

        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath( guids [ i ] );
            PartsData data = AssetDatabase.LoadAssetAtPath<PartsData>( path );

            if ( data != null ) registeredIds.Add( data.Id );
        }

        var missingIds = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < conditions.Count; i++ )
        {
            if ( registeredIds.Contains( conditions [ i ].PartId ) == false )
                missingIds.Add( conditions [ i ].PartId );
        }

        for ( int i = 0; i < guides.Count; i++ )
        {
            if ( registeredIds.Contains( guides [ i ].PartId ) == false )
                missingIds.Add( guides [ i ].PartId );
        }

        if ( missingIds.Count == 0 ) return;

        Debug.LogWarning(
            "튜토리얼 데이터는 생성됐지만 아직 등록되지 않은 파츠가 있습니다: " +
            string.Join( ", ", missingIds ) );
    }
}
