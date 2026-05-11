using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para ordenar listas fácilmente
using TMPro;

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

    [Header("Sistema de Formación")]
    public GameObject panelFormacion;
    public TextMeshProUGUI[] textosFormacion; // Arrastra aquí los 3 "Texto_Nombre" de tus slots
    public List<HeroStats> listaParty = new List<HeroStats>();

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartCombat(GameObject enemy) {
        currentEnemy = enemy;
        state = CombatState.START;
        
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        
        if (listaParty.Count == 0) {
            listaParty = FindObjectsOfType<HeroStats>().ToList();
        }

        // Mostramos el menú de formación y actualizamos los nombres
        if (panelFormacion != null) {
            ActualizarTextosFormacion();
            panelFormacion.SetActive(true);
        } else {
            StartCoroutine(CombatSequence());
        }
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

    // --- NUEVAS FUNCIONES PARA LOS BOTONES QUE ORDENAN LOS HERÓES ---
    public void ActualizarTextosFormacion() {
        // Recorre los slots y les pone el nombre del héroe que está en esa posición de la lista
        for (int i = 0; i < textosFormacion.Length; i++) {
            if (i < listaParty.Count && textosFormacion[i] != null) {
                textosFormacion[i].text = listaParty[i].unitName;
            }
        }
    }

    public void MoverHeroeArriba(int index) {
        if (index <= 0 || index >= listaParty.Count) return; // No puede subir más si ya está arriba
        
        HeroStats temp = listaParty[index];
        listaParty[index] = listaParty[index - 1];
        listaParty[index - 1] = temp;
        
        ActualizarTextosFormacion();
    }

    public void MoverHeroeAbajo(int index) {
        if (index < 0 || index >= listaParty.Count - 1) return; // No puede bajar más si ya está abajo
        
        HeroStats temp = listaParty[index];
        listaParty[index] = listaParty[index + 1];
        listaParty[index + 1] = temp;
        
        ActualizarTextosFormacion();
    }

    public void ConfirmarFormacion() {
        if (panelFormacion != null) panelFormacion.SetActive(false);
        StartCoroutine(CombatSequence()); // ¡Ahora sí, a pelear!
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

    // --- LÓGICA DE DEBILIDADES Y MAESTRÍA ---
    float CalcularMultiplicadorClasePosicion(UnitStats atacante, UnitStats defensor) {
        float multiplicador = 1.0f;

        // 1. Efectividad de Clase (Ejemplo: Melee vence Rango, Rango vence Tanque, Tanque vence Melee)
        // Ajusta esto según el triángulo exacto de tu GDD
        if (atacante.unitClass == UnitClass.Melee && defensor.unitClass == UnitClass.Rango) multiplicador += 0.5f;
        else if (atacante.unitClass == UnitClass.Rango && defensor.unitClass == UnitClass.Tanque) multiplicador += 0.5f;
        else if (atacante.unitClass == UnitClass.Tanque && defensor.unitClass == UnitClass.Melee) multiplicador += 0.5f;

        // 2. Efectividad de Posición (Ejemplo: Volando recibe menos daño de Tierra)
        if (atacante.unitPosition == UnitPosition.Tierra && defensor.unitPosition == UnitPosition.Volando) multiplicador -= 0.3f;
        
        // 3. El stat de Maestría aumenta directamente el multiplicador (Ej: 10 de maestría = +10% de daño)
        float bonoMaestria = atacante.mastery * 0.01f;
        multiplicador += bonoMaestria;

        return Mathf.Max(0.1f, multiplicador); // Para asegurar que no cure al enemigo por error xdd
    }

    // --- CÁLCULO DE DAÑO JUGADOR ---

    IEnumerator PlayerAttackRoutine(int costoEnergia, float multiplicadorDano) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        EnemyStats target = currentEnemy.GetComponent<EnemyStats>();

        heroesDefendiendo[attacker] = false;

        // Calculamos debilidades y maestría
        float multiplicadorClase = CalcularMultiplicadorClasePosicion(attacker, target);
        
        // Daño Total = (Ataque * Multiplicador Ataque Especial * Multiplicador Clase) - Defensa
        float baseDamage = attacker.GetTotalAttack() * multiplicadorDano * multiplicadorClase;
        int damage = Mathf.RoundToInt(baseDamage) - target.defense; 
        if (damage < 1) damage = 1; 

        Debug.Log($"¡{attacker.unitName} ataca! (Bono Debilidad/Maestría: x{multiplicadorClase}). Hizo {damage} de daño.");
        target.TakeDamage(damage);

        yield return new WaitForSecondsRealtime(1f); 
        ActualizarPantallaVida();
        AvanzarTurno(); 
    }

    // --- CÁLCULO DE DAÑO ENEMIGO ---
    IEnumerator EnemyTurnRoutine() {
        Debug.Log("Turno del enemigo...");
        yield return new WaitForSecondsRealtime(1f); 

        EnemyStats enemyAttacker = currentActor.GetComponent<EnemyStats>();
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();

        List<HeroStats> heroesVivos = new List<HeroStats>();
        foreach (var hero in heroes) {
            if (hero.currentHP > 0) heroesVivos.Add(hero);
        }

        if (heroesVivos.Count > 0 && enemyAttacker != null) {
            HeroStats targetHero = heroesVivos[Random.Range(0, heroesVivos.Count)];

            // Asignar tecla de esquive según el GDD
            KeyCode teclaEsquive = KeyCode.Space;
            if (targetHero.unitName.Contains("Sieg")) teclaEsquive = KeyCode.E;
            else if (targetHero.unitName.Contains("Merlin")) teclaEsquive = KeyCode.R;
            else if (targetHero.unitName.Contains("Heracles")) teclaEsquive = KeyCode.T;

            Debug.Log($"¡El enemigo va a atacar a {targetHero.unitName}! Presiona {teclaEsquive} para esquivar.");

            bool terminoEsquive = false;

            // Lanzamos el QTE de defensa
            QTEManager.Instance.IniciarEsquive(teclaEsquive, (multiplicadorEsquive) => {
                // Si multiplicadorEsquive es 1.5f (Perfect), esquivó. Si es 0.5f (Failure), se lo comió.
                int damage = enemyAttacker.attack - targetHero.defense;
                if (damage < 1) damage = 1;

                if (multiplicadorEsquive >= 1.5f) {
                    Debug.Log($"¡{targetHero.unitName} ESQUIVÓ EL ATAQUE PERFECTAMENTE!");
                    damage = 0; 
                } else if (heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero]) {
                    damage = damage / 2;
                    Debug.Log($"¡{targetHero.unitName} no esquivó, pero ESTÁ DEFENDIENDO! Recibe: {damage}.");
                } else {
                    Debug.Log($"¡{targetHero.unitName} se comió el ataque! Recibe: {damage}.");
                }

                targetHero.TakeDamage(damage);
                ActualizarPantallaVida();
                terminoEsquive = true;
            });

            // Esperamos a que el jugador termine el QTE antes de avanzar de turno
            yield return new WaitUntil(() => terminoEsquive);
        }

        yield return new WaitForSecondsRealtime(1f);
        AvanzarTurno(); 
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
        // Agregamos un 3er parámetro: la cantidad de teclas que van a salir (ej. 3 letras)
        EjecutarAtaqueEspecial(10, 1.5f, 3); 
    }

    public void OnCortePro() {
        EjecutarAtaqueEspecial(20, 2.0f, 4); // Secuencia de 4 letras
    }

    public void OnCorteLoko() {
        EjecutarAtaqueEspecial(50, 3.5f, 6); // Secuencia de 6 letras, más difícil
    }

    void EjecutarAtaqueEspecial(int costoEnergia, float multiplicadorDanoBase, int longitudSecuencia) {
        if (state != CombatState.WAITING_FOR_INPUT) return;

        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        
        // Verificamos si tiene suficiente energía
        if (attacker.UseEnergy(costoEnergia)) {
            panelBotonesAccion.SetActive(false);
            state = CombatState.BUSY;
            ActualizarPantallaVida(); // Refrescamos la barra amarilla de la interfaz
            
            Debug.Log("¡Iniciando secuencia QTE!");

            
            // Llamamos al QTEManager. Cuando el jugador termine (falle o acierte), 
            // el QTEManager nos va a devolver un 'multiplicadorQTE' (ej. 1.5 si fue Perfecto, 0.5 si falló)
            QTEManager.Instance.IniciarQTE(longitudSecuencia, (multiplicadorQTE) => {     
                Debug.Log($"QTE Terminado con multiplicador: {multiplicadorQTE}"); 
                float multiplicadorFinal = multiplicadorDanoBase * multiplicadorQTE;
                StartCoroutine(PlayerAttackRoutine(costoEnergia, multiplicadorFinal));
            });

        } else {
            Debug.Log("¡No tienes suficiente energía para este corte!");
        }
    }
}