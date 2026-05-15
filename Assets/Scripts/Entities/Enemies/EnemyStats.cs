using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : UnitStats {
    
    [Header("Configuración de Nivel")]
    [Tooltip("Activa esto en Jefes o enemigos fijos para que el Spawner no cambie su nivel. Pon el nivel manualmente arriba.")]
    public bool nivelManual = false; 

    [Header("Recompensas (Drop)")]
    public WeaponData weaponDrop;
    [Range(0, 100)] public int dropChance = 20; 

    [Header("UI Visual en la Arena")]
    public GameObject canvasUI; 
    public Image barraVida;
    public Image circuloClase;
    public Image circuloPosicion;

    [Header("Sprites de Clase")]
    public Sprite spriteMelee;
    public Sprite spriteRango;
    public Sprite spriteTanque;

    [Header("Sprites de Posición")]
    public Sprite spriteTierra;
    public Sprite spriteBajoTierra;
    public Sprite spriteVolando;

    void Awake() {
        if (canvasUI != null) canvasUI.SetActive(false);
    }

    void Start() {
        // Si es un Jefe o está puesto a mano en el mapa, escalamos sus stats desde el inicio
        if (nivelManual) {
            EscalarEstadisticas();
        } else if (level == 1 && currentHP == 0) {
            // Por seguridad, si es Nivel 1 le damos su vida base
            currentHP = maxHP;
        }

        if (string.IsNullOrEmpty(unitName)) {
            unitName = gameObject.name;
        }
    }

    // --- NUEVO: CRECIMIENTO DE ESTADÍSTICAS ---
    public void EscalarEstadisticas() {
        if (level > 1) {
            int nivelesExtra = level - 1;
            
            // Fórmula genérica de crecimiento (Puedes cambiar estos números si quieres enemigos más duros)
            maxHP += (nivelesExtra * 25);
            attack += (nivelesExtra * 8);
            defense += (nivelesExtra * 6);
            speed += (nivelesExtra * 4);
        }
        
        // Lo curamos al tope de su nueva vida máxima
        currentHP = maxHP;
    }

    public void ActivarUICombate() {
        if (canvasUI != null) canvasUI.SetActive(true);
        ActualizarVisuales();
    }

    public override void TakeDamage(int damage) {
        base.TakeDamage(damage); 
        ActualizarVisuales();    
    }

    public void ActualizarVisuales() {
        if (barraVida != null && maxHP > 0) {
            barraVida.fillAmount = (float)currentHP / (float)maxHP;
        }

        if (circuloClase != null) {
            if (unitClass == UnitClass.Melee) circuloClase.sprite = spriteMelee;
            else if (unitClass == UnitClass.Rango) circuloClase.sprite = spriteRango;
            else if (unitClass == UnitClass.Tanque) circuloClase.sprite = spriteTanque;
            
            circuloClase.color = Color.white; 
        }

        if (circuloPosicion != null) {
            if (unitPosition == UnitPosition.Tierra) circuloPosicion.sprite = spriteTierra;
            else if (unitPosition == UnitPosition.Volando) circuloPosicion.sprite = spriteVolando;
            else if (unitPosition == UnitPosition.BajoTierra) circuloPosicion.sprite = spriteBajoTierra;
            
            circuloPosicion.color = Color.white; 
        }
    }

    public WeaponData TryGetDrop() {
        if (weaponDrop == null) return null;
        int randomValue = Random.Range(1, 101);
        if (randomValue <= dropChance) return weaponDrop; 
        return null; 
    }
}