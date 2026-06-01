using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogoCombateUI : MonoBehaviour {
    public static DialogoCombateUI Instance;

    [Header("Referencias UI")]
    public GameObject panelDialogo; 
    public TextMeshProUGUI textoDialogo; 

    [Header("Configuración")]
    [Tooltip("Cuántos mensajes se muestran al mismo tiempo antes de borrar el más viejo.")]
    public int maxLineas = 4; 

    private List<string> lineasActivas = new List<string>();

    void Awake() {
        if (Instance == null) Instance = this;
    }

    public void AgregarMensaje(string mensaje) {
        if (panelDialogo != null && !panelDialogo.activeSelf) {
            panelDialogo.SetActive(true);
        }
        lineasActivas.Add(mensaje);

        if (lineasActivas.Count > maxLineas) {
            lineasActivas.RemoveAt(0); 
        }

        ActualizarTexto();
    }
    
    void ActualizarTexto() {
        if (textoDialogo != null) {
            textoDialogo.text = string.Join("\n", lineasActivas);
        }
    }

    public void LimpiarMensajes() {
        lineasActivas.Clear();
        if (textoDialogo != null) textoDialogo.text = "";
        if (panelDialogo != null) panelDialogo.SetActive(false);
    }
}