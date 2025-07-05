using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Morphotype
{
    Black,
    Red,
    Yellow
}

public enum HabitatType { Earth, Grass }

public enum BeetleState { Walking, Idle, Turning, Fallen, Spining }

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
    // tempo mínimo sem colisão antes de sair de Idle
    public float idleRecoveryDelay = 0.5f;
    private float noCollisionTimer;

    [Header("Patrol Settings (Optional)")]
    public List<Transform> patrolPoints;
    public float zigzagAmplitude = 0.5f;
    public float zigzagFrequency = 2f;

    [Header("Morphotype Selection")]
    public Morphotype morphotype;

    [Header("Habitat Type (Earth or Grass)")]
    public HabitatType habitat;

    [HideInInspector]
    public GameManager.BeetleConfig config;

    // State & movement
    protected BeetleState state;
    protected float stateTimer;
    protected Vector2 direction;
    private Vector2 lastMove;

    // Turn timing
    private float turnTimer;
    private int currentIndex;
    private float zigzagTimer;

    // Components
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider;

    public Sprite grassCursorSprite;
    private Texture2D grassCursorTex;
    public GameObject windEffectPrefab;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        InitDirection();
        turnTimer = Random.Range(0f, turnInterval);
        zigzagTimer = Random.Range(0f, 1.5f);
        ChangeState(BeetleState.Walking);

        // extrai a Texture2D do Sprite
        if (grassCursorSprite != null)
            grassCursorTex = grassCursorSprite.texture;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        turnTimer -= Time.deltaTime;

        bool colliding = IsCollidingWithBeetle();

        switch (state)
        {
            case BeetleState.Walking:
                PerformWalk();
                if (turnTimer <= 0f)
                {
                    if (!colliding && Random.value < turnChance && !IsCollidingWithPatrolPoint())
                        ChangeState(BeetleState.Turning, 0.75f);
                    turnTimer = turnInterval;
                }
                if (!colliding)
                    TryEnterIdle(colliding);
                break;

            case BeetleState.Idle:
                stateTimer -= Time.deltaTime;
                if (colliding)
                    noCollisionTimer = idleRecoveryDelay;
                else
                    noCollisionTimer -= Time.deltaTime;

                if (stateTimer <= 0f && noCollisionTimer <= 0f)
                    ChangeState(BeetleState.Walking);
                break;

            case BeetleState.Turning:
                if (stateTimer <= 0f)
                {
                    spriteRenderer.flipX = !spriteRenderer.flipX;
                    if (patrolPoints != null && patrolPoints.Count > 0)
                    {
                        // advance to next patrol point in sequence
                        currentIndex = (currentIndex + 1) % patrolPoints.Count;
                        if (IsCollidingWithPatrolPoint())
                            ChangeState(BeetleState.Walking);
                        else if (IsCollidingWithBeetle())
                            ChangeState(BeetleState.Idle);
                        else
                            ChangeState(BeetleState.Walking);
                    }
                    else
                    {
                        direction = -direction;
                        ChangeState(BeetleState.Walking);
                    }
                }
                break;

            case BeetleState.Fallen:
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
        Vector2 move;
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            Vector2 target = patrolPoints[currentIndex].position;
            Vector2 toTarget = (target - (Vector2)transform.position).normalized;

            zigzagTimer += Time.deltaTime * zigzagFrequency * Mathf.PI * 2f;
            Vector2 perp = new Vector2(-toTarget.y, toTarget.x);
            move = (toTarget + perp * Mathf.Sin(zigzagTimer) * zigzagAmplitude).normalized;

            if (Vector2.Distance(transform.position, target) < 0.1f)
                if (currentIndex == 0 || currentIndex == patrolPoints.Count - 1)
                    ChangeState(BeetleState.Turning, 0.75f);
                else
                {
                    currentIndex = (currentIndex + 1) % patrolPoints.Count;
                }
        }
        else
        {
            move = direction.normalized;
        }
        lastMove = move;
        transform.Translate(move * speed * Time.deltaTime);
    }

    protected void TryEnterIdle(bool colliding)
    {
        if (colliding) return;
        if (Random.value < idleChancePerSecond * Time.deltaTime)
            ChangeState(BeetleState.Idle, Random.Range(minIdleTime, maxIdleTime));
    }

    private bool IsCollidingWithBeetle()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0f);
        foreach (var c in hits)
            if (c != boxCollider && c.CompareTag("Beetle"))
                return true;
        return false;
    }

    private bool IsCollidingWithPatrolPoint()
    {
        if (patrolPoints == null) return false;
        foreach (var pt in patrolPoints)
        {
            Collider2D col = pt.GetComponent<Collider2D>();
            if (col != null && boxCollider.bounds.Intersects(col.bounds))
                return true;
        }
        return false;
    }

    protected void ChangeState(BeetleState newState, float duration = 0f)
    {
        state = newState;
        stateTimer = duration;
        if (newState == BeetleState.Idle)
            noCollisionTimer = idleRecoveryDelay;
        animator.SetBool("isWalking", newState == BeetleState.Walking);
        animator.SetBool("isFallen", newState == BeetleState.Fallen);
        animator.SetBool("isTurning", newState == BeetleState.Turning);
        animator.SetBool("isIdle", newState == BeetleState.Idle);
        animator.SetBool("isSpining", newState == BeetleState.Spining);
    }

    public void KnockDown()
    {
        if (state != BeetleState.Fallen)
        {
            if (this.habitat == HabitatType.Earth)
            {
                return;
            }

            if (windEffectPrefab == null) return;
            // 1) Pega a posição do clique na tela
            Vector3 screenPos = Input.mousePosition;

            // 2) Converte para posição no mundo, z = 0 para 2D
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            // 3) Instancia o efeito ali
            Instantiate(windEffectPrefab, worldPos, Quaternion.identity);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            ChangeState(BeetleState.Fallen);
            this.habitat = HabitatType.Earth;
            StartCoroutine(FallToY(1.6f, 0.3f));  // 0.3s de duração, por exemplo
        }
    }

    private IEnumerator FallToY(float targetY, float duration)
    {
        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, targetY, start.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    void OnMouseDown()
    {
        KnockDown();
    }

    void OnMouseEnter()
    {
        if (habitat == HabitatType.Grass && grassCursorTex != null)
            Cursor.SetCursor(grassCursorTex, Vector2.zero, CursorMode.Auto);
    }
    void OnMouseExit()
    {
        if (habitat == HabitatType.Grass)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}