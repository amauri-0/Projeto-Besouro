using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class Frog : MonoBehaviour
{
    [Header("Velocidade de movimento em unidades/segundo")]
    public float speed = 8f;

    [Header("Limites de movimento no eixo X")]
    public float minX = -11f;
    public float maxX = 11f;

    private BoxCollider2D boxCollider;
    private Animator animator;
    private bool isBusy = false; // bloqueia movimento e input durante animação

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isBusy)
        {
            HandleMovement();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EatBeetle();
            }
        }
    }

    private void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");
        float newX = transform.position.x + input * speed * Time.deltaTime;
        newX = Mathf.Clamp(newX, minX, maxX);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private void EatBeetle()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0f);
        Beetle target = null;
        float minY = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Beetle"))
            {
                float y = hit.transform.position.y;
                if (y < minY)
                {
                    minY = y;
                    target = hit.GetComponent<Beetle>();
                }
            }
        }

        string animName;
        float destroyDelay = 0f;

        if (target == null)
        {
            animName = "Tongue_Nothing";
        }
        else
        {
            float[] positions = { -0.8f, 0f, 0.8f, 1.6f };
            float[] delays = { 0.1f, 0.133f, 0.166f, 0.2f };
            int bestIndex = 0;
            float smallestDiff = Mathf.Abs(minY - positions[0]);
            for (int i = 1; i < positions.Length; i++)
            {
                float diff = Mathf.Abs(minY - positions[i]);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    bestIndex = i;
                }
            }

            string morph = target.morphotype.ToString();
            string suffix = (bestIndex + 2).ToString();
            animName = morph + "_" + suffix;
            destroyDelay = delays[bestIndex];
        }

        StartCoroutine(EatAnimationRoutine(target, animName, destroyDelay));
    }

    private IEnumerator EatAnimationRoutine(Beetle target, string animationName, float destroyDelay)
    {
        isBusy = true;
        animator.Play(animationName);

        // Espera para destruir o besouro após tempo específico
        if (target != null)
            yield return new WaitForSeconds(destroyDelay);
        if (target != null)
            Destroy(target.gameObject);

        // Obtém duração da animação
        var clips = animator.runtimeAnimatorController.animationClips;
        float duration = 0f;
        foreach (var clip in clips)
        {
            if (clip.name == animationName)
            {
                duration = clip.length;
                break;
            }
        }

        // Aguarda o resto da animação antes de voltar ao Idle
        float remaining = Mathf.Max(0, duration - destroyDelay);
        yield return new WaitForSeconds(remaining);

        animator.Play("Idle");
        yield return new WaitForSeconds(0.25f);
        isBusy = false;
    }
}
