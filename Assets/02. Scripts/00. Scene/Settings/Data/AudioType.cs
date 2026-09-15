/// <summary>
/// 배경 음악 종류
/// </summary>
public enum BGMType
{
    Title,       //타이틀 배경 음악
    Play,       //플레이 배경 음악
    Event       //이벤트 배경 음악
}

/// <summary>
/// 효과음 종류
/// </summary>
public enum SFXType
{
    Button,       //기본 UI 클릭
    Panel,       //패널 표시와 숨김
    Parts,       //파츠 조작
    Impact,       //파츠 안착
    Dialogue,       //대화 진행
    BudgetIncome,       //자금 수입
    BudgetExpense,       //자금 지출
    Purchase,       //구매 성공
    Sell,       //판매 성공
    Success,       //일반 성공
    Failure,       //일반 실패
    Denied,       //입력 거부
    Warning,       //경고
    Achievement,       //업적 달성
    Reward       //보상 획득
}