using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("설정 시간 (분)")]
    public float setMinutes = 3f;

    private float currentTime;
    private TextMeshProUGUI textUI;
    private bool isTimeOver = false;

    void Start()
    {
        // 현재 오브젝트의 TMP 텍스트 가져오기
        textUI = GetComponent<TextMeshProUGUI>();

        // 분 → 초 변환
        currentTime = setMinutes * 60f;
    }

    void Update()
    {
        if (isTimeOver)
            return;

        // 시간 감소
        currentTime -= Time.deltaTime;

        // 시간 종료
        if (currentTime <= 0)
        {
            currentTime = 0;
            isTimeOver = true;

            TimeOver();
        }

        // 분/초 계산
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        // 텍스트 표시
        textUI.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TimeOver()
    {
        Debug.Log("시간 종료!");

        // 게임 멈추기
        Time.timeScale = 0f;
    }
}