using UnityEngine;

public class HeroEquipment : MonoBehaviour
{
    public ItemData[] slots = new ItemData[3];
    private HeroStats myStats;

    void Awake() {
        myStats = GetComponent<HeroStats>();
    }

    public bool PuedeEquipar(int slotIndex, ItemData item)
    {
        if (item == null) return true; 
        if (item.category == ItemCategory.ObjetoClave) return false; 

        if (slotIndex == 0) {
            return item.category == ItemCategory.Arma && item.requiredClass == myStats.unitClass;
        } else {
            return item.category == ItemCategory.Objeto;
        }
    }

    public bool Equipar(int slotIndex, ItemData item)
    {
        if (!PuedeEquipar(slotIndex, item)) return false;

        if (slots[slotIndex] != null) {
            InventarioEnum.Instance.AddItem(slots[slotIndex].itemID, 1);
            RemoverBonoDeStats(slots[slotIndex]); 
        }

        if (item != null) {
            InventarioEnum.Instance.RemoveItem(item.itemID, 1);
            AplicarBonoDeStats(item); 
        }

        slots[slotIndex] = item;
        
        if (slotIndex == 0) myStats.equippedWeapon = item;

        return true;
    }

    public void Desequipar(int slotIndex)
    {
        if (slots[slotIndex] == null) return;
        
        InventarioEnum.Instance.AddItem(slots[slotIndex].itemID, 1);
        
        // Al quitarnos el objeto, perdemos los bonos
        RemoverBonoDeStats(slots[slotIndex]); 

        slots[slotIndex] = null;
        if (slotIndex == 0) myStats.equippedWeapon = null;
    }

    private void AplicarBonoDeStats(ItemData item) {
        if (item == null) return;
        myStats.maxHP += item.bonusHP;
        myStats.currentHP += item.bonusHP;
        myStats.maxEnergy += item.bonusEnergy;
        myStats.currentEnergy += item.bonusEnergy;
        
        myStats.attack += item.bonusATK;
        myStats.defense += item.bonusDEF;
        myStats.speed += item.bonusSPD;
        myStats.mastery += item.bonusMastery;
    }

    private void RemoverBonoDeStats(ItemData item) {
        if (item == null) return;
        myStats.maxHP -= item.bonusHP;
        if (myStats.currentHP > myStats.maxHP) myStats.currentHP = myStats.maxHP;
        
        myStats.maxEnergy -= item.bonusEnergy;
        if (myStats.currentEnergy > myStats.maxEnergy) myStats.currentEnergy = myStats.maxEnergy;

        myStats.attack -= item.bonusATK;
        myStats.defense -= item.bonusDEF;
        myStats.speed -= item.bonusSPD;
        myStats.mastery -= item.bonusMastery;
    }
}