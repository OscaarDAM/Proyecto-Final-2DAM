using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Button attackButton;

    private PlayerCombat playerCombat;

    void Start()
    {
        StartCoroutine(AssignPlayerCombatCoroutine());
    }

    IEnumerator AssignPlayerCombatCoroutine()
    {
        // Esperar hasta que el jugador exista y tenga PlayerCombat
        while (playerCombat == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerCombat = player.GetComponent<PlayerCombat>();
                if (playerCombat != null && attackButton != null)
                {
                    attackButton.onClick.RemoveAllListeners();
                    attackButton.onClick.AddListener(() => playerCombat.TriggerAttack());
                    Debug.Log("Listener asignado al botón de ataque");
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
