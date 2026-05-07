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

    [Header("내용 표시 딜레이")]
    public float contentDelay = 0.2f;

    private bool isOpen = false;

    private Coroutine anim;

    void Start()
    {
        // 시작 상태
        panel.sizeDelta =
            new Vector2(
                panel.sizeDelta.x,
                closedHeight
            );

        // 내용 숨김
        content.gameObject.SetActive(false);

        // 화살표 아래 방향
        arrow.localRotation =
            Quaternion.Euler(0, 0, 0);
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        // 기존 애니메이션 중지
        if (anim != null)
            StopCoroutine(anim);

        // 목표 높이
        float targetHeight =
            isOpen ? openHeight : closedHeight;

        // 패널 애니메이션 시작
        anim = StartCoroutine(
            AnimateHeight(targetHeight)
        );

        // 펼칠 때
        if (isOpen)
        {
            // 내용 딜레이 표시
            StartCoroutine(ShowContentDelay());

            // 화살표 반전
            arrow.localRotation =
                Quaternion.Euler(0, 0, 180f);
        }
        // 닫을 때
        else
        {
            // 내용 숨김
            content.gameObject.SetActive(false);

            // 화살표 원래 방향
            arrow.localRotation =
                Quaternion.Euler(0, 0, 0);
        }
    }

    IEnumerator ShowContentDelay()
    {
        yield return new WaitForSeconds(
            contentDelay
        );

        content.gameObject.SetActive(true);
    }

    IEnumerator AnimateHeight(float target)
    {
        while (
            Mathf.Abs(panel.sizeDelta.y - target)
            > 1f
        )
        {
            float newY = Mathf.Lerp(
                panel.sizeDelta.y,
                target,
                Time.deltaTime * speed
            );

            panel.sizeDelta =
                new Vector2(
                    panel.sizeDelta.x,
                    newY
                );

            yield return null;
        }

        // 정확한 값 보정
        panel.sizeDelta =
            new Vector2(
                panel.sizeDelta.x,
                target
            );
    }
}