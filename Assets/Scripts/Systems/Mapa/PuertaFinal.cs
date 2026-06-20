using UnityEngine;

public class PuertaFinal : MonoBehaviour {
    [Header("Objetos Visuales")]
    public GameObject objetoPuertaCerrada;
    public GameObject objetoPuertaAbierta;
    private BoxCollider2D colisionMuro;
    private Transform player;

    [Header("Interacción")]
    public float distanciaInteraccion = 2.5f;
    private bool puertaAbierta = false;

    [Header("Referencias UI")]
    public GameObject contenedorPrincipal;
    public GameObject panelPregunta;
    public GameObject panelSinLlave;
    public GameObject panelVictoria;
    public GameObject panelCreditos; 

    void Start() {
        colisionMuro = GetComponent<BoxCollider2D>();
        
        objetoPuertaCerrada.SetActive(true);
        objetoPuertaAbierta.SetActive(false);
        colisionMuro.isTrigger = false; 
        
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false); 
        if(panelPregunta) panelPregunta.SetActive(false);
        if(panelSinLlave) panelSinLlave.SetActive(false);
        if(panelVictoria) panelVictoria.SetActive(false);
        if(panelCreditos) panelCreditos.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (puertaAbierta || player == null || Time.timeScale == 0) return;

        if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                InteraccionHUD();
            }
        }
    }

    public void InteraccionHUD() {
        Time.timeScale = 0f; 
        if(contenedorPrincipal) contenedorPrincipal.SetActive(true); 
        if(panelPregunta) panelPregunta.SetActive(true);
    }

    public void CerrarDialogos() {
        panelPregunta.SetActive(false);
        panelSinLlave.SetActive(false);
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false);
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

    void AbrirPuerta() {
        puertaAbierta = true;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayLlave();
            AudioManager.Instance.PlayPuerta();
        }
        
        objetoPuertaCerrada.SetActive(false);
        objetoPuertaAbierta.SetActive(true);
        if (InventarioEnum.Instance != null) InventarioEnum.Instance.RemoveItem(Item.Llave, 1);

        colisionMuro.isTrigger = true; 
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false); 
        Time.timeScale = 1f; 
    }

    void OnTriggerEnter2D(Collider2D other) { RevisarVictoria(other); }
    void OnTriggerStay2D(Collider2D other) { RevisarVictoria(other); }

    void RevisarVictoria(Collider2D other) {
        if (puertaAbierta && other.CompareTag("Player")) {
            Time.timeScale = 0f; 
            if(contenedorPrincipal) contenedorPrincipal.SetActive(true); 
            if(panelVictoria) panelVictoria.SetActive(true);
        }
    }

    public void BotonAceptarVictoria() {
        if(panelVictoria) panelVictoria.SetActive(false);
        if(panelCreditos) panelCreditos.SetActive(true);
        
        if (AudioManager.Instance != null) {
            AudioManager.Instance.SendMessage("PlayMusicaCreditos", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void BotonVolverAlMenu() {
        Time.timeScale = 1f;
        if (GameFlowController.Instance != null) GameFlowController.Instance.VolverAlMenuPrincipal();
    }
}