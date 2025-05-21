using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image barraVida;
    private PlayerJoystickMove playerMove;
    private float vidaMaxima;

    void Update()
    {
        // Si todavía no encontramos al jugador, intentamos encontrarlo
        if (playerMove == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMove = player.GetComponent<PlayerJoystickMove>();
                vidaMaxima = playerMove.health;
            }
        }

        // Si ya tenemos el jugador, actualizamos la barra
        if (playerMove != null && vidaMaxima > 0 && barraVida != null)
        {
            barraVida.fillAmount = (float)playerMove.health / vidaMaxima;
        }
    }
}
