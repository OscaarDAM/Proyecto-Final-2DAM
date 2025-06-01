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
    public GameObject opcionesPanel; // 🎛️ Panel que contiene sliders y botón de volver
    public Slider sliderSonidos;
    public Slider sliderMusica;
    public Button botonVolver;

    [Header("Audio")]
    public AudioClip sonidoBoton;
    public AudioClip sonidoCamion; // 🎵 Sonido de camión arrancando
    public AudioClip musicaFondo;  // 🎶 Música que suena nada más entrar
    public AudioSource audioSourceSonidos; // 🔊 AudioSource para efectos
    public AudioSource audioSourceMusica;  // 🎶 AudioSource para música

    [Header("Camión")]
    public GameObject camion; // 🚚 Referencia al objeto del camión
    public float distanciaMovimiento = 10f; // Cuánto se moverá el camión hacia la derecha
    public float duracionMovimiento = 2f;   // Tiempo que tardará en moverse

    private bool hasTapped = false;
    private bool animacionEnCurso = false; // 🔄 Para saber si la animación de inicio ya empezó
    private string escenaDestinoPendiente = ""; // 🎯 Escena que se debe cargar si se salta

    void Awake()
    {
        // Configurar audios si no se asignaron
        if (audioSourceSonidos == null)
            audioSourceSonidos = gameObject.AddComponent<AudioSource>();
        if (audioSourceMusica == null)
            audioSourceMusica = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Oculta todo excepto el "Pulsa aquí"
        title.SetActive(false);
        buttonJugar.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonSalir.SetActive(false);
        buttonCreditos.SetActive(false);
        opcionesPanel.SetActive(false); // Ocultar menú de opciones al inicio

        // 🎶 Reproducir música al entrar
        if (musicaFondo != null)
        {
            audioSourceMusica.clip = musicaFondo;
            audioSourceMusica.loop = true;
            audioSourceMusica.Play();
        }

        // Inicializar sliders con el volumen actual
        sliderSonidos.value = audioSourceSonidos.volume;
        sliderMusica.value = audioSourceMusica.volume;

        // Añadir listeners
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
            // ⏩ Si se hace clic durante la animación del camión, saltar directamente a la escena
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

    // 🟢 Nuevo método que se llama desde el botón "Jugar"
    public void IniciarJuego(string nombreEscena)
    {
        StartCoroutine(SecuenciaInicioJuego(nombreEscena));
    }

    // ⏱️ Corrutina que maneja la secuencia: sonido, ocultar UI, mover camión, cambiar de escena
    private IEnumerator SecuenciaInicioJuego(string nombreEscena)
    {
        animacionEnCurso = true; // 🔁 Marcar que está en curso
        escenaDestinoPendiente = nombreEscena;

        ReproducirSonidoCamion();

        // 🔻 Ocultar elementos con fade usando LeanTween
        OcultarElemento(title);
        OcultarElemento(buttonJugar);
        OcultarElemento(buttonOpciones);
        OcultarElemento(buttonCreditos);
        OcultarElemento(buttonSalir);

        // ⏳ Esperar 6 segundos antes de mover el camión
        yield return new WaitForSeconds(6f);

        // 🚚 Mover el camión hacia la derecha
        Vector3 destino = camion.transform.position + Vector3.right * distanciaMovimiento;
        LeanTween.move(camion, destino, duracionMovimiento).setEase(LeanTweenType.easeInOutSine);

        // ⌛ Esperar a que termine el movimiento
        yield return new WaitForSeconds(duracionMovimiento);

        // 🔁 Transición final a la nueva escena
        TransicionEscenasUI.Instance.DisolverSalida(() =>
        {
            PlayerPrefs.SetInt("Level", 1);
            SceneManager.LoadScene(nombreEscena);
        });
    }

    // 🔻 Ocultar cualquier GameObject con CanvasGroup usando LeanTween
    private void OcultarElemento(GameObject elemento)
    {
        CanvasGroup cg = elemento.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            LeanTween.alphaCanvas(cg, 0f, 0.5f).setEase(LeanTweenType.easeInCubic)
                     .setOnComplete(() => elemento.SetActive(false));
        }
    }

    // ⚙️ MÉTODOS DE OPCIONES
    public void AbrirOpciones()
    {
        ReproducirSonido();

        // Ocultar botones
        buttonJugar.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonCreditos.SetActive(false);
        buttonSalir.SetActive(false);

        // Mostrar panel de opciones
        opcionesPanel.SetActive(true);
    }

    private void CerrarOpciones()
    {
        ReproducirSonido();

        opcionesPanel.SetActive(false);

        // Mostrar botones de nuevo
        buttonJugar.SetActive(true);
        buttonOpciones.SetActive(true);
        buttonCreditos.SetActive(true);
        buttonSalir.SetActive(true);
    }

    private void ActualizarVolumenSonidos(float valor)
    {
        audioSourceSonidos.volume = valor;
    }

    private void ActualizarVolumenMusica(float valor)
    {
        audioSourceMusica.volume = valor;
    }
}
