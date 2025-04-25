using UnityEngine;

public class O_catenulataBeetle : BeetleClass
{
    private void Start()
    {
        // Inicializa valores específicos
        beetleSpecies = "Ogdoecosta catenulata";
        beetleSpeed = 2f;
    }

    // Sobrescreve o comportamento se necessário
    public override void BeetleBehavior()
    {
        base.BeetleBehavior(); // Chama o comportamento base
        Debug.Log("Vivendo minha vida medíocre!");
    }
}