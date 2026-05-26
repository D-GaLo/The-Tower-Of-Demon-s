using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyStats : UnitStats {
    
    [Header("Configuración de Nivel y Jefe")]
    [Tooltip("Activar esto en Jefes o enemigos fijos para que el Spawner no cambie su nivel.")]
    public bool nivelManual = false; 
    public bool esJefe = false;
    public int ataquesPorTurno = 1; 

    [Tooltip("Activar esto si se pusieron las estadísticas exactas a mano en el Inspector y no quieres que el nivel las altere.")]
    public bool statsManuales = false; 
    
    [Header("Compañeros de Combate")]
    [Tooltip("Enemigos que pueden aparecer junto a este enemigo en combates normales.")]
    public GameObject[] posiblesCompaneros;
    [Tooltip("Enemigos que el Jefe invocará cuando su vida baje al 50%.")]
    public GameObject[] enemigosAInvocar;
    [HideInInspector] public bool yaInvoco = false;

    [Header("Dropeos (Orden de prioridad: arriba a abajo)")]
    public DropData[] posiblesDropeos;

    [Header("UI Visual en la Arena")]
    public GameObject canvasUI; 
    public Image barraVida;
    public Image circuloClase;
    public Image circuloPosicion;
    public TextMeshProUGUI textoNivel;

    [Header("Sprites de Clase")]
    public Sprite spriteMelee;
    public Sprite spriteRango;
    public Sprite spriteTanque;

    [Header("Sprites de Posición")]
    public Sprite spriteTierra;
    public Sprite spriteBajoTierra;
    public Sprite spriteVolando;


    [HideInInspector] public int baseMaxHP;
    [HideInInspector] public int baseAttack;
    [HideInInspector] public int baseDefense;
    [HideInInspector] public int baseSpeed;
    [HideInInspector] public bool basesGuardadas = false;

    void Awake() {
        if (canvasUI != null) canvasUI.SetActive(false);
    }

    void Start() {
        if (nivelManual) {
            EscalarEstadisticas();
        } else if (level == 1 && currentHP == 0) {
            currentHP = maxHP;
        }

        if (string.IsNullOrEmpty(unitName)) {
            unitName = gameObject.name;
        }
    }

    public void EscalarEstadisticas() {
        if (statsManuales) {
            currentHP = maxHP; 
            return;
        }

        if (!basesGuardadas) {
            baseMaxHP = maxHP;
            baseAttack = attack;
            baseDefense = defense;
            baseSpeed = speed;
            basesGuardadas = true;
        }

        if (level > 1) {
            int nivelesExtra = level - 1;
            
            maxHP = baseMaxHP + (nivelesExtra * 50);
            attack = baseAttack + (nivelesExtra * 25);
            defense = baseDefense + (nivelesExtra * 20);
            speed = baseSpeed + (nivelesExtra * 20);
        } else {
            maxHP = baseMaxHP;
            attack = baseAttack;
            defense = baseDefense;
            speed = baseSpeed;
        }
        
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

        if (textoNivel != null) textoNivel.text = "Lv. " + level;

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
}

[System.Serializable]
public class DropData {
    public ItemData item;
    [Range(0, 100)] public int probabilidad;
}