using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour {
    public Transform player;
    public float detectionRange = 5f; 
    public float speed = 2f;

    void Start() {
        // Si no asignaste el jugador, intenta buscarlo por Tag
        if (player == null) {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null) player = p.transform;
        }
    }

    void Update() {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange) {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Debug.Log("El enemigo te alcanzó. ¡Modo combate!");
            SceneManager.LoadScene("Combate"); // Cambio a escena de combate 
        }
    }
}