using UnityEngine;

public class HeroStats : UnitStats {
    
    [Header("Estadísticas Exclusivas de Héroes")]
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Equipamiento")]
    public WeaponData equippedWeapon;

    void Start() {
        currentHP = maxHP;
        currentEnergy = maxEnergy;
    }

    public bool UseEnergy(int amount) {
        if (currentEnergy >= amount) {
            currentEnergy -= amount;
            return true;
        }
        return false;
    }

    public int GetTotalAttack() {
        int total = attack;
        if (equippedWeapon != null) {
            total += equippedWeapon.attackBonus;
        }
        return total;
    }

    public void TryEquipWeapon(WeaponData newWeapon) {
        if (newWeapon.requiredClass == this.unitClass) {
            equippedWeapon = newWeapon;
            Debug.Log($"¡{unitName} se equipó {newWeapon.weaponName}! Ataque total ahora es {GetTotalAttack()}");
        }
    }
}