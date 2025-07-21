using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Stage Containers")]
    public GameObject parentBeetles;
    public GameObject parentTutorial;
    public GameObject parentPredator;
    public GameObject parentNews;
    public GameObject parentLosses;

    [Header("Predator Reference")]
    [Tooltip("Objeto que define o centro da transição circular na fase Predator.")]
    public Transform predatorTarget;

    [Header("Tutorial Settings")]
    public List<GameObject> tutorialPanels;
    public Button tutorialPrevButton;
    public Button tutorialNextButton;
    public Button skipTutorialButton;
    private int tutorialIndex = 0;
    private bool tutorialShown = false;

    [Header("Predator Stage")]
    public float predatorDuration = 15f;

    [Header("Cycle Settings")]
    public int totalCycles = 3;
    private int currentCycle = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip beetlesClip;
    public AudioClip predatorClip;
    public AudioClip newsClip;

    [Header("Transition Panel")]
    [Tooltip("GameObject que contém o RawImage de transição.")]
    public GameObject transitionPanelObject;
    [Tooltip("Material que usa o shader UI/CircleWipe")]
    public Material transitionMaterial;
    public float transitionSpeed = 1f;

    [Header("UI References")]
    public Button newsNextButton;
    public Button reviewNextButton;

    [Header("Losses UI")]
    public Transform driftContainer;
    public Transform eatenContainer;
    public Sprite[] beetleIcons;       // 0=Black,1=Red,2=Yellow
    public GameObject iconPrefab;

    [Header("Spawner Reference")]
    public BeetleSpawner beetleSpawner; // arrastar no Inspector

    private Camera mainCam;
    private RawImage transitionImage;

    void Awake()
    {
        transitionImage = transitionPanelObject.GetComponent<RawImage>();
        transitionPanelObject.SetActive(false);
        transitionMaterial.SetFloat("_Progress", 1f);
    }

    void Start()
    {
        mainCam = Camera.main;

        // Tutorial buttons
        tutorialPrevButton.onClick.AddListener(OnTutorialPrev);
        tutorialNextButton.onClick.AddListener(OnTutorialNext);
        skipTutorialButton.onClick.AddListener(OnSkipTutorial);

        // Other flows
        newsNextButton.onClick.AddListener(OnNewsProceed);
        reviewNextButton.onClick.AddListener(OnReviewProceed);

        StartCoroutine(MainFlow());
    }

    private IEnumerator MainFlow()
    {
        while (currentCycle < totalCycles)
        {
            yield return ShowTransition(parentBeetles, true);
            yield return ShowBeetles();

            if (!tutorialShown)
            {
                yield return ShowTransition(parentTutorial, false);
                yield return RunTutorial();
                tutorialShown = true;
            }

            yield return ShowTransition(parentPredator, true);
            yield return ShowPredator();

            yield return ShowTransition(parentNews, false, predatorTarget);
            yield return ShowNews();
            yield return new WaitUntil(() => !newsNextButton.interactable);

            GameManager.Instance.KillOneThirdByDrift();
            ShowBeetleLosses(
                GameManager.Instance.driftConfigs,
                GameManager.Instance.eatenConfigs
            );
            GameManager.Instance.RefillByGenetics(21);

            yield return ShowTransition(parentLosses, false);
            yield return ShowReview();
            yield return new WaitUntil(() => !reviewNextButton.interactable);

            currentCycle++;
        }
    }

    private IEnumerator ShowBeetles()
    {
        ActivateOnly(parentBeetles);
        audioSource.PlayOneShot(beetlesClip);
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator RunTutorial()
    {
        ActivateOnly(parentTutorial);
        UpdateTutorialButtons();
        // mantém enquanto tutorialIndex indica painel válido
        while (tutorialIndex < tutorialPanels.Count)
            yield return null;
    }

    private void UpdateTutorialButtons()
    {
        // Prev só clicável quando houver painel anterior
        tutorialPrevButton.interactable = tutorialIndex > 0;
        // Next sempre clicável; se no último painel, faz skip
        tutorialNextButton.interactable = true;
        skipTutorialButton.interactable = true;

        // Ativa o painel atual (se tutorialIndex < count)
        for (int i = 0; i < tutorialPanels.Count; i++)
            tutorialPanels[i].SetActive(i == tutorialIndex);
    }

    private void OnTutorialPrev()
    {
        if (tutorialIndex > 0)
            tutorialIndex--;
        UpdateTutorialButtons();
    }

    private void OnTutorialNext()
    {
        if (tutorialIndex < tutorialPanels.Count - 1)
        {
            tutorialIndex++;
        }
        else
        {
            // chega no último -> skip
            tutorialIndex = tutorialPanels.Count;
        }
        UpdateTutorialButtons();
    }

    private void OnSkipTutorial()
    {
        tutorialIndex = tutorialPanels.Count;
        UpdateTutorialButtons();
    }

    private IEnumerator ShowPredator()
    {
        ActivateOnly(parentPredator);
        audioSource.PlayOneShot(predatorClip);
        yield return new WaitForSeconds(predatorDuration);
    }

    private IEnumerator ShowNews()
    {
        ActivateOnly(parentNews);
        audioSource.PlayOneShot(newsClip);
        newsNextButton.interactable = true;
        yield return null;
    }

    private IEnumerator ShowReview()
    {
        ActivateOnly(parentLosses);
        reviewNextButton.interactable = true;
        yield return null;
    }

    private void OnNewsProceed() => newsNextButton.interactable = false;
    private void OnReviewProceed() => reviewNextButton.interactable = false;

    private IEnumerator ShowTransition(GameObject next, bool beetle, Transform centerTarget = null)
    {
        transitionPanelObject.SetActive(true);
        Vector2 centerUV = centerTarget != null
            ? mainCam.WorldToViewportPoint(centerTarget.position)
            : new Vector2(0.5f, 0.5f);
        transitionMaterial.SetVector("_Center", centerUV);

        float p = 1f;
        while (p > 0f)
        {
            p -= transitionSpeed * Time.unscaledDeltaTime;
            transitionMaterial.SetFloat("_Progress", Mathf.Clamp01(p));
            yield return null;
        }

        ActivateOnly(next);
        if (beetle)
        {
            GameManager.Instance.RefillByGenetics(21);
            beetleSpawner.SpawnBeetles();
        }
        else
        {
            GameManager.Instance.RefillByGenetics(21);
            beetleSpawner.ClearBeetles();
        }

            while (p < 1f)
            {
                p += transitionSpeed * Time.unscaledDeltaTime;
                transitionMaterial.SetFloat("_Progress", Mathf.Clamp01(p));
                yield return null;
            }

        transitionPanelObject.SetActive(false);
    }

    private void ActivateOnly(GameObject go)
    {
        var all = new List<GameObject> { parentBeetles, parentTutorial, parentPredator, parentNews, parentLosses };
        foreach (var obj in all)
            if (obj != null)
                obj.SetActive(obj == go);
    }

    public void ShowBeetleLosses(
        List<GameManager.BeetleConfig> driftList,
        List<GameManager.BeetleConfig> eatenList)
    {
        foreach (Transform t in driftContainer) Destroy(t.gameObject);
        foreach (Transform t in eatenContainer) Destroy(t.gameObject);

        foreach (var cfg in driftList)
            CreateIcon(cfg.morphotype, driftContainer);
        foreach (var cfg in eatenList)
            CreateIcon(cfg.morphotype, eatenContainer);

        ActivateOnly(parentLosses);
    }

    private void CreateIcon(Morphotype m, Transform parent)
    {
        var go = Instantiate(iconPrefab, parent);
        var img = go.GetComponent<Image>();
        img.sprite = beetleIcons[(int)m];
        img.preserveAspect = true;
    }
}
