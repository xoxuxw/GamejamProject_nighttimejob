using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("설정 시간 (분)")]
    public float setMinutes = 3f;

    private float currentTime;

    private TextMeshProUGUI textUI;

    private bool isTimeOver = false;

    // 이전 값 저장용
    private float lastSetMinutes;

    void Start()
    {
        textUI = GetComponent<TextMeshProUGUI>();

        currentTime = setMinutes * 60f;

        lastSetMinutes = setMinutes;
    }

    void Update()
    {
        // Inspector 값 변경 감지
        if (setMinutes != lastSetMinutes)
        {
            currentTime = setMinutes * 60f;

            lastSetMinutes = setMinutes;

            isTimeOver = false;
        }

        if (isTimeOver)
            return;

        // 시간 감소
        currentTime -= Time.deltaTime;

        // 시간 종료
        if (currentTime <= 0)
        {
            currentTime = 0;

            isTimeOver = true;

            // TimeOver();
        }

        // 분/초 계산
        int minutes =
            Mathf.FloorToInt(currentTime / 60);

        int seconds =
            Mathf.FloorToInt(currentTime % 60);

        // 텍스트 출력
        textUI.text =
            string.Format("{0:00}:{1:00}",
                minutes,
                seconds);
    }

    void TimeOver()
    {
        Debug.Log("시간 종료!");

        Time.timeScale = 0f;
    }
}