using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LobbyUIController : MonoBehaviour
{
    private VisualElement root;

    // 오버레이
    private VisualElement overlaySettings;
    private VisualElement overlayQuit;

    // 슬라이더
    private Slider sliderMaster;
    private Slider sliderBgm;
    private Slider sliderSfx;

    // 드롭다운
    private DropdownField dropdownFps;
    private DropdownField dropdownShadow;

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        // 오버레이
        overlaySettings = root.Q<VisualElement>("overlay-settings");
        overlayQuit     = root.Q<VisualElement>("overlay-quit");

        // 슬라이더
        sliderMaster = root.Q<Slider>("slider-master");
        sliderBgm    = root.Q<Slider>("slider-bgm");
        sliderSfx    = root.Q<Slider>("slider-sfx");

        // 드롭다운 선택지 등록
        dropdownFps = root.Q<DropdownField>("dropdown-fps");
        dropdownFps.choices = new System.Collections.Generic.List<string>
            { "30", "60", "120", "무제한" };
        dropdownFps.index = 1; // 기본값 60

        dropdownShadow = root.Q<DropdownField>("dropdown-shadow");
        dropdownShadow.choices = new System.Collections.Generic.List<string>
            { "끄기", "낮음", "중간", "높음" };
        dropdownShadow.index = 2; // 기본값 중간

        // 메인 메뉴 버튼
        root.Q<Button>("btn-start").clicked    += OnStartClicked;
        root.Q<Button>("btn-settings").clicked += OnSettingsClicked;
        root.Q<Button>("btn-quit").clicked     += OnQuitClicked;

        // 설정 닫기
        root.Q<Button>("btn-close-settings").clicked += OnCloseSettings;

        // 나가기 확인
        root.Q<Button>("btn-confirm-yes").clicked += OnConfirmYes;
        root.Q<Button>("btn-confirm-no").clicked  += OnConfirmNo;

        // 시작 시 오버레이 숨김
        overlaySettings.RemoveFromClassList("visible");
        overlayQuit.RemoveFromClassList("visible");
    }

    // ── 버튼 핸들러 ──

    void OnStartClicked()
    {
        SceneManager.LoadScene("MainScene"); // 씬 이름 맞게 수정해주세요
    }

    void OnSettingsClicked()
    {
        overlaySettings.AddToClassList("visible");
    }

    void OnQuitClicked()
    {
        overlayQuit.AddToClassList("visible");
    }

    void OnCloseSettings()
    {
        overlaySettings.RemoveFromClassList("visible");
    }

    void OnConfirmYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnConfirmNo()
    {
        overlayQuit.RemoveFromClassList("visible");
    }
}