using UnityEngine;

public class RoomCamera : MonoBehaviour {
    public Transform player;
    public float suavizado = 8f;

    private RoomCenter[] todosLosCentros;
    private Transform centroActual;
    private bool enCombate = false;

    void Start() {
        todosLosCentros = FindObjectsOfType<RoomCenter>();
    }

    public void CambiarModoCombate(bool estado, Vector3 posicionCombate) {
        enCombate = estado;
        if (enCombate) {
            transform.position = posicionCombate;
        } else {
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

        centroActual = ObtenerCentroMasCercano();

        if (centroActual != null) {
            Vector3 targetPosition = new Vector3(centroActual.position.x, centroActual.position.y, -10f);
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