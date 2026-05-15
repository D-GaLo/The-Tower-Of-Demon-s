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
        currentHP = maxHP;
        if (string.IsNullOrEmpty(unitName)) {
            unitName = gameObject.name;
        }
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
        // 1. Actualizar la barra de vida
        if (barraVida != null && maxHP > 0) {
            barraVida.fillAmount = (float)currentHP / (float)maxHP;
        }

        // 2. Asignar el sprite de la clase
        if (circuloClase != null) {
            if (unitClass == UnitClass.Melee) circuloClase.sprite = spriteMelee;
            else if (unitClass == UnitClass.Rango) circuloClase.sprite = spriteRango;
            else if (unitClass == UnitClass.Tanque) circuloClase.sprite = spriteTanque;
            
            // Ponemos el color en blanco para que la imagen se vea con sus colores reales
            circuloClase.color = Color.white; 
        }

        // 3. Asignar el sprite de la posición
        if (circuloPosicion != null) {
            if (unitPosition == UnitPosition.Tierra) circuloPosicion.sprite = spriteTierra;
            else if (unitPosition == UnitPosition.Volando) circuloPosicion.sprite = spriteVolando;
            else if (unitPosition == UnitPosition.BajoTierra) circuloPosicion.sprite = spriteBajoTierra;
            
            // Ponemos el color en blanco
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