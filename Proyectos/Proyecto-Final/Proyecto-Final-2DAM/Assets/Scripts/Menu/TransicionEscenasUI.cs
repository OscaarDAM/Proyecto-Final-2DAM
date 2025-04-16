using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransicionEscenasUI : MonoBehaviour
{
    [Header("Disolver")]

    public CanvasGroup canvasGroup; // Referencia al CanvasGroup que controla la opacidad del canvas

    public float tiempoDeEspera = 1f; // Tiempo de espera antes de iniciar la transición


    public void Start()
    {
        DisolverEntrada();
    }
    public void DisolverEntrada()
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, tiempoDeEspera);
    }
}
