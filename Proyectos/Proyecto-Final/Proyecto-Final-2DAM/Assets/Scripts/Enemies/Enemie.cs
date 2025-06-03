using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    // Configuración general del enemigo
    public float speed = 0.5f;
    public float health = 100f;

    private Transform player;
    private Rigidbody2D rb;

    private bool canFollow = false; // Se activa al entrar a la sala
    private float timeSinceActivated = 0f;
    private float chaseDelay; // Tiempo aleatorio antes de perseguir

    public Bounds roomBounds; // Límites de la sala donde patrulla

    private enum EnemyState { Idle, Patrolling, Chasing }
    private EnemyState currentState = EnemyState.Patrolling;

    // Variables para patrullaje
    private Vector2 patrolTarget;
    private float patrolRadius;
    private float patrolCooldown;
    private float patrolTimer = 0f;

    // Disparo
    public GameObject projectilePrefab;
    public Transform firePoint;

    private bool isShooting = false; // Si el enemigo está disparando
    private float shootCooldown = 2f; // Tiempo entre disparos
    private float lastShotTime = 0f;

    private float exitShootAreaTimer = 0f;
    private bool waitingAfterExit = false; // Espera después de salir del área de disparo

    // Pociones al morir
    public GameObject potionPrefab;
    [Range(0f, 1f)] public float dropChance = 0.3f;

    // Relocalización después del disparo
    private bool isRelocating = false;
    private Vector2 relocationTarget;
    private float relocationSpeed = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolRadius = Random.Range(2f, 4f);
        patrolCooldown = Random.Range(1.5f, 3f);
        chaseDelay = Random.Range(0.3f, 1.2f); // Tiempo aleatorio antes de perseguir
        patrolTarget = GetNewPatrolTarget();
    }

    private void Update()
    {
        // Tecla para matar todos los enemigos (debug)
        if (Input.GetKeyDown(KeyCode.M))
        {
            MatarTodosLosEnemigos();
        }
    }

    // Método para destruir todos los enemigos del juego
    private void MatarTodosLosEnemigos()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemigo in enemigos)
        {
            enemigo.GetComponent<EnemyFollow>().TakeDamage(1000f);
        }
    }

    private void FixedUpdate()
    {
        // Encontrar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Si no está activado, sigue patrullando
        if (!canFollow)
        {
            Patrol();
            return;
        }

        // Esperar cierto tiempo antes de empezar a perseguir
        timeSinceActivated += Time.fixedDeltaTime;
        if (timeSinceActivated < chaseDelay)
        {
            Patrol();
        }
        else
        {
            currentState = EnemyState.Chasing;
        }

        // Si está disparando, no patrulla ni persigue
        if (isShooting)
        {
            if (isRelocating)
            {
                RelocateAfterShot(); // Se mueve tras disparar
            }
            else
            {
                ShootAtPlayer(); // Dispara al jugador
            }
            return;
        }

        // Si acaba de salir del área de disparo, espera 2 segundos antes de volver a perseguir
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

        // Ejecutar comportamiento según el estado actual
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

    // Comportamiento de patrullaje aleatorio dentro de la sala
    private void Patrol()
    {
        patrolTimer += Time.fixedDeltaTime;

        // Cambiar de objetivo si llegó o pasó mucho tiempo
        if (Vector2.Distance(rb.position, patrolTarget) < 0.1f || patrolTimer > patrolCooldown)
        {
            patrolTarget = GetNewPatrolTarget();
            patrolTimer = 0f;
        }

        // Movimiento suave hacia el objetivo
        Vector2 direction = (patrolTarget - rb.position).normalized;
        Vector2 targetVelocity = direction * speed * 0.5f;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.1f);
    }

    // Genera un nuevo punto aleatorio dentro del área para patrullar
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

    // Persecución del jugador
    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 target = player.position;

        if (!roomBounds.Contains(target)) return;

        // Dirección hacia el jugador
        Vector2 direction = (target - rb.position).normalized;

        // Vector de separación para no chocar con otros enemigos
        Vector2 separation = GetSeparationVector() * 0.5f;

        // Movimiento hacia el jugador ajustado con separación
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

    // Evita que se amontonen los enemigos entre sí
    private Vector2 GetSeparationVector()
    {
        Vector2 separation = Vector2.zero;
        int count = 0;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.6f); // menor radio

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

        if (count > 0)
        {
            separation /= count;
        }

        return separation;
    }

    // Activar persecución
    public void SetCanFollow(bool value)
    {
        canFollow = value;
        timeSinceActivated = 0f;

        if (canFollow)
        {
            currentState = EnemyState.Patrolling;
        }
    }

    // Recibir daño
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            KillEnemy();
        }
    }

    // Muerte del enemigo y posible soltado de poción
    private void KillEnemy()
    {
        Debug.Log("El enemigo ha sido destruido.");

        if (potionPrefab != null && Random.value < dropChance)
        {
            Instantiate(potionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Llamado por RoomTrigger al entrar
    public void SetShooting(bool value)
    {
        isShooting = value;
        isRelocating = false;
        rb.velocity = Vector2.zero;
    }

    // Llamado por RoomTrigger al salir del área de disparo
    public void OnExitShootingArea()
    {
        isShooting = false;
        waitingAfterExit = true;
        exitShootAreaTimer = 0f;
    }

    // Lógica de disparo hacia el jugador
    private void ShootAtPlayer()
    {
        if (player == null || projectilePrefab == null || firePoint == null) return;

        rb.velocity = Vector2.zero;

        if (Time.time - lastShotTime >= shootCooldown)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Ignorar colisión con el propio enemigo
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            Collider2D enemyCollider = GetComponent<Collider2D>();
            if (bulletCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, enemyCollider);
            }

            // Disparo con velocidad
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = dir * 5f;
            }

            lastShotTime = Time.time;

            // Escoge un nuevo punto al que moverse tras disparar
            relocationTarget = ClampToRoomBounds(GetNewPatrolTarget());
            isRelocating = true;
        }
    }

    // Movimiento a un punto aleatorio tras disparar
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
            lastShotTime = Time.time; // Reinicia cooldown de disparo
        }
    }

    // Asegura que un punto esté dentro de los límites de la sala
    private Vector2 ClampToRoomBounds(Vector2 point)
    {
        float clampedX = Mathf.Clamp(point.x, roomBounds.min.x + 0.5f, roomBounds.max.x - 0.5f);
        float clampedY = Mathf.Clamp(point.y, roomBounds.min.y + 0.5f, roomBounds.max.y - 0.5f);
        return new Vector2(clampedX, clampedY);
    }
}
