using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Button))]
public class ButtonAnimator : MonoBehaviour
{
    [Header("Parâmetros de Trigger")]
    [SerializeField] private string pressTrigger = "press";
    [SerializeField] private string reflectTrigger = "reflect";

    [Header("Configuração do Reflect (opcional)")]
    [SerializeField] private bool useReflect = false;
    [SerializeField] private float reflectInterval = 10f;

    [Header("Ações após o Press (substitui OnClick padrão)")]
    public UnityEvent onClickDelayed;

    [Header("Button Sound")]
    [Tooltip("Nome do GameObject que contém o AudioSource de som de clique.")]
    [SerializeField] private string buttonSoundObjectName = "ButtonSound";

    private Animator animator;
    private Button button;
    private AudioSource buttonSoundSource;

    void Awake()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();

        GameObject bs = GameObject.Find(buttonSoundObjectName);
        if (bs != null)
            buttonSoundSource = bs.GetComponent<AudioSource>();
        else
            Debug.LogWarning($"ButtonAnimator: GameObject '{buttonSoundObjectName}' não encontrado.");

        button.onClick.AddListener(OnButtonClicked);
    }

    void Start()
    {
        if (useReflect)
            ScheduleNextReflect();
    }

    private void OnButtonClicked()
    {
        if (buttonSoundSource != null)
            buttonSoundSource.Play();

        button.interactable = false;
        animator.SetTrigger(pressTrigger);

        if (useReflect)
        {
            CancelInvoke(nameof(TriggerReflect));
            ScheduleNextReflect();
        }

        float pressDuration = GetCurrentClipLength(pressTrigger);
        StartCoroutine(InvokeAfterDelay(pressDuration + 0.2f));
    }

    private IEnumerator InvokeAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        onClickDelayed.Invoke();
        button.interactable = true;
    }

    private void ScheduleNextReflect()
    {
        Invoke(nameof(TriggerReflect), reflectInterval);
    }

    private void TriggerReflect()
    {
        if (!useReflect) return;

        animator.SetTrigger(reflectTrigger);
        ScheduleNextReflect();
    }

    private float GetCurrentClipLength(string triggerName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.ToLower().Contains(triggerName.ToLower()))
                return clip.length;
        }
        return 0.2f;
    }

    private void OnEnable()
    {
        button.interactable = true;
    }
}
