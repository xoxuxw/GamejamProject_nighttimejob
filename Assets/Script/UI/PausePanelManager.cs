using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ESC 키로 Pause 패널을 열고 닫으며 Time.timeScale을 제어합니다.
/// 씬에 하나만 존재하는 싱글턴입니다.
/// New Input System을 사용합니다.
///
/// [사용법]
/// 1. 빈 GameObject에 이 컴포넌트를 추가합니다.
/// 2. Inspector에서 pausePanel에 Pause UI 패널을 연결합니다.
/// </summary>
public class PausePanelManager : MonoBehaviour
{
    public static PausePanelManager Instance { get; private set; }

    [Header("Pause 패널")]
    [SerializeField] private GameObject pausePanel;

    [Header("옵션")]
    [Tooltip("Pause 시 Time.timeScale을 0으로 설정합니다.")]
    [SerializeField] private bool freezeTimeOnPause = true;

    public bool IsPaused { get; private set; }

    private InputAction escapeAction;

    // ──────────────────────────────────────────────

    void Awake()
    {
        // 싱글턴 설정
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

        // ESC 키 InputAction 등록
        escapeAction = new InputAction(binding: "<Keyboard>/escape");
        escapeAction.performed += _ => Toggle();
    }

    void OnEnable()
    {
        escapeAction?.Enable();
    }

    void OnDisable()
    {
        escapeAction?.Disable();
    }

    // ──────────────────────────────────────────────

    /// <summary>Pause 상태를 토글합니다.</summary>
    public void Toggle()
    {
        if (IsPaused) Resume();
        else          Pause();
    }

    /// <summary>게임을 일시정지합니다.</summary>
    public void Pause()
    {
        if (pausePanel == null) return;

        IsPaused = true;
        pausePanel.SetActive(true);

        if (freezeTimeOnPause)
            Time.timeScale = 0f;
    }

    /// <summary>게임을 재개합니다. Pause 패널의 Resume 버튼에 연결하세요.</summary>
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

        // 이 인스턴스가 싱글턴일 때만 timeScale 복구
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}