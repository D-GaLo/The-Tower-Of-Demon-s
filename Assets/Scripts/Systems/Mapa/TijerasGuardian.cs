using UnityEngine;

public class TijerasGuardian : MonoBehaviour {
    [Header("Referencias del Mundo")]
    public GameObject slimePadre; 
    public GameObject visualTijerasConBaba;
    public GameObject visualTijerasLimpias; 

    [Header("Interacción")]
    public float distanciaInteraccion = 2.5f;
    private Transform player;

    [Header("Interfaz de Mensajes")]
    public GameObject contenedorPrincipal; 
    public GameObject panelMensajeVictoria; 
    public GameObject panelPregunta;        
    public GameObject panelObtenidas;       
    public GameObject panelMensajeOculto;   

    private bool jefeDerrotado = false;
    private bool tijerasRecogidas = false;

    void Start() {
        if(visualTijerasConBaba) visualTijerasConBaba.SetActive(true);
        if(visualTijerasLimpias) visualTijerasLimpias.SetActive(false);
        
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false);
        if(panelMensajeVictoria) panelMensajeVictoria.SetActive(false);
        if(panelPregunta) panelPregunta.SetActive(false);
        if(panelObtenidas) panelObtenidas.SetActive(false);
    
        if(panelMensajeOculto) panelMensajeOculto.SetActive(false); 

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update() {
        if (Time.timeScale == 0) return; 

        if (slimePadre == null && !jefeDerrotado) {
            jefeDerrotado = true;
            
            if(visualTijerasConBaba) visualTijerasConBaba.SetActive(false);
            if(visualTijerasLimpias) visualTijerasLimpias.SetActive(true);
            
            MostrarPanel(panelMensajeVictoria);
            return; 
        }

        if (!tijerasRecogidas && player != null) {
            if (Vector2.Distance(transform.position, player.position) <= distanciaInteraccion) {
                if (Input.GetKeyDown(KeyCode.E)) {
                    
                    if (jefeDerrotado) {
                        MostrarPanel(panelPregunta);
                    } else {
                        MostrarPanel(panelMensajeOculto);
                    }

                }
            }
        }
    }

    void MostrarPanel(GameObject panel) {
        Time.timeScale = 0f; 
        if(contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if(panel) panel.SetActive(true);
    }

    public void BotonTomarTijeras() {
        if(panelPregunta) panelPregunta.SetActive(false);
        
        tijerasRecogidas = true;
        if(visualTijerasLimpias) visualTijerasLimpias.SetActive(false); 

        if (InventarioEnum.Instance != null) {
            InventarioEnum.Instance.AddItem(Item.Tijeras, 1);
        }

        if(panelObtenidas) panelObtenidas.SetActive(true);
    }

    public void CerrarDialogos() {
        if(panelMensajeVictoria) panelMensajeVictoria.SetActive(false);
        if(panelPregunta) panelPregunta.SetActive(false);
        if(panelObtenidas) panelObtenidas.SetActive(false);
        if(panelMensajeOculto) panelMensajeOculto.SetActive(false); 
        
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false);
        
        Time.timeScale = 1f; 
        
        if (tijerasRecogidas) {
            Destroy(gameObject); 
        }
    }
}