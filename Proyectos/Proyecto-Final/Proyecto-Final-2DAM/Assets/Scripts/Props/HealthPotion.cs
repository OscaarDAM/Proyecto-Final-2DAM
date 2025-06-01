using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int healAmount = 20; // Cuánta vida cura la poción
    private bool isCollected = false; // Para evitar múltiples recogidas

    private SpriteRenderer sr; // Referencia al SpriteRenderer
    private Vector3 originalScale;
    private Vector3 startPos; // Posición inicial para el efecto de flotación

    public float floatAmplitude = 0.1f; // Cuánto se eleva y baja la poción
    public float floatFrequency = 1f; // Qué tan rápido flota

    public GameObject healEffectPrefab; // Prefab de partículas (opcional)
    public AudioClip pickupSound; // Sonido al recoger la poción (opcional)
    private AudioSource audioSource;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        startPos = transform.position;

        // Escala inicial cero para efecto pop
        transform.localScale = Vector3.zero;

        // Lanzamos animación de aparición
        LeanTween.scale(gameObject, originalScale, 0.3f).setEaseOutBack();

        // Aseguramos que haya un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Efecto de flotación (arriba y abajo suavemente)
        float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
    }

    // Cuando el jugador entra en el trigger de la poción
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerJoystickMove player = other.GetComponent<PlayerJoystickMove>();

            if (player != null)
            {
                player.Heal(healAmount); // Curamos al jugador
                isCollected = true;

                // Reproducir sonido si hay uno asignado
                if (pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

                // Instanciar efecto visual si está disponible
                if (healEffectPrefab != null)
                {
                    Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                }

                // Animación de desaparición
                LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseInBack();
                LeanTween.alpha(gameObject, 0f, 0.2f).setOnComplete(() =>
                {
                    Destroy(gameObject); // Destruimos después del efecto
                });
            }
        }
    }
}
