using UnityEngine;

public class UIMapaManager : MonoBehaviour {
    [Header("Referencias UI")]
    public GameObject panelMapa; // El panel gris/negro de fondo
    public RectTransform punteroJugador; // El icono que indica dónde estás

    [Header("Sincronización de Salas")]
    [Tooltip("Arrastra aquí los RoomCenters del mundo 2D")]
    public Transform[] centrosMundo; 
    
    [Tooltip("Arrastra aquí los objetos centrales de tu mapa UI en EL MISMO ORDEN")]
    public RectTransform[] centrosUI; 

    private bool mapaActivo = false;

    void Start() {
        if (panelMapa != null) panelMapa.SetActive(false);
    }

    // Esta función la pones en el OnClick() del Botón
    public void ToggleMapa() {
        mapaActivo = !mapaActivo;
        panelMapa.SetActive(mapaActivo);

        if (mapaActivo) {
            Time.timeScale = 0f; // Pausamos el juego
            ActualizarUbicacionPuntero();
        } else {
            Time.timeScale = 1f; // Reanudamos el juego
        }
    }

    void ActualizarUbicacionPuntero() {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        int indiceSalaActual = 0;
        float distanciaMenor = Mathf.Infinity;

        // 1. Averiguamos en qué índice de nuestra lista está el jugador en el mundo real
        for (int i = 0; i < centrosMundo.Length; i++) {
            if (centrosMundo[i] == null) continue;

            float distancia = Vector2.Distance(p.transform.position, centrosMundo[i].position);
            if (distancia < distanciaMenor) {
                distanciaMenor = distancia;
                indiceSalaActual = i; // Guardamos el número de la sala ganadora
            }
        }

        // 2. Movemos el puntero al cuadrito UI que tenga ESE MISMO índice
        if (centrosUI.Length > indiceSalaActual && centrosUI[indiceSalaActual] != null) {
            // Mueve el puntero exactamente a la posición del centro UI
            punteroJugador.position = centrosUI[indiceSalaActual].position;
            punteroJugador.SetAsLastSibling(); // Lo pone al frente para que no quede detrás del mapa
        }
    }
}