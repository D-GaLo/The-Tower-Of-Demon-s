using UnityEngine;

public class Enredaderas : MonoBehaviour {
    private Transform player;
    private BoxCollider2D colisionMuro;

    [Header("Configuración")]
    public float distanciaInteraccion = 2.5f;

    [Header("Referencias UI (Reutilizadas)")]
    public GameObject contenedorPrincipal; // El panel oscuro
    public GameObject panelPregunta;        // "¿Quieres cortar las enredaderas?"
    public GameObject panelSinTijeras;      // "No tienes con qué cortar esto"

    void Start() {
        colisionMuro = GetComponent<BoxCollider2D>();

        // Aseguramos que la UI empiece apagada
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelSinTijeras) panelSinTijeras.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (player == null || Time.timeScale == 0) return;

        // Detectar interacción con E
        if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                AbrirDialogoPregunta();
            }
        }
    }

    // --- FUNCIONES DE INTERFAZ ---

    public void AbrirDialogoPregunta() {
        Time.timeScale = 0f; // Pausar juego
        if (contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if (panelPregunta) panelPregunta.SetActive(true);
    }

    public void CerrarDialogos() {
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelSinTijeras) panelSinTijeras.SetActive(false);
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        Time.timeScale = 1f; // Reanudar juego
    }

public void IntentarCortar() {
        if (panelPregunta) panelPregunta.SetActive(false);

        // 1. Verificamos si el inventario existe en el mundo
        if (InventarioEnum.Instance == null) {
            Debug.LogError("<color=red>[ERROR]</color> No hay ningún InventarioEnum en la escena. ¡Pon el script en un objeto vacío!");
            if (panelSinTijeras) panelSinTijeras.SetActive(true);
            return;
        }

        // 2. Le preguntamos al inventario cuántas tijeras tiene exactamente
        int cantidadTijeras = InventarioEnum.Instance.GetCantidad(Item.Tijeras);
        Debug.Log($"<color=yellow>[Inventario]</color> Tienes exactamente {cantidadTijeras} tijeras en tu bolsa.");

        // 3. Tomamos la decisión
        if (cantidadTijeras > 0) {
            Cortar();
        } else {
            if (panelSinTijeras) panelSinTijeras.SetActive(true);
        }
    }

    void Cortar() {
        // Quitamos la pausa
        Time.timeScale = 1f;
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);

        // Desactivamos el objeto (la imagen y la colisión se van)
        Debug.Log("¡Enredaderas cortadas!");
        gameObject.SetActive(false); 
    }
}