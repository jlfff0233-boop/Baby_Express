using System;
using UnityEngine;


/// <summary>
/// 배치 파츠 저장 데이터
/// </summary>
[Serializable]
public class PlacedPartSaveData
{
    [SerializeField] int _placementNumber;      //파츠 배치 번호
    [SerializeField] string _partId;        //파츠 아이디
    [SerializeField] int _partIndex;        //파츠 인덱스
    [SerializeField] Vector2 _localPosition;        //로컬 위치
    [SerializeField] float _rotation;       //회전
    [SerializeField] float _scale;      //스케일

    /// <summary>
    /// 파츠 배치 번호
    /// </summary>
    public int PlacementNumber => _placementNumber;
    /// <summary>
    /// 파츠 아이디
    /// </summary>
    public string PartId => _partId;
    /// <summary>
    /// 파츠 인덱스
    /// </summary>
    public int PartIndex => _partIndex;
    /// <summary>
    /// 로컬 위치
    /// </summary>
    public Vector2 LocalPosition => _localPosition;
    /// <summary>
    /// 회전
    /// </summary>
    public float Rotation => _rotation;
    /// <summary>
    /// 스케일
    /// </summary>
    public float Scale => _scale;

    /// <summary>
    /// 배치 파츠 저장 데이터 생성
    /// </summary>
    /// <param name="placedPart">배치한 파츠</param>
    public PlacedPartSaveData ( PlacedPartData placedPart )
    {
        _placementNumber = placedPart.PlacementNumber;
        _partId = placedPart.PartId;
        _partIndex = placedPart.PartIndex;
        _localPosition = placedPart.LocalPosition;
        _rotation = placedPart.Rotation;
        _scale = placedPart.Scale;
    }
}
