using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    private bool isCollected = false;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Vector3 startPos;

    public float floatAmplitude = 0.1f;
    public float floatFrequency = 1f;

    public GameObject healEffectPrefab;
    public AudioClip pickupSound;

    public AudioSource audioSource;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        startPos = transform.position;

        // Efecto de aparición
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, originalScale, 0.3f).setEaseOutBack();

        // Asegurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Aplicar volumen desde AudioManager si existe
        if (AudioManager.Instance != null)
            audioSource.volume = AudioManager.Instance.VolumenSonidos;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerJoystickMove player = other.GetComponent<PlayerJoystickMove>();
            if (player != null)
            {
                int randomHeal = Random.Range(3, 7);
                player.Heal(randomHeal);
                isCollected = true;

                // Reproducir sonido si hay clip
                if (pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

                // Instanciar partículas
                if (healEffectPrefab != null)
                {
                    Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                }

                // Desaparecer visualmente
                LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseInBack();
                LeanTween.alpha(gameObject, 0f, 0.2f).setOnComplete(() =>
                {
                    Destroy(gameObject);
                });
            }
        }
    }
}
