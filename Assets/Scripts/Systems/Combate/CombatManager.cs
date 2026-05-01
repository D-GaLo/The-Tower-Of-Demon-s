using UnityEngine;
using System.Collections;

// Definimos los posibles estados del combate
public enum CombatState { START, PLAYER_TURN, ENEMY_TURN, WON, LOST }

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;

    public CombatState state;
    private GameObject currentEnemy;

    void Awake() {
        // Configuramos el Singleton para llamarlo fácilmente desde otros scripts
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Esta función la llamaremos desde el GameFlowController
    public void StartCombat(GameObject enemy) {
        currentEnemy = enemy;
        state = CombatState.START;
        StartCoroutine(CombatSequence());
    }

    IEnumerator CombatSequence() {
        Debug.Log("Configurando arena para pelear contra: " + currentEnemy.name);
        
        // Esperamos un segundito (en tiempo real porque el juego está pausado)
        yield return new WaitForSecondsRealtime(1f); 

        // Pasamos al turno del jugador
        state = CombatState.PLAYER_TURN;
        PlayerTurn();
    }

    void PlayerTurn() {
        Debug.Log("Turno del Jugador. Esperando acción...");
        // Aquí encenderíamos los botones de la UI de combate (Atacar, Defender, Objeto)
    }

    // Esta función la llamará el botón de "Atacar" en tu UI
    public void OnPlayerAttackButton() {
        if (state != CombatState.PLAYER_TURN) return;

        Debug.Log("El jugador eligió atacar. Iniciando QTE...");
        
        // Simulamos que ya atacó y le toca al enemigo
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine() {
        state = CombatState.ENEMY_TURN;
        
        Debug.Log("Turno del enemigo...");
        yield return new WaitForSecondsRealtime(1.5f); 
        
        Debug.Log("El enemigo atacó.");

        // Por ahora simularemos que derrotamos al enemigo:
        EndCombat(true); 
    }

    // ----- AQUÍ ESTÁN LAS MODIFICACIONES DE DROP -----
    void EndCombat(bool playerWon) {
        if (playerWon) {
            state = CombatState.WON;
            Debug.Log("¡Victoria! Has ganado el combate.");

            // Lógica de Drop de arma
            if (currentEnemy != null) {
                EnemyStats enemyStats = currentEnemy.GetComponent<EnemyStats>();
                if (enemyStats != null) {
                    WeaponData droppedWeapon = enemyStats.TryGetDrop(); // Tiramos el dado del 20%
                    
                    if (droppedWeapon != null) {
                        Debug.Log($"¡BOOYAH! El enemigo soltó un arma: {droppedWeapon.weaponName}!");
                        
                        // Buscamos a los héroes en la escena y vemos de quién es la talla del zapato xd
                        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
                        foreach(HeroStats hero in heroes) {
                            hero.TryEquipWeapon(droppedWeapon);
                        }
                    } else {
                        Debug.Log("El enemigo no soltó nada esta vez.");
                    }
                }
            }

        } else {
            state = CombatState.LOST;
            Debug.Log("Game Over...");
        }

        // Le avisamos al controlador general que el combate terminó
        if (GameFlowController.Instance != null) {
            GameFlowController.Instance.TerminarCombate();
        }
    }
}