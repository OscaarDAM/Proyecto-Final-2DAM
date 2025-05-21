using UnityEngine;

public class PlayerJoystickMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    public Joystick joystick; // Se asignará por código

    public int health = 3;
    private bool isDead = false;

    public GameOverManager gameOverManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Asignar joystick automáticamente si no está asignado en el Inspector
        if (joystick == null)
        {
            joystick = FindObjectOfType<FixedJoystick>();
        }

        // También buscar GameOverManager si no se asignó
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
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            health = 0;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            health--;
        }

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
