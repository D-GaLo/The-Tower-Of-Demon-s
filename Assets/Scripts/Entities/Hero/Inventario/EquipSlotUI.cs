using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipSlotUI : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;        
    public HeroEquipment equipo;  
    public Image iconoImagen;
    public TextMeshProUGUI nombreTexto;

    public void Refrescar()
    {
        if (equipo == null || nombreTexto == null) return;
        
        ItemData item = equipo.slots[slotIndex];
        if (item == null){
            nombreTexto.text = (slotIndex == 0) ? "Arma" : "Accesorio";
            if (iconoImagen != null) iconoImagen.enabled = false;
        }
        else
        {
            nombreTexto.text = item.itemName;
            if (iconoImagen != null) {
                iconoImagen.sprite = item.itemIcon;
                iconoImagen.enabled = true;
            }
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left){
            if (InventarioPanelUI.Instance != null && AudioManager.Instance != null) {
                AudioManager.Instance.PlayClic();
                InventarioPanelUI.Instance.AbrirParaSlot(this);
            }
        }
        else if (e.button == PointerEventData.InputButton.Right && equipo != null){
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
            equipo.Desequipar(slotIndex);
            Refrescar();
        }
    }
}