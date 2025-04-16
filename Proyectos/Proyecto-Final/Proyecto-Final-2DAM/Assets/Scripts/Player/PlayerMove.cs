using UnityEngine;

public class PlayerMove              : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidad de movimiento

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura de input (horizontal y vertical)
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D o Flechas izquierda/derecha
        movement.y = Input.GetAxisRaw("Vertical");   // W/S o Flechas arriba/abajo

        // Normaliza el vector para que las diagonales no sean más rápidas
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        // Movimiento del personaje
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
