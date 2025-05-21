using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CodePause : MonoBehaviour
{

    public GameObject pauseMenu;
    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused == false)
            {
                pauseMenu.SetActive(true);
                isPaused = true;

                Time.timeScale = 0f; // Pausa el juego
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        Time.timeScale = 1f; // Reanuda el juego
    }

    public void QuitGame()
    {
        Application.Quit(); // Cierra la aplicación
        Debug.Log("Saliendo del juego...");
    }

    public void irMenuPrincipal()
    {
        Time.timeScale = 1f; // Reanuda el juego antes de cambiar de escena
        SceneManager.LoadScene("Menu"); // Cambia a la escena del menú principal
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        isPaused = true;

        Time.timeScale = 0f; // Pausa el juego
    }
}