using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Button))]
public class HoverColorButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("텍스트 색상")]
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color hoverColor  = Color.red;

    [Header("전환 속도")]
    [SerializeField] private float fadeDuration = 0.15f;

    private TextMeshProUGUI _text;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();

        if (_text == null)
        {
            Debug.LogError("TextMeshProUGUI를 찾을 수 없습니다!", this);
            return;
        }

        _text.color = normalColor;
    }

    // 패널이 켜질 때마다 색상 초기화
    void OnEnable()
    {
        if (_text != null)
            _text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData _) => FadeTo(hoverColor);
    public void OnPointerExit (PointerEventData _) => FadeTo(normalColor);

    private void FadeTo(Color target)
    {
        if (_text == null) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(Color target)
    {
        Color start = _text.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _text.color = Color.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        _text.color = target;
    }
}