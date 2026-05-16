using UnityEngine;

public class HeroStats : UnitStats {
    
    [Header("Estadísticas Exclusivas de Héroes")]
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Experiencia y Progreso")]
    public int currentXP = 0;
    public int maxLevel = 10;
    public int[] xpParaNivel = { 0, 100, 250, 450, 750, 1550, 2350, 3750, 5150, 8650 };

    [Header("Equipamiento")]
    public WeaponData equippedWeapon;

    private const int LIMITE_HP = 1000;
    private const int LIMITE_ATK = 500;
    private const int LIMITE_DEF = 500;
    private const int LIMITE_SPD = 500;
    private const int LIMITE_ENERGIA = 500;
    private const int LIMITE_MAESTRIA = 500;

    void Start() {
        ConfigurarStatsBaseIniciales();
        currentHP = maxHP;
        currentEnergy = maxEnergy;
    }

    void ConfigurarStatsBaseIniciales() {
        if (level == 1) {
            if (unitName == "Sieg") {
                maxHP = 120; attack = 15; defense = 12; speed = 15; maxEnergy = 100;
            } 
            else if (unitName == "Merlin") {
                maxHP = 80; attack = 25; defense = 5; speed = 18; maxEnergy = 150;
            }
            else if (unitName == "Heracles") {
                maxHP = 160; attack = 20; defense = 25; speed = 6; maxEnergy = 80;
            }
        }
    }

    public void GanarExperiencia(int cantidadXP) {
        if (level >= maxLevel) return;

        currentXP += cantidadXP;
        Debug.Log($"<color=yellow>[Experiencia]</color> {unitName} ganó {cantidadXP} XP. Total: {currentXP}/{xpParaNivel[level]}");

        while (level < maxLevel && currentXP >= xpParaNivel[level]) {
            SubirDeNivel();
        }
    }

    void SubirDeNivel() {
        level++;
        Debug.Log($"<color=green>[Level UP!]</color> ¡{unitName} alcanzó el Nivel {level}!");

        if (unitName == "Sieg") {
            maxHP += 40;
            attack += 20;
            defense += 20;
            speed += 20;
            maxEnergy += 22;
            mastery += 13;
        } 
        else if (unitName == "Merlin") {
            maxHP += 25;
            attack += 25;
            defense += 10;
            speed += 18;
            maxEnergy += 25;
            mastery += 15;
        }
        else if (unitName == "Heracles") {
            maxHP += 45;
            attack += 22;
            defense += 25;
            speed += 10;
            maxEnergy += 15;
            mastery += 10;
        }

        AplicarLimitesDeGDD();

        currentHP = maxHP;
        currentEnergy = maxEnergy;
    }

    void AplicarLimitesDeGDD() {
        maxHP = Mathf.Min(maxHP, LIMITE_HP);
        attack = Mathf.Min(attack, LIMITE_ATK);
        defense = Mathf.Min(defense, LIMITE_DEF);
        speed = Mathf.Min(speed, LIMITE_SPD);
        maxEnergy = Mathf.Min(maxEnergy, LIMITE_ENERGIA);
        mastery = Mathf.Min(mastery, LIMITE_MAESTRIA);
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