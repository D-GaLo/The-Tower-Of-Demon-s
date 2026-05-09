using UnityEngine;

public class TijerasGuardian : MonoBehaviour {
    [Header("Referencias del Mundo")]
    public GameObject slimePadre; 
    public GameObject visualTijerasMapa; // La imagen con baba en el suelo

    [Header("Interfaz de Mensaje")]
    public GameObject contenedorPrincipal; 
    public GameObject panelMensaje; 

    private bool entregado = false;

    void Start() {
        if(panelMensaje) panelMensaje.SetActive(false);
    }

    void Update() {
        // Detecta si el Slime Padre fue destruido tras el combate
        if (slimePadre == null && !entregado) {
            DarPremio();
        }
    }

    void DarPremio() {
        entregado = true;
        if(visualTijerasMapa) visualTijerasMapa.SetActive(false);

        // Registro en el inventario
        if (InventarioEnum.Instance != null) {
            InventarioEnum.Instance.AddItem(Item.Tijeras, 1);
        }

        // Mostrar la UI que ya tiene el texto escrito
        Time.timeScale = 0f; 
        if(contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if(panelMensaje) panelMensaje.SetActive(true);
    }

    // Función para el botón "Aceptar" del panel
    public void CerrarMensaje() {
        if(panelMensaje) panelMensaje.SetActive(false);
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false);
        Time.timeScale = 1f; 
        Destroy(gameObject); 
    }
}