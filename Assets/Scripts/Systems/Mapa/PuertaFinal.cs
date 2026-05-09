using UnityEngine;

public class PuertaFinal : MonoBehaviour {
    [Header("Objetos Visuales de la Puerta")]
    public GameObject objetoPuertaCerrada;
    public GameObject objetoPuertaAbierta;
    
    private BoxCollider2D colisionMuro;
    private Transform player;

    [Header("Interacción")]
    public float distanciaInteraccion = 2.5f;
    private bool puertaAbierta = false;

    [Header("Referencias UI")]
    public GameObject contenedorPrincipal; // <-- NUEVO: El panel oscuro que cubre todo
    public GameObject panelPregunta;
    public GameObject panelSinLlave;
    public GameObject panelVictoria;

    void Start() {
        colisionMuro = GetComponent<BoxCollider2D>();
        
        objetoPuertaCerrada.SetActive(true);
        objetoPuertaAbierta.SetActive(false);
        colisionMuro.isTrigger = false; 
        
        // Ahora apagamos TODO al iniciar (incluso el escudo que bloquea los clics)
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false); 
        if(panelPregunta) panelPregunta.SetActive(false);
        if(panelSinLlave) panelSinLlave.SetActive(false);
        if(panelVictoria) panelVictoria.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (puertaAbierta || player == null || Time.timeScale == 0) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                AbrirDialogoPregunta();
            }
        }
    }

    // --- FUNCIONES PARA LOS BOTONES DE LA UI ---

    public void AbrirDialogoPregunta() {
        Time.timeScale = 0f; 
        if(contenedorPrincipal) contenedorPrincipal.SetActive(true); // Encendemos el escudo
        panelPregunta.SetActive(true);
    }

    public void CerrarDialogos() {
        panelPregunta.SetActive(false);
        panelSinLlave.SetActive(false);
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false); // Apagamos el escudo
        Time.timeScale = 1f; 
    }

    public void IntentarAbrir() {
        panelPregunta.SetActive(false);

        if (InventarioEnum.Instance != null && InventarioEnum.Instance.GetCantidad(Item.Llave) > 0) {
            AbrirPuerta();
        } else {
            panelSinLlave.SetActive(true); 
        }
    }

    // --- LÓGICA INTERNA ---

    void AbrirPuerta() {
        puertaAbierta = true;
        
        objetoPuertaCerrada.SetActive(false);
        objetoPuertaAbierta.SetActive(true);
        
        if (InventarioEnum.Instance != null) {
            InventarioEnum.Instance.RemoveItem(Item.Llave, 1);
        }

        colisionMuro.isTrigger = true; 
        
        // Como ya la abrimos, quitamos la interfaz oscura para poder caminar
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false); 
        
        Time.timeScale = 1f; 
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (puertaAbierta && other.CompareTag("Player")) {
            Time.timeScale = 0f; 
            
            // Volvemos a poner la interfaz oscura para la pantalla final
            if(contenedorPrincipal) contenedorPrincipal.SetActive(true); 
            panelVictoria.SetActive(true);
        }
    }
}