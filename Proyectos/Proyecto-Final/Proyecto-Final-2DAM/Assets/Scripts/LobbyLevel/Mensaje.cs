using UnityEngine;
using TMPro; // si usás TextMeshPro

public class MostrarMensajeTrigger : MonoBehaviour
{
    public GameObject mensajeUI; // Asigná aquí el objeto del mensaje en el Inspector

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            mensajeUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            mensajeUI.SetActive(false);
        }
    }
}
