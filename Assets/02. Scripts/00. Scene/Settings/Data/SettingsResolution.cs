using System;

/// <summary>
/// 설정 해상도 값
/// </summary>
[Serializable]
public readonly struct SettingsResolution
{
    /// <summary>
    /// 화면 너비
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 화면 높이
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// 설정 해상도 생성
    /// </summary>
    /// <param name="width">화면 너비</param>
    /// <param name="height">화면 높이</param>
    public SettingsResolution ( int width, int height )
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 같은 해상도인지 확인
    /// </summary>
    /// <param name="other">비교할 해상도</param>
    /// <returns>너비와 높이가 같은지 여부</returns>
    public bool IsSame ( SettingsResolution other )
    {
        return Width == other.Width && Height == other.Height;
    }

    /// <summary>
    /// 해상도 표시 문자열 반환
    /// </summary>
    /// <returns>너비와 높이 문자열</returns>
    public override string ToString ()
    {
        return $"{Width} x {Height}";
    }
}