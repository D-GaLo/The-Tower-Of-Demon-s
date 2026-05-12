using UnityEngine;

public class InventarioPanelUI : MonoBehaviour
{
    public static InventarioPanelUI Instance;
    
    [Header("Referencias")]
    public GameObject panelVisual;
    
    private EquipSlotUI slotSiendoEditado;

    private void Awake() { 
        Instance = this; 
        panelVisual.SetActive(false); 
    }

    public void AbrirParaSlot(EquipSlotUI slotPedidor)
    {
        slotSiendoEditado = slotPedidor;
        panelVisual.SetActive(true);
    }

    public void SeleccionarItem(Item itemSeleccionado)
    {
        if (slotSiendoEditado != null)
        {
            bool exito = slotSiendoEditado.equipo.Equipar(slotSiendoEditado.slotIndex, itemSeleccionado);
            
            if (exito)
            {
                slotSiendoEditado.Refrescar();
                panelVisual.SetActive(false); 
            }
        }
    }
}