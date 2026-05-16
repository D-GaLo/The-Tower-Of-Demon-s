using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {
    public GameObject[] enemyPrefabs; 
    public int maxEnemiesInRoom = 5; 
    public Vector2 areaSize = new Vector2(14, 6); 
    public float distanciaMinimaEntreEnemigos = 2f; 

    [Header("Configuración de Respawn")]
    public float tiempoRespawnMinutos = 3f;
    private List<GameObject> enemigosSpawneados = new List<GameObject>();
    private bool esperandoRespawn = false;

    void Start() {
        SpawnEnemies();
    }

    void Update() {
        if (esperandoRespawn || enemigosSpawneados.Count == 0) return;

        bool todosMuertos = true;
        for (int i = 0; i < enemigosSpawneados.Count; i++) {
            if (enemigosSpawneados[i] != null) {
                todosMuertos = false;
                break;
            }
        }

        if (todosMuertos) {
            StartCoroutine(RutinaRespawn());
        }
    }

    IEnumerator RutinaRespawn() {
        esperandoRespawn = true;
        enemigosSpawneados.Clear();
        
        Debug.Log($"<color=yellow>[Spawner]</color> Sala limpiada. Los enemigos volverán en {tiempoRespawnMinutos} minutos...");
        
        yield return new WaitForSeconds(tiempoRespawnMinutos * 60f); 
        
        Debug.Log("<color=yellow>[Spawner]</color> ¡Reapareciendo enemigos!");
        SpawnEnemies();
        esperandoRespawn = false;
    }

    void SpawnEnemies() {
        int enemiesToSpawn = Random.Range(2, maxEnemiesInRoom + 1);
        int intentosRealizados = 0;
        int spawnados = 0;

        int nivelJugador = 1;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            HeroStats hs = player.GetComponent<HeroStats>();
            if (hs != null) {
                nivelJugador = hs.level;
            }
        }

        while (spawnados < enemiesToSpawn && intentosRealizados < 50) {
            intentosRealizados++;

            Vector2 randomPos = new Vector2(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(-areaSize.y / 2, areaSize.y / 2)
            );

            Vector3 spawnPos = transform.position + new Vector3(randomPos.x, randomPos.y, 0);

            Collider2D hit = Physics2D.OverlapCircle(spawnPos, distanciaMinimaEntreEnemigos);
            
            if (hit == null) {
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject enemigoObj = Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);
                
                enemigosSpawneados.Add(enemigoObj);

                EnemyStats eStats = enemigoObj.GetComponent<EnemyStats>();
                if (eStats != null && !eStats.nivelManual) {
                    
                    int nivelCalculado = nivelJugador;
                    int dado = Random.Range(1, 101);

                    if (dado <= 45) { 
                        nivelCalculado = nivelJugador - 1;
                    } else if (dado <= 65) { 
                        nivelCalculado = nivelJugador + 1;
                    } else {
                        nivelCalculado = nivelJugador;
                    }

                    if (nivelCalculado < 1) nivelCalculado = 1;
                    if (nivelCalculado > 10) nivelCalculado = 10;

                    eStats.level = nivelCalculado;
                    eStats.EscalarEstadisticas();
                }

                EnemyAI scriptEnemigo = enemigoObj.GetComponent<EnemyAI>();
                if (scriptEnemigo != null) {
                    float margenMinimo = 3f;

                    float offsetX_A = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);                        
                    float offsetX_B = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);

                    if (Mathf.Abs(offsetX_A - offsetX_B) < margenMinimo) {
                        if (offsetX_A > 0) {
                            offsetX_B = offsetX_A - margenMinimo;
                        } else {
                            offsetX_B = offsetX_A + margenMinimo; 
                        }
                    }

                    Vector3 posGlobalA = new Vector3(transform.position.x + offsetX_A, spawnPos.y, 0);
                    Vector3 posGlobalB = new Vector3(transform.position.x + offsetX_B, spawnPos.y, 0);

                    scriptEnemigo.ConfigurarPatrulla(posGlobalA, posGlobalB);
                }
                spawnados++;
            }
        }
    }
}