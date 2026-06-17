using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("로비 씬 이름")]
    public string lobbySceneName = "Lobby";

    public bool IsPaused { get; private set; }

    private VisualElement pauseRoot;
    private VisualElement pauseMenu;
    private VisualElement overlaySettings;
    private VisualElement overlayQuit;

    // 슬라이더
    private Slider sliderMaster;
    private Slider sliderBgm;
    private Slider sliderSfx;

    // 드롭다운
    private DropdownField dropdownFps;
    private DropdownField dropdownShadow;

    private InputAction escapeAction;

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        pauseRoot       = root.Q<VisualElement>("pause-root");
        pauseMenu       = root.Q<VisualElement>("pause-menu");
        overlaySettings = root.Q<VisualElement>("overlay-settings");
        overlayQuit     = root.Q<VisualElement>("overlay-quit");

        // 슬라이더 + 값 표시
        sliderMaster = root.Q<Slider>("slider-master");
        sliderBgm    = root.Q<Slider>("slider-bgm");
        sliderSfx    = root.Q<Slider>("slider-sfx");

        var valueMaster = root.Q<Label>("value-master");
        var valueBgm    = root.Q<Label>("value-bgm");
        var valueSfx    = root.Q<Label>("value-sfx");

        sliderMaster.RegisterValueChangedCallback(evt =>
            valueMaster.text = Mathf.RoundToInt(evt.newValue * 100).ToString());
        sliderBgm.RegisterValueChangedCallback(evt =>
            valueBgm.text = Mathf.RoundToInt(evt.newValue * 100).ToString());
        sliderSfx.RegisterValueChangedCallback(evt =>
            valueSfx.text = Mathf.RoundToInt(evt.newValue * 100).ToString());

        // 드롭다운
        dropdownFps = root.Q<DropdownField>("dropdown-fps");
        dropdownFps.choices = new List<string> { "30", "60", "120", "무제한" };
        dropdownFps.index = 1;

        dropdownShadow = root.Q<DropdownField>("dropdown-shadow");
        dropdownShadow.choices = new List<string> { "끄기", "낮음", "중간", "높음" };
        dropdownShadow.index = 2;

        // 일시정지 메뉴 버튼
        root.Q<Button>("btn-resume").clicked   += Resume;
        root.Q<Button>("btn-settings").clicked += OpenSettings;
        root.Q<Button>("btn-tomenu").clicked   += OpenQuitConfirm;

        // 설정 닫기
        root.Q<Button>("btn-close-settings").clicked += CloseSettings;

        // 메뉴 나가기 확인
        root.Q<Button>("btn-confirm-yes").clicked += OnConfirmYes;
        root.Q<Button>("btn-confirm-no").clicked  += OnConfirmNo;

        // 시작 상태: 다 숨김
        pauseMenu.style.display = DisplayStyle.None;
        overlaySettings.RemoveFromClassList("visible");
        overlayQuit.RemoveFromClassList("visible");

        // ESC 입력
        escapeAction = new InputAction(binding: "<Keyboard>/escape");
        escapeAction.performed += _ => OnEscapePressed();
        escapeAction.Enable();
    }

    void OnDisable()
    {
        escapeAction?.Disable();
        escapeAction?.Dispose();
    }

    private void OnEscapePressed()
    {
        // 설정 열려있으면 설정만 닫기
        if (overlaySettings.ClassListContains("visible"))
        {
            CloseSettings();
            return;
        }

        // 나가기 확인 열려있으면 그것만 닫기
        if (overlayQuit.ClassListContains("visible"))
        {
            overlayQuit.RemoveFromClassList("visible");
            return;
        }

        // 아니면 일시정지 토글
        Toggle();
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else          Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        pauseMenu.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        IsPaused = false;
        pauseMenu.style.display = DisplayStyle.None;
        overlaySettings.RemoveFromClassList("visible");
        overlayQuit.RemoveFromClassList("visible");
        Time.timeScale = 1f;
    }

    void OpenSettings()
    {
        overlaySettings.AddToClassList("visible");
    }

    void CloseSettings()
    {
        overlaySettings.RemoveFromClassList("visible");
    }

    void OpenQuitConfirm()
    {
        overlayQuit.AddToClassList("visible");
    }

    void OnConfirmYes()
    {
        // 메뉴로 나가기 → timeScale 복구 후 로비 로드
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
    }

    void OnConfirmNo()
    {
        overlayQuit.RemoveFromClassList("visible");
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}