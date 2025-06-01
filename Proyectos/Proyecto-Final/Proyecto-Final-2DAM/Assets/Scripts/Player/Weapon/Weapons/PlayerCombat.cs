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
        Vector2 moveInput = new Vector2(movementScript.joystick.Horizontal, movementScript.joystick.Vertical);

        // Flip del arma
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

    System.Collections.IEnumerator Attack()
    {
        isAttacking = true;

        if (broomCollider != null)
            broomCollider.enabled = true;

        Vector2 moveInput = new Vector2(movementScript.joystick.Horizontal, movementScript.joystick.Vertical);
        if (moveInput.sqrMagnitude < 0.1f)
        {
            moveInput = movementScript.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 start = originalLocalPosition;
        Vector3 end = start + (Vector3)(moveInput.normalized * attackDistance);

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            broom.localPosition = Vector3.Lerp(start, end, elapsed / attackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Hacer el raycast desde tipPoint hacia la dirección del movimiento
        Vector2 origin = tipPoint.position;
        Vector2 direction = moveInput.normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, attackDistance, enemyLayer);
        Debug.DrawRay(origin, direction * attackDistance, Color.red, 1f);

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

        // Volver la escoba
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
            broomCollider.enabled = false;

        weaponHolder.rotation = Quaternion.identity;
        isAttacking = false;
    }

    public void TriggerAttack()
    {
        if (!isAttacking)
        {
            Debug.Log("🟢 TriggerAttack ejecutado");
            StartCoroutine(Attack());
        }
    }
}
