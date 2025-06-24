using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Frog : MonoBehaviour
{
    [Header("Velocidade de movimento em unidades/segundo")]
    public float speed = 8f;

    [Header("Limites de movimento no eixo X")]
    public float minX = -11f;
    public float maxX = 11f;

    void Update()
    {
        // Leitura do eixo horizontal: A/D, ←/→
        float input = Input.GetAxisRaw("Horizontal");
        // input será -1 (esquerda), 0 (neutro) ou +1 (direita)

        // Calcula novo X
        float newX = transform.position.x + input * speed * Time.deltaTime;

        // Aplica clamp para manter dentro dos limites
        newX = Mathf.Clamp(newX, minX, maxX);

        // Atualiza posição mantendo Y e Z originais
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
