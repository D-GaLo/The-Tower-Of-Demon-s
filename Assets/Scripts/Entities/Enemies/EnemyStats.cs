using UnityEngine;

public class EnemyStats : UnitStats {
    
    [Header("Recompensas (Drop)")]
    [Tooltip("Arrastra aquí un ScriptableObject de arma. Déjalo vacío si no suelta nada.")]
    public WeaponData weaponDrop;

    [Tooltip("Probabilidad de soltar el arma (0 a 100%).")]
    [Range(0, 100)]
    public int dropChance = 20; // Aquí está tu 20% de probabilidad de soltar el arma modificable desde el Inspector

    void Start() {
        currentHP = maxHP;
        if (string.IsNullOrEmpty(unitName)) {
            unitName = gameObject.name;
        }
    }

    // Lanza el dado para ver si suelta el arma
    public WeaponData TryGetDrop() {
        if (weaponDrop == null) return null;

        // Genera un número del 1 al 100
        int randomValue = Random.Range(1, 101);
        if (randomValue <= dropChance) {
            return weaponDrop; // ¡Cayó en el 20%! Suelta el arma
        }
        return null; // Mala suerte, no soltó nada
    }
}