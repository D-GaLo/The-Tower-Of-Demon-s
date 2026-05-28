using UnityEngine;

public class UIMapaManager : MonoBehaviour {
    [Header("Referencias UI")]
    public GameObject panelMapa;
    public RectTransform punteroJugador;

    [Header("Sincronización de Salas")]
    [Tooltip("Arrastrar aquí los RoomCenters del mundo 2D")]
    public Transform[] centrosMundo; 
    
    [Tooltip("Arrastrar aquí los objetos centrales del mapaUI en el mismo orden que los RoomCenters del mundo")]
    public RectTransform[] centrosUI; 


     private bool mapaActivo = false;

    void Start() {
        if (panelMapa != null) panelMapa.SetActive(false);
    }

    void Update() {
        if (mapaActivo && Input.GetKeyDown(KeyCode.Escape)) {
            CerrarMapa();
        }
    }

    public void ToggleMapa() {
        if (mapaActivo) {
            CerrarMapa();
        } else {
            AbrirMapa();
        }
    }

    public void AbrirMapa() {
        mapaActivo = true;
        panelMapa.SetActive(true);

        Time.timeScale = 0f;
        ActualizarUbicacionPuntero();
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
    }

    public void CerrarMapa() {
        mapaActivo = false;
        panelMapa.SetActive(false);
        
        if (Menu.Instance == null || !Menu.Instance.estaPausado) {
            Time.timeScale = 1f; 
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
    }

    void ActualizarUbicacionPuntero() {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        int indiceSalaActual = 0;
        float distanciaMenor = Mathf.Infinity;

        for (int i = 0; i < centrosMundo.Length; i++) {
            if (centrosMundo[i] == null) continue;

            float distancia = Vector2.Distance(p.transform.position, centrosMundo[i].position);
            if (distancia < distanciaMenor) {
                distanciaMenor = distancia;
                indiceSalaActual = i;
            }
        }

        if (centrosUI.Length > indiceSalaActual && centrosUI[indiceSalaActual] != null) {
            punteroJugador.position = centrosUI[indiceSalaActual].position;
            punteroJugador.SetAsLastSibling();
        }
    }
}