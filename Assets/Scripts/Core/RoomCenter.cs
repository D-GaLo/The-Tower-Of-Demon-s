using UnityEngine;

public class RoomCenter : MonoBehaviour {
    [Header("Ajustes de Cámara")]
    [Tooltip("Desplazamiento de la cámara desde el centro de la sala.")]
    public Vector2 offsetCamara = Vector2.zero;
    
    [Tooltip("Tamaño del zoom de la cámara. Mayor número = más alejado.")]
    public float tamanoCamara = 5f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.5f);

        Vector3 posicionReal = transform.position + new Vector3(offsetCamara.x, offsetCamara.y, 0);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(posicionReal, new Vector3(18, 10, 0)); 
        Gizmos.DrawLine(transform.position, posicionReal);
    }
}