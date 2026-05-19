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
        int cantidad = InventarioEnum.Instance.GetCantidad(item);
        iconoImagen.sprite = icono;
        iconoImagen.enabled = cantidad > 0;
        cantidadTexto.text = cantidad > 0 ? cantidad.ToString() : "";
        if (cantidad > 0)
        {
            iconoImagen.color = Color.white;
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