using UnityEngine;
using UnityEngine.UI; 

public class EnemyStats : UnitStats {
    
    [Header("Recompensas (Drop)")]
    public WeaponData weaponDrop;
    [Range(0, 100)] public int dropChance = 20; 

    [Header("UI Visual en la Arena")]
    public Image barraVida;
    public Image circuloClase;
    public Image circuloPosicion;

    void Start() {
        currentHP = maxHP;
        if (string.IsNullOrEmpty(unitName)) {
            unitName = gameObject.name;
        }
        ActualizarVisuales();
    }

    // Sobreescribimos la función de recibir daño para que baje la barrita
    public override void TakeDamage(int damage) {
        base.TakeDamage(damage);
        ActualizarVisuales();    // Actualiza la barrita
    }

    void ActualizarVisuales() {
        if (barraVida != null) {
            barraVida.fillAmount = (float)currentHP / maxHP;
        }

        // Colorear el Círculo de Clase
        if (circuloClase != null) {
            if (unitClass == UnitClass.Melee) circuloClase.color = Color.red;
            else if (unitClass == UnitClass.Rango) circuloClase.color = Color.green;
            else if (unitClass == UnitClass.Tanque) circuloClase.color = Color.blue;
        }

        // Colorear el Círculo de Posición
        if (circuloPosicion != null) {
            if (unitPosition == UnitPosition.Tierra) circuloPosicion.color = new Color(0.6f, 0.3f, 0f); // Café
            else if (unitPosition == UnitPosition.Volando) circuloPosicion.color = Color.cyan;
            else if (unitPosition == UnitPosition.BajoTierra) circuloPosicion.color = Color.black;
        }
    }

    public WeaponData TryGetDrop() {
        if (weaponDrop == null) return null;
        int randomValue = Random.Range(1, 101);
        if (randomValue <= dropChance) return weaponDrop; 
        return null; 
    }
}