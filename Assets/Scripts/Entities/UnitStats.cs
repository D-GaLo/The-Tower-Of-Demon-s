using UnityEngine;

// Clases y Posiciones según las reglas de tu GDD
public enum UnitClass { Melee, Rango, Tanque }
public enum UnitPosition { Tierra, BajoTierra, Volando }

public class UnitStats : MonoBehaviour {
    [Header("Identidad y Tipos")]
    public string unitName;
    public UnitClass unitClass;
    public UnitPosition unitPosition;

    [Header("Estadísticas Base")]
    public int maxHP = 100;
    public int currentHP;
    public int attack = 10;
    public int defense = 5;
    public int speed = 10; // Vital para decidir quién ataca primero

    // El método virtual permite que los hijos (Hero o Enemy) lo modifiquen si quieren
    public virtual void TakeDamage(int damage) {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        
        Debug.Log($"{unitName} recibió {damage} de daño. HP restante: {currentHP}");
    }
}