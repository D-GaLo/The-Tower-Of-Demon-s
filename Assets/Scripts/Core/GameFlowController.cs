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
    public GameObject bordesNegros; 
    public GameObject uiCombate; 
    public GameObject botonMapa;
    private GameObject enemigoActual;
    private bool enCombate = false;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IniciarCombate(GameObject enemigo) {
        // --- LA TRAMPA: Esto nos dirá quién activó el combate realmente ---
        Debug.Log("🚨 ¡ALERTA DE COMBATE! El objeto enviado al administrador fue: " + (enemigo != null ? enemigo.name : "Nulo/Vacío"));

        if (enCombate) return; 
        enCombate = true;
        enemigoActual = enemigo;
        StartCoroutine(TransicionACombate());
    }

    IEnumerator TransicionACombate() {
        // 1. Cubrir pantalla (Forzando que se dibuje por encima de TODO)
        if (pantallaTransicion != null) {
            pantallaTransicion.transform.SetAsLastSibling();
            pantallaTransicion.SetActive(true);
        }

        // 2. Pausar el nivel
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(0.5f);

        // 3. Apagar bordes, mover cámara y encender UI de combate
        if (bordesNegros != null) bordesNegros.SetActive(false);
        if (botonMapa != null) botonMapa.SetActive(false);
        if (camaraPrincipal != null) camaraPrincipal.CambiarModoCombate(true, posicionArenaCombate);
        if (uiCombate != null) uiCombate.SetActive(true);

        // Le avisamos al CombatManager que ya llegamos a la arena y puede empezar
        if (CombatManager.Instance != null) {
            CombatManager.Instance.StartCombat(enemigoActual);
        }

        // 4. Quitar la pantalla de transición
        if (pantallaTransicion != null) pantallaTransicion.SetActive(false);
    }

    public void TerminarCombate() {
        StartCoroutine(TransicionAExploracion());
    }

    IEnumerator TransicionAExploracion() {
        // 1. Cubrir pantalla de nuevo (al frente de todo)
        if (pantallaTransicion != null) {
            pantallaTransicion.transform.SetAsLastSibling();
            pantallaTransicion.SetActive(true);
        }
        
        if (uiCombate != null) uiCombate.SetActive(false);

        yield return new WaitForSecondsRealtime(0.5f);

        // 2. Destruir al enemigo con el que chocamos
        if (enemigoActual != null) {
            Destroy(enemigoActual);
        }

        // 3. Restaurar la exploración (El Vector3.zero ya no afecta porque la cámara lo calcula sola)
        if (camaraPrincipal != null) camaraPrincipal.CambiarModoCombate(false, Vector3.zero);
        if (bordesNegros != null) bordesNegros.SetActive(true);
        if (botonMapa != null) botonMapa.SetActive(true); //
        
        Time.timeScale = 1f; 
        enCombate = false;

        // 4. Quitar transición
        if (pantallaTransicion != null) pantallaTransicion.SetActive(false);
    }
}