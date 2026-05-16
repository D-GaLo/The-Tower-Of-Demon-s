using UnityEngine;

public class TijerasGuardian : MonoBehaviour {
    [Header("Referencias del Mundo")]
    public GameObject slimePadre; 
    public GameObject visualTijerasMapa;

    [Header("Interfaz de Mensaje")]
    public GameObject contenedorPrincipal; 
    public GameObject panelMensaje; 

    private bool entregado = false;

    void Start() {
        if(panelMensaje) panelMensaje.SetActive(false);
    }

    void Update() {
        if (slimePadre == null && !entregado) {
            DarPremio();
        }
    }

    void DarPremio() {
        entregado = true;
        if(visualTijerasMapa) visualTijerasMapa.SetActive(false);

        if (InventarioEnum.Instance != null) {
            InventarioEnum.Instance.AddItem(Item.Tijeras, 1);
        }

        Time.timeScale = 0f; 
        if(contenedorPrincipal) contenedorPrincipal.SetActive(true);
        if(panelMensaje) panelMensaje.SetActive(true);
    }

    public void CerrarMensaje() {
        if(panelMensaje) panelMensaje.SetActive(false);
        if(contenedorPrincipal) contenedorPrincipal.SetActive(false);
        Time.timeScale = 1f; 
        Destroy(gameObject); 
    }
}