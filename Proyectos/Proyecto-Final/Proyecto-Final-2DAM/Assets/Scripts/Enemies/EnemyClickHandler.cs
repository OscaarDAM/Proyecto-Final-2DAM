using UnityEngine;

public class EnemyClickHandler : MonoBehaviour
{
    public int damageAmount = 100000;  // Daño que se aplicará al enemigo

    void Update()
    {
        // Detectar si el jugador hace clic con el ratón
        if (Input.GetMouseButtonDown(0))  // 0 es el botón izquierdo del ratón
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            // Verificar si el raycast tocó algo
            if (hit.collider != null)
            {
                // Verificar si el objeto tocado es un enemigo
                EnemyFollow enemy = hit.collider.GetComponent<EnemyFollow>();
                if (enemy != null)
                {
                    // Aplicar daño al enemigo
                    enemy.TakeDamage(damageAmount);
                    Debug.Log("Enemigo tocado! Vida restante: " + enemy.currentHealth);
                }
            }
        }
    }
}
