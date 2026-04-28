using UnityEngine;
using System.Collections.Generic;

public class RoomCamera : MonoBehaviour {
    public Transform player;
    public float suavizado = 8f;

    private RoomCenter[] todosLosCentros;
    private Transform centroActual;

    void Start() {
        // Buscamos todos los centros que existan en el mapa al iniciar
        todosLosCentros = FindObjectsOfType<RoomCenter>();
    }

    void LateUpdate() {
        if (player == null) return;

        // Buscamos cuál es el centro más cercano a Sieg
        centroActual = ObtenerCentroMasCercano();

        if (centroActual != null) {
            // La cámara viaja hacia la posición de ese objeto
            Vector3 targetPosition = new Vector3(centroActual.position.x, centroActual.position.y, -10f);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * suavizado);
        }
    }

    Transform ObtenerCentroMasCercano() {
        Transform masCercano = null;
        float distanciaMenor = Mathf.Infinity;
        Vector3 posicionJugador = player.position;

        foreach (RoomCenter centro in todosLosCentros) {
            float distancia = Vector3.Distance(posicionJugador, centro.transform.position);
            if (distancia < distanciaMenor) {
                distanciaMenor = distancia;
                masCercano = centro.transform;
            }
        }

        return masCercano;
    }
}