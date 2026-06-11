using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class UIButtonAction : MonoBehaviour
{
    public enum ButtonAction
    {
        TogglePanel,
        OpenPanel,
        ClosePanel,
        QuitGame,
        LoadScene,
    }

    [SerializeField] private ButtonAction action = ButtonAction.TogglePanel;
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private GameObject[] otherPanels;
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject confirmPanel;

    void Awake()
    {
        if (action != ButtonAction.QuitGame && 
            action != ButtonAction.LoadScene && 
            targetPanel == null)
        {
            Debug.LogError("targetPanel이 연결되지 않았습니다!", this);
            return;
        }

        if (action != ButtonAction.ClosePanel && 
            action != ButtonAction.QuitGame && 
            action != ButtonAction.LoadScene)
            targetPanel.SetActive(false);

        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        foreach (var panel in otherPanels)
        {
            if (panel == null) continue;
            panel.SetActive(false);
        }

        switch (action)
        {
            case ButtonAction.TogglePanel:
                targetPanel.SetActive(!targetPanel.activeSelf);
                break;
            case ButtonAction.OpenPanel:
                targetPanel.SetActive(true);
                break;
            case ButtonAction.ClosePanel:
                targetPanel.SetActive(false);
                break;
            case ButtonAction.QuitGame:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
            case ButtonAction.LoadScene:
                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError("sceneName이 비어 있습니다!", this);
                    return;
                }

                // confirmPanel이 꺼져 있을 때만 확인 패널 표시
                if (SceneManager.GetActiveScene().name == "MainScene" && sceneName == "Lobby")
                {
                    if (confirmPanel == null)
                    {
                        Debug.LogError("confirmPanel이 연결되지 않았습니다!", this);
                        return;
                    }

                    if (!confirmPanel.activeSelf)
                    {
                        confirmPanel.SetActive(true);
                        return;
                    }
                }

                SceneManager.LoadScene(sceneName);
                break;
        }
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }
}