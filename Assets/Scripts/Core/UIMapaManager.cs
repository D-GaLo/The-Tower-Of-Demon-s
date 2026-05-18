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

    [Tooltip("Arrastrar aquí el botón de Estadísticas")]
    public GameObject botonEstadisticas; 

    private bool mapaActivo = false;

    void Start() {
        if (panelMapa != null) panelMapa.SetActive(false);
    }

    public void ToggleMapa() {
        mapaActivo = !mapaActivo;
        panelMapa.SetActive(mapaActivo);

        if (mapaActivo) {
            Time.timeScale = 0f;
            if (botonEstadisticas != null) botonEstadisticas.SetActive(false);
            ActualizarUbicacionPuntero();
        } else {
            Time.timeScale = 1f;
            if (botonEstadisticas != null) botonEstadisticas.SetActive(true);
            
        }
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