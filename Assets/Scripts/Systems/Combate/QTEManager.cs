using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;

public class QTEManager : MonoBehaviour {
    public static QTEManager Instance;

    [Header("UI Elementos")]
    public GameObject panelQTE;
    public GameObject[] contenedoresTeclas; 
    public Image[] imagenesTeclas;          
    public TextMeshProUGUI textoResultado;
    public TextMeshProUGUI textoPresiona;
    public Image barraTiempo;               

    [Header("Sprites de tus Teclas (¡Asígnalos!)")]
    public Sprite sprite_A;
    public Sprite sprite_S;
    public Sprite sprite_W;
    public Sprite sprite_D;
    public Sprite sprite_E;
    public Sprite sprite_R;
    public Sprite sprite_T;

    [Header("Configuración")]
    public float tiempoParaQTE = 3.0f;

    private List<KeyCode> teclasAtaque = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D };
    private List<KeyCode> todasLasTeclasValidas = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D, KeyCode.E, KeyCode.R, KeyCode.T };
    private List<KeyCode> secuenciaActual = new List<KeyCode>();
    private List<KeyCode> teclasEsquive = new List<KeyCode> { KeyCode.E, KeyCode.R, KeyCode.T };
    private int indiceActual = 0;
    
    private bool qteActivo = false;
    private float tiempoRestante; 

    private Color colorDefault = Color.white;
    private Color colorAcierto = Color.green; 

    private System.Action<float> onQTEComplete; 

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IniciarQTE(int longitud, System.Action<float> callback) {
        onQTEComplete = callback;
        secuenciaActual.Clear();
        indiceActual = 0;
        tiempoRestante = tiempoParaQTE; 

        for (int i = 0; i < longitud; i++) {
            secuenciaActual.Add(teclasAtaque[Random.Range(0, teclasAtaque.Count)]);
        }

        ConfigurarVisuales();
    }

    public void IniciarEsquive(KeyCode tecla, System.Action<float> callback) {
        onQTEComplete = callback;
        secuenciaActual.Clear();
        secuenciaActual.Add(tecla);
        indiceActual = 0;
        tiempoRestante = tiempoParaQTE / 2f; 

        ConfigurarVisuales();
    }

    // --- QTE DE ESQUIVE EN ÁREA (E, R, T) ---
    public void IniciarEsquiveAoE(int longitud, System.Action<float> callback) {
        onQTEComplete = callback;
        secuenciaActual.Clear();
        indiceActual = 0;
        tiempoRestante = tiempoParaQTE; // Tiempo completo para que logren leer las teclas

        // Genera una secuencia solo con E, R y T
        for (int i = 0; i < longitud; i++) {
            secuenciaActual.Add(teclasEsquive[Random.Range(0, teclasEsquive.Count)]);
        }

        ConfigurarVisuales();
    }

    Sprite ObtenerSprite(KeyCode tecla) {
        switch (tecla) {
            case KeyCode.A: return sprite_A;
            case KeyCode.S: return sprite_S;
            case KeyCode.W: return sprite_W;
            case KeyCode.D: return sprite_D;
            case KeyCode.E: return sprite_E;
            case KeyCode.R: return sprite_R;
            case KeyCode.T: return sprite_T;
            default: return null;
        }
    }

    void ConfigurarVisuales() {
        qteActivo = true;
        panelQTE.SetActive(true);
        
        // --- CONTROL DE TEXTOS AL INICIO ---
        if (textoResultado != null) textoResultado.gameObject.SetActive(false); // Ocultamos el resultado
        
        if (textoPresiona != null) {
            textoPresiona.gameObject.SetActive(true); // Mostramos el "Presiona"
            textoPresiona.text = "¡Presiona!";
            textoPresiona.color = Color.white;
        }

        if (barraTiempo != null) barraTiempo.fillAmount = 1f;

        for (int i = 0; i < imagenesTeclas.Length; i++) {
            if (i < secuenciaActual.Count) {
                if (contenedoresTeclas.Length > i && contenedoresTeclas[i] != null) contenedoresTeclas[i].SetActive(true);
                imagenesTeclas[i].sprite = ObtenerSprite(secuenciaActual[i]);
                imagenesTeclas[i].color = colorDefault;
            } else {
                if (contenedoresTeclas.Length > i && contenedoresTeclas[i] != null) contenedoresTeclas[i].SetActive(false);
            }
        }
    }

    void Update() {
        if (!qteActivo) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        if (barraTiempo != null) barraTiempo.fillAmount = tiempoRestante / tiempoParaQTE;

        if (tiempoRestante <= 0) {
            TerminarQTE(0f); 
            return;
        }

        if (Input.anyKeyDown) {
            KeyCode teclaPresionada = DetectarTeclaValida();

            if (teclaPresionada != KeyCode.None) {
                if (teclaPresionada == secuenciaActual[indiceActual]) {
                    imagenesTeclas[indiceActual].color = colorAcierto;
                    indiceActual++;

                    if (indiceActual >= secuenciaActual.Count) {
                        TerminarQTE(1f); 
                    }
                } else {
                    float porcentaje = (float)indiceActual / secuenciaActual.Count;
                    TerminarQTE(porcentaje); 
                }
            }
        }
    }

    KeyCode DetectarTeclaValida() {
        foreach (KeyCode key in todasLasTeclasValidas) {
            if (Input.GetKeyDown(key)) return key;
        }
        return KeyCode.None;
    }
    void TerminarQTE(float porcentajeExito) {
        qteActivo = false;
        
        // El multiplicador que enviaremos al CombatManager
        float multiplicadorFinal = 1.0f;

        // --- CONTROL DE TEXTOS AL FINALIZAR ---
        if (textoPresiona != null) textoPresiona.gameObject.SetActive(false); 
        if (textoResultado != null) textoResultado.gameObject.SetActive(true); 

        // 1. Regla: Secuencia correcta al 100% -> Daño no alterado (1.0x)
        if (porcentajeExito >= 1.0f) {
            if (textoResultado != null) { textoResultado.text = "Perfect"; textoResultado.color = Color.green; }
            multiplicadorFinal = 1.0f; 
        } 
        // 2. Regla: Falla menos de la mitad (Es decir, acierta el 50% o más) -> Mitad de daño (0.5x)
        else if (porcentajeExito >= 0.5f) {
            if (textoResultado != null) { textoResultado.text = "Great"; textoResultado.color = new Color(1f, 0.5f, 0f); } // Naranja
            multiplicadorFinal = 0.5f; 
        } 
        // 3 y 4. Regla: Falla más de la mitad o se acaba el tiempo (0.0f) -> Falla el ataque (0.0x)
        else {
            if (textoResultado != null) { textoResultado.text = "Failure"; textoResultado.color = Color.red; }
            multiplicadorFinal = 0f; 
        }

        if (gameObject.activeInHierarchy) {
            StartCoroutine(OcultarPanelYDano(multiplicadorFinal));
        } else {
            if (onQTEComplete != null) onQTEComplete.Invoke(multiplicadorFinal);
        }
    }

    IEnumerator OcultarPanelYDano(float mult) {
        yield return new WaitForSecondsRealtime(1.0f); 
        panelQTE.SetActive(false);
        if (onQTEComplete != null) onQTEComplete.Invoke(mult);
    }
}