using UnityEngine;

[CreateAssetMenu(fileName = "NuevaArma", menuName = "TowerOfDemons/Arma")]
public class WeaponData : ScriptableObject {
    public string weaponName = "Espada Básica";
    
    [Tooltip("¿Quién puede usar esto? Melee, Rango o Tanque")]
    public UnitClass requiredClass; 
    
    [Tooltip("¿Cuánto ataque suma?")]
    public int attackBonus = 5;
}