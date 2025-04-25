using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    public float speed = 3f;
    private Transform player;
    private Rigidbody2D rb;
    private bool canFollow = false;

    // Variable de vida del enemigo
    public float health = 100f; // Vida inicial del enemigo

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Reasigna la referencia al jugador en cada frame de FixedUpdate para asegurar que siempre se tiene la posición más actual.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Si no hay jugador o el enemigo no puede seguir, se sale de la función.
        if (player == null || !canFollow) return;

        // Calcular la dirección hacia el jugador y mover al enemigo hacia él.
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        Vector2 newPos = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    // Método para activar o desactivar el seguimiento del jugador
    public void SetCanFollow(bool value)
    {
        canFollow = value;
    }

    // Método para aplicar daño al enemigo
    public void TakeDamage(float damage)
    {
        health -= damage;  // Reducir la vida del enemigo

        if (health <= 0)
        {
            KillEnemy();  // Si la vida llega a 0, matar al enemigo
        }
    }

    // Método para destruir al enemigo
    public void KillEnemy()
    {
        Debug.Log("El enemigo ha sido destruido.");
        Destroy(gameObject);  // Destruir el objeto enemigo
    }
}
