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


    void Start()
    {
        Refrescar();
    }

    public void Refrescar()
    {
        if (equipo == null) return;
        if (equipo.slots == null || slotIndex >= equipo.slots.Length) return;
        Item item = equipo.slots[slotIndex];
        if (item == Item.None){
            if (nombreTexto != null){
                if (slotIndex == 0)
                {
                    nombreTexto.text = "Arma";
                }
                else
                {
                    nombreTexto.text = "Item";
                }
            }
        }
        else
        {
            if (nombreTexto != null) 
               nombreTexto.text = item.ToString();
        }

        if (iconoImagen != null){
            iconoImagen.enabled = true;
            if (item == Item.None)
            {
                iconoImagen.sprite = null;
                iconoImagen.color = new Color(1, 1, 1, 0); 
            }
            else
            {
                if (InventarioEnum.Instance != null){
                    Sprite spriteDelObjeto = InventarioEnum.Instance.GetSpriteDeItem(item);

                    if (spriteDelObjeto != null)
                    {
                        iconoImagen.sprite = spriteDelObjeto; 
                        iconoImagen.color = Color.white; 
                    }
                    else
                    {
                        iconoImagen.sprite = null;
                        iconoImagen.color = new Color(1, 1, 1, 0);
                    }
                }
                else
                {
                    Debug.LogWarning(" No se encontró la instancia de InventarioEnum en la escena.");
                    iconoImagen.sprite = null;
                    iconoImagen.color = new Color(1, 1, 1, 0);
                }
            }

        }
            //iconoImagen.enabled = item != Item.None;
            
    }

    public void OnDrop(PointerEventData e)
    {
        if (InventarioSlotUI.itemArrastrado == Item.None || equipo == null) return;

        bool paEquipar = equipo.Equipar(slotIndex, InventarioSlotUI.itemArrastrado);
        if (paEquipar){
            Refrescar();
           if (InventarioSlotUI.slotOrigen != null){
                InventarioSlotUI.slotOrigen.Refrescar();
                /*InventarioSlotUI.slotOrigen.Setup(
                    InventarioSlotUI.slotOrigen.item,
                    InventarioSlotUI.slotOrigen.iconoImagen.sprite
                );*/
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

    public void ActualizarSlot(WeaponData armaEquipada)
    {
        
        if (equipo == null || equipo.slots == null) return;

        if (armaEquipada != null)
        {
           
            string nombreLimpio = armaEquipada.weaponName.Replace(" ", ""); 
            if (System.Enum.TryParse(nombreLimpio, out Item itemConvertido))
            {   
                if (slotIndex < equipo.slots.Length){
                    equipo.slots[slotIndex] = itemConvertido;
                }
               
            }
        }
        else
        {
            if (slotIndex < equipo.slots.Length){
                equipo.slots[slotIndex] = Item.None;
            }
        }

        Refrescar(); 
        
    }
}