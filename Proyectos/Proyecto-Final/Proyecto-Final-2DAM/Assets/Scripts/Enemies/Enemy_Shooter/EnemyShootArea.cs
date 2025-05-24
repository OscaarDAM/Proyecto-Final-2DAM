using UnityEngine;

public class EnemyShootArea : MonoBehaviour
{
     private EnemyFollow enemyFollow;

    private void Awake()
    {
        // Obtener la referencia al enemigo padre
        enemyFollow = GetComponentInParent<EnemyFollow>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador entró en el área de disparo");
            enemyFollow.SetShooting(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyFollow.OnExitShootingArea();
        }
    }
}
