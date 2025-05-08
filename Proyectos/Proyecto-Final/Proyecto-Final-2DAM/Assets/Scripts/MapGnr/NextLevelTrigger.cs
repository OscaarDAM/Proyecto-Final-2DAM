using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    // Este método se llama cuando el jugador toca el tile especial
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Aquí puedes hacer otras cosas antes de recargar la escena, como actualizar el tiempo o guardar el estado.
            RecargarEscena();
        }
    }

    // Método para recargar la escena
    private void RecargarEscena()
    {
        // Esto recarga la escena activa (la misma en la que estás)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
