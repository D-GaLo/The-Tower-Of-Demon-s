using UnityEngine;

public class FuenteCurativa : MonoBehaviour {
    [Header("Visuales de la Fuente")]
    public GameObject objetoFuenteLlena;
    public GameObject objetoFuenteVacia;

    [Header("Configuración")]
    public float distanciaInteraccion = 2f;
    public int combatesParaRecargar = 2; // Número de batallas necesarias para que vuelva a haber agua
    
    private bool fuenteVacia = false;
    private int combatesAlVaciar = 0; // Guarda el récord de batallas que llevabas al beber
    private Transform player;

    [Header("UI - Paneles (Con texto pre-hecho)")]
    public GameObject contenedorPrincipal; 
    public GameObject panelPregunta; 
    public GameObject panelCurado;   
    public GameObject panelSeca; // <-- NUEVO: "La fuente no tiene agua en este momento"

    void Start() {
        if (objetoFuenteLlena) objetoFuenteLlena.SetActive(true);
        if (objetoFuenteVacia) objetoFuenteVacia.SetActive(false);

        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelCurado) panelCurado.SetActive(false);
        if (panelSeca) panelSeca.SetActive(false); // Apagamos el nuevo panel

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (player == null || Time.timeScale == 0) return;

        // 1. REVISAR SI DEBEMOS RECARGAR LA FUENTE
        if (fuenteVacia && GameFlowController.Instance != null) {
            // Si las batallas actuales son mayores o iguales a (las que tenías + las que te pide)
            if (GameFlowController.Instance.combatesCompletados >= (combatesAlVaciar + combatesParaRecargar)) {
                RecargarFuente();
            }
        }

        // 2. DETECTAR INTERACCIÓN
        if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                // Si la fuente tiene agua, preguntamos si quiere beber. Si no, le mostramos el panel de rechazo.
                if (!fuenteVacia) {
                    AbrirPanel(panelPregunta);
                } else {
                    AbrirPanel(panelSeca);
                }
            }
        }
    }

    // --- FUNCIONES INTERNAS ---

    void RecargarFuente() {
        fuenteVacia = false;
        if (objetoFuenteLlena) objetoFuenteLlena.SetActive(true);
        if (objetoFuenteVacia) objetoFuenteVacia.SetActive(false);
        Debug.Log("<color=cyan>[Fuente]</color> Magia pura: La fuente se ha recargado de agua.");
    }

    void AbrirPanel(GameObject panelSeleccionado) {
        Time.timeScale = 0f; 
        if (contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if (panelSeleccionado) panelSeleccionado.SetActive(true);
    }

    // --- FUNCIONES PARA LOS BOTONES DE LA INTERFAZ ---

    // Va en el botón "SÍ" del panelPregunta
    public void BeberAgua() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayFuente();
        fuenteVacia = true;
        
        // Guardamos el número de batallas que llevas en este momento exacto
        if (GameFlowController.Instance != null) {
            combatesAlVaciar = GameFlowController.Instance.combatesCompletados;
        }

        if (objetoFuenteLlena) objetoFuenteLlena.SetActive(false);
        if (objetoFuenteVacia) objetoFuenteVacia.SetActive(true);

        if (panelPregunta) panelPregunta.SetActive(false);

        CurarEquipo();

        if (panelCurado) panelCurado.SetActive(true);
    }

    void CurarEquipo() {
        HeroStats[] heroes = FindObjectsOfType<HeroStats>();
        foreach(HeroStats heroe in heroes) {
            heroe.currentHP = heroe.maxHP;
            heroe.currentEnergy = heroe.maxEnergy; 
            
            HeroUI ui = heroe.GetComponent<HeroUI>();
            if (ui != null) ui.ConfigurarUI(heroe); 
        }
    }

    // Va en los botones "NO" (panelPregunta), "ACEPTAR" (panelCurado) y "ACEPTAR" (panelSeca)
    public void CerrarDialogos() {
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelCurado) panelCurado.SetActive(false);
        if (panelSeca) panelSeca.SetActive(false);
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        
        Time.timeScale = 1f; 
    }
}