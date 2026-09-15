using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일일 제작 완료 기록 세이브 데이터
/// </summary>
[Serializable]
public class DailyCraftRecordSaveData
{
    [SerializeField] string _orderId;       //제작 주문 아이디
    [SerializeField] bool _isDiscarded;       //제작물 폐기 여부
    [SerializeField] bool _hasActivatedTheme;       //일반 테마 활성 여부
    [SerializeField] bool _hasCompleteTheme;       //완성 테마 달성 여부
    [SerializeField] int _themeConflictCount;       //상극 테마 쌍 수
    [SerializeField] bool _isTargetThemeCompleted;       //목표 테마 완성 여부
    [SerializeField] bool _areAllRequirementsCompleted;       //주요 요구 전체 달성 여부
    [SerializeField] bool _areAllWishesCompleted;       //희망 사항 전체 달성 여부
    [SerializeField] List<DailyItemRecordSaveData> _usedParts;       //사용 파츠 기록

    /// <summary>
    /// 주문 아이디
    /// </summary>
    public string OrderId => _orderId;
    /// <summary>
    /// 제작 취소 여부
    /// </summary>
    public bool IsDiscarded => _isDiscarded;
    /// <summary>
    /// 활성화 테마 존재 여부
    /// </summary>
    public bool HasActivatedTheme => _hasActivatedTheme;
    /// <summary>
    /// 완성 테마 존재 여부
    /// </summary>
    public bool HasCompleteTheme => _hasCompleteTheme;
    /// <summary>
    /// 상극 테마 쌍 수
    /// </summary>
    public int ThemeConflictCount => _themeConflictCount;
    /// <summary>
    /// 목표 테마 완성 여부
    /// </summary>
    public bool IsTargetThemeCompleted => _isTargetThemeCompleted;
    /// <summary>
    /// 주요 요구 사항 충족 여부
    /// </summary>
    public bool AreAllRequirementsCompleted => _areAllRequirementsCompleted;
    /// <summary>
    /// 희망 사항 충족 여부
    /// </summary>
    public bool AreAllWishesCompleted => _areAllWishesCompleted;
    /// <summary>
    /// 사용 파츠 목록
    /// </summary>
    public IReadOnlyList<DailyItemRecordSaveData> UsedParts => _usedParts;

    /// <summary>
    /// 일일 제작 완료 기록 세이브 데이터 생성
    /// </summary>
    /// <param name="record">일일 제작 완료 기록</param>
    public DailyCraftRecordSaveData ( DailyCraftRecord record )
    {
        _orderId = record.OrderId;
        _isDiscarded = record.IsDiscarded;
        _hasActivatedTheme = record.HasActivatedTheme;
        _hasCompleteTheme = record.HasCompleteTheme;
        _themeConflictCount = record.ThemeConflictCount;
        _isTargetThemeCompleted = record.IsTargetThemeCompleted;

        _areAllRequirementsCompleted = record.AreAllRequirementsCompleted;

        _areAllWishesCompleted = record.AreAllWishesCompleted;

        _usedParts = new List<DailyItemRecordSaveData>( record.UsedParts.Count );

        for ( int i = 0; i < record.UsedParts.Count; i++ )
        {
            _usedParts.Add( new DailyItemRecordSaveData( record.UsedParts [ i ] ) );
        }
    }
}
