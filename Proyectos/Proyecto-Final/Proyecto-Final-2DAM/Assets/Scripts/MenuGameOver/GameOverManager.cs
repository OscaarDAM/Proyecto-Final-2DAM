using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject canvasGameOver;
    public GameObject canvasGameOverBackground;
    public TextMeshProUGUI gameOverText;
    public Button menuButton;
    public Button quitButton;

    public float fadeDuration = 2f;

    void Start()
    {
        canvasGameOver.SetActive(false);
    }

    public void ShowGameOver()
    {

        // 💥 PAUSAR EL JUEGO
        Time.timeScale = 0f;

        // Ocultar el canvas de fondo del juego y mostrar el canvas de Game Over
        canvasGameOverBackground.SetActive(false);
        
        canvasGameOver.SetActive(true);

        gameOverText.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        // Establecer transparencia inicial en 0 (invisibles)
        SetUIElementAlpha(gameOverText, 0f);
        SetUIElementAlpha(menuButton.image, 0f);
        SetUIElementAlpha(menuButton.GetComponentInChildren<TextMeshProUGUI>(), 0f);
        SetUIElementAlpha(quitButton.image, 0f);
        SetUIElementAlpha(quitButton.GetComponentInChildren<TextMeshProUGUI>(), 0f);

        StartCoroutine(FadeInUIElements());
    }

    IEnumerator FadeInUIElements()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // usamos unscaledDeltaTime para que el fade no se frene si pausamos el tiempo
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            // Aplicar alpha a texto y botones
            SetUIElementAlpha(gameOverText, alpha);
            SetUIElementAlpha(menuButton.image, alpha);
            SetUIElementAlpha(menuButton.GetComponentInChildren<TextMeshProUGUI>(), alpha);
            SetUIElementAlpha(quitButton.image, alpha);
            SetUIElementAlpha(quitButton.GetComponentInChildren<TextMeshProUGUI>(), alpha);

            yield return null;
        }
    }

    void SetUIElementAlpha(Graphic uiElement, float alpha)
    {
        if (uiElement != null)
        {
            Color color = uiElement.color;
            color.a = alpha;
            uiElement.color = color;
        }
    }

    // Botón: Volver al menú principal
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // reanudar el tiempo
        SceneManager.LoadScene("Menu"); // Cambiar a tu nombre de escena real
    }

    // Botón: Salir de la aplicación
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Solo en modo editor
#endif
    }
}
