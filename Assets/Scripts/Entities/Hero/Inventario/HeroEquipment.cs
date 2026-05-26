
using UnityEngine;

public class HeroEquipment : MonoBehaviour
{
    public Item[] slots = new Item[3] { Item.None, Item.None, Item.None };

    public static bool EsArma(Item item)
    {
        return item == Item.Espada || item == Item.Baculo || item== Item.Escudo;
    }

    public bool PuedeEquipar(int slotIndex, Item item)
    {
        if (item == Item.None) return true; 
        if (slotIndex == 0) return EsArma(item); 
        return true;//!EsArma(item);
    }

    public bool Equipar(int slotIndex, Item item)
    {
        if (!PuedeEquipar(slotIndex, item)) return false;

        if (slots[slotIndex] != Item.None)
            InventarioEnum.Instance.AddItem(slots[slotIndex], 1);

        if (item != Item.None)
            InventarioEnum.Instance.RemoveItem(item, 1);

        slots[slotIndex] = item;
        return true;
    }

    public void Desequipar(int slotIndex)
    {
        if (slots[slotIndex] == Item.None) return;
        InventarioEnum.Instance.AddItem(slots[slotIndex], 1);
        slots[slotIndex] = Item.None;
    }
}