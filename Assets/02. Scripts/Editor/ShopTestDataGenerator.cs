using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 상점 테스트 데이터 생성기 - 상품 데이터 복제 및 데이터 맵 등록
/// </summary>
public class ShopTestDataGenerator : EditorWindow
{
    PurchasableData _sourceData;       //복제할 원본 상품 데이터
    PurchasableDataMap _dataMap;       //복제 상품을 등록할 데이터 맵
    int _createCount = 12;      //생성 수량

    /// <summary>
    /// 테스트 데이터 생성기 열기
    /// </summary>
    [MenuItem( "Tools/상점/테스트 데이터 생성" )]
    static void OpenWindow ()
    {
        //생성기 창 열기
        GetWindow<ShopTestDataGenerator>( "상점 테스트 데이터" );
    }

    /// <summary>
    /// 테스트 데이터 생성기 표시
    /// </summary>
    void OnGUI ()
    {
        //테스트 데이터 설정 표시
        _sourceData = ( PurchasableData ) EditorGUILayout.ObjectField( "원본 상품 데이터", _sourceData, typeof( PurchasableData ), false );
        _dataMap = ( PurchasableDataMap ) EditorGUILayout.ObjectField( "상품 데이터 맵", _dataMap, typeof( PurchasableDataMap ), false );
        _createCount = EditorGUILayout.IntSlider( "생성 수량", _createCount, 1, 50 );

        //필수 데이터가 없으면 생성 버튼 비활성화
        using ( new EditorGUI.DisabledScope( _sourceData == null || _dataMap == null ) )
        {
            //테스트 데이터 생성
            if ( GUILayout.Button( "테스트 데이터 생성" ) ) Generate( );
        }
    }

    /// <summary>
    /// 테스트 상품 데이터 생성
    /// </summary>
    void Generate ()
    {
        //원본 상품 아이디 확인
        if ( string.IsNullOrWhiteSpace( _sourceData.Id ) )
        {
            Debug.LogWarning( "원본 상품 아이디가 없습니다." );
            return;
        }

        //테스트 데이터 폴더 생성
        const string parentFolder = "Assets/03. Configs";
        const string outputFolder = parentFolder + "/ShopTestData";

        if ( AssetDatabase.IsValidFolder( outputFolder ) == false ) AssetDatabase.CreateFolder( parentFolder, "ShopTestData" );

        //생성 배치 구분값 설정
        string batchId = DateTime.Now.ToString( "yyMMddHHmmss" );
        List<PurchasableData> generatedDatas = new List<PurchasableData>( _createCount );

        //상품 데이터 복제
        for ( int i = 0; i < _createCount; i++ )
        {
            string index = ( i + 1 ).ToString( "00" );
            string assetPath = $"{outputFolder}/{_sourceData.name}_Test_{batchId}_{index}.asset";

            //원본 상품 데이터 복제
            if ( AssetDatabase.CopyAsset( AssetDatabase.GetAssetPath( _sourceData ), assetPath ) == false ) continue;

            //복제한 상품 데이터 조회
            PurchasableData generatedData = AssetDatabase.LoadAssetAtPath<PurchasableData>( assetPath );

            if ( generatedData == null ) continue;

            //복제 상품 표시값 설정
            SetTestValues( generatedData, batchId, i );
            generatedDatas.Add( generatedData );
        }

        //생성한 상품을 데이터 맵에 등록
        AddToDataMap( generatedDatas );

        //변경 내용 저장 및 갱신
        AssetDatabase.SaveAssets( );
        AssetDatabase.Refresh( );

        Debug.Log( $"상점 테스트 데이터 {generatedDatas.Count}개 생성 완료" );
    }

    /// <summary>
    /// 테스트 상품 표시값 설정
    /// </summary>
    /// <param name="data">복제한 상품 데이터</param>
    /// <param name="batchId">생성 배치 구분값</param>
    /// <param name="index">생성 순서</param>
    void SetTestValues ( PurchasableData data, string batchId, int index )
    {
        //복제 상품 직렬화 데이터 조회
        SerializedObject serializedData = new SerializedObject( data );
        serializedData.Update( );

        //상품 아이디와 이름 설정
        serializedData.FindProperty( "_id" ).stringValue = $"{_sourceData.Id}_TEST_{batchId}_{index + 1:00}";
        serializedData.FindProperty( "_name" ).stringValue = index == _createCount - 1 ? $"{_sourceData.Name} 아주 긴 상품 이름 표시 테스트 {index + 1}" : $"{_sourceData.Name} 테스트 {index + 1}";

        //가격과 재고 경곗값 설정
        float [ ] testPrices = { 0.0f, 999.0f, 1000.0f, 9999.0f, 100000.0f, 999999.9f };
        int [ ] testStocks = { 0, 1, 10, 99 };

        serializedData.FindProperty( "_basePrice" ).floatValue = testPrices [ index % testPrices.Length ];
        serializedData.FindProperty( "_baseStockQuantity" ).intValue = testStocks [ index % testStocks.Length ];

        //복제 상품 변경값 적용
        serializedData.ApplyModifiedPropertiesWithoutUndo( );
        EditorUtility.SetDirty( data );
    }

    /// <summary>
    /// 생성한 상품을 데이터 맵에 등록
    /// </summary>
    /// <param name="generatedDatas">생성한 상품 데이터 목록</param>
    void AddToDataMap ( IReadOnlyList<PurchasableData> generatedDatas )
    {
        //등록할 상품이 없으면 종료
        if ( generatedDatas == null || generatedDatas.Count == 0 ) return;

        //데이터 맵 직렬화 배열 조회
        SerializedObject serializedMap = new SerializedObject( _dataMap );
        serializedMap.Update( );
        SerializedProperty datasProperty = serializedMap.FindProperty( "_datas" );
        int startIndex = datasProperty.arraySize;

        //생성한 상품 수만큼 배열 확장
        datasProperty.arraySize += generatedDatas.Count;

        //생성한 상품 데이터 등록
        for ( int i = 0; i < generatedDatas.Count; i++ )
        {
            datasProperty.GetArrayElementAtIndex( startIndex + i ).objectReferenceValue = generatedDatas [ i ];
        }

        //데이터 맵 변경값 적용
        serializedMap.ApplyModifiedPropertiesWithoutUndo( );
        EditorUtility.SetDirty( _dataMap );
    }
}
