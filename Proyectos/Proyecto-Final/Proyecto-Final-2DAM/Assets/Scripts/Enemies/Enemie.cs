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

    // === NUEVAS VARIABLES PARA DISPARO ===
    public GameObject projectilePrefab;
    public Transform firePoint;

    private bool isShooting = false;
    private float shootCooldown = 2f;
    private float lastShotTime = 0f;
    private float exitShootAreaTimer = 0f;
    private bool waitingAfterExit = false;

    // === NUEVA VARIABLE PARA SOLTAR POCIÓN ===
    public GameObject potionPrefab; // Prefab de la poción asignado desde el inspector
    [Range(0f, 1f)] public float dropChance = 0.3f; // Probabilidad de soltar poción (30%)

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolRadius = Random.Range(2f, 4f);
        patrolCooldown = Random.Range(1.5f, 3f);
        chaseDelay = Random.Range(0.3f, 1.2f);
        patrolTarget = GetNewPatrolTarget();
    }

    private void Update()
    {
        // Comprobamos si se presiona la tecla 'M'
        if (Input.GetKeyDown(KeyCode.M))
        {
            MatarTodosLosEnemigos();
        }
    }

    private void MatarTodosLosEnemigos()
    {
        // Encontrar todos los enemigos en la escena
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemigo in enemigos)
        {
            // Llamar al método de destrucción de cada enemigo
            enemigo.GetComponent<EnemyFollow>().TakeDamage(1000f);  // Damos un daño alto para destruirlo
        }
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

        // === SI ESTÁ DISPARANDO, SE QUEDA QUIETO Y DISPARA ===
        if (isShooting)
        {
            ShootAtPlayer();
            return;
        }

        // === SI SALIÓ DEL ÁREA DE DISPARO, ESPERA 2 SEGUNDOS ANTES DE VOLVER A PERSEGUIR ===
        if (waitingAfterExit)
        {
            exitShootAreaTimer += Time.fixedDeltaTime;
            if (exitShootAreaTimer >= 2f)
            {
                waitingAfterExit = false;
                currentState = EnemyState.Chasing;
            }
            else
            {
                rb.velocity = Vector2.zero;
                return;
            }
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

        // === SOLTAR POCIÓN DE FORMA ALEATORIA ===
        if (potionPrefab != null && Random.value < dropChance)
        {
            Instantiate(potionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // === NUEVOS MÉTODOS PARA DISPARO ===

    public void SetShooting(bool value)
    {
        isShooting = value;
        if (value)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void OnExitShootingArea()
    {
        isShooting = false;
        waitingAfterExit = true;
        exitShootAreaTimer = 0f;
    }

    private void ShootAtPlayer()
    {
        if (player == null || projectilePrefab == null || firePoint == null) return;

        rb.velocity = Vector2.zero;

        if (Time.time - lastShotTime >= shootCooldown)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Ignorar colisión con el enemigo
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            Collider2D enemyCollider = GetComponent<Collider2D>();
            if (bulletCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, enemyCollider);
            }

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = dir * 5f; // Velocidad hacia el jugador
            }

            lastShotTime = Time.time;
        }
    }
}
