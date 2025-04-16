using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionEscenasUI : MonoBehaviour
{

    public static TransicionEscenasUI Instance; // Instancia de la clase para el patrón Singleton

    [Header("Disolver")]

    public CanvasGroup canvasGroup; // Referencia al CanvasGroup que controla la opacidad del canvas

    public float tiempoDeEsperaEntrada = 1f; // Tiempo de espera para la disolución de entrada
    public float tiempoDeEsperaSalida = 1f; // Tiempo de espera para la disolución de salida


    public void Start()
    {
        DisolverEntrada();
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Asigna la instancia si no existe
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destruye el objeto si ya existe una instancia
        }
    }

    public void DisolverEntrada()
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, tiempoDeEsperaEntrada).setOnComplete(() =>
        {
            canvasGroup.interactable = false; // Desactiva la interacción con el canvas después de la disolución
            canvasGroup.blocksRaycasts = false; // Desactiva los raycasts para evitar interacciones no deseadas
        });
    }

    public void DisolverSalida(System.Action alTerminar)
{
    canvasGroup.interactable = true;
    canvasGroup.blocksRaycasts = true;

    LeanTween.alphaCanvas(canvasGroup, 1f, tiempoDeEsperaSalida).setOnComplete(() =>
    {
        alTerminar?.Invoke(); // Ejecuta la acción que le pasaste
    });
}

}
