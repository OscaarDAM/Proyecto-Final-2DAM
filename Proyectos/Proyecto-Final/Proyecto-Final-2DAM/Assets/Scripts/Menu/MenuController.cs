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

    [Header("Opciones")]
    public GameObject opcionesPanel; 
    public Slider sliderSonidos;
    public Slider sliderMusica;
    public Button botonVolver;

    // 🔤 NUEVO: Referencias a los textos de los sliders
    public TextMeshProUGUI textoSonidos;
    public TextMeshProUGUI textoMusica;

    [Header("Audio")]
    public AudioClip sonidoBoton;
    public AudioClip sonidoCamion;
    public AudioClip musicaFondo;
    public AudioSource audioSourceSonidos;
    public AudioSource audioSourceMusica;

    [Header("Camión")]
    public GameObject camion;
    public float distanciaMovimiento = 10f;
    public float duracionMovimiento = 2f;
    public ParticleSystem humoCamion;

    private bool hasTapped = false;
    private bool animacionEnCurso = false;
    private string escenaDestinoPendiente = "";

    void Awake()
    {
        if (audioSourceSonidos == null)
            audioSourceSonidos = gameObject.AddComponent<AudioSource>();
        if (audioSourceMusica == null)
            audioSourceMusica = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        title.SetActive(false);
        buttonJugar.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonCreditos.SetActive(false);
        buttonSalir.SetActive(false);
        opcionesPanel.SetActive(false);

        // ❌ Ocultar textos de sliders al inicio
        if (textoSonidos != null) textoSonidos.gameObject.SetActive(false);
        if (textoMusica != null) textoMusica.gameObject.SetActive(false);

        if (musicaFondo != null)
        {
            audioSourceMusica.clip = musicaFondo;
            audioSourceMusica.loop = true;

            float volMusica = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
            audioSourceMusica.volume = volMusica;
            sliderMusica.value = volMusica;

            audioSourceMusica.Play();
        }

        float volSonidos = PlayerPrefs.GetFloat("VolumenSonidos", 0.5f);
        audioSourceSonidos.volume = volSonidos;
        sliderSonidos.value = volSonidos;

        sliderSonidos.onValueChanged.AddListener(ActualizarVolumenSonidos);
        sliderMusica.onValueChanged.AddListener(ActualizarVolumenMusica);
        botonVolver.onClick.AddListener(CerrarOpciones);

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
        else if (animacionEnCurso && Input.GetMouseButtonDown(0))
        {
            TransicionEscenasUI.Instance.DisolverSalida(() =>
            {
                PlayerPrefs.SetInt("Level", 1);
                SceneManager.LoadScene(escenaDestinoPendiente);
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
            audioSourceSonidos.PlayOneShot(sonidoBoton);
        }
    }

    private void ReproducirSonidoCamion()
    {
        if (sonidoCamion != null)
        {
            audioSourceSonidos.PlayOneShot(sonidoCamion);
        }
    }

    public void SalirDelJuego()
    {
        ReproducirSonido();
        Application.Quit();
    }

    public void IniciarJuego(string nombreEscena)
    {
        StartCoroutine(SecuenciaInicioJuego(nombreEscena));
    }

    private IEnumerator SecuenciaInicioJuego(string nombreEscena)
    {
        animacionEnCurso = true;
        escenaDestinoPendiente = nombreEscena;

        ReproducirSonidoCamion();

        OcultarElemento(title);
        OcultarElemento(buttonJugar);
        OcultarElemento(buttonOpciones);
        OcultarElemento(buttonCreditos);
        OcultarElemento(buttonSalir);

        yield return new WaitForSeconds(6f);

        if (humoCamion != null)
        {
            humoCamion.Play();
        }

        Vector3 destino = camion.transform.position + Vector3.right * distanciaMovimiento;
        LeanTween.move(camion, destino, duracionMovimiento).setEase(LeanTweenType.easeInOutSine);

        yield return new WaitForSeconds(duracionMovimiento);

        if (humoCamion != null)
        {
            humoCamion.Stop();
        }

        TransicionEscenasUI.Instance.DisolverSalida(() =>
        {
            PlayerPrefs.SetInt("Level", 1);
            SceneManager.LoadScene(nombreEscena);
        });
    }

    private void OcultarElemento(GameObject elemento)
    {
        CanvasGroup cg = elemento.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            LeanTween.alphaCanvas(cg, 0f, 0.5f).setEase(LeanTweenType.easeInCubic)
                     .setOnComplete(() => elemento.SetActive(false));
        }
    }

    // 📂 Abrir menú de opciones
    public void AbrirOpciones()
    {
        ReproducirSonido();

        buttonJugar.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonCreditos.SetActive(false);
        buttonSalir.SetActive(false);

        opcionesPanel.SetActive(true);

        // ✅ Mostrar los textos junto con los sliders
        if (textoSonidos != null) textoSonidos.gameObject.SetActive(true);
        if (textoMusica != null) textoMusica.gameObject.SetActive(true);
    }

    private void CerrarOpciones()
    {
        ReproducirSonido();

        opcionesPanel.SetActive(false);

        // ❌ Ocultar textos
        if (textoSonidos != null) textoSonidos.gameObject.SetActive(false);
        if (textoMusica != null) textoMusica.gameObject.SetActive(false);

        buttonJugar.SetActive(true);
        buttonOpciones.SetActive(true);
        buttonCreditos.SetActive(true);
        buttonSalir.SetActive(true);
    }

    private void ActualizarVolumenSonidos(float valor)
    {
        audioSourceSonidos.volume = valor;
        PlayerPrefs.SetFloat("VolumenSonidos", valor);
        PlayerPrefs.Save();
    }

    private void ActualizarVolumenMusica(float valor)
    {
        audioSourceMusica.volume = valor;
        PlayerPrefs.SetFloat("VolumenMusica", valor);
        PlayerPrefs.Save();
    }
}
