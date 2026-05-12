using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public enum CombatState { START, WAITING_FOR_INPUT, BUSY, WON, LOST }

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;

    public CombatState state;
    public List<GameObject> activeEnemies = new List<GameObject>();

    [Header("Marcadores de Arena")]
    public Transform[] heroPositions; 
    public Transform[] enemyPositions;   

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

    private List<GameObject> turnQueue = new List<GameObject>();
    private GameObject currentActor; 

    [Header("Sistema de Formación")]
    public GameObject panelFormacion;
    public TextMeshProUGUI[] textosFormacion; 
    public List<HeroStats> listaParty = new List<HeroStats>();

    [Header("Guía de Fortalezas")]
    public GameObject panelGuiaFortalezas;

    [Header("Menú Selección Enemigo")]
    public GameObject menuSeleccionEnemigo;
    public UnityEngine.UI.Button[] botonesSeleccionEnemigo; 
    public TextMeshProUGUI[] textosBotonesEnemigo; 

    // Variables para "guardar" el ataque mientras el jugador elige a la víctima
    private int ataquePendienteCosto;
    private float ataquePendienteMultiplicador;
    private int ataquePendienteSecuencia;
    private bool ataquePendienteEsEspecial;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartCombat(GameObject enemy) {
        activeEnemies.Clear();
        activeEnemies.Add(enemy);
        
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        
        if (listaParty.Count == 0) {
            listaParty = FindObjectsOfType<HeroStats>().ToList();
        }

        if (panelFormacion != null) {
            ActualizarTextosFormacion();
            panelFormacion.SetActive(true);
        } else {
            StartCoroutine(CombatSequence());
        }
    }

    IEnumerator CombatSequence() {
        Debug.Log("Teletransportando según formación elegida...");
        
        enemyOriginalPosition = activeEnemies[0].transform.position;
        int enemigosExtra = Random.Range(0, 3); // Spawnea de 1 a 3
        for(int i = 0; i < enemigosExtra; i++) {
            GameObject clon = Instantiate(activeEnemies[0]);
            clon.name = activeEnemies[0].name + " " + (i + 2); 
            clon.GetComponent<EnemyStats>().unitName = clon.name;
            activeEnemies.Add(clon);
        }

        for(int i = 0; i < activeEnemies.Count && i < enemyPositions.Length; i++) {
            activeEnemies[i].transform.position = enemyPositions[i].position;
            EnemyStats statsEnemigo = activeEnemies[i].GetComponent<EnemyStats>();
            if (statsEnemigo != null) statsEnemigo.ActivarUICombate();
        }

        heroOriginalPositions.Clear(); 
        heroesDefendiendo.Clear(); 

        for (int i = 0; i < listaParty.Count && i < heroPositions.Length; i++) {
            GameObject heroObj = listaParty[i].gameObject;
            
            heroOriginalPositions[heroObj] = heroObj.transform.position; 
            heroObj.transform.position = heroPositions[i].position;
            heroesDefendiendo[listaParty[i]] = false; 

            if (i < heroesUI.Length && heroesUI[i] != null) {
                heroesUI[i].ConfigurarUI(listaParty[i]);
            }
        }

        yield return new WaitForSecondsRealtime(1f); 

        CalcularOrdenDeTurnos();
        AvanzarTurno();
    }

    public void ActualizarTextosFormacion() {
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
        StartCoroutine(CombatSequence()); 
    }

    void CalcularOrdenDeTurnos() {
        turnQueue.Clear();
        
        // Agregamos a todos los héroes vivos
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
        foreach(var h in heroes) {
            if (h.currentHP > 0) turnQueue.Add(h.gameObject);
        }

        foreach(var enemyObj in activeEnemies) {
            EnemyStats eStats = enemyObj.GetComponent<EnemyStats>();
            if (eStats != null && eStats.currentHP > 0) turnQueue.Add(enemyObj);
        }

        // Ordenamos la lista de mayor Velocidad a menor Velocidad
        turnQueue.Sort((actorA, actorB) => GetSpeed(actorB).CompareTo(GetSpeed(actorA)));

        Debug.Log("--- NUEVA RONDA ---");
        foreach(var actor in turnQueue) {
            Debug.Log($"En la fila: {actor.name} (Velocidad: {GetSpeed(actor)})");
        }
    }

    // Función auxiliar para leer la velocidad 
    int GetSpeed(GameObject actor) {
        HeroStats hs = actor.GetComponent<HeroStats>();
        if (hs != null) return hs.speed;
        EnemyStats es = actor.GetComponent<EnemyStats>();
        if (es != null) return es.speed;
        return 0;
    }

    void AvanzarTurno() {
        if (RevisarVictoriaODerrota()) return;
        if (turnQueue.Count == 0) {
            CalcularOrdenDeTurnos();
        }

        currentActor = turnQueue[0];
        turnQueue.RemoveAt(0);

        if (EstaMuerto(currentActor)) {
            AvanzarTurno();
            return;
        }

        HeroStats heroActor = currentActor.GetComponent<HeroStats>();
        if (heroActor != null) {
            state = CombatState.WAITING_FOR_INPUT;
            Debug.Log($"¡Es turno de {heroActor.unitName}! Esperando acción...");
            if (panelBotonesAccion != null){ 
                panelBotonesAccion.SetActive(true);
                MostrarMenu(menuPrincipal);
                PosicionarMenuSobreHeroe(currentActor); 
            }
        } else {
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
        bool todosEnemigosMuertos = true;
        foreach(var enemyObj in activeEnemies) {
            EnemyStats eStats = enemyObj.GetComponent<EnemyStats>();
            if (eStats != null && eStats.currentHP > 0) {
                todosEnemigosMuertos = false;
                break;
            }
        }

        if (todosEnemigosMuertos) {
            EndCombat(true); 
            return true;
        }

        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
        bool todosMuertos = true;
        foreach (HeroStats hero in heroes) {
            if (hero.currentHP > 0) { todosMuertos = false; break; }
        }
        
        if (todosMuertos) {
            EndCombat(false); 
            return true;
        }
        return false;
    }

    void PosicionarMenuSobreHeroe(GameObject heroe) {
        if (Camera.main == null || panelBotonesAccion == null) return;
        
        Vector3 posicionPantalla = Camera.main.WorldToScreenPoint(heroe.transform.position + menuOffset);
        
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

    
    public void OnPlayerDefendButton() {
        if (state != CombatState.WAITING_FOR_INPUT) return;
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        state = CombatState.BUSY;

        HeroStats hero = currentActor.GetComponent<HeroStats>();
        if (hero != null) {
            heroesDefendiendo[hero] = true; 
            Debug.Log($"¡{hero.unitName} adopta una postura defensiva!");
        }
        
        AvanzarTurno(); 
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

    IEnumerator PlayerAttackRoutine(GameObject targetEnemyObj, float multiplicadorDano) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        EnemyStats target = targetEnemyObj.GetComponent<EnemyStats>();

        heroesDefendiendo[attacker] = false;

        float multiplicadorClase = CalcularMultiplicadorClasePosicion(attacker, target);
        
        float baseDamage = attacker.GetTotalAttack() * multiplicadorDano * multiplicadorClase;
        int damage = Mathf.RoundToInt(baseDamage) - target.defense; 
        if (damage < 1) damage = 1; 

        Debug.Log($"¡{attacker.unitName} ataca a {target.unitName}! (Bono x{multiplicadorClase}). Hizo {damage} de daño.");
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
            EnemyStats enemyStats = activeEnemies[0].GetComponent<EnemyStats>();
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
        if (menuSeleccionEnemigo != null) menuSeleccionEnemigo.SetActive(false);

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
        PrepararAtaque(0, 1f, 0, false); // Normal no gasta energía ni usa QTE
    }

    public void OnCorteChido() { PrepararAtaque(10, 1.5f, 3, true); }
    public void OnCortePro() { PrepararAtaque(20, 2.0f, 4, true); }
    public void OnCorteLoko() { PrepararAtaque(50, 3.5f, 6, true); }

    void PrepararAtaque(int costo, float mult, int seqLen, bool esEspecial) {
        if (state != CombatState.WAITING_FOR_INPUT) return;

        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        
        // Revisamos si tiene energía ANTES de abrir el menú de selección
        if (esEspecial && attacker.currentEnergy < costo) {
            Debug.Log("¡No tienes suficiente energía para este corte!");
            return;
        }

        // Guardamos los datos del ataque en la memoria
        ataquePendienteCosto = costo;
        ataquePendienteMultiplicador = mult;
        ataquePendienteSecuencia = seqLen;
        ataquePendienteEsEspecial = esEspecial;

        // Actualizamos los botones para que solo muestren enemigos vivos
        for (int i = 0; i < botonesSeleccionEnemigo.Length; i++) {
            if (i < activeEnemies.Count && activeEnemies[i] != null && activeEnemies[i].GetComponent<EnemyStats>().currentHP > 0) {
                botonesSeleccionEnemigo[i].gameObject.SetActive(true);
                textosBotonesEnemigo[i].text = activeEnemies[i].GetComponent<EnemyStats>().unitName;
            } else {
                botonesSeleccionEnemigo[i].gameObject.SetActive(false); // Apagamos el botón si no hay enemigo o está muerto
            }
        }

        MostrarMenu(menuSeleccionEnemigo);
    }

    // --- ACCIÓN: CONFIRMAR EL OBJETIVO Y ATACAR ---
    public void ConfirmarAtaqueAEnemigo(int enemyIndex) {
        if (state != CombatState.WAITING_FOR_INPUT) return;

        // Seleccionamos al enemigo de la lista según el botón que presionó
        GameObject targetEnemyObj = activeEnemies[enemyIndex];
        HeroStats attacker = currentActor.GetComponent<HeroStats>();

        // Ahora sí, descontamos la energía
        if (ataquePendienteEsEspecial) {
            attacker.UseEnergy(ataquePendienteCosto);
            ActualizarPantallaVida();
        }

        panelBotonesAccion.SetActive(false);
        state = CombatState.BUSY;

        if (ataquePendienteEsEspecial) {
            Debug.Log("¡Iniciando secuencia QTE!");
            QTEManager.Instance.IniciarQTE(ataquePendienteSecuencia, (multiplicadorQTE) => {     
                float multiplicadorFinal = ataquePendienteMultiplicador * multiplicadorQTE;
                StartCoroutine(PlayerAttackRoutine(targetEnemyObj, multiplicadorFinal));
            });
        } else {
            // Ataque normal directo
            StartCoroutine(PlayerAttackRoutine(targetEnemyObj, 1f));
        }
    }

    public void ToggleGuiaFortalezas() {
        if (panelGuiaFortalezas != null) {
            panelGuiaFortalezas.SetActive(!panelGuiaFortalezas.activeSelf);
        }
    }
}