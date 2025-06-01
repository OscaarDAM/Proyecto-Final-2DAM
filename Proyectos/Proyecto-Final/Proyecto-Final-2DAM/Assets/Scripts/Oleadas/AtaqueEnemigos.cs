using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AutoEnemyController : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN BÁSICA ===")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRange = 3f;
    
    [Header("=== PATRULLAJE ===")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float patrolWaitTime = 2f;
    
    [Header("=== COMBATE ===")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private bool canShoot = true;
    
    [Header("=== DROPS ===")]
    [SerializeField] private GameObject[] dropItems;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.3f;
    
    [Header("=== CONFIGURACIÓN AUTOMÁTICA ===")]
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private bool autoCreateFirePoint = true;
    [SerializeField] private bool useRoomBounds = true;
    
    // Referencias privadas
    private Transform player;
    private Rigidbody2D rb;
    private Transform firePoint;
    private Bounds roomBounds;
    
    // Variables de estado
    private EnemyState currentState = EnemyState.Patrol;
    private Vector2 patrolTarget;
    private Vector2 originalPosition;
    private float lastShootTime;
    private float patrolTimer;
    private bool isInitialized = false;
    
    // Enums
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    private void Awake()
    {
        AutoSetup();
    }

    private void Start()
    {
        Initialize();
    }

    private void AutoSetup()
    {
        // Auto-obtener Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configurar Rigidbody2D automáticamente
        rb.gravityScale = 0f;
        rb.drag = 5f;
        rb.angularDrag = 5f;
        rb.freezeRotation = true;
        
        // Auto-configurar tag si no está puesto
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Enemy";
        }
        
        // Auto-crear FirePoint si no existe
        if (autoCreateFirePoint && firePoint == null)
        {
            CreateFirePoint();
        }
        
        // Buscar bounds de la habitación automáticamente
        if (useRoomBounds)
        {
            FindRoomBounds();
        }
    }

    private void CreateFirePoint()
    {
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(transform);
        firePointObj.transform.localPosition = Vector3.right * 0.5f; // Posición relativa al enemigo
        firePoint = firePointObj.transform;
    }

    private void FindRoomBounds()
    {
        // Buscar RoomTrigger o similar
        RoomTrigger roomTrigger = FindObjectOfType<RoomTrigger>();
        if (roomTrigger != null)
        {
            // Intentar obtener bounds del collider del RoomTrigger
            Collider2D roomCollider = roomTrigger.GetComponent<Collider2D>();
            if (roomCollider != null)
            {
                roomBounds = roomCollider.bounds;
            }
            else
            {
                // Si no tiene collider, usar bounds por defecto
                roomBounds = new Bounds(roomTrigger.transform.position, Vector3.one * 15f);
            }
        }
        else
        {
            // Crear bounds por defecto basados en la cámara
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                float height = mainCam.orthographicSize * 2f;
                float width = height * mainCam.aspect;
                roomBounds = new Bounds(transform.position, new Vector3(width, height, 0));
            }
            else
            {
                // Bounds por defecto
                roomBounds = new Bounds(transform.position, Vector3.one * 20f);
            }
        }
    }

    private void Initialize()
    {
        originalPosition = transform.position;
        
        // Auto-encontrar jugador
        if (autoFindPlayer && player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Configurar patrullaje inicial
        SetNewPatrolTarget();
        
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || currentState == EnemyState.Dead) return;
        
        // Debug: Matar todos los enemigos con M
        if (Input.GetKeyDown(KeyCode.M))
        {
            KillAllEnemies();
        }
        
        UpdateAI();
    }

    private void UpdateAI()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Determinar estado basado en distancia
        if (distanceToPlayer <= attackRange && canShoot)
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
        
        // Ejecutar comportamiento según estado
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }
    }

    private void Patrol()
    {
        patrolTimer += Time.deltaTime;
        
        // Si llegó al objetivo o pasó mucho tiempo, buscar nuevo objetivo
        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f || patrolTimer > patrolWaitTime)
        {
            SetNewPatrolTarget();
            patrolTimer = 0f;
        }
        
        // Moverse hacia el objetivo de patrulla
        MoveTowards(patrolTarget, patrolSpeed);
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 targetPosition = originalPosition + randomDirection * patrolRadius;
        
        // Asegurar que esté dentro de los límites
        if (roomBounds.Contains(targetPosition))
        {
            patrolTarget = targetPosition;
        }
        else
        {
            patrolTarget = originalPosition;
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;
        
        Vector2 targetPosition = player.position;
        
        // Solo perseguir si el jugador está dentro de los límites
        if (roomBounds.Contains(targetPosition))
        {
            MoveTowards(targetPosition, speed);
        }
    }

    private void AttackPlayer()
    {
        if (!canShoot || projectilePrefab == null || firePoint == null) return;
        
        // Detenerse para atacar
        rb.velocity = Vector2.zero;
        
        // Mirar hacia el jugador
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Disparar si ha pasado el cooldown
        if (Time.time - lastShootTime >= shootCooldown)
        {
            Shoot(direction);
            lastShootTime = Time.time;
        }
    }

    private void Shoot(Vector2 direction)
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Configurar bala automáticamente
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = direction * projectileSpeed;
        }
        
        // Ignorar colisión con el enemigo
        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (bulletCollider != null && enemyCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, enemyCollider);
        }
        
        // Auto-destruir bala después de 5 segundos
        Destroy(bullet, 5f);
    }

    private void MoveTowards(Vector2 target, float moveSpeed)
    {
        Vector2 direction = (target - rb.position).normalized;
        Vector2 targetVelocity = direction * moveSpeed;
        
        // Aplicar separación con otros enemigos
        Vector2 separation = GetSeparationForce();
        targetVelocity += separation;
        
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 5f);
    }

    private Vector2 GetSeparationForce()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        
        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject && enemy.CompareTag("Enemy"))
            {
                Vector2 diff = transform.position - enemy.transform.position;
                float distance = diff.magnitude;
                
                if (distance > 0 && distance < 1.5f)
                {
                    separation += diff.normalized / distance;
                }
            }
        }
        
        return separation * 0.5f;
    }

    public void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;
        
        health -= damage;
        
        // Efecto visual de daño (opcional)
        StartCoroutine(DamageFlash());
        
        if (health <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        rb.velocity = Vector2.zero;
        
        // Soltar items
        DropItems();
        
        // Destruir después de un breve delay
        Destroy(gameObject, 0.1f);
    }

    private void DropItems()
    {
        if (dropItems.Length > 0 && Random.value < dropChance)
        {
            GameObject itemToDrop = dropItems[Random.Range(0, dropItems.Length)];
            if (itemToDrop != null)
            {
                Instantiate(itemToDrop, transform.position, Quaternion.identity);
            }
        }
    }

    private void KillAllEnemies()
    {
        AutoEnemyController[] allEnemies = FindObjectsOfType<AutoEnemyController>();
        foreach (var enemy in allEnemies)
        {
            enemy.TakeDamage(9999f);
        }
    }

    // Métodos públicos para configuración externa
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    public void SetRoomBounds(Bounds bounds)
    {
        roomBounds = bounds;
        useRoomBounds = true;
    }

    public void SetFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
        autoCreateFirePoint = false;
    }

    // Gizmos para debug
    private void OnDrawGizmosSelected()
    {
        // Radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Radio de patrulla
        Gizmos.color = Color.blue;
        Vector3 patrolCenter = Application.isPlaying ? originalPosition : transform.position;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
        
        // Bounds de la habitación
        if (useRoomBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(roomBounds.center, roomBounds.size);
        }
    }
}