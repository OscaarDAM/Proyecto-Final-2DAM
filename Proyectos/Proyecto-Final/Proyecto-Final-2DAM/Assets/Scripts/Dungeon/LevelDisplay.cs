using UnityEngine;
using TMPro;

public class LevelDisplay : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("Level", 1);
        levelText.text = "Nivel: " + currentLevel;
    }
}
