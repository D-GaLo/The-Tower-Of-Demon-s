using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para ordenar listas fácilmente

public enum CombatState { START, WAITING_FOR_INPUT, BUSY, WON, LOST }

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;

    public CombatState state;
    private GameObject currentEnemy;

    [Header("Marcadores de Arena")]
    public Transform[] heroPositions; 
    public Transform enemyPosition;   

    [Header("UI Menús Flotantes")]
    public GameObject panelBotonesAccion; 

    public GameObject menuPrincipal;
    public GameObject menuTipoAtaque;
    public GameObject menuEspeciales;
    public Vector3 menuOffset = new Vector3(0, 1.5f, 0);

    public HeroUI[] heroesUI;

    private Vector3 enemyOriginalPosition;
    private Dictionary<GameObject, Vector3> heroOriginalPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<HeroStats, bool> heroesDefendiendo = new Dictionary<HeroStats, bool>();

    // --- NUEVO: Sistema de Turnos Dinámicos ---
    private List<GameObject> turnQueue = new List<GameObject>();
    private GameObject currentActor; // El que está atacando en este momento

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartCombat(GameObject enemy) {
        currentEnemy = enemy;
        state = CombatState.START;
        
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        StartCoroutine(CombatSequence());
    }

    IEnumerator CombatSequence() {
        Debug.Log("Teletransportando y preparando arena...");
        
        enemyOriginalPosition = currentEnemy.transform.position; 
        currentEnemy.transform.position = enemyPosition.position; 

        HeroStats[] activeHeroes = FindObjectsOfType<HeroStats>(); 
        heroOriginalPositions.Clear(); 
        heroesDefendiendo.Clear(); 

        for (int i = 0; i < activeHeroes.Length && i < heroPositions.Length; i++) {
            GameObject heroObj = activeHeroes[i].gameObject;
            heroOriginalPositions[heroObj] = heroObj.transform.position; 
            heroObj.transform.position = heroPositions[i].position;
            heroesDefendiendo[activeHeroes[i]] = false; 

            if (i < heroesUI.Length && heroesUI[i] != null) {
                heroesUI[i].ConfigurarUI(activeHeroes[i]);
            }
        }

        yield return new WaitForSecondsRealtime(1f); 

        // En lugar de ir directo al jugador, calculamos los turnos
        CalcularOrdenDeTurnos();
        AvanzarTurno();
    }

    // --- MAGIA: ORDENAR POR VELOCIDAD ---
    void CalcularOrdenDeTurnos() {
        turnQueue.Clear();
        
        // Agregamos a todos los héroes vivos
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
        foreach(var h in heroes) {
            if (h.currentHP > 0) turnQueue.Add(h.gameObject);
        }

        // Agregamos al enemigo (si está vivo)
        EnemyStats eStats = currentEnemy.GetComponent<EnemyStats>();
        if (eStats != null && eStats.currentHP > 0) {
            turnQueue.Add(currentEnemy);
        }

        // Ordenamos la lista de mayor Velocidad a menor Velocidad
        turnQueue.Sort((actorA, actorB) => GetSpeed(actorB).CompareTo(GetSpeed(actorA)));

        Debug.Log("--- NUEVA RONDA ---");
        foreach(var actor in turnQueue) {
            Debug.Log($"En la fila: {actor.name} (Velocidad: {GetSpeed(actor)})");
        }
    }

    // Función auxiliar para leer la velocidad sin importar si es héroe o enemigo
    int GetSpeed(GameObject actor) {
        HeroStats hs = actor.GetComponent<HeroStats>();
        if (hs != null) return hs.speed;
        EnemyStats es = actor.GetComponent<EnemyStats>();
        if (es != null) return es.speed;
        return 0;
    }

    // --- CONTROLADOR DE TURNOS ---
    void AvanzarTurno() {
        // 1. Revisar si el combate ya terminó
        if (RevisarVictoriaODerrota()) return;

        // 2. Si la fila se vació, volvemos a calcular una nueva ronda
        if (turnQueue.Count == 0) {
            CalcularOrdenDeTurnos();
        }

        // 3. Sacamos al primero de la fila
        currentActor = turnQueue[0];
        turnQueue.RemoveAt(0);

        // Si por alguna razón el que sigue ya está muerto, saltamos su turno
        if (EstaMuerto(currentActor)) {
            AvanzarTurno();
            return;
        }

        // 4. ¿De quién es el turno?
        HeroStats heroActor = currentActor.GetComponent<HeroStats>();
        if (heroActor != null) {
            // ES TURNO DE UN HÉROE
            state = CombatState.WAITING_FOR_INPUT;
            Debug.Log($"¡Es turno de {heroActor.unitName}! Esperando acción...");
            if (panelBotonesAccion != null){ 
                panelBotonesAccion.SetActive(true);
                MostrarMenu(menuPrincipal);
                PosicionarMenuSobreHeroe(currentActor); 
            }
        } else {
            // ES TURNO DEL ENEMIGO
            state = CombatState.BUSY;
            if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    bool EstaMuerto(GameObject actor) {
        HeroStats hs = actor.GetComponent<HeroStats>();
        if (hs != null) return hs.currentHP <= 0;
        EnemyStats es = actor.GetComponent<EnemyStats>();
        if (es != null) return es.currentHP <= 0;
        return true; 
    }

    bool RevisarVictoriaODerrota() {
        EnemyStats enemyStats = currentEnemy.GetComponent<EnemyStats>();
        if (enemyStats == null || enemyStats.currentHP <= 0) {
            EndCombat(true); // Enemigo muerto = Victoria
            return true;
        }

        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
        bool todosMuertos = true;
        foreach (HeroStats hero in heroes) {
            if (hero.currentHP > 0) { todosMuertos = false; break; }
        }
        
        if (todosMuertos) {
            EndCombat(false); // Todos los héroes muertos = Derrota
            return true;
        }
        return false;
    }

    void PosicionarMenuSobreHeroe(GameObject heroe) {
        if (Camera.main == null || panelBotonesAccion == null) return;
        
        // Convertimos la posición 3D del héroe (+ altura) a coordenadas 2D de la pantalla
        Vector3 posicionPantalla = Camera.main.WorldToScreenPoint(heroe.transform.position + menuOffset);
        
        // Movemos el panel a esa posición
        panelBotonesAccion.transform.position = posicionPantalla;
    }


    public void ActualizarPantallaVida() {
        if (heroesUI == null) return;
        foreach (HeroUI ui in heroesUI) {
            if (ui != null){ 
                ui.ActualizarVida();
                ui.ActualizarEnergia();
            }
        }
    }

    
    // --- ACCIÓN: DEFENDER ---
    public void OnPlayerDefendButton() {
        if (state != CombatState.WAITING_FOR_INPUT) return;
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        state = CombatState.BUSY;

        HeroStats hero = currentActor.GetComponent<HeroStats>();
        if (hero != null) {
            heroesDefendiendo[hero] = true; 
            Debug.Log($"¡{hero.unitName} adopta una postura defensiva!");
        }
        
        AvanzarTurno(); // Termina su turno al instante
    }

    public void OnPlayerFleeButton() {
        if (state != CombatState.WAITING_FOR_INPUT) return;
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        Debug.Log("¡Retirada táctica!");
        RestaurarPosiciones();
        if (GameFlowController.Instance != null) GameFlowController.Instance.TerminarCombate();
    }

    // --- CÁLCULO DE DAÑO JUGADOR ---
    IEnumerator PlayerAttackRoutine(int costoEnergia, float multiplicadorDano) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        EnemyStats target = currentEnemy.GetComponent<EnemyStats>();

        // Si ataca, deja de estar en postura defensiva
        heroesDefendiendo[attacker] = false;

        // Calculamos daño con el multiplicador del ataque especial
        float baseDamage = attacker.GetTotalAttack() * multiplicadorDano;
        int damage = Mathf.RoundToInt(baseDamage) - target.defense; 
        if (damage < 1) damage = 1; 

        Debug.Log($"¡{attacker.unitName} ataca al enemigo! Hizo {damage} de daño. (HP Enemigo: {target.currentHP} -> {target.currentHP - damage})");
        target.TakeDamage(damage);

        yield return new WaitForSecondsRealtime(1f); 
        ActualizarPantallaVida();
        AvanzarTurno(); // Pasamos al siguiente en la fila
    }

    // --- CÁLCULO DE DAÑO ENEMIGO ---
    IEnumerator EnemyTurnRoutine() {
        Debug.Log("Turno del enemigo...");
        yield return new WaitForSecondsRealtime(1f); 

        EnemyStats enemyAttacker = currentActor.GetComponent<EnemyStats>();
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();

        // El enemigo escoge a un héroe VIVO al azar
        List<HeroStats> heroesVivos = new List<HeroStats>();
        foreach (var hero in heroes) {
            if (hero.currentHP > 0) heroesVivos.Add(hero);
        }

        if (heroesVivos.Count > 0 && enemyAttacker != null) {
            int randomTarget = Random.Range(0, heroesVivos.Count);
            HeroStats targetHero = heroesVivos[randomTarget];

            int damage = enemyAttacker.attack - targetHero.defense;
            if (damage < 1) damage = 1;

            if (heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero]) {
                damage = damage / 2; 
                if (damage < 1) damage = 1;
                Debug.Log($"¡El enemigo ataca a {targetHero.unitName}, pero SE DEFIENDE! Recibe: {damage}.");
            } else {
                Debug.Log($"¡El enemigo ataca a {targetHero.unitName}! Recibe: {damage}.");
            }

            targetHero.TakeDamage(damage);

            ActualizarPantallaVida();
        }

        yield return new WaitForSecondsRealtime(1f);
        AvanzarTurno(); // Termina el turno del enemigo, pasamos al siguiente
    }

    void EndCombat(bool playerWon) {
        if (playerWon) {
            state = CombatState.WON;
            Debug.Log("¡Termina el combate: VICTORIA!");
            EnemyStats enemyStats = currentEnemy.GetComponent<EnemyStats>();
            if (enemyStats != null) {
                WeaponData droppedWeapon = enemyStats.TryGetDrop(); 
                if (droppedWeapon != null) {
                    Debug.Log($"¡Objeto obtenido: {droppedWeapon.weaponName}!");
                    foreach(HeroStats hero in FindObjectsOfType<HeroStats>()) hero.TryEquipWeapon(droppedWeapon); 
                }
            }
        } else {
            state = CombatState.LOST;
            Debug.Log("¡Termina el combate: DERROTA! Game Over...");
        }

        RestaurarPosiciones();
        if (GameFlowController.Instance != null) GameFlowController.Instance.TerminarCombate();
    }

    void RestaurarPosiciones() {
        foreach (var hero in heroOriginalPositions) {
             if(hero.Key != null) hero.Key.transform.position = hero.Value;
        }
    }

    // --- NUEVO: SISTEMA DE SUB-MENÚS ---
    public void MostrarMenu(GameObject menuAMostrar) {
        if (menuPrincipal != null) menuPrincipal.SetActive(false);
        if (menuTipoAtaque != null) menuTipoAtaque.SetActive(false);
        if (menuEspeciales != null) menuEspeciales.SetActive(false);

        if (menuAMostrar != null) menuAMostrar.SetActive(true);
    }

    // Botón "Atacar" del menú principal
    public void OnBotonMenuAtacar() {
        MostrarMenu(menuTipoAtaque);
    }

    // Botón "Atq. Especial"
    public void OnBotonMenuEspeciales() {
        MostrarMenu(menuEspeciales);
    }

    // Botón "Atras" (sirve para cualquier menú)
    public void OnBotonAtras() {
        MostrarMenu(menuPrincipal);
    }

    // --- ACCIÓN: ATACAR ---
    public void OnAtaqueNormal() {
        if (state != CombatState.WAITING_FOR_INPUT) return;
        panelBotonesAccion.SetActive(false);
        state = CombatState.BUSY;
        StartCoroutine(PlayerAttackRoutine(0, 1f)); // 0 costo, x1 daño
    }

    // --- BOTONES DE CORTES ESPECIALES ---
    public void OnCorteChido() {
        EjecutarAtaqueEspecial(10, 1.5f); // Gasta 10, hace x1.5 daño
    }

    public void OnCortePro() {
        EjecutarAtaqueEspecial(20, 2.0f); // Gasta 20, hace x2.0 daño
    }

    public void OnCorteLoko() {
        EjecutarAtaqueEspecial(50, 3.5f); // Gasta 50, hace x3.5 daño
    }

    void EjecutarAtaqueEspecial(int costoEnergia, float multiplicadorDano) {
        if (state != CombatState.WAITING_FOR_INPUT) return;

        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        
        // Verificamos si tiene suficiente energía
        if (attacker.UseEnergy(costoEnergia)) {
            panelBotonesAccion.SetActive(false);
            state = CombatState.BUSY;
            ActualizarPantallaVida(); // Refrescamos la barra amarilla
            StartCoroutine(PlayerAttackRoutine(costoEnergia, multiplicadorDano));
        } else {
            Debug.Log("¡No tienes suficiente energía para este corte!");
            // Opcional: Podrías hacer que el botón vibre o suene un error
        }
    }

}