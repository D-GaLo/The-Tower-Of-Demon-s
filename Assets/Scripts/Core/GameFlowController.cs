using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour {
    public static GameFlowController Instance; 

    [Header("Cámara y Posiciones")]
    public RoomCamera camaraPrincipal;
    public Vector3 posicionArenaCombate = new Vector3(1000f, 1000f, -10f);
    
    [Tooltip("Arrastrar aquí el punto de reaparición.")]
    public Transform puntoReaparicion;

    [Header("UI (Interfaces)")]
    public GameObject pantallaTransicion; 
    public GameObject uiCombate; 
    
    [Header("Botones de Exploración")]
    public GameObject botonMapa;
    public GameObject botonEstadisticas;

    public GameObject botonInventario;
    public GameObject botonEspada;
    public GameObject botonInteraccion;
    public GameObject visualEspada; 

    [Header("Numero de combates completados")]
    public int combatesCompletados = 0;

    [Header("Menu")]
    public string nombreEscenaMenu = "Menu";

    private GameObject enemigoActual;
    private bool enCombate = false;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IniciarCombate(GameObject enemigo, bool ventajaJugador = false) {
        if (enCombate) return; 
        enCombate = true;
        enemigoActual = enemigo;
        StartCoroutine(TransicionACombate(ventajaJugador));
    }

    IEnumerator TransicionACombate(bool ventajaJugador) {
        if (pantallaTransicion != null) {
            pantallaTransicion.transform.SetAsLastSibling();
            pantallaTransicion.SetActive(true);
        }

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);

        if (camaraPrincipal != null) camaraPrincipal.CambiarModoCombate(true, posicionArenaCombate);
        
        if (botonMapa != null) botonMapa.SetActive(false);
        if (botonEstadisticas != null) botonEstadisticas.SetActive(false);
        if (botonInventario != null) botonInventario.SetActive(false);
        if (botonEspada != null) botonEspada.SetActive(false);
        if (botonInteraccion != null) botonInteraccion.SetActive(false);
        if (visualEspada != null) visualEspada.SetActive(false); 

        Time.timeScale = 1f;
        if (uiCombate != null) uiCombate.SetActive(true);

        if (CombatManager.Instance != null) {
            CombatManager.Instance.StartCombat(enemigoActual, ventajaJugador);
        }

        if (AudioManager.Instance != null) {
            bool esJefe = enemigoActual.GetComponent<EnemyStats>().esJefe;
            AudioManager.Instance.ReproducirMusicaCombate(esJefe);
        }

        if (pantallaTransicion != null) pantallaTransicion.SetActive(false);
    }

    public void TerminarCombate(bool victoria, bool huyo = false) {
        if (victoria) {
            combatesCompletados++;
        }
        StartCoroutine(TransicionAExploracion(victoria, huyo));
    }

    IEnumerator TransicionAExploracion(bool victoria, bool huyo) {
        if (pantallaTransicion != null) {
            pantallaTransicion.transform.SetAsLastSibling();
            pantallaTransicion.SetActive(true);
        }
        
        if (uiCombate != null) uiCombate.SetActive(false);
        yield return new WaitForSecondsRealtime(0.5f);

        if (victoria && enemigoActual != null) {
            Destroy(enemigoActual);
        }

        if (!victoria && !huyo) {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null) {
                Vector3 destino = puntoReaparicion != null ? puntoReaparicion.position : new Vector3(0f, -2.5f, 0f);
                
                destino.z = player.transform.position.z; 
                
                player.TeletransportarAlSpawn(destino);
            }

            HeroStats[] heroes = FindObjectsOfType<HeroStats>();
            foreach (HeroStats h in heroes) {
                h.currentHP = Mathf.Max(1, Mathf.RoundToInt(h.maxHP * 0.25f));
                h.currentEnergy = 0;
            }
        }

        if (camaraPrincipal != null) camaraPrincipal.CambiarModoCombate(false, Vector3.zero);
        
        if (botonMapa != null) botonMapa.SetActive(true);
        if (botonEstadisticas != null) botonEstadisticas.SetActive(true);
        if (botonInventario != null) botonInventario.SetActive(true);   
        if (botonEspada != null) botonEspada.SetActive(true);
        if (botonInteraccion != null) botonInteraccion.SetActive(true);

        if (pantallaTransicion != null) pantallaTransicion.SetActive(false);
        enCombate = false;

        if (AudioManager.Instance != null) AudioManager.Instance.ReproducirMusicaAmbiental();
    }

    public void VolverAlMenuPrincipal() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }
    
}