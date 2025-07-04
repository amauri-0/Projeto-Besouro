using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton padr�o
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Serializable]
    public struct BeetleConfig
    {
        [Header("Defina o morphotype e habitat de cada besouro")]
        public Morphotype morphotype;
        public HabitatType habitat;
    }

    [Header("Configura��es dos 21 besouros")]
    [Tooltip("Ajuste o tamanho do array para 21 no Inspector e defina para cada elemento Morphotype e Habitat")]
    public BeetleConfig[] beetleConfigs = new BeetleConfig[21];
}