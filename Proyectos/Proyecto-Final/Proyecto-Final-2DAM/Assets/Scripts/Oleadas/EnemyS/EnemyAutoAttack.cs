using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAutoAttack : MonoBehaviour, IDamageable
{
    [Header("Movimiento y salud")]
    public float speed = 0.5f;
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Ataque")]
    public GameObject projectilePrefab;      // Prefab del proyectil
    public Transform firePoint;              // Punto de disparo
    private float shootCooldown = 2f;        // Tiempo entre disparos
    private float lastShotTime = 0f;

    [Header("Movimiento luego de disparar")]
    public Bounds roomBounds;                // Límites de la sala
    private Vector2 relocationTarget;        // Objetivo temporal tras disparar
    private float relocationSpeed = 1.5f;
    private float separationDistance = 1.2f; // Distancia mínima de separación entre enemigos

    [Header("Drop al morir")]
    public GameObject[] potionPrefabs;       // Prefabs de pociones a soltar
    [Range(0f, 1f)]
    public float dropChance = 0.3f;          // Probabilidad de soltar poción

    private Transform player;                // Referencia al jugador
    private Rigidbody2D rb;

    // Estados del enemigo
    private enum EnemyState { Idle, Shooting, Moving }
    private EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        // Encuentra al jugador si aún no lo tiene
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player == null) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (Time.time - lastShotTime >= shootCooldown)
                    currentState = EnemyState.Shooting;
                break;

            case EnemyState.Shooting:
                ShootAtPlayer();                       // Dispara al jugador
                relocationTarget = GetSafeRelocationTarget();  // Decide nueva posición
                currentState = EnemyState.Moving;      // Cambia a estado de movimiento
                break;

            case EnemyState.Moving:
                MoveToTarget();                        // Se mueve a nueva posición
                break;
        }
    }

    private void ShootAtPlayer()
    {
        if (player == null || firePoint == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.velocity = dir * 5f; // Velocidad del proyectil

        // Ignora la colisión con el enemigo que lo dispara
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        Collider2D enemyCol = GetComponent<Collider2D>();
        if (bulletCol != null && enemyCol != null)
            Physics2D.IgnoreCollision(bulletCol, enemyCol);

        lastShotTime = Time.time;
    }

    private void MoveToTarget()
    {
        Vector2 dir = (relocationTarget - rb.position).normalized;
        rb.velocity = dir * relocationSpeed;

        if (Vector2.Distance(rb.position, relocationTarget) < 0.05f)
        {
            rb.velocity = Vector2.zero;
            currentState = EnemyState.Idle;
        }
    }

    // Genera un nuevo punto donde moverse evitando estar cerca de otros enemigos
    private Vector2 GetSafeRelocationTarget()
    {
        Vector2 candidate = rb.position;
        int maxAttempts = 20;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(0.4f, 1f); // Movimiento corto
            Vector2 offset = randomDir * randomDist;
            candidate = ClampToRoomBounds(rb.position + offset);

            if (IsFarFromOtherEnemies(candidate))
                break;

            attempts++;
        }

        return candidate;
    }

    // Verifica que la nueva posición no esté muy cerca de otros enemigos
    private bool IsFarFromOtherEnemies(Vector2 position)
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(position, separationDistance);
        foreach (var col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
                return false;
        }
        return true;
    }

    // Mantiene al enemigo dentro de los límites de la sala
    private Vector2 ClampToRoomBounds(Vector2 point)
    {
        float clampedX = Mathf.Clamp(point.x, roomBounds.min.x + 0.5f, roomBounds.max.x - 0.5f);
        float clampedY = Mathf.Clamp(point.y, roomBounds.min.y + 0.5f, roomBounds.max.y - 0.5f);
        return new Vector2(clampedX, clampedY);
    }

    // Recibe daño
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            KillEnemy();
    }

    // Muerte del enemigo y posible drop
    private void KillEnemy()
    {
        if (potionPrefabs != null && potionPrefabs.Length > 0 && Random.value < dropChance)
        {
            int index = Random.Range(0, potionPrefabs.Length);
            Instantiate(potionPrefabs[index], transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void SetHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
    }
}
