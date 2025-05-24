using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 1;

    private void Start()
    {
        // Destruir automáticamente tras 2 segundos
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerJoystickMove player = other.GetComponent<PlayerJoystickMove>();
            if (player != null)
            {
                player.health -= damage;
            }
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject); // Se destruye al chocar con algo sólido
        }
    }
}
