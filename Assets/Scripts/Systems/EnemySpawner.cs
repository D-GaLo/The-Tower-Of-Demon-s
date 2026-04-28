using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    public GameObject[] enemyPrefabs; 
    public int maxEnemiesInRoom = 5; // Regla del GDD: Máximo 5 enemigos
    public Vector2 areaSize = new Vector2(14, 6); // Un poco menor que la sala (18xX)
    public float distanciaMinimaEntreEnemigos = 2f; 

    void Start() {
        SpawnEnemies();
    }

    void SpawnEnemies() {
        int enemiesToSpawn = Random.Range(2, maxEnemiesInRoom + 1);
        int intentosRealizados = 0;
        int spawnados = 0;

        // Intentamos colocar enemigos hasta llegar al número o agotar intentos
        while (spawnados < enemiesToSpawn && intentosRealizados < 50) {
            intentosRealizados++;

            Vector2 randomPos = new Vector2(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(-areaSize.y / 2, areaSize.y / 2)
            );

            Vector3 spawnPos = transform.position + new Vector3(randomPos.x, randomPos.y, 0);

            // Verificamos si hay otro enemigo demasiado cerca
            Collider2D hit = Physics2D.OverlapCircle(spawnPos, distanciaMinimaEntreEnemigos);
            
            if (hit == null) {
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);
                spawnados++;
            }
        }
    }
}