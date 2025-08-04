using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingMenuManager : MonoBehaviour
{
    public static SettingMenuManager Instance { get; private set; }

    [HideInInspector] public TMP_Dropdown resDropDown;
    [HideInInspector] public Toggle fullScreenToggle;

    private bool isFullScreen;
    private int selectedResolutionIndex;
    private List<Resolution> selectedResolutionList = new List<Resolution>();

    // Resoluções suportadas pelo jogo, em ordem decrescente
    private readonly List<Vector2Int> baseAllowedResolutions = new List<Vector2Int>()
{
    new Vector2Int(1920, 1080),
    new Vector2Int(1600, 900),
    new Vector2Int(1366, 768),
    new Vector2Int(1280, 720),
    new Vector2Int(854, 480),
    new Vector2Int(640, 360),
};

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
            BuildResolutionList();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Deve ser chamado explicitamente em cada cena que contenha o menu de resolução/toggle,
    /// tipicamente via um script UIInitializer no Awake() da cena.
    /// </summary>
    public void InitializeUI(TMP_Dropdown dropdown, Toggle toggle)
    {
        resDropDown = dropdown;
        fullScreenToggle = toggle;

        PopulateDropdown();
        SyncUI();

        resDropDown.onValueChanged.RemoveAllListeners();
        resDropDown.onValueChanged.AddListener(OnResolutionDropdownChanged);
        fullScreenToggle.onValueChanged.RemoveAllListeners();
        fullScreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);
    }

    private void LoadSettings()
    {
        selectedResolutionIndex = PlayerPrefs.GetInt("ResIndex", 0);
        isFullScreen = PlayerPrefs.GetInt("IsFullScreen", 0) == 1;
    }

    private void BuildResolutionList()
    {
        // Pega resolução atual do monitor onde o jogo está rodando
        Resolution current = Screen.currentResolution;
        int maxWidth = current.width;
        int maxHeight = current.height;

        selectedResolutionList.Clear();
        foreach (var allowed in baseAllowedResolutions)
        {
            // Só inclui se couber na tela atual
            if (allowed.x <= maxWidth && allowed.y <= maxHeight)
            {
                selectedResolutionList.Add(new Resolution { width = allowed.x, height = allowed.y, refreshRateRatio = current.refreshRateRatio });
            }
        }

        // Ajusta índice salvo se for maior que opções disponíveis
        if (selectedResolutionIndex >= selectedResolutionList.Count)
            selectedResolutionIndex = selectedResolutionList.Count - 1;

        // Aplica para garantir consistência
        ApplySettings();
    }

    private void PopulateDropdown()
    {
        if (resDropDown == null) return;
        var options = new List<string>();
        foreach (var res in selectedResolutionList)
            options.Add(res.width + " x " + res.height);

        resDropDown.ClearOptions();
        resDropDown.AddOptions(options);
    }

    private void SyncUI()
    {
        if (resDropDown == null || fullScreenToggle == null) return;
        resDropDown.value = Mathf.Clamp(selectedResolutionIndex, 0, selectedResolutionList.Count - 1);
        resDropDown.RefreshShownValue();
        fullScreenToggle.isOn = isFullScreen;
    }

    private void OnResolutionDropdownChanged(int index)
    {
        selectedResolutionIndex = index;
        ApplySettings();
    }

    private void OnFullScreenToggleChanged(bool fullscreen)
    {
        isFullScreen = fullscreen;
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (selectedResolutionList.Count == 0)
            return;

        var r = selectedResolutionList[selectedResolutionIndex];
        Screen.SetResolution(r.width, r.height, isFullScreen);

        PlayerPrefs.SetInt("ResIndex", selectedResolutionIndex);
        PlayerPrefs.SetInt("IsFullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
