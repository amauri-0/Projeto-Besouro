using TMPro;
using UnityEngine;
using System.Collections;

public class Countdown : MonoBehaviour
{
    [Header("Countdown")]
    public TextMeshProUGUI countdownText;

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1f;
    }

    public void StartGameWithCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    public IEnumerator CountdownRoutine()
    {
        // Garante que esteja pausado
        PauseGame();

        // Sequência de valores
        string[] steps = { "3", "2", "1", "Vai!" };
        foreach (var s in steps)
        {
            countdownText.text = s;
            countdownText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownText.gameObject.SetActive(false);
        UnpauseGame();
    }
}
