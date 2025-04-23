using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public List<EnemyFollow> roomEnemies = new List<EnemyFollow>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))  // tecla temporal
        {
            Debug.Log("Forzando activación de enemigos");
            foreach (var enemy in roomEnemies)
            {
                enemy.SetCanFollow(true);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entró en trigger de sala.");

            foreach (var enemy in roomEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetCanFollow(true);
                    Debug.Log("Activado seguimiento en: " + enemy.name);
                }
            }

            //Destroy(gameObject);
        }
    }


}
