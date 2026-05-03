using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private Rigidbody2D rb;
    private Transform player; 

    [Header("Persecución")]
    public float detectionRange = 5f; 
    public float speed = 2f;

    [Header("Puntos de patrulla")]
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 currentTarget;

    private bool isReturning = false; 

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) {
            player = p.transform;
        }
    }

    public void ConfigurarPatrulla(Vector3 pA, Vector3 pB) {
        pointA = pA;
        pointB = pB;
        currentTarget = pointB; 
    }

    void FixedUpdate() {
        if (player == null || Time.timeScale == 0) {
            rb.velocity = Vector2.zero; 
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange) {
            // ESTADO 1: PERSECUCIÓN
            isReturning = true; 
            Perseguir();
        } else if (isReturning) {
            // ESTADO 2: REGRESAR A LA PATRULLA
            RegresarAPatrulla();
        } else {
            // ESTADO 3: PATRULLA NORMAL
            Patrullar();
        }
    }

    void Perseguir() {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
        Flip(direction.x);
    }

    void RegresarAPatrulla() {
        if (currentTarget == null || currentTarget == Vector3.zero) return;

        Vector2 direction = (currentTarget - transform.position).normalized;
        rb.velocity = direction * speed;
        Flip(direction.x);

        // Si ya está lo suficientemente cerca de su punto de retorno
        if (Vector2.Distance(transform.position, currentTarget) < 0.1f) {
            // FIX: Lo "encajamos" exactamente en la coordenada para evitar desvíos milimétricos
            transform.position = currentTarget;
            rb.velocity = Vector2.zero; // FIX: Matamos la inercia por completo
            isReturning = false;
            SwitchTarget(); 
        }
    }

    void Patrullar() {
        if (currentTarget == null || currentTarget == Vector3.zero) return;

        float directionX = Mathf.Sign(currentTarget.x - transform.position.x);
        
        // FIX: Forzamos la velocidad Y a 0 para que deje de patinar hacia arriba o abajo
        rb.velocity = new Vector2(directionX * speed, 0f);

        if (Mathf.Abs(transform.position.x - currentTarget.x) < 0.1f) {
            // FIX: Lo mantenemos firme en su carril X
            transform.position = new Vector3(currentTarget.x, transform.position.y, transform.position.z);
            SwitchTarget();
        }
        Flip(directionX);
    }

    void Flip(float directionX) {
        if (Mathf.Abs(directionX) < 0.01f) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void SwitchTarget() {
        if (currentTarget == pointA) {
            currentTarget = pointB;
        } else {
            currentTarget = pointA;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && Time.timeScale != 0) {
            if (GameFlowController.Instance != null) {
                rb.velocity = Vector2.zero; 
                GameFlowController.Instance.IniciarCombate(gameObject);
            }
        }
    }
}