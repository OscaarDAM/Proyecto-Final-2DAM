using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    public float speed = 3f;
    private Transform player;
    private Rigidbody2D rb;
    private bool canFollow = false;

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

    public void SetCanFollow(bool value)
    {
        canFollow = value;
    }
}
