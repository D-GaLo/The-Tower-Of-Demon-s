using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CombatState { START, PLAYER_TURN, ENEMY_TURN, WON, LOST }

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;

    public CombatState state;
    private GameObject currentEnemy;

    [Header("Marcadores de Arena")]
    public Transform[] heroPositions; 
    public Transform enemyPosition;   

    [Header("UI Específica")]
    [Tooltip("Arrastra aquí el Panel que contiene los botones de Atacar, Defender, etc.")]
    public GameObject panelBotonesAccion; 

    private Vector3 enemyOriginalPosition;
    private Dictionary<GameObject, Vector3> heroOriginalPositions = new Dictionary<GameObject, Vector3>();

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartCombat(GameObject enemy) {
        currentEnemy = enemy;
        state = CombatState.START;
        
        // Escondemos los botones mientras se configura todo
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        
        StartCoroutine(CombatSequence());
    }

    IEnumerator CombatSequence() {
        Debug.Log("Configurando arena para pelear contra: " + currentEnemy.name);
        
        enemyOriginalPosition = currentEnemy.transform.position; 
        currentEnemy.transform.position = enemyPosition.position; 

        HeroStats[] activeHeroes = FindObjectsOfType<HeroStats>(); 
        heroOriginalPositions.Clear(); 

        for (int i = 0; i < activeHeroes.Length && i < heroPositions.Length; i++) {
            GameObject heroObj = activeHeroes[i].gameObject;
            heroOriginalPositions[heroObj] = heroObj.transform.position; 
            heroObj.transform.position = heroPositions[i].position;      
        }

        yield return new WaitForSecondsRealtime(1f); 

        state = CombatState.PLAYER_TURN;
        PlayerTurn();
    }

    void PlayerTurn() {
        Debug.Log("Turno del Jugador. Esperando acción...");
        // ¡Aquí encendemos los botones para que puedas decidir!
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(true);
    }

    public void OnPlayerAttackButton() {
        if (state != CombatState.PLAYER_TURN) return;

        // ¡Apagamos los botones en cuanto atacas para que no hagas doble clic!
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);

        Debug.Log("Iniciando secuencia de ataque del jugador...");
        StartCoroutine(PlayerAttackRoutine());
    }

    public void OnPlayerDefendButton() {
        if (state != CombatState.PLAYER_TURN) return;

        // Apagamos TODO el panel de botones
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);

        Debug.Log("El jugador se defiende. Pasa el turno al enemigo.");
        
        // Aquí luego le pondremos una "bandera" al héroe para que reciba la mitad de daño
        // Por ahora, solo pasa el turno al enemigo
        StartCoroutine(EnemyTurnRoutine());
    }

    public void OnPlayerFleeButton() {
        if (state != CombatState.PLAYER_TURN) return;

        // Apagamos los botones
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);

        Debug.Log("¡El jugador usó la retirada táctica! Terminando combate...");

        // 1. Devolvemos a los héroes a donde estaban caminando en el mapa
        foreach (var hero in heroOriginalPositions) {
             if(hero.Key != null) hero.Key.transform.position = hero.Value;
        }

        // 2. Le avisamos al GameFlowController que haga la transición de regreso
        if (GameFlowController.Instance != null) {
            GameFlowController.Instance.TerminarCombate();
        }
    }

    // --- NUEVA LÓGICA DE ATAQUE DEL JUGADOR ---
    IEnumerator PlayerAttackRoutine() {
        // Obtenemos los stats
        EnemyStats enemyStats = currentEnemy.GetComponent<EnemyStats>();
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();

        // Simulamos que el primer héroe (Sieg) ataca. (Más adelante puedes hacer que ataquen los 3).
        if (heroes.Length > 0 && enemyStats != null) {
            int damage = heroes[0].GetTotalAttack() - enemyStats.defense; // Fórmula básica de daño
            if (damage < 1) damage = 1; // Mínimo hacemos 1 de daño

            Debug.Log($"¡{heroes[0].unitName} ataca! Hizo {damage} de daño.");
            enemyStats.TakeDamage(damage);
        }

        yield return new WaitForSecondsRealtime(1f); 

        // Verificamos si ganamos
        if (enemyStats != null && enemyStats.currentHP <= 0) {
            EndCombat(true); // ¡Ganamos!
        } else {
            StartCoroutine(EnemyTurnRoutine()); // Si sobrevive, le toca al enemigo
        }
    }

    // --- NUEVA LÓGICA DE ATAQUE DEL ENEMIGO ---
    IEnumerator EnemyTurnRoutine() {
        state = CombatState.ENEMY_TURN;
        Debug.Log("Turno del enemigo...");
        yield return new WaitForSecondsRealtime(1f); 

        EnemyStats enemyStats = currentEnemy.GetComponent<EnemyStats>();
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();

        // El enemigo ataca a un héroe al azar
        if (heroes.Length > 0 && enemyStats != null) {
            int randomTarget = Random.Range(0, heroes.Length);
            HeroStats targetHero = heroes[randomTarget];

            int damage = enemyStats.attack - targetHero.defense;
            if (damage < 1) damage = 1;

            Debug.Log($"¡El enemigo ataca a {targetHero.unitName} y hace {damage} de daño!");
            targetHero.TakeDamage(damage);
        }

        yield return new WaitForSecondsRealtime(1f);

        // Verificamos si todos los héroes murieron (Game Over)
        bool todosMuertos = true;
        foreach (HeroStats hero in heroes) {
            if (hero.currentHP > 0) {
                todosMuertos = false; // Alguien sigue vivo
                break;
            }
        }

        if (todosMuertos) {
            EndCombat(false); // Perdimos
        } else {
            state = CombatState.PLAYER_TURN;
            PlayerTurn(); // Regresamos el turno al jugador
        }
    }

    void EndCombat(bool playerWon) {
        if (playerWon) {
            state = CombatState.WON;
            Debug.Log("¡Victoria! Has ganado el combate.");

            if (currentEnemy != null) {
                EnemyStats enemyStats = currentEnemy.GetComponent<EnemyStats>();
                if (enemyStats != null) {
                    WeaponData droppedWeapon = enemyStats.TryGetDrop(); 
                    if (droppedWeapon != null) {
                        Debug.Log($"¡BOOYAH! El enemigo soltó un arma: {droppedWeapon.weaponName}!");
                        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
                        foreach(HeroStats hero in heroes) {
                            hero.TryEquipWeapon(droppedWeapon); 
                        }
                    }
                }
            }
        } else {
            state = CombatState.LOST;
            Debug.Log("Game Over...");
            // Aquí llamarías a tu pantalla de reinicio
        }

        // Devolver a los héroes a donde estaban caminando
        foreach (var hero in heroOriginalPositions) {
             if(hero.Key != null) hero.Key.transform.position = hero.Value;
        }

        if (GameFlowController.Instance != null) {
            GameFlowController.Instance.TerminarCombate();
        }
    }
}