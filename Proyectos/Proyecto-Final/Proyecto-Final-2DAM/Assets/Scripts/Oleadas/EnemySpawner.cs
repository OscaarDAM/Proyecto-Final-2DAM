using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Zonas de Spawn")]
    public Transform[] spawnZones; // Zonas donde pueden aparecer enemigos

    [Header("Prefab del enemigo")]
    public GameObject enemyPrefab;

    [Header("Rango de enemigos por spawn")]
    public int minEnemies = 1;
    public int maxEnemies = 5;

    [Header("Tiempo entre spawns")]
    public float spawnInterval = 30f; // Tiempo entre cada oleada

    [Header("UI")]
    public TMP_Text spawnIndicatorText; // Texto que indica dónde spawnearon
    public float indicatorDisplayTime = 3f; // Tiempo que dura visible el texto

    [Header("Spawn dinámico")]
    public float doubleSpawnChance = 0f; // Porcentaje de probabilidad de doble zona
    public float chanceIncreasePerMinute = 10f; // Cuánto aumenta cada minuto
    private float chanceTimer = 0f; // Temporizador para controlar el minuto

    private float timer; // Temporizador del spawn
    private Coroutine indicatorCoroutine; // Para manejar el texto en pantalla

    void Start()
    {
        // Inicializa el temporizador con el intervalo
        timer = spawnInterval;

        // Spawnea enemigos al inicio en una sola zona
        SpawnEnemiesInZones(1);
    }

    void Update()
    {
        // Disminuye los temporizadores con el tiempo
        timer -= Time.deltaTime;
        chanceTimer += Time.deltaTime;

        // Cada minuto aumenta la probabilidad de doble zona
        if (chanceTimer >= 60f)
        {
            chanceTimer = 0f;
            doubleSpawnChance = Mathf.Clamp(doubleSpawnChance + chanceIncreasePerMinute, 0f, 100f);
        }

        // Si se cumplió el intervalo, spawnea enemigos
        if (timer <= 0f)
        {
            // Decide si spawnear en 1 o 2 zonas
            int zonesToSpawn = (Random.Range(0f, 100f) < doubleSpawnChance) ? 2 : 1;

            SpawnEnemiesInZones(zonesToSpawn);
            timer = spawnInterval; // Reinicia el temporizador
        }
    }

    // Spawnea enemigos en la cantidad de zonas indicada
    void SpawnEnemiesInZones(int numberOfZones)
    {
        List<int> chosenIndexes = new List<int>();

        // Selecciona zonas aleatorias sin repetir
        while (chosenIndexes.Count < numberOfZones && chosenIndexes.Count < spawnZones.Length)
        {
            int randomIndex = Random.Range(0, spawnZones.Length);
            if (!chosenIndexes.Contains(randomIndex))
                chosenIndexes.Add(randomIndex);
        }

        // Spawnea enemigos en cada zona elegida
        foreach (int index in chosenIndexes)
        {
            Transform zone = spawnZones[index];
            int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                // Posición aleatoria dentro de la zona
                Vector2 offset = Random.insideUnitCircle * 1.5f;
                Vector3 spawnPosition = zone.position + new Vector3(offset.x, offset.y, 0);

                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }

            // Muestra texto con el nombre de la zona
            ShowSpawnIndicator(zone.name);
        }
    }

    // Muestra el nombre de la zona en pantalla durante unos segundos
    void ShowSpawnIndicator(string zoneName)
    {
        if (spawnIndicatorText == null) return;

        // Detiene el mensaje anterior si todavía está mostrándose
        if (indicatorCoroutine != null)
            StopCoroutine(indicatorCoroutine);

        spawnIndicatorText.text = $"{zoneName}";
        spawnIndicatorText.gameObject.SetActive(true);

        indicatorCoroutine = StartCoroutine(HideIndicatorAfterDelay());
    }

    // Oculta el texto después de cierto tiempo
    System.Collections.IEnumerator HideIndicatorAfterDelay()
    {
        yield return new WaitForSeconds(indicatorDisplayTime);
        spawnIndicatorText.gameObject.SetActive(false);
    }
}
