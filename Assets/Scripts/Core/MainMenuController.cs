using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {
    [Header("Paneles de Menú secundarios")]
    public GameObject panelTutorial;
    public GameObject panelHistoria;
    public GameObject panelCreditos;

    [Header("Configuración del Tutorial")]
    public Image contenedorImagenTutorial;
    [Tooltip("Arrastrar aquí en orden los Sprites de las imágenes del tutorial.")]
    public Sprite[] imagenesTutorial;
    private int indiceTutorialActual = 0;

    [Header("Configuración de Escena")]
    public string nombreEscenaMapa = "Mapa";

    [Header("Sistema de Audio")]
    [Tooltip("AudioSource dedicado a la música de fondo.")]
    public AudioSource fuenteMusica; 
    [Tooltip("AudioSource dedicado a los efectos de sonido.")]
    public AudioSource fuenteSonidos; 
    public AudioClip sonidoClic;

    void Start() {
        CerrarTodosLosPaneles(); 
        
        PlayerPrefs.SetInt("KonamiActivado", 0);
        PlayerPrefs.Save();

        if (EasterEggsManager.Instance != null) {
            EasterEggsManager.Instance.ResetearSecretos();
        }

        if (fuenteMusica != null && !fuenteMusica.isPlaying) {
            fuenteMusica.Play();
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            BotonCerrarPanel();
        }
    }

    public void DetenerMusicaMenu() {
        if (fuenteMusica != null) fuenteMusica.Stop();
    }

    public void ReproducirSonidoClic() {
        if (fuenteSonidos != null && sonidoClic != null) {
            fuenteSonidos.PlayOneShot(sonidoClic);
        }
    }

    public void OnBotonIniciar() {
        ReproducirSonidoClic();
        SceneManager.LoadScene(nombreEscenaMapa);
    }

    public void OnBotonComoJugar() {
        ReproducirSonidoClic();
        CerrarTodosLosPaneles();
        if (panelTutorial != null) {
            panelTutorial.SetActive(true);
            indiceTutorialActual = 0;
            ActualizarImagenTutorial();
        }
    }

    public void OnBotonHistoria() {
        ReproducirSonidoClic();
        CerrarTodosLosPaneles();
        if (panelHistoria != null) {
            panelHistoria.SetActive(true);
        }
    }

    public void OnBotonCreditos() {
        ReproducirSonidoClic();
        CerrarTodosLosPaneles();
        if (panelCreditos != null) {
            panelCreditos.SetActive(true);
        }
    }

    public void OnBotonSalir() {
        ReproducirSonidoClic();
        Debug.Log("Cerrando el juego...");
        Application.Quit(); 
    }

    public void OnBotonSiguienteTutorial() {
        ReproducirSonidoClic();
        if (imagenesTutorial == null || imagenesTutorial.Length == 0) return;

        indiceTutorialActual++;
        if (indiceTutorialActual >= imagenesTutorial.Length) indiceTutorialActual = 0;
        
        ActualizarImagenTutorial();
    }

    public void OnBotonAnteriorTutorial() {
        ReproducirSonidoClic();
        if (imagenesTutorial == null || imagenesTutorial.Length == 0) return;

        indiceTutorialActual--;
        if (indiceTutorialActual < 0) indiceTutorialActual = imagenesTutorial.Length - 1;
        
        ActualizarImagenTutorial();
    }

    void ActualizarImagenTutorial() {
        if (contenedorImagenTutorial != null && imagenesTutorial.Length > 0) {
            contenedorImagenTutorial.sprite = imagenesTutorial[indiceTutorialActual];
        }
    }

    public void BotonCerrarPanel() {
        ReproducirSonidoClic();
        CerrarTodosLosPaneles();
    }

    public void CerrarTodosLosPaneles() {
        if (panelTutorial != null) panelTutorial.SetActive(false);
        if (panelHistoria != null) panelHistoria.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }
}