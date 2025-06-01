using UnityEngine;

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
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.flipX = false; // Ahora sí puedes usarlo

        health = maxHealth;

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
        health = (int)Mathf.Min(health + amount, maxHealth); // No permite superar la vida máxima
    }

    
}   