using UnityEngine;

public class BeetleClass : MonoBehaviour
{
    [SerializeField] protected string beetleSpecies;
    [SerializeField] protected float beetleSpeed;

    // Propriedades para acesso seguro
    public string Species => beetleSpecies;
    public float Speed => beetleSpeed;

    // Método virtual que pode ser sobrescrito
    public virtual void BeetleBehavior()
    {
        Debug.Log("Comportamento base do besouro");
    }
    void OnMouseDown()
    {
        Debug.Log("Inseto clicado!");
    }
}