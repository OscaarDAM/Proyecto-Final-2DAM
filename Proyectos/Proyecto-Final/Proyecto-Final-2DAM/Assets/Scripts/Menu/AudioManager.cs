using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public float VolumenMusica => PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
    public float VolumenSonidos => PlayerPrefs.GetFloat("VolumenSonidos", 0.5f);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject); // Evita duplicados
        }
    }
}
