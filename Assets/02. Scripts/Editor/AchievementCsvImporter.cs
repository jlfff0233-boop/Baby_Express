using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 업적 CSV 3종을 업적 데이터 에셋과 데이터맵으로 생성 또는 갱신
/// </summary>
public static class AchievementCsvImporter
{
    const string MenuPath = "Assets/업적/CSV 데이터 생성";
    const string AchievementFileName = "Achievements.csv";
    const string StageFileName = "AchievementStages.csv";
    const string RewardFileName = "AchievementRewards.csv";

    /// <summary>
    /// CSV 업적 한 항목의 생성 값
    /// </summary>
    class AchievementValues
    {
        public string Id;
        public AchvCategory Category;
        public string DisplayName;
        public string Description;
        public bool IsHidden;
        public AchvProgressType ProgressType;
        public List<StageValues> Stages = new List<StageValues>( );
    }

    /// <summary>
    /// CSV 업적 단계 한 항목의 생성 값
    /// </summary>
    class StageValues
    {
        public int StageIndex;
        public int TargetValue;
        public string DisplayName;
        public string Description;
        public List<RewardValues> Rewards = new List<RewardValues>( );
    }

    /// <summary>
    /// CSV 업적 보상 한 항목의 생성 값
    /// </summary>
    class RewardValues
    {
        public int RewardIndex;
        public AchvRewardType Type;
        public float Amount;
        public string TargetId;
    }

    /// <summary>
    /// 헤더 조회가 끝난 CSV 표
    /// </summary>
    class CsvTable
    {
        public Dictionary<string, int> Columns;
        public List<List<string>> Rows;
    }

    /// <summary>
    /// 선택한 업적 데이터맵에 CSV를 적용할 수 있는지 확인
    /// </summary>
    /// <returns>메뉴 활성화 여부</returns>
    [MenuItem( MenuPath, true )]
    static bool ValidateImport ()
    {
        return Selection.activeObject is AchvDataMap;
    }

    /// <summary>
    /// 선택한 업적 데이터맵과 같은 폴더에 CSV 업적 데이터 생성
    /// </summary>
    [MenuItem( MenuPath )]
    static void Import ()
    {
        AchvDataMap dataMap = Selection.activeObject as AchvDataMap;
        string sourcePath = EditorUtility.OpenFilePanel(
            "Achievements.csv 선택", string.Empty, "csv" );

        if ( string.IsNullOrEmpty( sourcePath ) ) return;

        try
        {
            ValidateMainFileName( sourcePath );

            string sourceFolder = Path.GetDirectoryName( sourcePath );
            string stagePath = Path.Combine( sourceFolder, StageFileName );
            string rewardPath = Path.Combine( sourceFolder, RewardFileName );

            List<AchievementValues> values = ReadValues(
                sourcePath, stagePath, rewardPath );

            string outputFolder = GetOutputFolder( dataMap );
            Dictionary<string, AchvData> existingDatas =
                FindExistingDatas( outputFolder );

            var generatedDatas = new List<AchvData>( values.Count );
            int createdCount = 0;
            int updatedCount = 0;

            AssetDatabase.StartAssetEditing( );

            try
            {
                for ( int i = 0; i < values.Count; i++ )
                {
                    AchievementValues current = values [ i ];
                    AchvData data = CreateOrLoadData(
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
                $"업적 CSV 적용 완료, 생성 {createdCount}개, " +
                $"갱신 {updatedCount}개, 데이터맵 등록 {generatedDatas.Count}개" );
        }
        catch ( Exception exception )
        {
            Debug.LogError( $"업적 CSV 가져오기 실패: {exception.Message}" );
        }
    }

    /// <summary>
    /// 선택 파일이 업적 기본 CSV인지 확인
    /// </summary>
    /// <param name="sourcePath">선택한 CSV 경로</param>
    static void ValidateMainFileName ( string sourcePath )
    {
        if ( Path.GetFileName( sourcePath ).Equals(
            AchievementFileName, StringComparison.OrdinalIgnoreCase ) )
        {
            return;
        }

        throw new InvalidDataException(
            $"{AchievementFileName} 파일을 선택해 주세요." );
    }

    /// <summary>
    /// 업적·단계·보상 CSV를 읽어 업적 생성 값 구성
    /// </summary>
    /// <param name="achievementPath">업적 CSV 경로</param>
    /// <param name="stagePath">단계 CSV 경로</param>
    /// <param name="rewardPath">보상 CSV 경로</param>
    /// <returns>검증이 끝난 업적 생성 값 목록</returns>
    static List<AchievementValues> ReadValues (
        string achievementPath, string stagePath, string rewardPath )
    {
        CsvTable achievements = ReadTable(
            achievementPath,
            "Id", "Category", "DisplayName", "Description",
            "IsHidden", "ProgressType", "Status" );

        CsvTable stages = ReadTable(
            stagePath,
            "AchievementId", "StageIndex", "TargetValue",
            "DisplayName", "Description" );

        CsvTable rewards = ReadTable(
            rewardPath,
            "AchievementId", "StageIndex", "RewardIndex",
            "RewardType", "Amount", "TargetId", "Status" );

        List<AchievementValues> values = ReadAchievements( achievements );
        Dictionary<string, AchievementValues> valueMap =
            BuildAchievementMap( values );

        ReadStages( stages, valueMap );
        ReadRewards( rewards, valueMap );
        ValidateValues( values );

        return values;
    }

    /// <summary>
    /// 확정 상태의 업적 기본값 읽기
    /// </summary>
    /// <param name="table">업적 CSV 표</param>
    /// <returns>업적 생성 값 목록</returns>
    static List<AchievementValues> ReadAchievements ( CsvTable table )
    {
        var values = new List<AchievementValues>( );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            List<string> row = table.Rows [ i ];

            if ( IsConfirmed( GetValue( table, row, "Status" ) ) == false )
                continue;

            string id = GetRequiredValue( table, row, "Id", i );

            var current = new AchievementValues
            {
                Id = id,
                Category = ParseCategory(
                    GetRequiredValue( table, row, "Category", i ), i ),
                DisplayName = GetRequiredValue(
                    table, row, "DisplayName", i ),
                Description = GetValue( table, row, "Description" ),
                IsHidden = ParseBool(
                    GetRequiredValue( table, row, "IsHidden", i ), i ),
                ProgressType = ParseProgressType(
                    GetRequiredValue( table, row, "ProgressType", i ), i )
            };

            values.Add( current );
        }

        return values;
    }

    /// <summary>
    /// 업적 단계값 읽기
    /// </summary>
    /// <param name="table">단계 CSV 표</param>
    /// <param name="valueMap">업적 아이디별 생성 값</param>
    static void ReadStages (
        CsvTable table,
        IReadOnlyDictionary<string, AchievementValues> valueMap )
    {
        var stageKeys = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            List<string> row = table.Rows [ i ];
            string achievementId = GetRequiredValue(
                table, row, "AchievementId", i );

            if ( valueMap.TryGetValue(
                achievementId, out AchievementValues achievement ) == false )
            {
                throw CreateRowError(
                    i, $"업적을 찾을 수 없는 단계: {achievementId}" );
            }

            int stageIndex = ParsePositiveInt(
                GetRequiredValue( table, row, "StageIndex", i ), i );
            string stageKey = $"{achievementId}:{stageIndex}";

            if ( stageKeys.Add( stageKey ) == false )
                throw CreateRowError( i, $"중복 업적 단계: {stageKey}" );

            achievement.Stages.Add( new StageValues
            {
                StageIndex = stageIndex,
                TargetValue = ParsePositiveInt(
                    GetRequiredValue( table, row, "TargetValue", i ), i ),
                DisplayName = GetValue( table, row, "DisplayName" ),
                Description = GetValue( table, row, "Description" )
            } );
        }
    }

    /// <summary>
    /// 확정 상태의 업적 보상값 읽기
    /// </summary>
    /// <param name="table">보상 CSV 표</param>
    /// <param name="valueMap">업적 아이디별 생성 값</param>
    static void ReadRewards (
        CsvTable table,
        IReadOnlyDictionary<string, AchievementValues> valueMap )
    {
        var rewardKeys = new HashSet<string>( StringComparer.Ordinal );

        for ( int i = 0; i < table.Rows.Count; i++ )
        {
            List<string> row = table.Rows [ i ];
            string status = GetValue( table, row, "Status" );

            //대상이 미정인 보상은 임의 아이디 없이 제외
            if ( status.Equals(
                "TargetPending", StringComparison.OrdinalIgnoreCase ) )
            {
                continue;
            }

            if ( IsConfirmed( status ) == false )
                throw CreateRowError( i, $"알 수 없는 보상 상태: {status}" );

            string achievementId = GetRequiredValue(
                table, row, "AchievementId", i );
            int stageIndex = ParsePositiveInt(
                GetRequiredValue( table, row, "StageIndex", i ), i );
            int rewardIndex = ParsePositiveInt(
                GetRequiredValue( table, row, "RewardIndex", i ), i );

            StageValues stage = FindStage(
                valueMap, achievementId, stageIndex, i );
            string rewardKey =
                $"{achievementId}:{stageIndex}:{rewardIndex}";

            if ( rewardKeys.Add( rewardKey ) == false )
                throw CreateRowError( i, $"중복 업적 보상: {rewardKey}" );

            if ( Enum.TryParse(
                GetRequiredValue( table, row, "RewardType", i ), true,
                out AchvRewardType rewardType ) == false )
            {
                throw CreateRowError( i, $"알 수 없는 보상 종류: {rewardKey}" );
            }

            var reward = new RewardValues
            {
                RewardIndex = rewardIndex,
                Type = rewardType,
                Amount = ParseOptionalFloat(
                    GetValue( table, row, "Amount" ), i ),
                TargetId = GetValue( table, row, "TargetId" )
            };

            ValidateReward( reward, rewardKey, i );
            stage.Rewards.Add( reward );
        }
    }

    /// <summary>
    /// 업적 생성 값 전체 정렬과 단계 규칙 검증
    /// </summary>
    /// <param name="values">업적 생성 값 목록</param>
    static void ValidateValues ( IReadOnlyList<AchievementValues> values )
    {
        for ( int i = 0; i < values.Count; i++ )
        {
            AchievementValues achievement = values [ i ];
            achievement.Stages.Sort(
                ( left, right ) => left.StageIndex.CompareTo( right.StageIndex ) );

            if ( achievement.Stages.Count == 0 )
                throw new InvalidDataException(
                    $"업적 단계가 없습니다: {achievement.Id}" );

            int previousTarget = 0;

            for ( int j = 0; j < achievement.Stages.Count; j++ )
            {
                StageValues stage = achievement.Stages [ j ];

                if ( stage.StageIndex != j + 1 )
                    throw new InvalidDataException(
                        $"업적 단계 번호가 연속되지 않습니다: " +
                        $"{achievement.Id}, {stage.StageIndex}" );

                if ( stage.TargetValue <= previousTarget )
                    throw new InvalidDataException(
                        $"업적 목표값은 이전 단계보다 커야 합니다: " +
                        $"{achievement.Id}, {stage.StageIndex}" );

                stage.Rewards.Sort(
                    ( left, right ) =>
                        left.RewardIndex.CompareTo( right.RewardIndex ) );
                previousTarget = stage.TargetValue;
            }
        }
    }

    /// <summary>
    /// 업적 아이디 조회 맵 생성과 중복 검증
    /// </summary>
    /// <param name="values">업적 생성 값 목록</param>
    /// <returns>업적 아이디별 생성 값</returns>
    static Dictionary<string, AchievementValues> BuildAchievementMap (
        IReadOnlyList<AchievementValues> values )
    {
        var map = new Dictionary<string, AchievementValues>(
            StringComparer.Ordinal );

        for ( int i = 0; i < values.Count; i++ )
        {
            if ( map.TryAdd( values [ i ].Id, values [ i ] ) == false )
                throw new InvalidDataException(
                    $"중복 업적 아이디: {values [ i ].Id}" );
        }

        return map;
    }

    /// <summary>
    /// 업적 단계 조회
    /// </summary>
    /// <param name="valueMap">업적 아이디별 생성 값</param>
    /// <param name="achievementId">업적 아이디</param>
    /// <param name="stageIndex">단계 번호</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>조회한 업적 단계</returns>
    static StageValues FindStage (
        IReadOnlyDictionary<string, AchievementValues> valueMap,
        string achievementId, int stageIndex, int rowIndex )
    {
        if ( valueMap.TryGetValue(
            achievementId, out AchievementValues achievement ) == false )
        {
            throw CreateRowError(
                rowIndex, $"업적을 찾을 수 없는 보상: {achievementId}" );
        }

        for ( int i = 0; i < achievement.Stages.Count; i++ )
        {
            if ( achievement.Stages [ i ].StageIndex == stageIndex )
                return achievement.Stages [ i ];
        }

        throw CreateRowError(
            rowIndex, $"단계를 찾을 수 없는 보상: " +
            $"{achievementId}, {stageIndex}" );
    }

    /// <summary>
    /// 업적 보상 종류별 필수값 검증
    /// </summary>
    /// <param name="reward">검증할 보상 값</param>
    /// <param name="rewardKey">보상 식별 키</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    static void ValidateReward (
        RewardValues reward, string rewardKey, int rowIndex )
    {
        if ( reward.Type == AchvRewardType.Gold && reward.Amount <= 0f )
            throw CreateRowError(
                rowIndex, $"골드 보상 수치가 올바르지 않습니다: {rewardKey}" );

        if ( reward.Type != AchvRewardType.None &&
            reward.Type != AchvRewardType.Gold &&
            string.IsNullOrWhiteSpace( reward.TargetId ) )
        {
            throw CreateRowError(
                rowIndex, $"보상 대상 아이디가 없습니다: {rewardKey}" );
        }
    }

    /// <summary>
    /// 현재 코드 enum에 맞춰 업적 진행도 종류 변환
    /// </summary>
    /// <param name="value">CSV 진행도 종류</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>업적 진행도 종류</returns>
    static AchvProgressType ParseProgressType (
        string value, int rowIndex )
    {
        switch ( value )
        {
            case "CraftCompleteCount":
                return AchvProgressType.CraftCount;

            case "NormalThemeSynergyCount":
                return AchvProgressType.ActivatedThemeCraftCount;

            case "CompleteThemeCount":
                return AchvProgressType.CompleteThemeCraftCount;

            case "ThemeActiveCraftCount":
                return AchvProgressType.ActivatedThemeCraftCount;
        }

        if ( Enum.TryParse(
            value, true, out AchvProgressType progressType ) )
        {
            return progressType;
        }

        throw CreateRowError( rowIndex, $"알 수 없는 진행도 종류: {value}" );
    }

    /// <summary>
    /// 현재 코드 enum에 맞춰 업적 분류 변환
    /// </summary>
    /// <param name="value">CSV 업적 분류</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>업적 분류</returns>
    static AchvCategory ParseCategory ( string value, int rowIndex )
    {
        //CSV의 Other는 현재 프로젝트의 기타 분류인 Etc로 연결
        if ( value.Equals( "Other", StringComparison.OrdinalIgnoreCase ) )
            return AchvCategory.Etc;

        if ( Enum.TryParse( value, true, out AchvCategory category ) )
            return category;

        throw CreateRowError( rowIndex, $"알 수 없는 업적 분류: {value}" );
    }

    /// <summary>
    /// 업적 데이터맵 폴더에 있는 기존 업적 에셋 조회
    /// </summary>
    /// <param name="outputFolder">업적 출력 폴더</param>
    /// <returns>업적 아이디별 기존 에셋</returns>
    static Dictionary<string, AchvData> FindExistingDatas (
        string outputFolder )
    {
        var datas = new Dictionary<string, AchvData>( StringComparer.Ordinal );
        string [ ] guids = AssetDatabase.FindAssets(
            "t:AchvData", new [ ] { outputFolder } );

        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath( guids [ i ] );
            AchvData data = AssetDatabase.LoadAssetAtPath<AchvData>( path );

            if ( data == null || string.IsNullOrWhiteSpace( data.Id ) )
                continue;

            if ( datas.TryAdd( data.Id, data ) == false )
                throw new InvalidDataException(
                    $"기존 업적 에셋 아이디가 중복되었습니다: {data.Id}" );
        }

        return datas;
    }

    /// <summary>
    /// 업적 에셋을 아이디로 조회하거나 새로 생성
    /// </summary>
    /// <param name="id">업적 아이디</param>
    /// <param name="outputFolder">업적 출력 폴더</param>
    /// <param name="existingDatas">기존 업적 에셋</param>
    /// <param name="createdCount">생성 수</param>
    /// <param name="updatedCount">갱신 수</param>
    /// <returns>생성 또는 조회한 업적 에셋</returns>
    static AchvData CreateOrLoadData (
        string id, string outputFolder,
        IReadOnlyDictionary<string, AchvData> existingDatas,
        ref int createdCount, ref int updatedCount )
    {
        if ( existingDatas.TryGetValue( id, out AchvData data ) )
        {
            Undo.RecordObject( data, "업적 CSV 데이터 갱신" );
            updatedCount++;
            return data;
        }

        string assetPath = $"{outputFolder}/{id}.asset";

        if ( AssetDatabase.LoadMainAssetAtPath( assetPath ) != null )
            throw new InvalidOperationException(
                $"같은 경로에 다른 에셋이 있습니다: {assetPath}" );

        data = ScriptableObject.CreateInstance<AchvData>( );
        data.name = id;
        AssetDatabase.CreateAsset( data, assetPath );
        createdCount++;
        return data;
    }

    /// <summary>
    /// 업적 에셋에 CSV 값 적용
    /// </summary>
    /// <param name="data">적용할 업적 에셋</param>
    /// <param name="values">업적 생성 값</param>
    static void UpdateData ( AchvData data, AchievementValues values )
    {
        var serializedData = new SerializedObject( data );
        serializedData.FindProperty( "_id" ).stringValue = values.Id;
        serializedData.FindProperty( "_category" ).enumValueIndex =
            ( int ) values.Category;
        serializedData.FindProperty( "_displayName" ).stringValue =
            values.DisplayName;
        serializedData.FindProperty( "_description" ).stringValue =
            values.Description;
        serializedData.FindProperty( "_isHidden" ).boolValue =
            values.IsHidden;
        serializedData.FindProperty( "_progressType" ).enumValueIndex =
            ( int ) values.ProgressType;

        SerializedProperty stages = serializedData.FindProperty( "_stages" );
        stages.arraySize = values.Stages.Count;

        for ( int i = 0; i < values.Stages.Count; i++ )
            UpdateStage( stages.GetArrayElementAtIndex( i ), values.Stages [ i ] );

        serializedData.ApplyModifiedProperties( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// 직렬화된 업적 단계에 CSV 값 적용
    /// </summary>
    /// <param name="stage">직렬화된 업적 단계</param>
    /// <param name="values">업적 단계 생성 값</param>
    static void UpdateStage ( SerializedProperty stage, StageValues values )
    {
        stage.FindPropertyRelative( "_targetValue" ).intValue =
            values.TargetValue;
        stage.FindPropertyRelative( "_displayName" ).stringValue =
            values.DisplayName;
        stage.FindPropertyRelative( "_description" ).stringValue =
            values.Description;

        SerializedProperty rewards =
            stage.FindPropertyRelative( "_rewards" );
        rewards.arraySize = values.Rewards.Count;

        for ( int i = 0; i < values.Rewards.Count; i++ )
        {
            RewardValues rewardValues = values.Rewards [ i ];
            SerializedProperty reward = rewards.GetArrayElementAtIndex( i );

            reward.FindPropertyRelative( "_type" ).enumValueIndex =
                ( int ) rewardValues.Type;
            reward.FindPropertyRelative( "_amount" ).floatValue =
                rewardValues.Amount;
            reward.FindPropertyRelative( "_targetId" ).stringValue =
                rewardValues.TargetId;
        }
    }

    /// <summary>
    /// 생성한 업적 목록으로 선택 데이터맵 갱신
    /// </summary>
    /// <param name="dataMap">선택한 업적 데이터맵</param>
    /// <param name="datas">생성 또는 갱신한 업적 데이터</param>
    static void UpdateDataMap (
        AchvDataMap dataMap, IReadOnlyList<AchvData> datas )
    {
        Undo.RecordObject( dataMap, "업적 CSV 데이터맵 갱신" );

        var serializedMap = new SerializedObject( dataMap );
        SerializedProperty property = serializedMap.FindProperty( "_datas" );
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
    /// 선택 데이터맵의 에셋 폴더 조회
    /// </summary>
    /// <param name="dataMap">선택한 업적 데이터맵</param>
    /// <returns>업적 에셋 출력 폴더</returns>
    static string GetOutputFolder ( AchvDataMap dataMap )
    {
        string mapPath = AssetDatabase.GetAssetPath( dataMap );
        string folder = Path.GetDirectoryName( mapPath )?.Replace( '\\', '/' );

        if ( string.IsNullOrEmpty( folder ) ||
            AssetDatabase.IsValidFolder( folder ) == false )
        {
            throw new DirectoryNotFoundException(
                "선택한 업적 데이터맵의 폴더를 찾을 수 없습니다." );
        }

        return folder;
    }

    /// <summary>
    /// 공유 중인 파일도 읽을 수 있도록 CSV 표 생성
    /// </summary>
    /// <param name="path">CSV 파일 경로</param>
    /// <param name="requiredHeaders">필수 헤더 목록</param>
    /// <returns>CSV 표</returns>
    static CsvTable ReadTable ( string path, params string [ ] requiredHeaders )
    {
        if ( File.Exists( path ) == false )
            throw new FileNotFoundException(
                $"필수 CSV 파일이 없습니다: {Path.GetFileName( path )}" );

        List<List<string>> rows = ParseCsv( ReadTextShared( path ) );

        if ( rows.Count == 0 )
            throw new InvalidDataException(
                $"CSV가 비어 있습니다: {Path.GetFileName( path )}" );

        var columns = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase );

        for ( int i = 0; i < rows [ 0 ].Count; i++ )
        {
            string header = rows [ 0 ] [ i ].Trim( '\uFEFF', ' ' );

            if ( columns.TryAdd( header, i ) == false )
                throw new InvalidDataException(
                    $"중복 CSV 헤더: {header}" );
        }

        for ( int i = 0; i < requiredHeaders.Length; i++ )
        {
            if ( columns.ContainsKey( requiredHeaders [ i ] ) == false )
                throw new InvalidDataException(
                    $"필수 CSV 헤더가 없습니다: {requiredHeaders [ i ]}" );
        }

        rows.RemoveAt( 0 );
        rows.RemoveAll( IsEmptyRow );

        return new CsvTable { Columns = columns, Rows = rows };
    }

    /// <summary>
    /// 다른 프로그램이 연 CSV 파일을 공유 읽기로 로드
    /// </summary>
    /// <param name="path">CSV 파일 경로</param>
    /// <returns>CSV 전체 문자열</returns>
    static string ReadTextShared ( string path )
    {
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
        using var reader = new StreamReader(
            stream, Encoding.UTF8, true );
        return reader.ReadToEnd( );
    }

    /// <summary>
    /// 따옴표와 줄바꿈을 지원해 CSV 문자열 분리
    /// </summary>
    /// <param name="text">CSV 전체 문자열</param>
    /// <returns>CSV 행과 열</returns>
    static List<List<string>> ParseCsv ( string text )
    {
        var rows = new List<List<string>>( );
        var row = new List<string>( );
        var field = new StringBuilder( );
        bool isQuoted = false;

        for ( int i = 0; i < text.Length; i++ )
        {
            char current = text [ i ];

            if ( current == '"' )
            {
                if ( isQuoted && i + 1 < text.Length && text [ i + 1 ] == '"' )
                {
                    field.Append( '"' );
                    i++;
                }
                else
                {
                    isQuoted = isQuoted == false;
                }

                continue;
            }

            if ( current == ',' && isQuoted == false )
            {
                row.Add( field.ToString( ) );
                field.Clear( );
                continue;
            }

            if ( current == '\n' && isQuoted == false )
            {
                row.Add( field.ToString( ).TrimEnd( '\r' ) );
                field.Clear( );
                rows.Add( row );
                row = new List<string>( );
                continue;
            }

            field.Append( current );
        }

        if ( isQuoted )
            throw new InvalidDataException( "닫히지 않은 CSV 따옴표가 있습니다." );

        if ( field.Length > 0 || row.Count > 0 )
        {
            row.Add( field.ToString( ).TrimEnd( '\r' ) );
            rows.Add( row );
        }

        return rows;
    }

    /// <summary>
    /// CSV 행이 모두 공백인지 확인
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
    /// CSV 열 값 조회
    /// </summary>
    /// <param name="table">CSV 표</param>
    /// <param name="row">CSV 행</param>
    /// <param name="column">열 이름</param>
    /// <returns>공백을 제거한 열 값</returns>
    static string GetValue (
        CsvTable table, IReadOnlyList<string> row, string column )
    {
        int index = table.Columns [ column ];
        return index < row.Count ? row [ index ].Trim( ) : string.Empty;
    }

    /// <summary>
    /// 필수 CSV 열 값 조회
    /// </summary>
    /// <param name="table">CSV 표</param>
    /// <param name="row">CSV 행</param>
    /// <param name="column">열 이름</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>공백을 제거한 필수 값</returns>
    static string GetRequiredValue (
        CsvTable table, IReadOnlyList<string> row,
        string column, int rowIndex )
    {
        string value = GetValue( table, row, column );

        if ( string.IsNullOrWhiteSpace( value ) )
            throw CreateRowError( rowIndex, $"필수 값이 없습니다: {column}" );

        return value;
    }

    /// <summary>
    /// Confirmed 상태 확인
    /// </summary>
    /// <param name="status">CSV 상태값</param>
    /// <returns>확정 상태 여부</returns>
    static bool IsConfirmed ( string status )
    {
        return status.Equals(
            "Confirmed", StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>
    /// CSV bool 값 변환
    /// </summary>
    /// <param name="value">CSV 값</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>변환한 bool 값</returns>
    static bool ParseBool ( string value, int rowIndex )
    {
        if ( bool.TryParse( value, out bool result ) )
            return result;

        throw CreateRowError( rowIndex, $"bool 값이 올바르지 않습니다: {value}" );
    }

    /// <summary>
    /// CSV 양의 정수값 변환
    /// </summary>
    /// <param name="value">CSV 값</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>변환한 양의 정수값</returns>
    static int ParsePositiveInt ( string value, int rowIndex )
    {
        if ( int.TryParse(
            value, NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int result ) && result > 0 )
        {
            return result;
        }

        throw CreateRowError( rowIndex, $"양의 정수가 아닙니다: {value}" );
    }

    /// <summary>
    /// 선택 입력인 CSV 실수값 변환
    /// </summary>
    /// <param name="value">CSV 값</param>
    /// <param name="rowIndex">CSV 행 번호</param>
    /// <returns>변환한 실수값, 공백이면 0</returns>
    static float ParseOptionalFloat ( string value, int rowIndex )
    {
        if ( string.IsNullOrWhiteSpace( value ) )
            return 0f;

        if ( float.TryParse(
            value, NumberStyles.Float, CultureInfo.InvariantCulture,
            out float result ) )
        {
            return result;
        }

        throw CreateRowError( rowIndex, $"실수값이 올바르지 않습니다: {value}" );
    }

    /// <summary>
    /// CSV 실제 행 번호를 포함한 오류 생성
    /// </summary>
    /// <param name="rowIndex">헤더를 제외한 행 번호</param>
    /// <param name="message">오류 내용</param>
    /// <returns>CSV 행 오류</returns>
    static InvalidDataException CreateRowError (
        int rowIndex, string message )
    {
        return new InvalidDataException(
            $"{rowIndex + 2}행: {message}" );
    }
}
