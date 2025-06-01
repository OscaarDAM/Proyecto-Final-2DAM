using UnityEngine;

public class Enemy2D : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    
    [Header("Configuración de Ataque")]
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    
    [Header("Áreas de Detección")]
    public float chaseRange = 8f;
    public float attackRange = 4f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Referencias
    private Transform player;
    
    // Estados internos
    private float lastAttackTime = -999f; // Permitir ataque inmediato la primera vez
    private enum EnemyState { Idle, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Idle;
    
    void Start()
    {
        FindPlayer();
        
        // Si no se asignó el punto de spawn del proyectil, usar la posición del enemigo
        if (projectileSpawnPoint == null)
            projectileSpawnPoint = transform;
    }
    
    void FindPlayer()
    {
        if (player == null)
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Buscando jugador...");
            
            // Buscar por tag "Player" primero
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (showDebugInfo)
                    Debug.Log($"{gameObject.name}: ✓ Jugador encontrado por tag 'Player': {playerObj.name}");
                return;
            }
            
            if (showDebugInfo)
                Debug.LogWarning($"{gameObject.name}: No se encontró objeto con tag 'Player'");
            
            // Si no encuentra por tag, buscar por nombres comunes
            string[] commonPlayerNames = { "Player", "player", "Player(Clone)", "Jugador", "Character" };
            
            foreach (string name in commonPlayerNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    player = obj.transform;
                    if (showDebugInfo)
                        Debug.Log($"{gameObject.name}: ✓ Jugador encontrado por nombre '{name}'. Recomendable usar tag 'Player'.");
                    return;
                }
            }
            
            // Listar todos los objetos en la escena para debug
            if (showDebugInfo)
            {
                Debug.LogError($"{gameObject.name}: ¡NO SE PUDO ENCONTRAR AL JUGADOR!");
                Debug.Log("Objetos en la escena:");
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("player") || obj.name.ToLower().Contains("character"))
                    {
                        Debug.Log($"- Posible jugador encontrado: {obj.name} (Tag: {obj.tag})");
                    }
                }
            }
        }
    }
    
    void Update()
    {
        // Debug crítico cada segundo
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"=== {gameObject.name} DEBUG ===");
            Debug.Log($"Player encontrado: {(player != null ? "SÍ" : "NO")}");
            if (player != null)
            {
                Debug.Log($"Posición enemigo: {transform.position}");
                Debug.Log($"Posición jugador: {player.position}");
            }
        }
        
        // Asegurarse de que tenemos referencia al jugador
        if (player == null)
        {
            FindPlayer();
            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.LogError($"{gameObject.name}: ¡NO SE ENCUENTRA AL JUGADOR! Revisa el tag 'Player'");
            }
            return; // Salir si no encontramos jugador
        }
        
        // Calcular distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Debug de distancias
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Distancia: {distanceToPlayer:F1} | ChaseRange: {chaseRange} | AttackRange: {attackRange}");
            Debug.Log($"¿Debería perseguir? {(distanceToPlayer <= chaseRange ? "SÍ" : "NO")}");
            Debug.Log($"¿Debería atacar? {(distanceToPlayer <= attackRange ? "SÍ" : "NO")}");
        }
        
        // Actualizar estado basado en distancia
        UpdateState(distanceToPlayer);
        
        // Ejecutar comportamiento según el estado
        HandleBehavior(distanceToPlayer);
        
        // Debug info cada segundo aproximadamente
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"ESTADO ACTUAL: {currentState} | Último ataque: {(Time.time - lastAttackTime):F1}s atrás");
            Debug.Log($"========================");
        }
    }
    
    void UpdateState(float distanceToPlayer)
    {
        EnemyState previousState = currentState;
        
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
        
        // Debug cuando cambia de estado
        if (showDebugInfo && previousState != currentState)
        {
            Debug.Log($"{gameObject.name}: Estado cambió de {previousState} a {currentState} (Distancia: {distanceToPlayer:F1})");
        }
    }
    
    void HandleBehavior(float distanceToPlayer)
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // El enemigo permanece quieto
                break;
                
            case EnemyState.Chasing:
                ChasePlayer();
                break;
                
            case EnemyState.Attacking:
                AttackPlayer();
                break;
        }
    }
    
    void ChasePlayer()
    {
        if (player == null) return;
        
        // Calcular dirección hacia el jugador
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Mover hacia el jugador
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        
        // Rotar hacia el jugador
        RotateTowards(direction);
        
        if (showDebugInfo && Time.frameCount % 30 == 0) // Debug cada medio segundo
        {
            Debug.Log($"{gameObject.name}: Persiguiendo jugador - Dirección: {direction}");
        }
    }
    
    void AttackPlayer()
    {
        if (player == null) return;
        
        // Rotar hacia el jugador mientras ataca
        Vector2 direction = (player.position - transform.position).normalized;
        RotateTowards(direction);
        
        // Atacar si ha pasado el tiempo de cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            LaunchProjectile();
            lastAttackTime = Time.time;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: ¡Atacando! Próximo ataque en {attackCooldown} segundos. Tiempo actual: {Time.time:F1}");
        }
        else
        {
            // Debug para mostrar tiempo restante
            float timeRemaining = (lastAttackTime + attackCooldown) - Time.time;
            if (showDebugInfo && Time.frameCount % 60 == 0) // Cada segundo
            {
                Debug.Log($"{gameObject.name}: Esperando para atacar. Tiempo restante: {timeRemaining:F1}s");
            }
        }
    }
    
    void RotateTowards(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        
        // Calcular la diferencia de ángulo más corta
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        // Rotar gradualmente
        float rotationStep = rotationSpeed * Time.deltaTime;
        if (Mathf.Abs(angleDifference) < rotationStep)
        {
            transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        }
        else
        {
            float newAngle = currentAngle + Mathf.Sign(angleDifference) * rotationStep;
            transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.forward);
        }
    }
    
    void LaunchProjectile()
    {
        if (projectilePrefab == null)
        {
            if (showDebugInfo)
                Debug.LogWarning($"{gameObject.name}: No se asignó projectilePrefab");
            return;
        }
        
        if (player == null) return;
        
        // Calcular dirección hacia el jugador
        Vector2 direction = (player.position - projectileSpawnPoint.position).normalized;
        
        // Instanciar proyectil
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        
        // Intentar inicializar el proyectil de múltiples formas
        bool initialized = InitializeProjectile(projectile, direction);
        
        if (!initialized && showDebugInfo)
        {
            Debug.LogWarning($"{gameObject.name}: No se pudo inicializar el proyectil correctamente");
        }
    }
    
    bool InitializeProjectile(GameObject projectile, Vector2 direction)
    {
        // 1. Buscar componente SimpleProjectile
        SimpleProjectile simpleProj = projectile.GetComponent<SimpleProjectile>();
        if (simpleProj != null)
        {
            simpleProj.Initialize(direction);
            return true;
        }
        
        // 2. Buscar cualquier script con método Initialize
        MonoBehaviour[] scripts = projectile.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            var initMethod = script.GetType().GetMethod("Initialize", new System.Type[] { typeof(Vector2) });
            if (initMethod != null)
            {
                try
                {
                    initMethod.Invoke(script, new object[] { direction });
                    return true;
                }
                catch (System.Exception e)
                {
                    if (showDebugInfo)
                        Debug.LogWarning($"Error al inicializar proyectil: {e.Message}");
                }
            }
        }
        
        // 3. Usar Rigidbody2D como fallback
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * 8f; // velocidad por defecto
            rb.gravityScale = 0f; // Sin gravedad para proyectiles
            
            // Rotar el proyectil
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // Destruir después de 5 segundos
            Destroy(projectile, 5f);
            
            return true;
        }
        
        return false;
    }
    
    // Visualizar las áreas en el editor
    void OnDrawGizmosSelected()
    {
        // Área de persecución (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Área de ataque (rojo)
        Gizmos.color = Color.red;  
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Punto de spawn del proyectil
        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.2f);
        }
        
        // Línea hacia el jugador
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
    
    void OnDrawGizmos()
    {
        // Mostrar siempre las áreas si showDebugInfo está activo
        if (showDebugInfo)
        {
            Gizmos.color = new Color(0, 0, 1, 0.1f); // Azul transparente
            Gizmos.DrawSphere(transform.position, chaseRange);
            
            Gizmos.color = new Color(1, 0, 0, 0.1f); // Rojo transparente
            Gizmos.DrawSphere(transform.position, attackRange);
        }
    }
}

// Script mejorado para el proyectil
public class SimpleProjectile : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 8f;
    public float lifetime = 5f;
    public int damage = 1;
    
    private Vector2 direction;
    private Rigidbody2D rb;
    private bool initialized = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
            
        rb.gravityScale = 0f;
        
        // Asegurar que el proyectil tenga un collider
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.1f;
        }
        
        Destroy(gameObject, lifetime);
        
        // Si no se inicializó desde fuera, mover hacia adelante
        if (!initialized)
        {
            rb.velocity = transform.right * speed;
        }
    }
    
    public void Initialize(Vector2 dir)
    {
        initialized = true;
        direction = dir.normalized;
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Rotar el proyectil para que apunte en la dirección correcta
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Aplicar daño al jugador
            ApplyDamageToPlayer(other);
            DestroyProjectile();
        }
        else if (ShouldDestroyOnContact(other))
        {
            DestroyProjectile();
        }
    }
    
    void ApplyDamageToPlayer(Collider2D playerCollider)
    {
        MonoBehaviour[] playerComponents = playerCollider.GetComponents<MonoBehaviour>();
        bool damageApplied = false;
        
        foreach (var component in playerComponents)
        {
            var methods = component.GetType().GetMethods();
            foreach (var method in methods)
            {
                string methodName = method.Name.ToLower();
                if ((methodName.Contains("damage") || methodName.Contains("hit") || methodName.Contains("hurt")) 
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(int))
                {
                    try
                    {
                        method.Invoke(component, new object[] { damage });
                        damageApplied = true;
                        Debug.Log($"Proyectil aplicó {damage} de daño usando {method.Name}");
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            if (damageApplied) break;
        }
        
        if (!damageApplied)
        {
            Debug.Log($"Proyectil golpeó al jugador pero no se encontró método de daño. Daño previsto: {damage}");
        }
    }
    
    bool ShouldDestroyOnContact(Collider2D other)
    {
        return other.CompareTag("Wall") || 
               other.CompareTag("Ground") || 
               other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
               other.gameObject.layer == LayerMask.NameToLayer("Wall") ||
               other.gameObject.layer == LayerMask.NameToLayer("Default");
    }
    
    void DestroyProjectile()
    {
        // Aquí podrías agregar efectos de partículas o sonido
        Destroy(gameObject);
    }
}