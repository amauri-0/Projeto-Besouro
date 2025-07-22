using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class TmpFloating : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("Amplitude máxima do deslocamento vertical")]
    public float amplitude = 3f;      // em pixels
    [Tooltip("Velocidade do movimento (ciclos por segundo)")]
    public float speed = 0.5f;

    private RectTransform rt;
    private Vector2 originalPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        // salva a posição inicial para referenciar o offset
        originalPos = rt.anchoredPosition;
    }

    void Update()
    {
        // calcula deslocamento senoidal (entre –amplitude e +amplitude)
        float offsetY = Mathf.Sin(Time.time * speed * Mathf.PI * 2f) * amplitude;
        rt.anchoredPosition = originalPos + new Vector2(0f, offsetY);
    }
}