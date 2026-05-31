using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogoCombateUI : MonoBehaviour {
    public static DialogoCombateUI Instance;

    [Header("Referencias UI")]
    public GameObject panelDialogo; 
    public TextMeshProUGUI textoDialogo; 

    [Header("Configuración")]
    public float tiempoPorMensaje = 1.5f; 

    private Queue<string> colaMensajes = new Queue<string>();
    private bool mostrandoMensaje = false;

    void Awake() {
        if (Instance == null) Instance = this;
    }

    public void AgregarMensaje(string mensaje) {
        colaMensajes.Enqueue(mensaje);
        if (!mostrandoMensaje) {
            StartCoroutine(MostrarMensajesRoutine());
        }
    }

    IEnumerator MostrarMensajesRoutine() {
        mostrandoMensaje = true;

        if (panelDialogo != null && !panelDialogo.activeSelf) {
            panelDialogo.SetActive(true);
        }

        while (colaMensajes.Count > 0) {
            string mensajeActual = colaMensajes.Dequeue();
            textoDialogo.text = mensajeActual;
            
            yield return new WaitForSecondsRealtime(tiempoPorMensaje);
        }

        mostrandoMensaje = false;
    }
    
    public void LimpiarMensajes() {
        colaMensajes.Clear();
        textoDialogo.text = "";
        mostrandoMensaje = false;
    }
}