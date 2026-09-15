using System.Collections.Generic;

/// <summary>
/// 엔딩 메달 등급
/// </summary>
public enum EndingMedalGrade
{
    Gold,       //금메달
    Silver,     //은메달
    Bronze,     //동메달
}

/// <summary>
/// 엔딩 기록 문구 색상 종류
/// </summary>
public enum EndingTextTone
{
    Default,        //기본 문구
    Income,         //수입 문구
    Expense,        //지출 문구
}

/// <summary>
/// 엔딩 기록 한 줄 표시 데이터
/// </summary>
public class EndingInfoLineViewData
{
    /// <summary>
    /// 표시 문구
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// 문구 색상 종류
    /// </summary>
    public EndingTextTone TextTone { get; }

    /// <summary>
    /// 엔딩 기록 한 줄 표시 데이터 생성
    /// </summary>
    /// <param name="text">표시 문구</param>
    /// <param name="textTone">문구 색상 종류</param>
    public EndingInfoLineViewData (
        string text, EndingTextTone textTone = EndingTextTone.Default )
    {
        Text = text;
        TextTone = textTone;
    }
}

/// <summary>
/// 엔딩 슬롯 표시 데이터
/// </summary>
public class EndingSlotViewData
{
    /// <summary>
    /// 슬롯 제목
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 메달 등급
    /// </summary>
    public EndingMedalGrade MedalGrade { get; }

    /// <summary>
    /// 엔딩 기록 목록
    /// </summary>
    public IReadOnlyList<EndingInfoLineViewData> InfoLines { get; }

    /// <summary>
    /// 엔딩 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="title">슬롯 제목</param>
    /// <param name="medalGrade">메달 등급</param>
    /// <param name="infoLines">엔딩 기록 목록</param>
    public EndingSlotViewData (
        string title,
        EndingMedalGrade medalGrade,
        IReadOnlyList<EndingInfoLineViewData> infoLines )
    {
        Title = title;
        MedalGrade = medalGrade;
        InfoLines = infoLines;
    }
}
