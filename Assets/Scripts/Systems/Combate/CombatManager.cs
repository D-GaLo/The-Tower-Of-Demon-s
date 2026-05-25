using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public enum CombatState { START, WAITING_FOR_INPUT, BUSY, WON, LOST }

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;

    public CombatState state;
    private bool enCombateActivo = false; 
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

    [Header("UI de textos Ataques Especiales")]
    public TextMeshProUGUI textoEspecial1;
    public TextMeshProUGUI textoEspecial2;
    public TextMeshProUGUI textoEspecial3;

    private bool ataquePendienteEsAoE = false;

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

    [Header("Paneles de Fin de Combate")]
    public GameObject panelVictoria;
    public TextMeshProUGUI textoVictoriaXP;
    public TextMeshProUGUI textoVictoriaNivel;
    public TextMeshProUGUI textoVictoriaObjeto;

    public GameObject panelDerrota;


    private int ataquePendienteCosto;
    private float ataquePendienteMultiplicador;
    private int ataquePendienteSecuencia;
    private bool ataquePendienteEsEspecial;

    [Header("UI Especial Jefes")]
    public GameObject panelTituloJefe;
    public TextMeshProUGUI textoTituloJefe;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void StartCombat(GameObject enemy, bool ventajaJugador = false) {
        if (enCombateActivo) return;
        enCombateActivo = true;
        
        activeEnemies.Clear();
        activeEnemies.Add(enemy);
        
        if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
        
        if (listaParty.Count == 0) {
            listaParty = FindObjectsOfType<HeroStats>().ToList();
        }

        listaParty = listaParty.OrderBy(h => 
            h.unitName == "Sieg" ? 0 : 
            h.unitName == "Merlin" ? 1 : 2
        ).ToList();

        StartCoroutine(PrepararArenaYMostrarFormacion(ventajaJugador));
    }

    void EndCombat(bool playerWon) {
        enCombateActivo = false; 
        
        if (playerWon) {
            state = CombatState.WON;
            Debug.Log("¡Termina el combate: VICTORIA!");

            if (AudioManager.Instance != null) AudioManager.Instance.PlayVictoria();

            int xpDeEstaPelea = 0;
            foreach (GameObject enemyObj in activeEnemies) {
                if (enemyObj != null) {
                    EnemyStats eStats = enemyObj.GetComponent<EnemyStats>();
                    if (eStats != null) xpDeEstaPelea += (eStats.level * 20) + 30;
                }
            }

            if (textoVictoriaXP != null) textoVictoriaXP.text = $"XP Obtenida: {xpDeEstaPelea}";
            string recuentoNiveles = "";

            foreach (HeroStats heroe in listaParty) {
                if (heroe != null && heroe.currentHP > 0) { 
                    
                    int nivelViejo = heroe.level;
                    int hpViejo = heroe.maxHP;
                    int atqViejo = heroe.attack;
                    int defViejo = heroe.defense;

                    heroe.GanarExperiencia(xpDeEstaPelea);
                    
                    if (heroe.level > nivelViejo) {
                        recuentoNiveles += $"<color=yellow>¡{heroe.unitName} alcanzó el Nivel {heroe.level}!</color>\n";
                        recuentoNiveles += $"HP: {hpViejo} -> {heroe.maxHP} | Atq: {atqViejo} -> {heroe.attack} | Def: {defViejo} -> {heroe.defense}\n\n";
                    }

                    HeroUI ui = heroe.GetComponent<HeroUI>();
                    if (ui != null) ui.ActualizarNivelYXP();
                }
            }

            if (textoVictoriaNivel != null) {
                textoVictoriaNivel.text = string.IsNullOrEmpty(recuentoNiveles) ? "Los héroes ganaron experiencia." : recuentoNiveles;
            }

            EnemyStats enemyStats = activeEnemies[0].GetComponent<EnemyStats>();
            if (enemyStats != null) {
                WeaponData droppedWeapon = enemyStats.TryGetDrop(); 
                if (droppedWeapon != null && textoVictoriaObjeto != null) {
                    textoVictoriaObjeto.text = $"¡Objeto obtenido: {droppedWeapon.weaponName}!";
                    foreach(HeroStats hero in FindObjectsOfType<HeroStats>()) hero.TryEquipWeapon(droppedWeapon); 
                } else if (textoVictoriaObjeto != null) {
                    textoVictoriaObjeto.text = "Ningún objeto obtenido.";
                }
            }

            if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
            if (panelVictoria != null) panelVictoria.SetActive(true);

        } else {
            state = CombatState.LOST;
            Debug.Log("¡Termina el combate: DERROTA! Game Over...");

            if (AudioManager.Instance != null) AudioManager.Instance.PlayDerrota();
            
            if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
            if (panelDerrota != null) panelDerrota.SetActive(true);
        }

        RestaurarPosiciones();
    }

    IEnumerator PrepararArenaYMostrarFormacion(bool ventajaJugador) {
        Debug.Log("Spawneando enemigos para que el jugador los analice...");
        
        EnemyStats statsEnemigoPrincipal = activeEnemies[0].GetComponent<EnemyStats>();
        enemyOriginalPosition = activeEnemies[0].transform.position;
        
        activeEnemies[0].name = activeEnemies[0].name.Replace("(Clone)", "").Trim();
        
        if (string.IsNullOrEmpty(statsEnemigoPrincipal.unitName)) {
            statsEnemigoPrincipal.unitName = activeEnemies[0].name;
        }

        if (!statsEnemigoPrincipal.nivelManual && !statsEnemigoPrincipal.esJefe) {
            
            int nivelMinimoGrupo = int.MaxValue;
            foreach (HeroStats heroe in listaParty) {
                if (heroe.level < nivelMinimoGrupo) nivelMinimoGrupo = heroe.level;
            }
            
            if (nivelMinimoGrupo == int.MaxValue) nivelMinimoGrupo = 1;

            int dado = Random.Range(1, 101);
            int nivelCalculado = nivelMinimoGrupo;

            if (dado <= 65) {
                nivelCalculado = nivelMinimoGrupo;
            } 
            else if (dado <= 85) {
                nivelCalculado = nivelMinimoGrupo - 1;
            } 
            else {
                nivelCalculado = nivelMinimoGrupo + 1;
            }

            if (nivelCalculado < 1) nivelCalculado = 1;
            if (nivelCalculado > 10) nivelCalculado = 10;

            statsEnemigoPrincipal.level = nivelCalculado;
            statsEnemigoPrincipal.EscalarEstadisticas();
        }

        DesactivarAILocal(activeEnemies[0]);

        if (!statsEnemigoPrincipal.esJefe) {
            int enemigosExtra = Random.Range(0, 3); 
            
            int limiteRestante = enemyPositions.Length - activeEnemies.Count;
            if (enemigosExtra > limiteRestante) enemigosExtra = limiteRestante;

            for(int i = 0; i < enemigosExtra; i++) {
                GameObject prefabAInstanciar = activeEnemies[0]; 

                if (statsEnemigoPrincipal.posiblesCompaneros != null && statsEnemigoPrincipal.posiblesCompaneros.Length > 0) {
                    int randomIndex = Random.Range(0, statsEnemigoPrincipal.posiblesCompaneros.Length);
                    prefabAInstanciar = statsEnemigoPrincipal.posiblesCompaneros[randomIndex];
                }

                GameObject clon = Instantiate(prefabAInstanciar);
                clon.name = prefabAInstanciar.name.Replace("(Clone)", "").Trim(); 
                
                DesactivarAILocal(clon); 
                
                EnemyStats statsClon = clon.GetComponent<EnemyStats>();
                
                if (string.IsNullOrEmpty(statsClon.unitName)) {
                    statsClon.unitName = clon.name;
                }
                
                statsClon.level = statsEnemigoPrincipal.level;
                statsClon.nivelManual = false; 
                statsClon.esJefe = false; 
                statsClon.EscalarEstadisticas();

                activeEnemies.Add(clon);
            }
        }

        for(int i = 0; i < activeEnemies.Count && i < enemyPositions.Length; i++) {
            activeEnemies[i].transform.position = enemyPositions[i].position;
            
            Vector3 escalaLocal = activeEnemies[i].transform.localScale;
            activeEnemies[i].transform.localScale = new Vector3(-Mathf.Abs(escalaLocal.x), escalaLocal.y, escalaLocal.z);

            EnemyStats statsEnemigo = activeEnemies[i].GetComponent<EnemyStats>();
            if (statsEnemigo != null) {
                statsEnemigo.ActivarUICombate();
                if (statsEnemigo.canvasUI != null) {
                    Vector3 escalaCanvas = statsEnemigo.canvasUI.transform.localScale;
                    statsEnemigo.canvasUI.transform.localScale = new Vector3(-Mathf.Abs(escalaCanvas.x), escalaCanvas.y, escalaCanvas.z);
                }
            }
        }

        if (ventajaJugador) {
            Debug.Log("<color=green>¡ATAQUE SORPRESA!</color> Los enemigos inician con menos vida.");
            foreach(GameObject obj in activeEnemies) {
                EnemyStats eStats = obj.GetComponent<EnemyStats>();
                if (eStats != null) {
                    float porcentaje = eStats.esJefe ? 0.10f : 0.20f;
                    int damage = Mathf.RoundToInt(eStats.maxHP * porcentaje);
                    eStats.currentHP -= damage;
                    if (eStats.currentHP < 1) eStats.currentHP = 1; 
                    eStats.ActualizarVisuales();
                }
            }
        } else {
            Debug.Log("<color=red>¡EMBOSCADA!</color> Los héroes inician con -20% de vida.");
            foreach(HeroStats hStats in listaParty) {
                if (hStats != null && hStats.currentHP > 0) {
                    int damage = Mathf.RoundToInt(hStats.maxHP * 0.20f);
                    hStats.currentHP -= damage;
                    if (hStats.currentHP < 1) hStats.currentHP = 1;
                }
            }
        }

        heroOriginalPositions.Clear(); 
        heroesDefendiendo.Clear(); 

        foreach (HeroStats heroe in listaParty) {
            heroOriginalPositions[heroe.gameObject] = heroe.transform.position;
            heroesDefendiendo[heroe] = false;
        }

        for (int i = 0; i < listaParty.Count && i < heroPositions.Length; i++) {
            GameObject heroObj = listaParty[i].gameObject;
            if (heroObj.GetComponent<HeroStats>().unitName == "Sieg") {
                heroObj.transform.position = heroPositions[i].position;
            }
        }

        for (int i = 0; i < listaParty.Count && i < heroPositions.Length; i++) {
            GameObject heroObj = listaParty[i].gameObject;
            if (heroObj.GetComponent<HeroStats>().unitName != "Sieg") {
                heroObj.transform.position = heroPositions[i].position;
            }
            
            if (i < heroesUI.Length && heroesUI[i] != null) {
                heroesUI[i].ConfigurarUI(listaParty[i]);
            }
        }

        ActualizarPantallaVida(); 


        if (statsEnemigoPrincipal.esJefe) {
            if (panelTituloJefe != null) {
                panelTituloJefe.SetActive(true);
                if (textoTituloJefe != null) {
                    
                    string tituloImponente = "";
                    
                    if (statsEnemigoPrincipal.unitName.Contains("Nucifera")) {
                        tituloImponente = "<color=#8B0000><size=130%><b>NUCIFERA</b></size></color>\n<size=50%><i><color=#FF4500>Guardián del Primer Piso</color></i></size>";
                    } 
                    else if (statsEnemigoPrincipal.unitName.Contains("Slime")) {
                        tituloImponente = "<color=#006400><size=130%><b>SLIME PADRE</b></size></color>\n<size=50%><i><color=#32CD32>La Pesadilla Gelatinosa</color></i></size>";
                    } 
                    else {
                        tituloImponente = $"<color=red><size=130%><b>{statsEnemigoPrincipal.unitName.ToUpper()}</b></size></color>";
                    }

                    textoTituloJefe.text = tituloImponente;
                }
            }
            yield return new WaitForSecondsRealtime(3.0f);
            if (panelTituloJefe != null) panelTituloJefe.SetActive(false);
        }

        yield return new WaitForSecondsRealtime(0.5f); 

        if (panelFormacion != null) panelFormacion.SetActive(true);
        else ConfirmarFormacion(); 
    } 

    IEnumerator RevisarInvocacionJefe(EnemyStats jefe) {
        if (jefe.esJefe && !jefe.yaInvoco && jefe.currentHP > 0 && jefe.currentHP <= (jefe.maxHP / 2)) {
            jefe.yaInvoco = true;
            Debug.Log($"<color=red>¡ALERTA!</color> ¡{jefe.unitName} se ha enfurecido e invoca refuerzos!");
            
            if (jefe.enemigosAInvocar != null && jefe.enemigosAInvocar.Length > 0) {
                
                List<int> huecosLibres = new List<int>();
                for (int i = 0; i < enemyPositions.Length; i++) {
                    bool ocupado = false;
                    foreach (var enemy in activeEnemies) {
                        if (enemy.activeSelf && Vector3.Distance(enemy.transform.position, enemyPositions[i].position) < 0.5f) {
                            ocupado = true;
                            break;
                        }
                    }
                    if (!ocupado) huecosLibres.Add(i);
                }

                int cantidadAInvocar = Mathf.Min(huecosLibres.Count, jefe.enemigosAInvocar.Length);

                for (int i = 0; i < cantidadAInvocar; i++) {
                    GameObject prefab = jefe.enemigosAInvocar[i];
                    GameObject clon = Instantiate(prefab);
                    clon.name = prefab.name; 
                    
                    DesactivarAILocal(clon); 
                    
                    EnemyStats statsClon = clon.GetComponent<EnemyStats>();
                    
                    if (string.IsNullOrEmpty(statsClon.unitName)) {
                        statsClon.unitName = clon.name;
                    }
                    
                    statsClon.level = jefe.level; 
                    statsClon.nivelManual = false;
                    statsClon.esJefe = false; 
                    statsClon.EscalarEstadisticas();

                    activeEnemies.Add(clon);
                    turnQueue.Add(clon); 

                    int posIndex = huecosLibres[i];
                    clon.transform.position = enemyPositions[posIndex].position;

                    Vector3 escalaLocal = clon.transform.localScale;
                    clon.transform.localScale = new Vector3(-Mathf.Abs(escalaLocal.x), escalaLocal.y, escalaLocal.z);

                    statsClon.ActivarUICombate();
                    if (statsClon.canvasUI != null) {
                        Vector3 escalaCanvas = statsClon.canvasUI.transform.localScale;
                        statsClon.canvasUI.transform.localScale = new Vector3(-Mathf.Abs(escalaCanvas.x), escalaCanvas.y, escalaCanvas.z);
                    }
                }
                
                if (cantidadAInvocar > 0) yield return new WaitForSecondsRealtime(1.5f);
            }
        }
    }



    public void SeleccionarPosicion(string nombreHeroe_Y_Posicion) {
        string[] datos = nombreHeroe_Y_Posicion.Split('_');
        string heroeObjetivo = datos[0];
        string posElegida = datos[1];

        HeroStats heroe = listaParty.Find(h => h.unitName == heroeObjetivo);
        if (heroe != null) {
            if (posElegida == "Tierra") heroe.unitPosition = UnitPosition.Tierra;
            else if (posElegida == "BajoTierra") heroe.unitPosition = UnitPosition.BajoTierra;
            else if (posElegida == "Volando") heroe.unitPosition = UnitPosition.Volando;
            
            HeroUI ui = System.Array.Find(heroesUI, u => u.nombreText.text == heroe.unitName);
            if (ui != null) ui.ConfigurarUI(heroe);
        }
    }

    public void ConfirmarFormacion() {
        if (panelFormacion != null) panelFormacion.SetActive(false);
        CalcularOrdenDeTurnos();
        AvanzarTurno();
    }

    void CalcularOrdenDeTurnos() {
        turnQueue.Clear();
        
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
        foreach(var h in heroes) {
            if (h.currentHP > 0) turnQueue.Add(h.gameObject);
        }

        foreach(var enemyObj in activeEnemies) {
            EnemyStats eStats = enemyObj.GetComponent<EnemyStats>();
            if (eStats != null && eStats.currentHP > 0) turnQueue.Add(enemyObj);
        }

        turnQueue.Sort((actorA, actorB) => GetSpeed(actorB).CompareTo(GetSpeed(actorA)));

        Debug.Log("--- NUEVA RONDA ---");
        foreach(var actor in turnQueue) {
            Debug.Log($"En la fila: {actor.name} (Velocidad: {GetSpeed(actor)})");
        }
    }

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
        
        enCombateActivo = false; 
        
        RestaurarPosiciones();

        if (GameFlowController.Instance != null) GameFlowController.Instance.TerminarCombate(false, true);
    }

    float CalcularMultiplicadorClasePosicion(UnitStats atacante, UnitStats defensor) {
        float multiplicador = 1.0f;

        if (atacante.unitClass == UnitClass.Melee && defensor.unitClass == UnitClass.Rango) { multiplicador += 0.15f; }
        else if (atacante.unitClass == UnitClass.Rango && defensor.unitClass == UnitClass.Tanque) { multiplicador += 0.15f; }
        else if (atacante.unitClass == UnitClass.Tanque && defensor.unitClass == UnitClass.Melee) { multiplicador += 0.15f; }
        // Desventajas de Clase
        else if (atacante.unitClass == UnitClass.Rango && defensor.unitClass == UnitClass.Melee) { multiplicador -= 0.15f; }
        else if (atacante.unitClass == UnitClass.Tanque && defensor.unitClass == UnitClass.Rango) { multiplicador -= 0.15f; }
        else if (atacante.unitClass == UnitClass.Melee && defensor.unitClass == UnitClass.Tanque) { multiplicador -= 0.15f; }


        if (atacante.unitPosition == UnitPosition.Volando && defensor.unitPosition == UnitPosition.Tierra) { multiplicador += 0.15f; }
        else if (atacante.unitPosition == UnitPosition.Tierra && defensor.unitPosition == UnitPosition.BajoTierra) { multiplicador += 0.15f; }
        else if (atacante.unitPosition == UnitPosition.BajoTierra && defensor.unitPosition == UnitPosition.Volando) { multiplicador += 0.15f; }
        // Desventajas de Posición
        else if (atacante.unitPosition == UnitPosition.Tierra && defensor.unitPosition == UnitPosition.Volando) { multiplicador -= 0.15f; }
        else if (atacante.unitPosition == UnitPosition.BajoTierra && defensor.unitPosition == UnitPosition.Tierra) { multiplicador -= 0.15f; }
        else if (atacante.unitPosition == UnitPosition.Volando && defensor.unitPosition == UnitPosition.BajoTierra) { multiplicador -= 0.15f; }

        return Mathf.Max(0.0f, multiplicador); 
    }


    IEnumerator PlayerAttackRoutine(GameObject targetEnemyObj, float multiplicadorQTE) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        EnemyStats target = targetEnemyObj.GetComponent<EnemyStats>();

        heroesDefendiendo[attacker] = false;

        if (multiplicadorQTE <= 0f) {
            Debug.Log($"¡{attacker.unitName} falló el ataque por completo!");
        } 
        else {

            float danoBruto = attacker.GetTotalAttack() - target.defense;
            if (danoBruto < 1) danoBruto = 1;

            float multiplicadorClase = CalcularMultiplicadorClasePosicion(attacker, target);
            
            int damage = Mathf.RoundToInt(danoBruto * ataquePendienteMultiplicador * multiplicadorQTE * multiplicadorClase);
            if (damage < 1) damage = 1;

            Debug.Log($"¡{attacker.unitName} ataca a {target.unitName}! Hizo {damage} de daño.");
            target.TakeDamage(damage);

            if (target.currentHP <= 0) {
                Debug.Log($"{target.unitName} ha sido derrotado y desaparece.");
                target.gameObject.SetActive(false); 
            } else {
                yield return StartCoroutine(RevisarInvocacionJefe(target));
            }
        }

        yield return new WaitForSecondsRealtime(1f); 
        ActualizarPantallaVida();
        AvanzarTurno(); 
    }

    IEnumerator PlayerAoEAttackRoutine(float multiplicadorQTE) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        heroesDefendiendo[attacker] = false;

        if (multiplicadorQTE <= 0f) {
            Debug.Log($"¡{attacker.unitName} falló el ataque en ÁREA por completo!");
        } 
        else {
            Debug.Log($"¡{attacker.unitName} lanza un ataque en ÁREA a todos los enemigos!");

            List<EnemyStats> jefesHeridos = new List<EnemyStats>();

            foreach (var enemy in activeEnemies) {
                EnemyStats target = enemy.GetComponent<EnemyStats>();
                if (target != null && target.currentHP > 0) {
                    
                    float danoBruto = attacker.GetTotalAttack() - target.defense;
                    if (danoBruto < 1) danoBruto = 1;

                    float multiplicadorClase = CalcularMultiplicadorClasePosicion(attacker, target);
                    
                    int damage = Mathf.RoundToInt(danoBruto * ataquePendienteMultiplicador * multiplicadorQTE * multiplicadorClase);
                    if (damage < 1) damage = 1;

                    target.TakeDamage(damage);

                    if (target.currentHP <= 0) {
                        target.gameObject.SetActive(false); 
                    } else if (target.esJefe && !target.yaInvoco && target.currentHP <= (target.maxHP / 2)) {
                        jefesHeridos.Add(target);
                    }
                }
            }

            foreach (var jefe in jefesHeridos) {
                yield return StartCoroutine(RevisarInvocacionJefe(jefe));
            }
        }

        yield return new WaitForSecondsRealtime(1f); 
        ActualizarPantallaVida();
        AvanzarTurno(); 
    }

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
            
            if (enemyAttacker.esJefe && enemyAttacker.currentHP <= (enemyAttacker.maxHP / 2)) {
                Debug.Log($"<color=red>¡{enemyAttacker.unitName} USA SU ATAQUE DEFINITIVO EN ÁREA!</color>");

                bool terminoEsquive = false;
                float multiplicadorJugador = 1f;

                int maxMastery = 0;
                foreach (HeroStats h in heroesVivos) {
                    if (h.mastery > maxMastery) maxMastery = h.mastery;
                }
                
                int reduccionTeclas = maxMastery / 50;
                int secuenciaFinal = Mathf.Max(1, 6 - reduccionTeclas);
                
                float aumentoTiempo = (maxMastery / 30) * 0.10f;
                float tiempoFinal = (QTEManager.Instance.tiempoParaQTE - 0.5f) * (1.0f + aumentoTiempo);

                QTEManager.Instance.IniciarEsquiveAoE(secuenciaFinal, tiempoFinal, (multiplicadorEsquive) => {
                    multiplicadorJugador = multiplicadorEsquive;
                    terminoEsquive = true;
                });

                yield return new WaitUntil(() => terminoEsquive);

                foreach (HeroStats targetHero in heroesVivos) {
                    float danoBase = enemyAttacker.attack - targetHero.defense;
                    if (danoBase < 1) danoBase = 1;

                    float multClase = CalcularMultiplicadorClasePosicion(enemyAttacker, targetHero);
                    int damage = Mathf.RoundToInt(danoBase * multClase);
                    if (damage < 1) damage = 1;

                    if (multiplicadorJugador == 1.0f) {
                        damage = 0; 
                        Debug.Log($"<color=cyan>¡{targetHero.unitName} ESQUIVÓ PERFECTAMENTE! (Daño 0)</color>");
                    }
                    else if (multiplicadorJugador == 0.5f) {
                        damage = damage / 2; 
                        Debug.Log($"<color=yellow>¡{targetHero.unitName} esquivó parcialmente! Recibe {damage} de daño.</color>");
                    } else {
                        Debug.Log($"<color=purple>¡{targetHero.unitName} falló al esquivar! Recibe el daño completo: {damage}.</color>");
                    }

                    if (damage > 0 && heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero]) {
                        damage = damage / 2;
                        Debug.Log($"¡Pero {targetHero.unitName} estaba DEFENDIENDO! Daño reducido.");
                    }

                    if (damage > 0) targetHero.TakeDamage(damage);
                }
                ActualizarPantallaVida();
            } 

            else {
                int ataquesRestantes = enemyAttacker.ataquesPorTurno;
                if (ataquesRestantes < 1) ataquesRestantes = 1;

                for (int i = 0; i < ataquesRestantes; i++) {
                    heroesVivos.Clear();
                    foreach (var hero in heroes) if (hero.currentHP > 0) heroesVivos.Add(hero);
                    if (heroesVivos.Count == 0) break; 

                    HeroStats targetHero = heroesVivos[Random.Range(0, heroesVivos.Count)];

                    float danoBase = enemyAttacker.attack - targetHero.defense;
                    if (danoBase < 1) danoBase = 1;

                    float multClase = CalcularMultiplicadorClasePosicion(enemyAttacker, targetHero);
                    int damage = Mathf.RoundToInt(danoBase * multClase);
                    if (damage < 1) damage = 1;

                    if (enemyAttacker.level > targetHero.level) {
                        Debug.Log($"<color=red>¡PELIGRO!</color> ¡Ataque ineludible de {enemyAttacker.unitName}!");
                        if (heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero]) damage = damage / 2;
                        
                        targetHero.TakeDamage(damage);
                        ActualizarPantallaVida();
                        yield return new WaitForSecondsRealtime(1.5f); 
                    } 
                    else {
                        KeyCode teclaEsquive = KeyCode.Space;
                        if (targetHero.unitName.Contains("Sieg")) teclaEsquive = KeyCode.E;
                        else if (targetHero.unitName.Contains("Merlin")) teclaEsquive = KeyCode.R;
                        else if (targetHero.unitName.Contains("Heracles")) teclaEsquive = KeyCode.T;

                        Debug.Log($"¡El enemigo ataca a {targetHero.unitName}! Presiona {teclaEsquive} para esquivar.");

                        bool terminoEsquive = false;

                        float aumentoTiempo = (targetHero.mastery / 30) * 0.10f;
                        
                        float tiempoFinal = (QTEManager.Instance.tiempoParaQTE / 3f) * (1.0f + aumentoTiempo);

                        QTEManager.Instance.IniciarEsquive(teclaEsquive, tiempoFinal, (multiplicadorEsquive) => {
                            if (multiplicadorEsquive == 1.0f) damage = 0; 
                            else if (multiplicadorEsquive == 0.5f) damage = damage / 2;

                            if (damage > 0 && heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero]) {
                                damage = damage / 2;
                            }

                            if (damage > 0) targetHero.TakeDamage(damage);
                            
                            ActualizarPantallaVida();
                            terminoEsquive = true;
                        });

                        yield return new WaitUntil(() => terminoEsquive);
                    }
                    
                    if (i < ataquesRestantes - 1) yield return new WaitForSecondsRealtime(0.5f);
                }
            }
        }

        yield return new WaitForSecondsRealtime(1f);
        AvanzarTurno(); 
    }


    void RestaurarPosiciones() {
        foreach (var hero in heroOriginalPositions) {
             if(hero.Key != null && hero.Key.GetComponent<HeroStats>().unitName == "Sieg") {
                 hero.Key.transform.position = hero.Value;
             }
        }

        foreach (var hero in heroOriginalPositions) {
             if(hero.Key != null && hero.Key.GetComponent<HeroStats>().unitName != "Sieg") {
                 hero.Key.transform.position = hero.Value;
             }
        }

        if (activeEnemies.Count > 0 && activeEnemies[0] != null) {
            activeEnemies[0].transform.position = enemyOriginalPosition;
            
            MonoBehaviour ai = (MonoBehaviour)activeEnemies[0].GetComponent("EnemyAI");
            if (ai != null) ai.enabled = true;
            
            Rigidbody2D rb = activeEnemies[0].GetComponent<Rigidbody2D>();
            if (rb != null) rb.isKinematic = false;

            EnemyStats statsPrincipal = activeEnemies[0].GetComponent<EnemyStats>();
            if (statsPrincipal != null) {
                statsPrincipal.currentHP = statsPrincipal.maxHP; 
                if (statsPrincipal.canvasUI != null) {
                    statsPrincipal.canvasUI.SetActive(false); 
                }
            }

            for (int i = 1; i < activeEnemies.Count; i++) {
                if (activeEnemies[i] != null) Destroy(activeEnemies[i]);
            }
        }
    }

    public void MostrarMenu(GameObject menuAMostrar) {
        if (menuPrincipal != null) menuPrincipal.SetActive(false);
        if (menuTipoAtaque != null) menuTipoAtaque.SetActive(false);
        if (menuEspeciales != null) menuEspeciales.SetActive(false);
        if (menuSeleccionEnemigo != null) menuSeleccionEnemigo.SetActive(false);

        if (menuAMostrar != null) menuAMostrar.SetActive(true);
    }


    public void OnBotonMenuAtacar() {
        MostrarMenu(menuTipoAtaque);
    }

    public void OnBotonMenuEspeciales() {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();

        if (attacker.unitName == "Sieg") {
            if (textoEspecial1 != null) textoEspecial1.text = "Corte Veloz (10)";
            if (textoEspecial2 != null) textoEspecial2.text = "Corte Destructor (20)";
            if (textoEspecial3 != null) textoEspecial3.text = "Tajo Divino (50)";
        } 
        else if (attacker.unitName == "Merlin") {
            if (textoEspecial1 != null) textoEspecial1.text = "Bala de Hielo (15)";
            if (textoEspecial2 != null) textoEspecial2.text = "Rayo Arcano (30)";
            if (textoEspecial3 != null) textoEspecial3.text = "Lluvia de Meteoros (60)";
        }
        else if (attacker.unitName == "Heracles") {
            if (textoEspecial1 != null) textoEspecial1.text = "Rompe Cráneos (15)";
            if (textoEspecial2 != null) textoEspecial2.text = "Ejecución Titánica (35)";
            if (textoEspecial3 != null) textoEspecial3.text = "Terremoto Infernal (65)";
        }

        MostrarMenu(menuEspeciales);
    }

    public void OnBotonAtras() {
        MostrarMenu(menuPrincipal);
    }

    
    public void OnAtaqueNormal() {
        Debug.Log("<color=cyan>[Combate]</color> Botón de Ataque Normal presionado.");
        PrepararAtaque(0, 1.4f, 2, true, false); 
    }
    public void OnEspecial1() { 
        Debug.Log("<color=cyan>[Combate]</color> Botón Especial 1 presionado."); 
        EjecutarAtaqueDinamico(1); 
    }
    public void OnEspecial2() { 
        Debug.Log("<color=cyan>[Combate]</color> Botón Especial 2 presionado."); 
        EjecutarAtaqueDinamico(2); 
    }
    public void OnEspecial3() { 
        Debug.Log("<color=cyan>[Combate]</color> Botón Especial 3 presionado."); 
        EjecutarAtaqueDinamico(3); 
    }

    void EjecutarAtaqueDinamico(int slotEspecial) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        string nombre = attacker.unitName;

        int costo = 10; float mult = 1.5f; int seqLen = 3; bool esAoE = false;

        if (nombre == "Sieg") {
            if (slotEspecial == 1) { costo = 10; mult = 1.5f; seqLen = 3; }
            else if (slotEspecial == 2) { costo = 20; mult = 1.8f; seqLen = 4; }
            else if (slotEspecial == 3) { costo = 50; mult = 3.0f; seqLen = 6; }
        } 
        else if (nombre == "Merlin") {
            if (slotEspecial == 1) { costo = 15; mult = 1.9f; seqLen = 3; }
            else if (slotEspecial == 2) { costo = 30; mult = 2.2f; seqLen = 4; }
            else if (slotEspecial == 3) { costo = 60; mult = 1.8f; seqLen = 6; esAoE = true; } 
        }
        else if (nombre == "Heracles") {
            if (slotEspecial == 1) { costo = 15; mult = 2f; seqLen = 3; }
            else if (slotEspecial == 2) { costo = 35; mult = 2.5f; seqLen = 5; }
            else if (slotEspecial == 3) { costo = 65; mult = 3.5f; seqLen = 6; }
        }

        PrepararAtaque(costo, mult, seqLen, true, esAoE);
    }

    void PrepararAtaque(int costo, float mult, int seqLen, bool esEspecial, bool esAoE = false) {
        if (state != CombatState.WAITING_FOR_INPUT) return;
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        
        if (esEspecial && attacker.currentEnergy < costo) {
            Debug.LogWarning($"<color=red>¡ERROR!</color> {attacker.unitName} intentó usar un ataque de coste {costo}, pero solo tiene {attacker.currentEnergy} de energía.");
            return;
        }

        ataquePendienteCosto = costo;
        ataquePendienteMultiplicador = mult;
        ataquePendienteSecuencia = seqLen;
        ataquePendienteEsEspecial = esEspecial;
        ataquePendienteEsAoE = esAoE;

        if (esAoE) {
            Debug.Log("<color=yellow>[Combate]</color> Preparando ataque en área. Omitiendo selección de objetivo.");
            ConfirmarAtaqueAEnemigo(-1); 
        } else {
            for (int i = 0; i < botonesSeleccionEnemigo.Length; i++) {
                if (i < activeEnemies.Count && activeEnemies[i] != null && activeEnemies[i].GetComponent<EnemyStats>().currentHP > 0) {
                    botonesSeleccionEnemigo[i].gameObject.SetActive(true);
                    textosBotonesEnemigo[i].text = activeEnemies[i].GetComponent<EnemyStats>().unitName;
                } else {
                    botonesSeleccionEnemigo[i].gameObject.SetActive(false);
                }
            }
            MostrarMenu(menuSeleccionEnemigo);
        }
    }

    public void ConfirmarAtaqueAEnemigo(int enemyIndex) {
        if (state != CombatState.WAITING_FOR_INPUT) return;

        HeroStats attacker = currentActor.GetComponent<HeroStats>();

        if (ataquePendienteEsEspecial) {
            attacker.UseEnergy(ataquePendienteCosto);
            ActualizarPantallaVida();
        }

        panelBotonesAccion.SetActive(false);
        state = CombatState.BUSY;

        if (ataquePendienteEsEspecial) {
            Debug.Log("¡Iniciando secuencia QTE!");

            int reduccionTeclas = attacker.mastery / 50;
            int secuenciaFinal = Mathf.Max(1, ataquePendienteSecuencia - reduccionTeclas);

            float aumentoTiempo = (attacker.mastery / 30) * 0.10f;
            float tiempoFinal = QTEManager.Instance.tiempoParaQTE * (1.0f + aumentoTiempo);

            QTEManager.Instance.IniciarQTE(secuenciaFinal, tiempoFinal, (multiplicadorQTE) => {     
                float multiplicadorFinal = ataquePendienteMultiplicador * multiplicadorQTE;
                
                if (ataquePendienteEsAoE) {
                    StartCoroutine(PlayerAoEAttackRoutine(multiplicadorFinal));
                } else {
                    GameObject targetEnemyObj = activeEnemies[enemyIndex];
                    StartCoroutine(PlayerAttackRoutine(targetEnemyObj, multiplicadorFinal));
                }
            });
        } else {
            GameObject targetEnemyObj = activeEnemies[enemyIndex];
            StartCoroutine(PlayerAttackRoutine(targetEnemyObj, 1f));
        }
    }

    public void ToggleGuiaFortalezas() {
        if (panelGuiaFortalezas != null) {
            panelGuiaFortalezas.SetActive(!panelGuiaFortalezas.activeSelf);
        }
    }

    void DesactivarAILocal(GameObject enemigo) {
        MonoBehaviour ai = (MonoBehaviour)enemigo.GetComponent("EnemyAI");
        if (ai != null) ai.enabled = false;
        
        Rigidbody2D rb = enemigo.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }

    public void BotonAceptarVictoria() {
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (GameFlowController.Instance != null) GameFlowController.Instance.TerminarCombate(true, false);
    }

    public void BotonContinuarDerrota() {
        if (panelDerrota != null) panelDerrota.SetActive(false);
        if (GameFlowController.Instance != null) GameFlowController.Instance.TerminarCombate(false, false);
    }

    public void BotonMenuDerrota() {
        Debug.Log("Saliendo al menú principal...");
        Time.timeScale = 1f;
        if (GameFlowController.Instance != null) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameFlowController.Instance.nombreEscenaMenu);
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu"); 
        }
    }

    
}