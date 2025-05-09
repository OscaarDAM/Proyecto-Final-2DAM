using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove player = other.GetComponent<PlayerMove>();
            if (player != null)
            {
                PlayerPrefs.SetInt("PlayerHealth", player.health);
                // Puedes guardar más datos si los agregas después, como puntuación, nivel, etc.
            }

            // Reinicia la escena actual (puedes cambiarlo por otro nombre de escena si quieres)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
