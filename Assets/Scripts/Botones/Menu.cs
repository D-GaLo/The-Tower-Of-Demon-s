using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Menu : MonoBehaviour
{
    public static Menu Instance { get; private set; }

    [Header("Paneles de Pausa")]
    [Tooltip("El botón único en el HUD que abre la pausa")]
    public GameObject botonMenuPausaHUD; 
    [Tooltip("El panel con los botones de exploración (Mapa, Items, Stats)")]
    public GameObject menuPausaMapa; 
    [Tooltip("El panel con los botones de combate (Guía)")]
    public GameObject menuPausaCombate; 
    [Tooltip("El objeto Panel¿ComoJugar?")]
    public GameObject panelComoJugar; 

    [Header("Bloqueo de Seguridad")]
    public GameObject panelMapaUI; 
    public GameObject panelHeroesStats;

    [Header("Configuración del Carrusel")]
    public Image imagenCarrusel; 
    public Sprite[] spritesCarrusel; 
    
    private int indiceActual = 0;
    public bool estaPausado = false; 

    void Awake() {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (menuPausaMapa != null) menuPausaMapa.SetActive(false);
        if (menuPausaCombate != null) menuPausaCombate.SetActive(false);
        if (panelComoJugar != null) panelComoJugar.SetActive(false);
            
        ActualizarImagen();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool mapaAbierto = (panelMapaUI != null && panelMapaUI.activeSelf);
            bool statsAbiertos = (panelHeroesStats != null && panelHeroesStats.activeSelf);
            bool inventarioAbierto = (InventarioPanelUI.Instance != null && InventarioPanelUI.Instance.panelVisual.activeSelf);
            
            if (mapaAbierto || statsAbiertos || inventarioAbierto) 
            {
                return;
            }

            if (panelComoJugar != null && panelComoJugar.activeSelf)
            {
                CerrarComoJugar();
            }
            else 
            {
                TogglePausa();
            }
        }
    }

    public void TogglePausa()
    {
        if (estaPausado) DesactivarPausa();
        else ActivarPausa();
    }

    public void ActivarPausa()
    {
        estaPausado = true;
        Time.timeScale = 0f; 
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();

        if (botonMenuPausaHUD != null) botonMenuPausaHUD.SetActive(false);

        bool enCombate = false;
        if (GameFlowController.Instance != null) {
            enCombate = GameFlowController.Instance.enCombate;
        }

        if (enCombate) {
            if (menuPausaCombate != null) menuPausaCombate.SetActive(true);
        } else {
            if (menuPausaMapa != null) menuPausaMapa.SetActive(true);
        }
    }

    public void DesactivarPausa()
    {
        estaPausado = false;
        Time.timeScale = 1f; 

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();

        if (menuPausaMapa != null) menuPausaMapa.SetActive(false);
        if (menuPausaCombate != null) menuPausaCombate.SetActive(false);

        if (botonMenuPausaHUD != null) botonMenuPausaHUD.SetActive(true);
    }

    public void AbrirComoJugar()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();

        if (menuPausaMapa != null) menuPausaMapa.SetActive(false);
        if (menuPausaCombate != null) menuPausaCombate.SetActive(false);

        if (panelComoJugar != null) panelComoJugar.SetActive(true);
        
        indiceActual = 0; 
        ActualizarImagen();
    }

    public void CerrarComoJugar()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        if (panelComoJugar != null) panelComoJugar.SetActive(false);
        
        bool enCombate = false;
        if (GameFlowController.Instance != null) {
            enCombate = GameFlowController.Instance.enCombate;
        }

        if (enCombate) {
            if (menuPausaCombate != null) menuPausaCombate.SetActive(true);
        } else {
            if (menuPausaMapa != null) menuPausaMapa.SetActive(true);
        }
    }

    public void SalirJuego()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SiguienteImagen()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        if (spritesCarrusel == null || spritesCarrusel.Length == 0) return;
        indiceActual++;
        if (indiceActual >= spritesCarrusel.Length) indiceActual = 0;
        ActualizarImagen();
    }

    public void ImagenAnterior()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        if (spritesCarrusel == null || spritesCarrusel.Length == 0) return;
        indiceActual--;
        if (indiceActual < 0) indiceActual = spritesCarrusel.Length - 1;
        ActualizarImagen();
    }

    private void ActualizarImagen()
    {
        if (imagenCarrusel != null && spritesCarrusel.Length > 0)
        {
            imagenCarrusel.sprite = spritesCarrusel[indiceActual];
        }
    }
}