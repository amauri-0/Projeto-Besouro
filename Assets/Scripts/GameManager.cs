using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            RefillByGenetics(21);
        }
    }

    [Serializable]
    public struct BeetleConfig
    {
        public Morphotype morphotype;
        public HabitatType habitat;
    }

    [Header("Configurações dos besouros ativos")]
    public List<BeetleConfig> beetleConfigs = new List<BeetleConfig>();

    [HideInInspector] public List<BeetleConfig> eatenConfigs = new List<BeetleConfig>();
    [HideInInspector] public List<BeetleConfig> driftConfigs = new List<BeetleConfig>();

    /// <summary>
    /// Chamado por Frog quando um besouro é comido.
    /// Remove de beetleConfigs e adiciona a eatenConfigs.
    /// </summary>
    public void OnBeetleEaten(BeetleConfig cfg)
    {
        if (beetleConfigs.Remove(cfg))
        {
            eatenConfigs.Add(cfg);
            Debug.Log($"Beetle comido: {cfg.morphotype}, {cfg.habitat}");
        }
        else
        {
            Debug.LogWarning("Tentou remover um BeetleConfig que não estava ativo!");
        }
    }

    /// <summary>
    /// Repreenche beetleConfigs com novos cfgs via herança genética simples.
    /// </summary>
    public void RefillByGenetics(int totalTarget)
    {
        int remanescentes = beetleConfigs.Count;
        int faltantes = totalTarget - remanescentes;
        if (faltantes <= 0)
        {
            Debug.Log("Nenhuma reposição necessária.");
            return;
        }

        // Agrupar por morfotipo e habitat dos remanescentes
        var combosRemanescentes = beetleConfigs
            .GroupBy(cfg => new { cfg.morphotype, cfg.habitat })
            .Select(g => new { g.Key.morphotype, g.Key.habitat })
            .ToList();
        int numCombos = combosRemanescentes.Count;

        if (numCombos == 0)
        {
            Debug.LogWarning("Nenhum grupo remanescente para basear herança genética!");
            return;
        }

        // Distribuição base e excesso para atingir exatamente faltantes
        int baseCount = faltantes / numCombos;
        int excess = faltantes % numCombos;

        var rnd = new System.Random();
        var novosTotal = new List<BeetleConfig>();

        for (int i = 0; i < numCombos; i++)
        {
            var morph = combosRemanescentes[i].morphotype;
            var habitat = combosRemanescentes[i].habitat;
            int alvo = baseCount + (i < excess ? 1 : 0);

            // Seleciona candidatos que correspondam a este combo
            var candidatos = eatenConfigs
                .Where(cfg => cfg.morphotype == morph && cfg.habitat == habitat)
                .ToList();
            if (candidatos.Count == 0)
                candidatos = beetleConfigs
                    .Where(cfg => cfg.morphotype == morph && cfg.habitat == habitat)
                    .ToList();

            for (int j = 0; j < alvo; j++)
            {
                // Gerar novo indivíduo mantendo combo
                var child = new BeetleConfig
                {
                    morphotype = morph,
                    habitat = habitat
                };
                novosTotal.Add(child);
            }
        }

        // Atualiza o vetor principal e limpa comidos
        beetleConfigs.AddRange(novosTotal);
        eatenConfigs.Clear();
        driftConfigs.Clear();

        Debug.Log($"População completada com herança genética. Total atual: {beetleConfigs.Count}");
    }


    public void KillOneThirdByDrift()
    {
        int total = beetleConfigs.Count;
        int toKill = total / 3;
        var rnd = new System.Random();

        // embaralha índice
        var indices = Enumerable.Range(0, total).OrderBy(_ => rnd.Next()).Take(toKill).ToList();

        // itera em ordem decrescente para remover sem bagunçar índices
        foreach (int i in indices.OrderByDescending(i => i))
        {
            var cfg = beetleConfigs[i];
            beetleConfigs.RemoveAt(i);
            driftConfigs.Add(cfg);
        }
    }
}
