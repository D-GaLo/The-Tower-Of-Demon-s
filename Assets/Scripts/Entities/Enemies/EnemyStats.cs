using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : UnitStats {
    
    [Header("Recompensas (Drop)")]
    public WeaponData weaponDrop;
    [Range(0, 100)] public int dropChance = 20; 

    [Header("UI Visual en la Arena")]
    public GameObject canvasUI; 
    public Image barraVida;
    public Image circuloClase;
    public Image circuloPosicion;

    void Start() {
        currentHP = maxHP;
        if (string.IsNullOrEmpty(unitName)) {
            unitName = gameObject.name;
        }
        
        // APAGAMOS EL CANVAS AL INICIAR PARA QUE NO SE VEA EN EL MAPA
        if (canvasUI != null) canvasUI.SetActive(false);
    }

    // Esta función la llamará el CombatManager cuando los clone en la arena
    public void ActivarUICombate() {
        if (canvasUI != null) canvasUI.SetActive(true);
        ActualizarVisuales();
    }

    // Sobreescribimos la función de daño para que la barra se mueva
    public override void TakeDamage(int damage) {
        base.TakeDamage(damage); 
        ActualizarVisuales();    
    }

    public void ActualizarVisuales() {
        // Actualizar barra de vida
        if (barraVida != null && maxHP > 0) {
            // Aseguramos que la división use decimales con (float)
            barraVida.fillAmount = (float)currentHP / (float)maxHP;
        }

        // Colorear Círculos
        if (circuloClase != null) {
            if (unitClass == UnitClass.Melee) circuloClase.color = Color.red;
            else if (unitClass == UnitClass.Rango) circuloClase.color = Color.green;
            else if (unitClass == UnitClass.Tanque) circuloClase.color = Color.blue;
        }

        if (circuloPosicion != null) {
            if (unitPosition == UnitPosition.Tierra) circuloPosicion.color = new Color(0.6f, 0.3f, 0f);
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