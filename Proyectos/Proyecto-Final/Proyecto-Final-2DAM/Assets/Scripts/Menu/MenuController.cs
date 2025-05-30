using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tapToStart;
    public GameObject title;
    public GameObject buttonJugar;
    public GameObject buttonOpciones;
    public GameObject buttonCreditos;
    public GameObject buttonSalir;

    [Header("Audio")]
    public AudioClip sonidoBoton;
    private AudioSource audioSource;

    private bool hasTapped = false;

    void Awake()
    {
        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Oculta todo excepto el "Pulsa aquí"
        title.SetActive(false);
        buttonJugar.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonSalir.SetActive(false);
        buttonCreditos.SetActive(false);

        StartCoroutine(BlinkTapToStart());
    }

    void Update()
    {
        if (!hasTapped && Input.GetMouseButtonDown(0))
        {
            hasTapped = true;
            StopAllCoroutines();
            LeanTween.alphaCanvas(tapToStart.GetComponent<CanvasGroup>(), 0f, 0.5f).setOnComplete(() =>
            {
                tapToStart.SetActive(false);
                ShowTitle();
            });
        }
    }

    IEnumerator BlinkTapToStart()
    {
        CanvasGroup cg = tapToStart.GetComponent<CanvasGroup>();
        while (true)
        {
            LeanTween.alphaCanvas(cg, 0f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            LeanTween.alphaCanvas(cg, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void ShowTitle()
    {
        title.SetActive(true);
        CanvasGroup cg = title.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        LeanTween.alphaCanvas(cg, 1f, 1f).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
        {
            ShowButtons();
        });
    }

    void ShowButtons()
    {
        ShowButton(buttonJugar, 0f);
        ShowButton(buttonOpciones, 0.5f);
        ShowButton(buttonCreditos, 1f);
        ShowButton(buttonSalir, 1.5f);
    }

    void ShowButton(GameObject button, float delay)
    {
        button.SetActive(true);
        CanvasGroup cg = button.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        LeanTween.alphaCanvas(cg, 1f, 0.5f).setDelay(delay).setEase(LeanTweenType.easeOutCubic);
    }

    public void ReproducirSonido()
    {
        if (sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }
    }

    public void SalirDelJuego()
    {
        ReproducirSonido();
        Application.Quit();
    }

    public void CambiarAScena(string nombreEscena)
    {
        ReproducirSonido();

        // Si tienes un sistema de transición tipo fade
        TransicionEscenasUI.Instance.DisolverSalida(() =>
        {
            PlayerPrefs.SetInt("Level", 1);
            SceneManager.LoadScene(nombreEscena);
        });

        // Si NO tienes sistema de transición, usa este:
        // StartCoroutine(CargarEscenaDespuesDeSonido(nombreEscena));
    }

    private IEnumerator CargarEscenaDespuesDeSonido(string nombreEscena)
    {
        yield return new WaitForSeconds(sonidoBoton != null ? sonidoBoton.length : 0);
        SceneManager.LoadScene(nombreEscena);
    }
}
