using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject panelCombate;
    public GameObject panelAMostrar; 

    void Start()
    {
        if (panelAMostrar != null)
            panelAMostrar.SetActive(false);
    }

    public void ActivarPanel()
    {
        if (panelAMostrar != null)
        {
            panelAMostrar.SetActive(true);
            Time.timeScale = 0f; 
        }
    }

    public void DesactivarPanel()
    {
        if (panelAMostrar != null)
        {
            panelAMostrar.SetActive(false);
            Time.timeScale = 1f; 
        }
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}