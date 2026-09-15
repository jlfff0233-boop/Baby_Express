using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 뷰 - 오디오와 화면 설정 표시 및 입력 전달
/// </summary>
public class SettingsView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] PanelTweenView _panelTween;       //설정 패널 연출

    [Header( "----- 오디오 입력 -----" )]
    [SerializeField] Slider _masterVolumeSlider;       //마스터 볼륨
    [SerializeField] Slider _bgmVolumeSlider;       //배경 음악 볼륨
    [SerializeField] Slider _sfxVolumeSlider;       //효과음 볼륨
    [SerializeField] Toggle _masterMuteToggle;       //마스터 음소거
    [SerializeField] Toggle _bgmMuteToggle;       //배경 음악 음소거
    [SerializeField] Toggle _sfxMuteToggle;       //효과음 음소거

    [Header( "----- 화면 입력 -----" )]
    [SerializeField] GameObject _displayGroup;       //화면 설정 그룹
    [SerializeField] TMP_Dropdown _windowModeDropdown;       //화면 모드
    [SerializeField] TMP_Dropdown _resolutionDropdown;       //해상도

    [Header( "----- 화면 설정 확인 -----" )]
    [SerializeField] GameObject _displayConfirmPanel;       //화면 설정 확인 패널
    [SerializeField] TMP_Text _displayConfirmText;       //화면 설정 확인 안내
    [SerializeField] Button _displayConfirmButton;       //화면 설정 확인 버튼
    [SerializeField] Button _displayCancelButton;       //화면 설정 복구 버튼

    PanelTweenView _displayConfirmTween;       //화면 설정 확인 패널 입력 상태 관리

    [Header( "----- 버튼 입력 -----" )]
    [SerializeField] Button _saveButton;       //저장 탭 버튼
    [SerializeField] Button _applyButton;       //적용 버튼
    [SerializeField] Button _cancelButton;       //취소 버튼
    [SerializeField] Button _closeButton;       //상단 닫기 버튼

    #region ----- 이벤트 -----
    /// <summary>
    /// 마스터 볼륨 변경 이벤트
    /// </summary>
    public event Action<float> OnMasterVolumeChanged;

    /// <summary>
    /// 배경 음악 볼륨 변경 이벤트
    /// </summary>
    public event Action<float> OnBGMVolumeChanged;

    /// <summary>
    /// 효과음 볼륨 변경 이벤트
    /// </summary>
    public event Action<float> OnSFXVolumeChanged;

    /// <summary>
    /// 마스터 음소거 변경 이벤트
    /// </summary>
    public event Action<bool> OnMasterMuteChanged;

    /// <summary>
    /// 배경 음악 음소거 변경 이벤트
    /// </summary>
    public event Action<bool> OnBGMMuteChanged;

    /// <summary>
    /// 효과음 음소거 변경 이벤트
    /// </summary>
    public event Action<bool> OnSFXMuteChanged;

    /// <summary>
    /// 전체 화면 여부 변경 이벤트
    /// </summary>
    public event Action<bool> OnWindowModeChanged;

    /// <summary>
    /// 해상도 선택 변경 이벤트
    /// </summary>
    public event Action<int> OnResolutionChanged;

    /// <summary>
    /// 화면 설정 확정 이벤트
    /// </summary>
    public event Action OnDisplayConfirm;

    /// <summary>
    /// 화면 설정 복구 이벤트
    /// </summary>
    public event Action OnDisplayCancel;

    /// <summary>
    /// 저장 화면 열기 이벤트
    /// </summary>
    public event Action OnSaveOpen;

    /// <summary>
    /// 설정 적용 이벤트
    /// </summary>
    public event Action OnApply;

    /// <summary>
    /// 설정 닫기 이벤트
    /// </summary>
    public event Action OnClose;
    #endregion

    #region ----- 초기화 / 이벤트 -----

    /// <summary>
    /// 설정 입력 초기화 및 이벤트 연결
    /// </summary>
    void Awake ()
    {
        //기존 Play 씬의 저장 탭 참조 보완
        if ( _saveButton == null )
            _saveButton = FindButton( "SaveTap" );

        //설정 확인과 취소 버튼에 공용 클릭 연출 연결
        _displayConfirmButton.BindClickHighlight( );
        _displayCancelButton.BindClickHighlight( );
        _applyButton.BindClickHighlight( );
        _cancelButton.BindClickHighlight( );
        _closeButton.BindClickHighlight( );

        if ( _saveButton != null )
            _saveButton.BindClickHighlight( );

        InitVolumeSlider( _masterVolumeSlider );
        InitVolumeSlider( _bgmVolumeSlider );
        InitVolumeSlider( _sfxVolumeSlider );

        InitWindowModeOptions( );
        _displayConfirmTween =
            _displayConfirmPanel.GetComponent<PanelTweenView>( );

        _masterVolumeSlider.onValueChanged.AddListener( ChangeMasterVolume );
        _bgmVolumeSlider.onValueChanged.AddListener( ChangeBGMVolume );
        _sfxVolumeSlider.onValueChanged.AddListener( ChangeSFXVolume );
        _masterMuteToggle.onValueChanged.AddListener( ChangeMasterMute );
        _bgmMuteToggle.onValueChanged.AddListener( ChangeBGMMute );
        _sfxMuteToggle.onValueChanged.AddListener( ChangeSFXMute );

        _windowModeDropdown.onValueChanged.AddListener( ChangeWindowMode );
        _resolutionDropdown.onValueChanged.AddListener( ChangeResolution );
        _displayConfirmButton.onClick.AddListener( ConfirmDisplay );
        _displayCancelButton.onClick.AddListener( CancelDisplay );

        //타이틀 설정에는 저장 탭을 사용하지 않음
        if ( _saveButton != null )
            _saveButton.onClick.AddListener( OpenSave );

        _applyButton.onClick.AddListener( Apply );
        _cancelButton.onClick.AddListener( Close );
        _closeButton.onClick.AddListener( Close );

        HideDisplayConfirm( );
    }

    /// <summary>
    /// 현재 설정 화면의 이름으로 버튼 조회
    /// </summary>
    /// <param name="buttonName">찾을 버튼 오브젝트 이름</param>
    /// <returns>일치하는 버튼</returns>
    Button FindButton ( string buttonName )
    {
        Button [ ] buttons =
            GetComponentsInChildren<Button>( true );

        for ( int i = 0; i < buttons.Length; i++ )
        {
            if ( buttons [ i ].name == buttonName )
                return buttons [ i ];
        }

        return null;
    }

    /// <summary>
    /// 설정 입력 이벤트 해제
    /// </summary>
    void OnDestroy ()
    {
        _masterVolumeSlider.onValueChanged.RemoveListener(
            ChangeMasterVolume );
        _bgmVolumeSlider.onValueChanged.RemoveListener(
            ChangeBGMVolume );
        _sfxVolumeSlider.onValueChanged.RemoveListener(
            ChangeSFXVolume );
        _masterMuteToggle.onValueChanged.RemoveListener(
            ChangeMasterMute );
        _bgmMuteToggle.onValueChanged.RemoveListener(
            ChangeBGMMute );
        _sfxMuteToggle.onValueChanged.RemoveListener(
            ChangeSFXMute );

        _windowModeDropdown.onValueChanged.RemoveListener(
            ChangeWindowMode );
        _resolutionDropdown.onValueChanged.RemoveListener(
            ChangeResolution );
        _displayConfirmButton.onClick.RemoveListener( ConfirmDisplay );
        _displayCancelButton.onClick.RemoveListener( CancelDisplay );

        if ( _saveButton != null )
            _saveButton.onClick.RemoveListener( OpenSave );

        _applyButton.onClick.RemoveListener( Apply );
        _cancelButton.onClick.RemoveListener( Close );
        _closeButton.onClick.RemoveListener( Close );
    }

    /// <summary>
    /// 볼륨 슬라이더 범위 초기화
    /// </summary>
    /// <param name="slider">초기화할 슬라이더</param>
    void InitVolumeSlider ( Slider slider )
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    /// <summary>
    /// 화면 모드 Dropdown 항목 초기화
    /// </summary>
    void InitWindowModeOptions ()
    {
        List<string> options = new List<string>
        {
            "창 모드",
            "전체 화면"
        };

        _windowModeDropdown.ClearOptions( );
        _windowModeDropdown.AddOptions( options );
    }

    #endregion

    #region ----- 화면 표시 -----

    /// <summary>
    /// 설정 화면 표시
    /// </summary>
    public void Show ()
    {
        gameObject.SetActive ( true );
        _panelTween.Show( );
    }

    /// <summary>
    /// 설정 화면 숨김
    /// </summary>
    public void Hide ()
    {
        _panelTween.Hide ( CompleteHide );
    }

    /// <summary>
    /// 설정 화면 즉시 숨김
    /// </summary>
    public void HideInstant ( )
    {
        _panelTween.SetVisible( false );
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 설정 화면 퇴장 완료 처리
    /// </summary>
    void CompleteHide ( )
    {
        gameObject.SetActive ( false );
    }

    /// <summary>
    /// 현재 플랫폼에 맞춰 화면 설정 그룹 표시 여부 변경
    /// </summary>
    /// <param name="isVisible">화면 설정 표시 여부</param>
    public void SetDisplayVisible ( bool isVisible )
    {
        _displayGroup.SetActive( isVisible );
    }

    /// <summary>
    /// 화면 설정 확인 패널 표시
    /// </summary>
    /// <param name="remainingSeconds">복구까지 남은 시간</param>
    public void ShowDisplayConfirm ( int remainingSeconds )
    {
        //재표시할 때 확인 패널의 CanvasGroup 입력 상태도 함께 복구
        _displayConfirmTween.SetVisible( true );
        SetDisplayConfirmTime( remainingSeconds );
    }

    /// <summary>
    /// 화면 설정 확인 제한 시간 표시
    /// </summary>
    /// <param name="remainingSeconds">복구까지 남은 시간</param>
    public void SetDisplayConfirmTime ( int remainingSeconds )
    {
        _displayConfirmText.text =
            "이 화면 설정을 유지하시겠습니까?\n" +
            $"{remainingSeconds}초 후 이전 설정으로 돌아갑니다.";
    }

    /// <summary>
    /// 화면 설정 확인 패널 숨김
    /// </summary>
    public void HideDisplayConfirm ()
    {
        _displayConfirmTween.SetVisible( false );
    }

    /// <summary>
    /// 현재 편집 중인 볼륨 표시
    /// </summary>
    /// <param name="masterVolume">마스터 볼륨</param>
    /// <param name="bgmVolume">배경 음악 볼륨</param>
    /// <param name="sfxVolume">효과음 볼륨</param>
    public void SetVolumes (
        float masterVolume, float bgmVolume, float sfxVolume )
    {
        _masterVolumeSlider.SetValueWithoutNotify( masterVolume );
        _bgmVolumeSlider.SetValueWithoutNotify( bgmVolume );
        _sfxVolumeSlider.SetValueWithoutNotify( sfxVolume );
    }

    /// <summary>
    /// 현재 편집 중인 음소거 상태 표시
    /// </summary>
    /// <param name="isMasterMuted">마스터 음소거 여부</param>
    /// <param name="isBGMMuted">배경 음악 음소거 여부</param>
    /// <param name="isSFXMuted">효과음 음소거 여부</param>
    public void SetMutes (
        bool isMasterMuted, bool isBGMMuted, bool isSFXMuted )
    {
        _masterMuteToggle.SetIsOnWithoutNotify( isMasterMuted );
        _bgmMuteToggle.SetIsOnWithoutNotify( isBGMMuted );
        _sfxMuteToggle.SetIsOnWithoutNotify( isSFXMuted );
    }

    /// <summary>
    /// 현재 편집 중인 화면 모드 표시
    /// </summary>
    /// <param name="isFullScreen">전체 화면 여부</param>
    public void SetWindowMode ( bool isFullScreen )
    {
        int optionIndex = isFullScreen ? 1 : 0;

        _windowModeDropdown.SetValueWithoutNotify( optionIndex );
        _windowModeDropdown.RefreshShownValue( );
    }

    /// <summary>
    /// 지원 해상도 드롭다운 항목 설정
    /// </summary>
    /// <param name="resolutions">지원 해상도 목록</param>
    public void SetResolutionOptions (
        IReadOnlyList<SettingsResolution> resolutions )
    {
        List<string> options = new List<string>( resolutions.Count );

        for ( int i = 0; i < resolutions.Count; i++ )
            options.Add( resolutions [ i ].ToString( ) );

        _resolutionDropdown.ClearOptions( );
        _resolutionDropdown.AddOptions( options );
    }

    /// <summary>
    /// 현재 편집 중인 해상도 선택 표시
    /// </summary>
    /// <param name="optionIndex">선택할 해상도 인덱스</param>
    public void SetResolutionIndex ( int optionIndex )
    {
        _resolutionDropdown.SetValueWithoutNotify( optionIndex );
        _resolutionDropdown.RefreshShownValue( );
    }

    #endregion

    #region ----- 입력 전달 -----

    /// <summary>
    /// 마스터 볼륨 변경 요청 전달
    /// </summary>
    /// <param name="volume">변경한 볼륨</param>
    void ChangeMasterVolume ( float volume )
    {
        OnMasterVolumeChanged?.Invoke( volume );
    }

    /// <summary>
    /// 배경 음악 볼륨 변경 요청 전달
    /// </summary>
    /// <param name="volume">변경한 볼륨</param>
    void ChangeBGMVolume ( float volume )
    {
        OnBGMVolumeChanged?.Invoke( volume );
    }

    /// <summary>
    /// 효과음 볼륨 변경 요청 전달
    /// </summary>
    /// <param name="volume">변경한 볼륨</param>
    void ChangeSFXVolume ( float volume )
    {
        OnSFXVolumeChanged?.Invoke( volume );
    }

    /// <summary>
    /// 마스터 음소거 변경 요청 전달
    /// </summary>
    /// <param name="isMuted">음소거 여부</param>
    void ChangeMasterMute ( bool isMuted )
    {
        OnMasterMuteChanged?.Invoke( isMuted );
    }

    /// <summary>
    /// 배경 음악 음소거 변경 요청 전달
    /// </summary>
    /// <param name="isMuted">음소거 여부</param>
    void ChangeBGMMute ( bool isMuted )
    {
        OnBGMMuteChanged?.Invoke( isMuted );
    }

    /// <summary>
    /// 효과음 음소거 변경 요청 전달
    /// </summary>
    /// <param name="isMuted">음소거 여부</param>
    void ChangeSFXMute ( bool isMuted )
    {
        OnSFXMuteChanged?.Invoke( isMuted );
    }

    /// <summary>
    /// 화면 모드 변경 요청 전달
    /// </summary>
    /// <param name="optionIndex">선택한 화면 모드 인덱스</param>
    void ChangeWindowMode ( int optionIndex )
    {
        bool isFullScreen = optionIndex == 1;

        OnWindowModeChanged?.Invoke( isFullScreen );
    }

    /// <summary>
    /// 해상도 변경 요청 전달
    /// </summary>
    /// <param name="optionIndex">선택한 해상도 인덱스</param>
    void ChangeResolution ( int optionIndex )
    {
        OnResolutionChanged?.Invoke( optionIndex );
    }

    /// <summary>
    /// 화면 설정 확정 요청 전달
    /// </summary>
    void ConfirmDisplay ()
    {
        OnDisplayConfirm?.Invoke( );
    }

    /// <summary>
    /// 화면 설정 복구 요청 전달
    /// </summary>
    void CancelDisplay ()
    {
        OnDisplayCancel?.Invoke( );
    }

    /// <summary>
    /// 저장 화면 열기 요청 전달
    /// </summary>
    void OpenSave ()
    {
        OnSaveOpen?.Invoke( );
    }

    /// <summary>
    /// 설정 적용 요청 전달
    /// </summary>
    void Apply ()
    {
        OnApply?.Invoke( );
    }

    /// <summary>
    /// 설정 닫기 요청 전달
    /// </summary>
    void Close ()
    {
        OnClose?.Invoke( );
    }

    #endregion
}
