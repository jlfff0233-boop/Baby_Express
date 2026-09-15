using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 저장 데이터 - 저장 파일 식별과 슬롯 목록 표시에 사용하는 기본 정보
/// </summary>
[Serializable]
public class SaveData
{
    [SerializeField] int _version;      //버전
    [SerializeField] int _slotNumber;       //슬롯 번호
    [SerializeField] int _totalDay;     //총 영업일 수
    [SerializeField] int _achievedAchvCount;        //달성 업적 수
    [SerializeField] float _budget;     //현재 자금
    [SerializeField] string _savedAtUtc;        //저장 시각
    [SerializeField] float _time;       //현재 시간
    [SerializeField] int _highGradeEvaluationCount;     //B등급 이상 평가 누적 수
    [SerializeField] List<string> _unlockedIds;     //해금 상품 아이디 리스트
    [SerializeField] List<string> _newUnlockedIds;       //아직 확인하지 않은 해금 상품 아이디 리스트

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 저장 파일 형식 버전
    /// </summary>
    public int Version => _version;

    /// <summary>
    /// 저장 슬롯 번호
    /// </summary>
    public int SlotNumber => _slotNumber;

    /// <summary>
    /// 누적 영업일
    /// </summary>
    public int TotalDay => _totalDay;

    /// <summary>
    /// 달성한 업적 개수
    /// </summary>
    public int AchievedAchvCount => _achievedAchvCount;

    /// <summary>
    /// 저장 당시 보유 골드
    /// </summary>
    public float Budget => _budget;

    /// <summary>
    /// UTC 기준 저장 시각 문자열
    /// </summary>
    public string SavedAtUtc => _savedAtUtc;

    /// <summary>
    /// 현재 시간
    /// </summary>
    public float Time => _time;

    /// <summary>
    /// B등급 이상 평가 누적 수
    /// </summary>
    public int HighGradeEvaluationCount => _highGradeEvaluationCount;

    /// <summary>
    /// 해금된 상품 아이디 목록
    /// </summary>
    public IReadOnlyList<string> UnlockedIds => _unlockedIds;

    /// <summary>
    /// 아직 확인하지 않은 해금 상품 아이디 목록
    /// </summary>
    public IReadOnlyList<string> NewUnlockedIds
    {
        get
        {
            if ( _newUnlockedIds == null )
                return Array.Empty<string>( );

            return _newUnlockedIds;
        }
    }
    #endregion


    /// <summary>
    /// 저장 데이터 생성
    /// </summary>
    /// <param name="version">저장 파일 형식 버전</param>
    /// <param name="slotNumber">저장 슬롯 번호</param>
    /// <param name="time">현재 시간</param>
    /// <param name="totalDay">누적 영업일</param>
    /// <param name="highGradeEvaluationCount">B등급 이상 평가 누적 수</param>
    /// <param name="achievedAchvCount">달성한 업적 개수</param>
    /// <param name="budget">저장 당시 보유 골드</param>
    /// <param name="unlockedIds">해금된 상품 아이디 목록</param>
    /// <param name="newUnlockedIds">아직 확인하지 않은 해금 상품 아이디 목록</param>
    /// <param name="savedAtUtc">UTC 기준 저장 시각</param>
    public SaveData (
        int version, int slotNumber,
        float time, int totalDay,
        int highGradeEvaluationCount, int achievedAchvCount,
        float budget, IReadOnlyCollection<string> unlockedIds,
        IReadOnlyCollection<string> newUnlockedIds,
        DateTime savedAtUtc )
    {
        _version = version;
        _slotNumber = slotNumber;

        _time = time;
        _totalDay = totalDay;
        _highGradeEvaluationCount = highGradeEvaluationCount;
        _achievedAchvCount = achievedAchvCount;
        _budget = budget;
        _unlockedIds = new List<string>( unlockedIds );
        _newUnlockedIds = new List<string>( newUnlockedIds );

        _savedAtUtc = savedAtUtc
            .ToUniversalTime( )
            .ToString( "O", CultureInfo.InvariantCulture );
    }
}
