using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// 저장 파일 처리기 - JSON 저장 파일의 읽기, 쓰기, 검증과 삭제 담당
/// </summary>
public class SaveFileHandler
{
    public const int CurrentVersion = 1;
    public const string InvalidDataMessage =
        "저장 데이터가 올바르지 않습니다.";

    readonly SavePathProvider _pathProvider;       //저장 파일 경로 제공자

    /// <summary>
    /// 저장 파일 처리기 생성
    /// </summary>
    /// <param name="pathProvider">저장 파일 경로 제공자</param>
    public SaveFileHandler ( SavePathProvider pathProvider )
    {
        _pathProvider = pathProvider;
    }

    #region ----- 슬롯 정보 -----

    /// <summary>
    /// 지정한 슬롯의 목록 표시 정보 반환
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <returns>저장 슬롯 표시 데이터</returns>
    public SaveSlotData GetSlotData ( int slotNumber )
    {
        //사용할 수 있는지 슬롯 번호 검사
        if ( _pathProvider.IsValidSlot( slotNumber ) == false )
        {
            return SaveSlotData.CreateCorrupted(
                slotNumber, "유효하지 않은 슬롯입니다." );
        }

        //저장 결과 가져오기
        SaveResult result = Read( slotNumber, out SaveFileData saveFileData );

        //결과가 없으면 빈 슬롯 데이터 생성
        if ( result == SaveResult.NotFound )
            return SaveSlotData.CreateEmpty( slotNumber );

        //실패 시
        if ( result != SaveResult.Success )
        {
            return SaveSlotData.CreateCorrupted(
                slotNumber, GetErrorMessage( result ) );
        }

        //저장 시각 가져오기
        DateTime.TryParse(
            saveFileData.Data.SavedAtUtc,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTime savedAtUtc );

        //저장 데이터 생성
        return SaveSlotData.CreateValid(
            saveFileData.Data, savedAtUtc.ToUniversalTime( ) );
    }

    #endregion

    #region ----- 일반 저장 -----

    /// <summary>
    /// 일반 저장 파일 조회
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <param name="saveFileData">조회한 저장 파일 데이터</param>
    /// <returns>저장 파일 조회 결과</returns>
    public SaveResult Read ( int slotNumber, out SaveFileData saveFileData )
    {
        //저장 경로 가져오기
        string path = _pathProvider.GetSaveFilePath( slotNumber );
        //저장 파일 데이터 반환
        return ReadFile( slotNumber, path, out saveFileData );
    }

    /// <summary>
    /// 일반 저장 파일을 임시 파일에 검증한 뒤 교체
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    /// <param name="saveFileData">기록할 저장 파일 데이터</param>
    /// <returns>저장 파일 기록 결과</returns>
    public SaveResult Write ( int slotNumber, SaveFileData saveFileData )
    {
        //저장 경로 가져오기
        string path = _pathProvider.GetSaveFilePath( slotNumber );
        //저장 파일 데이터 쓰기
        return WriteFile( slotNumber, path, saveFileData );
    }

    /// <summary>
    /// 저장 파일 삭제
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    /// <returns>저장 파일 삭제 결과</returns>
    public SaveResult Delete ( int slotNumber )
    {
        //저장 경로 가져오기
        string path = _pathProvider.GetSaveFilePath( slotNumber );
        //저장 파일 삭제
        return DeleteFile( slotNumber, path );
    }

    #endregion

    #region ----- 주간 복구 저장 -----

    /// <summary>
    /// 주간 복구 파일 조회
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <param name="saveFileData">조회한 주간 복구 파일 데이터</param>
    /// <returns>주간 복구 파일 조회 결과</returns>
    public SaveResult ReadWeekSnapshot (
        int slotNumber, out SaveFileData saveFileData )
    {
        //저장 경로 가져오기
        string path = _pathProvider.GetWeekFilePath( slotNumber );
        return ReadFile( slotNumber, path, out saveFileData );
    }

    /// <summary>
    /// 주간 복구 파일을 임시 파일에 검증한 뒤 교체
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    /// <param name="saveFileData">기록할 주간 복구 파일 데이터</param>
    /// <returns>주간 복구 파일 기록 결과</returns>
    public SaveResult WriteWeekSnapshot (
        int slotNumber, SaveFileData saveFileData )
    {
        //저장 경로 가져오기
        string path = _pathProvider.GetWeekFilePath( slotNumber );
        return WriteFile( slotNumber, path, saveFileData );
    }

    /// <summary>
    /// 주간 복구 파일 삭제
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    /// <returns>주간 복구 파일 삭제 결과</returns>
    public SaveResult DeleteWeekSnapshot ( int slotNumber )
    {
        //저장 경로 가져오기
        string path = _pathProvider.GetWeekFilePath( slotNumber );
        return DeleteFile( slotNumber, path );
    }

    #endregion

    #region ----- 파일 처리 -----

    /// <summary>
    /// 지정한 경로의 JSON 저장 파일을 읽고 검증
    /// </summary>
    /// <param name="slotNumber">조회할 슬롯 번호</param>
    /// <param name="path">조회할 파일 경로</param>
    /// <param name="saveFileData">조회한 저장 파일 데이터</param>
    /// <returns>저장 파일 조회 결과</returns>
    SaveResult ReadFile (
        int slotNumber, string path, out SaveFileData saveFileData )
    {
        //기본값 설정
        saveFileData = null;

        //슬롯 번호 검사
        if ( _pathProvider.IsValidSlot( slotNumber ) == false )
            return SaveResult.InvalidSlot;
        //저장 경로에 파일이 없다면
        if ( File.Exists( path ) == false )
            return SaveResult.NotFound;

        try
        {
            //저장 파일 가져오기
            string json = File.ReadAllText( path );
            saveFileData = JsonUtility.FromJson<SaveFileData>( json );

            //검사
            return Validate( slotNumber, saveFileData );
        }
        catch
        {
            //비우고 실패 결과 반환
            saveFileData = null;
            return SaveResult.ReadFailed;
        }
    }

    /// <summary>
    /// 임시 파일을 먼저 생성하고 다시 읽어 검증한 뒤 기존 파일 교체
    /// </summary>
    /// <param name="slotNumber">저장할 슬롯 번호</param>
    /// <param name="targetPath">교체할 저장 파일 경로</param>
    /// <param name="saveFileData">기록할 저장 파일 데이터</param>
    /// <returns>저장 파일 기록 결과</returns>
    SaveResult WriteFile (
        int slotNumber, string targetPath, SaveFileData saveFileData )
    {
        //저장 결과 검사
        SaveResult result = Validate( slotNumber, saveFileData );

        //실패 시 결과 반환
        if ( result != SaveResult.Success )
            return result;

        //임시 저장 경로
        string tempPath = _pathProvider.GetTempFilePath( targetPath );

        try
        {
            //저장 경로 만들기
            Directory.CreateDirectory( _pathProvider.SaveDirectory );

            //json 문자열로 변환
            string json = JsonUtility.ToJson( saveFileData, true );
            //쓰기
            File.WriteAllText( tempPath, json );

            //저장 결과 검사
            SaveResult readResult = ReadFile( slotNumber, tempPath, out _ );

            //결과가 성공이 아니라면
            if ( readResult != SaveResult.Success )
            {
                TryDeleteTempFile( tempPath );      //삭제 시도
                return SaveResult.WriteFailed;      //결과 반환
            }

            //저장 경로에 파일이 있다면
            if ( File.Exists( targetPath ) == true )
                File.Replace( tempPath, targetPath, null );

            else
                File.Move( tempPath, targetPath );


            return SaveResult.Success;
        }
        catch
        {
            TryDeleteTempFile( tempPath );
            return SaveResult.WriteFailed;
        }
    }

    /// <summary>
    /// 지정한 저장 파일 삭제
    /// </summary>
    /// <param name="slotNumber">삭제할 슬롯 번호</param>
    /// <param name="path">삭제할 저장 파일 경로</param>
    /// <returns>저장 파일 삭제 결과</returns>
    SaveResult DeleteFile ( int slotNumber, string path )
    {
        //슬롯 번호 확인
        if ( _pathProvider.IsValidSlot( slotNumber ) == false )
            return SaveResult.InvalidSlot;

        //경로에 파일이 없으면 종료
        if ( File.Exists( path ) == false )
            return SaveResult.NotFound;

        try
        {
            File.Delete( path );        //삭제 시도
            return SaveResult.Success;      //결과 반환
        }
        catch
        {
            return SaveResult.DeleteFailed;     //실패 반환
        }
    }

    /// <summary>
    /// 쓰기 실패 후 남은 임시 파일 정리
    /// </summary>
    /// <param name="tempPath">정리할 임시 파일 경로</param>
    void TryDeleteTempFile ( string tempPath )
    {
        try
        {
            //저장 경로에 파일이 있으면 삭제
            if ( File.Exists( tempPath ) == true )
                File.Delete( tempPath );

        }
        catch
        {
            Debug.LogWarning(
                $"저장 임시 파일을 정리하지 못했습니다: {tempPath}" );
        }
    }

    #endregion

    #region ----- 검증 -----

    /// <summary>
    /// 저장 파일의 기본 데이터와 영업일 정보 검증
    /// </summary>
    /// <param name="slotNumber">검증할 슬롯 번호</param>
    /// <param name="saveFileData">검증할 저장 파일 데이터</param>
    /// <returns>저장 데이터 검증 결과</returns>
    SaveResult Validate ( int slotNumber, SaveFileData saveFileData )
    {
        //슬롯 번호 검사
        if ( _pathProvider.IsValidSlot( slotNumber ) == false )
            return SaveResult.InvalidSlot;

        SaveData saveData = saveFileData?.Data;
        BusinessDaySaveData businessDayData = saveFileData?.BusinessDayData;

        InventorySaveData inventoryData = saveFileData?.InventoryData;
        ShopSaveData shopData = saveFileData?.ShopData;

        OrderSaveData orderData = saveFileData?.OrderData;
        OrderGeneratorSaveData orderGeneratorData = saveFileData?.OrderGeneratorData;
        CraftSaveData craftData = saveFileData?.CraftData;

        DeliverySaveData deliveryData = saveFileData?.DeliveryData;
        DailyRecordModelSaveData dailyRecordData = saveFileData?.DailyRecordData;

        SettlementSaveData settlementData = saveFileData?.SettlementData;

        MaintenanceSaveData maintenanceData = saveFileData?.MaintenanceData;
        EmployeeSaveData employeeData = saveFileData?.EmployeeData;

        AchvSaveData achvData = saveFileData?.AchvData;

        //필수 저장 데이터 검사
        if ( saveData == null || businessDayData == null ||
            achvData == null ||
            inventoryData == null || shopData == null ||
            orderData == null || orderGeneratorData == null ||
            craftData == null || deliveryData == null ||
            dailyRecordData == null || settlementData == null ||
            maintenanceData == null || employeeData == null )
        {
            return SaveResult.InvalidData;
        }

        //세이브 데이터 목록 검사
        if ( achvData.States == null ||
            craftData.Results == null ||
            deliveryData.Schedules == null ||
            deliveryData.Results == null ||
            dailyRecordData.CurrentRecord == null ||
            dailyRecordData.PastRecords == null ||
            settlementData.WeeklySettlements == null ||
            maintenanceData.States == null ||
            employeeData.States == null )
        {
            return SaveResult.InvalidData;
        }

        //공용 플레이 상태 검사
        if ( saveData.Version <= 0 ||
            saveData.SlotNumber != slotNumber ||
            saveData.Time < 0f ||
            float.IsNaN( saveData.Time ) ||
            float.IsInfinity( saveData.Time ) ||
            saveData.TotalDay < 1 ||
            saveData.HighGradeEvaluationCount < 0 ||
            saveData.AchievedAchvCount < 0 ||
            saveData.Budget < 0f ||
            float.IsNaN( saveData.Budget ) ||
            float.IsInfinity( saveData.Budget ) ||
            saveData.UnlockedIds == null )
        {
            return SaveResult.InvalidData;
        }

        var unlockedIds = new HashSet<string>( saveData.UnlockedIds );

        //확인 대기 해금은 실제 해금 상품에 포함되어야 함
        for ( int i = 0; i < saveData.NewUnlockedIds.Count; i++ )
        {
            if ( unlockedIds.Contains(
                saveData.NewUnlockedIds [ i ] ) == false )
            {
                return SaveResult.InvalidData;
            }
        }

        //영업일 상태 검사
        if ( IsValidBusinessDayData( businessDayData ) == false )
            return SaveResult.InvalidData;

        //인벤토리 상태 검사
        if ( IsValidInventoryData( inventoryData ) == false )
            return SaveResult.InvalidData;

        //상점 상태 검사
        if ( IsValidShopData( shopData ) == false )
            return SaveResult.InvalidData;

        //고객 주문 저장 상태 검사
        if ( IsValidOrderData( orderData ) == false )
            return SaveResult.InvalidData;

        //주문 생성 저장 상태 검사
        if ( IsValidOrderGeneratorData(
            orderGeneratorData ) == false )
        {
            return SaveResult.InvalidData;
        }

        //저장 시각 검사
        bool hasSavedTime = DateTime.TryParse(
            saveData.SavedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _ );

        if ( hasSavedTime == false )
            return SaveResult.InvalidData;

        return SaveResult.Success;
    }

    /// <summary>
    /// 영업일 저장 데이터의 값 범위 확인
    /// </summary>
    /// <param name="saveData">검증할 영업일 저장 데이터</param>
    /// <returns>영업일 저장 데이터 유효 여부</returns>
    bool IsValidBusinessDayData ( BusinessDaySaveData saveData )
    {
        //저장 데이터 검사
        if ( saveData.CraftLimit <= 0 ||
            saveData.CraftCount < 0 ||
            saveData.CraftCount > saveData.CraftLimit ||
            saveData.BaseCraftLimit <= 0 ||
            saveData.NextCraftLimit <= 0 ||
            saveData.EmployeeCraftBonus < 0 ||
            saveData.CraftLimit - saveData.EmployeeCraftBonus <= 0 ||
            saveData.NextCraftLimit - saveData.EmployeeCraftBonus <= 0 )
        {
            return false;
        }

        //빠른 재입고 횟수 검사
        if ( saveData.QuickRestockLimit <= 0 ||
            saveData.QuickRestockCount < 0 ||
            saveData.QuickRestockCount > saveData.QuickRestockLimit )
        {
            return false;
        }

        return Enum.IsDefined( typeof( DayEndReason ), saveData.EndReason );
    }

    /// <summary>
    /// 인벤토리 저장 데이터의 값 범위 확인
    /// </summary>
    /// <param name="saveData">검증할 인벤토리 저장 데이터</param>
    /// <returns>인벤토리 저장 데이터 유효 여부</returns>
    bool IsValidInventoryData ( InventorySaveData saveData )
    {
        //인벤토리 세이브 데이터 용량 및 보유 아이템 확인
        if ( saveData.Capacity <= 0 || saveData.Items == null )
        {
            return false;
        }

        var ids = new HashSet<string>( );

        foreach ( InventoryItemSaveData itemData in saveData.Items )
        {
            if ( itemData == null || string.IsNullOrWhiteSpace( itemData.Id ) == true ||
                itemData.Quantity <= 0 || ids.Add( itemData.Id ) == false )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 상점 저장 데이터의 값 범위 확인
    /// </summary>
    /// <param name="saveData">검증할 상점 저장 데이터</param>
    /// <returns>상점 저장 데이터 유효 여부</returns>
    bool IsValidShopData ( ShopSaveData saveData )
    {
        if ( saveData.Items == null ||
            saveData.PartMaxStockBonus < 0 ||
            ( saveData.PartRestockSpan != 0 &&
            saveData.PartRestockSpan < 3 ) )
        {
            return false;
        }

        var ids = new HashSet<string>( );

        foreach ( ShopItemSaveData itemData in saveData.Items )
        {
            if ( itemData == null ||
                string.IsNullOrWhiteSpace( itemData.Id ) == true ||
                ids.Add( itemData.Id ) == false ||
                itemData.CurrentPrice < 0f ||
                float.IsNaN( itemData.CurrentPrice ) == true ||
                float.IsInfinity( itemData.CurrentPrice ) == true ||
                itemData.MaxStock <= 0 ||
                itemData.RemainingStock < 0 ||
                itemData.RemainingStock > itemData.MaxStock ||
                itemData.RestockSpan < 0 ||
                itemData.NextRestockDay < 0 ||
                itemData.PendingRestockSpan < 0 )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 고객 주문 저장 데이터의 기본 값 범위 확인
    /// </summary>
    /// <param name="saveData">검증할 고객 주문 데이터</param>
    /// <returns>고객 주문 저장 데이터 유효 여부</returns>
    bool IsValidOrderData ( OrderSaveData saveData )
    {
        //데이터 확인
        if ( saveData.WaitingLimit < 1 ||
            saveData.WaitingLimit > CustomerOrderModel.MaxWaitingLimit ||
            saveData.NextCreatedNumber < 1 ||
            saveData.Orders == null ||
            Enum.IsDefined( typeof( OrderPenaltyType ), saveData.PenaltyType ) == false )
        {
            return false;
        }

        if ( saveData.PenaltyType == OrderPenaltyType.None )
        {
            if ( saveData.OrderReduction != 0 || saveData.PenaltyRemainingDays != 0 )
            {
                return false;
            }
        }
        else if ( saveData.OrderReduction <= 0 || saveData.PenaltyRemainingDays <= 0 )
        {
            return false;
        }

        var orderIds = new HashSet<string>( );
        var createdNumbers = new HashSet<int>( );

        foreach ( CustomerOrderSaveData orderData in saveData.Orders )
        {
            if ( orderData == null ||
                string.IsNullOrWhiteSpace(
                    orderData.OrderId ) == true ||
                orderData.CreatedNumber < 1 ||
                orderData.Requirements == null ||
                orderData.Wishes == null ||
                orderData.TargetThemes == null ||
                orderData.ExcludedThemes == null ||
                orderIds.Add( orderData.OrderId ) == false ||
                createdNumbers.Add( orderData.CreatedNumber ) == false )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 주문 생성 저장 데이터의 기본 값 범위 확인
    /// </summary>
    /// <param name="saveData">검증할 주문 생성 데이터</param>
    /// <returns>주문 생성 저장 데이터 유효 여부</returns>
    bool IsValidOrderGeneratorData ( OrderGeneratorSaveData saveData )
    {
        if ( saveData.IsApplied == false )
            return true;

        if ( saveData.EasyWeight < 0 || saveData.NormalWeight < 0 || saveData.HardWeight < 0 )
            return false;

        long totalWeight =
            ( long ) saveData.EasyWeight + saveData.NormalWeight + saveData.HardWeight;

        return totalWeight > 0 && totalWeight <= int.MaxValue;
    }

    /// <summary>
    /// 저장 결과에 대응하는 슬롯 오류 문구 반환
    /// </summary>
    /// <param name="result">저장 파일 처리 결과</param>
    /// <returns>슬롯에 표시할 오류 문구</returns>
    string GetErrorMessage ( SaveResult result )
    {
        return result == SaveResult.InvalidData
            ? InvalidDataMessage
            : "저장 파일을 읽지 못했습니다.";
    }

    #endregion
}
