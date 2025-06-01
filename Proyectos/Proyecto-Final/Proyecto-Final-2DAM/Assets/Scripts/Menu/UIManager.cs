using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button attackButton;

    private PlayerCombat playerCombat;

    void Start()
    {
        // Busca el jugador en la escena (por ejemplo por tag)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCombat = player.GetComponent<PlayerCombat>();
        }

        if (attackButton != null && playerCombat != null)
        {
            attackButton.onClick.AddListener(() => playerCombat.TriggerAttack());
        }
    }
}
