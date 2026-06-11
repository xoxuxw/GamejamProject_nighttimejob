using UnityEngine;
using UnityEngine.InputSystem;

public class PausePanelManager : MonoBehaviour
{
    public static PausePanelManager Instance { get; private set; }

    [Header("Pause 패널")]
    [SerializeField] private GameObject pausePanel;

    [Header("하위 패널 (세팅, 나가기 등)")]
    [SerializeField] private GameObject[] subPanels;

    [Header("옵션")]
    [Tooltip("Pause 시 Time.timeScale을 0으로 설정합니다.")]
    [SerializeField] private bool freezeTimeOnPause = true;

    public bool IsPaused { get; private set; }

    private InputAction escapeAction;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pausePanel == null)
        {
            Debug.LogError("[PausePanelManager] pausePanel이 연결되지 않았습니다!", this);
            return;
        }

        pausePanel.SetActive(false);

        escapeAction = new InputAction(binding: "<Keyboard>/escape");
        escapeAction.performed += _ => OnEscapePressed();
    }

    void OnEnable()  => escapeAction?.Enable();
    void OnDisable() => escapeAction?.Disable();

    private void OnEscapePressed()
    {
        // 활성화된 하위 패널이 있으면 그것만 먼저 끄기
        foreach (var panel in subPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                panel.SetActive(false);
                return;
            }
        }

        // 하위 패널 없으면 퍼즈 토글
        Toggle();
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else          Pause();
    }

    public void Pause()
    {
        if (pausePanel == null) return;

        IsPaused = true;
        pausePanel.SetActive(true);

        if (freezeTimeOnPause)
            Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (pausePanel == null) return;

        IsPaused = false;
        pausePanel.SetActive(false);

        if (freezeTimeOnPause)
            Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        escapeAction?.Dispose();

        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}