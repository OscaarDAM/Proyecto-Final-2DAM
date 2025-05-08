using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    public float speed = 0.5f;
    public float health = 100f;

    private Transform player;
    private Rigidbody2D rb;

    private bool canFollow = false;
    private float timeSinceActivated = 0f;
    private float chaseDelay;

    public Bounds roomBounds; // Limites de la sala asignados desde RoomTrigger

    private enum EnemyState { Idle, Patrolling, Chasing }
    private EnemyState currentState = EnemyState.Patrolling;

    private Vector2 patrolTarget;
    private float patrolRadius;
    private float patrolCooldown;
    private float patrolTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolRadius = Random.Range(2f, 4f);
        patrolCooldown = Random.Range(1.5f, 3f);
        chaseDelay = Random.Range(0.3f, 1.2f);
        patrolTarget = GetNewPatrolTarget();
    }

    private void FixedUpdate()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (!canFollow)
        {
            Patrol();
            return;
        }

        timeSinceActivated += Time.fixedDeltaTime;

        if (timeSinceActivated < chaseDelay)
        {
            Patrol();
        }
        else
        {
            currentState = EnemyState.Chasing;
        }

        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
        }
    }

    private void Patrol()
    {
        patrolTimer += Time.fixedDeltaTime;

        if (Vector2.Distance(rb.position, patrolTarget) < 0.1f || patrolTimer > patrolCooldown)
        {
            patrolTarget = GetNewPatrolTarget();
            patrolTimer = 0f;
        }

        Vector2 direction = (patrolTarget - rb.position).normalized;
        Vector2 targetVelocity = direction * speed * 0.5f;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.1f);
    }

    private Vector2 GetNewPatrolTarget()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            Vector2 candidate = rb.position + randomOffset;

            if (roomBounds.Contains(candidate))
                return candidate;
        }

        return rb.position;
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 target = player.position;

        if (!roomBounds.Contains(target)) return;

        Vector2 direction = (target - rb.position).normalized;
        Vector2 separation = GetSeparationVector() * 0.5f;

        Vector2 finalDir = (direction + separation).normalized;
        Vector2 targetVelocity = finalDir * speed;

        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.2f);
    }

    private Vector2 GetSeparationVector()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 1f);

        foreach (var col in nearby)
        {
            if (col.gameObject != this.gameObject && col.CompareTag("Enemy"))
            {
                Vector2 diff = (Vector2)(transform.position - col.transform.position);
                float dist = diff.magnitude;
                if (dist > 0)
                    separation += diff.normalized / dist;
            }
        }

        return separation;
    }

    public void SetCanFollow(bool value)
    {
        canFollow = value;
        timeSinceActivated = 0f;

        if (canFollow)
        {
            currentState = EnemyState.Patrolling;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            KillEnemy();
        }
    }

    private void KillEnemy()
    {
        Debug.Log("El enemigo ha sido destruido.");
        Destroy(gameObject);
    }
}
