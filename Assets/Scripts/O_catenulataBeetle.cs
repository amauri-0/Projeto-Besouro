using UnityEngine;

public class O_catenulataBeetle : BeetleClass
{
    private void Start()
    {
        // Inicializa valores espec�ficos
        beetleSpecies = "Ogdoecosta catenulata";
        beetleSpeed = 2f;
    }

    // Sobrescreve o comportamento se necess�rio
    public override void BeetleBehavior()
    {
        base.BeetleBehavior(); // Chama o comportamento base
        Debug.Log("Vivendo minha vida med�ocre!");
    }
}