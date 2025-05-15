using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class TilemapDoorDetector : MonoBehaviour
{
    public Tilemap tilemapPared;

    // Para Puerta 1
    public TileBase nuevoTilePuerta1;
    public Vector3Int posicionPuerta1 = new Vector3Int(-1, -4, 0);
    public string scenePuerta1 = "Dungeon";

    // Para Puerta 2
    public TileBase nuevoTilePuerta2;
    public Vector3Int posicionPuerta2 = new Vector3Int(7, -4, 0);
    public string scenePuerta2 = "NivelB";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PuertaDungeon"))
        {
            StartCoroutine(ActivarPuerta(posicionPuerta1, nuevoTilePuerta1, scenePuerta1));
        }
        else if (other.CompareTag("PuertaOleadas"))
        {
            StartCoroutine(ActivarPuerta(posicionPuerta2, nuevoTilePuerta2, scenePuerta2));
        }
    }

    private IEnumerator ActivarPuerta(Vector3Int posicionTile, TileBase nuevoTile, string sceneName)
    {

        // Desactivar el script PlayerMove para que no se mueva el jugador
        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        // Cambiar el tile en la posición correspondiente
        tilemapPared.SetTile(posicionTile, nuevoTile);

        // Esperar 2 segundos antes de iniciar la disolución
        yield return new WaitForSeconds(1f);

        // Llamar a DisolverSalida y cargar la escena cuando termine
        if (TransicionEscenasUI.Instance != null)
        {
            TransicionEscenasUI.Instance.DisolverSalida(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            // Si no existe la instancia, cargar la escena directamente como fallback
            SceneManager.LoadScene(sceneName);
        }
    }

}
