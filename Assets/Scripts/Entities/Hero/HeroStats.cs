using UnityEngine;

public class HeroStats : UnitStats {
    
    [Header("Estadísticas Exclusivas de Héroes")]
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Experiencia y Progreso")]
    public int currentXP = 0;
    public int maxLevel = 10;
    // Puntos totales necesarios para alcanzar el siguiente nivel. 
    // Índice 1 = XP para Nivel 2, Índice 2 = XP para Nivel 3, etc.
    public int[] xpParaNivel = { 0, 100, 300, 650, 1250, 2250, 3750, 5950, 9150, 13650 };

    [Header("Equipamiento")]
    public WeaponData equippedWeapon;

    // --- LÍMITES DE ESTADÍSTICAS GDD ---
    private const int LIMITE_HP = 500;
    private const int LIMITE_ATK = 200;
    private const int LIMITE_DEF = 200;
    private const int LIMITE_SPD = 200;
    private const int LIMITE_ENERGIA = 300;
    private const int LIMITE_MAESTRIA = 120;

    void Start() {
        ConfigurarStatsBaseIniciales(); // Asegura que los personajes tengan sus stats correctos al Nivel 1
        currentHP = maxHP;
        currentEnergy = maxEnergy;
    }

    // Configura las estadísticas base automáticamente para no tener que hacerlo a mano en Unity
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

    // --- SISTEMA DE EXPERIENCIA Y SUBIDA DE NIVEL ---
    public void GanarExperiencia(int cantidadXP) {
        if (level >= maxLevel) return; // Si es nivel 10, no hace nada

        currentXP += cantidadXP;
        Debug.Log($"<color=yellow>[Experiencia]</color> {unitName} ganó {cantidadXP} XP. Total: {currentXP}/{xpParaNivel[level]}");

        // Si la experiencia supera el requisito, sube de nivel (usa While por si gana XP para subir 2 niveles de golpe)
        while (level < maxLevel && currentXP >= xpParaNivel[level]) {
            SubirDeNivel();
        }
    }

    void SubirDeNivel() {
        level++;
        Debug.Log($"<color=green>[Level UP!]</color> ¡{unitName} alcanzó el Nivel {level}!");

        // Crecimiento de estadísticas según la identidad del personaje
        if (unitName == "Sieg") {
            // Equilibrado en todo
            maxHP += 40;
            attack += 20;
            defense += 20;
            speed += 20;
            maxEnergy += 22;
            mastery += 13;
        } 
        else if (unitName == "Merlin") {
            // Mago: Cañón de cristal (Mucho ataque/energía, poca defensa)
            maxHP += 25;
            attack += 25;
            defense += 10;
            speed += 18;
            maxEnergy += 25;
            mastery += 15;
        }
        else if (unitName == "Heracles") {
            // Tanque: Fuerte, muy resistente, pero muy lento
            maxHP += 45;
            attack += 22;
            defense += 25;
            speed += 10;
            maxEnergy += 15;
            mastery += 10;
        }

        AplicarLimitesDeGDD();

        // Al subir de nivel, se recupera toda la vida y la energía como recompensa
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

    // --- FUNCIONES ORIGINALES ---
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