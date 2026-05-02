using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private Rigidbody2D rb;
    public Transform player;
    public float detectionRange = 5f; 
    public float speed = 2f;


    [Header("Puntos de patrulla")]
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 currentTarget;

    void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
    }

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
        }else {
            Move();
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

    public void ConfigurarPatrulla(Vector3 pA, Vector3 pB) {
        pointA = pA;
        pointB = pB;
        currentTarget = pointB; 

        Debug.Log($"{gameObject.name} configurado en: {pointA} y {pointB}");
    }

    void Move(){
        if (currentTarget == null || currentTarget == Vector3.zero) return;

        float direction= Mathf.Sign(currentTarget.x- transform.position.x);
        rb.velocity=new Vector2(direction *speed, rb.velocity.y);

        if(Mathf.Abs(transform.position.x - currentTarget.x) < 0.1f){
            SwitchTarget();
        }
        Flip(direction);
    }


    void Flip(float direction){
        if(direction==0)return;

        Vector3 scale= transform.localScale;
        scale.x= Mathf.Sign(direction) *Mathf.Abs(scale.x);
        transform.localScale=scale;
    }

    void SwitchTarget(){
        if(currentTarget==pointA){
            currentTarget=pointB;
        }else{
            currentTarget=pointA;
        }
    }


}