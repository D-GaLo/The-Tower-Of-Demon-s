using UnityEngine;

public class RoomCamera : MonoBehaviour {
    public Transform player;
    
    [Header("Dimensiones de la Sala")]
    public float roomWidth = 18f;  // Ancho de la sala 
    public float roomHeight = 10f; // Alto de la sala (según tu indicación)

    [Header("Configuración de Seguimiento")]
    public float suavizado = 8f;

    void LateUpdate() {
        if (player == null) return;

        // Calculamos el índice de la sala en X (cada 18 unidades)
        float indexX = Mathf.Round(player.position.x / roomWidth);
        
        // Calculamos el índice de la sala en Y (cada 10 unidades)
        float indexY = Mathf.Round(player.position.y / roomHeight);
        
        // El centro exacto de la sala donde se encuentra el jugador
        float targetX = indexX * roomWidth;
        float targetY = indexY * roomHeight;
        
        // Mantener Z en -10 para que no se pegue al plano 2D
        Vector3 targetPosition = new Vector3(targetX, targetY, -10f);
        
        // Desplazamiento suave hacia el centro de la nueva sala
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * suavizado);
    }
}