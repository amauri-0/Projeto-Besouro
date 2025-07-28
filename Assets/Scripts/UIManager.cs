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
    [Header("Conclusion Scene")]
    [Tooltip("Objeto que representa a cena final após todos os ciclos.")]
    public GameObject parentDesfecho;

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
    public float predatorDuration = 14f;

    [Header("Cycle Settings")]
    public int totalCycles = 3;
    private int currentCycle = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip beetlesClip;
    public AudioClip predatorClip;
    public AudioClip newsClip;
    public AudioClip desfechoClip;
    public GameObject cronometerSource;

    [Header("Transition Panel")]
    [Tooltip("GameObject que contém o RawImage de transição.")]
    public GameObject transitionPanelObject;
    [Tooltip("Material que usa o shader UI/CircleWipe")]
    public Material transitionMaterial;
    public float transitionSpeed = 1f;

    [Header("UI References")]
    public Button newsNextButton;
    public Button reviewNextButton;
    public GameObject menuButton;

    [Header("Losses UI")]
    public Transform driftContainer;
    public Transform eatenContainer;
    public Sprite[] beetleIcons;       // 0=Black,1=Red,2=Yellow
    public GameObject iconPrefab;

    [Header("Spawner Reference")]
    public BeetleSpawner beetleSpawner; // arrastar no Inspector

    [Header("Screenshot")]
    public TakeScreenshot takeScreenshot;

    [Header("Countdown")]
    public Countdown countdown;

    [Header("NewsManager")]
    public NewsManager newsManager;

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
            yield return ShowTransition(parentBeetles, true, true);
            yield return ShowBeetles();

            if (!tutorialShown)
            {
                yield return ShowTransition(parentTutorial, false, false);
                yield return RunTutorial();
                tutorialShown = true;
            }

            yield return ShowTransition(parentPredator, true, false);
            yield return ShowPredator();
            ShowBeetleEaten(GameManager.Instance.eatenConfigs);

            NewsManager.ShowRandomGlobal();
            newsManager.ResetPositionNews();
            yield return ShowTransition(parentNews, false, false, predatorTarget);
            yield return ShowNews();
            GameManager.Instance.KillOneThirdByDrift();
            ShowBeetleDrift(GameManager.Instance.driftConfigs);
            yield return new WaitUntil(() => !newsNextButton.interactable);

            yield return ShowTransition(parentLosses, false, false);
            yield return ShowReview();
            yield return new WaitUntil(() => !reviewNextButton.interactable);

            currentCycle++;
        }

        yield return ShowTransition(parentBeetles, true, true);
        yield return ShowBeetles();
        // Cena final após todos os ciclos
        yield return ShowTransition(parentDesfecho, false, false);
        ActivateOnly(parentDesfecho);
        if (desfechoClip != null)
            audioSource.PlayOneShot(desfechoClip);
    }

    private IEnumerator ShowBeetles()
    {
        ActivateOnly(parentBeetles);
        audioSource.PlayOneShot(beetlesClip);
        yield return new WaitForSeconds(0.8f);
        CaptureAndShowScreenshot();
        yield return new WaitForSeconds(1.2f);
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
        tutorialPrevButton.interactable = tutorialIndex > 0;
        tutorialNextButton.interactable = true;
        skipTutorialButton.interactable = true;

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
            UpdateTutorialButtons();
        }
        else
        {
            tutorialIndex = tutorialPanels.Count;
        }
    }

    private void OnSkipTutorial()
    {
        tutorialIndex = tutorialPanels.Count;
    }

    private IEnumerator ShowPredator()
    {
        ActivateOnly(parentPredator);
        audioSource.PlayOneShot(predatorClip);
        yield return new WaitForSeconds(predatorDuration-1);
        cronometerSource.SetActive(true);
        yield return new WaitForSeconds(1);
    }

    private IEnumerator ShowNews()
    {
        cronometerSource.SetActive(false);
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

    private IEnumerator ShowTransition(GameObject next, bool beetle, bool spining, Transform centerTarget = null)
    {
        bool activeMenuButton = true;
        transitionPanelObject.SetActive(true);
        Vector2 centerUV = centerTarget != null
            ? mainCam.WorldToViewportPoint(centerTarget.position)
            : new Vector2(0.5f, 0.5f);
        transitionMaterial.SetVector("_Center", centerUV);

        menuButton.SetActive(false);
        float p = 0.6f;
        while (p > 0f)
        {
            p -= transitionSpeed * Time.unscaledDeltaTime;
            transitionMaterial.SetFloat("_Progress", Mathf.Clamp01(p));
            yield return null;
        }

        ActivateOnly(next);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (beetle)
        {
            GameManager.Instance.RefillByGenetics(21);
            beetleSpawner.SpawnBeetles();
            if(spining)
                SetAllBeetlesToSpining();
            else
            {
                activeMenuButton = false;
                StartCountdownAndUnpause();
                SetAllBeetlesToWalking();
            }
        }
        else
        {
            GameManager.Instance.RefillByGenetics(21);
            beetleSpawner.ClearBeetles();
        }

        while (p < 0.6f)
        {
            p += transitionSpeed * Time.unscaledDeltaTime;
            transitionMaterial.SetFloat("_Progress", Mathf.Clamp01(p));
            yield return null;
        }

        transitionPanelObject.SetActive(false);
        if(activeMenuButton)
            menuButton.SetActive(true);
    }

    private void ActivateOnly(GameObject go)
    {
        var all = new List<GameObject> { parentBeetles, parentTutorial, parentPredator, parentNews, parentLosses, parentDesfecho };
        foreach (var obj in all)
            if (obj != null)
                obj.SetActive(obj == go);
    }

    public void ShowBeetleEaten(List<GameManager.BeetleConfig> eatenList)
    {
        foreach (Transform t in eatenContainer) Destroy(t.gameObject);

        foreach (var cfg in eatenList)
            CreateIcon(cfg.morphotype, eatenContainer);
    }

    public void ShowBeetleDrift(List<GameManager.BeetleConfig> driftList)
    {
        foreach (Transform t in driftContainer) Destroy(t.gameObject);

        foreach (var cfg in driftList)
            CreateIcon(cfg.morphotype, driftContainer);
    }

    private void CreateIcon(Morphotype m, Transform parent)
    {
        var go = Instantiate(iconPrefab, parent);
        var img = go.GetComponent<Image>();
        img.sprite = beetleIcons[(int)m];
        img.preserveAspect = true;
    }

    public void SetAllBeetlesToSpining()
    {
        foreach (var beetle in FindObjectsByType<Beetle>(FindObjectsSortMode.None))
        {
            int random = Random.Range(0, 2);
            if (random == 0)
            {
                beetle.StartSpining();
            }
        }
    }
    public void SetAllBeetlesToWalking()
    {
        foreach (var beetle in FindObjectsByType<Beetle>(FindObjectsSortMode.None))
        {
            beetle.ResumeWalking();
        }
    }

    public void CaptureAndShowScreenshot()
    {
        if (takeScreenshot != null)
            StartCoroutine(takeScreenshot.TakeScreenshotAndShow());
        else
            Debug.LogWarning("UIManager: takeScreenshot não atribuído no Inspector.");
    }

    public void StartCountdownAndUnpause()
    {
        if (countdown != null)
        {
            PauseGame();

            StartCoroutine(countdown.CountdownRoutine());
        }
        else
        {
            Debug.LogWarning("UIManager: countdownManager não atribuído!");
        }
    }

    public static void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public static void UnpauseGame()
    {
        Time.timeScale = 1f;
    }
}
