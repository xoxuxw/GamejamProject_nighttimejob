using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class MainHUDController : MonoBehaviour
{
    [Header("타이머 설정 (분)")]
    public float setMinutes = 3f;

    [Header("돈")]
    public int money = 1000000;

    [Header("업무 목록")]
    public TaskData[] tasks;

    [Header("대화 내용")]
    [TextArea] public string[] dialogues;
    public string speakerName = "점장님";
    public float typingSpeed = 0.05f;

    [Header("업무 리스트 애니메이션")]
    public float closedHeight = 110f;
    public float animSpeed = 8f;

    [System.Serializable]
    public class TaskData
    {
        public string taskText;
        public string subText;
        public bool completed;
    }

    // 타이머
    private float currentTime;
    private bool isTimeOver = false;

    // UI 요소
    private Label timerText;
    private Label moneyText;
    private VisualElement taskPanel;
    private ScrollView taskContent;
    private VisualElement taskArrow;
    private VisualElement dialoguePanel;
    private Label dialogueName;
    private Label dialogueText;

    // 업무 리스트 상태
    private bool isTaskOpen = false;
    private Coroutine taskAnim;

    // 대화 상태
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isTalking = false;
    private Coroutine typingCoroutine;

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        // 요소 찾기
        timerText     = root.Q<Label>("timer-text");
        moneyText     = root.Q<Label>("money-text");
        taskPanel     = root.Q<VisualElement>("task-panel");
        taskContent   = root.Q<ScrollView>("task-content");
        taskArrow     = root.Q<VisualElement>("task-arrow");
        dialoguePanel = root.Q<VisualElement>("dialogue-panel");
        dialogueName  = root.Q<Label>("dialogue-name");
        dialogueText  = root.Q<Label>("dialogue-text");

        // 업무 헤더 클릭 → 토글
        root.Q<Button>("task-header").clicked += ToggleTaskList;

        // 초기화
        currentTime = setMinutes * 60f;
        isTimeOver = false;

        moneyText.text = money.ToString("N0");

        // 업무 리스트 시작 상태
        taskPanel.style.height = closedHeight;
        taskContent.RemoveFromClassList("open");
        taskArrow.RemoveFromClassList("open");

        BuildTaskList();

        // 대화창 숨김
        dialoguePanel.RemoveFromClassList("visible");
        dialogueName.text = speakerName;
    }

    void Update()
    {
        UpdateTimer();
        HandleDialogueInput();
    }

    // ── 타이머 ──
    void UpdateTimer()
    {
        if (isTimeOver) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isTimeOver = true;
        }

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // ── 돈 설정 (외부에서 호출 가능) ──
    public void SetMoney(int value)
    {
        money = value;
        moneyText.text = money.ToString("N0");
    }

    // ── 업무 리스트 ──
    void BuildTaskList()
    {
        taskContent.Clear();

        foreach (var task in tasks)
        {
            var main = new Label(task.taskText);
            main.AddToClassList("task-item");

            var sub = new Label(task.subText);
            sub.AddToClassList("task-sub");

            if (task.completed)
            {
                main.style.color = Color.gray;
                sub.style.color = Color.gray;
            }

            taskContent.Add(main);
            taskContent.Add(sub);
        }
    }

    public void CompleteTask(int index)
    {
        if (index < 0 || index >= tasks.Length) return;
        tasks[index].completed = true;
        BuildTaskList();
    }

    float CalcOpenHeight()
    {
        // 펼침 최대 높이 (이 이상은 스크롤)
        float needed = closedHeight + tasks.Length * 150f + 30f;
        float maxHeight = 700f;
        return Mathf.Min(needed, maxHeight);
    }

    void ToggleTaskList()
    {
        isTaskOpen = !isTaskOpen;

        if (taskAnim != null) StopCoroutine(taskAnim);

        float target = isTaskOpen ? CalcOpenHeight() : closedHeight;

        if (isTaskOpen)
        {
            taskContent.AddToClassList("open");
            taskArrow.AddToClassList("open");
        }
        else
        {
            taskContent.RemoveFromClassList("open");
            taskArrow.RemoveFromClassList("open");
        }

        taskAnim = StartCoroutine(AnimateTaskHeight(target));
    }

    IEnumerator AnimateTaskHeight(float target)
    {
        float current = taskPanel.resolvedStyle.height;

        while (Mathf.Abs(current - target) > 1f)
        {
            current = Mathf.Lerp(current, target, Time.unscaledDeltaTime * animSpeed);
            taskPanel.style.height = current;
            yield return null;
        }

        taskPanel.style.height = target;
    }

    // ── 대화 ──
    void HandleDialogueInput()
    {
        // T키로 시작
        if (!isTalking &&
            Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            StartDialogue();
            return;
        }

        if (!isTalking) return;

        // 스페이스 or 좌클릭으로 진행
        bool nextInput = false;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
            nextInput = true;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
            nextInput = true;

        if (nextInput)
        {
            if (isTyping)
            {
                // 타이핑 중이면 즉시 완성
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                dialogueText.text = dialogues[currentIndex];
                isTyping = false;
            }
            else
            {
                NextDialogue();
            }
        }
    }

    public void StartDialogue()
    {
        if (dialogues == null || dialogues.Length == 0) return;

        dialoguePanel.AddToClassList("visible");
        isTalking = true;
        currentIndex = 0;
        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in dialogues[currentIndex])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex < dialogues.Length)
        {
            typingCoroutine = StartCoroutine(TypeDialogue());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        dialoguePanel.RemoveFromClassList("visible");
    }
}