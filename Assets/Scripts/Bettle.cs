using UnityEngine;
using System.Collections.Generic;

public enum BeetleState { Walking, Idle, Turning, Fallen, Spining}
public enum Morphotype { TypeA, TypeB, TypeC }

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
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

    // State & movement
    protected BeetleState state;
    protected float stateTimer;
    protected Vector2 direction;
    private Vector2 lastMove;

    // Turn timing
    private float turnTimer;
    private int currentIndex;
    private float zigzagTimer;
    private int patrolSign = 1;
    private bool canBePreyed = false;

    // Components
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // 1) Direção aleatória (você já faz)
        InitDirection();

        // 2) Timer de turnos começa em um ponto aleatório
        turnTimer = Random.Range(0f, turnInterval);

        // 3) Timer de zigzag
        zigzagTimer = Random.Range(0f, 1.5f);

        // 4) Começa andando
        ChangeState(BeetleState.Walking);
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
                        ChangeState(BeetleState.Turning, 0.75f);
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
                        patrolSign *= -1;
                        currentIndex = (currentIndex + patrolSign + patrolPoints.Count) % patrolPoints.Count;
                    }
                    else
                    {
                        direction = -direction;
                    }
                    ChangeState(BeetleState.Walking);
                }
                break;

            case BeetleState.Fallen:
                // permanece caído
                break;
        }

        // Flip sprite based on horizontal movement
        if (lastMove.x != 0f)
            spriteRenderer.flipX = lastMove.x < 0f;
    }

    /// <summary>
    /// Initialize with a random movement direction.
    /// </summary>
    protected void InitDirection()
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// Handles walking logic, with optional patrol + zigzag.
    /// </summary>
    private void PerformWalk()
    {
        Vector2 move;
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            Vector2 target = patrolPoints[currentIndex].position;
            Vector2 toTarget = (target - (Vector2)transform.position).normalized;

            zigzagTimer += Time.deltaTime * zigzagFrequency * Mathf.PI * 2f;
            Vector2 perp = new Vector2(-toTarget.y, toTarget.x);
            move = (toTarget + perp * Mathf.Sin(zigzagTimer) * zigzagAmplitude).normalized;

            if (Vector2.Distance(transform.position, target) < 0.1f)
            {
                ChangeState(BeetleState.Turning, 0.75f);
            }
        }
        else
        {
            move = direction.normalized;
        }

        lastMove = move;
        transform.Translate(move * speed * Time.deltaTime);
    }

    /// <summary>
    /// Randomly enter idle if not colliding with another beetle.
    /// </summary>
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

    /// <summary>
    /// Change beetle state and optionally set duration.
    /// </summary>
    protected void ChangeState(BeetleState newState, float duration = 0f)
    {
        state = newState;
        stateTimer = duration;
        Debug.Log("New state: " + newState);
        animator.SetBool("isWalking", newState == BeetleState.Walking);
        animator.SetBool("isFallen", newState == BeetleState.Fallen);
        animator.SetBool("isTurning", newState == BeetleState.Turning);
        animator.SetBool("isIdle", newState == BeetleState.Idle);
        animator.SetBool("isSpining", newState == BeetleState.Spining);
    }

    /// <summary>
    /// Knock down the beetle (Fallen state).
    /// </summary>
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
