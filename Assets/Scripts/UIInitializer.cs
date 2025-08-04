using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInitializer : MonoBehaviour
{
    [Header("Arraste aqui seu Dropdown e Toggle desta cena")]
    public TMP_Dropdown dropdown;
    public Toggle toggle;

    void Awake()
    {
        if (dropdown == null || toggle == null)
        {
            Debug.LogError("UIInitializer: dropdown ou toggle não atribuídos no Inspector.");
            return;
        }
        // Inicializa o manager com estas referências
        SettingMenuManager.Instance.InitializeUI(dropdown, toggle);
        Debug.Log("UIInitializer: Dropdown e Toggle inicializados.");
    }
}
