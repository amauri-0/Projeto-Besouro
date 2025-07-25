using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform textTransform;    // Posicione no Inspector onde deve começar
    public TextMeshProUGUI tmpText;
    public Image displayImage;

    [Header("Content")]
    public string[] messages;
    public Sprite[] sprites;

    [Header("Scroll Settings")]
    public float scrollSpeed = 200f;
    [Tooltip("Posição X para onde o texto deve pular (ex.: 1920).")]
    public float resetX = 2221f;

    [Header("Beetle Reference")]
    [Tooltip("O GameObject do besouro a ser ativado/desativado.")]
    public GameObject besouro;

    float textWidth;
    int idx;

    void Start()
    {
        // Exibe algo já no começo
        ShowRandom();
    }

    void Update()
    {
        // Move o texto para a esquerda
        Vector2 pos = textTransform.anchoredPosition;
        pos.x -= scrollSpeed * Time.deltaTime;

        // Condições diferentes de reset para cada idx (caso especial)
        if (idx == 0 && pos.x < -2400f)
            pos.x = resetX;
        else if (idx == 1 && pos.x < -3900f)
            pos.x = resetX;
        else if (idx == 2 && pos.x < -2900f)
            pos.x = resetX;

        textTransform.anchoredPosition = pos;
    }

    /// <summary>
    /// Escolhe aleatoriamente uma mensagem + sprite, ativa/desativa o besouro e
    /// recalcula a largura do texto.
    /// </summary>
    [ContextMenu("Show Random")]
    public void ShowRandom()
    {
        if (messages.Length == 0 || sprites.Length == 0 || messages.Length != sprites.Length)
        {
            Debug.LogError("Messages e Sprites devem estar com mesmo tamanho e não podem estar vazios.");
            return;
        }

        idx = Random.Range(0, messages.Length);
        tmpText.text = messages[idx];
        displayImage.sprite = sprites[idx];

        // Ativa o besouro apenas se idx == 2
        if (besouro != null)
            besouro.SetActive(idx == 2);

        // Força TMP a recalcular o tamanho
        Canvas.ForceUpdateCanvases();
        textWidth = tmpText.GetPreferredValues(tmpText.text).x;
    }
    public static void ShowRandomGlobal()
    {
        var inst = Object.FindFirstObjectByType<NewsManager>(FindObjectsInactive.Include);
        if (inst != null)
            inst.ShowRandom();
        else
            Debug.LogWarning("Nenhum NewsManager encontrado na cena!");
    }

    public void ResetPositionNews()
    {
        Vector2 pos = textTransform.anchoredPosition;
        pos.x -= scrollSpeed * Time.deltaTime;
        pos.x = resetX;
        textTransform.anchoredPosition = pos;
    }
}
