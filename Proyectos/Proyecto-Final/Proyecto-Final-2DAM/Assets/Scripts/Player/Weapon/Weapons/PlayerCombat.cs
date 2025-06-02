using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform weaponHolder;
    public Transform broom;
    public Transform tipPoint;
    public SpriteRenderer playerSprite;
    public float aimRotationSpeed = 15f;
    public float attackDistance = 0.5f;
    public float attackDuration = 0.2f;
    public int damage = 1000;
    public LayerMask enemyLayer;

    private bool isAttacking = false;
    private Vector3 originalLocalPosition;
    private PlayerJoystickMove movementScript;
    private Collider2D broomCollider;

    // ✅ Última dirección válida del joystick (por defecto hacia la derecha)
    private Vector2 lastAimDirection = Vector2.right;

    void Start()
    {
        originalLocalPosition = broom.localPosition;
        movementScript = GetComponent<PlayerJoystickMove>();
        broomCollider = broom.GetComponent<Collider2D>();

        if (broomCollider != null)
            broomCollider.enabled = false;
    }

    void Update()
    {
        // Entrada del joystick
        Vector2 moveInput = new Vector2(movementScript.joystick.Horizontal, movementScript.joystick.Vertical);

        // ✅ Si hay entrada significativa, actualizamos la última dirección
        if (moveInput.sqrMagnitude > 0.1f)
        {
            lastAimDirection = moveInput.normalized;
        }

        // Ajuste de posición del arma según el flip del sprite
        Vector3 weaponPos = weaponHolder.localPosition;
        weaponPos.x = Mathf.Abs(weaponPos.x) * (movementScript.spriteRenderer.flipX ? -1 : 1);
        weaponHolder.localPosition = weaponPos;

        // Escalado del arma
        Vector3 weaponScale = weaponHolder.localScale;
        weaponScale.y = 1;
        weaponScale.x = movementScript.spriteRenderer.flipX ? -1 : 1;
        weaponHolder.localScale = weaponScale;

        // Si no se está atacando, reseteamos rotación y posición del arma
        if (!isAttacking)
        {
            broom.localPosition = originalLocalPosition;
            weaponHolder.rotation = Quaternion.identity;
        }
    }

    // Llamar a este método para iniciar el ataque
    public void TriggerAttack()
    {
        if (!isAttacking)
        {
            Debug.Log("🟢 TriggerAttack ejecutado");
            StartCoroutine(Attack());
        }
    }

    // Corrutina que maneja el ataque
    System.Collections.IEnumerator Attack()
    {
        isAttacking = true;

        if (broomCollider != null)
            broomCollider.enabled = true;

        // ✅ Leer la entrada de nuevo para actualizar lastAimDirection si hay movimiento
        Vector2 moveInput = new Vector2(movementScript.joystick.Horizontal, movementScript.joystick.Vertical);
        if (moveInput.sqrMagnitude > 0.1f)
        {
            lastAimDirection = moveInput.normalized;
        }

        // ✅ Usar la última dirección guardada para el ataque
        Vector2 attackDirection = lastAimDirection;

        // Rotar el arma en la dirección del ataque
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle);

        // Movimiento hacia adelante del arma (animación de golpe)
        Vector3 start = originalLocalPosition;
        Vector3 end = start + (Vector3)(attackDirection * attackDistance);

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            broom.localPosition = Vector3.Lerp(start, end, elapsed / attackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Hacer raycast desde la punta del arma en la dirección del ataque
        Vector2 origin = tipPoint.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, attackDirection, attackDistance, enemyLayer);
        Debug.DrawRay(origin, attackDirection * attackDistance, Color.red, 1f);

        if (hit.collider != null)
        {
            Debug.Log($"✅ Raycast hit: {hit.collider.name}, tag: {hit.collider.tag}");

            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("✅ Enemigo detectado por tag.");
                var enemy = hit.collider.GetComponent<EnemyFollow>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("⚠️ El objeto tiene el tag Enemy pero no tiene el script EnemyFollow.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Raycast impactó algo, pero no tiene tag Enemy.");
            }
        }
        else
        {
            Debug.Log("❌ Raycast no impactó ningún objeto.");
        }

        // Volver el arma a su posición original
        float returnDuration = 0.1f;
        elapsed = 0f;
        Vector3 current = broom.localPosition;
        while (elapsed < returnDuration)
        {
            broom.localPosition = Vector3.Lerp(current, originalLocalPosition, elapsed / returnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset final
        broom.localPosition = originalLocalPosition;

        if (broomCollider != null)
            broomCollider.enabled = false;

        weaponHolder.rotation = Quaternion.identity;
        isAttacking = false;
    }
}
