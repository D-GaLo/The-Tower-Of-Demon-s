using UnityEngine;

public class EnredaderasNucifera : MonoBehaviour {
    [Header("Referencias del Mundo")]
    public GameObject nucifera;
    public GameObject imagenEnredaderas; 
    public GameObject imagenLlave;
    
    private BoxCollider2D colisionMuro; 

    [Header("Configuración")]
    public float distanciaInteraccion = 2.5f;

    [Header("UI - Paneles (Con texto pre-hecho)")]
    public GameObject contenedorPrincipal; 
    public GameObject panelVictoria;
    public GameObject panelPregunta;
    public GameObject panelSinTijeras;
    public GameObject panelNucifera;
    public GameObject panelLlave;

    private Transform player;
    
    private bool mensajeNuciferaMostrado = false;
    private bool enredaderasCortadas = false;
    private bool llaveRecogida = false;

    void Start() {
        colisionMuro = GetComponent<BoxCollider2D>();

        if (imagenEnredaderas) imagenEnredaderas.SetActive(true);
        if (imagenLlave) imagenLlave.SetActive(false);
        
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

        if (nucifera == null && !mensajeNuciferaMostrado) {
            mensajeNuciferaMostrado = true;
            MostrarPanel(panelVictoria);
            return; 
        }

        if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                ProcesarInteraccion();
            }
        }
    }

    void ProcesarInteraccion() {
        if (llaveRecogida) return;

        if (nucifera != null) {
            MostrarPanel(panelNucifera);
        } 
        else if (!enredaderasCortadas) {
            MostrarPanel(panelPregunta);
        } 
        else {
            RecogerLlave();
        }
    }

    void MostrarPanel(GameObject panelAAbrir) {
        Time.timeScale = 0f;
        if (contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if (panelAAbrir) panelAAbrir.SetActive(true);
    }

    public void IntentarCortar() {
        if (panelPregunta) panelPregunta.SetActive(false);

        if (InventarioEnum.Instance != null && InventarioEnum.Instance.GetCantidad(Item.Tijeras) > 0) {
            enredaderasCortadas = true;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayTijeras();
            
            if (imagenEnredaderas) imagenEnredaderas.SetActive(false);
            if (imagenLlave) imagenLlave.SetActive(true);
            if (colisionMuro) colisionMuro.enabled = false;

            if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
            Time.timeScale = 1f;
        } else {
            if (panelSinTijeras) panelSinTijeras.SetActive(true);
        }
    }

    void RecogerLlave() {
        llaveRecogida = true;
        if (imagenLlave) imagenLlave.SetActive(false);

        if (InventarioEnum.Instance != null) {
            InventarioEnum.Instance.AddItem(Item.Llave, 1);
        }

        MostrarPanel(panelLlave);
    }

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