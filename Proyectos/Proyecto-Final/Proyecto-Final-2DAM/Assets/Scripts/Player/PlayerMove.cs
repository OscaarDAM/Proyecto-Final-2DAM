using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidad de movimiento

    private Rigidbody2D rb;
    private Vector2 movement;

    public int health = 3; // Salud del jugador (ejemplo)
    private bool isDead = false;

    public GameOverManager gameOverManager; // Referencia al GameOverManager

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameOverManager = FindObjectOfType<GameOverManager>();
        Time.timeScale = 1f; // Asegurarse de que el tiempo está en marcha al inicio    
    }


    void Update()
    {
        // Detectar teclas de movimiento
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // Control de muerte por tecla K
        if (Input.GetKeyDown(KeyCode.K))
        {
            health = 0;
        }

        if (health <= 0 && !isDead)
        {
            isDead = true;
            gameOverManager.ShowGameOver();

        }
    }



    void FixedUpdate()
    {
        // Movimiento del personaje
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void QuitarTodaLaVida()
    {
        health = 0;
    }

}
