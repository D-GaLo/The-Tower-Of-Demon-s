using UnityEngine;

public class Enredaderas : MonoBehaviour {
    private Transform player;
    private BoxCollider2D colisionMuro;

    [Header("Configuración")]
    public float distanciaInteraccion = 2.5f;

    [Header("Referencias UI (Reutilizadas)")]
    public GameObject contenedorPrincipal;
    public GameObject panelPregunta;
    public GameObject panelSinTijeras;

    void Start() {
        colisionMuro = GetComponent<BoxCollider2D>();

        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelSinTijeras) panelSinTijeras.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (player == null || Time.timeScale == 0) return;

        if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
            if (Input.GetKeyDown(KeyCode.E)) {
                InteraccionHUD();
            }
        }
    }

    public void  InteraccionHUD() {
        AbrirDialogoPregunta();
    }

    public void AbrirDialogoPregunta() {
        Time.timeScale = 0f;
        if (contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if (panelPregunta) panelPregunta.SetActive(true);
    }

    public void CerrarDialogos() {
        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelSinTijeras) panelSinTijeras.SetActive(false);
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        Time.timeScale = 1f;
    }

    public void IntentarCortar() {
        if (panelPregunta) panelPregunta.SetActive(false);

        if (InventarioEnum.Instance == null) {
            Debug.LogError("<color=red>[ERROR]</color> No hay ningún InventarioEnum en la escena. ¡Pon el script en un objeto vacío!");
            if (panelSinTijeras) panelSinTijeras.SetActive(true);
            return;
        }

        int cantidadTijeras = InventarioEnum.Instance.GetCantidad(Item.Tijeras);

        if (cantidadTijeras > 0) {
            Cortar();
        } else {
            if (panelSinTijeras) panelSinTijeras.SetActive(true);
        }
    }

    void Cortar() {
        Time.timeScale = 1f;
        if (contenedorPrincipal) contenedorPrincipal.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTijeras();

        Debug.Log("¡Enredaderas cortadas!");
        gameObject.SetActive(false); 
    }
}