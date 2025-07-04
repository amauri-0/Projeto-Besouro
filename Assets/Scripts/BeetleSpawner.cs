using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeetleSpawner : MonoBehaviour
{
    [Header("Patrol Point Parents (23 Objects)")]
    [Tooltip("Arraste aqui 23 GameObjects: cada um deve ter exatamente 2 filhos (os pontos de patrol)")]
    public List<Transform> patrolPointParents = new List<Transform>();

    [Header("Beetle Prefabs")]
    [Tooltip("Ordem: Black, Red, Yellow (mesmo enum Morphotype)")]
    public GameObject[] beetlePrefabs;

    [Header("Checagem de colisão no spawn (apenas Earth)")]
    public float spawnCheckRadius = 0.5f;
    public int maxSpawnAttempts = 20;

    // parâmetros de regras
    const int EarthPairCount = 3;
    const int MaxEarthPerPair = 8;
    const int GrassPairCount = 21;
    const int MaxGrassPerPair = 1;

    void Start()
    {
        int expected = EarthPairCount + GrassPairCount;
        if (patrolPointParents.Count < expected)
        {
            Debug.LogError($"Precisam ser {expected} objetos pais de patrol points (23). Atualmente: {patrolPointParents.Count}.");
            return;
        }
        SpawnBeetles();
    }

    void SpawnBeetles()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("GameManager não encontrado na cena!");
            return;
        }

        int[] earthCounts = new int[EarthPairCount];
        bool[] grassUsed = new bool[GrassPairCount];

        for (int i = 0; i < gm.beetleConfigs.Length; i++)
        {
            var cfg = gm.beetleConfigs[i];
            int parentIndex;

            if (cfg.habitat == HabitatType.Earth)
            {
                var avail = earthCounts.Select((count, idx) => new { count, idx })
                    .Where(x => x.count < MaxEarthPerPair)
                    .Select(x => x.idx).ToList();
                if (avail.Count == 0)
                {
                    Debug.LogError($"Todas as {EarthPairCount} duplas de Earth já atingiram o limite de {MaxEarthPerPair} besouros.");
                    continue;
                }
                parentIndex = avail[Random.Range(0, avail.Count)];
                earthCounts[parentIndex]++;
            }
            else // Grass
            {
                var avail = grassUsed.Select((used, idx) => new { used, idx })
                    .Where(x => !x.used)
                    .Select(x => x.idx).ToList();
                if (avail.Count == 0)
                {
                    Debug.LogError($"Todas as {GrassPairCount} duplas de Grass já foram ocupadas.");
                    continue;
                }
                int grassIdx = avail[Random.Range(0, avail.Count)];
                grassUsed[grassIdx] = true;
                parentIndex = EarthPairCount + grassIdx;
            }

            var parent = patrolPointParents[parentIndex];
            if (parent.childCount < 2)
            {
                Debug.LogError($"Parent em índice {parentIndex} não tem 2 filhos!");
                continue;
            }
            Transform ptA = parent.GetChild(0);
            Transform ptB = parent.GetChild(1);

            float xMin = Mathf.Min(ptA.position.x, ptB.position.x);
            float xMax = Mathf.Max(ptA.position.x, ptB.position.x);
            float y = ptA.position.y;

            if (cfg.habitat == HabitatType.Grass)
            {
                // Spawn sem checagem de colisão
                Vector2 spawnPos = new Vector2(Random.Range(xMin, xMax), y);
                var prefab = beetlePrefabs[(int)cfg.morphotype];
                var go = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                var beetle = go.GetComponent<Beetle>();
                if (beetle != null)
                {
                    beetle.patrolPoints = new List<Transform> { ptA, ptB };
                    beetle.morphotype = cfg.morphotype;
                    beetle.habitat = cfg.habitat;
                }
            }
            else
            {
                // Spawn com checagem de colisão (Earth)
                bool spawned = false;
                for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
                {
                    Vector2 spawnPos = new Vector2(Random.Range(xMin, xMax), y);
                    if (Physics2D.OverlapCircle(spawnPos, spawnCheckRadius) == null)
                    {
                        var prefab = beetlePrefabs[(int)cfg.morphotype];
                        var go = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                        var beetle = go.GetComponent<Beetle>();
                        if (beetle != null)
                        {
                            beetle.patrolPoints = new List<Transform> { ptA, ptB };
                            beetle.morphotype = cfg.morphotype;
                            beetle.habitat = cfg.habitat;
                        }
                        spawned = true;
                        break;
                    }
                }
                if (!spawned)
                    Debug.LogError($"Falha ao spawnar besouro #{i} no parent {parentIndex} após {maxSpawnAttempts} tentativas.");
            }
        }
    }
}