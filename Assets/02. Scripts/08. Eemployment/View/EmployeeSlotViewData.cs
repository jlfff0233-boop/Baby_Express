using UnityEngine;

/// <summary>
/// 직원 슬롯 표시 데이터
/// </summary>
public class EmployeeSlotViewData
{
    /// <summary>
    /// 직원 아이디
    /// </summary>
    public string EmployeeId { get; }

    /// <summary>
    /// 직원 초상화
    /// </summary>
    public Sprite Portrait { get; }

    /// <summary>
    /// 직원 표시 이름
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 직원 업무
    /// </summary>
    public string Job { get; }

    /// <summary>
    /// 고용과 급여 정보
    /// </summary>
    public string Cost { get; }

    /// <summary>
    /// 현재 상태와 해금 안내
    /// </summary>
    public string Guide { get; }

    /// <summary>
    /// 입력 버튼 문구
    /// </summary>
    public string Action { get; }

    /// <summary>
    /// 입력 가능 여부
    /// </summary>
    public bool IsInteractable { get; }

    /// <summary>
    /// 직원 슬롯 표시 데이터 생성
    /// </summary>
    /// <param name="employeeId">직원 아이디</param>
    /// <param name="portrait">초상화</param>
    /// <param name="name">직원 이름</param>
    /// <param name="job">업무</param>
    /// <param name="cost">고용과 급여 정보</param>
    /// <param name="guide">현재 상태와 해금 안내</param>
    /// <param name="action">입력 버튼 문구</param>
    /// <param name="isInteractable">입력 가능 여부</param>
    public EmployeeSlotViewData (
        string employeeId, Sprite portrait,
        string name, string job,
        string cost, string guide,
        string action, bool isInteractable )
    {
        EmployeeId = employeeId;
        Portrait = portrait;
        Name = name;
        Job = job;
        Cost = cost;
        Guide = guide;
        Action = action;
        IsInteractable = isInteractable;
    }
}
