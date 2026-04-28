using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float speed = 5f;
    public float attackRange = 1.5f;
    public GameObject espadaVisual; 
    public float duracionAtaque = 0.15f;
    public float distanciaEspada = 0.8f; // Qué tan lejos de Sieg aparece la espada

    private SpriteRenderer spriteSieg;
    private Vector2 direccionMirada = Vector2.right;
    private float escalaFija = 0.7f;

    void Start() {
        spriteSieg = GetComponent<SpriteRenderer>();
        // Fijamos la escala del GDD una sola vez
        transform.localScale = new Vector3(escalaFija, escalaFija, 1f);
        
        if(espadaVisual != null) espadaVisual.SetActive(false);
    }

    void Update() {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Mover(moveX, moveY);
        ActualizarVisuales(moveX, moveY);

        if (Input.GetKeyDown(KeyCode.F)) {
            StopAllCoroutines();
            StartCoroutine(AnimacionAtaqueSemicircular());
            Atacar();
        }
    }

    void Mover(float x, float y) {
        // Usamos normalized para que el movimiento diagonal no sea más rápido
        Vector2 mov = new Vector2(x, y).normalized;
        transform.Translate(mov * speed * Time.deltaTime, Space.World);
    }

    void ActualizarVisuales(float x, float y) {
        if (x != 0 || y != 0) {
            direccionMirada = new Vector2(x, y).normalized;

            // Giramos el sprite visualmente sin tocar el transform
            if (x > 0) spriteSieg.flipX = false;
            else if (x < 0) spriteSieg.flipX = true;
        }
    }

    IEnumerator AnimacionAtaqueSemicircular() {
        if(espadaVisual != null) {
            // 1. Posicionar la espada según la dirección de Sieg
            float anguloBase = Mathf.Atan2(direccionMirada.y, direccionMirada.x) * Mathf.Rad2Deg;
            
            // Colocamos la espada en un círculo alrededor de Sieg
            espadaVisual.transform.localPosition = new Vector3(direccionMirada.x, direccionMirada.y, 0) * distanciaEspada;
            
            espadaVisual.SetActive(true);
            float tiempo = 0;

            while(tiempo < duracionAtaque) {
                tiempo += Time.deltaTime;
                // Arco de 180 grados (90 a cada lado de la dirección actual)
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