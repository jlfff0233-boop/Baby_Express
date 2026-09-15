using UnityEngine;

/// <summary>
/// 배치된 개별 파츠 데이터 - 배치 번호, 파츠 데이터, 순서, 위치, 회전, 스케일
/// </summary>
public class PlacedPartData
{
    int _placementNumber;       //배치 번호
    int _partIndex;             //앞뒤 순서 인덱스
    PartsData _partData;        //파츠 데이터

    Vector2 _localPosition;     //로컬 위치
    float _rotation;            //회전값
    float _scale;       //스케일

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 배치 번호
    /// </summary>
    public int PlacementNumber => _placementNumber;

    /// <summary>
    /// 파츠 데이터
    /// </summary>
    public PartsData PartData => _partData;

    /// <summary>
    /// 파츠 아이디
    /// </summary>
    public string PartId => _partData.Id;

    /// <summary>
    /// 앞뒤 순서 인덱스
    /// </summary>
    public int PartIndex => _partIndex;

    /// <summary>
    /// 로컬 위치
    /// </summary>
    public Vector2 LocalPosition => _localPosition;

    /// <summary>
    /// 회전값
    /// </summary>
    public float Rotation => _rotation;

    /// <summary>
    /// 스케일
    /// </summary>
    public float Scale => _scale;
    #endregion

    /// <summary>
    /// 배치 파츠 데이터 생성
    /// </summary>
    /// <param name="placementNumber">배치 번호</param>
    /// <param name="partData">파츠 데이터</param>
    /// <param name="partIndex">앞뒤 순서 인덱스</param>
    /// <param name="localPosition">로컬 위치</param>
    /// <param name="scale">균등 스케일</param>
    public PlacedPartData (
        int placementNumber, PartsData partData, int partIndex,
        Vector2 localPosition, float scale )
    {
        _placementNumber = placementNumber;
        _partData = partData;
        _partIndex = partIndex;

        _localPosition = localPosition;
        _scale = scale;
    }

    /// <summary>
    /// 저장 앞뒤 순서 설정
    /// </summary>
    /// <param name="partIndex">변경할 앞뒤 순서 인덱스</param>
    public void SetPartIndex ( int partIndex )
    {
        _partIndex = partIndex;
    }

    /// <summary>
    /// 로컬 위치 설정
    /// </summary>
    /// <param name="localPosition">변경할 로컬 위치</param>
    public void SetLocalPosition ( Vector2 localPosition )
    {
        _localPosition = localPosition;
    }

    /// <summary>
    /// 회전값 설정
    /// </summary>
    /// <param name="rotation">변경할 회전값</param>
    public void SetRotation ( float rotation )
    {
        _rotation = Mathf.Repeat( rotation, 360f );
    }

    /// <summary>
    /// 스케일 설정
    /// </summary>
    /// <param name="scale">변경할 스케일</param>
    public void SetScale ( float scale )
    {
        _scale = scale;
    }

}
