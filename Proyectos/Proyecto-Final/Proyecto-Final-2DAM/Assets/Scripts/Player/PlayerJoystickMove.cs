using UnityEngine;

public class PlayerJoystickMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    public Joystick joystick;
    public int health = 3;
    private bool isDead = false;

    public GameOverManager gameOverManager;

    private Animator animator;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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

            // Actualizar dirección si hay movimiento horizontal
            if (Mathf.Abs(movement.x) > 0.01f)
            {
                isFacingRight = movement.x > 0;
            }

            // Actualizar animaciones
            bool isWalking = movement.magnitude > 0.1f;

            // Esto activa los parámetros que controlan las 4 animaciones
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isFacingRight", isFacingRight);
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
}
