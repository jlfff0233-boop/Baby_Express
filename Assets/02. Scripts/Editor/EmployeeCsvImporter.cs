using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 직원 CSV를 직원 데이터 에셋과 데이터맵으로 생성 또는 갱신
/// </summary>
public static class EmployeeCsvImporter
{
    const string MenuPath = "Assets/직원/CSV 데이터 생성";

    /// <summary>
    /// CSV 직원 한 항목의 생성 값
    /// </summary>
    class EmployeeValues
    {
        public string Id;
        public EmployeeSpecies Species;
        public string FirstName;
        public string LastName;
        public string DisplayName;
        public EmployeeJobType JobType;
        public EmployeeUnlockType UnlockType;
        public int RequiredDay;
        public float HireCost;
        public float WeeklyWage;
        public EmployeeEffectType EffectType;
        public float EffectValue;
        public string DialoguePortraitKey;
        public string HirePortraitKey;
    }

    /// <summary>
    /// 선택한 직원 데이터맵에 CSV를 적용할 수 있는지 확인
    /// </summary>
    /// <returns>메뉴 활성화 여부</returns>
    [MenuItem( MenuPath, true )]
    static bool ValidateImport ()
    {
        return Selection.activeObject is EmployeeDataMap;
    }

    /// <summary>
    /// 선택한 직원 데이터맵과 같은 폴더에 직원 데이터 생성
    /// </summary>
    [MenuItem( MenuPath )]
    static void Import ()
    {
        EmployeeDataMap dataMap =
            Selection.activeObject as EmployeeDataMap;
        string sourcePath = EditorUtility.OpenFilePanel(
            "Employees CSV 선택", string.Empty, "csv" );

        if ( string.IsNullOrEmpty( sourcePath ) ) return;

        try
        {
            List<EmployeeValues> values = ReadValues( sourcePath );
            string outputFolder = GetOutputFolder( dataMap );
            Dictionary<string, EmployeeData> existingDatas =
                FindExistingDatas( outputFolder );
            var generatedDatas = new List<EmployeeData>( values.Count );
            int createdCount = 0;
            int updatedCount = 0;

            AssetDatabase.StartAssetEditing( );

            try
            {
                for ( int i = 0; i < values.Count; i++ )
                {
                    EmployeeValues current = values [ i ];
                    EmployeeData data = CreateOrLoadData(
                        current.Id, outputFolder, existingDatas,
                        ref createdCount, ref updatedCount );

                    UpdateData( data, current );
                    generatedDatas.Add( data );
                }

                UpdateDataMap( dataMap, generatedDatas );
            }
            finally
            {
                AssetDatabase.StopAssetEditing( );
            }

            AssetDatabase.SaveAssets( );
            AssetDatabase.Refresh( );

            Debug.Log(
                $"직원 CSV 적용 완료, 생성 {createdCount}개, " +
                $"갱신 {updatedCount}개, 데이터맵 등록 {generatedDatas.Count}개" );
        }
        catch ( Exception exception )
        {
            Debug.LogError(
                $"직원 CSV 가져오기 실패: {exception.Message}" );
        }
    }

    /// <summary>
    /// CSV에서 확정 직원 값 읽기
    /// </summary>
    /// <param name="sourcePath">직원 CSV 경로</param>
    /// <returns>확정 직원 값 목록</returns>
    static List<EmployeeValues> ReadValues ( string sourcePath )
    {
        List<List<string>> rows = ReadCsv( sourcePath );

        if ( rows.Count < 2 )
            throw new InvalidDataException( "직원 CSV 데이터가 없습니다." );

        Dictionary<string, int> columns = CreateColumns( rows [ 0 ] );
        ValidateColumns( columns );

        var values = new List<EmployeeValues>( );
        var ids = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 1; i < rows.Count; i++ )
        {
            List<string> row = rows [ i ];

            if ( IsEmptyRow( row ) ||
                GetValue( row, columns, "Status" ) != "Confirmed" )
                continue;

            string id = GetRequiredValue( row, columns, "Id", i + 1 );

            if ( ids.Add( id ) == false )
                throw new InvalidDataException(
                    $"{i + 1}행 직원 ID가 중복되었습니다: {id}" );

            values.Add( new EmployeeValues
            {
                Id = id,
                Species = ParseEnum<EmployeeSpecies>(
                    row, columns, "Species", i + 1 ),
                FirstName = GetRequiredValue(
                    row, columns, "FirstName", i + 1 ),
                LastName = GetRequiredValue(
                    row, columns, "LastName", i + 1 ),
                DisplayName = GetRequiredValue(
                    row, columns, "FullName", i + 1 ),
                JobType = ParseJobType(
                    GetRequiredValue( row, columns, "Role", i + 1 ), i + 1 ),
                UnlockType = ParseEnum<EmployeeUnlockType>(
                    row, columns, "UnlockType", i + 1 ),
                RequiredDay = ParseInt(
                    row, columns, "RequiredDay", i + 1 ),
                HireCost = ParseFloat(
                    row, columns, "HireCost", i + 1 ),
                WeeklyWage = ParseFloat(
                    row, columns, "WeeklySalary", i + 1 ),
                EffectType = ParseEnum<EmployeeEffectType>(
                    row, columns, "EffectType", i + 1 ),
                EffectValue = ParseFloat(
                    row, columns, "EffectValue", i + 1 ),
                DialoguePortraitKey = GetOptionalValue(
                    row, columns, "DialoguePortraitKey",
                    GetValue( row, columns, "PortraitKey" ) ),
                HirePortraitKey = GetOptionalValue(
                    row, columns, "HirePortraitKey", string.Empty )
            } );
        }

        if ( values.Count == 0 )
            throw new InvalidDataException( "Confirmed 직원이 없습니다." );

        return values;
    }

    /// <summary>
    /// 직원 직종 CSV 명칭을 현재 enum으로 변환
    /// </summary>
    /// <param name="role">CSV 직종 명칭</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <returns>직원 직종</returns>
    static EmployeeJobType ParseJobType ( string role, int rowNumber )
    {
        return role switch
        {
            "Courier" => EmployeeJobType.Delivery,
            "ErrandRunner" => EmployeeJobType.OperationsRunner,
            "StockClerk" => EmployeeJobType.StockClerk,
            "Agent" => EmployeeJobType.Agent,
            "Engineer" => EmployeeJobType.Engineer,
            "Researcher" => EmployeeJobType.Researcher,
            "Carrier" => EmployeeJobType.Carrier,
            "Accountant" => EmployeeJobType.Accountant,
            _ => throw new InvalidDataException(
                $"{rowNumber}행 Role 값이 잘못되었습니다: {role}" )
        };
    }

    /// <summary>
    /// 직원 데이터 에셋 생성 또는 조회
    /// </summary>
    static EmployeeData CreateOrLoadData (
        string id, string outputFolder,
        Dictionary<string, EmployeeData> existingDatas,
        ref int createdCount, ref int updatedCount )
    {
        if ( existingDatas.TryGetValue( id, out EmployeeData data ) )
        {
            updatedCount++;
            return data;
        }

        data = ScriptableObject.CreateInstance<EmployeeData>( );
        AssetDatabase.CreateAsset(
            data, $"{outputFolder}/{id}.asset" );
        createdCount++;
        return data;
    }

    /// <summary>
    /// 직원 데이터 에셋 값 갱신
    /// </summary>
    static void UpdateData ( EmployeeData data, EmployeeValues values )
    {
        var serialized = new SerializedObject( data );
        SetString( serialized, "_id", values.Id );
        SetEnum( serialized, "_species", values.Species );
        SetString( serialized, "_firstName", values.FirstName );
        SetString( serialized, "_lastName", values.LastName );
        SetString( serialized, "_displayName", values.DisplayName );
        SetEnum( serialized, "_jobType", values.JobType );
        SetFloat( serialized, "_hireCost", values.HireCost );
        SetFloat( serialized, "_weeklyWage", values.WeeklyWage );
        SetEnum( serialized, "_unlockType", values.UnlockType );
        SetInt( serialized, "_requiredDay", values.RequiredDay );
        SetEnum( serialized, "_effectType", values.EffectType );
        SetFloat( serialized, "_effectValue", values.EffectValue );

        //이전 런타임 참조도 새 데이터와 같은 결과를 사용하도록 유지
        SetInt( serialized, "_deliverySlotIncrease",
            values.EffectType == EmployeeEffectType.DeliverySlotBonus
                ? Mathf.RoundToInt( values.EffectValue )
                : 0 );
        SetInt( serialized, "_requiredWeeklySettlementCount",
            values.UnlockType == EmployeeUnlockType.FirstWeeklySettlement
                ? 1
                : 0 );

        SetSpriteIfFound(
            serialized, "_dialoguePortrait", values.DialoguePortraitKey );
        SetSpriteIfFound(
            serialized, "_hirePortrait", values.HirePortraitKey );

        serialized.ApplyModifiedPropertiesWithoutUndo( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// 직원 데이터맵 목록 갱신
    /// </summary>
    static void UpdateDataMap (
        EmployeeDataMap dataMap, List<EmployeeData> datas )
    {
        var serialized = new SerializedObject( dataMap );
        SerializedProperty property = serialized.FindProperty( "_datas" );
        property.arraySize = datas.Count;

        for ( int i = 0; i < datas.Count; i++ )
            property.GetArrayElementAtIndex( i ).objectReferenceValue = datas [ i ];

        serialized.ApplyModifiedPropertiesWithoutUndo( );
        EditorUtility.SetDirty( dataMap );
    }

    /// <summary>
    /// 출력 폴더의 기존 직원 데이터를 ID로 조회
    /// </summary>
    static Dictionary<string, EmployeeData> FindExistingDatas (
        string outputFolder )
    {
        var datas = new Dictionary<string, EmployeeData>(
            StringComparer.Ordinal );
        string [ ] guids = AssetDatabase.FindAssets(
            "t:EmployeeData", new [ ] { outputFolder } );

        for ( int i = 0; i < guids.Length; i++ )
        {
            EmployeeData data = AssetDatabase.LoadAssetAtPath<EmployeeData>(
                AssetDatabase.GUIDToAssetPath( guids [ i ] ) );

            if ( data != null && string.IsNullOrWhiteSpace( data.Id ) == false )
                datas.TryAdd( data.Id, data );
        }

        return datas;
    }

    /// <summary>
    /// 선택 데이터맵이 위치한 출력 폴더 조회
    /// </summary>
    static string GetOutputFolder ( EmployeeDataMap dataMap )
    {
        string mapPath = AssetDatabase.GetAssetPath( dataMap );
        string folder = Path.GetDirectoryName( mapPath )?.Replace( '\\', '/' );

        if ( string.IsNullOrWhiteSpace( folder ) )
            throw new InvalidDataException( "직원 데이터맵 경로가 없습니다." );

        return folder;
    }

    /// <summary>
    /// 키와 이름이 정확히 일치하는 Sprite가 있으면 기존 값 교체
    /// </summary>
    static void SetSpriteIfFound (
        SerializedObject serialized, string propertyName, string key )
    {
        if ( string.IsNullOrWhiteSpace( key ) ) return;

        string [ ] guids = AssetDatabase.FindAssets( $"{key} t:Sprite" );

        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath( guids [ i ] );
            UnityEngine.Object [ ] assets =
                AssetDatabase.LoadAllAssetsAtPath( path );

            for ( int j = 0; j < assets.Length; j++ )
            {
                if ( assets [ j ] is not Sprite sprite || sprite.name != key )
                    continue;

                serialized.FindProperty( propertyName ).objectReferenceValue = sprite;
                return;
            }
        }
    }

    /// <summary>
    /// CSV 파일을 행과 열 목록으로 읽기
    /// </summary>
    static List<List<string>> ReadCsv ( string path )
    {
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
        using var reader = new StreamReader(
            stream, new UTF8Encoding( true ), true );
        return ParseCsv( reader.ReadToEnd( ) );
    }

    /// <summary>
    /// 따옴표와 줄바꿈을 포함한 CSV 문자열 파싱
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
                }
                else
                    inQuotes = !inQuotes;
            }
            else if ( current == ',' && inQuotes == false )
            {
                row.Add( field.ToString( ).Trim( ) );
                field.Clear( );
            }
            else if ( ( current == '\r' || current == '\n' ) &&
                inQuotes == false )
            {
                if ( current == '\r' && i + 1 < text.Length && text [ i + 1 ] == '\n' )
                    i++;

                row.Add( field.ToString( ).Trim( ) );
                field.Clear( );
                rows.Add( row );
                row = new List<string>( );
            }
            else
                field.Append( current );
        }

        if ( field.Length > 0 || row.Count > 0 )
        {
            row.Add( field.ToString( ).Trim( ) );
            rows.Add( row );
        }

        return rows;
    }

    /// <summary>
    /// CSV 헤더 이름과 열 인덱스 생성
    /// </summary>
    static Dictionary<string, int> CreateColumns ( List<string> header )
    {
        var columns = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase );

        for ( int i = 0; i < header.Count; i++ )
            columns [ header [ i ].TrimStart( '\uFEFF' ) ] = i;

        return columns;
    }

    /// <summary>
    /// 직원 CSV 필수 열 확인
    /// </summary>
    static void ValidateColumns ( Dictionary<string, int> columns )
    {
        string [ ] required =
        {
            "Id", "Species", "FirstName", "LastName", "FullName",
            "Role", "UnlockType", "RequiredDay", "HireCost",
            "WeeklySalary", "EffectType", "EffectValue", "Status"
        };

        for ( int i = 0; i < required.Length; i++ )
        {
            if ( columns.ContainsKey( required [ i ] ) == false )
                throw new InvalidDataException(
                    $"필수 열이 없습니다: {required [ i ]}" );
        }
    }

    static string GetRequiredValue (
        List<string> row, Dictionary<string, int> columns,
        string name, int rowNumber )
    {
        string value = GetValue( row, columns, name );

        if ( string.IsNullOrWhiteSpace( value ) )
            throw new InvalidDataException(
                $"{rowNumber}행 {name} 값이 비어 있습니다." );

        return value;
    }

    static string GetOptionalValue (
        List<string> row, Dictionary<string, int> columns,
        string name, string fallback )
    {
        string value = GetValue( row, columns, name );
        return string.IsNullOrWhiteSpace( value ) ? fallback : value;
    }

    static string GetValue (
        List<string> row, Dictionary<string, int> columns, string name )
    {
        return columns.TryGetValue( name, out int index ) && index < row.Count
            ? row [ index ].Trim( )
            : string.Empty;
    }

    static T ParseEnum<T> (
        List<string> row, Dictionary<string, int> columns,
        string name, int rowNumber ) where T : struct
    {
        string value = GetRequiredValue( row, columns, name, rowNumber );

        if ( Enum.TryParse( value, true, out T parsed ) )
            return parsed;

        throw new InvalidDataException(
            $"{rowNumber}행 {name} 값이 잘못되었습니다: {value}" );
    }

    static int ParseInt (
        List<string> row, Dictionary<string, int> columns,
        string name, int rowNumber )
    {
        string value = GetRequiredValue( row, columns, name, rowNumber );

        if ( int.TryParse( value, NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int parsed ) && parsed >= 0 )
            return parsed;

        throw new InvalidDataException(
            $"{rowNumber}행 {name} 값이 잘못되었습니다: {value}" );
    }

    static float ParseFloat (
        List<string> row, Dictionary<string, int> columns,
        string name, int rowNumber )
    {
        string value = GetRequiredValue( row, columns, name, rowNumber );

        if ( float.TryParse( value, NumberStyles.Float,
            CultureInfo.InvariantCulture, out float parsed ) && parsed >= 0f )
            return parsed;

        throw new InvalidDataException(
            $"{rowNumber}행 {name} 값이 잘못되었습니다: {value}" );
    }

    static bool IsEmptyRow ( List<string> row )
    {
        for ( int i = 0; i < row.Count; i++ )
        {
            if ( string.IsNullOrWhiteSpace( row [ i ] ) == false )
                return false;
        }

        return true;
    }

    static void SetString (
        SerializedObject serialized, string name, string value )
    {
        serialized.FindProperty( name ).stringValue = value;
    }

    static void SetInt (
        SerializedObject serialized, string name, int value )
    {
        serialized.FindProperty( name ).intValue = value;
    }

    static void SetFloat (
        SerializedObject serialized, string name, float value )
    {
        serialized.FindProperty( name ).floatValue = value;
    }

    static void SetEnum<T> (
        SerializedObject serialized, string name, T value ) where T : Enum
    {
        serialized.FindProperty( name ).enumValueIndex = Convert.ToInt32( value );
    }
}
