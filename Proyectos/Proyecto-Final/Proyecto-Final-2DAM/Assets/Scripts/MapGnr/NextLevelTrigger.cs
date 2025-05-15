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
            }

            // Incrementa el nivel
            int currentLevel = PlayerPrefs.GetInt("Level", 1);
            PlayerPrefs.SetInt("Level", currentLevel + 1);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
