using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour, IDamageable
{
    // Configuración general del enemigo
    public float speed = 0.5f;
    public int maxHealth = 5;
    public int currentHealth = 5;

    private Transform player;
    private Rigidbody2D rb;

    private bool canFollow = false; // Se activa al entrar a la sala
    private float timeSinceActivated = 0f;
    private float chaseDelay;

    public Bounds roomBounds; // Límites de la sala

    private enum EnemyState { Idle, Patrolling, Chasing }
    private EnemyState currentState = EnemyState.Patrolling;

    // Patrullaje
    private Vector2 patrolTarget;
    private float patrolRadius;
    private float patrolCooldown;
    private float patrolTimer = 0f;

    // Disparo
    public GameObject projectilePrefab;
    public Transform firePoint;

    private bool isShooting = false;
    private float shootCooldown = 2f;
    private float lastShotTime = 0f;

    private float exitShootAreaTimer = 0f;
    private bool waitingAfterExit = false;

    // Sonido de disparo
    public AudioClip shootClip;          // Clip de audio a reproducir al disparar
    private AudioSource audioSource;     // Componente para reproducir el sonido

    // Pociones al morir
    public GameObject potionPrefab;
    [Range(0f, 1f)] public float dropChance = 0.3f;

    // Reubicación tras disparar
    private bool isRelocating = false;
    private Vector2 relocationTarget;
    private float relocationSpeed = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolRadius = Random.Range(2f, 4f);
        patrolCooldown = Random.Range(1.5f, 3f);
        chaseDelay = Random.Range(0.3f, 1.2f);
        patrolTarget = GetNewPatrolTarget();

        // Obtener o agregar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MatarTodosLosEnemigos();
        }
    }

    private void MatarTodosLosEnemigos()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemigo in enemigos)
        {
            enemigo.GetComponent<EnemyFollow>().TakeDamage(1000);
        }
    }

    private void FixedUpdate()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

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

        if (isShooting)
        {
            if (isRelocating)
                RelocateAfterShot();
            else
                ShootAtPlayer();
            return;
        }

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

        Vector2 nextPos = rb.position + finalDir * speed * Time.fixedDeltaTime;
        if (!roomBounds.Contains(nextPos))
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 targetVelocity = finalDir * speed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.2f);
    }

    private Vector2 GetSeparationVector()
    {
        Vector2 separation = Vector2.zero;
        int count = 0;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        foreach (var col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector2 diff = (Vector2)(transform.position - col.transform.position);
                float dist = diff.magnitude;
                if (dist > 0.01f)
                {
                    separation += diff.normalized / dist;
                    count++;
                }
            }
        }

        if (count > 0) separation /= count;

        return separation;
    }

    public void SetCanFollow(bool value)
    {
        canFollow = value;
        timeSinceActivated = 0f;
        if (canFollow) currentState = EnemyState.Patrolling;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) KillEnemy();
    }

    private void KillEnemy()
    {
        Debug.Log("El enemigo ha sido destruido.");

        if (potionPrefab != null && Random.value < dropChance)
        {
            Instantiate(potionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void SetShooting(bool value)
    {
        isShooting = value;
        isRelocating = false;
        rb.velocity = Vector2.zero;
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

            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            Collider2D enemyCollider = GetComponent<Collider2D>();
            if (bulletCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, enemyCollider);
            }

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = dir * 5f;
            }

            lastShotTime = Time.time;

            // 🔊 Reproducir sonido de disparo
            if (shootClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootClip);
            }

            relocationTarget = ClampToRoomBounds(GetNewPatrolTarget());
            isRelocating = true;
        }
    }

    private void RelocateAfterShot()
    {
        if (!roomBounds.Contains(relocationTarget))
        {
            isRelocating = false;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = (relocationTarget - rb.position).normalized;
        rb.velocity = direction * relocationSpeed;

        if (Vector2.Distance(rb.position, relocationTarget) < 0.1f)
        {
            rb.velocity = Vector2.zero;
            isRelocating = false;
            lastShotTime = Time.time;
        }
    }

    private Vector2 ClampToRoomBounds(Vector2 point)
    {
        float clampedX = Mathf.Clamp(point.x, roomBounds.min.x + 0.5f, roomBounds.max.x - 0.5f);
        float clampedY = Mathf.Clamp(point.y, roomBounds.min.y + 0.5f, roomBounds.max.y - 0.5f);
        return new Vector2(clampedX, clampedY);
    }
}
