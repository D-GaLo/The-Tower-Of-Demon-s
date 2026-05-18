using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Menu : MonoBehaviour
{
    [Header("Paneles Generales")]
    [Tooltip("Arrastrar aqui el objeto de opcciones")]
    public GameObject panelAMostrar; 
    [Tooltip("Arrastrar aqui el objeto hijo Menu")]
    public GameObject panelBotonesPausa; 
    [Tooltip("El objeto Panel¿ComoJugar?")]
    public GameObject panelComoJugar; 

    [Header("Botones Externos (HUD)")]
    [Tooltip("Arrastrar aquí el botón de Estadísticas")]
    public GameObject botonEstadisticas; 
    [Tooltip("Arrastrar aquí el botón de Mapa")]
    public GameObject botonMapa; 

    [Header("Bloqueo de Seguridad")]
    [Tooltip("Arrastra aquí el Panel del Mapa")]
    public GameObject panelMapaUI; 
    [Tooltip("Arrastra aquí el Panel de Estadísticas")]
    public GameObject panelHeroesStats;

    [Header("Configuración del Carrusel")]
    public Image imagenCarrusel; 
    public Sprite[] spritesCarrusel; 
    
    private int indiceActual = 0;

    void Start()
    {
        if (panelAMostrar != null) panelAMostrar.SetActive(false);
        if (panelComoJugar != null) panelComoJugar.SetActive(false);
            
        ActualizarImagen();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool mapaAbierto = (panelMapaUI != null && panelMapaUI.activeSelf);
            bool statsAbiertos = (panelHeroesStats != null && panelHeroesStats.activeSelf);
            
            if (mapaAbierto || statsAbiertos) 
            {
                return;
            }
            if (panelComoJugar != null && panelComoJugar.activeSelf)
            {
                CerrarComoJugar();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
            }
            else if (panelAMostrar != null && panelAMostrar.activeSelf)
            {
                DesactivarPanel();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
            }
            else if (panelAMostrar != null && !panelAMostrar.activeSelf)
            {
                ActivarPanel();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
            }
        }
    }

    public void ActivarPanel()
    {
        if (panelAMostrar != null)
        {
            panelAMostrar.SetActive(true);
            
            if (panelBotonesPausa != null) panelBotonesPausa.SetActive(true);
            if (panelComoJugar != null) panelComoJugar.SetActive(false);
            
            
            Time.timeScale = 0f; 
        }
    }

    public void DesactivarPanel()
    {
        if (panelAMostrar != null)
        {
            panelAMostrar.SetActive(false);
            
            if (botonEstadisticas != null) botonEstadisticas.SetActive(true); 
            if (botonMapa != null) botonMapa.SetActive(true);

            Time.timeScale = 1f; 
        }
    }

    public void AbrirComoJugar()
    {
        if (panelBotonesPausa != null) panelBotonesPausa.SetActive(false);
        if (botonEstadisticas != null) botonEstadisticas.SetActive(false); 
        if (botonMapa != null) botonMapa.SetActive(false);

        if (panelComoJugar != null) panelComoJugar.SetActive(true);
        
        indiceActual = 0; 
        ActualizarImagen();
    }

    public void CerrarComoJugar()
    {
        if (panelComoJugar != null) panelComoJugar.SetActive(false);
        if (panelBotonesPausa != null) panelBotonesPausa.SetActive(true);
        if (botonEstadisticas != null) botonEstadisticas.SetActive(true); 
        if (botonMapa != null) botonMapa.SetActive(true);
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SiguienteImagen()
    {
        if (spritesCarrusel == null || spritesCarrusel.Length == 0) return;
        indiceActual++;
        if (indiceActual >= spritesCarrusel.Length) indiceActual = 0;
        ActualizarImagen();
    }

    public void ImagenAnterior()
    {
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