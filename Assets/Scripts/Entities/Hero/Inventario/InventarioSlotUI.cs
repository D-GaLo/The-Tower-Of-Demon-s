using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventarioSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (InventarioEnum.Instance.GetCantidad(item) <= 0) return;

        itemArrastrado = item;
        slotOrigen = this;

        fantasma = new GameObject("Fantasma");
        fantasma.transform.SetParent(transform.root, false); 
        var img = fantasma.AddComponent<Image>();
        img.sprite = iconoImagen.sprite;
        img.raycastTarget = false;
        var rect = fantasma.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(48, 48);
    }

    public void OnDrag(PointerEventData e)
    {
        if (fantasma == null) return;
        fantasma.transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (fantasma != null) Destroy(fantasma);
        itemArrastrado = Item.None;
        slotOrigen = null;
    }
}