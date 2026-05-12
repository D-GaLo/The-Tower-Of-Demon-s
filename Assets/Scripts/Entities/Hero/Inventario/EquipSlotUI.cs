using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public int slotIndex;        
    public HeroEquipment equipo;  
    public Image iconoImagen;
    public TextMeshProUGUI nombreTexto;

    public void Refrescar()
    {
        if (equipo == null) return;
        if (nombreTexto == null) return;
        Item item = equipo.slots[slotIndex];
        if (item == Item.None){
            if (slotIndex == 0)
            {
                nombreTexto.text = "Arma";
            }
            else
            {
                nombreTexto.text = "Item";
            }
        }
        else
        {
            nombreTexto.text = item.ToString();
        }

        if (iconoImagen != null)
            iconoImagen.enabled = item != Item.None;
            
    }

    public void OnDrop(PointerEventData e)
    {
        if (InventarioSlotUI.itemArrastrado == Item.None || equipo == null) return;

        bool paEquipar = equipo.Equipar(slotIndex, InventarioSlotUI.itemArrastrado);
        if (paEquipar){
            Refrescar();
           if (InventarioSlotUI.slotOrigen != null){
                InventarioSlotUI.slotOrigen.Setup(
                    InventarioSlotUI.slotOrigen.item,
                    InventarioSlotUI.slotOrigen.iconoImagen.sprite
                );
            }
        }
        else{
            Debug.Log("Item no válido para este slot");
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left){
            InventarioPanelUI.Instance.AbrirParaSlot(this);
        }

        else if (e.button == PointerEventData.InputButton.Right && equipo != null){
            equipo.Desequipar(slotIndex);
            Refrescar();
        }
    }
}