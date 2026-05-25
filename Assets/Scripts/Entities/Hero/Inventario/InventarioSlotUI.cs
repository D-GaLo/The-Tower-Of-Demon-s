using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventarioSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Item item;
    public Image iconoImagen;
    public TextMeshProUGUI cantidadTexto;

    public static Item itemArrastrado = Item.None;
    public static InventarioSlotUI slotOrigen = null;

    private static GameObject fantasma;

    public void Setup(Item _item, Sprite icono)
    {
        item = _item;
        item = _item;
        if (iconoImagen != null)
        {
            iconoImagen.sprite = icono;
        }
        
        Refrescar();
    }


    public void Refrescar()
    {
        if (InventarioEnum.Instance == null) return;

        int cantidad = InventarioEnum.Instance.GetCantidad(item);

        if (iconoImagen != null)
        {
            if (item == Item.None || cantidad <= 0)
            {
                iconoImagen.sprite = null;
                iconoImagen.color = new Color(1, 1, 1, 0); 
            }
            else
            {
                iconoImagen.color = Color.white;
                iconoImagen.enabled = true;
                
                if(iconoImagen.sprite == null)
                {
                    iconoImagen.sprite = InventarioEnum.Instance.GetSpriteDeItem(item);
                }
            }
        }

        if (cantidadTexto != null)
        {
            if (item == Item.None || cantidad <= 0)
            {
                cantidadTexto.text = "";
            }
            else
            {
                cantidadTexto.text = cantidad.ToString();
            }
        }
    }

    public void OnPointerClick(PointerEventData e){
        if (InventarioEnum.Instance.GetCantidad(item) > 0)
        {
            Debug.Log("Clic en item: " + item);
            InventarioPanelUI.Instance.SeleccionarItem(item);
        }
    }

}