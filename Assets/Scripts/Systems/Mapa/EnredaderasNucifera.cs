using UnityEngine;

public class EnredaderasNucifera : MonoBehaviour {
    [Header("Referencias del Mundo")]
    public GameObject nucifera; // El enemigo Nucifera
    public GameObject imagenEnredaderas; 
    public GameObject imagenLlave;
    
    private BoxCollider2D colisionMuro; 

    [Header("Configuración")]
    public float distanciaInteraccion = 2.5f;

    [Header("UI - Paneles (Con texto pre-hecho)")]
    public GameObject contenedorPrincipal; 
    public GameObject panelVictoria;   // Aparece al morir Nucifera
    public GameObject panelPregunta;   // "¿Quieres cortar las enredaderas?"
    public GameObject panelSinTijeras; // "No tienes tijeras"
    public GameObject panelNucifera;   // "Nucifera ha fortalecido..."
    public GameObject panelLlave;      // "¡Obtuviste la llave!"

    private Transform player;
    
    // Banderas de estado
    private bool mensajeNuciferaMostrado = false;
    private bool enredaderasCortadas = false;
    private bool llaveRecogida = false;

    void Start() {
        colisionMuro = GetComponent<BoxCollider2D>();

        // Estado visual inicial
        if (imagenEnredaderas) imagenEnredaderas.SetActive(true);
        if (imagenLlave) imagenLlave.SetActive(false);
        
        // Apagar UI por seguridad al iniciar
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        if (panelVictoria) panelVictoria.SetActive(false);
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelSinTijeras) panelSinTijeras.SetActive(false);
        if (panelNucifera) panelNucifera.SetActive(false);
        if (panelLlave) panelLlave.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (player == null || Time.timeScale == 0) return;

        // EVENTO 1: Nucifera muere
        if (nucifera == null && !mensajeNuciferaMostrado) {
            mensajeNuciferaMostrado = true;
            MostrarPanel(panelVictoria);
            return; 
        }

        // EVENTO 2: Interacción con la Tecla E
        if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                ProcesarInteraccion();
            }
        }
    }

    void ProcesarInteraccion() {
        if (llaveRecogida) return;

        if (nucifera != null) {
            // Nucifera sigue vivo -> Rechazo
            MostrarPanel(panelNucifera);
        } 
        else if (!enredaderasCortadas) {
            // Nucifera muerto, enredaderas puestas -> Pregunta
            MostrarPanel(panelPregunta);
        } 
        else {
            // Enredaderas cortadas -> Recoger la llave
            RecogerLlave();
        }
    }

    // Función para mostrar cualquier panel pausando el juego
    void MostrarPanel(GameObject panelAAbrir) {
        Time.timeScale = 0f;
        if (contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if (panelAAbrir) panelAAbrir.SetActive(true);
    }

    // --- FUNCIONES PARA LOS BOTONES ---

    // Pon esta función SOLO en el botón "SÍ" del panelPregunta
    public void IntentarCortar() {
        if (panelPregunta) panelPregunta.SetActive(false);

        if (InventarioEnum.Instance != null && InventarioEnum.Instance.GetCantidad(Item.Tijeras) > 0) {
            enredaderasCortadas = true;
            
            // Cambiamos el apartado visual
            if (imagenEnredaderas) imagenEnredaderas.SetActive(false);
            if (imagenLlave) imagenLlave.SetActive(true);
            if (colisionMuro) colisionMuro.enabled = false;

            // Quitamos la UI para que el jugador vuelva a presionar 'E' y recoja la llave
            if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
            Time.timeScale = 1f;
        } else {
            // Cambiamos directo al panel de error sin quitar la pausa
            if (panelSinTijeras) panelSinTijeras.SetActive(true);
        }
    }

    void RecogerLlave() {
        llaveRecogida = true;
        if (imagenLlave) imagenLlave.SetActive(false); // La llave desaparece del mapa

        if (InventarioEnum.Instance != null) {
            InventarioEnum.Instance.AddItem(Item.Llave, 1);
        }

        MostrarPanel(panelLlave);
    }

    // Pon esta función en TODOS los botones de "Aceptar", "Cerrar", o "No"
    public void CerrarDialogos() {
        if (panelVictoria) panelVictoria.SetActive(false);
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelSinTijeras) panelSinTijeras.SetActive(false);
        if (panelNucifera) panelNucifera.SetActive(false);
        if (panelLlave) panelLlave.SetActive(false);
        
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        Time.timeScale = 1f;
    }
}