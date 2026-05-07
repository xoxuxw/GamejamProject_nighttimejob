using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueTest : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [TextArea]
    public string[] dialogues;

    public float typingSpeed = 0.05f;

    private int currentIndex = 0;

    private bool isTyping = false;
    private bool isTalking = false;

    void Start()
    {
        // 시작 시 숨김
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // T 키로 시작
        if (!isTalking &&
            Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            StartDialogue();
        }

        if (!isTalking)
            return;

        // 스페이스 OR 좌클릭
        bool nextInput = false;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            nextInput = true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            nextInput = true;
        }

        if (nextInput)
        {
            // 타이핑 중이면 즉시 출력
            if (isTyping)
            {
                StopAllCoroutines();

                dialogueText.text =
                    dialogues[currentIndex];

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
        dialoguePanel.SetActive(true);

        isTalking = true;

        currentIndex = 0;

        StartCoroutine(TypeDialogue());
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char c in dialogues[currentIndex])
        {
            dialogueText.text += c;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        isTyping = false;
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex < dialogues.Length)
        {
            StartCoroutine(TypeDialogue());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isTalking = false;

        dialoguePanel.SetActive(false);
    }
}