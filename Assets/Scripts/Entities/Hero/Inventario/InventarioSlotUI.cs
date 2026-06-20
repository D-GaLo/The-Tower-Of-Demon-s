using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventarioSlotUI : MonoBehaviour {
    public Image iconoObjeto;
    public TextMeshProUGUI textoCantidad;

    private ItemData itemData;
    private int cantidadActual;
    private InventarioPanelUI panelPrincipal;

    public void Configurar(ItemData data, int cantidad, InventarioPanelUI panel) {
        itemData = data;
        cantidadActual = cantidad;
        panelPrincipal = panel;

        iconoObjeto.sprite = data.itemIcon;
        
        if (data.maxStack > 1) {
            textoCantidad.text = cantidad.ToString();
            textoCantidad.gameObject.SetActive(true);
        } else {
            textoCantidad.gameObject.SetActive(false);
        }

        if (cantidad <= 0) {
            iconoObjeto.color = new Color(0.3f, 0.3f, 0.3f, 1f); 
        } else {
            iconoObjeto.color = Color.white;
        }
    }

    public void AlHacerClic() {
        if (panelPrincipal != null && itemData != null && AudioManager.Instance != null) {
            AudioManager.Instance.PlayClic();
            panelPrincipal.MostrarDetalles(itemData, cantidadActual);
        }
    }
}