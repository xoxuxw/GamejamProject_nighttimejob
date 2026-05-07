using UnityEngine;
using System.Collections;

public class DropdownUI : MonoBehaviour
{
    [Header("연결")]
    public RectTransform panel;   // 전체 패널
    public RectTransform content; // 내용
    public RectTransform arrow;   // 화살표

    [Header("크기")]
    public float closedHeight = 120f;
    public float openHeight = 350f;

    [Header("속도")]
    public float speed = 8f;

    private bool isOpen = false;
    private Coroutine anim;

    void Start()
    {
        // 시작 상태
        panel.sizeDelta =
            new Vector2(panel.sizeDelta.x, closedHeight);

        content.gameObject.SetActive(false);

        // 화살표 아래 방향
        arrow.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        // 내용 표시
        content.gameObject.SetActive(isOpen);

        // 기존 애니메이션 중지
        if (anim != null)
            StopCoroutine(anim);

        // 목표 높이
        float targetHeight =
            isOpen ? openHeight : closedHeight;

        // 애니메이션 시작
        anim = StartCoroutine(
            AnimateHeight(targetHeight)
        );

        // 화살표 회전
        if (isOpen)
        {
            // ▲
            arrow.localRotation =
                Quaternion.Euler(0, 0, 180f);
        }
        else
        {
            // ▼
            arrow.localRotation =
                Quaternion.Euler(0, 0, 0);
        }
    }

    IEnumerator AnimateHeight(float target)
    {
        while (Mathf.Abs(panel.sizeDelta.y - target) > 1f)
        {
            float newY = Mathf.Lerp(
                panel.sizeDelta.y,
                target,
                Time.deltaTime * speed
            );

            panel.sizeDelta =
                new Vector2(panel.sizeDelta.x, newY);

            yield return null;
        }

        // 정확히 맞춤
        panel.sizeDelta =
            new Vector2(panel.sizeDelta.x, target);
    }
}