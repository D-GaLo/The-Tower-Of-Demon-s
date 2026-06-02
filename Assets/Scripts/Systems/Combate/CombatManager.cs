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
    
    private List<string> botinDelCombate = new List<string>();

    private int ataquePendienteCosto;
    private float ataquePendienteMultiplicador;
    private int ataquePendienteSecuencia;
    private bool ataquePendienteEsEspecial;

    [Header("UI Especial Jefes")]
    public GameObject panelTituloJefe;
    public TextMeshProUGUI textoTituloJefe;

    [Header("Cinemática Nucifera Fase 2")]
    public GameObject panelNegroTransicion;
    public TextMeshProUGUI textoPanelNegro;
    public GameObject prefabNuciferaFase2;
    
    private bool nuciferaFase2Iniciada = false;
    private int turnosNuciferaFase2 = 0;
    private Dictionary<HeroStats, bool> heroesAtrapados = new Dictionary<HeroStats, bool>();

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void StartCombat(GameObject enemy, bool ventajaJugador = false) {
        if (enCombateActivo) return;
        enCombateActivo = true;
        

        nuciferaFase2Iniciada = false;
        turnosNuciferaFase2 = 0;
        heroesAtrapados.Clear();

        if (DialogoCombateUI.Instance != null) {
            DialogoCombateUI.Instance.LimpiarMensajes();
        }
        
        botinDelCombate.Clear();
        
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

            if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
            if (panelVictoria != null) panelVictoria.SetActive(true);

            if (textoVictoriaObjeto != null) {
                StopCoroutine("ParpadearTextoVictoria"); 
                textoVictoriaObjeto.color = Color.white; 

                if (botinDelCombate.Count > 0) {
                    textoVictoriaObjeto.text = "Objetos obtenidos:\n• " + string.Join("\n• ", botinDelCombate);
                    StartCoroutine("ParpadearTextoVictoria");
                } else {
                    textoVictoriaObjeto.text = "Ningún objeto obtenido.";
                }
            }

        } else {
            state = CombatState.LOST;
            Debug.Log("¡Termina el combate: DERROTA! Game Over...");

            if (AudioManager.Instance != null) AudioManager.Instance.PlayDerrota();
            
            if (panelBotonesAccion != null) panelBotonesAccion.SetActive(false);
            if (panelDerrota != null) panelDerrota.SetActive(true);
        }

        RestaurarPosiciones();
    }

    IEnumerator ParpadearTextoVictoria() {
        while (panelVictoria != null && panelVictoria.activeSelf && textoVictoriaObjeto != null) {
            textoVictoriaObjeto.color = Color.yellow;
            yield return new WaitForSecondsRealtime(0.3f);
            textoVictoriaObjeto.color = Color.white;
            yield return new WaitForSecondsRealtime(0.3f);
        }
        if (textoVictoriaObjeto != null) textoVictoriaObjeto.color = Color.white; 
    }

    IEnumerator PrepararArenaYMostrarFormacion(bool ventajaJugador) {
        Debug.Log("Spawneando enemigos para que el jugador los analice...");
        
        EnemyStats statsEnemigoPrincipal = activeEnemies[0].GetComponent<EnemyStats>();
        enemyOriginalPosition = activeEnemies[0].transform.position;
        
        activeEnemies[0].name = activeEnemies[0].name.Replace("(Clone)", "").Trim();
        
        if (string.IsNullOrEmpty(statsEnemigoPrincipal.unitName)) {
            statsEnemigoPrincipal.unitName = activeEnemies[0].name;
        }

        int maxNivelGrupo = 1;
        foreach (HeroStats heroe in listaParty) {
            if (heroe.level > maxNivelGrupo) maxNivelGrupo = heroe.level;
        }

        if (!statsEnemigoPrincipal.nivelManual && !statsEnemigoPrincipal.esJefe) {
            
            int nivelMinimoGrupo = int.MaxValue;
            foreach (HeroStats heroe in listaParty) {
                if (heroe.level < nivelMinimoGrupo) nivelMinimoGrupo = heroe.level;
            }
            if (nivelMinimoGrupo == int.MaxValue) nivelMinimoGrupo = 1;

            int dado = Random.Range(1, 101);
            int nivelCalculado = nivelMinimoGrupo;

            if (dado <= 65) nivelCalculado = nivelMinimoGrupo;
            else if (dado <= 85) nivelCalculado = nivelMinimoGrupo - 1;
            else nivelCalculado = nivelMinimoGrupo + 1;

            if (nivelCalculado < 1) nivelCalculado = 1;
            if (nivelCalculado > 10) nivelCalculado = 10;

            statsEnemigoPrincipal.level = nivelCalculado;
            statsEnemigoPrincipal.EscalarEstadisticas();
        }

        statsEnemigoPrincipal.nivelPeligroso = (statsEnemigoPrincipal.level > maxNivelGrupo);

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
                if (string.IsNullOrEmpty(statsClon.unitName)) statsClon.unitName = clon.name;
                
                statsClon.level = statsEnemigoPrincipal.level;
                statsClon.nivelManual = false; 
                statsClon.esJefe = false; 
                
                statsClon.nivelPeligroso = (statsClon.level > maxNivelGrupo);
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
            DialogoCombateUI.Instance.AgregarMensaje("<color=green>¡ATAQUE SORPRESA!</color> Los enemigos inician con menos vida.");
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
            DialogoCombateUI.Instance.AgregarMensaje("<color=red>¡EMBOSCADA!</color> Los héroes inician con menos vida.");
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
                    else if (statsEnemigoPrincipal.unitName.Contains("Rebone")) {
                        tituloImponente = "<color=#9370DB><size=130%><b>REBONE</b></size></color>\n<size=50%><i><color=#8A2BE2>El Portero del Abismo</color></i></size>";
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
            
            if (nuciferaFase2Iniciada && jefe.unitName.Contains("Nucifera")) {
                DialogoCombateUI.Instance.AgregarMensaje($"<color=#32CD32>¡Nucifera revela su auténtica forma!</color>");
                yield return new WaitForSecondsRealtime(2f);
            } 
            else {
                DialogoCombateUI.Instance.AgregarMensaje($"<color=red>¡ALERTA!</color> ¡{jefe.unitName} se enfurece e invoca refuerzos!");
                
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
                        if (string.IsNullOrEmpty(statsClon.unitName)) statsClon.unitName = clon.name;
                        
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
            if (heroesAtrapados.ContainsKey(heroActor) && heroesAtrapados[heroActor]) {
                heroesAtrapados[heroActor] = false;
                
                DialogoCombateUI.Instance.AgregarMensaje($"¡<color=#00BFFF>{heroActor.unitName}</color> está luchando por zafarse de las enredaderas!");
                StartCoroutine(SaltarTurnoAtrapado(heroActor));
                return;
            }

            state = CombatState.WAITING_FOR_INPUT;
            DialogoCombateUI.Instance.AgregarMensaje($"¡Es el turno de <color=#00BFFF>{heroActor.unitName}</color>!");
            if (panelBotonesAccion != null){ 
                panelBotonesAccion.SetActive(true);
                MostrarMenu(menuPrincipal);
                PosicionarMenuSobreHeroe(currentActor); 
            }
        } else {
            state = CombatState.BUSY;
            EnemyStats enemyStats = currentActor.GetComponent<EnemyStats>();
            if (enemyStats != null) DialogoCombateUI.Instance.AgregarMensaje($"Turno de <color=#FF8C00>{enemyStats.unitName}</color>.");
            
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
        bool nuciferaFase1Derrotado = false;

        foreach(var enemyObj in activeEnemies) {
            EnemyStats eStats = enemyObj.GetComponent<EnemyStats>();
            if (eStats != null && eStats.currentHP > 0) {
                todosEnemigosMuertos = false;
                break;
            }
            if (eStats != null && eStats.currentHP <= 0 && eStats.unitName.Contains("Nucifera") && !nuciferaFase2Iniciada) {
                nuciferaFase1Derrotado = true;
            }
        }

        if (todosEnemigosMuertos) {
            if (nuciferaFase1Derrotado) {
                StartCoroutine(TransicionNuciferaFase2());
                return true; 
            }
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
            
            int curaHP = Mathf.RoundToInt(hero.maxHP * 0.15f);
            int curaEnergia = Mathf.RoundToInt(hero.maxEnergy * 0.20f);
            
            hero.currentHP = Mathf.Clamp(hero.currentHP + curaHP, 0, hero.maxHP);
            hero.currentEnergy = Mathf.Clamp(hero.currentEnergy + curaEnergia, 0, hero.maxEnergy);

            DialogoCombateUI.Instance.AgregarMensaje($"¡{hero.unitName} se defiende! Recupera <color=green>{curaHP} HP</color> y <color=yellow>{curaEnergia} ENG</color>.");
        }
        
        ActualizarPantallaVida();
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

    float CalcularMultiplicadorClasePosicion(UnitStats atacante, UnitStats defensor, out int nivelVentaja) {
        float multiplicador = 1.0f;
        nivelVentaja = 0;

        if (atacante.unitClass == UnitClass.Melee && defensor.unitClass == UnitClass.Rango) { multiplicador += 0.15f; nivelVentaja++; }
        else if (atacante.unitClass == UnitClass.Rango && defensor.unitClass == UnitClass.Tanque) { multiplicador += 0.15f; nivelVentaja++; }
        else if (atacante.unitClass == UnitClass.Tanque && defensor.unitClass == UnitClass.Melee) { multiplicador += 0.15f; nivelVentaja++; }
        else if (atacante.unitClass == UnitClass.Rango && defensor.unitClass == UnitClass.Melee) { multiplicador -= 0.15f; nivelVentaja--; }
        else if (atacante.unitClass == UnitClass.Tanque && defensor.unitClass == UnitClass.Rango) { multiplicador -= 0.15f; nivelVentaja--; }
        else if (atacante.unitClass == UnitClass.Melee && defensor.unitClass == UnitClass.Tanque) { multiplicador -= 0.15f; nivelVentaja--; }


        if (atacante.unitPosition == UnitPosition.Volando && defensor.unitPosition == UnitPosition.Tierra) { multiplicador += 0.15f; nivelVentaja++; }
        else if (atacante.unitPosition == UnitPosition.Tierra && defensor.unitPosition == UnitPosition.BajoTierra) { multiplicador += 0.15f; nivelVentaja++; }
        else if (atacante.unitPosition == UnitPosition.BajoTierra && defensor.unitPosition == UnitPosition.Volando) { multiplicador += 0.15f; nivelVentaja++; }
        else if (atacante.unitPosition == UnitPosition.Tierra && defensor.unitPosition == UnitPosition.Volando) { multiplicador -= 0.15f; nivelVentaja--; }
        else if (atacante.unitPosition == UnitPosition.BajoTierra && defensor.unitPosition == UnitPosition.Tierra) { multiplicador -= 0.15f; nivelVentaja--; }
        else if (atacante.unitPosition == UnitPosition.Volando && defensor.unitPosition == UnitPosition.BajoTierra) { multiplicador -= 0.15f; nivelVentaja--; }

        return Mathf.Max(0.0f, multiplicador); 
    }


    IEnumerator PlayerAttackRoutine(GameObject targetEnemyObj, float multiplicadorQTE) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        EnemyStats target = targetEnemyObj.GetComponent<EnemyStats>();

        heroesDefendiendo[attacker] = false;

        if (ataquePendienteEsEspecial) {
            DialogoCombateUI.Instance.AgregarMensaje($"{attacker.unitName} usó un Especial. Consumió <color=yellow>{ataquePendienteCosto} ENG</color>.");
        }

        if (multiplicadorQTE <= 0f) {
            DialogoCombateUI.Instance.AgregarMensaje($"¡{attacker.unitName} falló el ataque por completo!");
        } 
        else {
            if (ataquePendienteEsEspecial) {
                if (multiplicadorQTE >= ataquePendienteMultiplicador) DialogoCombateUI.Instance.AgregarMensaje("¡Secuencia <color=green>PERFECTA</color>!");
                else DialogoCombateUI.Instance.AgregarMensaje("Secuencia completada <color=yellow>a medias</color>.");
            }

            float danoBruto = attacker.GetTotalAttack() - target.defense;
            if (danoBruto < 1) danoBruto = 1;

            int nivelVentaja;
            float multiplicadorClase = CalcularMultiplicadorClasePosicion(attacker, target, out nivelVentaja);
            
            if (nivelVentaja == 1) DialogoCombateUI.Instance.AgregarMensaje("¡El ataque tiene <color=yellow>Ventaja x1</color>!");
            else if (nivelVentaja >= 2) DialogoCombateUI.Instance.AgregarMensaje("¡El ataque tiene <color=green>VENTAJA x2</color>!");
            else if (nivelVentaja == -1) DialogoCombateUI.Instance.AgregarMensaje("El ataque tiene <color=orange>Desventaja x1</color>.");
            else if (nivelVentaja <= -2) DialogoCombateUI.Instance.AgregarMensaje("El ataque tiene <color=red>DESVENTAJA x2</color>.");

            int damage = Mathf.RoundToInt(danoBruto * ataquePendienteMultiplicador * multiplicadorQTE * multiplicadorClase);
            if (damage < 1) damage = 1;

            DialogoCombateUI.Instance.AgregarMensaje($"¡{attacker.unitName} hizo <color=red>{damage} de daño</color> a {target.unitName}!");
            target.TakeDamage(damage);

            if (target.currentHP <= 0) {
                DialogoCombateUI.Instance.AgregarMensaje($"¡{target.unitName} ha sido derrotado!");
                
                if (nuciferaFase2Iniciada && target.unitName.Contains("Nucifera")) {
                    if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayRugido", SendMessageOptions.DontRequireReceiver);
                }
                
                if (target.posiblesDropeos != null && target.posiblesDropeos.Length > 0) {
                    int tirada = Random.Range(1, 101);
                    int probabilidadAcumulada = 0;

                    foreach (DropData drop in target.posiblesDropeos) {
                        if (drop.item != null) {
                            probabilidadAcumulada += drop.probabilidad;
                            if (tirada <= probabilidadAcumulada) {
                                InventarioEnum.Instance.AddItem(drop.item.itemID, 1);
                                botinDelCombate.Add(drop.item.itemName);
                                break; 
                            }
                        }
                    }
                }
                target.gameObject.SetActive(false); 
            } else {
                yield return StartCoroutine(RevisarInvocacionJefe(target));
            }
        }

        yield return new WaitForSecondsRealtime(1.5f); 
        ActualizarPantallaVida();
        AvanzarTurno(); 
    }

    IEnumerator PlayerAoEAttackRoutine(float multiplicadorQTE) {
        HeroStats attacker = currentActor.GetComponent<HeroStats>();
        heroesDefendiendo[attacker] = false;

        if (ataquePendienteEsEspecial) {
            DialogoCombateUI.Instance.AgregarMensaje($"{attacker.unitName} usó un Especial en ÁREA. Consumió <color=yellow>{ataquePendienteCosto} ENG</color>.");
        }

        if (multiplicadorQTE <= 0f) {
            DialogoCombateUI.Instance.AgregarMensaje($"¡{attacker.unitName} falló el ataque por completo!");
        } 
        else {
            if (multiplicadorQTE >= ataquePendienteMultiplicador) DialogoCombateUI.Instance.AgregarMensaje("¡Secuencia <color=green>PERFECTA</color>!");
            else DialogoCombateUI.Instance.AgregarMensaje("Secuencia completada <color=yellow>a medias</color>.");

            List<EnemyStats> jefesHeridos = new List<EnemyStats>();

            foreach (var enemy in activeEnemies) {
                EnemyStats target = enemy.GetComponent<EnemyStats>();
                if (target != null && target.currentHP > 0) {
                    
                    float danoBruto = attacker.GetTotalAttack() - target.defense;
                    if (danoBruto < 1) danoBruto = 1;

                    int nivelVentaja;
                    float multiplicadorClase = CalcularMultiplicadorClasePosicion(attacker, target, out nivelVentaja);
                    
                    int damage = Mathf.RoundToInt(danoBruto * ataquePendienteMultiplicador * multiplicadorQTE * multiplicadorClase);
                    if (damage < 1) damage = 1;

                    string msgVentaja = "";
                    if (nivelVentaja == 1) msgVentaja = " (Ventaja x1)";
                    else if (nivelVentaja >= 2) msgVentaja = " (Ventaja x2)";
                    else if (nivelVentaja == -1) msgVentaja = " (Desventaja x1)";
                    else if (nivelVentaja <= -2) msgVentaja = " (Desventaja x2)";

                    DialogoCombateUI.Instance.AgregarMensaje($"¡Daño a {target.unitName}: <color=red>{damage}</color>!{msgVentaja}");
                    target.TakeDamage(damage);

                    if (target.currentHP <= 0) {
                DialogoCombateUI.Instance.AgregarMensaje($"¡{target.unitName} ha sido derrotado!");
                

                if (nuciferaFase2Iniciada && target.unitName.Contains("Nucifera")) {
                    if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayRugido", SendMessageOptions.DontRequireReceiver);
                }
                
                if (target.posiblesDropeos != null && target.posiblesDropeos.Length > 0) {
                            int tirada = Random.Range(1, 101);
                            int probabilidadAcumulada = 0;
                            foreach (DropData drop in target.posiblesDropeos) {
                                if (drop.item != null) {
                                    probabilidadAcumulada += drop.probabilidad;
                                    if (tirada <= probabilidadAcumulada) {
                                        InventarioEnum.Instance.AddItem(drop.item.itemID, 1);
                                        botinDelCombate.Add(drop.item.itemName);
                                        break; 
                                    }
                                }
                            }
                        }
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

        yield return new WaitForSecondsRealtime(1.5f); 
        ActualizarPantallaVida();
        AvanzarTurno(); 
    }

    IEnumerator EnemyTurnRoutine() {
        yield return new WaitForSecondsRealtime(1f); 

        EnemyStats enemyAttacker = currentActor.GetComponent<EnemyStats>();
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();

        List<HeroStats> heroesVivos = new List<HeroStats>();
        foreach (var hero in heroes) {
            if (hero.currentHP > 0) heroesVivos.Add(hero);
        }

        if (heroesVivos.Count > 0 && enemyAttacker != null) {
            
            bool esNuciferaP2 = nuciferaFase2Iniciada && enemyAttacker.unitName.Contains("Nucifera");

            if (esNuciferaP2) {
                turnosNuciferaFase2++;
                if (turnosNuciferaFase2 % 2 == 0) {
                    enemyAttacker.unitClass = (UnitClass)Random.Range(1, 4);
                    enemyAttacker.unitPosition = (UnitPosition)Random.Range(0, 3);
                    enemyAttacker.ActualizarVisuales();
                    DialogoCombateUI.Instance.AgregarMensaje($"¡<color=#FF8C00>{enemyAttacker.unitName}</color> muta! Ahora es <color=yellow>{enemyAttacker.unitClass}</color> y está <color=yellow>{enemyAttacker.unitPosition}</color>.");
                    yield return new WaitForSecondsRealtime(1.5f);
                }
            }

            bool usarDefinitivo = enemyAttacker.esJefe && (enemyAttacker.currentHP <= (enemyAttacker.maxHP / 2));
            
            if (esNuciferaP2) {
                usarDefinitivo = (turnosNuciferaFase2 % 2 == 0);
            }

            if (usarDefinitivo) {
                DialogoCombateUI.Instance.AgregarMensaje($"<color=red>¡{enemyAttacker.unitName.ToUpper()} USA SU ATAQUE DEFINITIVO EN ÁREA!</color>");

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

                    int nivelVentaja;
                    float multClase = CalcularMultiplicadorClasePosicion(enemyAttacker, targetHero, out nivelVentaja);
                    int damage = Mathf.RoundToInt(danoBase * multClase);
                    if (damage < 1) damage = 1;

                    float multFinalJugador = multiplicadorJugador;
                    if (heroesAtrapados.ContainsKey(targetHero) && heroesAtrapados[targetHero]) {
                        if (multFinalJugador == 1.0f) {
                            multFinalJugador = 0.5f;
                            DialogoCombateUI.Instance.AgregarMensaje($"<color=orange>¡{targetHero.unitName} está atrapado y solo esquiva a medias!</color>");
                        }
                    }

                    if (multFinalJugador == 1.0f) {
                        damage = 0; 
                        DialogoCombateUI.Instance.AgregarMensaje($"<color=#32CD32>¡{targetHero.unitName} esquivó el ataque definitivo!</color>");
                    }
                    else if (multFinalJugador == 0.5f) {
                        damage = damage / 3; 
                        DialogoCombateUI.Instance.AgregarMensaje($"<color=yellow>{targetHero.unitName} esquivó parcialmente.</color>");
                    } else {
                        DialogoCombateUI.Instance.AgregarMensaje($"<color=purple>¡{targetHero.unitName} falló al esquivar!</color>");
                    }

                    if (damage > 0 && heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero]) {
                        damage = Mathf.RoundToInt(damage * 0.75f);
                    }

                    if (damage > 0) {
                        DialogoCombateUI.Instance.AgregarMensaje($"{targetHero.unitName} recibe <color=#FF4444>{damage} de daño</color>.");
                        targetHero.TakeDamage(damage);
                        

                        if (targetHero.currentHP <= 0) {
                            LimpiarMuerteHeroe(targetHero);
                            DialogoCombateUI.Instance.AgregarMensaje($"<color=red>¡{targetHero.unitName} ha caído!</color>");
                        }
                    }
                }
                ActualizarPantallaVida();
            } 
            else {
                bool hizoEnredaderas = false;
                if (esNuciferaP2 && enemyAttacker.currentHP <= (enemyAttacker.maxHP / 2)) {
                    bool hayAtrapados = heroesVivos.Any(h => heroesAtrapados.ContainsKey(h) && heroesAtrapados[h]);
                    
                    if (!hayAtrapados && Random.Range(0, 100) < 40) {
                        hizoEnredaderas = true;
                    }
                }

                if (hizoEnredaderas) {
                    DialogoCombateUI.Instance.AgregarMensaje($"<color=#32CD32>¡Nucifera lanza enredaderas traicioneras!</color>");
                    List<HeroStats> libres = heroesVivos.Where(h => !heroesAtrapados.ContainsKey(h) || !heroesAtrapados[h]).ToList();
                    
                    if (libres.Count > 0) {
                        HeroStats victima = libres[Random.Range(0, libres.Count)];
                        heroesAtrapados[victima] = true;
                        
                        StartCoroutine(MantenerEnredaderaVisible(victima));
                        
                        DialogoCombateUI.Instance.AgregarMensaje($"¡<color=#00BFFF>{victima.unitName}</color> fue atrapado y perderá su turno!");
                    } else {
                        DialogoCombateUI.Instance.AgregarMensaje($"¡Pero todos los héroes ya están atrapados!");
                    }
                    yield return new WaitForSecondsRealtime(2f);
                } 
                
                int ataquesRestantes = enemyAttacker.ataquesPorTurno;
                if (ataquesRestantes < 1) ataquesRestantes = 1;

                for (int i = 0; i < ataquesRestantes; i++) {
                    heroesVivos.Clear();
                    foreach (var hero in heroes) if (hero.currentHP > 0) heroesVivos.Add(hero);
                    if (heroesVivos.Count == 0) break; 

                    List<HeroStats> objetivos = new List<HeroStats>();

                    if (enemyAttacker.unitName.Contains("Rebone") || esNuciferaP2) {
                        DialogoCombateUI.Instance.AgregarMensaje($"¡<color=#FF8C00>{enemyAttacker.unitName}</color> lanza un poderoso ataque en ÁREA!");
                        objetivos.AddRange(heroesVivos);
                    } else {
                        HeroStats targetHero = heroesVivos[Random.Range(0, heroesVivos.Count)];
                        DialogoCombateUI.Instance.AgregarMensaje($"¡<color=#FF8C00>{enemyAttacker.unitName}</color> ataca a <color=#00BFFF>{targetHero.unitName}</color>!");
                        objetivos.Add(targetHero);
                    }

                    foreach (HeroStats targetHero in objetivos) {
                        if (targetHero.currentHP <= 0) continue; 

                        float danoBase = enemyAttacker.attack - targetHero.defense;
                        if (danoBase < 1) danoBase = 1;

                        int nivelVentaja;
                        float multClase = CalcularMultiplicadorClasePosicion(enemyAttacker, targetHero, out nivelVentaja);
                        int damage = Mathf.RoundToInt(danoBase * multClase);
                        if (damage < 1) damage = 1;

                        bool estaDefendiendo = heroesDefendiendo.ContainsKey(targetHero) && heroesDefendiendo[targetHero];

                        if (enemyAttacker.level > targetHero.level || estaDefendiendo) {
                            if (estaDefendiendo) {
                                damage = Mathf.RoundToInt(damage * 0.75f);
                                DialogoCombateUI.Instance.AgregarMensaje($"¡{targetHero.unitName} bloqueó con su guardia!");
                            } else {
                                if (!enemyAttacker.unitName.Contains("Rebone") && !esNuciferaP2) {
                                    DialogoCombateUI.Instance.AgregarMensaje($"<color=#FF8C00>¡Ataque ineludible de {enemyAttacker.unitName}!</color>");
                                } else {
                                    DialogoCombateUI.Instance.AgregarMensaje($"<color=#FF8C00>¡Ineludible para {targetHero.unitName}!</color>");
                                }
                            }
                            
                            targetHero.TakeDamage(damage);
                            DialogoCombateUI.Instance.AgregarMensaje($"{targetHero.unitName} recibe <color=#FF4444>{damage} de daño</color>.");

                            if (targetHero.currentHP <= 0) {
                                LimpiarMuerteHeroe(targetHero);
                                DialogoCombateUI.Instance.AgregarMensaje($"<color=red>¡{targetHero.unitName} ha caído!</color>");
                            }
                            
                            ActualizarPantallaVida();
                            yield return new WaitForSecondsRealtime(1.0f); 
                        } 
                        else {
                            KeyCode teclaEsquive = KeyCode.Space;
                            if (targetHero.unitName.Contains("Sieg")) teclaEsquive = KeyCode.E;
                            else if (targetHero.unitName.Contains("Merlin")) teclaEsquive = KeyCode.R;
                            else if (targetHero.unitName.Contains("Heracles")) teclaEsquive = KeyCode.T;

                            bool terminoEsquive = false;

                            float aumentoTiempo = (targetHero.mastery / 30) * 0.10f;
                            float tiempoFinal = (QTEManager.Instance.tiempoParaQTE / 3f) * (1.0f + aumentoTiempo);

                            if (enemyAttacker.unitName.Contains("Rebone") || esNuciferaP2) {
                                DialogoCombateUI.Instance.AgregarMensaje($"¡{targetHero.unitName}, presiona {teclaEsquive}!");
                            }

                            QTEManager.Instance.IniciarEsquive(teclaEsquive, tiempoFinal, (multiplicadorEsquive) => {
                                
                                float multFinalJugador = multiplicadorEsquive;
                                if (heroesAtrapados.ContainsKey(targetHero) && heroesAtrapados[targetHero]) {
                                    if (multFinalJugador == 1.0f) {
                                        multFinalJugador = 0.5f;
                                        DialogoCombateUI.Instance.AgregarMensaje($"<color=orange>¡{targetHero.unitName} está atrapado y solo esquiva a medias!</color>");
                                    }
                                }

                                if (multFinalJugador == 1.0f) {
                                    damage = 0; 
                                    DialogoCombateUI.Instance.AgregarMensaje($"<color=#32CD32>¡{targetHero.unitName} lo esquivó perfectamente!</color>");
                                }
                                else if (multFinalJugador == 0.5f) {
                                    damage = damage / 3;
                                    DialogoCombateUI.Instance.AgregarMensaje($"<color=yellow>{targetHero.unitName} esquivó parcialmente.</color>");
                                } else {
                                    DialogoCombateUI.Instance.AgregarMensaje($"<color=purple>¡{targetHero.unitName} no pudo esquivar!</color>");
                                }

                                if (damage > 0) {
                                    targetHero.TakeDamage(damage);
                                    DialogoCombateUI.Instance.AgregarMensaje($"{targetHero.unitName} recibe <color=#FF4444>{damage} de daño</color>.");
                                    if (targetHero.currentHP <= 0) {
                                        LimpiarMuerteHeroe(targetHero);
                                        DialogoCombateUI.Instance.AgregarMensaje($"<color=red>¡{targetHero.unitName} ha caído!</color>");
                                    }
                                }
                                
                                ActualizarPantallaVida();
                                terminoEsquive = true;
                            });

                            yield return new WaitUntil(() => terminoEsquive);
                        }
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
            if (state != CombatState.WON) {
                activeEnemies[0].SetActive(true); 
                SpriteRenderer sr = activeEnemies[0].GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
            
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
            if (textoEspecial1 != null) textoEspecial1.text = "Bala de Hielo (15) (Área)";
            if (textoEspecial2 != null) textoEspecial2.text = "Rayo Arcano (30) (Área)";
            if (textoEspecial3 != null) textoEspecial3.text = "Lluvia de Meteoros (60) (Área)";
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
            if (slotEspecial == 1) { costo = 10; mult = 2f; seqLen = 3; }
            else if (slotEspecial == 2) { costo = 20; mult = 2.5f; seqLen = 4; }
            else if (slotEspecial == 3) { costo = 50; mult = 3.0f; seqLen = 6; }
        } 
        else if (nombre == "Merlin") {
            if (slotEspecial == 1) { costo = 15; mult = 1.5f; seqLen = 3; esAoE = true; }
            else if (slotEspecial == 2) { costo = 30; mult = 2f; seqLen = 4; esAoE = true; }
            else if (slotEspecial == 3) { costo = 60; mult = 2.5f; seqLen = 6; esAoE = true; } 
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
            DialogoCombateUI.Instance.AgregarMensaje($"<color=red>¡Sin Energía!</color> {attacker.unitName} necesita {costo} ENG.");
            return;
        }

        ataquePendienteCosto = costo;
        ataquePendienteMultiplicador = mult;
        ataquePendienteSecuencia = seqLen;
        ataquePendienteEsEspecial = esEspecial;
        ataquePendienteEsAoE = esAoE;

        if (esAoE) {
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
            bool estaActiva = !panelGuiaFortalezas.activeSelf;
            panelGuiaFortalezas.SetActive(estaActiva);
            
            Time.timeScale = estaActiva ? 0f : 1f;
            
            if (AudioManager.Instance != null) {
                if (estaActiva) AudioManager.Instance.PlayClic();
                else AudioManager.Instance.PlayClic();
            }
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

    void LimpiarMuerteHeroe(HeroStats hero) {
        if (heroesAtrapados.ContainsKey(hero)) heroesAtrapados[hero] = false;
        Transform enredadera = hero.transform.Find("EnredaderaTrampa");
        if (enredadera != null) enredadera.gameObject.SetActive(false);
    }

    IEnumerator TransicionNuciferaFase2() {
        enCombateActivo = false; 

        if (AudioManager.Instance != null) {
            AudioSource bgm = AudioManager.Instance.GetComponent<AudioSource>();
            if (bgm != null) bgm.Stop();
            AudioManager.Instance.SendMessage("PlayCargaEnergia", SendMessageOptions.DontRequireReceiver);
        }

        if (panelNegroTransicion != null) {
            panelNegroTransicion.SetActive(true);
            panelNegroTransicion.transform.SetAsLastSibling();
            if (textoPanelNegro != null) textoPanelNegro.text = "<color=red>Nucifera florece y acaba con esas alimañas...</color>";
        }

        yield return new WaitForSecondsRealtime(4f);

        GameObject nuciferaFase1 = activeEnemies[0];
        SpriteRenderer srNucifera1 = nuciferaFase1.GetComponent<SpriteRenderer>();
        if (srNucifera1 != null) srNucifera1.color = Color.white;
        
        nuciferaFase1.SetActive(false);
        
        for (int i = 1; i < activeEnemies.Count; i++) {
            if (activeEnemies[i] != null) Destroy(activeEnemies[i]);
        }
        
        activeEnemies.Clear();
        activeEnemies.Add(nuciferaFase1);
        
        botinDelCombate.Clear();
        turnQueue.Clear();
        if (DialogoCombateUI.Instance != null) DialogoCombateUI.Instance.LimpiarMensajes();

        GameObject nucifera2 = Instantiate(prefabNuciferaFase2);
        nucifera2.name = "Nucifera Fase 2";
        DesactivarAILocal(nucifera2);
        
        EnemyStats statsF2 = nucifera2.GetComponent<EnemyStats>();
        if (string.IsNullOrEmpty(statsF2.unitName)) statsF2.unitName = "Nucifera";
        statsF2.esJefe = true;
        statsF2.nivelManual = true; 
        statsF2.EscalarEstadisticas();

        int maxNivelGrupo = 1;
        foreach (HeroStats heroe in listaParty) {
            if (heroe.level > maxNivelGrupo) maxNivelGrupo = heroe.level;
        }
        statsF2.nivelPeligroso = (statsF2.level > maxNivelGrupo);

        nucifera2.transform.position = enemyPositions[0].position; 
        
        Vector3 esc = nucifera2.transform.localScale;
        nucifera2.transform.localScale = new Vector3(-Mathf.Abs(esc.x), esc.y, esc.z);
        statsF2.ActivarUICombate();
        if (statsF2.canvasUI != null) {
            Vector3 cEsc = statsF2.canvasUI.transform.localScale;
            statsF2.canvasUI.transform.localScale = new Vector3(-Mathf.Abs(cEsc.x), cEsc.y, cEsc.z);
        }

        activeEnemies.Add(nucifera2);
        
        if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayMusicaCombateFinal", SendMessageOptions.DontRequireReceiver);
        if (panelNegroTransicion != null) panelNegroTransicion.SetActive(false);

        if (panelTituloJefe != null) {
            panelTituloJefe.SetActive(true);
            if (textoTituloJefe != null) textoTituloJefe.text = "<color=#8B0000><size=130%><b>NUCIFERA</b></size></color>\n<size=50%><i><color=#32CD32>Flor de la Oscuridad</color></i></size>";
        }
        yield return new WaitForSecondsRealtime(3f);
        if (panelTituloJefe != null) panelTituloJefe.SetActive(false);

        foreach (HeroStats heroe in listaParty) {
            heroe.currentHP = heroe.maxHP;
            heroe.currentEnergy = heroe.maxEnergy;
            
            heroe.gameObject.SetActive(true);
            SpriteRenderer sr = heroe.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
            Transform canvasNombre = heroe.transform.Find("Canvas_NombreHeroe");
            if (canvasNombre != null) canvasNombre.gameObject.SetActive(true);
            
            LimpiarMuerteHeroe(heroe);
        }
        ActualizarPantallaVida();

        DialogoCombateUI.Instance.AgregarMensaje("<color=#32CD32>La Fuente de los Sueños ha restaurado las fuerzas de los heroes...</color>");
        yield return new WaitForSecondsRealtime(2.5f);

        if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayRugido", SendMessageOptions.DontRequireReceiver);
        yield return StartCoroutine(ShakeCamera(2f, 0.15f));

        DialogoCombateUI.Instance.AgregarMensaje("<color=red>Nucifera ha florecido gracias al poder del rey demonio.</color>");
        yield return new WaitForSecondsRealtime(2f);

        nuciferaFase2Iniciada = true;
        enCombateActivo = true;
        ConfirmarFormacion(); 
    }

    IEnumerator ShakeCamera(float duration, float magnitude) {
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0.0f;
        while (elapsed < duration) {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Camera.main.transform.position = originalPos;
    }

    IEnumerator SaltarTurnoAtrapado(HeroStats hero) {
        yield return new WaitForSecondsRealtime(1.5f);
        DialogoCombateUI.Instance.AgregarMensaje($"¡<color=#00BFFF>{hero.unitName}</color> logró liberarse, pero perdió su turno!");
        
        Transform enredadera = hero.transform.Find("EnredaderaTrampa");
        if (enredadera != null) enredadera.gameObject.SetActive(false);
        
        yield return new WaitForSecondsRealtime(1.5f);
        AvanzarTurno();
    }

    IEnumerator MantenerEnredaderaVisible(HeroStats hero) {
        Transform enredadera = hero.transform.Find("EnredaderaTrampa");
        if (enredadera == null) yield break;
        
        SpriteRenderer sr = enredadera.GetComponent<SpriteRenderer>();

        while (hero != null && hero.currentHP > 0 && heroesAtrapados.ContainsKey(hero) && heroesAtrapados[hero]) {
            enredadera.gameObject.SetActive(true);
            if (sr != null) {
                sr.enabled = true;
                sr.color = Color.white;
            }
            yield return null;
        }
    }
    
}