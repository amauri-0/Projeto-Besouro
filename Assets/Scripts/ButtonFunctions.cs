using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void ChangeSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("SceneChanger: sceneName est� vazio ou nulo.");
    }

    public void ChangeSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(sceneIndex);
        else
            Debug.LogWarning($"SceneChanger: �ndice de cena inv�lido ({sceneIndex}).");
    }

    public void QuitGame()
    {
        // No editor, isso n�o fecha; em build, encerra o jogo
#if UNITY_EDITOR
        //UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Saiu do jogo");
#else
        Application.Quit();
#endif
    }
}