using UnityEngine;

public class EnemyAI : MonoBehaviour {
    public Transform player;
    public float detectionRange = 5f; 
    public float speed = 2f;

    // --- El candado anti-spam de clones ---
    private bool combateIniciado = false;

    void Start() {
        if (player == null) {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null) player = p.transform;
        }
    }

    void Update() {
        // Si el tiempo está detenido (en combate), el enemigo no se mueve
        if (player == null || Time.timeScale == 0) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange) {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            // Solo iniciamos si NO se ha iniciado ya y si el juego no está pausado
            if (!combateIniciado && Time.timeScale != 0) {
                combateIniciado = true; // Bloqueamos la puerta para que no se hagan clones
                
                if (GameFlowController.Instance != null) {
                    GameFlowController.Instance.IniciarCombate(gameObject);
                }
            }
        }
    }
}