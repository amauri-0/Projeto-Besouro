using UnityEngine;
using System.Collections.Generic;

public enum BeetleState { Walking, Idle, Turning, Fallen }
public enum Morphotype { TypeA, TypeB, TypeC }

[RequireComponent(typeof(Collider2D))]
public class Beetle : MonoBehaviour
{
    [Header("General Settings")]
    public float speed = 2f;
    public float turnInterval = 3f;
    [Range(0f, 1f)] public float turnChance = 0.3f;

    [Header("Idle Settings")]
    public float idleChancePerSecond = 0.2f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 2f;

    [Header("Patrol Settings (Optional)")]
    public List<Transform> patrolPoints;
    public float zigzagAmplitude = 0.5f;
    public float zigzagFrequency = 2f;

    protected BeetleState state;
    protected float stateTimer;
    protected Vector2 direction;
    private float turnTimer;
    private int currentIndex;
    private float zigzagTimer;
    private int patrolSign = 1;
    private bool canBePreyed = false;

    void Awake()
    {
        InitDirection();
        ChangeState(BeetleState.Walking);
        turnTimer = turnInterval;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        turnTimer -= Time.deltaTime;

        switch (state)
        {
            case BeetleState.Walking:
                PerformWalk();
                if (turnTimer <= 0f)
                {
                    if (Random.value < turnChance)
                        ChangeState(BeetleState.Turning, 0.5f);
                    turnTimer = turnInterval;
                }
                TryEnterIdle();
                break;

            case BeetleState.Idle:
                if (stateTimer <= 0f)
                    ChangeState(BeetleState.Walking);
                break;

            case BeetleState.Turning:
                if (stateTimer <= 0f)
                {
                    if (patrolPoints != null && patrolPoints.Count > 0)
                    {
                        // Inverte a ordem de patrulha
                        patrolSign *= -1;
                        // Ajusta índice para continuar de forma contínua
                        currentIndex = (currentIndex + patrolSign + patrolPoints.Count) % patrolPoints.Count;
                    }
                    else
                    {
                        // Inverte direção de movimento livre
                        direction = -direction;
                    }
                    ChangeState(BeetleState.Walking);
                }
                break;

            case BeetleState.Fallen:
                // Permanece caído
                break;
        }
    }

    protected void InitDirection()
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void PerformWalk()
    {
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            Vector2 target = patrolPoints[currentIndex].position;
            Vector2 toTarget = (target - (Vector2)transform.position).normalized;
            zigzagTimer += Time.deltaTime * zigzagFrequency * Mathf.PI * 2f;
            Vector2 perp = new Vector2(-toTarget.y, toTarget.x);
            Vector2 move = (toTarget + perp * Mathf.Sin(zigzagTimer) * zigzagAmplitude).normalized;
            transform.Translate(move * speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, target) < 0.1f)
                currentIndex = (currentIndex + patrolSign + patrolPoints.Count) % patrolPoints.Count;
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    protected void TryEnterIdle()
    {
        if (Random.value < idleChancePerSecond * Time.deltaTime)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            bool collision = false;
            foreach (var c in hits)
                if (c.gameObject != gameObject && c.GetComponent<Beetle>() != null)
                    collision = true;

            if (!collision)
                ChangeState(BeetleState.Idle, Random.Range(minIdleTime, maxIdleTime));
        }
    }

    protected void ChangeState(BeetleState newState, float duration = 0f)
    {
        state = newState;
        stateTimer = duration;
        Debug.Log("New state: "+newState);
    }

    public void KnockDown()
    {
        if (state != BeetleState.Fallen)
        {
            ChangeState(BeetleState.Fallen);
            canBePreyed = true;
        }
    }

    public bool CanBePreyed() => canBePreyed;

    void OnMouseDown()
    {
        KnockDown();
    }
}
