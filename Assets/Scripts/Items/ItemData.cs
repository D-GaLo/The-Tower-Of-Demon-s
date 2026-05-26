using UnityEngine;

public enum ItemCategory {
    Arma,
    Objeto,
    ObjetoClave
}

[CreateAssetMenu(fileName = "NuevoObjeto", menuName = "TowerOfDemons/Objeto")]
public class ItemData : ScriptableObject {
    [Header("Identificación Visual")]
    public Item itemID;
    public string itemName;
    [TextArea(2, 4)] public string itemDescription;
    public Sprite itemIcon;
    public ItemCategory category;

    [Header("Reglas de Inventario")]
    [Tooltip("Límite: 1 para Armas/Claves, 20 para Objetos")]
    public int maxStack = 1; 
    public bool equipable = true; 
    [Tooltip("Solo aplica si el objeto es un Arma (Melee, Rango, Tanque)")]
    public UnitClass requiredClass;

    [Header("Aumento de Estadísticas")]
    public int bonusATK;
    public int bonusDEF;
    public int bonusSPD;
    public int bonusHP;
    public int bonusEnergy;
    public int bonusMastery;
}