using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QTEManager : MonoBehaviour {
    public static QTEManager Instance;

    [Header("UI Elementos")]
    public GameObject panelQTE;
    public TextMeshProUGUI[] letrasUI;
    public TextMeshProUGUI textoResultado;

    [Header("Configuración")]
    public float tiempoParaQTE = 3.0f; // Tienes 3 segundos para completarlo

    // Solo para generar ataques aleatorios
    private List<KeyCode> teclasAtaque = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D };

    // Todas las teclas que el jugador puede presionar para que el script responda
    private List<KeyCode> todasLasTeclasValidas = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D, KeyCode.E, KeyCode.R, KeyCode.T };
    private List<KeyCode> secuenciaActual = new List<KeyCode>();
    private int indiceActual = 0;
    
    private bool qteActivo = false;

    // Colores para el feedback visual
    private Color colorDefault = Color.white;
    private Color colorAcierto = Color.yellow; 

    // Callback para decirle al CombatManager cómo nos fue
    private System.Action<float> onQTEComplete; 

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update() {
        if (!qteActivo) return;

        // Escuchamos si el jugador presiona alguna tecla
        if (Input.anyKeyDown) {
            // Buscamos cuál de las 4 teclas válidas presionó
            foreach (KeyCode key in todasLasTeclasValidas) {
                if (Input.GetKeyDown(key)) {
                    VerificarTecla(key);
                    break; 
                }
            }
        }
    }

    public void IniciarQTE(int longitudSecuencia, System.Action<float> callbackResultados) {
        onQTEComplete = callbackResultados;
        qteActivo = true;
        indiceActual = 0;
        textoResultado.text = "";

        GenerarSecuencia(longitudSecuencia);
        ActualizarPantalla();

        panelQTE.SetActive(true);
        StartCoroutine(TemporizadorQTE());
    }

    public void IniciarEsquive(KeyCode teclaAsignada, System.Action<float> callbackResultados) {
        onQTEComplete = callbackResultados;
        qteActivo = true;
        indiceActual = 0;
        textoResultado.text = "";

        // Limpiamos y preparamos una secuencia de 1 sola tecla
        secuenciaActual.Clear();
        secuenciaActual.Add(teclaAsignada);
        
        ActualizarPantalla();
        panelQTE.SetActive(true);
        StartCoroutine(TemporizadorQTE()); // Usa el mismo tiempo (3 segs) o puedes crear una variable nueva para que esquivar sea más rápido
    }

    void GenerarSecuencia(int longitud) {
        secuenciaActual.Clear();
        // El GDD dice que la secuencia debe ser mostrada  y solo puede tener A, S, W, D.
        for (int i = 0; i < longitud; i++) {
            KeyCode teclaAleatoria = teclasAtaque[Random.Range(0, teclasAtaque.Count)];
            secuenciaActual.Add(teclaAleatoria);
        }
    }

    void ActualizarPantalla() {
        for (int i = 0; i < letrasUI.Length; i++) {
            if (i < secuenciaActual.Count) {
                letrasUI[i].gameObject.SetActive(true);
                letrasUI[i].text = secuenciaActual[i].ToString();
                
                // Si ya la pasamos, la pintamos de amarillo 
                letrasUI[i].color = (i < indiceActual) ? colorAcierto : colorDefault;
            } else {
                letrasUI[i].gameObject.SetActive(false); // Ocultamos textos que sobren
            }
        }
    }

    void VerificarTecla(KeyCode teclaPresionada) {
        if (teclaPresionada == secuenciaActual[indiceActual]) {
            // ¡Acierto! Avanzamos
            indiceActual++;
            ActualizarPantalla();

            if (indiceActual >= secuenciaActual.Count) {
                TerminarQTE(true); // Terminó toda la secuencia
            }
        } else {
            // ¡Fallo! Se equivocó de tecla
            TerminarQTE(false);
        }
    }

    IEnumerator TemporizadorQTE() {
        float tiempoRestante = tiempoParaQTE;
        while (tiempoRestante > 0 && qteActivo) {
            // unscaledDeltaTime ignora si el juego está pausado
            tiempoRestante -= Time.unscaledDeltaTime; 
            yield return null;
        }

        // Si se acaba el tiempo y seguía activo, cuenta como fallo
        if (qteActivo) {
            TerminarQTE(false);
        }
    }

    void TerminarQTE(bool completadoPorTiempoOBoton) {
        qteActivo = false;
        
        float porcentajeExito = (float)indiceActual / secuenciaActual.Count;
        float multiplicadorFinal = 1.0f; // Si fallas muy feo, haces daño normal (x1)
        
        // Apagamos las letras para que no se encimen con el resultado
        foreach(var letra in letrasUI) letra.gameObject.SetActive(false);
        
        if (porcentajeExito == 1.0f) {
            textoResultado.text = "Perfect";
            textoResultado.color = Color.green;
            multiplicadorFinal = 1.5f; // Bonus máximo
        } 
        else if (porcentajeExito > 0.5f) {
            textoResultado.text = "Great";
            textoResultado.color = new Color(1f, 0.5f, 0f); // Naranja
            multiplicadorFinal = 1.2f; // Bonus medio
        } 
        else {
            textoResultado.text = "Failure";
            textoResultado.color = new Color(0.5f, 0f, 0.5f); // Morado
            multiplicadorFinal = 0.5f; // Castigo por fallar
        }

        // Verificamos si el objeto Controlador_QTE sigue encendido en la jerarquía
        if (gameObject.activeInHierarchy) {
            StartCoroutine(OcultarPanelYDano(multiplicadorFinal));
        } else {
            // Si por alguna razón el objeto se apagó, forzamos el final para que no se trabe el juego
            Debug.LogWarning("El QTEManager se apagó inesperadamente. Forzando ataque.");
            if (onQTEComplete != null) {
                onQTEComplete.Invoke(multiplicadorFinal);
            }
        }
    }

    IEnumerator OcultarPanelYDano(float mult) {
        // Usamos Realtime para que no se congele si el juego está pausado
        yield return new WaitForSecondsRealtime(1.0f); 
        
        // PRIMERO: Le avisamos al CombatManager que ya terminamos.
        if (onQTEComplete != null) {
            onQTEComplete.Invoke(mult);
        } else {
            Debug.LogError("Error: onQTEComplete es nulo.");
        }

        // SEGUNDO: Apagamos el panel.
        panelQTE.SetActive(false);
    }
}