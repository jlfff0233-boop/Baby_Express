using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 저장 슬롯 표시와 입력 뷰
/// </summary>
public class SaveSlotView : MonoBehaviour
{
    [SerializeField] TMP_Text _slotNumberText;       //슬롯 번호
    [SerializeField] TMP_Text _savedAtText;       //저장 시각
    [SerializeField] TMP_Text _progressText;       //진행 상황
    [SerializeField] TMP_Text _saveButtonText;       //저장 버튼 문구

    [SerializeField] Button _saveButton;       //저장 또는 덮어쓰기 버튼
    [SerializeField] Button _loadButton;       //불러오기 버튼
    [SerializeField] Button _deleteButton;       //삭제 버튼

    /// <summary>
    /// 슬롯 저장 입력 이벤트(슬롯 번호)
    /// </summary>
    public event Action<int> OnSave;

    /// <summary>
    /// 슬롯 불러오기 입력 이벤트(슬롯 번호)
    /// </summary>
    public event Action<int> OnLoad;

    /// <summary>
    /// 슬롯 삭제 입력 이벤트(슬롯 번호)
    /// </summary>
    public event Action<int> OnDelete;

    int _slotNumber;       //현재 슬롯 번호
    SlotTweenView _slotTween;       //슬롯 등장과 선택 연출


    #region ----- 초기화 -----

    /// <summary>
    /// 비활성 상태에서도 사용할 슬롯 연출 초기화
    /// </summary>
    void InitializeRuntime ()
    {
        if ( _slotTween != null ) return;

        _slotTween =
            gameObject.GetOrAddComponent<SlotTweenView>( );
    }

    /// <summary>
    /// 슬롯 버튼 이벤트 연결
    /// </summary>
    void Awake ( )
    {
        InitializeRuntime( );

        //표시용 텍스트가 슬롯 버튼 입력을 가로막지 않도록 설정
        _slotNumberText.raycastTarget = false;
        _savedAtText.raycastTarget = false;
        _progressText.raycastTarget = false;
        _saveButtonText.raycastTarget = false;

        _saveButton.onClick.AddListener ( Save );
        _loadButton.onClick.AddListener ( Load );
        _deleteButton.onClick.AddListener ( Delete );
    }

    /// <summary>
    /// 슬롯 버튼 이벤트 해제
    /// </summary>
    void OnDestroy ( )
    {
        _saveButton.onClick.RemoveListener ( Save );
        _loadButton.onClick.RemoveListener ( Load );
        _deleteButton.onClick.RemoveListener ( Delete );
    }

    #endregion

    #region ----- 표시 -----

    /// <summary>
    /// 저장 슬롯 정보 표시
    /// </summary>
    /// <param name="data">저장 슬롯 데이터</param>
    /// <param name="mode">슬롯 기능</param>
    public void SetData (
        SaveSlotData data , SaveSlotMode mode )
    {
        InitializeRuntime( );

        //일반 정보 갱신에서는 이전 실행 연출만 정리
        _slotTween.ResetInstant( );

        _slotNumber = data.SlotNumber;
        _slotNumberText.text = $"Slot No.{data.SlotNumber}";

        SetButtons ( data.State , mode );

        switch ( data.State )
        {
            case SaveSlotState.Empty:       //비어 있을 때
                ShowEmpty ( mode );
                break;

            case SaveSlotState.Valid:       //값이 있을 때
                ShowValid ( data );
                break;

            case SaveSlotState.Corrupted:
                ShowCorrupted ( data , mode );        //오류가 있을 때
                break;
        }

        //저장 슬롯 인스턴스마다 최초 한 번만 등장 연출 재생
        _slotTween.PlayAppearOnce( );
    }

    /// <summary>
    /// 비어 있는 슬롯 표시
    /// </summary>
    /// <param name="mode">저장 슬롯 화면의 사용 목적</param>
    void ShowEmpty ( SaveSlotMode mode )
    {
        _savedAtText.text = "저장 데이터 없음";
        _progressText.text = string.Empty;
    }

    /// <summary>
    /// 정상 저장 슬롯 표시
    /// </summary>
    /// <param name="data">저장 슬롯 데이터</param>
    void ShowValid ( SaveSlotData data )
    {
        DateTime localSavedAt =
            data.SavedAtUtc.ToLocalTime ( );

        _savedAtText.text =
            localSavedAt.ToString ( "yyyy년 MM월 dd일 HH:mm" );

        _progressText.text =
            $"누적 영업일: {data.TotalDay}일\n" +
            $"달성 업적: {data.AchievedAchvCount}개\n" +
            $"골드: {data.Budget:N0}G";

    }

    /// <summary>
    /// 손상된 저장 슬롯 표시
    /// </summary>
    /// <param name="data">저장 슬롯 데이터</param>
    /// <param name="mode">저장 슬롯 화면의 사용 목적</param>
    void ShowCorrupted ( SaveSlotData data , SaveSlotMode mode )
    {
        _savedAtText.text = "손상된 저장 데이터";
        _progressText.text = data.ErrorMessage;

    }

    /// <summary>
    /// 슬롯 상태와 화면 목적에 맞는 버튼 상태 설정
    /// </summary>
    /// <param name="state">저장 슬롯 상태</param>
    /// <param name="mode">저장 슬롯 화면의 사용 목적</param>
    void SetButtons ( SaveSlotState state , SaveSlotMode mode )
    {
        bool showSaveButton =
            mode == SaveSlotMode.NewGame ||
            mode == SaveSlotMode.Save;

        bool showLoadButton =
            mode == SaveSlotMode.Load ||
            mode == SaveSlotMode.Save;

        _saveButton.gameObject.SetActive ( showSaveButton );
        _loadButton.gameObject.SetActive ( showLoadButton );
        _deleteButton.gameObject.SetActive ( state != SaveSlotState.Empty );

        _saveButton.interactable = showSaveButton;
        _loadButton.interactable =
            showLoadButton && state == SaveSlotState.Valid;
        _deleteButton.interactable = state != SaveSlotState.Empty;

        if ( mode == SaveSlotMode.NewGame &&
            state == SaveSlotState.Empty )
        {
            _saveButtonText.text = "시작하기";
            return;
        }

        if ( state == SaveSlotState.Empty )
        {
            _saveButtonText.text = "저장하기";
            return;
        }

        _saveButtonText.text = "덮어쓰기";
    }

    #endregion

    #region ----- 입력 -----

    /// <summary>
    /// 현재 상태를 선택 슬롯에 저장 요청
    /// </summary>
    void Save ( )
    {
        _slotTween.PlaySelected( );
        OnSave?.Invoke ( _slotNumber );
    }

    /// <summary>
    /// 선택 슬롯의 저장 데이터 불러오기 요청
    /// </summary>
    void Load ( )
    {
        _slotTween.PlaySelected( );
        OnLoad?.Invoke ( _slotNumber );
    }

    /// <summary>
    /// 슬롯 삭제 입력 전달
    /// </summary>
    void Delete ( )
    {
        _slotTween.PlaySelected( );
        OnDelete?.Invoke ( _slotNumber );
    }

    #endregion
}
