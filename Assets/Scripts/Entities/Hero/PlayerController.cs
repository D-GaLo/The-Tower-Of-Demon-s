using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

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
    private float escalaFija = 1f;
    
    private bool isAttacking = false;
    private bool interaccionSono = false;

    [Header("Botones HUD (UI)")]
    public Button botonAtacarHUD;
    public Button botonInteractuarHUD;
    private Color colorOriginalInteract = Color.white;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        spriteSieg = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(escalaFija, escalaFija, 1f);
        
        if(espadaVisual != null) espadaVisual.SetActive(false);

        if(botonInteractuarHUD != null) {
            colorOriginalInteract = botonInteractuarHUD.image.color;
            botonInteractuarHUD.onClick.AddListener(BotonInteractuarHUD_Click);
        }
        if(botonAtacarHUD != null) {
            botonAtacarHUD.onClick.AddListener(BotonAtacarHUD_Click);
        }
    }

    void Update() {
        if (Time.timeScale == 0) return;

        bool enCombate = false;
        if (GameFlowController.Instance != null) {
            enCombate = GameFlowController.Instance.enCombate || 
                       (GameFlowController.Instance.uiCombate != null && GameFlowController.Instance.uiCombate.activeSelf);
        }

        if (enCombate) {
            inputMovimiento = Vector2.zero;
            return; 
        }

        inputMovimiento.x = Input.GetAxisRaw("Horizontal");
        inputMovimiento.y = Input.GetAxisRaw("Vertical");

        ActualizarVisuales(inputMovimiento.x, inputMovimiento.y);
        VerificarInteraccionesHUD();

        if (Input.GetKeyDown(KeyCode.F)) BotonAtacarHUD_Click();
    }

    void FixedUpdate() {
       if (Time.timeScale == 0) {
            rb.velocity = Vector2.zero; 
            return;
        }

        bool enCombate = false;
        if (GameFlowController.Instance != null) {
            enCombate = GameFlowController.Instance.enCombate || 
                       (GameFlowController.Instance.uiCombate != null && GameFlowController.Instance.uiCombate.activeSelf);
        }

        if (enCombate) {
            rb.velocity = Vector2.zero;
            return;
        }
        
        rb.velocity = inputMovimiento.normalized * speed;
    }

    void ActualizarVisuales(float x, float y) {
        if (x != 0 || y != 0) {
            direccionMirada = new Vector2(x, y).normalized;
            if (x > 0) spriteSieg.flipX = false;
            else if (x < 0) spriteSieg.flipX = true;
        }
    }

    public void BotonAtacarHUD_Click() {
        if (Time.timeScale == 0 || isAttacking) return; 
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEspada();
        
        StartCoroutine(AnimacionAtaqueSemicircular());
        Atacar();
    }

    public void BotonInteractuarHUD_Click() {
        if (Time.timeScale == 0) return;
        
        MonoBehaviour closest = null;
        float minDistance = Mathf.Infinity;

        foreach(var p in FindObjectsOfType<PuertaFinal>()) {
            float d = Vector2.Distance(transform.position, p.transform.position);
            if(d <= p.distanciaInteraccion && d < minDistance) { minDistance = d; closest = p; }
        }
        foreach(var e in FindObjectsOfType<Enredaderas>()) {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if(d <= e.distanciaInteraccion && d < minDistance) { minDistance = d; closest = e; }
        }
        foreach(var eN in FindObjectsOfType<EnredaderasNucifera>()) {
            float d = Vector2.Distance(transform.position, eN.transform.position);
            if(d <= eN.distanciaInteraccion && d < minDistance) { minDistance = d; closest = eN; }
        }
        foreach(var f in FindObjectsOfType<FuenteCurativa>()) {
            float d = Vector2.Distance(transform.position, f.transform.position);
            if(d <= f.distanciaInteraccion && d < minDistance) { minDistance = d; closest = f; }
        }
        foreach(var t in FindObjectsOfType<TijerasGuardian>()) {
            float d = Vector2.Distance(transform.position, t.transform.position);
            if(d <= t.distanciaInteraccion && d < minDistance) { minDistance = d; closest = t; }
        }

        if (closest != null) {
            closest.SendMessage("InteraccionHUD", SendMessageOptions.DontRequireReceiver);
        }
    }

    void VerificarInteraccionesHUD() {
        if (botonInteractuarHUD == null) return;
        
        bool interactableCerca = false;

        foreach(var p in FindObjectsOfType<PuertaFinal>()) 
            if(Vector2.Distance(transform.position, p.transform.position) <= p.distanciaInteraccion) interactableCerca = true;
        foreach(var e in FindObjectsOfType<Enredaderas>()) 
            if(Vector2.Distance(transform.position, e.transform.position) <= e.distanciaInteraccion) interactableCerca = true;
        foreach(var eN in FindObjectsOfType<EnredaderasNucifera>()) 
            if(Vector2.Distance(transform.position, eN.transform.position) <= eN.distanciaInteraccion) interactableCerca = true;
        foreach(var f in FindObjectsOfType<FuenteCurativa>()) 
            if(Vector2.Distance(transform.position, f.transform.position) <= f.distanciaInteraccion) interactableCerca = true;
        foreach(var t in FindObjectsOfType<TijerasGuardian>()) 
            if(Vector2.Distance(transform.position, t.transform.position) <= t.distanciaInteraccion) interactableCerca = true;

        botonInteractuarHUD.interactable = interactableCerca;
        if (interactableCerca) {
            botonInteractuarHUD.image.color = colorOriginalInteract;
            
            if (!interaccionSono && AudioManager.Instance != null) {
                AudioManager.Instance.PlayInteraccion();
                interaccionSono = true;
            }
        } else {
            botonInteractuarHUD.image.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            interaccionSono = false;
        }
    }

    IEnumerator AnimacionAtaqueSemicircular() {
        isAttacking = true;
        if(espadaVisual != null) {
            float anguloBase = Mathf.Atan2(direccionMirada.y, direccionMirada.x) * Mathf.Rad2Deg;
            espadaVisual.transform.localPosition = new Vector3(direccionMirada.x, direccionMirada.y, 0) * distanciaEspada;
            
            espadaVisual.SetActive(true);
            float tiempo = 0;
            while(tiempo < duracionAtaque) {
                tiempo += Time.unscaledDeltaTime; 
                float progresoArco = Mathf.Lerp(90, -90, tiempo / duracionAtaque);
                espadaVisual.transform.localRotation = Quaternion.Euler(0, 0, anguloBase + progresoArco);
                yield return null;
            }
            espadaVisual.SetActive(false);
        }
        isAttacking = false;
    }

    void Atacar() {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach(Collider2D enemy in hitEnemies) {
            if(enemy.CompareTag("Enemy")) {
                EnemyStats stats = enemy.GetComponentInParent<EnemyStats>();
                
                if (stats != null && GameFlowController.Instance != null) {
                    GameFlowController.Instance.IniciarCombate(stats.gameObject, true);
                }
                break;
            }
        }
    }

    public void TeletransportarAlSpawn(Vector3 posicion) {
        if (transform.parent != null) {
            Rigidbody2D parentRb = transform.parent.GetComponent<Rigidbody2D>();
            if (parentRb != null) {
                parentRb.velocity = Vector2.zero; 
                parentRb.position = posicion;    
            }
            transform.parent.position = posicion;
        } else {
            if (rb != null) {
                rb.velocity = Vector2.zero;
                rb.position = posicion;
            }
            transform.position = posicion;
        }
    }
}