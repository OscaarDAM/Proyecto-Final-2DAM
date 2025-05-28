using UnityEngine;

public class PlayerJoystickMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    public Joystick joystick; // solo este joystick
    public WeaponController weapon;

    public int maxHealth = 4;
    public int health = 4;
    private bool isDead = false;

    public GameOverManager gameOverManager;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 lastDirection = Vector2.right;

    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (joystick == null)
            joystick = FindObjectOfType<FixedJoystick>();

        if (gameOverManager == null)
            gameOverManager = FindObjectOfType<GameOverManager>();

        Time.timeScale = 1f;
    }

    void Update()
    {
        movement = new Vector2(joystick.Horizontal, joystick.Vertical).normalized;

        if (movement.magnitude > 0.1f)
        {
            lastDirection = movement;
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        if (Mathf.Abs(movement.x) > 0.01f)
            spriteRenderer.flipX = movement.x < 0;

        if (weapon != null)
            weapon.Aim(lastDirection);

        if (Input.GetKeyDown(KeyCode.K)) health = 0;
        if (Input.GetKeyDown(KeyCode.J)) health--;

        if (health <= 0 && !isDead)
        {
            isDead = true;
            gameOverManager.ShowGameOver();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void Heal(float amount)
    {
        health = (int)Mathf.Min(health + amount, maxHealth);
    }

    // Llamado por el botón de disparo
    public void OnShootButtonPressed()
    {
        if (weapon != null)
            weapon.TryShoot();
    }
}
