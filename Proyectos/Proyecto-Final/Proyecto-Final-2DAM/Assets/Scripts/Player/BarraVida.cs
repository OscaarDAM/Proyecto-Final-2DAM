using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image barraVida; // Imagen que representa la barra de vida
    private PlayerJoystickMove playerMove; // Referencia al script del jugador
    private float vidaMaxima; // Vida máxima del jugador

    void Update()
    {
        // Si todavía no encontramos al jugador, intentamos encontrarlo
        if (playerMove == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMove = player.GetComponent<PlayerJoystickMove>();
                vidaMaxima = playerMove.maxHealth; // Obtenemos la vida máxima
            }
        }

        // Si ya tenemos el jugador, actualizamos la barra
        if (playerMove != null && vidaMaxima > 0 && barraVida != null)
        {
            barraVida.fillAmount = (float)playerMove.health / vidaMaxima;
        }
    }
}
