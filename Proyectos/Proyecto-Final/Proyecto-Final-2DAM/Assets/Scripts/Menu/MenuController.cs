using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    public AudioClip sonidoBoton;
    private AudioSource audioSource;


    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void SalirDelJuego()
    {
        ReproducirSonido();
        Application.Quit();
    }

    public void ReproducirSonido()
    {
        if (sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }
    }

    // Cambia a la escena después de que el sonido se haya reproducido
    private IEnumerator CargarEscenaDespuesDeSonido(string nombreEscena)
    {
        yield return new WaitForSeconds(sonidoBoton != null ? sonidoBoton.length : 0);
        SceneManager.LoadScene(nombreEscena);
    }
    public void CambiarAScena(string nombreEscena)
    {
        ReproducirSonido();
        StartCoroutine(CargarEscenaDespuesDeSonido(nombreEscena));
    }
}
