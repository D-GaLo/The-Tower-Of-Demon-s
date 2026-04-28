using UnityEngine;

// Este script solo sirve para "marcar" el objeto
public class RoomCenter : MonoBehaviour {
    // Puedes agregar un Gizmo para verlo en el editor pero que sea invisible en el juego
    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(18, 10, 0));
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}