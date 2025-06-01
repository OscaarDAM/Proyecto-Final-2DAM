using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform weaponHolder;
    public Transform broom;
    public SpriteRenderer playerSprite;
    public float aimRotationSpeed = 15f;
    public float attackDistance = 0.5f;
    public float attackDuration = 0.2f;
    public LayerMask enemyLayer;
    public int damage = 1;

    private bool isAttacking = false;
    private Vector3 originalLocalPosition;
    private PlayerJoystickMove movementScript;

    void Start()
    {
        originalLocalPosition = broom.localPosition;
        movementScript = GetComponent<PlayerJoystickMove>();
    }

    void Update()
    {
        Vector2 moveInput = new Vector2(movementScript.joystick.Horizontal, movementScript.joystick.Vertical);

        // Flip visual del arma según el sprite del personaje
        Vector3 weaponLocalPos = weaponHolder.localPosition;
        weaponLocalPos.x = Mathf.Abs(weaponLocalPos.x) * (movementScript.spriteRenderer.flipX ? -1 : 1);
        weaponHolder.localPosition = weaponLocalPos;

        // Rotar el arma hacia la dirección del joystick
        if (moveInput.sqrMagnitude > 0.1f && !isAttacking)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            weaponHolder.rotation = Quaternion.Lerp(weaponHolder.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * aimRotationSpeed);
        }

        // No hacemos nada aquí respecto al botón (se controla con TriggerAttack)
    }

    public void TriggerAttack()
    {
        if (!isAttacking)
            StartCoroutine(Attack());
    }

    System.Collections.IEnumerator Attack()
    {
        isAttacking = true;

        Vector3 start = broom.localPosition;
        Vector3 end = start + weaponHolder.right.normalized * attackDistance;

        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            broom.localPosition = Vector3.Lerp(start, end, elapsed / attackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Detectar colisión con enemigos
        RaycastHit2D hit = Physics2D.Raycast(broom.position, weaponHolder.right, 0.5f, enemyLayer);
        if (hit.collider != null)
        {
            var enemy = hit.collider.GetComponent<EnemyFollow>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        broom.localPosition = start;
        isAttacking = false;
    }
}
