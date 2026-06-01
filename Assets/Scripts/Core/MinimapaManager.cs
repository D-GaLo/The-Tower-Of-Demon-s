using UnityEngine;

public class MinimapaManager : MonoBehaviour {
    [Header("Referencias UI del Minimapa")]
    public RectTransform punteroJugador;

    [Header("Sincronización de Salas")]
    [Tooltip("Arrastrar aquí los RoomCenters del mundo 2D (Los mismos del mapa grande)")]
    public Transform[] centrosMundo; 
    
    [Tooltip("Arrastrar aquí los objetos centrales de las salas, pero los del MINIMAPA UI")]
    public RectTransform[] centrosUI; 

    void Update() {
        if (Time.timeScale == 0) return;
        
        ActualizarUbicacionPuntero();
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