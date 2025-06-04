using UnityEngine;

public enum TipoAudio { Musica, Efecto }

public class AudioPrefab : MonoBehaviour
{
    public TipoAudio tipoAudio;  // Define si es música o efecto
    private AudioSource audioSrc;

    void Awake()
    {
        // Intentar obtener el AudioSource directamente
        audioSrc = GetComponent<AudioSource>();

        // Si no está en este GameObject, buscar en hijos
        if (audioSrc == null)
        {
            audioSrc = GetComponentInChildren<AudioSource>();
        }

        // Asignar volumen según el tipo
        if (audioSrc != null && AudioManager.Instance != null)
        {
            audioSrc.volume = tipoAudio == TipoAudio.Musica 
                ? AudioManager.Instance.VolumenMusica 
                : AudioManager.Instance.VolumenSonidos;
        }
        else
        {
            Debug.LogWarning($"⚠️ AudioPrefab en '{gameObject.name}' no encontró AudioSource o AudioManager.");
        }
    }
}
