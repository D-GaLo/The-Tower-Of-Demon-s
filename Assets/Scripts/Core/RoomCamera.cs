using UnityEngine;

public class RoomCamera : MonoBehaviour {
    public Transform player;
    public float suavizado = 8f;

    private RoomCenter[] todosLosCentros;
    private Transform centroActual;
    private bool enCombate = false;

    void Start() {
        // Buscamos todos los centros que existan en el mapa al iniciar
        todosLosCentros = FindObjectsOfType<RoomCenter>();
    }

    // Función llamada por el GameFlowController
    public void CambiarModoCombate(bool estado, Vector3 posicionCombate) {
        enCombate = estado;
        if (enCombate) {
            // Teletransporte instantáneo a la arena
            transform.position = posicionCombate;
        } else {
            // Al volver del combate, la cámara salta instantáneamente a la sala actual
            if (player != null) {
                Transform masCercano = ObtenerCentroMasCercano();
                if (masCercano != null) {
                    transform.position = new Vector3(masCercano.position.x, masCercano.position.y, -10f);
                }
            }
        }
    }

    void LateUpdate() {
        if (player == null || enCombate) return; 

        // Buscamos cuál es el centro más cercano a Sieg
        centroActual = ObtenerCentroMasCercano();

        if (centroActual != null) {
            Vector3 targetPosition = new Vector3(centroActual.position.x, centroActual.position.y, -10f);
            // Usamos unscaledDeltaTime por si acaso hay pausas de tiempo
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * suavizado);
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