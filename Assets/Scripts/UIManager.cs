using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;  

public class UIManager : MonoBehaviour
{
    [Header("Canvas principal do tutorial")]
    [Tooltip("Arraste aqui o Canvas principal (contendo todos os panels e a contagem).")]
    public GameObject mainCanvas;

    [Header("Lista de telas do tutorial (qualquer número)")]
    [Tooltip("Arraste aqui todos os panels/canvases do tutorial, na ordem.")]
    public List<GameObject> tutorialPanels;

    [Header("UI da contagem regressiva")]
    [Tooltip("TMP_Text para exibir 3, 2, 1, Vai!")]
    public TMPro.TMP_Text countdownText;

    private int currentIndex = 0;

    void Start()
    {
        // Ativa o Canvas principal
        if (mainCanvas != null)
            mainCanvas.SetActive(true);

        // Pausa o jogo
        Time.timeScale = 0f;

        // Garante que só o primeiro panel esteja ativo
        for (int i = 0; i < tutorialPanels.Count; i++)
            tutorialPanels[i].SetActive(i == 0);

        // Esconde o texto de contagem até o fim do tutorial
        countdownText.gameObject.SetActive(false);
    }

    // Chamado pelo botão Avançar
    public void Next()
    {
        // fecha o panel atual
        tutorialPanels[currentIndex].SetActive(false);

        currentIndex++;
        if (currentIndex < tutorialPanels.Count)
        {
            // mostra o próximo
            tutorialPanels[currentIndex].SetActive(true);
        }
        else
        {
            // passou do último → acaba o tutorial
            EndTutorial();
        }
    }

    // Chamado pelo botão Voltar
    public void Previous()
    {
        if (currentIndex == 0) return;

        // fecha atual
        tutorialPanels[currentIndex].SetActive(false);
        currentIndex--;

        // mostra o anterior
        tutorialPanels[currentIndex].SetActive(true);
    }

    // Chamado pelo botão Pular
    public void Skip()
    {
        EndTutorial();
    }

    private void EndTutorial()
    {
        // garante que todos os panels estejam fechados
        foreach (var panel in tutorialPanels)
            panel.SetActive(false);

        // Desativa o Canvas principal
        if (mainCanvas != null)
            mainCanvas.SetActive(false);

        // inicia a contagem regressiva
        StartCoroutine(CountdownAndResume());
    }

    private IEnumerator CountdownAndResume()
    {
        countdownText.gameObject.SetActive(true);

        // Use WaitForSecondsRealtime porque Time.timeScale está em zero
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownText.text = "Vai!";
        yield return new WaitForSecondsRealtime(1f);

        // Esconde texto e despausa o jogo
        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
