using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TaskListUI : MonoBehaviour
{
    [System.Serializable]
    public class TaskData
    {
        [Header("메인 업무")]
        public string taskText;

        [Header("서브 텍스트")]
        [TextArea]
        public string subText;

        [Header("완료 여부")]
        public bool completed;
    }

    [Header("업무 목록")]
    public TaskData[] tasks;

    [Header("연결")]
    public Transform contentParent;

    [Header("프리팹")]
    public GameObject textPrefab;

    void Start()
    {
        CreateTaskTexts();
    }

    void Update()
    {
        // 테스트용
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            CompleteTask(0);
        }
    }

    public void CompleteTask(int index)
    {
        if (index < 0 || index >= tasks.Length)
            return;

        tasks[index].completed = true;

        CreateTaskTexts();
    }

    void CreateTaskTexts()
    {
        // 기존 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 새 생성
        foreach (TaskData task in tasks)
        {
            GameObject obj =
                Instantiate(textPrefab, contentParent);

            TextMeshProUGUI mainText =
                obj.GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI subText =
                obj.transform.Find("SubText")
                    .GetComponent<TextMeshProUGUI>();

            // 텍스트 적용
            mainText.text = task.taskText;
            subText.text = task.subText;

            // 완료 처리
            if (task.completed)
            {
                mainText.color = Color.gray;
                subText.color = Color.gray;

                mainText.fontStyle =
                    FontStyles.Strikethrough;
            }
            else
            {
                mainText.color = Color.white;

                subText.color =
                    new Color(0.7f, 0.7f, 0.7f);

                mainText.fontStyle =
                    FontStyles.Normal;
            }
        }
    }
}