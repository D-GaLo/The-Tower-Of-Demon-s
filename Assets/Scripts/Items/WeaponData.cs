using UnityEngine;

// Esto te añade una opción en el menú de click derecho de Unity
[CreateAssetMenu(fileName = "NuevaArma", menuName = "TowerOfDemons/Arma")]
public class WeaponData : ScriptableObject {
    public string weaponName = "Espada Básica";
    
    [Tooltip("¿Quién puede usar esto? Melee, Rango o Tanque")]
    public UnitClass requiredClass; // Referencia a las clases de tu GDD
    
    [Tooltip("¿Cuánto ataque suma?")]
    public int attackBonus = 5;
}