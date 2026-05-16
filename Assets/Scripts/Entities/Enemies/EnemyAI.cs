using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private Rigidbody2D rb;
    private Transform player; 

    [Header("Configuración Especial")]
    [Tooltip("Si se activa, el Spawner no podrá cambiar sus puntos de patrulla.")]
    public bool esEnemigoEspecial = false; 

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

        if (currentTarget == Vector3.zero) {
            if (pointA != Vector3.zero || pointB != Vector3.zero) {
                currentTarget = pointB;
                Debug.Log($"[EnemyAI] {gameObject.name} iniciando patrulla manual entre {pointA} y {pointB}");
            }
        }
    }

    public void ConfigurarPatrulla(Vector3 pA, Vector3 pB) {
        if (esEnemigoEspecial) return; 

        pointA = pA;
        pointB = pB;
        currentTarget = pointB; 
    }

    void FixedUpdate() {
        if (player == null || Time.timeScale == 0) {
            rb.velocity = Vector2.zero; 
            return;
        }

        if (GameFlowController.Instance != null && GameFlowController.Instance.uiCombate != null && GameFlowController.Instance.uiCombate.activeSelf) {
            rb.velocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange) {
            isReturning = true; 
            Perseguir();
        } else if (isReturning) {
            RegresarAPatrulla();
        } else {
            Patrullar();
        }
    }

    void Perseguir() {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
        Flip(direction.x);
    }

    void RegresarAPatrulla() {
        if (currentTarget == Vector3.zero) return;

        Vector2 direction = (currentTarget - transform.position).normalized;
        rb.velocity = direction * speed;
        Flip(direction.x);

        if (Vector2.Distance(transform.position, currentTarget) < 0.1f) {
            transform.position = currentTarget;
            rb.velocity = Vector2.zero;
            isReturning = false;
            SwitchTarget(); 
        }
    }

    void Patrullar() {
        if (currentTarget == Vector3.zero) return;

        float directionX = Mathf.Sign(currentTarget.x - transform.position.x);
        rb.velocity = new Vector2(directionX * speed, 0f);

        if (Mathf.Abs(transform.position.x - currentTarget.x) < 0.1f) {
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