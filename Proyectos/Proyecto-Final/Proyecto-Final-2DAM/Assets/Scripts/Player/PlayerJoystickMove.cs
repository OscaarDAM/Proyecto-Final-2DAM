using UnityEngine;
using System.Collections;

public class PlayerJoystickMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    public Joystick joystick;
    public int maxHealth = 4;
    public int health = 4;
    private bool isDead = false;

    public GameOverManager gameOverManager;

    private Animator animator;
    public SpriteRenderer spriteRenderer;

    // Fuerza del rebote al chocar con enemigo
    public float bounceForce = 8f;

    // VARIABLES PARA EL REBOTE
    private bool isBouncing = false;
    private float bounceDuration = 0.3f; // Duración del rebote en segundos
    private float bounceTimer = 0f;
    private Vector2 bounceDirection;

    // 🔴 VARIABLES PARA CAMBIO DE COLOR AL RECIBIR DAÑO
    private Color originalColor;
    private Coroutine flashCoroutine;

    public CameraShake cameraShake;

    // 🟡 VARIABLES PARA INMORTALIDAD
    private bool isImmortal = false;
    private float immortalTimer = 0f;
    private Coroutine immortalCoroutine;
    private Coroutine blinkCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.flipX = false; // Ahora sí puedes usarlo

        health = maxHealth;

        // Guardamos el color original
        originalColor = spriteRenderer.color;

        if (joystick == null)
        {
            joystick = FindObjectOfType<FixedJoystick>();
        }

        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (joystick != null)
        {
            movement = new Vector2(joystick.Horizontal, joystick.Vertical).normalized;

            bool isWalking = movement.magnitude > 0.1f;
            animator.SetBool("isWalking", isWalking);

            // Giro del sprite horizontalmente
            if (Mathf.Abs(movement.x) > 0.01f)
            {
                spriteRenderer.flipX = movement.x < 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.K)) health = 0;
        if (Input.GetKeyDown(KeyCode.J)) health--;

        // 🟡 Timer de inmortalidad
        if (isImmortal)
        {
            immortalTimer -= Time.deltaTime;
            if (immortalTimer <= 0f)
            {
                isImmortal = false;

                if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
                spriteRenderer.enabled = true;
                spriteRenderer.color = originalColor;
            }
        }

        if (health <= 0 && !isDead)
        {
            isDead = true;
            gameOverManager.ShowGameOver();
        }
    }

    void FixedUpdate()
    {
        if (isBouncing)
        {
            bounceTimer += Time.fixedDeltaTime;

            // Aplicar la velocidad de rebote
            rb.velocity = bounceDirection * bounceForce;

            if (bounceTimer >= bounceDuration)
            {
                isBouncing = false;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // Movimiento normal del jugador
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // Detecta colisión con enemigo para rebotar
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignorar colisiones con la escoba (arma)
        if (collision.gameObject.CompareTag("Weapon"))
        {
            return; // No hacer nada
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 🟡 No recibir daño si es inmortal
            if (isImmortal) return;

            // Dirección del rebote (desde el enemigo hacia el jugador)
            bounceDirection = (rb.position - (Vector2)collision.transform.position).normalized;
            isBouncing = true;
            bounceTimer = 0f;

            // Aplica daño al jugador
            health--;

            if (cameraShake != null)
            {
                StartCoroutine(cameraShake.Shake(0.15f, 0.1f)); // 💥 Shake al recibir daño
            }

            // 🔴 Efecto visual al recibir daño
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRed());

            if (health <= 0 && !isDead)
            {
                isDead = true;
                gameOverManager.ShowGameOver();
            }
        }
    }

    public void Heal(float amount)
    {
        health = (int)Mathf.Min(health + amount, maxHealth); // No permite superar la vida máxima
    }

    // 🔴 Corrutina para poner al jugador rojo brevemente
    private IEnumerator FlashRed()
    {
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f); // Color rojizo

        yield return new WaitForSeconds(0.2f); // Duración del efecto

        // Restaurar color solo si no está inmortal
        if (!isImmortal)
            spriteRenderer.color = originalColor;
    }

    // 🟡 Método público para hacer al jugador inmortal por un tiempo
    public void BecomeImmortal(float duration)
    {
        isImmortal = true;
        immortalTimer = duration;

        // Cambiar a color amarillo brillante mientras es inmortal
        spriteRenderer.color = new Color(1f, 1f, 0.3f);

        // Reiniciar efecto si ya estaba activo
        if (immortalCoroutine != null)
            StopCoroutine(immortalCoroutine);
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        immortalCoroutine = StartCoroutine(ImmortalityEffect(duration));
        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    // 🟡 Corrutina para terminar el estado inmortal después del tiempo
    private IEnumerator ImmortalityEffect(float duration)
    {
        yield return new WaitForSeconds(duration);

        isImmortal = false;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        spriteRenderer.enabled = true;
        spriteRenderer.color = originalColor;
    }

    // 🟡 Efecto de parpadeo mientras es inmortal
    private IEnumerator BlinkEffect()
    {
        while (isImmortal)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 🟡 Puedes usar esto si necesitas consultar desde otro script
    public bool IsImmortal()
    {
        return isImmortal;
    }
}
