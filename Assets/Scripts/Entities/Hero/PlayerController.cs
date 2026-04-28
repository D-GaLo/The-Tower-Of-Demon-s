using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float speed = 5f;
    public float attackRange = 1.5f;
    public GameObject espadaVisual; 
    public float duracionAtaque = 0.15f;
    public float distanciaEspada = 0.8f; 

    private Rigidbody2D rb;
    private SpriteRenderer spriteSieg;
    private Vector2 direccionMirada = Vector2.right;
    private Vector2 inputMovimiento;
    private float escalaFija = 0.7f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        spriteSieg = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(escalaFija, escalaFija, 1f);
        
        if(espadaVisual != null) espadaVisual.SetActive(false);
    }

    void Update() {
        // Capturamos el input
        inputMovimiento.x = Input.GetAxisRaw("Horizontal");
        inputMovimiento.y = Input.GetAxisRaw("Vertical");

        ActualizarVisuales(inputMovimiento.x, inputMovimiento.y);

        if (Input.GetKeyDown(KeyCode.F)) {
            StopAllCoroutines();
            StartCoroutine(AnimacionAtaqueSemicircular());
            Atacar();
        }
    }

    void FixedUpdate() {
        // Movimiento físico profesional: evita atravesar paredes
        rb.velocity = inputMovimiento.normalized * speed;
    }

    void ActualizarVisuales(float x, float y) {
        if (x != 0 || y != 0) {
            direccionMirada = new Vector2(x, y).normalized;
            if (x > 0) spriteSieg.flipX = false;
            else if (x < 0) spriteSieg.flipX = true;
        }
    }

    IEnumerator AnimacionAtaqueSemicircular() {
        if(espadaVisual != null) {
            float anguloBase = Mathf.Atan2(direccionMirada.y, direccionMirada.x) * Mathf.Rad2Deg;
            espadaVisual.transform.localPosition = new Vector3(direccionMirada.x, direccionMirada.y, 0) * distanciaEspada;
            
            espadaVisual.SetActive(true);
            float tiempo = 0;
            while(tiempo < duracionAtaque) {
                tiempo += Time.deltaTime;
                float progresoArco = Mathf.Lerp(90, -90, tiempo / duracionAtaque);
                espadaVisual.transform.localRotation = Quaternion.Euler(0, 0, anguloBase + progresoArco);
                yield return null;
            }
            espadaVisual.SetActive(false);
        }
    }

    void Atacar() {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach(Collider2D enemy in hitEnemies) {
            if(enemy.CompareTag("Enemy")) {
                SceneManager.LoadScene("Combate");
            }
        }
    }
}