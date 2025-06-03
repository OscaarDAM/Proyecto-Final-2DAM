using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform weaponHolder;
    public Transform broom;
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

    private Vector2 lastAimDirection = Vector2.right;

    void Start()
    {
        originalLocalPosition = broom.localPosition;
        movementScript = GetComponent<PlayerJoystickMove>();
        broomCollider = broom.GetComponent<Collider2D>();

        if (broomCollider != null)
        {
            broomCollider.isTrigger = true;  // Muy importante
            broomCollider.enabled = false;   // Collider desactivado fuera del ataque
        }
    }

    void Update()
    {
        Vector2 moveInput = new Vector2(movementScript.joystick.Horizontal, movementScript.joystick.Vertical);

        if (moveInput.sqrMagnitude > 0.1f)
        {
            lastAimDirection = moveInput.normalized;
        }

        // Ajuste posición y escala del arma (igual que antes)
        Vector3 weaponPos = weaponHolder.localPosition;
        weaponPos.x = Mathf.Abs(weaponPos.x) * (movementScript.spriteRenderer.flipX ? -1 : 1);
        weaponHolder.localPosition = weaponPos;

        Vector3 weaponScale = weaponHolder.localScale;
        weaponScale.y = 1;
        weaponScale.x = movementScript.spriteRenderer.flipX ? -1 : 1;
        weaponHolder.localScale = weaponScale;

        if (!isAttacking)
        {
            broom.localPosition = originalLocalPosition;
            weaponHolder.rotation = Quaternion.identity;
        }
    }

    public void TriggerAttack()
    {
        if (!isAttacking)
        {
            Debug.Log("🟢 TriggerAttack ejecutado");
            StartCoroutine(Attack());
        }
    }

    System.Collections.IEnumerator Attack()
    {
        isAttacking = true;

        if (broomCollider != null)
            broomCollider.enabled = true;  // Activamos collider trigger

        Vector2 attackDirection = lastAimDirection;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 start = originalLocalPosition;
        Vector3 end = start + (Vector3)(attackDirection * attackDistance);

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            broom.localPosition = Vector3.Lerp(start, end, elapsed / attackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Volver el arma a posición original
        float returnDuration = 0.1f;
        elapsed = 0f;
        Vector3 current = broom.localPosition;
        while (elapsed < returnDuration)
        {
            broom.localPosition = Vector3.Lerp(current, originalLocalPosition, elapsed / returnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        broom.localPosition = originalLocalPosition;

        if (broomCollider != null)
            broomCollider.enabled = false;  // Desactivamos collider fuera del ataque

        weaponHolder.rotation = Quaternion.identity;
        isAttacking = false;
    }

    // Aquí detectamos enemigos al entrar en el trigger del arma
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking) return;  // Solo dañamos si estamos atacando

        // Comprobamos si está en la capa de enemigos
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"✅ Arma trigger hit: {other.name}");
                var enemy = other.GetComponent<EnemyFollow>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
