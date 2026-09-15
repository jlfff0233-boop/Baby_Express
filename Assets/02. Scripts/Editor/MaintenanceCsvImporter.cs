using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 선택한 CSV의 정비 데이터를 일괄 생성 또는 갱신
/// </summary>
public static class MaintenanceCsvImporter
{
    const string MenuPath = "Assets/정비/선택 CSV로 데이터 생성";

    /// <summary>
    /// CSV 정비 항목(생성용)
    /// </summary>
    class MaintenanceValues
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductType ProductType { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public MaintenanceApplyType ApplyType { get; set; }
        public float BasePrice { get; set; }
        public List<MaintenanceLevelValues> Levels { get; } = new List<MaintenanceLevelValues>( );
    }

    /// <summary>
    /// CSV 정비 단계(생성용)
    /// </summary>
    class MaintenanceLevelValues
    {
        public int Level { get; set; }
        public float UpgradeCost { get; set; }
        public List<MaintenanceEffectValues> Effects { get; } =
            new List<MaintenanceEffectValues>( );
        public List<MaintenanceRequirementValues> Requirements { get; } =
            new List<MaintenanceRequirementValues>( );
    }

    /// <summary>
    /// CSV 정비 효과(생성용)
    /// </summary>
    class MaintenanceEffectValues
    {
        public string TargetId { get; set; }
        public MaintenanceEffectType Type { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// CSV 정비 선행 조건(생성용)
    /// </summary>
    class MaintenanceRequirementValues
    {
        public MaintenanceRequirementType Type { get; set; }
        public int RequiredValue { get; set; }
        public WeeklyRating RequiredRating { get; set; }
        public string RequiredMaintenanceId { get; set; }
        public int RequiredLevel { get; set; }
    }

    /// <summary>
    /// 선택 CSV 정비 데이터 생성 메뉴 표시 여부
    /// </summary>
    /// <returns>메뉴 표시 여부</returns>
    [MenuItem( MenuPath, true )]
    static bool ValidateImport ()
    {
        if ( Selection.activeObject is not TextAsset ) return false;

        string path = AssetDatabase.GetAssetPath( Selection.activeObject );
        return Path.GetExtension( path ).Equals(
            ".csv", StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>
    /// 선택 CSV 정비 데이터 생성
    /// </summary>
    [MenuItem( MenuPath )]
    static void Import ()
    {
        TextAsset csv = Selection.activeObject as TextAsset;
        string csvPath = AssetDatabase.GetAssetPath( csv );

        if ( TryReadCsv(
            csv.text, out List<MaintenanceValues> maintenances,
            out string error ) == false )
        {
            Debug.LogError( $"정비 CSV 가져오기 실패: {error}" );
            return;
        }

        if ( TryGetDataMap( out PurchasableDataMap dataMap, out error ) == false )
        {
            Debug.LogError( $"정비 CSV 가져오기 실패: {error}" );
            return;
        }

        if ( ValidateReferences(
            maintenances, dataMap, out error ) == false )
        {
            Debug.LogError( $"정비 CSV 가져오기 실패: {error}" );
            return;
        }

        var generatedDatas = new List<MaintenanceData>( );
        int createdCount = 0;
        int updatedCount = 0;

        AssetDatabase.StartAssetEditing( );

        try
        {
            for ( int i = 0; i < maintenances.Count; i++ )
            {
                MaintenanceValues values = maintenances [ i ];
                string folderPath = GetOutputFolderPath(
                    csvPath, values.MaintenanceType );

                if ( AssetDatabase.IsValidFolder( folderPath ) == false )
                    throw new DirectoryNotFoundException(
                        $"정비 데이터 폴더가 없습니다: {folderPath}" );

                string assetPath = $"{folderPath}/{values.Id}.asset";
                MaintenanceData data = AssetDatabase.LoadAssetAtPath<MaintenanceData>( assetPath );

                if ( data == null && AssetDatabase.LoadMainAssetAtPath( assetPath ) != null )
                    throw new InvalidOperationException(
                        $"같은 경로에 다른 종류의 에셋이 있습니다: {assetPath}" );

                if ( data == null )
                {
                    data = ScriptableObject.CreateInstance<MaintenanceData>( );
                    data.name = values.Id;
                    AssetDatabase.CreateAsset( data, assetPath );
                    createdCount++;
                }
                else
                {
                    Undo.RecordObject( data, "정비 CSV 데이터 갱신" );
                    updatedCount++;
                }

                UpdateData( data, values );
                generatedDatas.Add( data );
            }

            UpdateDataMap( dataMap, generatedDatas );
        }
        catch ( Exception exception )
        {
            Debug.LogError( $"정비 CSV 에셋 생성 실패: {exception.Message}" );
            return;
        }
        finally
        {
            AssetDatabase.StopAssetEditing( );
        }

        AssetDatabase.SaveAssets( );
        AssetDatabase.Refresh( );

        Debug.Log(
            $"정비 CSV 적용 완료, 생성 {createdCount}개, " +
            $"갱신 {updatedCount}개, 데이터맵 등록 {generatedDatas.Count}개" );
    }

    /// <summary>
    /// 정비 효과 대상과 선행 정비 참조 검증
    /// </summary>
    /// <param name="maintenances">CSV 정비 데이터 목록</param>
    /// <param name="dataMap">공용 상품 데이터맵</param>
    /// <param name="error">실패 사유</param>
    /// <returns>검증 성공 여부</returns>
    static bool ValidateReferences (
        IReadOnlyList<MaintenanceValues> maintenances,
        PurchasableDataMap dataMap, out string error )
    {
        error = string.Empty;
        var partIds = new HashSet<string>( StringComparer.Ordinal );
        var maintenanceIds = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < dataMap.PurchasableDatas.Count; i++ )
        {
            PurchasableData data = dataMap.PurchasableDatas [ i ];

            if ( data is PartsData ) partIds.Add( data.Id );
            if ( data is MaintenanceData ) maintenanceIds.Add( data.Id );
        }

        for ( int i = 0; i < maintenances.Count; i++ )
            maintenanceIds.Add( maintenances [ i ].Id );

        for ( int i = 0; i < maintenances.Count; i++ )
        {
            MaintenanceValues maintenance = maintenances [ i ];

            if ( maintenance.MaintenanceType == MaintenanceType.Research &&
                maintenance.ProductType != ProductType.Equipment )
            {
                error = $"연구 상품은 장비로 등록해야 합니다: {maintenance.Id}";
                return false;
            }

            for ( int j = 0; j < maintenance.Levels.Count; j++ )
            {
                MaintenanceLevelValues level = maintenance.Levels [ j ];

                for ( int k = 0; k < level.Effects.Count; k++ )
                {
                    MaintenanceEffectValues effect = level.Effects [ k ];

                    if ( effect.Type == MaintenanceEffectType.PartUnlock &&
                        partIds.Contains( effect.TargetId ) == false )
                    {
                        error = $"등록되지 않은 파츠 아이디입니다: {effect.TargetId}";
                        return false;
                    }
                }

                for ( int k = 0; k < level.Requirements.Count; k++ )
                {
                    MaintenanceRequirementValues requirement =
                        level.Requirements [ k ];

                    if ( requirement.Type ==
                        MaintenanceRequirementType.MaintenanceLevel &&
                        maintenanceIds.Contains(
                            requirement.RequiredMaintenanceId ) == false )
                    {
                        error = $"등록되지 않은 선행 정비 아이디입니다: " +
                            $"{requirement.RequiredMaintenanceId}";
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 정비 종류별 데이터 출력 폴더 반환
    /// </summary>
    /// <param name="csvPath">CSV 에셋 경로</param>
    /// <param name="maintenanceType">정비 종류</param>
    /// <returns>정비 데이터 출력 폴더</returns>
    static string GetOutputFolderPath (
        string csvPath, MaintenanceType maintenanceType )
    {
        string csvFolderPath =
            Path.GetDirectoryName( csvPath ).Replace( '\\', '/' );
        string maintenanceFolderPath =
            Path.GetDirectoryName( csvFolderPath ).Replace( '\\', '/' );

        return $"{maintenanceFolderPath}/{maintenanceType}";
    }

    /// <summary>
    /// 공용 상품 데이터맵 조회
    /// </summary>
    /// <param name="dataMap">조회한 공용 상품 데이터맵</param>
    /// <param name="error">실패 사유</param>
    /// <returns>조회 성공 여부</returns>
    static bool TryGetDataMap (
        out PurchasableDataMap dataMap, out string error )
    {
        dataMap = null;
        error = string.Empty;

        string [ ] startSettingGuids =
            AssetDatabase.FindAssets( "t:PlayStartSettingsData" );

        if ( startSettingGuids.Length != 1 )
        {
            error = "PlayStartSettingsData 에셋은 하나여야 합니다.";
            return false;
        }

        string startSettingPath =
            AssetDatabase.GUIDToAssetPath( startSettingGuids [ 0 ] );
        PlayStartSettingsData startSettings =
            AssetDatabase.LoadAssetAtPath<PlayStartSettingsData>(
                startSettingPath );
        PurchasableDataMap startMap =
            startSettings?.UnlockedProductMap;

        if ( startMap == null )
        {
            error = "초기 상품 데이터맵이 없습니다.";
            return false;
        }

        string [ ] guids = AssetDatabase.FindAssets( "t:PurchasableDataMap" );
        var candidates = new List<PurchasableDataMap>( );

        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath( guids [ i ] );
            PurchasableDataMap currentMap =
                AssetDatabase.LoadAssetAtPath<PurchasableDataMap>( path );

            if ( currentMap != null && currentMap != startMap )
                candidates.Add( currentMap );
        }

        if ( candidates.Count == 1 )
        {
            dataMap = candidates [ 0 ];
            return true;
        }

        error = candidates.Count == 0
            ? "초기 상품 맵을 제외한 공용 상품 데이터맵이 없습니다."
            : "초기 상품 맵을 제외한 공용 상품 데이터맵이 여러 개입니다.";
        return false;
    }

    /// <summary>
    /// 생성한 정비 데이터를 공용 상품 데이터맵에 등록
    /// </summary>
    /// <param name="dataMap">공용 상품 데이터맵</param>
    /// <param name="generatedDatas">생성 또는 갱신한 정비 데이터</param>
    static void UpdateDataMap (
        PurchasableDataMap dataMap,
        IReadOnlyList<MaintenanceData> generatedDatas )
    {
        var datas = new List<PurchasableData>( dataMap.PurchasableDatas );

        for ( int i = 0; i < generatedDatas.Count; i++ )
        {
            MaintenanceData generatedData = generatedDatas [ i ];
            int index = FindMaintenanceIndex( datas, generatedData.Id );

            //같은 정비 아이디가 있으면 최신 에셋으로 교체
            if ( index >= 0 )
                datas [ index ] = generatedData;
            else
                datas.Add( generatedData );
        }

        var serializedMap = new SerializedObject( dataMap );
        SerializedProperty datasProperty = serializedMap.FindProperty( "_datas" );
        datasProperty.arraySize = datas.Count;

        for ( int i = 0; i < datas.Count; i++ )
        {
            datasProperty.GetArrayElementAtIndex( i ).objectReferenceValue =
                datas [ i ];
        }

        serializedMap.ApplyModifiedPropertiesWithoutUndo( );
        EditorUtility.SetDirty( dataMap );
    }

    /// <summary>
    /// 같은 아이디의 정비 데이터 위치 조회
    /// </summary>
    /// <param name="datas">공용 상품 데이터 목록</param>
    /// <param name="id">정비 아이디</param>
    /// <returns>정비 데이터 위치, 없으면 -1</returns>
    static int FindMaintenanceIndex (
        IReadOnlyList<PurchasableData> datas, string id )
    {
        for ( int i = 0; i < datas.Count; i++ )
        {
            if ( datas [ i ] is MaintenanceData maintenanceData &&
                maintenanceData.Id == id )
                return i;
        }

        return -1;
    }

    /// <summary>
    /// 정비 에셋에 CSV 값 적용
    /// </summary>
    /// <param name="data">적용할 정비 에셋</param>
    /// <param name="values">CSV 정비 값</param>
    static void UpdateData ( MaintenanceData data, MaintenanceValues values )
    {
        var serializedData = new SerializedObject( data );

        serializedData.FindProperty( "_productType" ).enumValueIndex =
            ( int ) values.ProductType;
        serializedData.FindProperty( "_name" ).stringValue = values.Name;
        serializedData.FindProperty( "_id" ).stringValue = values.Id;
        serializedData.FindProperty( "_basePrice" ).floatValue = values.BasePrice;
        serializedData.FindProperty( "_baseStockQuantity" ).intValue = 1;
        serializedData.FindProperty( "_baseRestockSpan" ).intValue = 0;
        serializedData.FindProperty( "_disc" ).stringValue = values.Description;
        serializedData.FindProperty( "_type" ).enumValueIndex =
            ( int ) values.MaintenanceType;
        serializedData.FindProperty( "_applyType" ).enumValueIndex =
            ( int ) values.ApplyType;

        SerializedProperty levels = serializedData.FindProperty( "_levels" );
        levels.arraySize = values.Levels.Count;

        for ( int i = 0; i < values.Levels.Count; i++ )
        {
            MaintenanceLevelValues levelValues = values.Levels [ i ];
            SerializedProperty level = levels.GetArrayElementAtIndex( i );

            level.FindPropertyRelative( "_upgradeCost" ).floatValue =
                levelValues.UpgradeCost;

            UpdateEffects(
                level.FindPropertyRelative( "_effects" ),
                levelValues.Effects );
            UpdateRequirements(
                level.FindPropertyRelative( "_requirements" ),
                levelValues.Requirements );
        }

        serializedData.ApplyModifiedProperties( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// 정비 단계 효과 갱신
    /// </summary>
    /// <param name="effectsProperty">직렬화된 효과 배열</param>
    /// <param name="effectValues">CSV 효과값 목록</param>
    static void UpdateEffects (
        SerializedProperty effectsProperty,
        IReadOnlyList<MaintenanceEffectValues> effectValues )
    {
        effectsProperty.arraySize = effectValues.Count;

        for ( int i = 0; i < effectValues.Count; i++ )
        {
            MaintenanceEffectValues values = effectValues [ i ];
            SerializedProperty effect =
                effectsProperty.GetArrayElementAtIndex( i );

            effect.FindPropertyRelative( "_targetId" ).stringValue =
                values.TargetId;
            effect.FindPropertyRelative( "_type" ).enumValueIndex =
                ( int ) values.Type;
            effect.FindPropertyRelative( "_value" ).floatValue =
                values.Value;
        }
    }

    /// <summary>
    /// 정비 단계 선행 조건 갱신
    /// </summary>
    /// <param name="requirementsProperty">직렬화된 선행 조건 배열</param>
    /// <param name="requirementValues">CSV 선행 조건값 목록</param>
    static void UpdateRequirements (
        SerializedProperty requirementsProperty,
        IReadOnlyList<MaintenanceRequirementValues> requirementValues )
    {
        requirementsProperty.arraySize = requirementValues.Count;

        for ( int i = 0; i < requirementValues.Count; i++ )
        {
            MaintenanceRequirementValues values = requirementValues [ i ];
            SerializedProperty requirement =
                requirementsProperty.GetArrayElementAtIndex( i );

            requirement.FindPropertyRelative( "_type" ).enumValueIndex =
                ( int ) values.Type;
            requirement.FindPropertyRelative( "_requiredValue" ).intValue =
                values.RequiredValue;
            requirement.FindPropertyRelative( "_requiredRating" ).enumValueIndex =
                ( int ) values.RequiredRating;
            requirement.FindPropertyRelative(
                "_requiredMaintenanceId" ).stringValue =
                values.RequiredMaintenanceId;
            requirement.FindPropertyRelative( "_requiredLevel" ).intValue =
                values.RequiredLevel;
        }
    }

    /// <summary>
    /// CSV 정비 데이터 읽기
    /// </summary>
    /// <param name="csvText">CSV 원문</param>
    /// <param name="maintenances">읽은 정비 데이터</param>
    /// <param name="error">실패 사유</param>
    /// <returns>읽기 성공 여부</returns>
    static bool TryReadCsv (
        string csvText, out List<MaintenanceValues> maintenances,
        out string error )
    {
        maintenances = new List<MaintenanceValues>( );
        error = string.Empty;

        List<List<string>> rows = ReadRows( csvText );

        if ( rows.Count < 2 )
        {
            error = "데이터 행이 없습니다.";
            return false;
        }

        Dictionary<string, int> columns = CreateColumnMap( rows [ 0 ] );
        string [ ] requiredColumns =
        {
            "Id", "Name", "Description", "ApplyType", "BasePrice",
            "Level", "UpgradeCost", "EffectType", "EffectValue"
        };

        for ( int i = 0; i < requiredColumns.Length; i++ )
        {
            if ( columns.ContainsKey( requiredColumns [ i ] ) ) continue;

            error = $"필수 열이 없습니다: {requiredColumns [ i ]}";
            return false;
        }

        MaintenanceValues current = null;

        for ( int i = 1; i < rows.Count; i++ )
        {
            List<string> row = rows [ i ];
            int rowNumber = i + 1;
            string id = GetValue( row, columns [ "Id" ] );

            if ( string.IsNullOrWhiteSpace( id ) == false )
            {
                if ( TryCreateMaintenance(
                    row, columns, id, rowNumber,
                    out current, out error ) == false )
                    return false;

                maintenances.Add( current );
            }
            else if ( current == null )
            {
                error = $"{rowNumber}행의 정비 아이디가 없습니다.";
                return false;
            }
            else if ( TryValidateSharedValues(
                row, columns, current, rowNumber, out error ) == false )
                return false;

            if ( TryCreateLevel(
                row, columns, current, rowNumber,
                out MaintenanceLevelValues level, out error ) == false )
                return false;

            current.Levels.Add( level );
        }

        return ValidateMaintenances( maintenances, out error );
    }

    /// <summary>
    /// CSV 첫 행의 정비 공통값 생성
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="columns">CSV 열 위치</param>
    /// <param name="id">정비 아이디</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <param name="values">생성한 정비 공통값</param>
    /// <param name="error">실패 사유</param>
    /// <returns>생성 성공 여부</returns>
    static bool TryCreateMaintenance (
        List<string> row, Dictionary<string, int> columns,
        string id, int rowNumber, out MaintenanceValues values,
        out string error )
    {
        values = null;
        error = string.Empty;

        string name = GetValue( row, columns [ "Name" ] );
        string description = GetValue( row, columns [ "Description" ] );
        string productTypeText = GetOptionalValue(
            row, columns, "ProductType" );
        string maintenanceTypeText = GetOptionalValue(
            row, columns, "MaintenanceType" );
        string applyTypeText = GetValue( row, columns [ "ApplyType" ] );
        string basePriceText = GetValue( row, columns [ "BasePrice" ] );

        if ( string.IsNullOrWhiteSpace( name ) ||
            string.IsNullOrWhiteSpace( description ) ||
            Enum.TryParse( applyTypeText, true,
                out MaintenanceApplyType applyType ) == false ||
            TryParseNonNegativeFloat( basePriceText, out float basePrice ) == false )
        {
            error = $"{rowNumber}행의 정비 공통값이 올바르지 않습니다.";
            return false;
        }

        ProductType productType = ProductType.Expansion;
        MaintenanceType maintenanceType = MaintenanceType.Facility;

        if ( ( string.IsNullOrWhiteSpace( productTypeText ) == false &&
               Enum.TryParse(
                   productTypeText, true, out productType ) == false ) ||
             ( string.IsNullOrWhiteSpace( maintenanceTypeText ) == false &&
               Enum.TryParse(
                   maintenanceTypeText, true, out maintenanceType ) == false ) )
        {
            error = $"{rowNumber}행의 상품 또는 정비 종류가 올바르지 않습니다.";
            return false;
        }

        if ( id.IndexOfAny( Path.GetInvalidFileNameChars( ) ) >= 0 )
        {
            error = $"{rowNumber}행의 아이디를 파일명으로 사용할 수 없습니다: {id}";
            return false;
        }

        values = new MaintenanceValues
        {
            Id = id,
            Name = name,
            Description = description,
            ProductType = productType,
            MaintenanceType = maintenanceType,
            ApplyType = applyType,
            BasePrice = basePrice
        };

        return true;
    }

    /// <summary>
    /// CSV 후속 행의 공통값 일치 여부 확인
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="columns">CSV 열 위치</param>
    /// <param name="current">현재 정비 공통값</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <param name="error">실패 사유</param>
    /// <returns>일치 여부</returns>
    static bool TryValidateSharedValues (
        List<string> row, Dictionary<string, int> columns,
        MaintenanceValues current, int rowNumber, out string error )
    {
        error = string.Empty;
        string productTypeText = GetOptionalValue(
            row, columns, "ProductType" );
        string maintenanceTypeText = GetOptionalValue(
            row, columns, "MaintenanceType" );
        string applyTypeText = GetValue( row, columns [ "ApplyType" ] );
        string basePriceText = GetValue( row, columns [ "BasePrice" ] );

        if ( string.IsNullOrWhiteSpace( productTypeText ) == false &&
            ( Enum.TryParse( productTypeText, true,
                out ProductType productType ) == false ||
              productType != current.ProductType ) )
        {
            error = $"{rowNumber}행의 상품 종류가 첫 단계와 다릅니다.";
            return false;
        }

        if ( string.IsNullOrWhiteSpace( maintenanceTypeText ) == false &&
            ( Enum.TryParse( maintenanceTypeText, true,
                out MaintenanceType maintenanceType ) == false ||
              maintenanceType != current.MaintenanceType ) )
        {
            error = $"{rowNumber}행의 정비 종류가 첫 단계와 다릅니다.";
            return false;
        }

        if ( string.IsNullOrWhiteSpace( applyTypeText ) == false &&
            ( Enum.TryParse( applyTypeText, true,
                out MaintenanceApplyType applyType ) == false ||
              applyType != current.ApplyType ) )
        {
            error = $"{rowNumber}행의 적용 시점이 첫 단계와 다릅니다.";
            return false;
        }

        if ( string.IsNullOrWhiteSpace( basePriceText ) == false &&
            ( TryParseNonNegativeFloat(
                basePriceText, out float basePrice ) == false ||
              basePrice != current.BasePrice ) )
        {
            error = $"{rowNumber}행의 기본 가격이 첫 단계와 다릅니다.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// CSV 단계값 생성
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="columns">CSV 열 위치</param>
    /// <param name="current">현재 정비 공통값</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <param name="level">생성한 정비 단계값</param>
    /// <param name="error">실패 사유</param>
    /// <returns>생성 성공 여부</returns>
    static bool TryCreateLevel (
        List<string> row, Dictionary<string, int> columns,
        MaintenanceValues current, int rowNumber,
        out MaintenanceLevelValues level, out string error )
    {
        level = null;
        error = string.Empty;

        int levelNumber = 0;
        float upgradeCost = 0f;

        bool isValid =
            int.TryParse( GetValue( row, columns [ "Level" ] ),
                out levelNumber ) && levelNumber > 0 &&
            TryParseNonNegativeFloat(
                GetValue( row, columns [ "UpgradeCost" ] ),
                out upgradeCost );

        if ( isValid == false )
        {
            error = $"{rowNumber}행의 단계 또는 비용이 올바르지 않습니다.";
            return false;
        }

        level = new MaintenanceLevelValues
        {
            Level = levelNumber,
            UpgradeCost = upgradeCost
        };

        if ( TryCreateEffects(
            row, columns, rowNumber, level.Effects, out error ) == false )
            return false;

        return TryCreateRequirements(
            row, columns, rowNumber, level.Requirements, out error );
    }

    /// <summary>
    /// CSV 단계 효과값 생성
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="columns">CSV 열 위치</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <param name="effects">생성한 효과 목록</param>
    /// <param name="error">실패 사유</param>
    /// <returns>생성 성공 여부</returns>
    static bool TryCreateEffects (
        List<string> row, Dictionary<string, int> columns,
        int rowNumber, List<MaintenanceEffectValues> effects,
        out string error )
    {
        error = string.Empty;
        string effectTypeText = GetValue( row, columns [ "EffectType" ] );
        string targetIdsText = GetOptionalValue(
            row, columns, "TargetIds" );
        string effectValueText = GetValue( row, columns [ "EffectValue" ] );

        //효과 종류가 비어 있으면 후속 파츠 추가용 빈 단계 허용
        if ( string.IsNullOrWhiteSpace( effectTypeText ) )
        {
            if ( string.IsNullOrWhiteSpace( targetIdsText ) &&
                string.IsNullOrWhiteSpace( effectValueText ) )
                return true;

            error = $"{rowNumber}행의 효과 종류가 없습니다.";
            return false;
        }

        if ( Enum.TryParse( effectTypeText, true,
            out MaintenanceEffectType effectType ) == false ||
            float.TryParse(
                effectValueText, NumberStyles.Float,
                CultureInfo.InvariantCulture, out float effectValue ) == false ||
            float.IsNaN( effectValue ) || float.IsInfinity( effectValue ) )
        {
            error = $"{rowNumber}행의 효과값이 올바르지 않습니다.";
            return false;
        }

        string [ ] targetIds = targetIdsText.Split(
            new [ ] { '|' }, StringSplitOptions.RemoveEmptyEntries );

        if ( effectType == MaintenanceEffectType.PartUnlock &&
            ( effectValue != 0f || targetIds.Length == 0 ) )
        {
            error = $"{rowNumber}행의 파츠 해금 효과값이 올바르지 않습니다.";
            return false;
        }

        if ( effectType == MaintenanceEffectType.InformationUnlock &&
            ( effectValue != 0f || targetIds.Length == 0 ) )
        {
            error = $"{rowNumber}행의 정보 해금 효과값이 올바르지 않습니다.";
            return false;
        }

        if ( effectType == MaintenanceEffectType.ThemeScoreBonus &&
            ( effectValue <= 0f || targetIds.Length != 1 ||
              Enum.TryParse( targetIds [ 0 ].Trim( ), true,
                  out PartTheme theme ) == false ||
              theme.IsTraitTheme( ) == false ) )
        {
            error = $"{rowNumber}행의 테마 연구 효과값이 올바르지 않습니다.";
            return false;
        }

        //대상 아이디를 사용하지 않는 기존 시설 효과는 효과 하나 생성
        if ( targetIds.Length == 0 )
        {
            effects.Add( new MaintenanceEffectValues
            {
                TargetId = string.Empty,
                Type = effectType,
                Value = effectValue
            } );
            return true;
        }

        var uniqueTargetIds = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < targetIds.Length; i++ )
        {
            string targetId = targetIds [ i ].Trim( );

            if ( string.IsNullOrWhiteSpace( targetId ) ||
                uniqueTargetIds.Add( targetId ) == false )
            {
                error = $"{rowNumber}행의 효과 대상 아이디가 중복되거나 비어 있습니다.";
                return false;
            }

            if ( effectType == MaintenanceEffectType.InformationUnlock &&
                InformationUnlockId.IsDefined( targetId ) == false )
            {
                error = $"{rowNumber}행의 정보 해금 아이디가 등록되지 않았습니다: " +
                    targetId;
                return false;
            }

            effects.Add( new MaintenanceEffectValues
            {
                TargetId = targetId,
                Type = effectType,
                Value = effectValue
            } );
        }

        return true;
    }

    /// <summary>
    /// CSV 단계 선행 조건값 생성
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="columns">CSV 열 위치</param>
    /// <param name="rowNumber">CSV 행 번호</param>
    /// <param name="requirements">생성한 선행 조건 목록</param>
    /// <param name="error">실패 사유</param>
    /// <returns>생성 성공 여부</returns>
    static bool TryCreateRequirements (
        List<string> row, Dictionary<string, int> columns,
        int rowNumber, List<MaintenanceRequirementValues> requirements,
        out string error )
    {
        error = string.Empty;
        string totalDayText = GetOptionalValue(
            row, columns, "RequiredTotalDay" );
        string weeklyRatingText = GetOptionalValue(
            row, columns, "RequiredWeeklyRating" );
        string maintenanceId = GetOptionalValue(
            row, columns, "RequiredMaintenanceId" );
        string maintenanceLevelText = GetOptionalValue(
            row, columns, "RequiredMaintenanceLevel" );

        if ( string.IsNullOrWhiteSpace( totalDayText ) == false )
        {
            if ( int.TryParse( totalDayText, out int totalDay ) == false ||
                totalDay < 1 )
            {
                error = $"{rowNumber}행의 누적 영업일 조건이 올바르지 않습니다.";
                return false;
            }

            requirements.Add( new MaintenanceRequirementValues
            {
                Type = MaintenanceRequirementType.TotalDay,
                RequiredValue = totalDay
            } );
        }

        if ( string.IsNullOrWhiteSpace( weeklyRatingText ) == false )
        {
            if ( Enum.TryParse( weeklyRatingText, true,
                out WeeklyRating weeklyRating ) == false ||
                weeklyRating == WeeklyRating.Insufficient )
            {
                error = $"{rowNumber}행의 주간 평가 조건이 올바르지 않습니다.";
                return false;
            }

            requirements.Add( new MaintenanceRequirementValues
            {
                Type = MaintenanceRequirementType.WeeklyRating,
                RequiredRating = weeklyRating
            } );
        }

        bool hasMaintenanceId =
            string.IsNullOrWhiteSpace( maintenanceId ) == false;
        bool hasMaintenanceLevel =
            string.IsNullOrWhiteSpace( maintenanceLevelText ) == false;

        if ( hasMaintenanceId != hasMaintenanceLevel )
        {
            error = $"{rowNumber}행의 선행 정비 아이디와 단계가 함께 필요합니다.";
            return false;
        }

        if ( hasMaintenanceId )
        {
            if ( int.TryParse(
                maintenanceLevelText, out int maintenanceLevel ) == false ||
                maintenanceLevel < 1 )
            {
                error = $"{rowNumber}행의 선행 정비 단계가 올바르지 않습니다.";
                return false;
            }

            requirements.Add( new MaintenanceRequirementValues
            {
                Type = MaintenanceRequirementType.MaintenanceLevel,
                RequiredMaintenanceId = maintenanceId,
                RequiredLevel = maintenanceLevel
            } );
        }

        return true;
    }

    /// <summary>
    /// 전체 정비 단계 순서와 아이디 중복 확인
    /// </summary>
    /// <param name="maintenances">정비 데이터 목록</param>
    /// <param name="error">실패 사유</param>
    /// <returns>검증 성공 여부</returns>
    static bool ValidateMaintenances (
        List<MaintenanceValues> maintenances, out string error )
    {
        error = string.Empty;
        var ids = new HashSet<string>( );

        for ( int i = 0; i < maintenances.Count; i++ )
        {
            MaintenanceValues values = maintenances [ i ];

            if ( ids.Add( values.Id ) == false )
            {
                error = $"정비 아이디가 중복됩니다: {values.Id}";
                return false;
            }

            for ( int j = 0; j < values.Levels.Count; j++ )
            {
                if ( values.Levels [ j ].Level == j + 1 ) continue;

                error = $"{values.Id}의 단계가 1부터 순서대로 입력되지 않았습니다.";
                return false;
            }
        }

        return maintenances.Count > 0;
    }

    /// <summary>
    /// CSV 헤더 열 위치 생성
    /// </summary>
    /// <param name="header">CSV 헤더 행</param>
    /// <returns>열 이름별 위치</returns>
    static Dictionary<string, int> CreateColumnMap ( List<string> header )
    {
        var columns = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );

        for ( int i = 0; i < header.Count; i++ )
        {
            string column = header [ i ].Trim( ).TrimStart( '\uFEFF' );

            if ( string.IsNullOrWhiteSpace( column ) == false )
                columns [ column ] = i;
        }

        return columns;
    }

    /// <summary>
    /// CSV 전체 행 읽기
    /// </summary>
    /// <param name="csvText">CSV 원문</param>
    /// <returns>CSV 행 목록</returns>
    static List<List<string>> ReadRows ( string csvText )
    {
        var rows = new List<List<string>>( );
        var row = new List<string>( );
        var value = new System.Text.StringBuilder( );
        bool isQuoted = false;

        for ( int i = 0; i < csvText.Length; i++ )
        {
            char character = csvText [ i ];

            if ( character == '"' )
            {
                if ( isQuoted && i + 1 < csvText.Length &&
                    csvText [ i + 1 ] == '"' )
                {
                    value.Append( '"' );
                    i++;
                }
                else
                    isQuoted = !isQuoted;
            }
            else if ( character == ',' && isQuoted == false )
            {
                row.Add( value.ToString( ).Trim( ) );
                value.Clear( );
            }
            else if ( ( character == '\r' || character == '\n' ) &&
                isQuoted == false )
            {
                if ( character == '\r' && i + 1 < csvText.Length &&
                    csvText [ i + 1 ] == '\n' )
                    i++;

                row.Add( value.ToString( ).Trim( ) );
                value.Clear( );

                if ( IsEmptyRow( row ) == false ) rows.Add( row );
                row = new List<string>( );
            }
            else
                value.Append( character );
        }

        row.Add( value.ToString( ).Trim( ) );
        if ( IsEmptyRow( row ) == false ) rows.Add( row );

        return rows;
    }

    /// <summary>
    /// CSV 빈 행 여부 확인
    /// </summary>
    /// <param name="row">CSV 행</param>
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
    /// CSV 지정 열 값 반환
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="index">열 위치</param>
    /// <returns>열 값</returns>
    static string GetValue ( List<string> row, int index )
    {
        return index >= 0 && index < row.Count
            ? row [ index ].Trim( )
            : string.Empty;
    }

    /// <summary>
    /// CSV 선택 열 값 반환
    /// </summary>
    /// <param name="row">CSV 행</param>
    /// <param name="columns">CSV 열 위치</param>
    /// <param name="columnName">선택 열 이름</param>
    /// <returns>열이 없으면 빈 문자열</returns>
    static string GetOptionalValue (
        List<string> row, IReadOnlyDictionary<string, int> columns,
        string columnName )
    {
        return columns.TryGetValue( columnName, out int index )
            ? GetValue( row, index )
            : string.Empty;
    }

    /// <summary>
    /// 0 이상 실수 변환
    /// </summary>
    /// <param name="text">변환할 문자열</param>
    /// <param name="value">변환한 값</param>
    /// <returns>변환 성공 여부</returns>
    static bool TryParseNonNegativeFloat ( string text, out float value )
    {
        return float.TryParse(
            text, NumberStyles.Float,
            CultureInfo.InvariantCulture, out value ) &&
            float.IsNaN( value ) == false &&
            float.IsInfinity( value ) == false &&
            value >= 0f;
    }
}
