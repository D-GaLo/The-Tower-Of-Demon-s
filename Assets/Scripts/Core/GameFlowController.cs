using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameFlowController : MonoBehaviour {
    public static GameFlowController Instance; 

    [Header("Cámara y Posiciones")]
    public RoomCamera camaraPrincipal;
    public Vector3 posicionArenaCombate = new Vector3(1000f, 1000f, -10f);

    [Header("UI (Interfaces)")]
    public GameObject pantallaTransicion; 
    public GameObject uiCombate; 
    
    [Header("Botones de Exploración (Ocultos en combate)")]
    public GameObject botonMapa;
    public GameObject botonEstadisticas;
    public GameObject botonEspada;
    public GameObject botonInteraccion;
    public GameObject visualEspada; 

    // --- NUEVO: CONTADOR DE COMBATES ---
    [Header("Estadísticas Globales")]
    public int combatesCompletados = 0;

    private GameObject enemigoActual;
    private bool enCombate = false;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IniciarCombate(GameObject enemigo) {
        if (enCombate) return; 
        enCombate = true;
        enemigoActual = enemigo;
        StartCoroutine(TransicionACombate());
    }

    IEnumerator TransicionACombate() {
        if (pantallaTransicion != null) {
            pantallaTransicion.transform.SetAsLastSibling();
            pantallaTransicion.SetActive(true);
        }

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);

        if (camaraPrincipal != null) camaraPrincipal.CambiarModoCombate(true, posicionArenaCombate);;
        
        if (botonMapa != null) botonMapa.SetActive(false);
        if (botonEstadisticas != null) botonEstadisticas.SetActive(false);
        if (botonEspada != null) botonEspada.SetActive(false);
        if (botonInteraccion != null) botonInteraccion.SetActive(false);
        if (visualEspada != null) visualEspada.SetActive(false); 

        Time.timeScale = 1f;
        if (uiCombate != null) uiCombate.SetActive(true);

        if (CombatManager.Instance != null) {
            CombatManager.Instance.StartCombat(enemigoActual);
        }

        if (pantallaTransicion != null) pantallaTransicion.SetActive(false);
    }

    public void TerminarCombate() {
        // --- NUEVO: SUMAMOS 1 AL CONTADOR ---
        combatesCompletados++;
        StartCoroutine(TransicionAExploracion());
    }

    IEnumerator TransicionAExploracion() {
        if (pantallaTransicion != null) {
            pantallaTransicion.transform.SetAsLastSibling();
            pantallaTransicion.SetActive(true);
        }
        
        if (uiCombate != null) uiCombate.SetActive(false);
        yield return new WaitForSecondsRealtime(0.5f);

        if (enemigoActual != null) {
            Destroy(enemigoActual);
        }

        if (camaraPrincipal != null) camaraPrincipal.CambiarModoCombate(false, Vector3.zero);
        
        if (botonMapa != null) botonMapa.SetActive(true);
        if (botonEstadisticas != null) botonEstadisticas.SetActive(true);
        if (botonEspada != null) botonEspada.SetActive(true);
        if (botonInteraccion != null) botonInteraccion.SetActive(true);

        if (pantallaTransicion != null) pantallaTransicion.SetActive(false);
        enCombate = false;
    }
}