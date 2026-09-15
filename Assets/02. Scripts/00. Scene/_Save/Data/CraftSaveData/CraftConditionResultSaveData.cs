using System;
using UnityEngine;

/// <summary>
/// 제작 조건 판정 결과 저장 데이터
/// </summary>
[Serializable]
public class CraftConditionResultSaveData
{
    [SerializeField] string _partId;        //파츠 아이디
    [SerializeField] int _requiredQuantity;     //요구 수량
    [SerializeField] int _usedQuantity;     //사용 수량
    [SerializeField] bool _isCompleted;     //충족 여부
    [SerializeField] float _scoreChanged;       //점수 변화

    /// <summary>
    /// 파츠 아이디
    /// </summary>
    public string PartId => _partId;
    /// <summary>
    /// 요구 수량
    /// </summary>
    public int RequiredQuantity => _requiredQuantity;
    /// <summary>
    /// 사용 수량
    /// </summary>
    public int UsedQuantity => _usedQuantity;
    /// <summary>
    /// 충족 여부
    /// </summary>
    public bool IsCompleted => _isCompleted;
    /// <summary>
    /// 점수 변화
    /// </summary>
    public float ScoreChanged => _scoreChanged;


    /// <summary>
    /// 제작 조건 판정 결과 세이브 데이터 생성자
    /// </summary>
    /// <param name="result">제작 조건 판정 결과</param>
    public CraftConditionResultSaveData ( CraftConditionResult result )
    {
        _partId = result.PartId;
        _requiredQuantity = result.RequiredQuantity;
        _usedQuantity = result.UsedQuantity;
        _isCompleted = result.IsCompleted;
        _scoreChanged = result.ScoreChanged;
    }
}
