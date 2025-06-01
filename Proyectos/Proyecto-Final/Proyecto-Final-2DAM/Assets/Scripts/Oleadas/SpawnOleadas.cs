using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public int spawnPointCount = 8;
    public float radius = 10f;
    public GameObject enemyPrefab;
    public TextMeshProUGUI enemyCounterText;

    private List<Vector2> spawnPoints = new List<Vector2>();
    private int difficultyLevel = 1;
    private float interval = 20f;

    private float elapsedTime = 0f;

    void Start()
    {
        for (int i = 0; i < spawnPointCount; i++)
        {
            float angle = i * Mathf.PI * 2f / spawnPointCount;
            Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            spawnPoints.Add(newPos);
        }

        // Mostrar tiempo inicial
        UpdateTimerText();

        // Iniciar con primera oleada
        SpawnEnemies(difficultyLevel);

        // Comenzar oleadas
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (enemyCounterText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            enemyCounterText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            difficultyLevel++;
            SpawnEnemies(difficultyLevel);
        }
    }

    void SpawnEnemies(int difficulty)
    {
        foreach (Vector2 point in spawnPoints)
        {
            int enemyCount = Random.Range(1, difficulty + 1);
            for (int i = 0; i < enemyCount; i++)
            {
                Vector2 spawnPos = (Vector2)player.position + point + Random.insideUnitCircle * 1.5f;
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                EnemyFollow followScript = enemy.GetComponent<EnemyFollow>();
                if (followScript != null)
                {
                    followScript.SetCanFollow(true);
                    followScript.roomBounds = new Bounds(player.position, new Vector3(50, 50, 0));
                }
            }
        }
    }
}
