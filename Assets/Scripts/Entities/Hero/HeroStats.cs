using UnityEngine;

// Hereda de UnitStats, así que ya tiene HP, Ataque, Velocidad, etc.
public class HeroStats : UnitStats {
    
    [Header("Estadísticas Exclusivas de Héroes")]
    public int maxEnergy = 100;
    public int currentEnergy;
    
    [Tooltip("Reduce teclas o aumenta el tiempo del QTE")]
    public int mastery = 0; 

    [Header("Equipamiento")]
    public WeaponData equippedWeapon;

    void Start() {
        // Inicializamos los valores al tope cuando empieza el juego
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

    // Calcula el ataque sumando la estadística base más el arma
    public int GetTotalAttack() {
        int total = attack;
        if (equippedWeapon != null) {
            total += equippedWeapon.attackBonus;
        }
        return total;
    }

    // Intenta ponerse el arma si la clase coincide
    public void TryEquipWeapon(WeaponData newWeapon) {
        if (newWeapon.requiredClass == this.unitClass) {
            equippedWeapon = newWeapon;
            Debug.Log($"¡{unitName} se equipó {newWeapon.weaponName}! Ataque total ahora es {GetTotalAttack()}");
        }
    }
}