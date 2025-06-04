using UnityEngine;

public class ImmortalityPotion : MonoBehaviour
{
    private bool isCollected = false;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Vector3 startPos;

    public float floatAmplitude = 0.1f;
    public float floatFrequency = 1f;

    public GameObject immortalityEffectPrefab;
    public AudioClip pickupSound;
    private AudioSource audioSource;

    public float immortalityDuration = 5f; // Duración de la inmortalidad en segundos

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        startPos = transform.position;

        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, originalScale, 0.3f).setEaseOutBack();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
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
                player.BecomeImmortal(immortalityDuration);
                isCollected = true;

                if (pickupSound != null)
                    audioSource.PlayOneShot(pickupSound);

                if (immortalityEffectPrefab != null)
                    Instantiate(immortalityEffectPrefab, transform.position, Quaternion.identity);

                LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseInBack();
                LeanTween.alpha(gameObject, 0f, 0.2f).setOnComplete(() =>
                {
                    Destroy(gameObject);
                });
            }
        }
    }
}
    