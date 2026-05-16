using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {
    [Header("Paneles de Menú secundarios")]
    public GameObject panelTutorial;
    public GameObject panelHistoria;
    public GameObject panelCreditos;

    [Header("Configuración del Tutorial (Carrusel)")]
    public Image contenedorImagenTutorial;
    [Tooltip("Arrastra aquí en orden los Sprites de las imágenes de tu tutorial.")]
    public Sprite[] imagenesTutorial;
    private int indiceTutorialActual = 0;

    [Header("Configuración de Escena")]
    public string nombreEscenaMapa = "Mapa";

    [Header("Sistema de Audio Independiente")]
    [Tooltip("AudioSource dedicado a la música de fondo.")]
    public AudioSource fuenteMusica; 
    [Tooltip("AudioSource dedicado a los efectos de sonido (clics).")]
    public AudioSource fuenteSonidos; 
    public AudioClip sonidoClic;

    void Start() {
        CerrarTodosLosPaneles();
        
        // Iniciamos la música ambiental si hay una asignada
        if (fuenteMusica != null && !fuenteMusica.isPlaying) {
            fuenteMusica.Play();
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            CerrarTodosLosPaneles();
            ReproducirSonidoClic(); // Opcional: Sonido al cerrar con Esc
        }
    }

    // --- FUNCIÓN DE SONIDO ---
    public void ReproducirSonidoClic() {
        if (fuenteSonidos != null && sonidoClic != null) {
            fuenteSonidos.PlayOneShot(sonidoClic);
        }
    }

    // --- BOTONES PRINCIPALES ---
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

    // --- NAVEGACIÓN DEL CARRUSEL ---
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
        ReproducirSonidoClic();
        if (panelTutorial != null) panelTutorial.SetActive(false);
        if (panelHistoria != null) panelHistoria.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }
}